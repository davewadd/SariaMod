using Microsoft.Xna.Framework;
using System.Linq;
using System.Reflection;
using System.Text;
using Terraria;
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.GameContent.Events;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ID;
using System;
using System.IO;
using Terraria.ModLoader;
using Terraria.ObjectData;
using SariaMod.Items;
using SariaMod.Buffs;
using SariaMod.Items.Strange;
using SariaMod.Dusts;
using SariaMod.Items.Amber;
using SariaMod.Items.Amethyst;
using SariaMod.Items.Bands;
using SariaMod.Items.Emerald;
using SariaMod.Items.Ruby;
using SariaMod.Items.Sapphire;
using SariaMod.Items.Topaz;
using SariaMod.Items.zPearls;
using SariaMod.Items.zTalking;
using Terraria.Localization;
using Terraria.Audio;
using Terraria.UI;
using SariaMod;
using Terraria.GameContent.UI.Elements;
using ReLogic.Content;
using Terraria.DataStructures;
namespace SariaMod
{
    public static class MiscUtilities
    {
        public static void StartSandstorm()
        {
            typeof(Sandstorm).GetMethod("StartSandstorm", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
        }

        public static void SendPacket(Player player, ModPacket packet, bool server)
        {
            // Client: Send the packet only to the host.
            if (!server)
                packet.Send();
            // Server: Send the packet to every OTHER client.
            else
                packet.Send(-1, player.whoAmI);
        }

        internal static void SetUpCandle(ModTile mt, bool lavaImmune = false, int offset = -4)
        {
            Main.tileLighted[mt.Type] = true;
            Main.tileFrameImportant[mt.Type] = true;
            Main.tileLavaDeath[mt.Type] = !lavaImmune;
            Main.tileWaterDeath[mt.Type] = false;
            TileObjectData.newTile.CopyFrom(TileObjectData.StyleOnTable1x1);
            TileObjectData.newTile.CoordinateHeights = new int[1] { 20 };
            TileObjectData.newTile.LavaDeath = !lavaImmune;
            TileObjectData.newTile.DrawYOffset = offset;
            TileObjectData.addTile(mt.Type);
            mt.AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTorch);
        }

        public static void StopSandstorm()
        {
            Sandstorm.Happening = false;
        }

        public static FairyPlayer Fairy(Player player)
        {
            return player.GetModPlayer<FairyPlayer>();
        }

        public static FairyProjectile Fairy(Projectile proj)
        {
            return proj.GetGlobalProjectile<FairyProjectile>();
        }

        public static int CountProjectiles(int Type)
        {
            return Main.projectile.Count((Projectile proj) => proj.type == Type && proj.active);
        }

        public static bool InSpace(Player player)
        {
            float x = (float)Main.maxTilesX / 4200f;
            x *= x;
            return (float)((double)(player.position.Y / 16f - (60f + 10f * x)) / (Main.worldSurface / 6.0)) < 1f;
        }

        public static void HealingProjectile(Projectile projectile, int healing, int playerToHeal, int timeCheck = 120)
        {
            Player player = Main.LocalPlayer;
            Vector2 playerVector = player.Center - projectile.Center;
            float playerDist = playerVector.Length();
            if (player.Hitbox.Intersects(projectile.Hitbox))
            {
                {
                    player.HealEffect(healing, broadcast: false);
                    player.statLife += healing;
                    if (player.statLife > player.statLifeMax2)
                    {
                        player.statLife = player.statLifeMax2;
                    }
                    NetMessage.SendData(66, -1, -1);
                }
            }
        }

        public static void HealingProjectile2(Projectile projectile, int healing, int playerToHeal, float homingVelocity, float N, bool autoHomes = true, int timeCheck = 120)
        {
            Player player = Main.player[playerToHeal];
            float homingSpeed = homingVelocity;
            player.HealEffect(healing, broadcast: false);
            player.statLife += healing;
            if (player.statLife > player.statLifeMax2)
            {
                player.statLife = player.statLifeMax2;
            }
            NetMessage.SendData(66, -1, -1, null, playerToHeal, healing);
        }

        public static string ColorMessage(string msg, Color color)
        {
            StringBuilder stringBuilder = new StringBuilder(msg.Length + 12);
            stringBuilder.Append("[c/").Append(color.Hex4()).Append(':')
                .Append(msg)
                .Append(']');
            return stringBuilder.ToString();
        }

        public static void LightHitWire(int type, int i, int j, int tileX, int tileY)
        {
            int x = i - Main.tile[i, j].TileFrameX / 18 % tileX;
            int y = j - Main.tile[i, j].TileFrameY / 18 % tileY;
            int tileXX18 = 18 * tileX;
            for (int l = x; l < x + tileX; l++)
            {
                for (int m = y; m < y + tileY; m++)
                {
                    if (Main.tile[l, m].HasTile && Main.tile[l, m].TileType == type)
                    {
                        if (Main.tile[l, m].TileFrameX < tileXX18)
                            Main.tile[l, m].TileFrameX += (short)(tileXX18);
                        else
                            Main.tile[l, m].TileFrameX -= (short)(tileXX18);
                    }
                }
            }
            if (Wiring.running)
            {
                for (int k = 0; k < tileX; k++)
                {
                    for (int l = 0; l < tileY; l++)
                        Wiring.SkipWire(x + k, y + l);
                }
            }
        }

        public static void SummonRupeeShard(Projectile projectile, int ProjectileType, int CrystalState)
        {
            Player player = Main.player[projectile.owner];
            Projectile.NewProjectile(projectile.GetSource_FromThis(), projectile.position.X + 100, projectile.position.Y - 60, 0, 0, ProjectileType, (int)(projectile.damage), 0f, projectile.owner, player.whoAmI, projectile.whoAmI);
        }

        public static bool IsTouchingWaterBarrier(Projectile projectile)
        {
            Player player = Main.player[projectile.owner];
            int WaterVeil = ModContent.ProjectileType<WaterBarrier3>();
            int WaterVeil2 = ModContent.ProjectileType<WaterBarrier>();
            int owner = player.whoAmI;
            for (int l = 0; l < 1000; l++)
            {
                if (Main.projectile[l].active && l != projectile.whoAmI && ((Main.projectile[l].type == WaterVeil || Main.projectile[l].type == WaterVeil2)))
                {
                    if (Main.projectile[l].Hitbox.Intersects(projectile.Hitbox))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool IsUnderThunderCloud(Projectile projectile)
        {
            Player player = Main.player[projectile.owner];
            int CloudStrife = ModContent.ProjectileType<LightningCloud>();
            for (int l = 0; l < 1000; l++)
            {
                if (Main.projectile[l].active && l != projectile.whoAmI && ((Main.projectile[l].type == CloudStrife)))
                {
                    {
                        Vector2 UpWardPosition = projectile.Center;
                        int sneezespot = 18;
                        if (projectile.spriteDirection > 0)
                        {
                            sneezespot = 18;
                        }
                        if (projectile.spriteDirection < 0)
                        {
                            sneezespot = 3;
                        }
                        UpWardPosition.X += sneezespot;
                        Vector2 CloudPosition = Main.projectile[l].Center;
                        bool NoCover = Collision.CanHitLine(UpWardPosition, projectile.width / 4, projectile.height - 50, CloudPosition, 0, 1);
                        if ((Math.Abs(UpWardPosition.X - CloudPosition.X) <= 100) && (UpWardPosition.Y >= CloudPosition.Y) && (Math.Abs(UpWardPosition.Y - CloudPosition.Y) <= 1000) && NoCover)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

    }
}
