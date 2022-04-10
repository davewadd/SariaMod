using System;
using FairyMod.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace FairyMod.Projectiles
{
	public class FairyGlobalNPC : GlobalNPC
	{
		public bool LinkCable;

		

		public override bool InstancePerEntity => true;

		
		public override void UpdateLifeRegen(NPC npc, ref int damage)
		{
			if (LinkCable)
			{
				npc.lifeRegen -= 999999;
				if (damage < 10)
				{
					damage = 9999;
				}
			}
		}
	}
}

