using Terraria;
using Terraria.ModLoader;

namespace SariaMod.Dusts
{
	public class Sneeze : ModDust
	{
		public override void SetStaticDefaults() {
			UpdateType = 33;

		}

		public override void OnSpawn(Dust dust) {
			dust.alpha = 0;
			dust.velocity *= 0.5f;
			dust.velocity.Y += 1f;

		}
	}
}