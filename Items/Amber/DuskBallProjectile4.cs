using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;



using SariaMod.Items.Strange;
using System;
using Terraria;
using SariaMod.Buffs;
using SariaMod.Items.Sapphire;
using SariaMod.Dusts;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.Amber
{
	public class DuskBallProjectile4 : ModProjectile
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
			if (Math.Abs(Projectile.oldVelocity.Y) >= 1f)
			{
				SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Pokebounce"), base.Projectile.Center);
			}

			return false;
		}
		private const int sphereRadius = 3;

		public override void AI()
		{
			Player player = Main.player[base.Projectile.owner];
			FairyPlayer modPlayer = player.Fairy();
			Lighting.AddLight(Projectile.Center, Color.Green.ToVector3() * 1f);
			if (Projectile.timeLeft == 10)
			{
				for (int j = 0; j < 72; j++)
				{
					Dust dust = Dust.NewDustPerfect(Projectile.Center, 113);
					dust.velocity = ((float)Math.PI * 2f * Vector2.Dot(((float)j / 72f * ((float)Math.PI * 2f)).ToRotationVector2(), player.velocity.SafeNormalize(Vector2.UnitY).RotatedBy((float)j / 72f * ((float)Math.PI * -2f)))).ToRotationVector2();
					dust.velocity = dust.velocity.RotatedBy((float)j / 36f * ((float)Math.PI * 2f)) * 8f;
					dust.noGravity = true;
					dust.scale *= 3.9f;
				}
				Item.NewItem(Projectile.GetSource_FromThis(), (int)(Projectile.position.X + 0), (int)(Projectile.position.Y + 0), 0, 0, ModContent.ItemType<DuskBall2>());


			}
		}
		

	}
}

