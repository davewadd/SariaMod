using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


using System.IO;
using SariaMod.Items.Strange;
using System;
using Terraria;
using SariaMod.Buffs;
using SariaMod.Items.Sapphire;
using SariaMod.Dusts;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.Emerald
{
	public class SpecialrupeeShield : ModProjectile
	{
		public override void SetStaticDefaults()
		{
		    
			base.DisplayName.SetDefault("Child");
			Main.projFrames[base.Projectile.type] = 1;
			ProjectileID.Sets.MinionShot[base.Projectile.type] = true;
			ProjectileID.Sets.TrailingMode[base.Projectile.type] = 2;
			ProjectileID.Sets.TrailCacheLength[base.Projectile.type] = 30;
			
		}
		private double rotationVariation;
		public override void SetDefaults()
		{
			base.Projectile.width = 100;
			base.Projectile.height = 100;
			base.Projectile.netImportant = true;
			base.Projectile.friendly = true;
			base.Projectile.ignoreWater = true;
			base.Projectile.usesLocalNPCImmunity = true;
			base.Projectile.localNPCHitCooldown = 7;
			base.Projectile.minionSlots = 0f;
			base.Projectile.extraUpdates = 1;
			
			base.Projectile.penetrate = -1;
			base.Projectile.tileCollide = false;
			base.Projectile.timeLeft = 300;
		}
		public override bool? CanCutTiles()
		{
			return false;
		}
		public override bool MinionContactDamage()
		{
			
			return false;
		}
		public override bool? CanHitNPC(NPC target)
		{
			return false;
		}

		private int number;
		private const int sphereRadius = 3;
		private double rotation;
		public override void SendExtraAI(BinaryWriter writer)
		{
			writer.Write(number);
		}
		public override void ReceiveExtraAI(BinaryReader reader)
		{
			number = (int)reader.ReadSingle();
		}
		public override void AI()
		{
			Player player = Main.player[base.Projectile.owner];
			FairyPlayer modPlayer = player.Fairy();
			if (number <= 10)
			{
				for (int i = 0; i < 1000; i++)
					if (Main.projectile[i].active && i != base.Projectile.whoAmI && Main.projectile[i].Hitbox.Intersects(base.Projectile.Hitbox) && Main.projectile[i].active && ((!Main.projectile[i].friendly && Main.projectile[i].hostile) || (Main.projectile[i].trap)))
					{
						Main.projectile[i].Kill();
						SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact, base.Projectile.Center);
						number += 1;
						Projectile.netUpdate = true;
					}
			}
			if (number >= 11)
            {
				Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + new Vector2(0f, -80f), Vector2.One.RotatedByRandom(6.2831854820251465) * 3f, ModContent.ProjectileType<Shard6>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
				Projectile.Kill();
			}
			if (Projectile.timeLeft <= 10)
			{
				Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + new Vector2(0f, -80f), Vector2.One.RotatedByRandom(6.2831854820251465) * 3f, ModContent.ProjectileType<Shard6>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
				Projectile.Kill();
			}
			if (player.HasBuff(ModContent.BuffType<SariaBuff>()))
			{
				Projectile.timeLeft = 300;
			}
			int SpinSpeed = 21;
			int owner = player.whoAmI;
			Vector2 This = player.Center;
			This.X += 18;
			base.Projectile.Center = This + Utils.RotatedBy(new Vector2(150f, 0f), rotation);
			rotation += SpinSpeed + rotationVariation;
			// If your minion is flying, you want to do this independently of any conditions
			Projectile.scale = 1.5f;
			Lighting.AddLight(Projectile.Center, Color.Magenta.ToVector3() * 1f);
		}

		public override void PostDraw(Color lightColor)
		{
			Player player = Main.player[base.Projectile.owner];
			FairyPlayer modPlayer = player.Fairy();
			{
				Vector2 drawPosition;

				for (int i = 1; i < 30; i++)
				{
					Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Emerald/SpecialrupeeShield");
					Vector2 startPos = base.Projectile.oldPos[i] + base.Projectile.Size * 0.5f - Main.screenPosition;
					int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
					int frameY = frameHeight * base.Projectile.frame;
					float completionRatio = (float)i / (float)base.Projectile.oldPos.Length;
					Color drawColor = Color.Lerp(lightColor, Color.LightPink, 20f);
					drawColor = Color.Lerp(drawColor, Color.DarkViolet, completionRatio);
					drawColor = Color.Lerp(drawColor, Color.Transparent, completionRatio);
					Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
					Vector2 origin = rectangle.Size() / 2f;
					float rotation = 0.23f;
					float scale = base.Projectile.scale * .98f;
					SpriteEffects spriteEffects = SpriteEffects.None;
				


					Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, layerDepth: 0f);

				}

			}
		}
	}
}

