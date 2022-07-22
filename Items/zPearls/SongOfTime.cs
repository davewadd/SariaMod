using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;

using Terraria;
using SariaMod.Buffs;



using SariaMod.Dusts;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.zPearls
{
	public class SongOfTime : ModProjectile
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
			base.projectile.timeLeft = 500;
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
			
				if (projectile.timeLeft >= 500)
            {
				Main.PlaySound(base.mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/SongOfTime"), player.Center);
				Main.time = 0.0;
				Main.dayTime = !Main.dayTime;
				if (Main.dayTime && ++Main.moonPhase >= 8)
				{
					Main.moonPhase = 0;
				}
			}
			if (((player.ownedProjectileCounts[ModContent.ProjectileType<TimeNote>()] > 0f)) && projectile.timeLeft <= 455)
			{
				projectile.timeLeft = 2;
			}
			projectile.position.X = player.position.X ;
			projectile.position.Y = player.position.Y - 80;
			
			
		}

	

		

	



	}
}
