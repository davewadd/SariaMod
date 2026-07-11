using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SariaMod.Buffs;
using SariaMod.Dusts;
using SariaMod.Items.Emerald;
using SariaMod.Items.Strange;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using SariaMod.Items.Sapphire;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using ReLogic.Content;

namespace SariaMod
{
    public class FairyPlayerMiscEffects : ModPlayer
    {
        private const int sphereRadius3 = 1;
        private static int LavaSoundTimer;
        private static int Soundtimer;

        // Persistent globs that trail behind the oasis wind lines
        private struct WindGlob
        {
            public Vector2 Pos;
            public Vector2 Vel;
            public float Radius;
            public float Alpha;
            public Color Color;
            public int Age;
            public int MaxAge;
        }
        private List<WindGlob> _windGlobs;
        internal FadingSoundPlayer fadingLavaSound;
        internal FadingSoundPlayer fadingSandstormSound;
        private int _sandstormIndoorCheckTimer;
        private bool _sandstormIndoors;

        private int _sariaProximityBuffTimer;
        private static Texture2D whitePixelTexture;
        
        // Rain Ocarina vignette effect constants
        private const int VIGNETTE_EFFECT_DURATION = 180; // Match projectile duration (3 seconds)
        private const int FADE_IN_FRAMES = 30; // 0.5 seconds fade in
        private const int FADE_OUT_FRAMES = 60; // 1 second fade out

        public static readonly SoundStyle OutdoorRain = new SoundStyle("SariaMod/Sounds/Rain")
        {
            IsLooped = true,
            MaxInstances = 1
        };
        public static readonly SoundStyle RainIndoors = new SoundStyle("SariaMod/Sounds/RainIndoors")
        {
            IsLooped = true,
            MaxInstances = 1
        };
        public static readonly SoundStyle Thunder1 = new SoundStyle("SariaMod/Sounds/Thunder1");
        public static readonly SoundStyle Thunder2 = new SoundStyle("SariaMod/Sounds/Thunder2");
        public static readonly SoundStyle Thunder3 = new SoundStyle("SariaMod/Sounds/Thunder3");
        public static readonly SoundStyle Thunder4 = new SoundStyle("SariaMod/Sounds/Thunder4");
        public static readonly SoundStyle ThunderThighs = new SoundStyle("SariaMod/Sounds/ThunderThighs");

        private SoundEffectInstance outdoorRainInstance;
        private SoundEffectInstance indoorRainInstance;
        private bool playRainSound;
        private bool wasPlayingRain;
        private bool wasFocusLost; // Track if we lost focus while sounds were playing
        
        // Volume fade tracking
        private float outdoorCurrentVolume = 0f;
        private float outdoorTargetVolume = 0f;
        private float indoorCurrentVolume = 0f;
        private float indoorTargetVolume = 0f;
        private const float FADE_SPEED = 1f / 102f; // 1.7 second fade (60 * 1.7 = 102 frames)

        private static readonly int[][] sarialevelXpThresholds =
        {
             new int[] { 375, 750, 1125, 1500, 1875, 2250, 2625, 3000 },
             new int[] { 1125, 2250, 3375, 4500, 5625, 6750, 7875, 9000 },
             new int[] { 2500, 5000, 7500, 10000, 12500, 15000, 17500, 20000 },
             new int[] { 5000, 10000, 15000, 20000, 25000, 30000, 35000, 40000 },
             new int[] { 10000, 20000, 30000, 40000, 50000, 60000, 70000, 80000 },
             new int[] { 30000, 60000, 90000, 120000, 150000, 180000, 210000, 240000 }
        };

        public static void FairyPostUpdateMiscEffects(Player player, Mod mod)
        {
            FairyPlayer modPlayer = player.Fairy();
            FairyPlayerMiscEffects miscEffects = player.GetModPlayer<FairyPlayerMiscEffects>();
            MiscEffects(player, modPlayer, mod, miscEffects);
        }

        public void DrawScreenVignetteOnly(float effectProgress)
        {
            DrawScreenVignette(effectProgress);
        }

        /// <summary>
        /// Draws the Oasis Ocarina visual effects (vignette and sand columns).
        /// </summary>
        /// <param name="projectileTimeLeft">Time remaining on the SandstormOcarinaNote projectile</param>
        public void DrawOasisOcarinaEffects(int projectileTimeLeft)
        {
            // Calculate effect progress for fading
            // Fade in during first FADE_IN_FRAMES, hold, then fade out during last FADE_OUT_FRAMES
            float effectIntensity;
            int timePassed = VIGNETTE_EFFECT_DURATION - projectileTimeLeft;

            if (timePassed < FADE_IN_FRAMES)
            {
                // Fading in
                effectIntensity = (float)timePassed / FADE_IN_FRAMES;
            }
            else if (projectileTimeLeft < FADE_OUT_FRAMES)
            {
                // Fading out
                effectIntensity = (float)projectileTimeLeft / FADE_OUT_FRAMES;
            }
            else
            {
                // Full intensity
                effectIntensity = 1f;
            }

            // Apply easing for smoother fade
            float easedIntensity = effectIntensity * effectIntensity * (3f - 2f * effectIntensity); // Smoothstep

            DrawSandColumns(effectIntensity, projectileTimeLeft,
                new Color(255, 140, 30),   // Orange for lines
                new Color(255, 190, 80),   // Lighter orange for fill
                spawnEmbers: true);

            // Draw the vignette effect
            DrawScreenVignetteOnly(easedIntensity);
        }

        /// <summary>
        /// Draws the sandy light columns that spread from the player's center.
        /// </summary>
        private void DrawSandColumns(float intensity, int projectileTimeLeft, Color lineColor, Color fillColor, bool spawnEmbers = false)
        {
            if (intensity <= 0f) return;

            // Initialize texture if needed
            if (whitePixelTexture == null || whitePixelTexture.IsDisposed)
            {
                whitePixelTexture = new Texture2D(Main.graphics.GraphicsDevice, 1, 1);
                whitePixelTexture.SetData(new Color[] { Color.White });
            }

            int timePassed = VIGNETTE_EFFECT_DURATION - projectileTimeLeft;

            // Calculate spread progress (0 = centered/overlapped, 1 = full spread at 4 tiles)
            float spreadProgress;
            if (timePassed < FADE_IN_FRAMES)
            {
                // Spreading outward during fade in
                spreadProgress = (float)timePassed / FADE_IN_FRAMES;
            }
            else if (projectileTimeLeft < FADE_OUT_FRAMES)
            {
                // Converging back to center during fade out
                spreadProgress = (float)projectileTimeLeft / FADE_OUT_FRAMES;
            }
            else
            {
                // Full spread
                spreadProgress = 1f;
            }

            // Apply easing for smooth spread animation
            spreadProgress = spreadProgress * spreadProgress * (3f - 2f * spreadProgress);

            // Player position - feet are at bottom of player hitbox
            Vector2 playerFeet = new Vector2(Player.Center.X, Player.position.Y + Player.height);
            Vector2 screenFeet = playerFeet - Main.screenPosition;

            // Column dimensions
            float maxSpread = 64f; // 4 tiles (64 pixels) to each side
            float currentSpread = maxSpread * spreadProgress;
            float lineWidth = 4f; // Width of the sand lines

            // Vertical extent - full 4 tiles down, plus spherical rounding
            float bottomExtend = 64f * spreadProgress; // 4 tiles below feet (straight part)
            float sphereRadius = currentSpread; // Sphere radius matches the spread width
            float maxHeight = 1600f; // 100 tiles up
            float currentHeight = maxHeight * spreadProgress; // Height grows with spread

            // Don't draw if no height
            if (currentHeight < 1f) return;

            // Calculate positions
            float leftLineX = screenFeet.X - currentSpread;
            float rightLineX = screenFeet.X + currentSpread;

            // Spawn particles from the top of the pillar
            if (currentSpread > 5f)
            {
                // Calculate spawn intensity based on effect phase
                float spawnIntensity;
                if (timePassed < FADE_IN_FRAMES)
                {
                    spawnIntensity = (float)timePassed / FADE_IN_FRAMES;
                }
                else if (projectileTimeLeft < FADE_OUT_FRAMES)
                {
                    spawnIntensity = (float)projectileTimeLeft / FADE_OUT_FRAMES;
                }
                else
                {
                    spawnIntensity = 1f;
                }

                // Apply easing for smoother ramp
                spawnIntensity = spawnIntensity * spawnIntensity;

                int maxRainPerFrame = 10;
                int rainCount = (int)(maxRainPerFrame * spawnIntensity);

                if (spawnIntensity > 0.1f && rainCount < 1)
                    rainCount = 1;

                for (int r = 0; r < rainCount; r++)
                {
                    float rainWorldX = Player.Center.X + Main.rand.NextFloat(-currentSpread * 0.95f, currentSpread * 0.95f);
                    float rainWorldY = playerFeet.Y - currentHeight - Main.rand.NextFloat(0f, 30f);

                    int rainGoreType = Main.rand.Next(3) switch
                    {
                        0 => 706,
                        1 => 707,
                        _ => 708
                    };

                    Gore rainGore = Gore.NewGoreDirect(
                        Player.GetSource_FromThis(),
                        new Vector2(rainWorldX, rainWorldY),
                        new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(6f, 10f)),
                        rainGoreType,
                        Main.rand.NextFloat(0.8f, 1.2f)
                    );

                    if (rainGore != null)
                    {
                        rainGore.timeLeft = Gore.goreTime;
                    }
                }
            }

