using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
namespace SariaMod.Dusts
{
    public class FlameDust3 : ModDust
    {
        public override void OnSpawn(Dust dust)
        {
            dust.velocity *= Main.rand.NextVector2CircularEdge(1f, 1f) * 800f;
            dust.position += dust.velocity;
            dust.noGravity = true;
            dust.noLight = true;
            dust.scale = 5f;
        }
        public override bool Update(Dust dust)
        {
            dust.position.Y += -.02f;
            dust.position.X += -.02f;
            {
                dust.position -= dust.velocity;
            }
            dust.scale *= .95f;
            dust.alpha += 2;
            float light = 5.5f * dust.scale;
            Lighting.AddLight(dust.position, Color.Yellow.ToVector3() * .01f);
            if (dust.alpha == 300)
            {
                dust.active = false;
            }
            return false;
        }
        public override Color? GetAlpha(Dust dust, Color lightColor)
            => new Color(lightColor.R, lightColor.G, lightColor.B, 25);
    }
}