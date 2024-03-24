using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;

using Terraria;
using SariaMod.Buffs;
using Terraria.Audio;




using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.Topaz
{
	public class LightningStrikeRed : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			base.DisplayName.SetDefault("Blade");
			ProjectileID.Sets.TrailCacheLength[base.Projectile.type] = 6;
			ProjectileID.Sets.TrailingMode[base.Projectile.type] = 0;
			Main.projFrames[base.Projectile.type] = 4;
		}

		public override void SetDefaults()
		{
			base.Projectile.width = 40;
			base.Projectile.height = 400;
			
			base.Projectile.alpha = 100;
			base.Projectile.friendly = true;
			base.Projectile.tileCollide = false;
			
			base.Projectile.penetrate = -1;
			base.Projectile.timeLeft = 150;
			base.Projectile.ignoreWater = true;
			
			base.Projectile.usesLocalNPCImmunity = true;
			base.Projectile.localNPCHitCooldown = 60;
		}

		public override void AI()
		{
			Player player = Main.player[base.Projectile.owner];
			FairyPlayer modPlayer = player.Fairy();
			Projectile.damage /= 2;
			FairyGlobalProjectile.HomeInOnNPC(base.Projectile, ignoreTiles: true, 600f, 25f, 20f);
			Lighting.AddLight(Projectile.Center, Color.Red.ToVector3() * 3f);
			if (player.HasBuff(ModContent.BuffType<Overcharged>()))
			{
			
				Projectile.localNPCHitCooldown = 30;
			}
			{



				if (Projectile.timeLeft >= 140 && Projectile.timeLeft <= 150)
				{
					Vector2 Blue = Projectile.Center;
					Blue.Y = -120;
					Blue.X = 0;
					Projectile.NewProjectile(Projectile.GetSource_FromThis(), Blue + Utils.RandomVector2(Main.rand, 0f, -120f),Vector2.One.RotatedByRandom(6.2831854820251465) * 0f, ModContent.ProjectileType<Fiz>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
					
				}
				
				if (Projectile.timeLeft >= 190)
				{
					base.Projectile.velocity.X = (1 * player.direction);
					base.Projectile.velocity.Y = 0;
				}
				else if (Projectile.timeLeft < 190)
					{
						base.Projectile.velocity.X = 0;
						base.Projectile.velocity.Y = 0;
					}
				base.Projectile.frameCounter++;
				if (base.Projectile.frameCounter >= 4)
				{
					base.Projectile.frame++;
					base.Projectile.frameCounter = 0;

				}
				if (base.Projectile.frame >= Main.projFrames[base.Projectile.type])
				{
					base.Projectile.frame = 3;
					
				}
			
			}
			
			
			
			if (Projectile.timeLeft >= 150)
            {
				SoundEngine.PlaySound(SoundID.Item116, base.Projectile.Center);
				SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Lightning"));
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
			target.AddBuff(BuffID.Electrified, 300);
			modPlayer.SariaXp++;
			
			knockback /= 1000;
		}



	}
}
