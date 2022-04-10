using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FairyMod;
using System;
using FairyMod.Projectiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items
{
	public class PsychicBlade : ModProjectile
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
			base.projectile.width = 10;
			base.projectile.height = 10;
			base.projectile.aiStyle = 21;
			base.projectile.alpha = 100;
			base.projectile.friendly = true;
			base.projectile.tileCollide = false;
			
			base.projectile.penetrate = 5;
			base.projectile.timeLeft = 200;
			base.projectile.ignoreWater = true;
			aiType = 274;
			base.projectile.usesLocalNPCImmunity = true;
			base.projectile.localNPCHitCooldown = 4;
		}

		public override void AI()
		{
			Lighting.AddLight(base.projectile.Center, 0f, 0.5f, 0f);
			FairyGlobalProjectile.HomeInOnNPC(base.projectile, ignoreTiles: true, 600f, 25f, 20f);
			{
				base.projectile.frameCounter++;
				if (base.projectile.frameCounter > 6)
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
			target.AddBuff(BuffID.Confused, 300);

		}
	



	}
}
