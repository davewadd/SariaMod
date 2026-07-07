using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using SariaMod.TileGlow;
using SariaMod.Buffs;

namespace SariaMod.Gores
{
    /// <summary>
    /// Manages the visual frozen effect overlay on NPCs that have been hit by cold attacks
    /// Uses a GlobalNPC to properly tint NPCs during their normal draw cycle, similar to Dangersense
    /// OPTIMIZED: Uses Dictionary for O(1) lookups instead of List with O(n) searches
    /// </summary>
    public class FrozenNPCVisualManager : ModSystem
    {
        private class FrozenNPCData
        {
            public int NPCIndex;
            public int CustomTimer;  // Custom timer that's synced across clients
            public int Duration;
            public bool HasEnemyFrozenBuff;

            public FrozenNPCData(int npcIndex, int customTimer, int duration)
            {
                NPCIndex = npcIndex;
                CustomTimer = customTimer;
                Duration = duration;
                HasEnemyFrozenBuff = false;
            }
        }

        // OPTIMIZATION: Changed from List to Dictionary for O(1) lookups by NPC index
        private static Dictionary<int, FrozenNPCData> markedNPCs = new Dictionary<int, FrozenNPCData>();
        private static List<int> keysToRemove = new List<int>(); // Reusable list for cleanup
        private const int FROZEN_DURATION = 1200; // 20 seconds (60 ticks/sec * 20)

        /// <summary>
        /// Validates that an NPC index is within valid bounds
        /// </summary>
        private static bool IsValidNPCIndex(int npcIndex)
        {
            return npcIndex >= 0 && npcIndex < Main.npc.Length;
        }

        public override void Load()
        {
            On.Terraria.NPC.UpdateNPC += Hook_NPC_UpdateNPC_CheckFrozenStatus;
        }

        public override void Unload()
        {
            On.Terraria.NPC.UpdateNPC -= Hook_NPC_UpdateNPC_CheckFrozenStatus;
            markedNPCs?.Clear();
            markedNPCs = null;
            keysToRemove?.Clear();
            keysToRemove = null;
        }

        /// <summary>
        /// Increment all custom timers every game tick, but only if not currently frozen
        /// This runs once per game tick (60 times per second) instead of every frame
        /// </summary>
        public override void PostUpdateEverything()
        {
            if (Main.dedServ)
                return;

            keysToRemove.Clear();

            // OPTIMIZATION: Iterate dictionary values directly
            foreach (var kvp in markedNPCs)
            {
                int npcIndex = kvp.Key;
                FrozenNPCData data = kvp.Value;

                if (!IsValidNPCIndex(npcIndex))
                {
                    keysToRemove.Add(npcIndex);
                    continue;
                }

                NPC npc = Main.npc[npcIndex];

                // If the NPC is no longer active, clear the mark immediately so a new
                // NPC that reuses this whoAmI slot doesn't inherit the frozen status.
                if (!npc.active)
                {
                    keysToRemove.Add(npcIndex);
                    continue;
                }

                bool hasFrozenBuff = npc.active && npc.HasBuff(ModContent.BuffType<EnemyFrozen>());
                
                // Only increment if not frozen
                if (!hasFrozenBuff && data.CustomTimer < data.Duration)
                {
                    data.CustomTimer++;
                }
                
                // Remove expired entries
                if (data.CustomTimer >= data.Duration)
                {
                    keysToRemove.Add(npcIndex);
                }
            }

            // Remove expired entries
            foreach (int key in keysToRemove)
            {
                markedNPCs.Remove(key);
            }
        }

