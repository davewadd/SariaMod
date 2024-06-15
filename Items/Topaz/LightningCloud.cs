using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SariaMod.Items.Topaz;
using SariaMod.Items.Emerald;
using SariaMod.Items.Amber;
using SariaMod.Items.Amethyst;
using SariaMod.Items.Ruby;
using SariaMod.Items;
using SariaMod.Items.zPearls;
using System;
using SariaMod.Items.Bands;
using SariaMod.Buffs;
using SariaMod.Dusts;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using SariaMod.Items.Strange;
using SariaMod.Items.Sapphire;
using Terraria.ModLoader;
using System.IO;

namespace SariaMod.Items.Topaz
{
	public class LightningCloud : ModProjectile
	{
		public float Rotation;
		public float Rotation2;
		public int Invi;
		public override void SendExtraAI(BinaryWriter writer)
		{
			writer.Write(Rotation);
			writer.Write(Invi);
			writer.Write(Rotation2);
		}
		public override void ReceiveExtraAI(BinaryReader reader)
		{
			Rotation = (int)reader.ReadInt32();
			Invi = (int)reader.ReadInt32();
			Rotation2 = (int)reader.ReadInt32();
		}
		public override void SetStaticDefaults()
		{
			base.DisplayName.SetDefault("Saria");
			ProjectileID.Sets.TrailCacheLength[base.Projectile.type] = 7;
			ProjectileID.Sets.TrailingMode[base.Projectile.type] = 0;
			Main.projFrames[base.Projectile.type] = 1;
		}

		public override void SetDefaults()
		{
			base.Projectile.width = 30;
			base.Projectile.height = 30;

			base.Projectile.alpha = 300;
			base.Projectile.friendly = true;
			base.Projectile.tileCollide = false;

			base.Projectile.penetrate = 1;
			base.Projectile.timeLeft = 500;
			base.Projectile.ignoreWater = true;

			base.Projectile.usesLocalNPCImmunity = true;
			base.Projectile.localNPCHitCooldown = 4;
		}
		public override bool? CanCutTiles()
		{
			return false;
		}
		public override bool? CanHitNPC(NPC target)
		{
			return false;
		}
		public override void AI()
		{
			Player player = Main.player[base.Projectile.owner];
			Projectile mother = Main.projectile[(int)base.Projectile.ai[1]];
			
			if (Projectile.alpha > 100)
            {
				Projectile.alpha -= 3;
            }
			if (Main.player[Main.myPlayer].active && !Main.player[Main.myPlayer].ZoneSnow)
			{
				if (Main.rand.NextBool(2))//controls the speed of when the sparkles spawn
				{
					float radius = (float)Math.Sqrt(Main.rand.Next(4 * 30));
					double angle = Main.rand.NextDouble() * 5.0 * Math.PI;


					Dust.NewDust(new Vector2(Projectile.Center.X + radius * (float)Math.Cos(angle), (Projectile.Center.Y + 10) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<Rain2>(), 0f, 0f, 0, default(Color), 1.5f);
				}//end of dust stuff
			}
			if (Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneSnow)
			{
				if (Main.rand.NextBool(20))//controls the speed of when the sparkles spawn
				{
					float radius = (float)Math.Sqrt(Main.rand.Next(4 * 30));
					double angle = Main.rand.NextDouble() * 5.0 * Math.PI;


					Dust.NewDust(new Vector2(Projectile.Center.X + radius * (float)Math.Cos(angle), (Projectile.Center.Y + 10) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<Snow2>(), 0f, 0f, 0, default(Color), 1.5f);
				}//end of dust stuff
			}
			for (int d = 0; d < 1; d++)
			{
				
				double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
				Dust.NewDust(new Vector2(Projectile.Center.X - 10, Projectile.Center.Y - 10), 0, 0, ModContent.DustType<Clouds>(), 0f, 0f, 0, default(Color), 1.5f);

			}
			Rotation += .065f;
			Rotation2 -= .065f;






			
			{
				int head = -1;
				for (int i = 0; i < Main.maxProjectiles; i++)
				{
					if (Main.projectile[i].active && Main.projectile[i].owner == Main.myPlayer)
					{
						if (head == -1 && Main.projectile[i].type == ModContent.ProjectileType<MechwormHead>())
						{
							head = i;
						}

						if (head != -1)
						{
							break;
						}
					}
				}

				if (head == -1)
				{



					int tailIndex;
					{
						tailIndex = -1;
						if (Main.myPlayer != player.whoAmI)
							return;
						int curr = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<MechwormHead>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
						if (Main.projectile.IndexInRange(curr))
							curr = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<MechwormBody>(), (int)(Projectile.damage), Projectile.owner, player.whoAmI, Main.projectile[curr].identity, 0f);
						if (Main.projectile.IndexInRange(curr))
							Main.projectile[curr].originalDamage = Projectile.damage;
						int prev = curr;
						for (int i = 0; i < 12; i++)
							curr = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<MechwormBody>(), (int)(Projectile.damage), Projectile.owner, player.whoAmI, Main.projectile[curr].identity, 0f);
						if (Main.projectile.IndexInRange(curr))
							Main.projectile[curr].originalDamage = Projectile.damage;
						Main.projectile[prev].localAI[1] = curr;
						if ((player.ownedProjectileCounts[ModContent.ProjectileType<MechwormBody>()] <= 60f))
						{
							for (int i = 0; i < 1; i++)
								curr = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<MechwormBody>(), (int)(Projectile.damage), Projectile.owner, player.whoAmI, Main.projectile[curr].identity, 0f);
							if (Main.projectile.IndexInRange(curr))
								Main.projectile[curr].originalDamage = Projectile.damage;
							Main.projectile[prev].localAI[1] = curr;
						}

						tailIndex = curr;
					}
				}
			}



