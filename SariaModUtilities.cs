using Terraria.ModLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using SariaMod.Items;
using System.ComponentModel;
using Terraria.ModLoader.Config;
using Microsoft.Xna.Framework;
using Terraria.GameContent.Events;




using Terraria;
using Terraria.ID;

namespace SariaMod
{
	public static class SariaModUtilities
	{
		public static void StartSandstorm()
		{
			typeof(Sandstorm).GetMethod("StartSandstorm", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
		}
		public static void StopSandstorm()
		{
			Sandstorm.Happening = false;
		}
		public static FairyPlayer Fairy(this Player player)
		{
			return player.GetModPlayer<FairyPlayer>();
		}
		public static int CountProjectiles(int Type)
		{
			return Main.projectile.Count((Projectile proj) => proj.type == Type && proj.active);
		}
		public static bool InSpace(this Player player)
		{
			float x = (float)Main.maxTilesX / 4200f;
			x *= x;
			return (float)((double)(player.position.Y / 16f - (60f + 10f * x)) / (Main.worldSurface / 6.0)) < 1f;
		}
		public static void HealingProjectile(Projectile projectile, int healing, int playerToHeal, float homingVelocity, float N, bool autoHomes = true, int timeCheck = 120)
		{
			Player player = Main.player[playerToHeal];
			float homingSpeed = homingVelocity;
			if (player.lifeMagnet)
			{
				homingSpeed *= 1.5f;
			}
			Vector2 playerVector = player.Center - projectile.Center;
			float playerDist = playerVector.Length();
			if (playerDist < 500f && projectile.position.X < player.position.X + (float)player.width && projectile.position.X + (float)projectile.width > player.position.X && projectile.position.Y < player.position.Y + (float)player.height && projectile.position.Y + (float)projectile.height > player.position.Y)
			{
				
				{
					player.HealEffect(healing, broadcast: false);
					player.statLife += healing;
					if (player.statLife > player.statLifeMax2)
					{
						player.statLife = player.statLifeMax2;
					}
					NetMessage.SendData(66, -1, -1, null, playerToHeal, healing);
				}
				if (player.ownedProjectileCounts[ModContent.ProjectileType<Heal>()] < 1f)
				{
					

					for (int j = 0; j < 1; j++) //set to 2
					{
						Projectile.NewProjectile(player.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 1f, ModContent.ProjectileType<Heal>(), 0, 0, player.whoAmI, projectile.whoAmI);

					}
				}
				projectile.Kill();
			}
			if (autoHomes)
			{
				playerDist = homingSpeed / playerDist;
				playerVector.X *= playerDist;
				playerVector.Y *= playerDist;
				projectile.velocity.X = (projectile.velocity.X * N + playerVector.X) / (N + 1f);
				projectile.velocity.Y = (projectile.velocity.Y * N + playerVector.Y) / (N + 1f);
			}
			else if (player.lifeMagnet && projectile.timeLeft < timeCheck)
			{
				playerDist = homingVelocity / playerDist;
				playerVector.X *= playerDist;
				playerVector.Y *= playerDist;
				projectile.velocity.X = (projectile.velocity.X * N + playerVector.X) / (N + 1f);
				projectile.velocity.Y = (projectile.velocity.Y * N + playerVector.Y) / (N + 1f);
			}
		}
		public static float MinionDamage(this Player player)
		{
			return player.allDamage + player.minionDamage - 1f;
		}
		public static void KillShootProjectileMany(Player player, params int[] projTypes)
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
		public static string ColorMessage(string msg, Color color)
		{
			StringBuilder stringBuilder = new StringBuilder(msg.Length + 12);
			stringBuilder.Append("[c/").Append(color.Hex4()).Append(':')
				.Append(msg)
				.Append(']');
			return stringBuilder.ToString();
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