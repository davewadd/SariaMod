using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace SariaMod.Items
{
    public class NoMove : ModProjectile
    {
        public const float DistanceToCheck = 1100f;
        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Mother");
            Main.projFrames[base.Projectile.type] = 1;
            Main.projPet[Projectile.type] = true;
            ProjectileID.Sets.MinionSacrificable[base.Projectile.type] = false;
            ProjectileID.Sets.MinionTargettingFeature[base.Projectile.type] = true;
        }
        public override bool? CanCutTiles()
        {
            return false;
        }
        public override bool MinionContactDamage()
        {
            return false;
        }
        public override void SetDefaults()
        {
            base.Projectile.width = 96;
            base.Projectile.height = 78;
            base.Projectile.netImportant = true;
            base.Projectile.friendly = true;
            base.Projectile.ignoreWater = false;
            base.Projectile.usesLocalNPCImmunity = true;
            base.Projectile.localNPCHitCooldown = 50;
            base.Projectile.penetrate = -1;
            base.Projectile.tileCollide = false;
            Projectile.alpha = 0;
        }
        public override void AI()
        {
            Player player = Main.player[base.Projectile.owner];
            FairyPlayer modPlayer = player.Fairy();
            //////////////////////////////faces start
        }
    }
}