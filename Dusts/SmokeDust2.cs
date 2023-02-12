using Terraria;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace SariaMod.Dusts
{
	public class SmokeDust2 : ModDust
	{
		public override void OnSpawn(Dust dust) {
			dust.velocity *= 0.4f;
			dust.noGravity = true;
			dust.noLight = true;
			dust.scale = 7f;
			dust.alpha = 180;
		}

		public override bool Update(Dust dust) {
			dust.position += dust.velocity;
			dust.rotation += dust.velocity.X * 0.15f;
			dust.scale *= 0.99f;
			dust.alpha += 1;
			if (dust.velocity.Y > 0)
			{
				dust.velocity.Y *= -1;
			}
			float light = 0.35f * dust.scale;
			Lighting.AddLight(dust.position, Color.DarkOrange.ToVector3() * 2f);
			if (dust.alpha == 300f)
			{
				dust.active = false;
			}
			if (dust.scale < 0.5f) {
				dust.active = false;
			}
			return false;
		}
		public override Color? GetAlpha(Dust dust, Color lightColor)
			=> new Color(lightColor.R, lightColor.G, lightColor.B, 25);
	}
}