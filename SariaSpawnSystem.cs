using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using SariaMod.Diagnostics;

namespace SariaMod
{
    /// <summary>
    /// Redirects natural NPC spawns to Saria's location while LinkCable mode is active.
    /// For the duration of the synchronous <see cref="NPC.SpawnNPC"/> call only, the
    /// owning player's spawn anchor (world position + town-NPC suppression count) is moved
    /// to Saria's center, then restored immediately in a finally block before any
    /// rendering, AI, lighting, or networking runs.
    ///
    /// Diagnostics proved the earlier "fake decoy player slot" approach never works:
    /// vanilla builds its spawn box around REAL active player slots, so a decoy slot is
    /// never chosen as a spawn anchor (newSpawns stayed 0 every tick). Relocating the real
    /// owner is the only reliable anchor — and because the player's real position no longer
    /// hosts a spawn anchor during the call, it also stops enemies spawning at the player
    /// while in link mode.
    /// </summary>
    public class SariaSpawnSystem : ModSystem
    {
        /// <summary>
        /// Identifies which anchor the in-progress <see cref="NPC.SpawnNPC"/> pass is
        /// running for, so <c>FairyGlobalNPC.EditSpawnRate</c> can apply the matching half
        /// of the world spawn cap. <see cref="None"/> means a normal (un-split) call.
        /// </summary>
        public enum SpawnPass { None, Player, Saria }

        /// <summary>Set by the hook around each <c>orig()</c> pass; read in EditSpawnRate.</summary>
        public static SpawnPass CurrentPass { get; private set; } = SpawnPass.None;

        /// <summary>
        /// Player index of the LinkCable owner the active split is running for, or -1
        /// outside a split. Vanilla's SpawnNPC loops over ALL active players — in
        /// multiplayer EditSpawnRate therefore also fires for players who are NOT the
        /// split owner, and those players must get plain vanilla behavior (pass 1) or
        /// full suppression (pass 2) instead of the owner's regional gate.
        /// </summary>
        public static int CurrentOwnerIndex { get; private set; } = -1;

        /// <summary>
        /// The active Saria mod-projectile for the split in progress (null outside it).
        /// Lets EditSpawnRate read her candle/environment flags (CalmMind candle, Reaj
        /// candle) so HER surroundings drive her pass's spawn modifiers.
        /// </summary>
        public static Items.Strange.Saria CurrentSaria { get; private set; }

        /// <summary>
        /// True only for the throttled subset of split calls that should emit spawn-rate
        /// diagnostics. EditSpawnRate checks this so the per-pass rate logs line up with
        /// this hook's own before/after spawn logs instead of spamming every tick.
        /// </summary>
        public static bool LogRatesThisCall { get; private set; }

        /// <summary>World-space center of the player anchor for the active split (Player pass origin).</summary>
        public static Vector2 PlayerAnchor { get; private set; }

        /// <summary>World-space center of the Saria anchor for the active split (Saria pass origin).</summary>
        public static Vector2 SariaAnchor { get; private set; }

        /// <summary>
        /// The effective world spawn cap most recently seen by <c>FairyGlobalNPC.EditSpawnRate</c>
        /// (the engine-provided <c>maxSpawns</c> after biome/event adjustments, captured before
        /// the per-region split rewrites it). Shared so the hook log and debug trackers report
        /// the real cap without depending on a vanilla constant.
        /// </summary>
        public static int LastWorldCap { get; set; } = 5;

        /// <summary>
        /// The actual TARGET cap most recently assigned to the PLAYER region: their own
        /// engine-computed cap, clamped to a CEILING of <c>Main.maxNPCs −
        /// <see cref="SariaRegionMaxSpawns"/></c> (clamp only — never raised). Outside the
        /// split both this and <see cref="LastSariaCap"/> mirror the plain engine cap. Set
        /// by <c>FairyGlobalNPC.ApplyRegionGate</c>.
        /// </summary>
        public static int LastPlayerCap { get; set; } = 5;

        /// <summary>
        /// The actual TARGET cap most recently assigned to the SARIA region: her
        /// location's engine/candle-computed cap, clamped to a CEILING of
        /// <see cref="SariaRegionMaxSpawns"/> (clamp only — a quiet forest still gives
        /// her that forest's natural ~5-6). Set by <c>FairyGlobalNPC.ApplyRegionGate</c>.
        /// </summary>
        public static int LastSariaCap { get; set; } = 5;

        /// <summary>
        /// Hard CEILING for Saria's region: her computed cap may never exceed this many
        /// NPC slots (candles included), and the player's cap is ceilinged at
        /// <c>Main.maxNPCs − 50</c> so this reservation always stays spawnable. This is a
        /// clamp, never a target — see <c>FairyGlobalNPC.ApplyRegionGate</c>.
        /// </summary>
        public const int SariaRegionMaxSpawns = 50;

