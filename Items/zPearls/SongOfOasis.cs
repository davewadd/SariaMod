using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Events;

namespace SariaMod.Items.zPearls
{
	public class SongOfOasis : ModProjectile
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
			if (base.projectile.timeLeft >= 500)
			{
				Main.PlaySound(base.mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/SongOfStorms"), player.Center);
				if (Sandstorm.Happening)
				{
					SariaModUtilities.StopSandstorm();
				}
				else
				{
					SariaModUtilities.StartSandstorm();
				}
			}
			if ((float)player.ownedProjectileCounts[ModContent.ProjectileType<OasisNote>()] > 0f && base.projectile.timeLeft <= 455)
			{
				base.projectile.timeLeft = 2;
			}
			base.projectile.position.X = player.position.X;
			base.projectile.position.Y = player.position.Y - 80f;
		}
	}
}
