using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using SariaMod.Items.Strange;

namespace SariaMod.Netcode
{
    public static class PsychicFieldNetworking
    {
        public const byte PacketId = 253;

        public static void RequestFieldSummon(Projectile sariaProjectile, Projectile chargeProjectile, Vector2 fieldPosition)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                return;
            }

            ModPacket packet = SariaMod.Instance.GetPacket();
            packet.Write(PacketId);
            packet.Write(sariaProjectile.identity);
            packet.Write(chargeProjectile.identity);
            packet.Write(fieldPosition.X);
            packet.Write(fieldPosition.Y);
            packet.Send();
        }

        public static void HandlePacket(BinaryReader reader, int whoAmI)
        {
            int sariaIdentity = reader.ReadInt32();
            int chargeIdentity = reader.ReadInt32();
            float positionX = reader.ReadSingle();
            float positionY = reader.ReadSingle();

            if (Main.netMode == NetmodeID.Server)
            {
                PsychicFieldSystem.TrySummonFieldFromNetwork(
                    whoAmI,
                    sariaIdentity,
                    chargeIdentity,
                    new Vector2(positionX, positionY));
            }
        }
    }
}
