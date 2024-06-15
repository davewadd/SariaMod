using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using SariaMod.Buffs;
using SariaMod.Items.Sapphire;
using SariaMod.Items.Strange;
using SariaMod.Dusts;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using System.IO;

namespace SariaMod.Items.Topaz
{
	public class Bolt : ModProjectile
	{
		public override void SetStaticDefaults()
		{
		    
						base.DisplayName.SetDefault("Saria");
			Main.projFrames[base.Projectile.type] = 4;
			ProjectileID.Sets.MinionShot[base.Projectile.type] = true;
			ProjectileID.Sets.TrailingMode[base.Projectile.type] = 2;
			ProjectileID.Sets.TrailCacheLength[base.Projectile.type] = 30;
			
		}

		public override void SetDefaults()
		{
			base.Projectile.width = 32;
			base.Projectile.height = 16;
			base.Projectile.netImportant = true;
			base.Projectile.friendly = true;
			base.Projectile.ignoreWater = true;
			base.Projectile.usesLocalNPCImmunity = true;
			base.Projectile.localNPCHitCooldown = 100;
			base.Projectile.minionSlots = 0f;
			base.Projectile.extraUpdates = 1;
			
			base.Projectile.penetrate = -1;
			base.Projectile.tileCollide = false;
			base.Projectile.timeLeft = 800;
			base.Projectile.minion = true;
		}
		
		public override bool? CanCutTiles()
		{
			return false;
		}
		public override void SendExtraAI(BinaryWriter writer)
		{
			
		}
		public override void ReceiveExtraAI(BinaryReader reader)
		{
			
		}
		public override bool MinionContactDamage()
		{
			return true;
		}
		public override bool? CanHitNPC(NPC target)
		{
			return base.Projectile.timeLeft < 100 && target.CanBeChasedBy(base.Projectile);
		}
		private const int sphereRadius = 3;
		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			Player player = Main.player[Projectile.owner];
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
			target.AddBuff(BuffID.Slow, 300);
			target.AddBuff(ModContent.BuffType<SariaCurse2>(), 50);
			
			
			knockback /= 4;
		}
		public override void AI()
		{
			Player player = Main.player[Projectile.owner];
			FairyPlayer modPlayer = player.Fairy();
			Projectile mother = Main.projectile[(int)base.Projectile.ai[1]];
			Projectile.knockBack = 10;
			
			///Start of Damage
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
			///end of damage

			Projectile.rotation = Projectile.velocity.ToRotation();

			if (Projectile.timeLeft < 100)
            {
				Projectile.aiStyle = 1;
            }
			float speed = 70f;
			float inertia = 20f;
			
				Projectile.netUpdate = true;
				Vector2 mouse = Main.MouseWorld;
			
			mouse.X += 10f;
				mouse.Y -= 5f;
				Vector2 idlePosition = player.Center;
				idlePosition.Y -= 48f; // Go up 48 coordinates (three tiles from the center of the player)

				// If your minion doesn't aimlessly move around when it's idle, you need to "put" it into the line of other summoned minions
				// The index is projectile.minionPos
				float minionPositionOffsetX = (10 + Projectile.minionPos * 40) * -player.direction;
				idlePosition.X += minionPositionOffsetX; // Go behind the player


			if (Projectile.timeLeft > 400 && Main.myPlayer == Projectile.owner)
				{

					Vector2 direction2 = mouse - Projectile.Center;
					direction2.Normalize();
					direction2 *= speed;

					Projectile.velocity = (Projectile.velocity * (19 - 2) + direction2) / 22;

				}
			
				// All of this code below this line is adapted from Spazmamini code (ID 388, aiStyle 66)

				// Teleport to player if distance is too big
				Vector2 vectorToIdlePosition = idlePosition - Projectile.Center;
				float distanceToIdlePosition = vectorToIdlePosition.Length();
				if (Main.myPlayer == player.whoAmI && distanceToIdlePosition > 200000f)
				{
					// Whenever you deal with non-regular events that change the behavior or position drastically, make sure to only run the code on the owner of the projectile,
					// and then set netUpdate to true
					Projectile.position = idlePosition;
					Projectile.velocity *= 0.1f;
					Projectile.netUpdate = true;
				}

			// If your minion is flying, you want to do this independently of any conditions


			base.Projectile.frameCounter++;
			if (base.Projectile.frameCounter >= 2)
			{
				base.Projectile.frame++;
				base.Projectile.frameCounter = 0;

			}
			if (base.Projectile.frame >= Main.projFrames[base.Projectile.type])
			{
				base.Projectile.frame = 0;

			}

			// Starting search distance
			float distanceFromTarget = 10f;
				Vector2 targetCenter = Projectile.position;
				bool foundTarget = false;

				// This code is required if your minion weapon has the targeting feature
				


				// friendly needs to be set to true so the minion can deal contact damage
				// friendly needs to be set to false so it doesn't damage things like target dummies while idling
				// Both things depend on if it has a target or not, so it's just one assignment here
				// You don't need this assignment if your minion is shooting things instead of dealing contact damage





				// Default movement parameters (here for attacking)
				
				if (Projectile.timeLeft == 500)
				{
					SoundEngine.PlaySound(SoundID.DD2_SkyDragonsFuryShot, base.Projectile.Center);
				}

				// Minion has a target: attack (here, fly towards the enemy)
				if (distanceFromTarget > 40f && Projectile.timeLeft <= 400)
				{
					// The immediate range around the target (so it doesn't latch onto it when close)
					Vector2 direction = targetCenter - Projectile.Center;
					direction.Normalize();
					direction *= speed;

					Projectile.velocity = (Projectile.velocity * (inertia - 2) + direction) / inertia;
				}

				
			
		}
		public override bool PreDraw(ref Color lightColor)
		{
			{
				
				Texture2D starTexture = TextureAssets.Projectile[ModContent.ProjectileType<LocatorBeam>()].Value;
				Vector2 drawPosition;
				
				for (int i = 1; i < base.Projectile.oldPos.Length; i++)
				{
					float completionRatio = (float)i / (float)base.Projectile.oldPos.Length;
					Color drawColor = Color.Lerp(lightColor, Color.LightPink, 2f);
					drawColor = Color.Lerp(drawColor, Color.DarkViolet, completionRatio);
					drawColor = Color.Lerp(drawColor, Color.Transparent, completionRatio);
					drawPosition = base.Projectile.oldPos[i] + base.Projectile.Size * 0.5f - Main.screenPosition;
					Main.spriteBatch.Draw(starTexture, drawPosition, null, base.Projectile.GetAlpha(drawColor), 0, Utils.Size(starTexture) * 0.5f, base.Projectile.scale, SpriteEffects.None, 0f);
				}
				for (int j = 0; j < 1; j++)
				{
					Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Topaz/Bolt");
					Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
					int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
					int frameY = frameHeight * base.Projectile.frame;
					Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
					Vector2 origin = rectangle.Size() / 2f;
					float rotation = base.Projectile.rotation;
					float scale = base.Projectile.scale;
					SpriteEffects spriteEffects = SpriteEffects.None;
					

					Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(lightColor), rotation, origin, scale, spriteEffects, 0f);
					Projectile.netUpdate = true;

				}
					return false;
				
			}


			
		}

	}
}

