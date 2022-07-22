using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;




using System;
using Terraria;
using SariaMod.Items.Sapphire;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.Amethyst
{
	public class Ghostsmoke : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			base.DisplayName.SetDefault("Child");
			Main.projFrames[base.projectile.type] = 4;
			ProjectileID.Sets.MinionShot[base.projectile.type] = true;
		}

		public override void SetDefaults()
		{
			base.projectile.width = 20;
			base.projectile.height = 20;
			base.projectile.netImportant = true;
			base.projectile.friendly = true;
			base.projectile.ignoreWater = true;
			base.projectile.usesLocalNPCImmunity = true;
			base.projectile.localNPCHitCooldown = 7;
			base.projectile.minionSlots = 0f;
			base.projectile.extraUpdates = 1;
			
			base.projectile.penetrate = -1;
			base.projectile.tileCollide = false;
			base.projectile.timeLeft = 500;
			base.projectile.minion = true;
		}
		public override bool? CanCutTiles()
		{
			return false;
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
			target.AddBuff(BuffID.ShadowFlame, 1000);
			target.AddBuff(BuffID.Slow, 300);
			damage = 1;
			knockback *= 0;
		}
		public override void AI()
		{
			
			Player player = Main.player[projectile.owner];
			float speed = 15;
			float smoke = -1;
			Projectile mother = Main.projectile[(int)base.projectile.ai[0]];
			Vector2 idlePosition = mother.Center;
			
			idlePosition.Y -= 88f;
			idlePosition.X -= 88f;
			Vector2 vectorToIdlePosition = idlePosition - projectile.Center;
			float distanceToIdlePosition = vectorToIdlePosition.Length();
			{
				// Minion has a target: attack (here, fly towards the enemy)

				float distanceFromTarget = 10f;
				Vector2 targetCenter = projectile.position;
				bool foundTarget = false;

				if (projectile.timeLeft >= 200)
				{
					if (projectile.velocity.Y >= 1)
					{
						projectile.velocity.Y *= -1;
					}
				}
				
				else if (projectile.timeLeft < 200 && foundTarget && distanceFromTarget > 1)
				{
					Vector2 direction = targetCenter - projectile.Center;
					projectile.velocity = (projectile.velocity * (12 - 2) + direction) / 20;
				}
			}









					// friendly needs to be set to true so the minion can deal contact damage
					// friendly needs to be set to false so it doesn't damage things like target dummies while idling
					// Both things depend on if it has a target or not, so it's just one assignment here
					// You don't need this assignment if your minion is shooting things instead of dealing contact damage


					Lighting.AddLight(projectile.Center, Color.LightBlue.ToVector3() * 0.78f);
			// Default movement parameters (here for attacking)
			
			
			
		
			
			

			
			int frameSpeed = 15;
			{
				base.projectile.frameCounter++;
				if (projectile.frameCounter >= frameSpeed)
					
          
					if (base.projectile.frameCounter > 4)
					{
						base.projectile.frame++;
						base.projectile.frameCounter = 0;
					}
				if (base.projectile.frame >= 4)
				{
					base.projectile.frame = 3;
				}
								
			}
		}
		public override Color? GetAlpha(Color lightColor)
		{
			if (base.projectile.timeLeft < 85)
			{
				byte b2 = (byte)(base.projectile.timeLeft * 3);
				byte a2 = (byte)(100f * ((float)(int)b2 / 255f));
				return new Color(b2, b2, b2, a2);
			}
			return new Color(255, 255, 255, 100);
		}
	}
}

