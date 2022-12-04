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

	public class SariaCurse : ModBuff
	{
		public override void SetDefaults()
		{
			DisplayName.SetDefault("SariaCurse");
			Description.SetDefault("Saria Curses your enemies!");
			Main.debuff[base.Type] = true;
			Main.pvpBuff[base.Type] = true;
			Main.buffNoSave[base.Type] = true;
			Main.buffNoTimeDisplay[base.Type] = false;
			longerExpertDebuff = false;

		}
		private const int sphereRadius = 30;

		public override void Update(NPC npc, ref int buffIndex)
		{
			npc.buffTime[buffIndex] = 18000;
			if (!npc.boss)
			{
				npc.noTileCollide = false;
				npc.noGravity = false;
				if (Main.rand.NextBool(800))
				{
					if (npc.life > npc.lifeMax / 10)
					{
						npc.life -= (npc.lifeMax / 40);
						Main.PlaySound(base.mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Poe"), npc.Center);
					}
					if (npc.life < npc.lifeMax / 10 && npc.life > 100)
					{
						npc.life -= (npc.life / 40) + 1;
						Main.PlaySound(base.mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Poe"), npc.Center);
					}
				}
				if (Main.rand.NextBool(2))//controls the speed of when the sparkles spawn
				{
					float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius * sphereRadius));
					double angle = Main.rand.NextDouble() * 2.0 * Math.PI;
					Dust.NewDust(new Vector2(npc.Center.X + radius * (float)Math.Cos(angle), npc.Center.Y + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<Shadow>(), 0f, 0f, 0, default(Color), 1.5f);
				}
			}
			if (npc.boss)
            {
				if (Main.rand.NextBool(800))
				{
					if (npc.life > npc.lifeMax / 10)
					{
						npc.life -= (npc.lifeMax / 50);
						Main.PlaySound(base.mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Poe"), npc.Center);
					}
					if (npc.life < npc.lifeMax / 10)
					{
						npc.life -= (npc.life / 50)+1;
						Main.PlaySound(base.mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Poe"), npc.Center);
					}
				}
				if (Main.rand.NextBool(2))//controls the speed of when the sparkles spawn
				{
					float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius * sphereRadius));
					double angle = Main.rand.NextDouble() * 2.0 * Math.PI;
					Dust.NewDust(new Vector2(npc.Center.X + radius * (float)Math.Cos(angle), npc.Center.Y + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<Shadow>(), 0f, 0f, 0, default(Color), 1.5f);
				}
			}
		}
		}
}