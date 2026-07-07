using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
namespace SariaMod.Items.zPearls
{
    public class Bloodmoon : ModProjectile
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
            if (base.Projectile.timeLeft == 10)
            {
                if (Main.dayTime)
                {
                    Main.time = 54300;
                }
                SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/DeadHand"), player.Center);
                SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/DLong"), player.Center);
                if (!Main.dayTime && !Main.bloodMoon)
                {
                    Main.bloodMoon = true;
                }
            }
            base.Projectile.position.X = player.position.X;
            base.Projectile.position.Y = player.position.Y - 80f;
        }
    }
}
