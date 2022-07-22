using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;

using Terraria;
using SariaMod.Buffs;



using SariaMod.Dusts;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.zPearls
{
	public class ForestNote : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			base.DisplayName.SetDefault("Blade");
			ProjectileID.Sets.TrailCacheLength[base.projectile.type] = 7;
			ProjectileID.Sets.TrailingMode[base.projectile.type] = 0;
			
		}

		public override void SetDefaults()
		{
			base.projectile.width = 30;
			base.projectile.height = 30;
			
			base.projectile.alpha = 0;
			base.projectile.friendly = true;
			base.projectile.tileCollide = false;
			
			base.projectile.penetrate = 1;
			base.projectile.timeLeft = 500;
			base.projectile.ignoreWater = true;
			
			base.projectile.usesLocalNPCImmunity = true;
			base.projectile.localNPCHitCooldown = 4;
		}

		public override bool? CanHitNPC(NPC target)
        {
			return false;
        }
		public override void AI()
		{
			Player player = Main.player[base.projectile.owner];
			Projectile mother = Main.projectile[(int)base.projectile.ai[0]];
			Lighting.AddLight(projectile.Center, Color.LightGreen.ToVector3() * 1f);
			projectile.position.X = player.position.X ;
			projectile.position.Y = player.position.Y - 80;
			if (((player.ownedProjectileCounts[ModContent.ProjectileType<TimeNote>()] > 0f)))
			{
				projectile.Kill();
			}
			if (((player.ownedProjectileCounts[ModContent.ProjectileType<NotePlay>()] > 0f)))
            {
				projectile.timeLeft = 2;
            }
			if (((player.ownedProjectileCounts[ModContent.ProjectileType<NotePlay>()] > 0f)) && ((player.ownedProjectileCounts[ModContent.ProjectileType<SariasSong>()] == 0f)))
			{
				Projectile.NewProjectile(base.projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 1f, ModContent.ProjectileType<SariasSong>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
			}
			if (projectile.velocity.X >= 0)
			{
				projectile.spriteDirection = 1;
			}
			if (projectile.velocity.X <= -0)
			{
				projectile.spriteDirection = -1;
			}
		}

	

		

	



	}
}
