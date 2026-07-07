using Terraria.ModLoader;
namespace SariaMod.Items.Emerald
{
    public class Sweetspot2 : BaseSweetspot
    {
        protected override int DamageDivisor => 12;
        protected override int SpikeFollowType => ModContent.ProjectileType<Emeraldspike2>();
        protected override bool HasProjectileKillLoop => true;
        protected override int HitCheckSpawnType => ModContent.ProjectileType<HitCheck2>();
        protected override int LaunchHitBoxType => ModContent.ProjectileType<LaunchHitBox2>();
    }
}
