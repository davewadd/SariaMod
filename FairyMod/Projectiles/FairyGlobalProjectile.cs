using System;
using FairyMod.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace FairyMod.Projectiles
{
	public class FairyGlobalProjectile : GlobalProjectile
	{
		public bool forceMinion;

		public float spawnedPlayerMinionDamageValue = 1f;

		public int spawnedPlayerMinionProjectileDamageValue;

		public bool overridesMinionDamagePrevention;
		public override bool InstancePerEntity => true;

		
			public static void DrawCenteredAndAfterimage(Projectile projectile, Color lightColor, int trailingMode, int typeOneDistanceMultiplier = 1, Texture2D texture = null, bool drawCentered = true)
		{
			if (texture == null)
			{
				texture = Main.projectileTexture[projectile.type];
			}
			int frameHeight = texture.Height / Main.projFrames[projectile.type];
			int frameY = frameHeight * projectile.frame;
			float scale = projectile.scale;
			float rotation = projectile.rotation;
			Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
			Vector2 origin = rectangle.Size() / 2f;
			SpriteEffects spriteEffects = SpriteEffects.None;
			if (projectile.spriteDirection == -1)
			{
				spriteEffects = SpriteEffects.FlipHorizontally;
			}
			if (FairyConfig.Instance.Afterimages)
			{
				Vector2 centerOffset = (drawCentered ? (projectile.Size / 2f) : Vector2.Zero);
				switch (trailingMode)
				{
					case 0:
						{
							for (int i = 0; i < projectile.oldPos.Length; i++)
							{
								Vector2 drawPos = projectile.oldPos[i] + centerOffset - Main.screenPosition + new Vector2(0f, projectile.gfxOffY);
								Color color = projectile.GetAlpha(lightColor) * ((float)(projectile.oldPos.Length - i) / (float)projectile.oldPos.Length);
								Main.spriteBatch.Draw(texture, drawPos, rectangle, color, rotation, origin, scale, spriteEffects, 0f);
							}
							break;
						}
					case 1:
						{
							Color drawColor = projectile.GetAlpha(lightColor);
							int afterimageCount = ProjectileID.Sets.TrailCacheLength[projectile.type];
							for (int k = 0; k < afterimageCount; k += typeOneDistanceMultiplier)
							{
								Vector2 drawPos2 = projectile.oldPos[k] + centerOffset - Main.screenPosition + new Vector2(0f, projectile.gfxOffY);
								if (k > 0)
								{
									float colorMult = afterimageCount - k;
									drawColor *= colorMult / ((float)afterimageCount * 1.5f);
								}
								Main.spriteBatch.Draw(texture, drawPos2, rectangle, drawColor, rotation, origin, scale, spriteEffects, 0f);
							}
							break;
						}
					case 2:
						{
							for (int j = 0; j < projectile.oldPos.Length; j++)
							{
								float afterimageRot = projectile.oldRot[j];
								SpriteEffects sfxForThisAfterimage = ((projectile.oldSpriteDirection[j] == -1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
								Vector2 drawPos3 = projectile.oldPos[j] + centerOffset - Main.screenPosition + new Vector2(0f, projectile.gfxOffY);
								Color color2 = projectile.GetAlpha(lightColor) * ((float)(projectile.oldPos.Length - j) / (float)projectile.oldPos.Length);
								Main.spriteBatch.Draw(texture, drawPos3, rectangle, color2, afterimageRot, origin, scale, sfxForThisAfterimage, 0f);
							}
							break;
						}
				}
			}
			if (!FairyConfig.Instance.Afterimages || ProjectileID.Sets.TrailCacheLength[projectile.type] <= 0)
			{
				Vector2 startPos = (drawCentered ? projectile.Center : projectile.position);
				Main.spriteBatch.Draw(texture, startPos - Main.screenPosition + new Vector2(0f, projectile.gfxOffY), rectangle, projectile.GetAlpha(lightColor), rotation, origin, scale, spriteEffects, 0f);
			}
		}
		
			public static void HomeInOnNPC(Projectile projectile, bool ignoreTiles, float distanceRequired, float homingVelocity, float N)
		{
			Vector2 center = projectile.Center;
			bool homeIn = false;
			for (int i = 0; i < 200; i++)
			{
				if (Main.npc[i].CanBeChasedBy(projectile))
				{
					float extraDistance = Main.npc[i].width / 2 + Main.npc[i].height / 2;
					bool canHit = true;
					if (extraDistance < distanceRequired && !ignoreTiles)
					{
						canHit = Collision.CanHit(projectile.Center, 1, 1, Main.npc[i].Center, 1, 1);
					}
					if (Vector2.Distance(Main.npc[i].Center, projectile.Center) < distanceRequired + extraDistance && canHit)
					{
						center = Main.npc[i].Center;
                        break;
					}
				}

			}
			if (!projectile.friendly)
			{
				homeIn = false;
			}
			if (homeIn)
			{
				Vector2 homeInVector = projectile.DirectionTo(center);
				if (homeInVector.HasNaNs())
				{
					homeInVector = Vector2.UnitY;
				}
				projectile.velocity = (projectile.velocity * N + homeInVector * homingVelocity) / (N + 1f);
			}
		}
	}
}

