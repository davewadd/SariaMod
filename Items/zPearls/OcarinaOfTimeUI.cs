using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using System.Collections.Generic;
using ReLogic.Utilities;

namespace SariaMod.Items.zPearls
{
    /// <summary>
    /// Clock UI projectile for the Ocarina of Time.
    /// Displays a clock where players can drag the hand to select a time.
    /// Single day (12 AM - 11:59 PM) with moon phase preservation.
    /// </summary>
    public class OcarinaOfTimeUI : ModProjectile
    {
        // Constants
        private const float CLOCK_SCALE = 8.0f;
        private const float HAND_TIP_DISTANCE = 120f;
        private const float BUTTON_SIZE = 40f;
        private const float VIGNETTE_INTENSITY = 0.7f;
        private const float HAND_GRAB_RADIUS = 30f;
        private const int TRANSITION_DURATION = 480; // 8 seconds at 60fps
        private const float DAWN_HOUR = 4.5f; // 4:30 AM - when moon phase advances
        
        // Static property to check if time is transitioning (for preventing multiple uses)
        public static bool IsTimeTransitioning 
        { 
            get 
            {
                // Check if any OcarinaOfTimeNote projectile is active
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    if (Main.projectile[i].active && 
                        Main.projectile[i].type == ModContent.ProjectileType<OcarinaOfTimeNote>())
                    {
                        return true;
                    }
                }
                return false;
            }
        }
        
        // State tracking
        private bool isDragging = false;
        private float selectedHour = 0f; // 0-24 hours
        private float targetHour = 0f;
        private float startingHour = 0f;
        private int uiOpenTimer = 0; // Delay before buttons become active
        private const int UI_ACTIVATION_DELAY = 15; // 15 frames before buttons work
        
        
        // Track previous mouse angle for directional dragging
        private float previousMouseAngle = 0f;
        private bool hasValidPreviousAngle = false;
        
        // Clock tick sound tracking during dragging - very slow tick rate
        private float lastTickHour = 0f;
        private const float TICK_INTERVAL = 2.0f; // Play tick every 2 hours of clock movement (much less spammy)
        
        // Transition clock tick tracking
        private int lastTransitionTickFrame = 0;
        private const int TRANSITION_TICK_INTERVAL = 60; // Tick every second during transition
        
        // Moon phase tracking (to prevent infinite cycling)
        // earlyPhase = moon phase for 12:00 AM to 4:30 AM (before dawn)
        // latePhase = moon phase for 4:30 AM to 11:59 PM (after dawn)
        private int savedEarlyMoonPhase = 0;
        private int savedLateMoonPhase = 0;
        
        
        // Button states
        private bool confirmHovered = false;
        private bool exitHovered = false;
        