        /// <summary>
        /// Mark an NPC as frozen locally. For re-freezes sends SyncTimer only.
        /// Does NOT send FreezeNPC — call FrozenNPCNetworking.SendFreezeNPC explicitly at weapon sites.
        /// </summary>
        public static void MarkNPCAsFrozen(int npcIndex)
        {
            if (!IsValidNPCIndex(npcIndex))
                return;

            if (markedNPCs.TryGetValue(npcIndex, out FrozenNPCData existing))
            {
                // Re-freeze: reset timer locally only.
                // Hook_NPC_UpdateNPC_CheckFrozenStatus resets this every frame on all clients
                // while the buff is active, so no packet needed here.
                existing.CustomTimer = 0;
                existing.HasEnemyFrozenBuff = true;
                return;
            }

            // First-time freeze: add locally only — packet was already sent upstream by the weapon site
            markedNPCs[npcIndex] = new FrozenNPCData(npcIndex, 0, FROZEN_DURATION) { HasEnemyFrozenBuff = true };
        }

        /// <summary>
        /// Apply frozen mark locally only — NO network send.
        /// Called when receiving a FreezeNPC packet on a client.
        /// </summary>
        public static void MarkNPCAsFrozenLocal(int npcIndex)
        {
            if (!IsValidNPCIndex(npcIndex))
                return;

            if (markedNPCs.TryGetValue(npcIndex, out FrozenNPCData existing))
            {
                existing.CustomTimer = 0;
                existing.HasEnemyFrozenBuff = true;
                return;
            }

            markedNPCs[npcIndex] = new FrozenNPCData(npcIndex, 0, FROZEN_DURATION) { HasEnemyFrozenBuff = true };
        }

        /// <summary>
        /// Mark an NPC as frozen for late joiners - called when receiving NPC sync data.
        /// Does not send network packets to avoid infinite loops.
        /// </summary>
        public static void MarkNPCAsFrozenForLateJoiner(int npcIndex)
        {
            if (!IsValidNPCIndex(npcIndex))
                return;

            if (markedNPCs.TryGetValue(npcIndex, out FrozenNPCData existing))
            {
                existing.CustomTimer = 0;
                existing.HasEnemyFrozenBuff = true;
                return;
            }

            markedNPCs[npcIndex] = new FrozenNPCData(npcIndex, 0, FROZEN_DURATION) { HasEnemyFrozenBuff = true };
        }

        /// <summary>
        /// Sync frozen timer from network packet (SyncTimer sub-type).
        /// </summary>
        public static void SyncFrozenTimer(int npcIndex, int timerValue)
        {
            if (!IsValidNPCIndex(npcIndex))
                return;

            if (markedNPCs.TryGetValue(npcIndex, out FrozenNPCData existing))
            {
                existing.CustomTimer = timerValue;
                existing.HasEnemyFrozenBuff = true;
                return;
            }

            markedNPCs[npcIndex] = new FrozenNPCData(npcIndex, timerValue, FROZEN_DURATION) { HasEnemyFrozenBuff = true };
        }

        /// <summary>
        /// Check if an NPC is currently marked as frozen
        /// OPTIMIZATION: O(1) dictionary lookup instead of O(n) list search
        /// </summary>
        public static bool IsNPCFrozen(int npcIndex)
        {
            if (!IsValidNPCIndex(npcIndex))
                return false;

            // OPTIMIZATION: O(1) dictionary lookup
            if (markedNPCs.TryGetValue(npcIndex, out FrozenNPCData data))
            {
                if (data.CustomTimer < data.Duration)
                {
                    return true;
                }
                else
                {
                    markedNPCs.Remove(npcIndex);
                }
            }
            return false;
        }

        /// <summary>
        /// Check if an NPC is currently marked as frozen AND actually had the EnemyFrozen buff.
        /// Use this for gore spawn eligibility (mark-only NPCs should not spawn frozen gores).
        /// </summary>
        public static bool WasActuallyFrozen(int npcIndex)
        {
            if (!IsValidNPCIndex(npcIndex))
                return false;

            if (markedNPCs.TryGetValue(npcIndex, out FrozenNPCData data))
                return data.CustomTimer < data.Duration && data.HasEnemyFrozenBuff;

            return false;
        }

