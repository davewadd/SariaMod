using Terraria;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace SariaMod.Dusts
{
	public class FlameDustSaria : ModDust
	{
		public override void OnSpawn(Dust dust) {
			dust.velocity *= 0.4f;
			dust.noGravity = true;
			dust.scale = 1.5f;
		}

		public override bool Update(Dust dust) {
			dust.position += dust.velocity;
			dust.rotation += dust.velocity.X * 0.15f;
			dust.scale -= 0.1f;
			float light = 0.35f * dust.scale;
			Lighting.AddLight(dust.position, Color.OrangeRed.ToVector3() * 1.5f);
			if (dust.scale < 0.01f) {
				dust.active = false;
			}
			return false;
		}
		public override bool MidUpdate(Dust dust)
		{
			if (!dust.noGravity)
			{
				dust.velocity.Y += 0.05f;
			}

			if (dust.noLight)
			{
				return false;
			}

			float strength = dust.scale * 1.4f;
			if (strength > 1f)
			{
				strength = 1f;
			}
			
			return false;
		}

		public override Color? GetAlpha(Dust dust, Color lightColor)
			=> new Color(lightColor.R, lightColor.G, lightColor.B, 25);
	}
}