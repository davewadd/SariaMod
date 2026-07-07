using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;

namespace SariaMod.Items.Bands
{
    /// <summary>
    /// Combat mode damager projectile for Hookshot/Longshot
    /// This projectile follows the owner's hook and deals damage to the hooked enemy
    /// It is larger than the hook itself and handles all combat damage
    /// 
    /// Damage: CombatDamage / 2 (Hookshot = 25, Longshot = 50)
    /// Deals damage every 0.5 seconds while attached
    /// </summary>
    public class HookDamager : ModProjectile
    {
        // ai[0] = target NPC index
        // ai[1] = isLongshot (0 = hookshot, 1 = longshot)
        
        private int TargetNPCIndex
        {
            get => (int)Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }
        
        private bool IsLongshot
        {
            get => Projectile.ai[1] == 1f;
            set => Projectile.ai[1] = value ? 1f : 0f;
        }
        
        private const int DamageInterval = 30; // Deal damage every 0.5 seconds

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Hook Strike");
        }

        public override void SetDefaults()
        {
            Projectile.width = 48;
            Projectile.height = 48;
            Projectile.alpha = 255; // Invisible
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.tileCollide = false;
            Projectile.netImportant = true;
            Projectile.penetrate = -1; // Infinite pierce
            Projectile.timeLeft = 600; // 10 seconds max
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = DamageInterval;
            Projectile.aiStyle = -1;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            
            // Kill if owner is dead or inactive
            if (!owner.active || owner.dead)
            {
                Projectile.Kill();
                return;
            }

            // Validate target NPC
            if (TargetNPCIndex < 0 || TargetNPCIndex >= Main.maxNPCs)
            {
                Projectile.Kill();
                return;
            }

            NPC target = Main.npc[TargetNPCIndex];

            // If target died (from ANY source - other player, trap, summon, etc.), kill this damager
            if (!target.active || target.life <= 0)
            {
                Projectile.Kill();
                return;
            }

            // Find the owner's hook projectile
            Projectile hookProj = HookshotHelper.FindOwnerHookProjectile(Projectile.owner);

            // If no hook exists, kill this damager
            if (hookProj == null)
            {
                Projectile.Kill();
                return;
            }

            // Check if hook is still attached to target
            if (!HookshotHelper.IsHookAttachedToNPC(hookProj, TargetNPCIndex))
            {
                Projectile.Kill();
                return;
            }

            // Follow the hook's center (not the target, the HOOK)
            Projectile.Center = hookProj.Center;
            Projectile.velocity = Vector2.Zero;

            // Spawn visual dust occasionally
            if (Main.rand.NextBool(5))
            {
                Vector2 dustPos = Projectile.Center + Main.rand.NextVector2Circular(24f, 24f);
                Dust d = Dust.NewDustPerfect(dustPos, DustID.Electric, Vector2.Zero, 100, default, 0.6f);
                d.noGravity = true;
                d.fadeIn = 0.3f;
            }
        }

        public override bool? CanHitNPC(NPC target)
        {
            // Only hit the target we're attached to
            if (target.whoAmI == TargetNPCIndex)
            {
                return true;
            }
            return false;
        }

        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            // No knockback - enemy is frozen by hookshot
            knockback = 0f;
            
            // Set hit direction based on player position
            Player owner = Main.player[Projectile.owner];
            hitDirection = target.Center.X > owner.Center.X ? 1 : -1;
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            // Visual feedback on hit
            for (int i = 0; i < 5; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2CircularEdge(2f, 2f);
                Dust.NewDust(target.Center, 0, 0, DustID.Electric, dustVel.X, dustVel.Y, 100, default, 1f);
            }
        }

        public override bool PreDraw(ref Microsoft.Xna.Framework.Color lightColor)
        {
            // Invisible - don't draw anything
            return false;
        }
    }
}
