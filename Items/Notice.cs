using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
namespace SariaMod.Items
{
    public class Notice : ModProjectile
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
            {
                return false;
            }
        }
        public override void SetDefaults()
        {
            base.Projectile.width = 11;
            base.Projectile.height = 27;
            base.Projectile.netImportant = true;
            base.Projectile.friendly = true;
            Projectile.alpha = 300;
            base.Projectile.ignoreWater = false;
            base.Projectile.usesLocalNPCImmunity = true;
            base.Projectile.localNPCHitCooldown = 50;
            base.Projectile.minionSlots = 0f;
            base.Projectile.timeLeft = 100;
            base.Projectile.penetrate = -1;
            base.Projectile.tileCollide = false;
            base.Projectile.minion = true;
        }
        public override void AI()
        {
            Player player = Main.player[base.Projectile.owner];
            FairyPlayer modPlayer = player.Fairy();
            if (Projectile.timeLeft >= 200)
            {
                SoundEngine.PlaySound(SoundID.Item30, base.Projectile.Center);
            }
            if (Projectile.timeLeft == 100)
            {
                SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Notice"), base.Projectile.Center);
            }
        }
    }
}