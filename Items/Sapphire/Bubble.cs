using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SariaMod.Items.Strange;
using SariaMod.Dusts;
using System;
using Terraria;
using SariaMod.Items.Sapphire;
using Terraria.Audio;
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
			Main.projFrames[base.Projectile.type] = 1;
			ProjectileID.Sets.MinionShot[base.Projectile.type] = true;
			ProjectileID.Sets.TrailingMode[base.Projectile.type] = 2;
			
		}

		public override void SetDefaults()
		{
			base.Projectile.width = 34;
			base.Projectile.height = 34;
			base.Projectile.netImportant = true;
			base.Projectile.friendly = false;
			base.Projectile.ignoreWater = true;
			base.Projectile.usesLocalNPCImmunity = true;
			base.Projectile.localNPCHitCooldown = 7;
			base.Projectile.minionSlots = 0f;
			base.Projectile.extraUpdates = 1;
			Projectile.alpha = 100;
			base.Projectile.penetrate = -1;
			base.Projectile.tileCollide = false;
			base.Projectile.timeLeft = 500;
			Projectile.DamageType = DamageClass.Magic;
		}
		public override bool? CanCutTiles()
		{
			return false;
		}
		public override bool MinionContactDamage()
		{
			Player player = Main.player[base.Projectile.owner];
			FairyPlayer modPlayer = player.Fairy();
			NPC target = base.Projectile.Center.MinionHoming(500f, player);
			if (Projectile.frame <= 2)
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
			Player player = Main.player[Projectile.owner];
			Player player2 = Main.LocalPlayer;
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
			target.buffImmune[ModContent.BuffType<Frostburn2>()] = false;
			target.AddBuff(ModContent.BuffType<Frostburn2>(), 200);
			FairyPlayer modPlayer = player.Fairy();
			modPlayer.SariaXp++;
			if (Main.rand.NextBool())//controls the speed of when the sparkles spawn
			{
				float radius = (float)Math.Sqrt(Main.rand.Next(34 * 34));
				double angle = Main.rand.NextDouble() * 5.0 * Math.PI;


				Dust.NewDust(new Vector2(Projectile.Center.X + radius * (float)Math.Cos(angle), Projectile.Center.Y + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<Water>(), 0f, 0f, 0, default(Color), 1.5f);
			}//end of dust stuff
			
			Projectile.scale = 1.5f;
			knockback /= 100;
			////ahhhhhh
			SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Bubblepop"));
			if (Main.rand.NextBool(10))//controls the speed of when the sparkles spawn
				{
					float radius = (float)Math.Sqrt(Main.rand.Next(34 * 34));
					double angle = Main.rand.NextDouble() * 5.0 * Math.PI;


					
				}
			

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
				damage -= (damage) / 2;
			}
			
			
		}
		public override void AI()
		{
			Player player = Main.player[Projectile.owner];
			FairyPlayer modPlayer = player.Fairy();
			Player player2 = Main.LocalPlayer;
			Projectile mother = Main.projectile[(int)base.Projectile.ai[1]];
			if (modPlayer.Sarialevel == 6)
			{
				Projectile.damage = 900 + (modPlayer.SariaXp / 40);
			}
			else if (modPlayer.Sarialevel == 5)
			{
				Projectile.damage = 200 + (modPlayer.SariaXp / 342);
			}
			else if (modPlayer.Sarialevel == 4)
			{
				Projectile.damage = 75 + (modPlayer.SariaXp / 640);
			}
			else if (modPlayer.Sarialevel == 3)
			{
				Projectile.damage = 50 + (modPlayer.SariaXp / 1600);
			}
			else if (modPlayer.Sarialevel == 2)
			{
				Projectile.damage = 26 + (modPlayer.SariaXp / 833);
			}

			else if (modPlayer.Sarialevel == 1)
			{
				Projectile.damage = 15 + (modPlayer.SariaXp / 818);
			}
			else
			{
				Projectile.damage = 10 + (modPlayer.SariaXp / 600);
			}
			if (Main.rand.NextBool(20))//controls the speed of when the sparkles spawn
			{
				float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius2 * sphereRadius2));
				double angle = Main.rand.NextDouble() * 5.0 * Math.PI;

				Dust.NewDust(new Vector2((Projectile.Center.X) + radius * (float)Math.Cos(angle), (Projectile.Center.Y) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<Cold>(), 0f, 0f, 0, default(Color), 1.5f);
			}
			if (Main.rand.NextBool(20))//controls the speed of when the sparkles spawn
			{
				float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius2 * sphereRadius2));
				double angle = Main.rand.NextDouble() * 5.0 * Math.PI;

				Dust.NewDust(new Vector2((Projectile.Center.X) + radius * (float)Math.Cos(angle), (Projectile.Center.Y) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<Snow2>(), 0f, 0f, 0, default(Color), 1.5f);
			}
			
			{
				float between = Vector2.Distance(player2.Center, Projectile.Center);
				// Reasonable distance away so it doesn't target across multiple screens
				if (between < 1000f)
				{
					player2.AddBuff(BuffID.Regeneration, 30);

				}
			}
			

			

			// If your minion is flying, you want to do this independently of any conditions
		

			if (Projectile.timeLeft <= 10)
			{
				Projectile.scale = 1.5f;
				if (Main.rand.NextBool())//controls the speed of when the sparkles spawn
				{
					float radius = (float)Math.Sqrt(Main.rand.Next(34 * 34));
					double angle = Main.rand.NextDouble() * 5.0 * Math.PI;


					Dust.NewDust(new Vector2(Projectile.Center.X + radius * (float)Math.Cos(angle), Projectile.Center.Y + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<Water>(), 0f, 0f, 0, default(Color), 1.5f);
				}//end of dust stuff
				if (Main.rand.NextBool())//controls the speed of when the sparkles spawn
				{
					float radius = (float)Math.Sqrt(Main.rand.Next(34 * 34));
					double angle = Main.rand.NextDouble() * 5.0 * Math.PI;


					Dust.NewDust(new Vector2(Projectile.Center.X + radius * (float)Math.Cos(angle), Projectile.Center.Y + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<BubbleDust>(), 0f, 0f, 0, default(Color), 1.5f);
				}//end of dust stuff
				
				SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Bubblepop") with { Volume = 2f, Pitch = 1.3f });
				

			}
			
			bool foundTarget = true;
			
			if (Projectile.timeLeft < 20)
			{
				SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Bubblepop"));
			}
			// friendly needs to be set to true so the minion can deal contact damage
			// friendly needs to be set to false so it doesn't damage things like target dummies while idling
			// Both things depend on if it has a target or not, so it's just one assignment here
			// You don't need this assignment if your minion is shooting things instead of dealing contact damage
			Projectile.friendly = foundTarget;



			Lighting.AddLight(Projectile.Center, new Color(0, 0, Main.DiscoB).ToVector3() * 2f);
			// Default movement parameters (here for attacking)
			float speed = 13f;
			float inertia = 12f;
			if (Projectile.timeLeft == 3000)
			{
				SoundEngine.PlaySound(SoundID.Drown, base.Projectile.Center);
			}
			Vector2 idlePosition = Main.MouseWorld;

			

			
		
			

		}




	}
	}


