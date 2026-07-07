using Terraria;
using Terraria.ModLoader;
namespace SariaMod.Dusts
{
    public class ChargeFlameDust : ModDust
    {
        public override void OnSpawn(Dust dust)
        {
            dust.velocity *= Main.rand.NextVector2CircularEdge(1f, 1f) * 2f;
            dust.noGravity = true;
            dust.noLight = true;
            dust.scale = 5f;
        }
        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            dust.scale *= .92f;
            dust.alpha += 3;
            float light = 5.5f * dust.scale;
            float strength = dust.scale * 1.4f;
            if (strength > 1f)
            {
                strength = 1f;
            }
            Lighting.AddLight(dust.position, 0.1f * strength, 0.3f * strength, 0.2f * strength);
            if (dust.alpha == 300)
            {
                dust.active = false;
            }
            return false;
        }
    }
}