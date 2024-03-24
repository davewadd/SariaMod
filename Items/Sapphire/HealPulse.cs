using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SariaMod.Items.Topaz;
using SariaMod.Items.Emerald;
using SariaMod.Items.Amber;
using SariaMod.Items.Amethyst;
using SariaMod.Items.Ruby;
using SariaMod.Items;
using SariaMod.Items.zPearls;
using System;
using SariaMod.Items.Bands;
using SariaMod.Buffs;
using SariaMod.Dusts;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using SariaMod.Items.Strange;
using SariaMod.Items.Sapphire;
using Terraria.ModLoader;
using System.IO;

namespace SariaMod.Items.Sapphire
{
	public class HealPulse : ModProjectile
	{
		private int Stockpile;
		public override void SetStaticDefaults()
		{

			base.DisplayName.SetDefault("Child");
			Main.projFrames[base.Projectile.type] = 8;
			ProjectileID.Sets.MinionShot[base.Projectile.type] = true;
			ProjectileID.Sets.TrailingMode[base.Projectile.type] = 2;
			ProjectileID.Sets.TrailCacheLength[base.Projectile.type] = 30;

		}
		public override void SendExtraAI(BinaryWriter writer)
		{
			writer.Write(Stockpile);
		}
		public override void ReceiveExtraAI(BinaryReader reader)
		{
			Stockpile = (int)reader.ReadSingle();
		}
		public override void SetDefaults()
		{
			base.Projectile.width = 30;
			base.Projectile.height = 30;
			base.Projectile.netImportant = true;
			base.Projectile.friendly = true;
			base.Projectile.ignoreWater = true;
			base.Projectile.usesLocalNPCImmunity = true;
			base.Projectile.localNPCHitCooldown = 20;
			base.Projectile.minionSlots = 0f;
			base.Projectile.extraUpdates = 1;

			base.Projectile.penetrate = -1;
			base.Projectile.tileCollide = false;
			base.Projectile.timeLeft = 100;
			base.Projectile.minion = true;
		}
		private int HitCount;
        private float healing;

        public override bool? CanCutTiles()
		{
			return false;
		}

		public override bool MinionContactDamage()
		{
			return true;
		}
		public override bool? CanHitNPC(NPC target)
		{
			return base.Projectile.timeLeft < 400 && target.CanBeChasedBy(base.Projectile);
		}
		private const int sphereRadius = 3;
		
