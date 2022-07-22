using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;

using Terraria;




using Terraria.ID;
using SariaMod.Buffs;
using Terraria.ModLoader;

namespace SariaMod.Items.Amethyst
{
	public class GhostBarrier : ModProjectile
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
			Player player = Main.player[base.projectile.owner];
			FairyPlayer modPlayer = player.Fairy();
			if (target.friendly)
            {
				return false;
            }
			
			else 
			{
				return true;
			}
		}
		public override void SetDefaults()
		{
			base.projectile.width = 1000;
			base.projectile.height = 1000;
			
			base.projectile.alpha = 260;
			base.projectile.friendly = true;
			base.projectile.tileCollide = false;
			
			base.projectile.penetrate = -1;
			base.projectile.timeLeft = 4;
			base.projectile.ignoreWater = true;
			
			base.projectile.usesLocalNPCImmunity = true;
			base.projectile.localNPCHitCooldown = 20;
		}


		public override void AI()
		{
			Player player = Main.player[base.projectile.owner];
			Lighting.AddLight(base.projectile.Center, 0f, 0.5f, 0f);
			FairyGlobalProjectile.HomeInOnNPC(base.projectile, ignoreTiles: true, 600f, 25f, 20f);
			Projectile mother = Main.projectile[(int)base.projectile.ai[0]];
			if (base.projectile.localAI[0] == 0f)
			{
				Main.PlaySound(SoundID.DD2_LightningBugDeath, base.projectile.Center);
				Main.PlaySound(SoundID.DD2_GhastlyGlaiveImpactGhost, base.projectile.Center);
				base.projectile.localAI[0] = 1f;
			}
			if (projectile.timeLeft > 200)
            {
				projectile.timeLeft = 200;
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
			target.AddBuff(ModContent.BuffType<SariaCurse>(), 200);
			projectile.timeLeft += 5;
			damage *= 0;
			knockback = 0;
			if (!target.boss)
			{
				target.noTileCollide = false;
				
				
			}
				
			
			
			
		}



	}
}
