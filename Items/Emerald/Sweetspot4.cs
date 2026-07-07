using Terraria.ModLoader;
namespace SariaMod.Items.Emerald
{
    public class Sweetspot4 : BaseSweetspot
    {
        protected override int ProjectileWidth => 500;
        protected override int SpikeFollowType => ModContent.ProjectileType<Emeraldspike3>();
        protected override int KillConditionSpikeType => ModContent.ProjectileType<Emeraldspike3_2>();
        protected override bool HasProjectileKillLoop => true;
        protected override int LaunchHitBoxType => ModContent.ProjectileType<LaunchHitBox3>();
    }
}