        // ── Deep spawn-cap diagnostics ───────────────────────────────────────────
        // Raw numbers behind the last ApplyRegionGate computation for each pass, so the
        // debug panel and log can show exactly what vanilla's global check was fed and
        // why, instead of just the final pass/fail outcome.
        /// <summary>World-wide non-town NPC slot count (<see cref="CountAllSlots"/>) at the moment the last region gate was computed.</summary>
        public static float LastGlobalSlotCount { get; set; }
        /// <summary>NPC slots attributed to the PLAYER region (nearest-anchor) when its gate was last computed.</summary>
        public static float LastPlayerRegionSlots { get; set; }
        /// <summary>NPC slots attributed to the SARIA region (nearest-anchor) when its gate was last computed.</summary>
        public static float LastSariaRegionSlots { get; set; }
        /// <summary>The engine/candle-computed cap entering the gate for the PLAYER pass, BEFORE the ceiling clamp.</summary>
        public static int LastPlayerComputedCap { get; set; }
        /// <summary>The engine/candle-computed cap entering the gate for the SARIA pass, BEFORE the 50 ceiling clamp.</summary>
        public static int LastSariaComputedCap { get; set; }
        /// <summary>The actual maxSpawns value fed to vanilla for the PLAYER pass (the global-to-regional trick value, not the meaningful target).</summary>
        public static int LastPlayerFedMaxSpawns { get; set; }
        /// <summary>The actual maxSpawns value fed to vanilla for the SARIA pass (the global-to-regional trick value, not the meaningful target).</summary>
        public static int LastSariaFedMaxSpawns { get; set; }

        /// <summary>
        /// <see cref="Main.GameUpdateCount"/> when a <c>SyncSpawnDebug</c> packet last
        /// arrived from the server. The split-spawn accounting is computed where
        /// <c>NPC.SpawnNPC</c> runs — the SERVER in multiplayer — so on clients every
        /// cap/fed/slot static above is stale unless refreshed by that packet. The debug
        /// panel checks <see cref="ServerDebugFresh"/> to prefer server-pushed values
        /// over locally recomputed ones.
        /// </summary>
        public static uint LastServerDebugSyncTime { get; set; }

        /// <summary>
        /// True while server-pushed spawn accounting is recent enough to trust (~3s).
        /// The server sends on the same ~1s cadence as the split log line, so a fresh
        /// window means LinkCable split is actively running server-side.
        /// </summary>
        public static bool ServerDebugFresh =>
            Main.netMode == NetmodeID.MultiplayerClient &&
            LastServerDebugSyncTime != 0 &&
            Main.GameUpdateCount - LastServerDebugSyncTime < 180;

        /// <summary>
        /// Straight-line world distance (pixels) between the owner's REAL position and
        /// Saria's center, captured whenever a LinkCable anchor is resolved. Vanilla town-NPC
        /// suppression and the region boxes both depend on how far apart these two points
        /// actually are, so this is logged to check whether the two regions overlap enough
        /// for one town to suppress spawns in both at once.
        /// </summary>
        public static float LastOwnerToSariaDistance { get; set; }

        /// <summary>
        /// The vanilla town-NPC proximity count (<see cref="Player.townNPCs"/>-equivalent)
        /// seen by the PLAYER pass — i.e. the owner's real <c>townNPCs</c> field at the moment
        /// EditSpawnRate ran for that pass. Vanilla suppresses natural spawns heavily when this
        /// is high, independent of the maxSpawns/spawnRate cap math.
        /// </summary>
        public static float LastPlayerTownNPCs { get; set; }

        /// <summary>
        /// The vanilla town-NPC proximity count computed for the SARIA pass via
        /// <see cref="CountTownNPCsNear"/> around Saria's center. Compared against
        /// <see cref="LastPlayerTownNPCs"/> to see whether both passes are being suppressed by
        /// the SAME nearby town (e.g. Saria following the player close enough that her region
        /// still counts the player's town NPCs as "nearby").
        /// </summary>
        public static float LastSariaTownNPCs { get; set; }

        // Throttle diagnostic logging so we don't spam the file every tick.
        private static int _logCooldown;

        // Separate throttle for the "no LinkCable anchor found" gate log, which fires on a
        // totally different path than the split-pass logging above (this is the case where
        // the split NEVER engages, so _logCooldown/_npcActiveBefore are never touched).
        private static int _gateLogCooldown;

        // Throttle for split exception logging — a repeating fault would otherwise write
        // a stack trace every tick (60/s) and drown the log.
        private static int _splitErrorLogCooldown;

        // Throttle for streaming tile sections around Saria to the owner's client
        // (server-side). CheckSection no-ops for already-sent sections, but it still
        // walks the section grid — once a second is responsive and cheap.
        private static int _sectionStreamCooldown;

        // Tracks which NPC slots were active before orig() runs, so we can detect *new*
        // spawns. The net total (delta) hides spawns that coincide with despawns in the
        // same call, so we diff slot-by-slot instead.
        private static bool[] _npcActiveBefore;

        // ── Per-pass spawn attribution accumulators ─────────────────────────────
        // Every split call snapshots + diffs around EACH pass's orig() separately, so a
        // spawn is attributed to the pass that actually created it (no more post-hoc
        // distance guessing, which mislabelled spawns whenever the anchors were close).
        // Counts accumulate across every split call between throttled log lines, so the
        // log reports COMPLETE totals for the window instead of a 1-tick-in-60 sample.
        private static int _playerPassSpawnsAccum;
        private static int _sariaPassSpawnsAccum;

        // New spawns REMOVED by the cross-view veto (CullNewSpawnsInsideView) in the same
        // window — logged next to the spawn totals so "region is quiet" and "region is
        // spawning fine but everything is being vetoed" are distinguishable in the log.
        private static int _playerPassCulledAccum;
        private static int _sariaPassCulledAccum;

