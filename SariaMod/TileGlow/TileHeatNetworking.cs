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
        private const byte SubType_BeamImpact = 2;

        public static void SendRadiusHeatPacket(Vector2 worldCenter, float radius, int duration, int owner = -1, int damage = 0)
        {
            if (Main.netMode == NetmodeID.Server)
            {
                TileHeatManager.ApplyHeatInRadius(worldCenter, radius, duration, owner, damage);

                ModPacket packet = SariaMod.Instance.GetPacket();
                packet.Write(PacketId);
                packet.Write(SubType_RadiusHeat);
                packet.Write(worldCenter.X);
                packet.Write(worldCenter.Y);
                packet.Write(radius);
                packet.Write(duration);
                packet.Write(owner);
                packet.Write(damage);
                packet.Send();
            }
            else if (Main.netMode == NetmodeID.SinglePlayer)
            {
                TileHeatManager.ApplyHeatInRadius(worldCenter, radius, duration, owner, damage);
            }
        }

        public static void SendBeamImpactPacket(int tileX, int tileY, int duration, int owner, int damage)
        {
            if (Main.netMode != NetmodeID.Server)
                return;

            ModPacket packet = SariaMod.Instance.GetPacket();
            packet.Write(PacketId);
            packet.Write(SubType_BeamImpact);
            packet.Write(tileX);
            packet.Write(tileY);
            packet.Write(duration);
            packet.Write(owner);
            packet.Write(damage);
            packet.Send();
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
                case SubType_BeamImpact:
                    HandleBeamImpactPacket(reader);
                    break;
            }
        }

        private static void HandleRadiusHeatPacket(BinaryReader reader, int whoAmI)
        {
            float centerX = reader.ReadSingle();
            float centerY = reader.ReadSingle();
            float radius = reader.ReadSingle();
            int duration = reader.ReadInt32();
            int owner = reader.ReadInt32();
            int damage = reader.ReadInt32();

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
                packet.Write(owner);
                packet.Write(damage);
                packet.Send(-1, whoAmI);

                if (!Main.dedServ)
                {
                    TileHeatManager.ApplyHeatInRadius(center, radius, duration, owner, damage);
                }
            }
            else if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                TileHeatManager.ApplyHeatInRadius(center, radius, duration, owner, damage);
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

        private static void HandleBeamImpactPacket(BinaryReader reader)
        {
            int tileX = reader.ReadInt32();
            int tileY = reader.ReadInt32();
            int duration = reader.ReadInt32();
            int owner = reader.ReadInt32();
            int damage = reader.ReadInt32();

            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                TileHeatManager.ApplyBeamImpact(tileX, tileY, duration, owner, damage);
            }
        }
    }
}
