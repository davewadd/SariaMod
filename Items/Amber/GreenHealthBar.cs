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

namespace SariaMod.Items.Amber
{
	public class GreenHealthBar: ModProjectile
	{
		public override void SetDefaults()
		{
			
			
			base.projectile.width = 42;
			base.projectile.height = 40;
			base.projectile.hostile = false;
			base.projectile.friendly = false;
			base.projectile.ignoreWater = true;
			
			base.projectile.timeLeft = 340;
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
			Main.projFrames[base.projectile.type] = 4;
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
			

			
			if (player.dead || !player.active)
			{
				
				projectile.Kill();
			}
			Projectile mother = Main.projectile[(int)base.projectile.ai[0]];
			if (!mother.active )
			{
				base.projectile.Kill();
				return;
			}
			projectile.timeLeft = mother.timeLeft;
			base.projectile.position.X = mother.Center.X - 20;
			base.projectile.position.Y = mother.Center.Y + 30;
			base.projectile.netUpdate = true;
			if (projectile.timeLeft <= 1000)
            {
				projectile.frame = 0;
            }
			else if (projectile.timeLeft > 1000 && (projectile.timeLeft <= 2500))
			{
				projectile.frame = 1;
			}
			else if (projectile.timeLeft > 2500 && (projectile.timeLeft <= 4500))
			{
				projectile.frame = 2;
			}
			else if (projectile.timeLeft > 4500 && (projectile.timeLeft <= 6000))
			{
				projectile.frame = 3;
			}









			Lighting.AddLight(projectile.Center, Color.Red.ToVector3() * 1f);
			
		}
	}
}
