using Microsoft.Xna.Framework;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using SariaMod.Items.Strange;
using Terraria;
using Terraria.ID;
using SariaMod.Buffs;
using Terraria.ModLoader;

namespace SariaMod.Items.Strange
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

	public class SariaBuff : ModBuff
	{
		public override void SetDefaults()
		{
			DisplayName.SetDefault("FairySpirit");
			Description.SetDefault("Saria now watches over you\n-She will give you added buffs as she levels up!");
			Main.debuff[base.Type] = false;
			Main.pvpBuff[base.Type] = true;
			Main.buffNoSave[base.Type] = false;
			Main.buffNoTimeDisplay[base.Type] = true;
			longerExpertDebuff = false;

		}
		public override void Update(Player player, ref int buffIndex)
		{
			FairyPlayer modPlayer = player.Fairy();
			if (player.ownedProjectileCounts[ModContent.ProjectileType<Saria>()] > 0)
			{
				if (modPlayer.Sarialevel == 0)
				{
					player.buffTime[buffIndex] = 18000;
					player.statLifeMax2 += 25;
					player.detectCreature = true;
					player.noFallDmg = true;
				}
				if (modPlayer.Sarialevel == 1)
				{
					
					player.buffTime[buffIndex] = 18000;
					player.statLifeMax2 += 50;
					player.waterWalk = true;
					player.detectCreature = true;
					player.gills = true;
					player.dangerSense = true;
					player.accFlipper = true;
					player.ignoreWater = true;
					player.noFallDmg = true;
					player.AddBuff(BuffID.Warmth, 20);
				}
				if (modPlayer.Sarialevel == 2)
				{
					player.buffTime[buffIndex] = 18000;
					player.statLifeMax2 += 75;
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
				}
				if (modPlayer.Sarialevel == 3)
				{
					player.buffTime[buffIndex] = 18000;
					player.statLifeMax2 += 100;
					player.waterWalk = true;
					player.detectCreature = true;
					player.lavaImmune = true;
					player.fireWalk = true;
					player.noFallDmg = true;
					player.AddBuff(BuffID.WellFed, 20);
					player.dangerSense = true;
					player.resistCold = true;
					player.gills = true;
					player.accFlipper = true;
					player.ignoreWater = true;
					player.AddBuff(BuffID.ObsidianSkin, 20);
					player.AddBuff(BuffID.Warmth, 20);
					player.lavaTime = 180000;
				}
				if (modPlayer.Sarialevel == 4)
				{
					player.buffTime[buffIndex] = 18000;
					player.statLifeMax2 += 125;
					player.pickSpeed += -60000;
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
				}
				if (modPlayer.Sarialevel == 5)
				{
					player.buffTime[buffIndex] = 18000;
					player.statLifeMax2 += 125;
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
					player.noFallDmg = true;
					player.ignoreWater = true;
					player.AddBuff(BuffID.ObsidianSkin, 20);
					player.AddBuff(BuffID.Warmth, 20);
					player.lavaTime = 180000;
				}
				if (modPlayer.Sarialevel == 6)
				{
					player.buffTime[buffIndex] = 18000;
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