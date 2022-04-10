using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FairyMod.Projectiles;
using SariaMod.Items;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.Sapphire
{
	public class SapphirePsychicBeam : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			base.DisplayName.SetDefault("Laser");
			Main.projFrames[base.projectile.type] = 3;
			ProjectileID.Sets.MinionShot[base.projectile.type] = true;
		}

		public override void SetDefaults()
		{
			base.projectile.width = 14;
			base.projectile.height = 14;
			base.projectile.light = 0.5f;
			base.projectile.alpha = 255;
			base.projectile.extraUpdates = 2;
			base.projectile.tileCollide = true;
			base.projectile.friendly = true;
			base.projectile.minion = true;
			base.projectile.minionSlots = 0f;
			base.projectile.ignoreWater = true;
			base.projectile.aiStyle = 1;
			aiType = 242;
			base.projectile.penetrate = 1;
			base.projectile.timeLeft = 600;
		}
		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
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
			
			target.AddBuff(BuffID.Frostburn, 300);
			target.AddBuff(BuffID.Slow, 300);
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			FairyGlobalProjectile.DrawCenteredAndAfterimage(base.projectile, lightColor, ProjectileID.Sets.TrailingMode[base.projectile.type], 2);
			return true;
		}

		public override void AI()
		{
			Lighting.AddLight(projectile.Center, Color.LightBlue.ToVector3() * 0.78f);
			base.projectile.frameCounter++;
			if (base.projectile.frameCounter > 6)
			{
				base.projectile.frame++;
				base.projectile.frameCounter = 0;
			}
			if (base.projectile.frame >= Main.projFrames[base.projectile.type])
			{
				base.projectile.frame = 0;
			}
			if (base.projectile.timeLeft >= 599)
			{
				Main.PlaySound(base.mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/WaterBeam"), base.projectile.Center);
			}
		}	
	}
}
