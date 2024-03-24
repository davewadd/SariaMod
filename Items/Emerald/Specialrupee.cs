using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;

using Terraria;
using Terraria.Audio;
using SariaMod.Items.Strange;
using SariaMod.Buffs;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.Emerald
{
	public class Specialrupee : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			base.DisplayName.SetDefault("Blade");
			ProjectileID.Sets.TrailingMode[base.Projectile.type] = 2;
			ProjectileID.Sets.TrailCacheLength[base.Projectile.type] = 30;
			Main.projFrames[base.Projectile.type] = 1;
		}

		public override void SetDefaults()
		{
			base.Projectile.width = 30;
			base.Projectile.height = 30;
			
			base.Projectile.alpha = 0;
			base.Projectile.friendly = true;
			base.Projectile.tileCollide = false;
			
			base.Projectile.penetrate = 1;
			base.Projectile.timeLeft = 600;
			base.Projectile.ignoreWater = true;
			
			base.Projectile.usesLocalNPCImmunity = true;
			base.Projectile.localNPCHitCooldown = 4;
		}

		public override bool? CanHitNPC(NPC target)
        {
			return false;
        }
		public override void AI()
		{
			Player player = Main.player[base.Projectile.owner];
			int owner = player.whoAmI;
			int MotherSaria = ModContent.ProjectileType<Saria>();
			for (int i = 0; i < 1000; i++)
			{
				float inertia = 13f;
				Vector2 idlePosition = Main.projectile[i].Center;
				idlePosition.X += (30 * Main.projectile[i].direction) + 20;
				Vector2 vectorToIdlePosition = idlePosition - Projectile.Center;
				float distanceToIdlePosition = vectorToIdlePosition.Length();
				Vector2 direction = idlePosition - Projectile.Center;

				if (Main.projectile[i].active && i != base.Projectile.whoAmI && ((Main.projectile[i].type == MotherSaria && Main.projectile[i].owner == owner)))
				{

					{
						Projectile.velocity = (((Projectile.velocity * (13 - 2) + direction) / 20));
						Projectile.netUpdate = true;
					}

				}

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
						targetCenter.Y += 0f;
						targetCenter.X += 0f;
						foundTarget = true;
					}
				}


				
				base.Projectile.rotation += 0.075f;
				// friendly needs to be set to true so the minion can deal contact damage
				// friendly needs to be set to false so it doesn't damage things like target dummies while idling
				// Both things depend on if it has a target or not, so it's just one assignment here
				// You don't need this assignment if your minion is shooting things instead of dealing contact damage
				Projectile.friendly = foundTarget;

				if (player.ownedProjectileCounts[ModContent.ProjectileType<RupeeAttack>()] > 0f && player.ownedProjectileCounts[ModContent.ProjectileType<SpecialrupeeShield>()] > 0f)
				{
					
					Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + 0, 0, 0, ModContent.ProjectileType<Specialrupee3>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
					Projectile.Kill();
				}
				if (player.ownedProjectileCounts[ModContent.ProjectileType<RupeeAttack>()] > 0f && player.ownedProjectileCounts[ModContent.ProjectileType<SpecialrupeeShield>()] <= 0f)
				{
					SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Nayru2"), Projectile.Center);
					SoundEngine.PlaySound(SoundID.DD2_EtherianPortalOpen, base.Projectile.Center);
					Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + 0, 0, 0, ModContent.ProjectileType<SpecialrupeeShield>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
					Projectile.Kill();
				}
				if (Projectile.timeLeft <= 10 && Projectile.timeLeft > 2)
				{
					Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + 0, 0, 0, ModContent.ProjectileType<Silverrupee>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
					SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/SilverRupee3"), Projectile.Center);
					Projectile.Kill();
				}
				Lighting.AddLight(Projectile.Center, Color.Magenta.ToVector3() * 1f);
				// Default movement parameters (here for attacking)

				
			}
			
		}


		






	}
}
