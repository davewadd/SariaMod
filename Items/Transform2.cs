using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace SariaMod.Items
{
    public class Transform2 : ModProjectile
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
            base.Projectile.timeLeft = 300;
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
            {
                // friendly needs to be set to true so the minion can deal contact damage
                // friendly needs to be set to false so it doesn't damage things like target dummies while idling
                // Both things depend on if it has a target or not, so it's just one assignment here
                // You don't need this assignment if your minion is shooting things instead of dealing contact damage
                FairyProjectile.HomeInOnNPC(base.Projectile, ignoreTiles: true, 600f, 25f, 20f);
                {
                    float distanceFromTarget = 10f;
                    Vector2 targetCenter = Projectile.position;
                    bool foundTarget = false;
                    // This code is required if your minion weapon has the targeting feature
                    if (player.HasMinionAttackTargetNPC)
                    {
                        NPC npc = Main.npc[player.MinionAttackTargetNPC];
                        float between = Vector2.Distance(npc.Center, Projectile.Center);
                        // Reasonable distance away so it doesn't target across multiple screens
                        if (between < 2000f)
                        {
                            distanceFromTarget = between;
                            targetCenter = npc.Center;
                            targetCenter.Y -= 0f;
                            targetCenter.X += 0f;
                            foundTarget = true;
                        }
                    }
                }
                // Default movement parameters (here for attacking)
            }
        }
    }
}
