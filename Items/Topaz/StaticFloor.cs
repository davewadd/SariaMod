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
	public class StaticFloor : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			base.DisplayName.SetDefault("Blade");
			ProjectileID.Sets.TrailCacheLength[base.projectile.type] = 6;
			ProjectileID.Sets.TrailingMode[base.projectile.type] = 0;
			Main.projFrames[base.projectile.type] = 5;
		}

		public override void SetDefaults()
		{
			base.projectile.width = 40;
			base.projectile.height = 40;
			
			base.projectile.alpha = 100;
			base.projectile.friendly = true;
			base.projectile.tileCollide = false;
			
			base.projectile.penetrate = -1;
			base.projectile.timeLeft = 1000;
			base.projectile.ignoreWater = true;
			
			base.projectile.usesLocalNPCImmunity = true;
			base.projectile.localNPCHitCooldown = 3;
		}
		
		public override void AI()
		{
			Player player = Main.player[base.projectile.owner];
			Player player2 = Main.LocalPlayer;
			FairyPlayer modPlayer = player.Fairy();
			projectile.velocity.X = 0;
			projectile.velocity.Y = 0;
			if (Main.rand.NextBool(30))//controls the speed of when the sparkles spawn
			{
				float radius = (float)Math.Sqrt(Main.rand.Next(34 * 34));
				double angle = Main.rand.NextDouble() * 5.0 * Math.PI;


				Dust.NewDust(new Vector2(projectile.Center.X + radius * (float)Math.Cos(angle), projectile.Center.Y + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<StaticDust>(), 0f, 0f, 0, default(Color), 1.5f);
			}//end of dust stuff
			FairyGlobalProjectile.HomeInOnNPC(base.projectile, ignoreTiles: true, 600f, 25f, 20f);
			Lighting.AddLight(projectile.Center, Color.LightSkyBlue.ToVector3() * 0.78f);

			{

				
				base.projectile.frameCounter++;
				if (base.projectile.frameCounter >= 2)
				{
					base.projectile.frame++;
					base.projectile.frameCounter = 0;

				}
				if (base.projectile.frame >= Main.projFrames[base.projectile.type])
				{
					base.projectile.frame = 0;

				}

				if (base.projectile.timeLeft == 9990)
				{
						{
							for (int j = 0; j < 1; j++) //set to 2
							{
							Main.PlaySound(SoundID.NPCHit34, base.projectile.Center);
							Projectile.NewProjectile(base.projectile.Center + new Vector2 ( 0f, 0f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ProjectileType<LeftSpark>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
							Projectile.NewProjectile(base.projectile.Center + new Vector2(0f, 0f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ProjectileType<RightSpark>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
						}
						}
					
					
				}
				else if (base.projectile.timeLeft == 800)
				{
					{
						for (int j = 0; j < 1; j++) //set to 2
						{
							Main.PlaySound(SoundID.NPCHit34, base.projectile.Center);
							Projectile.NewProjectile(base.projectile.Center + new Vector2(0f, 0f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ProjectileType<LeftSpark>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
							Projectile.NewProjectile(base.projectile.Center + new Vector2(0f, 0f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ProjectileType<RightSpark>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
						}
					}


				}
				else if (base.projectile.timeLeft == 600)
				{
					{
						for (int j = 0; j < 1; j++) //set to 2
						{
							Main.PlaySound(SoundID.NPCHit34, base.projectile.Center);
							Projectile.NewProjectile(base.projectile.Center + new Vector2(0f, 0f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ProjectileType<LeftSpark>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
							Projectile.NewProjectile(base.projectile.Center + new Vector2(0f, 0f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ProjectileType<RightSpark>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
						}
					}


				}
				else if (base.projectile.timeLeft == 400)
				{
					{
						for (int j = 0; j < 1; j++) //set to 2
						{
							Main.PlaySound(SoundID.NPCHit34, base.projectile.Center);
							Projectile.NewProjectile(base.projectile.Center + new Vector2(0f, 0f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ProjectileType<LeftSpark>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
							Projectile.NewProjectile(base.projectile.Center + new Vector2(0f, 0f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ProjectileType<RightSpark>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
						}
					}


				}
				else if (base.projectile.timeLeft == 200)
				{
					{
						for (int j = 0; j < 1; j++) //set to 2
						{
							Main.PlaySound(SoundID.NPCHit34, base.projectile.Center);
							Projectile.NewProjectile(base.projectile.Center + new Vector2(0f, 0f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ProjectileType<LeftSpark>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
							Projectile.NewProjectile(base.projectile.Center + new Vector2(0f, 0f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ProjectileType<RightSpark>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
						}
					}


				}

			}
			{
				float between = Vector2.Distance(player2.Center, projectile.Center);
				// Reasonable distance away so it doesn't target across multiple screens
				if (between < 100f)
				{
					player2.AddBuff(BuffID.Swiftness, 3000);

				}
			}



		}

		public override Color? GetAlpha(Color lightColor)
		{
			if (base.projectile.timeLeft < 85)
			{
				byte b2 = (byte)(base.projectile.timeLeft * 3);
				byte a2 = (byte)(100f * ((float)(int)b2 / 255f));
				return new Color(b2, b2, b2, a2);
			}
			return new Color(255, 255, 255, 100);
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			FairyGlobalProjectile.DrawCenteredAndAfterimage(base.projectile, lightColor, ProjectileID.Sets.TrailingMode[base.projectile.type]);
			return false;
		}
		
		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			Player player = Main.player[base.projectile.owner];
			FairyPlayer modPlayer = player.Fairy();
			Vector2 direction = target.Center - player.Center;
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
			modPlayer.SariaXp++;

			knockback = 0;
			damage /= 100;
		}



	}
}
