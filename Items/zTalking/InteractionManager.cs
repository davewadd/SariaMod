using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using SariaMod.Items.Strange;

namespace SariaMod.Items.zTalking
{
    public class InteractionManager
    {
        // Anti-Repeat: Must track a History of the last 3 'Interactive' dialogues.
        private static List<string> interactiveHistory = new List<string>();
        
        // Pool of interactive dialogue IDs
        private static List<string> interactiveDialogues = new List<string> 
        { 
            // "interactive_greeting_1", 
            // "interactive_greeting_2", 
            // "interactive_greeting_3", 
            // "interactive_greeting_4" 
        };
        
        // Track if the current/last session was interactive
        public static bool IsInteractiveSession { get; set; }

        public static bool CanTriggerInteractive(FairyPlayer player)
        {
            return player.totalTalkingTime <= 0 && player.smallTalkingTime <= 0;
        }

        public static string GetRandomInteractiveDialogue()
        {
            // Currently disabled/empty to prevent crashes and logic execution
            return "";
        }

        public static void RegisterInteractiveDialogue(string dialogueId)
        {
            if (interactiveHistory.Contains(dialogueId))
            {
                return;
            }
            
            interactiveHistory.Add(dialogueId);
            if (interactiveHistory.Count > 3)
            {
                interactiveHistory.RemoveAt(0);
            }
        }

        public static bool IsDialogueInHistory(string dialogueId)
        {
            return interactiveHistory.Contains(dialogueId);
        }

        public static void ClearHistory()
        {
            interactiveHistory.Clear();
        }

        public static void UpdateProximityChecks(Projectile saria)
        {
            if (saria == null || !saria.active) return;

            Player player = Main.player[saria.owner];
            FairyPlayer fairyPlayer = player.Fairy();

            // Check conditions: Timers must be 0
            if (fairyPlayer.totalTalkingTime > 0 || fairyPlayer.smallTalkingTime > 0)
                return;

            // Check History: 'forest_sunflower_interaction'
            if (IsDialogueInHistory("forest_sunflower_interaction"))
                return;

            // Scan for Sunflowers (15 tile radius) — owner-side only via Saria's scan method
            Saria sariaProj = saria.ModProjectile as Saria;
            bool sunflowerFound = sariaProj != null && sariaProj.ScanTilesInRadius(TileID.Sunflower, 15);

            if (sunflowerFound)
            {
                // Trigger interaction
                RegisterInteractiveDialogue("forest_sunflower_interaction");
                IsInteractiveSession = true;
                SariaUISystem.DisplayDialogue("forest_sunflower_interaction", saria);
            }
        }
    }
}
