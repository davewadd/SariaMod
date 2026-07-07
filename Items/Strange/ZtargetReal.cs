using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.Strange
{
    public class ZtargetReal : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Saria");
            ProjectileID.Sets.TrailCacheLength[base.Projectile.type] = 7;
            ProjectileID.Sets.TrailingMode[base.Projectile.type] = 0;
            Main.projFrames[base.Projectile.type] = 1;
        }
        
        public override void SetDefaults()
        {
            base.Projectile.width = 82;
            base.Projectile.height = 82;
            base.Projectile.netImportant = true;
            base.Projectile.alpha = 0;
            base.Projectile.friendly = true;
            base.Projectile.tileCollide = false;
            base.Projectile.penetrate = -1;
            base.Projectile.timeLeft = 500;
            base.Projectile.ignoreWater = true;
            base.Projectile.usesLocalNPCImmunity = true;
            base.Projectile.localNPCHitCooldown = 4;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            FairyPlayer modPlayer = player.Fairy();

            // ONLY the owner controls movement (Magic Missile pattern)
            if (Projectile.owner == Main.myPlayer)
            {
                if (!modPlayer.HealBallRightHoldActive || player.HeldItem.type != ModContent.ItemType<HealBall>())
                {
                    Projectile.Kill();
                    return;
                }

                Vector2 targetPosition = Main.MouseWorld;
                Vector2 direction = targetPosition - Projectile.Center;
                float distance = direction.Length();

                if (distance > 1f)
                {
                    direction.Normalize();

                    // Speed scales with distance for responsive feel
                    float desiredSpeed = MathHelper.Clamp(distance * 0.5f, 8f, 48f);

                    if (distance >= 64f)
                    {
                        // Flying mode: smooth turn toward target
                        Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * desiredSpeed, 0.25f);
                    }
                    else
                    {
                        // Hover mode: brake and nudge (prevents jitter when close)
                        Projectile.velocity *= 0.5f;
                        Projectile.velocity += direction * (distance * 0.15f);
                    }
                }
                else
                {
                    // Very close: stop
                    Projectile.velocity *= 0.3f;
                }

                // Sync velocity to other clients
                Projectile.netUpdate = true;
            }

            // ALL clients: visuals and common updates
            Projectile.timeLeft = 2;
            Projectile.scale = 0.7f;
            Projectile.rotation += 0.07f;
            Lighting.AddLight(Projectile.Center, Color.LightGoldenrodYellow.ToVector3() * 1f);
        }

        public override bool? CanCutTiles() => false;
        public override bool? CanHitNPC(NPC target) => false;
        public override bool MinionContactDamage() => false;
    }
}