        // ── Region box (shared with FairyGlobalNPC.CheckActive keep-alive box) ──────
        // Half-extents of the per-anchor region used for BOTH the no-despawn box and the
        // entity-count that gates each half of the spawn cap. Kept here as the single
        // source of truth so the tracker readout and the cap logic never diverge.
        //
        // Sized from NPC.sWidth/sHeight (vanilla's fixed 1920x1080 "assumed screen" that
        // ALL of its spawn-range math uses) rather than Main.screenWidth/Height: the
        // resolution-dependent values are 0/meaningless on a dedicated server — where
        // SpawnNPC actually runs in multiplayer — which would have collapsed the box and
        // the keep-alive region to nothing. Fixed constants also make the region size
        // identical for every machine regardless of window size or zoom.
        public static float BoxHalfWidth  => NPC.sWidth  * 0.9f;
        public static float BoxHalfHeight => NPC.sHeight * 0.9f;

        // ── Merged-region budget threshold ─────────────────────────────────────
        // When the owner and Saria are closer than this (px), their region boxes
        // overlap by more than half and describe essentially ONE place. Running two
        // independent budgets there let the shared area legally fill to BOTH targets
        // combined (~2× the cap) before either gate closed — the "enemies exceed the
        // cap when Saria is near the player" report. Under this distance the gate
        // treats both regions as one merged budget: population = playerSlots +
        // sariaSlots, target = max(playerTarget, sariaTarget) — spawning stays fully
        // alive (both passes still run and can spawn), it just stops double-counting
        // the budget. At or beyond this distance the regions are separate places and
        // the independent budgets stand unchanged.
        public static float RegionMergeDistance => BoxHalfWidth;

        /// <summary>
        /// True when the last region gate ran in merged-budget mode (owner and Saria
        /// within <see cref="RegionMergeDistance"/>). Mirrored to the owner's client via
        /// SyncSpawnDebug so the debug panel can present the shared budget correctly.
        /// </summary>
        public static bool LastRegionsMerged { get; set; }

        // ── Cross-view spawn veto extents ──────────────────────────────────────
        // Mirror of the safe rect vanilla builds around every ACTIVE player when it picks
        // a spawn tile: safeRange = (int)(assumedScreenTiles * 0.52) tiles per axis,
        // centred on the player. Vanilla applies that rect to real players only — Saria
        // isn't one, and during her pass the owner is relocated onto her, so the owner's
        // REAL view loses its protection for that pass. CullNewSpawnsInsideView uses
        // these to give the unprotected party the exact same no-spawn margin vanilla
        // gives a real player — and NOT MORE. An earlier version added half a screen on
        // top (0.5*screen + safeRange ≈ 1952×1084 px), which is LARGER than vanilla's
        // whole spawn ring (spawnRangeX/Y ≈ 992–1344 px out horizontally, 544–736 px
        // vertically): whenever owner and Saria were within ~2 screens of each other,
        // EVERY spawn from one pass landed inside the other's oversized veto box and was
        // culled — natural spawning around Saria looked completely stalled unless both
        // players moved far away (the multiplayer report). safeRange alone (992×544 px)
        // sits INSIDE the spawn ring, so nearby spawns survive exactly like vanilla's.
        public static float ViewVetoHalfWidth  => (int)(NPC.sWidth  / 16 * 0.52) * 16f;
        public static float ViewVetoHalfHeight => (int)(NPC.sHeight / 16 * 0.52) * 16f;

        public override void Load()
        {
            On.Terraria.NPC.SpawnNPC += Hook_SpawnNPC;
            SariaDebug.Log("Spawn", "SariaSpawnSystem.Load() — SpawnNPC hook attached.", Color.Lime);
        }

        public override void Unload()
        {
            On.Terraria.NPC.SpawnNPC -= Hook_SpawnNPC;
        }

        /// <summary>
        /// Final safety net for the LinkCable aggro redirect: runs after every NPC has
        /// updated each tick. If an aggro relocation somehow survived the whole NPC phase
        /// (PostAI skipped by an exception, an NPC deactivating mid-AI, or a Transform
        /// instance swap), restore the owner's real position before rendering/netcode.
        /// </summary>
        public override void PostUpdateNPCs()
        {
            FairyGlobalNPC.RestoreAggroRelocation();
            FairyGlobalNPC.LogAggroActivity();
            StreamSectionsAroundSaria();
        }

        /// <summary>
        /// Server-side world streaming for LinkCable mode. The server only sends tile
        /// sections a client's PLAYER approaches, so when Saria roams far from her owner
        /// the owner's client never receives the terrain around her — the world visibly
        /// ends in an unloaded slice at the last section their body touched, until the
        /// owner physically walks closer. While LinkCable is engaged, push the sections
        /// around Saria's position to her owner's client exactly the way vanilla does
        /// for teleports/pylons (<see cref="RemoteClient.CheckSection"/>): it diffs
        /// against the client's section map and transmits only what's missing, so the
        /// once-a-second cadence costs nothing once the area has been sent. Saria tops
        /// out around ~2.5 px/tick, far slower than a 3200px section per second, so the
        /// throttle can never fall behind her.
        /// </summary>
        private static void StreamSectionsAroundSaria()
        {
            if (Main.netMode != NetmodeID.Server)
                return;

            if (_sectionStreamCooldown > 0)
            {
                _sectionStreamCooldown--;
                return;
            }
            _sectionStreamCooldown = 60; // ~1s

            int sariaType = ModContent.ProjectileType<Items.Strange.Saria>();
            for (int p = 0; p < Main.maxProjectiles; p++)
            {
                Projectile proj = Main.projectile[p];
                if (!proj.active || proj.type != sariaType)
                    continue;

                Player owner = Main.player[proj.owner];
                if (!owner.active || owner.dead)
                    continue;
                if (!owner.TryGetModPlayer(out FairyPlayer fp) || !fp.LinkCable)
                    continue;

                RemoteClient client = Netplay.Clients[proj.owner];
                if (client == null || !client.IsActive)
                    continue;

                RemoteClient.CheckSection(proj.owner, proj.Center);
            }
        }

