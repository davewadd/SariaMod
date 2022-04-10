using Microsoft.Xna.Framework; 
using FairyMod.FaiPlayer;
using FairyMod.Projectiles;
using System;
using SariaMod.Items.Strange;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.Playerattack
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

	public class LowPP : ModBuff
	{
		public override void SetDefaults()
		{
			DisplayName.SetDefault("LowPP");
			Description.SetDefault("Saria now watches over you\n-Her foresight will detect nearby enemies\n-Psychic powers keep you from taking fall damage");
			Main.debuff[base.Type] = false;
			Main.pvpBuff[base.Type] = true;
			Main.buffNoSave[base.Type] = false;
			longerExpertDebuff = false;

		}
		public override void Update(Player player, ref int buffIndex)
		{
			if (player.HasBuff(ModContent.BuffType<LowPP>()))
			{
				player.buffTime[buffIndex] = 18000;
				buffIndex--;

			}
		
		}

	}
}