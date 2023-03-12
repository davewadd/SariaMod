using System;
using Microsoft.Xna.Framework;

using SariaMod.Items.Sapphire;
using SariaMod.Items.Ruby;
using SariaMod.Items.Topaz;
using SariaMod.Items.Emerald;
using SariaMod.Items.Amber;
using SariaMod.Items.Amethyst;
using SariaMod.Items.Diamond;
using SariaMod.Items.Platinum;
using SariaMod.Items.Barrier;
using SariaMod.Items.Strange;
using SariaMod.Items.zPearls;
using SariaMod.Items.Bands;
using SariaMod.Buffs;

using Terraria;
using Terraria.ID;
using SariaMod.Dusts;
using Terraria.ModLoader;

namespace SariaMod.Items.Emerald
{
	public class Emeraldfairy: ModProjectile
	{
		public override void SetDefaults()
		{
			
			
			base.projectile.width = 42;
			base.projectile.height = 40;
			base.projectile.hostile = false;
			base.projectile.friendly = false;
			base.projectile.ignoreWater = true;
			
			base.projectile.timeLeft = 2000;
			base.projectile.penetrate = -1;
			base.projectile.tileCollide = false;
	
			base.projectile.localNPCHitCooldown = 20;
		
			base.projectile.netImportant = true;
			base.projectile.usesLocalNPCImmunity = true;
			


		}
		private const int sphereRadius = 1;
		public override void SetStaticDefaults()
		{
			base.DisplayName.SetDefault("Psychic Turret");
			Main.projFrames[base.projectile.type] = 4;
			Main.projPet[projectile.type] = true;
			ProjectileID.Sets.MinionSacrificable[base.projectile.type] = false;
			ProjectileID.Sets.MinionTargettingFeature[base.projectile.type] = true;
		}
        public override bool CanDamage()
        {
			return false;
        }

        public override void AI()
		{
			Player player = Main.player[base.projectile.owner];
			FairyPlayer modPlayer = player.Fairy();
			if (Main.rand.NextBool(20))//controls the speed of when the sparkles spawn
			{
				float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius * sphereRadius));
				double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
				
				
				Dust.NewDust(new Vector2(projectile.Center.X + radius * (float)Math.Cos(angle), projectile.Center.Y + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<Psychic2>(), 0f, 0f, 0, default(Color), 1.5f);
			}//end of dust stuff

			if (player.MinionDamage() != base.projectile.Fairy().spawnedPlayerMinionDamageValue)
			{
				int trueDamage = (int)((float)base.projectile.Fairy().spawnedPlayerMinionProjectileDamageValue / base.projectile.Fairy().spawnedPlayerMinionDamageValue * player.MinionDamage());
				base.projectile.damage = trueDamage;
			}
			if (player.dead || !player.active)
			{
				
				projectile.Kill();
			}
			if ((player.ownedProjectileCounts[ModContent.ProjectileType<PurpleBallReturn>()] > 0f))
			{
				for (int j = 0; j < 72; j++)
				{
					Dust dust = Dust.NewDustPerfect(projectile.Center, 113);
					dust.velocity = ((float)Math.PI * 2f * Vector2.Dot(((float)j / 72f * ((float)Math.PI * 2f)).ToRotationVector2(), player.velocity.SafeNormalize(Vector2.UnitY).RotatedBy((float)j / 72f * ((float)Math.PI * -2f)))).ToRotationVector2();
					dust.velocity = dust.velocity.RotatedBy((float)j / 36f * ((float)Math.PI * 2f)) * 8f;
					dust.noGravity = true;
					dust.scale *= 3.9f;

				}
				Projectile.NewProjectile(projectile.Center, Utils.NextVector2Circular(Main.rand, 0, 2), ModContent.ProjectileType<PurpleGemBallProjectile2>(), projectile.damage, projectile.knockBack, player.whoAmI);
				projectile.Kill();
			}

			projectile.timeLeft = 200;
			float speed = 8f;
			float inertia = 20f;
			for (int i = 0; i < 200; i++)
			{
				NPC target = Main.npc[i];
				float shootToX = target.position.X + (float)target.width * 0.5f - base.projectile.Center.X;
				float shootToY = target.position.Y + (float)target.height * 0.5f - base.projectile.Center.Y;
				float distance = (float)Math.Sqrt(shootToX * shootToX + shootToY * shootToY);

				if (distance < 1020f && target.catchItem == 0 && !target.friendly && Collision.CanHitLine(base.projectile.position, base.projectile.width, base.projectile.height, target.position, target.width, target.height) && target.active && target.type != 488 && base.projectile.ai[0] > 60f)
				{
					distance = 1.6f / distance;
					shootToX *= distance * 3f;
					shootToY *= distance * 3f;
					
					base.projectile.ai[0] = 0f;
				}
			}



