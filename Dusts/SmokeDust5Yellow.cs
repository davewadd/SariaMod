using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
namespace SariaMod.Dusts
{
    public class SmokeDust5Yellow : BaseSmokeDust5
    {
        protected override Color LightColor => Color.Yellow;
        protected override float LightColorIntensity => 3f;
    }
}