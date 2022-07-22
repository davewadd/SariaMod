using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;

using Terraria;


using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.Amber
{
	public class Mothdust2 : ModProjectile
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
			base.projectile.timeLeft = 300;
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
			FairyGlobalProjectile.HomeInOnNPC(base.projectile, ignoreTiles: true, 600f, 25f, 20f);
			base.projectile.rotation += 0.005f;
			projectile.velocity.Y = mother.velocity.Y;
			projectile.velocity.X = mother.velocity.X;
			if (!mother.active)
			{
				base.projectile.Kill();
				return;
			}
			{
				
				
				// friendly needs to be set to true so the minion can deal contact damage
				// friendly needs to be set to false so it doesn't damage things like target dummies while idling
				// Both things depend on if it has a target or not, so it's just one assignment here
				// You don't need this assignment if your minion is shooting things instead of dealing contact damage
				

			
				
				Lighting.AddLight(projectile.Center, Color.LightYellow.ToVector3() * 1f);
				// Default movement parameters (here for attacking)
			

			}



			

		}





	}
}
