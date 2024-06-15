using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using SariaMod.Buffs;
using System.IO;
using Terraria.GameContent;
using Terraria.ID;
using SariaMod.Items.Strange;
using SariaMod.Items.Sapphire;
using SariaMod.Items.Ruby;
using SariaMod.Items.Topaz;
using SariaMod.Items.Emerald;
using System.Collections.Generic;
using SariaMod.Items.Amber;
using SariaMod.Items.Amethyst;
using Terraria.Audio;
using SariaMod.Dusts;
using Terraria.ModLoader;

namespace SariaMod.Items.Ruby
{
	public class WillOWisp : ModProjectile
	{
		public override void SetStaticDefaults()
		{
						base.DisplayName.SetDefault("Saria");
			ProjectileID.Sets.TrailCacheLength[base.Projectile.type] = 7;
			ProjectileID.Sets.TrailingMode[base.Projectile.type] = 0;
			Main.projFrames[base.Projectile.type] = 4;

		}

		public override void SetDefaults()
		{
			base.Projectile.width = 30;
			base.Projectile.height = 30;
			
			base.Projectile.alpha = 200;
			base.Projectile.friendly = true;
			base.Projectile.tileCollide = false;
			base.Projectile.netImportant = true;
			base.Projectile.penetrate = -1;
			base.Projectile.timeLeft = 3500;
			base.Projectile.ignoreWater = true;
			Projectile.scale *= 1.6f;
			base.Projectile.usesLocalNPCImmunity = true;
			base.Projectile.localNPCHitCooldown = 4;
		}
		private const int sphereRadius = 20;
		public ref float WispIndex => ref Projectile.ai[1];
		public Player player => Main.player[Projectile.owner];
		public ref float Timer => ref Projectile.ai[0];

		public int WispFluxCounter;
		public int WispFlux;
		private int WispProjCount;
		public int WispHitCooldown;
		public override void ReceiveExtraAI(BinaryReader reader)
		{
			WispFluxCounter = (int)reader.ReadInt32();
			WispFlux = (int)reader.ReadInt32();
			WispHitCooldown = (int)reader.ReadInt32();
			WispProjCount = (int)reader.ReadInt32();
		}
		public override void SendExtraAI(BinaryWriter writer)
		{
			writer.Write(WispFluxCounter);
			writer.Write(WispFlux);
			writer.Write(WispHitCooldown);
			writer.Write(WispProjCount);
		}

		public float WispPositionAngle 
		{
			get
			{
				float WispCount = (player.ownedProjectileCounts[ModContent.ProjectileType<WillOWisp>()]);
				Vector2 destination = player.Center + (Vector2.UnitX ) * player.width * 5.6f * player.direction;
				
				if (WispCount <= 1f)
					WispCount = 1f;
				return MathHelper.TwoPi * WispIndex / WispCount + Timer * 0.025f;
			}
		}
		public override bool? CanCutTiles()
		{
			return false;
		}
		public override bool? CanHitNPC(NPC target)
		{
			if (WispHitCooldown <= 0)
			{
				return target.CanBeChasedBy(base.Projectile);
			}
			return false;
		}
		
