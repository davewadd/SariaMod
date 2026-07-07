using Microsoft.Xna.Framework;
using Terraria;
namespace SariaMod.Dusts
{
    public class Sparkle : BaseTemplateDust
    {
        protected override bool NoLight => true;
        public override Color? GetAlpha(Dust dust, Color lightColor) => null;
    }
}