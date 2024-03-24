using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SariaMod.Buffs;
using SariaMod.Dusts;
using SariaMod;
using System;
using Terraria;
using SariaMod.Items.Sapphire;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.Sapphire
{
	public class HealBubble : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			base.DisplayName.SetDefault("Child");
			Main.projFrames[base.Projectile.type] = 2;
			ProjectileID.Sets.MinionShot[base.Projectile.type] = true;
			ProjectileID.Sets.TrailingMode[base.Projectile.type] = 0;
			ProjectileID.Sets.TrailCacheLength[base.Projectile.type] = 30;
		}
		public int owner = 255;
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
		
		public override void AI()
		{
			Player player = Main.player[Projectile.owner];
			Player player2 = Main.LocalPlayer;
			Projectile.rotation += Projectile.velocity.X * 0.15f;
			float num3 = 0f;
			int num4 = owner;
			for (int i = 0; i < 255; i++)
			{
				if (Main.player[i].active && !Main.player[i].dead && ((!Main.player[owner].hostile && !Main.player[i].hostile) || Main.player[owner].team == Main.player[i].team))
				{
					int num5 = Main.player[i].statLifeMax2 - Main.player[i].statLife;
					if ((float)num5 > num3)
					{
						num3 = num5;
						num4 = i;
					}
				}
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

