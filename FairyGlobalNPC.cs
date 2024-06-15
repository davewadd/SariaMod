using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SariaMod.Items.Topaz;
using SariaMod.Items.Emerald;
using SariaMod.Items.Amber;
using SariaMod.Items.Amethyst;
using SariaMod.Items.Ruby;
using SariaMod.Items;
using SariaMod.Items.zPearls;
using System;
using SariaMod.Items.Bands;
using SariaMod.Buffs;
using SariaMod.Dusts;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using SariaMod.Items.Strange;
using SariaMod.Items.Sapphire;
using Terraria.ModLoader;
using System.IO;

namespace SariaMod
{
	public class FairyGlobalNPC : GlobalNPC
	{

		public override bool InstancePerEntity => true;
		
		public bool SariaCurseD;
		public bool Burning2;
		public bool Stronger;
		public bool Frostburn2;
		private int MuchBigger;

		public override void ResetEffects(NPC npc)
		{
			SariaCurseD = false;
			Burning2 = false;
			Frostburn2 = false;
			Stronger = false;
		}
		
		public override void UpdateLifeRegen(NPC npc, ref int damage)
		{
			
			if (Frostburn2)
			{
				// These lines zero out any positive lifeRegen. This is expected for all bad life regeneration effects.
				if (npc.lifeRegen > 0)
				{
					npc.lifeRegen = 0;
				}
				npc.lifeRegen -= 16;
				if (damage < (npc.lifeMax * .005f))
				{
					damage = (int)((npc.lifeMax * .005f) + 1);
				}

			}
			if (Burning2)
			{
				// These lines zero out any positive lifeRegen. This is expected for all bad life regeneration effects.
				if (npc.lifeRegen > 0)
				{
					npc.lifeRegen = 0;
				}
				npc.lifeRegen -= 16;
				if (damage < (npc.lifeMax * .005f))
				{
					damage = (int)((npc.lifeMax * .005f) + 1);
				}
				if (SariaCurseD)
				{
					if (npc.lifeRegen > 0)
					{
						npc.lifeRegen = 0;
					}
					npc.lifeRegen -= 16;
					if (damage < (npc.lifeMax * .01f))
					{
						damage = (int)((npc.lifeMax * .01f) + 1);
					}
					if (!npc.boss)
					{
						npc.noTileCollide = false;
						npc.noGravity = false;
					}
				}
				
			}
			
		}
		public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
			
		{
			if (Main.player[Main.myPlayer].ZoneCorrupt)
			{
				spawnRate = (int)((double)spawnRate * .5f);
				maxSpawns = (int)((float)maxSpawns * 4f);
			}
			if (Main.bloodMoon)
            {
				spawnRate = (int)((double)spawnRate * .000001f);
				maxSpawns = (int)((float)maxSpawns * 30f);
			}
			
			if (Main.moonPhase == 0 && !Main.dayTime)
            {
				spawnRate = (int)((double)spawnRate * .000000009f);
				maxSpawns = (int)((float)maxSpawns * 50f);
			}
			

		}
		public override void OnHitPlayer(NPC npc, Player target, int damage, bool crit)
		{
		}
		public override void AI(NPC npc)
        {
			
            if (npc.friendly == false && npc.lifeMax > 10)
            {

            }
            if (npc.lifeMax <= 25)
            {
				if (Main.rand.Next(4) == 0)
				{
					npc.DeathSound = new SoundStyle($"{nameof(SariaMod)}/Sounds/Blunt");
				}
				else if (Main.rand.Next(4) == 1)
				{
					npc.DeathSound = new SoundStyle($"{nameof(SariaMod)}/Sounds/Death5");
				}
				else if (Main.rand.Next(4) == 1)
				{
					npc.DeathSound = new SoundStyle($"{nameof(SariaMod)}/Sounds/Die");
				}
				else if (Main.rand.Next(4) == 1)
				{
					npc.DeathSound = new SoundStyle($"{nameof(SariaMod)}/Sounds/Die2");
				}
				else if (Main.rand.Next(4) == 1)
				{
					npc.DeathSound = new SoundStyle($"{nameof(SariaMod)}/Sounds/Die3");
				}

			}
			if (!npc.boss && !npc.townNPC && npc.lifeMax > 10)
			{
				if (Main.rand.Next(800000) == 5)
				{
					SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/BeatMe"), npc.Center);
				}

			}
			if (!npc.boss && !npc.townNPC && npc.lifeMax > 25)

			{
				if (Main.rand.Next(800000) == 5)
				{
					npc.DeathSound = new SoundStyle($"{nameof(SariaMod)}/Sounds/Death6");
				}
			}
		}
        public override void OnKill(NPC npc)
		{
			if (npc.type == NPCID.EyeofCthulhu)
			{
				if (Main.rand.Next(4) == 0)
				{
					Item.NewItem(npc.GetSource_FromThis(), (int)(npc.position.X + 0), (int)(npc.position.Y + 0), 0, 0, ModContent.ItemType<SusEye>());
				}
			}
			if (!npc.SpawnedFromStatue)
			{
				
				
				if (Main.rand.Next(50) == 0)
				{
					Item.NewItem(npc.GetSource_FromThis(), (int)(npc.position.X + 0), (int)(npc.position.Y + 0), 0, 0, ModContent.ItemType<XpPearl>());
				}
				if (Main.rand.Next(70) == 0)
				{
					Item.NewItem(npc.GetSource_FromThis(), (int)(npc.position.X + 0), (int)(npc.position.Y + 0), 0, 0, ModContent.ItemType<FrozenYogurt>());
				}
				if (Main.rand.Next(150) == 0)
				{
					Item.NewItem(npc.GetSource_FromThis(), (int)(npc.position.X + 0), (int)(npc.position.Y + 0), 0, 0, ModContent.ItemType<MediumXpPearl>());
				}
				if (Main.rand.Next(300) == 0)
				{
					Item.NewItem(npc.GetSource_FromThis(), (int)(npc.position.X + 0), (int)(npc.position.Y + 0), 0, 0, ModContent.ItemType<SariasConfect>());
				}
				if (Main.rand.Next(600) == 0)
				{
					Item.NewItem(npc.GetSource_FromThis(), (int)(npc.position.X + 0), (int)(npc.position.Y + 0), 0, 0, ModContent.ItemType<LargeXpPearl>());
				}
				if (Main.rand.Next(1000) == 0)
				{
					Item.NewItem(npc.GetSource_FromThis(), (int)(npc.position.X + 0), (int)(npc.position.Y + 0), 0, 0, ModContent.ItemType<SoaringConcoction>());
				}
				if (Main.rand.Next(25000) == 0)
				{
					Item.NewItem(npc.GetSource_FromThis(), (int)(npc.position.X + 0), (int)(npc.position.Y + 0), 0, 0, ModContent.ItemType<RareXpPearl>());
				}
			}
			
			
		}
		}
}

