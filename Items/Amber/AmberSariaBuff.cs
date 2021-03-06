using Microsoft.Xna.Framework;

using SariaMod.Buffs;
using System;
using SariaMod.Items.Amber;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.Amber
{
	/*
	 * This file contains all the code necessary for a minion
	 * - ModItem
	 *     the weapon which you use to summon the minion with
	 * - ModBuff
	 *     the icon you can click on to despawn the minion
	 * - ModProjectile 
	 *     the minion itself
	 *     
	 * It is not recommended to put all these classes in the same file. For demonstrations sake they are all compacted together so you get a better overwiew.
	 * To get a better understanding of how everything works together, and how to code minion AI, read the guide: https://github.com/tModLoader/tModLoader/wiki/Basic-Minion-Guide
	 * This is NOT an in-depth guide to advanced minion AI
	 */

	public class AmberSariaBuff : ModBuff
	{
		public override void SetDefaults()
		{
			DisplayName.SetDefault("Amber Spirit");
			Description.SetDefault("Saria now has the Amber upgrade\n-Saria unleshes an old plague onto foes\n-Saria now raises life, Health regeneration, and Defense!\n-Saria's shots are now quicker!");
			Main.debuff[base.Type] = false;
			Main.pvpBuff[base.Type] = true;
			Main.buffNoSave[base.Type] = false;
			Main.buffNoTimeDisplay[base.Type] = true;
			longerExpertDebuff = false;

		}
		public override void Update(Player player, ref int buffIndex)
		{
			if (player.ownedProjectileCounts[ModContent.ProjectileType<AmberSariaMinion>()] > 0)
			{
				
				player.statLifeMax2 += 125;
				player.honey = true;
				player.crimsonRegen = true;
				player.accOreFinder = true;
				player.findTreasure = true;
				player.waterWalk = true;
				player.detectCreature = true;
				player.lavaImmune = true;
				player.fireWalk = true;
				player.dangerSense = true;
				player.wellFed = true;
				player.noFallDmg = true;
				player.resistCold = true;
				player.gills = true;
				player.accFlipper = true;
				player.detectCreature = true;
				player.noFallDmg = true;
				player.AddBuff(BuffID.ObsidianSkin, 20);
				player.AddBuff(BuffID.Warmth, 20);
				player.lavaTime = 180000;
				if (player.buffTime[buffIndex] <= 10)
				{
					player.buffTime[buffIndex] = 18000;
					if (!player.HasBuff(ModContent.BuffType<Soothing>()))
					{
						player.AddBuff(ModContent.BuffType<Sickness>(), 18000);
					}
				}
			}
			else if (player.ownedProjectileCounts[ModContent.ProjectileType<ASariaMinion>()] > 0)
			{
				
				player.statLifeMax2 += 125;
				player.honey = true;
				player.crimsonRegen = true;
				player.accOreFinder = true;
				player.findTreasure = true;
				player.waterWalk = true;
				player.detectCreature = true;
				player.lavaImmune = true;
				player.fireWalk = true;
				player.dangerSense = true;
				player.wellFed = true;
				player.noFallDmg = true;
				player.resistCold = true;
				player.gills = true;
				player.accFlipper = true;
				player.detectCreature = true;
				player.noFallDmg = true;
				player.AddBuff(BuffID.ObsidianSkin, 20);
				player.AddBuff(BuffID.Warmth, 20);
				player.lavaTime = 180000;
				if (player.buffTime[buffIndex] <= 10)
				{
					player.buffTime[buffIndex] = 18000;
					if (!player.HasBuff(ModContent.BuffType<Soothing>()))
					{
						player.AddBuff(ModContent.BuffType<Sickness>(), 18000);
					}
				}
			}
			else if(player.ownedProjectileCounts[ModContent.ProjectileType<ASSariaMinion>()] > 0)
			{
				
				player.statLifeMax2 += 125;
				player.honey = true;
				player.crimsonRegen = true;
				player.accOreFinder = true;
				player.findTreasure = true;
				player.waterWalk = true;
				player.detectCreature = true;
				player.lavaImmune = true;
				player.fireWalk = true;
				player.dangerSense = true;
				player.wellFed = true;
				player.noFallDmg = true;
				player.resistCold = true;
				player.gills = true;
				player.accFlipper = true;
				player.detectCreature = true;
				player.noFallDmg = true;
				player.AddBuff(BuffID.ObsidianSkin, 20);
				player.AddBuff(BuffID.Warmth, 20);
				player.lavaTime = 180000;
				if (player.buffTime[buffIndex] <= 10)
				{
					player.buffTime[buffIndex] = 18000;
					if (!player.HasBuff(ModContent.BuffType<Soothing>()))
					{
						player.AddBuff(ModContent.BuffType<Sickness>(), 18000);
					}
				}
			}
			else if(player.ownedProjectileCounts[ModContent.ProjectileType<ARSariaMinion>()] > 0)
			{
				
				player.statLifeMax2 += 125;
				player.honey = true;
				player.crimsonRegen = true;
				player.accOreFinder = true;
				player.findTreasure = true;
				player.waterWalk = true;
				player.detectCreature = true;
				player.lavaImmune = true;
				player.fireWalk = true;
				player.dangerSense = true;
				player.wellFed = true;
				player.noFallDmg = true;
				player.resistCold = true;
				player.gills = true;
				player.accFlipper = true;
				player.detectCreature = true;
				player.noFallDmg = true;
				player.AddBuff(BuffID.ObsidianSkin, 20);
				player.AddBuff(BuffID.Warmth, 20);
				player.lavaTime = 180000;
				if (player.buffTime[buffIndex] <= 10)
				{
					player.buffTime[buffIndex] = 18000;
					if (!player.HasBuff(ModContent.BuffType<Soothing>()))
					{
						player.AddBuff(ModContent.BuffType<Sickness>(), 18000);
					}
				}
			}
			else if(player.ownedProjectileCounts[ModContent.ProjectileType<ATSariaMinion>()] > 0)
			{
			
				player.statLifeMax2 += 125;
				player.honey = true;
				player.crimsonRegen = true;
				player.accOreFinder = true;
				player.findTreasure = true;
				player.waterWalk = true;
				player.detectCreature = true;
				player.lavaImmune = true;
				player.fireWalk = true;
				player.dangerSense = true;
				player.wellFed = true;
				player.noFallDmg = true;
				player.resistCold = true;
				player.gills = true;
				player.accFlipper = true;
				player.detectCreature = true;
				player.noFallDmg = true;
				player.AddBuff(BuffID.ObsidianSkin, 20);
				player.AddBuff(BuffID.Warmth, 20);
				player.lavaTime = 180000;
				if (player.buffTime[buffIndex] <= 10)
				{
					player.buffTime[buffIndex] = 18000;
					if (!player.HasBuff(ModContent.BuffType<Soothing>()))
					{
						player.AddBuff(ModContent.BuffType<Sickness>(), 18000);
					}
				}
			}
			else if(player.ownedProjectileCounts[ModContent.ProjectileType<AESariaMinion>()] > 0)
			{
				
				player.statLifeMax2 += 125;
				player.honey = true;
				player.crimsonRegen = true;
				player.accOreFinder = true;
				player.findTreasure = true;
				player.waterWalk = true;
				player.detectCreature = true;
				player.lavaImmune = true;
				player.fireWalk = true;
				player.dangerSense = true;
				player.wellFed = true;
				player.noFallDmg = true;
				player.resistCold = true;
				player.gills = true;
				player.accFlipper = true;
				player.detectCreature = true;
				player.noFallDmg = true;
				player.AddBuff(BuffID.ObsidianSkin, 20);
				player.AddBuff(BuffID.Warmth, 20);
				player.lavaTime = 180000;
				if (player.buffTime[buffIndex] <= 10)
				{
					player.buffTime[buffIndex] = 18000;
					if (!player.HasBuff(ModContent.BuffType<Soothing>()))
					{
						player.AddBuff(ModContent.BuffType<Sickness>(), 18000);
					}
				}
			}
			else
			{
				player.DelBuff(buffIndex);
				buffIndex--;
				
			}
		}

	}
}