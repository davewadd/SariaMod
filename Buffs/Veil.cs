using Microsoft.Xna.Framework;
using SariaMod.Items.Ruby;
using SariaMod.Items.Sapphire;
using SariaMod.Items.Topaz;
using SariaMod.Items.Emerald;
using SariaMod.Items.Amber;
using SariaMod.Items.Amethyst;
 
using SariaMod.Items.Platinum;
using SariaMod.Items.Strange;
using SariaMod.Dusts;
using System;
using Terraria;
using Terraria.Audio;
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
	 * This is NOT an in-depth guide to advanced minion AI
	 */

	public class Veil : ModBuff
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Veil");
			Description.SetDefault("An aqua veil surrounds You!\n Can keep you safe from the heat of hell, Space and will allow you to enter Lava\nYou can now also breath in water\n \nYou will Freeze solid in cold Biomes!!");
			Main.debuff[Type] = false;
			Main.pvpBuff[Type] = true;
			Main.buffNoTimeDisplay[base.Type] = false;
			Main.buffNoSave[Type] = true;
		}
		private int freeze;
		private const int sphereRadius = 30;
		public override void Update(Player player, ref int buffIndex)
		{
			bool Warm = player.behindBackWall && player.HasBuff(BuffID.Campfire);
			bool immunityToCold = player.HasBuff(BuffID.Warmth) || player.HasBuff(BuffID.OnFire) || player.arcticDivingGear;
			if (player.buffTime[buffIndex] == 10798)
            {
				freeze = 0;
            }
			if (freeze == 0)
			{
				player.waterWalk = true;
				player.gills = true;
				player.accFlipper = true;
				player.ignoreWater = true;
			}
				player.lavaImmune = true;
				player.fireWalk = true;
				player.lavaTime = 180000;
			
			

				if (immunityToCold == true || Warm == true)
				{

					{
						freeze = 0;
					}
				}

			
			if (player.lavaWet)
            {
				if (Main.rand.NextBool(80))
				{
					SoundEngine.PlaySound(SoundID.LiquidsHoneyLava, player.Center);
					for (int i = 0; i < 50; i++)
					{
						Vector2 speed = Main.rand.NextVector2CircularEdge(1f, 1f);
						Dust d = Dust.NewDustPerfect(player.Center, ModContent.DustType<BubbleDust2>(), speed * -2, Scale: 2.7f);
						d.noGravity = true;
					}

				}
			}
			if (player.ZoneUnderworldHeight)
			{
				if (Main.rand.NextBool(80))
				{
					SoundEngine.PlaySound(SoundID.LiquidsHoneyLava, player.Center);
					for (int i = 0; i < 50; i++)
					{
						Vector2 speed = Main.rand.NextVector2CircularEdge(1f, 1f);
						Dust d = Dust.NewDustPerfect(player.Center, ModContent.DustType<BubbleDust2>(), speed * -2, Scale: 2.7f);
						d.noGravity = true;
					}

				}
			}
			if (player.ZoneSnow)
			{
				if (immunityToCold != true && Warm != true)
				{
					if (freeze == 0)
					{
						SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/HardIce"), player.Center);
						freeze = 1;
					}
					player.frozen = true;
				}


			}
			if (freeze == 0)
				{
				if (Main.rand.NextBool(800))
				{
					SoundEngine.PlaySound(SoundID.Drown, player.Center);

				}
				if (Main.rand.NextBool(4))//controls the speed of when the sparkles spawn
				{
					float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius * sphereRadius));
					double angle = Main.rand.NextDouble() * 2.0 * Math.PI;
					Dust.NewDust(new Vector2(player.Center.X + radius * (float)Math.Cos(angle), player.Center.Y + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<BubbleDustSmall>(), 0f, 0f, 0, default(Color), 1.5f);
				}
			}
		}

		
	

		
	}
}