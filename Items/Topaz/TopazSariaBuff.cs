using Microsoft.Xna.Framework; 
using FairyMod.FaiPlayer;
using FairyMod.Projectiles;
using System;
using SariaMod.Items.Topaz;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.Topaz
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

	public class TopazSariaBuff : ModBuff
	{
		public override void SetDefaults()
		{
			DisplayName.SetDefault("Topaz Spirit");
			Description.SetDefault("Saria now has the Topaz upgrade\n-Psychic Seekers will now inflict Ichor!\n-Saria now lowers health but raises defense and endurance\n-Enemies now become Electrified!\n-Shot enemies become slower\nYou are now always WellFed\nTreasure items become clear to you now!");
			Main.debuff[base.Type] = false;
			Main.pvpBuff[base.Type] = true;
			Main.buffNoSave[base.Type] = false;
			Main.buffNoTimeDisplay[base.Type] = true;
			longerExpertDebuff = false;

		}
		public override void Update(Player player, ref int buffIndex)
		{
			if (player.ownedProjectileCounts[ModContent.ProjectileType<TopazSariaMinion>()] > 0)
			{
				player.buffTime[buffIndex] = 18000;
				player.statLifeMax2 -= 100;
				player.statDefense = 50;
                player.endurance *= 1.5f;
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