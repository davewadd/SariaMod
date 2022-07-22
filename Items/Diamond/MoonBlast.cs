using System;
using Microsoft.Xna.Framework;

using SariaMod.Items.Sapphire;
using SariaMod.Items.Ruby;
using SariaMod.Items.Topaz;
using SariaMod.Items.Emerald;
using SariaMod.Items.Amber;
using SariaMod.Items.Amethyst;
using SariaMod.Items.Diamond;

using Microsoft.Xna.Framework.Graphics;
using SariaMod.Items.Platinum;
using SariaMod.Items.Barrier;
using SariaMod.Items.Strange;
using SariaMod.Items.zPearls;
using SariaMod.Items.Bands;
using SariaMod.Buffs;

using SariaMod.Dusts;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.Diamond
{
	public class MoonBlast: ModProjectile
	{
		public override void SetDefaults()
		{
			
			
			base.projectile.width = 42;
			base.projectile.height = 40;
			base.projectile.hostile = false;
			base.projectile.friendly = false;
			base.projectile.ignoreWater = true;
			
			base.projectile.timeLeft = 340;
			base.projectile.penetrate = -1;
			base.projectile.tileCollide = false;
			base.projectile.minion = false;
			base.projectile.localNPCHitCooldown = 5;
			base.projectile.minionSlots = 0f;
			base.projectile.netImportant = true;
			base.projectile.usesLocalNPCImmunity = true;
			


		}
		
		public override void SetStaticDefaults()
		{
			base.DisplayName.SetDefault("Psychic Turret");
			Main.projFrames[base.projectile.type] = 0;
			Main.projPet[projectile.type] = true;
			ProjectileID.Sets.MinionSacrificable[base.projectile.type] = false;
			ProjectileID.Sets.MinionTargettingFeature[base.projectile.type] = true;
		}
        public override bool CanDamage()
        {
			return false;
        }
		private const int sphereRadius = 30;
		public override void AI()
		{
			Player player = Main.player[base.projectile.owner];
			FairyPlayer modPlayer = player.Fairy();

			if (Main.rand.NextBool(30))//controls the speed of when the sparkles spawn
			{
				float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius * sphereRadius));
				double angle = Main.rand.NextDouble() * 2.0 * Math.PI;
				Dust.NewDust(new Vector2(projectile.Center.X + radius * (float)Math.Cos(angle), projectile.Center.Y + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<Sparkle>(), 0f, 0f, 0, default(Color), 1.5f);
			}
			if (player.dead || !player.active)
			{
				
				projectile.Kill();
			}
			Projectile mother = Main.projectile[(int)base.projectile.ai[0]];
			if (!mother.active )
			{
				base.projectile.Kill();
				return;
			}
			
			base.projectile.position.X = mother.Center.X -20;
			base.projectile.position.Y = mother.Center.Y - 180;
			base.projectile.netUpdate = true;
			projectile.timeLeft += (player.ownedProjectileCounts[ModContent.ProjectileType<FairyBubble>()]);
			if (projectile.timeLeft <= 1000)
            {
				projectile.frame = 0;
            }
			else if (projectile.timeLeft > 1000 && (projectile.timeLeft <= 2500))
			{
				
			}
			else if (projectile.timeLeft > 2500 && (projectile.timeLeft <= 4500))
			{
				
			}
			else if (projectile.timeLeft > 4500 && (projectile.timeLeft <= 6500))
			{
				
			}
			else if (projectile.timeLeft > 6500 && (projectile.timeLeft <= 9000))
			{
				
			}
			
			if (projectile.frame == 0 && (player.ownedProjectileCounts[ModContent.ProjectileType<Ring1>()] <= 0))
            {
				Projectile.NewProjectile(base.projectile.Center + new Vector2(0f, 0f), Vector2.One.RotatedByRandom(6.2831854820251465) * 0f, ModContent.ProjectileType<Ring1>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
			
		}
			if ((projectile.timeLeft > 2500 && (projectile.timeLeft <= 4500)) && (player.ownedProjectileCounts[ModContent.ProjectileType<Ring2>()] <= 0))
			{
				Projectile.NewProjectile(base.projectile.Center + new Vector2(0f, 0f), Vector2.One.RotatedByRandom(6.2831854820251465) * 0f, ModContent.ProjectileType<Ring2>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);

			}
			if ((projectile.timeLeft > 4500 && (projectile.timeLeft <= 6500)) && (player.ownedProjectileCounts[ModContent.ProjectileType<Ring3>()] <= 0))
			{
				Projectile.NewProjectile(base.projectile.Center + new Vector2(0f, 0f), Vector2.One.RotatedByRandom(6.2831854820251465) * 0f, ModContent.ProjectileType<Ring3>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);

			}
			if (player.ownedProjectileCounts[ModContent.ProjectileType<DazzlingGleam2>()] >= 1f)
			{

				for (int j = 0; j < 1; j++) //set to 2
				{
					Projectile.NewProjectile(base.projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ProjectileType<FairyBarrier>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
				}
			}
				float Attack = projectile.timeLeft/ 100;

			if (player.ownedProjectileCounts[ModContent.ProjectileType<DazzlingGleam>()] >= 1f)
            {
				if (Main.rand.NextBool())//controls the speed of when the sparkles spawn
				{
					float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius * sphereRadius));
					double angle = Main.rand.NextDouble() * 2.0 * Math.PI;
					Dust.NewDust(new Vector2(projectile.Center.X + radius * (float)Math.Cos(angle), projectile.Center.Y + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<Sparkle>(), 0f, 0f, 0, default(Color), 1.5f);
				}
				for (int j = 0; j < 1; j++) //set to 2
				if ((player.ownedProjectileCounts[ModContent.ProjectileType<Ring3>()] <= 0) && (player.ownedProjectileCounts[ModContent.ProjectileType<Ring2>()] <= 0))
                    {
						Projectile.NewProjectile(base.projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ProjectileType<GleamBomb>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
					}
				if ((player.ownedProjectileCounts[ModContent.ProjectileType<Ring3>()] <= 0) && (player.ownedProjectileCounts[ModContent.ProjectileType<Ring2>()] > 0))
				{
					Projectile.NewProjectile(base.projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ProjectileType<GleamBomb2>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
				}
				if ((player.ownedProjectileCounts[ModContent.ProjectileType<Ring3>()] > 0) && (player.ownedProjectileCounts[ModContent.ProjectileType<Ring2>()] > 0))
				{
					Projectile.NewProjectile(base.projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ProjectileType<GleamBomb3>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
				}
			}
			if ((player.ownedProjectileCounts[ModContent.ProjectileType<GleamBomb>()] > 0) || (player.ownedProjectileCounts[ModContent.ProjectileType<GleamBomb2>()] > 0) || (player.ownedProjectileCounts[ModContent.ProjectileType<GleamBomb3>()] > 0))
            {
				projectile.timeLeft = 340;
            }
			if (player.ownedProjectileCounts[ModContent.ProjectileType<GleamBomb>()] > 0)
            {
				if (Main.rand.NextBool(60))//controls the speed of when the sparkles spawn
                {
					Projectile.NewProjectile(base.projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ProjectileType<SmallBomb>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
				}
			}
			if (player.ownedProjectileCounts[ModContent.ProjectileType<GleamBomb2>()] > 0)
			{
				if (Main.rand.NextBool(20))//controls the speed of when the sparkles spawn
				{
					Projectile.NewProjectile(base.projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ProjectileType<SmallBomb>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
				}
			}
			if (player.ownedProjectileCounts[ModContent.ProjectileType<GleamBomb3>()] > 0)
			{
				if (Main.rand.NextBool())//controls the speed of when the sparkles spawn
				{
					Projectile.NewProjectile(base.projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ProjectileType<SmallBomb>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
				}
			}



			Lighting.AddLight(projectile.Center, Color.FloralWhite.ToVector3() * 3f);
			
		}
		

	
	}
}
