using System;
using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.TileGlow
{
    public static class TileHeatNetworking
    {
        public const byte PacketId = 255;

        private const byte SubType_RadiusHeat = 0;
        private const byte SubType_SingleTile = 1;

        public static void SendRadiusHeatPacket(Vector2 worldCenter, float radius, int duration)
        {
            if (Main.netMode == NetmodeID.Server)
            {
                ModPacket packet = SariaMod.Instance.GetPacket();
                packet.Write(PacketId);
                packet.Write(SubType_RadiusHeat);
                packet.Write(worldCenter.X);
                packet.Write(worldCenter.Y);
                packet.Write(radius);
                packet.Write(duration);
                packet.Send();
            }
            else if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                ModPacket packet = SariaMod.Instance.GetPacket();
                packet.Write(PacketId);
                packet.Write(SubType_RadiusHeat);
                packet.Write(worldCenter.X);
                packet.Write(worldCenter.Y);
                packet.Write(radius);
                packet.Write(duration);
                packet.Send();

                TileHeatManager.ApplyHeatInRadius(worldCenter, radius, duration);
            }
        }

        public static void HandlePacket(BinaryReader reader, int whoAmI)
        {
            byte subType = reader.ReadByte();

            switch (subType)
            {
                case SubType_RadiusHeat:
                    HandleRadiusHeatPacket(reader, whoAmI);
                    break;
                case SubType_SingleTile:
                    HandleSingleTilePacket(reader);
                    break;
            }
        }

        private static void HandleRadiusHeatPacket(BinaryReader reader, int whoAmI)
        {
            float centerX = reader.ReadSingle();
            float centerY = reader.ReadSingle();
            float radius = reader.ReadSingle();
            int duration = reader.ReadInt32();

            Vector2 center = new Vector2(centerX, centerY);

            if (Main.netMode == NetmodeID.Server)
            {
                ModPacket packet = SariaMod.Instance.GetPacket();
                packet.Write(PacketId);
                packet.Write(SubType_RadiusHeat);
                packet.Write(centerX);
                packet.Write(centerY);
                packet.Write(radius);
                packet.Write(duration);
                packet.Send(-1, whoAmI);

                if (!Main.dedServ)
                {
                    TileHeatManager.ApplyHeatInRadius(center, radius, duration);
                }
            }
            else if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                TileHeatManager.ApplyHeatInRadius(center, radius, duration);
            }
        }

        private static void HandleSingleTilePacket(BinaryReader reader)
        {
            int x = reader.ReadInt32();
            int y = reader.ReadInt32();
            float distFromCenter = reader.ReadSingle();
            float maxRadius = reader.ReadSingle();
            int duration = reader.ReadInt32();

            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                TileHeatManager.ApplyHeatToTile(x, y, distFromCenter, maxRadius, duration);
            }
        }
    }
}
