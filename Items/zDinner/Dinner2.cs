using Microsoft.Xna.Framework;
using SariaMod.Items.zDinner;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using System;
namespace SariaMod.Items.zDinner
{
    public class Dinner2 : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Dinner2");
            ProjectileID.Sets.TrailCacheLength[base.Projectile.type] = 7;
            ProjectileID.Sets.TrailingMode[base.Projectile.type] = 0;
        }
        public override void SetDefaults()
        {
            base.Projectile.width = 30;
            base.Projectile.height = 30;
            base.Projectile.aiStyle = -1;
            base.Projectile.alpha = 0;
            base.Projectile.friendly = true;
            base.Projectile.tileCollide = true;
            base.Projectile.netImportant = true;
            base.Projectile.penetrate = 2;
            base.Projectile.timeLeft = 350;
            base.Projectile.ignoreWater = true;
            base.Projectile.usesLocalNPCImmunity = true;
            base.Projectile.localNPCHitCooldown = 4;
        }
        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            Player player = Main.player[Projectile.owner];
            if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center.X + 0, Projectile.Center.Y + 0, 0, 0, ModContent.ProjectileType<DinnerBomb>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
            Projectile.Kill();
        }
        public override void AI()
        {
            base.Projectile.rotation += 0.095f;
            // This is the variable used as a bounce counter.
            // It will be 0 until the first tile collision.
            float bounceCounter = Projectile.localAI[0];
            const float initialCurveTime = 60f;
            const float gravity = 0.2f;
            // Only run the initial AI phase if the projectile has not bounced.
            if (Projectile.timeLeft == 350)
            {
                SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/ohboy"), Projectile.Center);
            }
            if (bounceCounter == 0)
            {
                if (Projectile.ai[0] < initialCurveTime)
                {
                    Projectile.velocity.X *= 0.98f;
                    Projectile.velocity.Y *= 0.98f;
                    Projectile.velocity.X += (0.05f * Projectile.direction);
                }
                else
                {
                    Projectile.velocity.Y += gravity;
                    if (Projectile.velocity.Y > 16f)
                    {
                        Projectile.velocity.Y = 16f;
                    }
                }
            }
            else
            {
                // After the first bounce, continue to apply gravity but stop the initial curving logic.
                Projectile.velocity.Y += gravity;
                if (Projectile.velocity.Y > 16f)
                {
                    Projectile.velocity.Y = 16f;
                }
            }
            Projectile.ai[0]++;
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Player player = Main.player[Projectile.owner];
            if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center.X + 0, Projectile.Center.Y + 0, 0, 0, ModContent.ProjectileType<DinnerBomb>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
            Projectile.netUpdate = true;
            Projectile.Kill();
            // Increment the bounce counter when a collision occurs.
            // Return false to allow the projectile to continue.
            return true;
        }
    }
}