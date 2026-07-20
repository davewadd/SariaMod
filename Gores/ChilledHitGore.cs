using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;

namespace SariaMod.Gores
{
    /// <summary>
    /// A small ice chip shed by a living Chilled enemy when it takes damage.
    /// It reuses the IceGore2 texture but has an independent, short lifetime.
    /// </summary>
    public class ChilledHitGore : ModGore
    {
        public const int LifetimeTicks = 180;
        private const int FadeOutTicks = 60;

        public override string Texture => "SariaMod/Gores/IceGore2";

        public override bool Update(Gore gore)
        {
            // Gore defaults can assign a much longer lifetime. Keep this fragment below
            // the requested four-second limit even if another system changes its spawn data.
            if (gore.timeLeft > LifetimeTicks)
            {
                gore.timeLeft = LifetimeTicks;
            }

            if (gore.timeLeft <= 1)
            {
                gore.active = false;
                return false;
            }

            gore.velocity.X *= 0.98f;
            gore.rotation += gore.velocity.X * 0.08f;

            if (gore.timeLeft <= FadeOutTicks)
            {
                gore.alpha = Math.Min(255, gore.alpha + 5);
            }

            float lightStrength = 0.2f * gore.scale * (1f - gore.alpha / 255f);
            Lighting.AddLight(gore.position, new Vector3(0.6f, 0.85f, 1f) * lightStrength);
            return true;
        }
    }
}
