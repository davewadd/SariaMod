using Microsoft.Xna.Framework; 
using FairyMod.FaiPlayer;
using FairyMod.Projectiles;
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
			Description.SetDefault("Saria now has the Amethyst upgrade\n-Psychic Seekers will inflicts poison and Shadow flames\n-Saria's shots are much quicker\nAll attacks will now start to shave enemy\ndefense down");
			Main.buffNoSave[base.Type] = true;
			Main.buffNoTimeDisplay[base.Type] = true;
			

		}
		public override void Update(Player player, ref int buffIndex)
		{
			if (player.ownedProjectileCounts[ModContent.ProjectileType<AmethystSariaMinion>()] > 0)
			{
				player.buffTime[buffIndex] = 18000;
				player.statLifeMax2 += 150;
				player.statDefense += 60;
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
			}
			else
			{
				player.DelBuff(buffIndex);
				buffIndex--;
				
			}
			
		}

	}
}