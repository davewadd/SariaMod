using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using SariaMod.Netcode;

namespace SariaMod.Buffs
{
    /// <summary>
    /// ModPlayer to track Veil buff freeze state per-player.
    /// This is necessary because ModBuff instances are shared across all players,
    /// so we can't use instance variables in the buff itself.
    /// </summary>
    public class VeilPlayer : ModPlayer
    {
        /// <summary>
        /// 0 = not frozen by Veil, 1 = frozen by Veil
        /// </summary>
        public int veilFreezeState = 0;
        
        /// <summary>
        /// Tracks previous freeze state for detecting changes
        /// </summary>
        private int previousFreezeState = 0;
        
        /// <summary>
        /// Whether the player has the Veil buff active
        /// </summary>
        public bool hasVeilBuff = false;

        public override void ResetEffects()
        {
            // Reset hasVeilBuff - it will be set to true by the buff's Update method
            hasVeilBuff = false;
        }

        public override void PostUpdate()
        {
            // If player no longer has Veil buff, reset freeze state
            if (!hasVeilBuff && veilFreezeState != 0)
            {
                veilFreezeState = 0;
                
                // Sync the unfrozen state
                if (Main.myPlayer == Player.whoAmI && Main.netMode != NetmodeID.SinglePlayer)
                {
                    PlayerDebuffSyncNetworking.SendVeilFreezeState(Player.whoAmI, false);
                }
            }
            
            // Detect freeze state changes and sync
            if (Main.myPlayer == Player.whoAmI)
            {
                if (veilFreezeState != previousFreezeState)
                {
                    if (Main.netMode != NetmodeID.SinglePlayer)
                    {
                        PlayerDebuffSyncNetworking.SendVeilFreezeState(Player.whoAmI, veilFreezeState > 0);
                    }
                    previousFreezeState = veilFreezeState;
                }
            }
            
            // Apply frozen state if Veil freeze is active
            if (veilFreezeState > 0)
            {
                Player.frozen = true;
            }
        }

        public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
        {
            // Only sync Veil freeze state if the player is actually frozen - avoids spamming on world join
            if (Main.netMode == NetmodeID.Server && veilFreezeState > 0)
            {
                PlayerDebuffSyncNetworking.SendVeilFreezeState(Player.whoAmI, true);
            }
        }
    }
}
