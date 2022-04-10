using System;
using Microsoft.Xna.Framework;
using FairyMod.FaiPlayer;
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
using SariaMod.Items.Playerattack;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.Playerattack
{
	public class Psybeam : ModProjectile
	{
		public override void SetDefaults()
		{
			
			
			base.projectile.width = 42;
			base.projectile.height = 40;
			base.projectile.hostile = false;
			base.projectile.friendly = false;
			base.projectile.ignoreWater = true;
			Main.projFrames[base.projectile.type] = 8;
			base.projectile.timeLeft = 200;
			base.projectile.penetrate = -1;
			base.projectile.tileCollide = false;
			
		
			
		}
		
		public override void SetStaticDefaults()
		{
			base.DisplayName.SetDefault("Psychic Turret");
			ProjectileID.Sets.MinionTargettingFeature[projectile.type] = true;
		}
		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{

			damage /= 4;
		
		}
		
		public override void AI()
		{
			Player player = Main.player[base.projectile.owner];
			FairyPlayer modPlayer = player.Fairy();
			if (player.ownedProjectileCounts[ModContent.ProjectileType<Psybeam>()] > 0)
			{
				
				base.projectile.timeLeft = 1800;
			}
			
			if (player.HasBuff(ModContent.BuffType<SariaBuff>()) && (player.ownedProjectileCounts[ModContent.ProjectileType<Psybeam>()] > 1f))
			{
				SariaModUtilities.KillShootProjectile(player, ModContent.ProjectileType<Psybeam>());
			}
			if (player.HasBuff(ModContent.BuffType<SapphireSariaBuff>()) && (player.ownedProjectileCounts[ModContent.ProjectileType<Psybeam>()] > 1f))
			{
				SariaModUtilities.KillShootProjectile(player, ModContent.ProjectileType<Psybeam>());
			}
			if (player.HasBuff(ModContent.BuffType<RubySariaBuff>()) && (player.ownedProjectileCounts[ModContent.ProjectileType<Psybeam>()] > 2f))
			{
				SariaModUtilities.KillShootProjectile(player, ModContent.ProjectileType<Psybeam>());
			}
			if (player.HasBuff(ModContent.BuffType<TopazSariaBuff>()) && (player.ownedProjectileCounts[ModContent.ProjectileType<Psybeam>()] > 2f))
			{
				SariaModUtilities.KillShootProjectile(player, ModContent.ProjectileType<Psybeam>());
			}
			if (player.HasBuff(ModContent.BuffType<EmeraldSariaBuff>()) && (player.ownedProjectileCounts[ModContent.ProjectileType<Psybeam>()] > 2f))
			{
				SariaModUtilities.KillShootProjectile(player, ModContent.ProjectileType<Psybeam>());
			}
			if (player.HasBuff(ModContent.BuffType<AmberSariaBuff>()) && (player.ownedProjectileCounts[ModContent.ProjectileType<Psybeam>()] > 3f))
			{
				SariaModUtilities.KillShootProjectile(player, ModContent.ProjectileType<Psybeam>());
			}
			if (player.HasBuff(ModContent.BuffType<AmethystSariaBuff>()) && (player.ownedProjectileCounts[ModContent.ProjectileType<Psybeam>()] > 3f))
			{
				SariaModUtilities.KillShootProjectile(player, ModContent.ProjectileType<Psybeam>());
			}
			if (player.HasBuff(ModContent.BuffType<DiamondSariaBuff>()) && (player.ownedProjectileCounts[ModContent.ProjectileType<Psybeam>()] > 4f))
			{
				SariaModUtilities.KillShootProjectile(player, ModContent.ProjectileType<Psybeam>());
			}
			if (player.HasBuff(ModContent.BuffType<PlatinumSariaBuff>()) && (player.ownedProjectileCounts[ModContent.ProjectileType<Psybeam>()] > 5f))
			{
				SariaModUtilities.KillShootProjectile(player, ModContent.ProjectileType<Psybeam>());
			}
			if (player.HasBuff(ModContent.BuffType<PlatinumBlueSariaBuff>()) && (player.ownedProjectileCounts[ModContent.ProjectileType<Psybeam>()] > 18f))
			{
				SariaModUtilities.KillShootProjectile(player, ModContent.ProjectileType<Psybeam>());
			}
			if (player.HasBuff(ModContent.BuffType<PlatinumPurpleSariaBuff>()) && (player.ownedProjectileCounts[ModContent.ProjectileType<Psybeam>()] > 18f))
			{
				SariaModUtilities.KillShootProjectile(player, ModContent.ProjectileType<Psybeam>());
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
					Projectile.NewProjectile(base.projectile.Center, base.projectile.DirectionTo(target.Center) * 10f, ModContent.ProjectileType<PsyBlade>(), base.projectile.damage, 0f, base.projectile.owner);
					base.projectile.ai[0] = 0f;
				}
			}
			if (player.statLife < (player.statLifeMax2) - (player.statLifeMax2 / 4))
			{
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
					// Reasonable distance away so it doesn't target across multiple screens
					if (between < 2000f)
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
							bool closeThroughWall = between < 10f;
							if (((closest && inRange) || !foundTarget) && (lineOfSight || closeThroughWall))
							{
								distanceFromTarget = between;
								targetCenter = npc.Center;
								foundTarget = true;
							}
						}
					}
				}

				// friendly needs to be set to true so the minion can deal contact damage
				// friendly needs to be set to false so it doesn't damage things like target dummies while idling
				// Both things depend on if it has a target or not, so it's just one assignment here
				// You don't need this assignment if your minion is shooting things instead of dealing contact damage
				projectile.friendly = foundTarget;




				// Default movement parameters (here for attacking)
				

				if (foundTarget)
				{
					// Minion has a target: attack (here, fly towards the enemy)
					if (distanceFromTarget > 40f)
					{
						// The immediate range around the target (so it doesn't latch onto it when close)
						Vector2 direction = targetCenter - projectile.Center;
						direction.Normalize();
						direction *= speed;
						projectile.velocity = (projectile.velocity * (inertia - 2) + direction) / inertia;
					}
				}
				else
				{
					// Minion doesn't have a target: return to player and idle
					if (distanceToIdlePosition > 600f)
					{
						// Speed up the minion if it's away from the player
						speed = 12f;
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
						projectile.velocity.Y = -0.05f;
					}
				}
			}
			else if (player.statLife >= (player.statLifeMax2)-(player.statLifeMax2/4))
            {
				projectile.velocity.X = 0;
				projectile.velocity.Y = 0;
            }
			Lighting.AddLight(projectile.Center, Color.LightBlue.ToVector3() * 0.78f);
			base.projectile.ai[0] += 1f;
			base.projectile.frameCounter++;
			if (base.projectile.frameCounter > 10)
			{
				base.projectile.frame++;
				base.projectile.frameCounter = 0;
			}
			if (base.projectile.frame >= 8)
			{
				base.projectile.frame = 0;
			}

		}
	}
}
