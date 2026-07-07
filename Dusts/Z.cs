using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
namespace SariaMod.Dusts
{
    public class Z : ModDust
    {
        public override void OnSpawn(Dust dust)
        {
            dust.velocity *= 0.4f;
            dust.noGravity = true;
            dust.noLight = true;
            dust.scale = 1f;
            dust.alpha = 0;
        }
        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            dust.scale *= 1.01f;
            dust.alpha += 1;
            if (dust.alpha == 300f)
            {
                dust.active = false;
            }
            if (dust.velocity.Y > 0)
            {
                dust.velocity.Y *= -1;
            }
            if ((dust.velocity.X) > 0f)
            {
                dust.velocity.X = .2f;
            }
            if ((dust.velocity.X) < 0f)
            {
                dust.velocity.X = -.2f;
            }
            float light = 0.35f * dust.scale;
            Lighting.AddLight(dust.position, Color.Red.ToVector3() * 1f);
            if (dust.scale > 4f)
            {
                dust.active = false;
            }
            return false;
        }
    }
}