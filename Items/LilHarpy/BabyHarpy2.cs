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

namespace SariaMod.Items.LilHarpy
{
	public class BabyHarpy2: ModProjectile
	{
		public override void SetDefaults()
		{
			
			
			base.projectile.width = 42;
			base.projectile.height = 40;
			base.projectile.friendly = true;
			base.projectile.ignoreWater = true;
			base.projectile.timeLeft = 200;
			base.projectile.penetrate = -1;
			base.projectile.tileCollide = false;
			base.projectile.minion = true;
			base.projectile.localNPCHitCooldown = 20;
			base.projectile.minionSlots = 0f;
			base.projectile.netImportant = true;
			base.projectile.usesLocalNPCImmunity = true;
			


		}
		static int Timer;
		public override void SetStaticDefaults()
		{
			base.DisplayName.SetDefault("Psychic Turret");
			Main.projFrames[base.projectile.type] = 10;
			Main.projPet[projectile.type] = true;
			ProjectileID.Sets.MinionSacrificable[base.projectile.type] = false;
			ProjectileID.Sets.MinionTargettingFeature[base.projectile.type] = true;
		}

		public override bool MinionContactDamage()
		{
			Player player = Main.player[base.projectile.owner];
			FairyPlayer modPlayer = player.Fairy();
			NPC target = base.projectile.Center.MinionHoming(500f, player);
			if (target != null)
			{
				return false;
			}
			else
			{
				return false;
			}
		}
		public override void AI()
		{
			Player player = Main.player[base.projectile.owner];
			FairyPlayer modPlayer = player.Fairy();
			if (NPC.downedMoonlord)
			{
				projectile.damage = 100 * player.maxMinions;
			}
			else if (NPC.downedPlantBoss)
			{
				projectile.damage = 75 * player.maxMinions;
			}
			else if (Main.hardMode)
			{
				projectile.damage = 40 * player.maxMinions;
			}
			else 
			{
				projectile.damage = 10 * player.maxMinions;
			}
			
			if (player.dead || !player.active)
			{
				player.ClearBuff(ModContent.BuffType<BabyHarpyBuff>());
				projectile.Kill();
			}
			if (player.HasBuff(ModContent.BuffType<BabyHarpyBuff>()))
			{
				projectile.timeLeft = 2;
			}
			if (!player.HasBuff(ModContent.BuffType<BabyHarpyBuff>()))
			{
				projectile.Kill();
			}
			float speed = 8f;
			float inertia = 20f;
			
			
				Vector2 idlePosition = player.Center;
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



				// Starting search distance
				float distanceFromTarget = 10f;
				Vector2 targetCenter = projectile.position;
				bool foundTarget = false;

				// This code is required if your minion weapon has the targeting feature
				if (player.HasMinionAttackTargetNPC)
				{
					NPC npc = Main.npc[player.MinionAttackTargetNPC];
					float between = Vector2.Distance(npc.Center, projectile.Center);
				bool lineOfSight = Collision.CanHitLine(projectile.position, projectile.width, projectile.height, npc.position, npc.width, npc.height);
				// Reasonable distance away so it doesn't target across multiple screens
				bool closeThroughWall = between < 320f;
				if (between < 2000f && (lineOfSight || closeThroughWall))
				{
						distanceFromTarget = between;
						targetCenter = npc.Center;
						foundTarget = true;
					}
				}
				if (!foundTarget)
				{
					// This code is required either way, used for finding a target
					for (int i = 0; i < Main.maxNPCs; i++)
					{
						NPC npc = Main.npc[i];
						if (npc.CanBeChasedBy())
						{
							float between = Vector2.Distance(npc.Center, projectile.Center);
							bool closest = Vector2.Distance(projectile.Center, targetCenter) > between;
							bool inRange = between < distanceFromTarget;
							bool lineOfSight = Collision.CanHitLine(projectile.position, projectile.width, projectile.height, npc.position, npc.width, npc.height);
							// Additional check for this specific minion behavior, otherwise it will stop attacking once it dashed through an enemy while flying though tiles afterwards
							// The number depends on various parameters seen in the movement code below. Test different ones out until it works alright
							bool closeThroughWall = between < 260f;
							if (((between < 500) && (lineOfSight || closeThroughWall)))
							{
								distanceFromTarget = between;
								targetCenter = npc.Center;
								foundTarget = true;
							}
						}
					}
				}
			int attacktimer = 120;
			if (NPC.downedMoonlord)
			{
				attacktimer = 60;
			}
			else if (NPC.downedPlantBoss)
			{
				attacktimer = 80;
			}
			else if (Main.hardMode)
			{
				attacktimer = 95;
			}
			else if (NPC.downedBoss1)
			{
				attacktimer = 105;
			}
			Timer++;
			if (Timer >= attacktimer && foundTarget && (player.ownedProjectileCounts[ModContent.ProjectileType<Feather>()] <= 0f))
             {
				
				
					Projectile.NewProjectile(base.projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 1f, ModContent.ProjectileType<Feather>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
				
				Timer = 0;
                }
			// friendly needs to be set to true so the minion can deal contact damage
			// friendly needs to be set to false so it doesn't damage things like target dummies while idling
			// Both things depend on if it has a target or not, so it's just one assignment here
			// You don't need this assignment if your minion is shooting things instead of dealing contact damage
			

			Vector2 idlePosition2 = player.Center;
			idlePosition2.Y -= 48f;
			idlePosition2.X += minionPositionOffsetX;
			// Default movement parameters (here for attacking)
			projectile.rotation = projectile.velocity.X * 0.05f;

			{ 

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
			}
				if (projectile.velocity.X >= 0)
				{
					projectile.spriteDirection = -1;
				}
				if (projectile.velocity.X <= -0)
				{
					projectile.spriteDirection = 1;
				}
			
			Lighting.AddLight(projectile.Center, Color.LightBlue.ToVector3() * 0.78f);
			int frameSpeed = 10; //reduced by half due to framecounter speedup
			projectile.frameCounter += 2;
			if (projectile.frameCounter >= frameSpeed)
			{
				projectile.frameCounter = 0;

				if (distanceToIdlePosition >= 400)
				{
					
					projectile.frame++;

					if (base.projectile.frameCounter < 10)
					{

						base.projectile.frameCounter = 0;

					}
					if (base.projectile.frame >= 10)
					{
						base.projectile.frame = 7;
					}
					if (base.projectile.frame < 7)
					{
						base.projectile.frame = 7;

					}
				}
				else if (distanceToIdlePosition < 400)
				{
					
					base.projectile.frame++;
					if (base.projectile.frameCounter >= 10)
					{
						
						base.projectile.frameCounter = 0;

					}
					if (base.projectile.frame >= 6)
					{
						base.projectile.frame = 0;

					}
				}

			}
		}
	}
}
