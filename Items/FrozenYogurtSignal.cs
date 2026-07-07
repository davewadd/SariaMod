using SariaMod.Items.Strange;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace SariaMod.Items
{
    public class FrozenYogurtSignal : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Saria");
            ProjectileID.Sets.TrailCacheLength[base.Projectile.type] = 7;
            ProjectileID.Sets.TrailingMode[base.Projectile.type] = 0;
        }
        public override void SetDefaults()
        {
            base.Projectile.width = 30;
            base.Projectile.height = 30;
            base.Projectile.alpha = 300;
            base.Projectile.friendly = true;
            base.Projectile.tileCollide = false;
            base.Projectile.netImportant = true;
            base.Projectile.penetrate = 1;
            base.Projectile.timeLeft = 10;
            base.Projectile.ignoreWater = true;
            base.Projectile.usesLocalNPCImmunity = true;
            base.Projectile.localNPCHitCooldown = 4;
        }
        public override bool? CanHitNPC(NPC target)
        {
            return false;
        }
        public override void AI()
        {
            Player player = Main.player[base.Projectile.owner];
            Projectile mother = Main.projectile[(int)base.Projectile.ai[1]];
            FairyProjectile.HomeInOnNPC(base.Projectile, ignoreTiles: true, 600f, 25f, 20f);
            base.Projectile.rotation += 0.095f;
            Projectile.timeLeft = 100;
            for (int g = 0; g < Main.maxProjectiles; g++)
            {
                if (Main.projectile[g].active && Main.projectile[g].ModProjectile is Saria modProjectile && (modProjectile.Eating == 3) && Main.projectile[g].owner == player.whoAmI)
                {
                    Projectile.Kill();
                }
            }
        }
    }
}
