using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;

using Terraria;
using SariaMod.Buffs;
using SariaMod.Dusts;
using Terraria.Audio;

using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.Topaz
{
	public class StaticFloor : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			base.DisplayName.SetDefault("Blade");
			ProjectileID.Sets.TrailCacheLength[base.Projectile.type] = 6;
			ProjectileID.Sets.TrailingMode[base.Projectile.type] = 0;
			Main.projFrames[base.Projectile.type] = 5;
		}

		public override void SetDefaults()
		{
			base.Projectile.width = 40;
			base.Projectile.height = 40;
			
			base.Projectile.alpha = 100;
			base.Projectile.friendly = true;
			base.Projectile.tileCollide = false;
			
			base.Projectile.penetrate = -1;
			base.Projectile.timeLeft = 1000;
			base.Projectile.ignoreWater = true;
			
			base.Projectile.usesLocalNPCImmunity = true;
			base.Projectile.localNPCHitCooldown = 3;
		}
		
		public override void AI()
		{
			Player player = Main.player[base.Projectile.owner];
			Player player2 = Main.LocalPlayer;
			FairyPlayer modPlayer = player.Fairy();
			Projectile.velocity.X = 0;
			Projectile.velocity.Y = 0;
			if (Main.rand.NextBool(30))//controls the speed of when the sparkles spawn
			{
				float radius = (float)Math.Sqrt(Main.rand.Next(34 * 34));
				double angle = Main.rand.NextDouble() * 5.0 * Math.PI;


				Dust.NewDust(new Vector2(Projectile.Center.X + radius * (float)Math.Cos(angle), Projectile.Center.Y + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<StaticDust>(), 0f, 0f, 0, default(Color), 1.5f);
			}//end of dust stuff
			FairyGlobalProjectile.HomeInOnNPC(base.Projectile, ignoreTiles: true, 600f, 25f, 20f);
			Lighting.AddLight(Projectile.Center, Color.LightSkyBlue.ToVector3() * 0.78f);

			{

				
				base.Projectile.frameCounter++;
				if (base.Projectile.frameCounter >= 2)
				{
					base.Projectile.frame++;
					base.Projectile.frameCounter = 0;

				}
				if (base.Projectile.frame >= Main.projFrames[base.Projectile.type])
				{
					base.Projectile.frame = 0;

				}

				if (base.Projectile.timeLeft == 9990)
				{
						{
							for (int j = 0; j < 1; j++) //set to 2
							{
							SoundEngine.PlaySound(SoundID.NPCHit34, base.Projectile.Center);
							Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + 0, 0, 0, ModContent.ProjectileType<LeftSpark>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
							Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + 0, 0, 0, ModContent.ProjectileType<RightSpark>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
						}
						}
					
					
				}
				else if (base.Projectile.timeLeft == 800)
				{
					{
						for (int j = 0; j < 1; j++) //set to 2
						{
							SoundEngine.PlaySound(SoundID.NPCHit34, base.Projectile.Center);
							Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + 0, 0, 0, ModContent.ProjectileType<LeftSpark>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
							Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + 0, 0, 0, ModContent.ProjectileType<RightSpark>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
						}
					}


				}
				else if (base.Projectile.timeLeft == 600)
				{
					{
						for (int j = 0; j < 1; j++) //set to 2
						{
							SoundEngine.PlaySound(SoundID.NPCHit34, base.Projectile.Center);
							Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + 0, 0, 0, ModContent.ProjectileType<LeftSpark>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
							Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + 0, 0, 0, ModContent.ProjectileType<RightSpark>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
						}
					}


				}
				else if (base.Projectile.timeLeft == 400)
				{
					{
						for (int j = 0; j < 1; j++) //set to 2
						{
							SoundEngine.PlaySound(SoundID.NPCHit34, base.Projectile.Center);
							Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + 0, 0, 0, ModContent.ProjectileType<LeftSpark>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
							Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + 0, 0, 0, ModContent.ProjectileType<RightSpark>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
						}
					}


				}
				else if (base.Projectile.timeLeft == 200)
				{
					{
						for (int j = 0; j < 1; j++) //set to 2
						{
							SoundEngine.PlaySound(SoundID.NPCHit34, base.Projectile.Center);
							Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + 0, 0, 0, ModContent.ProjectileType<LeftSpark>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
							Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + 0, 0, 0, ModContent.ProjectileType<RightSpark>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
						}
					}


				}

			}
			{
				float between = Vector2.Distance(player2.Center, Projectile.Center);
				// Reasonable distance away so it doesn't target across multiple screens
				if (between < 100f)
				{
					player2.AddBuff(BuffID.Swiftness, 3000);

				}
			}



		}

		public override Color? GetAlpha(Color lightColor)
		{
			if (base.Projectile.timeLeft < 85)
			{
				byte b2 = (byte)(base.Projectile.timeLeft * 3);
				byte a2 = (byte)(100f * ((float)(int)b2 / 255f));
				return new Color(b2, b2, b2, a2);
			}
			return new Color(255, 255, 255, 100);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			FairyGlobalProjectile.DrawCenteredAndAfterimage(base.Projectile, lightColor, ProjectileID.Sets.TrailingMode[base.Projectile.type]);
			return false;
		}
		
		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			Player player = Main.player[base.Projectile.owner];
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
