using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
namespace SariaMod.Dusts
{
    public class SnowRingFog : ModDust
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
        public override bool MidUpdate(Dust dust)
        {
            if (!dust.noGravity)
            {
                dust.velocity.Y += 0.05f;
            }
            dust.noGravity = true;
            if (dust.noLight)
            {
                return false;
            }
            dust.alpha++;
            float strength = dust.scale * 1.4f;
            if (strength > 1f)
            {
                strength = 1f;
            }
            if (dust.alpha > 270)
            {
                dust.active = false;
            }
            Lighting.AddLight(dust.position, 0.1f * strength, 0.2f * strength, 0.7f * strength);
            return false;
        }
        public override Color? GetAlpha(Dust dust, Color lightColor)
            => new Color(lightColor.R, lightColor.G, lightColor.B, 2);
    }
}