            // Draw outer glow outline - a wider pass of the pillar at half opacity drawn first (behind everything)
            // Pulse the outline size using a sine wave: 14px minimum, expanding out and in
            float pulseMin = 14f;
            float pulseMax = 48f; // How far out the pulse reaches at its widest
            float pulseSpeed = 1.2f; // Oscillations per second
            float pulse = (float)Math.Sin(Main.GameUpdateCount * (Math.PI * 2.0 / 60.0) * pulseSpeed);
            float outlineExpand = pulseMin + (pulseMax - pulseMin) * (pulse * 0.5f + 0.5f); // remap -1..1 to pulseMin..pulseMax
            float outlineOpacity = 0.3f * intensity; // Half of the fill opacity
            if (currentSpread > 1f && outlineOpacity > 0.005f)
            {
                float outlineLeft = screenFeet.X - currentSpread - outlineExpand;
                float outlineWidth = (currentSpread + outlineExpand) * 2f;

                Main.spriteBatch.Draw(whitePixelTexture,
                    new Rectangle((int)outlineLeft, (int)(screenFeet.Y - currentHeight), (int)outlineWidth, (int)currentHeight),
                    fillColor * outlineOpacity);

                // Wavy edge ripple on the halo
                DrawWavyEdgeOverlay(screenFeet, screenFeet.X, currentSpread + outlineExpand,
                    currentHeight, fillColor, outlineOpacity * 0.6f);

                // Outline below feet
                Main.spriteBatch.Draw(whitePixelTexture,
                    new Rectangle((int)outlineLeft, (int)screenFeet.Y, (int)outlineWidth, (int)bottomExtend),
                    fillColor * outlineOpacity);

                // Wider spherical bottom cap for the outline
                DrawSphericalBottomFill(screenFeet.X, screenFeet.Y + bottomExtend, currentSpread + outlineExpand, fillColor, outlineOpacity);
            }

            // Draw the fill rectangle between the two lines
            float fillOpacity = 0.6f;

            if (currentSpread > 1f)
            {
                float fillWidth = (currentSpread * 2f) - lineWidth;
                if (fillWidth > 0)
                {
                    Main.spriteBatch.Draw(whitePixelTexture,
                        new Rectangle((int)(leftLineX + lineWidth / 2f), (int)(screenFeet.Y - currentHeight), (int)fillWidth, (int)currentHeight),
                        fillColor * (fillOpacity * intensity));

                    // Wavy edge ripple on the inner fill
                    DrawWavyEdgeOverlay(screenFeet, screenFeet.X, currentSpread,
                        currentHeight, fillColor, fillOpacity * intensity * 0.5f);

                    float belowOpacity = fillOpacity * intensity;
                    Color belowFillColor = fillColor * belowOpacity;
                    Rectangle belowFillRect = new Rectangle(
                        (int)(leftLineX + lineWidth / 2f),
                        (int)screenFeet.Y,
                        (int)fillWidth,
                        (int)bottomExtend
                    );
                    Main.spriteBatch.Draw(whitePixelTexture, belowFillRect, belowFillColor);

                    DrawSphericalBottomFill(screenFeet.X, screenFeet.Y + bottomExtend, currentSpread, fillColor, fillOpacity * intensity);
                }
            }

            // Draw spiral wind effects that wrap around the pillar
            if (currentHeight > 100f && spreadProgress > 0.3f)
            {
                DrawSpiralWindEffects(screenFeet, currentSpread, currentHeight, timePassed, intensity, false, spawnEmbers); // Back spirals (underlap)
            }

            // Draw the left and right lines with spherical bottom
            DrawTaperedLineWithSphere(leftLineX - lineWidth / 2f, screenFeet.Y, bottomExtend, currentHeight, lineWidth, currentSpread, lineColor, intensity * 0.35f, true);
            DrawTaperedLineWithSphere(rightLineX - lineWidth / 2f, screenFeet.Y, bottomExtend, currentHeight, lineWidth, currentSpread, lineColor, intensity * 0.35f, false);

            // Draw front spirals (overlap) after the lines
            if (currentHeight > 100f && spreadProgress > 0.3f)
            {
                DrawSpiralWindEffects(screenFeet, currentSpread, currentHeight, timePassed, intensity, true, spawnEmbers); // Front spirals (overlap)
            }
        }

        /// <summary>
        /// Draws the Rain Ocarina visual effects
        /// </summary>
        /// <param name="projectileTimeLeft">Time remaining on the RainOcarinaNote projectile</param>
        public void DrawRainOcarinaEffects(int projectileTimeLeft)
        {
            // Calculate effect progress for fading
            // Fade in during first FADE_IN_FRAMES, hold, then fade out during last FADE_OUT_FRAMES
            float effectIntensity;
            int timePassed = VIGNETTE_EFFECT_DURATION - projectileTimeLeft;
            
            if (timePassed < FADE_IN_FRAMES)
            {
                // Fading in
                effectIntensity = (float)timePassed / FADE_IN_FRAMES;
            }
            else if (projectileTimeLeft < FADE_OUT_FRAMES)
            {
                // Fading out
                effectIntensity = (float)projectileTimeLeft / FADE_OUT_FRAMES;
            }
            else
            {
                // Full intensity
                effectIntensity = 1f;
            }
            
            // Apply easing for smoother fade
            float easedIntensity = effectIntensity * effectIntensity * (3f - 2f * effectIntensity); // Smoothstep
            
            // Draw the light columns around the player
            DrawLightColumns(effectIntensity, projectileTimeLeft);
            
            // Draw the vignette effect
            DrawScreenVignetteOnly(easedIntensity);
        }




        /// <summary>
        /// Draws the yellow light columns that spread from the player's center.
        /// </summary>
        private void DrawLightColumns(float intensity, int projectileTimeLeft)
        {
            // Identical visual to the Rain Ocarina but with yellow tones
            DrawSandColumns(intensity, projectileTimeLeft,
                new Color(255, 220, 80),   // Yellow for lines
                new Color(255, 255, 150)); // Lighter yellow for fill
        }
        