        private void Hook_SpawnNPC(On.Terraria.NPC.orig_SpawnNPC orig)
        {
            // Vanilla SpawnNPC is a no-op on clients; don't relocate anything there.
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                orig();
                return;
            }

            // Find the first owner whose Saria is in LinkCable mode. Only then do we split
            // the spawn cap into two regions; otherwise run the vanilla call COMPLETELY
            // untouched (no anchors, no gate, no Saria influence of any kind), but emit a
            // throttled state line so the non-LinkCable path is observable too — the
            // "enemies ignore the cap with LinkCable off" report needs real numbers.
            if (!TryFindLinkCableAnchor(out int ownerIndex, out Vector2 sariaCenter))
            {
                CurrentPass = SpawnPass.None;
                bool logVanilla = false;
                if (_gateLogCooldown > 0)
                    _gateLogCooldown--;
                if (_gateLogCooldown <= 0)
                {
                    logVanilla = true;
                    _gateLogCooldown = 300; // ~5s at 60 ticks/sec — this path can run every tick
                    SnapshotActiveNPCs();
                }
                orig();
                if (logVanilla)
                    LogVanillaState();
                return;
            }

            Player owner = Main.player[ownerIndex];

            // Publish both anchors so EditSpawnRate / trackers can measure each region.
            PlayerAnchor = owner.Center;
            SariaAnchor  = sariaCenter;
            CurrentSaria = FindSariaModProjectile(ownerIndex);
            CurrentOwnerIndex = ownerIndex;

            // Real (pre-relocation) owner<->Saria distance. Vanilla's town-NPC suppression
            // and the region boxes both hinge on how far apart these two points are — if
            // Saria is following close behind the player, the SAME nearby town can suppress
            // spawns for both regions at once, which would explain low/zero spawns on both
            // sides even while ApplyRegionCap reports "willPass=YES".
            LastOwnerToSariaDistance = Vector2.Distance(owner.Center, sariaCenter);

            // Save the owner's real spawn-anchor state (restored after the Saria pass).
            Vector2 savedPosition = owner.position;
            Vector2 savedVelocity = owner.velocity;
            float savedTownNPCs = owner.townNPCs;

            // The owner's REAL view center — needed by the Saria pass's cross-view veto,
            // because by the time that pass runs the owner has been relocated onto Saria
            // and owner.Center no longer points at the player's actual screen.
            Vector2 ownerRealCenter = savedPosition + new Vector2(owner.width * 0.5f, owner.height * 0.5f);

            bool log = false;
            if (_logCooldown > 0)
                _logCooldown--;
            if (_logCooldown <= 0)
            {
                log = true;
                _logCooldown = 60;
            }
            LogRatesThisCall = log;

            // Refresh Saria's candle flags LIVE before her pass reads them in
            // EditSpawnRate. The movement-gated snapshot scan could be stale and used a
            // different radius than a real player's candle range, so detection and
            // application disagreed with distance.
            CurrentSaria?.RefreshCandleEnvironment();

            // NOTE: both passes are always attempted now. The real gate against
            // "how many are already near this region" lives in
            // FairyGlobalNPC.ApplyRegionGate, which fixes the actual bug: vanilla's own
            // maxSpawns check is GLOBAL (one world-wide NPC count), not per-region, so a
            // boolean pre-check here could never make the two sides truly independent —
            // see ApplyRegionGate for the full explanation and the fix.

