using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;


using Terraria;
using SariaMod.Buffs;


using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.Emerald
{
	public class Silverspike2 : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			base.DisplayName.SetDefault("Blade");
			ProjectileID.Sets.TrailCacheLength[base.projectile.type] = 6;
			ProjectileID.Sets.TrailingMode[base.projectile.type] = 0;
			Main.projFrames[base.projectile.type] = 1;
		}

		public override void SetDefaults()
		{
			base.projectile.width = 300;
			base.projectile.height = 500;
			base.projectile.aiStyle = 21;
			base.projectile.alpha = 20;
			base.projectile.friendly = true;
			base.projectile.tileCollide = false;
			
			base.projectile.penetrate = -1;
			base.projectile.timeLeft = 2000;
			base.projectile.ignoreWater = true;
			aiType = 274;
			base.projectile.usesLocalNPCImmunity = true;
			base.projectile.localNPCHitCooldown = 20;
		}
		
		public override void AI()
		{
			Player player = Main.player[base.projectile.owner];
			Player player2 = Main.LocalPlayer;
			FairyPlayer modPlayer = player.Fairy();
			if (player.HasBuff(ModContent.BuffType<StatRaise>()))
			{
				projectile.localNPCHitCooldown = 16;
			}
			if (player.HasBuff(ModContent.BuffType<StatLower>()))
			{
				projectile.localNPCHitCooldown = 160;

			}
			projectile.velocity.X = 0;
			projectile.velocity.Y = 0;
			{
				projectile.localNPCHitCooldown = 14;
			}
			for (int i = 0; i < 1000; i++)
				if (Main.projectile[i].active && i != base.projectile.whoAmI && Main.projectile[i].Hitbox.Intersects(base.projectile.Hitbox) && Main.projectile[i].active && ((!Main.projectile[i].friendly && Main.projectile[i].hostile) || (Main.projectile[i].trap)))
				{
					Main.projectile[i].Kill();
				}
			FairyGlobalProjectile.HomeInOnNPC(base.projectile, ignoreTiles: true, 600f, 25f, 20f);
			Lighting.AddLight(projectile.Center, Color.Silver.ToVector3() * 2f);
			float distanceFromTarget = 10f;
			Vector2 targetCenter = projectile.position;
			bool foundTarget = false;
			float speed = 20;
			Vector2 direction = targetCenter - projectile.Center;
			
			
			{
				float between = Vector2.Distance(player2.Center, projectile.Center);
				// Reasonable distance away so it doesn't target across multiple screens
				if (between < 1000f)
				{
					player2.AddBuff(BuffID.Endurance, 3);

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
						bool closest = Vector2.Distance(projectile.Center, targetCenter) > between;
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
			
				
			
			if (projectile.timeLeft <= 50)
			{
				for (int j = 0; j < 4; j++) //set to 2
				{
					Projectile.NewProjectile(base.projectile.Center + Utils.RandomVector2(Main.rand, -200f, 200f), Vector2.One.RotatedByRandom(6.2831854820251465) * 1f, ModContent.ProjectileType<Crystalshard4>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
				}
				for (int j = 0; j < 5; j++) //set to 2
				{
					Projectile.NewProjectile(base.projectile.Center + Utils.RandomVector2(Main.rand, -200f, 200f), Vector2.One.RotatedByRandom(6.2831854820251465) * 1f, ModContent.ProjectileType<Crystalshard3>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
				}
				Main.PlaySound(SoundID.DD2_WitherBeastCrystalImpact, base.projectile.Center);
				projectile.Kill();
			}
		}

		public override Color? GetAlpha(Color lightColor)
		{
			if (base.projectile.timeLeft < 85)
			{
				byte b2 = (byte)(base.projectile.timeLeft * 3);
				byte a2 = (byte)(100f * ((float)(int)b2 / 255f));
				return new Color(b2, b2, b2, a2);
			}
			return new Color(255, 255, 255, 100);
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			FairyGlobalProjectile.DrawCenteredAndAfterimage(base.projectile, lightColor, ProjectileID.Sets.TrailingMode[base.projectile.type]);
			return false;
		}

		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			
			Player player = Main.player[base.projectile.owner];
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
			projectile.timeLeft -= 15;
			if (!player.HasBuff(ModContent.BuffType<Overcharged>()))
			{
				if (Main.rand.NextBool(20))

				{
					{

						Item.NewItem(base.projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ItemType<LivingSilverShard>());
					}
				}
			}
			if (player.HasBuff(ModContent.BuffType<Overcharged>()))
			{
				if (Main.rand.NextBool(10))

				{
					Item.NewItem(base.projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ItemType<LivingSilverShard>());
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