using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using SariaMod.Buffs;

namespace SariaMod.Netcode
{
    /// <summary>
    /// Handles networking for syncing player debuff/incapacitated states across multiplayer.
    /// This ensures all clients see when a player is frozen by the Veil buff or other effects.
    /// </summary>
    public static class PlayerDebuffSyncNetworking
    {
        // PacketId 248 - smaller than existing ones (249=Fire, 250=Saria, 251=IceDome, 252=FrozenGore/TileGlow, 253=FrozenTimer, 254=Hookshot)
        public const byte PacketId = 248;

        /// <summary>
        /// Sub-packet types for different sync operations
        /// </summary>
        private enum SubPacketType : byte
        {
            VeilFreezeState = 0,    // Sync Veil buff freeze state
            PlayerFrozenState = 1,  // Sync player.frozen directly
        }

        #region Send Methods

        /// <summary>
        /// Send the Veil freeze state to all other clients.
        /// Call this when a player's Veil freeze state changes.
        /// </summary>
        public static void SendVeilFreezeState(int playerIndex, bool isFrozen)
        {
            if (Main.netMode == NetmodeID.SinglePlayer) return;

            ModPacket packet = SariaMod.Instance.GetPacket();
            packet.Write(PacketId);
            packet.Write((byte)SubPacketType.VeilFreezeState);
            packet.Write((byte)playerIndex);
            packet.Write(isFrozen);

            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                // Client sends to server, server will rebroadcast
                packet.Send();
            }
            else if (Main.netMode == NetmodeID.Server)
            {
                // Server sends to all clients
                packet.Send(-1, playerIndex);
            }
        }

        /// <summary>
        /// Send the player.frozen state directly to ensure all clients sync.
        /// Call this when player.frozen changes due to any effect.
        /// </summary>
        public static void SendPlayerFrozenState(int playerIndex, bool isFrozen)
        {
            if (Main.netMode == NetmodeID.SinglePlayer) return;

            ModPacket packet = SariaMod.Instance.GetPacket();
            packet.Write(PacketId);
            packet.Write((byte)SubPacketType.PlayerFrozenState);
            packet.Write((byte)playerIndex);
            packet.Write(isFrozen);

            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                // Client sends to server, server will rebroadcast
                packet.Send();
            }
            else if (Main.netMode == NetmodeID.Server)
            {
                // Server sends to all clients
                packet.Send(-1, playerIndex);
            }
        }

        #endregion

        #region Handle Methods

        /// <summary>
        /// Handle incoming packets - called from SariaMod.HandlePacket
        /// </summary>
        public static void HandlePacket(BinaryReader reader, int whoAmI)
        {
            SubPacketType subType = (SubPacketType)reader.ReadByte();

            switch (subType)
            {
                case SubPacketType.VeilFreezeState:
                    HandleVeilFreezeState(reader, whoAmI);
                    break;
                case SubPacketType.PlayerFrozenState:
                    HandlePlayerFrozenState(reader, whoAmI);
                    break;
            }
        }

        private static void HandleVeilFreezeState(BinaryReader reader, int whoAmI)
        {
            int playerIndex = reader.ReadByte();
            bool isFrozen = reader.ReadBoolean();

            // Validate player index
            if (playerIndex < 0 || playerIndex >= Main.maxPlayers)
                return;

            Player player = Main.player[playerIndex];
            if (!player.active)
                return;

            // Get the VeilPlayer and update its state
            VeilPlayer veilPlayer = player.GetModPlayer<VeilPlayer>();
            veilPlayer.veilFreezeState = isFrozen ? 1 : 0;

            // Also apply frozen state directly
            if (isFrozen)
            {
                player.frozen = true;
            }

            // If server received this, rebroadcast to all other clients
            if (Main.netMode == NetmodeID.Server)
            {
                ModPacket packet = SariaMod.Instance.GetPacket();
                packet.Write(PacketId);
                packet.Write((byte)SubPacketType.VeilFreezeState);
                packet.Write((byte)playerIndex);
                packet.Write(isFrozen);
                packet.Send(-1, whoAmI); // Send to all except the sender
            }
        }

        private static void HandlePlayerFrozenState(BinaryReader reader, int whoAmI)
        {
            int playerIndex = reader.ReadByte();
            bool isFrozen = reader.ReadBoolean();

            // Validate player index
            if (playerIndex < 0 || playerIndex >= Main.maxPlayers)
                return;

            Player player = Main.player[playerIndex];
            if (!player.active)
                return;

            // Apply frozen state directly
            player.frozen = isFrozen;

            // If server received this, rebroadcast to all other clients
            if (Main.netMode == NetmodeID.Server)
            {
                ModPacket packet = SariaMod.Instance.GetPacket();
                packet.Write(PacketId);
                packet.Write((byte)SubPacketType.PlayerFrozenState);
                packet.Write((byte)playerIndex);
                packet.Write(isFrozen);
                packet.Send(-1, whoAmI); // Send to all except the sender
            }
        }

        #endregion
    }
}
