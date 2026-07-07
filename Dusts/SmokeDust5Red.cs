using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
namespace SariaMod.Dusts
{
    public class SmokeDust5Red : BaseSmokeDust5
    {
        protected override Color DustColor => Color.OrangeRed;
        protected override Color LightColor => Color.OrangeRed;
        protected override float LightColorIntensity => 1f;
    }
}