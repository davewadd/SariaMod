using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;

using Terraria;
using SariaMod.Buffs;

using SariaMod.Dusts;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.Amethyst
{
	public class ShadowSneak : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			base.DisplayName.SetDefault("Blade");
			ProjectileID.Sets.TrailCacheLength[base.projectile.type] = 6;
			ProjectileID.Sets.TrailingMode[base.projectile.type] = 0;
			Main.projFrames[base.projectile.type] = 15;
		}

		public override void SetDefaults()
		{
			base.projectile.width = 100;
			base.projectile.height = 270;
			base.projectile.aiStyle = 21;
			base.projectile.alpha = 0;
			base.projectile.friendly = true;
			base.projectile.tileCollide = false;
			
			base.projectile.penetrate = -1;
			base.projectile.timeLeft = 200;
			base.projectile.ignoreWater = true;
			aiType = 274;
			base.projectile.usesLocalNPCImmunity = true;
			base.projectile.localNPCHitCooldown = 20;
		}
		private const int sphereRadius = 3;
		public override void AI()
		{
			Player player = Main.player[base.projectile.owner];
			FairyPlayer modPlayer = player.Fairy();

			FairyGlobalProjectile.HomeInOnNPC(base.projectile, ignoreTiles: true, 600f, 25f, 20f);
			Lighting.AddLight(projectile.Center, Color.DarkViolet.ToVector3() * 12f);
			if (projectile.frame >= 5)
			{
				if (Main.rand.NextBool())//controls the speed of when the sparkles spawn
				{

					float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius * sphereRadius));
					double angle = Main.rand.NextDouble() * 2.0 * Math.PI;
					Dust.NewDust(new Vector2(projectile.Center.X + radius * (float)Math.Cos(angle), (projectile.Center.Y - 130) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<Shadow2>(), 0f, 0f, 0, default(Color), 1.5f);
				}
			}
			if (projectile.frame <= 2)
			{
				if (!player.HasBuff(ModContent.BuffType<StatLower>()))
				{
					for (int j = 0; j < 10; j++) //set to 2
					{
						Projectile.NewProjectile(base.projectile.Center + new Vector2(20f, 130f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ProjectileType<Ghostsmoke>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
					}
				}
			}
			{

				projectile.knockBack = 5;
				if (projectile.timeLeft >= 190)
				{
					base.projectile.velocity.X = (float)((.001) * player.direction);
				}
				base.projectile.velocity.Y = 0;
				base.projectile.frameCounter++;
				int frameSpeed = 15;
				{
					base.projectile.frameCounter++;
					if (projectile.frameCounter >= frameSpeed)


						if (base.projectile.frameCounter > 14)
						{
							base.projectile.frame++;
							base.projectile.frameCounter = 0;
						}
					if (base.projectile.frame >= 14)
					{

						base.projectile.frame = 14;
						projectile.timeLeft -= 30;
					}
					
				}
			}
				if (projectile.timeLeft >= 200)
            {
				Main.PlaySound(SoundID.NPCDeath59, base.projectile.Center);
				
			}
			if (projectile.timeLeft == 90)
			{
				Main.PlaySound(base.mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Spiritcrawl"), base.projectile.Center);
				Main.PlaySound(base.mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Poe"), base.projectile.Center);
			}
			
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
			target.buffImmune[ModContent.BuffType<SariaCurse>()] = false;
			target.AddBuff(ModContent.BuffType<SariaCurse>(), 2000);
			modPlayer.SariaXp++;
			if (target.defense > 200)
			{
				target.defense = 200;
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
