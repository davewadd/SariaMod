using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
namespace SariaMod.Dusts
{
    public class SmokeDust5Yellow : ModDust
    {
        public override void OnSpawn(Dust dust)
        {
            dust.frame.Width = 84;
            dust.frame.Height = 86;
            dust.frame.Y = 1;
            dust.noGravity = true;
            dust.noLight = true;
            dust.scale = .5f;
        }
        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            dust.velocity *= .98f;
            dust.scale *= 1.01f;
            dust.alpha += 2;
            dust.GetColor(Color.Yellow);
            float light = 0.35f / dust.scale;
            Lighting.AddLight(dust.position, Color.Yellow.ToVector3() * 3f);
            if (dust.alpha == 300)
            {
                dust.active = false;
            }
            return false;
        }
    }
}