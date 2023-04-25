using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
	public class BigEye : ModProjectile
	{
		public override void SetDefaults()
		{


			base.projectile.width = 114;
			base.projectile.height = 110;
			base.projectile.friendly = true;
			base.projectile.ignoreWater = true;
			base.projectile.timeLeft = 200;
			base.projectile.penetrate = -1;
			base.projectile.tileCollide = false;
			base.projectile.minion = true;
			base.projectile.localNPCHitCooldown = 15;
			base.projectile.minionSlots = 0f;
			base.projectile.netImportant = true;
			base.projectile.usesLocalNPCImmunity = true;



		}
		static int TimetoCharge;
		static int Form;
		static int Form2;
		public override void SetStaticDefaults()
		{
			base.DisplayName.SetDefault("Psychic Turret");
			Main.projFrames[base.projectile.type] = 6;
			Main.projPet[projectile.type] = true;
			ProjectileID.Sets.MinionSacrificable[base.projectile.type] = false;
			ProjectileID.Sets.MinionTargettingFeature[base.projectile.type] = true;
			ProjectileID.Sets.TrailingMode[base.projectile.type] = 0;
			ProjectileID.Sets.TrailCacheLength[base.projectile.type] = 10;
		}

		public override bool MinionContactDamage()
		{
			Player player = Main.player[base.projectile.owner];
			FairyPlayer modPlayer = player.Fairy();
			NPC target = base.projectile.Center.MinionHoming(500f, player);
			if (target != null)
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			
			if (Form <= 0)
			{
				Form2++;
			}
		}
		public override void AI()
		{
			Player player = Main.player[base.projectile.owner];
			FairyPlayer modPlayer = player.Fairy();
			if (Form2 >= 50)
			{
				Form = 3000;
				Form2 = 0;
			}
			Form--;
			if (Form <= 0)
			{
				if (NPC.downedMoonlord)
				{
					projectile.damage = 500 + (player.statDefense / 2);
				}
				else if (NPC.downedPlantBoss)
				{
					projectile.damage = 200 + (player.statDefense / 2);
				}
				else if (Main.hardMode)
				{
					projectile.damage = 90 + (player.statDefense / 2);
				}
				else
				{
					projectile.damage = 10 + (player.statDefense / 2);
				}
			}
			if (Form > 0 )
            {
				if (NPC.downedMoonlord)
				{
					projectile.damage = ((int)((500 + (player.statDefense / 2)) * 1.5));
				}
				else if (NPC.downedPlantBoss)
				{
					projectile.damage = ((int)((200 + (player.statDefense / 2)) * 1.5));
				}
				else if (Main.hardMode)
				{
					projectile.damage = ((int)((90 + (player.statDefense / 2)) * 1.5));
				}
				else
				{
					projectile.damage = ((int)((10 + (player.statDefense / 2)) * 1.5));
				}
			}
			if (player.dead || !player.active)
			{
				player.ClearBuff(ModContent.BuffType<BigEyeBuff>());
				projectile.Kill();
			}
			if (player.HasBuff(ModContent.BuffType<BigEyeBuff>()))
			{
				projectile.timeLeft = 2;
			}
			if (!player.HasBuff(ModContent.BuffType<BigEyeBuff>()))
			{
				projectile.Kill();
			}
			float speed = 4f;
			float inertia = 20f;


			Vector2 idlePosition = player.Center;
			idlePosition.Y -= 148f; // Go up 48 coordinates (three tiles from the center of the player)

			// If your minion doesn't aimlessly move around when it's idle, you need to "put" it into the line of other summoned minions
			// The index is projectile.minionPos
			float minionPositionOffsetX = (20 + projectile.minionPos * 40) * -player.direction;
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
				bool closeThroughWall = between < 2000f;
				if (between < 2000f && (closeThroughWall))
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
						bool closeThroughWall = between < 500f;
						if (((closest && inRange) || !foundTarget) && (closeThroughWall))
						{
							distanceFromTarget = between;
							targetCenter = npc.Center;
							foundTarget = true;
						}
					}
				}
			}


			{
				if (foundTarget)
				{
					{
						Vector2 idlePosition3 = targetCenter;
						idlePosition3.Y -= 130f;
						idlePosition3.X -= 60;
						speed = 30f;
						inertia = 120f;
						Vector2 vectorToIdlePosition3 = idlePosition3 - projectile.Center;
						float distanceToIdlePosition3 = vectorToIdlePosition3.Length();
						Vector2 direction2 = idlePosition3 - projectile.Center;
						direction2.Normalize();
						direction2 *= speed;

						projectile.velocity = (projectile.velocity * (inertia - 8) + direction2) / inertia;
						if (distanceToIdlePosition3 < 310)
						{
							TimetoCharge++;
						}
					}
					if (Form > 0)
					{
						if (TimetoCharge >= 30)
						{
							speed = 2110f;
							inertia = 60f;
							{
								// The immediate range around the target (so it doesn't latch onto it when close)
								Vector2 direction = targetCenter - projectile.Center;
								direction.Normalize();
								direction *= speed;
								projectile.velocity = (projectile.velocity * (inertia - 8) + direction) / inertia;
								Main.PlaySound(base.mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Roar"), projectile.Center);
								TimetoCharge = 0;
							}
						}
					}
					if (Form < 0)
					{
						if (TimetoCharge >= 80)
						{
							speed = 2110f;
							inertia = 60f;
							{
								// The immediate range around the target (so it doesn't latch onto it when close)
								Vector2 direction = targetCenter - projectile.Center;
								direction.Normalize();
								direction *= speed;
								projectile.velocity = (projectile.velocity * (inertia - 8) + direction) / inertia;
								Main.PlaySound(SoundID.Roar, base.projectile.Center, 0);
								TimetoCharge = 0;
							}
						}
					}
				}
			}
			// friendly needs to be set to true so the minion can deal contact damage
			// friendly needs to be set to false so it doesn't damage things like target dummies while idling
			// Both things depend on if it has a target or not, so it's just one assignment here
			// You don't need this assignment if your minion is shooting things instead of dealing contact damage


			Vector2 idlePosition2 = player.Center;
			idlePosition2.Y -= 148f;
			idlePosition2.X += minionPositionOffsetX;
			// Default movement parameters (here for attacking)
			if (!foundTarget)
			{
				projectile.rotation = projectile.AngleTo(player.Center) + (float)(base.projectile.spriteDirection == 1).ToInt() * (float)Math.PI;
			}
			if (foundTarget)
			{
				projectile.rotation = projectile.AngleTo(targetCenter) + (float)(base.projectile.spriteDirection == 1).ToInt() * (float)Math.PI;
			}
			if (!foundTarget)
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


			Lighting.AddLight(projectile.Center, Color.LightBlue.ToVector3() * 0.78f);
			int frameSpeed = 10; //reduced by half due to framecounter speedup
			projectile.frameCounter += 2;
			if (projectile.frameCounter >= frameSpeed)
			{
				projectile.frameCounter = 0;


				{



					{

						base.projectile.frame++;
						if (Form <= 0)
						{
							if (base.projectile.frameCounter >= 10)
							{

								base.projectile.frameCounter = 0;

							}
							if (base.projectile.frame >= 3)
							{
								base.projectile.frame = 0;

							}
						}
						if (Form > 0)
						{
							if (base.projectile.frameCounter >= 10)
							{

								base.projectile.frameCounter = 4;

							}
							if (base.projectile.frame >= 6)
							{
								base.projectile.frame = 4;

							}
						}
					}
				}


			}

		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			Player player = Main.player[base.projectile.owner];
			FairyPlayer modPlayer = player.Fairy();
			Vector2 drawPosition;

			for (int i = 1; i < 3; i++)
			{
				Texture2D texture = Main.projectileTexture[ModContent.ProjectileType<BigEye>()];
				Vector2 startPos = base.projectile.oldPos[i] + base.projectile.Size * 0.5f - Main.screenPosition;
				int frameHeight = texture.Height / Main.projFrames[base.projectile.type];
				int frameY = frameHeight * base.projectile.frame;
				float completionRatio = (float)i / (float)base.projectile.oldPos.Length;
				Color drawColor = Color.Lerp(lightColor, Color.LightPink, 20f);
				drawColor = Color.Lerp(drawColor, Color.DarkViolet, completionRatio);
				drawColor = Color.Lerp(drawColor, Color.Transparent, completionRatio);
				Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
				Vector2 origin = rectangle.Size() / 2f;
				float rotation = base.projectile.rotation;
				float scale = base.projectile.scale;
				SpriteEffects spriteEffects = SpriteEffects.None;
				startPos.Y += 1;
				startPos.X += +17;

				if (base.projectile.spriteDirection == -1)
				{
					spriteEffects = SpriteEffects.FlipHorizontally;
				}
				Main.spriteBatch.Draw(texture, startPos, rectangle, base.projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, layerDepth: 0f);

			}
			
			
				{
					Texture2D texture = Main.projectileTexture[ModContent.ProjectileType<BigEye>()];
					Vector2 startPos = base.projectile.Center - Main.screenPosition + new Vector2(0f, base.projectile.gfxOffY);
					int frameHeight = texture.Height / Main.projFrames[base.projectile.type];
					int frameY = frameHeight * base.projectile.frame;
					Color drawColor = Color.Lerp(lightColor, Color.WhiteSmoke, 20f);
					drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
					Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
					Vector2 origin = rectangle.Size() / 2f;
					float rotation = base.projectile.rotation;
					float scale = base.projectile.scale;
					SpriteEffects spriteEffects = SpriteEffects.None;
					startPos.Y += 1;
					startPos.X += +17;
					if (base.projectile.spriteDirection == -1)
					{
						spriteEffects = SpriteEffects.FlipHorizontally;
					}
					Main.spriteBatch.Draw(texture, startPos, rectangle, base.projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
				}
			
			return false;
		}

	}

}
