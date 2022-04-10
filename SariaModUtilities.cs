using Terraria.ModLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.ComponentModel;
using Terraria.ModLoader.Config;
using Microsoft.Xna.Framework;
using FairyMod.Projectiles;
using FairyMod.FaiPlayer;
using FairyMod;
using Terraria;
using Terraria.ID;

namespace SariaMod
{
	public static class SariaModUtilities
	{

		public static FairyPlayer Fairy(this Player player)
		{
			return player.GetModPlayer<FairyPlayer>();
		}
		public static int CountProjectiles(int Type)
		{
			return Main.projectile.Count((Projectile proj) => proj.type == Type && proj.active);
		}

		public static float MinionDamage(this Player player)
		{
			return player.allDamage + player.minionDamage - 1f;
		}

		public static void KillShootProjectile(Player player, params int[] projTypes)
		{
			for (int x = 0; x < 1000; x++)
			{
				Projectile proj = Main.projectile[x];
				if (proj.active && proj.owner == player.whoAmI && projTypes.Contains(proj.type))
				{
					proj.Kill();
				}
			}
		}
		
		public static FairyGlobalProjectile Fairy(this Projectile proj)
		{
			return proj.GetGlobalProjectile<FairyGlobalProjectile>();
		}
		public static Item ActiveItem(this Player player)
		{
			if (!Main.mouseItem.IsAir)
			{
				return Main.mouseItem;
			}
			return player.HeldItem;
		}
		public static NPC MinionHoming(this Vector2 origin, float maxDistanceToCheck, Player owner, bool ignoreTiles = true)
		{
			if (owner == null || owner.whoAmI < 0 || owner.whoAmI > 255 || owner.MinionAttackTargetNPC < 0 || owner.MinionAttackTargetNPC > 200)
			{
				return origin.ClosestNPCAt(maxDistanceToCheck, ignoreTiles);
			}
			NPC npc = Main.npc[owner.MinionAttackTargetNPC];
			bool canHit = true;
			if (!ignoreTiles)
			{
				canHit = Collision.CanHit(origin, 1, 1, npc.Center, 1, 1);
			}
			if (owner.HasMinionAttackTargetNPC && canHit)
			{
				return npc;
			}
			return origin.ClosestNPCAt(maxDistanceToCheck, ignoreTiles);
		}
		public static NPC ClosestNPCAt(this Vector2 origin, float maxDistanceToCheck, bool ignoreTiles = true, bool bossPriority = false)
		{
			NPC closestTarget = null;
			float distance = maxDistanceToCheck;
			if (bossPriority)
			{
				bool bossFound = false;
				for (int index2 = 0; index2 < Main.npc.Length; index2++)
				{
					if ((bossFound && !Main.npc[index2].boss && Main.npc[index2].type != NPCID.WallofFleshEye) || !Main.npc[index2].CanBeChasedBy())
					{
						continue;
					}
					float extraDistance2 = Main.npc[index2].width / 2 + Main.npc[index2].height / 2;
					bool canHit2 = true;
					if (extraDistance2 < distance && !ignoreTiles)
					{
						canHit2 = Collision.CanHit(origin, 1, 1, Main.npc[index2].Center, 1, 1);
					}
					if (Vector2.Distance(origin, Main.npc[index2].Center) < distance + extraDistance2 && canHit2)
					{
						if (Main.npc[index2].boss || Main.npc[index2].type == NPCID.WallofFleshEye)
						{
							bossFound = true;
						}
						distance = Vector2.Distance(origin, Main.npc[index2].Center);
						closestTarget = Main.npc[index2];
					}
				}
			}
			else
			{
				for (int index = 0; index < Main.npc.Length; index++)
				{
					if (Main.npc[index].CanBeChasedBy())
					{
						float extraDistance = Main.npc[index].width / 2 + Main.npc[index].height / 2;
						bool canHit = true;
						if (extraDistance < distance && !ignoreTiles)
						{
							canHit = Collision.CanHit(origin, 1, 1, Main.npc[index].Center, 1, 1);
						}
						if (Vector2.Distance(origin, Main.npc[index].Center) < distance + extraDistance && canHit)
						{
							distance = Vector2.Distance(origin, Main.npc[index].Center);
							closestTarget = Main.npc[index];
						}
					}
				}
			}
			return closestTarget;
		}
		


	}
}