using Terraria;
using Terraria.ModLoader;
namespace SariaMod.Dusts
{
    public class HeartDust : ModDust
    {
        public override void OnSpawn(Dust dust)
        {
            dust.velocity *= 0.4f;
            if (dust.velocity.Y > 0)
            {
                dust.velocity.Y *= -1;
            }
            dust.noGravity = true;
            dust.noLight = true;
            dust.scale *= 1.8f;
            dust.alpha = 0;
        }
        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            dust.rotation = 0;
            dust.scale *= 0.99f;
            float light = 0.35f * dust.scale;
            Lighting.AddLight(dust.position, light, light, light);
            if (dust.scale < 0.5f)
            {
                dust.active = false;
            }
            return false;
        }
    }
}