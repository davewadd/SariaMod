using Terraria;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace SariaMod.Dusts
{
	public class MegaDust : ModDust
	{
		public override void OnSpawn(Dust dust) {
			dust.velocity *= 0.4f;
			dust.noGravity = true;
			dust.noLight = true;
			dust.scale *= 1.5f;
		}

		public override bool Update(Dust dust) {
			dust.position += dust.velocity;
			dust.rotation += dust.velocity.X * 0.15f;
			dust.scale *= 0.99f;
			float light = 0.35f * dust.scale;
			Lighting.AddLight(dust.position, new Color(Main.DiscoR, Main.DiscoG, Main.DiscoB).ToVector3() * 1.5f);
			if (dust.scale < 0.5f) {
				dust.active = false;
			}
			return false;
		}

		public override Color? GetAlpha(Dust dust, Color lightColor)
			=> new Color(Main.DiscoR, Main.DiscoG, Main.DiscoB, 25);
	}
}