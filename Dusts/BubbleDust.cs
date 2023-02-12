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
			Lighting.AddLight(dust.position, Color.LightBlue.ToVector3() * 2f);
			if (dust.scale < 0.5f) {
				dust.active = false;
			}
			return false;
		}
	}
}