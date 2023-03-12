using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;




using System;
using Terraria;
using SariaMod.Buffs;
using SariaMod.Items.Sapphire;
using SariaMod.Dusts;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.Emerald
{
	public class Silverrupee2 : ModProjectile
	{
		public override void SetStaticDefaults()
		{
		    
			base.DisplayName.SetDefault("Child");
			Main.projFrames[base.projectile.type] = 1;
			ProjectileID.Sets.MinionShot[base.projectile.type] = true;
			ProjectileID.Sets.TrailingMode[base.projectile.type] = 2;
			ProjectileID.Sets.TrailCacheLength[base.projectile.type] = 30;
			
		}
		
		public override void SetDefaults()
		{
			base.projectile.width = 16;
			base.projectile.height = 16;
			base.projectile.netImportant = true;
			base.projectile.friendly = false;
			base.projectile.ignoreWater = true;
			base.projectile.usesLocalNPCImmunity = true;
			base.projectile.localNPCHitCooldown = 7;
			base.projectile.minionSlots = 0f;
			base.projectile.extraUpdates = 1;
			projectile.aiStyle = 14;
			
			base.projectile.penetrate = 2;
			base.projectile.tileCollide = true;
			base.projectile.timeLeft = 300;
		}
		public override bool? CanCutTiles()
		{
			return false;
		}
		public override bool MinionContactDamage()
		{
			
			return true;
		}
		public override bool? CanHitNPC(NPC target)
		{
			return false;
		}
		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			Player player = Main.player[base.projectile.owner];
			FairyPlayer modPlayer = player.Fairy();

	
			{
				base.projectile.velocity.X = 0f - (oldVelocity.X * -.6f);

			}

			{
				base.projectile.velocity.Y = 0f - (oldVelocity.Y*.6f);
				
			}
			if (Math.Abs(projectile.oldVelocity.Y) >= 1f)
            {
				Main.PlaySound(SoundID.Item49, base.projectile.Center);
			}

				return false;
		}
		private const int sphereRadius = 3;

		public override void AI()
		{
			Player player = Main.player[base.projectile.owner];
			FairyPlayer modPlayer = player.Fairy();
			
			if (projectile.timeLeft == 10)
			{
				for (int k = 0; k < 3; k++)
				{

					Projectile.NewProjectile(base.projectile.Center + new Vector2(0f, -80f), Vector2.One.RotatedByRandom(6.2831854820251465) * 3f, ModContent.ProjectileType<Shard3>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
				}
				Projectile.NewProjectile(base.projectile.Center + new Vector2(0f, -80f), new Vector2(0, 0), ModContent.ProjectileType<Silverspike1>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
				projectile.Kill();
			}
			Lighting.AddLight(projectile.Center, Color.Gray.ToVector3() * 1f);
		}
		

	}
}

