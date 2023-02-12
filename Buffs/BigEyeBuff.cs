using Microsoft.Xna.Framework; 


using System;
using SariaMod.Items.Strange;
using Terraria;
using Terraria.ID;
using SariaMod.Items.LilHarpy;
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

	public class BigEyeBuff : ModBuff
	{
		public override void SetDefaults()
		{
			DisplayName.SetDefault("Big Eye");
			Description.SetDefault("The eye watches you!");
			Main.debuff[base.Type] = false;
			Main.pvpBuff[base.Type] = true;
			Main.buffNoSave[base.Type] = false;
			Main.buffNoTimeDisplay[base.Type] = true;
			longerExpertDebuff = false;
			Main.vanityPet[Type] = true;

		}
		public override void Update(Player player, ref int buffIndex)
		{
			if ( ((player.ownedProjectileCounts[ModContent.ProjectileType<BigEye>()] > 0f)))
            {
				player.buffTime[buffIndex] = 18000;
			}
			player.buffTime[buffIndex] = 18000;
			bool petProjectileNotSpawned = player.ownedProjectileCounts[ModContent.ProjectileType<BigEye>()] <= 0;
			if (petProjectileNotSpawned && player.whoAmI == Main.myPlayer)
			{
				Projectile.NewProjectile(player.position.X + (float)(player.width / 2), player.position.Y + (float)(player.height / 2), 0f, 0f, ModContent.ProjectileType<BigEye>(), 0, 0f, player.whoAmI, 0f, 0f);
			}
		}

	}
}