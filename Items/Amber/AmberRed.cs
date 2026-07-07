using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
namespace SariaMod.Items.Amber
{
    public class AmberRed : ModProjectile
    {
        public override void SetDefaults()
        {
            base.Projectile.width = 42;
            base.Projectile.height = 40;
            base.Projectile.hostile = false;
            base.Projectile.friendly = false;
            base.Projectile.ignoreWater = true;
            base.Projectile.timeLeft = 340;
            base.Projectile.penetrate = -1;
            base.Projectile.tileCollide = false;
            base.Projectile.minion = false;
            base.Projectile.localNPCHitCooldown = 5;
            base.Projectile.minionSlots = 0f;
            base.Projectile.netImportant = true;
            base.Projectile.usesLocalNPCImmunity = true;
        }
        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Psychic Turret");
            Main.projFrames[base.Projectile.type] = 2;
            Main.projPet[Projectile.type] = true;
            ProjectileID.Sets.MinionSacrificable[base.Projectile.type] = false;
            ProjectileID.Sets.MinionTargettingFeature[base.Projectile.type] = true;
        }
        public override bool? CanDamage()/* tModPorter Suggestion: Return null instead of true */
        {
            return false;
        }
        public override void AI()
        {
            Player player = Main.player[base.Projectile.owner];
            FairyPlayer modPlayer = player.Fairy();
            if (player.dead || !player.active)
            {
                Projectile.Kill();
            }
            Projectile mother = Main.projectile[(int)base.Projectile.ai[1]];
            if (!mother.active)
            {
                base.Projectile.Kill();
                return;
            }
            if (Projectile.timeLeft <= 150)
            {
                Projectile.frame = 1;
            }
            if (Projectile.timeLeft == 150)
            {
                SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact, base.Projectile.Center);
            }
            float speed = 8f;
            float inertia = 20f;
            for (int i = 0; i < 200; i++)
            {
                NPC target = Main.npc[i];
                float shootToX = target.position.X + (float)target.width * 0.5f - base.Projectile.Center.X;
                float shootToY = target.position.Y + (float)target.height * 0.5f - base.Projectile.Center.Y;
                float distance = (float)Math.Sqrt(shootToX * shootToX + shootToY * shootToY);
                if (distance < 1020f && target.catchItem == 0 && !target.friendly && Collision.CanHitLine(base.Projectile.position, base.Projectile.width, base.Projectile.height, target.position, target.width, target.height) && target.active && target.type != 488 && base.Projectile.ai[0] > 60f)
                {
                    distance = 1.6f / distance;
                    shootToX *= distance * 3f;
                    shootToY *= distance * 3f;
                    base.Projectile.ai[0] = 0f;
                }
            }
            Vector2 idlePosition = mother.Center;
            idlePosition.Y -= 48f; // Go up 48 coordinates (three tiles from the center of the player)
            // If your minion doesn't aimlessly move around when it's idle, you need to "put" it into the line of other summoned minions
            // The index is projectile.minionPos
            float minionPositionOffsetX = (10 + Projectile.minionPos * 40) * -player.direction;
            idlePosition.X += minionPositionOffsetX; // Go behind the player
            // All of this code below this line is adapted from Spazmamini code (ID 388, aiStyle 66)
            // Teleport to player if distance is too big
            Vector2 vectorToIdlePosition = idlePosition - Projectile.Center;
            float distanceToIdlePosition = vectorToIdlePosition.Length();
            if (Main.myPlayer == player.whoAmI && distanceToIdlePosition > 2000f)
            {
                // Whenever you deal with non-regular events that change the behavior or position drastically, make sure to only run the code on the owner of the projectile,
                // and then set netUpdate to true
                Projectile.position = idlePosition;
                Projectile.velocity *= 0.1f;
                Projectile.netUpdate = true;
            }
            // If your minion is flying, you want to do this independently of any conditions
            float overlapVelocity = 0.04f;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                // Fix overlap with other minions
                Projectile other = Main.projectile[i];
                if (i != Projectile.whoAmI && other.active && other.owner == Projectile.owner && Math.Abs(Projectile.position.X - other.position.X) + Math.Abs(Projectile.position.Y - other.position.Y) < Projectile.width)
                {
                    if (Projectile.position.X < other.position.X) Projectile.velocity.X -= overlapVelocity;
                    else Projectile.velocity.X += overlapVelocity;
                    if (Projectile.position.Y < other.position.Y) Projectile.velocity.Y -= overlapVelocity;
                    else Projectile.velocity.Y += overlapVelocity;
                }
            }
            Vector2 idlePosition2 = player.Center;
            idlePosition2.X += minionPositionOffsetX;
            // Default movement parameters (here for attacking)
            Projectile.rotation = Projectile.velocity.X * 0.05f;
            Projectile.velocity.X = mother.velocity.X;
            {
                // Minion doesn't have a target: return to player and idle
                if (distanceToIdlePosition > 450f)
                {
                    // Speed up the minion if it's away from the player
                    speed = 30f;
                    inertia = 60f;
                }
                if (distanceToIdlePosition > 400f)
                {
                    // Speed up the minion if it's away from the player
                    speed = 8f;
                    inertia = 60f;
                }
                else
                {
                    // Slow down the minion if closer to the player
                    speed = 4f;
                    inertia = 80f;
                }
                if (distanceToIdlePosition > 20f)
                {
                    // The immediate range around the player (when it passively floats about)
                    // This is a simple movement formula using the two parameters and its desired direction to create a "homing" movement
                    vectorToIdlePosition.Normalize();
                    vectorToIdlePosition *= speed;
                    Projectile.velocity = (Projectile.velocity * (inertia - 1) + vectorToIdlePosition) / inertia;
                }
            }
            if (Projectile.velocity.X >= 0)
            {
                Projectile.spriteDirection = 1;
            }
            if (Projectile.velocity.X <= -0)
            {
                Projectile.spriteDirection = -1;
            }
            if (Projectile.timeLeft == 10)
            {
                for (int j = 0; j < 5; j++) //set to 2
                {
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + Utils.RandomVector2(Main.rand, 0f, 0f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ProjectileType<AmberShard>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                }
                SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact, base.Projectile.Center);
                for (int j = 0; j < 1; j++) //set to 2
                {
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + 0, 0, 0, ModContent.ProjectileType<RedMoth>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                }
            }
            Lighting.AddLight(Projectile.Center, Color.Orange.ToVector3() * 1f);
        }
    }
}
