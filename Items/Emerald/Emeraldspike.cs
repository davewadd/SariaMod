using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;


using Terraria;
using SariaMod.Buffs;
using Terraria.Audio;



using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.Emerald
{
	public class Emeraldspike : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			base.DisplayName.SetDefault("Blade");
			ProjectileID.Sets.TrailCacheLength[base.Projectile.type] = 6;
			ProjectileID.Sets.TrailingMode[base.Projectile.type] = 0;
			Main.projFrames[base.Projectile.type] = 3;
		}

		public override void SetDefaults()
		{
			base.Projectile.width = 150;
			base.Projectile.height = 150;
			base.Projectile.aiStyle = 21;
			base.Projectile.alpha = 100;
			base.Projectile.friendly = true;
			base.Projectile.tileCollide = false;
			base.Projectile.penetrate = -1;
			base.Projectile.timeLeft = 2000;
			base.Projectile.ignoreWater = true;
			AIType = 274;
			base.Projectile.usesLocalNPCImmunity = true;
			base.Projectile.localNPCHitCooldown = 20;
		}
		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			Projectile.velocity.Y = 0;
			
				return false;

		}
		public override void AI()
		{
			Player player = Main.player[base.Projectile.owner];
			Player player2 = Main.LocalPlayer;
			FairyPlayer modPlayer = player.Fairy();
			if (player.HasBuff(ModContent.BuffType<StatRaise>()))
			{
				Projectile.localNPCHitCooldown = 16;
			}
			if (player.HasBuff(ModContent.BuffType<StatLower>()))
			{
				Projectile.localNPCHitCooldown = 160;

			}
			if (player.HasBuff(ModContent.BuffType<Overcharged>()))
			{
				Projectile.localNPCHitCooldown = 14;
			}
			FairyGlobalProjectile.HomeInOnNPC(base.Projectile, ignoreTiles: true, 600f, 25f, 20f);
			Lighting.AddLight(Projectile.Center, Color.LimeGreen.ToVector3() * 0.78f);
			float distanceFromTarget = 10f;
			Vector2 targetCenter = Projectile.position;
			bool foundTarget = false;
			float speed = 20;
			Vector2 direction = targetCenter - Projectile.Center;


			if (Projectile.timeLeft >= 1990)
			{
				base.Projectile.velocity.X = (float)((.001) * player.direction);
			}

			
			
			if (Projectile.velocity.X > 0)
			{
				Projectile.spriteDirection = 1;
			}
			if (Projectile.velocity.X < 0)
			{
				Projectile.spriteDirection = -1;
			}
			

			{
				float between = Vector2.Distance(player2.Center, Projectile.Center);
				// Reasonable distance away so it doesn't target across multiple screens
				if (between < 1000f)
				{
					player2.AddBuff(BuffID.Endurance, 3);

				}
			}


			int frameSpeed = 15;
			{
				base.Projectile.frameCounter++;
				if (Projectile.frameCounter >= frameSpeed)


					if (base.Projectile.frameCounter > 3)
					{
						base.Projectile.frame++;
						base.Projectile.frameCounter = 0;
					}
				if (base.Projectile.frame >= 3)
				{

					base.Projectile.frame = 2;
				}

			}
			
			if (!foundTarget)
			{
				// This code is required either way, used for finding a target
				for (int i = 0; i < Main.maxNPCs; i++)
				{
					NPC npc = Main.npc[i];
					if (npc.CanBeChasedBy())
					{
						float between = Vector2.Distance(npc.Center, player.Center);
						bool closest = Vector2.Distance(Projectile.Center, targetCenter) > between;
						bool inRange = between < distanceFromTarget;

						// Additional check for this specific minion behavior, otherwise it will stop attacking once it dashed through an enemy while flying though tiles afterwards
						// The number depends on various parameters seen in the movement code below. Test different ones out until it works alright
						bool closeThroughWall = between < 1000f;
						if (((closest && inRange) || !foundTarget) && (closeThroughWall))
						{
							distanceFromTarget = between;
							targetCenter = npc.Center;
							targetCenter.Y -= 0f;
							targetCenter.X += 0f;
							foundTarget = true;
						}
					}
				}
			}
			if ((player.ownedProjectileCounts[ModContent.ProjectileType<Emeraldspike>()] >= 4f) && (!player.HasBuff(ModContent.BuffType<Overcharged>())))
            {
				Projectile.timeLeft -= 2;
            }
			if (Projectile.frame == 0)
			{
				SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact, base.Projectile.Center);
			}
			if (Projectile.frame == 1)
			{
				SoundEngine.PlaySound(SoundID.DD2_WitherBeastDeath, base.Projectile.Center);
			}
			if (Projectile.frame == 1)
			{
				SoundEngine.PlaySound(SoundID.DD2_DarkMageHealImpact, base.Projectile.Center);
			}
			if (Projectile.timeLeft <= 50)
			{
				for (int j = 0; j < 5; j++) //set to 2
				{
					Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 1f, ModContent.ProjectileType<Crystalshard>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
				}
				SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact, base.Projectile.Center);
				Projectile.Kill();
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
			Player player2 = Main.LocalPlayer;
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
			target.AddBuff(BuffID.Electrified, 300);
			target.AddBuff(BuffID.Slow, 300);
			Projectile.timeLeft -= 15;
			modPlayer.SariaXp++;
			if (!player.HasBuff(ModContent.BuffType<Overcharged>()))
			{
				if (Main.rand.NextBool(100))

				{
					{
						Item.NewItem(Projectile.GetSource_FromThis(), Projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), ModContent.ItemType<LivingGreenShard>());
					}
				}
			}
			if (player.HasBuff(ModContent.BuffType<Overcharged>()))
			{
				if (Main.rand.NextBool(20))

				{
					Item.NewItem(Projectile.GetSource_FromThis(), Projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), ModContent.ItemType<LivingGreenShard>());
				}
				float between = Vector2.Distance(player2.Center, Projectile.Center);
				// Reasonable distance away so it doesn't target across multiple screens
				if (between < 120f)
                {
					player.statLife += 10;
				}
		}
				if (player.HasBuff(ModContent.BuffType<StatRaise>()))
			{
				damage += (damage) / 4;
			}
			if (player.HasBuff(ModContent.BuffType<StatLower>()))
			{
				damage /= 2;

			}
		}



	}
}
