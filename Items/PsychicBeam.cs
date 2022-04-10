using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FairyMod.Projectiles;
using SariaMod.Items;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items
{
	public class PsychicBeam : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			base.DisplayName.SetDefault("Laser");
			Main.projFrames[base.projectile.type] = 1;
			ProjectileID.Sets.MinionShot[base.projectile.type] = true;
		}

		public override void SetDefaults()
		{
			base.projectile.width = 14;
			base.projectile.height = 14;
			base.projectile.light = 0.5f;
			base.projectile.alpha = 255;
			base.projectile.extraUpdates = 0;
			base.projectile.tileCollide = false;
			base.projectile.friendly = true;
			base.projectile.minion = true;
			base.projectile.minionSlots = 0f;
			base.projectile.ignoreWater = true;
			base.projectile.aiStyle = 1;
			aiType = 242;
			base.projectile.velocity.X = 0f;
			base.projectile.penetrate = 1;
			base.projectile.timeLeft = 600;
		}
	
	public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			FairyGlobalProjectile.DrawCenteredAndAfterimage(base.projectile, lightColor, ProjectileID.Sets.TrailingMode[base.projectile.type], 2);
			return true;
	
		}
	}
}
