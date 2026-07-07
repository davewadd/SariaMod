using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
namespace SariaMod.Dusts
{
    public class BurningPsychic3 : ModDust
    {
        public override void OnSpawn(Dust dust)
        {
            dust.position += 180 * dust.velocity;
            dust.noGravity = true;
            dust.noLight = true;
            dust.scale *= 1.1f;
        }
        public override bool Update(Dust dust)
        {
            dust.position.Y += -.02f;
            dust.position.X += -.02f;
            {
                dust.position -= dust.velocity;
            }
            dust.rotation += dust.velocity.X * 0.15f;
            dust.scale *= 0.99f;
            float light = 0.15f * dust.scale;
            Lighting.AddLight(dust.position, light, light, light);
            if (dust.scale < 0.5f)
            {
                dust.active = false;
            }
            return false;
        }
        public override Color? GetAlpha(Dust dust, Color lightColor)
            => new Color(lightColor.R, lightColor.G, lightColor.B, 25);
    }
}