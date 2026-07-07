using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
namespace SariaMod.Dusts
{
    public class Cold2 : BaseTemplateDust
    {
        protected override float VelocityMultiplier => 0.2f;
        protected override bool NoLight => true;
        protected override float ScaleMultiplier => 1f;
        protected override int InitialAlpha => 0;
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
        public override Color? GetAlpha(Dust dust, Color lightColor) => null;
    }
}