            // While camera-on-Saria perception is active, the owner's zone flags (and the
            // global SceneMetrics) describe SARIA's location for the whole tick. Spawning
            // must not see that: it made Saria's biome (e.g. an active sandstorm) drive
            // the PLAYER pass, so her enemies spawned at the player, while her own pass
            // ran off the stale movement-gated snapshot (no ZoneSandstorm) and got almost
            // nothing. Fix: bypass perception and recompute LIVE biome state per pass,
            // then hand state back to perception afterwards in the finally block.
            SariaPerceptionSystem.BypassPerception = true;
            bool playerPassCompleted = false;
            try
            {
                // ── PASS 1: Player region ─────────────────────────────────────────
                // Owner at their REAL position with their REAL biome: rescan the global
                // SceneMetrics at the owner and recompute zone flags from scratch so no
                // Saria-perceived state leaks into the player's spawn pool or rates.
                RecomputeBiomesAt(owner, owner.Center);
                CurrentPass = SpawnPass.Player;
                LastPlayerTownNPCs = owner.townNPCs;
                SnapshotActiveNPCs();
                orig();
                // Cross-view veto: the player's pass must never pop an enemy inside
                // SARIA's view. Cull first (flips vetoed spawns inactive), then count —
                // so the spawn total reflects survivors only.
                _playerPassCulledAccum += CullNewSpawnsInsideView(sariaCenter);
                _playerPassSpawnsAccum += CountNewSpawnsSinceSnapshot();
                playerPassCompleted = true;

                // ── PASS 2: Saria region ──────────────────────────────────────────
                // Relocate the owner's spawn anchor onto Saria. Position drives the spawn
                // box; townNPCs is recomputed for Saria's surroundings so town suppression
                // follows Saria. ApplyRegionGate gates this pass to Saria's own region
                // population vs her clamped cap, independent of the player's population.
                CurrentPass = SpawnPass.Saria;
                owner.position = sariaCenter - new Vector2(owner.width * 0.5f, owner.height * 0.5f);
                owner.velocity = Vector2.Zero;
                owner.townNPCs = CountTownNPCsNear(sariaCenter);
                LastSariaTownNPCs = owner.townNPCs;

                // Recompute LIVE biome state at Saria's position (not the movement-gated
                // snapshot, which misses dynamic zones like ZoneSandstorm/ZoneRain
                // entirely). Vanilla and modded spawn pools, rates and caps all resolve
                // against her actual surroundings, including live events.
                RecomputeBiomesAt(owner, sariaCenter);

                SnapshotActiveNPCs();
                orig();
                // Cross-view veto: Saria's pass must never pop an enemy inside the
                // PLAYER's real view (vanilla is protecting Saria's position this pass,
                // not the player's). Cull first, then count survivors.
                _sariaPassCulledAccum += CullNewSpawnsInsideView(ownerRealCenter);
                _sariaPassSpawnsAccum += CountNewSpawnsSinceSnapshot();
            }
            catch (System.Exception ex)
            {
                // A fault mid-split previously VANISHED: the finally restored state, the
                // exception propagated into vanilla's update loop, and spawning looked
                // "stunted to zero" with an empty log — exactly the multiplayer symptom.
                // Capture it (throttled) so the log names the culprit, then swallow: the
                // fallback below keeps world spawning alive even if the split is broken.
                if (_splitErrorLogCooldown <= 0)
                {
                    _splitErrorLogCooldown = 300; // ~5s
                    SariaDebug.Log("Spawn",
                        $"SPLIT FAULT ({CurrentPass} pass): {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}",
                        Color.Red);
                }
            }
            finally
            {
                // Restore the real owner BEFORE anything else (render/AI/net) can observe it.
                owner.position = savedPosition;
                owner.velocity = savedVelocity;
                owner.townNPCs = savedTownNPCs;

                CurrentPass = SpawnPass.None;
                CurrentSaria = null;
                CurrentOwnerIndex = -1;
                LogRatesThisCall = false;

                // Hand biome state back to the perception system: with the bypass lifted,
                // this rescan + recompute go through the perception hooks again, so while
                // camera mode is active they re-centre on Saria (matching the rest of the
                // tick) and otherwise they restore plain real-position state. This keeps
                // draw/music consumers later in the frame consistent with perception.
                // Guarded because the finally must NEVER throw: a fault here escapes the
                // catch above (finally faults propagate), aborts NPC.SpawnNPC before the
                // vanilla fallback below can run, and — repeating every tick — froze ALL
                // natural spawning for every player whenever LinkCable was on in
                // multiplayer (the exact bug the SPLIT FAULT log line caught).
                SariaPerceptionSystem.BypassPerception = false;
                try
                {
                    RecomputeBiomesAt(owner, owner.Center);
                }
                catch { }
            }

            if (_splitErrorLogCooldown > 0)
                _splitErrorLogCooldown--;

            // Fault fallback: if the split died before the PLAYER pass finished, the world
            // got NO spawn attempt at all this call. Run one plain vanilla pass (owner
            // state already restored, CurrentPass=None so EditSpawnRate treats it as a
            // normal call) — a broken split degrades to vanilla behavior instead of
            // freezing all spawning.
            if (!playerPassCompleted)
            {
                orig();
            }

            if (log)
            {
                CountRegionSlots(out float playerSlots, out float sariaSlots, out float globalSlots);
                SariaDebug.Log("Spawn",
                    $"Split window done. spawnsThisWindow: player={_playerPassSpawnsAccum} (culled {_playerPassCulledAccum}) " +
                    $"saria={_sariaPassSpawnsAccum} (culled {_sariaPassCulledAccum}) | " +
                    $"regionSlots: player={playerSlots:F1}/{LastPlayerCap} saria={sariaSlots:F1}/{LastSariaCap} " +
                    $"global={globalSlots:F1} | ownerToSariaDist={LastOwnerToSariaDistance:F0} " +
                    $"playerTownNPCs={LastPlayerTownNPCs:F1} sariaTownNPCs={LastSariaTownNPCs:F1} netMode={Main.netMode}");
                _playerPassSpawnsAccum = 0;
                _sariaPassSpawnsAccum = 0;
                _playerPassCulledAccum = 0;
                _sariaPassCulledAccum = 0;

                // Multiplayer: push the authoritative split accounting to the owner's
                // client on the same ~1s cadence. All these statics only get written
                // where SpawnNPC runs (HERE, the server) — without this packet the
                // owner's debug panel recomputed everything locally from its own NPC
                // array and its own stale caps, which is exactly why its numbers never
                // matched what the server's gate was actually doing.
                if (Main.netMode == NetmodeID.Server)
                {
                    RemoteClient client = Netplay.Clients[ownerIndex];
                    if (client != null && client.IsActive)
                    {
                        ModPacket packet = SariaMod.Instance.GetPacket();
                        packet.Write((byte)SariaMod.SoundMessageType.SyncSpawnDebug);
                        packet.Write(LastPlayerCap);
                        packet.Write(LastSariaCap);
                        packet.Write(LastPlayerFedMaxSpawns);
                        packet.Write(LastSariaFedMaxSpawns);
                        packet.Write(playerSlots);
                        packet.Write(sariaSlots);
                        packet.Write(globalSlots);
                        packet.Write(LastRegionsMerged);
                        packet.Send(ownerIndex);
                    }
                }
            }
        }

