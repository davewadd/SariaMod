using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using SariaMod.Buffs;
using SariaMod.Dusts;
using SariaMod.Items.Amber;
using SariaMod.Items.Amethyst;
using SariaMod.Items.Bands;
using SariaMod.Items.Emerald;
using SariaMod.Items.Ruby;
using SariaMod.Items.Sapphire;
using SariaMod.Items.Topaz;
using SariaMod.Items.zPearls;
using SariaMod.Items.zTalking;
using SariaMod.Items.Strange;
using Terraria.Localization;
using System;
using Terraria.Map;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Events;
using Terraria.ID;
using Terraria.ModLoader;
using System.Linq;
using SariaMod.Diagnostics;
using SariaMod.Netcode.SariaSoundSync;
using ReLogic.Utilities;
namespace SariaMod.Items.Strange
{
    public partial class Saria
    {
        public override void AI()
        {
            {
                Player player = Main.player[Projectile.owner];
                Player player2 = Main.LocalPlayer;
                FairyPlayer modPlayer = player.Fairy();
                FairyProjectile modprojectile = Projectile.Fairy();

                Rectangle movehitbox = Projectile.Hitbox;
                int owner = player.whoAmI;
                ///recharge effect
                if (CantAttack && CantAttackTimer <= 0 && !IsTransforming)
                {
                    Vector2 dustPosition = (Projectile.spriteDirection == 1) ? Projectile.Right : Projectile.Center;
                    for (int i = 0; i < 50; i++)
                    {
                        Vector2 dustspeed5 = Main.rand.NextVector2CircularEdge(1f, 1f) * -5;
                        Dust d = Dust.NewDustPerfect(dustPosition, ModContent.DustType<AbsorbPsychic>(), dustspeed5, Scale: 1.5f);
                        d.noGravity = true;
                    }
                    SoundEngine.PlaySound(SoundID.MaxMana, Projectile.Center);
                    CantAttack = false;
                }
                ///
                //////////////Transformation Timer
                ///
                if (TransformTimer > 0)
                {
                    TransformTimer--;

                    // All clients play the loop — each manages their own SlotId instance
                    if (Main.netMode != NetmodeID.Server)
                    {
                        bool isFirstTick = TransformTimer == TransformDuration - 1;
                        bool isLoopTick  = !isFirstTick && (TransformTimer % 181 == 0);
                        if (isFirstTick)
                        {
                            SoundEngine.PlaySound(SoundID.Item4, Projectile.Center);
                            if (SoundEngine.TryGetActiveSound(_transformLoopSlot, out ActiveSound old))
                                old.Stop();
                            _transformLoopSlot = SoundEngine.PlaySound(
                                new SoundStyle("SariaMod/Sounds/TransformLoop") { MaxInstances = 1, Volume = 0.7f },
                                Projectile.Center);
                            _transformLoopAge = 0;
                        }
                        else if (isLoopTick)
                        {
                            if (SoundEngine.TryGetActiveSound(_transformLoopSlot, out ActiveSound prev))
                                prev.Stop();
                            _transformLoopSlot = SoundEngine.PlaySound(
                                new SoundStyle("SariaMod/Sounds/TransformLoop") { MaxInstances = 1, Volume = 0.7f },
                                Projectile.Center);
                        }

                        // Track Saria's position every tick so the sound follows her
                        if (SoundEngine.TryGetActiveSound(_transformLoopSlot, out ActiveSound active))
                        {
                            active.Position = Projectile.Center;
                            // Ramp from 0.5 → 1.0 over TransformDuration; stays at 1.0 after that
                            if (_transformLoopAge >= 0)
                            {
                                active.Volume = 0.5f + 0.5f * Math.Clamp(_transformLoopAge / (float)TransformDuration, 0f, 1f);
                                _transformLoopAge++;
                            }
                        }
                    }

                    if (TransformTimer == 0 && PendingTransform >= 0)
                    {
                        Transform = PendingTransform;
                        PendingTransform = -1;
                        BiomeTime = 100;
                        Projectile.netUpdate = true;

                        // Stop the loop and play the completion sting
                        if (Main.netMode != NetmodeID.Server)
                        {
                            if (SoundEngine.TryGetActiveSound(_transformLoopSlot, out ActiveSound done))
                                done.Stop();
                            SoundEngine.PlaySound(SoundID.Item4, Projectile.Center);
                            _transformLoopAge = -1;
                        }
                    }
                }
                ///
                Projectile.SariaBaseDamage();
                Projectile.SariaBiomeEffectivness((int)BiomeTime, (int)Transform);

                // Extinguished visuals/audio — runs on all clients since Extinguished buff is synced.
                // Mood is also set here so all clients see the sad face reaction.
                if (Transform == 2 && player.HasBuff(ModContent.BuffType<Buffs.Extinguished>()) && Main.netMode != NetmodeID.Server)
                {
                    Projectile.SneezeDust(ModContent.DustType<Dusts.SmokeDust3>(), 20, 6, -10, 3, -12);
                    if (SoundTimer2 <= 0)
                    {
                        SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/mist"), Projectile.Center);
                        SoundTimer2 += 200;
                    }
                }
                Projectile.SariaBubbleFaceSpawner((bool)Sleep, (int)CanMove, (bool)Cursed, (int)Mood);
                Projectile.damage /= 2;
                Projectile.knockBack = 10;
                bool newXpTimer = player.HasBuff(ModContent.BuffType<XPBuff>());
                if (XpTimer != newXpTimer)
                {
                    XpTimer = newXpTimer;
                    Projectile.netUpdate = true;
                }
                ///Channeling
                bool NotActive = Eating <= 0 && !IsPlayerAsleep && !Sleep;
                bool HoldingHealBall = player.HeldItem.type == ModContent.ItemType<HealBall>();
                bool HoldingHealBallInInventory = player.HasItem(ModContent.ItemType<HealBall>());
                bool CanChanneltoBeginWith = (ChannelTime > 20 && Eating <= 0 && !IsPlayerAsleep && !Sleep && !SariaTalking); /// if you only want her to attack after certain frames after charging edit this to match what frames you want to look for
                bool playerischanneling = (player.channel == true && HoldingHealBall && ChangeForm <= 0 && !SariaTalking && Main.myPlayer == Projectile.owner && !Main.mouseRight && !player.noItems);
                bool notActive = Eating <= 0 && !IsPlayerAsleep && !Sleep && !SariaTalking;
                bool holdingHealBall = player.HeldItem.type == ModContent.ItemType<HealBall>();
                bool canChanneltoBeginWith = (ChannelTime > ShortChannelThreshold && notActive);
                bool playerIsChanneling = (player.channel && holdingHealBall && ChangeForm <= 0 && !SariaTalking && Main.myPlayer == Projectile.owner && !Main.mouseRight && !player.noItems);
                // 1. Handle Channeling and Time Progression
                if (playerIsChanneling)
                {
                    UpdateChannelTime(player, modPlayer);
                }
                // 2. Spawn the Transform UI
                if (playerIsChanneling && player.ownedProjectileCounts[ModContent.ProjectileType<Transform>()] <= 0f && canChanneltoBeginWith)
                {
                    SpawnTransformUI(player);
                }
                // 3. Handle Channel Release and Actions
                if (player.ownedProjectileCounts[ModContent.ProjectileType<Transform>()] > 0f && !player.channel)
                {
                    HandleChannelRelease(player, NotActive);
                }

                // --- CUTSCENE TRIGGERS ---
                if (Main.myPlayer == Projectile.owner)
                {
                    var tracker = player.GetModPlayer<SariaInteractionTrackerPlayer>();

                    // Hallow Cutscene Trigger
                    if (player.ZoneHallow)
                    {
                        // Add pending cutscene: ID="HallowIntro", Target="cutscene_hallow_intro", Button="Talk", Duration=5min, Condition="InHallow"
                        tracker.AddPendingCutscene("HallowIntro", "cutscene_hallow_intro", "Talk", 5.0, "InHallow");
                    }

                    // Zora Form (Transform 1) Trigger
                    // Trigger when entering Transform 1
                    if (Transform == 1)
                    {
                        // Add pending cutscene: ID="ZoraIntro", Target="cutscene_zora_intro", Button="Talk", Duration=5min, Condition="NotForm_1"
                        // Note: Condition is NotForm_1, so it won't be available while in Transform 1.
                        tracker.AddPendingCutscene("ZoraIntro", "cutscene_zora_intro", "Talk", 5.0, "NotForm_1");
                    }

                    // Example of Dependent Cutscene (Commented out)
                    // This would only trigger if "ZoraIntro" has been completed.
                    /*
                    if (Transform == 2)
                    {
                         tracker.AddPendingCutscene("FireIntro", "cutscene_fire_intro", "Talk", 5.0, "Completed_ZoraIntro");
                    }
                    */

                    }

                // Owner-side scans — run once per second (every 60 ticks)
                if (Main.myPlayer == Projectile.owner && Main.GameUpdateCount % 60 == 0)
                {
                    UpdateSariaZones();
                    InteractionManager.UpdateProximityChecks(Projectile);
                }

                /// ChangeForm stuff
                ///
                if (ChangeForm >= 1 && !SariaTalking && player.ownedProjectileCounts[ModContent.ProjectileType<FormChangeOverlay>()] <= 0f)
                {
                    if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<FormChangeOverlay>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, Projectile.whoAmI);
                }
                if (player.HeldItem.type != ModContent.ItemType<HealBall>() && CantAttackTimer < 100)
                {
                    CantAttackTimer = 100;
                }
                if (ChannelTime > 20 && NotActive && player.channel == true && HoldingHealBall && CantAttackTimer <= 0 && !player.HasBuff(ModContent.BuffType<HealpulseBuff>()) && !Main.mouseRight && !IsTransforming)
                {
                    ChannelState++;
                }
                {
                    if (ChannelState > 20 && NotActive)
                    {
                        IsCharging = 1;
                    }
                    else
                    {
                        IsCharging = 0;
                    }
                }
                if (CantAttackTimer > 0)
                {
                    CantAttackTimer--;
                }
                if (BiomeTime > 0)
                {
                    BiomeTime--;
                }
                // --- Period timer + natural SicknessBar decay ---
                // Every 360 ticks: base -4; ×3 in bad biome; ×2 if Drained; +10 if Soothing (additive); +15 if Overcharged (additive).
                if (Main.myPlayer == Projectile.owner)
                {
                    bool isBadBiome   = player.HasBuff(ModContent.BuffType<StatLower>());
                    bool hasSoothing  = player.HasBuff(ModContent.BuffType<Soothing>());
                    bool hasOvercharged = player.HasBuff(ModContent.BuffType<Overcharged>());
                    bool hasDrained   = player.HasBuff(ModContent.BuffType<Drained>());

                    // ---- STEP 1: multiplicative modifiers (apply to loss only, never to gains) ----
                    int baseLoss = isBadBiome ? 12 : 4;
                    if (hasDrained) baseLoss *= 2;
                    // Add future *N loss debuffs here: baseLoss *= N;

                    // ---- STEP 2: start from the raw loss ----
                    int change = -baseLoss;

                    // ---- STEP 3: additive bonuses (always last — never doubled by step 1) ----
                    if (hasSoothing)    change += 10; // e.g. -12 + 10 = -2; -4 + 10 = +6
                    if (hasOvercharged) change += 15;
                    // Add future +N recovery bonuses here: change += N;

                    SicknessDecayChange = change;

                    _sicknessDecayTimer++;
                    if (_sicknessDecayTimer >= 360)
                    {
                        _sicknessDecayTimer = 0;
                        SicknessBar = Math.Clamp(SicknessBar + SicknessDecayChange, 0, SicknessBarMax);
                        Projectile.netUpdate = true;
                    }

                    // Period timer: pure countdown only. All mood triggers are in SariaBubbleFaceSpawner.
                    if (_periodTimer > 0)
                    {
                        _periodTimer--;
                        if (_periodTimer == 0)
                            Projectile.netUpdate = true;
                    }
                    if (_statRaiseSoundCooldown > 0) _statRaiseSoundCooldown--;
                    if (_statLowerSoundCooldown > 0) _statLowerSoundCooldown--;
                }
                // --- End Period timer ---
                if (FlashCooldownTimer > 0)
                {
                    FlashCooldownTimer--;
                    if (FlashCooldownTimer == 0)
                        Projectile.netUpdate = true;
                }
                if (_moodOverrideTimer > 0)
                {
                    _moodOverrideTimer--;
                    Mood = _moodOverrideTarget;
                    if (_moodOverrideTimer == 0)
                    {
                        Mood = 0;
                        _moodPriority = 0;
                        Projectile.netUpdate = true;
                    }
                }
                if (SicknessBar <= SicknessBarMax / 5)
                {
                    if (!player.HasBuff(ModContent.BuffType<Soothing>()) && !player.HasBuff(ModContent.BuffType<Sickness>()))
                    {
                        if (Main.myPlayer == Projectile.owner) player.AddBuff(ModContent.BuffType<Sickness>(), 30000);
                    }
                }
                if ((!(HoldingHealBall) && SariaTalking) || (!SariaUISystem.IsDialogueActive))
                {
                    SariaTalking = false;
                }
                if (SoundTimer2 > 0)
                {
                    SoundTimer2--;
                }
                if (ChangeForm <= 0)
                {
                    SelectSound = false;
                }
                bool newCursed = player.HasBuff(ModContent.BuffType<BloodmoonBuff>()) || player.HasBuff(ModContent.BuffType<EclipseBuff>()) || Mood == (int)MoodState.Cursed;
                if (Cursed != newCursed)
                {
                    Cursed = newCursed;
                    Projectile.netUpdate = true;
                }
                Follow = Cursed;

                // LinkCable follow: active when cable is on, Saria is not cursed, and a marker is placed.
                // Owner-only computation; synced to all clients via SendExtraAI so they enter the correct movement branch.
                if (Main.myPlayer == Projectile.owner)
                {
                    bool prevLinkCableFollow = _linkCableFollow;
                    _linkCableFollow = modPlayer.LinkCable && !Cursed && modPlayer.LinkCableTarget != Vector2.Zero;
                    if (_linkCableFollow != prevLinkCableFollow)
                        Projectile.netUpdate = true;
                    if (_linkCableFollow)
                    {
                        // Inject the cable marker directly as the marked position.
                        Vector2 prevMarked = _followMarkedPosition;
                        _followMarkedPosition = modPlayer.LinkCableTarget;
                        _followMarkedNumber   = int.MaxValue - 1; // distinct sentinel; not a trail dot
                        if (Vector2.DistanceSquared(prevMarked, _followMarkedPosition) > (8f * 8f))
                            Projectile.netUpdate = true;
                    }
                    else if (!Follow && _followMarkedPosition != Vector2.Zero && _followMarkedNumber == int.MaxValue - 1)
                    {
                        // Cable was just turned off — clear the injected marker.
                        _followMarkedPosition = Vector2.Zero;
                        _followMarkedNumber   = -1;
                        Projectile.netUpdate  = true;
                    }
                }

                // Follow trail: track the player's path when they are out of Saria's sight.
                // Owner-only; dots are purely local draw-side data.
                if (Main.myPlayer == Projectile.owner)
                {
                    if (!Follow && !_linkCableFollow)
                    {
                        // Clear trail when Follow is inactive; reset everything for next session.
                        if (_followTrailDots.Count > 0)
                        {
                            _followTrailDots.Clear();
                            _followTrailDistAccum = 0f;
                        }
                        _followTrailNextNumber = 1;
                        _playerVisibleToSaria  = false;
                        _followTrailLastPos    = player.Center;
                        if (_followMarkedPosition != Vector2.Zero)
                        {
                            _followMarkedPosition = Vector2.Zero;
                            _followMarkedNumber   = -1;
                            Projectile.netUpdate  = true;
                        }
                        if (_followPath.Count > 0)
                        {
                            _followPath.Clear();
                            _followPathLastGoal = Vector2.Zero;
                            Projectile.netUpdate = true;
                        }
                    }
                    else if (Follow)
                    {
                        // Sight check: player within 500 world units AND clear line to Saria.
                        bool currentlyVisible =
                            Vector2.Distance(player.Center, Projectile.Center) <= 500f &&
                            Collision.CanHitLine(player.Center, 1, 1, Projectile.Center, 1, 1);

                        if (currentlyVisible)
                        {
                            // Player is visible — no trail needed; wipe any existing dots.
                            if (_followTrailDots.Count > 0)
                            {
                                _followTrailDots.Clear();
                                _followTrailDistAccum = 0f;
                                _followTrailNextNumber = 1;
                            }
                            _followTrailLastPos = player.Center;
                        }
                        else
                        {
                            // Falling edge (visible → not visible): reset and immediately drop dot #1.
                            if (_playerVisibleToSaria)
                            {
                                _followTrailDots.Clear();
                                _followTrailDistAccum  = 0f;
                                _followTrailNextNumber = 1;
                                _followTrailDots.Add((player.Center, _followTrailNextNumber++));
                                _followTrailLastPos = player.Center;
                            }

                            // Regular distance accumulation and dot placement while out of sight.
                            if (_followTrailLastPos == Vector2.Zero)
                                _followTrailLastPos = player.Center;

                            float moved = Vector2.Distance(player.Center, _followTrailLastPos);
                            _followTrailLastPos    = player.Center;
                            _followTrailDistAccum += moved;

                            if (_followTrailDistAccum >= FollowTrailInterval)
                            {
                                // Use % to drain the full backlog in one step — prevents a fast
                                // jump from building up a remainder that fires a cluster of
                                // delayed dots over the following ticks.
                                _followTrailDistAccum %= FollowTrailInterval;
                                _followTrailDots.Add((player.Center, _followTrailNextNumber++));
                                if (_followTrailDots.Count > FollowTrailMaxDots)
                                    _followTrailDots.RemoveAt(0);
                            }
                        }

                        _playerVisibleToSaria = currentlyVisible;

                        // Marked location selection.
                        // When player is within FollowMarkerRange with clear LoS, path directly to them.
                        // Otherwise target the highest-numbered visible trail dot.
                        bool playerNearAndVisible =
                            Vector2.Distance(player.Center, Projectile.Center) <= FollowMarkerRange &&
                            Collision.CanHitLine(player.Center, 1, 1, Projectile.Center, 1, 1);

                        Vector2 newMarked       = Vector2.Zero;
                        int     newMarkedNumber = -1;
                        if (playerNearAndVisible)
                        {
                            // Direct sight to player — path straight to them for the final approach.
                            newMarked       = player.Center;
                            newMarkedNumber = int.MaxValue;
                        }
                        else if (_followTrailDots.Count == 0)
                        {
                            // No dots and player not visible — place one at player's position immediately.
                            int newNum = _followTrailNextNumber++;
                            _followTrailDots.Add((player.Center, newNum));
                            newMarked       = player.Center;
                            newMarkedNumber = newNum;
                            Projectile.netUpdate = true;
                        }
                        else if (_followTrailDots.Count > 0)
                        {
                            // Prefer highest-numbered dot within FollowMarkerRange with clear LoS from Saria.
                            int     bestNumber = -1;
                            Vector2 bestPos    = Vector2.Zero;
                            for (int di = 0; di < _followTrailDots.Count; di++)
                            {
                                var dot = _followTrailDots[di];
                                if (Vector2.Distance(Projectile.Center, dot.Position) <= FollowMarkerRange &&
                                    Collision.CanHitLine(Projectile.Center, 1, 1, dot.Position, 1, 1) &&
                                    dot.Number > bestNumber)
                                {
                                    bestNumber = dot.Number;
                                    bestPos    = dot.Position;
                                }
                            }
                            // Fall back to oldest dot if none are visible within range.
                            if (bestPos != Vector2.Zero)
                            {
                                newMarked       = bestPos;
                                newMarkedNumber = bestNumber;
                            }
                            else
                            {
                                newMarked       = _followTrailDots[0].Position;
                                newMarkedNumber = _followTrailDots[0].Number;
                            }

                            // Shortcut: scan within FollowShortcutRange for a higher-numbered dot
                            // with clear LoS. If found, skip ahead to it and prune all dots below it.
                            int     shortcutBestNumber = newMarkedNumber;
                            Vector2 shortcutBestPos    = Vector2.Zero;
                            for (int di = 0; di < _followTrailDots.Count; di++)
                            {
                                var dot = _followTrailDots[di];
                                if (dot.Number > shortcutBestNumber &&
                                    Vector2.Distance(Projectile.Center, dot.Position) <= FollowShortcutRange &&
                                    Collision.CanHitLine(Projectile.Center, 1, 1, dot.Position, 1, 1))
                                {
                                    shortcutBestNumber = dot.Number;
                                    shortcutBestPos    = dot.Position;
                                }
                            }
                            if (shortcutBestPos != Vector2.Zero)
                            {
                                // Prune all dots with a number lower than the shortcut target.
                                for (int di = _followTrailDots.Count - 1; di >= 0; di--)
                                {
                                    if (_followTrailDots[di].Number < shortcutBestNumber)
                                        _followTrailDots.RemoveAt(di);
                                }
                                newMarked       = shortcutBestPos;
                                newMarkedNumber = shortcutBestNumber;
                            }
                        }

                        bool markMoved   = Vector2.DistanceSquared(_followMarkedPosition, newMarked) > (8f * 8f);
                        bool numberShift = _followMarkedNumber != newMarkedNumber;
                        _followMarkedPosition = newMarked;
                        _followMarkedNumber   = newMarkedNumber;
                        if (markMoved || numberShift)
                            Projectile.netUpdate = true;

                        // A* path planning to the marked location (owner-only).
                        // Runs only when a marked dot exists; recomputes on goal change
                        // or on the FollowPathRecalcTicks throttle. The resulting tile
                        // trail is synced to all clients (path change => netUpdate).
                        if (_followMarkedPosition != Vector2.Zero)
                        {
                            if (_followPathTimer > 0)
                                _followPathTimer--;

                            bool goalChanged = Vector2.DistanceSquared(_followPathLastGoal, _followMarkedPosition) > (8f * 8f);
                            // Only run the periodic timer replan when she has no active path
                            // (empty or already at the last node) — avoids resetting mid-traversal.
                            bool pathActive = _followPath.Count > 0 &&
                                              _followPathIndex < _followPath.Count - 1;
                            // Wall-block exception: if a wall is blocking her direction toward the
                            // current waypoint while mid-path, allow an immediate replan so she can
                            // route around newly placed tiles.
                            bool wallBlockingPath = false;
                            if (pathActive && _followPathIndex < _followPath.Count)
                            {
                                Vector2 toNext = _followPath[_followPathIndex] - SariaNavRef;
                                if ((_wallPausedLeft  && toNext.X < 0f) ||
                                    (_wallPausedRight && toNext.X > 0f))
                                    wallBlockingPath = true;
                            }
                            if (goalChanged || wallBlockingPath || (!pathActive && _followPathTimer <= 0))
                            {
                                _followPathTimer    = FollowPathRecalcTicks;
                                _followPathLastGoal = _followMarkedPosition;

                                float pathAllowance = _followMarkedNumber == int.MaxValue
                                    ? FollowPathPlayerAllowance
                                    : FollowPathAllowance;
                                var newPath = SariaPathfinder.FindPath(
                                    SariaNavRef, _followMarkedPosition,
                                    FollowPathFootprintWidth, FollowPathFootprintHeight,
                                    pathAllowance, Transform == 1);

                                if (!FollowPathsEqual(_followPath, newPath))
                                {
                                    _followPath.Clear();
                                    if (newPath != null)
                                        _followPath.AddRange(newPath);
                                    _followPathIndex = 0;
                                    Projectile.netUpdate = true;
                                }

                                // A* failed — path is null or empty and we have no existing path.
                                // Trigger the path-blocked teleport if not already running.
                                bool pathFailed = (_followPath.Count == 0) && _pathTeleportTimer <= 0
                                                  && _inWallTeleportTimer <= 0;
                                if (pathFailed && _followMarkedPosition != Vector2.Zero)
                                {
                                    float distToGoal = Vector2.Distance(SariaNavRef, _followMarkedPosition);
                                    const float directThresholdPx = PathTeleportDirectTiles * 16f;

                                    if (distToGoal > directThresholdPx)
                                    {
                                        // Too far — teleport directly on top of the dot.
                                        _pathTeleportTarget = _followMarkedPosition;
                                    }
                                    else
                                    {
                                        // Nearby — run reverse A* from dot toward Saria for
                                        // the closest reachable landing spot.
                                        var reversePath = SariaPathfinder.FindPath(
                                            _followMarkedPosition, SariaNavRef,
                                            FollowPathFootprintWidth, FollowPathFootprintHeight,
                                            FollowPathAllowance, Transform == 1);
                                        _pathTeleportTarget = (reversePath != null && reversePath.Count > 0)
                                            ? reversePath[0]
                                            : _followMarkedPosition;
                                    }

                                    // Lock destination and start 5-second wind-up.
                                    _pathTeleportTimer = PathTeleportDuration;
                                    StartTeleportWindUp(_pathTeleportTarget, PathTeleportDuration);
                                }
                            }
                        }
                        else if (_followPath.Count > 0)
                        {
                            // Marked location cleared while following — drop the path.
                            _followPath.Clear();
                            _followPathLastGoal = Vector2.Zero;
                            Projectile.netUpdate = true;
                        }
                    }
                    else if (_linkCableFollow)
                    {
                        // LinkCable A* planning: same path machinery as Follow but no trail dots.
                        // _followMarkedPosition is already set to LinkCableTarget above.
                        if (_followMarkedPosition != Vector2.Zero)
                        {
                            if (_followPathTimer > 0)
                                _followPathTimer--;

                            bool goalChanged = Vector2.DistanceSquared(_followPathLastGoal, _followMarkedPosition) > (8f * 8f);
                            bool pathActive  = _followPath.Count > 0 && _followPathIndex < _followPath.Count - 1;
                            // Arrival deadzone: if she's resting near the marker with no active
                            // path, don't re-path. This lets the ground probes nudge her for
                            // clearance without A* yanking her back to the exact tile every tick.
                            bool restingAtMarker = !pathActive
                                && Vector2.Distance(SariaNavRef, _followMarkedPosition) <= LinkCableArrivalDeadzone;
                            bool wallBlockingPath = false;
                            if (pathActive && _followPathIndex < _followPath.Count)
                            {
                                Vector2 toNext = _followPath[_followPathIndex] - SariaNavRef;
                                if ((_wallPausedLeft  && toNext.X < 0f) ||
                                    (_wallPausedRight && toNext.X > 0f))
                                    wallBlockingPath = true;
                            }
                            if (!restingAtMarker && (goalChanged || wallBlockingPath || (!pathActive && _followPathTimer <= 0)))
                            {
                                _followPathTimer    = FollowPathRecalcTicks;
                                _followPathLastGoal = _followMarkedPosition;

                                var newPath = SariaPathfinder.FindPath(
                                    SariaNavRef, _followMarkedPosition,
                                    FollowPathFootprintWidth, FollowPathFootprintHeight,
                                    FollowPathPlayerAllowance, Transform == 1);

                                if (!FollowPathsEqual(_followPath, newPath))
                                {
                                    _followPath.Clear();
                                    if (newPath != null)
                                        _followPath.AddRange(newPath);
                                    _followPathIndex = 0;
                                    Projectile.netUpdate = true;
                                }

                                // Teleport fallback if A* fails.
                                bool pathFailed = (_followPath.Count == 0) && _pathTeleportTimer <= 0
                                                  && _inWallTeleportTimer <= 0;
                                if (pathFailed)
                                {
                                    float distToGoal = Vector2.Distance(SariaNavRef, _followMarkedPosition);
                                    const float directThresholdPx = PathTeleportDirectTiles * 16f;
                                    if (distToGoal > directThresholdPx)
                                    {
                                        _pathTeleportTarget = _followMarkedPosition;
                                    }
                                    else
                                    {
                                        var reversePath = SariaPathfinder.FindPath(
                                            _followMarkedPosition, SariaNavRef,
                                            FollowPathFootprintWidth, FollowPathFootprintHeight,
                                            FollowPathPlayerAllowance, Transform == 1);
                                        _pathTeleportTarget = (reversePath != null && reversePath.Count > 0)
                                            ? reversePath[0]
                                            : _followMarkedPosition;
                                    }
                                    _pathTeleportTimer = PathTeleportDuration;
                                    StartTeleportWindUp(_pathTeleportTarget, PathTeleportDuration);
                                }
                            }
                        }
                        else if (_followPath.Count > 0)
                        {
                            _followPath.Clear();
                            _followPathLastGoal = Vector2.Zero;
                            Projectile.netUpdate = true;
                        }
                    }
                }
                if (player.HasBuff(ModContent.BuffType<Soothing>()) && player.HasBuff(ModContent.BuffType<Sickness>()))
                {
                    player.ClearBuff(ModContent.BuffType<Sickness>());
                }
                /////////////// End of Transformation Timer
                ///
                int dustspeed = 40;
                if ((Projectile.frame >= 36 && Projectile.frame <= 42))
                {
                    dustspeed = 5;
                }
                if (Transform == 2)
                {
                    Projectile.SneezeDust(ModContent.DustType<FlameDustSaria>(), 30, 100, -10, 3, -12);
                }
                // Emit MediumXpPearl-like light when Transform == 1 and in water (non-lava)
                if (Transform == 1)
                {
                    if (Projectile.IsMostlyInNonLavaLiquid())
                    {
                        Lighting.AddLight(Projectile.Center, Color.Blue.ToVector3() * 1.2f);
                        if (Main.myPlayer == Projectile.owner)
                        {
                            Main.player[Projectile.owner].AddBuff(BuffID.Gills, 2);
                        }
                    }
                }

                // Emit air bubbles when underwater using custom BubbleDust
                // Bubbles float up slowly, emit light, and despawn when they exit water
                if (Projectile.IsTopHalfMostlyInNonLavaLiquid() && Main.netMode != NetmodeID.Server)
                {
                    // Slower spawn rate: ~1 out of 25 ticks
                    if (Main.rand.NextBool(25))
                    {
                        // Calculate spawn position near Saria's mouth/face area
                        float sneezespot = (Projectile.spriteDirection > 0) ? 3f : -12f;
                        Vector2 spawnPos = new Vector2(
                            Projectile.Center.X + sneezespot + Main.rand.NextFloat(-2f, 2f),
                            Projectile.Center.Y - 10f + Main.rand.NextFloat(-3f, 3f)
                        );

                        // Create bubble with slow upward velocity
                        Vector2 vel = new Vector2(Main.rand.NextFloat(-0.1f, 0.1f), Main.rand.NextFloat(-0.4f, -0.15f));
                        Dust bubble = Dust.NewDustPerfect(spawnPos, ModContent.DustType<BubbleDust>(), vel);
                        bubble.noGravity = true;
                    }
                }
                if (Transform == 6)
                {
                    Projectile.SneezeDust(ModContent.DustType<ShadowFlameDust>(), 30, 100, -10, 3, -12);
                }
                // Dark shadow aura when mood is Cursed
                if (Mood == (int)MoodState.Cursed && Main.netMode != NetmodeID.Server)
                {
                    Projectile.SneezeDust(ModContent.DustType<ShadowFlameDust>(), 60, 2, -10, 3, -12);
                    Lighting.AddLight(Projectile.Center, new Vector3(0.25f, 0f, 0.3f));
                }
                if (Projectile.frame == 22 && (Eating % 5 == 0) && (!Sleep) && !BloodSneeze && (!player.HasBuff(ModContent.BuffType<StatLower>()) && !player.HasBuff(ModContent.BuffType<Sickness>()) && !player.HasBuff(ModContent.BuffType<BloodmoonBuff>()) && !player.HasBuff(ModContent.BuffType<EclipseBuff>())))
                {
                    Projectile.SneezeDust(ModContent.DustType<Sneeze>(), 1, 1, -10, 3, -12);
                }
                if (Projectile.frame == 22 && (Eating % 5 == 0) && (!Sleep) && (BloodSneeze || player.HasBuff(ModContent.BuffType<StatLower>()) || player.HasBuff(ModContent.BuffType<Sickness>()) || player.HasBuff(ModContent.BuffType<BloodmoonBuff>()) || player.HasBuff(ModContent.BuffType<EclipseBuff>())))
                {
                    Projectile.SneezeDust(ModContent.DustType<Blood>(), 1, 1, -10, 3, -12);
                    Projectile.SneezeDust(ModContent.DustType<Blood>(), 30, 1, -10, 3, -12);
                }
                if ((player.active && Main.bloodMoon) && ((!player.HasBuff(ModContent.BuffType<Soothing>()))))
                {
                    player.AddBuff(ModContent.BuffType<BloodmoonBuff>(), 20);
                    Projectile.SneezeDust(ModContent.DustType<Blood>(), 30, 1, -10, 3, -12);
                    Projectile.SneezeDust(ModContent.DustType<BlackSmoke>(), 20, 6, -10, 3, -12);
                }
                Projectile.SneezeDust(ModContent.DustType<Psychic2>(), (int)dustspeed, 6, 34, 3, -12);

                // Fog breath - only when NOT underwater, using Saria's own synced zone flags
                bool isUnderwater = Projectile.IsTopHalfMostlyInNonLavaLiquid();
                if (!isUnderwater && (((SariaZoneSnow) && !(SariaExtensions1.IsLineSegmentPartiallyWalled(new Vector2(Projectile.Center.X, Projectile.position.Y), Projectile.Center, 0.75f) && SariaHasCampfire)) || (SariaZoneSpace) && !(SariaExtensions1.IsLineSegmentPartiallyWalled(new Vector2(Projectile.Center.X, Projectile.position.Y), Projectile.Center, 0.75f) && SariaHasCampfire) || (SariaZoneDesert && !Main.dayTime) && !(SariaExtensions1.IsLineSegmentPartiallyWalled(new Vector2(Projectile.Center.X, Projectile.position.Y), Projectile.Center, 0.75f) && SariaHasCampfire) || (SariaZoneRain && !SariaZoneJungle && !(SariaZoneDesert && Main.dayTime)) && !(SariaExtensions1.IsLineSegmentPartiallyWalled(new Vector2(Projectile.Center.X, Projectile.position.Y), Projectile.Center, 0.75f) && SariaHasCampfire)))
                {
                    if (Projectile.velocity.X <= 1)
                    {
                        Projectile.SneezeDust(ModContent.DustType<Fog>(), 50, 1, -10, 10, -17);
                    }
                    else if (Projectile.velocity.X > 1)
                    {
                        Projectile.SneezeDust(ModContent.DustType<Fog>(), 5, 1, -10, 10, -17);
                    }
                }//end of dust stuff
                if (Projectile.localAI[0] == 0f && Main.myPlayer == Projectile.owner)
                {
                    Projectile.Fairy().spawnedPlayerMinionProjectileDamageValue = Projectile.damage;
                    modPlayer.smallTalkingTime = Main.rand.Next(30 * 60, 40 * 60 + 1);
                    for (int j = 0; j < 1; j++) //set to 2
                    {
                        if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<PowerUp>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, Projectile.whoAmI);
                        if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<Ztarget>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, Projectile.whoAmI);
                        if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<HealCursor>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, Projectile.whoAmI);
                        CantAttackTimer = 120;
                    }
                    Projectile.localAI[0] = 1f;
                }
                ////Ztargets
                if (player.ownedProjectileCounts[ModContent.ProjectileType<HealCursor>()] <= 0f)
                {
                    if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<HealCursor>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, Projectile.whoAmI);
                }
                Projectile.Ztargets((int)ChannelState, (int)Transform);
                ///
                if (player.dead)
                {
                    modPlayer.SariaXp /= 2;
                }
                if (player.HasBuff(ModContent.BuffType<SariaBuff>()))
                {
                    Projectile.timeLeft = 10;
                }
                if (!player.HasBuff(ModContent.BuffType<SariaBuff>()) && Projectile.timeLeft == 1)
                {
                    if (Main.myPlayer == Projectile.owner)
                    {
                        Projectile.Kill();
                    }
                }
                if ((!HoldingHealBallInInventory && !HoldingHealBall) && Projectile.timeLeft == 1)
                {
                    if (Main.myPlayer == Projectile.owner)
                    {
                        Projectile.Kill();
                    }
                }
                /// AiStuff
                Vector2 targetCenter = Projectile.position;
                bool foundTarget = false;
                bool CanSee = false;

                // --- PRIORITY: ZtargetReal projectile (owned by same player) ---
                int ztargetRealType = ModContent.ProjectileType<ZtargetReal>();
                float bestZtargetDist = 2000f;

                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile p = Main.projectile[i];
                    if (!p.active)
                        continue;

                    if (p.type != ztargetRealType)
                        continue;

                    // only consider the one(s) owned by the same player as this Saria projectile
                    if (p.owner != Projectile.owner)
                        continue;

                    float between = Vector2.Distance(p.Center, Projectile.Center);
                    if (between >= bestZtargetDist)
                        continue;

                    bool canSeeIt = Collision.CanHitLine(Projectile.Center, 1, 1, p.position, p.width, p.height);

                    bestZtargetDist = between;
                    targetCenter = p.Center;
                    foundTarget = true;
                    CanSee = true;
                }

                // --- FALLBACK: your existing NPC targeting logic ---
                if (!foundTarget && player.HasMinionAttackTargetNPC && player.HeldItem.type == ModContent.ItemType<HealBall>())
                {
                    NPC npc = Main.npc[player.MinionAttackTargetNPC];
                    bool CanSeeit = Collision.CanHitLine(Projectile.Center, 1, 1, npc.position, npc.width, npc.height);
                    float between = Vector2.Distance(npc.Center, Projectile.Center);

                    if (between < 2000f)
                    {
                        targetCenter = npc.Center;
                        foundTarget = true;
                        if (CanSeeit)
                            CanSee = true;
                    }
                }

                if (!foundTarget && Main.myPlayer == Projectile.owner)
                {
                    Vector2 bestFrozenTarget = Vector2.Zero;
                    float bestFrozenDistance = -1f;
                    bool bestFrozenCanSee = false;

                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        NPC npc = Main.npc[i];
                        if (!npc.CanBeChasedBy())
                            continue;

                        float between = _linkCableFollow
                            ? Vector2.Distance(npc.Center, Projectile.Center)
                            : Vector2.Distance(npc.Center, player.Center);
                        bool closeThroughWall = between < 800f;
                        bool canSeeit = Collision.CanHitLine(Projectile.Center, 1, 1, npc.position, npc.width, npc.height);
                        if (!closeThroughWall)
                            continue;

                        if (Transform == 1)
                        {
                            int frozenBuffId = ModContent.BuffType<EnemyFrozen>();
                            bool isFrozen = npc.HasBuff(frozenBuffId);

                            if (!isFrozen)
                            {
                                if (!foundTarget || Vector2.Distance(player.Center, targetCenter) > between)
                                {
                                    targetCenter = npc.Center;
                                    foundTarget = true;
                                    CanSee = canSeeit;
                                }
                            }
                            else
                            {
                                if (bestFrozenDistance == -1f || bestFrozenDistance > between)
                                {
                                    bestFrozenTarget = npc.Center;
                                    bestFrozenDistance = between;
                                    bestFrozenCanSee = canSeeit;
                                }
                            }
                        }
                        else
                        {
                            bool closest = Vector2.Distance(player.Center, targetCenter) > between;
                            if (closest || !foundTarget)
                            {
                                targetCenter = npc.Center;
                                foundTarget = true;
                                CanSee = canSeeit;
                            }
                        }
                    }

                    if (Transform == 1 && !foundTarget && bestFrozenDistance != -1f)
                    {
                        targetCenter = bestFrozenTarget;
                        foundTarget = true;
                        CanSee = bestFrozenCanSee;
                    }
                }
                // Biome weakness forced sneeze — suppress targeting so she can't
                // start new attacks while the sneeze is pending or playing.
                // Current attacks finish naturally, but no new ones begin.
                if ((player.HasBuff(ModContent.BuffType<StatLower>()) || Cursed) && (IdleAnimator.IsSneezeQueued || Sneezing))
                {
                    foundTarget = false;
                }
                // Transformation in progress — suppress targeting so she can't enter
                // a new attack state while changing forms. Current attacks finish naturally.
                if (IsTransforming)
                {
                    foundTarget = false;
                }
                Projectile.SariaAI((int)Transform, (int)ChannelTime, (bool)NotActive, (bool)foundTarget, (bool)Sleep, (bool)HoldingHealBall, (int)CantAttackTimer, (int)ChannelState, (int)Eating, (bool)CanSee);

                if ((Main.rand.NextBool(550) || foundTarget) && SpecialAnimate <= 0)
                {
                    SpecialAnimate = 60;
                }
                if (SpecialAnimate > 0)
                {
                    SpecialAnimate--;
                }
                /////end
                //Flashupdate stuff
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    float between = Vector2.Distance(Main.projectile[i].Center, player.Center);
                    if (between <= 100)
                    {
                        if (Main.projectile[i].active && i != Projectile.whoAmI && ((!Main.projectile[i].friendly && Main.projectile[i].hostile) || (Main.projectile[i].trap)) && Main.myPlayer == Projectile.owner)
                        {
                            if ((!player.HasBuff(ModContent.BuffType<Sickness>()) && (!player.HasBuff(ModContent.BuffType<BloodmoonBuff>()) && (!player.HasBuff(ModContent.BuffType<EclipseBuff>()) && FlashCooldownTimer <= 0))) && Main.myPlayer == Projectile.owner)
                            {
                                Projectile.TriggerFlash();
                                Projectile.NewProjectile(Projectile.GetSource_FromThis(),
                                    player.Center.X, player.Center.Y, 0, 0,
                                    ModContent.ProjectileType<FlashBarrier>(),
                                    (int)Projectile.damage, 0f, Projectile.owner, player.whoAmI, Projectile.whoAmI);
                                FlashCooldownTimer = 1800;
                                SoundEngine.PlaySound(SoundID.Item76, Projectile.Center);
                                for (int o = 0; o < 50; o++)
                                {
                                    Vector2 speed2 = Main.rand.NextVector2CircularEdge(1.1f, 1.1f);
                                    Dust d = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<PsychicRingDust>(), speed2 * 15, Scale: 4f);
                                    d.noGravity = true;
                                }
                            }
                        }
                    }
                }
                if (FlashCooldownTimer > 0)
                {
                    Projectile.SneezeDust(ModContent.DustType<Psychic>(), 20, 6, -10, 3, -12);
                }
                if (CantAttackTimer > 0)
                {
                    CantAttack = true;
                }
                if (Projectile.frame >= 44 && Projectile.frame <= 55 && Transform == 1)
                {
                    Projectile.AttackDust(ModContent.DustType<BubbleDust>(), 8, 34);
                }
                if (Projectile.frame >= 44 && Projectile.frame <= 55 && Transform == 2)
                {
                    Projectile.AttackDust(ModContent.DustType<FlameDust>(), 8, 34);
                }
                if (Projectile.frame >= 44 && Projectile.frame <= 55 && Transform == 3)
                {
                    Projectile.AttackDust2();
                }
                if (Projectile.frame >= 44 && Projectile.frame <= 55 && Transform == 6)
                {
                    Projectile.AttackDust(ModContent.DustType<ShadowFlameDust>(), 1, 34);
                }
                Vector2 idlePosition = player.Center;
                float speed = 2;
                float Close = 60;
                if (Eating <= 0 && !Sleep)
                {
                    if (player.HeldItem.type == ModContent.ItemType<FrozenYogurt>() || player.HeldItem.type == ModContent.ItemType<SariasConfect>())
                    {
                        Close = 20;
                        if ((player.ownedProjectileCounts[ModContent.ProjectileType<Notice>()] <= 0f) && !Holding)
                        {
                            if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<Notice>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, Projectile.whoAmI);
                            Holding = true;
                            MoveTimer = 0;
                             }
                        }
                        if (HoldingHealBall)
                    {
                        Close = 60;
                        if (Holding)
                        {
                            Holding = false;
                        }
                    }
                    if (player.HeldItem.type != ModContent.ItemType<FrozenYogurt>() && player.HeldItem.type != ModContent.ItemType<SariasConfect>() && player.HeldItem.type != ModContent.ItemType<HealBall>())
                    {
                        if (player.statLife >= (player.statLifeMax2 - player.statLifeMax2 / 12))
                        {
                            Close = 30;
                        }
                        else
                        {
                            Close = 60;
                        }
                        if (Holding)
                        {
                            Holding = false;
                        }
                    }
                    // Emeraldspike force: push physical close to 0 while spikes are active.
                    bool spikeActive = player.ownedProjectileCounts[ModContent.ProjectileType<Emeraldspike>()] > 0f
                        || player.ownedProjectileCounts[ModContent.ProjectileType<Emeraldspike2>()] > 0f
                        || player.ownedProjectileCounts[ModContent.ProjectileType<Emeraldspike3>()] > 0f;

                    _closeTracker.Update(Projectile, player, Close, spikeActive);
                }
                float SariaOffsetX = _closeTracker.GetOffsetX(Projectile, player, Close);
                idlePosition.Y -= 15f;
                idlePosition.X += SariaOffsetX;
                _debugIdlePosition = idlePosition;
                Vector2 vectorToIdlePosition = idlePosition - Projectile.Center;
                float distanceToIdlePosition = vectorToIdlePosition.Length();
                bool _oneWallGreen = _detectorResults[2].Green ^ _detectorResults[3].Green;
                if (_oneWallGreen && Vector2.Distance(Projectile.Center, player.Center) <= 19f && player.velocity.Length() < 1f && !Sleep && Eating <= 0)
                {
                    if (!_followSight)
                        _lockedIdlePosition = idlePosition; // snapshot position on rising edge
                    _followSight = true;
                }
                else if (Vector2.Distance(Projectile.Center, player.Center) > 20 || Sleep || Eating > 0)
                {
                    _followSight = false;
                }
                // While FollowSight is active, freeze the idle position so Saria
                // stops drifting and the direction vector stays at zero.
                // Also mirror the player's facing direction continuously while latched.
                if (_followSight)
                {
                    Projectile.spriteDirection = player.direction;
                    idlePosition = _lockedIdlePosition;
                    vectorToIdlePosition = idlePosition - Projectile.Center;
                    distanceToIdlePosition = vectorToIdlePosition.Length();
                    _debugIdlePosition = idlePosition;
                }
                if (player.HasBuff(ModContent.BuffType<Veil>()) && Transform == 1)
                {
                    player.AddBuff(ModContent.BuffType<Veil>(), 8800);
                }

                Vector2 direction = idlePosition - Projectile.Center;

                if (Follow || _linkCableFollow)
                {
                    // LinkCable mode: redirect idle position to the placed marker so all
                    // subsequent checks (LoS, direction, distance) operate on the cable target.
                    if (_linkCableFollow && _followMarkedPosition != Vector2.Zero)
                    {
                        idlePosition = _followMarkedPosition;
                        vectorToIdlePosition = idlePosition - Projectile.Center;
                        distanceToIdlePosition = vectorToIdlePosition.Length();
                    }

                    bool canSeeIdle = Collision.CanHitLine(Projectile.Center, 1, 1, idlePosition, 1, 1);
                    // Cursed Follow: player must be within FollowMarkerRange AND visible to skip A*.
                    // LinkCable: always use A* — never take the direct-steer shortcut.
                    bool playerDirectVisible = !_linkCableFollow && canSeeIdle &&
                        distanceToIdlePosition <= FollowMarkerRange;

                    if (playerDirectVisible)
                    {
                        // Direct line-of-sight — go straight, no A* needed.
                        if (_followPath.Count > 0)
                        {
                            _followPath.Clear();
                            _followPathLastGoal = Vector2.Zero;
                        }
                        direction = idlePosition - Projectile.Center;
                        _cursedSpeedScale = Math.Min(_cursedSpeedScale + 0.04f, 1f);
                        _cursedSeparated  = false;
                    }
                    else if (_followPath.Count > 0)
                    {
                        // Path exists — steer toward the current waypoint, bypassing
                        // the separation halt so she keeps walking even when far away.
                        // Clamp index in case the path shrank since last tick.
                        if (_followPathIndex >= _followPath.Count)
                            _followPathIndex = _followPath.Count - 1;

                        bool atLastNode = _followPathIndex == _followPath.Count - 1;

                        // Advance intermediate waypoints when close enough.
                        // Owner-only: each client's SariaNavRef drifts slightly between syncs,
                        // so letting all clients advance independently causes them to target
                        // different waypoints and produce opposing velocity vectors → jitter.
                        // The owner advances and immediately syncs the new index via netUpdate
                        // so clients converge to the same waypoint within a frame or two.
                        if (Main.myPlayer == Projectile.owner)
                        {
                            bool didAdvance = false;
                            while (!atLastNode &&
                                   Vector2.Distance(SariaNavRef, _followPath[_followPathIndex]) <= 28f)
                            {
                                _followPathIndex++;
                                atLastNode = _followPathIndex == _followPath.Count - 1;
                                didAdvance = true;
                            }
                            if (didAdvance)
                                Projectile.netUpdate = true;
                        }

                        Vector2 toWaypoint = _followPath[_followPathIndex] - SariaNavRef;
                        float dist = toWaypoint.Length();

                        if (atLastNode)
                        {
                            // Final node: arrived.
                            if (dist <= 16f)
                            {
                                // State mutations are owner-only so non-owner clients never
                                // clear the path prematurely on their locally extrapolated position.
                                if (Main.myPlayer == Projectile.owner)
                                {
                                    if (Follow)
                                    {
                                        // Cursed mode: consume the trail dot and clear the mark so
                                        // the next evaluation can pick the next dot.
                                        for (int di = _followTrailDots.Count - 1; di >= 0; di--)
                                        {
                                            if (_followTrailDots[di].Position == _followMarkedPosition)
                                            {
                                                _followTrailDots.RemoveAt(di);
                                                break;
                                            }
                                        }
                                        _followPath.Clear();
                                        _followPathLastGoal   = Vector2.Zero;
                                        _followMarkedPosition = Vector2.Zero;
                                        _followMarkedNumber   = -1;
                                        Projectile.netUpdate  = true;
                                    }
                                    else
                                    {
                                        // LinkCable mode: arrived at marker — hold position, clear path
                                        // but keep _followMarkedPosition so she stays put.
                                        _followPath.Clear();
                                        _followPathLastGoal = Vector2.Zero;
                                        Projectile.netUpdate = true;
                                    }
                                }
                                direction = Vector2.Zero;
                            }
                            else
                            {
                                // Approaching final node: scale force by distance so she
                                // decelerates smoothly rather than oscillating.
                                float scale = Math.Min(dist, 100f);
                                direction = dist > 0.01f ? toWaypoint / dist * scale : Vector2.Zero;
                            }
                        }
                        else
                        {
                            // Intermediate node: full saturated force.
                            direction = dist > 0.01f ? toWaypoint / dist * 100f : Vector2.Zero;
                        }

                        // Keep speed-scale at 1 while following path (no bleed-off).
                        _cursedSpeedScale   = Math.Min(_cursedSpeedScale + 0.04f, 1f);
                        _cursedSeparated    = false;
                    }
                    else
                    {
                        if (Follow)
                        {
                            // No path — original separation logic unchanged.
                            // Enter separated when distance > CursedSeparationRadius.
                            // Exit only when distance <= CursedSeparationRadius AND clear LOS.
                            bool wasSeparated = _cursedSeparated;
                            if (!_cursedSeparated)
                            {
                                if (distanceToIdlePosition > CursedSeparationRadius)
                                    _cursedSeparated = true;
                            }
                            else
                            {
                                if (distanceToIdlePosition <= CursedSeparationRadius && canSeeIdle)
                                    _cursedSeparated = false;
                            }
                            // Transition: separated → reunited — reset MoveTimer so she can move immediately.
                            if (wasSeparated && !_cursedSeparated)
                            {
                                MoveTimer = 0;
                            }

                            if (_cursedSeparated)
                            {
                                // Too far away — let momentum bleed off naturally; scale decays to 0.
                                direction = Vector2.Zero;
                                _cursedSpeedScale = Math.Max(_cursedSpeedScale - 0.025f, 0f);
                            }
                            else
                            {
                                // Close enough — follow idle position; ramp speed back up.
                                direction = idlePosition - Projectile.Center;
                                _cursedSpeedScale = Math.Min(_cursedSpeedScale + 0.04f, 1f);
                            }
                        }
                        else
                        {
                            // LinkCable mode: no path yet — hold position.
                            direction = Vector2.Zero;
                            _cursedSpeedScale = Math.Min(_cursedSpeedScale + 0.04f, 1f);
                        }
                    }
                }
                else
                {
                    _cursedSeparated  = false;
                    _cursedSpeedScale = 1f;

                    // Far-teleport: when neither Follow nor Cursed is active and the idle
                    // position is beyond IdleTeleportThreshold, teleport instead of flying.
                    if (Main.myPlayer == Projectile.owner
                        && distanceToIdlePosition > IdleTeleportThreshold
                        && _idleTeleportTimer  <= 0
                        && _inWallTeleportTimer <= 0
                        && _pathTeleportTimer  <= 0
                        && CanMove > 0)
                    {
                        // Lock the target and start the 2-second wind-up.
                        _idleTeleportTarget = idlePosition;
                        _idleTeleportTimer  = IdleTeleportDuration;
                        StartTeleportWindUp(idlePosition, IdleTeleportDuration);
                    }

                    // Suppress normal flying movement while the idle teleport wind-up is active.
                    if (_idleTeleportTimer > 0)
                        direction = Vector2.Zero;
                }
                if (foundTarget)
                {
                    {
                        speed = 2;
                        // GreenPause: if the inner Green line of a wall detector is solid,
                        // suppress direction.X toward that wall so the movement formula
                        // cannot drift Saria into the tile. Priority is unaffected.
                        Vector2 gatedDirection = direction;
                        if (_wallPausedLeft  && gatedDirection.X < 0f) gatedDirection.X = 0f;
                        if (_wallPausedRight && gatedDirection.X > 0f) gatedDirection.X = 0f;
                        // Owner-authoritative while following: see note on the main integration below.
                        if (Main.myPlayer == Projectile.owner || !(Follow || _linkCableFollow))
                            Projectile.velocity = (((Projectile.velocity * (13 - speed) + gatedDirection) / 20) * CanMove);
                    }
                }
                int newCanMove;
                if (Sleep || Eating == 3 || Eating == 4 || Eating == 5 || Sneezing || (ChannelState > 0 && (IsCharging <= 0 || Projectile.frame <= 8)))// if you want Saria to not move when charging, copy---- || ChannelState > 0  ----- and put it behind Eating == 4
                {
                    newCanMove = 0;
                }
                else if ((MoveTimer >= 275 && ((Projectile.frame >= 0) && (Projectile.frame <= 36)) && distanceToIdlePosition <= 180 && (Math.Abs(Projectile.velocity.X) <= .5) && (player.statLife >= player.statLifeMax2)))
                {
                    newCanMove = 0;
                }
                else
                {
                    newCanMove = 1;
                }
                // Override: keep CanMove = 1 when a food signal is active, the player is
                // holding the food item, and Saria is in idle frames so she walks to the player.
                bool foodSignalActive = Eating <= 0 && !Sleep && !Sneezing && Projectile.frame <= SariaIdleAnimator.IdleFrameMax && (player.ownedProjectileCounts[ModContent.ProjectileType<FrozenYogurtSignal>()] > 0f || player.ownedProjectileCounts[ModContent.ProjectileType<Competitivetime>()] > 0f);
                if (foodSignalActive)
                {
                    newCanMove = 1;
                }
                // Override: keep CanMove = 1 while actively following the A* path so the
                // idle-stop timer does not freeze her mid-walk.
                // Covers both intermediate nodes and the final approach (> 16px away).
                // Eating > 0 takes priority: she must not move while consuming food.
                if ((Follow || _linkCableFollow) && !Sneezing && Eating <= 0 && _followPath.Count > 0 &&
                    Vector2.Distance(SariaNavRef, _followPath[_followPath.Count - 1]) > 16f)
                {
                    newCanMove = 1;
                }
                // Override: stop Saria in place while the player is holding the FeelingRod.
                if (player.HeldItem.type == ModContent.ItemType<FeelingRod>())
                {
                    newCanMove = 0;
                }
                if (CanMove != newCanMove)
                {
                    CanMove = newCanMove;
                }
                if (Sleep && (distanceToIdlePosition > 280))
                {
                    MoveTimer = 0;
                }
                if (ChannelState > 0)
                {
                    MoveTimer = 0;
                }
                {
                    Vector2 gatedDir = direction;
                    if (_followPath.Count == 0)
                    {
                        if (_wallPausedLeft  && gatedDir.X < 0f) gatedDir.X = 0f;
                        if (_wallPausedRight && gatedDir.X > 0f) gatedDir.X = 0f;
                    }
                    // Owner-authoritative movement: only the owner integrates velocity while in
                    // Follow/LinkCable mode. A non-owner sits ~100px behind the synced position,
                    // so re-running this formula makes it compute full speed toward the marker
                    // while the owner decelerates — then netUpdate snaps it back, causing jitter.
                    // Non-owners coast on the synced velocity and let position updates land.
                    if (Main.myPlayer == Projectile.owner || !(Follow || _linkCableFollow))
                        Projectile.velocity = ((Projectile.velocity * (13 - speed) + gatedDir) / 20) * CanMove;
                }
                // Follow / LinkCable mode: cap speed on both axes, scaled by _cursedSpeedScale.
                // Decays to 0 when separated (natural halt), ramps up when rejoining player.
                if (Follow || _linkCableFollow)
                {
                    float maxSpeedX = (2.25f + 0.30f * MathF.Sin(Main.GameUpdateCount * (MathF.PI * 2f / 300f))) * _cursedSpeedScale;
                    float maxSpeedY = 4.5f * _cursedSpeedScale;
                    Projectile.velocity.X = Math.Clamp(Projectile.velocity.X, -maxSpeedX, maxSpeedX);
                    Projectile.velocity.Y = Math.Clamp(Projectile.velocity.Y, -maxSpeedY, maxSpeedY);
                    // Gentle bob: sine wave on Y so she floats rather than walking rigidly.
                    // Suppressed when a solid tile is within 1 tile above her head so she
                    // doesn't clip into ceilings in doorways or 3-tile-high hallways.
                    // Owner-only: GameUpdateCount is not synced, so a non-owner would bob at a
                    // different sine phase and fight the owner's netUpdate, causing jitter.
                    if (Main.myPlayer == Projectile.owner && Math.Abs(Projectile.velocity.X) > 0.1f)
                    {
                        bool lowCeiling = Collision.SolidCollision(
                            new Vector2(Projectile.position.X, Projectile.position.Y - 16f),
                            Projectile.width, 16);
                        // Walking a path on the ground: ground probes are disabled during
                        // path-follow and tileCollide is off, so the bob pumps her INTO the
                        // floor — then waypoint steering pulls her back up to the path line
                        // next ticks. That push-down/pull-up cycle reads as a small vertical
                        // stutter while she walks. Ground within 1.5 tiles (24px) underfoot
                        // (platforms/slopes included, same reach as the settle scan probe)
                        // → she is WALKING, not floating: skip the bob.
                        bool walkingOnGround = _followPath.Count > 0 && Collision.SolidCollision(
                            new Vector2(Projectile.position.X, Projectile.position.Y + Projectile.height),
                            Projectile.width, 24, true);
                        if (!lowCeiling && !walkingOnGround)
                            Projectile.velocity.Y += 0.35f * MathF.Sin(Main.GameUpdateCount * (MathF.PI * 2f / 100f));
                    }
                    // Force a denser sync while following so the client's velocity refreshes every
                    // ~2 ticks instead of every 3-4, keeping vertical motion smooth on remotes.
                    if (Main.myPlayer == Projectile.owner && _followPath.Count > 0 && Main.GameUpdateCount % 2 == 0)
                        Projectile.netUpdate = true;
                }
                // Ground-riding correction: nudge Saria's Y position so she rides just
                // above tile surfaces without clipping into them.
                // Probe 1 (hitbox): is her body overlapping a solid tile? → push up.
                // Probe 2 (ground line): 20px wide line directly under her feet — is it
                //   touching a tile? → she is properly grounded, do nothing.
                // Scan probe: 1.5 tiles (24px) tall below feet — is there a tile nearby?
                //   If ground line is clear but tile is close, and she's not sleeping → settle down.
                // Platforms, half-tiles, and slopes are included via acceptTopSurfaces=true.
                // X velocity is never touched. Settle-down is skipped while sleeping.
                {
                    // Ground-probe corrections only apply when Saria is within TileCollisionRadius
                    // of the player (non-Follow modes). Outside that range she floats freely.
                    // Hysteresis: probe activates at TileCollisionRadius, deactivates only at
                    // TileCollisionRadius + TileProbeHysteresis to prevent boundary jitter.
                    if (!Follow && !_cursedSeparated && !(_linkCableFollow && _followPath.Count > 0))
                    {
                        float distToPlayer = Vector2.Distance(Projectile.Center, player.Center);
                        // Fix 3: out-of-bounds recovery — disable probes immediately if Saria leaves the
                        // playable world, and only re-enable them once she has returned AND reached her
                        // idle position. This prevents her from re-entering the world still clipped into tiles.
                        bool outOfBounds = Projectile.Center.Y > (Main.maxTilesY - 10) * 16f
                                        || Projectile.Center.Y < 0f
                                        || Projectile.Center.X > (Main.maxTilesX - 10) * 16f
                                        || Projectile.Center.X < 0f;
                        _dbgOutOfBounds = outOfBounds;
                        bool nearIdle = distanceToIdlePosition <= 1f && !outOfBounds;
                        // Both wall probes fired last frame → Saria is wedged between two walls.
                        // Disable all detector corrections so she can phase through to idle position,
                        // mirroring the "too far from player" bypass. Re-enables naturally via nearIdle.
                        bool bothWallsWedged = _detectorResults[2].Pink && _detectorResults[3].Pink;
                        if ((outOfBounds && CanMove > 0) || bothWallsWedged)
                        {
                            ProbesActive = false;
                        }
                        else if (!ProbesActive)
                        {
                            // Re-enable only when within range AND at most one detector is active.
                            // Two or more active means she's still in cramped/solid geometry — stay off.
                            // LinkCable arrived: skip the player-distance gate entirely.
                            bool withinRange = _linkCableFollow
                                ? !outOfBounds
                                : distToPlayer <= TileCollisionRadius && !outOfBounds;
                            int activeCount =
                                (_detectorResults[0].IsActive ? 1 : 0) +
                                (_detectorResults[1].IsActive ? 1 : 0) +
                                (_detectorResults[2].Pink     ? 1 : 0) +
                                (_detectorResults[3].Pink     ? 1 : 0);
                            if (withinRange && activeCount <= 1 && !_inWall)
                                ProbesActive = true;
                        }
                        else if (ProbesActive && !_linkCableFollow && distToPlayer > TileCollisionRadius + TileProbeHysteresis && CanMove > 0)
                            ProbesActive = false;
                    }
                    else
                    {
                        // Following A* path: disable probes so detectors don't fight path movement.
                        if (_followPath.Count > 0)
                            ProbesActive = false;
                        else
                            // Cursed mode keeps probes on while not separated.
                            ProbesActive = !_cursedSeparated;
                    }
                    bool applyProbes = ProbesActive;

                    // InWall detection — uses the body-fit box and orange box to detect solid tile coverage.
                    {
                        Vector2 iwSpritePos = new Vector2(
                            (float)Math.Round(Projectile.position.X),
                            (float)Math.Round(Projectile.position.Y));

                        SariaDetector.GetFacingDir(_detectorConfigs[0].RotationDegrees, out int iw0x, out int iw0y);
                        SariaDetector.GetFacingDir(_detectorConfigs[2].RotationDegrees, out int iw2x, out int iw2y);
                        SariaDetector.GetFacingDir(_detectorConfigs[3].RotationDegrees, out int iw3x, out int iw3y);

                        SariaDetector.GetProbeRects(in _detectorConfigs[0], iwSpritePos, iw0x, iw0y,
                            out _, out Rectangle iwFeet, out _,
                            Projectile.width, Projectile.spriteDirection);
                        SariaDetector.GetProbeRects(in _detectorConfigs[2], iwSpritePos, iw2x, iw2y,
                            out Rectangle iwWallLPink, out Rectangle iwWallL, out _);
                        SariaDetector.GetProbeRects(in _detectorConfigs[3], iwSpritePos, iw3x, iw3y,
                            out Rectangle iwWallRPink, out Rectangle iwWallR, out _);

                        // Body-fit box scan (inner green faces + feet bottom).
                        // bBottom stops one pixel above the feet green bottom so the ground
                        // tile itself is never counted — prevents false positives when moving fast.
                        int bLeft   = iwWallL.Right;
                        int bRight  = iwWallR.Left - 2;
                        int bTop    = Math.Min(iwWallL.Y, iwWallR.Y);
                        int bBottom = iwFeet.Bottom - 2 - 3;

                        int tLeft   = bLeft  / 16;
                        int tRight  = (bRight  - 1) / 16;
                        int tTop    = bTop    / 16;
                        int tBottom = (bBottom - 1) / 16;

                        int total = 0, solid = 0;
                        for (int tx = tLeft; tx <= tRight; tx++)
                        {
                            for (int ty = tTop; ty <= tBottom; ty++)
                            {
                                total++;
                                if (tx < 0 || ty < 0 || tx >= Main.maxTilesX || ty >= Main.maxTilesY)
                                { solid++; continue; }
                                Tile t = Main.tile[tx, ty];
                                if (t.HasTile && Main.tileSolid[t.TileType]
                                    && !Main.tileSolidTop[t.TileType] && !t.IsActuated
                                    && t.Slope == SlopeType.Solid && !t.IsHalfBlock)
                                    solid++;
                            }
                        }
                        bool overallCoverage = total > 0 && (float)solid / total >= 0.25f;

                        int tCenterX = (bLeft + bRight) / 2 / 16;
                        bool spineSolid = true;
                        for (int ty = tTop; ty <= tBottom; ty++)
                        {
                            if (tCenterX < 0 || ty < 0 || tCenterX >= Main.maxTilesX || ty >= Main.maxTilesY)
                                continue;
                            Tile t = Main.tile[tCenterX, ty];
                            if (!(t.HasTile && Main.tileSolid[t.TileType]
                                  && !Main.tileSolidTop[t.TileType] && !t.IsActuated))
                            { spineSolid = false; break; }
                        }
                        if (tTop > tBottom) spineSolid = false;

                        // Orange box scan (wall probe pink rects) — 40% coverage triggers inwall.
                        int obLeft   = iwWallLPink.Left;
                        int obRight  = iwWallRPink.Right - 2;
                        int obTop    = Math.Min(iwWallLPink.Top,    iwWallRPink.Top);
                        int obBottom = Math.Max(iwWallLPink.Bottom, iwWallRPink.Bottom);

                        int obTLeft   = obLeft  / 16;
                        int obTRight  = (obRight  - 1) / 16;
                        int obTTop    = obTop    / 16;
                        int obTBottom = (obBottom - 1) / 16;

                        int obTotal = 0, obSolid = 0;
                        for (int tx = obTLeft; tx <= obTRight; tx++)
                        {
                            for (int ty = obTTop; ty <= obTBottom; ty++)
                            {
                                obTotal++;
                                if (tx < 0 || ty < 0 || tx >= Main.maxTilesX || ty >= Main.maxTilesY)
                                { obSolid++; continue; }
                                Tile t = Main.tile[tx, ty];
                                if (t.HasTile && Main.tileSolid[t.TileType]
                                    && !Main.tileSolidTop[t.TileType] && !t.IsActuated)
                                    obSolid++;
                            }
                        }
                        bool orangeCoverage = obTotal > 0 && (float)obSolid / obTotal >= 0.25f;

                        _inWall = overallCoverage || spineSolid || orangeCoverage;
                        _dbgOverallCoverage = total > 0 ? (float)solid / total : 0f;
                        _dbgOrangeCoverage  = obTotal > 0 ? (float)obSolid / obTotal : 0f;
                    }

                    // Escape-target search and teleport state machine — owner-only.
                    // Phase 1: _inWall true → _inWallStuckTimer counts up to InWallStuckThreshold.
                    // Phase 2: threshold reached → _inWallTeleportTimer set to InWallTeleportDuration;
                    //          target locked, cannot be changed, stuck timer halted.
                    // Phase 3: teleport timer reaches 0 → teleport executed, everything reset.
                    if (Main.myPlayer == Projectile.owner)
                    {
                        if (_inWallTeleportTimer > 0)
                        {
                            // ── Teleport phase: target is locked, count down ──
                            _inWallTeleportTimer--;

                            if (_inWallTeleportTimer == 0)
                            {
                                // Execute teleport.
                                if (_inWallEscapeTarget != Vector2.Zero)
                                {
                                    // Burst: source position (where she was).
                                    if (Main.netMode != NetmodeID.Server)
                                    {
                                        // Stop wind-up loop and play completion sting (mirrors transform end).
                                            if (SoundEngine.TryGetActiveSound(_tpLoopSlot, out ActiveSound tpDone))
                                                tpDone.Stop();
                                            if (SoundEngine.TryGetActiveSound(_tpDestLoopSlot, out ActiveSound tpDestDone))
                                                tpDestDone.Stop();
                                        SpawnTeleportBurst(Projectile.Center);
                                        SoundEngine.PlaySound(SoundID.Item4, Projectile.Center);
                                    }

                                    // Align body-fit box bottom (spritePos.Y + 76) with the footprint bottom.
                                    const float bBottomOffset     = 76f;
                                    float footprintHalfPx = FollowPathFootprintHeight * 8f; // 3 * 8 = 24
                                    Projectile.position = new Vector2(
                                        _inWallEscapeTarget.X - Projectile.width  * 0.5f,
                                        _inWallEscapeTarget.Y + footprintHalfPx - bBottomOffset);
                                    Projectile.velocity  = Vector2.Zero;
                                    Projectile.netUpdate = true;

                                    // Burst: destination position (where she landed).
                                    if (Main.netMode != NetmodeID.Server)
                                    {
                                        SpawnTeleportBurst(Projectile.Center);
                                        // Play without position so it's always audible even when
                                        // the destination is far off-screen.
                                        SoundEngine.PlaySound(SoundID.Item4);
                                    }
                                }
                                // Reset all in-wall escape state.
                                bool wasInWallEscape = _pathTeleportTimer <= 0 && _idleTeleportTimer <= 0;
                                _inWallEscapeTarget = Vector2.Zero;
                                _inWallStuckTimer   = 0;
                                _tpActiveDuration   = 0;

                                // Re-enable probes after a pure in-wall escape teleport so she
                                // snaps back to normal collision handling at the new position.
                                if (wasInWallEscape)
                                    ProbesActive = true;

                                // If this was a path-teleport, clear the path so she replans from
                                // the new position rather than resuming the old broken path.
                                if (_pathTeleportTimer > 0)
                                {
                                    _pathTeleportTimer    = 0;
                                    _pathTeleportTarget   = Vector2.Zero;
                                    _followPath.Clear();
                                    _followPathLastGoal   = Vector2.Zero;
                                    Projectile.netUpdate  = true;
                                }

                                // If this was an idle far-teleport, clear its state.
                                if (_idleTeleportTimer > 0)
                                {
                                    _idleTeleportTimer  = 0;
                                    _idleTeleportTarget = Vector2.Zero;
                                }
                            }
                        }
                        else if (_inWall)
                        {
                            // ── Phase 1: accumulate stuck time ──
                            _inWallStuckTimer++;

                            // Search for escape target every tick while stuck so it stays fresh.
                            // Priority target depends on separation state:
                            //   _cursedSeparated true  → bias toward player (she wants to get back)
                            //   _cursedSeparated false → bias toward marked location (continue the path)
                            int iwOriginX = (int)Math.Floor(Projectile.Center.X / 16f) - FollowPathFootprintWidth  / 2;
                            int iwOriginY = (int)Math.Floor(Projectile.Center.Y / 16f) - FollowPathFootprintHeight / 2;
                            var iwOrigin = new Microsoft.Xna.Framework.Point(iwOriginX, iwOriginY);

                            Vector2 iwPriority = _cursedSeparated
                                ? (_followMarkedPosition != Vector2.Zero ? _followMarkedPosition : player.Center)
                                : player.Center;

                            var iwCandidate = SariaPathfinder.NudgeTowardTarget(
                                iwOrigin, FollowPathFootprintWidth, FollowPathFootprintHeight, iwPriority);

                            _inWallEscapeTarget = iwCandidate.X != int.MinValue
                                ? new Vector2(
                                    (iwCandidate.X + FollowPathFootprintWidth  * 0.5f) * 16f,
                                    (iwCandidate.Y + FollowPathFootprintHeight * 0.5f) * 16f)
                                : Vector2.Zero;

                            // Threshold reached → lock target and enter teleport phase.
                            if (_inWallStuckTimer >= InWallStuckThreshold && _inWallEscapeTarget != Vector2.Zero)
                            {
                                // Target is now frozen for the entire teleport wind-up.
                                StartTeleportWindUp(_inWallEscapeTarget, InWallTeleportDuration);
                            }
                        }
                        else
                        {
                            // Not stuck — reset both timers and clear target.
                            _inWallStuckTimer   = 0;
                            _inWallEscapeTarget = Vector2.Zero;
                            // Note: _inWallTeleportTimer is NOT reset here; once teleport is
                            // committed it runs to completion even if _inWall briefly clears.
                        }
                    }

                    SariaDetector.Apply(_detectorConfigs, _detectorResults, Projectile.position,
                        applyProbes, Sleep, ref Projectile.position, ref Projectile.velocity,
                        Projectile.width, Projectile.spriteDirection);

                    // Cache results for debug draw.
                    _dbgHitboxInTile   = _detectorResults[0].Pink;
                    _dbgGroundTouching = _detectorResults[0].Green;
                    _dbgTileBelow      = _detectorResults[0].Yellow;
                    _dbgWallLeft       = _detectorResults[2].Pink;
                    _dbgWallRight      = _detectorResults[3].Pink;
                    _wallPausedLeft    = _detectorResults[2].GreenPaused;
                    _wallPausedRight   = _detectorResults[3].GreenPaused;

                    // Pressure plate trigger — owner only.
                    // Scans the bottom pixel row of Saria's hitbox for any tile that
                    // vanilla marks as a player pressure plate. Fires HitSwitch once on
                    // the rising edge (first tick she overlaps it) and resets when she
                    // fully leaves, matching how vanilla handles player stepping on plates.
                    if (Main.myPlayer == Projectile.owner)
                    {
                        int ppTileY     = (int)((Projectile.position.Y + Projectile.height + -16) / 16f);
                        int ppTileXLeft = (int)(Projectile.position.X / 16f);
                        int ppTileXRight= (int)((Projectile.position.X + Projectile.width - 1) / 16f);

                        bool onPlateNow = false;
                        for (int tx = ppTileXLeft; tx <= ppTileXRight; tx++)
                        {
                            if (tx < 0 || ppTileY < 0 || tx >= Main.maxTilesX || ppTileY >= Main.maxTilesY)
                                continue;
                            Tile t = Main.tile[tx, ppTileY];
                            if (!t.HasTile) continue;
                            // All player-triggerable pressure plate tile types (vanilla IDs).
                            int tt = t.TileType;
                            bool isPlayerPlate = tt == 135  // Red
                                              || tt == 137  // Green
                                              || tt == 138  // Gray
                                              || tt == 262  // Lihzahrd
                                              || tt == 420; // Brown
                            if (!isPlayerPlate) continue;

                            onPlateNow = true;
                                if (!_wasOnPressurePlateLastFrame)
                                {
                                    if (Main.netMode == NetmodeID.MultiplayerClient)
                                        NetMessage.SendData(MessageID.HitSwitch, -1, -1, null, tx, ppTileY);
                                    else
                                        Wiring.HitSwitch(tx, ppTileY);
                                }
                        }
                        _wasOnPressurePlateLastFrame = onPlateNow;
                    }
                    _wasGroundedLastFrame = _dbgGroundTouching;

                    // Fix 2: if the physics detector confirms a wall on Saria's idle side,
                    // reinforce the hold timer so CachedClose doesn't ease outward into the tile.
                    float _distToPlayerForWall = Vector2.Distance(Projectile.Center, player.Center);
                    _closeTracker.ReinforceFromWall(_dbgWallLeft, _dbgWallRight, player.direction, _distToPlayerForWall);

                    // Fix 3: zero out X velocity toward the wall so Saria waits instead of jittering.
                    // Skipped while following A* path so walls on the route don't block her.
                    bool wallOnIdleSide = (player.direction > 0 && _dbgWallRight) || (player.direction < 0 && _dbgWallLeft);
                    if (wallOnIdleSide && _followPath.Count == 0)
                    {
                        float towardWall = player.direction; // +1 right, -1 left
                        if (Math.Sign(Projectile.velocity.X) == (int)towardWall)
                            Projectile.velocity.X = 0f;
                    }
                }

                // Door auto-open/close — only while she is actively traversing an A* trail.
                if (Main.myPlayer == Projectile.owner && _followPath.Count > 0)
                {
                    // Use the orange hitbox — the bounding rect of the two pink wall-probe
                    // rectangles (configs [2] left and [3] right), exactly as drawn by the
                    // debug overlay. This is the narrow center-body box, not the full hitbox.
                    Vector2 spritePos = new Vector2(
                        (float)Math.Round(Projectile.position.X),
                        (float)Math.Round(Projectile.position.Y));

                    SariaDetector.GetFacingDir(_detectorConfigs[2].RotationDegrees, out int owlx, out int owly);
                    SariaDetector.GetFacingDir(_detectorConfigs[3].RotationDegrees, out int owrx, out int owry);
                    SariaDetector.GetProbeRects(in _detectorConfigs[2], spritePos, owlx, owly,
                        out Rectangle orangeWallL, out _, out _);
                    SariaDetector.GetProbeRects(in _detectorConfigs[3], spritePos, owrx, owry,
                        out Rectangle orangeWallR, out _, out _);

                    // Bounding rect of both pink probes (matches the orange box in the debug overlay).
                    int orangeLeft   = orangeWallL.Left;
                    int orangeRight  = orangeWallR.Right;
                    int orangeTop    = Math.Min(orangeWallL.Top,    orangeWallR.Top);
                    int orangeBottom = Math.Max(orangeWallL.Bottom, orangeWallR.Bottom);

                    // Convert pixel rect to tile range, extended 1 tile out on each side in X.
                    int fpLeft   = (int)Math.Floor((float)orangeLeft  / 16f) - 1;
                    int fpRight  = (int)Math.Floor((float)orangeRight / 16f) + 1;
                    int fpTop    = (int)Math.Floor((float)orangeTop    / 16f);
                    int fpBottom = (int)Math.Floor((float)orangeBottom / 16f);

                    var touchingDoors = new System.Collections.Generic.HashSet<Point>();

                    for (int tx = fpLeft; tx <= fpRight; tx++)
                    {
                        for (int ty = fpTop; ty <= fpBottom; ty++)
                        {
                            if (tx < 0 || ty < 0 || tx >= Main.maxTilesX || ty >= Main.maxTilesY)
                                continue;
                            Tile t = Main.tile[tx, ty];
                            if (!t.HasTile) continue;

                            if (t.TileType == TileID.ClosedDoor || t.TileType == TileID.TallGateClosed)
                            {
                                // Normalize ty to the TOP tile of this door column so the stored key
                                // is the same no matter which tile in the 3-tall multi-tile the scan
                                // happens to hit. Without this, a single-pixel shift in Saria's
                                // vertical position changes fpTop/fpBottom, dropping the stored ty
                                // out of the scan range — the open-door tiles then add a different
                                // (tx, otherTy) to touchingDoors, the key isn't found, and the door
                                // is spuriously closed then immediately re-opened on the next tick.
                                int closedTopTy = ty;
                                while (closedTopTy > 0
                                       && Main.tile[tx, closedTopTy - 1].HasTile
                                       && Main.tile[tx, closedTopTy - 1].TileType == t.TileType)
                                    closedTopTy--;

                                // Try spriteDirection first; if blocked try the other side.
                                int dir     = Projectile.spriteDirection;
                                int usedDir = dir;
                                bool opened = WorldGen.OpenDoor(tx, ty, dir);
                                if (!opened)
                                {
                                    usedDir = -dir;
                                    opened  = WorldGen.OpenDoor(tx, ty, -dir);
                                }

                                if (opened)
                                {
                                    // Key is the HINGE column (original tx) at the normalized top tile.
                                    // After opening, the hinge tile keeps TileID.OpenDoor so the scan
                                    // always finds it at the same position, making the key stable.
                                    // Using the panel column (tx+usedDir) was wrong because the panel
                                    // often falls outside the narrow wall-probe scan range, so the key
                                    // was never matched → spurious close+reopen every tick.
                                    var openKey = new Point(tx, closedTopTy);
                                    _sariaOpenedDoors.Add(openKey);
                                    touchingDoors.Add(openKey);
                                    // Broadcast the open so all clients + server update. The owner is
                                    // usually a client, so gating on Server alone never synced. Action 0 = open.
                                    if (Main.netMode != NetmodeID.SinglePlayer)
                                        NetMessage.SendData(MessageID.ToggleDoorState, -1, -1, null, 0, tx, ty, usedDir);
                                }
                            }

                            if (t.TileType == TileID.OpenDoor || t.TileType == TileID.TallGateOpen)
                            {
                                // Normalize to the top tile of this open door column so the key here
                                // matches the key stored in _sariaOpenedDoors on the open tick.
                                int openTopTy = ty;
                                while (openTopTy > 0
                                       && Main.tile[tx, openTopTy - 1].HasTile
                                       && Main.tile[tx, openTopTy - 1].TileType == t.TileType)
                                    openTopTy--;
                                touchingDoors.Add(new Point(tx, openTopTy));
                            }
                        }
                    }

                    // Close doors she opened that she's no longer touching.
                    var toClose = new System.Collections.Generic.List<Point>();
                    foreach (Point dp in _sariaOpenedDoors)
                    {
                        if (!touchingDoors.Contains(dp))
                            toClose.Add(dp);
                    }
                    foreach (Point dp in toClose)
                    {
                        _sariaOpenedDoors.Remove(dp);
                        if (dp.X >= 0 && dp.Y >= 0 && dp.X < Main.maxTilesX && dp.Y < Main.maxTilesY)
                        {
                            bool closed = WorldGen.CloseDoor(dp.X, dp.Y, false);
                            // Broadcast the close so all clients + server update. Action 1 = close.
                            if (closed && Main.netMode != NetmodeID.SinglePlayer)
                                NetMessage.SendData(MessageID.ToggleDoorState, -1, -1, null, 1, dp.X, dp.Y, 0);
                        }
                    }
                }
                if (Eating <= 0 && !Sneezing && !Sleep && player.ownedProjectileCounts[ModContent.ProjectileType<FrozenYogurtSignal>()] > 0f && distanceToIdlePosition <= 20 && Projectile.frame < 36)
                {
                    SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Healpulse"), player.Center);
                    player.AddBuff(ModContent.BuffType<Soothing>(), 18000);
                    SetMoodFor(MoodState.Happy, 600, priority: 1);
                    Eating = 3;
                    Projectile.frame = 0;
                }
                if (Eating <= 0 && !Sneezing && !Sleep && player.ownedProjectileCounts[ModContent.ProjectileType<Competitivetime>()] > 0f && distanceToIdlePosition <= 20 && Projectile.frame < 36)
                {
                    player.AddBuff(ModContent.BuffType<Overcharged>(), 45000);
                    SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/StatRaise"), Projectile.Center);
                    if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<PowerUp>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, Projectile.whoAmI);
                    SetMoodFor(MoodState.Happy, 800, priority: 1);
                    Eating = 4;
                    Projectile.frame = 0;
                }
                if (Eating == 3 || Eating == 4 || Eating ==5)
                {
                    Projectile.spriteDirection = 1;
                }
                if (player.statLife < (player.statLifeMax2) / 4 && !player.HasBuff(ModContent.BuffType<HealpulseBuff>()) && !player.HasBuff(ModContent.BuffType<Sickness>()) && !player.HasBuff(ModContent.BuffType<BloodmoonBuff>()) && !player.HasBuff(ModContent.BuffType<EclipseBuff>()))
                {
                    SetMoodFor(MoodState.Sad, 420, priority: 1);
                    player.statLife += 500;
                    player.AddBuff(ModContent.BuffType<HealpulseBuff>(), 3000);
                    for (int j = 0; j < 1; j++) //set to 2
                    {
                        if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<Heal>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, Projectile.whoAmI);
                    }
                }
                if (Projectile.NewIdlePosition(51))
                {
                    if (Projectile.velocity.X >= 0.25)
                    {
                        Projectile.spriteDirection = 1;
                    }
                    if (Projectile.velocity.X <= -0.25)
                    {
                        Projectile.spriteDirection = -1;
                    }
                }
                else if (!Projectile.NewIdlePosition(51))
                {
                    if (Projectile.velocity.X >= 1.25)
                    {
                        Projectile.spriteDirection = 1;
                    }
                    if (Projectile.velocity.X <= -1.25)
                    {
                        Projectile.spriteDirection = -1;
                    }
                }

                if (Projectile.frame == 25 && Sleep && MoveTimer >= 550)
                {
                    Projectile.SneezeDust(ModContent.DustType<Z>(), 40, 1, -10, 3, -12);
                }
                ///Sleep Ai
                if ((Math.Abs(Projectile.velocity.X) >= 0.5f) || (Math.Abs(Projectile.velocity.Y) >= 0.5f))
                {
                    MoveTimer = 0;
                }
                if ((Math.Abs(Projectile.velocity.X) < 0.5f) && (Math.Abs(Projectile.velocity.Y) < 0.5f))
                {
                    int moveTimerRate = HoldingHealBall ? 1 : 2;
                    if (MoveTimer < 10000 && Sleep)
                    {
                        MoveTimer += 1;
                    }
                    else if (MoveTimer < 5000 && !Sleep)
                    {
                        MoveTimer += moveTimerRate;
                    }
                }
                if (SariaTalking && !Sleep)
                {
                    if (MoveTimer >= 277)
                    {
                        MoveTimer = 276;
                    }
                }
                if (MoveTimer == 0)
                {
                    Sleep = false;
                    SleepHeal = 0;
                    Projectile.netUpdate = true;
                }
                if (Sleep && MoveTimer >= 8000 && SleepHeal <= 0 && (Main.myPlayer == Projectile.owner))
                {
                    SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Healpulse"), player.Center);
                    player.AddBuff(ModContent.BuffType<Soothing>(), 44000);
                    SetMoodFor(MoodState.Normal, 180, priority: 0);
                    SleepHeal = 1;
                    if (player.HasBuff(ModContent.BuffType<Drained>()))
                    {
                        player.ClearBuff(ModContent.BuffType<Drained>());
                    }
                }
                if (Sleep && MoveTimer >= 10000 && (Main.myPlayer == Projectile.owner))
                {
                    if (player.HasBuff(ModContent.BuffType<Drained>()))
                    {
                        player.ClearBuff(ModContent.BuffType<Drained>());
                    }
                    if (MoveTimer >= 10000)
                    {
                        player.AddBuff(ModContent.BuffType<Overcharged>(), 30000);
                        SicknessBar = SicknessBarMax;
                        if (SoundTimer <= 0)
                        {
                            SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/StatRaise"), Projectile.Center);
                            for (int j = 0; j < 1; j++) //set to 2
                            {
                                if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<PowerUp>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, Projectile.whoAmI);
                            }
                            SoundTimer = 1;
                        }
                        SetMoodFor(MoodState.Happy, 180, priority: 5);
                        MoveTimer = 0;
                        SoundTimer = 0;
                        Projectile.netUpdate = true;
                    }
                }
                if (player.sleeping.isSleeping && Eating <= 0)
                {
                    if (MoveTimer <= 6000)
                    {
                        MoveTimer = 6000;
                    }
                    if (IsPlayerAsleep && !Sleep)
                    {
                        if (Projectile.frame < 14)
                        {
                            Projectile.frame = 14;
                        }
                    }
                    else if (!IsPlayerAsleep && !Sleep && ChannelState <= 0)
                    {
                        if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<Notice>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, Projectile.whoAmI);
                        IsPlayerAsleep = true;
                    }
                }
                if (!player.sleeping.isSleeping)
                {
                    IsPlayerAsleep = false;
                }
                if (MoveTimer >= (5000) && Projectile.frame == 19 && !foundTarget && Eating <= 0 && Mood != (int)MoodState.Cursed)
                {
                    Sleep = true;
                    Projectile.netUpdate = true;
                }
                ///eatingAI
                if (Projectile.frame == 25 && (Eating == 3 || Eating == 4))
                {
                    Projectile.SneezeDust(ModContent.DustType<Fog>(), 1, 1, -10, 10, -17);
                }
                if (Projectile.frame == 37 && (Eating == 3 || Eating == 4))
                {
                    Eating = 5;
                    Projectile.frame = 22;
                    Vector2 Throw = Projectile.Center;
                    Throw.Y += 0f;
                    Throw.X += 40f;
                    Vector2 ThrowToo = Projectile.DirectionTo(Main.MouseWorld).SafeNormalize(Vector2.Zero);
                    if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Throw, ThrowToo * 10, ModContent.ProjectileType<EmptyCup>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, Projectile.whoAmI);
                }

                ///end of sleep ai
                // Update psychic eye overlay opacity (fade-in during charge/attack/flash cooldown, ~2s fade-out after)
                bool flashCooldownActive = FlashCooldownTimer > 0;
                SariaPsychicEyes.UpdateOpacity(Projectile, ChannelState, Transform, flashCooldownActive);
                // Visual-only eye-tracking tick — runs on EVERY client so remote
                // observers see Saria's eyes follow her owner just like the owner does.
                // Reads only synced state (_eyeFreeMode, _eyeLookingBack, projectile.owner)
                // so it cannot desync gameplay.
                IdleAnimator.UpdateEyeOffsetVisual(Projectile, (int)Transform);

                // Non-owner clients: force states directly from synced bools every tick
                // so the transition animations never flicker between packets.
                if (Projectile.owner != Main.myPlayer)
                {
                    //IdleAnimator.ApplySyncedLegState(LegsIsCasual, LegsGoingToCasual, LegsIsProper, LegsGoingToProper); // commented out to verify eye fix independently
                    IdleAnimator.ApplySyncedLegState(LegsIsCasual, LegsGoingToCasual, LegsIsProper, LegsGoingToProper);
                    IdleAnimator.ApplySyncedArmState(ArmsIsDown, ArmsGoingUp, ArmsIsUp, ArmsGoingDown);
                    IdleAnimator.ApplySyncedEyeState(EyesLooking, EyesBlinking, EyesOpening);
                    IdleAnimator.DisplayedMood = DisplayedMoodSync;

                    // Detect transform start/end from synced TransformTimer for non-owner sound playback
                    if (Main.netMode != NetmodeID.Server)
                    {
                        bool remoteJustStarted = _prevTransformTimerRemote <= 0 && TransformTimer > 0;
                        bool remoteJustEnded   = _prevTransformTimerRemote > 0  && TransformTimer == 0;
                        if (remoteJustStarted)
                        {
                            SoundEngine.PlaySound(SoundID.Item4, Projectile.Center);
                            if (SoundEngine.TryGetActiveSound(_transformLoopSlot, out ActiveSound old))
                                old.Stop();
                            _transformLoopSlot = SoundEngine.PlaySound(
                                new SoundStyle("SariaMod/Sounds/TransformLoop") { MaxInstances = 1, Volume = 0.7f },
                                Projectile.Center);
                        }
                        else if (remoteJustEnded)
                        {
                            if (SoundEngine.TryGetActiveSound(_transformLoopSlot, out ActiveSound done))
                                done.Stop();
                            SoundEngine.PlaySound(SoundID.Item4, Projectile.Center);
                        }

                        // Keep loop position tracking Saria every tick
                        if (TransformTimer > 0 && SoundEngine.TryGetActiveSound(_transformLoopSlot, out ActiveSound active))
                            active.Position = Projectile.Center;

                        _prevTransformTimerRemote = TransformTimer;
                    }
                }

                // Teleport wind-up detection — outside the non-owner block so it runs for all clients.
                if (Main.netMode != NetmodeID.Server && Projectile.owner != Main.myPlayer)
                {
                    bool tpJustStarted = _prevTeleportTimerRemote <= 0 && _inWallTeleportTimer > 0;
                    bool tpJustEnded   = _prevTeleportTimerRemote > 0  && _inWallTeleportTimer == 0;

                    if (tpJustStarted)
                    {
                        // Cache both positions now — by the time tpJustEnded fires the
                        // netUpdate will have snapped Projectile.Center and zeroed _inWallEscapeTarget.
                        _tpCachedSrc  = Projectile.Center;
                        _tpCachedDest = _inWallEscapeTarget;

                        SoundEngine.PlaySound(SoundID.Item4, Projectile.Center);
                        if (SoundEngine.TryGetActiveSound(_tpLoopSlot, out ActiveSound tpOld))
                            tpOld.Stop();
                        _tpLoopSlot = SoundEngine.PlaySound(
                            new SoundStyle("SariaMod/Sounds/TransformLoop") { MaxInstances = 1, Volume = 0.7f },
                            Projectile.Center);
                        for (int _i = 0; _i < 20; _i++)
                        {
                            Vector2 _vel = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(1.5f, 4f);
                            Dust _d = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<AbsorbPsychic>(), _vel, Scale: 1.4f);
                            _d.noGravity = true;
                        }
                    }
                    else if (tpJustEnded)
                    {
                        if (SoundEngine.TryGetActiveSound(_tpLoopSlot,     out ActiveSound src)) src.Stop();
                        if (SoundEngine.TryGetActiveSound(_tpDestLoopSlot, out ActiveSound dst)) dst.Stop();
                        SoundEngine.PlaySound(SoundID.Item4, _tpCachedSrc != Vector2.Zero ? _tpCachedSrc : Projectile.Center);
                        SoundEngine.PlaySound(SoundID.Item4);
                        // Source burst — at Saria's position before the teleport.
                        SpawnTeleportBurst(_tpCachedSrc != Vector2.Zero ? _tpCachedSrc : Projectile.Center);
                        // Destination burst — at the locked teleport target.
                        if (_tpCachedDest != Vector2.Zero)
                        {
                            SpawnTeleportBurst(_tpCachedDest);
                        }
                        _tpCachedSrc  = Vector2.Zero;
                        _tpCachedDest = Vector2.Zero;
                    }

                    // Snapshot BEFORE the decrement so that on the tick after the countdown
                    // reaches 0, _prevTeleportTimerRemote is still positive and tpJustEnded fires.
                    _prevTeleportTimerRemote = _inWallTeleportTimer;

                    if (_inWallTeleportTimer > 0)
                    {
                        _inWallTeleportTimer--;
                        TickTeleportPhase();
                        if (SoundEngine.TryGetActiveSound(_tpLoopSlot, out ActiveSound tpActive))
                            tpActive.Position = Projectile.Center;
                    }
                }
                if (Projectile.owner == Main.myPlayer)
                {
                    bool newLegsIsCasual      = IdleAnimator.CurrentLegState == SariaIdleAnimator.LegState.Casual;
                    bool newLegsGoingToCasual  = IdleAnimator.CurrentLegState == SariaIdleAnimator.LegState.GoingToCasual;
                    bool newLegsIsProper       = IdleAnimator.CurrentLegState == SariaIdleAnimator.LegState.Proper;
                    bool newLegsGoingToProper  = IdleAnimator.CurrentLegState == SariaIdleAnimator.LegState.GoingToProper;
                    bool newArmsIsDown         = IdleAnimator.CurrentArmState == SariaIdleAnimator.ArmState.Down;
                    bool newArmsGoingUp        = IdleAnimator.CurrentArmState == SariaIdleAnimator.ArmState.GoingUp;
                    bool newArmsIsUp           = IdleAnimator.CurrentArmState == SariaIdleAnimator.ArmState.Up;
                    bool newArmsGoingDown      = IdleAnimator.CurrentArmState == SariaIdleAnimator.ArmState.GoingDown;
                    bool newEyesLooking        = IdleAnimator.CurrentEyeState == SariaIdleAnimator.EyeState.Looking;
                    bool newEyesBlinking       = IdleAnimator.CurrentEyeState == SariaIdleAnimator.EyeState.Blinking;
                    bool newEyesOpening        = IdleAnimator.CurrentEyeState == SariaIdleAnimator.EyeState.Opening;
                    int newDisplayedMoodSync   = IdleAnimator.DisplayedMood;
                    if (Projectile.frame != frameToSync || Projectile.spriteDirection != directionToSync
                        || newLegsIsCasual != LegsIsCasual || newLegsGoingToCasual != LegsGoingToCasual
                        || newLegsIsProper != LegsIsProper || newLegsGoingToProper != LegsGoingToProper
                        || newArmsIsDown != ArmsIsDown || newArmsGoingUp != ArmsGoingUp
                        || newArmsIsUp != ArmsIsUp || newArmsGoingDown != ArmsGoingDown
                        || newEyesLooking != EyesLooking || newEyesBlinking != EyesBlinking
                        || newEyesOpening != EyesOpening || newDisplayedMoodSync != DisplayedMoodSync)
                    {
                        frameToSync = Projectile.frame;
                        syncedFrameCounter = Projectile.frameCounter;
                        directionToSync = Projectile.spriteDirection;
                        LegsIsCasual     = newLegsIsCasual;
                        LegsGoingToCasual = newLegsGoingToCasual;
                        LegsIsProper     = newLegsIsProper;
                        LegsGoingToProper = newLegsGoingToProper;
                        ArmsIsDown    = newArmsIsDown;
                        ArmsGoingUp   = newArmsGoingUp;
                        ArmsIsUp      = newArmsIsUp;
                        ArmsGoingDown = newArmsGoingDown;
                        EyesLooking  = newEyesLooking;
                        EyesBlinking = newEyesBlinking;
                        EyesOpening  = newEyesOpening;
                        DisplayedMoodSync = newDisplayedMoodSync;
                        Projectile.netUpdate = true;
                    }
                    int frameSpeed = 30; //reduced by half due to framecounter speedup
                    Projectile.frameCounter += 2;
                    if (Projectile.frameCounter >= frameSpeed)
                    {
                        Projectile.frameCounter = 0;
                        if (Projectile.frame >= Main.projFrames[ModContent.ProjectileType<Saria>()]) //error here! you had the wrong projectile id, so the animation did not use the right frames
                        {
                            Projectile.frame = 0;
                        }

                        if (Projectile.ai[0] == 0 || Projectile.ai[0] == 3 || Projectile.ai[0] == 4) //only run these animations if not attacking! no longer overrides
                        {
                            if ((Projectile.velocity.Y) > -1f && (Projectile.velocity.Y) < 1f && Math.Abs(Projectile.velocity.X) <= .25) //Idle animation, notice how I have (
                                                                                                                                         //
                                                                                                                                         //.Y greater than -3f and less than 4f. this DID conflict with the rising and Falling animations but this is how i fixed it.
                            { ////however you set up the attack animation, make sure that none of these other animations override it.
                              //that's easy legit just
                                Projectile.frame++;
                                if (IdleAnimator.IsActive && Projectile.frame > SariaIdleAnimator.IdleFrameMax && !IsPlayerAsleep)
                                {
                                    Projectile.frame = 0;
                                }
                                if (Projectile.frameCounter <= 36)
                                {
                                    Projectile.frameCounter = 0;
                                }
                                if (Sleep && MoveTimer > 250)
                                {
                                    if (Projectile.frame == 20)
                                    {
                                        Projectile.frame = 22;
                                        PlaySyncedSariaSound(SariaSoundId.Hover);
                                    }
                                    if (Projectile.frame >= 26 && MoveTimer >= 550)
                                    {
                                        Projectile.frame = 22;
                                        PlaySyncedSariaSound(SariaSoundId.Hover);
                                }
                                }
                                ///Charging animation
                                if (NotActive && ChannelState > 0)
                                {
                                    if (Projectile.frame >= 36 || Projectile.frame < 4)
                                    {
                                        if (IsCharging <= 0)
                                        {
                                            Projectile.frame = 4;
                                        }
                                        else
                                        {
                                            Projectile.frame = 8;
                                        }
                                    }
                                    if (Projectile.frame >= 4 && Projectile.frame < 36 && IsCharging <= 0)
                                    {
                                        Projectile.frame = 4;
                                    }
                                    if (Projectile.frame >= 12 && Projectile.frame < 36)
                                    {
                                        Projectile.frame = 8;
                                    }
                                }
                                ////end of charging animation
                                // Timer doubling at frame 22 during sneeze animation
                                if (Projectile.frame == 22 && Sneezing)
                                {
                                    IdleAnimator.OnSneezeDust();
                                }
                                if (Projectile.frame == 26 && (player.ownedProjectileCounts[ModContent.ProjectileType<Notice>()] <= 0f) && Sleep)
                                {
                                    if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<Notice>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, Projectile.whoAmI);
                                }
                                if (Projectile.frame == 23 && !Sleep)
                                {
                                    PlaySyncedSariaSound(SariaSoundId.Step2);
                                }
                                if (Projectile.frame >= 36 && Eating % 5 == 0)
                                {
                                    Projectile.frame = 0;
                                    Eating = 0;
                                    if (Sleep)
                                    {
                                        Sleep = false;
                                    }
                                    PlaySyncedSariaSound(SariaSoundId.Step1);
                                }
                                // Sneeze animation completion — frame 35 displays for one tick, wraps at 36
                                if (Sneezing && Projectile.frame > 36)
                                {
                                    Projectile.frame = 0;
                                    Sneezing = false;
                                    BloodSneeze = false;
                                }
                                ////this is the random heal timer for when player is standng still. this one will need to be reworked to be on a seperate timer during idle animation.
                                if (Projectile.frame == 18 && player.statLife < ((player.statLifeMax2) - (player.statLifeMax2 / 4)) && !player.HasBuff(ModContent.BuffType<Healpulse2Buff>()))
                                                {
                                                    SetMoodFor(MoodState.Sad, 420, priority: 1);
                                                    player.statLife += 500;
                                                    if (!player.HasBuff(ModContent.BuffType<Healpulse2Buff>()))
                                                    {
                                                        for (int j = 0; j < 1; j++) //set to 2
                                                        {
                                                                    if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<Heal>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, Projectile.whoAmI);
                                                                }
                                            }
                                            player.AddBuff(ModContent.BuffType<Healpulse2Buff>(), 3000);
                                }
                            }
                            if ((Projectile.velocity.Y) < 4f && Math.Abs(Projectile.velocity.X) > 0.25f && Math.Abs(Projectile.velocity.X) < 4f) //walking animation and such
                            {
                                Projectile.frame++;
                                Projectile.frameCounter += 3;
                                if (Projectile.frame <= 40)
                                {
                                    Projectile.frameCounter = 0;
                                }
                                if (Projectile.frame >= 40)
                                {
                                    Projectile.frame = 36;
                                }
                                if (Projectile.frame < 36)
                                {
                                    Projectile.frame = 36;
                                }
                            }
                            if ((Projectile.velocity.Y) < 4f && Math.Abs(Projectile.velocity.X) >= 4f)//running or (floating) animation
                            {
                                Projectile.frame++;
                                if (Projectile.frameCounter < 43)
                                {
                                    Projectile.frameCounter = 0;
                                }
                                if (Projectile.frame >= 43)
                                {
                                    Projectile.frame = 40;
                                    PlaySyncedSariaSound(SariaSoundId.Hover);
                                }
                                if (Projectile.frame < 40)
                                {
                                    Projectile.frame = 40;
                                }
                            }
                            if ((Projectile.velocity.Y) < -1f) //rising animation
                            {
                                Projectile.frame++;
                                if (Projectile.frameCounter < 43)
                                {
                                    Projectile.frameCounter = 0;
                                }
                                if (Projectile.frame >= 43)
                                {
                                    Projectile.frame = 40;
                                }
                                if (Projectile.frame < 40)
                                {
                                    Projectile.frame = 40;
                                    PlaySyncedSariaSound(SariaSoundId.Fly);
                                }
                            }
                            if ((Projectile.velocity.Y > 4f && Math.Abs(Projectile.velocity.X) > 0.25f) || (Projectile.velocity.Y > 1f && Math.Abs(Projectile.velocity.X) < 0.25f)) //falling while nearly still
                            {
                                Projectile.frame++;
                                if (Projectile.frameCounter < 99)
                                {
                                    Projectile.frameCounter = 0;
                                }
                                if (Projectile.frame >= 99)
                                {
                                    Projectile.frame = 97;
                                }
                                if (Projectile.frame < 97)
                                {
                                    Projectile.frame = 97;
                                }
                            }
                        }
                        Projectile.SariaAttacks((int)Transform, (int)CantAttackTimer, (int)ChannelAttack, (bool)foundTarget, (Vector2)targetCenter);
                    }
                    // --- Sneeze Timer (ticks constantly, not just during idle) ---
                    // Biome rates with indoor/outdoor distinction
                    if (player.behindBackWall && player.HasBuff(BuffID.Campfire))
                    {
                        IdleAnimator.SneezeBiomeRate = 0.05f;
                    }
                    else
                    {
                        bool isIndoors = player.behindBackWall;
                        if (player.ZoneSnow)
                            IdleAnimator.SneezeBiomeRate = isIndoors ? 1.3f : 3.0f;
                        else if (player.ZoneJungle)
                            IdleAnimator.SneezeBiomeRate = isIndoors ? 1.0f : 3.0f;
                        else if (player.ZoneForest)
                            IdleAnimator.SneezeBiomeRate = isIndoors ? 1.0f : 2.0f;
                        else if (player.ZoneSkyHeight)
                            IdleAnimator.SneezeBiomeRate = 1.5f;
                        else if (player.ZoneDesert && !Main.dayTime)
                            IdleAnimator.SneezeBiomeRate = 1.5f;
                        else if (player.ZoneHallow)
                            IdleAnimator.SneezeBiomeRate = 0.2f;
                        else
                            IdleAnimator.SneezeBiomeRate = 1.0f;

                        // Rain bonus (outdoor only, not in Zora/water form)
                        if (player.ZoneRain && !isIndoors && Transform != 1)
                            IdleAnimator.SneezeBiomeRate = Math.Max(IdleAnimator.SneezeBiomeRate, 2.0f);

                        // Biome weakness bonus — StatLower means she's struggling,
                        // stacks +3 on top of whatever the biome already gives
                        if (player.HasBuff(ModContent.BuffType<StatLower>()))
                            IdleAnimator.SneezeBiomeRate += 3.0f;
                    }

                    // Cursed — flat addition like StatLower, stacks aggressively
                    if (Cursed)
                        IdleAnimator.SneezeBiomeRate += 4.0f;

                    // Standing still long enough to sleep — sneeze builds faster
                    // (outside the campfire if/else so it stacks even indoors)
                    if (MoveTimer >= 5000)
                        IdleAnimator.SneezeBiomeRate += HoldingHealBall ? 1.0f : 1.5f;

                    // Boss alive → suppress sneezing entirely (timer stays at max)
                    bool bossAlive = false;
                    for (int b = 0; b < Main.maxNPCs; b++)
                    {
                        if (Main.npc[b].active && Main.npc[b].boss) { bossAlive = true; break; }
                    }

                    if (bossAlive)
                        IdleAnimator.ResetSneezeTimer();
                    else if (!Sneezing)
                        IdleAnimator.TickSneezeTimer();

                    // --- Idle Animator (runs AFTER frame advance so Update sees the same
                    //     Projectile.frame that Draw will see — eliminates 1-tick desync
                    //     that caused flicker/jitter on state transitions) ---
                    bool isIdleForAnimator =
                        (Projectile.ai[0] == 0 || Projectile.ai[0] == 3 || Projectile.ai[0] == 4)
                        && Projectile.velocity.Y > -1f && Projectile.velocity.Y < 1f
                        && Math.Abs(Projectile.velocity.X) <= 0.25f
                        && !Sleep && !IsPlayerAsleep && Eating <= 0 && ChannelState <= 0;

                    if (isIdleForAnimator)
                    {
                        if (!IdleAnimator.IsActive)
                        {
                            // Failsafe: if idle conditions are met but frame is stuck
                            // between idle range and walking range (4-35), reset to 0.
                            // Skip if Sneezing — the sneeze animation uses frames 12-35.
                            if (Projectile.frame > SariaIdleAnimator.IdleFrameMax && Projectile.frame < 36 && !Sneezing)
                            {
                                Projectile.frame = 0;
                            }

                            // DON'T activate until frame naturally enters idle range (0-3).
                            if (Projectile.frame <= SariaIdleAnimator.IdleFrameMax)
                            {
                                IdleAnimator.IsActive = true;
                                Projectile.frameCounter = 0; // clean timing on entry
                                Sneezing = false; // Clear sneeze flag when returning to normal idle
                                BloodSneeze = false;
                            }
                        }

                        // Only run idle logic once actually active
                        if (IdleAnimator.IsActive)
                        {
                            // Safety clamp — should rarely fire now that activation is delayed
                            if (Projectile.frame > SariaIdleAnimator.IdleFrameMax)
                            {
                                Projectile.frame = 0;
                            }

                            IdleAnimator.Update(Projectile, (int)Transform);

                            // Sneeze trigger — overlays aligned + warmup complete, start sneeze animation
                            if (IdleAnimator.IsSneezeReady)
                            {
                                Projectile.frame = 12;
                                IdleAnimator.OnSneezeStart();
                                IdleAnimator.Deactivate();
                                Sneezing = true;
                                BloodSneeze = player.HasBuff(ModContent.BuffType<StatLower>()) || Cursed;
                            }
                        }
                    }
                    else if (IdleAnimator.IsActive)
                    {
                        // Normal idle interrupted (movement, etc.) — timer keeps ticking naturally
                        IdleAnimator.Interrupt();
                    }
                    // Note: Sneezing is protected by CanMove=0, so isIdleForAnimator stays true
                    // and we never reach this branch during frames 12-35.
                    // --- End Idle Animator ---

                    // --- Forced Sneeze (biome weakness / StatLower debuff / Cursed) ---
                    // When weakened by biome disadvantage OR cursed, the sneeze fires as soon as
                    // the timer reaches 0 regardless of animation state (walking, flying, etc.).
                    // Only charging and active attack animations are excluded.
                    // CanMove is forced to 0 so she stops even if enemies are nearby.
                    if ((player.HasBuff(ModContent.BuffType<StatLower>()) || Cursed) && IdleAnimator.IsSneezeQueued
                        && !Sneezing && !Sleep && Eating <= 0 && ChannelState <= 0
                        && !(Projectile.frame >= 44 && Projectile.frame <= 55)
                        && Projectile.ai[0] != 1 && Projectile.ai[0] != 2)
                    {
                        Projectile.frame = 12;
                        Projectile.frameCounter = 0;
                        Sneezing = true;
                        BloodSneeze = true;
                        CanMove = 0;
                        IdleAnimator.OnSneezeStart();
                        if (IdleAnimator.IsActive)
                            IdleAnimator.Deactivate();
                    }
                    // --- End Forced Sneeze ---
                }
            }
            // Tick teleport phase every AI tick so it advances even when Saria is off-screen.
            if (Main.netMode != NetmodeID.Server)
                TickTeleportPhase();
        }
    }
}
