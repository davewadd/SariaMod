using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;

using SariaMod.Dusts;
using Terraria;
using Terraria.Audio;



using Terraria.ID;
using SariaMod.Buffs;
using Terraria.ModLoader;

namespace SariaMod.Items.Ruby
{
	public class RubyPsychicSeeker : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			base.DisplayName.SetDefault("Blade");
			ProjectileID.Sets.TrailCacheLength[base.Projectile.type] = 8;
			ProjectileID.Sets.TrailingMode[base.Projectile.type] = 0;
			Main.projFrames[base.Projectile.type] = 8;
		}

		public override void SetDefaults()
		{
			base.Projectile.width = 30;
			base.Projectile.height = 30;
			
			base.Projectile.alpha = 100;
			base.Projectile.friendly = true;
			base.Projectile.tileCollide = false;
				Projectile.penetrate = 1;
				base.Projectile.timeLeft = 200;
			base.Projectile.ignoreWater = true;
			
			base.Projectile.usesLocalNPCImmunity = true;
			base.Projectile.localNPCHitCooldown = 4;
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
		private const int sphereRadius = 2;
		public override void AI()
		{
			Player player = Main.player[base.Projectile.owner];
			Lighting.AddLight(base.Projectile.Center, 0f, 0.5f, 0f);
			if (Main.rand.NextBool())//controls the speed of when the sparkles spawn
			{
				float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius * sphereRadius));
				double angle = Main.rand.NextDouble() * 2.0 * Math.PI;
				Dust.NewDust(new Vector2(Projectile.Center.X + radius * (float)Math.Cos(angle), Projectile.Center.Y + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<FlameDust>(), 0f, 0f, 0, default(Color), 1.5f);
				Dust.NewDust(new Vector2(Projectile.Center.X + radius * (float)Math.Cos(angle), Projectile.Center.Y + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<SmokeDust>(), 0f, 0f, 0, default(Color), 1.5f);
			}
			
			FairyGlobalProjectile.HomeInOnNPC(base.Projectile, ignoreTiles: true, 600f, 25f, 20f);
			{
				float distanceFromTarget = 10f;
				Vector2 targetCenter = Projectile.position;
				bool foundTarget = false;

				// This code is required if your minion weapon has the targeting feature
				if (player.HasMinionAttackTargetNPC)
				{
					NPC npc = Main.npc[player.MinionAttackTargetNPC];
					float between = Vector2.Distance(npc.Center, Projectile.Center);
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
							bool closest = Vector2.Distance(Projectile.Center, targetCenter) > between;
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
				


				Lighting.AddLight(Projectile.Center, Color.LightPink.ToVector3() * 0.78f);
				// Default movement parameters (here for attacking)
				float speed = 20f;
				float nah = 20;
				float inertia = 20f;
				if (distanceFromTarget > 40f && Projectile.timeLeft <= 400)
				{
					if (player.HasBuff(ModContent.BuffType<StatRaise>()))
					{
						speed = 25f;
					}
					if (player.HasBuff(ModContent.BuffType<StatLower>()))
					{
						speed /= 3;

					}
					// The immediate range around the target (so it doesn't latch onto it when close)
					Vector2 direction = targetCenter - Projectile.Center;
					direction.Normalize();
					direction *= speed;

					Projectile.velocity = (Projectile.velocity * (inertia - 2) + direction) / inertia;
				}
				base.Projectile.frameCounter++;
				if (base.Projectile.frameCounter > 6)
				{
					base.Projectile.frame++;
					base.Projectile.frameCounter = 0;
				}
				if (base.Projectile.frame >= Main.projFrames[base.Projectile.type])
				{
					base.Projectile.frame = 0;
				}
			}
			if (Projectile.timeLeft >= 200)
            {
				SoundEngine.PlaySound(SoundID.Item69, base.Projectile.Center);
			}
			if (Projectile.timeLeft == 1)
            {
				
					for (int j = 0; j < 1; j++) //set to 2
					{
					Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + 0, 0, 0, ModContent.ProjectileType<Explosion>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
					
					SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Bomb"));
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

			for (int j = 0; j < 1; j++) //set to 2
			{
				Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + 0, 0, 0, ModContent.ProjectileType<Explosion>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
				SoundEngine.PlaySound(SoundID.Item116, base.Projectile.Center);
				SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Bomb"), Projectile.Center);
			}
			if (player.HasBuff(ModContent.BuffType<StatRaise>()))
			{
				damage += (damage) / 4;
			}
			if (player.HasBuff(ModContent.BuffType<StatLower>()))
			{
				damage /= 2;

			}
		}



	}
}