        /// <summary>
        /// Draws spiral wind effects that appear to wrap around the light pillar.
        /// </summary>
        /// <summary>
        /// Draws spiral wind effects that appear to wrap around the light pillar.
        /// </summary>
        private void DrawSpiralWindEffects(Vector2 screenFeet, float spread, float height, int timePassed, float intensity, bool drawFront, bool spawnEmbers = false)
        {
            Color windColor = new Color(240, 245, 255);

            int numSpirals = 3;
            float spiralSpeed = 0.006f;
            float rotations = 2.5f;

            // Wind orbits a little wider than the pillar so it visibly sweeps in and out
            float windRadius = spread + 24f;

            // Stroke dimensions: long axis follows the path direction, thin cross axis
            float strokeLength = spread * 1.4f;
            float strokeThickness = 3f;

            // How far back along the orbit each trail step reaches (fraction of a full rotation)
            float trailArc = 0.14f; // ~50 degrees worth of arc per trail
            int trailCount = 8;

            // Ember colours used when spawnEmbers is true
            Color[] emberColors = new Color[]
            {
                new Color(255, 255, 255),  // pure white
                new Color(255, 250, 220),  // warm white
                new Color(255, 245, 180),  // soft cream
            };

            for (int s = 0; s < numSpirals; s++)
            {
                float spiralOffset = (float)s / numSpirals;
                float vp = (timePassed * spiralSpeed + spiralOffset) % 1f; // 0..1, bottom to top

                if (vp < 0.05f) continue;

                // Vertical fade at top and bottom
                float vertAlpha = 1f;
                if (vp < 0.15f)      vertAlpha = vp / 0.15f;
                else if (vp > 0.75f) vertAlpha = (1f - vp) / 0.25f;

                float baseAngle = vp * rotations * MathHelper.TwoPi + (s * MathHelper.TwoPi / numSpirals);

                for (int t = 0; t <= trailCount; t++)
                {
                    float trailFrac = (float)t / trailCount;

                    // Step back along the orbit by a fraction of a full rotation
                    float ta = baseAngle - trailFrac * (MathHelper.TwoPi * trailArc);

                    float tSin = (float)Math.Sin(ta);
                    float tCos = (float)Math.Cos(ta);

                    // Front layer: cos > 0 (sweeping to the right / facing viewer)
                    bool isFront = tCos > 0;
                    if (isFront != drawFront) continue;

                    // Horizontal position on the orbit
                    float strokeX = screenFeet.X + tSin * windRadius;

                    // Vertical position — trail steps drop slightly lower on the pillar
                    float trailVP = vp - trailFrac * (trailArc / rotations);
                    if (trailVP < 0f) continue;
                    float strokeY = screenFeet.Y - (trailVP * height * 0.85f);

                    // Tangent of the spiral path at this angle:
                    //   dX/d(angle) = cos(angle) * windRadius
                    //   dY/d(angle) = -height * 0.85 / (rotations * 2π)   (vertical rise per radian)
                    float dX = tCos * windRadius;
                    float dY = -(height * 0.85f) / (rotations * MathHelper.TwoPi);
                    float strokeAngle = (float)Math.Atan2(dY, dX);

                    // Trail fades and shrinks toward the tail
                    float trailAlpha = (1f - trailFrac * 0.9f) * vertAlpha;
                    // Back layer is dimmer (hidden behind the pillar)
                    float layerDim = drawFront ? 1f : 0.45f;
                    byte alpha = (byte)(115 * intensity * trailAlpha * layerDim);
                    if (alpha < 2) continue;

                    float thisLength = strokeLength * (1f - trailFrac * 0.55f);
                    float thisThick  = strokeThickness * (1f - trailFrac * 0.4f);

                    Main.spriteBatch.Draw(
                        whitePixelTexture,
                        new Vector2(strokeX, strokeY),
                        null,
                        new Color(windColor.R, windColor.G, windColor.B, alpha),
                        strokeAngle,
                        new Vector2(0.5f, 0.5f), // centre origin so rotation is around the stroke midpoint
                        new Vector2(thisLength, thisThick),
                        SpriteEffects.None,
                        0f
                    );

                    // Spawn persistent trailing globs at the head of each spiral arm (t==0 only)
                    if (spawnEmbers && t == 0 && Main.rand.Next(2) == 0)
                    {
                        // Velocity loosely follows the tangent plus a little upward drift
                        float speed = Main.rand.NextFloat(0.4f, 1.2f);
                        Vector2 tangent = new Vector2(dX, dY);
                        if (tangent != Vector2.Zero) tangent.Normalize();
                        Vector2 vel = tangent * speed + new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), Main.rand.NextFloat(-0.6f, -0.1f));

                        int maxAge = Main.rand.Next(18, 40);
                        if (_windGlobs == null) _windGlobs = new List<WindGlob>();
                        _windGlobs.Add(new WindGlob
                        {
                            Pos    = new Vector2(strokeX, strokeY),
                            Vel    = vel,
                            Radius = Main.rand.NextFloat(5f, 14f),
                            Alpha  = Main.rand.NextFloat(0.7f, 1.0f) * intensity * vertAlpha,
                            Color  = emberColors[Main.rand.Next(emberColors.Length)],
                            Age    = 0,
                            MaxAge = maxAge,
                        });
                    }
                }
            }

            // Update and draw persistent globs (only on the front pass to avoid double-drawing)
            if (spawnEmbers && drawFront && _windGlobs != null)
            {
                for (int i = _windGlobs.Count - 1; i >= 0; i--)
                {
                    WindGlob g = _windGlobs[i];
                    g.Age++;
                    g.Pos += g.Vel;
                    // Gentle drag so they float rather than shoot away
                    g.Vel *= 0.93f;

                    if (g.Age >= g.MaxAge)
                    {
                        _windGlobs.RemoveAt(i);
                        continue;
                    }

                    float lifeFrac = 1f - (float)g.Age / g.MaxAge;
                    float drawAlpha = g.Alpha * lifeFrac;
                    float drawRadius = g.Radius * (0.5f + 0.5f * lifeFrac); // shrink slightly as they age

                    DrawFilledCircle(g.Pos, drawRadius, g.Color, drawAlpha);
                    _windGlobs[i] = g;
                }
            }
        }

        /// <summary>
        /// Draws a spherical bottom cap for the fill area.
        /// </summary>
        /// <summary>
        /// Draws faint travelling waves along the left and right edges of a pillar column.
        /// Each wave is a thin vertical strip whose X nudges slightly inward/outward with a
        /// sine that scrolls upward over time, giving the appearance of light rippling up the edge.
        /// </summary>
        private void DrawWavyEdgeOverlay(Vector2 screenFeet, float centerX, float halfWidth,
                                         float columnHeight, Color color, float opacity)
        {
            if (opacity < 0.005f || columnHeight < 2f || halfWidth < 2f) return;

            // How many horizontal strips to draw (more = smoother wave)
            int strips = 60;
            float stripH = columnHeight / strips;

            // Wave parameters
            float waveAmplitude = 4f;    // Max pixels the edge ripples in/out
            float waveFrequency = 4f;    // Spatial frequency: full cycles across the column height
            float waveScrollSpeed = 0.07f; // How fast the wave travels upward (radians per frame)
            float timeOffset = Main.GameUpdateCount * waveScrollSpeed;

            Color c = color * opacity;

            for (int i = 0; i < strips; i++)
            {
                float normY = (float)i / strips;                      // 0 = top, 1 = bottom of column
                float segTop    = screenFeet.Y - columnHeight + i * stripH;
                float segBottom = segTop + stripH;

                // The wave phase scrolls upward, so higher strips are ahead in the wave
                float phase = normY * waveFrequency * MathHelper.TwoPi - timeOffset;
                float wave  = (float)Math.Sin(phase) * waveAmplitude; // + = outward, - = inward

                float stripW = 3f; // pixel width of the edge strip

                // Left edge — positive wave pushes further left (outward)
                float leftEdgeX = centerX - halfWidth + wave - stripW;
                Main.spriteBatch.Draw(whitePixelTexture,
                    new Rectangle((int)leftEdgeX, (int)segTop, (int)stripW, Math.Max(1, (int)(segBottom - segTop))),
                    c);

                // Right edge — positive wave pushes further right (outward)
                float rightEdgeX = centerX + halfWidth + wave;
                Main.spriteBatch.Draw(whitePixelTexture,
                    new Rectangle((int)rightEdgeX, (int)segTop, (int)stripW, Math.Max(1, (int)(segBottom - segTop))),
                    c);
            }
        }

        /// <summary>
        /// Draws a fully-filled solid circle using scanline rows — identical technique to
        /// DrawGlobSpot in Saria.cs, giving a clean round disc with no gaps or seams.
        /// </summary>
        private void DrawFilledCircle(Vector2 center, float radius, Color color, float opacity)
        {
            if (radius < 0.5f || opacity < 0.005f) return;
            int r = Math.Max(1, (int)radius);
            Color c = color * opacity;
            for (int dy = -r; dy <= r; dy++)
            {
                float normY = r > 0 ? dy / (float)r : 0f;
                float halfW = (float)Math.Sqrt(Math.Max(0f, 1f - normY * normY)) * r;
                if (halfW < 0.5f) continue;
                Main.spriteBatch.Draw(whitePixelTexture,
                    new Rectangle((int)(center.X - halfW), (int)(center.Y + dy), Math.Max(1, (int)(halfW * 2f)), 1),
                    c);
            }
        }

        private void DrawSphericalBottomFill(float centerX, float topY, float radius, Color color, float opacity)
        {
            if (radius < 2f || opacity < 0.005f) return;
            
            // Draw the sphere as horizontal slices — all slices use the same opacity so no seam lines appear
            Color uniformColor = color * opacity;
            int slices = 16;
            for (int i = 0; i < slices; i++)
            {
                float progress = (float)(i + 1) / slices;
                float angle = progress * MathHelper.PiOver2;

                float sliceWidth = (float)Math.Cos(angle) * radius * 2f;
                float sliceY = topY + (float)Math.Sin(angle) * radius;
                float prevSliceY = topY + (float)Math.Sin((float)i / slices * MathHelper.PiOver2) * radius;

                if (sliceWidth < 2f) continue;
                int sliceH = (int)sliceY - (int)prevSliceY;
                if (sliceH < 1) sliceH = 1;

                Main.spriteBatch.Draw(whitePixelTexture,
                    new Rectangle((int)(centerX - sliceWidth / 2f), (int)prevSliceY, (int)sliceWidth, sliceH),
                    uniformColor);
            }
        }
        
        /// <summary>
        /// Draws a vertical line that tapers off at the top and curves into a sphere at the bottom.
        /// </summary>
        private void DrawTaperedLineWithSphere(float x, float feetY, float bottomExtend, float height, float width, float sphereRadius, Color color, float intensity, bool isLeftLine)
        {
            byte solidAlpha = (byte)(180 * intensity);
            if (solidAlpha >= 2)
            {
                Color solidColor = new Color(color.R, color.G, color.B, solidAlpha);
                // Single solid rect for the full column above feet
                Main.spriteBatch.Draw(whitePixelTexture,
                    new Rectangle((int)x, (int)(feetY - height), (int)width, (int)height),
                    solidColor);
            }

            // Draw straight line below feet (full 4 tiles)
            byte belowAlpha = (byte)(180 * intensity);
            Color belowColor = new Color(color.R, color.G, color.B, belowAlpha);
            Rectangle belowRect = new Rectangle(
                (int)x,
                (int)feetY,
                (int)width,
                (int)bottomExtend
            );
            Main.spriteBatch.Draw(whitePixelTexture, belowRect, belowColor);
            
            // Draw the spherical curve at the bottom
            float bottomY = feetY + bottomExtend;
            float centerX = isLeftLine ? (x + width + sphereRadius) : (x - sphereRadius);
            
            int curveSegments = 8;
            for (int i = 0; i < curveSegments; i++)
            {
                float progress = (float)(i + 1) / curveSegments;
                float angle = progress * MathHelper.PiOver2;
                
                // Calculate position on the quarter circle
                float curveX, curveY;
                if (isLeftLine)
                {
                    // Left line curves inward (to the right)
                    curveX = centerX - (float)Math.Cos(angle) * sphereRadius;
                    curveY = bottomY + (float)Math.Sin(angle) * sphereRadius;
                }
                else
                {
                    // Right line curves inward (to the left)
                    curveX = centerX + (float)Math.Cos(angle) * sphereRadius;
                    curveY = bottomY + (float)Math.Sin(angle) * sphereRadius;
                }
                
                float prevProgress = (float)i / curveSegments;
                float prevAngle = prevProgress * MathHelper.PiOver2;
                float prevY = bottomY + (float)Math.Sin(prevAngle) * sphereRadius;
                
                // Fade out along the curve
                float curveAlpha = 1f - (progress * progress);
                byte alpha = (byte)(180 * intensity * curveAlpha);
                if (alpha < 2) continue;
                
                Color segColor = new Color(color.R, color.G, color.B, alpha);
                
                Rectangle curveRect = new Rectangle(
                    (int)(isLeftLine ? curveX : curveX - width),
                    (int)prevY,
                    (int)width,
                    (int)curveY - (int)prevY
                );

                Main.spriteBatch.Draw(whitePixelTexture, curveRect, segColor);
            }
        }

        /// <summary>
        /// Draws a circular vignette effect centered on the player.
        /// Creates darkness around the edges that fades toward the player's position.
        /// </summary>
        /// <param name="intensity">Effect intensity from 0 (invisible) to 1 (full darkness)</param>
        private void DrawScreenVignette(float intensity)
        {
            if (intensity <= 0f) return;
            
            // Initialize white pixel texture if needed
            if (whitePixelTexture == null || whitePixelTexture.IsDisposed)
            {
                whitePixelTexture = new Texture2D(Main.graphics.GraphicsDevice, 1, 1);
                whitePixelTexture.SetData(new Color[] { Color.White });
            }
            
            
            // Player position on screen (center of the clear area)
            Vector2 playerScreenPos = Player.Center - Main.screenPosition;
            
            // Define the clear radius around player and max darkness radius
            float clearRadius = 35f; // Completely clear area around player (smaller = darkness closer)
            float maxDarkRadius = new Vector2(Main.screenWidth, Main.screenHeight).Length() * 0.4f;
            
            // Draw the vignette using a grid-based approach for smooth circular gradient
            // This divides the screen into cells and draws each with appropriate alpha
            int cellSize = 32; // Larger cells = better performance, smaller = smoother gradient
            int cellsX = (Main.screenWidth / cellSize) + 2;
            int cellsY = (Main.screenHeight / cellSize) + 2;
            
            for (int gridY = 0; gridY < cellsY; gridY++)
            {
                for (int gridX = 0; gridX < cellsX; gridX++)
                {
                    // Calculate cell center position
                    float cellCenterX = gridX * cellSize + cellSize / 2f;
                    float cellCenterY = gridY * cellSize + cellSize / 2f;
                    
                    // Calculate distance from player
                    float dx = cellCenterX - playerScreenPos.X;
                    float dy = cellCenterY - playerScreenPos.Y;
                    float distance = (float)Math.Sqrt(dx * dx + dy * dy);
                    
                    // Skip cells within the clear radius
                    if (distance < clearRadius - cellSize)
                        continue;
                    
                    // Calculate alpha based on distance (0 at clearRadius, max at maxDarkRadius)
                    float fadeRange = maxDarkRadius - clearRadius;
                    float fadeProgress = Math.Max(0f, (distance - clearRadius) / fadeRange);
                    fadeProgress = Math.Min(1f, fadeProgress);
                    
                    // Apply stronger easing for more dramatic falloff
                    float darkness = fadeProgress * fadeProgress * fadeProgress; // Cubic for sharper edge
                    
                    // Add base darkness so even the transition area is darker
                    darkness = 0.15f + (darkness * 0.85f);
                    
                    // Calculate final alpha (200 max = slightly translucent at darkest)
                    byte alpha = (byte)(darkness * 200f * intensity);
                    
                    if (alpha < 3) continue; // Skip nearly invisible cells
                    
                    Color cellColor = new Color(0, 0, 0, alpha);
                    
                    // Draw the cell
                    Rectangle cellRect = new Rectangle(
                        gridX * cellSize,
                        gridY * cellSize,
                        cellSize,
                        cellSize
                    );
                    
                    Main.spriteBatch.Draw(whitePixelTexture, cellRect, cellColor);
                }
            }
        }

        public void StopAllLoopedSounds()
        {
            if (outdoorRainInstance != null)
            {
                outdoorRainInstance.Stop(false);
                outdoorRainInstance = null;
            }
            if (indoorRainInstance != null)
            {
                indoorRainInstance.Stop(false);
                indoorRainInstance = null;
            }
            outdoorCurrentVolume = 0f;
            outdoorTargetVolume = 0f;
            indoorCurrentVolume = 0f;
            indoorTargetVolume = 0f;
            fadingLavaSound?.StopImmediate();
            fadingSandstormSound?.StopImmediate();
            SariaModSystem.CustomRainSoundIsPlaying = false;
        }

        /// <summary>
        /// Called from SariaModSystem to handle focus loss/gain for rain sounds.
        /// This is needed because in single player, PostUpdate doesn't run when unfocused.
        /// </summary>
        public void HandleFocusChange(bool hasFocus)
        {
            if (!hasFocus)
            {
                // Window lost focus - pause any playing sounds
                if (outdoorRainInstance != null && outdoorRainInstance.State == SoundState.Playing)
                    outdoorRainInstance.Pause();
                if (indoorRainInstance != null && indoorRainInstance.State == SoundState.Playing)
                    indoorRainInstance.Pause();
                fadingLavaSound?.Pause();
                fadingSandstormSound?.Pause();
                wasFocusLost = true;
            }
            // Don't handle focus regain here - let HandleRainSoundState do it
            // so the sounds can properly restart with fading
        }
        public override void Initialize()
        {
            playRainSound = false;
            wasPlayingRain = false;
            wasFocusLost = false;
            outdoorCurrentVolume = 0f;
            outdoorTargetVolume = 0f;
            indoorCurrentVolume = 0f;
            indoorTargetVolume = 0f;
            fadingLavaSound = new FadingSoundPlayer(fadeSpeed: 1f / 60f);
            fadingSandstormSound = new FadingSoundPlayer(fadeSpeed: 1f / 60f);
        }

        public override void OnEnterWorld(Player player)
        {
            if (Main.netMode == NetmodeID.Server)
            {
                ModPacket packet = Mod.GetPacket();
                packet.Write((byte)SariaMod.SoundMessageType.SyncRainSoundState);
                packet.Write(playRainSound);
                packet.Send(-1, player.whoAmI);
            }
        }

        public void ReceiveRainSoundState(bool newState)
        {
            if (playRainSound != newState)
            {
                playRainSound = newState;
                if (!playRainSound)
                {
                    StopAllLoopedSounds();
                }
            }
        }

        private bool CheckForMostlyFullWallCoverage(int playerTileX, int playerTileY, int rectangleSize, float percentageRequired)
        {
            int coveredTiles = 0;
            int totalTiles = 0;
            for (int x = playerTileX - rectangleSize; x <= playerTileX + rectangleSize; x++)
            {
                for (int y = playerTileY - rectangleSize; y <= playerTileY + rectangleSize; y++)
                {
                    if (x == playerTileX && (y == playerTileY || y == playerTileY + 1))
                    {
                        continue;
                    }
                    totalTiles++;
                    Tile tile = Framing.GetTileSafely(x, y);
                    if ((tile.HasTile && Main.tileSolid[tile.TileType] && !tile.IsActuated) || tile.WallType > 0)
                    {
                        coveredTiles++;
                    }
                }
            }
            if (totalTiles == 0) return false;
            float coveragePercentage = (float)coveredTiles / totalTiles;
            return coveragePercentage >= percentageRequired;
        }

        /// <summary>
        /// Checks if the player is protected by background walls (for space/environment debuff immunity).
        /// Uses a more lenient check than rain coverage - only requires walls behind the player area.
        /// </summary>
        private static bool CheckForWallProtection(int playerTileX, int playerTileY, int rectangleSize, float percentageRequired)
        {
            int wallTiles = 0;
            int totalTiles = 0;
            for (int x = playerTileX - rectangleSize; x <= playerTileX + rectangleSize; x++)
            {
                for (int y = playerTileY - rectangleSize; y <= playerTileY + rectangleSize; y++)
                {
                    totalTiles++;
                    Tile tile = Framing.GetTileSafely(x, y);
                    // Only count background walls (not solid tiles) for space protection
                    if (tile.WallType > 0)
                    {
                        wallTiles++;
                    }
                }
            }
            if (totalTiles == 0) return false;
            float wallPercentage = (float)wallTiles / totalTiles;
            return wallPercentage >= percentageRequired;
        }

        private float CalculateCoverage(int x, int startY, int endY)
        {
            int coveredTiles = 0;
            int totalTiles = 0;
            for (int y = startY; y <= endY; y++)
            {
                totalTiles++;
                Tile tile = Framing.GetTileSafely(x, y);
                if ((tile.HasTile && Main.tileSolid[tile.TileType] && !tile.IsActuated) || tile.WallType > 0)
                {
                    coveredTiles++;
                }
            }
            if (totalTiles == 0) return 0f;
            return (float)coveredTiles / totalTiles;
        }

        private void HandleRainSoundState(bool isRaining, int closestCeilingTileY, int furthestCeilingTileY, bool reachedMaxSearchHeight)
        {
            if (!Main.hasFocus)
            {
                return;
            }

            // Suppress rain sounds during a sandstorm — the sandstorm sound takes over
            if (Terraria.GameContent.Events.Sandstorm.Happening && Player.ZoneDesert)
            {
                isRaining = false;
            }

            // Handle returning from focus loss - clean up old instances and reset volumes
            if (wasFocusLost)
            {
                wasFocusLost = false;

                // Stop any paused/old sound instances
                if (outdoorRainInstance != null)
                {
                    outdoorRainInstance.Stop(true);
                    outdoorRainInstance = null;
                }
                if (indoorRainInstance != null)
                {
                    indoorRainInstance.Stop(true);
                    indoorRainInstance = null;
                }

                // Also discard paused FadingSoundPlayer instances so they restart with a fade-in
                fadingLavaSound?.StopImmediate();
                fadingSandstormSound?.StopImmediate();

                // Reset current volumes to 0 so sounds will fade back in
                outdoorCurrentVolume = 0f;
                indoorCurrentVolume = 0f;
            }

            if (!isRaining)
            {
                // Fade out both sounds
                outdoorTargetVolume = 0f;
                indoorTargetVolume = 0f;

                UpdateVolumeFades();

                // Stop sounds only when fully faded out
                if (outdoorCurrentVolume <= 0.001f && outdoorRainInstance != null)
                {
                    outdoorRainInstance.Stop(true);
                    outdoorRainInstance = null;
                }
                if (indoorCurrentVolume <= 0.001f && indoorRainInstance != null)
                {
                    indoorRainInstance.Stop(true);
                    indoorRainInstance = null;
                }

                if (outdoorRainInstance == null && indoorRainInstance == null)
                {
                    SariaModSystem.CustomRainSoundIsPlaying = false;
                }
                return;
            }

            int playerTileX = (int)(Player.Center.X / 16f);
            int playerTileY = (int)(Player.Center.Y / 16f);
            const float requiredCoverage = 0.75f;
            const int wallSearchRadius = 2;
            bool isCovered = false;

            if (furthestCeilingTileY != -1)
            {
                float areaCoverageFurthest = CalculateCoverage(playerTileX, furthestCeilingTileY, playerTileY);
                bool isMostlySurrounded = CheckForMostlyFullWallCoverage(playerTileX, playerTileY, wallSearchRadius, requiredCoverage);
                if (areaCoverageFurthest >= requiredCoverage && isMostlySurrounded)
                {
                    isCovered = true;
                }
            }
            if (!isCovered && closestCeilingTileY != -1)
            {
                float areaCoverageClosest = CalculateCoverage(playerTileX, closestCeilingTileY, playerTileY);
                bool isMostlySurrounded = CheckForMostlyFullWallCoverage(playerTileX, playerTileY, wallSearchRadius, requiredCoverage);
                if (areaCoverageClosest >= requiredCoverage && isMostlySurrounded)
                {
                    isCovered = true;
                }
            }

            // Calculate depth-based volume multiplier
            float depthVolumeMult = 1f;
            float surfaceY = (float)Main.worldSurface * 16f;
            float dirtLayerY = (float)Main.rockLayer * 16f;

            if (Player.Center.Y > surfaceY)
            {
                float depthBelowSurface = Player.Center.Y - surfaceY;
                float fadeDistance = (dirtLayerY - surfaceY) * 0.5f;

                if (fadeDistance > 0)
                {
                    depthVolumeMult = 1f - Math.Min(1f, depthBelowSurface / fadeDistance);
                    depthVolumeMult = Math.Max(0.1f, depthVolumeMult);
                }
            }

            bool outsiderain = !isCovered;
            bool insiderain = isCovered;
            
            // Calculate target volumes based on conditions
            if (outsiderain)
            {
                outdoorTargetVolume = 0.3f * depthVolumeMult;
                indoorTargetVolume = 0f;
            }
            else if (insiderain)
            {
                int distanceToCeiling;
                if (reachedMaxSearchHeight)
                {
                    distanceToCeiling = 100;
                }
                else
                {
                    distanceToCeiling = furthestCeilingTileY != -1 ? playerTileY - furthestCeilingTileY : 50;
                }
                
                float minDistanceForMaxVolume = 5f;
                float maxDistanceForMinVolume = 100f;
                float indoorBaseVolume;
                if (distanceToCeiling <= minDistanceForMaxVolume)
                {
                    indoorBaseVolume = 0.2f;
                }
                else if (distanceToCeiling >= maxDistanceForMinVolume)
                {
                    indoorBaseVolume = 0.05f;
                }
                else
                {
                    float scaleFactor = (distanceToCeiling - minDistanceForMaxVolume) / (maxDistanceForMinVolume - minDistanceForMaxVolume);
                    indoorBaseVolume = MathHelper.Lerp(0.2f, 0.05f, scaleFactor);
                }
                
                indoorTargetVolume = indoorBaseVolume * depthVolumeMult;
                outdoorTargetVolume = 0f;
            }
            else
            {
                outdoorTargetVolume = 0f;
                indoorTargetVolume = 0f;
            }
            
            // Ensure outdoor sound instance exists if needed
            if (outdoorTargetVolume > 0 || outdoorCurrentVolume > 0.001f)
            {
                if (outdoorRainInstance == null || outdoorRainInstance.State == SoundState.Stopped)
                {
                    Asset<SoundEffect> asset = ModContent.Request<SoundEffect>("SariaMod/Sounds/Rain", AssetRequestMode.ImmediateLoad);
                    if (asset != null && asset.IsLoaded)
                    {
                        outdoorRainInstance = asset.Value.CreateInstance();
                        if (outdoorRainInstance != null)
                        {
                            outdoorRainInstance.Volume = outdoorCurrentVolume;
                            outdoorRainInstance.IsLooped = true;
                            outdoorRainInstance.Play();
                        }
                    }
                }
            }
            
            // Ensure indoor sound instance exists if needed
            if (indoorTargetVolume > 0 || indoorCurrentVolume > 0.001f)
            {
                if (indoorRainInstance == null || indoorRainInstance.State == SoundState.Stopped)
                {
                    Asset<SoundEffect> asset = ModContent.Request<SoundEffect>("SariaMod/Sounds/RainIndoors", AssetRequestMode.ImmediateLoad);
                    if (asset != null && asset.IsLoaded)
                    {
                        indoorRainInstance = asset.Value.CreateInstance();
                        if (indoorRainInstance != null)
                        {
                            indoorRainInstance.Volume = indoorCurrentVolume;
                            indoorRainInstance.IsLooped = true;
                            indoorRainInstance.Play();
                        }
                    }
                }
            }
            
            // Update volume fading
            UpdateVolumeFades();
            
            // Stop sounds that have fully faded out
            if (outdoorCurrentVolume <= 0.001f && outdoorTargetVolume <= 0 && outdoorRainInstance != null)
            {
                outdoorRainInstance.Stop(true);
                outdoorRainInstance = null;
            }
            if (indoorCurrentVolume <= 0.001f && indoorTargetVolume <= 0 && indoorRainInstance != null)
            {
                indoorRainInstance.Stop(true);
                indoorRainInstance = null;
            }
            
            SariaModSystem.CustomRainSoundIsPlaying = (outdoorRainInstance != null || indoorRainInstance != null);
            
            // Thunder
            if (Main.rand.NextBool(600) && (insiderain || outsiderain))
            {
                string thunderPath = Main.rand.Next(5) switch
                {
                    0 => "SariaMod/Sounds/Thunder1",
                    1 => "SariaMod/Sounds/Thunder2",
                    2 => "SariaMod/Sounds/Thunder3",
                    3 => "SariaMod/Sounds/Thunder4",
                    4 => "SariaMod/Sounds/ThunderThighs",
                    _ => ""
                };
                if (!string.IsNullOrEmpty(thunderPath))
                {
                    Asset<SoundEffect> thunderAsset = ModContent.Request<SoundEffect>(thunderPath);
                    if (thunderAsset != null && thunderAsset.IsLoaded)
                    {
                        SoundEffectInstance thunderInstance = thunderAsset.Value.CreateInstance();
                        if (thunderInstance != null)
                        {
                            thunderInstance.Volume = Main.ambientVolume;
                            thunderInstance.Play();
                        }
                    }
                }
            }
        }
        
        private void UpdateVolumeFades()
        {
            // Fade outdoor volume
            if (outdoorCurrentVolume < outdoorTargetVolume)
            {
                outdoorCurrentVolume = Math.Min(outdoorCurrentVolume + FADE_SPEED, outdoorTargetVolume);
            }
            else if (outdoorCurrentVolume > outdoorTargetVolume)
            {
                outdoorCurrentVolume = Math.Max(outdoorCurrentVolume - FADE_SPEED, outdoorTargetVolume);
            }
            
            // Fade indoor volume
            if (indoorCurrentVolume < indoorTargetVolume)
            {
                indoorCurrentVolume = Math.Min(indoorCurrentVolume + FADE_SPEED, indoorTargetVolume);
            }
            else if (indoorCurrentVolume > indoorTargetVolume)
            {
                indoorCurrentVolume = Math.Max(indoorCurrentVolume - FADE_SPEED, indoorTargetVolume);
            }
            
            // Apply volumes to sound instances
            if (outdoorRainInstance != null && outdoorRainInstance.State == SoundState.Playing)
            {
                outdoorRainInstance.Volume = outdoorCurrentVolume * Main.ambientVolume;
            }
            if (indoorRainInstance != null && indoorRainInstance.State == SoundState.Playing)
            {
                indoorRainInstance.Volume = indoorCurrentVolume * Main.ambientVolume;
            }
        }
        public override void PostUpdate()
        {
            if (Player.whoAmI != Main.myPlayer)
            {
                return;
            }
            
            // Rain sounds should only play when:
            // 1. Player is in a rain zone and not in snow
            // 2. Player is NOT underground (underground background is not active)
            // 3. Player is not in an open cave area (no walls behind them below surface)
            // 
            // Underground backgrounds are active in: ZoneDirtLayerHeight, ZoneRockLayerHeight, ZoneUnderworldHeight
            // The basic rock/dirt cave background shows when player has no wall behind them and is below surface level
            bool isUnderground = Player.ZoneDirtLayerHeight || Player.ZoneRockLayerHeight || Player.ZoneUnderworldHeight;
            
            // Check if player is in an open cave (no background wall) below the surface
            // This shows the basic rock/dirt background even at relatively shallow depths
            bool isInOpenCave = !Player.behindBackWall && (Player.Center.Y > Main.worldSurface * 16);
            
            bool isRaining = Player.ZoneRain && !Player.ZoneSnow && !isUnderground && !isInOpenCave;
            
            if (Main.netMode == NetmodeID.Server)
            {
                if (wasPlayingRain != isRaining)
                {
                    ModPacket packet = Mod.GetPacket();
                    packet.Write((byte)SariaMod.SoundMessageType.SyncRainSoundState);
                    packet.Write(isRaining);
                    packet.Send();
                    wasPlayingRain = isRaining;
                }
            }
            playRainSound = isRaining;
            int playerTileX = (int)(Player.Center.X / 16f);
            int playerTileY = (int)(Player.Center.Y / 16f);
            int maxSearchHeight = 120;
            int closestCeilingTileY = -1;
            int furthestCeilingTileY = -1;
            bool reachedMaxSearchHeight = false; // Track if we searched the entire height without finding a sky gap
            
            for (int y = playerTileY - 1; y > playerTileY - maxSearchHeight; y--)
            {
                Tile tile = Framing.GetTileSafely(playerTileX, y);
                if (tile.HasTile && Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType])
                {
                    closestCeilingTileY = y;
                    break;
                }
            }
            if (closestCeilingTileY != -1)
            {
                furthestCeilingTileY = closestCeilingTileY;
                int consecutiveBlanks = 0;
                bool foundSkyGap = false;
                for (int y = closestCeilingTileY - 1; y > playerTileY - maxSearchHeight; y--)
                {
                    Tile tile = Framing.GetTileSafely(playerTileX, y);
                    if (!tile.HasTile && tile.WallType == 0)
                    {
                        consecutiveBlanks++;
                        if (consecutiveBlanks >= 5)
                        {
                            furthestCeilingTileY = y + 5;
                            foundSkyGap = true;
                            break;
                        }
                    }
                    else
                    {
                        consecutiveBlanks = 0;
                    }
                }
                // If we searched all the way up and never found a sky gap, we're deep underground
                reachedMaxSearchHeight = !foundSkyGap;
            }
            HandleRainSoundState(playRainSound, closestCeilingTileY, furthestCeilingTileY, reachedMaxSearchHeight);

            if (++_sariaProximityBuffTimer >= 10)
            {
                _sariaProximityBuffTimer = 0;
                ApplySariaProximityBuffs();
            }
        }

        private void ApplySariaProximityBuffs()
        {
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];
                if (!proj.active || proj.type != ModContent.ProjectileType<Saria>())
                    continue;
                if (!(proj.ModProjectile is Saria saria))
                    continue;

                float dist = Vector2.Distance(Player.Center, proj.Center);

                // Transform 2 (fire form): warm nearby players within 500 units
                if (saria.Transform == 2 && dist < 500f)
                {
                    Player.resistCold = true;
                    Player.AddBuff(BuffID.Warmth, 20);
                }

                // Transform 3 (lightning form): electrify nearby players if Saria is in water
                if (saria.Transform == 3 && dist < 500f)
                {
                    bool sariaWet = Collision.WetCollision(proj.position, proj.width / 2, proj.height / 2)
                                 && !Collision.LavaCollision(proj.position, proj.width / 2, proj.height / 2);
                    if (sariaWet)
                        Player.AddBuff(BuffID.Electrified, 20);
                }
            }
        }

        private static void MiscEffects(Player player, FairyPlayer modPlayer, Mod mod, FairyPlayerMiscEffects miscEffects)
        {
            Player player2 = Main.LocalPlayer;
            if (player.statLife < (player.statLifeMax2 / 2))
            {
                player.QuickHeal();
            }

            if (player.ownedProjectileCounts[ModContent.ProjectileType<BufferProj>()] > 0f)
            {
                if (player.velocity.Y > 1)
                {
                    player.velocity.Y = -10;
                }
            }
            if (player.ownedProjectileCounts[ModContent.ProjectileType<BufferProj>()] > 0f || player.ownedProjectileCounts[ModContent.ProjectileType<Sweetspot4>()] > 0f)
            {
                player.controlDown = false;
                player.controlLeft = false;
                player.controlRight = false;
                player.controlMount = false;
                player.controlJump = false;
                player.controlHook = false;
            }
            if (player.ownedProjectileCounts[ModContent.ProjectileType<Emeraldspike>()] > 0f || player.ownedProjectileCounts[ModContent.ProjectileType<Emeraldspike2>()] > 0f || (player.ownedProjectileCounts[ModContent.ProjectileType<Emeraldspike3>()] > 0f && player.ownedProjectileCounts[ModContent.ProjectileType<BufferProj>()] <= 0f))
            {
                player.maxFallSpeed = 30f;
                if (player.velocity.Y < 1)
                {
                    player.velocity.Y = 10;
                }
                player.moveSpeed = 0;
                player.wingAccRunSpeed = 0;
            }
            switch (modPlayer.Sarialevel)
            {
                case 0:
                    modPlayer.TMPoints = 0;
                    break;
                case 1:
                    modPlayer.TMPoints = 1 - modPlayer.TMPointsUsed;
                    break;
                case 2:
                    modPlayer.TMPoints = 3 - modPlayer.TMPointsUsed;
                    break;
                case 3:
                    modPlayer.TMPoints = 6 - modPlayer.TMPointsUsed;
                    break;
                case 4:
                    modPlayer.TMPoints = 9 - modPlayer.TMPointsUsed;
                    break;
                case 5:
                    modPlayer.TMPoints = 10 - modPlayer.TMPointsUsed;
                    break;
                case 6:
                    modPlayer.TMPoints = 13 - modPlayer.TMPointsUsed;
                    break;
                default:
                    modPlayer.TMPoints = -modPlayer.TMPointsUsed;
                    break;
            }
            switch (modPlayer.Sarialevel)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                    int[] xpThresholds = sarialevelXpThresholds[modPlayer.Sarialevel];
                    for (int i = 0; i < xpThresholds.Length; i++)
                    {
                        if (modPlayer.SariaXp <= xpThresholds[i])
                        {
                            modPlayer.XPBarLevel = i;
                            break;
                        }
                    }
                    if (modPlayer.SariaXp > xpThresholds[xpThresholds.Length - 1])
                    {
                        modPlayer.XPBarLevel = 8;
                        modPlayer.SariaXp = xpThresholds[xpThresholds.Length - 1] + 1;
                    }
                    break;
                case 6:
                    modPlayer.XPBarLevel = 8;
                    break;
                default:
                    modPlayer.XPBarLevel = 0;
                    modPlayer.SariaXp = 0;
                    break;
            }
            if (Soundtimer > 0)
            {
                Soundtimer--;
            }
            LavaSoundTimer++;
            if (Main.gameMenu)
            {
                miscEffects.fadingLavaSound.StopImmediate();
                miscEffects.fadingSandstormSound.StopImmediate();
            }
            else
            {
            if (player.whoAmI == Main.myPlayer)
            {
            miscEffects.fadingLavaSound.Update();
            if (player.ZoneUnderworldHeight)
            {
                if (Main.rand.NextBool(5000))
                {
                    SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Cave10"));
                }
                miscEffects.fadingLavaSound.Play("SariaMod/Sounds/LavaSound", fadeIn: true);
                if (Main.rand.NextBool(1))
                {
                    float radius = (float)Math.Sqrt(Main.rand.Next(3000 * 3000));
                    double angle = Main.rand.NextDouble() * 2.0 * Math.PI;
                    Dust.NewDust(new Vector2(player.Center.X + radius * (float)Math.Cos(angle), player.Center.Y + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<SmokeDust>(), 0f, 0f, 0, default(Color), 1.5f);
                }
            }
            else
            {
                miscEffects.fadingLavaSound.Stop();
            }
            miscEffects.fadingSandstormSound.Update();
            if (Terraria.GameContent.Events.Sandstorm.Happening && player.ZoneDesert)
            {
                // Re-check indoor status every 30 seconds (1800 ticks)
                miscEffects._sandstormIndoorCheckTimer++;
                if (miscEffects._sandstormIndoorCheckTimer >= 1800 || miscEffects._sandstormIndoorCheckTimer == 1)
                {
                    miscEffects._sandstormIndoorCheckTimer = 0;
                    int ptx = (int)(player.Center.X / 16f);
                    int pty = (int)(player.Center.Y / 16f);
                    miscEffects._sandstormIndoors = miscEffects.CheckForMostlyFullWallCoverage(ptx, pty, 2, 0.75f);
                }
                miscEffects.fadingSandstormSound.TargetVolume = miscEffects._sandstormIndoors ? 0.25f : 1f;
                miscEffects.fadingSandstormSound.Play("SariaMod/Sounds/Sandstorm", fadeIn: true);
            }
            else
            {
                miscEffects._sandstormIndoorCheckTimer = 0;
                miscEffects._sandstormIndoors = false;
                miscEffects.fadingSandstormSound.Stop();
            }
            } // end owner check
            } // end else (not gameMenu)
            float sneezespot = 5;
            bool Warm = (player.behindBackWall && player.HasBuff(BuffID.Campfire));
            bool immunityToCold = player.HasBuff(BuffID.Warmth) || player.HasBuff(BuffID.OnFire) || player.arcticDivingGear || player.HasBuff(ModContent.BuffType<WillOWispBuff>());
            bool immunityToHeat = player.HasBuff(BuffID.ObsidianSkin) || player.lavaImmune || player.ZoneWaterCandle || player.HasBuff(ModContent.BuffType<Veil>());
            if (player.whoAmI == Main.myPlayer)
            {
                player.buffImmune[ModContent.BuffType<Frostburn2>()] = false;
                player.buffImmune[ModContent.BuffType<Frozen2>()] = false;
                player.buffImmune[ModContent.BuffType<Burning2>()] = false;
                if (player.ZoneSnow && Main.raining && (!immunityToCold || (player.HasBuff(ModContent.BuffType<StatLower>()) && !Warm)))
                {
                    modPlayer.FreezingTemp++;
                    if (!player.behindBackWall)
                    {
                        modPlayer.FreezingTemp++;
                        player.AddBuff(ModContent.BuffType<Frostburn2>(), 2);
                    }
                }
                if (player.ZoneSnow && player.wet && !immunityToCold)
                {
                    modPlayer.FreezingTemp += 3;
                    {
                        modPlayer.FreezingTemp++;
                        player.AddBuff(ModContent.BuffType<Frostburn2>(), 2);
                    }
                }
                if (modPlayer.FreezingTemp >= 3000)
                {
                    player.AddBuff(ModContent.BuffType<Frozen2>(), 398);
                    SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/HardIce"), player.Center);
                    modPlayer.FreezingTemp = 0;
                }
            }
            if (immunityToCold && modPlayer.FreezingTemp > 0)
            {
                modPlayer.FreezingTemp--;
            }
            if (player.whoAmI == Main.myPlayer)
            {
                if (!player.behindBackWall && (!immunityToHeat || (player.HasBuff(ModContent.BuffType<StatLower>()) && !player.HasBuff(ModContent.BuffType<Veil>()))) && player.ZoneUnderworldHeight)
                {
                    player.AddBuff(ModContent.BuffType<Burning2>(), 2, quiet: false);
                }
            }
            // For space debuffs, use a more robust wall check that looks at multiple tiles around the player
            // This prevents debuffs from applying when player is inside a structure with walls
            int spaceTileX = (int)(player.Center.X / 16f);
            int spaceTileY = (int)(player.Center.Y / 16f);
            bool isProtectedByWalls = CheckForWallProtection(spaceTileX, spaceTileY, 2, 0.5f);

            if (!isProtectedByWalls && (!immunityToCold || (player.HasBuff(ModContent.BuffType<StatLower>()) && (!Warm && !immunityToCold))) && player.InSpace())
            {
                player.AddBuff(ModContent.BuffType<Frostburn3>(), 2, quiet: false);
            }
            if (!isProtectedByWalls && (!immunityToHeat || (player.HasBuff(ModContent.BuffType<StatLower>()) && !player.HasBuff(ModContent.BuffType<Veil>()))) && player.InSpace())
            {
                player.AddBuff(ModContent.BuffType<Burning2>(), 2, quiet: false);
            }
            if (player.whoAmI == Main.myPlayer)
            {
                bool shouldShowFog = ((player.active && player.ZoneSnow) && !(player.behindBackWall && player.HasBuff(BuffID.Campfire))) || ((player.active && player.ZoneSkyHeight) && !(player.behindBackWall && player.HasBuff(BuffID.Campfire))) || ((player.active && player.ZoneDesert && !Main.dayTime) && !(player.behindBackWall && player.HasBuff(BuffID.Campfire))) || ((player.active && player.ZoneRain && !player.ZoneJungle && !(player.ZoneDesert && Main.dayTime)) && !(player.behindBackWall && player.HasBuff(BuffID.Campfire)));

                // Sync to other clients if it changed and cooldown has expired
                if (modPlayer.FogBreathPacketCooldown > 0)
                    modPlayer.FogBreathPacketCooldown--;

                if (modPlayer.ShowFogBreath != shouldShowFog && modPlayer.FogBreathPacketCooldown <= 0)
                {
                    modPlayer.ShowFogBreath = shouldShowFog;
                    if (Main.netMode != NetmodeID.SinglePlayer)
                    {
                        ModPacket packet = mod.GetPacket();
                        packet.Write((byte)SariaMod.SoundMessageType.SyncFogBreath);
                        packet.Write((byte)player.whoAmI);
                        packet.Write(shouldShowFog);
                        packet.Send();
                        modPlayer.FogBreathPacketCooldown = 120;
                    }
                }

                if (modPlayer.ShowFogBreath)
                {
                    if (player.velocity.X <= 1)
                    {
                        if (Main.rand.NextBool(50))
                        {
                            float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius3 * sphereRadius3));
                            double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                            if (player.direction > 0) sneezespot = 10;
                            if (player.direction < 0) sneezespot = -8;
                            for (int j = 0; j < 2; j++)
                                Dust.NewDust(new Vector2((player.Center.X + sneezespot) + radius * (float)Math.Cos(angle), (player.Center.Y - 10) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<Fog>(), 0f, 0f, 0, default(Color), 1.5f);
                        }
                    }
                    else if (player.velocity.X > 1)
                    {
                        if (Main.rand.NextBool(10))
                        {
                            float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius3 * sphereRadius3));
                            double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                            if (player.direction > 0) sneezespot = 10;
                            if (player.direction < 0) sneezespot = -8;
                            for (int j = 0; j < 2; j++)
                                Dust.NewDust(new Vector2((player.Center.X + sneezespot) + radius * (float)Math.Cos(angle), (player.Center.Y - 10) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<Fog>(), 0f, 0f, 0, default(Color), 1.5f);
                        }
                    }
                }
            }
            else if (modPlayer.ShowFogBreath)
            {
                // Remote player - spawn fog based on synced bool
                if (player.velocity.X <= 1)
                {
                    if (Main.rand.NextBool(50))
                    {
                        float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius3 * sphereRadius3));
                        double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                        float remoteSneezespot = player.direction > 0 ? 10f : -8f;
                        for (int j = 0; j < 2; j++)
                            Dust.NewDust(new Vector2((player.Center.X + remoteSneezespot) + radius * (float)Math.Cos(angle), (player.Center.Y - 10) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<Fog>(), 0f, 0f, 0, default(Color), 1.5f);
                    }
                }
                else if (player.velocity.X > 1)
                {
                    if (Main.rand.NextBool(10))
                    {
                        float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius3 * sphereRadius3));
                        double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                        float remoteSneezespot = player.direction > 0 ? 10f : -8f;
                        for (int j = 0; j < 2; j++)
                            Dust.NewDust(new Vector2((player.Center.X + remoteSneezespot) + radius * (float)Math.Cos(angle), (player.Center.Y - 10) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<Fog>(), 0f, 0f, 0, default(Color), 1.5f);
                    }
                }
            }
        }
    }
}
