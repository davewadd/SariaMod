using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using SariaMod.Buffs;
using SariaMod.Dusts;
using SariaMod.Items.Strange;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;

using SariaMod.Items.zBookcases;
using Terraria.Localization;
using SariaMod;
using SariaMod.Items.Bands;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace SariaMod
{
	public class FairyPlayerMiscEffects : ModPlayer
	{
        private const int sphereRadius3 = 1;
        private static int timer;
        public static void FairyPostUpdateMiscEffects(Player player, Mod mod)
		{
			FairyPlayer modPlayer = player.Fairy();
			MiscEffects(player, modPlayer, mod);
		}
        
        private static void MiscEffects(Player player, FairyPlayer modPlayer, Mod mod)
		{
           
            if (modPlayer.Sarialevel == 0)
            {
                if (modPlayer.SariaXp <= 375)
                {
                    modPlayer.XPBarLevel = 0;
                }
                else if (modPlayer.SariaXp > 375 && (modPlayer.SariaXp <= 750))
                {
                    modPlayer.XPBarLevel = 1;
                }
                else if (modPlayer.SariaXp > 750 && (modPlayer.SariaXp <= 1125))
                {
                    modPlayer.XPBarLevel = 2;
                }
                else if (modPlayer.SariaXp > 1125 && (modPlayer.SariaXp <= 1500))
                {
                    modPlayer.XPBarLevel = 3;
                }
                else if (modPlayer.SariaXp > 1500 && (modPlayer.SariaXp <= 1875))
                {
                    modPlayer.XPBarLevel = 4;
                }
                else if (modPlayer.SariaXp > 1875 && (modPlayer.SariaXp <= 2250))
                {
                    modPlayer.XPBarLevel = 5;
                }
                else if (modPlayer.SariaXp > 2250 && (modPlayer.SariaXp <= 2625))
                {
                    modPlayer.XPBarLevel = 6;
                }
                else if (modPlayer.SariaXp > 2625 && (modPlayer.SariaXp <= 3000))
                {
                    modPlayer.XPBarLevel = 7;
                }
                else if ((modPlayer.SariaXp > 3000))
                {
                    modPlayer.XPBarLevel = 8;
                }
            }
            if (modPlayer.Sarialevel == 1)
            {
                if (modPlayer.SariaXp <= 1125)
                {
                    modPlayer.XPBarLevel = 0;
                }
                else if (modPlayer.SariaXp > 1125 && (modPlayer.SariaXp <= 2250))
                {
                    modPlayer.XPBarLevel = 1;
                }
                else if (modPlayer.SariaXp > 2250 && (modPlayer.SariaXp <= 3375))
                {
                    modPlayer.XPBarLevel = 2;
                }
                else if (modPlayer.SariaXp > 3375 && (modPlayer.SariaXp <= 4500))
                {
                    modPlayer.XPBarLevel = 3;
                }
                else if (modPlayer.SariaXp > 4500 && (modPlayer.SariaXp <= 5625))
                {
                    modPlayer.XPBarLevel = 4;
                }
                else if (modPlayer.SariaXp > 5625 && (modPlayer.SariaXp <= 6750))
                {
                    modPlayer.XPBarLevel = 5;
                }
                else if (modPlayer.SariaXp > 6750 && (modPlayer.SariaXp <= 7875))
                {
                    modPlayer.XPBarLevel = 6;
                }
                else if (modPlayer.SariaXp > 7875 && (modPlayer.SariaXp <= 9000))
                {
                    modPlayer.XPBarLevel = 7;
                }
                else if ((modPlayer.SariaXp > 9000))
                {
                    modPlayer.XPBarLevel = 8;
                }
            }
            if (modPlayer.Sarialevel == 2)
            {
                if (modPlayer.SariaXp <= 2500)
                {
                    modPlayer.XPBarLevel = 0;
                }
                else if (modPlayer.SariaXp > 2500 && (modPlayer.SariaXp <= 5000))
                {
                    modPlayer.XPBarLevel = 1;
                }
                else if (modPlayer.SariaXp > 5000 && (modPlayer.SariaXp <= 7500))
                {
                    modPlayer.XPBarLevel = 2;
                }
                else if (modPlayer.SariaXp > 7500 && (modPlayer.SariaXp <= 10000))
                {
                    modPlayer.XPBarLevel = 3;
                }
                else if (modPlayer.SariaXp > 10000 && (modPlayer.SariaXp <= 12500))
                {
                    modPlayer.XPBarLevel = 4;
                }
                else if (modPlayer.SariaXp > 12500 && (modPlayer.SariaXp <= 15000))
                {
                    modPlayer.XPBarLevel = 5;
                }
                else if (modPlayer.SariaXp > 15000 && (modPlayer.SariaXp <= 17500))
                {
                    modPlayer.XPBarLevel = 6;
                }
                else if (modPlayer.SariaXp > 17500 && (modPlayer.SariaXp <= 20000))
                {
                    modPlayer.XPBarLevel = 7;
                }
                else if ((modPlayer.SariaXp > 20000))
                {
                    modPlayer.XPBarLevel = 8;
                }
            }
            if (modPlayer.Sarialevel == 3)
            {
                if (modPlayer.SariaXp <= 5000)
                {
                    modPlayer.XPBarLevel = 0;
                }
                else if (modPlayer.SariaXp > 5000 && (modPlayer.SariaXp <= 10000))
                {
                    modPlayer.XPBarLevel = 1;
                }
                else if (modPlayer.SariaXp > 10000 && (modPlayer.SariaXp <= 15000))
                {
                    modPlayer.XPBarLevel = 2;
                }
                else if (modPlayer.SariaXp > 15000 && (modPlayer.SariaXp <= 20000))
                {
                    modPlayer.XPBarLevel = 3;
                }
                else if (modPlayer.SariaXp > 20000 && (modPlayer.SariaXp <= 25000))
                {
                    modPlayer.XPBarLevel = 4;
                }
                else if (modPlayer.SariaXp > 25000 && (modPlayer.SariaXp <= 30000))
                {
                    modPlayer.XPBarLevel = 5;
                }
                else if (modPlayer.SariaXp > 30000 && (modPlayer.SariaXp <= 35000))
                {
                    modPlayer.XPBarLevel = 6;
                }
                else if (modPlayer.SariaXp > 35000 && (modPlayer.SariaXp <= 40000))
                {
                    modPlayer.XPBarLevel = 7;
                }
                else if ((modPlayer.SariaXp > 40000))
                {
                    modPlayer.XPBarLevel = 8;
                }
            }
            if (modPlayer.Sarialevel == 4)
            {
                if (modPlayer.SariaXp <= 10000)
                {
                    modPlayer.XPBarLevel = 0;
                }
                else if (modPlayer.SariaXp > 10000 && (modPlayer.SariaXp <= 20000))
                {
                    modPlayer.XPBarLevel = 1;
                }
                else if (modPlayer.SariaXp > 20000 && (modPlayer.SariaXp <= 30000))
                {
                    modPlayer.XPBarLevel = 2;
                }
                else if (modPlayer.SariaXp > 30000 && (modPlayer.SariaXp <= 40000))
                {
                    modPlayer.XPBarLevel = 3;
                }
                else if (modPlayer.SariaXp > 40000 && (modPlayer.SariaXp <= 50000))
                {
                    modPlayer.XPBarLevel = 4;
                }
                else if (modPlayer.SariaXp > 50000 && (modPlayer.SariaXp <= 60000))
                {
                    modPlayer.XPBarLevel = 5;
                }
                else if (modPlayer.SariaXp > 60000 && (modPlayer.SariaXp <= 70000))
                {
                    modPlayer.XPBarLevel = 6;
                }
                else if (modPlayer.SariaXp > 70000 && (modPlayer.SariaXp <= 80000))
                {
                    modPlayer.XPBarLevel = 7;
                }
                else if ((modPlayer.SariaXp > 80000))
                {
                    modPlayer.XPBarLevel = 8;
                }
            }
            if (modPlayer.Sarialevel == 5)
            {
                if (modPlayer.SariaXp <= 30000)
                {
                    modPlayer.XPBarLevel = 0;
                }
                else if (modPlayer.SariaXp > 30000 && (modPlayer.SariaXp <= 60000))
                {
                    modPlayer.XPBarLevel = 1;
                }
                else if (modPlayer.SariaXp > 60000 && (modPlayer.SariaXp <= 90000))
                {
                    modPlayer.XPBarLevel = 2;
                }
                else if (modPlayer.SariaXp > 90000 && (modPlayer.SariaXp <= 120000))
                {
                    modPlayer.XPBarLevel = 3;
                }
                else if (modPlayer.SariaXp > 120000 && (modPlayer.SariaXp <= 150000))
                {
                    modPlayer.XPBarLevel = 4;
                }
                else if (modPlayer.SariaXp > 150000 && (modPlayer.SariaXp <= 180000))
                {
                    modPlayer.XPBarLevel = 5;
                }
                else if (modPlayer.SariaXp > 180000 && (modPlayer.SariaXp <= 210000))
                {
                    modPlayer.XPBarLevel = 6;
                }
                else if (modPlayer.SariaXp > 210000 && (modPlayer.SariaXp <= 240000))
                {
                    modPlayer.XPBarLevel = 7;
                }
                else if ((modPlayer.SariaXp > 240000))
                {
                    modPlayer.XPBarLevel = 8;
                }
            }
            if (modPlayer.Sarialevel == 6)
            {
                {
                    modPlayer.XPBarLevel = 8;
                }
            }
            if (modPlayer.SariaXp >= 240000 && NPC.downedFishron && modPlayer.Sarialevel == 5 && (player.ownedProjectileCounts[ModContent.ProjectileType<Saria>()] >= 1f))
            {
                modPlayer.Sarialevel = 6;
                for (int j = 0; j < 72; j++)
                {
                    Dust dust = Dust.NewDustPerfect(player.Center, 113);
                    dust.velocity = ((float)Math.PI * 2f * Vector2.Dot(((float)j / 72f * ((float)Math.PI * 2f)).ToRotationVector2(), player.velocity.SafeNormalize(Vector2.UnitY).RotatedBy((float)j / 72f * ((float)Math.PI * -2f)))).ToRotationVector2();
                    dust.velocity = dust.velocity.RotatedBy((float)j / 36f * ((float)Math.PI * 2f)) * 8f;
                    dust.noGravity = true;
                    dust.scale = 1.9f;
                    Main.PlaySound(SoundID.Item110, player.Center);
                    Main.PlaySound(SoundID.Item14, player.Center);
                }
                for (int j = 0; j < 1; j++)
                {
                    Item.NewItem(player.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ItemType<SariaSeventhFormNote>());
                }
            }
            if (modPlayer.SariaXp >= 80000 && NPC.downedPlantBoss && modPlayer.Sarialevel == 4 && (player.ownedProjectileCounts[ModContent.ProjectileType<Saria>()] >= 1f))
            {
                modPlayer.Sarialevel = 5;
                modPlayer.SariaXp = 0;
                for (int j = 0; j < 72; j++)
                {
                    Dust dust = Dust.NewDustPerfect(player.Center, 113);
                    dust.velocity = ((float)Math.PI * 2f * Vector2.Dot(((float)j / 72f * ((float)Math.PI * 2f)).ToRotationVector2(), player.velocity.SafeNormalize(Vector2.UnitY).RotatedBy((float)j / 72f * ((float)Math.PI * -2f)))).ToRotationVector2();
                    dust.velocity = dust.velocity.RotatedBy((float)j / 36f * ((float)Math.PI * 2f)) * 8f;
                    dust.noGravity = true;
                    dust.scale = 1.9f;
                    Main.PlaySound(SoundID.Item110, player.Center);
                    Main.PlaySound(SoundID.Item14, player.Center);
                }
                for (int j = 0; j < 1; j++)
                {
                    Item.NewItem(player.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ItemType<SariaSixthFormNote>());
                }
            }
            if (modPlayer.SariaXp >= 40000 && NPC.downedMechBossAny && modPlayer.Sarialevel == 3 && (player.ownedProjectileCounts[ModContent.ProjectileType<Saria>()] >= 1f))
            {
                modPlayer.Sarialevel = 4;
                modPlayer.SariaXp = 0;
                for (int j = 0; j < 72; j++)
                {
                    Dust dust = Dust.NewDustPerfect(player.Center, 113);
                    dust.velocity = ((float)Math.PI * 2f * Vector2.Dot(((float)j / 72f * ((float)Math.PI * 2f)).ToRotationVector2(), player.velocity.SafeNormalize(Vector2.UnitY).RotatedBy((float)j / 72f * ((float)Math.PI * -2f)))).ToRotationVector2();
                    dust.velocity = dust.velocity.RotatedBy((float)j / 36f * ((float)Math.PI * 2f)) * 8f;
                    dust.noGravity = true;
                    dust.scale = 1.9f;
                    Main.PlaySound(SoundID.Item110, player.Center);
                    Main.PlaySound(SoundID.Item14, player.Center);
                }
                for (int j = 0; j < 1; j++)
                {
                    Item.NewItem(player.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ItemType<SariaFifthFormNote>());
                }
                }
            if (modPlayer.SariaXp >= 20000 && Main.hardMode && modPlayer.Sarialevel == 2 && (player.ownedProjectileCounts[ModContent.ProjectileType<Saria>()] >= 1f))
            {
                modPlayer.Sarialevel = 3;
                modPlayer.SariaXp = 0;
                for (int j = 0; j < 72; j++)
                {
                    Dust dust = Dust.NewDustPerfect(player.Center, 113);
                    dust.velocity = ((float)Math.PI * 2f * Vector2.Dot(((float)j / 72f * ((float)Math.PI * 2f)).ToRotationVector2(), player.velocity.SafeNormalize(Vector2.UnitY).RotatedBy((float)j / 72f * ((float)Math.PI * -2f)))).ToRotationVector2();
                    dust.velocity = dust.velocity.RotatedBy((float)j / 36f * ((float)Math.PI * 2f)) * 8f;
                    dust.noGravity = true;
                    dust.scale = 1.9f;
                    Main.PlaySound(SoundID.Item110, player.Center);
                    Main.PlaySound(SoundID.Item14, player.Center);
                }
                for (int j = 0; j < 1; j++)
                {
                    Item.NewItem(player.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ItemType<SariaFourthFormNote>());
                }
            }
            if (modPlayer.SariaXp >= 9000 && NPC.downedQueenBee && modPlayer.Sarialevel == 1 && (player.ownedProjectileCounts[ModContent.ProjectileType<Saria>()] >= 1f))
            {
                modPlayer.Sarialevel = 2;
                modPlayer.SariaXp = 0;
                for (int j = 0; j < 72; j++)
                {
                    Dust dust = Dust.NewDustPerfect(player.Center, 113);
                    dust.velocity = ((float)Math.PI * 2f * Vector2.Dot(((float)j / 72f * ((float)Math.PI * 2f)).ToRotationVector2(), player.velocity.SafeNormalize(Vector2.UnitY).RotatedBy((float)j / 72f * ((float)Math.PI * -2f)))).ToRotationVector2();
                    dust.velocity = dust.velocity.RotatedBy((float)j / 36f * ((float)Math.PI * 2f)) * 8f;
                    dust.noGravity = true;
                    dust.scale = 1.9f;
                    Main.PlaySound(SoundID.Item110, player.Center);
                    Main.PlaySound(SoundID.Item14, player.Center);
                }
                for (int j = 0; j < 1; j++)
                {
                    Item.NewItem(player.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ItemType<SariaThirdFormNote>());
                }
            }
            if (modPlayer.SariaXp >= 3000 && NPC.downedSlimeKing && modPlayer.Sarialevel <= 0 && (player.ownedProjectileCounts[ModContent.ProjectileType<Saria>()] >= 1f))
            {
                modPlayer.Sarialevel = 1;
                modPlayer.SariaXp = 0;
                for (int j = 0; j < 72; j++)
                {
                    Dust dust = Dust.NewDustPerfect(player.Center, 113);
                    dust.velocity = ((float)Math.PI * 2f * Vector2.Dot(((float)j / 72f * ((float)Math.PI * 2f)).ToRotationVector2(), player.velocity.SafeNormalize(Vector2.UnitY).RotatedBy((float)j / 72f * ((float)Math.PI * -2f)))).ToRotationVector2();
                    dust.velocity = dust.velocity.RotatedBy((float)j / 36f * ((float)Math.PI * 2f)) * 8f;
                    dust.noGravity = true;
                    dust.scale = 1.9f;
                    Main.PlaySound(SoundID.Item110, player.Center);
                    Main.PlaySound(SoundID.Item14, player.Center);
                    
                }
                for (int j = 0; j < 1; j++)
                {
                    Item.NewItem(player.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ItemType<SariaSecondFormNote>());
                }
            }
           
            float sneezespot = 5;
            bool Warm = player.behindBackWall && player.HasBuff(BuffID.Campfire);
            bool immunityToCold = player.HasBuff(BuffID.Warmth) || (player.HasBuff(BuffID.Campfire) && player.behindBackWall) || player.HasBuff(BuffID.OnFire) || player.arcticDivingGear;
			bool immunityToHeat = player.HasBuff(BuffID.ObsidianSkin) || player.lavaImmune || player.ZoneWaterCandle;
			if (player.whoAmI == Main.myPlayer)
			{
                player.buffImmune[ModContent.BuffType<Frostburn2>()] = false;
                player.buffImmune[ModContent.BuffType<Frozen2>()] = false;
                player.buffImmune[ModContent.BuffType<Burning2>()] = false;
                if (player.ZoneSnow && Main.raining && (!immunityToCold || (player.HasBuff(ModContent.BuffType<StatLower>()) && !Warm)))
				{
                    modPlayer.FreezingTemp++;
                    if (!player.behindBackWall)
                    {
                        modPlayer.FreezingTemp++;
                        player.AddBuff(ModContent.BuffType<Frostburn2>(),2);
                    }
				}
                if (modPlayer.FreezingTemp >= 3000)
                {
                    player.AddBuff(ModContent.BuffType<Frozen2>(), 398);
                    modPlayer.FreezingTemp = 0;
                }
                
			}
            if (immunityToCold && modPlayer.FreezingTemp > 0)
            {
                modPlayer.FreezingTemp--;
            }
			{
				
				if (!player.behindBackWall && (!immunityToHeat || player.HasBuff(ModContent.BuffType<StatLower>())) && player.ZoneUnderworldHeight)
				{

                    player.AddBuff(ModContent.BuffType<Burning2>(), 2);
                }
			}
			if (!player.behindBackWall && (!immunityToCold || player.HasBuff(ModContent.BuffType<StatLower>())) && player.InSpace())
            {
                player.AddBuff(ModContent.BuffType<Frostburn3>(), 2);
            }
            if (!player.behindBackWall && (!immunityToHeat || player.HasBuff(ModContent.BuffType<StatLower>())) && player.InSpace())
            {
                player.AddBuff(ModContent.BuffType<Burning2>(), 2);
            }
            
            if (((Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneSnow) && !(Main.player[Main.myPlayer].behindBackWall && player.HasBuff((BuffID.Campfire)))) || (Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneSkyHeight) && !(Main.player[Main.myPlayer].behindBackWall && player.HasBuff((BuffID.Campfire))) || (Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneDesert && !Main.dayTime) && !(Main.player[Main.myPlayer].behindBackWall && player.HasBuff((BuffID.Campfire))) || (Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneRain && !Main.player[Main.myPlayer].ZoneJungle && !(Main.player[Main.myPlayer].ZoneDesert && Main.dayTime)) && !(Main.player[Main.myPlayer].behindBackWall && player.HasBuff((BuffID.Campfire))))
            {
                if (player.velocity.X <= 1)
                {
                    if (Main.rand.NextBool(50))
                    {
                        float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius3 * sphereRadius3));
                        double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                        if (player.direction > 0)
                        {
                            sneezespot = 10;
                        }
                        if (player.direction < 0)
                        {
                            sneezespot = -8;
                        }
                        for (int j = 0; j < 2; j++)
                        {
                            Dust.NewDust(new Vector2((player.Center.X + sneezespot) + radius * (float)Math.Cos(angle), (player.Center.Y - 10) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<Fog>(), 0f, 0f, 0, default(Color), 1.5f);

                        }
                    }
                }
                else if (player.velocity.X > 1)
                {
                    if (Main.rand.NextBool(10))
                    {
                        float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius3 * sphereRadius3));
                        double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                        if (player.direction > 0)
                        {
                            sneezespot = 10;
                        }
                        if (player.direction < 0)
                        {
                            sneezespot = -8;
                        }
                        for (int j = 0; j < 2; j++)
                        {
                            Dust.NewDust(new Vector2((player.Center.X + sneezespot) + radius * (float)Math.Cos(angle), (player.Center.Y - 10) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<Fog>(), 0f, 0f, 0, default(Color), 1.5f);

                        }
                    }
                }

            }
        }
	}
}