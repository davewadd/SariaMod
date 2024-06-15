using Terraria;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace SariaMod.Dusts
{
	public class BubbleDust : ModDust
	{
		public override void OnSpawn(Dust dust) {
			dust.velocity *= 0.4f;
			dust.noGravity = true;
			dust.noLight = true;
			dust.scale = 4f;
		}

		public override bool Update(Dust dust) {
			dust.position += dust.velocity;
			
			dust.scale *= 0.99f;
			float light = 0.35f * dust.scale;
			float strength = dust.scale * 1.4f;
			if (strength > 1f)
			{
				strength = 1f;
			}
			Lighting.AddLight(dust.position, 0.1f * strength, 0.2f * strength, 0.7f * strength);
			if (dust.scale < 0.5f) {
				dust.active = false;
			}
			return false;
		}
	}
}