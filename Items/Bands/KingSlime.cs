using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;

using Terraria;


using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.Bands
{
	public class KingSlime : ModProjectile
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
			
			base.projectile.alpha = 300;
			base.projectile.friendly = true;
			base.projectile.tileCollide = false;
			
			
			base.projectile.penetrate = 1;
			base.projectile.timeLeft = 20000;
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
			base.projectile.rotation += 0.095f;
			



			

		}





	}
}
