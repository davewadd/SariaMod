using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;



using SariaMod.Items.Strange;
using System;
using Terraria;
using SariaMod.Buffs;
using SariaMod.Items.Sapphire;
using SariaMod.Dusts;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.Emerald
{
	public class EmeraldfairyGem : ModProjectile
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
			base.projectile.penetrate = 2;
			base.projectile.tileCollide = true;
			base.projectile.timeLeft = 200;
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
			if (base.projectile.velocity.X != oldVelocity.X)
			{
				base.projectile.velocity.X = 0f - oldVelocity.X;
			}
			{
				base.projectile.velocity.Y = 0f - (oldVelocity.Y * .6f);

			}


			return false;
		}
		private const int sphereRadius = 3;

		public override void AI()
		{
			Player player = Main.player[base.projectile.owner];
			FairyPlayer modPlayer = player.Fairy();
			projectile.Center = player.Center;
			
			if (base.projectile.localAI[0] == 0f)
			{
				base.projectile.Fairy().spawnedPlayerMinionDamageValue = player.MinionDamage();
				base.projectile.Fairy().spawnedPlayerMinionProjectileDamageValue = base.projectile.damage;
				for (int j = 0; j < 1; j++) //set to 2
				{
					Projectile.NewProjectile(base.projectile.Center, new Vector2(0, 0), ModContent.ProjectileType<PinkFairyBarrier>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
				}
				base.projectile.localAI[0] = 1f;
			}
			projectile.timeLeft = 2;
			if ((player.ownedProjectileCounts[ModContent.ProjectileType<PurpleBallReturn>()] > 0f))
			{
				projectile.Kill();
			}

		}
	}
}

