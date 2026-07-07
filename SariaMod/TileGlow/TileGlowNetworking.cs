using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.TileGlow
{
    public static class TileGlowNetworking
    {
        public const byte PacketId = 247;
        
        // Sub-packet types
        private const byte SubType_RadiusGlow = 0;
        private const byte SubType_SingleTile = 1;
        
        /// <summary>
        /// Send a radius glow packet from server to all clients
        /// Called when a ColdWaveHitBox affects tiles
        /// </summary>
        public static void SendRadiusGlowPacket(Vector2 worldCenter, float radius, int duration)
        {
            if (Main.netMode == NetmodeID.Server)
            {
                ModPacket packet = SariaMod.Instance.GetPacket();
                packet.Write(PacketId);
                packet.Write(SubType_RadiusGlow);
                packet.Write(worldCenter.X);
                packet.Write(worldCenter.Y);
                packet.Write(radius);
                packet.Write(duration);
                packet.Send();
            }
            else if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                // Client sends to server and also applies locally immediately
                ModPacket packet = SariaMod.Instance.GetPacket();
                packet.Write(PacketId);
                packet.Write(SubType_RadiusGlow);
                packet.Write(worldCenter.X);
                packet.Write(worldCenter.Y);
                packet.Write(radius);
                packet.Write(duration);
                packet.Send();
                
                // Apply locally on the owner client immediately
                TileGlowManager.ApplyGlowInRadius(worldCenter, radius, duration);
            }
        }
        
        /// <summary>
        /// Handle incoming tile glow packets
        /// </summary>
        public static void HandlePacket(BinaryReader reader, int whoAmI)
        {
            byte subType = reader.ReadByte();
            
            switch (subType)
            {
                case SubType_RadiusGlow:
                    HandleRadiusGlowPacket(reader, whoAmI);
                    break;
                case SubType_SingleTile:
                    HandleSingleTilePacket(reader);
                    break;
            }
        }
        
        private static void HandleRadiusGlowPacket(BinaryReader reader, int whoAmI)
        {
            float centerX = reader.ReadSingle();
            float centerY = reader.ReadSingle();
            float radius = reader.ReadSingle();
            int duration = reader.ReadInt32();
            
            Vector2 center = new Vector2(centerX, centerY);
            
            if (Main.netMode == NetmodeID.Server)
            {
                // Server received from client, broadcast to all other clients
                ModPacket packet = SariaMod.Instance.GetPacket();
                packet.Write(PacketId);
                packet.Write(SubType_RadiusGlow);
                packet.Write(centerX);
                packet.Write(centerY);
                packet.Write(radius);
                packet.Write(duration);
                packet.Send(-1, whoAmI); // Send to all except sender
                
                // In host-and-play mode, the server also renders, so apply locally
                if (!Main.dedServ)
                {
                    TileGlowManager.ApplyGlowInRadius(center, radius, duration);
                }
            }
            else if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                // Client received from server, apply the glow locally
                TileGlowManager.ApplyGlowInRadius(center, radius, duration);
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
                TileGlowManager.ApplyGlowToTile(x, y, distFromCenter, maxRadius, duration);
            }
        }
    }
}
