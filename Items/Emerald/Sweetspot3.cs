using Terraria.ModLoader;
namespace SariaMod.Items.Emerald
{
    public class Sweetspot3 : BaseSweetspot
    {
        protected override int DamageDivisor => 2;
        protected override int SpikeFollowType => ModContent.ProjectileType<Emeraldspike3>();
        protected override bool HasProjectileKillLoop => true;
        protected override int HitCheckSpawnType => ModContent.ProjectileType<HitCheck3>();
    }
}
