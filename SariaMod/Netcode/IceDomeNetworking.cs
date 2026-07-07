using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Netcode
{
    public static class IceDomeNetworking
    {
        public const byte PacketId = 251;

        public static void SendPacket(int npcWhoAmI, int randomSize, int excludePlayer)
        {
            // Only the server should send this packet to clients
            if (Main.netMode == NetmodeID.Server)
            {
                ModPacket packet = SariaMod.Instance.GetPacket();
                packet.Write(PacketId);
                packet.Write(npcWhoAmI);
                packet.Write(randomSize);
                packet.Write((byte)excludePlayer);
                packet.Send(-1, excludePlayer); // exclude owner — they already handled it locally
            }
        }

        public static void HandlePacket(BinaryReader reader)
        {
            int npcWhoAmI  = reader.ReadInt32();
            int randomSize = reader.ReadInt32();
            int excludePlayer = reader.ReadByte(); // read but not used client-side; consumed so buffer stays clean

            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                if (npcWhoAmI >= 0 && npcWhoAmI < Main.maxNPCs)
                {
                    NPC npc = Main.npc[npcWhoAmI];
                    if (npc.active)
                    {
                        if (npc.TryGetGlobalNPC(out FairyGlobalNPC fairyNPC))
                        {
                            fairyNPC.StartIceDomeAnimation(randomSize);
                            fairyNPC.visualsAuthorized = true;
                        }
                    }
                }
            }
        }
    }
}
