using Terraria;
using Terraria.ModLoader;

namespace SariaMod.Dusts
{
	public class Sneeze : ModDust
	{
		public override void SetDefaults() {
			updateType = 33;

		}

		public override void OnSpawn(Dust dust) {
			dust.alpha = 0;
			dust.velocity *= 0.5f;
			dust.velocity.Y += 1f;

		}
	}
}