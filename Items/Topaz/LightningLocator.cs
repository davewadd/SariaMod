using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;

using Terraria;
using SariaMod.Buffs;



using SariaMod.Dusts;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.Topaz
{
	public class LightningLocator : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			base.DisplayName.SetDefault("Blade");
			ProjectileID.Sets.TrailCacheLength[base.projectile.type] = 7;
			ProjectileID.Sets.TrailingMode[base.projectile.type] = 0;
			Main.projFrames[base.projectile.type] = 4;
		}

		public override void SetDefaults()
		{
			base.projectile.width = 30;
			base.projectile.height = 30;
			
			base.projectile.alpha = 0;
			base.projectile.friendly = true;
			base.projectile.tileCollide = false;
			
			base.projectile.penetrate = 1;
			base.projectile.timeLeft = 500;
			base.projectile.ignoreWater = true;
			
			base.projectile.usesLocalNPCImmunity = true;
			base.projectile.localNPCHitCooldown = 4;
		}

		public override bool? CanHitNPC(NPC target)
        {
			return false;
        }
		public override void AI()
		{
			Player player = Main.player[base.projectile.owner];
			Projectile mother = Main.projectile[(int)base.projectile.ai[0]];
			if (Main.player[Main.myPlayer].active && !Main.player[Main.myPlayer].ZoneSnow)
			{
				if (Main.rand.NextBool(2))//controls the speed of when the sparkles spawn
				{
					float radius = (float)Math.Sqrt(Main.rand.Next(4 * 30));
					double angle = Main.rand.NextDouble() * 5.0 * Math.PI;


					Dust.NewDust(new Vector2(projectile.Center.X + radius * (float)Math.Cos(angle), (projectile.Center.Y + 10) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<Rain2>(), 0f, 0f, 0, default(Color), 1.5f);
				}//end of dust stuff
			}
			if (Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneSnow)
			{
				if (Main.rand.NextBool(20))//controls the speed of when the sparkles spawn
				{
					float radius = (float)Math.Sqrt(Main.rand.Next(4 * 30));
					double angle = Main.rand.NextDouble() * 5.0 * Math.PI;


					Dust.NewDust(new Vector2(projectile.Center.X + radius * (float)Math.Cos(angle), (projectile.Center.Y + 10) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<Snow2>(), 0f, 0f, 0, default(Color), 1.5f);
				}//end of dust stuff
			}
			FairyGlobalProjectile.HomeInOnNPC(base.projectile, ignoreTiles: true, 600f, 25f, 20f);
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
						targetCenter.Y -= 330f;
								targetCenter.X += 0f;
						foundTarget = true;
					}
				}
				if (!mother.active)
				{

					projectile.Kill();
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
								targetCenter.Y -= 330f;
								targetCenter.X += 0f;
								foundTarget = true;
							}
						}
					}
				}
				if (foundTarget)
                {
					projectile.timeLeft = 2000;
                }
				// friendly needs to be set to true so the minion can deal contact damage
				// friendly needs to be set to false so it doesn't damage things like target dummies while idling
				// Both things depend on if it has a target or not, so it's just one assignment here
				// You don't need this assignment if your minion is shooting things instead of dealing contact damage
				projectile.friendly = foundTarget;



				if ((player.ownedProjectileCounts[ModContent.ProjectileType<Static2>()] > 0f))
				{
					Projectile.NewProjectile(base.projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ProjectileType<Static>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
				}
				if ((player.ownedProjectileCounts[ModContent.ProjectileType<Static>()] > 1f)&& (player.ownedProjectileCounts[ModContent.ProjectileType<LightningStrike>()] == 0f))
				{
					Projectile.NewProjectile(base.projectile.Center + new Vector2( 2f, 160f), Vector2.One.RotatedByRandom(6.2831854820251465) * 0f, ModContent.ProjectileType<LightningStrike>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
				}
				Lighting.AddLight(projectile.Center, Color.LightGoldenrodYellow.ToVector3() * 1f);
				// Default movement parameters (here for attacking)
				float speed = 20;
				float inertia = 13f;
				Vector2 idlePosition = player.Center;
				float minionPositionOffsetX = ((60 + projectile.minionPos / 80) * player.direction) - 15;
				idlePosition.Y -= 70f;
				idlePosition.X += minionPositionOffsetX;
				Vector2 vectorToIdlePosition = idlePosition - projectile.Center;

				float distanceToIdlePosition = vectorToIdlePosition.Length();
				if (distanceFromTarget > 5f)
				{
					if (player.HasBuff(ModContent.BuffType<StatRaise>()))
					{
						projectile.position.Y = targetCenter.Y;
						projectile.position.X = targetCenter.X -12;

					}
					if (player.HasBuff(ModContent.BuffType<StatLower>()))
					{
						speed = 2;

					}
					// The immediate range around the target (so it doesn't latch onto it when close)
					Vector2 direction = targetCenter - projectile.Center;
					
					projectile.velocity = (projectile.velocity * (speed - inertia) + direction) / 20;
				}
				if (!foundTarget)
                {
					if ((distanceToIdlePosition >= 2000))
                    {
						projectile.position = mother.Center;
                    }
					
					else
					{
						
						inertia = 10;
						Vector2 direction2 = idlePosition - projectile.Center;
						
						projectile.velocity = (projectile.velocity * (inertia - 8) + direction2) / 20;
					}
				}
				else if (projectile.velocity == Vector2.Zero)
				{
					// If there is a case where it's not moving at all, give it a little "poke"
					projectile.velocity.X = -0.15f;
					projectile.velocity.Y = -0.05f;
				}
				int frameSpeed = 30; //reduced by half due to framecounter speedup
				projectile.frameCounter += 2;
				if (projectile.frameCounter >= frameSpeed)
				{
					projectile.frameCounter = 0;

					{ 
						base.projectile.frame++;
						if (base.projectile.frameCounter >= 4)
						{
							base.projectile.frame++;
							base.projectile.frameCounter = 0;

						}
						if (base.projectile.frame >= Main.projFrames[base.projectile.type])
						{
							base.projectile.frame = 0;

						}
					}
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
			target.buffImmune[ModContent.BuffType<Burning2>()] = false;
			target.AddBuff(ModContent.BuffType<Burning2>(), 200);
			target.AddBuff(BuffID.Electrified, 300);

			damage /= damage/4;
			
		}



	}
}
