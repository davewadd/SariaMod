using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;

using Terraria;





using Terraria.ID;
using SariaMod.Dusts;
using SariaMod.Buffs;
using Terraria.ModLoader;

namespace SariaMod.Items.Amethyst
{
	public class Shadowmelt : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			base.DisplayName.SetDefault("Blade");
			ProjectileID.Sets.TrailCacheLength[base.projectile.type] = 8;
			ProjectileID.Sets.TrailingMode[base.projectile.type] = 0;
			Main.projFrames[base.projectile.type] = 10;
		}

		public override void SetDefaults()
		{
			base.projectile.width = 30;
			base.projectile.height = 30;
			base.projectile.aiStyle = 1;
			base.projectile.alpha = 0;
			base.projectile.friendly = true;
			base.projectile.tileCollide = false;
			
			base.projectile.penetrate = 1;
			base.projectile.timeLeft = 200;
			base.projectile.ignoreWater = true;
			
			base.projectile.usesLocalNPCImmunity = true;
			base.projectile.localNPCHitCooldown = 4;
		}

		private const int sphereRadius = 3;
		public override void AI()
		{
			Player player = Main.player[base.projectile.owner];
			Lighting.AddLight(base.projectile.Center, 0f, 0.5f, 0f);
			FairyGlobalProjectile.HomeInOnNPC(base.projectile, ignoreTiles: true, 600f, 25f, 20f);
			if (Main.rand.NextBool())//controls the speed of when the sparkles spawn
			{
				float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius * sphereRadius));
				double angle = Main.rand.NextDouble() * 2.0 * Math.PI;
				Dust.NewDust(new Vector2(projectile.Center.X + radius * (float)Math.Cos(angle), projectile.Center.Y + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<Shadow2>(), 0f, 0f, 0, default(Color), 1.5f);
			}
			
			if (projectile.timeLeft == 198)
			{
				Main.PlaySound(SoundID.DD2_GhastlyGlaiveImpactGhost, base.projectile.Center);
			}
			{
				float distanceFromTarget = 10f;
				Vector2 targetCenter = projectile.position;
				bool foundTarget = false;

				// This code is required if your minion weapon has the targeting feature
				if (player.HasMinionAttackTargetNPC)
				{
					NPC npc = Main.npc[player.MinionAttackTargetNPC];
					float between = Vector2.Distance(npc.Center, projectile.Center);
					// Reasonable distance away so it doesn't target across multiple screens
					if (between < 2000f)
					{
						distanceFromTarget = between;
						targetCenter = npc.Center;
						if (projectile.timeLeft >= 90)
						{
							targetCenter.Y += 50;
						}
						else if (projectile.timeLeft < 90)
						{
							targetCenter.Y = 0;
						}
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
							bool closest = Vector2.Distance(projectile.Center, targetCenter) > between;
							bool inRange = between < distanceFromTarget;

							// Additional check for this specific minion behavior, otherwise it will stop attacking once it dashed through an enemy while flying though tiles afterwards
							// The number depends on various parameters seen in the movement code below. Test different ones out until it works alright
							bool closeThroughWall = between < 1000f;
							if (((closest && inRange) || !foundTarget) && (closeThroughWall))
							{
								distanceFromTarget = between;
								targetCenter = npc.Center;
								if (projectile.timeLeft >= 90)
								{
									targetCenter.Y += 50;
								}
								else if (projectile.timeLeft < 90)
								{
									targetCenter.Y = 0;
								}
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


				float speed = 70f;
				float inertia = 20f;
				Lighting.AddLight(projectile.Center, Color.LightPink.ToVector3() * 0.78f);
				// Default movement parameters (here for attacking)
				if (projectile.timeLeft >= 120)
                {
					speed = 10f;
					
				}
				if (projectile.timeLeft < 120)
				{
					speed = 70f;
				}

				if (distanceFromTarget > 40f && projectile.timeLeft <= 400)
				{
					// The immediate range around the target (so it doesn't latch onto it when close)
					Vector2 direction = targetCenter - projectile.Center;
					direction.Normalize();
					direction *= speed;

					projectile.velocity = (projectile.velocity * (inertia - 2) + direction) / inertia;
				}
				base.projectile.frameCounter++;
				if (base.projectile.frameCounter > 3)
				{
					base.projectile.frame++;
					base.projectile.frameCounter = 0;
				}
				if (Math.Abs(projectile.velocity.X) >= 3)
				{
					if (base.projectile.frame >= 8)
				{
						base.projectile.frame = 0;
					}
				}
				else if (Math.Abs(projectile.velocity.X) < 3)
				{
					if (base.projectile.frame >= 10)
					{
						base.projectile.frame = 8;
					}
					if (base.projectile.frame < 8)
					{
						base.projectile.frame = 8;
					}
				}
			}
			if (projectile.timeLeft >= 200)
            {
				Main.PlaySound(SoundID.Item69, base.projectile.Center);
			}
			if (projectile.timeLeft == 1)
            {
				for (int j = 0; j < 1; j++) //set to 2
				{
					Projectile.NewProjectile(base.projectile.Center + new Vector2( 0f, 0f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ProjectileType<ShadowSneak>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
				}
			}
			}

	

		

		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			Player player = Main.player[base.projectile.owner];
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
			target.buffImmune[ModContent.BuffType<SariaCurse>()] = false;
			target.AddBuff(ModContent.BuffType<SariaCurse>(), 2000);
			modPlayer.SariaXp++;


			{
				Main.PlaySound(SoundID.DD2_LightningBugDeath, base.projectile.Center);
			}
			{
				Projectile.NewProjectile(base.projectile.Center + new Vector2( 0f, -135f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ProjectileType<ShadowSneak>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
			}

			if (player.HasBuff(ModContent.BuffType<StatRaise>()))
			{
				damage += (damage) / 4;
			}
			if (player.HasBuff(ModContent.BuffType<StatLower>()))
			{
				damage /= 2;

			}
		}



	}
}
