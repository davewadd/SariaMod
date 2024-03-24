using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;

using Terraria;


using SariaMod.Buffs;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.Emerald
{
	public class Specialrupee3 : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			base.DisplayName.SetDefault("Blade");
			ProjectileID.Sets.TrailingMode[base.Projectile.type] = 2;
			ProjectileID.Sets.TrailCacheLength[base.Projectile.type] = 30;
			Main.projFrames[base.Projectile.type] = 1;
		}

		public override void SetDefaults()
		{
			base.Projectile.width = 30;
			base.Projectile.height = 30;
			
			base.Projectile.alpha = 0;
			base.Projectile.friendly = true;
			base.Projectile.tileCollide = false;
			
			base.Projectile.penetrate = 1;
			base.Projectile.timeLeft = 100;
			base.Projectile.ignoreWater = true;
			
			base.Projectile.usesLocalNPCImmunity = true;
			base.Projectile.localNPCHitCooldown = 4;
		}

		public override bool? CanHitNPC(NPC target)
        {
			return true;
        }
		public override void AI()
		{
			Player player = Main.player[base.Projectile.owner];
			Projectile mother = Main.projectile[(int)base.Projectile.ai[0]];
			FairyGlobalProjectile.HomeInOnNPC(base.Projectile, ignoreTiles: true, 600f, 25f, 20f);
			{
				float distanceFromTarget = 10f;
				Vector2 targetCenter = Projectile.position;
				bool foundTarget = false;

				// This code is required if your minion weapon has the targeting feature
				if (player.HasMinionAttackTargetNPC)
				{
					NPC npc = Main.npc[player.MinionAttackTargetNPC];
					float between = Vector2.Distance(npc.Center, player.Center);


					bool closest = Vector2.Distance(Projectile.Center, targetCenter) > between;
					bool inRange = between < distanceFromTarget;

					// Additional check for this specific minion behavior, otherwise it will stop attacking once it dashed through an enemy while flying though tiles afterwards
					// The number depends on various parameters seen in the movement code below. Test different ones out until it works alright
					bool closeThroughWall = between < 1000f;
					if (((closest && inRange) || !foundTarget) && (closeThroughWall))
					{
						distanceFromTarget = between;
						targetCenter = npc.Center;
						targetCenter.Y += 0f;
						targetCenter.X += 0f;
						foundTarget = true;
					}
				}


				if (!foundTarget)
				{
					// This code is required either way, used for finding a target
					for (int i = 0; i < Main.maxNPCs; i++)
					{
						NPC npc = Main.npc[i];
						if (npc.CanBeChasedBy())
						{
							float between = Vector2.Distance(npc.Center, player.Center);
							bool closest = Vector2.Distance(Projectile.Center, targetCenter) > between;
							bool inRange = between < distanceFromTarget;

							// Additional check for this specific minion behavior, otherwise it will stop attacking once it dashed through an enemy while flying though tiles afterwards
							// The number depends on various parameters seen in the movement code below. Test different ones out until it works alright
							bool closeThroughWall = between < 1000f;
							if (((closest && inRange) || !foundTarget) && (closeThroughWall))
							{
								distanceFromTarget = between;
								targetCenter = npc.Center;
								targetCenter.Y += 0f;
								targetCenter.X += 0f;
								foundTarget = true;
							}
						}
					}
				}
				base.Projectile.rotation += 0.075f;
				// friendly needs to be set to true so the minion can deal contact damage
				// friendly needs to be set to false so it doesn't damage things like target dummies while idling
				// Both things depend on if it has a target or not, so it's just one assignment here
				// You don't need this assignment if your minion is shooting things instead of dealing contact damage
				Projectile.friendly = foundTarget;

				if (Projectile.timeLeft == 10)
				{
					Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + 0, 0, 0, ModContent.ProjectileType<Specialrupee2>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
				}
				Lighting.AddLight(Projectile.Center, Color.Magenta.ToVector3() * 1f);
				// Default movement parameters (here for attacking)

				float inertia = 13f;
				Vector2 idlePosition = player.Center;
				float minionPositionOffsetX = ((60 + Projectile.minionPos / 80) * player.direction) - 15;
				idlePosition.Y -= 70f;
				idlePosition.X += minionPositionOffsetX;
				Vector2 vectorToIdlePosition = idlePosition - Projectile.Center;

				float distanceToIdlePosition = vectorToIdlePosition.Length();
				if (distanceFromTarget > 40f)
				{
					// The immediate range around the target (so it doesn't latch onto it when close)
					Vector2 direction = targetCenter - Projectile.Center;
					
					Projectile.velocity = (Projectile.velocity * (inertia - 2) + direction) / 20;
				}
				if (!foundTarget)
                {
					if ((distanceToIdlePosition >= 2000))
                    {
						Projectile.position = mother.Center;
                    }
					
					
					{
						
						inertia = 13;
						Vector2 direction2 = idlePosition - Projectile.Center;
						
						Projectile.velocity = (Projectile.velocity * (inertia - 2) + direction2) / 20;
					}
				}
				else if (Projectile.velocity == Vector2.Zero)
				{
					// If there is a case where it's not moving at all, give it a little "poke"
					Projectile.velocity.X = -0.15f;
					Projectile.velocity.Y = -0.05f;
				}
			}
			
		}

	

		

		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			Player player = Main.player[base.Projectile.owner];
			FairyPlayer modPlayer = player.Fairy();
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
			target.buffImmune[ModContent.BuffType<Burning2>()] = false;
			target.AddBuff(ModContent.BuffType<Burning2>(), 200);
		
			{
				Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + 0, 0, 0, ModContent.ProjectileType<Specialrupee2>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
			}

		}

		
	}
}
