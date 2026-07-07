using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
namespace SariaMod.Dusts
{
    public class ShadowFlameDust2 : BaseFlameDust
    {
        protected override int InitialAlpha => 200;
        public override Color? GetAlpha(Dust dust, Color lightColor) => null;
    }
}