        // One-time flag so the server-side BiomeLoader fault is reported exactly once
        // per session instead of once per pass per split tick.
        private static bool _biomeFaultLogged;

        /// <summary>
        /// Rescans the global SceneMetrics around <paramref name="center"/> and recomputes
        /// the owner's zone flags there — the per-pass "stand the anchor in its real
        /// biome" step. The <see cref="Player.UpdateBiomes"/> call is guarded because on a
        /// DEDICATED SERVER tModLoader's <c>BiomeLoader.UpdateBiomes</c> tail indexes
        /// mod-biome flags that remote Player instances don't have sized server-side and
        /// throws <see cref="System.ArgumentOutOfRangeException"/> — vanilla never calls
        /// UpdateBiomes on the server, so those flags only ever get sized client-side.
        /// Every VANILLA zone flag is already assigned before tML's tail call throws, and
        /// vanilla flags are what SpawnNPC's pool/rate logic reads, so swallowing the tail
        /// fault keeps the split fully functional server-side. Unguarded, this exception
        /// killed the whole call chain: pass 1 threw, the finally's own recompute threw
        /// AGAIN (a finally fault bypasses the catch), NPC.SpawnNPC aborted every tick,
        /// and ALL natural spawning froze for every player while LinkCable was on.
        /// </summary>
        private static void RecomputeBiomesAt(Player owner, Vector2 center)
        {
            Main.SceneMetrics.ScanAndExportToMain(new SceneMetricsScanSettings
            {
                BiomeScanCenterPositionInWorld = center,
                ScanOreFinderData = false,
            });
            try
            {
                owner.UpdateBiomes();
            }
            catch (System.Exception ex)
            {
                if (!_biomeFaultLogged)
                {
                    _biomeFaultLogged = true;
                    SariaDebug.LogSilent("Spawn",
                        $"UpdateBiomes fault suppressed (vanilla zone flags already applied — " +
                        $"tML mod-biome tail is unsized on the server): {ex.GetType().Name}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Counts NPC slots that became active since the last <see cref="SnapshotActiveNPCs"/>
        /// call, excluding town NPCs. Called immediately after each pass's <c>orig()</c> so
        /// every new spawn is attributed to the pass that actually created it.
        /// </summary>
        private static int CountNewSpawnsSinceSnapshot()
        {
            if (_npcActiveBefore == null)
                return 0;
            int n = 0;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !_npcActiveBefore[i] && !npc.townNPC)
                    n++;
            }
            return n;
        }

        /// <summary>
        /// Cross-view spawn veto: despawns any NPC that spawned during the pass that just
        /// ran (post-snapshot diff) whose center landed inside the protected view rect
        /// around <paramref name="protectedCenter"/> (<see cref="ViewVetoHalfWidth"/> ×
        /// <see cref="ViewVetoHalfHeight"/> — vanilla's own per-player safe-rect size).
        /// The spawn attempt is simply lost; the pass retries on its next tick and
        /// near-certainly rolls a different spot, which is exactly vanilla's own behavior
        /// when a candidate tile fails its player-proximity check. Runs before
        /// <see cref="CountNewSpawnsSinceSnapshot"/> so vetoed spawns never count toward
        /// the window totals, and the freed slot keeps every region/global count honest.
        /// Worm-style multi-part enemies spawn their segments in their first AI tick,
        /// which hasn't run yet — culling the head here means no orphaned segments.
        /// Returns how many spawns were vetoed.
        /// </summary>
        private static int CullNewSpawnsInsideView(Vector2 protectedCenter)
        {
            if (_npcActiveBefore == null)
                return 0;
            float halfW = ViewVetoHalfWidth;
            float halfH = ViewVetoHalfHeight;
            int culled = 0;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || _npcActiveBefore[i] || npc.townNPC)
                    continue;
                if (npc.Center.X < protectedCenter.X - halfW || npc.Center.X > protectedCenter.X + halfW ||
                    npc.Center.Y < protectedCenter.Y - halfH || npc.Center.Y > protectedCenter.Y + halfH)
                    continue;

                npc.active = false;
                if (Main.netMode == NetmodeID.Server)
                    NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, i);
                culled++;
            }
            return culled;
        }

        /// <summary>
        /// Finds the first active Saria projectile whose owner currently has LinkCable mode
        /// engaged. Only then do we want to hijack the spawn anchor.
        /// </summary>
        private static bool TryFindLinkCableAnchor(out int ownerIndex, out Vector2 sariaCenter)
        {
            ownerIndex = -1;
            sariaCenter = Vector2.Zero;

            int sariaType = ModContent.ProjectileType<Items.Strange.Saria>();
            for (int p = 0; p < Main.maxProjectiles; p++)
            {
                Projectile proj = Main.projectile[p];
                if (!proj.active || proj.type != sariaType)
                    continue;

                Player owner = Main.player[proj.owner];
                if (!owner.active || owner.dead)
                    continue;

                if (!owner.TryGetModPlayer(out FairyPlayer fp) || !fp.LinkCable)
                    continue;

                ownerIndex = proj.owner;
                sariaCenter = proj.Center;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Throttled state line for the vanilla (non-split) path in <see cref="Hook_SpawnNPC"/>.
        /// Reports WHY the split did not engage (LinkCable off, owner dead, no Saria at all)
        /// PLUS the actual vanilla numbers for that call: the engine cap EditSpawnRate last
        /// saw (<see cref="LastWorldCap"/>), the current global slot count vanilla checks
        /// against it, and how many NPCs this exact call spawned. This makes the
        /// "enemies spawn regardless of cap with LinkCable off" report directly checkable:
        /// if globalSlots >= cap and newSpawns > 0 on the same line, the cap is genuinely
        /// being bypassed; if globalSlots < cap, the spawning is legal and the cap number
        /// itself (e.g. moon-phase 50) is what needs tuning.
        /// </summary>
        private static void LogVanillaState()
        {
            int sariaType = ModContent.ProjectileType<Items.Strange.Saria>();
            int activeSariaCount = 0;
            string reason = "no active Saria projectile in world";

            for (int p = 0; p < Main.maxProjectiles; p++)
            {
                Projectile proj = Main.projectile[p];
                if (!proj.active || proj.type != sariaType)
                    continue;

                activeSariaCount++;
                Player owner = Main.player[proj.owner];
                if (!owner.active || owner.dead)
                {
                    reason = $"owner[{proj.owner}] active={owner.active} dead={owner.dead}";
                    continue;
                }

                if (!owner.TryGetModPlayer(out FairyPlayer fp) || !fp.LinkCable)
                {
                    reason = $"owner[{proj.owner}] LinkCable=False (HeldItem={owner.HeldItem?.Name ?? "none"})";
                    continue;
                }

                // Shouldn't happen (TryFindLinkCableAnchor would have matched this same
                // candidate), but guard anyway so this never contradicts the real gate.
                reason = $"owner[{proj.owner}] appeared eligible but TryFindLinkCableAnchor rejected it";
            }

            int newSpawns = CountNewSpawnsSinceSnapshot();
            float globalSlots = CountAllSlots();
            SariaDebug.LogSilent("SpawnGate",
                $"VANILLA path (split not engaged) — reason=\"{reason}\" activeSariaProjectiles={activeSariaCount} | " +
                $"worldCap={LastWorldCap} globalSlots={globalSlots:F1} " +
                $"capState={(globalSlots < LastWorldCap ? "UNDER" : "AT/OVER")} newSpawnsThisCall={newSpawns}");
        }

        /// <summary>
        /// Returns the <see cref="Items.Strange.Saria"/> mod-projectile instance for the given
        /// owner, or <c>null</c> if none is active. Used by the Saria spawn pass to read the
        /// biome snapshot that <c>UpdateSariaZones</c> populates once per movement cycle.
        /// </summary>
        private static Items.Strange.Saria FindSariaModProjectile(int ownerIndex)
        {
            int sariaType = ModContent.ProjectileType<Items.Strange.Saria>();
            for (int p = 0; p < Main.maxProjectiles; p++)
            {
                Projectile proj = Main.projectile[p];
                if (proj.active && proj.type == sariaType && proj.owner == ownerIndex)
                    return proj.ModProjectile as Items.Strange.Saria;
            }
            return null;
        }

        /// <summary>
        /// Mirrors the filter vanilla's <c>NPC.SpawnNPC</c> uses when accumulating its
        /// global <c>activeNPCs</c> slot count: active, not a town NPC, not the Skeleton
        /// Merchant (vanilla special-cases him as town-like despite <c>townNPC=false</c>),
        /// and not flagged <see cref="NPC.dontCountMe"/>. Keeping this predicate identical
        /// to vanilla's matters because the region gate feeds vanilla a value derived from
        /// OUR count — if the two counts disagree, the global-to-regional algebra drifts.
        /// </summary>
        private static bool CountsTowardSpawnCap(NPC npc)
        {
            return npc.active
                && !npc.townNPC
                && npc.type != NPCID.SkeletonMerchant
                && !npc.dontCountMe;
        }

        /// <summary>
        /// Counts every NPC that vanilla's spawn cap counts, weighted by
        /// <see cref="NPC.npcSlots"/>. This is the global tally vanilla's <c>SpawnNPC</c>
        /// check compares against <c>maxSpawns</c>.
        /// </summary>
        public static float CountAllSlots()
        {
            float count = 0f;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (CountsTowardSpawnCap(npc))
                    count += npc.npcSlots;
            }
            return count;
        }

        /// <summary>
        /// Box-scoped regional attribution with an exact global tally, all in one loop.
        ///
        /// Region membership: an NPC occupies a region's slots only if its center is
        /// INSIDE that anchor's box (<see cref="BoxHalfWidth"/> × <see cref="BoxHalfHeight"/>
        /// — the same box the keep-alive uses). Inside both boxes (overlap) → nearest
        /// anchor wins, so nothing is ever double-counted. Inside neither box → counts
        /// toward NO region, only the global tally.
        ///
        /// WHY not pure nearest-anchor: in multiplayer every enemy in the world got
        /// attributed to whichever anchor was "less far" — a second player fighting a
        /// horde 10,000px away silently filled BOTH regions to their caps, the gates fed
        /// vanilla maxSpawns=0, and owner+Saria spawning froze ("stunted to zero"). Region
        /// slots must mean "enemies actually AT this region".
        ///
        /// <paramref name="globalSlots"/> is the full world tally (same predicate vanilla
        /// uses), which the gate needs for its algebra: it feeds vanilla
        /// <c>globalSlots + headroom</c>, and vanilla's own global count (== this value,
        /// same predicate) &lt; fed ⟺ regionSlots &lt; target — out-of-region NPCs cancel
        /// out exactly, so a distant player's horde neither fills nor frees this region.
        /// </summary>
        public static void CountRegionSlots(out float playerSlots, out float sariaSlots, out float globalSlots)
        {
            Vector2 playerAnchor = PlayerAnchor;
            Vector2 sariaAnchor = SariaAnchor;
            float halfW = BoxHalfWidth;
            float halfH = BoxHalfHeight;
            playerSlots = 0f;
            sariaSlots = 0f;
            globalSlots = 0f;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!CountsTowardSpawnCap(npc))
                    continue;
                globalSlots += npc.npcSlots;

                bool inPlayerBox =
                    npc.Center.X >= playerAnchor.X - halfW && npc.Center.X <= playerAnchor.X + halfW &&
                    npc.Center.Y >= playerAnchor.Y - halfH && npc.Center.Y <= playerAnchor.Y + halfH;
                bool inSariaBox =
                    npc.Center.X >= sariaAnchor.X - halfW && npc.Center.X <= sariaAnchor.X + halfW &&
                    npc.Center.Y >= sariaAnchor.Y - halfH && npc.Center.Y <= sariaAnchor.Y + halfH;

                if (inPlayerBox && inSariaBox)
                {
                    // Overlap: nearest anchor wins so the NPC lands in exactly one region.
                    float dPlayer = Vector2.DistanceSquared(npc.Center, playerAnchor);
                    float dSaria = Vector2.DistanceSquared(npc.Center, sariaAnchor);
                    if (dSaria < dPlayer)
                        sariaSlots += npc.npcSlots;
                    else
                        playerSlots += npc.npcSlots;
                }
                else if (inPlayerBox)
                    playerSlots += npc.npcSlots;
                else if (inSariaBox)
                    sariaSlots += npc.npcSlots;
            }
        }

        /// <summary>
        /// Debug-panel region readout using the EXACT attribution rules the real gate
        /// uses (<see cref="CountRegionSlots"/>): same vanilla cap predicate
        /// (<see cref="CountsTowardSpawnCap"/> — the old panel counter skipped the
        /// <c>dontCountMe</c>/Skeleton Merchant exclusions), box membership per anchor,
        /// nearest-anchor tiebreak in the overlap so no NPC lands in both rows. The old
        /// per-box counter counted overlap NPCs TWICE (once per row), so the panel
        /// disagreed with the gate whenever the two boxes overlapped — the normal case
        /// with Saria near her owner. Also reports plain BODY counts alongside the
        /// slot-weighted values: vanilla's cap math runs on npcSlots (many enemies weigh
        /// 0.5/0.75), so "6 enemies on screen" can legitimately be "5.0 slots" — showing
        /// both makes the panel readable without knowing that.
        /// </summary>
        public static void CountRegionDisplay(Vector2 playerCenter, Vector2 sariaCenter,
            out float playerSlots, out int playerBodies, out float sariaSlots, out int sariaBodies)
        {
            float halfW = BoxHalfWidth;
            float halfH = BoxHalfHeight;
            playerSlots = 0f;
            sariaSlots = 0f;
            playerBodies = 0;
            sariaBodies = 0;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!CountsTowardSpawnCap(npc))
                    continue;

                bool inPlayerBox =
                    npc.Center.X >= playerCenter.X - halfW && npc.Center.X <= playerCenter.X + halfW &&
                    npc.Center.Y >= playerCenter.Y - halfH && npc.Center.Y <= playerCenter.Y + halfH;
                bool inSariaBox =
                    npc.Center.X >= sariaCenter.X - halfW && npc.Center.X <= sariaCenter.X + halfW &&
                    npc.Center.Y >= sariaCenter.Y - halfH && npc.Center.Y <= sariaCenter.Y + halfH;

                if (inPlayerBox && inSariaBox)
                {
                    // Overlap: nearest anchor wins — identical to CountRegionSlots.
                    float dPlayer = Vector2.DistanceSquared(npc.Center, playerCenter);
                    float dSaria = Vector2.DistanceSquared(npc.Center, sariaCenter);
                    if (dSaria < dPlayer) { sariaSlots += npc.npcSlots; sariaBodies++; }
                    else { playerSlots += npc.npcSlots; playerBodies++; }
                }
                else if (inPlayerBox) { playerSlots += npc.npcSlots; playerBodies++; }
                else if (inSariaBox) { sariaSlots += npc.npcSlots; sariaBodies++; }
            }
        }

        /// <summary>
        /// Records which NPC slots are active right before <c>orig()</c> so the post-call
        /// diff can identify slots that became active during the spawn pass.
        /// </summary>
        private static void SnapshotActiveNPCs()
        {
            if (_npcActiveBefore == null || _npcActiveBefore.Length != Main.maxNPCs)
                _npcActiveBefore = new bool[Main.maxNPCs];
            for (int i = 0; i < Main.maxNPCs; i++)
                _npcActiveBefore[i] = Main.npc[i].active;
        }

        /// <summary>
        /// Recreates vanilla's per-player town-NPC proximity count for an arbitrary world
        /// position. Vanilla stores this in <see cref="Player.townNPCs"/> during spawn
        /// bookkeeping and uses it to suppress natural spawns near settlements. We compute
        /// it for Saria's actual position so town suppression follows Saria during the
        /// spawn call, not the owner's real location.
        /// </summary>
        private static float CountTownNPCsNear(Vector2 center)
        {
            // Vanilla uses a ~62.5-tile (1000px) proximity for town spawn suppression.
            const float rangeSq = 1000f * 1000f;
            float count = 0f;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || !npc.townNPC)
                    continue;
                if (Vector2.DistanceSquared(npc.Center, center) <= rangeSq)
                    count += npc.npcSlots <= 0f ? 1f : npc.npcSlots;
            }
            return count;
        }
    }
}
