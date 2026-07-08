using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Utilities;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using SariaMod.Netcode.HookshotNetworking;
using SariaMod.Buffs;

namespace SariaMod.Items.Bands
{
    /// <summary>
    /// Base class for Hookshot/Longshot dash hitboxes
    /// Normal: 1 second invulnerability after catching hook
    /// Sweetspot: 2 seconds invulnerability after catching hook, double duration hitbox
    /// </summary>
    public abstract class BaseHookshotDashHitbox : ModProjectile
    {
        protected const int NormalLifeTime = 60;    // 1 second for normal dash
        protected const int SweetspotLifeTime = 120; // 2 seconds for sweetspot dash

        // ai[0] = 1 if spawned from combat mode dash-through (timer paused until player reaches target)
        private bool IsCombatModeDash
        {
            get => Projectile.ai[0] == 1f;
            set => Projectile.ai[0] = value ? 1f : 0f;
        }

        // ai[1] = 1 if this is a sweetspot hit (double damage, 2x duration, orange+blue dust)
        private bool IsSweetspotDash
        {
            get => Projectile.ai[1] == 1f;
            set => Projectile.ai[1] = value ? 1f : 0f;
        }

        // localAI[0] = 1 when timer should start (player has dashed through)
        private bool TimerStarted
        {
            get => Projectile.localAI[0] == 1f;
            set => Projectile.localAI[0] = value ? 1f : 0f;
        }

        private int GetLifeTime() => IsSweetspotDash ? SweetspotLifeTime : NormalLifeTime;

        public override string Texture => "SariaMod/Items/Bands/HookshotHook";

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1; // Infinite pierce
            Projectile.tileCollide = false;
            Projectile.timeLeft = 9999; // Don't use timeLeft, we manage lifetime manually
            Projectile.aiStyle = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
            Projectile.alpha = 255;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            if (!player.active || player.dead)
            {
                Projectile.Kill();
                return;
            }

            Projectile.Center = player.Center;

            // Grant invulnerability during the dash (before timer starts) for both normal and sweetspot
            if (IsCombatModeDash && !TimerStarted)
            {
                player.immune = true;
                player.immuneTime = 10;
                player.immuneNoBlink = true;
            }

            // For combat mode dash, check if player has reached the hook target (timer starts then)
            if (IsCombatModeDash && !TimerStarted)
            {
                // Check if player's hook is still pulling (timer hasn't started yet)
                bool hasActiveHook = player.ownedProjectileCounts[ModContent.ProjectileType<HookshotProjectile>()] > 0 ||
                                     player.ownedProjectileCounts[ModContent.ProjectileType<LongshotProjectile>()] > 0;

                if (!hasActiveHook)
                {
                    // Hook is gone, player dashed through - start the timer with appropriate duration
                    TimerStarted = true;
                    Projectile.timeLeft = GetLifeTime();
                }
            }
            else if (!IsCombatModeDash)
            {
                // Grapple mode - timer runs immediately
                if (!TimerStarted)
                {
                    TimerStarted = true;
                    Projectile.timeLeft = GetLifeTime();
                }
            }

            // Grant invulnerability while dash hitbox is active (after timer starts)
            if (TimerStarted && Projectile.timeLeft > 0)
            {
                player.immune = true;
                player.immuneTime = 10;
                player.immuneNoBlink = true;

                // Sweetspot mirage/afterimage effect continues after dash
                if (IsSweetspotDash)
                {
                    player.armorEffectDrawShadow = true;
                    player.armorEffectDrawOutlines = true;
                }
            }

            // Kill when timer expires (only if timer has started)
            if (TimerStarted && Projectile.timeLeft <= 0)
            {
                Projectile.Kill();
                return;
            }

            // Spawn dust particles
            if (Main.rand.NextBool(3))
            {
                Vector2 dustPos = player.Center + Main.rand.NextVector2Circular(20f, 20f);

                // Blue electric dust (always)
                Dust d = Dust.NewDustPerfect(dustPos, DustID.Electric, Vector2.Zero, 100, default, 0.5f);
                d.noGravity = true;
                d.fadeIn = 0.5f;

                // Orange dust (only for sweetspot)
                if (IsSweetspotDash)
                {
                    Vector2 orangeDustPos = player.Center + Main.rand.NextVector2Circular(20f, 20f);
                    Dust orangeDust = Dust.NewDustPerfect(orangeDustPos, DustID.Torch, Vector2.Zero, 100, default, 0.8f);
                    orangeDust.noGravity = true;
                    orangeDust.fadeIn = 0.5f;
                }
            }
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            // Blue electric dust (always)
            for (int i = 0; i < 5; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2CircularEdge(2f, 2f);
                Dust.NewDust(target.Center, 0, 0, DustID.Electric, dustVel.X, dustVel.Y, 100, default, 1f);
            }

            // Orange torch dust (only for sweetspot)
            if (IsSweetspotDash)
            {
                for (int i = 0; i < 5; i++)
                {
                    Vector2 dustVel = Main.rand.NextVector2CircularEdge(3f, 3f);
                    Dust.NewDust(target.Center, 0, 0, DustID.Torch, dustVel.X, dustVel.Y, 100, default, 1.2f);
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            return false;
        }
    }
}
