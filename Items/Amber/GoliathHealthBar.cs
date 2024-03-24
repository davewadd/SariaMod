using System;
using Microsoft.Xna.Framework;

using SariaMod.Items.Sapphire;
using SariaMod.Items.Ruby;
using SariaMod.Items.Topaz;
using SariaMod.Items.Emerald;
using SariaMod.Items.Amber;
using SariaMod.Items.Amethyst;
 
using SariaMod.Items.Platinum;
using SariaMod.Items.Barrier;
using SariaMod.Items.Strange;
using SariaMod.Items.zPearls;
using SariaMod.Items.Bands;
using SariaMod.Buffs;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.Amber
{
	public class GoliathHealthBar: ModProjectile
	{
		public override void SetDefaults()
		{
			
			
			base.Projectile.width = 42;
			base.Projectile.height = 40;
			base.Projectile.hostile = false;
			base.Projectile.friendly = false;
			base.Projectile.ignoreWater = true;
			
			base.Projectile.timeLeft = 340;
			base.Projectile.penetrate = -1;
			base.Projectile.tileCollide = false;
			base.Projectile.minion = true;
			base.Projectile.localNPCHitCooldown = 5;
			base.Projectile.minionSlots = 0f;
			base.Projectile.netImportant = true;
			base.Projectile.usesLocalNPCImmunity = true;
			


		}
		
		public override void SetStaticDefaults()
		{
			base.DisplayName.SetDefault("Psychic Turret");
			Main.projFrames[base.Projectile.type] = 8;
			Main.projPet[Projectile.type] = true;
			ProjectileID.Sets.MinionSacrificable[base.Projectile.type] = false;
			ProjectileID.Sets.MinionTargettingFeature[base.Projectile.type] = true;
		}
        public override bool? CanDamage()/* tModPorter Suggestion: Return null instead of true */
        {
			return false;
        }

        public override void AI()
		{
			Player player = Main.player[base.Projectile.owner];
			FairyPlayer modPlayer = player.Fairy();
			

			
			if (player.dead || !player.active)
			{
				
				Projectile.Kill();
			}
			Projectile mother = Main.projectile[(int)base.Projectile.ai[1]];
			if (!mother.active )
			{
				base.Projectile.Kill();
				return;
			}
			Projectile.timeLeft = mother.timeLeft;
			base.Projectile.position.X = mother.Center.X - 20;
			base.Projectile.position.Y = mother.Center.Y + 70;
			base.Projectile.netUpdate = true;
			if (Projectile.timeLeft <= 1000)
            {
				Projectile.frame = 0;
            }
			else if (Projectile.timeLeft > 1000 && (Projectile.timeLeft <= 2500))
			{
				Projectile.frame = 1;
			}
			else if (Projectile.timeLeft > 2500 && (Projectile.timeLeft <= 4500))
			{
				Projectile.frame = 2;
			}
			else if (Projectile.timeLeft > 4500 && (Projectile.timeLeft <= 6500))
			{
				Projectile.frame = 3;
			}
			else if (Projectile.timeLeft > 6500 && (Projectile.timeLeft <= 9000))
			{
				Projectile.frame = 4;
			}
			else if (Projectile.timeLeft > 9000 && (Projectile.timeLeft <= 10000))
			{
				Projectile.frame = 5;
			}
			else if (Projectile.timeLeft > 10000 && (Projectile.timeLeft <= 11000))
			{
				Projectile.frame = 6;
			}
			else if (Projectile.timeLeft > 11000 && (Projectile.timeLeft <= 12000))
			{
				Projectile.frame = 7;
			}








			Lighting.AddLight(Projectile.Center, Color.Red.ToVector3() * 1f);
			
		}
	}
}
