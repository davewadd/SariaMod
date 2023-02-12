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
using SariaMod.Dusts;
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

	public class Sickness : ModBuff
	{
		public override void SetDefaults()
		{
			DisplayName.SetDefault("Old Wounds");
			Description.SetDefault("Saria's old wounds begin to cause agony\nShe desperately needs a break!\nCrafting Frozen Yogurt or Increasing her mood may help!\nThe pain is Synchronized!");
			Main.debuff[base.Type] = true;
			Main.pvpBuff[base.Type] = false;
			Main.buffNoSave[base.Type] = false;
			Main.buffNoTimeDisplay[base.Type] = false;
			longerExpertDebuff = false;

		}
		private const int sphereRadius = 10;
		public override void Update(Player player, ref int buffIndex)
		{
			if (!player.HasBuff(ModContent.BuffType<Soothing>()))
			{

				if ((player.ownedProjectileCounts[ModContent.ProjectileType<Saria>()] > 0))
				{
					player.buffTime[buffIndex] = 5000;
					player.accRunSpeed = .5f;
					player.moveSpeed = .5f;
					player.maxRunSpeed = .005f;
					player.runAcceleration = .2f;
					player.statLifeMax2 /= 2;
					if (Main.rand.NextBool(20))//controls the speed of when the sparkles spawn
					{
						float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius * sphereRadius));
						double angle = Main.rand.NextDouble() * 2.0 * Math.PI;
						Dust.NewDust(new Vector2(player.Center.X + radius * (float)Math.Cos(angle), player.Center.Y + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<ShadowFlameDust>(), 0f, 0f, 0, default(Color), 1.5f);
					}
					if (Main.rand.NextBool(600))
					{
						Main.PlaySound(base.mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Poe"), player.Center);
					}
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