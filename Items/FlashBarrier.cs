using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;

using Terraria;




using Terraria.ID;
using SariaMod.Buffs;
using Terraria.ModLoader;

namespace SariaMod.Items
{
	public class FlashBarrier : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			base.DisplayName.SetDefault("Blade");
			ProjectileID.Sets.TrailCacheLength[base.Projectile.type] = 8;
			ProjectileID.Sets.TrailingMode[base.Projectile.type] = 0;
			Main.projFrames[base.Projectile.type] = 1;
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
			base.Projectile.width = 1000;
			base.Projectile.height = 1000;
			
			base.Projectile.alpha = 260;
			base.Projectile.friendly = true;
			base.Projectile.tileCollide = false;
			
			base.Projectile.penetrate = -1;
			base.Projectile.timeLeft = 4;
			base.Projectile.ignoreWater = true;
			
			base.Projectile.usesLocalNPCImmunity = true;
			base.Projectile.localNPCHitCooldown = 20;
		}


		public override void AI()
		{
			Player player = Main.player[base.Projectile.owner];
			Lighting.AddLight(base.Projectile.Center, 0f, 0.5f, 0f);
			FairyGlobalProjectile.HomeInOnNPC(base.Projectile, ignoreTiles: true, 600f, 25f, 20f);
			Projectile mother = Main.projectile[(int)base.Projectile.ai[1]];
			if (base.Projectile.localAI[0] == 0f)
			{
				
				base.Projectile.localAI[0] = 1f;
			}
			for (int i = 0; i < 1000; i++)
				if (Main.projectile[i].active && i != base.Projectile.whoAmI && Main.projectile[i].Hitbox.Intersects(base.Projectile.Hitbox) && Main.projectile[i].active && ((!Main.projectile[i].friendly && Main.projectile[i].hostile) || (Main.projectile[i].trap)))
				{
					Main.projectile[i].Kill();
				}
			if (Projectile.timeLeft > 200)
            {
				Projectile.timeLeft = 200;
            }
				NPC target = base.Projectile.Center.MinionHoming(100f, player);
			if (target != null && Projectile.ai[0] == 0)
			{
				
				Projectile.ai[0] = 1;
			}
			if (!mother.active)
			{

				Projectile.Kill();
			}
			
			Projectile.Center = mother.position;
			Projectile.velocity = mother.velocity;
			
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
			target.buffImmune[BuffID.Frozen] = false;
			target.buffImmune[BuffID.Electrified] = false;
			target.AddBuff(BuffID.Venom, 300);
			target.AddBuff(ModContent.BuffType<SariaCurse>(), 200);
			Projectile.timeLeft += 5;
			damage *= 0;
			knockback = 0;
			if (!target.boss)
			{
				target.noTileCollide = false;
				
				
			}
				
			
			
			
		}



	}
}
