using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using System.Linq;
using SariaMod.Items.Strange;

namespace SariaMod.Items.zTalking
{
    internal sealed class SariaInteractionTrackerPlayer : ModPlayer
    {
        private const string CompletedKey = "SariaMod:CompletedInteractions";
        private const string TotalKey = "SariaMod:TotalCompletedInteractions";
        private const string LostKey = "SariaMod:LostInteractions";
        private const string PendingKey = "SariaMod:PendingCutscenes";

        public readonly HashSet<int> CompletedInteractions = new();
        public readonly HashSet<int> LostInteractions = new();
        public readonly HashSet<string> CompletedCutsceneIDs = new(); // Track completed cutscenes by string ID
        public List<PendingCutscene> PendingCutscenes = new();

        public int TotalCompletedInteractions { get; private set; }

        public bool IsCompleted(int id) => CompletedInteractions.Contains(id);
        public bool IsLost(int id) => LostInteractions.Contains(id);
        public bool IsCutsceneCompleted(string id) => CompletedCutsceneIDs.Contains(id);

        public bool TryMarkCompleted(int id)
        {
            if (id <= 0) return false;
            if (LostInteractions.Contains(id)) return false;
            if (!CompletedInteractions.Add(id)) return false;

            TotalCompletedInteractions++;
            return true;
        }

        public bool TryMarkLost(int id)
        {
            if (id <= 0) return false;
            if (CompletedInteractions.Contains(id)) return false;
            return LostInteractions.Add(id);
        }

        public void AddPendingCutscene(string id, string targetNode, string buttonText, double durationMinutes, string conditionId)
        {
            // Don't add if already completed
            if (CompletedCutsceneIDs.Contains(id)) return;

            // Remove existing if any (refresh)
            PendingCutscenes.RemoveAll(p => p.ID == id);
            PendingCutscenes.Add(new PendingCutscene(id, targetNode, buttonText, durationMinutes, conditionId));
        }

        public PendingCutscene GetBestAvailableCutscene()
        {
            // Find all cutscenes where the condition is currently met
            var available = PendingCutscenes.Where(p => CheckCondition(p.ConditionID));
            
            // Return the one expiring soonest (lowest RemainingTime)
            return available.OrderBy(p => p.RemainingTime).FirstOrDefault();
        }

        public bool HasAnyPendingCutscenes()
        {
            return PendingCutscenes.Count > 0;
        }

        public void RemovePendingCutscene(string id)
        {
            PendingCutscenes.RemoveAll(p => p.ID == id);
        }

        public void CompletePendingCutscene(string id)
        {
            if (PendingCutscenes.RemoveAll(p => p.ID == id) > 0)
            {
                CompletedCutsceneIDs.Add(id);
            }
        }

        /// <summary>
        /// Resets a completed cutscene so it can be triggered again.
        /// Useful for testing. Call this from a debug item or command.
        /// </summary>
        public void ResetCutscene(string id)
        {
            CompletedCutsceneIDs.Remove(id);
            // Note: This doesn't re-add it to pending. The trigger logic in Saria.cs needs to run again.
        }

        public void ResetAllInteractions()
        {
            // Explicitly only reset integer-based interactions (interactive dialogues).
            // Cutscenes use string IDs (CompletedCutsceneIDs) and are NOT reset here.
            CompletedInteractions.Clear();
            LostInteractions.Clear();
            TotalCompletedInteractions = 0;
        }

        public override void PostUpdate()
        {
            // Update timers
            for (int i = PendingCutscenes.Count - 1; i >= 0; i--)
            {
                PendingCutscenes[i].RemainingTime--;
                if (PendingCutscenes[i].RemainingTime <= 0)
                {
                    PendingCutscenes.RemoveAt(i);
                }
            }
        }

        public bool CheckCondition(string conditionId)
        {
            if (string.IsNullOrEmpty(conditionId)) return true;

            // Example Condition Logic
            // Format: "NotForm_X" or "NotInBiome_Hallow"
            
            if (conditionId.StartsWith("NotForm_"))
            {
                if (int.TryParse(conditionId.Substring(8), out int formId))
                {
                    // Find Saria projectile to check form
                    var saria = Main.projectile.FirstOrDefault(p => p.active && p.owner == Player.whoAmI && p.type == ModContent.ProjectileType<Saria>())?.ModProjectile as Saria;
                    if (saria != null)
                    {
                        return saria.Transform != formId;
                    }
                }
            }

            if (conditionId.StartsWith("Form_"))
            {
                if (int.TryParse(conditionId.Substring(5), out int formId))
                {
                    var saria = Main.projectile.FirstOrDefault(p => p.active && p.owner == Player.whoAmI && p.type == ModContent.ProjectileType<Saria>())?.ModProjectile as Saria;
                    if (saria != null)
                    {
                        return saria.Transform == formId;
                    }
                }
            }
            
            if (conditionId == "NotInHallow")
            {
                return !Player.ZoneHallow;
            }

            if (conditionId == "InHallow")
            {
                return Player.ZoneHallow;
            }

            // Check if another cutscene has been completed
            // Format: "Completed_CutsceneID"
            if (conditionId.StartsWith("Completed_"))
            {
                string requiredId = conditionId.Substring(10);
                return CompletedCutsceneIDs.Contains(requiredId);
            }

            // Add more conditions as needed
            return true;
        }

        public override void SaveData(TagCompound tag)
        {
            tag[CompletedKey] = new List<int>(CompletedInteractions);
            tag[LostKey] = new List<int>(LostInteractions);
            tag[TotalKey] = TotalCompletedInteractions;
            tag[PendingKey] = PendingCutscenes.Select(p => p.Save()).ToList();
            tag["SariaMod:CompletedCutsceneIDs"] = new List<string>(CompletedCutsceneIDs);
        }

        public override void LoadData(TagCompound tag)
        {
            CompletedInteractions.Clear();
            LostInteractions.Clear();
            PendingCutscenes.Clear();
            CompletedCutsceneIDs.Clear();

            if (tag.ContainsKey(CompletedKey))
            {
                foreach (int id in tag.GetList<int>(CompletedKey))
                    CompletedInteractions.Add(id);
            }

            if (tag.ContainsKey(LostKey))
            {
                foreach (int id in tag.GetList<int>(LostKey))
                    LostInteractions.Add(id);
            }

            if (tag.ContainsKey(PendingKey))
            {
                var list = tag.GetList<TagCompound>(PendingKey);
                foreach (var item in list)
                {
                    PendingCutscenes.Add(PendingCutscene.Load(item));
                }
            }

            if (tag.ContainsKey("SariaMod:CompletedCutsceneIDs"))
            {
                foreach (string id in tag.GetList<string>("SariaMod:CompletedCutsceneIDs"))
                    CompletedCutsceneIDs.Add(id);
            }

            TotalCompletedInteractions = tag.ContainsKey(TotalKey) ? tag.GetInt(TotalKey) : CompletedInteractions.Count;
            if (TotalCompletedInteractions < CompletedInteractions.Count)
                TotalCompletedInteractions = CompletedInteractions.Count;
        }
    }
}
