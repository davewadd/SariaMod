using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SariaMod.Buffs;
using SariaMod.Dusts;
using System;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using SariaMod.Items.Sapphire;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.Sapphire
{
	public class HealBubble : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			base.DisplayName.SetDefault("Child");
			Main.projFrames[base.projectile.type] = 2;
			ProjectileID.Sets.MinionShot[base.projectile.type] = true;
			ProjectileID.Sets.TrailingMode[base.projectile.type] = 0;
			ProjectileID.Sets.TrailCacheLength[base.projectile.type] = 30;
		}

		public override void SetDefaults()
		{
			base.projectile.width = 12;
			base.projectile.height = 12;
			base.projectile.netImportant = true;
			base.projectile.friendly = false;
			base.projectile.ignoreWater = true;
			base.projectile.usesLocalNPCImmunity = true;
			base.projectile.localNPCHitCooldown = 20;
			base.projectile.minionSlots = 0f;
			base.projectile.extraUpdates = 1;

			base.projectile.penetrate = 6;
			base.projectile.tileCollide = false;
			base.projectile.timeLeft = 10000;
			base.projectile.minion = true;
		}
		public override bool? CanCutTiles()
		{
			return false;
		}
		public override bool MinionContactDamage()
		{
			Player player = Main.player[base.projectile.owner];
			FairyPlayer modPlayer = player.Fairy();
			NPC target = base.projectile.Center.MinionHoming(500f, player);
			if (projectile.frame <= 2)
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
			Player player = Main.player[projectile.owner];
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
			Player player = Main.player[base.projectile.owner];
			projectile.rotation += projectile.velocity.X * 0.15f;

			Projectile mother = Main.projectile[(int)base.projectile.ai[0]];



			if (NPC.downedMoonlord)
			{
				SariaModUtilities.HealingProjectile(base.projectile, 25, base.projectile.owner, 12f, 15f, autoHomes: true);
			}
			else if (NPC.downedPlantBoss)
			{
				SariaModUtilities.HealingProjectile(base.projectile, 20, base.projectile.owner, 12f, 15f, autoHomes: true);
			}
			else if (Main.hardMode)
			{
				SariaModUtilities.HealingProjectile(base.projectile, 15, base.projectile.owner, 12f, 15f, autoHomes: true);
			}
			else
			{
				SariaModUtilities.HealingProjectile(base.projectile, 10, base.projectile.owner, 12f, 15f, autoHomes: true);
			}
			










			int frameSpeed = 20; //reduced by half due to framecounter speedup
			projectile.frameCounter += 2;
			if (projectile.frameCounter >= frameSpeed)
			{
				base.projectile.frameCounter++;
				if (base.projectile.frameCounter > 2)
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
		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			{


				Vector2 drawPosition;

				for (int i = 1; i < 25; i++)
				{
					Texture2D texture = Main.projectileTexture[ModContent.ProjectileType<HealBubble>()];
					Vector2 startPos = base.projectile.oldPos[i] + base.projectile.Size * 0.5f - Main.screenPosition;
					int frameHeight = texture.Height / Main.projFrames[base.projectile.type];
					int frameY = frameHeight * base.projectile.frame;
					float completionRatio = (float)i / (float)base.projectile.oldPos.Length;
					Color drawColor = Color.Lerp(lightColor, Color.LightPink, 20f);
					drawColor = Color.Lerp(drawColor, Color.DarkViolet, completionRatio);
					drawColor = Color.Lerp(drawColor, Color.Transparent, completionRatio);
					Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
					Vector2 origin = rectangle.Size() / 2f;
					float rotation = base.projectile.rotation;
					float scale = base.projectile.scale;
					SpriteEffects spriteEffects = SpriteEffects.None;
					startPos.Y += 1;
					startPos.X += +17;

					if (base.projectile.spriteDirection == -1)
					{
						spriteEffects = SpriteEffects.FlipHorizontally;
					}
					Main.spriteBatch.Draw(texture, startPos, rectangle, base.projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, layerDepth: 0f);

				}
				return false;
			}
		}
	}
}

