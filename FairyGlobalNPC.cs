using SariaMod.Items.Bands;
using SariaMod.Items.zPearls;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using SariaMod.Items.Emerald;
using Microsoft.Xna.Framework;
using SariaMod.Buffs;
using SariaMod.Dusts;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.UI.Elements;
using ReLogic.Content;
using Terraria.DataStructures;
using System;
using SariaMod.Items.Ruby;
using System.Collections.Generic;
using SariaMod.Items.Amber;
using SariaMod.Items.Amethyst;
using SariaMod.Items.Sapphire;
using SariaMod.Items.Topaz;
using SariaMod.Items.zTalking;
using Terraria.Localization;
using Terraria.Map;
using System.IO;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.Graphics;
using Terraria.ObjectData;
using Terraria.ModLoader.IO;
using SariaMod.Gores;
using SariaMod.Diagnostics;
namespace SariaMod
{
    public class FairyGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;
        private readonly int buffToRemove = ModContent.BuffType<EnemyFrozen>();
        private static bool[] hasBuffSynced = new bool[Main.maxNPCs];
        public bool SariaCurseD;
        public bool Burning2;
        public bool GhostBurning;
        public bool Stronger;
        public bool Frostburn2;

        // Ice Dome Animation Fields
        public int IceDomeTimer;
        public bool IceDomeActive;
        private bool wasFrozen;
        public int freezeInitiatorPlayer = -1;
        public bool visualsAuthorized = false;
        
        // Frozen Frostburn Timer - reapplies Frostburn2 every minute (3600 frames) while frozen
        private int frozenFrostburnTimer = 0;
        private const int FrostburnReapplyInterval = 3600; // 60 seconds at 60fps

        // ── LinkCable aggro redirect (target Saria when the player is too far) ──────
        // When LinkCable mode is active and an NPC's owner is farther than this many
        // pixels while that owner's Saria is closer, the owner is briefly relocated onto
        // Saria around vanilla AI so the enemy chases Saria instead. Restored in PostAI
        // before render. With SEVERAL LinkCable Sarias active the anchor is chosen
        // PER-NPC — the nearest eligible Saria wins — so each enemy chases the Saria
        // actually near it instead of every enemy snapping to whichever Saria happened
        // to occupy the lowest projectile slot.
        //
        // IMPORTANT: the bookkeeping below is STATIC, not per-entity. NPC.Transform()
        // (Slimer at half HP, Lost Girl -> Nymph, worm splits, ...) rebuilds
        // npc.globalNPCs mid-AI, discarding the per-entity instance that held the restore
        // flag — which left the owner permanently teleported onto Saria. NPC AI is
        // single-threaded so only one relocation is ever in flight; static state survives
        // the instance swap, letting PostAI on the rebuilt instance (or the safety nets)
        // always restore the real position.
        private const float SariaAggroPlayerRange = 500f; // world px the player must exceed
        private static bool _aggroRelocated;
        private static int _aggroPlayerIndex = -1;
        private static Vector2 _aggroSavedPosition;
        private static Vector2 _aggroSavedVelocity;

        // Throttled visibility into how often the redirect actually engages vs. how many
        // hostile NPCs were merely eligible candidates (LinkCable on, NPC not
        // friendly/town/trivial) but didn't meet the distance condition. Previously this
        // system was only visible in the log when a leak needed recovering (AggroLeak
        // tag), so there was no way to tell "redirect never fires" from "redirect fires
        // constantly but has no visible effect". These are per-NPC-per-tick samples, not
        // distinct NPC counts (the same NPC is sampled again every tick it stays eligible).
        private static int _aggroEligibleSamples;
        private static int _aggroRedirectSamples;
        private static int _aggroLogCooldown;

        public void StartIceDomeAnimation(int randomSize)
        {
            if (IceDomeActive) return; // already running, don't restart from scratch
            RandomSize = randomSize;
            IceDomeTimer = 0;
            IceDomeActive = true;
        }

        public void SetFreezeInitiator(int playerIndex)
        {
            freezeInitiatorPlayer = playerIndex;
        }

        public override void ResetEffects(NPC npc)
        {
            SariaCurseD = false;
            Burning2 = false;
            GhostBurning = false;
            Frostburn2 = false;
            Stronger = false;
        }
        
        /// <summary>
        /// DrawEffects is called before the NPC is drawn - this is where we tint the NPC
        /// Uses FULL COLOR OVERRIDE to ensure frozen effect is visible at all light levels
        /// and adds a light blue light at NPC center for glowing frozen effect
        /// </summary>
        public override void DrawEffects(NPC npc, ref Color drawColor)
        {
            // Apply frozen tint effect if NPC is marked
            Color? frozenTint = FrozenNPCVisualManager.GetFrozenTintColor(npc.whoAmI);
            if (frozenTint.HasValue)
            {
                // Get blend strength from alpha (fades over time)
                float blendStrength = frozenTint.Value.A / 255f;
                
                // ADD LIGHT BLUE LIGHT at NPC center
                // This makes the NPC glow with icy blue light
                // Light values are 0-1 range for RGB
                float lightIntensity = blendStrength * 1.2f; // Slightly boost intensity
                float lightR = 0.6f * lightIntensity;   // Pale blue - some red
                float lightG = 0.85f * lightIntensity;  // High green for cyan tint
                float lightB = 1.0f * lightIntensity;   // Full blue for icy effect
                
                // Add the light at NPC center - this illuminates the NPC sprite
                Lighting.AddLight(npc.Center, lightR, lightG, lightB);
                
                // FULL COLOR OVERRIDE - blend towards pale icy blue
                Color targetColor = new Color(frozenTint.Value.R, frozenTint.Value.G, frozenTint.Value.B);
                
                // Strong blend towards pale icy blue - overrides original color
                float effectBlend = blendStrength * 0.92f;
                
                // Directly override the draw color by lerping towards pale icy blue
                drawColor = Color.Lerp(drawColor, targetColor, effectBlend);
                
                // Force minimum brightness to ensure visibility in dark areas
                byte minBrightness = (byte)(150 * blendStrength);
                if (drawColor.R < minBrightness)
                    drawColor.R = minBrightness;
                if (drawColor.G < minBrightness)
                    drawColor.G = minBrightness;
                if (drawColor.B < minBrightness)
                    drawColor.B = minBrightness;
                
                // Spawn dust particles based on effect strength
                float effectStrength = blendStrength;
                
                // Spawn Fog dust for misty frozen effect
                if (effectStrength > 0.2f && Main.rand.NextBool(25))
                {
                    Vector2 dustPos = npc.Center + new Vector2(
                        Main.rand.NextFloat(-npc.width / 2f, npc.width / 2f),
                        Main.rand.NextFloat(-npc.height / 2f, npc.height / 2f)
                    );
                    Vector2 dustVel = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1.5f, 0.5f));
                    Dust fog = Dust.NewDustPerfect(dustPos, ModContent.DustType<Fog>(), dustVel, 0, default, 1.2f);
                    fog.noGravity = true;
                }
                
