using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;

using Terraria;
using SariaMod.Buffs;




using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.Topaz
{
	public class LightningStrike : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			base.DisplayName.SetDefault("Blade");
			ProjectileID.Sets.TrailCacheLength[base.projectile.type] = 6;
			ProjectileID.Sets.TrailingMode[base.projectile.type] = 0;
			Main.projFrames[base.projectile.type] = 4;
		}

		public override void SetDefaults()
		{
			base.projectile.width = 40;
			base.projectile.height = 400;
			
			base.projectile.alpha = 100;
			base.projectile.friendly = true;
			base.projectile.tileCollide = false;
			
			base.projectile.penetrate = -1;
			base.projectile.timeLeft = 150;
			base.projectile.ignoreWater = true;
			
			base.projectile.usesLocalNPCImmunity = true;
			base.projectile.localNPCHitCooldown = 10;
		}

		public override void AI()
		{
			Player player = Main.player[base.projectile.owner];
			FairyPlayer modPlayer = player.Fairy();
			
			FairyGlobalProjectile.HomeInOnNPC(base.projectile, ignoreTiles: true, 600f, 25f, 20f);
			Lighting.AddLight(projectile.Center, Color.OrangeRed.ToVector3() * 0.78f);
			if (player.HasBuff(ModContent.BuffType<Overcharged>()))
			{
				projectile.width = 100;
				projectile.height = 450;
				projectile.localNPCHitCooldown = 6;
			}
			{



				if (projectile.timeLeft >= 140 && projectile.timeLeft <= 150)
				{
					Projectile.NewProjectile(base.projectile.Center + new Vector2(0f, -120f), Vector2.One.RotatedByRandom(6.2831854820251465) * 2f, ModContent.ProjectileType<Fiz>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
					if ((player.ownedProjectileCounts[ModContent.ProjectileType<Drop>()] <= 0f))
					{
						Projectile.NewProjectile(base.projectile.Center + new Vector2(0f, 100f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ProjectileType<Drop>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
					}
				}
				if (projectile.timeLeft >= 190)
				{
					base.projectile.velocity.X = (1 * player.direction);
					base.projectile.velocity.Y = 0;
				}
				else if (projectile.timeLeft < 190)
					{
						base.projectile.velocity.X = 0;
						base.projectile.velocity.Y = 0;
					}
				base.projectile.frameCounter++;
				if (base.projectile.frameCounter >= 4)
				{
					base.projectile.frame++;
					base.projectile.frameCounter = 0;

				}
				if (base.projectile.frame >= Main.projFrames[base.projectile.type])
				{
					base.projectile.frame = 3;
					
				}
			
			}
			
			
			
			if (projectile.timeLeft >= 150)
            {
				Main.PlaySound(SoundID.Item116, base.projectile.Center);
				Main.PlaySound(base.mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Lightning"), base.projectile.Center);
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
			target.AddBuff(BuffID.Electrified, 300);
			modPlayer.SariaXp++;
			if (player.HasBuff(ModContent.BuffType<StatRaise>()))
			{
				damage += (damage) / 4;
			}
			if (player.HasBuff(ModContent.BuffType<StatLower>()))
			{
				damage /= 2;

			}
			knockback /= 1000;
		}



	}
}
