using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SariaMod.Buffs;
using SariaMod.Dusts;
using SariaMod.Gores;
using SariaMod;
using SariaMod.Items.Strange;
using SariaMod.TileGlow;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.Ruby
{
    /// <summary>
    /// RovaBeam: the fire beam projectile fired from RovaCenter.
    /// Fixed length (one screen width), direction follows cursor slowly.
    /// Collides with tiles, applies heavy damage/knockback, heats tiles, and tracks burned gores.
    /// </summary>
    public class RovaBeam : ModProjectile
    {
        private const float MaxBeamLength = 2000f; // ~1 screen width
        private const float BeamCollisionWidth = 38f;
        private const float ManualTurnSpeed = 0.026f; // Moon Lord death beam style sluggish turn
        private const float AutoSweepHalfAngle = 0.2617994f; // 15 degrees
        private const float AutoSweepTotalAngle = 6.5449847f; // one full turn plus the initial 15-degree lead-in
        private const int AutoSweepDurationTicks = 390; // 6.5 seconds at 60 FPS
        private const float AutoTurnSpeed = AutoSweepTotalAngle / AutoSweepDurationTicks;

        private readonly int[] PlayerHitCooldowns = new int[256];
        private readonly int[] FriendlyNPCHitCooldowns = new int[200];
        private int LastHeatTileX = int.MinValue;
        private int LastHeatTileY = int.MinValue;
        private int LastHeatPacketTick = int.MinValue;

        private float BeamLength
        {
            get => Projectile.localAI[1];
            set => Projectile.localAI[1] = value;
        }

        private float CurrentAngle
        {
            get => Projectile.rotation;
            set => Projectile.rotation = value;
        }

        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Saria");
            Main.projFrames[base.Projectile.type] = 1;
            ProjectileID.Sets.MinionShot[Projectile.type] = true;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(BeamLength);
            writer.Write(CurrentAngle);
            writer.Write(Projectile.ai[1]);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            BeamLength = reader.ReadSingle();
            CurrentAngle = reader.ReadSingle();
            Projectile.ai[1] = reader.ReadSingle();
        }

        public override void SetDefaults()
        {
            Projectile.width = (int)MaxBeamLength; // 2000 — large hitbox so tML calls Colliding() for any NPC on screen
            Projectile.height = (int)MaxBeamLength;
            Projectile.netImportant = true;
            Projectile.alpha = 0;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 180; // ~3 seconds of firing time
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 25; // 25 frames hit immunity
            Projectile.minionSlots = 0f;
            Projectile.extraUpdates = 0;
            Projectile.DamageType = DamageClass.Summon;
        }

        public override bool? CanCutTiles()
        {
            return false;
        }

        public override bool MinionContactDamage()
        {
            return false;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Vector2 beamEnd = Projectile.Center + CurrentAngle.ToRotationVector2() * BeamLength;
            float collisionPoint = 0f;
            return Collision.CheckAABBvLineCollision(
                targetHitbox.TopLeft(),
                targetHitbox.Size(),
                Projectile.Center,
                beamEnd,
                BeamCollisionWidth,
                ref collisionPoint
            );
        }

        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            Player player = Main.player[Projectile.owner];

            // Heavy knockback in beam direction (8 is vanilla max; 25 was excessive)
            knockback = target.type == NPCID.TargetDummy ? 0f : 8f;
            Vector2 beamDir = CurrentAngle.ToRotationVector2();
            hitDirection = beamDir.X > 0 ? 1 : -1;

            // Apply burning debuffs
            target.buffImmune[BuffID.CursedInferno] = false;
            target.buffImmune[BuffID.Confused] = false;
            target.buffImmune[BuffID.Slow] = false;
            target.buffImmune[BuffID.ShadowFlame] = false;
            target.buffImmune[BuffID.Ichor] = false;
            target.buffImmune[BuffID.OnFire] = false;
            target.buffImmune[BuffID.Frostburn] = false;
            target.buffImmune[BuffID.Poisoned] = false;
            target.buffImmune[BuffID.Venom] = false;
            target.buffImmune[BuffID.Electrified] = false;
            target.buffImmune[ModContent.BuffType<Burning2>()] = false;

            target.AddBuff(ModContent.BuffType<Burning2>(), 300);
            target.AddBuff(BuffID.OnFire, 300);
            target.GetGlobalNPC<FairyGlobalNPC>().RovaBurnedHit = true;

            // XP gain
            FairyPlayer modPlayer = player.Fairy();
            if (modPlayer != null) modPlayer.SariaXp++;
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            Vector2 beamDir = CurrentAngle.ToRotationVector2();
            if (target.type != NPCID.TargetDummy)
                target.velocity += beamDir * 12f;
            target.netUpdate = true;

            if (Main.netMode == NetmodeID.Server)
            {
                NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, target.whoAmI);
            }
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            Projectile rovaCenter = FindRovaCenter();

            // If RovaCenter is gone, kill beam
            if (rovaCenter == null)
            {
                Projectile.Kill();
                return;
            }

            // Beam origin is at RovaCenter center
            Projectile.Center = rovaCenter.Center;

            if (Projectile.localAI[0] == 0f)
            {
                Vector2 initialDirection = Projectile.velocity.SafeNormalize(Vector2.UnitX);
                CurrentAngle = initialDirection.ToRotation();
                BeamLength = MaxBeamLength;
                Projectile.localAI[0] = 1f;
                
                // Set damage from Saria's level/XP (every other projectile uses this)
                Projectile.SariaBaseDamage();
                if (Projectile.damage <= 0)
                    Projectile.damage = 1;

                if (Projectile.ai[1] > 0f)
                {
                    Projectile.timeLeft = Math.Max(Projectile.timeLeft, AutoSweepDurationTicks + 20);
                }
            }

            bool angleChanged = false;
            bool autoSweepComplete = false;

            if (Projectile.ai[1] > 0f)
            {
                float rotationStep = Math.Min(AutoTurnSpeed, Projectile.ai[1]);
                CurrentAngle += rotationStep;
                Projectile.ai[1] = Math.Max(0f, Projectile.ai[1] - rotationStep);
                angleChanged = true;
                autoSweepComplete = Projectile.ai[1] <= 0f;
            }
            else if (TryGetManualTargetAngle(player, out float targetAngle))
            {
                angleChanged = TurnToward(targetAngle, ManualTurnSpeed);
            }

            if (angleChanged && (Main.netMode == NetmodeID.Server || Main.myPlayer == Projectile.owner))
            {
                Projectile.netUpdate = true;
            }

            // Extend beam lifetime while player is actively charging or right-click targeting.
            // Uses player input directly (not Ztarget4 scan) because Ztarget4 can briefly
            // expire if Saria's IsCharging flag flickers (CantAttackTimer, HealpulseBuff, ChannelState reset).
            bool isManualOverride = player.channel && player.HeldItem.type == ModContent.ItemType<HealBall>() && !Main.mouseRight;
            bool isRightClickTargeting = player.HasMinionAttackTargetNPC;
            if (isManualOverride || isRightClickTargeting)
            {
                Projectile.timeLeft = Math.Max(Projectile.timeLeft, 60);
            }
            Vector2 direction = CurrentAngle.ToRotationVector2();
            float hitDist = ScanForTiles(Projectile.Center, direction, MaxBeamLength);
            BeamLength = Math.Max(40f, hitDist);

            ApplyBeamHeat();
            ApplyPlayerContactDamage();
            ApplyFriendlyNPCContactDamage();

            if (autoSweepComplete)
            {
                Projectile.Kill();
                return;
            }

            // Play looping sound while beam is active
            if (Projectile.timeLeft % 30 == 0) // every 0.5s, replay the loop
            {
                SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/LightLoop"), Projectile.Center);
            }

            // Fire dust along beam
            if (Main.rand.NextBool(3))
            {
                float dustDist = Main.rand.NextFloat(0f, 1f) * BeamLength;
                Vector2 dustPos = Projectile.Center + direction * dustDist;
                Vector2 dustVel = direction.RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(-2f, 2f);
                Dust d = Dust.NewDustPerfect(dustPos, ModContent.DustType<FlameDust>(), dustVel, 0, default, Main.rand.NextFloat(0.8f, 1.5f));
                d.noGravity = true;
            }

            // Red light along beam
            for (float d = 0f; d < BeamLength; d += 100f)
            {
                Vector2 lightPos = Projectile.Center + direction * d;
                Lighting.AddLight(lightPos, new Color(255, 80, 20).ToVector3() * 1.5f);
            }

            // Keep the projectile hitbox centered on the beam source; Colliding handles the actual line.
            Projectile.position = Projectile.Center - new Vector2(Projectile.width / 2f, Projectile.height / 2f);
        }

        private Projectile FindRovaCenter()
        {
            int parentIndex = (int)Projectile.ai[0];
            if (parentIndex >= 0 && parentIndex < Main.maxProjectiles)
            {
                Projectile parent = Main.projectile[parentIndex];
                if (parent.active && parent.owner == Projectile.owner && parent.ModProjectile is RovaCenter)
                {
                    return parent;
                }
            }

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile projectile = Main.projectile[i];
                if (projectile.active && projectile.owner == Projectile.owner && projectile.ModProjectile is RovaCenter)
                {
                    return projectile;
                }
            }

            return null;
        }

        private bool TryGetManualTargetAngle(Player player, out float targetAngle)
        {
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile projectile = Main.projectile[i];
                if (projectile.active && projectile.owner == Projectile.owner && projectile.ModProjectile is Ztarget4)
                {
                    targetAngle = (projectile.Center - Projectile.Center).ToRotation();
                    return true;
                }
            }

            if (player.HasMinionAttackTargetNPC)
            {
                NPC targetNpc = Main.npc[player.MinionAttackTargetNPC];
                if (targetNpc.active)
                {
                    targetAngle = (targetNpc.Center - Projectile.Center).ToRotation();
                    return true;
                }
            }

            targetAngle = CurrentAngle;
            return false;
        }

        private bool TurnToward(float targetAngle, float turnSpeed)
        {
            float angleDiff = MathHelper.WrapAngle(targetAngle - CurrentAngle);
            if (Math.Abs(angleDiff) > turnSpeed)
            {
                CurrentAngle += Math.Sign(angleDiff) * turnSpeed;
                return true;
            }

            CurrentAngle = targetAngle;
            return Math.Abs(angleDiff) > 0.001f;
        }

        /// <summary>
        /// Scan tiles along beam direction to find the first solid tile.
        /// </summary>
        private static float ScanForTiles(Vector2 origin, Vector2 direction, float maxDist)
        {
            float step = 16f;
            for (int i = 1; i < (int)(maxDist / step); i++)
            {
                Vector2 pos = origin + direction * (i * step);

                int tileX = (int)(pos.X / 16f);
                int tileY = (int)(pos.Y / 16f);

                if (tileX < 0 || tileX >= Main.maxTilesX || tileY < 0 || tileY >= Main.maxTilesY)
                    return i * step;

                Tile tile = Main.tile[tileX, tileY];
                if (tile.HasTile && Main.tileSolid[tile.TileType])
                    return i * step;
            }

            return maxDist;
        }

        /// <summary>
        /// Apply tile heat along the beam path, heating tiles it touches.
        /// </summary>
        private void ApplyBeamHeat()
        {
            int currentTick = (int)Main.GameUpdateCount;
            if (currentTick % 8 != 0) return; // throttle

            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            if (BeamLength >= MaxBeamLength - 16f)
                return;

            Vector2 direction = CurrentAngle.ToRotationVector2();
            Vector2 impactPos = Projectile.Center + direction * BeamLength;
            int tileX = (int)(impactPos.X / 16f);
            int tileY = (int)(impactPos.Y / 16f);

            if (tileX < 0 || tileX >= Main.maxTilesX || tileY < 0 || tileY >= Main.maxTilesY)
                return;

            Tile tile = Main.tile[tileX, tileY];
            if (!tile.HasTile || !Main.tileSolid[tile.TileType])
                return;

            TileHeatManager.ApplyBeamImpact(tileX, tileY, TileHeatManager.DefaultHeatDuration, Projectile.owner, Projectile.damage);

            bool impactChanged = tileX != LastHeatTileX || tileY != LastHeatTileY;
            bool refreshPacket = currentTick - LastHeatPacketTick >= 12;
            if (Main.netMode == NetmodeID.Server && (impactChanged || refreshPacket))
            {
                TileHeatNetworking.SendBeamImpactPacket(tileX, tileY, TileHeatManager.DefaultHeatDuration, Projectile.owner, Projectile.damage);
                LastHeatPacketTick = currentTick;
            }

            LastHeatTileX = tileX;
            LastHeatTileY = tileY;
        }

        private void ApplyPlayerContactDamage()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            Vector2 beamDir = CurrentAngle.ToRotationVector2();
            Vector2 beamEnd = Projectile.Center + beamDir * BeamLength;

            for (int i = 0; i < Main.maxPlayers && i < PlayerHitCooldowns.Length; i++)
            {
                if (PlayerHitCooldowns[i] > 0)
                {
                    PlayerHitCooldowns[i]--;
                    continue;
                }

                Player target = Main.player[i];
                if (target == null || !target.active || target.dead)
                    continue;

                float collisionPoint = 0f;
                bool touchingBeam = Collision.CheckAABBvLineCollision(
                    target.Hitbox.TopLeft(),
                    target.Hitbox.Size(),
                    Projectile.Center,
                    beamEnd,
                    BeamCollisionWidth,
                    ref collisionPoint
                );

                if (!touchingBeam)
                    continue;

                int hitDirection = beamDir.X >= 0f ? 1 : -1;
                int damage = Math.Max(1, target.statLifeMax2 / 5);

                target.Hurt(PlayerDeathReason.ByProjectile(Projectile.owner, Projectile.whoAmI), damage, hitDirection, false, false, false, -1);
                target.velocity += beamDir * 12f;
                target.immune = true;
                target.immuneNoBlink = true;
                target.immuneTime = Math.Max(target.immuneTime, 30);
                PlayerHitCooldowns[i] = 45;
            }
        }

        private void ApplyFriendlyNPCContactDamage()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            Vector2 beamEnd = Projectile.Center + CurrentAngle.ToRotationVector2() * BeamLength;
            Vector2 beamDir = CurrentAngle.ToRotationVector2();

            for (int i = 0; i < Main.maxNPCs && i < FriendlyNPCHitCooldowns.Length; i++)
            {
                if (FriendlyNPCHitCooldowns[i] > 0)
                {
                    FriendlyNPCHitCooldowns[i]--;
                    continue;
                }

                NPC target = Main.npc[i];
                bool isTargetDummy = target != null && target.type == NPCID.TargetDummy;
                if (target == null || !target.active || target.lifeMax <= 0 || target.immortal || (!isTargetDummy && target.dontTakeDamage))
                    continue;

                if (!isTargetDummy && !target.friendly && target.catchItem <= 0)
                    continue;

                float collisionPoint = 0f;
                bool touchingBeam = Collision.CheckAABBvLineCollision(
                    target.Hitbox.TopLeft(),
                    target.Hitbox.Size(),
                    Projectile.Center,
                    beamEnd,
                    BeamCollisionWidth,
                    ref collisionPoint
                );

                if (!touchingBeam)
                    continue;

                target.buffImmune[BuffID.OnFire] = false;
                target.buffImmune[ModContent.BuffType<Burning2>()] = false;
                target.AddBuff(ModContent.BuffType<Burning2>(), 180);
                target.AddBuff(BuffID.OnFire, 180);

                int hitDirection = beamDir.X >= 0f ? 1 : -1;
                int damage = Math.Max(1, target.lifeMax / 5);
                target.StrikeNPC(damage, isTargetDummy ? 0f : 8f, hitDirection);

                if (!isTargetDummy)
                    target.velocity += beamDir * 8f;

                target.netUpdate = true;
                if (Main.netMode != NetmodeID.Server)
                    BurnedGoreSystem.TrackGoresNearPosition(target.Center, 100f);
                FriendlyNPCHitCooldowns[i] = 25;

                if (Main.netMode == NetmodeID.Server)
                    NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, target.whoAmI);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Main.dedServ) return false;

            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Vector2 startPos = Projectile.Center - Main.screenPosition;
            Rectangle sourceRect = new Rectangle(0, 0, 1, 1);
            float pulse = 0.85f + (float)Math.Sin(Main.GameUpdateCount * 0.35f) * 0.15f;

            Texture2D beamTexture = RovaVisualAssets.Beam;
            if (beamTexture != null)
            {
                DrawAnimatedBeamTexture(beamTexture, startPos, pulse);
            }
            else
            {
                DrawBeamPass(pixel, sourceRect, startPos, BeamLength, 44f, new Color(255, 40, 0, 70) * pulse);
                DrawBeamPass(pixel, sourceRect, startPos, BeamLength, 24f, new Color(255, 95, 10, 110) * pulse);
                DrawBeamPass(pixel, sourceRect, startPos, BeamLength, 10f, new Color(255, 210, 60, 220));
                DrawBeamPass(pixel, sourceRect, startPos, BeamLength, 4f, new Color(255, 255, 220, 240));
            }

            return false;
        }

        private void DrawAnimatedBeamTexture(Texture2D texture, Vector2 startPos, float pulse)
        {
            const float segmentLength = 96f;
            float phase = (float)(Main.GameUpdateCount * 1.25f % segmentLength);
            Vector2 direction = CurrentAngle.ToRotationVector2();

            for (float segmentStart = -phase; segmentStart < BeamLength; segmentStart += segmentLength)
            {
                float visibleStart = Math.Max(0f, segmentStart);
                float visibleEnd = Math.Min(BeamLength, segmentStart + segmentLength);
                if (visibleEnd <= visibleStart)
                    continue;

                float sourceStartRatio = (visibleStart - segmentStart) / segmentLength;
                float sourceEndRatio = (visibleEnd - segmentStart) / segmentLength;
                int sourceX = Math.Clamp((int)(sourceStartRatio * texture.Width), 0, texture.Width - 1);
                int sourceEndX = Math.Clamp((int)(sourceEndRatio * texture.Width), sourceX + 1, texture.Width);
                int sourceWidth = sourceEndX - sourceX;
                Rectangle sourceRect = new Rectangle(sourceX, 0, sourceWidth, texture.Height);
                Vector2 drawPosition = startPos + direction * visibleStart;
                Vector2 scale = new Vector2(
                    (visibleEnd - visibleStart) / sourceWidth,
                    48f / texture.Height);

                Main.spriteBatch.Draw(
                    texture,
                    drawPosition,
                    sourceRect,
                    Color.White * pulse,
                    CurrentAngle,
                    new Vector2(0f, texture.Height / 2f),
                    scale,
                    SpriteEffects.None,
                    0f);
            }
        }

        private void DrawBeamPass(Texture2D pixel, Rectangle sourceRect, Vector2 startPos, float length, float width, Color color)
        {
            Main.spriteBatch.Draw(
                pixel,
                startPos,
                sourceRect,
                color,
                CurrentAngle,
                new Vector2(0f, 0.5f),
                new Vector2(length, width),
                SpriteEffects.None,
                0f
            );
        }

        public override void Kill(int timeLeft)
        {
            // Spawn fire burst on beam expire
            for (int i = 0; i < 15; i++)
            {
                Vector2 speed = Main.rand.NextVector2CircularEdge(1f, 1f);
                Dust d = Dust.NewDustPerfect(Projectile.Center + CurrentAngle.ToRotationVector2() * BeamLength, ModContent.DustType<FlameDust>(), speed * 4, Scale: 2.0f);
                d.noGravity = true;
            }
        }
    }
}
