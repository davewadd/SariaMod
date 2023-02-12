using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;

using Terraria;
using SariaMod.Buffs;
using SariaMod.Dusts;


using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.Ruby
{
	public class Explosion2 : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			base.DisplayName.SetDefault("Blade");
			ProjectileID.Sets.TrailCacheLength[base.projectile.type] = 6;
			ProjectileID.Sets.TrailingMode[base.projectile.type] = 0;
			Main.projFrames[base.projectile.type] = 5;
		}

		public override void SetDefaults()
		{
			base.projectile.width = 300;
			base.projectile.height = 300;
			base.projectile.aiStyle = 21;
			base.projectile.alpha = 100;
			base.projectile.friendly = true;
			base.projectile.tileCollide = false;
			
			base.projectile.penetrate = -1;
			base.projectile.timeLeft = 200;
			base.projectile.ignoreWater = true;
			aiType = 274;
			base.projectile.usesLocalNPCImmunity = true;
			base.projectile.localNPCHitCooldown = 20;
		}
		private const int sphereRadius = 100;
		public override void AI()
		{
			Player player = Main.player[base.projectile.owner];
			FairyPlayer modPlayer = player.Fairy();
			Lighting.AddLight(base.projectile.Center, 20f, 5f, 0f);
			FairyGlobalProjectile.HomeInOnNPC(base.projectile, ignoreTiles: true, 600f, 25f, 20f);
			if (Main.rand.NextBool())//controls the speed of when the sparkles spawn
			{
				for (int d = 0; d < 8; d++)
				{
					float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius * sphereRadius));
					double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
					Dust.NewDust(new Vector2(projectile.Center.X + radius * (float)Math.Cos(angle), (projectile.Center.Y - 10) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<FlameDust>(), 0f, 0f, 0, default(Color), 1.5f);

				}
			}
				if (Main.rand.NextBool())//controls the speed of when the sparkles spawn
				{
					for (int d = 0; d < 3; d++)
					{
						Projectile.NewProjectile(base.projectile.Center + new Vector2(0f, 0f), Vector2.One.RotatedByRandom(6) * 3f, ModContent.ProjectileType<Smokeball>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);

					}
				}
			
			if (player.HasBuff(ModContent.BuffType<Overcharged>()))
			{
				projectile.width = 450;
				projectile.height = 450;
				projectile.scale = 1.5f;
				projectile.localNPCHitCooldown = 25;
				if (base.projectile.timeLeft == 195)
				{
					Projectile.NewProjectile(base.projectile.Center + new Vector2(-70f, -70f), Vector2.One.RotatedByRandom(6) * 3f, ModContent.ProjectileType<Explosion3>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
				}
			}
			Lighting.AddLight(projectile.Center, Color.OrangeRed.ToVector3() * 0.78f);
			{
				
				projectile.knockBack = 50;
				base.projectile.velocity.X = (1 * player.direction);
				base.projectile.velocity.Y = 0;
				base.projectile.frameCounter++;
				if (base.projectile.frameCounter >= 5)
				{
					base.projectile.frame++;
					base.projectile.frameCounter = 0;

				}
				if (base.projectile.frame >= Main.projFrames[base.projectile.type])
				{
					base.projectile.frame = 0;
					base.projectile.Kill();
				}
				if (base.projectile.timeLeft == 195)
				{
					
						if (player.ownedProjectileCounts[ModContent.ProjectileType<Flame>()] < 60f)
						{
							for (int j = 0; j < 12; j++) //set to 2
							{
								Projectile.NewProjectile(base.projectile.Center + Utils.RandomVector2(Main.rand, -204f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 10f, ModContent.ProjectileType<Flame>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
							}
						}
					
					
				}
			}
			if (projectile.timeLeft >= 200)
            {
				Main.PlaySound(SoundID.Item116, base.projectile.Center);
				Main.PlaySound(base.mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Bomb"), base.projectile.Center);
			}
			if (projectile.timeLeft == 2)
			{
				Main.PlaySound(SoundID.DD2_SkyDragonsFuryShot, base.projectile.Center);
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
			target.buffImmune[ModContent.BuffType<Burning2>()] = false;
			target.AddBuff(ModContent.BuffType<Burning2>(), 200);
			modPlayer.SariaXp++;
			if (target.type == NPCID.Mothron|| target.type == NPCID.MourningWood || target.type == NPCID.Everscream)
            {
				damage *= 4;
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
