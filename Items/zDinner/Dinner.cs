using Microsoft.Xna.Framework; // Add this using directive for Vector2
using SariaMod.Items.zDinner; // Add this using directive for SplitDinner
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
namespace SariaMod.Items.zDinner
{
    public class Dinner : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Dinner");
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
            base.Projectile.tileCollide = true; // CHANGE: Set to true to enable collision
            base.Projectile.netImportant = true;
            base.Projectile.penetrate = -1;
            base.Projectile.timeLeft = 3500;
            base.Projectile.ignoreWater = true;
            base.Projectile.usesLocalNPCImmunity = true;
            base.Projectile.localNPCHitCooldown = 4;
        }
        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            Player player = Main.player[Projectile.owner];
            FairyPlayer modPlayer = player.Fairy();
            if ((player.ownedProjectileCounts[ModContent.ProjectileType<SplitDinner>()] >= 3f))
            {
                if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + 0, 0, 0, ModContent.ProjectileType<DinnerBomb>(), (int)(Projectile.damage), 20f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                Projectile.Kill();
            }
            else
            {
                if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + 0, 0, 0, ModContent.ProjectileType<DinnerBomb>(), (int)(Projectile.damage), 20f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
            }
        }
        public override void AI()
        {
            // The spinning logic can stay as-is
            base.Projectile.rotation += 0.095f;
            // Increment the AI timer
            Projectile.ai[0]++;
            if (Projectile.timeLeft == 350)
            {
                SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/ohboy"), Projectile.Center);
            }
            // Define flight parameters
            const float initialCurveTime = 60f; // Time in frames for the initial curve
            const float gravity = 0.2f;       // Strength of the gravity effect
            // --- AI PHASE LOGIC ---
            if (Projectile.ai[0] < initialCurveTime)
            {
                // PHASE 1: Initial Curve
                // Gradually slow the projectile down and apply a side curve.
                Projectile.velocity.X *= 0.98f;
                Projectile.velocity.Y *= 0.98f;
                // Add a small horizontal adjustment for the curve.
                // You can adjust the direction and strength of the curve.
                // Here, it curves based on its initial horizontal direction.
                Projectile.velocity.X += (0.05f * Projectile.direction);
            }
            else
            {
                // PHASE 2: Gravity-induced Fall
                // After the initial curve, start applying gravity.
                Projectile.velocity.Y += gravity;
                // Cap the falling speed to prevent it from getting too fast.
                if (Projectile.velocity.Y > 16f)
                {
                    Projectile.velocity.Y = 16f;
                }
            }
        }
        // --- ADDED: OnTileCollide hook ---
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Player player = Main.player[base.Projectile.owner];
            SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/JustDinner"), player.Center);
            Lighting.AddLight(Projectile.Center, Color.White.ToVector3() * .5f);
            if ((player.ownedProjectileCounts[ModContent.ProjectileType<SplitDinner>()] <= 3f))
            {
                if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + 0, 0, 0, ModContent.ProjectileType<SplitDinner>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
            }
            else
            {
                if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + 0, 0, 0, ModContent.ProjectileType<DinnerBomb>(), (int)(Projectile.damage), 20f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
            }
            return true;
        }
    }
}
