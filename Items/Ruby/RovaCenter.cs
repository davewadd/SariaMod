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
        private int _idleTimer;
        private bool _isFlickering;

        private const float BeamRange = 2000f;
        private const float TrackTurnSpeed = 0.04f;
        private const float AutoSweepHalfAngle = 0.2617994f; // 15 degrees
        private const float AutoSweepTotalAngle = AutoSweepHalfAngle * 2f;

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
            writer.Write(_idleTimer);
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
            _idleTimer = reader.ReadInt32();
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
            bool hasPersistenceUpgrade = Projectile.ai[0] >= 1f;
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

            // Kill immediately if owner is dead or Saria minion is gone
            if (player.dead || !player.active)
            {
                KillAllBeams();
                Projectile.Kill();
                return;
            }
            bool sariaAlive = false;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                if (Main.projectile[i].active && Main.projectile[i].owner == Projectile.owner && Main.projectile[i].ModProjectile is Saria)
                {
                    sariaAlive = true;
                    break;
                }
            }
            if (!sariaAlive)
            {
                KillAllBeams();
                Projectile.Kill();
                return;
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

            NPC visibleAutoTarget = FindVisibleAutoTarget();
            bool hasVisibleAutoTarget = visibleAutoTarget != null;

            // STATE: FIRING (State 0)
            // Beam fires 0.5s after spawn (30 ticks after ChargeFire2 sound)
            if (StateTimer > 30 && !BeamFired && !hasActiveBeam)
            {
                FireBeam();
                BeamFired = true;
                BeamCooldownTimer = 0;
            }

            // STATE: COOLDOWN (State 1) - beam has expired, wait for next fire
            if (BeamFired && !hasActiveBeam && StateTimer > 60 && (hasPersistenceUpgrade || _idleTimer < 300 || hasVisibleAutoTarget))
            {
                BeamCooldownTimer++;

                // After 600 ticks (10 seconds), auto-fire if enemy in range
                if (BeamCooldownTimer >= 600)
                {
                    if (visibleAutoTarget != null)
                    {
                        // Auto-fire: sweep a narrow cone centered on the target.
                        Vector2 toEnemy = visibleAutoTarget.Center - Projectile.Center;
                        float enemyAngle = toEnemy.ToRotation();

                        CurrentAngle = enemyAngle - AutoSweepHalfAngle;
                        TargetAngle = enemyAngle + AutoSweepHalfAngle;
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

                // Continuously rotate toward target so next beam starts at correct angle
                float diff = MathHelper.WrapAngle(TargetAngle - CurrentAngle);
                if (Math.Abs(diff) > TrackTurnSpeed)
                    CurrentAngle += Math.Sign(diff) * TrackTurnSpeed;
                else
                    CurrentAngle = TargetAngle;

                // If beam isn't active, fire it aimed at ztarget4
                if (!hasActiveBeam && StateTimer > 60)
                {
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

                    // Continuously rotate toward target so next beam starts at correct angle
                    float diff = MathHelper.WrapAngle(TargetAngle - CurrentAngle);
                    if (Math.Abs(diff) > TrackTurnSpeed)
                        CurrentAngle += Math.Sign(diff) * TrackTurnSpeed;
                    else
                        CurrentAngle = TargetAngle;

                    if (!hasActiveBeam && StateTimer > 60)
                    {
                        NextBeamAutoRotates = false;
                        FireBeam();
                        BeamFired = true;
                        BeamCooldownTimer = 0;
                        Projectile.netUpdate = true;
                    }
                }
            }


            // STATE: IDLE DESPAWN - no player input for 5s triggers flicker, 7s triggers despawn
            if (!hasPersistenceUpgrade && !hasActiveBeam && !playerCharging && foundZtarget4 < 0 && !hasRightClickTarget && !hasVisibleAutoTarget)
            {
                _idleTimer++;

                if (_idleTimer >= 420)
                {
                    KillAllBeams();
                    Projectile.Kill();
                    return;
                }

                _isFlickering = _idleTimer >= 300;
            }
            else
            {
                _idleTimer = 0;
                _isFlickering = false;
            }
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

            Lighting.AddLight(Projectile.Center, new Color(255, 60, 10).ToVector3() * 2f);
        }

        private void FireBeam()
        {
            int beamDamage = Math.Max(1, (int)(Projectile.damage));
            Vector2 beamVelocity = CurrentAngle.ToRotationVector2();

            if (Main.myPlayer == Projectile.owner)
            {
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center,
                    beamVelocity * 2f,
                    ModContent.ProjectileType<RovaBeam>(),
                    beamDamage,
                    Projectile.knockBack,
                    Projectile.owner,
                    Projectile.whoAmI, // ai[0] = RovaCenter whoAmI
                    NextBeamAutoRotates ? AutoSweepTotalAngle : 0f // ai[1] = remaining auto-sweep radians
                );
            }
        }

        private NPC FindVisibleAutoTarget()
        {
            NPC nearestEnemy = null;
            float nearestDist = BeamRange;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active)
                    continue;

                bool isTargetDummy = npc.type == NPCID.TargetDummy;
                if (!isTargetDummy && (npc.friendly || npc.lifeMax <= 0 || npc.dontTakeDamage))
                    continue;

                float distance = Vector2.Distance(Projectile.Center, npc.Center);
                if (distance >= nearestDist)
                    continue;

                if (!Collision.CanHitLine(Projectile.Center, 1, 1, npc.Center, 1, 1))
                    continue;

                nearestDist = distance;
                nearestEnemy = npc;
            }

            return nearestEnemy;
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
            // Flicker: skip every other pair of frames when idle-time despawn is imminent
            if (_isFlickering && Main.GameUpdateCount % 4 < 2)
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

        private void KillAllBeams()
        {
            for (int i = 0; i < 1000; i++)
            {
                if (Main.projectile[i].active && Main.projectile[i].ModProjectile is RovaBeam && Main.projectile[i].owner == Projectile.owner)
                {
                    Main.projectile[i].Kill();
                }
            }
        }

        public override void Kill(int timeLeft)
        {
            KillAllBeams();
            // Heated tiles fade naturally so other players' heat fields are not cleared.
        }
    }
}
