using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace SariaMod.Items.zPearls
{
    /// <summary>
    /// Visual effect projectile for Ocarina of Time transition.
    /// COPIED FROM RainOcarinaNote PATTERN - each player gets their own projectile.
    /// This projectile handles: visual clocks, time animation, sound, player animation.
    /// </summary>
    public class OcarinaOfTimeNote : ModProjectile
    {
        private const int EFFECT_DURATION = 480; // 8 seconds at 60fps
        private const int COOLDOWN_BUFFER = 360; // 6 seconds at 60fps — blocks ocarina re-use after transition
        private const float DAWN_HOUR = 4.5f; // 4:30 AM
        
        // Transition data stored in ai[] for network sync
        // ai[0] = startHour, ai[1] = targetHour
        private float StartHour => Projectile.ai[0];
        private float TargetHour => Projectile.ai[1];
        
        private int timer = 0;
        
        // Moon phase tracking - captured on first frame
        private int savedEarlyMoonPhase = 0;
        private int savedLateMoonPhase = 0;
        private bool moonPhasesCaptured = false;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Ocarina of Time Effect");
        }

        public override void SetDefaults()
        {
            Projectile.width = 1;
            Projectile.height = 1;
            Projectile.alpha = 255;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = EFFECT_DURATION + COOLDOWN_BUFFER;
            Projectile.ignoreWater = true;
            // Don't sync this projectile - we use the packet system to spawn on other clients
            // This prevents synced projectiles from affecting other players' animations
            Projectile.netImportant = false;
        }

        public override bool? CanHitNPC(NPC target) => false;
        public override bool CanHitPlayer(Player target) => false;
        public override bool CanHitPvp(Player target) => false;

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            
            if (!player.active || player.dead)
            {
                Projectile.Kill();
                return;
            }
            
            Projectile.Center = player.Center;
            timer++;
            
            // Capture moon phases on first frame
            if (!moonPhasesCaptured)
            {
                if (StartHour >= DAWN_HOUR)
                {
                    savedLateMoonPhase = Main.moonPhase;
                    savedEarlyMoonPhase = (Main.moonPhase + 7) % 8;
                }
                else
                {
                    savedEarlyMoonPhase = Main.moonPhase;
                    savedLateMoonPhase = (Main.moonPhase + 1) % 8;
                }
                moonPhasesCaptured = true;
            }
            

            // During cooldown buffer, the projectile stays alive to block
            // IsTimeTransitioning but all visual/audio/animation effects have ended.
            if (timer <= EFFECT_DURATION)
            {
                // Play song on first frame
                if (timer == 1)
                {
                    var soundStyle = new SoundStyle("SariaMod/Sounds/SongOfTime")
                    {
                        Volume = 1f,
                        MaxInstances = 1,
                        SoundLimitBehavior = SoundLimitBehavior.IgnoreNew,
                    };
                    SoundEngine.PlaySound(soundStyle);
                }

                // Only the LOCAL player's projectile should control animations
                // This prevents synced projectiles from other players affecting their animations on our screen
                if (Projectile.owner == Main.myPlayer)
                {
                    // Keep the SOURCE player (who used the ocarina) in "playing ocarina" animation
                    // localAI[0] == 0: This is the source player's projectile, animate the owner (ourselves)
                    // localAI[0] == 1: This is received from network packet, animate the source player stored in localAI[1]
                    if (Projectile.localAI[0] == 0f)
                    {
                        // We are the source player - animate ourselves
                        player.itemAnimation = 2;
                        player.itemAnimationMax = 2;
                        player.itemTime = 2;
                    }
                    else
                    {
                        // We received this from network - animate the source player (stored in localAI[1])
                        int sourcePlayerIndex = (int)Projectile.localAI[1];
                        if (sourcePlayerIndex >= 0 && sourcePlayerIndex < Main.maxPlayers)
                        {
                            Player sourcePlayer = Main.player[sourcePlayerIndex];
                            if (sourcePlayer.active)
                            {
                                sourcePlayer.itemAnimation = 2;
                                sourcePlayer.itemAnimationMax = 2;
                                sourcePlayer.itemTime = 2;
                            }
                        }
                    }
                }

                // Fade music
                Main.musicFade[Main.curMusic] = 0f;

                // Play tick every second
                if (timer % 60 == 0)
                {
                    var tickSound = new SoundStyle("SariaMod/Sounds/ClockTick")
                    {
                        Volume = 0.5f,
                        PitchVariance = 0.05f,
                    };
                    SoundEngine.PlaySound(tickSound);
                }

                // Animate time
                float progress = Math.Min(timer / (float)EFFECT_DURATION, 1f);
                float easedProgress = progress < 0.5f ? 2f * progress * progress : 1f - (float)Math.Pow(-2f * progress + 2f, 2f) / 2f;


                float hourDiff = TargetHour - StartHour;
                float currentHour = StartHour + hourDiff * easedProgress;
                currentHour = Math.Clamp(currentHour, 0f, 24f);

                // Set time locally
                SetGameTimeFromHour(currentHour);

                // Set moon phase based on current animated hour
                if (currentHour < DAWN_HOUR)
                {
                    Main.moonPhase = savedEarlyMoonPhase;
                }
                else
                {
                    Main.moonPhase = savedLateMoonPhase;
                }

                // At end of transition, show message (only for owner)
                if (timer >= EFFECT_DURATION - 1 && Projectile.owner == Main.myPlayer)
                {
                    int h = (int)TargetHour;
                    int m = (int)((TargetHour - h) * 60);
                    string period = h >= 12 ? "PM" : "AM";
                    int displayHour = h % 12;
                    if (displayHour == 0) displayHour = 12;
                    Main.NewText($"Time set to {displayHour}:{m:D2} {period}", 135, 206, 235);
                }
            }
        }
        
        private void SetGameTimeFromHour(float hour)
        {
            hour = Math.Clamp(hour, 0f, 24f);
            if (hour >= 24f) hour = 0f;
            
            if (hour >= 4.5f && hour < 19.5f)
            {
                Main.dayTime = true;
                Main.time = (hour - 4.5f) / 15.0 * 54000.0;
            }
            else
            {
                Main.dayTime = false;
                float nightHour = hour >= 19.5f ? hour - 19.5f : hour + 4.5f;
                Main.time = nightHour / 9.0 * 32400.0;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // Only draw for the local player's projectile
            if (Projectile.owner != Main.myPlayer)
                return false;

            // Don't draw anything during cooldown period (after transition completes)
            if (timer > EFFECT_DURATION)
                return false;

            SpriteBatch spriteBatch = Main.spriteBatch;
            
            // Draw vignette first (behind clocks)
            DrawVignette(spriteBatch);
            
            // Draw clocks on top
            DrawTransitionClocks(spriteBatch);
            
            return false;
        }
        
        private void DrawVignette(SpriteBatch spriteBatch)
        {
            // Create vignette: white/light blue in center -> blue -> black at edges
            Texture2D pixel = Terraria.GameContent.TextureAssets.MagicPixel.Value;
            
            // Get player screen position (center of vignette)
            Player player = Main.player[Projectile.owner];
            Vector2 playerScreenPos = player.Center - Main.screenPosition;
            
            // Calculate max distance from player to screen corners
            float maxDist = Math.Max(
                Math.Max(Vector2.Distance(playerScreenPos, Vector2.Zero),
                         Vector2.Distance(playerScreenPos, new Vector2(Main.screenWidth, 0))),
                Math.Max(Vector2.Distance(playerScreenPos, new Vector2(0, Main.screenHeight)),
                         Vector2.Distance(playerScreenPos, new Vector2(Main.screenWidth, Main.screenHeight)))
            );
            
            // Clear radius around player (no vignette here)
            float clearRadius = 120f;
            float vignetteIntensity = 0.85f;
            
            // Colors for the gradient: white -> light blue -> blue -> black
            Color white = Color.White;
            Color lightBlue = new Color(150, 200, 255);
            Color blue = new Color(50, 100, 200);
            Color black = Color.Black;
            
            // Fade in/out the vignette
            float progress = Math.Min(timer / (float)EFFECT_DURATION, 1f);
            float fadeAlpha = 1f;
            if (progress < 0.1f)
                fadeAlpha = progress / 0.1f;
            else if (progress > 0.9f)
                fadeAlpha = (1f - progress) / 0.1f;
            
            // Draw smooth rings from inside out
            int numRings = 40;
            float ringSpacing = (maxDist - clearRadius) / numRings;
            
            for (int ring = 0; ring < numRings; ring++)
            {
                float innerRadius = clearRadius + ring * ringSpacing;
                float outerRadius = innerRadius + ringSpacing + 5f;
                
                // normalizedDist: 0 = near player, 1 = far from player (at edges)
                float normalizedDist = (float)ring / numRings;
                
                // Color gradient: white -> light blue -> blue -> black
                Color vignetteColor;
                if (normalizedDist < 0.33f)
                {
                    // White to light blue (inner third)
                    float t = normalizedDist / 0.33f;
                    vignetteColor = Color.Lerp(white, lightBlue, t);
                }
                else if (normalizedDist < 0.66f)
                {
                    // Light blue to blue (middle third)
                    float t = (normalizedDist - 0.33f) / 0.33f;
                    vignetteColor = Color.Lerp(lightBlue, blue, t);
                }
                else
                {
                    // Blue to black (outer third)
                    float t = (normalizedDist - 0.66f) / 0.34f;
                    vignetteColor = Color.Lerp(blue, black, t);
                }
                
                // Alpha increases toward edges for darker vignette effect
                float alpha = normalizedDist * vignetteIntensity * fadeAlpha;
                vignetteColor *= alpha;
                
                // Draw ring segments
                int segments = 60;
                for (int i = 0; i < segments; i++)
                {
                    float angle1 = i * MathHelper.TwoPi / segments;
                    float angle2 = (i + 1) * MathHelper.TwoPi / segments;
                    
                    Vector2 inner1 = playerScreenPos + new Vector2((float)Math.Cos(angle1), (float)Math.Sin(angle1)) * innerRadius;
                    Vector2 inner2 = playerScreenPos + new Vector2((float)Math.Cos(angle2), (float)Math.Sin(angle2)) * innerRadius;
                    Vector2 outer1 = playerScreenPos + new Vector2((float)Math.Cos(angle1), (float)Math.Sin(angle1)) * outerRadius;
                    Vector2 outer2 = playerScreenPos + new Vector2((float)Math.Cos(angle2), (float)Math.Sin(angle2)) * outerRadius;
                    
                    float minX = Math.Min(Math.Min(inner1.X, inner2.X), Math.Min(outer1.X, outer2.X));
                    float maxX = Math.Max(Math.Max(inner1.X, inner2.X), Math.Max(outer1.X, outer2.X));
                    float minY = Math.Min(Math.Min(inner1.Y, inner2.Y), Math.Min(outer1.Y, outer2.Y));
                    float maxY = Math.Max(Math.Max(inner1.Y, inner2.Y), Math.Max(outer1.Y, outer2.Y));
                    
                    Rectangle rect = new Rectangle((int)minX, (int)minY, (int)(maxX - minX) + 1, (int)(maxY - minY) + 1);
                    spriteBatch.Draw(pixel, rect, vignetteColor);
                }
            }
        }
        
        private void DrawTransitionClocks(SpriteBatch spriteBatch)
        {
            Texture2D clockTexture = ModContent.Request<Texture2D>("SariaMod/Items/zPearls/ClockOfTime").Value;
            Texture2D handTexture = ModContent.Request<Texture2D>("SariaMod/Items/zPearls/ClockOfTimeHands").Value;
            
            Player localPlayer = Main.player[Main.myPlayer];
            Vector2 playerScreenPos = localPlayer.Center - Main.screenPosition;
            Vector2 screenCenter = new Vector2(Main.screenWidth / 2f, Main.screenHeight / 2f);
            
            float progress = Math.Min(timer / (float)EFFECT_DURATION, 1f);
            float easedProgress = progress < 0.5f ? 2f * progress * progress : 1f - (float)Math.Pow(-2f * progress + 2f, 2f) / 2f;
            
            float hourDiff = TargetHour - StartHour;
            float currentHour = StartHour + hourDiff * easedProgress;
            while (currentHour < 0) currentHour += 24f;
            while (currentHour >= 24f) currentHour -= 24f;
            
            float direction = hourDiff >= 0 ? 1f : -1f;
            
            // Fade in/out
            float fadeAlpha = 1f;
            if (progress < 0.1f)
                fadeAlpha = progress / 0.1f;
            else if (progress > 0.9f)
                fadeAlpha = (1f - progress) / 0.1f;
            
            // Main clock
            float mainClockScale = 3.0f;
            Vector2 mainClockOrigin = new Vector2(clockTexture.Width / 2f, clockTexture.Height / 2f);
            Vector2 mainHandOrigin = new Vector2(handTexture.Width / 2f, handTexture.Height / 2f);
            float mainHandRotation = currentHour / 24f * MathHelper.TwoPi;
            
            spriteBatch.Draw(clockTexture, playerScreenPos, null, Color.White * (0.5f * fadeAlpha), 0f, mainClockOrigin, mainClockScale, SpriteEffects.None, 0f);
            spriteBatch.Draw(handTexture, playerScreenPos, null, Color.White * (0.6f * fadeAlpha), mainHandRotation, mainHandOrigin, mainClockScale, SpriteEffects.None, 0f);
            
            // Smaller clocks spiraling
            int numSmallClocks = 8;
            float smallClockScale = 2.5f;
            float spiralRadius = 140f;
            float spiralRotation = timer * 0.02f * direction;
            
            for (int i = 0; i < numSmallClocks; i++)
            {
                float angleOffset = (i / (float)numSmallClocks) * MathHelper.TwoPi + spiralRotation;
                Vector2 clockPos = playerScreenPos + new Vector2(
                    (float)Math.Cos(angleOffset) * spiralRadius,
                    (float)Math.Sin(angleOffset) * spiralRadius
                );
                
                float clockHour = currentHour - (i + 1) * direction;
                while (clockHour < 0) clockHour += 24f;
                while (clockHour >= 24f) clockHour -= 24f;
                float handRotation = clockHour / 24f * MathHelper.TwoPi;
                
                Vector2 clockOrigin = new Vector2(clockTexture.Width / 2f, clockTexture.Height / 2f);
                Vector2 handOrigin = new Vector2(handTexture.Width / 2f, handTexture.Height / 2f);
                
                spriteBatch.Draw(clockTexture, clockPos, null, Color.White * fadeAlpha, 0f, clockOrigin, smallClockScale, SpriteEffects.None, 0f);
                spriteBatch.Draw(handTexture, clockPos, null, Color.White * fadeAlpha, handRotation, handOrigin, smallClockScale, SpriteEffects.None, 0f);
            }
            
            // Text
            string text = "Time is shifting...";
            Vector2 textSize = Terraria.GameContent.FontAssets.MouseText.Value.MeasureString(text);
            Vector2 textPos = screenCenter - textSize / 2 + new Vector2(0, -180);
            Terraria.Utils.DrawBorderString(spriteBatch, text, textPos, Color.Cyan * fadeAlpha, 1.5f);
        }
        
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, 
            List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            // Draw behind players so player is visible
            behindProjectiles.Add(index);
        }
    }
}
