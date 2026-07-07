using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.zPearls
{
    /// <summary>
    /// Visual-only projectile that stays at the player's center.
    /// Used to trigger vignette effects when Oasis Ocarina is played.
    /// </summary>
    public class SandstormOcarinaNote : ModProjectile
    {
        private const int EFFECT_DURATION = 180; // 3 seconds at 60fps

        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Sandstorm Ocarina Effect");
        }

        public override void SetDefaults()
        {
            base.Projectile.width = 1;
            base.Projectile.height = 1;
            base.Projectile.alpha = 255; // Fully invisible - visual effect is drawn separately
            base.Projectile.friendly = false;
            base.Projectile.hostile = false;
            base.Projectile.tileCollide = false;
            base.Projectile.penetrate = -1; // Infinite penetration (doesn't matter, can't hit)
            base.Projectile.timeLeft = EFFECT_DURATION;
            base.Projectile.ignoreWater = true;
            base.Projectile.netImportant = true; // Ensure multiplayer sync
        }

        public override bool? CanHitNPC(NPC target)
        {
            return false;
        }

        public override bool CanHitPlayer(Player target)
        {
            return false;
        }

        public override bool CanHitPvp(Player target)
        {
            return false;
        }

        public override void AI()
        {
            Player player = Main.player[base.Projectile.owner];

            // Keep projectile at player's center
            if (player.active && !player.dead)
            {
                base.Projectile.Center = player.Center;
            }
            else
            {
                // Kill projectile if owner is dead or inactive
                base.Projectile.Kill();
            }
        }
    }
}
