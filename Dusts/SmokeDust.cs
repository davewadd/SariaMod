using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
namespace SariaMod.Dusts
{
    public class SmokeDust : ModDust
    {
        public override void OnSpawn(Dust dust)
        {
            dust.velocity *= 0.4f;
            dust.noGravity = true;
            dust.noLight = true;
            dust.scale = 4f;
            dust.alpha = 0;
        }
        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            dust.rotation += dust.velocity.X * 0.15f;
            dust.scale *= 0.99f;
            dust.alpha += 1;
            if (dust.alpha == 300f)
            {
                dust.active = false;
            }
            if (dust.velocity.Y > 0)
            {
                dust.velocity.Y *= -1;
            }
            float light = 0.35f * dust.scale;
            Lighting.AddLight(dust.position, Color.OrangeRed.ToVector3() * 3f);
            if (dust.scale < 0.5f)
            {
                dust.active = false;
            }
            return false;
        }
    }
}