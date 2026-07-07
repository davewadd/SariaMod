using Microsoft.Xna.Framework;
using Terraria;
namespace SariaMod.Dusts
{
    public class Cold : BaseTemplateDust
    {
        protected override YBounceMode BounceMode => YBounceMode.None;
        protected override bool NoLight => true;
        protected override float ScaleMultiplier => 2f;
        protected override int InitialAlpha => 150;
        public override Color? GetAlpha(Dust dust, Color lightColor) => null;
    }
}