			float overlapVelocity = 0.04f;
			for (int i = 0; i < Main.maxProjectiles; i++)
			{
				// Fix overlap with other minions
				Projectile other = Main.projectile[i];
				if (i != projectile.whoAmI && other.active && other.owner == projectile.owner && Math.Abs(projectile.position.X - other.position.X) + Math.Abs(projectile.position.Y - other.position.Y) < projectile.width)
				{
					if (projectile.position.X < other.position.X) projectile.velocity.X -= overlapVelocity;
					else projectile.velocity.X += overlapVelocity;

					if (projectile.position.Y < other.position.Y) projectile.velocity.Y -= overlapVelocity;
					else projectile.velocity.Y += overlapVelocity;
				}
			}

			int owner = player.whoAmI;
			int GiantMoth = ModContent.ProjectileType<EmeraldfairyGem>();
			for (int i = 0; i < 1000; i++)
			{

				if (Main.projectile[i].active && i != base.projectile.whoAmI && ((Main.projectile[i].type == GiantMoth && Main.projectile[i].owner == owner)))
				{


					Vector2 idlePosition = Main.projectile[i].Center;
					idlePosition.Y -= 48f; // Go up 48 coordinates (three tiles from the center of the player)

					// If your minion doesn't aimlessly move around when it's idle, you need to "put" it into the line of other summoned minions
					// The index is projectile.minionPos
					float minionPositionOffsetX = (projectile.minionPos * 10) * player.direction;
					idlePosition.X += minionPositionOffsetX; // Go behind the player

					// All of this code below this line is adapted from Spazmamini code (ID 388, aiStyle 66)

					// Teleport to player if distance is too big
					Vector2 vectorToIdlePosition = idlePosition - projectile.Center;
					float distanceToIdlePosition = vectorToIdlePosition.Length();
					if (Main.myPlayer == player.whoAmI && distanceToIdlePosition > 2000f)
					{
						// Whenever you deal with non-regular events that change the behavior or position drastically, make sure to only run the code on the owner of the projectile,
						// and then set netUpdate to true
						projectile.position = idlePosition;
						projectile.velocity *= 0.1f;
						projectile.netUpdate = true;
					}

					// If your minion is flying, you want to do this independently of any conditions
					



			
					Vector2 idlePosition2 = Main.projectile[i].Center;
					idlePosition2.Y -= 48f;
					idlePosition2.X += minionPositionOffsetX;
					// Default movement parameters (here for attacking)
					projectile.rotation = projectile.velocity.X * 0.05f;



					{
						// Minion doesn't have a target: return to player and idle
						if (distanceToIdlePosition > 450f)
						{
							// Speed up the minion if it's away from the player
							speed = 30f;
							inertia = 60f;
						}
						if (distanceToIdlePosition > 400f)
						{
							// Speed up the minion if it's away from the player
							speed = 8f;
							inertia = 60f;
						}
						else
						{
							// Slow down the minion if closer to the player
							speed = 4f;
							inertia = 80f;
						}
						if (distanceToIdlePosition > 20f)
						{
							// The immediate range around the player (when it passively floats about)

							// This is a simple movement formula using the two parameters and its desired direction to create a "homing" movement
							vectorToIdlePosition.Normalize();
							vectorToIdlePosition *= speed;
							projectile.velocity = (projectile.velocity * (inertia - 1) + vectorToIdlePosition) / inertia;
						}
						else if (projectile.velocity == Vector2.Zero)
						{
							// If there is a case where it's not moving at all, give it a little "poke"
							projectile.velocity.X = -0.15f;
							projectile.velocity.Y = -0.15f;
						}
					}

					if (projectile.velocity.X >= 0)
					{
						projectile.spriteDirection = 1;
					}
					if (projectile.velocity.X <= -0)
					{
						projectile.spriteDirection = -1;
					}
				}
			}
			Lighting.AddLight(projectile.Center, Color.MediumPurple.ToVector3() * 1f);
			int frameSpeed = 10; //reduced by half due to framecounter speedup
			projectile.frameCounter += 2;
			if (projectile.frameCounter >= frameSpeed)
			{
				projectile.frameCounter = 0;

				
				
				{
					
					base.projectile.frame++;
					if (base.projectile.frameCounter >= 4)
					{
						
						base.projectile.frameCounter = 0;

					}
					if (base.projectile.frame >= 4)
					{
						base.projectile.frame = 0;

					}
				}
			}
		}
	}
}
