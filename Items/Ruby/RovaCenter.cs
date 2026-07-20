using Microsoft.Xna.Framework;
using SariaMod.Buffs;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Audio;
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
        private int AutoTargetNpcIndex = -1;
        private bool DespawnEruptionSpawned;
        private int _idleTimer;
        private int ZtargetProjectileIndex = -1;
        private int ActiveBeamProjectileIndex = -1;

        private const float BeamRange = 2000f;
        private const float TrackTurnSpeed = 0.04f;
        private const float PortalAuraRadius = 112f;
        private const int PortalAuraBuffDuration = 4;
        private const float AutoSweepTotalAngle = 6.8067847f; // 390 degrees: one full turn plus 30 degrees
        // ChargeFire1.wav is about 102 ticks long. ChargeFire2 starts after it
        // fully ends plus 1 second, then the beam waits 30 frames after
        // ChargeFire2 finishes before firing.
        private const int ChargeFire1SoundDurationTicks = 102;
        private const int DelayAfterChargeFire1Ticks = 60;
        private const int ChargeFire2StartTicks = ChargeFire1SoundDurationTicks + DelayAfterChargeFire1Ticks;
        private const int ChargeFire2SoundDurationTicks = 36;
        private const int BeamDelayAfterChargeFire2Ticks = ChargeFire2SoundDurationTicks + 30;
        private const int ChargeStartupTicks = ChargeFire2StartTicks + BeamDelayAfterChargeFire2Ticks;
        private const int DesiredBeamIntervalTicks = 10 * 60; // 10 seconds from one beam ending to the next beam firing
        private const int MinimumPostBeamChargeDelayTicks = 360; // 6 seconds before a new charge may begin
        private const int NextChargeDelayTicks = DesiredBeamIntervalTicks - ChargeStartupTicks;
        private const int OverheatedScreenDurationTicks = 180;
        private bool ChargeFire2Started;
        private bool ChargeFire1PresentationStarted;
        private bool ChargeFire2SoundPlayed;
        private bool OwnerBeamSpawnPending;
        private bool AwaitingBeamConfirmation;
        private bool HasActiveBeamThisTick;
        private int BeamSpawnRequestRetryTimer;
        private const int BeamSpawnRequestRetryTicks = 15;
        public bool ChargeFire2StartedValue => ChargeFire2Started;
        internal bool HasActiveBeamValue => HasActiveBeamThisTick;
        internal bool HasRovaSentryPersistenceUpgrade => Projectile.ai[0] >= 0.5f;
        internal float ScreenHeatIntensity
        {
            get
            {
                if (!BeamFired)
                    return 0f;

                if (HasActiveBeamThisTick)
                    return 1f;

                if (BeamCooldownTimer <= 0 || BeamCooldownTimer >= OverheatedScreenDurationTicks)
                    return 0f;

                return 1f - BeamCooldownTimer / (float)OverheatedScreenDurationTicks;
            }
        }

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
            writer.Write(AutoTargetNpcIndex);
            writer.Write(DespawnEruptionSpawned);
            writer.Write(_idleTimer);
            writer.Write(ChargeFire2Started);
            writer.Write(AwaitingBeamConfirmation);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            bool previousBeamFired = BeamFired;
            StateTimer = reader.ReadInt32();
            BeamCooldownTimer = reader.ReadInt32();
            AutoFireTimer = reader.ReadInt32();
            CurrentAngle = reader.ReadSingle();
            TargetAngle = reader.ReadSingle();
            BeamFired = reader.ReadBoolean();
            InitialAimSet = reader.ReadBoolean();
            NextBeamAutoRotates = reader.ReadBoolean();
            AutoTargetNpcIndex = reader.ReadInt32();
            DespawnEruptionSpawned = reader.ReadBoolean();
            _idleTimer = reader.ReadInt32();
            ChargeFire2Started = reader.ReadBoolean();
            AwaitingBeamConfirmation = reader.ReadBoolean();

            if (Main.netMode == NetmodeID.MultiplayerClient && Main.myPlayer == Projectile.owner)
            {
                if (previousBeamFired && !BeamFired)
                {
                    ChargeFire1PresentationStarted = false;
                    ChargeFire2SoundPlayed = false;
                    OwnerBeamSpawnPending = false;
                }

                if (BeamFired && AwaitingBeamConfirmation)
                    OwnerBeamSpawnPending = true;
                else if (!AwaitingBeamConfirmation)
                    OwnerBeamSpawnPending = false;
            }
        }

        public override void SetDefaults()
        {
            Projectile.width = 86;
            Projectile.height = 86;
            Projectile.netImportant = true;
            Projectile.alpha = 0;
            Projectile.hide = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 6000; // Base lifetime when fire upgrade 2 is inactive.
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
            bool isGameplayAuthority = Main.netMode != NetmodeID.MultiplayerClient;
            StateTimer++;
            Projectile.rotation += 0.025f;

            // Start each local presentation once instead of depending on a
            // network packet arriving before one exact timer tick.
            if (!ChargeFire1PresentationStarted && StateTimer >= 1 && Main.myPlayer == Projectile.owner)
            {
                ChargeFire1PresentationStarted = true;
                SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/ChargeFire1"), Projectile.Center);
                RestartRovaRing();
            }

            if (isGameplayAuthority && !ChargeFire2Started && StateTimer >= ChargeFire2StartTicks)
            {
                ChargeFire2Started = true;
                RovaProjectileLink.SyncState(Projectile);
            }

            if (ChargeFire2Started && !ChargeFire2SoundPlayed && Main.myPlayer == Projectile.owner)
            {
                ChargeFire2SoundPlayed = true;
                SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/ChargeFire2"), Projectile.Center);
            }

            // Kill immediately if owner is dead or Saria minion is gone
            if (isGameplayAuthority && (player.dead || !player.active))
            {
                KillAllBeams();
                Projectile.Kill();
                return;
            }
            if (isGameplayAuthority
                && player.ownedProjectileCounts[ModContent.ProjectileType<Saria>()] <= 0)
            {
                KillAllBeams();
                Projectile.Kill();
                return;
            }

            if (TryHandleOwnerUnsummon(player))
            {
                return;
            }

            // Fire upgrade 2 removes natural expiration. Explicit owner
            // unsummoning and Saria's removal are handled above and still kill
            // the center immediately.
            if (HasRovaSentryPersistenceUpgrade)
                Projectile.timeLeft = Math.Max(Projectile.timeLeft, 2);

            RovaLavaCoreVisual.Update(Projectile);

            // Check if player is holding left-click with the HealBall while RovaCenter exists.
            bool rightMouseBlocksCharge = Main.netMode == NetmodeID.SinglePlayer && Main.mouseRight;
            bool playerCharging = player.channel
                && player.HeldItem.type == ModContent.ItemType<HealBall>()
                && !rightMouseBlocksCharge;

            // Check if player is right-click targeting (ztarget exists)
            bool hasRightClickTarget = player.HasMinionAttackTargetNPC;

            // Look for ztarget4 for manual override
            int foundZtarget4 = FindOwnedZtarget4();

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
                if (isGameplayAuthority)
                {
                    RovaProjectileLink.SyncState(Projectile);
                }
            }

            // Check if a beam is currently alive
            bool hasActiveBeam = FindAttachedBeamVisual() != null;
            HasActiveBeamThisTick = hasActiveBeam;

            if (hasActiveBeam)
                Projectile.timeLeft = Math.Max(Projectile.timeLeft, 2);

            // The owning client creates player-owned projectiles so Terraria can
            // assign an identity that is safe to synchronize. Keep requesting
            // the beam until the server actually observes the linked projectile.
            if (isGameplayAuthority && AwaitingBeamConfirmation)
            {
                if (hasActiveBeam)
                {
                    AwaitingBeamConfirmation = false;
                    BeamSpawnRequestRetryTimer = 0;
                    RovaProjectileLink.SyncState(Projectile);
                }
                else if (Main.netMode == NetmodeID.Server)
                {
                    BeamSpawnRequestRetryTimer++;
                    if (BeamSpawnRequestRetryTimer >= BeamSpawnRequestRetryTicks)
                    {
                        BeamSpawnRequestRetryTimer = 0;
                        RovaProjectileLink.SyncState(Projectile);
                    }
                }
            }

            // A beam has to finish before its cooldown can advance. This timer is
            // intentionally measured from the end of the last beam, so holding
            // Ztarget4 cannot immediately fire another beam.
            if (BeamFired && !hasActiveBeam && !AwaitingBeamConfirmation)
            {
                BeamCooldownTimer++;
            }

            // Start the next charge at 10 seconds minus the charge sequence.
            // The resulting 372-tick delay is also later than the six-second
            // minimum requested after a beam ends.
            bool manualAttackRequested = foundZtarget4 >= 0 && playerCharging;
            bool canBeginNextCharge = isGameplayAuthority
                && BeamFired
                && !hasActiveBeam
                && BeamCooldownTimer >= MinimumPostBeamChargeDelayTicks
                && BeamCooldownTimer >= NextChargeDelayTicks;
            NPC visibleAutoTarget = canBeginNextCharge && !manualAttackRequested
                ? FindVisibleAutoTarget()
                : null;
            bool hasVisibleAutoTarget = visibleAutoTarget != null;
            bool autoAttackRequested = !manualAttackRequested && hasVisibleAutoTarget;
            if (canBeginNextCharge
                && (manualAttackRequested || autoAttackRequested))
            {
                BeginChargeSequence();

                if (autoAttackRequested)
                {
                    AutoTargetNpcIndex = visibleAutoTarget.whoAmI;
                    UpdateAutomaticSweepAim(visibleAutoTarget);
                    NextBeamAutoRotates = true;
                }

                RovaProjectileLink.SyncState(Projectile);
            }

            // Auto-fire can only sweep clockwise. Keep its starting angle
            // counterclockwise of the chosen enemy by exactly the angle the
            // beam will rotate while its head travels to that distance.
            if (isGameplayAuthority && NextBeamAutoRotates && !BeamFired && !hasActiveBeam)
            {
                NPC automaticTarget = GetTrackedAutomaticTarget();
                if (automaticTarget != null)
                    UpdateAutomaticSweepAim(automaticTarget);
            }

            // STATE: FIRING (State 0)
            // Beam fires one second after ChargeFire2 finishes.
            if (isGameplayAuthority
                && ChargeFire2Started
                && StateTimer > ChargeStartupTicks
                && !BeamFired
                && !hasActiveBeam)
            {
                // A manual target can disappear while the shared startup is
                // still playing. Decide the fallback here, before the beam is
                // created, so this changes only the upcoming shot. Losing
                // Ztarget4 after a beam already exists keeps its established
                // straight-ahead behavior in RovaBeam.
                if (foundZtarget4 < 0 && !NextBeamAutoRotates)
                {
                    NPC automaticTarget = GetTrackedAutomaticTarget();
                    if (automaticTarget != null)
                        UpdateAutomaticSweepAim(automaticTarget);

                    NextBeamAutoRotates = true;
                    RovaProjectileLink.SyncState(Projectile);
                }

                if (Main.netMode == NetmodeID.SinglePlayer)
                    FireBeam();

                BeamFired = true;
                AwaitingBeamConfirmation = Main.netMode == NetmodeID.Server;
                BeamSpawnRequestRetryTimer = 0;
                RovaProjectileLink.SyncState(Projectile);
            }

            if (OwnerBeamSpawnPending
                && Main.netMode == NetmodeID.MultiplayerClient
                && Main.myPlayer == Projectile.owner)
            {
                OwnerBeamSpawnPending = !FireBeam();
            }

            // STATE: AUTO-ROTATE (while auto-fire beam is active)
            if (isGameplayAuthority && BeamFired && hasActiveBeam && NextBeamAutoRotates)
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

                // The shared charge sequence fires the beam when it completes.
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

                }
            }


            // RovaCenter remains until the owner clicks it with the HealBall.
            _idleTimer = 0;
            // Do not preheat the area during the charge-up. The center begins
            // radiating only when its beam actually fires, then continues
            // briefly while the stored overheat is being discharged.
            if (ScreenHeatIntensity > 0f && StateTimer % 15 == 0)
            {
                if (Main.netMode == NetmodeID.Server)
                {
                    TileHeatNetworking.SendRadiusHeatPacket(
                        Projectile.Center,
                        100f,
                        TileHeatManager.DefaultHeatDuration,
                        Projectile.owner,
                        Projectile.damage);
                }
                else if (Main.netMode == NetmodeID.SinglePlayer)
                {
                    TileHeatManager.ApplyHeatInRadius(
                        Projectile.Center,
                        100f,
                        TileHeatManager.DefaultHeatDuration,
                        Projectile.owner,
                        Projectile.damage);
                }
            }

            if (StateTimer % 4 == 0)
            {
                ApplyPortalAura();
            }

        }

        private bool FireBeam()
        {
            int beamDamage = Math.Max(1, (int)(Projectile.damage));
            Vector2 beamVelocity = CurrentAngle.ToRotationVector2();

            if (Main.netMode != NetmodeID.SinglePlayer
                && (Main.netMode != NetmodeID.MultiplayerClient || Main.myPlayer != Projectile.owner))
            {
                return false;
            }

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile candidate = Main.projectile[i];
                if (candidate.active
                    && candidate.owner == Projectile.owner
                    && candidate.ModProjectile is RovaBeam
                    && RovaProjectileLink.Matches<RovaCenter>(
                        Projectile,
                        candidate.owner,
                        (int)candidate.ai[0]))
                {
                    if (Main.netMode == NetmodeID.MultiplayerClient)
                        candidate.netUpdate = true;

                    return true;
                }
            }

            int beamIndex = Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                Projectile.Center,
                beamVelocity * 2f,
                ModContent.ProjectileType<RovaBeam>(),
                beamDamage,
                Projectile.knockBack,
                Projectile.owner,
                RovaProjectileLink.GetHandle(Projectile), // ai[0] = RovaCenter network handle
                NextBeamAutoRotates ? AutoSweepTotalAngle : 0f // ai[1] = remaining auto-sweep radians
            );

            if (beamIndex < 0 || beamIndex >= Main.maxProjectiles)
                return false;

            if (Main.netMode == NetmodeID.MultiplayerClient)
                Main.projectile[beamIndex].netUpdate = true;

            return true;
        }

        private void ApplyPortalAura()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player target = Main.player[i];
                if (target == null || !target.active || target.dead)
                    continue;

                if (Vector2.DistanceSquared(target.Center, Projectile.Center)
                    > PortalAuraRadius * PortalAuraRadius)
                    continue;

                if (!TileHeatManager.IsPlayerFireProtected(target))
                {
                    target.buffImmune[ModContent.BuffType<Burning2>()] = false;
                    target.AddBuff(ModContent.BuffType<Burning2>(), PortalAuraBuffDuration, quiet: false);
                }
            }

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC target = Main.npc[i];
                if (target == null || !target.active || target.friendly || target.lifeMax <= 0 || target.dontTakeDamage)
                    continue;

                if (Vector2.DistanceSquared(target.Center, Projectile.Center)
                    > PortalAuraRadius * PortalAuraRadius)
                    continue;

                if (!target.buffImmune[ModContent.BuffType<Burning2>()])
                {
                    target.AddBuff(ModContent.BuffType<Burning2>(), PortalAuraBuffDuration);
                }
            }
        }

        private NPC FindVisibleAutoTarget()
        {
            NPC nearestEnemy = null;
            float nearestDistanceSquared = BeamRange * BeamRange;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active)
                    continue;

                bool isTargetDummy = npc.type == NPCID.TargetDummy;
                if (!isTargetDummy && (npc.friendly || npc.lifeMax <= 0 || npc.dontTakeDamage))
                    continue;

                float distanceSquared = Vector2.DistanceSquared(Projectile.Center, npc.Center);
                if (distanceSquared >= nearestDistanceSquared)
                    continue;

                if (!Collision.CanHitLine(Projectile.Center, 1, 1, npc.Center, 1, 1))
                    continue;

                nearestDistanceSquared = distanceSquared;
                nearestEnemy = npc;
            }

            return nearestEnemy;
        }

        private NPC GetTrackedAutomaticTarget()
        {
            if (AutoTargetNpcIndex >= 0 && AutoTargetNpcIndex < Main.maxNPCs)
            {
                NPC trackedTarget = Main.npc[AutoTargetNpcIndex];
                if (IsValidAutomaticTarget(trackedTarget))
                    return trackedTarget;
            }

            NPC fallbackTarget = FindVisibleAutoTarget();
            if (fallbackTarget != null)
            {
                AutoTargetNpcIndex = fallbackTarget.whoAmI;
                return fallbackTarget;
            }

            AutoTargetNpcIndex = -1;
            return null;
        }

        private bool IsValidAutomaticTarget(NPC npc)
        {
            if (npc == null || !npc.active)
                return false;

            bool isTargetDummy = npc.type == NPCID.TargetDummy;
            if (!isTargetDummy && (npc.friendly || npc.lifeMax <= 0 || npc.dontTakeDamage))
                return false;

            if (Vector2.DistanceSquared(Projectile.Center, npc.Center) > BeamRange * BeamRange)
                return false;

            return Collision.CanHitLine(Projectile.Center, 1, 1, npc.Center, 1, 1);
        }

        private void UpdateAutomaticSweepAim(NPC target)
        {
            Vector2 predictedPosition = target.Center;
            float travelTicks = 1f;

            // Two passes are enough to account for the fact that predicting
            // movement changes the distance, which in turn changes travel time.
            for (int i = 0; i < 2; i++)
            {
                float distance = Vector2.Distance(Projectile.Center, predictedPosition);
                travelTicks = RovaBeam.GetTravelTicksToDistance(distance);
                predictedPosition = target.Center + target.velocity * travelTicks;
            }

            Vector2 toPredictedTarget = predictedPosition - Projectile.Center;
            float predictedDistance = Math.Min(BeamRange, toPredictedTarget.Length());
            if (predictedDistance <= 0.001f)
                return;

            float enemyAngle = toPredictedTarget.ToRotation();
            float clockwiseLead = RovaBeam.GetClockwiseAutoLead(predictedDistance);
            CurrentAngle = enemyAngle - clockwiseLead;
            TargetAngle = enemyAngle;
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

        public override void DrawBehind(
            int index,
            List<int> behindNPCsAndTiles,
            List<int> behindNPCs,
            List<int> behindProjectiles,
            List<int> overPlayers,
            List<int> overWiresUI)
        {
            behindProjectiles.Add(index);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            RovaBeam beam = FindAttachedBeamVisual();
            beam?.DrawBodyBehindCore();
            RovaLavaCoreVisual.Draw(Projectile);
            beam?.DrawCoreConnectorOverCore();
            beam?.DrawEffectsOverCore();
            return false;
        }

        private RovaBeam FindAttachedBeamVisual()
        {
            if (ActiveBeamProjectileIndex >= 0
                && ActiveBeamProjectileIndex < Main.maxProjectiles)
            {
                Projectile cached = Main.projectile[ActiveBeamProjectileIndex];
                if (cached.active
                    && cached.owner == Projectile.owner
                    && cached.ModProjectile is RovaBeam cachedBeam
                    && RovaProjectileLink.Matches<RovaCenter>(
                        Projectile,
                        cached.owner,
                        (int)cached.ai[0]))
                {
                    return cachedBeam;
                }
            }

            ActiveBeamProjectileIndex = -1;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile projectile = Main.projectile[i];
                if (projectile.active
                    && projectile.owner == Projectile.owner
                    && projectile.ModProjectile is RovaBeam beam
                    && RovaProjectileLink.Matches<RovaCenter>(
                        Projectile,
                        projectile.owner,
                        (int)projectile.ai[0]))
                {
                    ActiveBeamProjectileIndex = i;
                    return beam;
                }
            }

            return null;
        }

        private int FindOwnedZtarget4()
        {
            if (ZtargetProjectileIndex >= 0 && ZtargetProjectileIndex < Main.maxProjectiles)
            {
                Projectile cached = Main.projectile[ZtargetProjectileIndex];
                if (cached.active
                    && cached.owner == Projectile.owner
                    && cached.ModProjectile is Ztarget4)
                {
                    return ZtargetProjectileIndex;
                }
            }

            ZtargetProjectileIndex = -1;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile projectile = Main.projectile[i];
                if (projectile.active
                    && projectile.owner == Projectile.owner
                    && projectile.ModProjectile is Ztarget4)
                {
                    ZtargetProjectileIndex = i;
                    return i;
                }
            }

            return -1;
        }

        private void KillAllBeams()
        {
            for (int i = 0; i < 1000; i++)
            {
                Projectile beamProjectile = Main.projectile[i];
                if (beamProjectile.active
                    && beamProjectile.ModProjectile is RovaBeam
                    && beamProjectile.owner == Projectile.owner
                    && RovaProjectileLink.Matches<RovaCenter>(
                        Projectile,
                        beamProjectile.owner,
                        (int)beamProjectile.ai[0]))
                {
                    beamProjectile.Kill();
                }

                if (Main.projectile[i].active
                    && Main.projectile[i].ModProjectile is RovaRing
                    && Main.projectile[i].owner == Projectile.owner
                    && RovaProjectileLink.Matches<RovaCenter>(
                        Projectile,
                        Main.projectile[i].owner,
                        (int)Main.projectile[i].ai[0]))
                {
                    Main.projectile[i].Kill();
                }
            }
        }

        private void BeginChargeSequence()
        {
            StateTimer = 0;
            BeamCooldownTimer = 0;
            ChargeFire2Started = false;
            BeamFired = false;
            AutoFireTimer = 0;
            NextBeamAutoRotates = false;
            AutoTargetNpcIndex = -1;
            ChargeFire1PresentationStarted = false;
            ChargeFire2SoundPlayed = false;
            OwnerBeamSpawnPending = false;
            AwaitingBeamConfirmation = false;
            BeamSpawnRequestRetryTimer = 0;
        }

        private void RestartRovaRing()
        {
            if (Main.myPlayer != Projectile.owner)
                return;

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                if (Main.projectile[i].active
                    && Main.projectile[i].ModProjectile is RovaRing
                    && Main.projectile[i].owner == Projectile.owner
                    && RovaProjectileLink.Matches<RovaCenter>(
                        Projectile,
                        Main.projectile[i].owner,
                        (int)Main.projectile[i].ai[0]))
                {
                    Main.projectile[i].Kill();
                }
            }

            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                Projectile.Center,
                Vector2.Zero,
                ModContent.ProjectileType<RovaRing>(),
                0,
                0f,
                Projectile.owner,
                RovaProjectileLink.GetHandle(Projectile));
        }

        public override void Kill(int timeLeft)
        {
            RovaLavaCoreVisual.Remove(Projectile.whoAmI);
            KillAllBeams();

            bool canSpawnEruption = Main.netMode == NetmodeID.SinglePlayer
                || (Main.netMode == NetmodeID.MultiplayerClient && Main.myPlayer == Projectile.owner);
            int eruptionType = ModContent.ProjectileType<Explosion2>();
            if (!DespawnEruptionSpawned
                && canSpawnEruption
                && EruptionProjectileLimitGlobal.CanSpawn(Projectile.owner, eruptionType))
            {
                DespawnEruptionSpawned = true;
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center.X,
                    Projectile.Center.Y,
                    0f,
                    0f,
                    eruptionType,
                    Math.Max(1, Projectile.damage),
                    Projectile.knockBack,
                    Projectile.owner,
                    -1f, // ai[0] = RovaCenter eruption lock sentinel
                    RovaProjectileLink.GetHandle(Projectile));
            }

            // Heated tiles fade naturally so other players' heat fields are not cleared.
        }
    }
}
