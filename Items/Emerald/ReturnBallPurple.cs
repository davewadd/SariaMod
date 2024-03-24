using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SariaMod.Buffs;
using SariaMod.Dusts;
using System;
using SariaMod.Items.Strange;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using SariaMod.Items.Sapphire;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.Emerald
{
	public class ReturnBallPurple : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			base.DisplayName.SetDefault("Child");
			Main.projFrames[base.Projectile.type] = 2;
			ProjectileID.Sets.MinionShot[base.Projectile.type] = true;
			ProjectileID.Sets.TrailingMode[base.Projectile.type] = 0;
			ProjectileID.Sets.TrailCacheLength[base.Projectile.type] = 30;
		}

		public override void SetDefaults()
		{
			base.Projectile.width = 12;
			base.Projectile.height = 12;
			base.Projectile.netImportant = true;
			base.Projectile.friendly = false;
			base.Projectile.ignoreWater = true;
			base.Projectile.usesLocalNPCImmunity = true;
			base.Projectile.localNPCHitCooldown = 20;
			base.Projectile.minionSlots = 0f;
			base.Projectile.extraUpdates = 1;

			base.Projectile.penetrate = 6;
			base.Projectile.tileCollide = false;
			base.Projectile.timeLeft = 10000;
			base.Projectile.minion = true;
		}
		public override bool? CanCutTiles()
		{
			return false;
		}
		public override bool MinionContactDamage()
		{
			Player player = Main.player[base.Projectile.owner];
			FairyPlayer modPlayer = player.Fairy();
			NPC target = base.Projectile.Center.MinionHoming(500f, player);
			if (Projectile.frame <= 2)
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			Player player = Main.player[Projectile.owner];
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
			if (Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneUnderworldHeight)
			{
				damage = damage * 1;
			}
			if (Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneSnow)
			{
				damage /= 3;
			}
			else
			{
				damage /= 2;
			}

			target.buffImmune[ModContent.BuffType<Frostburn2>()] = false;
			target.AddBuff(ModContent.BuffType<Frostburn2>(), 200);
			knockback /= 100;

		}
		public override void AI()
		{
			Player player = Main.player[base.Projectile.owner];
			Projectile.rotation += Projectile.velocity.X * 0.15f;

			Projectile mother = Main.projectile[(int)base.Projectile.ai[1]];
			float speed = 8f;
			float inertia = 20f;

			Vector2 idlePosition = player.Center;
			Vector2 vectorToIdlePosition = idlePosition - Projectile.Center;
			float distanceToIdlePosition = vectorToIdlePosition.Length();
			// Default movement parameters (here for attacking)
			Projectile.rotation = Projectile.velocity.X * 0.05f;

			{

				{
					// Minion doesn't have a target: return to player and idle
					
						// Speed up the minion if it's away from the player
						speed = 30f;
					inertia = 100;
					
					
					
					{
						// The immediate range around the player (when it passively floats about)

						// This is a simple movement formula using the two parameters and its desired direction to create a "homing" movement
						vectorToIdlePosition.Normalize();
						vectorToIdlePosition *= speed;
						Projectile.velocity = (Projectile.velocity * (inertia - 1) + vectorToIdlePosition) / inertia;
					}

					if (distanceToIdlePosition < 40f)
					{
						for (int j = 0; j < 72; j++)
						{
							Dust dust = Dust.NewDustPerfect(Projectile.Center, 113);
							dust.velocity = ((float)Math.PI * 2f * Vector2.Dot(((float)j / 72f * ((float)Math.PI * 2f)).ToRotationVector2(), player.velocity.SafeNormalize(Vector2.UnitY).RotatedBy((float)j / 72f * ((float)Math.PI * -2f)))).ToRotationVector2();
							dust.velocity = dust.velocity.RotatedBy((float)j / 36f * ((float)Math.PI * 2f)) * 8f;
							dust.noGravity = true;
							dust.scale *= 3.9f;
						}
						SoundEngine.PlaySound(SoundID.Item30, base.Projectile.Center);
						Projectile.Kill();
					}

				}
			}
			if (Projectile.timeLeft == 10000)
            {
				SoundEngine.PlaySound(SoundID.Item43, base.Projectile.Center);
				SoundEngine.PlaySound(SoundID.Item9, base.Projectile.Center);
				SoundEngine.PlaySound(SoundID.Item77, base.Projectile.Center);
			}


					int frameSpeed = 20; //reduced by half due to framecounter speedup
					Projectile.frameCounter += 2;
			if (Projectile.frameCounter >= frameSpeed)
			{
				base.Projectile.frameCounter++;
				if (base.Projectile.frameCounter > 2)
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
		public override bool PreDraw(ref Color lightColor)
		{
			{


				Vector2 drawPosition;

				for (int i = 1; i < 25; i++)
				{
					Texture2D texture = TextureAssets.Projectile[ModContent.ProjectileType<HealBubble>()].Value;
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
				return false;
			}
		}
	}
}

