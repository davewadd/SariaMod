using Microsoft.Xna.Framework;

using SariaMod.Items.Strange;
using SariaMod.Buffs;
using System;
using SariaMod.Items.Diamond;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.Diamond

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

	public class DiamondSariaBuff : ModBuff
	{
		public override void SetDefaults()
		{
			DisplayName.SetDefault("GuardianSpirit");
			Description.SetDefault("Saria now has the Diamond upgrade\n-There is nothing she cannot help you acheive!");
			Main.debuff[base.Type] = false;
			Main.pvpBuff[base.Type] = true;
			Main.buffNoSave[base.Type] = false;
			Main.buffNoTimeDisplay[base.Type] = true;
			longerExpertDebuff = false;

		}
		public override void Update(Player player, ref int buffIndex)
		{
			if (player.ownedProjectileCounts[ModContent.ProjectileType<Saria>()] > 0)
			{
				player.lavaTime = 180000;
				player.statLifeMax2 += 200;
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
			
			else
			{
				player.DelBuff(buffIndex);
				buffIndex--;
				
			}
		}

	}
}