        /// <summary>
        /// Get progress (0-1) of frozen effect duration using custom timer
        /// </summary>
        public static float GetFrozenProgress(int npcIndex)
        {
            if (!IsValidNPCIndex(npcIndex))
                return 0f;

            // OPTIMIZATION: O(1) dictionary lookup
            if (markedNPCs.TryGetValue(npcIndex, out FrozenNPCData data))
            {
                return data.CustomTimer / (float)data.Duration;
            }
            return 0f;
        }

        /// <summary>
        /// Get the frozen tint color for an NPC (used by GlobalNPC.DrawEffects)
        /// Returns null if NPC is not marked or effect has expired
        /// Returns a FULL OVERRIDE color (not minimums) to ensure it works at all light levels
        /// and overrides Hunter Potion effect
        /// </summary>
        public static Color? GetFrozenTintColor(int npcIndex)
        {
            if (!IsValidNPCIndex(npcIndex))
                return null;

            // OPTIMIZATION: O(1) dictionary lookup
            if (markedNPCs.TryGetValue(npcIndex, out FrozenNPCData data) && data.CustomTimer < data.Duration)
            {
                float progress = data.CustomTimer / (float)data.Duration;
                
                // Calculate fade multiplier (1.0 at start, 0.0 at end)
                float fadeMultiplier = 1f - progress;
                
                // Return full override color values for a vivid icy blue
                // Saturated ice blue: low R, mid G, full B
                byte targetR = 80;   // Low red for vivid blue
                byte targetG = 160;  // Mid green for cyan lean
                byte targetB = 255;  // Maximum blue

                // Alpha represents how strongly to apply the effect (fades over time)
                byte blendAmount = (byte)(fadeMultiplier * 255);
                
                return new Color(targetR, targetG, targetB, blendAmount);
            }
            return null;
        }

        /// <summary>
        /// Remove frozen mark from an NPC
        /// </summary>
        public static void UnmarkNPC(int npcIndex)
        {
            markedNPCs.Remove(npcIndex);
        }

        /// <summary>
        /// Hook to continuously reset timer while NPC has frozen buff
        /// OPTIMIZATION: Only updates NPCs that are actually frozen, O(1) lookup
        /// </summary>
        private void Hook_NPC_UpdateNPC_CheckFrozenStatus(On.Terraria.NPC.orig_UpdateNPC orig, NPC self, int i)
        {
            orig(self, i);

            bool hasFrozenBuff = self.HasBuff(ModContent.BuffType<EnemyFrozen>());
            
            // If NPC has frozen buff, reset its timer to 0 every frame
            if (hasFrozenBuff)
            {
                // OPTIMIZATION: O(1) dictionary lookup
                if (markedNPCs.TryGetValue(self.whoAmI, out FrozenNPCData data))
                {
                    data.CustomTimer = 0;
                    data.HasEnemyFrozenBuff = true;
                }
                else
                {
                    // If not already marked, mark it now
                    markedNPCs[self.whoAmI] = new FrozenNPCData(self.whoAmI, 0, FROZEN_DURATION) { HasEnemyFrozenBuff = true };
                }
            }
            else
            {
                // NPC no longer has frozen buff - stop resetting the timer
                if (markedNPCs.TryGetValue(self.whoAmI, out FrozenNPCData data))
                {
                    data.HasEnemyFrozenBuff = false;
                }
            }
        }

        /// <summary>
        /// Clear marked NPCs when world loads to prevent stuck states
        /// </summary>
        public override void OnWorldLoad()
        {
            ClearAllMarkedNPCs();
        }

        /// <summary>
        /// Clear marked NPCs when leaving world to prevent persistence
        /// </summary>
        public override void OnWorldUnload()
        {
            ClearAllMarkedNPCs();
        }

        /// <summary>
        /// Clear marked NPCs before the world is saved and quit
        /// </summary>
        public override void PreSaveAndQuit()
        {
            ClearAllMarkedNPCs();
        }

        /// <summary>
        /// Clear all marked NPCs
        /// </summary>
        public static void ClearAllMarkedNPCs()
        {
            markedNPCs?.Clear();
        }
    }
}
