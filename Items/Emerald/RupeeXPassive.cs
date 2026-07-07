using Microsoft.Xna.Framework;
using Terraria.ModLoader;
namespace SariaMod.Items.Emerald
{
    public class RupeeXPassive : BaseRupeeXPassive
    {
        protected override int[] DeactivationTargetTypes => new int[]
        {
            ModContent.ProjectileType<RupeeXPassive2>(),
            ModContent.ProjectileType<RupeeXPassive3>()
        };
        protected override int[] KillPropagationPassiveTypes => new int[]
        {
            ModContent.ProjectileType<RupeeXPassive2>(),
            ModContent.ProjectileType<RupeeXPassive3>()
        };
        protected override int[] KillPropagationChangeTypes => new int[]
        {
            ModContent.ProjectileType<Change1_2>(),
            ModContent.ProjectileType<Change1_3>()
        };
    }
}
