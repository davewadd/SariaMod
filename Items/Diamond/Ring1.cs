using System;
using Microsoft.Xna.Framework;

using SariaMod.Items.Sapphire;
using SariaMod.Items.Ruby;
using SariaMod.Items.Topaz;
using SariaMod.Items.Emerald;
using SariaMod.Items.Amber;
using SariaMod.Items.Amethyst;
using SariaMod.Items.Diamond;

using Microsoft.Xna.Framework.Graphics;
using SariaMod.Items.Platinum;
using SariaMod.Items.Barrier;
using SariaMod.Items.Strange;
using SariaMod.Items.zPearls;
using SariaMod.Items.Bands;
using SariaMod.Buffs;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.Diamond
{
	public class Ring1 : ModProjectile
	{
		public override void SetDefaults()
		{
			
			
			base.projectile.width = 42;
			base.projectile.height = 40;
			base.projectile.hostile = false;
			base.projectile.friendly = false;
			base.projectile.ignoreWater = true;
			projectile.alpha = 0;
			base.projectile.timeLeft = 20;
			base.projectile.penetrate = -1;
			base.projectile.tileCollide = false;
			base.projectile.minion = false;
			base.projectile.localNPCHitCooldown = 5;
			base.projectile.minionSlots = 0f;
			base.projectile.netImportant = true;
			base.projectile.usesLocalNPCImmunity = true;
			


		}
		
		public override void SetStaticDefaults()
		{
			base.DisplayName.SetDefault("Psychic Turret");
			Main.projFrames[base.projectile.type] = 0;
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
			if (mother.active && projectile.timeLeft <=50)
			{
				
				projectile.timeLeft = 250;
			}
			base.projectile.position.X = mother.Center.X - 21;
			base.projectile.position.Y = mother.Center.Y - 20;


			if (player.ownedProjectileCounts[ModContent.ProjectileType<MoonBlast>()] <= 0f)
            {
				projectile.Kill();
            }







				Lighting.AddLight(projectile.Center, Color.LightPink.ToVector3() * 2f);
			
		}

		public override Color? GetAlpha(Color lightColor)
		{
			if (base.projectile.timeLeft < 250)
			{
				byte b2 = (byte)(base.projectile.timeLeft * 4);
				byte a2 = (byte)(100f * ((float)(int)b2 / 255f));
				return new Color(b2, b2, b2, a2);
			}
			return new Color(255, 255, 255, 100);
		}

	}
}
