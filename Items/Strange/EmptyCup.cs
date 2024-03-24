using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;




using System;
using Terraria;
using SariaMod.Buffs;
using SariaMod.Items.Sapphire;
using SariaMod.Dusts;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.Strange
{
	public class EmptyCup : ModProjectile
	{
		public override void SetStaticDefaults()
		{
		    
			base.DisplayName.SetDefault("Child");
			Main.projFrames[base.Projectile.type] = 1;
			ProjectileID.Sets.MinionShot[base.Projectile.type] = true;
			ProjectileID.Sets.TrailingMode[base.Projectile.type] = 2;
			ProjectileID.Sets.TrailCacheLength[base.Projectile.type] = 30;

		}

		public override void SetDefaults()
		{
			base.Projectile.width = 100;
			base.Projectile.height = 100;
			base.Projectile.netImportant = true;
			base.Projectile.friendly = true;
			base.Projectile.ignoreWater = true;
			base.Projectile.usesLocalNPCImmunity = true;
			base.Projectile.localNPCHitCooldown = 20;
			base.Projectile.minionSlots = 0f;
			base.Projectile.extraUpdates = 1;
			base.Projectile.penetrate = -1;
			base.Projectile.tileCollide = false;
			base.Projectile.timeLeft = 500;
			base.Projectile.minion = true;
		}
		
		public override bool? CanCutTiles()
		{
			return false;
		}
		
		public override bool MinionContactDamage()
		{
			return true;
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
			
			
			FairyPlayer modPlayer = player.Fairy();
			modPlayer.SariaXp++;
			Projectile.netUpdate = true;

			knockback /= 4;
		}
		public override void AI()
		{
			Player player = Main.player[Projectile.owner];
			FairyPlayer modPlayer = player.Fairy();
			Projectile mother = Main.projectile[(int)base.Projectile.ai[1]];
			Projectile.knockBack = 20;
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
			
			{
				base.Projectile.rotation += 0.095f;
			}
			

			

			
		
		}
		public override bool PreDraw(ref Color lightColor)
        {
			Texture2D starTexture2 = TextureAssets.Projectile[ModContent.ProjectileType<EmptyCup>()].Value;
			Vector2 drawPosition;
			for (int j = 0; j < 4; j++)
			{
				drawPosition = base.Projectile.Center - Main.screenPosition;
				Main.spriteBatch.Draw(starTexture2, drawPosition, null, Color.White, base.Projectile.oldRot[j], Utils.Size(starTexture2) * 0.5f, base.Projectile.scale, SpriteEffects.None, 0f);
			}
			return false;
		}



	}
}

