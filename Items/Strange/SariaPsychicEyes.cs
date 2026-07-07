using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace SariaMod.Items.Strange
{
    /// <summary>
    /// Generates and caches eye-only overlay textures where the two red eye
    /// colors are replaced with pink, then draws them on top of the normal
    /// face during charging (fade-in) and attacks (rapid fade-in, ~1 s fade-out).
    /// Also handles blood-sneeze overlays that swap a different red with light blue.
    /// Form 7 (Transform 6) is completely excluded — it has its own separate eye sheet.
    /// Form 6 (Transform 5) uses its own two pink source colors from ColorsForEyes6.png.
    /// Also emits a subtle pink light and small trailing when moving.
    /// </summary>
    public class SariaPsychicEyes : ModSystem
    {
        // Source eye colors (left side of ColorsForEyes.png)
        private static readonly Color SourceColor1 = new Color(232, 56, 32, 255);   // bright red
        private static readonly Color SourceColor2 = new Color(152, 24, 8, 255);    // dark red

        // Alt source eye colors (idle eye sheets use slightly different reds)
        private static readonly Color AltSourceColor1 = new Color(238, 51, 26, 255);  // bright red (idle variant)
        private static readonly Color AltSourceColor2 = new Color(153, 22, 6, 255);   // dark red (idle + attack variant)

        // Fire form (form 3) dark eye color — baked into 3SariaAnimations textures
        private static readonly Color FireFormDarkEye = new Color(227, 106, 11, 255);  // orange (fire form dark eye)

        // Form 6 source eye colors (from ColorsForEyes6.png)
        private static readonly Color Form6SourceColor1 = new Color(238, 26, 86, 255);  // bright pink (form 6 bright eye)
        private static readonly Color Form6SourceColor2 = new Color(153, 6, 46, 255);   // dark pink (form 6 dark eye)

        // Replacement eye colors (right side of ColorsForEyes.png) — bright & heavy pink
        private static readonly Color ReplacementColor1 = new Color(242, 155, 214, 255); // bright pink
        private static readonly Color ReplacementColor2 = new Color(235, 82, 210, 255);  // dark pink

        // Timing
        private const float FadeInTicks = 30f;      // ~0.5 seconds to fully appear while charging
        private const float AttackFadeInTicks = 15f; // ~0.25 seconds to fully appear during attack
        private const float FadeOutTicks = 360f;     // ~6 seconds to dim out after charge/attack ends

        // Pulse effect
        private const float PulseSpeed = 0.04f;
        private const float PulseMin = 0.7f;
        private const float PulseMax = 1.0f;

        // Cached eye-only overlay textures keyed by the original face texture reference
        private static readonly Dictionary<Texture2D, Texture2D> _eyeOverlayCache = new Dictionary<Texture2D, Texture2D>();

        // Cached form 6 eye-only overlay textures (separate cache — form 6 uses pink source colors)
        private static readonly Dictionary<Texture2D, Texture2D> _eyeOverlayCacheForm6 = new Dictionary<Texture2D, Texture2D>();

        // Form 7 eye color swap map — loaded from PsychicEyesForm7.png at PostSetupContent
        private static Dictionary<Color, Color> _form7EyeMap;

        // Cached form 7 eye-only overlay textures (3 source colors, bg + pupil handled separately)
        private static readonly Dictionary<Texture2D, Texture2D> _eyeOverlayCacheForm7 = new Dictionary<Texture2D, Texture2D>();

        // Per-projectile tracked opacity (survives charge → attack transition)
        private static readonly Dictionary<int, float> _eyeOpacity = new Dictionary<int, float>();

        // Per-projectile trail movement factor (smoothly ramps 0→1 when moving, 1→0 when stopped)
        private static readonly Dictionary<int, float> _trailFade = new Dictionary<int, float>();

        // Per-projectile pulse timer (increments each tick)
        private static readonly Dictionary<int, float> _pulseTimer = new Dictionary<int, float>();

        // Blood sneeze eye colors (from ColorsForEyes.png — find light blue in sprites, replace with red)
        private static readonly Color BloodSneezeSource = new Color(133, 247, 249, 255);      // light blue (normal eye color in face textures)
        private static readonly Color BloodSneezeReplacement = new Color(204, 34, 11, 255);   // red (blood sneeze color)

        // Cached blood-sneeze eye-only overlay textures
        private static readonly Dictionary<Texture2D, Texture2D> _bloodSneezeOverlayCache = new Dictionary<Texture2D, Texture2D>();

        public override void PostSetupContent()
        {
            if (Main.dedServ)
                return;

            _form7EyeMap = TextureColorSwap.LoadSwapMap("SariaMod/PsychicEyesForm7");
        }

        public override void Unload()
        {
            foreach (var tex in _eyeOverlayCache.Values)
                tex?.Dispose();
            _eyeOverlayCache.Clear();

            foreach (var tex in _eyeOverlayCacheForm6.Values)
                tex?.Dispose();
            _eyeOverlayCacheForm6.Clear();

            foreach (var tex in _eyeOverlayCacheForm7.Values)
                tex?.Dispose();
            _eyeOverlayCacheForm7.Clear();
            _form7EyeMap = null;

            _eyeOpacity.Clear();
            _trailFade.Clear();
            _pulseTimer.Clear();

            foreach (var tex in _bloodSneezeOverlayCache.Values)
                tex?.Dispose();
            _bloodSneezeOverlayCache.Clear();
        }

        /// <summary>
        /// Returns a pulse multiplier (PulseMin..PulseMax) based on the per-projectile timer.
        /// </summary>
        private static float GetPulse(int projectileId)
        {
            if (!_pulseTimer.TryGetValue(projectileId, out float t))
                return 1f;
            float sine = (float)Math.Sin(t) * 0.5f + 0.5f; // 0..1
            return MathHelper.Lerp(PulseMin, PulseMax, sine);
        }

        /// <summary>
        /// Call once per tick from AI to update the eye overlay opacity.
        /// During charging: fades in over <see cref="FadeInTicks"/>.
        /// During attacks (frames 44-55): rapid fade-in over <see cref="AttackFadeInTicks"/>.
        /// During flash barrier cooldown: holds at full opacity.
        /// After activity ends: fades out over <see cref="FadeOutTicks"/> (~4 seconds).
        /// Forms 6 and 7 (Transform 5/6) are completely excluded.
        /// </summary>
        public static void UpdateOpacity(Projectile projectile, int channelState, int transform, bool flashCooldownActive = false)
        {
            int id = projectile.whoAmI;

            if (!_eyeOpacity.TryGetValue(id, out float current))
                current = 0f;

            bool isAttacking = projectile.frame >= 44 && projectile.frame <= 55;

            if (flashCooldownActive)
            {
                // Flash barrier cooldown: snap to full — eyes stay lit the entire duration
                current = 1f;
            }
            else if (channelState > 0)
            {
                // Charging: snap to the charge-progress value
                current = MathHelper.Clamp(channelState / FadeInTicks, 0f, 1f);
            }
            else if (isAttacking)
            {
                // Attacking: rapid fade-in
                current = Math.Min(1f, current + 1f / AttackFadeInTicks);
            }
            else if (current > 0f)
            {
                // Neither charging nor attacking — fade out over ~4 seconds
                current = Math.Max(0f, current - 1f / FadeOutTicks);
            }

            _eyeOpacity[id] = current;

            // Trail movement factor — smoothly ramps with velocity
            float speed = projectile.velocity.Length();
            float trailTarget = speed > 0.3f ? 1f : 0f;
            if (!_trailFade.TryGetValue(id, out float trailCurrent))
                trailCurrent = 0f;
            // Fade in quickly when moving, fade out slowly so the trail fully dissolves before disappearing
            float trailLerpRate = trailTarget > trailCurrent ? 0.14f : 0.03f;
            _trailFade[id] = MathHelper.Lerp(trailCurrent, trailTarget, trailLerpRate);

            // Pulse timer
            if (!_pulseTimer.TryGetValue(id, out float pulseT))
                pulseT = 0f;
            _pulseTimer[id] = pulseT + PulseSpeed;

            // Cleanup inactive projectiles
            if (!projectile.active)
            {
                _eyeOpacity.Remove(id);
                _trailFade.Remove(id);
                _pulseTimer.Remove(id);
            }
        }

        /// <summary>
        /// Gets or creates an eye-only overlay texture for the given face texture.
        /// Matches all four red eye color variants (main + idle sheets) and replaces
        /// with pink; everything else is transparent.
        /// </summary>
        /// <summary>
        /// Returns the current psychic eye opacity (0–1) for the given projectile.
        /// </summary>
        public static float GetOpacity(int whoAmI)
        {
            _eyeOpacity.TryGetValue(whoAmI, out float opacity);
            return opacity;
        }

        public static Texture2D GetEyeOverlay(Texture2D originalFace)
        {
            if (_eyeOverlayCache.TryGetValue(originalFace, out var cached))
                return cached;

            Color[] pixels = new Color[originalFace.Width * originalFace.Height];
            originalFace.GetData(pixels);

            Color[] overlayPixels = new Color[pixels.Length];
            for (int i = 0; i < pixels.Length; i++)
            {
                Color p = pixels[i];
                if ((p.R == SourceColor1.R && p.G == SourceColor1.G && p.B == SourceColor1.B && p.A == SourceColor1.A) ||
                    (p.R == AltSourceColor1.R && p.G == AltSourceColor1.G && p.B == AltSourceColor1.B && p.A == AltSourceColor1.A))
                    overlayPixels[i] = ReplacementColor1;
                else if ((p.R == SourceColor2.R && p.G == SourceColor2.G && p.B == SourceColor2.B && p.A == SourceColor2.A) ||
                         (p.R == AltSourceColor2.R && p.G == AltSourceColor2.G && p.B == AltSourceColor2.B && p.A == AltSourceColor2.A) ||
                         (p.R == FireFormDarkEye.R && p.G == FireFormDarkEye.G && p.B == FireFormDarkEye.B && p.A == FireFormDarkEye.A))
                    overlayPixels[i] = ReplacementColor2;
                else
                    overlayPixels[i] = Color.Transparent;
            }

            Texture2D overlay = new Texture2D(Main.graphics.GraphicsDevice, originalFace.Width, originalFace.Height);
            overlay.SetData(overlayPixels);

            _eyeOverlayCache[originalFace] = overlay;
            return overlay;
        }

        /// <summary>
        /// Draws the psychic eye overlay using the tracked per-projectile opacity.
        /// Self-illuminated (ignores world lighting), pulsing, with a soft bloom pass.
        /// Includes a subtle pink light and small trailing when moving.
        /// </summary>
        public static void DrawPsychicEyeOverlay(Projectile projectile, Texture2D faceTexture, int transform = 0)
        {
            if (!_eyeOpacity.TryGetValue(projectile.whoAmI, out float opacity) || opacity <= 0f)
                return;

            float pulse = GetPulse(projectile.whoAmI);
            float finalOpacity = opacity * pulse;

            Texture2D overlay = transform == 6 ? GetEyeOverlayForm7(faceTexture)
                              : transform == 5 ? GetEyeOverlayForm6(faceTexture)
                              : GetEyeOverlay(faceTexture);

            Vector2 startPos = projectile.Center - Main.screenPosition + new Vector2(0f, projectile.gfxOffY);
            int frameHeight = overlay.Height / Main.projFrames[projectile.type];
            int frameY = frameHeight * projectile.frame;
            Rectangle rectangle = new Rectangle(0, frameY, overlay.Width, frameHeight);
            Vector2 origin = rectangle.Size() / 2f;

            startPos.Y += 1; // matches SariaMaindraw startPosY=1

            SpriteEffects spriteEffects = SpriteEffects.None;
            if (projectile.spriteDirection == -1)
                spriteEffects = SpriteEffects.FlipHorizontally;

            // Pink light from the eyes
            if (finalOpacity > 0.1f)
            {
                Vector2 lightPos = projectile.Center + new Vector2(0f, -8f);
                Lighting.AddLight(lightPos, new Vector3(0.55f, 0.1f, 0.4f) * finalOpacity * 0.55f);
            }

            // Smooth light trail when moving (drawn behind the main overlay)
            if (!_trailFade.TryGetValue(projectile.whoAmI, out float trailFade))
                trailFade = 0f;

            if (trailFade > 0f && finalOpacity > 0f)
            {
                int trailSegments = Math.Min(6, projectile.oldPos.Length - 1);
                for (int i = 1; i <= trailSegments; i++)
                {
                    if (i >= projectile.oldPos.Length || projectile.oldPos[i] == Vector2.Zero)
                        continue;

                    // Smoothly interpolate between adjacent old positions for a fluid trail
                    Vector2 segCenter = projectile.oldPos[i] + projectile.Size * 0.5f;
                    if (i > 1 && projectile.oldPos[i - 1] != Vector2.Zero)
                    {
                        Vector2 prevCenter = projectile.oldPos[i - 1] + projectile.Size * 0.5f;
                        segCenter = Vector2.Lerp(prevCenter, segCenter, 0.5f);
                    }
                    Vector2 trailPos = segCenter - Main.screenPosition + new Vector2(0f, projectile.gfxOffY);
                    trailPos.Y += 1;

                    // Smooth cubic falloff for a natural light-trail look
                    float t = (float)i / (trailSegments + 1f);
                    float segmentFade = (1f - t) * (1f - t);
                    float trailAlpha = finalOpacity * trailFade * segmentFade * 0.7f;
                    float trailScale = projectile.scale * (1f - t * 0.08f);

                    // Self-illuminated pink-tinted trail
                    Color trailColor = Color.Lerp(Color.White, new Color(255, 140, 220), t * 0.6f) * trailAlpha;
                    Main.spriteBatch.Draw(overlay, trailPos, rectangle, trailColor, projectile.rotation, origin, trailScale, spriteEffects, 0f);
                }
            }

            // Bloom pass — slightly larger, pink-tinted, drawn behind the main overlay
            Color bloomColor = new Color(255, 140, 220) * finalOpacity * 0.35f;
            Main.spriteBatch.Draw(overlay, startPos, rectangle, bloomColor, projectile.rotation, origin, projectile.scale * 1.08f, spriteEffects, 0f);

            // Main overlay — self-illuminated (no GetAlpha)
            Color drawColor = Color.White * finalOpacity;
            Main.spriteBatch.Draw(overlay, startPos, rectangle, drawColor, projectile.rotation, origin, projectile.scale, spriteEffects, 0f);
        }

        /// <summary>
        /// Gets or creates a form 7 eye-only overlay texture.
        /// Checks all three form 7 source colors; colors not present on the texture
        /// simply contribute no pixels (transparent), so bg and pupil sheets can each
        /// carry any subset of the three colors without error.
        /// </summary>
        public static Texture2D GetEyeOverlayForm7(Texture2D originalFace)
        {
            if (_eyeOverlayCacheForm7.TryGetValue(originalFace, out var cached))
                return cached;

            Color[] pixels = new Color[originalFace.Width * originalFace.Height];
            originalFace.GetData(pixels);

            Color[] overlayPixels = new Color[pixels.Length];
            var map = _form7EyeMap;
            for (int i = 0; i < pixels.Length; i++)
            {
                if (map != null && map.TryGetValue(pixels[i], out Color replacement))
                    overlayPixels[i] = replacement;
                else
                    overlayPixels[i] = Color.Transparent;
            }

            Texture2D overlay = new Texture2D(Main.graphics.GraphicsDevice, originalFace.Width, originalFace.Height);
            overlay.SetData(overlayPixels);

            _eyeOverlayCacheForm7[originalFace] = overlay;
            return overlay;
        }

        /// <summary>
        /// Gets or creates a form 6 eye-only overlay texture.
        /// Detects the two pink source colors from ColorsForEyes6.png and replaces
        /// with the standard pink replacement colors; everything else is transparent.
        /// </summary>
        public static Texture2D GetEyeOverlayForm6(Texture2D originalFace)
        {
            if (_eyeOverlayCacheForm6.TryGetValue(originalFace, out var cached))
                return cached;

            Color[] pixels = new Color[originalFace.Width * originalFace.Height];
            originalFace.GetData(pixels);

            Color[] overlayPixels = new Color[pixels.Length];
            for (int i = 0; i < pixels.Length; i++)
            {
                Color p = pixels[i];
                if (p.R == Form6SourceColor1.R && p.G == Form6SourceColor1.G && p.B == Form6SourceColor1.B && p.A == Form6SourceColor1.A)
                    overlayPixels[i] = ReplacementColor1;
                else if (p.R == Form6SourceColor2.R && p.G == Form6SourceColor2.G && p.B == Form6SourceColor2.B && p.A == Form6SourceColor2.A)
                    overlayPixels[i] = ReplacementColor2;
                else
                    overlayPixels[i] = Color.Transparent;
            }

            Texture2D overlay = new Texture2D(Main.graphics.GraphicsDevice, originalFace.Width, originalFace.Height);
            overlay.SetData(overlayPixels);

            _eyeOverlayCacheForm6[originalFace] = overlay;
            return overlay;
        }

        /// <summary>
        /// Draws the psychic eye overlay for 12-frame idle eye sheets.
        /// Same visual treatment as the main overlay: self-illuminated, pulsing, bloom.
        /// Form 6 (transform 5) uses its own pink source colors via GetEyeOverlayForm6.
        /// Called from SariaIdleAnimator.DrawIdleEyes.
        /// </summary>
        public static void DrawPsychicIdleEyeOverlay(Projectile projectile, Texture2D eyeTexture, int eyeFrame, Vector2 eyeOffset, int transform = 0, int totalFrames = 12)
        {
            if (!_eyeOpacity.TryGetValue(projectile.whoAmI, out float opacity) || opacity <= 0f)
                return;

            float pulse = GetPulse(projectile.whoAmI);
            float finalOpacity = opacity * pulse;

            Texture2D overlay = transform == 6 ? GetEyeOverlayForm7(eyeTexture)
                              : transform == 5 ? GetEyeOverlayForm6(eyeTexture)
                              : GetEyeOverlay(eyeTexture);

            int frameHeight = overlay.Height / totalFrames;
            int frameY = frameHeight * Math.Clamp(eyeFrame, 0, totalFrames - 1);
            Rectangle sourceRect = new Rectangle(0, frameY, overlay.Width, frameHeight);
            Vector2 origin = sourceRect.Size() / 2f;

            Vector2 drawPos = projectile.Center - Main.screenPosition + new Vector2(0f, projectile.gfxOffY);
            drawPos.Y += 1f;
            drawPos += eyeOffset;

            SpriteEffects spriteEffects = SpriteEffects.None;
            if (projectile.spriteDirection == -1)
                spriteEffects = SpriteEffects.FlipHorizontally;

            // Pink light from the eyes
            if (finalOpacity > 0.1f)
            {
                Vector2 lightPos = projectile.Center + new Vector2(0f, -8f);
                Lighting.AddLight(lightPos, new Vector3(0.55f, 0.1f, 0.4f) * finalOpacity * 0.55f);
            }

            // Bloom pass
            Color bloomColor = new Color(255, 140, 220) * finalOpacity * 0.35f;
            Main.spriteBatch.Draw(overlay, drawPos, sourceRect, bloomColor, projectile.rotation, origin, projectile.scale * 1.08f, spriteEffects, 0f);

            // Main overlay — self-illuminated
            Color drawColor = Color.White * finalOpacity;
            Main.spriteBatch.Draw(overlay, drawPos, sourceRect, drawColor, projectile.rotation, origin, projectile.scale, spriteEffects, 0f);
        }

        /// <summary>
        /// Gets or creates a blood-sneeze eye-only overlay texture for the given face texture.
        /// Only the pixels matching the blood-sneeze source color are kept (with light-blue replacement);
        /// everything else is transparent.
        /// </summary>
        public static Texture2D GetBloodSneezeOverlay(Texture2D originalFace)
        {
            if (_bloodSneezeOverlayCache.TryGetValue(originalFace, out var cached))
                return cached;

            Color[] pixels = new Color[originalFace.Width * originalFace.Height];
            originalFace.GetData(pixels);

            Color[] overlayPixels = new Color[pixels.Length];
            for (int i = 0; i < pixels.Length; i++)
            {
                Color p = pixels[i];
                if (p.R == BloodSneezeSource.R && p.G == BloodSneezeSource.G && p.B == BloodSneezeSource.B && p.A == BloodSneezeSource.A)
                    overlayPixels[i] = BloodSneezeReplacement;
                else
                    overlayPixels[i] = Color.Transparent;
            }

            Texture2D overlay = new Texture2D(Main.graphics.GraphicsDevice, originalFace.Width, originalFace.Height);
            overlay.SetData(overlayPixels);

            _bloodSneezeOverlayCache[originalFace] = overlay;
            return overlay;
        }

        /// <summary>
        /// Draws the blood-sneeze eye overlay at full opacity when BloodSneeze is active.
        /// Reads BloodSneeze directly from the Saria ModProjectile — instant on/off,
        /// no fading. Draws on top of all other face overlays including psychic eyes.
        /// </summary>
        public static void DrawBloodSneezeEyeOverlay(Projectile projectile, Texture2D faceTexture)
        {
            if (projectile.ModProjectile is not Saria saria || !saria.BloodSneezeValue)
                return;

            Texture2D overlay = GetBloodSneezeOverlay(faceTexture);

            Vector2 startPos = projectile.Center - Main.screenPosition + new Vector2(0f, projectile.gfxOffY);
            int frameHeight = overlay.Height / Main.projFrames[projectile.type];
            int frameY = frameHeight * projectile.frame;
            Rectangle rectangle = new Rectangle(0, frameY, overlay.Width, frameHeight);
            Vector2 origin = rectangle.Size() / 2f;

            startPos.Y += 1;

            SpriteEffects spriteEffects = SpriteEffects.None;
            if (projectile.spriteDirection == -1)
                spriteEffects = SpriteEffects.FlipHorizontally;

            Main.spriteBatch.Draw(overlay, startPos, rectangle, projectile.GetAlpha(Color.White), projectile.rotation, origin, projectile.scale, spriteEffects, 0f);
        }
    }
}
