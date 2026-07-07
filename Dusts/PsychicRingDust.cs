using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
namespace SariaMod.Dusts
{
    public class PsychicRingDust : ModDust
    {
        public override void OnSpawn(Dust dust)
        {
            {
                float num105 = dust.scale * 0.3f;
                if (num105 > 1f)
                {
                    num105 = 1f;
                }
                float light = 0.15f * dust.scale;
                Lighting.AddLight(dust.position, light, light, light);
            }
        }
        public override Color? GetAlpha(Dust dust, Color lightColor)
            => new Color(lightColor.R, lightColor.G, lightColor.B, 25);
    }
}