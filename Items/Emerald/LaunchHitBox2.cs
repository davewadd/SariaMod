using Microsoft.Xna.Framework;
using SariaMod.Buffs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using SariaMod.Dusts;
namespace SariaMod.Items.Emerald
{
    public class LaunchHitBox2 : BaseLaunchHitBox
    {
        protected override int HitBoxWidth => 200;
        protected override int HitBoxHeight => 250;
        protected override int DamageMultiplier => -2;
        protected override int DebuffDuration => 20;
        protected override int EmeraldspikeType => ModContent.ProjectileType<Emeraldspike2>();
    }
}
