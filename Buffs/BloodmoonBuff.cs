using Microsoft.Xna.Framework;
using SariaMod.Items.Ruby;
using SariaMod.Items.Sapphire;
using SariaMod.Items.Topaz;
using SariaMod.Items.Emerald;
using SariaMod.Items.Amber;
using SariaMod.Items.Amethyst;
using SariaMod.Items.Diamond;
using SariaMod.Items.Platinum;
using SariaMod.Items.Strange;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Buffs
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

	public class BloodmoonBuff : ModBuff
	{
		public override void SetDefaults()
		{
			DisplayName.SetDefault("BloodLust");
			Description.SetDefault("You feel uneasy as Saria gazes at the BloodMoon\nShe unconsciously steals your lifeforce!");
			Main.debuff[base.Type] = true;
			Main.pvpBuff[base.Type] = true;
			Main.buffNoSave[base.Type] = false;
			Main.buffNoTimeDisplay[base.Type] = true;
			longerExpertDebuff = false;

		}
		public override void Update(Player player, ref int buffIndex)
		{
			
			if (!player.HasBuff(ModContent.BuffType<Soothing>()))
			{
				if (player.HasBuff(ModContent.BuffType<SariaBuff>()) && (Main.player[Main.myPlayer].active && Main.bloodMoon))
				{
					player.buffTime[buffIndex] = 18000;
					player.statLifeMax2 /= 2;
				}
				else if (player.HasBuff(ModContent.BuffType<SapphireSariaBuff>()) && (Main.player[Main.myPlayer].active && Main.bloodMoon))
				{
					player.buffTime[buffIndex] = 18000;
					player.statLifeMax2 /= 2;
				}
				else if (player.HasBuff(ModContent.BuffType<RubySariaBuff>()) && (Main.player[Main.myPlayer].active && Main.bloodMoon))
				{
					player.buffTime[buffIndex] = 18000;
					player.statLifeMax2 /= 2;
				}
				else if (player.HasBuff(ModContent.BuffType<TopazSariaBuff>()) && (Main.player[Main.myPlayer].active && Main.bloodMoon))
				{
					player.buffTime[buffIndex] = 18000;
					player.statLifeMax2 /= 2;
				}
				else if (player.HasBuff(ModContent.BuffType<EmeraldSariaBuff>()) && (Main.player[Main.myPlayer].active && Main.bloodMoon))
				{
					player.buffTime[buffIndex] = 18000;
					player.statLifeMax2 /= 2;
				}
				else if (player.HasBuff(ModContent.BuffType<AmberSariaBuff>()) && (Main.player[Main.myPlayer].active && Main.bloodMoon))
				{
					player.buffTime[buffIndex] = 18000;
					player.statLifeMax2 /= 2;

				}
				else if (player.HasBuff(ModContent.BuffType<AmethystSariaBuff>()) && (Main.player[Main.myPlayer].active && Main.bloodMoon))
				{
					player.buffTime[buffIndex] = 18000;
					player.statLifeMax2 /= 2;

				}
				else if (player.HasBuff(ModContent.BuffType<DiamondSariaBuff>()) && (Main.player[Main.myPlayer].active && Main.bloodMoon))
				{
					player.buffTime[buffIndex] = 18000;
					player.statLifeMax2 /= 2;

				}
				else
				{
					player.DelBuff(buffIndex);
					buffIndex--;

				}
			}
			else if (player.HasBuff(ModContent.BuffType<Soothing>()))
			{
				player.DelBuff(buffIndex);
				buffIndex--;
			}
		}
		

	}
}