                // Spawn Snow2 dust for icy particle effects
                if (effectStrength > 0.15f && Main.rand.NextBool(15))
                {
                    Vector2 dustPos = npc.Center + new Vector2(
                        Main.rand.NextFloat(-npc.width / 2f, npc.width / 2f),
                        Main.rand.NextFloat(-npc.height / 2f, npc.height / 2f)
                    );
                    Vector2 dustVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-0.5f, 0.5f));
                    Dust snow = Dust.NewDustPerfect(dustPos, ModContent.DustType<Snow2>(), dustVel, 0, default, Main.rand.NextFloat(0.8f, 1.4f));
                    snow.noGravity = true;
                }
            }
        }
        
        public int RandomSize { get; private set; }
        public override void SetDefaults(NPC npc)
        {
            RandomSize = Main.rand.Next(9, 12);
            IceDomeActive = false;
            IceDomeTimer = 0;
            wasFrozen = false;
            freezeInitiatorPlayer = -1;
            visualsAuthorized = false;
            // Add a check to ensure the index is within the array's bounds.
            if (npc.whoAmI >= 0 && npc.whoAmI < hasBuffSynced.Length)
            {
                hasBuffSynced[npc.whoAmI] = false;
            }
        }
        public override void OnSpawn(NPC npc, IEntitySource source)
        {
            RandomSize = Main.rand.Next(9, 12);
            IceDomeActive = false;
            IceDomeTimer = 0;
            wasFrozen = false;
            freezeInitiatorPlayer = -1;
            visualsAuthorized = false;
        }
        public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
        {
            binaryWriter.Write(RandomSize);
            binaryWriter.Write(freezeInitiatorPlayer);
            // We want to send all buffs, not just the custom one.
            for (int i = 0; i < NPC.maxBuffs; i++)
            {
                if (npc.buffType[i] > 0)
                {
                    bitWriter.WriteBit(true);
                    binaryWriter.Write(npc.buffType[i]);
                    binaryWriter.Write(npc.buffTime[i]);
                }
                else
                {
                    bitWriter.WriteBit(false);
                }
            }
        }
        // This hook is called on the client to read the packet.
        public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
        {
            RandomSize = binaryReader.ReadInt32();
            freezeInitiatorPlayer = binaryReader.ReadInt32();
            for (int i = 0; i < NPC.maxBuffs; i++)
            {
                if (bitReader.ReadBit())
                {
                    npc.buffType[i] = binaryReader.ReadInt32();
                    npc.buffTime[i] = binaryReader.ReadInt32();
                }
                else
                {
                    npc.buffType[i] = 0;
                }
            }
            
            // JIP PROTECTION: When a client joins and receives NPC data
            // Check if NPC has frozen buff and mark it for visual overlay
            int frozenBuffIndex = npc.FindBuffIndex(ModContent.BuffType<EnemyFrozen>());
            if (frozenBuffIndex != -1)
            {
                // Authorize ice dome visuals so frozen enemies are visible
                visualsAuthorized = true;
                
                // Mark NPC for frozen overlay visual effect (for late joiners)
                // This ensures the light blue tint appears on already-frozen enemies
                FrozenNPCVisualManager.MarkNPCAsFrozenForLateJoiner(npc.whoAmI);
            }
        }
        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            if (Frostburn2)
            {
                // These lines zero out any positive lifeRegen. This is expected for all bad life regeneration effects.
                if (npc.lifeRegen > 0)
                {
                    npc.lifeRegen = 0;
                }
                npc.lifeRegen -= 16;
                if (damage < (npc.lifeMax * .005f))
                {
                    damage = (int)((npc.lifeMax * .005f) + 1);
                }
            }
            if (Burning2)
            {
                // These lines zero out any positive lifeRegen. This is expected for all bad life regeneration effects.
                if (npc.lifeRegen > 0)
                {
                    npc.lifeRegen = 0;
                }
                npc.lifeRegen -= 16;
                if (damage < (npc.lifeMax * .005f))
                {
                    damage = (int)((npc.lifeMax * .005f) + 1);
                }
                if (SariaCurseD)
                {
                    if (npc.lifeRegen > 0)
                    {
                        npc.lifeRegen = 0;
                    }
                    npc.lifeRegen -= 16;
                    if (damage < (npc.lifeMax * .01f))
                    {
                        damage = (int)((npc.lifeMax * .01f) + 1);
                    }
                    if (!npc.boss)
                    {
                        npc.noTileCollide = false;
                        npc.noGravity = false;
                    }
                }
            }
            if (GhostBurning)
            {
                // These lines zero out any positive lifeRegen. This is expected for all bad life regeneration effects.
                if (npc.lifeRegen > 0)
                {
                    npc.lifeRegen = 0;
                }
                npc.lifeRegen -= 16;
                if (damage < (npc.lifeMax * .005f))
                {
                    damage = (int)((npc.lifeMax * .005f) + 1);
                }
            }
        }
        public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
        {
            SariaSpawnSystem.SpawnPass pass = SariaSpawnSystem.CurrentPass;

            // Non-owner players inside someone else's split window: vanilla's SpawnNPC
            // loops over EVERY active player within each pass, so during the owner's
            // split every other player is visited TWICE per tick. They must come
            // through unaffected: one normal attempt on the PLAYER pass (their real
            // location, their own buffs, NO regional gate -- the gate's slots/targets
            // describe the owner's two regions, not this player's location), and full
            // suppression on the SARIA pass so the window doesn't double their spawn
            // rate. Before this guard they inherited the owner's gate on BOTH passes:
            // owner regions full -> fed maxSpawns=0 -> every non-owner's spawns froze
            // (the multiplayer "stunted to zero" report). A second player who also has
            // LinkCable on counts as a non-owner here too -- only one split runs per
            // tick, and this keeps their spawns vanilla instead of frozen.
            if (pass != SariaSpawnSystem.SpawnPass.None && player.whoAmI != SariaSpawnSystem.CurrentOwnerIndex)
            {
                if (pass == SariaSpawnSystem.SpawnPass.Saria)
                {
                    // Second visit this tick -- their one attempt already ran in the
                    // player pass. Same suppression idiom the gate uses for full regions.
                    maxSpawns = 0;
                    return;
                }

                ApplyPersonalSpawnLogic(player, ref spawnRate, ref maxSpawns);
                if (spawnRate < 1)
                    spawnRate = 1;
                return;
            }

            // ── Saria pass: a real modded player's profile at HER location ───
            // During SariaSpawnSystem's Saria pass the owner is standing at Saria's
            // position with her LIVE zone flags recomputed, so the engine-provided
            // spawnRate / maxSpawns entering this hook already describe HER location.
            // She then gets the SAME environmental stacking a lone player standing
            // there would get from this mod (corrupt/blood moon/eclipse/full moon/
            // sandstorm — driven by HER recomputed zones plus the global clock), so
            // her cap moves with events exactly like the player's row does. Before
            // this, her pass had NO event logic at all: on a full moon the player's
            // cap jumped to 50 while hers stayed at the engine's ~5-6, which is why
            // the panel rows looked inconsistent. Only the PLAYER-PERSONAL buff
            // overrides are swapped out: her OWN candle environment (Calming Candle /
            // Reaj Candle tiles near HER) stands in for the owner's CalmMind /
            // CorruptMind buffs. Her 50-slot ceiling is applied by ApplyRegionGate
            // below.
            if (pass == SariaSpawnSystem.SpawnPass.Saria)
            {
                bool bloodMoonIsActive = Main.bloodMoon;
                bool eclipseIsActive = Main.eclipse;
                bool majorEventIsActive = Main.invasionType > 0 || player.ZoneDungeon;

                Items.Strange.Saria saria = SariaSpawnSystem.CurrentSaria;
                if (saria != null && saria.SariaHasCalmMindCandle)
                {
                    ApplyCalmMind(player, ref spawnRate, ref maxSpawns, majorEventIsActive, bloodMoonIsActive, eclipseIsActive);
                }
                else if (saria != null && saria.SariaHasReajCandle)
                {
                    ApplyCorruptMind(ref spawnRate, ref maxSpawns);
                }
                else
                {
                    ApplyNormalSpawnRates(player, ref spawnRate, ref maxSpawns, bloodMoonIsActive, eclipseIsActive);
                }
            }
            else
            {
                // ── Player logic ─────────────────────────────────────────────
                // Identical whether this is a normal (non-split) tick or the split's
                // Player pass: the player spawns exactly like a non-Saria-owner player
                // at their own location. Saria's existence contributes nothing here.
                ApplyPersonalSpawnLogic(player, ref spawnRate, ref maxSpawns);
            }

            // ApplyCorruptMind's tiny multiplier truncates spawnRate to 0. Floor it at 1
            // (0 and 1 both mean "attempt every tick" to vanilla's Next(rate)==0 pacing
            // check, so this changes no intended behavior) purely to guard against
            // zero-argument RNG/division edge cases downstream.
            if (spawnRate < 1)
                spawnRate = 1;

            // ── Regional gate (LinkCable split only) ─────────────────────────
            // Outside the split this only refreshes debug trackers — the player's
            // computed rates go to vanilla untouched and Saria has ZERO influence on
            // spawning. During a split pass it clamps this pass's target (player:
            // Main.maxNPCs−50 ceiling; Saria: 50 ceiling) and converts vanilla's global
            // maxSpawns check into a true regional one. See ApplyRegionGate.
            ApplyRegionGate(ref maxSpawns);

            LogPassRates(spawnRate, maxSpawns);
        }

        // A player's own spawn profile at their own location — buff overrides first
        // (CalmMind > CorruptMind), then the normal stacking rules. Shared by the
        // regular/player-pass branch of EditSpawnRate and by non-owner players getting
        // their vanilla attempt during someone else's split window.
        private void ApplyPersonalSpawnLogic(Player player, ref int spawnRate, ref int maxSpawns)
        {
            bool bloodMoonIsActive = Main.bloodMoon;
            bool eclipseIsActive = Main.eclipse;
            bool majorEventIsActive = Main.invasionType > 0 || player.ZoneDungeon; // Add other events here as needed
            // Check high-priority override buffs first.
            if (MiscUtilities.Fairy(player).CalmMind)
            {
                ApplyCalmMind(player, ref spawnRate, ref maxSpawns, majorEventIsActive, bloodMoonIsActive, eclipseIsActive);
            }
            else if (MiscUtilities.Fairy(player).CorruptMind)
            {
                ApplyCorruptMind(ref spawnRate, ref maxSpawns);
            }
            else // Apply normal stacking logic.
            {
                ApplyNormalSpawnRates(player, ref spawnRate, ref maxSpawns, bloodMoonIsActive, eclipseIsActive);
            }
        }

        // Emits one debug line per spawn pass with the FINAL effective rates, but only
        // on the throttled calls SariaSpawnSystem marks for logging (once per second),
        // so the rate lines pair up with the hook's own per-window spawn summary.
        //
        // Reading the line:
        //   computedCap → the cap the engine/candles produced for this location BEFORE
        //                 the ceiling clamp (what a lone player standing there would get)
        //   target      → computedCap after the ceiling (Saria: ≤50, player: ≤maxNPCs−50)
        //   regionSlots → nearest-anchor population of THIS region only
        //   headroom    → target − regionSlots (how many more may spawn here)
        //   gate        → OPEN (headroom > 0, spawns allowed) / CLOSED (region full,
        //                 vanilla was fed maxSpawns=0 so it cannot spawn at all)
        //   fedMax      → the raw value handed to vanilla's global check (trick value)
        private static void LogPassRates(int spawnRate, int maxSpawns)
        {
            if (!SariaSpawnSystem.LogRatesThisCall)
                return;

            SariaSpawnSystem.SpawnPass pass = SariaSpawnSystem.CurrentPass;
            if (pass == SariaSpawnSystem.SpawnPass.None)
                return;

            float global = SariaSpawnSystem.LastGlobalSlotCount;
            float ownerToSariaDist = SariaSpawnSystem.LastOwnerToSariaDistance;

            if (pass == SariaSpawnSystem.SpawnPass.Saria)
            {
                Items.Strange.Saria saria = SariaSpawnSystem.CurrentSaria;
                string candle = saria == null ? "none"
                    : saria.SariaHasCalmMindCandle ? "CalmingCandle"
                    : saria.SariaHasReajCandle ? "ReajCandle"
                    : "none";
                float regionSlots = SariaSpawnSystem.LastSariaRegionSlots;
                int target = SariaSpawnSystem.LastSariaCap;
                bool merged = SariaSpawnSystem.LastRegionsMerged;
                float gatePop = merged ? SariaSpawnSystem.LastPlayerRegionSlots + regionSlots : regionSlots;
                int gateTarget = merged ? System.Math.Max(target, SariaSpawnSystem.LastPlayerCap) : target;
                float headroom = gateTarget - gatePop;
                SariaDebug.Log("SpawnRate",
                    $"SARIA pass: spawnRate={spawnRate} candle={candle} " +
                    $"computedCap={SariaSpawnSystem.LastSariaComputedCap} target={target} (ceiling={SariaSpawnSystem.SariaRegionMaxSpawns}) " +
                    $"merged={merged} gatePop={gatePop:F1}/{gateTarget} headroom={headroom:F1} " +
                    $"gate={(headroom > 0f ? "OPEN" : "CLOSED")} fedMax={maxSpawns} global={global:F1} " +
                    $"townNPCs={SariaSpawnSystem.LastSariaTownNPCs:F1} ownerToSariaDist={ownerToSariaDist:F0}");
            }
            else
            {
                float regionSlots = SariaSpawnSystem.LastPlayerRegionSlots;
                int target = SariaSpawnSystem.LastPlayerCap;
                bool merged = SariaSpawnSystem.LastRegionsMerged;
                float gatePop = merged ? SariaSpawnSystem.LastSariaRegionSlots + regionSlots : regionSlots;
                int gateTarget = merged ? System.Math.Max(target, SariaSpawnSystem.LastSariaCap) : target;
                float headroom = gateTarget - gatePop;
                SariaDebug.Log("SpawnRate",
                    $"PLAYER pass: spawnRate={spawnRate} " +
                    $"computedCap={SariaSpawnSystem.LastPlayerComputedCap} target={target} (ceiling={Main.maxNPCs - SariaSpawnSystem.SariaRegionMaxSpawns}) " +
                    $"merged={merged} gatePop={gatePop:F1}/{gateTarget} headroom={headroom:F1} " +
                    $"gate={(headroom > 0f ? "OPEN" : "CLOSED")} fedMax={maxSpawns} global={global:F1} " +
                    $"townNPCs={SariaSpawnSystem.LastPlayerTownNPCs:F1} ownerToSariaDist={ownerToSariaDist:F0}");
            }
        }

        // ── The regional gate (LinkCable split only) ────────────────────────────
        //
        // WHY THIS EXISTS: vanilla's maxSpawns check in NPC.SpawnNPC is GLOBAL — it
        // sums npcSlots across the whole world and compares that single number against
        // maxSpawns. Two passes with different anchors therefore share one budget: a
        // full player region made Saria's empty region fail its check too, and vice
        // versa. They were never separate budgets.
        //
        // THE DESIGN (per the rework):
        //  * Each pass's TARGET is what the engine/candles computed for that location,
        //    clamped by a hard CEILING (never raised to it):
        //      Saria:  min(computed, SariaRegionMaxSpawns=50)
        //        — a Reaj candle can push her toward 50 but never past it; a quiet
        //          forest gives her the ~5-6 a real player would get there.
        //      Player: min(computed, Main.maxNPCs − 50)
        //        — their natural cap stands; the ceiling only bites if their modifiers
        //          go wild, so Saria's 50 always stays spawnable.
        //  * Each region's population is counted BOX-SCOPED
        //    (SariaSpawnSystem.CountRegionSlots): an NPC fills a region's slots only if
        //    it is physically inside that anchor's box (nearest anchor breaks overlap
        //    ties). NPCs near NEITHER anchor — e.g. a second player's horde across the
        //    map — fill NO region: they used to fill both via nearest-anchor and froze
        //    all split spawning in multiplayer.
        //  * The gate per pass:
        //      headroom = target − regionSlots
        //      headroom <= 0  →  feed maxSpawns = 0 (bulletproof: vanilla can never
        //                        spawn past a full region, regardless of accounting)
        //      headroom  > 0  →  feed maxSpawns = globalSlots + headroom, where
        //                        globalSlots is the EXACT world tally under vanilla's
        //                        own predicate, so vanilla's check (global < fed)
        //                        reduces algebraically to (regionSlots < target) —
        //                        out-of-region NPCs appear on both sides and cancel.
        private void ApplyRegionGate(ref int maxSpawns)
        {
            // Capture the real, engine-provided cap (after biome/event adjustments) so the
            // hook log and debug trackers can report it without a vanilla constant. This
            // runs every EditSpawnRate call, including non-split ones, keeping it fresh.
            SariaSpawnSystem.LastWorldCap = maxSpawns;

            SariaSpawnSystem.SpawnPass pass = SariaSpawnSystem.CurrentPass;
            if (pass == SariaSpawnSystem.SpawnPass.None)
            {
                // Not split — vanilla/player logic goes through untouched. Trackers mirror
                // the single engine cap so the debug panel reads sensibly with LinkCable off.
                SariaSpawnSystem.LastPlayerCap = maxSpawns;
                SariaSpawnSystem.LastSariaCap  = maxSpawns;
                SariaSpawnSystem.LastRegionsMerged = false;
                return;
            }

            SariaSpawnSystem.CountRegionSlots(out float playerSlots, out float sariaSlots, out float globalSlots);
            SariaSpawnSystem.LastGlobalSlotCount   = globalSlots;
            SariaSpawnSystem.LastSariaRegionSlots  = sariaSlots;
            SariaSpawnSystem.LastPlayerRegionSlots = playerSlots;

            // ── Merged budget when the two anchors describe the same place ──────
            // With Saria near her owner the boxes mostly overlap; nearest-anchor
            // attribution then SPLITS one shared population across two independent
            // budgets, so the overlap area could legally fill to BOTH targets
            // combined (~2× cap) before either gate closed — the "exceeds the cap
            // only when near the player" report. Merge into one budget: population
            // is the sum of both attributions (== everything near the pair), target
            // is the LARGER of the two (never the sum), so the shared area holds
            // exactly one region's worth of enemies. Both passes stay live under
            // the merged gate — spawning never freezes, it just shares one budget.
            bool merged = SariaSpawnSystem.LastOwnerToSariaDistance < SariaSpawnSystem.RegionMergeDistance;
            SariaSpawnSystem.LastRegionsMerged = merged;

            if (pass == SariaSpawnSystem.SpawnPass.Saria)
            {
                // CEILING, not target: her location's computed cap stands unless it
                // exceeds her 50-slot reservation.
                SariaSpawnSystem.LastSariaComputedCap = maxSpawns;
                int target = System.Math.Min(maxSpawns, SariaSpawnSystem.SariaRegionMaxSpawns);
                SariaSpawnSystem.LastSariaCap = target;

                float regionPop = merged ? playerSlots + sariaSlots : sariaSlots;
                int gateTarget = merged ? System.Math.Max(target, SariaSpawnSystem.LastPlayerCap) : target;
                float headroom = gateTarget - regionPop;
                maxSpawns = headroom <= 0f ? 0 : (int)(globalSlots + headroom);
                SariaSpawnSystem.LastSariaFedMaxSpawns = maxSpawns;
            }
            else
            {
                // Player's own natural cap, ceilinged so Saria's 50 always fits under the
                // shared hard entity limit — clamped only, never raised.
                SariaSpawnSystem.LastPlayerComputedCap = maxSpawns;
                int target = System.Math.Min(maxSpawns, Main.maxNPCs - SariaSpawnSystem.SariaRegionMaxSpawns);
                if (target < 0)
                    target = 0;
                SariaSpawnSystem.LastPlayerCap = target;

                // NOTE: on the player pass LastSariaCap still holds the previous Saria-pass
                // value — the passes run back-to-back each tick, so it's at most 1 tick
                // stale, and only consulted while merged (anchors nearly overlapping).
                float regionPop = merged ? playerSlots + sariaSlots : playerSlots;
                int gateTarget = merged ? System.Math.Max(target, SariaSpawnSystem.LastSariaCap) : target;
                float headroom = gateTarget - regionPop;
                maxSpawns = headroom <= 0f ? 0 : (int)(globalSlots + headroom);
                SariaSpawnSystem.LastPlayerFedMaxSpawns = maxSpawns;
            }
        }
        // Handles the logic for the CalmMind buff.
        private void ApplyCalmMind(Player player, ref int spawnRate, ref int maxSpawns, bool majorEventIsActive, bool bloodMoonIsActive, bool eclipseIsActive)
        {
            if (bloodMoonIsActive || eclipseIsActive)
            {
                // Set to normal rates during Blood Moon or Solar Eclipse.
                // Adjust these numbers to what you consider "normal."
                spawnRate = 600;
                maxSpawns = 6;
            }
            else if (!majorEventIsActive)
            {
                // Stop spawns completely otherwise.
                spawnRate = 0;
                maxSpawns = 0;
            }
        }
        // Handles the logic for the CorruptMind buff.
        private void ApplyCorruptMind(ref int spawnRate, ref int maxSpawns)
        {
            spawnRate = (int)((double)spawnRate * .00000000001);
            maxSpawns = (int)((float)maxSpawns * 30f);
        }
        // Handles the logic for normal, stacking spawn rate modifications.
        //
        // Two kinds of modifier live here:
        //  * EVENT caps (blood moon / eclipse / full moon) REPLACE the working cap with
        //    a fixed value — they define the night's baseline.
        //  * The SANDSTORM is a MULTIPLIER on top of whatever cap is in effect (engine
        //    value or an event's fixed cap). It used to be invisible on event nights:
        //    the engine's own sandstorm-boosted cap entered this method, then the full
        //    moon branch overwrote it with a flat 50 — so a sandstorm appeared to do
        //    nothing (or even read LOWER on rows that never got event logic at all).
        //    Multiplying AFTER the fixed assignments means a sandstorm now inherently
        //    boosts spawns on ANY night: normal ~6 → ~9, full moon 50 → 75, blood
        //    moon 30 → 45. ZoneSandstorm already folds in Sandstorm.Happening +
        //    desert + surface, and during the Saria pass it was recomputed at HER
        //    location, so each pass reacts to the storm it is actually standing in.
        private void ApplyNormalSpawnRates(Player player, ref int spawnRate, ref int maxSpawns, bool bloodMoonIsActive, bool eclipseIsActive)
        {
            float spawnRateMultiplier = 1f;
            float maxSpawnsMultiplier = 1f;
            int fixedMaxSpawns = maxSpawns;
            // Apply stacking effects based on conditions.
            if (player.ZoneCorrupt || player.ZoneCrimson)
            {
                spawnRateMultiplier *= 0.4f;
            }
            if (bloodMoonIsActive)
            {
                spawnRateMultiplier *= 0.000001f;
                fixedMaxSpawns = 30;
            }
            if (eclipseIsActive)
            {
                spawnRateMultiplier *= 0.1f;
                fixedMaxSpawns = 20;
            }
            if (Main.moonPhase == 0 && !Main.dayTime)
            {
                spawnRateMultiplier *= 0.2f;
                fixedMaxSpawns = 50;
            }
            if (player.ZoneSandstorm)
            {
                spawnRateMultiplier *= 0.6f;
                maxSpawnsMultiplier *= 1.5f;
            }
            spawnRate = (int)((double)spawnRate * spawnRateMultiplier);
            maxSpawns = (int)(fixedMaxSpawns * maxSpawnsMultiplier);
        }
        public override bool CheckActive(NPC npc)
        {
            if (npc.HasBuff(ModContent.BuffType<EnemyFrozen>()))
            {
                return false;
            }

            // Dead NPCs must be allowed to unload normally — never block cleanup for them.
            if (npc.life <= 0)
                return true;

            // Prevent despawn when the NPC is within a keep-alive box centred on any
            // active Saria projectile whose owner has LinkCable engaged. With LinkCable
            // OFF, Saria has zero influence on spawning/despawning — vanilla despawn
            // rules apply everywhere (per the rework: no Saria involvement outside link
            // mode; her old always-on keep-alive silently inflated NPC counts and made
            // caps look ignored).
            //
            // NOTE: vanilla spawns NPCs in a ring out to ~0.7 * screen from the spawn
            // anchor (they always appear just off-screen and walk in). A half-screen box
            // (0.5 * screen) is SMALLER than that ring, so enemies redirected to spawn at
            // Saria would land outside the box and despawn the very next frame. The box
            // extents come from SariaSpawnSystem so the keep-alive region, the entity
            // trackers, and the per-region spawn cap all use one shared size.
            int sariaType = ModContent.ProjectileType<Items.Strange.Saria>();
            float halfW = SariaSpawnSystem.BoxHalfWidth;
            float halfH = SariaSpawnSystem.BoxHalfHeight;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];
                if (!proj.active || proj.type != sariaType)
                    continue;

                Player owner = Main.player[proj.owner];
                if (!owner.active || owner.dead)
                    continue;
                if (!owner.TryGetModPlayer(out FairyPlayer fp) || !fp.LinkCable)
                    continue;

                if (npc.Center.X >= proj.Center.X - halfW &&
                    npc.Center.X <= proj.Center.X + halfW &&
                    npc.Center.Y >= proj.Center.Y - halfH &&
                    npc.Center.Y <= proj.Center.Y + halfH)
                {
                    return false;
                }
            }

            return true;
        }
        // ── LinkCable aggro redirect ───────────────────────────────────────────────
        // Vanilla enemies can only target a Player (npc.target), never a projectile. To
        // make them chase Saria we reuse SariaSpawnSystem's proven trick: temporarily move
        // the owning player onto Saria for the duration of vanilla AI, then restore in
        // PostAI before any render/net runs. Only fires while LinkCable is on, the player
        // is beyond SariaAggroPlayerRange of this NPC, and Saria is closer than the player.
        //
        // Runs on EVERY machine, clients included. Terraria clients simulate NPC AI
        // locally between server syncs using their synced copy of player positions — when
        // this redirect was server-only, the client's simulation walked the enemy toward
        // the owner's REAL position, then each authoritative server update snapped it
        // back toward Saria, over and over (the multiplayer "slide and jerk between
        // Saria and the player" report). Mirroring the exact same relocate/restore on the
        // client makes both simulations compute the same motion, so enemies walk smoothly
        // toward Saria everywhere. The relocation only ever touches the machine's local
        // COPY of the owner player mid-NPC-AI and is restored in PostAI (plus the PreAI /
        // PostUpdateNPCs safety nets), so nothing else ever observes the moved position.
        public override bool PreAI(NPC npc)
        {
            // Safety net: if a previous NPC's PostAI never ran (exception mid-AI), put the
            // owner back before this NPC's AI can observe the wrong position.
            RestoreAggroRelocation();

            if (npc.friendly || npc.townNPC || npc.lifeMax <= 5)
                return true;
            if (!TryGetAggroSariaAnchor(npc.Center, out int ownerIndex, out Vector2 sariaCenter, out bool anyLinkCableSaria))
            {
                if (anyLinkCableSaria)
                    _aggroEligibleSamples++;
                return true;
            }

            _aggroEligibleSamples++;
            _aggroRedirectSamples++;
            Player owner = Main.player[ownerIndex];
            _aggroRelocated = true;
            _aggroPlayerIndex = ownerIndex;
            _aggroSavedPosition = owner.position;
            _aggroSavedVelocity = owner.velocity;
            owner.position = sariaCenter - new Vector2(owner.width * 0.5f, owner.height * 0.5f);
            owner.velocity = Vector2.Zero;
            npc.target = ownerIndex; // ensure target selection resolves to the relocated owner
            return true;
        }
        public override void PostAI(NPC npc)
        {
            // Static restore: survives NPC.Transform() rebuilding this GlobalNPC instance
            // mid-AI (the rebuilt instance still sees the pending static relocation).
            RestoreAggroRelocation();
        }

        /// <summary>
        /// Puts the relocated owner back at their real position/velocity if an aggro
        /// relocation is still in flight. Called from PostAI (normal path), plus the top
        /// of PreAI and SariaSpawnSystem.PostUpdateNPCs as safety nets, so a skipped
        /// PostAI (exception or Transform instance swap) can never leave the player
        /// physically teleported onto Saria.
        /// </summary>
        internal static void RestoreAggroRelocation()
        {
            if (!_aggroRelocated || _aggroPlayerIndex < 0)
                return;

            Player owner = Main.player[_aggroPlayerIndex];
            owner.position = _aggroSavedPosition;
            owner.velocity = _aggroSavedVelocity;
            _aggroRelocated = false;
            _aggroPlayerIndex = -1;
        }

        /// <summary>
        /// Throttled (~5s) flush of the aggro-redirect activity counters accumulated across
        /// every PreAI call since the last flush. Called once per tick from
        /// SariaSpawnSystem.PostUpdateNPCs so normal redirect engagement is visible in
        /// debugsaria.txt even when nothing ever leaks (the only case AggroLeak covers).
        /// Zero counts across a whole session would mean the distance/range gate in PreAI
        /// never actually triggers, which is otherwise invisible.
        /// </summary>
        internal static void LogAggroActivity()
        {
            if (_aggroLogCooldown > 0)
            {
                _aggroLogCooldown--;
                return;
            }
            _aggroLogCooldown = 300; // ~5s at 60 ticks/sec

            SariaDebug.LogSilent("AggroActivity",
                $"Last ~5s: eligibleSamples={_aggroEligibleSamples} redirectSamples={_aggroRedirectSamples} " +
                $"(samples are per-NPC-per-tick, not distinct NPCs; range={SariaAggroPlayerRange:F0})");

            _aggroEligibleSamples = 0;
            _aggroRedirectSamples = 0;
        }
        // Picks the aggro anchor for ONE SPECIFIC NPC: the NEAREST LinkCable Saria whose
        // owner is beyond SariaAggroPlayerRange of the NPC and farther than she is.
        //
        // WHY PER-NPC: the old version returned the FIRST LinkCable Saria in projectile-
        // slot order — with two LinkCable pairs active, every enemy in the world anchored
        // to whichever Saria happened to sit in the lower slot, so enemies standing next
        // to pair B's Saria chased pair A's (possibly across the map), and which Saria
        // "won" flipped arbitrarily when projectile slots got reused. Evaluating each
        // candidate pair against THIS NPC's position gives every enemy the Saria that is
        // actually near it, and the same distances resolve identically on server and
        // clients (all inputs are synced positions), keeping the mirrored simulation in
        // agreement.
        //
        // anyLinkCableSaria reports whether at least one eligible pair existed at all
        // (for the activity counters), independent of whether a redirect was warranted.
        private static bool TryGetAggroSariaAnchor(Vector2 npcCenter, out int ownerIndex, out Vector2 sariaCenter, out bool anyLinkCableSaria)
        {
            ownerIndex = -1;
            sariaCenter = Vector2.Zero;
            anyLinkCableSaria = false;
            float bestSariaDist = float.MaxValue;
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

                anyLinkCableSaria = true;
                float ownerDist = Vector2.Distance(npcCenter, owner.Center);
                float sariaDist = Vector2.Distance(npcCenter, proj.Center);

                // This pair only redirects when ITS owner is too far away and ITS Saria
                // is the nearer of the two — same rule as before, now applied per pair.
                if (ownerDist <= SariaAggroPlayerRange || sariaDist >= ownerDist)
                    continue;

                if (sariaDist < bestSariaDist)
                {
                    bestSariaDist = sariaDist;
                    ownerIndex = proj.owner;
                    sariaCenter = proj.Center;
                }
            }
            return ownerIndex >= 0;
        }
        private void RemoveFrozenBuff(NPC npc)
        {
            // Handle the buff removal, ensuring it's synced in multiplayer.
            if (Main.netMode == NetmodeID.Server)
            {
                // The server removes the buff directly and syncs.
                int buffIndex = npc.FindBuffIndex(ModContent.BuffType<EnemyFrozen>());
                if (buffIndex != -1)
                {
                    npc.DelBuff(buffIndex);
                    npc.netUpdate = true;
                }
            }
            else if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                // A client sends a packet to the server to remove the buff.
                ModPacket packet = Mod.GetPacket();
                packet.Write((byte)SariaMod.SoundMessageType.RemoveBuff);
                packet.Write(npc.whoAmI);
                packet.Send();
            }
            else if (Main.netMode == NetmodeID.SinglePlayer)
            {
                // In single-player, remove the buff directly.
                int buffIndex = npc.FindBuffIndex(ModContent.BuffType<EnemyFrozen>());
                if (buffIndex != -1)
                {
                    npc.DelBuff(buffIndex);
                }
            }
        }
        public override void OnHitByItem(NPC npc, Player player, Item item, int damage, float knockback, bool crit)
        {
            if (npc.HasBuff(ModContent.BuffType<EnemyFrozen>()))
            {
                RemoveFrozenBuff(npc);
            }
        }
        public override void OnHitByProjectile(NPC npc, Projectile projectile, int damage, float knockback, bool crit)
        {
            Player player = Main.player[projectile.owner];
            if (npc.HasBuff(ModContent.BuffType<EnemyFrozen>()) && projectile.type != ModContent.ProjectileType<ColdWaveHitBox>() && projectile.type != ModContent.ProjectileType<HealBubble>() && projectile.type != ModContent.ProjectileType<ColdWaveCenter>())
            {
                RemoveFrozenBuff(npc);
            }
            if (projectile.type == ModContent.ProjectileType<LaunchHitBox>())
            {
                npc.position.Y = (player.position.Y - 50);
            }
        }
        public override void AI(NPC npc)
        {
            // Ice Dome Animation Trigger
            bool isFrozen = npc.HasBuff(ModContent.BuffType<EnemyFrozen>());
            if (isFrozen && !wasFrozen)
            {
                if (Main.netMode == NetmodeID.Server)
                {
                    Netcode.IceDomeNetworking.SendPacket(npc.whoAmI, RandomSize, freezeInitiatorPlayer);

                    // HOST: Check if local player (host) was the one who froze this enemy
                    if (!Main.dedServ)
                    {
                        if (freezeInitiatorPlayer == Main.myPlayer)
                        {
                            // Local host froze this enemy, skip animation and authorize visuals
                            IceDomeActive = true;
                            IceDomeTimer = 90;
                            visualsAuthorized = true;
                        }
                        else
                        {
                            // Another player froze it, let animation play normally
                            IceDomeActive = true;
                            IceDomeTimer = 0;
                            visualsAuthorized = true;
                        }
                    }
                }
                else if (Main.netMode == NetmodeID.MultiplayerClient)
                {
                    // The owner client is excluded from the IceDome packet, so they must
                    // start their own animation here. Other non-owner clients start theirs
                    // when they receive the IceDome packet from the server.
                    if (freezeInitiatorPlayer == Main.myPlayer)
                    {
                        IceDomeActive = true;
                        IceDomeTimer = 0;
                        visualsAuthorized = true;
                    }
                }
                else if (Main.netMode == NetmodeID.SinglePlayer)
                {
                    // SINGLEPLAYER: Always show animation growing from 0 to 90
                    IceDomeActive = true;
                    IceDomeTimer = 0;
                    visualsAuthorized = true;
                }
            }
            wasFrozen = isFrozen;

            if (IceDomeActive)
            {
                if (IceDomeTimer < 90)
                {
                    IceDomeTimer++;
                }
            }
            
            if (!isFrozen)
            {
                IceDomeActive = false;
                IceDomeTimer = 0;
                freezeInitiatorPlayer = -1;
                visualsAuthorized = false;
                frozenFrostburnTimer = 0; // Reset frostburn timer when no longer frozen
            }

            if (npc.HasBuff(ModContent.BuffType<MeteorSpikeDebuff>()) && !npc.HasBuff(ModContent.BuffType<MeteorLaunchDebuff>()))
            {
                if (npc.velocity.Y < 10)
                {
                    npc.velocity.Y = 10;
                    npc.netUpdate = true;
                }
            }
            if (npc.HasBuff(ModContent.BuffType<EnemyFrozen>()))
            {
                // Frostburn reapply timer - every minute (3600 frames) while frozen, reapply Frostburn2
                frozenFrostburnTimer++;
                if (frozenFrostburnTimer >= FrostburnReapplyInterval)
                {
                    frozenFrostburnTimer = 0;
                    
                    // Apply Frostburn2 debuff (lasts 10 seconds = 600 frames)
                    // Server handles buff application and syncs to clients
                    if (Main.netMode == NetmodeID.Server || Main.netMode == NetmodeID.SinglePlayer)
                    {
                        npc.AddBuff(ModContent.BuffType<Frostburn2>(), 600);
                        npc.netUpdate = true;
                    }
                }
                
                npc.frameCounter = 0;
                npc.velocity.X = 0;
                npc.netUpdate = true;
                Vector2 UpWardPosition = npc.Center;
                UpWardPosition.Y -= 30f;
                Vector2 UpWardPosition2 = npc.Center;
                UpWardPosition2.Y -= 50f;
                Vector2 UnderPosition = npc.Center;
                UnderPosition.Y += 30f;
                Vector2 UnderPosition2 = npc.Center;
                UnderPosition2.Y += ((npc.height/2) + .1f);
                bool over = Collision.WetCollision(UpWardPosition, npc.width, npc.height);
                bool over2 = Collision.WetCollision(UpWardPosition2, npc.width, npc.height);
                bool under = Collision.WetCollision(UnderPosition, npc.width, npc.height);
                bool under2 = Collision.WetCollision(UnderPosition2, npc.width, 1);
                bool iswet = Collision.WetCollision(npc.Center, npc.width, npc.height);
                float speed = 70;
                float inertia = 280f;
                npc.netUpdate = true;
                Vector2 direction = UpWardPosition - npc.Center;
                direction.Normalize();
                direction *= speed;
                float amplitude = .29f;
                float frequency = (RandomSize * .01f);
                if (!under && !over2 && !under2)
                {
                    npc.velocity.Y = 10;
                    npc.netUpdate = true;
                }
                else if (iswet && over && !npc.lavaWet)
                {
                    npc.velocity = (npc.velocity * (inertia - 17) + direction) / inertia;
                    npc.netUpdate = true;
                }
                else if (iswet && under && !over && !npc.lavaWet)
                {
                    npc.velocity.Y = 0;
                    npc.position.Y += amplitude * (float)Math.Sin(Main.time * frequency);
                    npc.netUpdate = true;
                }
                else
                {
                    npc.velocity.Y = 10;
                    npc.netUpdate = true;
                }
                npc.direction = -1;
                npc.rotation = 0;
                if (Main.rand.NextBool(20))
                {
                    float radius = (float)Math.Sqrt(Main.rand.Next(150 * 150));
                    double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                    Dust.NewDust(new Vector2((npc.Center.X) + radius * (float)Math.Cos(angle), (npc.Center.Y) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<Snow2>(), 0f, 0f, 0, default(Color), 1.5f);
                }
                if (IceDomeActive && IceDomeTimer < 90 && Main.rand.NextBool(10))
                {
                    Vector2 leftPos = npc.Center + new Vector2(-npc.width / 2 - Main.rand.Next(10), Main.rand.Next(-npc.height / 2, npc.height / 2));
                    Dust.NewDustPerfect(leftPos, ModContent.DustType<Fog>(), new Vector2(-2f, 0f), 0, default, 1.5f);


                    Vector2 rightPos = npc.Center + new Vector2(npc.width / 2 + Main.rand.Next(10), Main.rand.Next(-npc.height / 2, npc.height / 2));
                    Dust.NewDustPerfect(rightPos, ModContent.DustType<Fog>(), new Vector2(2f, 0f), 0, default, 1.5f);
                }
            }
            if (npc.HasBuff(ModContent.BuffType<MeteorSpikeDebuff>()))
            {
                float radius = (float)Math.Sqrt(Main.rand.Next(1 * 1));
                double angle = Main.rand.NextDouble() * 2.0 * Math.PI;
                Dust.NewDust(new Vector2(npc.Center.X + radius * (float)Math.Cos(angle), npc.Center.Y + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<GreySmoke>(), 0f, 0f, 0, default(Color), 4.5f);
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile projectile = Main.projectile[i];
                    Vector2 DotMatch = projectile.position;
                    DotMatch.Y -= 150;
                    bool CanSee = Collision.CanHitLine(projectile.position, projectile.width, projectile.height, DotMatch, 1, 1);
                    Player player = Main.player[projectile.owner];
                    int GiantMoth = ModContent.ProjectileType<Sweetspot>();
                    int owner = player.whoAmI;
                    if (Main.projectile[i].active && Main.projectile[i].Hitbox.Intersects(projectile.Hitbox) && ((Main.projectile[i].type == GiantMoth && Main.projectile[i].owner == owner)))
                    {
                        if (CanSee)
                        {
                            if (npc.velocity.Y < player.velocity.Y)
                            {
                                player.immuneTime = 30;
                                player.immune = true;
                                player.immuneNoBlink = true;
                                npc.position.Y = (player.position.Y + 50);
                                npc.netUpdate = true;
                            }
                        }
                    }
                }
            }
            if (npc.HasBuff(ModContent.BuffType<MeteorLaunchDebuff>()))
            {
                float radius = (float)Math.Sqrt(Main.rand.Next(1 * 1));
                double angle = Main.rand.NextDouble() * 2.0 * Math.PI;
                Dust.NewDust(new Vector2(npc.Center.X + radius * (float)Math.Cos(angle), npc.Center.Y + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<GreySmoke>(), 0f, 0f, 0, default(Color), 4.5f);
                if (npc.velocity.Y > -10)
                {
                    npc.velocity.Y = -10;
                    npc.netUpdate = true;
                }
            }
            if (Main.netMode == NetmodeID.Server && !npc.boss && !npc.townNPC && npc.lifeMax > 10 && npc.active && npc.friendly == false)
            {
                // Extremely rare chance to play a sound every tick.
                // A chance of 1 in 3600 is once every minute on average (60 ticks/sec * 60 sec).
                if (Main.rand.Next(160000) == 0)
                {
                    int soundIndex = 7;
                    ModPacket packet = Mod.GetPacket();
                    packet.Write((byte)SariaMod.SoundMessageType.PlaySound);
                    packet.Write(npc.whoAmI);
                    packet.Write(soundIndex);
                    packet.Send();
                }
            }
            // For single-player, run the same logic locally.
            else if (Main.netMode == NetmodeID.SinglePlayer && !npc.boss && !npc.townNPC && npc.lifeMax > 10 && npc.active && npc.friendly == false)
            {
                if (Main.rand.Next(160000) == 0)
                {
                    int soundIndex = 7;
                    SariaMod.PlaySound(npc.Center, soundIndex);
                }
            }
        }
        public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            // Draw for EnemyFrozen buff
            if (npc.HasBuff(ModContent.BuffType<EnemyFrozen>()) && visualsAuthorized)
            {
                float progress;
                
                // Different logic for Host/SinglePlayer vs Joining Clients
                // Host/SP: Default to invisible (progress=0) to prevent 1-frame pop on buff application
                // Clients: Default to full size (progress=1) so late-joiners see already-frozen enemies correctly
                if (Main.netMode == NetmodeID.MultiplayerClient)
                {
                    // CLIENT LOGIC: Default to full size for late-joiners
                    // Only animate if the animation is actively in progress
                    if (IceDomeActive && IceDomeTimer < 90)
                    {
                        progress = IceDomeTimer / 90f;
                    }
                    else
                    {
                        // Either animation finished or we're a late-joiner seeing an already-frozen enemy
                        progress = 1f;
                    }
                }
                else
                {
                    // HOST/SINGLEPLAYER LOGIC: Default to invisible to prevent 1-frame pop
                    // 1. Always start with progress = 0 (invisible/small state)
                    progress = 0f;
                    
                    // 2. Only set progress = 1 (full size) if IceDomeActive == true AND IceDomeTimer >= 90
                    if (IceDomeActive && IceDomeTimer >= 90)
                    {
                        progress = 1f;
                    }
                    // 3. Only animate if IceDomeActive == true AND IceDomeTimer > 0
                    else if (IceDomeActive && IceDomeTimer > 0)
                    {
                        progress = IceDomeTimer / 90f;
                    }
                    // else: progress stays 0 (invisible start state)
                }
                
                float currentScale = MathHelper.Lerp(0.01f, (RandomSize * 0.08f) * npc.scale, progress);
                float currentAlpha = MathHelper.Lerp(1f, 0.20f, progress);
                Vector2 startOffset = new Vector2(0f, npc.height / 2f);
                Vector2 currentOffset = Vector2.Lerp(startOffset, new Vector2(0f, -40f), progress);

                DrawBuffEffect(npc, spriteBatch, screenPos, "SariaMod/Items/Sapphire/IceDome", Color.Lerp(drawColor, Color.LightBlue, 0.80f), currentOffset, npc.velocity.X * -0.05f, SpriteEffects.None, currentScale, 1, currentAlpha);
            }
            // Draw effects for MeteorSpikeDebuff
            if (npc.HasBuff(ModContent.BuffType<MeteorSpikeDebuff>()))
            {
                // First Meteor Flow effect
                DrawBuffEffect(npc, spriteBatch, screenPos, "SariaMod/Items/Emerald/MeteorFlow", Color.Lerp(drawColor, Color.Red, 0.3f), new Vector2(-40f, -40f), npc.velocity.X * -0.05f, SpriteEffects.None, npc.scale * 5.7f, 4, 0.70f);
                // Second Meteor Flow effect
                DrawBuffEffect(npc, spriteBatch, screenPos, "SariaMod/Items/Emerald/MeteorFlow", Color.Lerp(drawColor, Color.WhiteSmoke, 2f), new Vector2(-10f, 0f), npc.velocity.X * -0.05f, SpriteEffects.None, npc.scale * 2.7f, 4, 0.50f);
            }
            // Draw effects for MeteorLaunchDebuff
            if (npc.HasBuff(ModContent.BuffType<MeteorLaunchDebuff>()))
            {
                // First Meteor Flow effect
                DrawBuffEffect(npc, spriteBatch, screenPos, "SariaMod/Items/Emerald/MeteorFlow", Color.Lerp(drawColor, Color.Red, 0.3f), new Vector2(-40f, -40f), npc.velocity.X * 0.05f, SpriteEffects.FlipVertically, npc.scale * 5.7f, 4, 0.70f);
                // Second Meteor Flow effect
                DrawBuffEffect(npc, spriteBatch, screenPos, "SariaMod/Items/Emerald/MeteorFlow", Color.Lerp(drawColor, Color.WhiteSmoke, 2f), new Vector2(-10f, 0f), npc.velocity.X * 0.05f, SpriteEffects.FlipVertically, npc.scale * 2.7f, 4, 0.50f);
            }
        }
        // A helper method that now uses screenPos for accurate drawing
        private void DrawBuffEffect(
            NPC npc,
            SpriteBatch spriteBatch,
            Vector2 screenPos,
            string texturePath,
            Color buffColor,
            Vector2 offset,
            float rotation,
            SpriteEffects spriteEffects,
            float scale,
            int verticalFrames,
            float transparentAlpha)
        {
            Texture2D texture = ModContent.Request<Texture2D>(texturePath).Value;
            Vector2 startPos = npc.Center - screenPos + new Vector2(0f, npc.gfxOffY); // Use screenPos here
            int frameHeight = texture.Height / verticalFrames;
            int frameY = (int)Main.GameUpdateCount / 6 % verticalFrames;
            Rectangle rectangle = texture.Frame(verticalFrames: verticalFrames, frameY: frameY);
            Vector2 origin = rectangle.Size() / 2f;
            startPos += offset;
            Color finalDrawColor = Color.Lerp(buffColor, Color.Transparent, transparentAlpha);
            spriteBatch.Draw(texture, startPos, rectangle, finalDrawColor, rotation, origin, scale, spriteEffects, 0f);
        }
        public override void HitEffect(NPC npc, int hitDirection, double damage)
        {
            bool hasFrozenBuff   = npc.HasBuff(ModContent.BuffType<EnemyFrozen>());
            bool wasFrozenByMark = FrozenNPCVisualManager.WasActuallyFrozen(npc.whoAmI);

            if (hasFrozenBuff || wasFrozenByMark)
            {
                if (Main.netMode == NetmodeID.Server)
                {
                    // We don't want Mod.Find<ModGore> to run on servers as it will crash because gores are not loaded on servers
                    npc.buffImmune[ModContent.BuffType<EnemyFrozen>()] = false;
                    return;
                }
                if (npc.life <= 0)
                {
                    int backGoreType = ModContent.GoreType<IceGore1>();
                    int frontGoreType = ModContent.GoreType<IceGore2>();
                    var entitySource = npc.GetSource_Death();
                    npc.DeathSound = SoundID.Item27;
                    float npcSize = Math.Max(npc.width, npc.height);
                    float goreScale = Math.Clamp(npcSize / 44f * 0.6f, 0.24f, 1.5f);
                    for (int i = 0; i < 2; i++)
                    {
                        int g1 = Gore.NewGore(entitySource, npc.position, new Vector2(Main.rand.Next(-6, 7), Main.rand.Next(-6, 7)), backGoreType);
                        if (g1 >= 0 && g1 < Main.maxGore) { Main.gore[g1].scale = goreScale; FrozenGoreSystem.TrackFrozenGore(Main.gore[g1]); }
                        int g2 = Gore.NewGore(entitySource, npc.position, new Vector2(Main.rand.Next(-6, 7), Main.rand.Next(-6, 7)), frontGoreType);
                        if (g2 >= 0 && g2 < Main.maxGore) { Main.gore[g2].scale = goreScale; FrozenGoreSystem.TrackFrozenGore(Main.gore[g2]); }
                    }

                    // Vanilla NPC.HitEffect already ran BEFORE this override, so vanilla death gores
                    // are already in Main.gore[]. Scan by proximity to pick them all up.
                    float radius = Math.Max(npc.width, npc.height) * 2f + 80f;
                    FrozenGoreSystem.TrackGoresNearPosition(npc.Center, radius);
                }
            }
            npc.buffImmune[ModContent.BuffType<EnemyFrozen>()] = false;
            if (hasFrozenBuff)
            {
                // This check is a failsafe, gore is only on clients anyway.
                if (Main.netMode != NetmodeID.Server)
                {
                    int backGoreType = ModContent.GoreType<IceGore2>();
                    for (int G = 0; G < 3; G++)
                    {
                        Gore B = Gore.NewGorePerfect(npc.GetSource_FromThis(), npc.position, new Vector2(Main.rand.Next(-6, 7), Main.rand.Next(-6, 7)), backGoreType, 2f);
                        B.light = .5f;
                        SoundEngine.PlaySound(SoundID.Item27, npc.Center);
                    }
                }
            }
        }
        public override bool PreKill(NPC npc)
        {
            // Allow vanilla death effects for all NPCs
            return true;
        }

        private int GetRandomDeathSoundIndex(NPC npc)
        {
            // Normal death sound logic.
            return Main.rand.Next(5);
        }
        public override void OnKill(NPC npc)
        {
            if (npc.lifeMax <= 25)
            {
                if (Main.netMode == NetmodeID.SinglePlayer)
                {
                    int soundIndex = GetRandomDeathSoundIndex(npc);
                    SariaMod.PlaySound(npc.Center, soundIndex);
                }
                else if (Main.netMode == NetmodeID.Server)
                {
                    int soundIndex = GetRandomDeathSoundIndex(npc);
                    ModPacket packet = Mod.GetPacket();
                    packet.Write((byte)SariaMod.SoundMessageType.PlaySound);
                    packet.Write(npc.whoAmI);
                    packet.Write(soundIndex);
                    packet.Send();
                }
            }
            if (Main.netMode == NetmodeID.Server && !npc.boss && !npc.townNPC && npc.lifeMax > 1 && npc.active && npc.friendly == false)
            {
                // Extremely rare chance to play a sound every tick.
                // A chance of 1 in 3600 is once every minute on average (60 ticks/sec * 60 sec).
                if (Main.rand.Next(800) == 0)
                {
                    int soundIndex = 6;
                    ModPacket packet = Mod.GetPacket();
                    packet.Write((byte)SariaMod.SoundMessageType.PlaySound);
                    packet.Write(npc.whoAmI);
                    packet.Write(soundIndex);
                    packet.Send();
                }
            }
            // For single-player, run the same logic locally.
            else if (Main.netMode == NetmodeID.SinglePlayer && !npc.boss && !npc.townNPC && npc.lifeMax > 1 && npc.active && npc.friendly == false)
            {
                if (Main.rand.Next(800) == 0)
                {
                    int soundIndex = 6;
                    SariaMod.PlaySound(npc.Center, soundIndex);
                }
            }
            if (!npc.SpawnedFromStatue)
            {
                if (Main.rand.Next(50) == 0)
                {
                    Item.NewItem(npc.GetSource_FromThis(), (int)(npc.position.X + 0), (int)(npc.position.Y + 0), 0, 0, ModContent.ItemType<XpPearl>());
                }
                if (Main.rand.Next(70) == 0)
                {
                    Item.NewItem(npc.GetSource_FromThis(), (int)(npc.position.X + 0), (int)(npc.position.Y + 0), 0, 0, ModContent.ItemType<FrozenYogurt>());
                }
                if (Main.rand.Next(150) == 0)
                {
                    Item.NewItem(npc.GetSource_FromThis(), (int)(npc.position.X + 0), (int)(npc.position.Y + 0), 0, 0, ModContent.ItemType<MediumXpPearl>());
                }
                if (Main.rand.Next(300) == 0)
                {
                    Item.NewItem(npc.GetSource_FromThis(), (int)(npc.position.X + 0), (int)(npc.position.Y + 0), 0, 0, ModContent.ItemType<SariasConfect>());
                }
                if (Main.rand.Next(600) == 0)
                {
                    Item.NewItem(npc.GetSource_FromThis(), (int)(npc.position.X + 0), (int)(npc.position.Y + 0), 0, 0, ModContent.ItemType<LargeXpPearl>());
                }
                if (Main.rand.Next(1000) == 0)
                {
                    Item.NewItem(npc.GetSource_FromThis(), (int)(npc.position.X + 0), (int)(npc.position.Y + 0), 0, 0, ModContent.ItemType<SoaringConcoction>());
                }
                if (Main.rand.Next(25000) == 0)
                {
                    Item.NewItem(npc.GetSource_FromThis(), (int)(npc.position.X + 0), (int)(npc.position.Y + 0), 0, 0, ModContent.ItemType<RareXpPearl>());
                }
            }
        }
    }
}
