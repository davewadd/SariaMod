using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FairyMod.FaiPlayer;
using FairyMod.Projectiles;
using SariaMod.Items.Platinum;
using SariaMod.Items.Diamond;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.Platinum
{
	public class PlatinumBlueAmberPsychicSeeker : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			base.DisplayName.SetDefault("Child");
			Main.projFrames[base.projectile.type] = 8;
			ProjectileID.Sets.MinionShot[base.projectile.type] = true;
		}

		public override void SetDefaults()
		{
			base.projectile.width = 52;
			base.projectile.height = 48;
			base.projectile.netImportant = true;
			base.projectile.friendly = true;
			base.projectile.ignoreWater = true;
			base.projectile.usesLocalNPCImmunity = true;
			base.projectile.localNPCHitCooldown = 7;
			base.projectile.minionSlots = 0f;
			base.projectile.extraUpdates = 1;
			base.projectile.timeLeft = 18000;
			base.projectile.penetrate = -1;
			base.projectile.tileCollide = false;
			base.projectile.timeLeft *= 5;
			base.projectile.minion = true;
		}
		public override bool? CanCutTiles()
		{
			return false;
		}
   
        public override bool MinionContactDamage()
		{
			return true;
		}
		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			target.buffImmune[BuffID.CursedInferno] = false;
			target.buffImmune[BuffID.Confused] = false;
			target.buffImmune[BuffID.Slow] = false;
			target.buffImmune[BuffID.ShadowFlame] = false;
			target.buffImmune[BuffID.Ichor] = false;
			target.buffImmune[BuffID.OnFire] = false;
			target.buffImmune[BuffID.Frostburn] = false;
			target.buffImmune[BuffID.Poisoned] = false;
			target.buffImmune[BuffID.Venom] = false;
			target.buffImmune[BuffID.Electrified] = false;
			target.AddBuff(BuffID.Confused, 300);
			target.AddBuff(BuffID.Slow, 300);
			target.AddBuff(BuffID.Electrified, 300);
			target.AddBuff(BuffID.Poisoned, 300);
			target.AddBuff(BuffID.Venom, 300);
			target.defense /= 10;
			target.noTileCollide = false;
			hitDirection = 1;
		}
		public override void AI()
		{

			Lighting.AddLight(projectile.Center, Color.DarkOrange.ToVector3() * 0.78f);

			Player player = Main.player[base.projectile.owner];
			if (base.projectile.localAI[0] == 0f)
			{
				base.projectile.Fairy().spawnedPlayerMinionDamageValue = player.MinionDamage();
				base.projectile.Fairy().spawnedPlayerMinionProjectileDamageValue = base.projectile.damage;
				base.projectile.localAI[0] = 1f;
			}
			if (player.MinionDamage() != base.projectile.Fairy().spawnedPlayerMinionDamageValue)
			{
				int trueDamage = (int)((float)base.projectile.Fairy().spawnedPlayerMinionProjectileDamageValue / base.projectile.Fairy().spawnedPlayerMinionDamageValue * player.MinionDamage());
				base.projectile.damage = trueDamage;
			}
			if (base.projectile.ai[0] < 0f || base.projectile.ai[0] >= (float)Main.projectile.Length)
			{
				base.projectile.Kill();
				return;
			}
			Projectile mother = Main.projectile[(int)base.projectile.ai[0]];
			if (!mother.active || mother.type != ModContent.ProjectileType<PlatinumBlueSariaMinion>())
			{
				base.projectile.Kill();
				return;
			}
			NPC target = base.projectile.Center.MinionHoming(500f, player);
			if (target != null)
			{
				base.projectile.ai[1] += 1f;
				if (base.projectile.ai[1] % 55f == 54f && Main.myPlayer == base.projectile.owner)
				{
					Projectile.NewProjectile(base.projectile.Center, base.projectile.DirectionTo(target.Center) * 19f, ModContent.ProjectileType<DiamondAmberPsychicBeam>(), mother.damage, mother.knockBack, mother.owner);
				}
			}
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
			float speed = 8f;
			float inertia = 20f;

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
			int frameSpeed = 20;
			{
				base.projectile.frameCounter++;
				if (projectile.frameCounter >= frameSpeed)
					if (base.projectile.frameCounter > 6)
					{
						base.projectile.frame++;
						base.projectile.frameCounter = 0;
					}
				if (base.projectile.frame >= Main.projFrames[base.projectile.type])
				{
					base.projectile.frame = 0;
				}
			}
		}
	}
}