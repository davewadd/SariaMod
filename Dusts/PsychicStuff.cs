using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
namespace SariaMod.Dusts
{
    public class PsychicStuff : ModDust
    {
        public override void OnSpawn(Dust dust)
        {
            dust.velocity *= 0f;
            dust.noGravity = true;
            dust.scale *= 0.99f;
        }
        public override bool Update(Dust dust)
        {
            dust.scale *= .99f;
            float light = 0.35f * dust.scale;
            Lighting.AddLight(dust.position, light, light, light);
            return false;
        }
        public override bool MidUpdate(Dust dust)
        {
            if (dust.scale < 0.5f)
            {
                dust.active = false;
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