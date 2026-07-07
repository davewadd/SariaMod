using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
namespace SariaMod.Dusts
{
    public class FlameDust2 : BaseFlameDust
    {
        protected override float InitialScale => 2.7f;
        protected override Vector3 MidLightColor => Color.OrangeRed.ToVector3();
        protected override float MidLightIntensity => 1f;
        public override Color? GetAlpha(Dust dust, Color lightColor)
            => new Color(lightColor.R, lightColor.G, lightColor.B, 160);
    }
}