		public override void AI()
		{
			Player player = Main.player[Projectile.owner];
			Player player2 = Main.LocalPlayer;
			FairyPlayer modPlayer = player.Fairy();
			Projectile mother = Main.projectile[(int)base.Projectile.ai[1]];
			if (player.HasBuff(ModContent.BuffType<SariaBuff>()))
			{
				Projectile.timeLeft = 100;
			}
			Projectile.knockBack = 10;
			if (Stockpile <= 100)
            {
				if (Main.rand.NextBool(16))
					for (int num189 = 0; num189 < 1; num189++)
					{
						int num190 = Dust.NewDust(base.Projectile.position + base.Projectile.velocity, base.Projectile.width, base.Projectile.height, 107, base.Projectile.velocity.X * 0.5f, base.Projectile.velocity.Y * 0.5f);

						Main.dust[num190].velocity *= 0.5f;
						Main.dust[num190].scale *= 0.3f;
						Main.dust[num190].fadeIn = 1f;
						Main.dust[num190].noGravity = true;
					}
			}
			if (Stockpile > 100 && Stockpile <= 300)
			{
				if (Main.rand.NextBool(8))
					for (int num189 = 0; num189 < 1; num189++)
					{
						int num190 = Dust.NewDust(base.Projectile.position + base.Projectile.velocity, base.Projectile.width, base.Projectile.height, 107, base.Projectile.velocity.X * 0.5f, base.Projectile.velocity.Y * 0.5f);

						Main.dust[num190].velocity *= 0.5f;
						Main.dust[num190].scale *= 0.3f;
						Main.dust[num190].fadeIn = 1f;
						Main.dust[num190].noGravity = true;
					}
			}
			if (Stockpile > 300 && Stockpile < 450)
			{
				if (Main.rand.NextBool(8))
					for (int num189 = 0; num189 < 1; num189++)
					{
						int num190 = Dust.NewDust(base.Projectile.position + base.Projectile.velocity, base.Projectile.width, base.Projectile.height, 106, base.Projectile.velocity.X * 0.5f, base.Projectile.velocity.Y * 0.5f);

						Main.dust[num190].velocity *= 0.5f;
						Main.dust[num190].scale *= 0.3f;
						Main.dust[num190].fadeIn = 1f;
						Main.dust[num190].noGravity = true;
					}
			}
			if (Stockpile > 450 && Stockpile < 510)
			{
				if (Main.rand.NextBool(8))
					for (int num189 = 0; num189 < 1; num189++)
					{
						int num190 = Dust.NewDust(base.Projectile.position + base.Projectile.velocity, base.Projectile.width, base.Projectile.height, 114, base.Projectile.velocity.X * 0.5f, base.Projectile.velocity.Y * 0.5f);

						Main.dust[num190].velocity *= 0.5f;
						Main.dust[num190].scale *= 0.3f;
						Main.dust[num190].fadeIn = 1f;
						Main.dust[num190].noGravity = true;
					}
			}
			if (Stockpile >= 510)
			{
				if (Main.rand.NextBool(8))
					for (int num189 = 0; num189 < 1; num189++)
					{
						int num190 = Dust.NewDust(base.Projectile.position + base.Projectile.velocity, base.Projectile.width, base.Projectile.height, 114, base.Projectile.velocity.X * 0.5f, base.Projectile.velocity.Y * 0.5f);

						Main.dust[num190].velocity *= 0.5f;
						Main.dust[num190].scale *= 0.3f;
						Main.dust[num190].fadeIn = 1f;
						Main.dust[num190].noGravity = true;
						int num191 = Dust.NewDust(base.Projectile.position + base.Projectile.velocity, base.Projectile.width, base.Projectile.height, 106, base.Projectile.velocity.X * 0.5f, base.Projectile.velocity.Y * 0.5f);

						Main.dust[num191].velocity *= 0.5f;
						Main.dust[num191].scale *= 0.3f;
						Main.dust[num191].fadeIn = 1f;
						Main.dust[num191].noGravity = true;
					}
			}
			if (Stockpile > 510)
            {
				Stockpile = 510;
            }
			int owner = player.whoAmI;
			int XpProjectile = ModContent.ProjectileType<HealBubble>();
			
			for (int i = 0; i < 1000; i++)
			{
				if (Main.projectile[i].active && i != base.Projectile.whoAmI && ((Main.projectile[i].type == XpProjectile && Main.projectile[i].owner == owner)))
				{
					Vector2 vectorToPulsePosition = Main.projectile[i].Center - Projectile.Center;
					float distanceToPulsePosition = vectorToPulsePosition.Length();
					if (distanceToPulsePosition <= 50)
					{
						if (Stockpile < 510)
						{
							Stockpile += 15;
						}
						Main.projectile[i].Kill();
						Projectile.netUpdate = true;
					}

				}

			}
			
			// Reasonable distance away so it doesn't target across multiple screens
			for (int P = 0; P < 10; P++)
			{
				if (Main.player[P].active && Main.player[P].statLife < Main.player[P].statLifeMax2)
				{
					float between = Vector2.Distance(Main.player[P].Center, Projectile.Center);
					if (between < 10f && Stockpile >= 15)
					{
						Stockpile -= 15;
						SariaModUtilities.HealingProjectile(base.Projectile, 15, 0, 12f, 15f, autoHomes: false);
						Projectile.netUpdate = true;
					}
				}
			}
			Vector2 mouse = Main.MouseWorld;
			mouse.X += 10f;
			mouse.Y -= 5f;
			Vector2 idlePosition = player.Center;
			idlePosition.Y -= 48f; // Go up 48 coordinates (three tiles from the center of the player)

			// If your minion doesn't aimlessly move around when it's idle, you need to "put" it into the line of other summoned minions
			// The index is projectile.minionPos
			float minionPositionOffsetX = (10 + Projectile.minionPos * 40) * -player.direction;
			idlePosition.X += minionPositionOffsetX; // Go behind the player

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
			if (base.Projectile.frameCounter > 6)
			{
				base.Projectile.frame++;
				base.Projectile.frameCounter = 0;
			}
			if (base.Projectile.frame >= Main.projFrames[base.Projectile.type])
			{
				base.Projectile.frame = 0;
			}


			// Starting search distance

			bool foundTarget = false;

			// This code is required if your minion weapon has the targeting feature





			// friendly needs to be set to true so the minion can deal contact damage
			// friendly needs to be set to false so it doesn't damage things like target dummies while idling
			// Both things depend on if it has a target or not, so it's just one assignment here
			// You don't need this assignment if your minion is shooting things instead of dealing contact damage





			// Default movement parameters (here for attacking)
			float speed = 70f;
			float inertia = 20f;


			// Minion has a target: attack (here, fly towards the enemy)





			Vector2 direction2 = mouse - Projectile.Center;
			direction2.Normalize();
			direction2 *= speed;

			Projectile.Center = Main.MouseWorld;



		}
		public override bool PreDraw(ref Color lightColor)
		{
			
			{
				Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Sapphire/HealPulse");
				Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
				int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
				int frameY = frameHeight * base.Projectile.frame;
				Color drawColor = Color.Lerp(lightColor, Color.LightPink, 20f);
				drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
				Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
				Vector2 origin = rectangle.Size() / 2f;
				float rotation = base.Projectile.rotation;
				float scale = base.Projectile.scale;
				SpriteEffects spriteEffects = SpriteEffects.None;
			
				
				Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
			}
			
			return false;
		}

	}
}

