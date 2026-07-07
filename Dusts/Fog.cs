using Terraria;
using Terraria.ModLoader;
namespace SariaMod.Dusts
{
    public class Fog : ModDust
    {
        public override void OnSpawn(Dust dust)
        {
            dust.velocity *= 0.08f;
            dust.velocity.Y = .08f;
            dust.noGravity = true;
            dust.noLight = true;
            dust.scale *= 2f;
            dust.alpha = 180;
        }
        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            dust.rotation += dust.velocity.X * 0.01f;
            float light = 0.01f * dust.scale;
            Lighting.AddLight(dust.position, light, light, light);
            dust.scale *= 1.001f;
            dust.alpha += 1;
            if (dust.alpha == 300f)
            {
                dust.active = false;
            }
            return false;
        }
    }
}