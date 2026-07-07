using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using SariaMod.Items.Bands;

namespace SariaMod.Netcode.HookshotNetworking
{
    /// <summary>
    /// GlobalNPC that tracks freeze state for NPCs grabbed by hookshot/longshot combat mode.
    /// This allows all clients to locally enforce the freeze without constant position syncing.
    /// 
    /// The freeze system works as follows:
    /// 1. Owner hooks an enemy and sends FreezeStart packet (once)
    /// 2. All clients mark the NPC as frozen and store the freeze position
    /// 3. Every frame, all clients locally enforce velocity=0 and position=frozenPosition
    /// 4. Owner sends FreezeEnd packet when releasing (once)
    /// 5. All clients clear the freeze state
    /// 
    /// This minimizes network traffic to just 2 packets per hookshot grab instead of 60+ per second.
    /// </summary>
    public class HookshotFreezeGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        /// <summary>
        /// Whether this NPC is currently frozen by a hookshot
        /// </summary>
        public bool isFrozenByHookshot = false;

        /// <summary>
        /// The player whoAmI who froze this NPC (for authority checking)
        /// </summary>
        public int frozenByPlayerIndex = -1;

        /// <summary>
        /// The position where the NPC should be frozen at
        /// </summary>
        public Vector2 frozenPosition = Vector2.Zero;

        /// <summary>
        /// Timer to track how long the NPC has been frozen (for timeout safety)
        /// </summary>
        public int freezeTimer = 0;

        /// <summary>
        /// Maximum freeze duration in frames (extended for testing - normally 180 = 3 seconds)
        /// Set to 3600 frames = 60 seconds (1 minute) for testing
        /// </summary>
        public const int MaxFreezeDuration = 3600; // TESTING: Extended to 1 minute (60 * 60 frames)

        public override void ResetEffects(NPC npc)
        {
            // Don't reset freeze state here - it persists until explicitly cleared
        }

        public override void PostAI(NPC npc)
        {
            // OPTIMIZATION: Skip NPCs that aren't frozen - no processing needed
            if (!isFrozenByHookshot)
                return;

            // Check if NPC died while frozen - auto-clear freeze state
            if (npc.life <= 0 || !npc.active)
            {
                
                
                // Clear player freeze state if they were the one who froze this NPC
                if (frozenByPlayerIndex >= 0 && frozenByPlayerIndex < Main.maxPlayers)
                {
                    Player freezer = Main.player[frozenByPlayerIndex];
                    if (freezer.active)
                    {
                        HookshotPlayer modPlayer = freezer.GetModPlayer<HookshotPlayer>();
                        if (modPlayer.hookedNPCIndex == npc.whoAmI)
                        {
                            modPlayer.ClearPlayerFreeze();
                            modPlayer.hookedNPCIndex = -1;
                        }
                    }
                }
                
                ClearFreeze(npc);
                return;
            }

            freezeTimer++;

            // Safety timeout - clear freeze if it's been too long (prevents stuck NPCs)
            if (freezeTimer > MaxFreezeDuration)
            {
                ClearFreeze(npc);
                return;
            }

            // Enforce freeze - set velocity to zero and snap to frozen position
            npc.velocity = Vector2.Zero;
            
            // Only snap position if we have a valid frozen position
            if (frozenPosition != Vector2.Zero)
            {
                // Check if position drifted significantly (more than 2 pixels)
                float drift = Vector2.Distance(npc.Center, frozenPosition);
                if (drift > 2f)
                {
                    // Snap back to frozen position
                    npc.Center = frozenPosition;
                    
                    
                }
            }

            
        }

        /// <summary>
        /// Start freezing this NPC at its current position
        /// </summary>
        public void StartFreeze(NPC npc, int playerIndex)
        {
            isFrozenByHookshot = true;
            frozenByPlayerIndex = playerIndex;
            frozenPosition = npc.Center;
            freezeTimer = 0;

            // Immediately stop the NPC
            npc.velocity = Vector2.Zero;

           
        }

        /// <summary>
        /// Clear the freeze state for this NPC
        /// </summary>
        public void ClearFreeze(NPC npc)
        {
            

            isFrozenByHookshot = false;
            frozenByPlayerIndex = -1;
            frozenPosition = Vector2.Zero;
            freezeTimer = 0;
        }

        /// <summary>
        /// Check if this NPC is frozen by a specific player
        /// </summary>
        public bool IsFrozenByPlayer(int playerIndex)
        {
            return isFrozenByHookshot && frozenByPlayerIndex == playerIndex;
        }
    }
}
