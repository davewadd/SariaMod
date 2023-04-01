using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;




using System;
using Terraria;
using SariaMod.Buffs;
using SariaMod.Items.Sapphire;
using SariaMod.Dusts;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.Strange
{
	public class Locator : ModProjectile
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
			base.projectile.width = 20;
			base.projectile.height = 20;
			base.projectile.netImportant = true;
			base.projectile.friendly = false;
			base.projectile.ignoreWater = true;
			base.projectile.usesLocalNPCImmunity = true;
			base.projectile.localNPCHitCooldown = 7;
			base.projectile.minionSlots = 0f;
			base.projectile.extraUpdates = 1;
			
			base.projectile.penetrate = 2;
			base.projectile.tileCollide = false;
			base.projectile.timeLeft = 500;
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
		private const int sphereRadius = 3;
		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			Player player = Main.player[projectile.owner];
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
			target.AddBuff(BuffID.Slow, 300);
			target.AddBuff(ModContent.BuffType<SariaCurse2>(), 50);
			Main.PlaySound(SoundID.DD2_WitherBeastDeath, base.projectile.Center);
			FairyPlayer modPlayer = player.Fairy();
			modPlayer.SariaXp++;
			if (player.HasBuff(ModContent.BuffType<StatRaise>()))
			{
				damage = damage;
			}
			if (player.HasBuff(ModContent.BuffType<StatLower>()))
			{
				damage /= 4 ;
			}
			else
            {
				damage -= (damage)/4;
            }
			if ((player.HasBuff(ModContent.BuffType<Overcharged>())))
			{

				int seeker = Projectile.NewProjectile(base.projectile.Center + Utils.NextVector2CircularEdge(Main.rand, 8f, 8f), Utils.NextVector2Circular(Main.rand, 12f, 12f), ModContent.ProjectileType<Locator2>(), base.projectile.damage, base.projectile.knockBack, base.projectile.owner);
				projectile.Kill();
			}
			knockback /= 4;
			projectile.Kill();
		}
		public override void AI()
		{
			Player player = Main.player[projectile.owner];

			Projectile mother = Main.projectile[(int)base.projectile.ai[0]];
			
			if (projectile.timeLeft >= 400)
			{
				base.projectile.rotation += 0.095f;
			}
			if (projectile.timeLeft < 400)
            {
				projectile.aiStyle = 1;
            }
			
			NPC target = base.projectile.Center.MinionHoming(500f, player);
			if (target != null)
			{
				base.projectile.ai[1] += 1f;
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
						float between = Vector2.Distance(npc.Center, player.Center);
						bool closest = Vector2.Distance(projectile.Center, targetCenter) > between;
						bool inRange = between < distanceFromTarget;

						// Additional check for this specific minion behavior, otherwise it will stop attacking once it dashed through an enemy while flying though tiles afterwards
						// The number depends on various parameters seen in the movement code below. Test different ones out until it works alright
						bool closeThroughWall = between < 1000f;
						if (((closest && inRange) || !foundTarget) && (closeThroughWall))
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
			float speed = 70f;
			float inertia = 20f;
			if (projectile.timeLeft == 500)
			{
				Main.PlaySound(SoundID.DD2_SkyDragonsFuryShot, base.projectile.Center);
			}

			// Minion has a target: attack (here, fly towards the enemy)
			if (distanceFromTarget > 40f && projectile.timeLeft <= 400)
			{
				// The immediate range around the target (so it doesn't latch onto it when close)
				Vector2 direction = targetCenter - projectile.Center;
				direction.Normalize();
				direction *= speed;

				projectile.velocity = (projectile.velocity * (inertia - 2) + direction) / inertia;
			}

			else if (projectile.timeLeft > 400)
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
		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			{
				Texture2D starTexture2 = Main.projectileTexture[ModContent.ProjectileType<Locator>()];
				Texture2D starTexture = Main.projectileTexture[ModContent.ProjectileType<LocatorBeam>()];
				Vector2 drawPosition;
				for (int i = 1; i < base.projectile.oldPos.Length; i++)
				{
					float completionRatio = (float)i / (float)base.projectile.oldPos.Length;
					Color drawColor = Color.Lerp(lightColor, Color.LightPink, 2f);
					drawColor = Color.Lerp(drawColor, Color.DarkViolet, completionRatio);
					drawColor = Color.Lerp(drawColor, Color.Transparent, completionRatio);
					drawPosition = base.projectile.oldPos[i] + base.projectile.Size * 0.5f - Main.screenPosition;
					spriteBatch.Draw(starTexture, drawPosition, null, base.projectile.GetAlpha(drawColor),0, Utils.Size(starTexture) * 0.5f, base.projectile.scale, SpriteEffects.None, 0f);
				}
				for (int j = 0; j < 1; j++)
				{
					drawPosition = base.projectile.Center - Main.screenPosition;
					spriteBatch.Draw(starTexture2, drawPosition, null, Color.White, base.projectile.oldRot[j], Utils.Size(starTexture2) * 0.5f, base.projectile.scale, SpriteEffects.None, 0f);
				}
					return false;
				
			}


			
		}

	}
}

