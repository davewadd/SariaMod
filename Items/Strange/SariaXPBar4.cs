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

namespace SariaMod.Items.Strange
{
	public class SariaXPBar4: ModProjectile
	{
		public override void SetDefaults()
		{
			
			
			base.Projectile.width = 40;
			base.Projectile.height = 20;
			base.Projectile.hostile = false;
			base.Projectile.friendly = false;
			base.Projectile.ignoreWater = true;
			
			base.Projectile.timeLeft = 200;
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
			Main.projFrames[base.Projectile.type] = 1;
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


			Projectile mother = Main.projectile[(int)base.Projectile.ai[1]];
			if (!mother.active)
			{
				base.Projectile.Kill();
				return;
			}
			if (player.ownedProjectileCounts[ModContent.ProjectileType<Transform>()] > 0f)
            {
				Projectile.timeLeft = 200;
            }
			Projectile.position.X = mother.Center.X;
			Projectile.position.Y = mother.Center.Y - 70;
			Projectile.spriteDirection = mother.spriteDirection;


			if (modPlayer.Sarialevel == 0)
			{
				if (modPlayer.SariaXp <= 375)
				{
					Projectile.frame = 0;
				}
				else if (modPlayer.SariaXp > 375 && (modPlayer.SariaXp <= 750))
				{
					Projectile.frame = 1;
				}
				else if (modPlayer.SariaXp > 750 && (modPlayer.SariaXp <= 1125))
				{
					Projectile.frame = 2;
				}
				else if (modPlayer.SariaXp > 1125 && (modPlayer.SariaXp <= 1500))
				{
					Projectile.frame = 3;
				}
				else if (modPlayer.SariaXp > 1500 && (modPlayer.SariaXp <= 1875))
				{
					Projectile.frame = 4;
				}
				else if (modPlayer.SariaXp > 1875 && (modPlayer.SariaXp <= 2250))
				{
					Projectile.frame = 5;
				}
				else if (modPlayer.SariaXp > 2250 && (modPlayer.SariaXp <= 2625))
				{
					Projectile.frame = 6;
				}
				else if (modPlayer.SariaXp > 2625 && (modPlayer.SariaXp <= 3000))
				{
					Projectile.frame = 7;
				}
				else if ((modPlayer.SariaXp > 3000))
				{
					Projectile.frame = 8;
				}
			}






			Lighting.AddLight(Projectile.Center, Color.Red.ToVector3() * 1f);
			
		}
	}
}
