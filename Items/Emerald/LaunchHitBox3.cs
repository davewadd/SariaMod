using Microsoft.Xna.Framework;
using SariaMod.Buffs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using SariaMod.Dusts;
namespace SariaMod.Items.Emerald
{
    public class LaunchHitBox3 : BaseLaunchHitBox
    {
        protected override int HitBoxWidth => 600;
        protected override int HitBoxHeight => 500;
        protected override int DamageMultiplier => 2;
        protected override int DebuffDuration => 60;
        protected override int EmeraldspikeType => ModContent.ProjectileType<Emeraldspike3>();
    }
}
