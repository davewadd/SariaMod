using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using SariaMod.Dusts;
using System;
using Terraria;
using SariaMod.Items.Sapphire;
using Terraria.ID;
using SariaMod.Buffs;
using Terraria.ModLoader;

namespace SariaMod.Items.Sapphire
{
	public class Bubble : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			base.DisplayName.SetDefault("Child");
			Main.projFrames[base.projectile.type] = 1;
			ProjectileID.Sets.MinionShot[base.projectile.type] = true;
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
			projectile.alpha = 100;
			base.projectile.penetrate = -1;
			base.projectile.tileCollide = false;
			base.projectile.timeLeft = 1000;
			base.projectile.minion = true;
		}
		public override bool? CanCutTiles()
		{
			return false;
		}
		public override bool MinionContactDamage()
		{
			Player player = Main.player[base.projectile.owner];
			FairyPlayer modPlayer = player.Fairy();
			NPC target = base.projectile.Center.MinionHoming(500f, player);
			if (projectile.frame <= 2)
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		private const int sphereRadius2 = 6;
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
			
			if (Main.rand.NextBool())//controls the speed of when the sparkles spawn
			{
				float radius = (float)Math.Sqrt(Main.rand.Next(34 * 34));
				double angle = Main.rand.NextDouble() * 5.0 * Math.PI;


				Dust.NewDust(new Vector2(projectile.Center.X + radius * (float)Math.Cos(angle), projectile.Center.Y + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<Water>(), 0f, 0f, 0, default(Color), 1.5f);
			}//end of dust stuff
			target.AddBuff(BuffID.Frostburn, 300);
			target.AddBuff(BuffID.Slow, 300);
			projectile.scale= 1.5f;
			knockback /= 100;
			
				Main.PlaySound(base.mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Bubblepop"), projectile.Center);
			
			if (player.HasBuff(ModContent.BuffType<StatRaise>()))
			{
				damage = (damage);
			}
			if (player.HasBuff(ModContent.BuffType<StatLower>()))
			{
				damage /= 4;

			}
			else
            {
				damage -= (damage)/2;
            }
			if (projectile.timeLeft > 20)
			{
				projectile.timeLeft = 20;
			}
			Projectile.NewProjectile(base.projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ProjectileType<HealBubble>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
		}
		public override void AI()
		{
			Player player = Main.player[projectile.owner];

			Projectile mother = Main.projectile[(int)base.projectile.ai[0]];
			if (Main.rand.NextBool(20))//controls the speed of when the sparkles spawn
			{
				float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius2 * sphereRadius2));
				double angle = Main.rand.NextDouble() * 5.0 * Math.PI;

				Dust.NewDust(new Vector2((projectile.Center.X) + radius * (float)Math.Cos(angle), (projectile.Center.Y) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<Cold>(), 0f, 0f, 0, default(Color), 1.5f);
			}
			if (Main.rand.NextBool(20))//controls the speed of when the sparkles spawn
			{
				float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius2 * sphereRadius2));
				double angle = Main.rand.NextDouble() * 5.0 * Math.PI;

				Dust.NewDust(new Vector2((projectile.Center.X) + radius * (float)Math.Cos(angle), (projectile.Center.Y) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<Snow2>(), 0f, 0f, 0, default(Color), 1.5f);
			}


			NPC target = base.projectile.Center.MinionHoming(500f, player);
			if (target != null)
			{
				base.projectile.ai[1] += 1f;
			}
			if (projectile.frame >= 5)
            {
				projectile.Kill();
            }
			Vector2 idlePosition = player.Center;
			idlePosition.Y -= 48f; // Go up 48 coordinates (three tiles from the center of the player)

			// If your minion doesn't aimlessly move around when it's idle, you need to "put" it into the line of other summoned minions
			// The index is projectile.minionPos
			float minionPositionOffsetX = (10 + projectile.minionPos * 40) * player.direction;
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
			
			if (projectile.timeLeft <= 10)
            {
				projectile.scale = 1.5f;
				if (Main.rand.NextBool())//controls the speed of when the sparkles spawn
				{
					float radius = (float)Math.Sqrt(Main.rand.Next(34 * 34));
					double angle = Main.rand.NextDouble() * 5.0 * Math.PI;


					Dust.NewDust(new Vector2(projectile.Center.X + radius * (float)Math.Cos(angle), projectile.Center.Y + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<Water>(), 0f, 0f, 0, default(Color), 1.5f);
				}//end of dust stuff
				Main.PlaySound(base.mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Bubblepop"), projectile.Center);
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
			if (projectile.timeLeft < 20)
            {
				Main.PlaySound(base.mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/BubblePop"), projectile.Center);
			}
			// friendly needs to be set to true so the minion can deal contact damage
			// friendly needs to be set to false so it doesn't damage things like target dummies while idling
			// Both things depend on if it has a target or not, so it's just one assignment here
			// You don't need this assignment if your minion is shooting things instead of dealing contact damage
			projectile.friendly = foundTarget;



			Lighting.AddLight(projectile.Center, Color.LightBlue.ToVector3() * 0.78f);
			// Default movement parameters (here for attacking)
			float speed = 12f;
			float inertia = 12f;
			if (projectile.timeLeft == 3000)
            {
				Main.PlaySound(SoundID.Drown, base.projectile.Center);
			}
			
			{
				// Minion has a target: attack (here, fly towards the enemy)
				if (distanceFromTarget > 20f )
				{
					if (player.HasBuff(ModContent.BuffType<StatRaise>()))
					{
						speed *= 2;
					}
					if (player.HasBuff(ModContent.BuffType<StatLower>()))
					{
						speed /= 2;

					}
					// The immediate range around the target (so it doesn't latch onto it when close)
					Vector2 direction = targetCenter - projectile.Center;
					direction.Normalize();
					direction *= speed;
					
					projectile.velocity = (projectile.velocity * (inertia - 2) + direction) / 20;
				}
			}
		
			
			

			
			
		}
	}
}

