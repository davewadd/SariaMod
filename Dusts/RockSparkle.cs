using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
namespace SariaMod.Dusts
{
    public class RockSparkle : ModDust
    {
        public override void OnSpawn(Dust dust)
        {
            dust.velocity *= 0;
            dust.noGravity = true;
            dust.scale *= 0.8f;
        }
        public override bool Update(Dust dust)
        {
            dust.rotation = 2.15f * dust.scale;
            dust.scale *= 0.92f;
            dust.GetColor(Color.WhiteSmoke);
            float light = 0f * dust.scale;
            Lighting.AddLight(dust.position, light, light, light);
            if (dust.scale < 0.5f)
            {
                dust.active = false;
            }
            return false;
        }
        public override bool MidUpdate(Dust dust)
        {
            return true;
        }
        public override Color? GetAlpha(Dust dust, Color lightColor) => new Color(Color.White.R, Color.White.G, Color.White.B, 300);
    }
}