using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;

using Terraria;
using SariaMod.Buffs;
using SariaMod.Dusts;
using Terraria.Audio;


using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.Ruby
{
	public class Explosion3 : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			base.DisplayName.SetDefault("Blade");
			ProjectileID.Sets.TrailCacheLength[base.Projectile.type] = 6;
			ProjectileID.Sets.TrailingMode[base.Projectile.type] = 0;
			Main.projFrames[base.Projectile.type] = 5;
		}

		public override void SetDefaults()
		{
			base.Projectile.width = 300;
			base.Projectile.height = 300;
			base.Projectile.aiStyle = 21;
			base.Projectile.alpha = 100;
			base.Projectile.friendly = true;
			base.Projectile.tileCollide = false;
			
			base.Projectile.penetrate = -1;
			base.Projectile.timeLeft = 200;
			base.Projectile.ignoreWater = true;
			AIType = 274;
			base.Projectile.usesLocalNPCImmunity = true;
			base.Projectile.localNPCHitCooldown = 20;
		}
		private const int sphereRadius = 100;
		public override void AI()
		{
			Player player = Main.player[base.Projectile.owner];
			FairyPlayer modPlayer = player.Fairy();
			Lighting.AddLight(base.Projectile.Center, 20f, 5f, 0f);
			FairyGlobalProjectile.HomeInOnNPC(base.Projectile, ignoreTiles: true, 600f, 25f, 20f);
			if (Main.rand.NextBool())//controls the speed of when the sparkles spawn
			{
				for (int d = 0; d < 8; d++)
				{
					float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius * sphereRadius));
					double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
					Dust.NewDust(new Vector2(Projectile.Center.X + radius * (float)Math.Cos(angle), (Projectile.Center.Y - 10) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<FlameDust>(), 0f, 0f, 0, default(Color), 1.5f);

				}
			}
				if (Main.rand.NextBool())//controls the speed of when the sparkles spawn
				{
					for (int d = 0; d < 3; d++)
					{
					Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + 0, 0, 0, ModContent.ProjectileType<Smokeball>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);

					}
				}
			
			if (player.HasBuff(ModContent.BuffType<Overcharged>()))
			{
				Projectile.width = 450;
				Projectile.height = 450;
				Projectile.scale = 1.5f;
				Projectile.localNPCHitCooldown = 25;
			}
			Lighting.AddLight(Projectile.Center, Color.OrangeRed.ToVector3() * 0.78f);
			{
				
				Projectile.knockBack = 50;
				base.Projectile.velocity.X = (1 * player.direction);
				base.Projectile.velocity.Y = 0;
				base.Projectile.frameCounter++;
				if (base.Projectile.frameCounter >= 5)
				{
					base.Projectile.frame++;
					base.Projectile.frameCounter = 0;

				}
				if (base.Projectile.frame >= Main.projFrames[base.Projectile.type])
				{
					base.Projectile.frame = 0;
					base.Projectile.Kill();
				}
				if (base.Projectile.timeLeft == 195)
				{
					
						if (player.ownedProjectileCounts[ModContent.ProjectileType<Flame>()] < 60f)
						{
							for (int j = 0; j < 12; j++) //set to 2
							{
							Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + 0, 0, 0, ModContent.ProjectileType<Flame>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
							}
						}
					
					
				}
			}
			if (Projectile.timeLeft >= 200)
            {
				SoundEngine.PlaySound(SoundID.Item116, base.Projectile.Center);
				SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Bomb"));
			}
			if (Projectile.timeLeft == 2)
			{
				SoundEngine.PlaySound(SoundID.DD2_SkyDragonsFuryShot, base.Projectile.Center);
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
