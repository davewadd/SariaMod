using Microsoft.Xna.Framework;
using SariaMod.Buffs;
using SariaMod.Dusts;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using SariaMod.Items.Amber;
using SariaMod.Items.Amethyst;
using SariaMod.Items.Bands;
using SariaMod.Items.Emerald;
using SariaMod.Items.Ruby;
using SariaMod.Items.Sapphire;
using SariaMod.Items.Topaz;
using System;
using System.IO;
using SariaMod.Items.Strange;
using SariaMod.Netcode;
namespace SariaMod.Items.Sapphire
{
    public class HealCursorVisual : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Saria");
            ProjectileID.Sets.TrailCacheLength[base.Projectile.type] = 7;
            ProjectileID.Sets.TrailingMode[base.Projectile.type] = 0;
        }
        public override void SetDefaults()
        {
            base.Projectile.width = 1;
            base.Projectile.height = 1;
            base.Projectile.alpha = 300;
            base.Projectile.friendly = true;
            base.Projectile.tileCollide = false;
            base.Projectile.netImportant = true;
            base.Projectile.penetrate = 1;
            base.Projectile.timeLeft = 500;
            base.Projectile.ignoreWater = true;
            base.Projectile.usesLocalNPCImmunity = true;
            base.Projectile.localNPCHitCooldown = 4;
        }
        public override bool? CanHitNPC(NPC target)
        {
            return false;
        }
        public override bool? CanCutTiles()
        {
            return false;
        }
        public override void AI()
        {
            Player player = Main.player[base.Projectile.owner];
            Player player2 = Main.LocalPlayer;
            FairyPlayer modPlayer = player.Fairy();
            Lighting.AddLight(base.Projectile.Center, 0f, 0.5f, 0f);
            int Yesh = ((player2.statManaMax2) / 8);
            int Yesh2 = ((player2.statManaMax2) / 5);
            Projectile mother = Main.projectile[(int)base.Projectile.ai[1]];
            if (player.ownedProjectileCounts[ModContent.ProjectileType<HealBarrier>()] > 0f)
            {
                Projectile.timeLeft = 500;
            }
            
            // Dust effect (runs on all clients)
            for (int num189 = 0; num189 < 1; num189++)
            {
                int num190 = Dust.NewDust(new Vector2(base.Projectile.position.X, base.Projectile.position.Y), 0, 0, 107);
                Main.dust[num190].velocity *= 0.5f;
                Main.dust[num190].scale *= 1.3f;
                Main.dust[num190].fadeIn = 1f;
                Main.dust[num190].noGravity = true;
            }
            
            // The owner and single-player use the live mouse exactly as before.
            // Multiplayer peers use the shared cursor stream so the server gets
            // exact input while observers get its smoothed presentation.
            SariaCursorNetworking.PublishLocalCursor(Projectile);
            Vector2 mouse = Vector2.Zero;
            bool hasCursor = Projectile.owner == Main.myPlayer;
            if (hasCursor)
            {
                mouse = Main.MouseWorld;
            }
            else if (Main.netMode != NetmodeID.SinglePlayer)
            {
                hasCursor = SariaCursorNetworking.TryGetCursor(Projectile.owner, out mouse);
            }

            if (hasCursor)
            {
                mouse.X += 0f;
                mouse.Y -= 5f;
                
                Vector2 direction = mouse - Projectile.Center;
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
                
            }
        }
    }
}
