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
			Main.projFrames[base.Projectile.type] = 1;
			ProjectileID.Sets.MinionShot[base.Projectile.type] = true;
			ProjectileID.Sets.TrailingMode[base.Projectile.type] = 2;
			ProjectileID.Sets.TrailCacheLength[base.Projectile.type] = 30;

		}

		public override void SetDefaults()
		{
			base.Projectile.width = 16;
			base.Projectile.height = 16;
			base.Projectile.netImportant = true;
			base.Projectile.friendly = false;
			base.Projectile.ignoreWater = true;
			base.Projectile.usesLocalNPCImmunity = true;
			base.Projectile.localNPCHitCooldown = 7;
			base.Projectile.minionSlots = 0f;
			base.Projectile.extraUpdates = 1;
			base.Projectile.penetrate = 2;
			base.Projectile.tileCollide = true;
			base.Projectile.timeLeft = 200;
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
			Player player = Main.player[base.Projectile.owner];
			FairyPlayer modPlayer = player.Fairy();
			if (base.Projectile.velocity.X != oldVelocity.X)
			{
				base.Projectile.velocity.X = 0f - oldVelocity.X;
			}
			{
				base.Projectile.velocity.Y = 0f - (oldVelocity.Y * .6f);

			}


			return false;
		}
		private const int sphereRadius = 3;

		public override void AI()
		{
			Player player = Main.player[base.Projectile.owner];
			FairyPlayer modPlayer = player.Fairy();
			Projectile.Center = player.Center;
			
			
			Projectile.timeLeft = 2;
			if ((player.ownedProjectileCounts[ModContent.ProjectileType<PurpleBallReturn>()] > 0f))
			{
				Projectile.Kill();
			}

		}
	}
}

