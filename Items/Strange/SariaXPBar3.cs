using System;
using Microsoft.Xna.Framework;

using SariaMod.Items.Sapphire;
using SariaMod.Items.Ruby;
using SariaMod.Items.Topaz;
using SariaMod.Items.Emerald;
using SariaMod.Items.Amber;
using SariaMod.Items.Amethyst;
using SariaMod.Items.Diamond;
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
	public class SariaXPBar3: ModProjectile
	{
		public override void SetDefaults()
		{
			
			
			base.projectile.width = 40;
			base.projectile.height = 20;
			base.projectile.hostile = false;
			base.projectile.friendly = false;
			base.projectile.ignoreWater = true;
			
			base.projectile.timeLeft = 200;
			base.projectile.penetrate = -1;
			base.projectile.tileCollide = false;
			base.projectile.minion = true;
			base.projectile.localNPCHitCooldown = 5;
			base.projectile.minionSlots = 0f;
			base.projectile.netImportant = true;
			base.projectile.usesLocalNPCImmunity = true;
			


		}
		public override void SetStaticDefaults()
		{
			base.DisplayName.SetDefault("Psychic Turret");
			Main.projFrames[base.projectile.type] = 1;
			Main.projPet[projectile.type] = true;
			ProjectileID.Sets.MinionSacrificable[base.projectile.type] = false;
			ProjectileID.Sets.MinionTargettingFeature[base.projectile.type] = true;
		}
        public override bool CanDamage()
        {
			return false;
        }

        public override void AI()
		{
			Player player = Main.player[base.projectile.owner];
			FairyPlayer modPlayer = player.Fairy();


			Projectile mother = Main.projectile[(int)base.projectile.ai[0]];
			if (!mother.active)
			{
				base.projectile.Kill();
				return;
			}
			if (player.ownedProjectileCounts[ModContent.ProjectileType<Transform>()] > 0f)
            {
				projectile.timeLeft = 200;
            }
			projectile.position.X = mother.Center.X;
			projectile.position.Y = mother.Center.Y - 70;
			projectile.spriteDirection = mother.spriteDirection;


			if (modPlayer.Sarialevel == 0)
			{
				if (modPlayer.SariaXp <= 375)
				{
					projectile.frame = 0;
				}
				else if (modPlayer.SariaXp > 375 && (modPlayer.SariaXp <= 750))
				{
					projectile.frame = 1;
				}
				else if (modPlayer.SariaXp > 750 && (modPlayer.SariaXp <= 1125))
				{
					projectile.frame = 2;
				}
				else if (modPlayer.SariaXp > 1125 && (modPlayer.SariaXp <= 1500))
				{
					projectile.frame = 3;
				}
				else if (modPlayer.SariaXp > 1500 && (modPlayer.SariaXp <= 1875))
				{
					projectile.frame = 4;
				}
				else if (modPlayer.SariaXp > 1875 && (modPlayer.SariaXp <= 2250))
				{
					projectile.frame = 5;
				}
				else if (modPlayer.SariaXp > 2250 && (modPlayer.SariaXp <= 2625))
				{
					projectile.frame = 6;
				}
				else if (modPlayer.SariaXp > 2625 && (modPlayer.SariaXp <= 3000))
				{
					projectile.frame = 7;
				}
				else if ((modPlayer.SariaXp > 3000))
				{
					projectile.frame = 8;
				}
			}






			Lighting.AddLight(projectile.Center, Color.Red.ToVector3() * 1f);
			
		}
	}
}
