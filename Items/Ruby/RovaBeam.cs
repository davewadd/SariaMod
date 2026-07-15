using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SariaMod.Buffs;
using SariaMod.Gores;
using SariaMod;
using SariaMod.Items.Strange;
using SariaMod.TileGlow;
using System;
using System.Collections.Generic;
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
        private const float BeamCollisionWidth = 14f;
        private const float BeamGlobStartDistance = 29f;
        private const float BeamStartupTravelSpeed = 12f; // roughly standard Terraria bullet velocity
        private const float BeamEstablishedTravelSpeed = 100f;
        private const int BeamStartupDurationTicks = 120; // 2 seconds at 60 FPS
        private const float ManualTurnSpeed = 0.026f; // Moon Lord death beam style sluggish turn
        private const float AutoSweepTotalAngle = 6.8067847f; // 390 degrees: one full turn plus 30 degrees
        private const int AutoFullSpeedSweepTicks = 520;
        private const float AutoTurnSpeed = AutoSweepTotalAngle / AutoFullSpeedSweepTicks;
        private const float AutoInitialTurnScale = 0.3f;
        private const int AutoAccelerationTicks = 180;
        private const int AutoSweepDurationTicks = 584;
        private const int BeamFadeOutTicks = 24;
        private const int BeamDrawScreenFluff = 4800;
        internal const int MaximumActiveBeamTicks = 600; // 10 seconds at 60 FPS; shared by every aim mode.
        private const float ImpactGlobMergeRange = 56f;
        private const int ImpactGlobCooldownTicks = 10;
        private const int ImpactGlobCount = 5;
        private const int MaxCoreSplashGlobs = 96;
        internal static readonly Color CoreSplashInnerColor = new Color(255, 255, 220);

        private readonly int[] PlayerHitCooldowns = new int[256];
        private readonly int[] FriendlyNPCHitCooldowns = new int[200];
        private int LastHeatTileX = int.MinValue;
        private int LastHeatTileY = int.MinValue;
        private int LastHeatPacketTick = int.MinValue;
        private readonly List<RovaLavaGlob> LavaGlobs = new List<RovaLavaGlob>();
        private readonly List<RovaCoreSplashGlob> CoreSplashGlobs = new List<RovaCoreSplashGlob>();
        private readonly List<RovaBeamImpactZone> ImpactGlobZones = new List<RovaBeamImpactZone>();
        private bool InitialCoreSplashPlayed;
        private bool BeamEnding;
        private bool FireUpgrade2Active;
        private bool AutoSweepMode;
        private int BeamFadeTimer;
        private int BeamActiveTimer;
        private int BeamStartupTimer;
        private int AutoSweepTimer;
        private int EndpointProjectileIndex = -1;
        private const int MaxFirePillars = 5;
        private readonly List<int> FirePillarProjectileIndices = new List<int>();
        private bool PillarSurfaceContactActive;
        private int LastPillarTileX = int.MinValue;
        private int LastPillarTileY = int.MinValue;
        private Vector2 LastPillarSurfaceNormal = -Vector2.UnitY;
        private bool SurfaceContactReached;
        private float SurfaceContactDistance = MaxBeamLength;
        private Vector2 SurfaceContactPosition;
        private Vector2 SurfaceNormal = -Vector2.UnitY;
        private int SurfaceContactTileX = -1;
        private int SurfaceContactTileY = -1;
        private readonly FadingSoundPlayer BeamLoopSound = new FadingSoundPlayer(
            fadeOutSpeed: 0.72f / BeamFadeOutTicks)
        {
            TargetVolume = 0.72f
        };
        private bool RovaFirePlayed;

        private struct RovaCoreSplashGlob
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Age;
            public float Life;
            public float Size;
            public Color Color;
        }

        private struct RovaBeamImpactZone
        {
            public Vector2 Position;
            public int TimeLeft;
        }

        private float BeamLength
        {
            get => Projectile.localAI[1];
            set => Projectile.localAI[1] = value;
        }

        private float BeamTravelLength;

        private float CurrentAngle
        {
            get => Projectile.rotation;
            set => Projectile.rotation = value;
        }

        internal static float GetTravelTicksToDistance(float distance)
        {
            distance = MathHelper.Clamp(distance, 0f, MaxBeamLength);
            float startupDistance = BeamStartupTravelSpeed * BeamStartupDurationTicks;
            float travelTicks;
            if (distance <= startupDistance)
                travelTicks = distance / BeamStartupTravelSpeed;
            else
                travelTicks = BeamStartupDurationTicks
                    + (distance - startupDistance) / BeamEstablishedTravelSpeed;

            // BeamTravelLength advances once per whole update. Rounding up
            // prevents the sweep from crossing an enemy one frame before the
            // beam head actually reaches that distance.
            return Math.Max(1f, (float)Math.Ceiling(travelTicks));
        }

        internal static float GetClockwiseAutoLead(float distance)
        {
            int travelTicks = (int)GetTravelTicksToDistance(distance);
            float rotation = 0f;
            for (int tick = 0; tick < travelTicks; tick++)
                rotation += GetAutoTurnSpeedForTick(tick);

            return rotation;
        }

        private static float GetAutoTurnSpeedForTick(int elapsedTicks)
        {
            float progress = MathHelper.Clamp(
                elapsedTicks / (float)AutoAccelerationTicks,
                0f,
                1f);
            float easedProgress = progress * progress * (3f - 2f * progress);
            float speedScale = MathHelper.Lerp(
                AutoInitialTurnScale,
                1f,
                easedProgress);
            return AutoTurnSpeed * speedScale;
        }

        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Saria");
            Main.projFrames[base.Projectile.type] = 1;
            ProjectileID.Sets.MinionShot[Projectile.type] = true;

            // RovaCenter draws the beam as part of its composite. Extend both
            // projectile draw ranges so that a distant source does not cull
            // the beam or its center before the beam can be rendered.
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = BeamDrawScreenFluff;
            int rovaCenterType = ModContent.ProjectileType<RovaCenter>();
            ProjectileID.Sets.DrawScreenCheckFluff[rovaCenterType] = Math.Max(
                ProjectileID.Sets.DrawScreenCheckFluff[rovaCenterType],
                BeamDrawScreenFluff);
        }

        public override void OnSpawn(IEntitySource source)
        {
            if (Main.dedServ)
                return;

            TryStartBeamAudio();
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(BeamLength);
            writer.Write(BeamTravelLength);
            writer.Write(BeamActiveTimer);
            writer.Write(BeamStartupTimer);
            writer.Write(AutoSweepTimer);
            writer.Write(CurrentAngle);
            writer.Write(Projectile.ai[1]);
            writer.Write(BeamEnding);
            writer.Write(AutoSweepMode);
            writer.Write(BeamFadeTimer);
            writer.Write(SurfaceContactReached);
            writer.Write(SurfaceContactDistance);
            writer.Write(SurfaceContactPosition.X);
            writer.Write(SurfaceContactPosition.Y);
            writer.Write(SurfaceNormal.X);
            writer.Write(SurfaceNormal.Y);
            writer.Write(SurfaceContactTileX);
            writer.Write(SurfaceContactTileY);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            BeamLength = reader.ReadSingle();
            BeamTravelLength = reader.ReadSingle();
            BeamActiveTimer = reader.ReadInt32();
            BeamStartupTimer = reader.ReadInt32();
            AutoSweepTimer = reader.ReadInt32();
            CurrentAngle = reader.ReadSingle();
            Projectile.ai[1] = reader.ReadSingle();
            BeamEnding = reader.ReadBoolean();
            AutoSweepMode = reader.ReadBoolean();
            BeamFadeTimer = reader.ReadInt32();
            SurfaceContactReached = reader.ReadBoolean();
            SurfaceContactDistance = reader.ReadSingle();
            SurfaceContactPosition = new Vector2(reader.ReadSingle(), reader.ReadSingle());
            SurfaceNormal = new Vector2(reader.ReadSingle(), reader.ReadSingle());
            SurfaceContactTileX = reader.ReadInt32();
            SurfaceContactTileY = reader.ReadInt32();
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
            Projectile.timeLeft = MaximumActiveBeamTicks + BeamFadeOutTicks + 2;
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

        public override bool? CanDamage()
        {
            // The beam remains damaging during its visual/audio taper and only
            // becomes harmless when the projectile is removed.
            return null;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Vector2 beamEnd = Projectile.Center + CurrentAngle.ToRotationVector2() * BeamTravelLength;
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
            if (Main.netMode != NetmodeID.Server)
                TrySpawnBeamImpactGlobs(target, beamDir);

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
            FireUpgrade2Active = rovaCenter.ModProjectile is RovaCenter center
                && center.HasRovaSentryPersistenceUpgrade;

            if (BeamEnding)
            {
                UpdateBeamFadeState();
                if (Projectile.active)
                {
                    ApplyPlayerContactDamage();
                    ApplyFriendlyNPCContactDamage();
                }
                return;
            }

            if (Projectile.localAI[0] == 0f)
            {
                Vector2 initialDirection = Projectile.velocity.SafeNormalize(Vector2.UnitX);
                CurrentAngle = initialDirection.ToRotation();
                BeamLength = MaxBeamLength;
                BeamTravelLength = 0f;
                BeamActiveTimer = 0;
                BeamStartupTimer = 0;
                AutoSweepTimer = 0;
                FirePillarProjectileIndices.Clear();
                PillarSurfaceContactActive = false;
                LastPillarTileX = int.MinValue;
                LastPillarTileY = int.MinValue;
                LastPillarSurfaceNormal = -initialDirection;
                SurfaceContactReached = false;
                SurfaceContactDistance = MaxBeamLength;
                SurfaceContactPosition = Projectile.Center;
                SurfaceNormal = -initialDirection;
                SurfaceContactTileX = -1;
                SurfaceContactTileY = -1;
                Projectile.localAI[0] = 1f;
                AutoSweepMode = Projectile.ai[1] > 0f;
                
                // Set damage from Saria's level/XP (every other projectile uses this)
                Projectile.SariaBaseDamage();
                if (Projectile.damage <= 0)
                    Projectile.damage = 1;

            }

            // This timer starts when the beam projectile begins firing and is
            // never reset by switching between automatic and Ztarget4 control.
            BeamActiveTimer++;
            if (BeamActiveTimer >= MaximumActiveBeamTicks)
            {
                BeginBeamFadeOut();
                return;
            }

            EnsureBeamEndpoint();

            if (!Main.dedServ)
                TryStartBeamAudio();

            bool angleChanged = false;
            bool autoSweepComplete = false;

            if (TryGetOwnedZtargetAngle(out float ztargetAngle))
            {
                if (Projectile.ai[1] > 0f || AutoSweepMode)
                {
                    Projectile.ai[1] = 0f;
                    AutoSweepMode = false;
                    Projectile.netUpdate = true;
                }

                angleChanged = TurnToward(ztargetAngle, ManualTurnSpeed);
            }
            else if (Projectile.ai[1] > 0f)
            {
                float rotationSpeed = GetAutoTurnSpeedForTick(AutoSweepTimer);
                float rotationStep = Math.Min(rotationSpeed, Projectile.ai[1]);
                CurrentAngle += rotationStep;
                Projectile.ai[1] = Math.Max(0f, Projectile.ai[1] - rotationStep);
                AutoSweepTimer++;
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

            Vector2 direction = CurrentAngle.ToRotationVector2();
            bool hasSolidSurface = TryScanForTileContact(
                Projectile.Center,
                direction,
                MaxBeamLength,
                out float contactDistance,
                out Vector2 contactPosition,
                out Vector2 contactNormal,
                out int contactTileX,
                out int contactTileY);
            BeamLength = hasSolidSurface
                ? contactDistance
                : MaxBeamLength;
            float travelSpeed = BeamStartupTimer < BeamStartupDurationTicks
                ? BeamStartupTravelSpeed
                : BeamEstablishedTravelSpeed;
            if (BeamStartupTimer < BeamStartupDurationTicks)
                BeamStartupTimer++;
            BeamTravelLength = Math.Min(BeamLength, BeamTravelLength + travelSpeed);
            UpdateSurfaceContactState(
                hasSolidSurface,
                contactDistance,
                contactPosition,
                contactNormal,
                contactTileX,
                contactTileY,
                direction);
            EnsureFirePillar();

            if (!Main.dedServ)
                UpdateBeamAudioSources();

            if (Main.netMode != NetmodeID.Server)
            {
                if (BeamTravelLength > BeamGlobStartDistance
                    && LavaGlobs.Count < 42
                    && Main.rand.NextBool(2))
                    RovaLavaGlobVisual.SpawnAlongBeam(
                        LavaGlobs,
                        Projectile.Center,
                        direction,
                        BeamTravelLength,
                        1,
                        BeamGlobStartDistance,
                        useFireUpgrade2Palette: FireUpgrade2Active,
                        defaultOuterColor: new Color(255, 66, 8, 220));

                RovaLavaGlobVisual.UpdateAlongBeam(LavaGlobs, Projectile.Center, direction, BeamTravelLength);
                UpdateCoreSplashGlobs(direction);
            }

            ApplyBeamHeat();
            ApplyPlayerContactDamage();
            ApplyFriendlyNPCContactDamage();

            if (autoSweepComplete)
            {
                BeginBeamFadeOut();
                return;
            }

            // Red light along beam
            for (float d = 0f; d < BeamTravelLength; d += 100f)
            {
                Vector2 lightPos = Projectile.Center + direction * d;
                Lighting.AddLight(lightPos, new Color(255, 165, 35).ToVector3() * 1.25f);
            }

            if (Projectile.timeLeft <= 1)
                BeginBeamFadeOut();

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

        private void EnsureBeamEndpoint()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            if (EndpointProjectileIndex >= 0
                && EndpointProjectileIndex < Main.maxProjectiles
                && Main.projectile[EndpointProjectileIndex].active
                && Main.projectile[EndpointProjectileIndex].ModProjectile is RovaBeamEndpoint)
            {
                return;
            }

            EndpointProjectileIndex = Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                GetBeamEndpointPosition(),
                Vector2.Zero,
                ModContent.ProjectileType<RovaBeamEndpoint>(),
                0,
                0f,
                Projectile.owner,
                Projectile.whoAmI);

            if (EndpointProjectileIndex >= 0 && EndpointProjectileIndex < Main.maxProjectiles)
                Main.projectile[EndpointProjectileIndex].netUpdate = true;
        }

        private void EnsureFirePillar()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            for (int i = FirePillarProjectileIndices.Count - 1; i >= 0; i--)
            {
                int index = FirePillarProjectileIndices[i];
                if (index < 0
                    || index >= Main.maxProjectiles
                    || !Main.projectile[index].active
                    || Main.projectile[index].ModProjectile is not RovaFirePillar)
                {
                    FirePillarProjectileIndices.RemoveAt(i);
                }
            }

            if (!SurfaceContactReached)
            {
                PillarSurfaceContactActive = false;
                return;
            }

            Vector2 direction = CurrentAngle.ToRotationVector2();
            int tileX = SurfaceContactTileX;
            int tileY = SurfaceContactTileY;
            Vector2 normal = SurfaceNormal.SafeNormalize(-direction);

            bool newSurfaceContact = !PillarSurfaceContactActive
                || tileX != LastPillarTileX
                || tileY != LastPillarTileY
                || Vector2.Dot(normal, LastPillarSurfaceNormal) < 0.98f;

            PillarSurfaceContactActive = true;
            LastPillarTileX = tileX;
            LastPillarTileY = tileY;
            LastPillarSurfaceNormal = normal;

            int visiblePillarCount = 0;
            foreach (int index in FirePillarProjectileIndices)
            {
                if (index >= 0
                    && index < Main.maxProjectiles
                    && Main.projectile[index].active
                    && Main.projectile[index].ModProjectile is RovaFirePillar pillar
                    && !pillar.IsErupted)
                {
                    visiblePillarCount++;
                }
            }

            if (!newSurfaceContact || visiblePillarCount >= MaxFirePillars)
                return;

            int pillarIndex = Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                SurfaceContactPosition,
                normal,
                ModContent.ProjectileType<RovaFirePillar>(),
                0,
                0f,
                Projectile.owner,
                Projectile.whoAmI);

            if (pillarIndex >= 0 && pillarIndex < Main.maxProjectiles)
            {
                FirePillarProjectileIndices.Add(pillarIndex);
                Main.projectile[pillarIndex].netUpdate = true;
            }
        }

        private void TryStartBeamAudio()
        {
            if (Main.dedServ)
                return;

            UpdateBeamAudioSources();

            if (!RovaFirePlayed)
            {
                SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/RovaFire"), Projectile.Center);
                RovaFirePlayed = true;
            }

            BeamLoopSound.PlayManaged("SariaMod/Sounds/RovaBeamLoop");
        }

        private void UpdateBeamAudioSources()
        {
            BeamLoopSound.SetSpatialSources(
                Projectile.Center,
                GetBeamEndpointPosition());
        }

        internal void BeginBeamFadeOut()
        {
            if (BeamEnding)
                return;

            BeamEnding = true;
            BeamFadeTimer = 0;
            Projectile.timeLeft = Math.Max(Projectile.timeLeft, BeamFadeOutTicks + 2);
            Projectile.netUpdate = true;

            if (!Main.dedServ && BeamLoopSound.IsPlaying)
            {
                // FadingSoundPlayer uses the same 24-tick duration as the
                // visual taper, so the sound and beam disappear together.
                BeamLoopSound.Stop();
            }
        }

        private void UpdateBeamFadeState()
        {
            Projectile.timeLeft = 2;
            BeamFadeTimer++;

            if (AutoSweepMode && TryGetOwnedZtargetAngle(out float ztargetAngle))
            {
                AutoSweepMode = false;
                Projectile.ai[1] = 0f;
                Projectile.netUpdate = true;
                TurnToward(ztargetAngle, ManualTurnSpeed);
            }

            if (AutoSweepMode)
            {
                // Self-controlled beams keep sweeping while they taper out.
                CurrentAngle += GetAutoTurnSpeedForTick(AutoSweepTimer);
                AutoSweepTimer++;

                if (Main.netMode == NetmodeID.Server && BeamFadeTimer % 3 == 0)
                    Projectile.netUpdate = true;
            }

            Vector2 direction = CurrentAngle.ToRotationVector2();
            bool hasSolidSurface = TryScanForTileContact(
                Projectile.Center,
                direction,
                MaxBeamLength,
                out float contactDistance,
                out Vector2 contactPosition,
                out Vector2 contactNormal,
                out int contactTileX,
                out int contactTileY);
            BeamLength = hasSolidSurface
                ? contactDistance
                : MaxBeamLength;
            BeamTravelLength = Math.Min(BeamTravelLength, BeamLength);
            UpdateSurfaceContactState(
                hasSolidSurface,
                contactDistance,
                contactPosition,
                contactNormal,
                contactTileX,
                contactTileY,
                direction);

            if (!Main.dedServ)
                UpdateBeamAudioSources();

            if (Main.netMode != NetmodeID.Server)
            {
                UpdateCoreSplashGlobs(direction, allowSpawn: false);
                RovaLavaGlobVisual.UpdateAlongBeam(
                    LavaGlobs,
                    Projectile.Center,
                    direction,
                    BeamTravelLength);

                float visualAlpha = GetBeamVisualAlpha();
                for (float d = 0f; d < BeamTravelLength; d += 100f)
                {
                    Vector2 lightPos = Projectile.Center + direction * d;
                    Lighting.AddLight(
                        lightPos,
                        new Color(255, 165, 35).ToVector3() * (1.25f * visualAlpha));
                }

                if (BeamFadeTimer % 3 == 0)
                    Projectile.netUpdate = true;
            }

            if (BeamFadeTimer >= BeamFadeOutTicks)
                Projectile.Kill();
        }

        private bool TryGetManualTargetAngle(Player player, out float targetAngle)
        {
            if (TryGetOwnedZtargetAngle(out targetAngle))
                return true;

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

        private bool TryGetOwnedZtargetAngle(out float targetAngle)
        {
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile projectile = Main.projectile[i];
                if (projectile.active
                    && projectile.owner == Projectile.owner
                    && projectile.ModProjectile is Ztarget4)
                {
                    targetAngle = (projectile.Center - Projectile.Center).ToRotation();
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

        private void UpdateSurfaceContactState(
            bool hasSolidSurface,
            float contactDistance,
            Vector2 contactPosition,
            Vector2 contactNormal,
            int contactTileX,
            int contactTileY,
            Vector2 direction)
        {
            SurfaceContactDistance = hasSolidSurface ? contactDistance : MaxBeamLength;
            SurfaceContactPosition = hasSolidSurface
                ? contactPosition
                : Projectile.Center + direction * BeamTravelLength;
            SurfaceNormal = hasSolidSurface
                ? contactNormal.SafeNormalize(-direction)
                : -direction;
            SurfaceContactTileX = hasSolidSurface ? contactTileX : -1;
            SurfaceContactTileY = hasSolidSurface ? contactTileY : -1;
            SurfaceContactReached = hasSolidSurface
                && BeamTravelLength + 0.01f >= SurfaceContactDistance;
        }

        /// <summary>
        /// Walk every tile cell crossed by the beam ray. Unlike fixed-distance
        /// sampling, this cannot skip a tile at shallow or sharp angles.
        /// </summary>
        private static bool TryScanForTileContact(
            Vector2 origin,
            Vector2 direction,
            float maxDistance,
            out float contactDistance,
            out Vector2 contactPosition,
            out Vector2 contactNormal,
            out int contactTileX,
            out int contactTileY)
        {
            direction = direction.SafeNormalize(Vector2.UnitX);
            contactDistance = maxDistance;
            contactPosition = origin + direction * maxDistance;
            contactNormal = -direction;
            contactTileX = -1;
            contactTileY = -1;

            int tileX = (int)Math.Floor(origin.X / 16f);
            int tileY = (int)Math.Floor(origin.Y / 16f);
            if (IsSolidBeamTile(tileX, tileY))
            {
                return SetTileContact(
                    origin,
                    direction,
                    maxDistance,
                    0f,
                    tileX,
                    tileY,
                    GetCardinalImpactNormal(direction),
                    out contactDistance,
                    out contactPosition,
                    out contactNormal,
                    out contactTileX,
                    out contactTileY);
            }

            int stepX = direction.X > 0f ? 1 : direction.X < 0f ? -1 : 0;
            int stepY = direction.Y > 0f ? 1 : direction.Y < 0f ? -1 : 0;
            float tDeltaX = stepX == 0 ? float.PositiveInfinity : 16f / Math.Abs(direction.X);
            float tDeltaY = stepY == 0 ? float.PositiveInfinity : 16f / Math.Abs(direction.Y);
            float nextBoundaryX = stepX > 0 ? (tileX + 1) * 16f : tileX * 16f;
            float nextBoundaryY = stepY > 0 ? (tileY + 1) * 16f : tileY * 16f;
            float tMaxX = stepX == 0
                ? float.PositiveInfinity
                : Math.Max(0f, (nextBoundaryX - origin.X) / direction.X);
            float tMaxY = stepY == 0
                ? float.PositiveInfinity
                : Math.Max(0f, (nextBoundaryY - origin.Y) / direction.Y);

            const float cornerEpsilon = 0.0001f;
            while (Math.Min(tMaxX, tMaxY) <= maxDistance)
            {
                bool crossesX = tMaxX <= tMaxY + cornerEpsilon;
                bool crossesY = tMaxY <= tMaxX + cornerEpsilon;

                if (crossesX && crossesY)
                {
                    float distance = Math.Min(tMaxX, tMaxY);
                    int nextTileX = tileX + stepX;
                    int nextTileY = tileY + stepY;
                    Vector2 xNormal = stepX > 0 ? -Vector2.UnitX : Vector2.UnitX;
                    Vector2 yNormal = stepY > 0 ? -Vector2.UnitY : Vector2.UnitY;

                    // At an exact corner the ray touches both neighboring
                    // cells. Count either solid neighbor instead of slipping
                    // diagonally through the seam.
                    if (stepX != 0 && IsSolidBeamTile(nextTileX, tileY))
                    {
                        return SetTileContact(
                            origin, direction, maxDistance, distance,
                            nextTileX, tileY, xNormal,
                            out contactDistance, out contactPosition, out contactNormal,
                            out contactTileX, out contactTileY);
                    }

                    if (stepY != 0 && IsSolidBeamTile(tileX, nextTileY))
                    {
                        return SetTileContact(
                            origin, direction, maxDistance, distance,
                            tileX, nextTileY, yNormal,
                            out contactDistance, out contactPosition, out contactNormal,
                            out contactTileX, out contactTileY);
                    }

                    tileX = nextTileX;
                    tileY = nextTileY;
                    tMaxX += tDeltaX;
                    tMaxY += tDeltaY;

                    if (IsSolidBeamTile(tileX, tileY))
                    {
                        Vector2 cornerNormal = Math.Abs(direction.X) >= Math.Abs(direction.Y)
                            ? xNormal
                            : yNormal;
                        return SetTileContact(
                            origin, direction, maxDistance, distance,
                            tileX, tileY, cornerNormal,
                            out contactDistance, out contactPosition, out contactNormal,
                            out contactTileX, out contactTileY);
                    }
                }
                else if (crossesX)
                {
                    float distance = tMaxX;
                    tileX += stepX;
                    tMaxX += tDeltaX;
                    if (IsSolidBeamTile(tileX, tileY))
                    {
                        Vector2 normal = stepX > 0 ? -Vector2.UnitX : Vector2.UnitX;
                        return SetTileContact(
                            origin, direction, maxDistance, distance,
                            tileX, tileY, normal,
                            out contactDistance, out contactPosition, out contactNormal,
                            out contactTileX, out contactTileY);
                    }
                }
                else
                {
                    float distance = tMaxY;
                    tileY += stepY;
                    tMaxY += tDeltaY;
                    if (IsSolidBeamTile(tileX, tileY))
                    {
                        Vector2 normal = stepY > 0 ? -Vector2.UnitY : Vector2.UnitY;
                        return SetTileContact(
                            origin, direction, maxDistance, distance,
                            tileX, tileY, normal,
                            out contactDistance, out contactPosition, out contactNormal,
                            out contactTileX, out contactTileY);
                    }
                }
            }

            return false;
        }

        private static bool IsSolidBeamTile(int tileX, int tileY)
        {
            if (tileX < 0 || tileX >= Main.maxTilesX || tileY < 0 || tileY >= Main.maxTilesY)
                return false;

            Tile tile = Main.tile[tileX, tileY];
            return tile.HasTile
                && Main.tileSolid[tile.TileType]
                && !TileID.Sets.Platforms[tile.TileType];
        }

        private static Vector2 GetCardinalImpactNormal(Vector2 direction)
        {
            return Math.Abs(direction.X) >= Math.Abs(direction.Y)
                ? (direction.X > 0f ? -Vector2.UnitX : Vector2.UnitX)
                : (direction.Y > 0f ? -Vector2.UnitY : Vector2.UnitY);
        }

        private static bool SetTileContact(
            Vector2 origin,
            Vector2 direction,
            float maxDistance,
            float distance,
            int tileX,
            int tileY,
            Vector2 normal,
            out float contactDistance,
            out Vector2 contactPosition,
            out Vector2 contactNormal,
            out int contactTileX,
            out int contactTileY)
        {
            contactDistance = MathHelper.Clamp(distance, 0f, maxDistance);
            contactPosition = origin + direction * contactDistance;
            contactNormal = normal;
            contactTileX = tileX;
            contactTileY = tileY;
            return true;
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

            if (!SurfaceContactReached)
                return;

            int tileX = SurfaceContactTileX;
            int tileY = SurfaceContactTileY;

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
            Vector2 beamEnd = Projectile.Center + beamDir * BeamTravelLength;

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

            Vector2 beamEnd = Projectile.Center + CurrentAngle.ToRotationVector2() * BeamTravelLength;
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
            // RovaCenter draws the complete source composite in a fixed order.
            return false;
        }

        internal void DrawBodyBehindCore()
        {
            DrawMoltenBeamBody(GetBeamVisualAlpha(), GetBeamWidthScale());
        }

        internal void DrawCoreConnectorOverCore()
        {
            float visualAlpha = GetBeamVisualAlpha();
            if (visualAlpha <= 0.005f)
                return;

            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Vector2 start = Projectile.Center - Main.screenPosition;
            float length = Math.Min(30f, BeamTravelLength);
            float pulse = 0.92f + (float)Math.Sin(Main.GameUpdateCount * 0.31f) * 0.08f;
            float widthScale = GetBeamWidthScale();

            // Only the two yellow layers cross visibly over the core. The
            // orange edge remains behind the core and appears at its perimeter.
            DrawBeamLayer(pixel, start, length, 7f * pulse * widthScale, new Color(255, 207, 40) * (0.9f * visualAlpha));
            DrawBeamLayer(pixel, start, length, 3.5f * pulse * widthScale, new Color(255, 239, 86) * visualAlpha);
        }

        internal void DrawEffectsOverCore()
        {
            float visualAlpha = GetBeamVisualAlpha();
            if (visualAlpha <= 0.005f)
                return;

            DrawCoreSplashGlobs(visualAlpha);
            if (FireUpgrade2Active)
            {
                RovaLavaGlobVisual.DrawStoredColors(
                    LavaGlobs,
                    Main.screenPosition,
                    new Color(255, 214, 70, 235),
                    0.95f * visualAlpha);
            }
            else
            {
                RovaLavaGlobVisual.Draw(
                    LavaGlobs,
                    Main.screenPosition,
                    new Color(255, 66, 8, 220) * visualAlpha,
                    new Color(255, 214, 70, 235) * visualAlpha,
                    0.95f);
            }
        }

        internal Vector2 GetBeamEndpointPosition()
        {
            // The historical tile scan intentionally samples inside the hit
            // tile. Keep collision behavior unchanged, but place the visible
            // rounded head on the resolved tile face so it cannot be buried.
            if (SurfaceContactReached)
                return SurfaceContactPosition;

            return Projectile.Center + CurrentAngle.ToRotationVector2() * BeamTravelLength;
        }

        internal Vector2 GetBeamDirection()
        {
            return CurrentAngle.ToRotationVector2();
        }

        internal Vector2 GetSurfaceContactPosition()
        {
            return SurfaceContactReached
                ? SurfaceContactPosition
                : GetBeamEndpointPosition();
        }

        internal Vector2 GetSurfaceNormal()
        {
            return (SurfaceContactReached ? SurfaceNormal : -CurrentAngle.ToRotationVector2())
                .SafeNormalize(-Vector2.UnitY);
        }

        internal Point GetSurfaceContactTileCoordinates()
        {
            return new Point(SurfaceContactTileX, SurfaceContactTileY);
        }

        internal bool HasSurfaceContact()
        {
            return SurfaceContactReached;
        }

        internal float GetBeamEndpointAlpha()
        {
            return GetBeamVisualAlpha();
        }

        internal float GetBeamEndpointWidthScale()
        {
            return GetBeamWidthScale();
        }

        internal bool IsBeamEnding => BeamEnding;

        private void DrawMoltenBeamBody(float visualAlpha, float widthScale)
        {
            if (visualAlpha <= 0.005f)
                return;

            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Vector2 start = Projectile.Center - Main.screenPosition;
            float pulse = 0.92f + (float)Math.Sin(Main.GameUpdateCount * 0.31f) * 0.08f;

            // Every beam layer starts at the exact yellow nucleus. There is no
            // separate rounded cap, so the overlap stays seamless and narrow.
            DrawBeamLayer(pixel, start, BeamTravelLength, 14f * pulse * widthScale, new Color(255, 148, 10) * (0.94f * visualAlpha));
            DrawBeamLayer(pixel, start, BeamTravelLength, 8f * pulse * widthScale, new Color(255, 205, 38) * (0.98f * visualAlpha));
            DrawBeamLayer(pixel, start, BeamTravelLength, 3.5f * pulse * widthScale, new Color(255, 239, 86) * visualAlpha);
        }

        private void UpdateCoreSplashGlobs(Vector2 beamDirection, bool allowSpawn = true)
        {
            for (int i = ImpactGlobZones.Count - 1; i >= 0; i--)
            {
                RovaBeamImpactZone zone = ImpactGlobZones[i];
                zone.TimeLeft--;
                if (zone.TimeLeft <= 0)
                    ImpactGlobZones.RemoveAt(i);
                else
                    ImpactGlobZones[i] = zone;
            }

            if (allowSpawn && !InitialCoreSplashPlayed)
            {
                InitialCoreSplashPlayed = true;
                SpawnCoreSplashGlobs(beamDirection, 11);
            }

            if (allowSpawn && CoreSplashGlobs.Count < 24 && Main.rand.NextBool(4))
                SpawnCoreSplashGlobs(beamDirection, 1);

            for (int i = CoreSplashGlobs.Count - 1; i >= 0; i--)
            {
                RovaCoreSplashGlob glob = CoreSplashGlobs[i];
                glob.Age++;
                glob.Position += glob.Velocity;
                glob.Velocity *= 0.955f;
                glob.Velocity.Y += 0.015f;

                if (glob.Age >= glob.Life)
                    CoreSplashGlobs.RemoveAt(i);
                else
                    CoreSplashGlobs[i] = glob;
            }
        }

        private void TrySpawnBeamImpactGlobs(NPC target, Vector2 beamDirection)
        {
            Vector2 direction = beamDirection.SafeNormalize(Vector2.UnitX);
            float distanceAlongBeam = Vector2.Dot(target.Center - Projectile.Center, direction);
            distanceAlongBeam = MathHelper.Clamp(distanceAlongBeam, 0f, BeamTravelLength);
            Vector2 impactPoint = Projectile.Center + direction * distanceAlongBeam;
            float mergeRangeSquared = ImpactGlobMergeRange * ImpactGlobMergeRange;

            foreach (RovaBeamImpactZone zone in ImpactGlobZones)
            {
                if (Vector2.DistanceSquared(zone.Position, impactPoint) <= mergeRangeSquared)
                    return;
            }

            int availableGlobSlots = MaxCoreSplashGlobs - CoreSplashGlobs.Count;
            if (availableGlobSlots <= 0)
                return;

            ImpactGlobZones.Add(new RovaBeamImpactZone
            {
                Position = impactPoint,
                TimeLeft = ImpactGlobCooldownTicks
            });

            SpawnCoreSplashGlobs(
                direction,
                Math.Min(ImpactGlobCount, availableGlobSlots),
                impactPoint);
        }

        private void SpawnCoreSplashGlobs(
            Vector2 beamDirection,
            int count,
            Vector2? spawnPoint = null)
        {
            Vector2 direction = beamDirection.SafeNormalize(Vector2.UnitX);
            Vector2 sideways = direction.RotatedBy(MathHelper.PiOver2);
            Vector2 exitPoint = spawnPoint ?? Projectile.Center;

            for (int i = 0; i < count; i++)
            {
                float spread = Main.rand.NextFloat(-0.58f, 0.58f);
                Vector2 velocity = direction.RotatedBy(spread) * Main.rand.NextFloat(2.1f, 6.2f);
                CoreSplashGlobs.Add(new RovaCoreSplashGlob
                {
                    Position = exitPoint
                        + direction * Main.rand.NextFloat(0f, 4f)
                        + sideways * Main.rand.NextFloat(-2.5f, 2.5f),
                    Velocity = velocity,
                    Age = 0f,
                    Life = Main.rand.NextFloat(20f, 42f),
                    Size = Main.rand.NextFloat(1.8f, 5.2f),
                    Color = GetCoreSplashColor()
                });
            }
        }

        internal static Color GetCoreSplashColor()
        {
            int roll = Main.rand.Next(10);
            if (roll == 0)
                return new Color(255, 170, 22); // rare yellow-orange
            if (roll <= 3)
                return new Color(255, 253, 210); // white-hot

            return new Color(255, 229, 66); // yellow
        }

        private void DrawCoreSplashGlobs(float visualAlpha)
        {
            if (visualAlpha <= 0.005f)
                return;

            Texture2D pixel = TextureAssets.MagicPixel.Value;
            foreach (RovaCoreSplashGlob glob in CoreSplashGlobs)
            {
                float progress = MathHelper.Clamp(glob.Age / glob.Life, 0f, 1f);
                float envelope = MathHelper.Clamp(
                    Math.Min(progress * 7f, (1f - progress) * 3.5f),
                    0f,
                    1f);
                Vector2 position = glob.Position - Main.screenPosition;
                float size = glob.Size * MathHelper.Lerp(1f, 0.62f, progress);

                RovaLavaGlobVisual.DrawSoftGlob(pixel, position, size, glob.Color * (envelope * visualAlpha));
                RovaLavaGlobVisual.DrawSoftGlob(
                    pixel,
                    position,
                    size * 0.42f,
                    CoreSplashInnerColor * (envelope * 0.88f * visualAlpha));
            }
        }

        private float GetBeamVisualAlpha()
        {
            if (!BeamEnding)
                return 1f;

            float progress = MathHelper.Clamp(
                BeamFadeTimer / (float)BeamFadeOutTicks,
                0f,
                1f);
            float alpha = 1f - progress;

            // Keep the main fade smooth while making the final taper visibly
            // flicker instead of simply snapping off.
            if (BeamFadeTimer % 5 == 2)
                alpha *= 0.42f;
            else if (BeamFadeTimer % 5 == 4)
                alpha *= 0.18f;

            return MathHelper.Clamp(alpha, 0f, 1f);
        }

        private float GetBeamWidthScale()
        {
            if (!BeamEnding)
                return 1f;

            float progress = MathHelper.Clamp(
                BeamFadeTimer / (float)BeamFadeOutTicks,
                0f,
                1f);
            return MathHelper.Lerp(1f, 0.05f, progress);
        }

        private void DrawBeamLayer(Texture2D pixel, Vector2 start, float length, float width, Color color)
        {
            DrawBeamSegment(pixel, start, length, width, color);
        }

        private void DrawBeamSegment(Texture2D pixel, Vector2 start, float length, float width, Color color)
        {
            if (length <= 0f || width <= 0f)
                return;

            Main.spriteBatch.Draw(
                pixel,
                start,
                new Rectangle(0, 0, 1, 1),
                color,
                CurrentAngle,
                new Vector2(0f, 0.5f),
                new Vector2(length, width),
                SpriteEffects.None,
                0f);
        }

        public override void Kill(int timeLeft)
        {
            if (EndpointProjectileIndex >= 0
                && EndpointProjectileIndex < Main.maxProjectiles
                && Main.projectile[EndpointProjectileIndex].active)
            {
                Main.projectile[EndpointProjectileIndex].Kill();
            }

            foreach (int pillarIndex in FirePillarProjectileIndices)
            {
                if (pillarIndex >= 0
                    && pillarIndex < Main.maxProjectiles
                    && Main.projectile[pillarIndex].active)
                {
                    Main.projectile[pillarIndex].netUpdate = true;
                }
            }

            if (!Main.dedServ && BeamLoopSound.IsPlaying)
            {
                BeamLoopSound.Stop();
            }

        }
    }

}
