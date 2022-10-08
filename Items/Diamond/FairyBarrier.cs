using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;
using SariaMod.Items.Strange;
using SariaMod.Items.Sapphire;
using Terraria;
using SariaMod.Items.Ruby;
using Terraria.ID;
using SariaMod.Dusts;
using SariaMod.Buffs;
using Terraria.ModLoader;

namespace SariaMod.Items.Diamond
{
	public class FairyBarrier : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			base.DisplayName.SetDefault("Blade");
			ProjectileID.Sets.TrailCacheLength[base.projectile.type] = 8;
			ProjectileID.Sets.TrailingMode[base.projectile.type] = 0;
			Main.projFrames[base.projectile.type] = 1;
		}
		public override bool? CanCutTiles()
		{
			return false;
		}
		
		public override bool? CanHitNPC(NPC target)
		{
			if (!target.friendly)
			{
				return true;
			}
			else
            {
				return false;
            }
		}
		public override void SetDefaults()
		{
			base.projectile.width = 2500;
			base.projectile.height = 2500;
			
			base.projectile.alpha = 300;
			base.projectile.friendly = true;
			base.projectile.tileCollide = false;
			
			base.projectile.penetrate = -1;
			base.projectile.timeLeft = 20;
			base.projectile.ignoreWater = true;
			projectile.minion = false;
			base.projectile.usesLocalNPCImmunity = true;
			base.projectile.localNPCHitCooldown = 20;
		}
		private const int sphereRadius = 1000;

		public override void AI()
		{
			Player player = Main.player[base.projectile.owner];
			Lighting.AddLight(base.projectile.Center, 0f, 0.5f, 0f);
			FairyGlobalProjectile.HomeInOnNPC(base.projectile, ignoreTiles: true, 600f, 25f, 20f);
			
			if (Main.rand.NextBool())//controls the speed of when the sparkles spawn
			{
				float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius * sphereRadius));
				double angle = Main.rand.NextDouble() * 2.0 * Math.PI;

				Dust.NewDust(new Vector2(projectile.Center.X + radius * (float)Math.Cos(angle), projectile.Center.Y + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<Sparkle>(), 0f, 0f, 0, default(Color), 1.5f);
				Dust.NewDust(new Vector2(projectile.Center.X + radius * (float)Math.Cos(angle), projectile.Center.Y + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<Sparkle>(), 0f, 0f, 0, default(Color), 1.5f);
				Dust.NewDust(new Vector2(projectile.Center.X + radius * (float)Math.Cos(angle), projectile.Center.Y + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<Sparkle>(), 0f, 0f, 0, default(Color), 1.5f);
				Dust.NewDust(new Vector2(projectile.Center.X + radius * (float)Math.Cos(angle), projectile.Center.Y + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<Sparkle>(), 0f, 0f, 0, default(Color), 1.5f);
			}
			Projectile mother = Main.projectile[(int)base.projectile.ai[0]];
			if (base.projectile.localAI[0] == 0f)
			{
				Main.PlaySound(SoundID.DD2_LightningBugDeath, base.projectile.Center);
				Main.PlaySound(SoundID.DD2_GhastlyGlaiveImpactGhost, base.projectile.Center);
				base.projectile.localAI[0] = 1f;
			}
			
				NPC target = base.projectile.Center.MinionHoming(100f, player);
			if (target != null && projectile.ai[0] == 0)
			{
				
				projectile.ai[0] = 1;
			}
			if (!mother.active)
			{

				projectile.Kill();
			}

			
			projectile.Center = mother.position;
			projectile.velocity = mother.velocity;
			
		}

	

		

		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			Player player = Main.player[base.projectile.owner];
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
			target.buffImmune[BuffID.Frozen] = false;
			target.buffImmune[BuffID.Electrified] = false;
			target.AddBuff(BuffID.Venom, 300);
			float Attack = 1;
			
				projectile.timeLeft += 3;

				{
					if (target.boss)
					{
						Attack = 3;
					}
				if (player.HasBuff(ModContent.BuffType<Overcharged>()))
				{
					Attack = 2;
				}
				for (int j = 0; j < Attack; j++) //set to 2
					{
						Projectile.NewProjectile(base.projectile.Center = target.Center, Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ProjectileType<FairyBubble>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
					}

				}
			
			damage /= 10;
			knockback = 0;
			
				
			
			
			
		}



	}
}