        // Screen center for UI positioning
        private Vector2 ScreenCenter => new Vector2(Main.screenWidth / 2f, Main.screenHeight / 2f);
        
        
        
        
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Ocarina of Time Clock");
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
            Projectile.timeLeft = 36000; // 10 minutes max
            Projectile.ignoreWater = true;
            // Don't sync this UI projectile - it's local to the player using it
            Projectile.netImportant = false;
        }

        public override bool? CanHitNPC(NPC target) => false;
        public override bool CanHitPlayer(Player target) => false;
        public override bool CanHitPvp(Player target) => false;

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(selectedHour);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            selectedHour = reader.ReadSingle();
        }



        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            
            
            // Keep projectile at player's center
            Projectile.Center = player.Center;
            
            // Initialize on first frame
            if (Projectile.localAI[0] == 0f)
            {
                startingHour = GetCurrentGameHour();
                selectedHour = startingHour;
                lastTickHour = startingHour; // Initialize tick tracking
                
                // Capture moon phases for the current calendar day
                // Moon phase advances at DAWN (4:30 AM), not dusk
                // earlyPhase = 12:00 AM to 4:30 AM (the overnight phase, before dawn)
                // latePhase = 4:30 AM to 11:59 PM (the new phase, after dawn)
                const float DAWN_HOUR = 4.5f; // 4:30 AM
                
                if (startingHour < DAWN_HOUR)
                {
                    // We're before dawn (12 AM - 4:30 AM), current phase is the "early/overnight" phase
                    savedEarlyMoonPhase = Main.moonPhase;
                    savedLateMoonPhase = (Main.moonPhase + 1) % 8; // Phase after dawn
                }
                else
                {
                    // We're after dawn (4:30 AM - 11:59 PM), current phase is the "late/day" phase
                    savedLateMoonPhase = Main.moonPhase;
                    savedEarlyMoonPhase = (Main.moonPhase + 7) % 8; // Previous phase (before dawn)
                }
                
                Projectile.localAI[0] = 1f;
            }
            
            // Increment UI open timer
            uiOpenTimer++;
            
            // Only process input for local player
            if (Main.myPlayer != Projectile.owner)
                return;
            
            // Kill if player is dead or inactive
            if (!player.active || player.dead)
            {
                Projectile.Kill();
                return;
            }
            
            // Don't freeze player - let them move freely while using the clock UI
            // Only prevent item use to avoid conflicts
            player.noItems = true;
            player.noThrow = 2;
            
            // Process mouse input (only after delay to prevent accidental clicks)
            if (uiOpenTimer >= UI_ACTIVATION_DELAY)
            {
                ProcessMouseInput();
            }
        }

        private float GetCurrentGameHour()
        {
            // Terraria time: 0 = 4:30 AM, 54000 = 4:30 AM next day
            // dayTime: true = 4:30 AM to 7:30 PM, false = 7:30 PM to 4:30 AM
            double time = Main.time;
            float hour;
            
            if (Main.dayTime)
            {
                // Day: 4:30 AM to 7:30 PM (15 hours)
                // 54000 ticks = 15 hours
                hour = 4.5f + (float)(time / 54000.0 * 15.0);
            }
            else
            {
                // Night: 7:30 PM to 4:30 AM (9 hours)
                // 32400 ticks = 9 hours
                hour = 19.5f + (float)(time / 32400.0 * 9.0);
                if (hour >= 24f)
                    hour -= 24f;
            }
            
            return hour;
        }

        private void SetGameTimeFromHour(float hour)
        {
            // Clamp hour to 0-24
            hour = Math.Clamp(hour, 0f, 24f);
            if (hour >= 24f) hour = 0f;
            
            // Convert hour to Terraria time
            if (hour >= 4.5f && hour < 19.5f)
            {
                // Daytime (4:30 AM to 7:30 PM)
                Main.dayTime = true;
                Main.time = (hour - 4.5f) / 15.0 * 54000.0;
            }
            else
            {
                // Nighttime (7:30 PM to 4:30 AM)
                Main.dayTime = false;
                float nightHour = hour >= 19.5f ? hour - 19.5f : hour + 4.5f;
                Main.time = nightHour / 9.0 * 32400.0;
            }
        }





        private void ProcessMouseInput()
        {
            Vector2 mouseScreen = new Vector2(Main.mouseX, Main.mouseY);
            Vector2 clockCenter = ScreenCenter;
            
            // Button positions - fixed distances from clock center
            Vector2 confirmPos = clockCenter + new Vector2(0, 200); // Below clock
            Vector2 exitPos = clockCenter + new Vector2(180, -180); // Top-right of clock
            
            // Check hover states
            confirmHovered = Vector2.Distance(mouseScreen, confirmPos) < BUTTON_SIZE;
            exitHovered = Vector2.Distance(mouseScreen, exitPos) < BUTTON_SIZE / 2;
            
            // Handle exit button - play Close sound
            if (exitHovered && Main.mouseLeft && Main.mouseLeftRelease)
            {
                SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Close"));
                Projectile.Kill();
                return;
            }
            
            // Handle confirm button
            if (confirmHovered && Main.mouseLeft && Main.mouseLeftRelease)
            {
                StartTimeTransition();
                return;
            }
            
            // Handle clock hand dragging
            Vector2 toMouse = mouseScreen - clockCenter;
            float distToCenter = toMouse.Length();
            
            // Calculate hand tip position
            float handAngle = HourToAngle(selectedHour);
            Vector2 handTip = clockCenter + new Vector2(
                (float)Math.Sin(handAngle) * HAND_TIP_DISTANCE,
                -(float)Math.Cos(handAngle) * HAND_TIP_DISTANCE
            );
            
            float distToHandTip = Vector2.Distance(mouseScreen, handTip);
            
            // Calculate current mouse angle from clock center
            float currentMouseAngle = (float)Math.Atan2(toMouse.X, -toMouse.Y);
            if (currentMouseAngle < 0) currentMouseAngle += MathHelper.TwoPi;
            
            if (Main.mouseLeft)
            {
                // Start dragging if clicking near hand tip or already dragging
                if (distToHandTip < HAND_GRAB_RADIUS || isDragging)
                {
                    if (!isDragging)
                    {
                        // Just started dragging - initialize the previous angle
                        isDragging = true;
                        previousMouseAngle = currentMouseAngle;
                        hasValidPreviousAngle = true;
                        lastTickHour = selectedHour; // Reset tick tracking when starting drag
                    }
                    else if (hasValidPreviousAngle && distToCenter > 30f && distToCenter < 300f)
                    {
                        // Calculate the angular difference (how much the mouse moved around the clock)
                        float angleDelta = currentMouseAngle - previousMouseAngle;
                        
                        // Handle wrapping around (crossing 0/2PI boundary)
                        if (angleDelta > MathHelper.Pi) angleDelta -= MathHelper.TwoPi;
                        if (angleDelta < -MathHelper.Pi) angleDelta += MathHelper.TwoPi;
                        
                        // Convert angle delta to hour delta
                        // Full circle (2PI) = 24 hours
                        float hourDelta = angleDelta / MathHelper.TwoPi * 24f;
                        
                        // Apply the delta to selected hour
                        selectedHour += hourDelta;
                        
                        
                        // Wrap around the day (0-24)
                        while (selectedHour < 0) selectedHour += 24f;
                        while (selectedHour >= 24f) selectedHour -= 24f;
                        
                        // Clamp to valid range
                        selectedHour = Math.Clamp(selectedHour, 0.01f, 23.99f);
                        
                        // Play clock tick sound based on movement (like a fast clock, not spammy)
                        float hoursMoved = Math.Abs(selectedHour - lastTickHour);
                        // Handle wrap-around for tick calculation
                        if (hoursMoved > 12f) hoursMoved = 24f - hoursMoved;
                        
                        if (hoursMoved >= TICK_INTERVAL)
                        {
                            // Play tick sound - quieter, like a clock ticking fast
                            var tickSound = new SoundStyle("SariaMod/Sounds/ClockTick")
                            {
                                Volume = 0.35f,
                                PitchVariance = 0.05f,
                                MaxInstances = 2,
                            };
                            SoundEngine.PlaySound(tickSound);
                            lastTickHour = selectedHour;
                        }
                    }
                    
                    // Update previous angle for next frame
                    previousMouseAngle = currentMouseAngle;
                }
            }
            else
            {
                isDragging = false;
                hasValidPreviousAngle = false;
            }
        }


        private float HourToAngle(float hour)
        {
            // 12 o'clock = 0 radians (pointing up)
            // Clockwise rotation: 6 hours = PI/2, 12 hours = PI, 18 hours = 3PI/2
            return hour / 24f * MathHelper.TwoPi;
        }

        private void StartTimeTransition()
        {
            // COPIED FROM RainOcarina PATTERN EXACTLY
            targetHour = selectedHour;
            startingHour = GetCurrentGameHour();
            
            Player player = Main.player[Projectile.owner];
            
            // Spawn the visual effect projectile on LOCAL player first (like RainOcarina)
            Projectile.NewProjectile(
                player.GetSource_FromThis(),
                player.Center,
                Vector2.Zero,
                ModContent.ProjectileType<OcarinaOfTimeNote>(),
                0,
                0f,
                player.whoAmI,
                startingHour,  // ai[0] = startHour
                targetHour     // ai[1] = targetHour
            );
            
            // Send packet to server to broadcast to other players (like RainOcarina)
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                ModPacket effectPacket = SariaMod.Instance.GetPacket();
                effectPacket.Write((byte)SariaMod.SoundMessageType.SyncTimeTransition);
                effectPacket.Write(startingHour);
                effectPacket.Write(targetHour);
                effectPacket.Write((byte)player.whoAmI); // Send source player ID
                effectPacket.Send();
            }
            
            // Kill the UI - the Note projectile handles everything now
            Projectile.Kill();
        }
        
        
        
        /// <summary>
        /// NOT USED ANYMORE - kept for compatibility. Effect is handled by OcarinaOfTimeNote projectile.
        /// </summary>
        // NOT USED - Kept for API compatibility but effect is now handled by OcarinaOfTimeNote projectile
        public static void StartRemoteTransitionEffect(float startHour, float targetHour, int duration, int sourcePlayer) { }
        public static void UpdateRemoteTransition() { }
        public static void DrawRemoteTransitionEffect(SpriteBatch spriteBatch) { }

        private string FormatHour(float hour)
        {
            int h = (int)hour;
            int m = (int)((hour - h) * 60);
            string period = h >= 12 ? "PM" : "AM";
            int displayHour = h % 12;
            if (displayHour == 0) displayHour = 12;
            return $"{displayHour}:{m:D2} {period}";
        }


        public override bool PreDraw(ref Color lightColor)
        {
            // Only draw for the owner
            if (Main.myPlayer != Projectile.owner)
                return false;
            
            SpriteBatch spriteBatch = Main.spriteBatch;
            
            
            // Draw vignette effect
            DrawVignette(spriteBatch);
            
            // Draw clock background
            DrawClock(spriteBatch);
            
            // Draw clock hand
            DrawClockHand(spriteBatch);
            
            // Draw buttons
            DrawButtons(spriteBatch);
            
            // Draw time display
            DrawTimeDisplay(spriteBatch);
            
            return false;
        }



        private void DrawVignette(SpriteBatch spriteBatch)
        {
            // Create smooth black to dark blue vignette effect centered on player
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
            Color darkBlue = new Color(10, 20, 50);
            
            // Draw smooth rings from outside in for better blending
            int numRings = 40;
            float ringSpacing = (maxDist - clearRadius) / numRings;
            
            for (int ring = numRings - 1; ring >= 0; ring--)
            {
                float innerRadius = clearRadius + ring * ringSpacing;
                float outerRadius = innerRadius + ringSpacing + 5f; // Slight overlap for smoothness
                
                float normalizedDist = (float)ring / numRings;
                float alpha = normalizedDist * normalizedDist * VIGNETTE_INTENSITY;
                Color vignetteColor = Color.Lerp(Color.Black, darkBlue, normalizedDist * 0.7f) * alpha;
                
                // Draw ring segments
                int segments = 60;
                for (int i = 0; i < segments; i++)
                {
                    float angle1 = i * MathHelper.TwoPi / segments;
                    float angle2 = (i + 1) * MathHelper.TwoPi / segments;
                    
                    // Draw a quad for each segment
                    Vector2 inner1 = playerScreenPos + new Vector2((float)Math.Cos(angle1), (float)Math.Sin(angle1)) * innerRadius;
                    Vector2 inner2 = playerScreenPos + new Vector2((float)Math.Cos(angle2), (float)Math.Sin(angle2)) * innerRadius;
                    Vector2 outer1 = playerScreenPos + new Vector2((float)Math.Cos(angle1), (float)Math.Sin(angle1)) * outerRadius;
                    Vector2 outer2 = playerScreenPos + new Vector2((float)Math.Cos(angle2), (float)Math.Sin(angle2)) * outerRadius;
                    
                    // Calculate bounding rectangle for this segment
                    float minX = Math.Min(Math.Min(inner1.X, inner2.X), Math.Min(outer1.X, outer2.X));
                    float maxX = Math.Max(Math.Max(inner1.X, inner2.X), Math.Max(outer1.X, outer2.X));
                    float minY = Math.Min(Math.Min(inner1.Y, inner2.Y), Math.Min(outer1.Y, outer2.Y));
                    float maxY = Math.Max(Math.Max(inner1.Y, inner2.Y), Math.Max(outer1.Y, outer2.Y));
                    
                    Rectangle rect = new Rectangle((int)minX, (int)minY, (int)(maxX - minX) + 1, (int)(maxY - minY) + 1);
                    spriteBatch.Draw(pixel, rect, vignetteColor);
                }
            }
        }

        private void DrawClock(SpriteBatch spriteBatch)
        {
            // Load clock texture
            Texture2D clockTexture = ModContent.Request<Texture2D>("SariaMod/Items/zPearls/ClockOfTime").Value;
            
            
            Vector2 clockCenter = ScreenCenter;
            Vector2 origin = new Vector2(clockTexture.Width / 2f, clockTexture.Height / 2f);
            
            // Draw clock scaled up
            spriteBatch.Draw(clockTexture, clockCenter, null, Color.White, 0f, origin, CLOCK_SCALE, SpriteEffects.None, 0f);
        }

        private void DrawClockHand(SpriteBatch spriteBatch)
        {
            // Load hand texture
            Texture2D handTexture = ModContent.Request<Texture2D>("SariaMod/Items/zPearls/ClockOfTimeHands").Value;
            
            Vector2 clockCenter = ScreenCenter;
            
            // Calculate rotation angle based on selected hour
            float rotation = HourToAngle(selectedHour);
            
            
            Vector2 origin = new Vector2(handTexture.Width / 2f, handTexture.Height / 2f);
            
            // Draw hand scaled up to match clock
            spriteBatch.Draw(handTexture, clockCenter, null, Color.White, rotation, origin, CLOCK_SCALE, SpriteEffects.None, 0f);
            
            // Draw cyan grab indicator at the hand tip
            Vector2 handTip = clockCenter + new Vector2(
                (float)Math.Sin(rotation) * HAND_TIP_DISTANCE,
                -(float)Math.Cos(rotation) * HAND_TIP_DISTANCE
            );
            
            // Draw pulsing cyan/white grab indicator
            Texture2D pixel = Terraria.GameContent.TextureAssets.MagicPixel.Value;
            float pulseAlpha = 0.5f + 0.3f * (float)Math.Sin(Main.GameUpdateCount * 0.1f);
            int glowSize = (int)(HAND_GRAB_RADIUS * 2);
            
            // Outer cyan glow
            spriteBatch.Draw(pixel, new Rectangle((int)(handTip.X - glowSize/2), (int)(handTip.Y - glowSize/2), glowSize, glowSize), 
                Color.Cyan * (pulseAlpha * 0.5f));
            
            // Inner white highlight
            int innerSize = glowSize / 2;
            spriteBatch.Draw(pixel, new Rectangle((int)(handTip.X - innerSize/2), (int)(handTip.Y - innerSize/2), innerSize, innerSize), 
                Color.White * pulseAlpha);
        }

        private void DrawButtons(SpriteBatch spriteBatch)
        {
            Vector2 clockCenter = ScreenCenter;
            
            // Confirm button - fixed position below clock
            Vector2 confirmPos = clockCenter + new Vector2(0, 200);
            Color confirmColor = confirmHovered ? new Color(100, 200, 100) : new Color(60, 150, 60);
            
            DrawRoundedButton(spriteBatch, confirmPos, BUTTON_SIZE * 2, BUTTON_SIZE, confirmColor);
            
            string confirmText = "Confirm";
            Vector2 textSize = Terraria.GameContent.FontAssets.MouseText.Value.MeasureString(confirmText);
            Terraria.Utils.DrawBorderString(spriteBatch, confirmText, 
                confirmPos - textSize / 2 + new Vector2(0, 2), Color.White);
            
            // Exit button (X) - fixed position top-right
            Vector2 exitPos = clockCenter + new Vector2(180, -180);
            Color exitColor = exitHovered ? new Color(200, 100, 100) : new Color(150, 60, 60);
            
            DrawRoundedButton(spriteBatch, exitPos, BUTTON_SIZE, BUTTON_SIZE, exitColor);
            
            string exitText = "X";
            Vector2 exitTextSize = Terraria.GameContent.FontAssets.MouseText.Value.MeasureString(exitText);
            Terraria.Utils.DrawBorderString(spriteBatch, exitText, 
                exitPos - exitTextSize / 2 + new Vector2(0, 2), Color.White);
        }

        private void DrawRoundedButton(SpriteBatch spriteBatch, Vector2 center, float width, float height, Color color)
        {
            Texture2D pixel = Terraria.GameContent.TextureAssets.MagicPixel.Value;
            
            Rectangle rect = new Rectangle(
                (int)(center.X - width / 2),
                (int)(center.Y - height / 2),
                (int)width,
                (int)height
            );
            
            spriteBatch.Draw(pixel, rect, color);
            
            Color borderColor = Color.White * 0.5f;
            int borderWidth = 2;
            
            spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, rect.Width, borderWidth), borderColor);
            spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y + rect.Height - borderWidth, rect.Width, borderWidth), borderColor);
            spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, borderWidth, rect.Height), borderColor);
            spriteBatch.Draw(pixel, new Rectangle(rect.X + rect.Width - borderWidth, rect.Y, borderWidth, rect.Height), borderColor);
        }




        private void DrawTimeDisplay(SpriteBatch spriteBatch)
        {
            Vector2 clockCenter = ScreenCenter;
            
            // Draw selected time above clock
            string timeText = FormatHour(selectedHour);
            string label = "Selected Time:";
            
            Vector2 labelSize = Terraria.GameContent.FontAssets.MouseText.Value.MeasureString(label);
            Vector2 timeSize = Terraria.GameContent.FontAssets.MouseText.Value.MeasureString(timeText);
            
            Vector2 labelPos = clockCenter + new Vector2(-labelSize.X / 2, -200);
            Vector2 timePos = clockCenter + new Vector2(-timeSize.X / 2, -170);
            
            Terraria.Utils.DrawBorderString(spriteBatch, label, labelPos, Color.LightGray);
            Terraria.Utils.DrawBorderString(spriteBatch, timeText, timePos, Color.Cyan, 1.3f);
            
            // Draw current game time below confirm button
            string currentTimeLabel = "Current Time:";
            string currentTime = FormatHour(GetCurrentGameHour());
            
            Vector2 currentLabelSize = Terraria.GameContent.FontAssets.MouseText.Value.MeasureString(currentTimeLabel);
            Vector2 currentTimeSize = Terraria.GameContent.FontAssets.MouseText.Value.MeasureString(currentTime);
            
            Vector2 currentLabelPos = clockCenter + new Vector2(-currentLabelSize.X / 2, 240);
            Vector2 currentTimePos = clockCenter + new Vector2(-currentTimeSize.X / 2, 265);
            
            Terraria.Utils.DrawBorderString(spriteBatch, currentTimeLabel, currentLabelPos, Color.LightGray * 0.8f);
            Terraria.Utils.DrawBorderString(spriteBatch, currentTime, currentTimePos, Color.SkyBlue);
            
            // Draw moon phase info on the left
            DrawMoonPhaseInfo(spriteBatch, clockCenter);
            
            // Draw drag instruction
            if (!isDragging)
            {
                string instruction = "Drag the clock hand to select time";
                Vector2 instructionSize = Terraria.GameContent.FontAssets.MouseText.Value.MeasureString(instruction);
                Vector2 instructionPos = clockCenter + new Vector2(-instructionSize.X / 2, 310);
                Terraria.Utils.DrawBorderString(spriteBatch, instruction, instructionPos, Color.White * 0.6f, 0.8f);
            }
        }
        
        private void DrawMoonPhaseInfo(SpriteBatch spriteBatch, Vector2 clockCenter)
        {
            // Moon phase info on the left side
            float leftX = clockCenter.X - 250;
            float startY = clockCenter.Y - 60;
            
            // Get moon phase name for selected time
            int displayPhase = selectedHour < DAWN_HOUR ? savedEarlyMoonPhase : savedLateMoonPhase;
            string moonPhaseName = displayPhase switch
            {
                0 => "Full Moon",
                1 => "Waning Gibbous",
                2 => "Third Quarter",
                3 => "Waning Crescent",
                4 => "New Moon",
                5 => "Waxing Crescent",
                6 => "First Quarter",
                7 => "Waxing Gibbous",
                _ => $"Phase {displayPhase}"
            };
            
            // Title
            Terraria.Utils.DrawBorderString(spriteBatch, "Moon Phase", new Vector2(leftX, startY), Color.LightBlue, 0.9f);
            
            // Phase name
            Terraria.Utils.DrawBorderString(spriteBatch, moonPhaseName, new Vector2(leftX, startY + 25), Color.White, 0.85f);
            
            // Show if crossing dawn boundary would change phase
            if (selectedHour < DAWN_HOUR && startingHour >= DAWN_HOUR)
            {
                Terraria.Utils.DrawBorderString(spriteBatch, "(previous night)", new Vector2(leftX, startY + 50), Color.Gray, 0.7f);
            }
            else if (selectedHour >= DAWN_HOUR && startingHour < DAWN_HOUR)
            {
                Terraria.Utils.DrawBorderString(spriteBatch, "(after dawn)", new Vector2(leftX, startY + 50), Color.Gray, 0.7f);
            }
        }
        
        /// <summary>
        /// Static method to draw the transition clocks.
        /// Each player sees clocks around THEMSELVES (not the ocarina user).
        /// Clocks are solid with no bobbing.
        /// </summary>
        private static void DrawTransitionClocksStatic(SpriteBatch spriteBatch, float startHour, float targetHour, float progress, int timer)
        {
            Texture2D pixel = Terraria.GameContent.TextureAssets.MagicPixel.Value;
            Texture2D clockTexture = ModContent.Request<Texture2D>("SariaMod/Items/zPearls/ClockOfTime").Value;
            Texture2D handTexture = ModContent.Request<Texture2D>("SariaMod/Items/zPearls/ClockOfTimeHands").Value;
            
            // Always use LOCAL player - each player sees clocks around themselves
            Player localPlayer = Main.player[Main.myPlayer];
            Vector2 playerScreenPos = localPlayer.Center - Main.screenPosition;
            Vector2 screenCenter = new Vector2(Main.screenWidth / 2f, Main.screenHeight / 2f);
            
            // Calculate current hour during transition
            float hourDiff = targetHour - startHour;
            float currentHour = startHour + hourDiff * progress;
            while (currentHour < 0) currentHour += 24f;
            while (currentHour >= 24f) currentHour -= 24f;
            
            // Determine direction: positive = clockwise, negative = counter-clockwise
            float direction = hourDiff >= 0 ? 1f : -1f;
            
            // Fade in/out based on progress (appear at start, disappear at end)
            float fadeAlpha = 1f;
            if (progress < 0.1f)
                fadeAlpha = progress / 0.1f; // Fade in
            else if (progress > 0.9f)
                fadeAlpha = (1f - progress) / 0.1f; // Fade out
            
            // Draw vignette
            float maxDist = Math.Max(
                Math.Max(Vector2.Distance(playerScreenPos, Vector2.Zero),
                         Vector2.Distance(playerScreenPos, new Vector2(Main.screenWidth, 0))),
                Math.Max(Vector2.Distance(playerScreenPos, new Vector2(0, Main.screenHeight)),
                         Vector2.Distance(playerScreenPos, new Vector2(Main.screenWidth, Main.screenHeight)))
            );
            
            float clearRadius = 80f;
            int numRings = 35;
            float ringSpacing = (maxDist - clearRadius) / numRings;
            float pulse = 0.5f + 0.2f * (float)Math.Sin(timer * 0.1f);
            
            for (int ring = numRings - 1; ring >= 0; ring--)
            {
                float innerRadius = clearRadius + ring * ringSpacing;
                float outerRadius = innerRadius + ringSpacing + 5f;
                
                float normalizedDist = (float)ring / numRings;
                float alpha = normalizedDist * pulse * fadeAlpha;
                Color flashColor = Color.Lerp(Color.CornflowerBlue, Color.White, normalizedDist * 0.5f) * alpha;
                
                int segments = 50;
                for (int i = 0; i < segments; i++)
                {
                    float angle1 = i * MathHelper.TwoPi / segments;
                    Vector2 inner1 = playerScreenPos + new Vector2((float)Math.Cos(angle1), (float)Math.Sin(angle1)) * innerRadius;
                    Vector2 outer1 = playerScreenPos + new Vector2((float)Math.Cos(angle1), (float)Math.Sin(angle1)) * outerRadius;
                    
                    float minX = Math.Min(inner1.X, outer1.X) - 5;
                    float maxX = Math.Max(inner1.X, outer1.X) + 5;
                    float minY = Math.Min(inner1.Y, outer1.Y) - 5;
                    float maxY = Math.Max(inner1.Y, outer1.Y) + 5;
                    
                    Rectangle rect = new Rectangle((int)minX, (int)minY, (int)(maxX - minX) + 1, (int)(maxY - minY) + 1);
                    spriteBatch.Draw(pixel, rect, flashColor);
                }
            }
            
            // Draw main clock centered on player (semi-transparent so player visible)
            float mainClockScale = 3.0f;
            Vector2 mainClockOrigin = new Vector2(clockTexture.Width / 2f, clockTexture.Height / 2f);
            Vector2 mainHandOrigin = new Vector2(handTexture.Width / 2f, handTexture.Height / 2f);
            float mainHandRotation = currentHour / 24f * MathHelper.TwoPi;
            
            // Draw main clock - NO wobble, solid position
            spriteBatch.Draw(clockTexture, playerScreenPos, null, Color.White * (0.5f * fadeAlpha), 0f, mainClockOrigin, mainClockScale, SpriteEffects.None, 0f);
            spriteBatch.Draw(handTexture, playerScreenPos, null, Color.White * (0.6f * fadeAlpha), mainHandRotation, mainHandOrigin, mainClockScale, SpriteEffects.None, 0f);
            
            // Draw smaller clocks spiraling outward - SOLID, no bobbing
            int numSmallClocks = 8;
            float smallClockScale = 2.5f;
            float spiralRadius = 140f; // Fixed radius - no pulsing
            float spiralRotation = timer * 0.02f * direction; // Smooth rotation based on time direction
            
            for (int i = 0; i < numSmallClocks; i++)
            {
                // Each clock is positioned in a circle around the player
                float angleOffset = (i / (float)numSmallClocks) * MathHelper.TwoPi + spiralRotation;
                Vector2 clockPos = playerScreenPos + new Vector2(
                    (float)Math.Cos(angleOffset) * spiralRadius,
                    (float)Math.Sin(angleOffset) * spiralRadius
                );
                
                // Each clock shows a different hour (offset by 1 hour each)
                float clockHour = currentHour - (i + 1) * direction;
                while (clockHour < 0) clockHour += 24f;
                while (clockHour >= 24f) clockHour -= 24f;
                float handRotation = clockHour / 24f * MathHelper.TwoPi;
                
                Vector2 clockOrigin = new Vector2(clockTexture.Width / 2f, clockTexture.Height / 2f);
                Vector2 handOrigin = new Vector2(handTexture.Width / 2f, handTexture.Height / 2f);
                
                // Draw AFTERIMAGES first (trailing behind)
                int numAfterimages = 5;
                for (int a = numAfterimages; a >= 1; a--)
                {
                    float afterimageAngle = angleOffset - (a * 0.06f * direction);
                    Vector2 afterimagePos = playerScreenPos + new Vector2(
                        (float)Math.Cos(afterimageAngle) * spiralRadius,
                        (float)Math.Sin(afterimageAngle) * spiralRadius
                    );
                    
                    float afterimageAlpha = 0.4f * (1f - (a / (float)(numAfterimages + 1))) * fadeAlpha;
                    Color afterimageColor = Color.Lerp(Color.Cyan, Color.White, 0.5f) * afterimageAlpha;
                    
                    spriteBatch.Draw(clockTexture, afterimagePos, null, afterimageColor, 0f, clockOrigin, smallClockScale * 0.95f, SpriteEffects.None, 0f);
                    spriteBatch.Draw(handTexture, afterimagePos, null, afterimageColor, handRotation, handOrigin, smallClockScale * 0.95f, SpriteEffects.None, 0f);
                }
                
                // Draw the SOLID main small clock - NO rotation wobble
                spriteBatch.Draw(clockTexture, clockPos, null, Color.White * fadeAlpha, 0f, clockOrigin, smallClockScale, SpriteEffects.None, 0f);
                spriteBatch.Draw(handTexture, clockPos, null, Color.White * fadeAlpha, handRotation, handOrigin, smallClockScale, SpriteEffects.None, 0f);
            }
            
            // Draw transitioning text
            string text = "Time is shifting...";
            Vector2 textSize = Terraria.GameContent.FontAssets.MouseText.Value.MeasureString(text);
            Vector2 textPos = screenCenter - textSize / 2 + new Vector2(0, -180);
            
            Color textColor = Color.Lerp(Color.Cyan, Color.White, 0.5f) * fadeAlpha;
            Terraria.Utils.DrawBorderString(spriteBatch, text, textPos, textColor, 1.5f);
            
            // Show target time below
            string targetText = $"-> {FormatHourStatic(targetHour)}";
            Vector2 targetSize = Terraria.GameContent.FontAssets.MouseText.Value.MeasureString(targetText);
            Vector2 targetPos = screenCenter + new Vector2(-targetSize.X / 2, 180);
            
            Terraria.Utils.DrawBorderString(spriteBatch, targetText, targetPos, Color.White * fadeAlpha, 1.2f);
        }
        
        
        private static string FormatHourStatic(float hour)
        {
            int h = (int)hour;
            int m = (int)((hour - h) * 60);
            string period = h >= 12 ? "PM" : "AM";
            int displayHour = h % 12;
            if (displayHour == 0) displayHour = 12;
            return $"{displayHour}:{m:D2} {period}";
        }

        public override void Kill(int timeLeft)
        {
            base.Kill(timeLeft);
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, 
            List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            // UI mode - draw over everything including wires UI
            overWiresUI.Add(index);
        }
    }
}
