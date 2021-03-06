using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.Events;
using System;

using Terraria;
using SariaMod.Buffs;



using SariaMod.Dusts;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.zPearls
{
	public class EmptyNote : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			base.DisplayName.SetDefault("Blade");
			ProjectileID.Sets.TrailCacheLength[base.projectile.type] = 7;
			ProjectileID.Sets.TrailingMode[base.projectile.type] = 0;
			
		}

		public override void SetDefaults()
		{
			base.projectile.width = 30;
			base.projectile.height = 30;
			
			base.projectile.alpha = 300;
			base.projectile.friendly = true;
			base.projectile.tileCollide = false;
			
			base.projectile.penetrate = 1;
			base.projectile.timeLeft = 2;
			base.projectile.ignoreWater = true;
			
			base.projectile.usesLocalNPCImmunity = true;
			base.projectile.localNPCHitCooldown = 4;
		}

		public override bool? CanHitNPC(NPC target)
        {
			return false;
        }
		public override void AI()
		{
			Player player = Main.player[base.projectile.owner];
			Projectile mother = Main.projectile[(int)base.projectile.ai[0]];
			Lighting.AddLight(projectile.Center, Color.LightGreen.ToVector3() * 1f);
			projectile.position.X = player.position.X ;
			projectile.position.Y = player.position.Y;
			if (projectile.timeLeft == 2 && Main.player[Main.myPlayer].active && Main.eclipse && Main.player[Main.myPlayer].ZoneOverworldHeight)
			{
				Item.NewItem(base.projectile.Center + new Vector2(0f, 0f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ItemType<EclipseOcarina>());
			}
			else if (projectile.timeLeft == 2 && Main.player[Main.myPlayer].active && Main.bloodMoon && Main.player[Main.myPlayer].ZoneOverworldHeight)
			{
				Item.NewItem(base.projectile.Center + new Vector2(0f, 0f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ItemType<BloodOcarina>());
			}
			else if (projectile.timeLeft == 2 && Main.player[Main.myPlayer].active && Sandstorm.Happening && Main.player[Main.myPlayer].ZoneOverworldHeight && Main.player[Main.myPlayer].ZoneDesert)
			{
				Item.NewItem(base.projectile.Center + new Vector2(0f, 0f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ItemType<SandOcarina>());
			}
			else if (projectile.timeLeft == 2 && Main.player[Main.myPlayer].active && Main.raining == true && Main.player[Main.myPlayer].ZoneOverworldHeight)
			{
				Item.NewItem(base.projectile.Center + new Vector2(0f, 0f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ItemType<RainOcarina>());
			}
			else if (projectile.timeLeft == 2 && Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneJungle)
			{
				Item.NewItem(base.projectile.Center + new Vector2(0f, 0f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ItemType<ForestOcarina>());
			}
			else if (projectile.timeLeft == 2)
			{
				Item.NewItem(base.projectile.Center + new Vector2(0f, 0f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ItemType<TimeOcarina>());
			}
			if (projectile.velocity.X >= 0)
			{
				projectile.spriteDirection = 1;
			}
			if (projectile.velocity.X <= -0)
			{
				projectile.spriteDirection = -1;
			}
		}

	

		

	



	}
}
