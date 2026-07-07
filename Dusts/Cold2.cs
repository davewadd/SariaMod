using Terraria;
using Terraria.ModLoader;
namespace SariaMod.Dusts
{
    public class Cold2 : ModDust
    {
        public override void OnSpawn(Dust dust)
        {
            dust.velocity *= 0.2f;
            dust.noGravity = true;
            dust.noLight = true;
            dust.scale *= 1f;
            dust.alpha = 0;
        }
        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            dust.rotation += dust.velocity.X * 0.15f;
            dust.scale *= 1.01f;
            dust.alpha++;
            if (dust.alpha >= 300)
            {
                dust.active = false;
            }
            float light = 0.05f * dust.scale;
            Lighting.AddLight(dust.position, light, light, light);
            if (dust.scale < 0.5f)
            {
                dust.active = false;
            }
            return false;
        }
    }
}