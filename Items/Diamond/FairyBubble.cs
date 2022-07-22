using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;






using System;
using Terraria;
using SariaMod.Items.Sapphire;
using SariaMod.Dusts;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.Diamond
{
	public class FairyBubble : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			base.DisplayName.SetDefault("Child");
			Main.projFrames[base.projectile.type] = 1;
			ProjectileID.Sets.MinionShot[base.projectile.type] = true;
		}

		public override void SetDefaults()
		{
			base.projectile.width = 20;
			base.projectile.height = 20;
			base.projectile.netImportant = true;
			base.projectile.friendly = false;
			base.projectile.ignoreWater = true;
			base.projectile.usesLocalNPCImmunity = true;
			base.projectile.localNPCHitCooldown = 7;
			base.projectile.minionSlots = 0f;
			base.projectile.extraUpdates = 1;
			
			base.projectile.penetrate = -1;
			base.projectile.tileCollide = false;
			base.projectile.timeLeft = 10000;
			base.projectile.minion = false;
		}
		public override bool? CanCutTiles()
		{
			return false;
		}
		public override bool? CanHitNPC(NPC target)
		{
			
			{
				return false;
			}

			
		}
		public override bool MinionContactDamage()
		{
			
			{
				return false;
			}
		}
		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			Player player = Main.player[projectile.owner];
			target.buffImmune[BuffID.CursedInferno] = false;
			target.buffImmune[BuffID.Confused] = false;
			target.buffImmune[BuffID.Slow] = false;
			target.buffImmune[BuffID.ShadowFlame] = false;
			target.buffImmune[BuffID.Ichor] = false;
			target.buffImmune[BuffID.OnFire] = false;
			target.buffImmune[BuffID.Frostburn] = false;
			target.buffImmune[BuffID.Poisoned] = false;
			target.buffImmune[BuffID.Venom] = false;
			target.buffImmune[BuffID.Electrified] = false;
			
		}
		private const int sphereRadius = 3;
		public override void AI()
		{
			Player player = Main.player[projectile.owner];


			if (Main.rand.NextBool(5))//controls the speed of when the sparkles spawn
			{
				float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius * sphereRadius));
				double angle = Main.rand.NextDouble() * 2.0 * Math.PI;
				Dust.NewDust(new Vector2(projectile.Center.X + radius * (float)Math.Cos(angle), projectile.Center.Y + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<Sparkle>(), 0f, 0f, 0, default(Color), 1.5f);
			}


			NPC target = base.projectile.Center.MinionHoming(500f, player);
			if (target != null)
			{
				base.projectile.ai[1] += 1f;
			}
			if (projectile.frame >= 5)
            {
				projectile.Kill();
            }
			Vector2 idlePosition = player.Center;
			float minionPositionOffsetX = ((60 + projectile.minionPos / 80) * player.direction) - 15;
			idlePosition.Y -= 165f;
			idlePosition.X += minionPositionOffsetX;
			Vector2 vectorToIdlePosition = idlePosition - projectile.Center;
			float distanceToIdlePosition = vectorToIdlePosition.Length();
			if (Main.myPlayer == player.whoAmI && distanceToIdlePosition > 2000f)
			{
				// Whenever you deal with non-regular events that change the behavior or position drastically, make sure to only run the code on the owner of the projectile,
				// and then set netUpdate to true
				projectile.position = idlePosition;
				projectile.velocity *= 0.1f;
				projectile.netUpdate = true;
			}

			// If your minion is flying, you want to do this independently of any conditions
			float overlapVelocity = 0.04f;
			for (int i = 0; i < Main.maxProjectiles; i++)
			{
				// Fix overlap with other minions
				Projectile other = Main.projectile[i];
				if (i != projectile.whoAmI && other.active && other.owner == projectile.owner && Math.Abs(projectile.position.X - other.position.X) + Math.Abs(projectile.position.Y - other.position.Y) < projectile.width)
				{
					if (projectile.position.X < other.position.X) projectile.velocity.X -= overlapVelocity;
					else projectile.velocity.X += overlapVelocity;

					if (projectile.position.Y < other.position.Y) projectile.velocity.Y -= overlapVelocity;
					else projectile.velocity.Y += overlapVelocity;
				}
			}
			

			
			// Starting search distance
			float distanceFromTarget = 10f;
			Vector2 targetCenter = projectile.position;
			bool foundTarget = false;

		
			if (!foundTarget)
			{
				// This code is required either way, used for finding a target
				for (int i = 0; i < Main.maxNPCs; i++)
				{
					NPC npc = Main.npc[i];
					if (npc.CanBeChasedBy())
					{
						float between = Vector2.Distance(npc.Center, player.Center);
						bool closest = Vector2.Distance(projectile.Center, targetCenter) > between;
						bool inRange = between < distanceFromTarget;
						
						// Additional check for this specific minion behavior, otherwise it will stop attacking once it dashed through an enemy while flying though tiles afterwards
						// The number depends on various parameters seen in the movement code below. Test different ones out until it works alright
						bool closeThroughWall = between < 1000f;
						if (((closest && inRange) || !foundTarget) && (closeThroughWall))
						{
							distanceFromTarget = between;
							targetCenter = npc.Center;
							foundTarget = true;
						}
					}
				}
			}

			// friendly needs to be set to true so the minion can deal contact damage
			// friendly needs to be set to false so it doesn't damage things like target dummies while idling
			// Both things depend on if it has a target or not, so it's just one assignment here
			// You don't need this assignment if your minion is shooting things instead of dealing contact damage
			projectile.friendly = foundTarget;



			Lighting.AddLight(projectile.Center, Color.LimeGreen.ToVector3() * 0.78f);
			// Default movement parameters (here for attacking)
			float speed = 25f;
			float inertia = 12f;
			
			
			
				// Minion has a target: attack (here, fly towards the enemy)
				
					// The immediate range around the target (so it doesn't latch onto it when close)
					Vector2 direction = idlePosition - projectile.Center;
					direction.Normalize();
					direction *= speed;
					
					projectile.velocity = (projectile.velocity * (inertia - 2) + direction) / 20;

			if (foundTarget || !foundTarget)
			{
				if (distanceToIdlePosition <= 10f)
				{
					
					projectile.Kill();
				}

			}
			

			
			int frameSpeed = 20;
			{
				base.projectile.frameCounter++;
				if (projectile.frameCounter >= frameSpeed)
					if (projectile.frame >= 5)
            {
				projectile.Kill();
            }
					if (base.projectile.frameCounter > 1)
					{
						base.projectile.frame++;
						base.projectile.frameCounter = 0;
					}
				if (base.projectile.frame >= 1)
				{
					base.projectile.frame = 0;
				}
									
			}
		}
	}
}

