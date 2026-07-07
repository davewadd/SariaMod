using Terraria.ModLoader;
namespace SariaMod.Items.Emerald
{
    public class HitCheck2 : BaseHitCheck
    {
        protected override bool SpawnKillEffects => true;
        protected override int PassiveProjectileType => ModContent.ProjectileType<RupeeXPassive2>();
    }
}
