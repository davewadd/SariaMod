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
using Terraria.ModLoader;

namespace SariaMod.Items.Amber
{
	public class AmberBlack2: ModProjectile
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
			Main.projFrames[base.projectile.type] = 2;
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
			if (projectile.timeLeft <= 150)
            {
				projectile.frame = 1;
            }
			if (projectile.timeLeft == 150)
			{
				Main.PlaySound(SoundID.DD2_WitherBeastCrystalImpact, base.projectile.Center);
			}
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
			
				Vector2 idlePosition = mother.Center;
				idlePosition.Y -= 48f; // Go up 48 coordinates (three tiles from the center of the player)

				// If your minion doesn't aimlessly move around when it's idle, you need to "put" it into the line of other summoned minions
				// The index is projectile.minionPos
				float minionPositionOffsetX = (10 + projectile.minionPos * 40) * -player.direction;
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



			
			Vector2 idlePosition2 = player.Center;
			
			idlePosition2.X += minionPositionOffsetX;
			// Default movement parameters (here for attacking)
			projectile.rotation = projectile.velocity.X * 0.05f;

			projectile.velocity.X = mother.velocity.X;
				
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
					
				}
			
				if (projectile.velocity.X >= 0)
				{
					projectile.spriteDirection = 1;
				}
				if (projectile.velocity.X <= -0)
				{
					projectile.spriteDirection = -1;
				}
			if (projectile.timeLeft == 10)
			{
				for (int j = 0; j < 5; j++) //set to 2
				{
					Projectile.NewProjectile(base.projectile.Center + new Vector2(0f, 0f), Vector2.One.RotatedByRandom(6.2831854820251465) * 1f, ModContent.ProjectileType<AmberShard>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
				}
				Main.PlaySound(SoundID.DD2_WitherBeastCrystalImpact, base.projectile.Center);
				for (int j = 0; j < 2; j++) //set to 2
				{
					Projectile.NewProjectile(base.projectile.Center + new Vector2( 0f, 0f), Vector2.One.RotatedByRandom(6.2831854820251465) * 1f, ModContent.ProjectileType<BlackMoth>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
				}
			}
			Lighting.AddLight(projectile.Center, Color.Orange.ToVector3() * 1f);
			
		}
	}
}
