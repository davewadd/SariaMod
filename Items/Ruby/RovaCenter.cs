using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SariaMod.Buffs;
using SariaMod.Dusts;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using SariaMod.Items.Strange;
using SariaMod.Gores;
using SariaMod.TileGlow;
using SariaMod.Netcode;

namespace SariaMod.Items.Ruby
{
    /// <summary>
    /// RovaCenter: the stationary fire emitter spawned after a successful charge.
    /// States:
    ///   0 = Firing (beam just spawned, following cursor)
    ///   1 = Cooldown (beam expired, idle)
    ///   2 = Auto-aim (no player input for 10s, auto-targets enemy)
    ///   3 = Manual-override (player charges while RovaCenter exists, beam aims at ztarget4)
    /// </summary>
    public class RovaCenter : ModProjectile
    {
        private int StateTimer;
        private int BeamCooldownTimer;
        private int AutoFireTimer;
        private float CurrentAngle;
        private float TargetAngle;
        private bool BeamFired;
        private bool InitialAimSet;
        private bool NextBeamAutoRotates;

        private const float BeamRange = 2000f;

        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Saria");
            Main.projFrames[base.Projectile.type] = 1;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(StateTimer);
            writer.Write(BeamCooldownTimer);
            writer.Write(AutoFireTimer);
            writer.Write(CurrentAngle);
            writer.Write(TargetAngle);
            writer.Write(BeamFired);
            writer.Write(InitialAimSet);
            writer.Write(NextBeamAutoRotates);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            StateTimer = reader.ReadInt32();
            BeamCooldownTimer = reader.ReadInt32();
            AutoFireTimer = reader.ReadInt32();
            CurrentAngle = reader.ReadSingle();
            TargetAngle = reader.ReadSingle();
            BeamFired = reader.ReadBoolean();
            InitialAimSet = reader.ReadBoolean();
            NextBeamAutoRotates = reader.ReadBoolean();
        }

        public override void SetDefaults()
        {
            Projectile.width = 86;
            Projectile.height = 86;
            Projectile.netImportant = true;
            Projectile.alpha = 0;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 6000; // 10 minutes max lifetime
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.minionSlots = 0f;
        }

        public override bool? CanCutTiles()
        {
            return false;
        }

        public override bool? CanHitNPC(NPC target)
        {
            return false;
        }

        public override bool MinionContactDamage()
        {
            return false;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            StateTimer++;

            // On spawn: play ChargeFire2 and spawn fire ring burst
            if (StateTimer == 1)
            {
                // Spawn ring of fire particles
                for (int i = 0; i < 30; i++)
                {
                    Vector2 speed = Main.rand.NextVector2CircularEdge(1f, 1f);
                    Dust d = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<ShadowFlameDustCharge>(), speed * 8, Scale: 4.0f);
                    d.noGravity = true;
                }
                Lighting.AddLight(Projectile.Center, new Color(255, 100, 20).ToVector3() * 4f);
            }

            if (TryHandleOwnerUnsummon(player))
            {
                return;
            }

            // Check if player is holding left-click with the HealBall while RovaCenter exists.
            bool playerCharging = player.channel && player.HeldItem.type == ModContent.ItemType<HealBall>() && !Main.mouseRight;

            // Check if player is right-click targeting (ztarget exists)
            bool hasRightClickTarget = player.HasMinionAttackTargetNPC;

            // Look for ztarget4 for manual override
            int foundZtarget4 = -1;
            for (int i = 0; i < 1000; i++)
            {
                if (Main.projectile[i].active && Main.projectile[i].ModProjectile is Ztarget4 && Main.projectile[i].owner == Projectile.owner)
                {
                    foundZtarget4 = i;
                    break;
                }
            }

            if (!InitialAimSet)
            {
                InitialAimSet = true;
                if (foundZtarget4 >= 0)
                {
                    TargetAngle = (Main.projectile[foundZtarget4].Center - Projectile.Center).ToRotation();
                }
                else if (player.HasMinionAttackTargetNPC && Main.npc[player.MinionAttackTargetNPC].active)
                {
                    TargetAngle = (Main.npc[player.MinionAttackTargetNPC].Center - Projectile.Center).ToRotation();
                }
                else if (Main.myPlayer == Projectile.owner)
                {
                    TargetAngle = (Main.MouseWorld - Projectile.Center).ToRotation();
                }

                CurrentAngle = TargetAngle;
                Projectile.netUpdate = true;
            }

