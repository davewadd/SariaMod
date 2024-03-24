using System;
using SariaMod.Buffs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SariaMod.Items.zPearls;
using SariaMod.Items.Bands;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;

namespace SariaMod
{
	public class FairyGlobalNPC : GlobalNPC
	{

		public override bool InstancePerEntity => true;
		
		public bool SariaCurseD;
		public bool Burning2;
		public bool Stronger;
		public bool Frostburn2;

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
			if (Stronger)
            {
				
			}
		}

		public override void AI(NPC npc)
        {
            if (npc.friendly == false && npc.lifeMax > 10)
            {

            }
            if (npc.lifeMax <= 20)
            {
				if (Main.rand.Next(4) == 0)
				{
					npc.DeathSound = new SoundStyle($"{nameof(SariaMod)}/Sounds/Blunt");
				}
				else if (Main.rand.Next(4) == 1)
				{
					npc.DeathSound = new SoundStyle($"{nameof(SariaMod)}/Sounds/Euh");
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

