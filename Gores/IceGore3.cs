using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System;
using Terraria.Utilities;
namespace SariaMod.Gores
{
    public class IceGore3 : ModGore
    {
        public override bool Update(Gore gore)
        {
            if (gore.numFrames == 0)
            {
                gore.scale = 2f;
                gore.timeLeft = 300;// Initial upward velocity
            }
            // --- Deterministic Randomization for Sway Amount ---
            if (gore.timeLeft <= 550)
            {
                int seed = (int)(gore.position.X * 1000) + (int)gore.position.Y;
                var seededRandom = new UnifiedRandom(seed);
                // INCREASE THIS RANGE for more side-to-side movement.
                float swayAmount = seededRandom.NextFloat(0.5f, 1.0f);
                // --- End of Deterministic Randomization ---
                if (gore.velocity.Y >= .9f)
                {
                    gore.velocity.Y = .5f;
                }
                float swaySpeed = 0.05f;
                float positionOffset = (float)Math.Sin(gore.position.X / 100f);
                gore.velocity.X = (float)Math.Sin(Main.timeForVisualEffects * swaySpeed + positionOffset) * swayAmount;
                gore.rotation += gore.velocity.X * 0.1f;
            }
                gore.scale -= 1f / 300f;
                float light = 0.45f * gore.scale;
                Lighting.AddLight(gore.position, light, light, light);
                return true;
        }
    }
}
