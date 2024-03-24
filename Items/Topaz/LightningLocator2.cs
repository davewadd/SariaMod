using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using System;

using Terraria;
using SariaMod.Buffs;

using SariaMod.Items.Strange;
using SariaMod.Items.Topaz;
using SariaMod.Dusts;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.Topaz
{
	public class LightningLocator2 : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			base.DisplayName.SetDefault("Blade");
			ProjectileID.Sets.TrailCacheLength[base.Projectile.type] = 7;
			ProjectileID.Sets.TrailingMode[base.Projectile.type] = 0;
			Main.projFrames[base.Projectile.type] = 4;
		}

		public override void SetDefaults()
		{
			base.Projectile.width = 30;
			base.Projectile.height = 30;
			
			base.Projectile.alpha = 0;
			base.Projectile.friendly = true;
			base.Projectile.tileCollide = false;
			
			base.Projectile.penetrate = 1;
			base.Projectile.timeLeft = 500;
			base.Projectile.ignoreWater = true;
			
			base.Projectile.usesLocalNPCImmunity = true;
			base.Projectile.localNPCHitCooldown = 4;
		}
		public override bool? CanCutTiles()
		{
			return false;
		}
		public override bool? CanHitNPC(NPC target)
        {
			return false;
        }
		public override void AI()
		{
			Player player = Main.player[base.Projectile.owner];
			Projectile mother = Main.projectile[(int)base.Projectile.ai[1]];
			if (Main.player[Main.myPlayer].active && !Main.player[Main.myPlayer].ZoneSnow)
			{
				if (Main.rand.NextBool(2))//controls the speed of when the sparkles spawn
				{
					float radius = (float)Math.Sqrt(Main.rand.Next(4 * 30));
					double angle = Main.rand.NextDouble() * 5.0 * Math.PI;


					Dust.NewDust(new Vector2(Projectile.Center.X + radius * (float)Math.Cos(angle), (Projectile.Center.Y + 10) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<Rain2>(), 0f, 0f, 0, default(Color), 1.5f);
				}//end of dust stuff
			}
			if (Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneSnow)
			{
				if (Main.rand.NextBool(20))//controls the speed of when the sparkles spawn
				{
					float radius = (float)Math.Sqrt(Main.rand.Next(4 * 30));
					double angle = Main.rand.NextDouble() * 5.0 * Math.PI;


					Dust.NewDust(new Vector2(Projectile.Center.X + radius * (float)Math.Cos(angle), (Projectile.Center.Y + 10) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<Snow2>(), 0f, 0f, 0, default(Color), 1.5f);
				}//end of dust stuff
			}
			if (Main.rand.NextBool(3))//controls the speed of when the sparkles spawn
			{
				Vector2 spot = Projectile.Center;
				spot.X = 0;
				spot.Y += 120;
				float radius = (float)Math.Sqrt(Main.rand.Next(34 * 34));
				double angle = Main.rand.NextDouble() * 5.0 * Math.PI;


				Dust.NewDust(new Vector2(Projectile.Center.X + radius * (float)Math.Cos(angle), (Projectile.Center.Y + 320) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<StaticDust>(), 0f, 0f, 0, default(Color), 1.5f);
			}//end of dust stuff
			

			{
					Projectile.timeLeft = 2000;
                }
			// friendly needs to be set to true so the minion can deal contact damage
			// friendly needs to be set to false so it doesn't damage things like target dummies while idling
			// Both things depend on if it has a target or not, so it's just one assignment here
			// You don't need this assignment if your minion is shooting things instead of dealing contact damage
			if (!player.HasBuff(ModContent.BuffType<SariaBuff>()))
			{
				Projectile.Kill();
			}


			if ((player.ownedProjectileCounts[ModContent.ProjectileType<LightningStrike3>()] <= 0f))
			{
				Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 20, Projectile.position.Y + 215, 0, 0, ModContent.ProjectileType<LightningStrike3>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
			}



			Lighting.AddLight(Projectile.Center, Color.LightGoldenrodYellow.ToVector3() * 1f);
			// Default movement parameters (here for attacking)
			
			// Go up 48 coordinates (three tiles from the center of the player)

			// If your minion doesn't aimlessly move around when it's idle, you need to "put" it into the line of other summoned minions
			// The index is projectile.minionPos

			// Go behind the player

			// All of this code below this line is adapted from Spazmamini code (ID 388, aiStyle 66)






			
			

			
			int frameSpeed = 30; //reduced by half due to framecounter speedup
				Projectile.frameCounter += 2;
				if (Projectile.frameCounter >= frameSpeed)
				{
					Projectile.frameCounter = 0;

					{ 
						base.Projectile.frame++;
						if (base.Projectile.frameCounter >= 4)
						{
							base.Projectile.frame++;
							base.Projectile.frameCounter = 0;

						}
						if (base.Projectile.frame >= Main.projFrames[base.Projectile.type])
						{
							base.Projectile.frame = 0;

						}
					}
				}
			
			
		}

	

		

		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			Player player = Main.player[base.Projectile.owner];
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
			target.AddBuff(BuffID.Electrified, 300);

			damage /= damage/4;
			
		}



	}
}
