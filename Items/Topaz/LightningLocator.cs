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
	public class LightningLocator : ModProjectile
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
			Projectile.netImportant = true;
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




			
				
				Lighting.AddLight(Projectile.Center, Color.LightGoldenrodYellow.ToVector3() * 1f);
			// Default movement parameters (here for attacking)
			Vector2 mouse2 = Main.MouseWorld;
			Vector2 mouse = Main.MouseWorld;
			
			mouse.Y -= 330f;
			// Go up 48 coordinates (three tiles from the center of the player)

			// If your minion doesn't aimlessly move around when it's idle, you need to "put" it into the line of other summoned minions
			// The index is projectile.minionPos

			// Go behind the player

			// All of this code below this line is adapted from Spazmamini code (ID 388, aiStyle 66)
			Vector2 vectorToIdlePosition2 = mouse2 - Projectile.Center;
			float distanceToIdlePosition2 = vectorToIdlePosition2.Length();
			// Teleport to player if distance is too big
			Vector2 vectorToIdlePosition = mouse - Projectile.Center;
			float distanceToIdlePosition = vectorToIdlePosition.Length();
			if (Main.myPlayer == player.whoAmI && distanceToIdlePosition > 200000f)
			{
				// Whenever you deal with non-regular events that change the behavior or position drastically, make sure to only run the code on the owner of the projectile,
				// and then set netUpdate to true
				Projectile.position = mouse;
				Projectile.velocity *= 0.1f;
				Projectile.netUpdate = true;
			}
			
			float shocking = 50f;
			float speed = 70f;
			float inertia = 20f;
			Vector2 direction2 = mouse - Projectile.Center;
			direction2.Normalize();
			direction2 *= speed;
			if (Main.myPlayer == player.whoAmI)
			{
				Projectile.Center = mouse;
			}
			NPC target = base.Projectile.Center.MinionHoming(500f, player);
			{
				for (int b = 0; b < Main.maxNPCs; b++)
				{
					
					NPC npc = Main.npc[b];
					float between2 = Vector2.Distance(npc.Center, mouse2);
					// Reasonable distance away so it doesn't target across multiple screens
					if (between2 < 1200f && npc.friendly != true && target != null && Main.npc[b].active == true)
					{
						shocking = 250 * player.ownedProjectileCounts[ModContent.ProjectileType<Static2>()] + 50;
						
						if (between2 < shocking)
						{


							if ((player.ownedProjectileCounts[ModContent.ProjectileType<Static>()] >= 1f) && (player.ownedProjectileCounts[ModContent.ProjectileType<LightningStrike2>()] <= 0f) && (!player.HasBuff(ModContent.BuffType<StatRaise>())) && (!player.HasBuff(ModContent.BuffType<Overcharged>())) && (!player.HasBuff(ModContent.BuffType<StatLower>())) && !Main.player[Main.myPlayer].ZoneSnow)
							{
								Projectile.NewProjectile(Projectile.GetSource_FromThis(), Main.npc[b].position.X + 20, Main.npc[b].position.Y - 125, 0, 0, ModContent.ProjectileType<LightningStrike2>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
							}
							if ((player.ownedProjectileCounts[ModContent.ProjectileType<Static>()] >= 1f) && (player.ownedProjectileCounts[ModContent.ProjectileType<LightningStrikeRed>()] <= 0f) && (!player.HasBuff(ModContent.BuffType<StatRaise>())) && (!player.HasBuff(ModContent.BuffType<Overcharged>())) && (player.HasBuff(ModContent.BuffType<StatLower>())) && !Main.player[Main.myPlayer].ZoneSnow)
							{
								Projectile.NewProjectile(Projectile.GetSource_FromThis(), Main.npc[b].position.X + 20, Main.npc[b].position.Y - 125, 0, 0, ModContent.ProjectileType<LightningStrikeRed>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
							}
							if ((player.ownedProjectileCounts[ModContent.ProjectileType<Static>()] >= 1f) && (player.ownedProjectileCounts[ModContent.ProjectileType<LightningStrikeBlue>()] <= 0f) && (player.HasBuff(ModContent.BuffType<StatRaise>())) && (!player.HasBuff(ModContent.BuffType<Overcharged>())) && (!player.HasBuff(ModContent.BuffType<StatLower>())) && !Main.player[Main.myPlayer].ZoneSnow)
							{
								Projectile.NewProjectile(Projectile.GetSource_FromThis(), Main.npc[b].position.X + 20, Main.npc[b].position.Y - 125, 0, 0, ModContent.ProjectileType<LightningStrikeBlue>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
							}
							if ((player.ownedProjectileCounts[ModContent.ProjectileType<Static>()] >= 1f) && (player.ownedProjectileCounts[ModContent.ProjectileType<LightningStrikePurple>()] <= 0f) && (player.HasBuff(ModContent.BuffType<Overcharged>())) && !Main.player[Main.myPlayer].ZoneSnow)
							{
								Projectile.NewProjectile(Projectile.GetSource_FromThis(), Main.npc[b].position.X + 20, Main.npc[b].position.Y - 125, 0, 0, ModContent.ProjectileType<LightningStrikePurple>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
							}
							if ((player.ownedProjectileCounts[ModContent.ProjectileType<Static>()] >= 1f) && (player.ownedProjectileCounts[ModContent.ProjectileType<LightningStrikePink>()] <= 0f) && Main.player[Main.myPlayer].ZoneSnow)
							{
								Projectile.NewProjectile(Projectile.GetSource_FromThis(), Main.npc[b].position.X + 20, Main.npc[b].position.Y - 125, 0, 0, ModContent.ProjectileType<LightningStrikePink>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
							}
							if (Main.rand.NextBool(10))//controls the speed of when the sparkles spawn
								{
									
									
									float radius = (float)Math.Sqrt(Main.rand.Next(34 * 34));
									double angle = Main.rand.NextDouble() * 5.0 * Math.PI;


									Dust.NewDust(new Vector2(Main.npc[b].Center.X + radius * (float)Math.Cos(angle), (Main.npc[b].Center.Y) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<StaticDust>(), 0f, 0f, 0, default(Color), 1.5f);
									
								}
								Projectile.netUpdate = true;
										}
								
							
						

					}
				}
			}
			
			float distanceFromTarget = 250 * player.ownedProjectileCounts[ModContent.ProjectileType<Static2>()] + 50;
			
			Vector2 targetCenter = Projectile.position;
			bool foundTarget = false;
			if (!foundTarget)
			{
				// This code is required either way, used for finding a target
				for (int i = 0; i < Main.maxNPCs; i++)
				{
					NPC npc = Main.npc[i];
					if (npc.CanBeChasedBy())
					{
						
						float between = Vector2.Distance(npc.Center, Projectile.Center);
						bool closest = Vector2.Distance(Projectile.Center, targetCenter) > between;
						bool inRange = between < distanceFromTarget;

						// Additional check for this specific minion behavior, otherwise it will stop attacking once it dashed through an enemy while flying though tiles afterwards
						// The number depends on various parameters seen in the movement code below. Test different ones out until it works alright
						bool closeThroughWall = between < 400f;
						if (((closest && inRange) || !foundTarget) && (closeThroughWall))
						{
							if (Main.rand.NextBool(30))//controls the speed of when the sparkles spawn
							{
								SoundEngine.PlaySound(SoundID.NPCHit34, base.Projectile.Center);
							}
							targetCenter = npc.Center;
							targetCenter.Y = 310f;
							targetCenter.X += 0f;
							foundTarget = true;
						}
					}
				}
			}
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