            // Check if a beam is currently alive
            bool hasActiveBeam = false;
            for (int i = 0; i < 1000; i++)
            {
                if (Main.projectile[i].active && Main.projectile[i].ModProjectile is RovaBeam && Main.projectile[i].owner == Projectile.owner)
                {
                    hasActiveBeam = true;
                    break;
                }
            }

            // STATE: FIRING (State 0)
            // Beam fires 0.5s after spawn (30 ticks after ChargeFire2 sound)
            if (StateTimer > 30 && !BeamFired && !hasActiveBeam)
            {
                FireBeam();
                BeamFired = true;
                BeamCooldownTimer = 0;
            }

            // STATE: COOLDOWN (State 1) - beam has expired, wait for next fire
            if (BeamFired && !hasActiveBeam && StateTimer > 60)
            {
                BeamCooldownTimer++;

                // After 600 ticks (10 seconds), auto-fire if enemy in range
                if (BeamCooldownTimer >= 600)
                {
                    // Find nearest enemy
                    NPC nearestEnemy = null;
                    float nearestDist = BeamRange; // ~1 screen width

                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        NPC npc = Main.npc[i];
                        if (!npc.active || npc.friendly || npc.lifeMax <= 0) continue;
                        if (npc.dontTakeDamage) continue;

                        float dist = Vector2.Distance(Projectile.Center, npc.Center);
                        if (dist < nearestDist)
                        {
                            // Check line of sight
                            if (Collision.CanHitLine(Projectile.Center, 1, 1, npc.Center, 1, 1))
                            {
                                nearestDist = dist;
                                nearestEnemy = npc;
                            }
                        }
                    }

                    if (nearestEnemy != null)
                    {
                        // Auto-fire: aim slightly off-center and rotate clockwise
                        Vector2 toEnemy = nearestEnemy.Center - Projectile.Center;
                        float enemyAngle = toEnemy.ToRotation();

                        // Start slightly off (counter-clockwise) so we sweep into the target
                        CurrentAngle = enemyAngle - 0.5f;
                        TargetAngle = enemyAngle + MathHelper.TwoPi; // full rotation
                        NextBeamAutoRotates = true;

                        FireBeam();
                        BeamFired = true;
                        BeamCooldownTimer = 0;
                        AutoFireTimer = 0;
                        Projectile.netUpdate = true;
                    }
                }
            }

            // STATE: AUTO-ROTATE (while auto-fire beam is active)
            if (BeamFired && hasActiveBeam && NextBeamAutoRotates)
            {
                AutoFireTimer++;

                if (AutoFireTimer > 260)
                {
                    NextBeamAutoRotates = false;
                    AutoFireTimer = 0;
                }
            }

            // STATE: MANUAL OVERRIDE - player holds left-click while RovaCenter exists
            if (foundZtarget4 >= 0 && playerCharging)
            {
                // ztarget4 is present but no charge countdown starts
                // Instead, RovaCenter aims beam at ztarget4 position
                Projectile ztarget = Main.projectile[foundZtarget4];
                Vector2 toTarget = ztarget.Center - Projectile.Center;
                TargetAngle = toTarget.ToRotation();

                // If beam isn't active, fire it aimed at ztarget4
                if (!hasActiveBeam && StateTimer > 60)
                {
                    CurrentAngle = TargetAngle;
                    NextBeamAutoRotates = false;
                    FireBeam();
                    BeamFired = true;
                    BeamCooldownTimer = 0;
                    Projectile.netUpdate = true;
                }
            }
            // RIGHT-CLICK TARGETING: aim beam at ztarget (right-click hold target)
            else if (hasRightClickTarget && !playerCharging)
            {
                NPC targetNpc = Main.npc[player.MinionAttackTargetNPC];
                if (targetNpc.active)
                {
                    Vector2 toTarget = targetNpc.Center - Projectile.Center;
                    TargetAngle = toTarget.ToRotation();

                    if (!hasActiveBeam && StateTimer > 60)
                    {
                        CurrentAngle = TargetAngle;
                        NextBeamAutoRotates = false;
                        FireBeam();
                        BeamFired = true;
                        BeamCooldownTimer = 0;
                        Projectile.netUpdate = true;
                    }
                }
            }

            // Tile heat: mark nearby tiles as heated while RovaCenter exists
            if (StateTimer % 15 == 0)
            {
                TileHeatManager.ApplyHeatInRadius(Projectile.Center, 100f, TileHeatManager.DefaultHeatDuration, Projectile.owner, Projectile.damage);
            }

            // Fire dust visual on RovaCenter itself
            if (Main.rand.NextBool(4))
            {
                Vector2 speed = Main.rand.NextVector2CircularEdge(1f, 1f);
                Dust d = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<ShadowFlameDustCharge>(), speed * 3, Scale: 2.5f);
                d.noGravity = true;
            }

            // Red light at center
            Lighting.AddLight(Projectile.Center, new Color(255, 60, 10).ToVector3() * 2f);
        }

        private void FireBeam()
        {
            Player player = Main.player[Projectile.owner];

            // Calculate beam direction from current angle
            Vector2 beamVelocity = CurrentAngle.ToRotationVector2();

            if (Main.myPlayer == Projectile.owner)
            {
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center,
                    beamVelocity * 2f,
                    ModContent.ProjectileType<RovaBeam>(),
                    (int)(Projectile.damage),
                    Projectile.knockBack,
                    Projectile.owner,
                    Projectile.whoAmI, // ai[0] = RovaCenter whoAmI
                    NextBeamAutoRotates ? 1f : 0f // ai[1] = auto-rotation mode
                );
            }
        }

        private bool TryHandleOwnerUnsummon(Player player)
        {
            if (Main.myPlayer != Projectile.owner)
                return false;

            if (player.HeldItem.type != ModContent.ItemType<HealBall>() || player.channel)
                return false;

            if (!Main.mouseLeft || !Main.mouseLeftRelease)
                return false;

            if (!Projectile.Hitbox.Contains(Main.MouseWorld.ToPoint()))
                return false;

            Main.mouseLeftRelease = false;
            Projectile.Kill();
            return true;
        }

        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            // Should never hit since CanHitNPC returns false
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Main.dedServ)
                return false;

            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Vector2 center = Projectile.Center - Main.screenPosition;
            Rectangle sourceRect = new Rectangle(0, 0, 1, 1);
            float pulse = 0.85f + (float)Math.Sin(Main.GameUpdateCount * 0.18f) * 0.15f;

            DrawCenteredPixel(pixel, sourceRect, center, new Vector2(42f, 42f) * pulse, new Color(255, 40, 0, 70));
            DrawCenteredPixel(pixel, sourceRect, center, new Vector2(24f, 24f) * pulse, new Color(255, 110, 20, 150));
            DrawCenteredPixel(pixel, sourceRect, center, new Vector2(12f, 12f), new Color(255, 245, 160, 230));

            for (int i = 0; i < 12; i++)
            {
                float angle = Projectile.rotation + i * MathHelper.TwoPi / 12f;
                Vector2 offset = angle.ToRotationVector2() * 25f * pulse;
                DrawCenteredPixel(pixel, sourceRect, center + offset, new Vector2(8f, 8f), new Color(255, 80, 0, 120));
            }

            return false;
        }

        private void DrawCenteredPixel(Texture2D pixel, Rectangle sourceRect, Vector2 position, Vector2 size, Color color)
        {
            Main.spriteBatch.Draw(pixel, position, sourceRect, color, 0f, new Vector2(0.5f, 0.5f), size, SpriteEffects.None, 0f);
        }

        public override void Kill(int timeLeft)
        {
            // Clean up any active beams
            for (int i = 0; i < 1000; i++)
            {
                if (Main.projectile[i].active && Main.projectile[i].ModProjectile is RovaBeam && Main.projectile[i].owner == Projectile.owner)
                {
                    Main.projectile[i].Kill();
                }
            }

            // Heated tiles fade naturally so other players' heat fields are not cleared.
        }
    }
}
