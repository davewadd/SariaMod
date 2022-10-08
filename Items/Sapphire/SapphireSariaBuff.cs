using Microsoft.Xna.Framework;




using SariaMod.Buffs;
using System;
using SariaMod.Items.Sapphire;
using Terraria;
using Terraria.ID;
using SariaMod.Items.Strange;
using Terraria.ModLoader;

namespace SariaMod.Items.Sapphire
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

	public class SapphireSariaBuff : ModBuff
	{
		public override void SetDefaults()
		{
			DisplayName.SetDefault("Sapphire Spirit");
			Description.SetDefault("Saria now has the Sapphire upgrade\n-Saria can now help you underwater\nColdSoul will help heal your soul");
			Main.debuff[base.Type] = false;
			Main.pvpBuff[base.Type] = true;
			Main.buffNoSave[base.Type] = false;
			Main.buffNoTimeDisplay[base.Type] = true;
			longerExpertDebuff = false;

		}
		public override void Update(Player player, ref int buffIndex)
		{
			if ((player.ownedProjectileCounts[ModContent.ProjectileType<SapphireSariaMinion>()] > 0))
			{
				 
				player.statLifeMax2 += 50;
				player.waterWalk = true;
				player.detectCreature = true;
				player.gills = true;
				player.dangerSense = true;
				player.accFlipper = true;
				player.ignoreWater = true;
				player.noFallDmg = true;
				player.AddBuff(BuffID.Warmth, 20);
				if (player.buffTime[buffIndex] <= 10)
				{
					player.buffTime[buffIndex] = 18000;
					if (!player.HasBuff(ModContent.BuffType<Soothing>()))
					{
						player.AddBuff(ModContent.BuffType<Sickness>(), 18000);
					}
				}

			}
		
			else if (player.ownedProjectileCounts[ModContent.ProjectileType<SapphireSariaMinion2>()] > 0)
			{
				 
				player.statLifeMax2 += 50;
				player.waterWalk = true;
				player.detectCreature = true;
				player.gills = true;
				player.dangerSense = true;
				player.accFlipper = true;
				player.noFallDmg = true;
				player.AddBuff(BuffID.Warmth, 20);
				if (player.buffTime[buffIndex] <= 10)
				{
					player.buffTime[buffIndex] = 18000;
					if (!player.HasBuff(ModContent.BuffType<Soothing>()))
					{
						player.AddBuff(ModContent.BuffType<Sickness>(), 18000);
					}
				}

			}
			else if (player.ownedProjectileCounts[ModContent.ProjectileType<SSariaMinion>()] > 0)
			{
				 
				player.statLifeMax2 += 50;
				player.waterWalk = true;
				player.detectCreature = true;
				player.gills = true;
				player.dangerSense = true;
				player.accFlipper = true;
				player.noFallDmg = true;
				player.AddBuff(BuffID.Warmth, 20);
				if (player.buffTime[buffIndex] <= 10)
				{
					player.buffTime[buffIndex] = 18000;
					if (!player.HasBuff(ModContent.BuffType<Soothing>()))
					{
						player.AddBuff(ModContent.BuffType<Sickness>(), 18000);
					}
				}

			}
			else if (player.ownedProjectileCounts[ModContent.ProjectileType<SSariaMinion2>()] > 0)
			{
				 
				player.statLifeMax2 += 50;
				player.waterWalk = true;
				player.detectCreature = true;
				player.gills = true;
				player.dangerSense = true;
				player.accFlipper = true;
				player.noFallDmg = true;
				player.AddBuff(BuffID.Warmth, 20);
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