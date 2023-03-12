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
	public class SilverGemBallProjectile2 : ModProjectile
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
			if (Math.Abs(projectile.oldVelocity.Y) >= 1f)
			{
				Main.PlaySound(base.mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Pokebounce"), base.projectile.Center);
			}

			return false;
		}
		private const int sphereRadius = 3;

		public override void AI()
		{
			Player player = Main.player[base.projectile.owner];
			FairyPlayer modPlayer = player.Fairy();
			Lighting.AddLight(projectile.Center, Color.Magenta.ToVector3() * 1f);
			if (projectile.timeLeft == 10)
			{
				for (int j = 0; j < 72; j++)
				{
					Dust dust = Dust.NewDustPerfect(projectile.Center, 113);
					dust.velocity = ((float)Math.PI * 2f * Vector2.Dot(((float)j / 72f * ((float)Math.PI * 2f)).ToRotationVector2(), player.velocity.SafeNormalize(Vector2.UnitY).RotatedBy((float)j / 72f * ((float)Math.PI * -2f)))).ToRotationVector2();
					dust.velocity = dust.velocity.RotatedBy((float)j / 36f * ((float)Math.PI * 2f)) * 8f;
					dust.noGravity = true;
					dust.scale *= 3.9f;
				}


				{
					Projectile.NewProjectile(projectile.Center + Utils.NextVector2CircularEdge(Main.rand, 8f, 8f), Utils.NextVector2Circular(Main.rand, 12f, 12f), ModContent.ProjectileType<ReturnBallSilver>(), projectile.damage, projectile.knockBack, player.whoAmI);
				}
			}
		}
		

	}
}