		public override void AI()
		{
			Player player = Main.player[base.Projectile.owner];
			if (Main.rand.NextBool(20))//controls the speed of when the sparkles spawn
			{
				float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius * sphereRadius));
				double angle = Main.rand.NextDouble() * 2.0 * Math.PI;
				Dust.NewDust(new Vector2(Projectile.Center.X + radius * (float)Math.Cos(angle), Projectile.Center.Y + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<ShadowFlameDust>(), 0f, 0f, 200, default(Color), 1.5f);
			}
			if (WispHitCooldown > 0)
            {
				WispHitCooldown--;
            }
			Timer++;
			Projectile.alpha = WispFlux;
			int Index = 0;
			float WispCount = (player.ownedProjectileCounts[ModContent.ProjectileType<WillOWisp>()]);
			float Mark = WispCount * 10;
			Vector2 idleDestination = player.Center + (WispPositionAngle.ToRotationVector2() * Mark);
			Projectile.Center = Vector2.Lerp(Projectile.Center, idleDestination, 0.85f);
			for (int p = 0; p < 1000; p++)
				if (Main.projectile[p].active && p != base.Projectile.whoAmI && Main.projectile[p].Hitbox.Intersects(base.Projectile.Hitbox) && Main.projectile[p].active && ((!Main.projectile[p].friendly && Main.projectile[p].hostile) || (Main.projectile[p].trap)))
				{
					
					for (int i = 0; i < 50; i++)
					{
						float radius = (float)Math.Sqrt(Main.rand.Next(34 * 34));
						double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
						Dust.NewDust(new Vector2(Projectile.Center.X + radius * (float)Math.Cos(angle), Projectile.Center.Y + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<ShadowFlameDust>(), 0f, 0f, 0, default(Color), 1.5f);
					}
					SoundEngine.PlaySound(SoundID.Item20, base.Projectile.Center);
					if (Main.rand.NextBool(11))
					{
						SoundEngine.PlaySound(SoundID.NPCDeath52, base.Projectile.Center);
					}
					else
					{
						SoundEngine.PlaySound(SoundID.NPCDeath6, base.Projectile.Center);
					}
					Main.projectile[p].Kill();
					WispProjCount++;
				}
			
			for (int g = 0; g < Main.maxProjectiles; g++)
			{
				if (Main.projectile[g].active && WispProjCount >=3 && Main.projectile[g].ModProjectile is WillOWisp modProjectile && Main.projectile[g].owner == player.whoAmI)
				{
					modProjectile.WispIndex = Index--;
					Projectile.Kill();
					Main.projectile[g].netUpdate = true;
				}

			}
			
			if (WispFluxCounter == 1)
            {
				WispFlux--;
            }
			if (WispFlux <= 0)
            {
				WispFluxCounter = 0;
            }
			if (WispFluxCounter == 0)
			{
				WispFlux++;
			}
			if (WispFlux >= 200)
            {
				WispFluxCounter = 1;
            }
			
			if (base.Projectile.localAI[0] == 0f)
			{
				if (WispCount >= 8)
				{
					for (int i = 0; i < 50; i++)
					{
						Vector2 speed = Main.rand.NextVector2CircularEdge(1f, 1f);
						Dust d = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Shadow2>(), speed * 6, Scale: 3.5f);
						d.noGravity = true;
					}
				}
				for (int d = 0; d < 50; d++)
				{
					float radius = (float)Math.Sqrt(Main.rand.Next(34 * 34));
					double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
					Dust.NewDust(new Vector2(Projectile.Center.X + radius * (float)Math.Cos(angle), Projectile.Center.Y + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<ShadowFlameDust>(), 0f, 0f, 0, default(Color), 1.5f);
				}
				for (int i = 0; i < Main.maxProjectiles; i++)
				{
					if (Main.projectile[i].active && Main.projectile[i].ModProjectile is WillOWisp modProjectile && Main.projectile[i].owner == player.whoAmI)
					{
						WispFluxCounter = modProjectile.WispFluxCounter;
						WispFlux = modProjectile.WispFlux;
						modProjectile.WispIndex = Index++;
						modProjectile.Timer = 0;
						Main.projectile[i].netUpdate = true;
					}

				}
				Projectile.localAI[0] = 1f;
			}
			
			if (player.HasBuff(ModContent.BuffType<WillOWispBuff>()))
			{
				Projectile.timeLeft = 2;
			}
			Lighting.AddLight(Projectile.Center, Color.DarkViolet.ToVector3() * 2f);
			int frameSpeed = 15;
			{
				base.Projectile.frameCounter++;
				if (Projectile.frameCounter >= frameSpeed)


					if (base.Projectile.frameCounter > 4)
					{
						base.Projectile.frame++;
						base.Projectile.frameCounter = 0;
					}
				if (base.Projectile.frame >= 4)
				{

					base.Projectile.frame = 0;
				}

			}
		}
		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
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
			target.AddBuff(ModContent.BuffType<Burning2>(), 1400);
			target.AddBuff(BuffID.Confused, 300);
			int myPlayer = Main.myPlayer;
			knockback = 10;
			if (Main.player[myPlayer].position.X + (float)(Main.player[myPlayer].width / 2) < Projectile.position.X + (float)(Projectile.width / 2))
			{
				hitDirection = 1;
			}
			else
			{
				hitDirection = -1;
			}
			damage /= 4;
			int Index = 0;
			for (int i = 0; i < 50; i++)
			{
				float radius = (float)Math.Sqrt(Main.rand.Next(34 * 34));
				double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
				Dust.NewDust(new Vector2(Projectile.Center.X + radius * (float)Math.Cos(angle), Projectile.Center.Y + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<ShadowFlameDust>(), 0f, 0f, 0, default(Color), 1.5f);
			}
			{
				for (int i = 0; i < Main.maxProjectiles; i++)
				{
					if (Main.projectile[i].active && Main.projectile[i].ModProjectile is WillOWisp modProjectile && Main.projectile[i].owner == player.whoAmI)
					{
						modProjectile.WispIndex = Index--;
						modProjectile.WispHitCooldown = 20;
						Main.projectile[i].netUpdate = true;
						SoundEngine.PlaySound(SoundID.Item20, base.Projectile.Center);
						if (Main.rand.NextBool(11))
						{
							SoundEngine.PlaySound(SoundID.NPCDeath52, base.Projectile.Center);
						}
						else 
						{
							SoundEngine.PlaySound(SoundID.NPCDeath6, base.Projectile.Center);
						}
						Projectile.Kill();
					}
				}
				Projectile.localAI[0] = 1f;
			}
		}
		
		
		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
		{
			overPlayers.Add(index);
			overWiresUI.Add(index);
		}
		public override bool PreDraw(ref Color lightColor)
		{
			{
				Texture2D starTexture2 = TextureAssets.Projectile[ModContent.ProjectileType<WillOWisp>()].Value;
				Vector2 drawPosition;

				{
					Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Ruby/WillOWispTexture");
					Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
					int frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<WillOWisp>()];
					Color drawColor = Color.Lerp(lightColor, Color.MediumPurple, 20f);
					Lighting.AddLight(Projectile.Center, Color.DarkViolet.ToVector3() * 0.78f);
					drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
					Rectangle rectangle = texture.Frame(verticalFrames: 4, frameY: (int)Main.GameUpdateCount / 6 % 4);
					Vector2 origin = rectangle.Size() / 2f;
					float rotation = 0;
					float scale = base.Projectile.scale;
					SpriteEffects spriteEffects = SpriteEffects.None;
					Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
				}
				return false;

			}



		}



	}
}
