using Terraria;
using Terraria.ModLoader;
namespace SariaMod.Dusts
{
    public class BubbleDust3 : ModDust
    {
        public override void OnSpawn(Dust dust)
        {
            dust.velocity *= 0.4f;
            dust.noGravity = true;
            dust.noLight = true;
            dust.scale = 2f;
        }
        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            dust.velocity.Y = -1;
            dust.scale *= 1.01f;
            float light = 0.35f * dust.scale;
            float strength = dust.scale * 1.4f;
            if (strength > 1f)
            {
                strength = 1f;
            }
            Lighting.AddLight(dust.position, 0.1f * strength, 0.2f * strength, 0.7f * strength);
            if (dust.scale > 5.5f)
            {
                dust.active = false;
            }
            return false;
        }
    }
}