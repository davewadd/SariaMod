using Microsoft.Xna.Framework;

using SariaMod.Buffs;
using System;
using SariaMod.Items.Amethyst;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.Amethyst
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

	public class AmethystSariaBuff : ModBuff
	{
		public override void SetDefaults()
		{
			DisplayName.SetDefault("Amethyst Spirit");
			Description.SetDefault("Saria now has the Amethyst upgrade\nShe can steal enemies abilities to go through walls\nShadowsneak will curse enemies");
			Main.buffNoSave[base.Type] = true;
			Main.buffNoTimeDisplay[base.Type] = true;
			

		}
		public override void Update(Player player, ref int buffIndex)
		{
			if (player.ownedProjectileCounts[ModContent.ProjectileType<AmethystSariaMinion>()] > 0)
			{
				
				player.statLifeMax2 += 150;
				player.accOreFinder = true;
				player.findTreasure = true;
				player.waterWalk = true;
				player.detectCreature = true;
				player.lavaImmune = true;
				player.fireWalk = true;
				player.dangerSense = true;
				player.noFallDmg = true;
				player.resistCold = true;
				player.gills = true;
				player.accFlipper = true;
				player.ignoreWater = true;
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
			else if (player.ownedProjectileCounts[ModContent.ProjectileType<AMSariaMinion>()] > 0)
			{
				
				player.statLifeMax2 += 150;
				player.honey = true;
				player.crimsonRegen = true;
				player.accOreFinder = true;
				player.findTreasure = true;
				player.thorns += 20;
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
			else if (player.ownedProjectileCounts[ModContent.ProjectileType<AMSSariaMinion>()] > 0)
			{
				
				player.statLifeMax2 += 150;
				player.honey = true;
				player.crimsonRegen = true;
				player.accOreFinder = true;
				player.findTreasure = true;
				player.thorns += 20;
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
			else if (player.ownedProjectileCounts[ModContent.ProjectileType<AMRSariaMinion>()] > 0)
			{
				
				player.statLifeMax2 += 150;
				player.honey = true;
				player.crimsonRegen = true;
				player.accOreFinder = true;
				player.findTreasure = true;
				player.thorns += 20;
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
			else if (player.ownedProjectileCounts[ModContent.ProjectileType<AMTSariaMinion>()] > 0)
			{
				
				player.statLifeMax2 += 150;
				player.honey = true;
				player.crimsonRegen = true;
				player.accOreFinder = true;
				player.findTreasure = true;
				player.thorns += 20;
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
			else if (player.ownedProjectileCounts[ModContent.ProjectileType<AMESariaMinion>()] > 0)
			{
				
				player.statLifeMax2 += 150;
				player.honey = true;
				player.crimsonRegen = true;
				player.accOreFinder = true;
				player.findTreasure = true;
				player.thorns += 20;
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
			else if (player.ownedProjectileCounts[ModContent.ProjectileType<AMASariaMinion>()] > 0)
			{
				 
				player.statLifeMax2 += 150;
				player.honey = true;
				player.crimsonRegen = true;
				player.accOreFinder = true;
				player.findTreasure = true;
				player.thorns += 20;
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