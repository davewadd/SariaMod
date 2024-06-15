using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using SariaMod.Buffs;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.Localization;
using SariaMod;
using SariaMod.Items.Bands;
using System.IO;
using System.Linq;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace SariaMod
{
    public class FairyNetCode : ModPlayer
    {
        public static void HandlePacket(Mod mod, BinaryReader reader, int whoAmI)
        {
            try
            {
                FairyModMessageType msgType = (FairyModMessageType)reader.ReadByte();
                switch (msgType)
                {
                    case FairyModMessageType.LevelSync:
                        Main.player[reader.ReadInt32()].Fairy().HandleSariaLevel(reader);
                        break;
                    case FairyModMessageType.XpBarSync:
                        Main.player[reader.ReadInt32()].Fairy().HandleXpBarLevel(reader);
                        break;
                    default:
                        SariaMod.Instance.Logger.Error($"Failed to parse Calamity packet: No Calamity packet exists with ID {msgType}.");
                        throw new Exception("Failed to parse Calamity packet: Invalid Calamity packet ID.");
                }

            }
            catch (Exception e)
            {
                if (e is EndOfStreamException eose)
                    SariaMod.Instance.Logger.Error("Failed to parse Saria packet: Packet was too short, missing data, or otherwise corrupt.", eose);
                else if (e is ObjectDisposedException ode)
                    SariaMod.Instance.Logger.Error("Failed to parse Saria packet: Packet reader disposed or destroyed.", ode);
                else if (e is IOException ioe)
                    SariaMod.Instance.Logger.Error("Failed to parse Saria packet: An unknown I/O error occurred.", ioe);
                else
                    throw; // this either will crash the game or be caught by TML's packet policing
            }
        }
       
    }
    public enum FairyModMessageType : byte
    {
        LevelSync,
        XpBarSync,
    }
}