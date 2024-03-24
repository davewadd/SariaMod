using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;
using SariaMod.Dusts;
using Terraria;
using Terraria.Audio;





using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.Topaz
{
	public class Fiz : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			base.DisplayName.SetDefault("Blade");
			ProjectileID.Sets.TrailCacheLength[base.Projectile.type] = 7;
			ProjectileID.Sets.TrailingMode[base.Projectile.type] = 0;
			Main.projFrames[base.Projectile.type] = 4;
		}

		public override void SetDefaults()
		{
			base.Projectile.width = 30;
			base.Projectile.height = 30;
			
			base.Projectile.alpha = 0;
			base.Projectile.friendly = true;
			base.Projectile.tileCollide = false;
			Projectile.scale = .5f;
			
			base.Projectile.penetrate = 1;
			base.Projectile.timeLeft = 300;
			base.Projectile.ignoreWater = true;
			
			base.Projectile.usesLocalNPCImmunity = true;
			base.Projectile.localNPCHitCooldown = 4;
		}

		public override bool? CanHitNPC(NPC target)
        {
			if (Projectile.timeLeft <= 200 && !target.friendly)
            {
				return true;
            }
			else
			return false;
        }
		public override void AI()
		{
			Player player = Main.player[base.Projectile.owner];
			Projectile.rotation += .05f;
			for (int num468 = 0; num468 < 3; num468++)
			{
				int num469 = Dust.NewDust(new Vector2(base.Projectile.position.X, base.Projectile.position.Y), base.Projectile.width, base.Projectile.height, 255, 255, 93, 0, default(Color), .5f);
				Main.dust[num469].noGravity = true;
				Main.dust[num469].velocity *= 0f;
			}
			if (Main.rand.NextBool(7))//controls the speed of when the sparkles spawn
			{
				float radius = (float)Math.Sqrt(Main.rand.Next(34 * 34));
				double angle = Main.rand.NextDouble() * 5.0 * Math.PI;


				Dust.NewDust(new Vector2(Projectile.Center.X + radius * (float)Math.Cos(angle), Projectile.Center.Y + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<StaticDust>(), 0f, 0f, 0, default(Color), 1.5f);
			}//end of dust stuff
			FairyGlobalProjectile.HomeInOnNPC(base.Projectile, ignoreTiles: true, 600f, 25f, 20f);
			{
				float distanceFromTarget = 10f;
				Vector2 targetCenter = Projectile.position;
				bool foundTarget = false;

				// This code is required if your minion weapon has the targeting feature
				if (player.HasMinionAttackTargetNPC)
				{
					NPC npc = Main.npc[player.MinionAttackTargetNPC];
					float between = Vector2.Distance(npc.Center, Projectile.Center);
					// Reasonable distance away so it doesn't target across multiple screens
					if (between < 20f)
					{
						distanceFromTarget = between;
						targetCenter = npc.Center;
						targetCenter.Y = 0f;
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
							float between = Vector2.Distance(npc.Center, Projectile.Center);
							bool closest = Vector2.Distance(Projectile.Center, targetCenter) > between;
							bool inRange = between < distanceFromTarget;

							// Additional check for this specific minion behavior, otherwise it will stop attacking once it dashed through an enemy while flying though tiles afterwards
							// The number depends on various parameters seen in the movement code below. Test different ones out until it works alright
							bool closeThroughWall = between < 1000f;
							if (((closest && inRange) || !foundTarget) && (closeThroughWall))
							{
								distanceFromTarget = between;
								targetCenter = npc.Center;
								targetCenter.Y -= 0f;
								targetCenter.X += 0f;
								foundTarget = true;
							}
						}
					}
				}
				
				// friendly needs to be set to true so the minion can deal contact damage
				// friendly needs to be set to false so it doesn't damage things like target dummies while idling
				// Both things depend on if it has a target or not, so it's just one assignment here
				// You don't need this assignment if your minion is shooting things instead of dealing contact damage
				Projectile.friendly = foundTarget;
				if (Projectile.timeLeft >= 200)
				{
					if (Projectile.velocity.Y <= -1)
					{
						Projectile.velocity.Y *= -1;
					}
				}
				else if (Projectile.timeLeft < 200 && !foundTarget)
                {
					Projectile.velocity.Y = (Projectile.velocity.Y * (12 - 2) + 1) / 20;
					Projectile.velocity.X = (Projectile.velocity.X * (12 - 2) + 1) / 20;

				}
				else if (Projectile.timeLeft < 200 && foundTarget && distanceFromTarget > 1)
                {
					Vector2 direction = targetCenter - Projectile.Center;
					Projectile.velocity = (Projectile.velocity * (12 - 2) + direction) / 20;
				}

				Lighting.AddLight(Projectile.Center, Color.LightGoldenrodYellow.ToVector3() * 1f);
				// Default movement parameters (here for attacking)

				if (Projectile.timeLeft >= 300)
				{
					
					SoundEngine.PlaySound(SoundID.DD2_LightningAuraZap, base.Projectile.Center);
				}
			}



			base.Projectile.frameCounter++;
			if (base.Projectile.frameCounter >= 4)
			{
				base.Projectile.frame++;
				base.Projectile.frameCounter = 0;

			}
			if (base.Projectile.frame >= Main.projFrames[base.Projectile.type])
			{
				base.Projectile.frame = 0;

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
			target.buffImmune[BuffID.Stoned] = false;
			target.AddBuff(BuffID.Ichor, 300);
			target.AddBuff(BuffID.Electrified, 300);
			target.AddBuff(BuffID.Slow, 300);
			target.AddBuff(BuffID.Stoned, 300);

			damage /= 4;
			
		}



	}
}
