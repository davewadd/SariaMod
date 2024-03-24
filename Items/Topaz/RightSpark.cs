using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;

using Terraria;
using SariaMod.Buffs;
using SariaMod.Dusts;
using Terraria.GameContent;



using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.Topaz
{
	public class RightSpark : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			base.DisplayName.SetDefault("Blade");
			ProjectileID.Sets.TrailCacheLength[base.Projectile.type] = 6;
			ProjectileID.Sets.TrailingMode[base.Projectile.type] = 0;
			Main.projFrames[base.Projectile.type] = 4;
			ProjectileID.Sets.TrailingMode[base.Projectile.type] = 1;
			ProjectileID.Sets.TrailCacheLength[base.Projectile.type] = 30;
		}

		public override void SetDefaults()
		{
			base.Projectile.width = 33;
			base.Projectile.height = 40;
			
			base.Projectile.alpha = 100;
			base.Projectile.friendly = true;
			base.Projectile.tileCollide = true;
			
			base.Projectile.penetrate = -1;
			base.Projectile.timeLeft = 200;
			base.Projectile.ignoreWater = true;
			
			base.Projectile.usesLocalNPCImmunity = true;
			base.Projectile.localNPCHitCooldown = 1;
		}
		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			Player player = Main.player[base.Projectile.owner];
			Projectile.velocity.Y = -1;
			Projectile.velocity.X = 4;

			return false;

		}
		public override void AI()
		{
			Player player = Main.player[base.Projectile.owner];
			Player player2 = Main.LocalPlayer;
			FairyPlayer modPlayer = player.Fairy();
			if (Main.rand.NextBool(30))//controls the speed of when the sparkles spawn
			{
				float radius = (float)Math.Sqrt(Main.rand.Next(34 * 34));
				double angle = Main.rand.NextDouble() * 5.0 * Math.PI;


				Dust.NewDust(new Vector2(Projectile.Center.X + radius * (float)Math.Cos(angle), Projectile.Center.Y + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<StaticDust>(), 0f, 0f, 0, default(Color), 1.5f);
			}//end of dust stuff
			FairyGlobalProjectile.HomeInOnNPC(base.Projectile, ignoreTiles: true, 600f, 25f, 20f);

			{
				float between = Vector2.Distance(player2.Center, Projectile.Center);
				// Reasonable distance away so it doesn't target across multiple screens
				if (between < 100f)
				{
					player2.AddBuff(BuffID.Swiftness, 3000);

				}
			}
			{
				Projectile.velocity.X = 4;
				Projectile.velocity.Y = 1;


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
			Player player = Main.player[base.Projectile.owner];
			FairyPlayer modPlayer = player.Fairy();
			{


				Vector2 drawPosition;
				for (int i = 1; i < 10; i++)
				{
					Texture2D texture = TextureAssets.Projectile[ModContent.ProjectileType<RightSpark>()].Value;
					Vector2 startPos = base.Projectile.oldPos[i] + base.Projectile.Size * 0.5f - Main.screenPosition;
					int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
					int frameY = frameHeight * base.Projectile.frame;
					float completionRatio = (float)i / (float)base.Projectile.oldPos.Length;
					Color drawColor = Color.Lerp(lightColor, Color.LightPink, 20f);
					drawColor = Color.Lerp(drawColor, Color.DarkViolet, completionRatio);
					drawColor = Color.Lerp(drawColor, Color.Transparent, completionRatio);
					Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
					Vector2 origin = rectangle.Size() / 2f;
					float rotation = base.Projectile.rotation;
					float scale = base.Projectile.scale;
					SpriteEffects spriteEffects = SpriteEffects.None;
					startPos.Y += 1;
					startPos.X += +17;
					
					if (base.Projectile.spriteDirection == -1)
					{
						spriteEffects = SpriteEffects.FlipHorizontally;
					}
					Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, layerDepth: 0f);

				}
			}
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
		
			knockback /= 50;
			damage /= 2;
		}



	}
}