			Lighting.AddLight(Projectile.Center, Color.DimGray.ToVector3() * 4f);











			


		}

		public override bool PreDraw(ref Color lightColor)
		{
			{
				Vector2 drawPosition;

				{
					Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Topaz/Cloud");
					Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
					int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
					int frameY = frameHeight * base.Projectile.frame;
					Rectangle rectangle = texture.Frame(verticalFrames: 1, frameY: (int)Main.GameUpdateCount / 6 % 1);
					Vector2 origin = rectangle.Size() / 2f;
					float rotation = Rotation;
					float scale = base.Projectile.scale * .80f;
					SpriteEffects spriteEffects = SpriteEffects.None;
					startPos.Y -= 10;
					startPos.X -= 20;

					Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(lightColor), rotation += .075f, origin, scale, spriteEffects, 0f);
					Projectile.netUpdate = true;

				}
				{
					Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Topaz/Cloud");
					Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
					int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
					int frameY = frameHeight * base.Projectile.frame;
					Rectangle rectangle = texture.Frame(verticalFrames: 1, frameY: (int)Main.GameUpdateCount / 6 % 1);
					Vector2 origin = rectangle.Size() / 2f;
					float rotation = Rotation;
					float scale = base.Projectile.scale * .80f;
					SpriteEffects spriteEffects = SpriteEffects.None;
					startPos.Y -= 10;
					startPos.X -= 45;

					Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(lightColor), rotation += .075f, origin, scale, spriteEffects, 0f);
					Projectile.netUpdate = true;

				}
				{
					Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Topaz/Cloud");
					Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
					int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
					int frameY = frameHeight * base.Projectile.frame;
					Rectangle rectangle = texture.Frame(verticalFrames: 1, frameY: (int)Main.GameUpdateCount / 6 % 1);
					Vector2 origin = rectangle.Size() / 2f;
					float rotation = Rotation;
					float scale = base.Projectile.scale * .60f;
					SpriteEffects spriteEffects = SpriteEffects.None;
					startPos.Y -= 10;
					startPos.X -= 70;

					Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(lightColor), rotation += .075f, origin, scale, spriteEffects, 0f);
					Projectile.netUpdate = true;

				}
				{
					Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Topaz/Cloud");
					Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
					int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
					int frameY = frameHeight * base.Projectile.frame;
					Rectangle rectangle = texture.Frame(verticalFrames: 1, frameY: (int)Main.GameUpdateCount / 6 % 1);
					Vector2 origin = rectangle.Size() / 2f;
					float rotation = Rotation;
					float scale = base.Projectile.scale * .60f;
					SpriteEffects spriteEffects = SpriteEffects.None;
					startPos.Y -= 30;
					startPos.X -= 10;

					Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(lightColor), rotation += .075f, origin, scale, spriteEffects, 0f);
					Projectile.netUpdate = true;

				}
				{
					Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Topaz/Cloud");
					Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
					int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
					int frameY = frameHeight * base.Projectile.frame;
					Rectangle rectangle = texture.Frame(verticalFrames: 1, frameY: (int)Main.GameUpdateCount / 6 % 1);
					Vector2 origin = rectangle.Size() / 2f;
					float rotation = Rotation2;
					float scale = base.Projectile.scale * .60f;
					SpriteEffects spriteEffects = SpriteEffects.None;
					startPos.Y -= 30;
					startPos.X -= -10;

					Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(lightColor), rotation, origin, scale, spriteEffects, 0f);
					Projectile.netUpdate = true;

				}
				{
					Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Topaz/Cloud");
					Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
					int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
					int frameY = frameHeight * base.Projectile.frame;
					Rectangle rectangle = texture.Frame(verticalFrames: 1, frameY: (int)Main.GameUpdateCount / 6 % 1);
					Vector2 origin = rectangle.Size() / 2f;
					float rotation = Rotation2;
					float scale = base.Projectile.scale* .80f;
					SpriteEffects spriteEffects = SpriteEffects.None;
					startPos.Y -= 10;
					startPos.X -= -20;

					Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(lightColor), rotation, origin, scale, spriteEffects, 0f);
					Projectile.netUpdate = true;

				}
				{
					Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Topaz/Cloud");
					Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
					int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
					int frameY = frameHeight * base.Projectile.frame;
					Rectangle rectangle = texture.Frame(verticalFrames: 1, frameY: (int)Main.GameUpdateCount / 6 % 1);
					Vector2 origin = rectangle.Size() / 2f;
					float rotation = Rotation2;
					float scale = base.Projectile.scale * .80f;
					SpriteEffects spriteEffects = SpriteEffects.None;
					startPos.Y -= 10;
					startPos.X -= -45;

					Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(lightColor), rotation, origin, scale, spriteEffects, 0f);
					Projectile.netUpdate = true;

				}
				{
					Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Topaz/Cloud");
					Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
					int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
					int frameY = frameHeight * base.Projectile.frame;
					Rectangle rectangle = texture.Frame(verticalFrames: 1, frameY: (int)Main.GameUpdateCount / 6 % 1);
					Vector2 origin = rectangle.Size() / 2f;
					float rotation = Rotation2;
					float scale = base.Projectile.scale * .60f;
					SpriteEffects spriteEffects = SpriteEffects.None;
					startPos.Y -= 10;
					startPos.X -= -70;

					Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(lightColor), rotation += .075f, origin, scale, spriteEffects, 0f);
					Projectile.netUpdate = true;

				}
				return false;

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
			target.buffImmune[ModContent.BuffType<Burning2>()] = false;
			target.AddBuff(ModContent.BuffType<Burning2>(), 200);
			target.AddBuff(BuffID.Electrified, 300);

			damage /= damage / 4;

		}
		

	}
}
