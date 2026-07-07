using Terraria.ModLoader;
namespace SariaMod.Items.Emerald
{
    public class HitCheck2_2 : BaseHitCheck
    {
        protected override bool SpawnKillEffects => true;
        protected override float KillDustScale => 3.5f;
        protected override int PassiveProjectileType => ModContent.ProjectileType<RupeeXPassive2>();
        protected override int PassiveDamageIncrement => 3;
    }
}
