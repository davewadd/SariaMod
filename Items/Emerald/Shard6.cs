using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;




using System;
using Terraria;
using SariaMod.Buffs;
using SariaMod.Items.Sapphire;
using SariaMod.Dusts;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.Emerald
{
	public class Shard6 : ModProjectile
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
			Projectile.aiStyle = 14;
			
			base.Projectile.penetrate = 2;
			base.Projectile.tileCollide = true;
			base.Projectile.timeLeft = 300;
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


			{
				base.Projectile.velocity.X = 0f - (oldVelocity.X * -.6f);

			}
			
			{
				base.Projectile.velocity.Y = 0f - (oldVelocity.Y*.6f);
				
			}
			if (Math.Abs(Projectile.oldVelocity.Y) >= 1f)
            {
				SoundEngine.PlaySound(SoundID.Item49, base.Projectile.Center);
			}

				return false;
		}
		private const int sphereRadius = 3;

		public override void AI()
		{
			Player player = Main.player[base.Projectile.owner];
			FairyPlayer modPlayer = player.Fairy();
			if (Projectile.timeLeft == 10)
			{
				for (int k = 0; k < 4; k++)
				{
					Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + new Vector2(0f, 0f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ProjectileType<SpecialRupeeShard>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
					Projectile.Kill();
				}
			}
			Lighting.AddLight(Projectile.Center, Color.Magenta.ToVector3() * 1f);
		}
		

	}
}

