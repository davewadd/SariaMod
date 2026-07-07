using Terraria;
using Terraria.ModLoader;
namespace SariaMod.Dusts
{
    public class BubbleDustSmall : ModDust
    {
        public override void OnSpawn(Dust dust)
        {
            dust.velocity *= 0.1f;
            dust.noGravity = true;
            dust.noLight = true;
            dust.scale = 1f;
        }
        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            dust.velocity.Y += -.005f;
            dust.scale *= 0.99f;
            float strength = 0.35f * dust.scale;
            if (strength > 1f)
            {
                strength = 1f;
            }
            Lighting.AddLight(dust.position, 0.1f * strength, 0.2f * strength, 0.7f * strength);
            if (dust.scale < 0.1f)
            {
                dust.active = false;
            }
            return false;
        }
    }
}