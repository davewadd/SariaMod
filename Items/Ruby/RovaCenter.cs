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
        private bool ChargeFire2Started;
        public bool ChargeFire2StartedValue => ChargeFire2Started;
        internal bool HasRovaSentryPersistenceUpgrade => Projectile.ai[0] >= 0.5f;

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
            AutoTargetNpcIndex = reader.ReadInt32();
            DespawnEruptionSpawned = reader.ReadBoolean();
            _idleTimer = reader.ReadInt32();
            ChargeFire2Started = reader.ReadBoolean();
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
            StateTimer++;
            Projectile.rotation += 0.025f;

            // Start each attack with ChargeFire1 and fade the core and centered RovaRing in.
            if (StateTimer == 1)
            {
                if (Main.myPlayer == Projectile.owner)
                {
                    SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/ChargeFire1"), Projectile.Center);
                }

                RestartRovaRing();

            }

            if (StateTimer == ChargeFire2StartTicks)
            {
                ChargeFire2Started = true;
                if (Main.myPlayer == Projectile.owner)
                {
                    SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/ChargeFire2"), Projectile.Center);
                }
                Projectile.netUpdate = true;
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

            // Fire upgrade 2 removes natural expiration. Explicit owner
            // unsummoning and Saria's removal are handled above and still kill
            // the center immediately.
            if (HasRovaSentryPersistenceUpgrade)
                Projectile.timeLeft = Math.Max(Projectile.timeLeft, 2);

            RovaLavaCoreVisual.Update(Projectile);

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
                if (Main.netMode == NetmodeID.Server || Main.myPlayer == Projectile.owner)
                {
                    Projectile.netUpdate = true;
                }
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

            if (hasActiveBeam)
                Projectile.timeLeft = Math.Max(Projectile.timeLeft, 2);

            NPC visibleAutoTarget = FindVisibleAutoTarget();
            bool hasVisibleAutoTarget = visibleAutoTarget != null;

            // A beam has to finish before its cooldown can advance. This timer is
            // intentionally measured from the end of the last beam, so holding
            // Ztarget4 cannot immediately fire another beam.
            if (BeamFired && !hasActiveBeam)
            {
                BeamCooldownTimer++;
            }

            // Start the next charge at 10 seconds minus the charge sequence.
            // The resulting 372-tick delay is also later than the six-second
            // minimum requested after a beam ends.
            bool manualAttackRequested = foundZtarget4 >= 0 && playerCharging;
            bool autoAttackRequested = !manualAttackRequested && hasVisibleAutoTarget;
            if (BeamFired
                && !hasActiveBeam
                && BeamCooldownTimer >= MinimumPostBeamChargeDelayTicks
                && BeamCooldownTimer >= NextChargeDelayTicks
                && (manualAttackRequested || autoAttackRequested))
            {
                BeginChargeSequence();

                if (autoAttackRequested)
                {
                    AutoTargetNpcIndex = visibleAutoTarget.whoAmI;
                    UpdateAutomaticSweepAim(visibleAutoTarget);
                    NextBeamAutoRotates = true;
                }

                Projectile.netUpdate = true;
            }

            // Auto-fire can only sweep clockwise. Keep its starting angle
            // counterclockwise of the chosen enemy by exactly the angle the
            // beam will rotate while its head travels to that distance.
            if (NextBeamAutoRotates && !BeamFired && !hasActiveBeam)
            {
                NPC automaticTarget = GetTrackedAutomaticTarget(visibleAutoTarget);
                if (automaticTarget != null)
                    UpdateAutomaticSweepAim(automaticTarget);
            }

            // STATE: FIRING (State 0)
            // Beam fires one second after ChargeFire2 finishes.
            if (ChargeFire2Started && StateTimer > ChargeStartupTicks && !BeamFired && !hasActiveBeam)
            {
                // A manual target can disappear while the shared startup is
                // still playing. Decide the fallback here, before the beam is
                // created, so this changes only the upcoming shot. Losing
                // Ztarget4 after a beam already exists keeps its established
                // straight-ahead behavior in RovaBeam.
                if (foundZtarget4 < 0 && !NextBeamAutoRotates)
                {
                    NPC automaticTarget = GetTrackedAutomaticTarget(visibleAutoTarget);
                    if (automaticTarget != null)
                        UpdateAutomaticSweepAim(automaticTarget);

                    NextBeamAutoRotates = true;
                    Projectile.netUpdate = true;
                }

                FireBeam();
                BeamFired = true;
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
            if (StateTimer % 15 == 0)
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

        private void FireBeam()
        {
            int beamDamage = Math.Max(1, (int)(Projectile.damage));
            Vector2 beamVelocity = CurrentAngle.ToRotationVector2();

            if (Main.netMode == NetmodeID.Server || Main.netMode == NetmodeID.SinglePlayer)
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

        private void ApplyPortalAura()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player target = Main.player[i];
                if (target == null || !target.active || target.dead)
                    continue;

                if (Vector2.Distance(target.Center, Projectile.Center) > PortalAuraRadius)
                    continue;

                if (!TileHeatManager.IsPlayerFireProtected(target))
                {
                    target.buffImmune[BuffID.OnFire] = false;
                    target.AddBuff(BuffID.OnFire, PortalAuraBuffDuration, quiet: false);
                }
            }

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC target = Main.npc[i];
                if (target == null || !target.active || target.friendly || target.lifeMax <= 0 || target.dontTakeDamage)
                    continue;

                if (Vector2.Distance(target.Center, Projectile.Center) > PortalAuraRadius)
                    continue;

                if (!target.buffImmune[BuffID.OnFire])
                {
                    target.AddBuff(BuffID.OnFire, PortalAuraBuffDuration);
                }
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

        private NPC GetTrackedAutomaticTarget(NPC fallbackTarget)
        {
            if (AutoTargetNpcIndex >= 0 && AutoTargetNpcIndex < Main.maxNPCs)
            {
                NPC trackedTarget = Main.npc[AutoTargetNpcIndex];
                if (IsValidAutomaticTarget(trackedTarget))
                    return trackedTarget;
            }

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
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile projectile = Main.projectile[i];
                if (projectile.active
                    && projectile.owner == Projectile.owner
                    && projectile.ModProjectile is RovaBeam beam
                    && (int)projectile.ai[0] == Projectile.whoAmI)
                {
                    return beam;
                }
            }

            return null;
        }

        private void KillAllBeams()
        {
            for (int i = 0; i < 1000; i++)
            {
                if (Main.projectile[i].active && Main.projectile[i].ModProjectile is RovaBeam && Main.projectile[i].owner == Projectile.owner)
                {
                    Main.projectile[i].Kill();
                }

                if (Main.projectile[i].active
                    && Main.projectile[i].ModProjectile is RovaRing
                    && Main.projectile[i].owner == Projectile.owner
                    && (int)Main.projectile[i].ai[0] == Projectile.whoAmI)
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
                    && (int)Main.projectile[i].ai[0] == Projectile.whoAmI)
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
                Projectile.whoAmI);
        }

        public override void Kill(int timeLeft)
        {
            RovaLavaCoreVisual.Remove(Projectile.whoAmI);
            KillAllBeams();

            if (!DespawnEruptionSpawned && Main.netMode != NetmodeID.MultiplayerClient)
            {
                DespawnEruptionSpawned = true;
                int eruptionIndex = Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center.X,
                    Projectile.Center.Y,
                    0f,
                    0f,
                    ModContent.ProjectileType<Explosion2>(),
                    Math.Max(1, Projectile.damage),
                    Projectile.knockBack,
                    Projectile.owner,
                    -1f, // ai[0] = RovaCenter eruption lock sentinel
                    Projectile.whoAmI);

                if (eruptionIndex >= 0 && eruptionIndex < Main.maxProjectiles)
                {
                    Main.projectile[eruptionIndex].netUpdate = true;
                }
            }

            // Heated tiles fade naturally so other players' heat fields are not cleared.
        }
    }
}
