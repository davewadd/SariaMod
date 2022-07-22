using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;





using System;
using Terraria;
using SariaMod.Buffs;
using SariaMod.Items.Sapphire;
using SariaMod.Dusts;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.Ruby
{
	public class Flame : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			base.DisplayName.SetDefault("Child");
			Main.projFrames[base.projectile.type] = 6;
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
			base.projectile.aiStyle = 1;
			base.projectile.penetrate = -1;
			projectile.tileCollide = true;
			base.projectile.timeLeft = 1500;
			
		}
		private const int sphereRadius = 30;
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
			target.AddBuff(BuffID.OnFire, 300);
			target.AddBuff(BuffID.Slow, 300);
			if (projectile.timeLeft >= 1400)
            {
				damage /= 2;
            }
			if (projectile.timeLeft < 1400)
			{
				damage /= 100;
				knockback /= 1000;
			}
			if (projectile.timeLeft < 1000)
            {
				knockback = 0;
            }
		}
		public override bool OnTileCollide(Vector2 oldVelocity)
        {
			projectile.velocity.Y = 1/4;
			projectile.velocity.X = 0;
			return false;

        }

        public override bool MinionContactDamage()
		{
		
			
				return true;
			
		}
		
		public override void AI()
		{
			Player player = Main.player[projectile.owner];

			if (Main.rand.NextBool(30))//controls the speed of when the sparkles spawn
			{
				float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius * sphereRadius));
				double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
				Dust.NewDust(new Vector2(projectile.Center.X + radius * (float)Math.Cos(angle), (projectile.Center.Y - 10) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<FlameDust>(), 0f, 0f, 0, default(Color), 1.5f);
			}

			// friendly needs to be set to true so the minion can deal contact damage
			// friendly needs to be set to false so it doesn't damage things like target dummies while idling
			// Both things depend on if it has a target or not, so it's just one assignment here
			// You don't need this assignment if your minion is shooting things instead of dealing contact damage


			Lighting.AddLight(projectile.Center, Color.OrangeRed.ToVector3() * 2f);
			// Default movement parameters (here for attacking)

			if (player.ownedProjectileCounts[ModContent.ProjectileType<RubyPsychicSeeker>()] > 0f && projectile.timeLeft > 50)

			{
				projectile.timeLeft = 50;
			}






			int frameSpeed = 15;
			{
				base.projectile.frameCounter++;
				if (projectile.frameCounter >= frameSpeed)
					
          
					if (base.projectile.frameCounter > 6)
					{
						base.projectile.frame++;
						base.projectile.frameCounter = 0;
					}
				if (base.projectile.frame >= 6)
				{
					
					base.projectile.frame = 0;
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

