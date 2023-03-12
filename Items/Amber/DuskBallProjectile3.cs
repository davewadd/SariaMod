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

namespace SariaMod.Items.Amber
{
	public class DuskBallProjectile3 : ModProjectile
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
				Main.PlaySound(base.mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Pokebounce"), base.projectile.Center);
			}

				return false;
		}
		private const int sphereRadius = 3;

		public override void AI()
		{
			Player player = Main.player[base.projectile.owner];
			FairyPlayer modPlayer = player.Fairy();
			Lighting.AddLight(projectile.Center, Color.Green.ToVector3() * 1f);
			int owner = player.whoAmI;
			int GiantMoth = ModContent.ProjectileType<GreenMothGoliath>();
			for (int i = 0; i < 1000; i++)
			{

				if (Main.projectile[i].active && i != base.projectile.whoAmI && ((Main.projectile[i].type == GiantMoth && Main.projectile[i].owner == owner)))
				{
					

					Vector2 idlePosition = Main.projectile[i].Center;
					Vector2 vectorToIdlePosition = idlePosition - projectile.Center;
					float distanceToIdlePosition = vectorToIdlePosition.Length();
					// Default movement parameters (here for attacking)
					

					{

						{
							// Minion doesn't have a target: return to player and idle

							// Speed up the minion if it's away from the player
							

							if (distanceToIdlePosition < 100f)
							{
								for (int j = 0; j < 72; j++)
								{
									Dust dust = Dust.NewDustPerfect(projectile.Center, 113);
									dust.velocity = ((float)Math.PI * 2f * Vector2.Dot(((float)j / 72f * ((float)Math.PI * 2f)).ToRotationVector2(), player.velocity.SafeNormalize(Vector2.UnitY).RotatedBy((float)j / 72f * ((float)Math.PI * -2f)))).ToRotationVector2();
									dust.velocity = dust.velocity.RotatedBy((float)j / 36f * ((float)Math.PI * 2f)) * 8f;
									dust.noGravity = true;
									dust.scale *= 3.9f;

								}
								Main.PlaySound(SoundID.Item30, base.projectile.Center);
								Projectile.NewProjectile(projectile.Center, Utils.NextVector2Circular(Main.rand, 0, 2), ModContent.ProjectileType<DuskBallProjectile4>(), projectile.damage, projectile.knockBack, player.whoAmI);
								Main.projectile[i].Kill();
								projectile.Kill();

								
								
							}

						}
					}
					
					

				}


			}
			if (projectile.timeLeft == 10)
			{
				Item.NewItem(projectile.Center + new Vector2(0f, 0f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ItemType<DuskBall>());
			}
		}
		

	}
}

