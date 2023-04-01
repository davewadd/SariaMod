using System;
using SariaMod.Buffs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

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
			
            
		}
		public override void NPCLoot(NPC npc)
		{
			if (npc.type == NPCID.EyeofCthulhu)
			{
				if (Main.rand.Next(4) == 0)
				{
					Item.NewItem((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height, base.mod.ItemType("SusEye"), 1, noBroadcast: false, 81);
				}
			}
			if (!npc.SpawnedFromStatue)
			{
				
				
				if (Main.rand.Next(50) == 0)
				{
					Item.NewItem((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height, base.mod.ItemType("XpPearl"), 1, noBroadcast: false, 83);
				}
				if (Main.rand.Next(150) == 0)
				{
					Item.NewItem((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height, base.mod.ItemType("MediumXpPearl"), 1, noBroadcast: false, 82);
				}
				if (Main.rand.Next(600) == 0)
				{
					Item.NewItem((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height, base.mod.ItemType("LargeXpPearl"));
				}
				if (Main.rand.Next(25000) == 0)
				{
					Item.NewItem((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height, base.mod.ItemType("RareXpPearl"));
				}
			}
			
		}
		}
}

