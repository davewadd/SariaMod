using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
namespace SariaMod.Dusts
{
    public class FlameDustSaria : BaseFlameDust
    {
        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            dust.rotation += dust.velocity.X * 0.15f;
            dust.scale -= 0.1f;
            float light = GetUpdateLight(dust);
            Lighting.AddLight(dust.position, Color.OrangeRed.ToVector3() * 1.5f);
            if (dust.scale < 0.01f)
            {
                dust.active = false;
            }
            return false;
        }
        public override bool MidUpdate(Dust dust)
        {
            if (!dust.noGravity)
            {
                dust.velocity.Y += 0.05f;
            }
            if (dust.noLight)
            {
                return false;
            }
            float strength = dust.scale * 1.4f;
            if (strength > 1f)
            {
                strength = 1f;
            }
            return false;
        }
    }
}