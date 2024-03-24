using System;
using Microsoft.Xna.Framework;

using SariaMod.Items.Sapphire;
using SariaMod.Items.Ruby;
using SariaMod.Items.Topaz;
using SariaMod.Items.Emerald;
using SariaMod.Items.Amber;
using SariaMod.Items.Amethyst;
 
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
	public class EmeraldfairySilver: ModProjectile
	{
		public override void SetDefaults()
		{
			
			
			base.Projectile.width = 42;
			base.Projectile.height = 40;
			base.Projectile.hostile = false;
			base.Projectile.friendly = false;
			base.Projectile.ignoreWater = true;
			
			base.Projectile.timeLeft = 2000;
			base.Projectile.penetrate = -1;
			base.Projectile.tileCollide = false;
			base.Projectile.minion = false;
			base.Projectile.localNPCHitCooldown = 20;
			base.Projectile.minionSlots = 0f;
			base.Projectile.netImportant = true;
			base.Projectile.usesLocalNPCImmunity = true;
			


		}
		private const int sphereRadius = 1;
		private  int Timer;
		public override void SetStaticDefaults()
		{
			base.DisplayName.SetDefault("Psychic Turret");
			Main.projFrames[base.Projectile.type] = 4;
			Main.projPet[Projectile.type] = true;
			ProjectileID.Sets.MinionSacrificable[base.Projectile.type] = false;
			ProjectileID.Sets.MinionTargettingFeature[base.Projectile.type] = true;
		}
        public override bool? CanDamage()/* tModPorter Suggestion: Return null instead of true */
        {
			return false;
        }

        public override void AI()
		{
			Player player = Main.player[base.Projectile.owner];
			FairyPlayer modPlayer = player.Fairy();
			if (Main.rand.NextBool(20))//controls the speed of when the sparkles spawn
			{
				float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius * sphereRadius));
				double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
				
				
				Dust.NewDust(new Vector2(Projectile.Center.X + radius * (float)Math.Cos(angle), Projectile.Center.Y + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<Psychic3>(), 0f, 0f, 0, default(Color), 1.5f);
			}//end of dust stuff
			int Healtimer = 550;
			Timer++;
			
			if (player.dead || !player.active)
			{
				for (int j = 0; j < 72; j++)
				{
					Dust dust = Dust.NewDustPerfect(Projectile.Center, 113);
					dust.velocity = ((float)Math.PI * 2f * Vector2.Dot(((float)j / 72f * ((float)Math.PI * 2f)).ToRotationVector2(), player.velocity.SafeNormalize(Vector2.UnitY).RotatedBy((float)j / 72f * ((float)Math.PI * -2f)))).ToRotationVector2();
					dust.velocity = dust.velocity.RotatedBy((float)j / 36f * ((float)Math.PI * 2f)) * 8f;
					dust.noGravity = true;
					dust.scale *= 3.9f;

				}
				Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + 0, 0, 0, ModContent.ProjectileType<SilverGemBallProjectile2>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
				
				Projectile.Kill();
			}
			if ((player.ownedProjectileCounts[ModContent.ProjectileType<SilverBallReturn>()] > 0f))
			{
				for (int j = 0; j < 72; j++)
				{
					Dust dust = Dust.NewDustPerfect(Projectile.Center, 113);
					dust.velocity = ((float)Math.PI * 2f * Vector2.Dot(((float)j / 72f * ((float)Math.PI * 2f)).ToRotationVector2(), player.velocity.SafeNormalize(Vector2.UnitY).RotatedBy((float)j / 72f * ((float)Math.PI * -2f)))).ToRotationVector2();
					dust.velocity = dust.velocity.RotatedBy((float)j / 36f * ((float)Math.PI * 2f)) * 8f;
					dust.noGravity = true;
					dust.scale *= 3.9f;

				}
				Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + 0, 0, 0, ModContent.ProjectileType<SilverGemBallProjectile2>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
				
				Projectile.Kill();
			}

			if (player.statLife < (player.statLifeMax2) / 2 && Timer >= Healtimer)
			{
				SariaModUtilities.HealingProjectile2(base.Projectile, 120, base.Projectile.owner, 12f, 15f, autoHomes: false);
				Timer = 0;
			}
			else if (player.statLife < (player.statLifeMax2) && Timer >= Healtimer)
			{
				SariaModUtilities.HealingProjectile2(base.Projectile, 45, base.Projectile.owner, 12f, 15f, autoHomes: false);
				Timer = 0;
			}
			Projectile.timeLeft = 200;
			float speed = 8f;
			float inertia = 20f;
			for (int i = 0; i < 200; i++)
			{
				NPC target = Main.npc[i];
				float shootToX = target.position.X + (float)target.width * 0.5f - base.Projectile.Center.X;
				float shootToY = target.position.Y + (float)target.height * 0.5f - base.Projectile.Center.Y;
				float distance = (float)Math.Sqrt(shootToX * shootToX + shootToY * shootToY);

				if (distance < 1020f && target.catchItem == 0 && !target.friendly && Collision.CanHitLine(base.Projectile.position, base.Projectile.width, base.Projectile.height, target.position, target.width, target.height) && target.active && target.type != 488 && base.Projectile.ai[0] > 60f)
				{
					distance = 1.6f / distance;
					shootToX *= distance * 3f;
					shootToY *= distance * 3f;
					
					base.Projectile.ai[0] = 0f;
				}
			}
			
				Vector2 idlePosition = player.Center;
				idlePosition.Y -= 48f; // Go up 48 coordinates (three tiles from the center of the player)

				// If your minion doesn't aimlessly move around when it's idle, you need to "put" it into the line of other summoned minions
				// The index is projectile.minionPos
				float minionPositionOffsetX = (Projectile.minionPos * 10) * player.direction;
				idlePosition.X += minionPositionOffsetX; // Go behind the player

				// All of this code below this line is adapted from Spazmamini code (ID 388, aiStyle 66)

				// Teleport to player if distance is too big
				Vector2 vectorToIdlePosition = idlePosition - Projectile.Center;
				float distanceToIdlePosition = vectorToIdlePosition.Length();
				if (Main.myPlayer == player.whoAmI && distanceToIdlePosition > 2000f)
				{
					// Whenever you deal with non-regular events that change the behavior or position drastically, make sure to only run the code on the owner of the projectile,
					// and then set netUpdate to true
					Projectile.position = idlePosition;
					Projectile.velocity *= 0.1f;
					Projectile.netUpdate = true;
				}

				// If your minion is flying, you want to do this independently of any conditions
				



			
			Vector2 idlePosition2 = player.Center;
			idlePosition2.Y -= 48f;
			idlePosition2.X += minionPositionOffsetX;
			// Default movement parameters (here for attacking)
			Projectile.rotation = Projectile.velocity.X * 0.05f;

			
				
				{// Minion doesn't have a target: return to player and idle
					if (distanceToIdlePosition > 450f)
					{
						// Speed up the minion if it's away from the player
						speed = 60f;
						inertia = 70f;
					}
				else if (distanceToIdlePosition > 400f)
				{
					// Speed up the minion if it's away from the player
					speed = 16f;
					inertia = 60f;
				}
				else
					{
						// Slow down the minion if closer to the player
						speed = 8f;
						inertia = 80f;
					}
					if (distanceToIdlePosition > 20f)
					{
						// The immediate range around the player (when it passively floats about)

						// This is a simple movement formula using the two parameters and its desired direction to create a "homing" movement
						vectorToIdlePosition.Normalize();
						vectorToIdlePosition *= speed;
						Projectile.velocity = (Projectile.velocity * (inertia - 1) + vectorToIdlePosition) / inertia;
					}
					else if (Projectile.velocity == Vector2.Zero)
					{
						// If there is a case where it's not moving at all, give it a little "poke"
						Projectile.velocity.X = -0.15f;
						Projectile.velocity.Y = -0.15f;
					}
				}
			
				if (Projectile.velocity.X >= 0)
				{
					Projectile.spriteDirection = 1;
				}
				if (Projectile.velocity.X <= -0)
				{
					Projectile.spriteDirection = -1;
				}
			
			Lighting.AddLight(Projectile.Center, Color.Silver.ToVector3() * 1f);
			int frameSpeed = 10; //reduced by half due to framecounter speedup
			Projectile.frameCounter += 2;
			if (Projectile.frameCounter >= frameSpeed)
			{
				Projectile.frameCounter = 0;

				
				
				{
					
					base.Projectile.frame++;
					if (base.Projectile.frameCounter >= 4)
					{
						
						base.Projectile.frameCounter = 0;

					}
					if (base.Projectile.frame >= 4)
					{
						base.Projectile.frame = 0;

					}
				}
			}
		}
	}
}
