using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
namespace SariaMod.Dusts
{
    public class Rain3 : ModDust
    {
        public override void OnSpawn(Dust dust)
        {
            dust.velocity.Y = Main.rand.Next(-10, 6) * 0.1f;
            dust.velocity.X *= 0.3f;
            dust.scale *= 1.1f;
        }
        public override bool MidUpdate(Dust dust)
        {
            dust.rotation *= 0;
            if (!dust.noGravity)
            {
                dust.velocity.Y += 0.5f;
            }
            if (dust.noLight)
            {
                return true;
            }
            float strength = dust.scale * 1.4f;
            if (strength > 1f)
            {
                strength = 1f;
            }
            return false;
        }
        public override Color? GetAlpha(Dust dust, Color lightColor)
            => new Color(lightColor.R, lightColor.G, lightColor.B, 25);
    }
}