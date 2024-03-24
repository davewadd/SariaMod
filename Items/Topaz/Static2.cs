using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using System;
using SariaMod.Buffs;
using Terraria;
using SariaMod.Dusts;
using Terraria.ID;
using Terraria.ModLoader;
using SariaMod.Items.Strange;

namespace SariaMod.Items.Topaz
{
	public class Static2 : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			base.DisplayName.SetDefault("Blade");
			ProjectileID.Sets.TrailCacheLength[base.Projectile.type] = 7;
			ProjectileID.Sets.TrailingMode[base.Projectile.type] = 0;
			Main.projFrames[base.Projectile.type] = 4;
		}

		public override void SetDefaults()
		{
			base.Projectile.width = 30;
			base.Projectile.height = 30;
			
			base.Projectile.alpha = 0;
			base.Projectile.friendly = true;
			base.Projectile.tileCollide = false;
			
			
			base.Projectile.penetrate = 1;
			base.Projectile.timeLeft = 2000;
			base.Projectile.ignoreWater = true;
			
			base.Projectile.usesLocalNPCImmunity = true;
			base.Projectile.localNPCHitCooldown = 4;
		}

		public override bool? CanHitNPC(NPC target)
        {
			return false;
        }
		public override void AI()
		{
			Player player = Main.player[base.Projectile.owner];
			Projectile mother = Main.projectile[(int)base.Projectile.ai[1]];
			if (Main.rand.NextBool(30))//controls the speed of when the sparkles spawn
			{
				float radius = (float)Math.Sqrt(Main.rand.Next(34 * 34));
				double angle = Main.rand.NextDouble() * 5.0 * Math.PI;


				Dust.NewDust(new Vector2(Projectile.Center.X + radius * (float)Math.Cos(angle), Projectile.Center.Y + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<StaticDust>(), 0f, 0f, 0, default(Color), 1.5f);
			}//end of dust stuff
			
			if (player.ownedProjectileCounts[ModContent.ProjectileType<LightningStrike2>()] >= 1f)
            {
				Projectile.Kill();
            }
			if (player.ownedProjectileCounts[ModContent.ProjectileType<LightningStrikeRed>()] >= 1f)
			{
				Projectile.Kill();
			}
			if (player.ownedProjectileCounts[ModContent.ProjectileType<LightningStrikeBlue>()] >= 1f)
			{
				Projectile.Kill();
			}
			if (player.ownedProjectileCounts[ModContent.ProjectileType<LightningStrikePurple>()] >= 1f)
			{
				Projectile.Kill();
			}
			if (player.ownedProjectileCounts[ModContent.ProjectileType<LightningStrikePink>()] >= 1f)
			{
				Projectile.Kill();
			}
			// This code is required if your minion weapon has the targeting feature
			Projectile.timeLeft = 2000;

			float speed = 150f;
			Vector2 mouse = Main.MouseWorld;
			mouse.Y -= 320;
			mouse.X += 10;
			Vector2 direction2 = mouse - Projectile.Center;
			direction2.Normalize();
			direction2 *= speed;

			Projectile.velocity = (Projectile.velocity * (10 - 2) + direction2) / 9;
			// friendly needs to be set to true so the minion can deal contact damage
			// friendly needs to be set to false so it doesn't damage things like target dummies while idling
			// Both things depend on if it has a target or not, so it's just one assignment here
			// You don't need this assignment if your minion is shooting things instead of dealing contact damage



			Lighting.AddLight(Projectile.Center, Color.LightGoldenrodYellow.ToVector3() * 1f);
			// Default movement parameters (here for attacking)

			if (!player.HasBuff(ModContent.BuffType<SariaBuff>()))
			{
				Projectile.Kill();
			}





				base.Projectile.frameCounter++;
			if (base.Projectile.frameCounter >= 4)
			{
				base.Projectile.frame++;
				base.Projectile.frameCounter = 0;

			}
			if (base.Projectile.frame >= Main.projFrames[base.Projectile.type])
			{
				base.Projectile.frame = 0;

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

			damage /= damage/4;
			
		}



	}
}
