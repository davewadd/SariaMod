using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;

using Terraria;




using Terraria.ID;
using SariaMod.Buffs;
using Terraria.ModLoader;

namespace SariaMod.Items.Emerald
{
	public class PinkFairyBarrier : ModProjectile
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
			
            {
				return false;
            }
			
			
		}
		public override void SetDefaults()
		{
			base.projectile.width = 170;
			base.projectile.height = 170;
			
			
			base.projectile.friendly = true;
			base.projectile.tileCollide = false;
			
			base.projectile.penetrate = -1;
			base.projectile.timeLeft = 4;
			base.projectile.ignoreWater = true;
			
			base.projectile.usesLocalNPCImmunity = true;
			base.projectile.localNPCHitCooldown = 20;
		}

		private int number;
		public override void AI()
		{
			Player player = Main.player[base.projectile.owner];
			FairyPlayer modPlayer = player.Fairy();
			Lighting.AddLight(base.projectile.Center, 0f, 0.5f, 0f);
			FairyGlobalProjectile.HomeInOnNPC(base.projectile, ignoreTiles: true, 600f, 25f, 20f);
			Projectile mother = Main.projectile[(int)base.projectile.ai[0]];
			number++;
			if (number >= 600)
			{
				if (modPlayer.FairyBreak >= 1)
				{
					modPlayer.FairyBreak -= 1;
					Main.PlaySound(SoundID.DD2_WitherBeastCrystalImpact, base.projectile.Center);
				}
				number = 0;
			}
			else if (modPlayer.FairyBreak >= 10 )
			{
				projectile.alpha = 300;
				Lighting.AddLight(projectile.Center, Color.Magenta.ToVector3() * .1f);
			}
			else if (modPlayer.FairyBreak > 4 && modPlayer.FairyBreak < 10)
			{
				projectile.alpha = 230;
				Lighting.AddLight(projectile.Center, Color.Magenta.ToVector3() * 1f);
			}
			else if (modPlayer.FairyBreak > 2 && modPlayer.FairyBreak <= 4)
				{
				projectile.alpha = 214;
				Lighting.AddLight(projectile.Center, Color.Magenta.ToVector3() * 2f);
			}
			else if (modPlayer.FairyBreak > 0 && modPlayer.FairyBreak <= 2)
			{
				projectile.alpha = 190;
				Lighting.AddLight(projectile.Center, Color.Magenta.ToVector3() * 3f);
			}
			else
            {
				projectile.alpha = 170;
				Lighting.AddLight(projectile.Center, Color.Magenta.ToVector3() * 4f);
			}
			if (base.projectile.localAI[0] == 0f)
			{
				
				base.projectile.localAI[0] = 1f;
			}
			if (projectile.alpha < 300)
			{
				for (int i = 0; i < 1000; i++)
					if (Main.projectile[i].active && i != base.projectile.whoAmI && Main.projectile[i].Hitbox.Intersects(base.projectile.Hitbox) && Main.projectile[i].active && ((!Main.projectile[i].friendly && Main.projectile[i].hostile) || (Main.projectile[i].trap)))
					{
						Main.projectile[i].Kill();
						if (modPlayer.FairyBreak < 10)
						{
							Main.PlaySound(SoundID.DD2_WitherBeastCrystalImpact, base.projectile.Center);
							modPlayer.FairyBreak += 1;
						}
					}
			}
			projectile.timeLeft = 2;
				NPC target = base.projectile.Center.MinionHoming(100f, player);
			if (target != null && projectile.ai[0] == 0)
			{
				
				projectile.ai[0] = 1;
			}
			if (!mother.active || mother.type != ModContent.ProjectileType<EmeraldfairyGem>())
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
			target.AddBuff(ModContent.BuffType<SariaCurse>(), 200);
			projectile.timeLeft += 5;
			damage *= 0;
			knockback = 0;
			
				
			
			
			
		}



	}
}
