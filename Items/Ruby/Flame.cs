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
			Main.projFrames[base.Projectile.type] = 6;
			ProjectileID.Sets.MinionShot[base.Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			base.Projectile.width = 20;
			base.Projectile.height = 20;
			base.Projectile.netImportant = true;
			base.Projectile.friendly = true;
			base.Projectile.ignoreWater = true;
			base.Projectile.usesLocalNPCImmunity = true;
			base.Projectile.localNPCHitCooldown = 7;
			base.Projectile.minionSlots = 0f;
			base.Projectile.extraUpdates = 1;
			base.Projectile.aiStyle = 1;
			base.Projectile.penetrate = -1;
			Projectile.tileCollide = true;
			base.Projectile.timeLeft = 1500;
			
		}
		private const int sphereRadius = 20;
		public override bool? CanCutTiles()
		{
			return false;
		}
		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			Player player = Main.player[Projectile.owner];
			FairyPlayer modPlayer = player.Fairy();
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
			target.buffImmune[ModContent.BuffType<Burning2>()] = false;
			target.AddBuff(ModContent.BuffType<Burning2>(), 200);
			if (player.HasBuff(ModContent.BuffType<Overcharged>()))
			{
				Projectile.timeLeft += 6;
			}
	
            
				damage /= 15;
				knockback *= 0;
            
		}
		public override bool OnTileCollide(Vector2 oldVelocity)
        {
			Projectile.velocity.Y = 1/4;
			Projectile.velocity.X = 0;
			return false;

        }

        public override bool MinionContactDamage()
		{
		
			
				return true;
			
		}
		
		public override void AI()
		{
			Player player = Main.player[Projectile.owner];
			Player player2 = Main.LocalPlayer;
			FairyPlayer modPlayer = player.Fairy();
			if (Main.rand.NextBool(30))//controls the speed of when the sparkles spawn
			{
				float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius * sphereRadius));
				double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
				Dust.NewDust(new Vector2(Projectile.Center.X + radius * (float)Math.Cos(angle), (Projectile.Center.Y - 10) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<FlameDust>(), 0f, 0f, 0, default(Color), 1.5f);
			}
			if (Main.rand.NextBool(40))//controls the speed of when the sparkles spawn
			{
				float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius * sphereRadius));
				double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
				Dust.NewDust(new Vector2(Projectile.Center.X + radius * (float)Math.Cos(angle), (Projectile.Center.Y - 15) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<SmokeDust3>(), 0f, 0f, 0, default(Color), 1.5f);
			}

			// friendly needs to be set to true so the minion can deal contact damage
			// friendly needs to be set to false so it doesn't damage things like target dummies while idling
			// Both things depend on if it has a target or not, so it's just one assignment here
			// You don't need this assignment if your minion is shooting things instead of dealing contact damage


			Lighting.AddLight(Projectile.Center, Color.OrangeRed.ToVector3() * 2f);
			// Default movement parameters (here for attacking)

			if (player.ownedProjectileCounts[ModContent.ProjectileType<Flame>()] > 12f && !player.HasBuff(ModContent.BuffType<Overcharged>()))

			{
				Projectile.timeLeft -= 3;
			}

			
			

			// This code is required if your minion weapon has the targeting feature
			{ 
				float between = Vector2.Distance(player2.Center, Projectile.Center);
				// Reasonable distance away so it doesn't target across multiple screens
				if (between <600f)
				{
					player2.AddBuff(BuffID.Campfire, 30);

				}
			}




			int frameSpeed = 15;
			{
				base.Projectile.frameCounter++;
				if (Projectile.frameCounter >= frameSpeed)
					
          
					if (base.Projectile.frameCounter > 6)
					{
						base.Projectile.frame++;
						base.Projectile.frameCounter = 0;
					}
				if (base.Projectile.frame >= 6)
				{
					
					base.Projectile.frame = 0;
				}
								
			}
		}
		public override Color? GetAlpha(Color lightColor)
		{
			if (base.Projectile.timeLeft < 85)
			{
				byte b2 = (byte)(base.Projectile.timeLeft * 3);
				byte a2 = (byte)(100f * ((float)(int)b2 / 255f));
				return new Color(b2, b2, b2, a2);
			}
			return new Color(255, 255, 255, 100);
		}
	}
}

