using Microsoft.Xna.Framework;
using System.Linq;
using System.Reflection;
using System.Text;
using Terraria;
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.GameContent.Events;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ID;
using System;
using System.IO;
using Terraria.ModLoader;
using Terraria.ObjectData;
using SariaMod.Items;
using SariaMod.Buffs;
using SariaMod.Items.Strange;
using SariaMod.Dusts;
using SariaMod.Items.Amber;
using SariaMod.Items.Amethyst;
using SariaMod.Items.Bands;
using SariaMod.Items.Emerald;
using SariaMod.Items.Ruby;
using SariaMod.Items.Sapphire;
using SariaMod.Items.Topaz;
using SariaMod.Items.zPearls;
using SariaMod.Items.zTalking;
using Terraria.Localization;
using Terraria.Audio;
using Terraria.UI;
using SariaMod;
using Terraria.GameContent.UI.Elements;
using ReLogic.Content;
using Terraria.DataStructures;
namespace SariaMod
{
    public static class SariaDrawingExtensions
    {
        public static float alpha1;
        public static bool alpha1Counter;
        public static float alpha2;
        public static bool alpha2Counter;
        public static float alpha3;
        public static bool alpha3Counter;
        public static int alpha3Phase;      // 0=rise, 1=hold, 2=flicker, 3=fade down
        public static int alpha3Timer;      // ticks remaining in current phase
        public static int alpha3FlickerCount; // flickers remaining in phase 2
        public static float alpha4;
        public static bool alpha4Counter;
        // Deduplicated alpha4 from SariaModUtilities2 (used by RupeeGlowandFadedraw)
        public static float rupeeAlpha4;
        public static bool rupeeAlpha4Counter;
        public static float alpha5;
        public static bool alpha5Counter;
        public static float alpha6;
        public static bool alpha6Counter;

        // Frame-guard so multiple callers per frame don't double-tick the alphas
        private static uint _lastAlphaUpdateFrame;

        // Static dictionary to track trail decay timers per projectile (by whoAmI)
        private static Dictionary<int, float> _trailDecayTimers = new Dictionary<int, float>();

        /// <summary>
        /// Ticks alpha1/2/3 once per game frame with an asymmetric pulse:
        /// fade-in (alpha decreasing → visible) is slightly quicker than
        /// fade-out (alpha increasing → transparent).
        /// Safe to call from many places; only the first call per frame does work.
        /// </summary>
        public static void UpdateAlphaCounters()
        {
            if (Main.GameUpdateCount == _lastAlphaUpdateFrame) return;
            _lastAlphaUpdateFrame = Main.GameUpdateCount;

            // Alpha4 — shared pulse for Form 5 eye glow + dialogue mask3 (~12s full cycle)
            // Middle ground: dialogue gets a bit longer, Saria eyes get a bit more common
            if (alpha4Counter)
                alpha4 -= 0.008f;   // fade in (~2.1s visible→gone)
            else
                alpha4 += 0.001f;   // fade out (~16.7s gone→visible)

            if (alpha4 <= 0f) { alpha4 = 0f; alpha4Counter = false; }
            if (alpha4 >= 1f) { alpha4 = 1f; alpha4Counter = true; }

            // Electric mask cycle (Mask2)
            SariaExtensions1.electricCycleTimer = (SariaExtensions1.electricCycleTimer + 1) % SariaExtensions1.ElectricCycleTotal;
            int t = SariaExtensions1.electricCycleTimer;
            int activeEnd = SariaExtensions1.ElectricActiveFrames;
            int fadeOutEnd = activeEnd + SariaExtensions1.ElectricFadeOutFrames;
            int offEnd = fadeOutEnd + SariaExtensions1.ElectricOffFrames;
            // int fadeInEnd = offEnd + ElectricFadeInFrames == ElectricCycleTotal

            if (t < activeEnd)
            {
                // Active phase — full intensity
                SariaExtensions1.electricIntensity = 1f;
            }
            else if (t < fadeOutEnd)
            {
                // Fade-out phase — 1→0 over ElectricFadeOutFrames
                SariaExtensions1.electricIntensity = 1f - (float)(t - activeEnd) / SariaExtensions1.ElectricFadeOutFrames;
            }
            else if (t < offEnd)
            {
                // Off phase — invisible
                SariaExtensions1.electricIntensity = 0f;
            }
            else
            {
                // Fade-in phase — 0→1 over ElectricFadeInFrames
                SariaExtensions1.electricIntensity = (float)(t - offEnd) / SariaExtensions1.ElectricFadeInFrames;
            }
        }

        public static void SariaMaindraw(Projectile projectile, Texture2D texture, bool Glowinthedark, bool ShoulditFlip, bool DoesitTrail, int startPosY, int HowlongisTrail, Color lightColor, int startPosX = 0, bool pointSample = false, float alphaScale = 1f)
        {
            Vector2 startPos = projectile.Center - Main.screenPosition + new Vector2(0f, projectile.gfxOffY);
            int frameHeight = texture.Height / Main.projFrames[projectile.type];
            int frameY = frameHeight * projectile.frame;
            Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
            Vector2 origin = rectangle.Size() / 2f;
            float rotation = projectile.rotation;
            float scale = projectile.scale;
            SpriteEffects spriteEffects = SpriteEffects.None;
            Color drawColor = lightColor;
            if (Glowinthedark)
            {
                drawColor = Color.Lerp(lightColor, Color.GhostWhite, 20f);
            }
            if (alphaScale < 1f)
            {
                drawColor *= alphaScale;
            }
            startPos.Y += startPosY;
            startPos.X += startPosX;
            if (ShoulditFlip)
            {
                if (projectile.spriteDirection == -1)
                {
                    spriteEffects = SpriteEffects.FlipHorizontally;
                }
            }
            if (!DoesitTrail)
            {
                if (pointSample)
                {
                    Main.spriteBatch.End();
                    Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                }
                Main.spriteBatch.Draw(texture, startPos, rectangle, projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                if (pointSample)
                {
                    Main.spriteBatch.End();
                    Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                }
            }
            if (DoesitTrail)
            {
                // Manage trail decay timer for this projectile
                if (!_trailDecayTimers.ContainsKey(projectile.whoAmI))
                {
                    _trailDecayTimers[projectile.whoAmI] = 0f;
                }

                // Update trail timer based on velocity
                if (projectile.velocity.Length() > 0.1f)
                {
                    _trailDecayTimers[projectile.whoAmI] = 30f; // Reset timer when moving
                }
                else if (_trailDecayTimers[projectile.whoAmI] > 0f)
                {
                    _trailDecayTimers[projectile.whoAmI]--; // Decay timer when stopped
                }

                float currentTimer = _trailDecayTimers[projectile.whoAmI];

                // Draw trail only if timer > 0 (moving or recently stopped)
                if (currentTimer > 0f)
                {
                    // Calculate fade factor: 1.0 when moving, gradually fades to 0
                    float trailFadeFactor = MathHelper.Clamp(currentTimer / 30f, 0f, 1f);

                    // Draw trail segments with interpolation for smooth flowing tail
                    for (int i = 1; i < HowlongisTrail; i++)
                    {
                        if (projectile.oldPos[i] == Vector2.Zero)
                            continue;

                        Vector2 currentPos = projectile.oldPos[i];
                        Vector2 previousPos = (i > 0) ? projectile.oldPos[i - 1] : projectile.Center;

                        if (previousPos == Vector2.Zero)
                            previousPos = currentPos;

                        // Interpolate between positions for smooth connected trail
                        int interpolationSteps = 3;
                        for (int t = 0; t <= interpolationSteps; t++)
                        {
                            float lerpAmount = (float)t / interpolationSteps;
                            Vector2 interpolatedPos = Vector2.Lerp(previousPos, currentPos, lerpAmount);

                            // Calculate completion ratio for the interpolated position
                            float completionRatio = ((float)i - 1 + lerpAmount) / (float)HowlongisTrail;

                            Vector2 trailPos = interpolatedPos + projectile.Size * 0.5f - Main.screenPosition;
                            trailPos.Y += startPosY;

                            // Cone effect: scale decreases smoothly toward the tail end
                            float trailScale = scale * MathHelper.Lerp(1f, 0.3f, completionRatio);

                            // Use original lerp with transparent for proper fading, with additional fade-out
                            Color trailColor = Color.Lerp(drawColor, Color.DeepPink, completionRatio);
                            trailColor = Color.Lerp(trailColor, Color.Transparent, completionRatio);

                            // Apply additional fade when stopping
                            trailColor = Color.Lerp(Color.Transparent, trailColor, trailFadeFactor);

                            Main.spriteBatch.Draw(texture, trailPos, rectangle, projectile.GetAlpha(trailColor), rotation, origin, trailScale, spriteEffects, 0f);
                        }
                    }
                }

                // Clean up timer for inactive projectiles
                if (!projectile.active && _trailDecayTimers.ContainsKey(projectile.whoAmI))
                {
                    _trailDecayTimers.Remove(projectile.whoAmI);
                }

                // Draw main sprite on top
                Main.spriteBatch.Draw(texture, startPos, rectangle, projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
            }
        }

        public static void SariaBubbleFaces(Projectile projectile, Texture2D texture, bool shoulditflip, int FrameSpeed, int NumFrames, int startPosY, Color lightColor)
        {
            Vector2 startPos = projectile.Center - Main.screenPosition + new Vector2(0f, projectile.gfxOffY);
            int frameHeight = texture.Height / NumFrames;
            int frameY = frameHeight * NumFrames;
            Color drawColor = Color.Lerp(lightColor, Color.WhiteSmoke, 20f);
            drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
            Rectangle rectangle = texture.Frame(verticalFrames: NumFrames, frameY: (int)Main.GameUpdateCount / FrameSpeed % NumFrames);
            Vector2 origin = rectangle.Size() / 2;
            float rotation = projectile.rotation;
            float scale = projectile.scale;
            SpriteEffects spriteEffects = SpriteEffects.None;
            startPos.Y += startPosY;
            if (projectile.spriteDirection == -1)
            {
                startPos.X += 0;
                if (shoulditflip)
                {
                    spriteEffects = SpriteEffects.FlipHorizontally;
                }
            }
            if (projectile.spriteDirection == 1)
            {
                startPos.X += 0;
            }
            Main.spriteBatch.Draw(texture, startPos, rectangle, projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
        }

        /// <summary>
        /// Draws animated sparks with layered electric effects:
        /// shimmer copies for crackling discharge, additive glow for
        /// luminance, and dynamic flickering electric light emission.
        /// </summary>
        public static void SariaSparksDraw(Projectile projectile, Texture2D texture, Color lightColor)
        {
            UpdateAlphaCounters();

            Vector2 startPos = projectile.Center - Main.screenPosition + new Vector2(0f, projectile.gfxOffY);
            Rectangle rectangle = texture.Frame(verticalFrames: 14, frameY: (int)Main.GameUpdateCount / 3 % 14);
            Vector2 origin = rectangle.Size() / 2f;
            float rotation = projectile.rotation;
            float scale = projectile.scale;
            SpriteEffects spriteEffects = SpriteEffects.None;
            if (projectile.spriteDirection == -1)
            {
                spriteEffects = SpriteEffects.FlipHorizontally;
            }
            startPos.Y += 1;

            // --- 1) Base spark draw (bright, glow-enabled) ---
            Color baseColor = Color.Lerp(lightColor, Color.GhostWhite, 20f);
            Main.spriteBatch.Draw(texture, startPos, rectangle, projectile.GetAlpha(baseColor), rotation, origin, scale, spriteEffects, 0f);

            // --- 2) Shimmer copies: electric crackling flicker ---
            // Multiple offset copies with electric tint and alpha=0 (pseudo-additive)
            // create a discharge / arcing effect. Sparks scatter in all directions.
            ulong randSeed = (Main.GameUpdateCount / 4) ^ (ulong)projectile.whoAmI;
            for (int c = 0; c < 4; c++)
            {
                float shakeX = Utils.RandomInt(ref randSeed, -12, 13) * 0.15f;
                float shakeY = Utils.RandomInt(ref randSeed, -12, 13) * 0.15f;
                Vector2 shimmerPos = startPos + new Vector2(shakeX, shakeY);
                Color shimmerColor = new Color(40, 100, 140, 0);
                Main.spriteBatch.Draw(texture, shimmerPos, rectangle, projectile.GetAlpha(shimmerColor), rotation, origin, scale, spriteEffects, 0f);
            }

            // --- 3) Additive glow pass: bright electric core ---
            // Pulsating intensity makes the sparks crackle. Color shifts cyan→blue.
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            float glowIntensity = MathHelper.Lerp(0.15f, 0.4f, 1f - alpha1);
            Color glowColor = Color.Lerp(Color.DeepSkyBlue, Color.Cyan, 1f - alpha2) * glowIntensity;
            Main.spriteBatch.Draw(texture, startPos, rectangle, projectile.GetAlpha(glowColor), rotation, origin, scale * 1.03f, spriteEffects, 0f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            // --- 4) Dynamic flickering electric light ---
            // Light oscillates in intensity and shifts cyan→blue-white.
            float lightPulse = MathHelper.Lerp(0.35f, 0.7f, 1f - alpha1);
            Vector3 sparkLight = Vector3.Lerp(Color.DeepSkyBlue.ToVector3(), Color.LightCyan.ToVector3(), 1f - alpha2) * lightPulse;
            Lighting.AddLight(projectile.Center, sparkLight);
        }

        /// <summary>
        /// Draws a body-mask overlay with an electrical effect:
        /// rapid pulsating visibility, shimmer copies for arcing
        /// discharge, and an additive glow pass for bright electric
        /// luminance. Uses the same frame/flip logic as SariaMaindraw.
        /// </summary>
        public static void SariaElectricMaskDraw(Projectile projectile, Texture2D texture, bool ShoulditFlip, Color lightColor, int startPosY = 1)
        {
            UpdateAlphaCounters();

            float intensity = SariaExtensions1.electricIntensity;
            if (intensity <= 0f) return; // off phase — skip everything

            Vector2 startPos = projectile.Center - Main.screenPosition + new Vector2(0f, projectile.gfxOffY);
            int frameHeight = texture.Height / Main.projFrames[projectile.type];
            int frameY = frameHeight * projectile.frame;
            Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
            Vector2 origin = rectangle.Size() / 2f;
            float rotation = projectile.rotation;
            float scale = projectile.scale;
            SpriteEffects spriteEffects = SpriteEffects.None;
            if (ShoulditFlip && projectile.spriteDirection == -1)
            {
                spriteEffects = SpriteEffects.FlipHorizontally;
            }
            startPos.Y += startPosY;

            // --- 1) Pulsating base draw (rapid flicker via alpha3, scaled by cycle intensity) ---
            Color baseColor = Color.Lerp(lightColor, Color.GhostWhite, 20f);
            float electricPulse = MathHelper.Lerp(0.3f, 1f, 1f - alpha3) * intensity;
            baseColor *= electricPulse;
            Main.spriteBatch.Draw(texture, startPos, rectangle, projectile.GetAlpha(baseColor), rotation, origin, scale, spriteEffects, 0f);

            // --- 2) Shimmer copies: electrical arcing ---
            // Jitter magnitude scales with intensity so arcs slow down as it fades.
            ulong randSeed = (Main.GameUpdateCount / 3) ^ (ulong)projectile.whoAmI;
            for (int c = 0; c < 3; c++)
            {
                float shakeX = Utils.RandomInt(ref randSeed, -8, 9) * 0.12f * intensity;
                float shakeY = Utils.RandomInt(ref randSeed, -8, 9) * 0.12f * intensity;
                Vector2 shimmerPos = startPos + new Vector2(shakeX, shakeY);
                Color shimmerColor = new Color(30, 90, 130, 0) * electricPulse;
                Main.spriteBatch.Draw(texture, shimmerPos, rectangle, projectile.GetAlpha(shimmerColor), rotation, origin, scale, spriteEffects, 0f);
            }

            // --- 3) Additive glow pass: bright electric core ---
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            float glowIntensity = MathHelper.Lerp(0.1f, 0.35f, 1f - alpha3) * intensity;
            Color glowColor = Color.Lerp(Color.DeepSkyBlue, Color.Cyan, 1f - alpha2) * glowIntensity;
            Main.spriteBatch.Draw(texture, startPos, rectangle, projectile.GetAlpha(glowColor), rotation, origin, scale * 1.02f, spriteEffects, 0f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }

        public static void FlatImageDraw(Projectile projectile, Texture2D texture, Color lightColor, int startPosX = 0, int startPosY = 0)
        {
            Vector2 startPos = new Vector2(Main.screenWidth + 0, Main.screenHeight - 0) / 2f + new Vector2(0f, 0f);
            int frameHeight = texture.Height / 1;
            int frameY = frameHeight * 1;
            Color drawColor = Color.Lerp(lightColor, Color.LightBlue, 90f);
            drawColor = Color.Lerp(drawColor, Color.GhostWhite, 90);
            drawColor = Color.Lerp(drawColor, Color.Transparent, .75f);
            Rectangle rectangle = texture.Frame(verticalFrames: 1, frameY: (int)Main.GameUpdateCount / 6 % 1);
            Vector2 origin = rectangle.Size() / 2f;
            float rotation = projectile.rotation;
            float scale = projectile.scale * 1.25f;
            startPos.X += (startPosX + 1.5f);
            startPos.Y += startPosY;
            SpriteEffects spriteEffects = SpriteEffects.None;
            Main.spriteBatch.Draw(texture, startPos, rectangle, projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
        }

        public static void VisualSetUpDraw(Projectile projectile, Texture2D texture, Color lightColor, int startPosX = 0, int startPosY = 0)
        {
            Vector2 startPos = new Vector2(Main.screenWidth + 0, Main.screenHeight - 0) / 2f + new Vector2(0f, 0f);
            int frameHeight = texture.Height / 1;
            int frameY = frameHeight * 1;
            Color drawColor = Color.Lerp(lightColor, Color.Yellow, 80f);
            drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
            Rectangle rectangle = texture.Frame(verticalFrames: 1, frameY: (int)Main.GameUpdateCount / 6 % 1);
            Vector2 origin = rectangle.Size() / 2f;
            float rotation = projectile.rotation;
            float scale = projectile.scale;
            startPos.X += startPosX;
            startPos.Y += startPosY;
            SpriteEffects spriteEffects = SpriteEffects.None;
            Main.spriteBatch.Draw(texture, startPos, rectangle, projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
        }

        public static void SariaEyesGlowandFadedraw(Projectile projectile, Texture2D texture, Color lightColor, Color WhatColor)
        {
            // alpha3=0 means FULL GLOW, alpha3=1 means INVISIBLE
            // Phase 0: Drop quickly to 0 (glow appears fast)
            // Phase 1: Hold at 0 (full glow stays visible)
            // Phase 2: Flicker — bump up ~20% (dim) then snap back to 0 (bright)
            // Phase 3: Rise slowly to 1 (glow fades away)
            switch (alpha3Phase)
            {
                case 0: // glow appears fast (alpha3 drops to 0)
                    alpha3 -= 0.02f;
                    if (alpha3 <= 0f)
                    {
                        alpha3 = 0f;
                        alpha3Phase = 1;
                        alpha3Timer = 360; // hold full glow ~6 seconds at 60fps
                    }
                    break;
                case 1: // hold at full glow
                    alpha3Timer--;
                    if (alpha3Timer <= 0)
                    {
                        alpha3Phase = 2;
                        alpha3Counter = false; // start dimming
                        alpha3Timer = 0;
                    }
                    break;
                case 2: // flicker — go up ~20% (dim), snap back to full, then fade
                    if (!alpha3Counter) // dimming up toward 0.2
                    {
                        alpha3 += 0.01f;
                        if (alpha3 >= 0.2f)
                        {
                            alpha3 = 0.2f;
                            alpha3Counter = true; // snap back to full
                        }
                    }
                    else // snapping back to full glow
                    {
                        alpha3 -= 0.04f;
                        if (alpha3 <= 0f)
                        {
                            alpha3 = 0f;
                            alpha3Phase = 3; // done flickering, start slow fade
                        }
                    }
                    break;
                case 3: // slow fade away (alpha3 rises to 1)
                    alpha3 += 0.001f;
                    if (alpha3 >= 1f)
                    {
                        alpha3 = 1f;
                        alpha3Phase = 0; // restart cycle
                    }
                    break;
            }
            Vector2 startPos = projectile.Center - Main.screenPosition + new Vector2(0f, projectile.gfxOffY);
            int frameHeight = texture.Height / Main.projFrames[projectile.type];
            int frameY = frameHeight * projectile.frame;
            Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
            Vector2 origin = rectangle.Size() / 2f;
            float rotation = projectile.rotation;
            float scale = projectile.scale;
            SpriteEffects spriteEffects = SpriteEffects.None;
            if (projectile.spriteDirection == -1)
            {
                spriteEffects = SpriteEffects.FlipHorizontally;
            }
            Lighting.AddLight(projectile.Center, Color.DeepPink.ToVector3() * (1f - alpha3));
            Color drawColor = Color.Lerp(lightColor, WhatColor, 300f);
            drawColor = Color.Lerp(drawColor, Color.Transparent, alpha3);
            float light = 80.15f * alpha1;
            startPos.Y += 1;
            startPos.X += 0;
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            Main.spriteBatch.Draw(texture, startPos, rectangle, (drawColor), rotation, origin, scale, spriteEffects, 0f);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }

        public static void DialogueUEyeMaskdraw(Projectile projectile, Texture2D texture, Color lightColor, Vector2 startPos2, int NumFrames, int WhichFrame)
        {
            Player player = Main.player[projectile.owner];
            int owner = player.whoAmI;
            Vector2 startPos = startPos2;
            int frameHeight = texture.Height / NumFrames;
            int frameY = frameHeight * NumFrames;
            Rectangle rectangle = texture.Frame(verticalFrames: NumFrames, frameY: WhichFrame);
            Vector2 origin = rectangle.Size() / NumFrames;
            float rotation = projectile.rotation;
            float scale = projectile.scale;
            SpriteEffects spriteEffects = SpriteEffects.None;
            Color drawColor = lightColor;
            drawColor = Color.Lerp(drawColor, Color.AntiqueWhite, 20f);
            drawColor = Color.Lerp(drawColor, Color.Transparent, alpha3);
            Main.spriteBatch.Draw(texture, startPos, rectangle, (drawColor), rotation, origin, scale, spriteEffects, 0f);
        }

        public static void DialogueUIMask3draw(Projectile projectile, Color lightColor, int startPosX = 0, int startPosY = 0)
        {
            Player player = Main.player[projectile.owner];
            int owner = player.whoAmI;
            Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/zTalking/Greetings2EmeraldMask3");
            Vector2 startPos = new Vector2(Main.screenWidth + 0, Main.screenHeight + 0) / 2f + new Vector2(0f, 0f);
            int frameHeight = texture.Height / Main.projFrames[projectile.type];
            int frameY = frameHeight * projectile.frame;
            Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
            Vector2 origin = rectangle.Size() / 2f;
            float rotation = projectile.rotation;
            float scale = projectile.scale;
            SpriteEffects spriteEffects = SpriteEffects.None;
            Color drawColor = lightColor;
            drawColor = Color.Lerp(drawColor, Color.FloralWhite, 30f);
            drawColor = Color.Lerp(drawColor, Color.Transparent, alpha3);
            startPos.X += startPosX;
            startPos.Y += startPosY;
            Main.spriteBatch.Draw(texture, startPos, rectangle, (drawColor), rotation, origin, scale, spriteEffects, 0f);
        }

        public static void DialogueUIMask2draw(Projectile projectile, Color lightColor, int startPosX = 0, int startPosY = 0)
        {
            Player player = Main.player[projectile.owner];
            int owner = player.whoAmI;
            Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/zTalking/Greetings2EmeraldMask2");
            Vector2 startPos = new Vector2(Main.screenWidth + 0, Main.screenHeight + 0) / 2f + new Vector2(0f, 0f);
            int frameHeight = texture.Height / Main.projFrames[projectile.type];
            int frameY = frameHeight * projectile.frame;
            Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
            Vector2 origin = rectangle.Size() / 2f;
            float rotation = projectile.rotation;
            float scale = projectile.scale;
            SpriteEffects spriteEffects = SpriteEffects.None;
            Color drawColor = lightColor;
            drawColor = Color.Lerp(drawColor, Color.FloralWhite, 30f);
            drawColor = Color.Lerp(drawColor, Color.Transparent, alpha1);
            startPos.X += startPosX;
            startPos.Y += startPosY;
            Main.spriteBatch.Draw(texture, startPos, rectangle, (drawColor), rotation, origin, scale, spriteEffects, 0f);
        }

        public static void DialogueUIMaskdraw(Projectile projectile, Color lightColor, int startPosX = 0, int startPosY = 0)
        {
            Player player = Main.player[projectile.owner];
            int owner = player.whoAmI;
            Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/zTalking/Greetings2EmeraldMask");
            Vector2 startPos = new Vector2(Main.screenWidth + 0, Main.screenHeight + 0) / 2f + new Vector2(0f, 0f);
            int frameHeight = texture.Height / Main.projFrames[projectile.type];
            int frameY = frameHeight * projectile.frame;
            Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
            Vector2 origin = rectangle.Size() / 2f;
            float rotation = projectile.rotation;
            float scale = projectile.scale;
            SpriteEffects spriteEffects = SpriteEffects.None;
            Color drawColor = lightColor;
            drawColor = Color.Lerp(drawColor, Color.FloralWhite, 30f);
            drawColor = Color.Lerp(drawColor, Color.Transparent, alpha2);
            startPos.X += startPosX;
            startPos.Y += startPosY;
            Main.spriteBatch.Draw(texture, startPos, rectangle, (drawColor), rotation, origin, scale, spriteEffects, 0f);
        }

        public static void DialogueUIFireMaskdraw(Projectile projectile, Color lightColor, Texture2D texture, int i, int j, int startPosX = 0, int startPosY = 0)
        {
            Player player = Main.player[projectile.owner];
            int owner = player.whoAmI;
            Vector2 startPos = new Vector2(Main.screenWidth + 0, Main.screenHeight + 0) / 2f + new Vector2(0f, 0f);
            int frameHeight = texture.Height / Main.projFrames[projectile.type];
            int frameY = frameHeight * projectile.frame;
            Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
            Vector2 origin = rectangle.Size() / 2f;
            float rotation = projectile.rotation;
            float scale = projectile.scale;
            SpriteEffects spriteEffects = SpriteEffects.None;
            Color drawColor = lightColor;
            drawColor = Color.Lerp(drawColor, Color.AntiqueWhite, 20f);
            ulong randShakeEffect = (Main.GameUpdateCount / 8) ^ (ulong)((long)j << 20 | (long)(uint)i);
            float drawPositionX = i * 1 - (int)Main.screenPosition.X - (texture.Width - 16f) / 2f;
            float drawPositionY = j * 1 - (int)Main.screenPosition.Y;
            float shakeX = Utils.RandomInt(ref randShakeEffect, -4, -3) * 0.07f;
            float shakeY = Utils.RandomInt(ref randShakeEffect, -4, 3) * 0.07f;
            startPos.Y += (startPosY + (1 + shakeX));
            startPos.X += (startPosX + (+0 + shakeY));
            Main.spriteBatch.Draw(texture, startPos, rectangle, (drawColor), rotation, origin, scale, spriteEffects, 0f);
        }

        public static void Saria5GlowMaskdraw(Projectile projectile, Texture2D texture, Color lightColor, bool counter1, bool counter2, bool doesFlip = false, int startPosX = 0)
        {
            if (alpha1Counter) alpha1 -= 0.001f;
            if (alpha1 <= 0f) { alpha1 = 0f; alpha1Counter = false; }
            if (!alpha1Counter) alpha1 += 0.001f;
            if (alpha1 >= 1f) { alpha1 = 1f; alpha1Counter = true; }

            if (alpha2Counter) alpha2 -= 0.002f;
            if (alpha2 <= 0f) { alpha2 = 0f; alpha2Counter = false; }
            if (!alpha2Counter) alpha2 += 0.002f;
            if (alpha2 >= 1f) { alpha2 = 1f; alpha2Counter = true; }

            Vector2 startPos = projectile.Center - Main.screenPosition + new Vector2(0f, projectile.gfxOffY);
            int frameHeight = texture.Height / Main.projFrames[projectile.type];
            int frameY = frameHeight * projectile.frame;
            Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
            Vector2 origin = rectangle.Size() / 2f;
            float rotation = projectile.rotation;
            float scale = projectile.scale;
            SpriteEffects spriteEffects = SpriteEffects.None;
            if (doesFlip && projectile.spriteDirection == -1)
            {
                spriteEffects = SpriteEffects.FlipHorizontally;
            }
            Lighting.AddLight(projectile.Center, Color.DeepPink.ToVector3() * (1f - alpha1));
            Lighting.AddLight(projectile.Center, Color.Green.ToVector3() * (1f - alpha2));
            Color drawColor = Color.Lerp(lightColor, Color.FloralWhite, 30f);
            if (counter1)
            {
                drawColor = Color.Lerp(drawColor, Color.Transparent, alpha1);
                // Use 5SariaBody for dust spawning so particles cover the full silhouette evenly
                var bodyTex = ModContent.Request<Texture2D>("SariaMod/Items/Strange/5SariaAnimations/5SariaBody").Value;
                projectile.RockDustOnVisiblePixels(bodyTex, ModContent.DustType<RockSparkle>(), 20,
                    Main.projFrames[projectile.type], projectile.frame, doesFlip, startPosX);
            }
            if (counter2)
            {
                drawColor = Color.Lerp(drawColor, Color.Transparent, alpha2);
            }
            startPos.Y += 1;
            startPos.X += startPosX;
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            Main.spriteBatch.Draw(texture, startPos, rectangle, (drawColor), rotation, origin, scale, spriteEffects, 0f);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }

        public static void Saria3GlowMaskdraw(Projectile projectile, Texture2D texture, int i, int j, bool ShoulditFlip, Color lightColor)
        {
            Vector2 startPos = projectile.Center - Main.screenPosition + new Vector2(0f, projectile.gfxOffY);
            int frameHeight = texture.Height / Main.projFrames[projectile.type];
            int frameY = frameHeight * projectile.frame;
            Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
            Vector2 origin = rectangle.Size() / 2f;
            float rotation = projectile.rotation;
            float scale = projectile.scale;
            SpriteEffects spriteEffects = SpriteEffects.None;
            Color drawColor = Color.Lerp(lightColor, Color.LightYellow, 30f);
            if (ShoulditFlip)
            {
                if (projectile.spriteDirection == -1)
                {
                    spriteEffects = SpriteEffects.FlipHorizontally;
                }
            }
            Lighting.AddLight(projectile.Center, Color.Yellow.ToVector3() * .2f);
            ulong randShakeEffect = (Main.GameUpdateCount / 8) ^ (ulong)((long)j << 20 | (long)(uint)i);
            float drawPositionX = i * 1 - (int)Main.screenPosition.X - (projectile.width - 16f) / 2f;
            float drawPositionY = j * 1 - (int)Main.screenPosition.Y;
            float shakeX = Utils.RandomInt(ref randShakeEffect, -4, -3) * 0.07f;
            float shakeY = Utils.RandomInt(ref randShakeEffect, -4, 3) * 0.07f;
            startPos.Y += (-15 + shakeX);
            startPos.X += (+0 + shakeY);
            Main.spriteBatch.Draw(texture, startPos, rectangle, projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
        }

        /// <summary>
        /// Draws animated fire hair with layered flame effects:
        /// shimmer copies for heat haze, additive glow for luminance,
        /// and dynamic flickering light emission.
        /// </summary>
        public static void SariaFireHairDraw(Projectile projectile, Texture2D texture, bool ShoulditFlip, int startPosY, Color lightColor)
        {
            UpdateAlphaCounters();

            Vector2 startPos = projectile.Center - Main.screenPosition + new Vector2(0f, projectile.gfxOffY);
            int frameHeight = texture.Height / Main.projFrames[projectile.type];
            int frameY = frameHeight * projectile.frame;
            Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
            Vector2 origin = rectangle.Size() / 2f;
            float rotation = projectile.rotation;
            float scale = projectile.scale;
            SpriteEffects spriteEffects = SpriteEffects.None;
            if (ShoulditFlip && projectile.spriteDirection == -1)
            {
                spriteEffects = SpriteEffects.FlipHorizontally;
            }
            startPos.Y += startPosY;

            // --- 1) Base fire draw (bright, glow-enabled) ---
            Color baseColor = Color.Lerp(lightColor, Color.GhostWhite, 20f);
            Main.spriteBatch.Draw(texture, startPos, rectangle, projectile.GetAlpha(baseColor), rotation, origin, scale, spriteEffects, 0f);

            // --- 2) Shimmer copies: Terraria-style flame flicker ---
            // Multiple offset copies with warm fire tint and alpha=0 (pseudo-additive)
            // create heat haze / shimmering edges. Bias shake upward since fire rises.
            ulong randSeed = (Main.GameUpdateCount / 6) ^ (ulong)projectile.whoAmI;
            for (int c = 0; c < 4; c++)
            {
                float shakeX = Utils.RandomInt(ref randSeed, -10, 11) * 0.12f;
                float shakeY = Utils.RandomInt(ref randSeed, -12, 5) * 0.15f;
                Vector2 shimmerPos = startPos + new Vector2(shakeX, shakeY);
                Color shimmerColor = new Color(120, 80, 30, 0);
                Main.spriteBatch.Draw(texture, shimmerPos, rectangle, projectile.GetAlpha(shimmerColor), rotation, origin, scale, spriteEffects, 0f);
            }

            // --- 3) Additive glow pass: bright luminous core ---
            // Pulsating intensity makes the fire breathe. Color shifts orange→gold.
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            float glowIntensity = MathHelper.Lerp(0.2f, 0.45f, 1f - alpha1);
            Color glowColor = Color.Lerp(Color.OrangeRed, Color.Gold, 1f - alpha2) * glowIntensity;
            Main.spriteBatch.Draw(texture, startPos, rectangle, projectile.GetAlpha(glowColor), rotation, origin, scale * 1.02f, spriteEffects, 0f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            // --- 4) Dynamic flickering fire light ---
            // Light oscillates in intensity and shifts orange→yellow.
            // Positioned above center since fire illuminates upward.
            float lightPulse = MathHelper.Lerp(0.3f, 0.55f, 1f - alpha1);
            Vector3 fireLight = Vector3.Lerp(Color.Orange.ToVector3(), Color.Yellow.ToVector3(), 1f - alpha2) * lightPulse;
            Vector2 lightPos = projectile.Center;
            lightPos.Y -= 10f;
            Lighting.AddLight(lightPos, fireLight);
        }

        public static void FrameChargeElectricitydraw(Projectile projectile, Texture2D texture, Color lightColor, bool nottoscreen, int startPosX = 0, int startPosY = 0)
        {
            Vector2 startPos = new Vector2(Main.screenWidth + 0, Main.screenHeight - 0) / 2f + new Vector2(0f, 0f);
            if (nottoscreen)
            {
                startPos = projectile.Center - Main.screenPosition + new Vector2(0f, projectile.gfxOffY);
            }
            int frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<PinkCharge>()];
            int frameY = frameHeight * Main.projFrames[ModContent.ProjectileType<PinkCharge>()];
            Rectangle rectangle = texture.Frame(verticalFrames: 14, frameY: (int)Main.GameUpdateCount / 3 % 14);
            Color drawColor = Color.Lerp(lightColor, Color.WhiteSmoke, 100f);
            drawColor = Color.Lerp(drawColor, Color.LightPink, 0);
            Vector2 origin = rectangle.Size() / 2f;
            float rotation = projectile.rotation;
            float scale = projectile.scale;
            SpriteEffects spriteEffects = SpriteEffects.None;
            startPos.X += startPosX;
            startPos.Y += startPosY;
            Main.spriteBatch.Draw(texture, startPos, rectangle, projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
        }

        public static void FrameChargedraw(Projectile projectile, Texture2D texture, Color lightColor, bool nottoscreen, bool Eightframes, int startPosX = 0, int startPosY = 0)
        {
            Vector2 startPos = new Vector2(Main.screenWidth + 0, Main.screenHeight - 0) / 2f + new Vector2(0f, 0f);
            if (nottoscreen)
            {
                startPos = projectile.Center - Main.screenPosition + new Vector2(0f, projectile.gfxOffY);
            }
            int frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<PinkCharge>()];
            int frameY = frameHeight * Main.projFrames[ModContent.ProjectileType<PinkCharge>()];
            Rectangle rectangle = texture.Frame(verticalFrames: 4, frameY: (int)Main.GameUpdateCount / 6 % 4);
            if (Eightframes)
            {
                frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<BlueCharge>()];
                frameY = frameHeight * Main.projFrames[ModContent.ProjectileType<BlueCharge>()];
                rectangle = texture.Frame(verticalFrames: 8, frameY: (int)Main.GameUpdateCount / 8 % 8);
            }
            Color drawColor = Color.Lerp(lightColor, Color.WhiteSmoke, 100f);
            drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
            Vector2 origin = rectangle.Size() / 2f;
            float rotation = projectile.rotation;
            float scale = projectile.scale;
            SpriteEffects spriteEffects = SpriteEffects.None;
            startPos.X += startPosX;
            startPos.Y += startPosY;
            Main.spriteBatch.Draw(texture, startPos, rectangle, projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
        }

        public static void DrawFlameEffect(Texture2D flameTexture, int i, int j, int offsetX = 0, int offsetY = 0)
        {
            Tile tile = Main.tile[i, j];
            Vector2 zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange, Main.offScreenRange);
            int width = 16;
            int height = 16;
            int yOffset = TileObjectData.GetTileData(tile).DrawYOffset;
            ulong randShakeEffect = Main.TileFrameSeed ^ (ulong)((long)j << 32 | (long)(uint)i);
            float drawPositionX = i * 16 - (int)Main.screenPosition.X - (width - 16f) / 2f;
            float drawPositionY = j * 16 - (int)Main.screenPosition.Y;
            for (int c = 0; c < 7; c++)
            {
                float shakeX = Utils.RandomInt(ref randShakeEffect, -10, 11) * 0.15f;
                float shakeY = Utils.RandomInt(ref randShakeEffect, -10, 1) * 0.35f;
                Main.spriteBatch.Draw(flameTexture, new Vector2(drawPositionX + shakeX, drawPositionY + shakeY + yOffset) + zero, new Rectangle(tile.TileFrameX + offsetX, tile.TileFrameY + offsetY, width, height), new Color(100, 100, 100, 0), 0f, default(Vector2), 1f, SpriteEffects.None, 0f);
            }
        }

        public static void DrawFlameSparks(int dustType, int rarity, int i, int j)
        {
            if (!Main.gamePaused && Main.instance.IsActive && (!Lighting.UpdateEveryFrame || Main.rand.NextBool(4)))
            {
                if (Main.rand.NextBool(rarity))
                {
                    int dust = Dust.NewDust(new Vector2(i * 16 + 4, j * 16 + 2), 4, 4, dustType, 0f, 0f, 100, default, 1f);
                    if (Main.rand.Next(3) != 0)
                        Main.dust[dust].noGravity = true;
                    // Prevent lag.
                    Main.dust[dust].noLightEmittence = true;
                    Main.dust[dust].velocity *= 0.3f;
                    Main.dust[dust].velocity.Y = Main.dust[dust].velocity.Y - 1.5f;
                }
            }
        }

        public static void BlueRingofdust(Projectile projectile)
        {
            BlueRingofdust(projectile, 72);
        }

        public static void BlueRingofdust(Projectile projectile, int howmany)
        {
            Player player = Main.player[projectile.owner];
            for (int j = 0; j < howmany; j++)
            {
                Dust dust = Dust.NewDustPerfect(projectile.Center, 113);
                dust.velocity = ((float)Math.PI * 2f * Vector2.Dot(((float)j / 72f * ((float)Math.PI * 2f)).ToRotationVector2(), player.velocity.SafeNormalize(Vector2.UnitY).RotatedBy((float)j / 72f * ((float)Math.PI * -2f)))).ToRotationVector2();
                dust.velocity = dust.velocity.RotatedBy((float)j / 36f * ((float)Math.PI * 2f)) * 8f;
                dust.noGravity = true;
                dust.scale *= 3.9f;
            }
        }

        /// <summary>
        /// Spawns dust only at positions that correspond to visible (non-transparent)
        /// pixels in the given mask texture's current frame. This keeps sparkles
        /// confined to the actual glowing body parts instead of floating in empty space.
        /// </summary>
        public static void RockDustOnVisiblePixels(Projectile projectile, Texture2D maskTexture,
            int dustType, int severity, int totalFrames, int currentFrame,
            bool doesFlip = false, int startPosX = 0)
        {
            if (!Main.rand.NextBool(severity)) return;
            if (maskTexture == null) return;

            int frameHeight = maskTexture.Height / totalFrames;
            int frameY = frameHeight * Math.Clamp(currentFrame, 0, totalFrames - 1);

            // Read only the current frame's pixels
            Color[] pixels = new Color[maskTexture.Width * frameHeight];
            maskTexture.GetData(0, new Rectangle(0, frameY, maskTexture.Width, frameHeight), pixels, 0, pixels.Length);

            // Group visible pixels by row so we can pick a uniform random Y first,
            // then a random X within that row. This prevents the selection from
            // being biased toward wide areas (dress/legs) and spreads dust evenly
            // from head to feet.
            var rowMap = new Dictionary<int, List<int>>();
            for (int y = 0; y < frameHeight; y++)
            {
                for (int x = 0; x < maskTexture.Width; x++)
                {
                    if (pixels[y * maskTexture.Width + x].A > 0)
                    {
                        if (!rowMap.ContainsKey(y))
                            rowMap[y] = new List<int>();
                        rowMap[y].Add(x);
                    }
                }
            }

            if (rowMap.Count == 0) return;

            // Pick a random row (uniform vertical distribution), then random X in that row
            var rows = new List<int>(rowMap.Keys);
            int py = rows[Main.rand.Next(rows.Count)];
            var xList = rowMap[py];
            int px = xList[Main.rand.Next(xList.Count)];

            // Convert pixel position to world position relative to projectile center
            // Origin is center of frame
            float originX = maskTexture.Width / 2f;
            float originY = frameHeight / 2f;
            float offsetX = px - originX;
            float offsetY = py - originY;

            // Handle horizontal flip
            if (doesFlip && projectile.spriteDirection == -1)
                offsetX = -offsetX;

            offsetX += startPosX;

            float worldX = projectile.Center.X + offsetX;
            float worldY = projectile.Center.Y + offsetY + 1f; // +1 matches the startPos.Y += 1 in draw

            Dust.NewDust(new Vector2(worldX, worldY), 0, 0, dustType, 0f, 0f, 0, default(Color), 1.5f);
        }

        public static void EmeraldspikeGlowandFadedraw(Projectile projectile, Texture2D texture, Color lightColor, Color WhatColor, float glowspeed, int numframes)
        {
            SpriteEffects spriteEffects = SpriteEffects.None;
            int frameHeight = texture.Height / numframes;
            Rectangle rectangle = texture.Frame(verticalFrames: numframes, frameY: 0);
            Vector2 origin = rectangle.Size() / 2f;
            float rotation = projectile.rotation;
            float scale = projectile.scale;
            Vector2 startPos = projectile.Center - Main.screenPosition;
            startPos.X += 0;
            startPos.Y += 0;
            if (projectile.type == ModContent.ProjectileType<Emeraldspike>())
            {
                Lighting.AddLight(projectile.Center, Color.Green.ToVector3() * (2f - glowspeed));
            }
            if (projectile.type == ModContent.ProjectileType<Emeraldspike2>())
            {
                Lighting.AddLight(projectile.Center, Color.Purple.ToVector3() * (2f - glowspeed));
            }
            if (projectile.type == ModContent.ProjectileType<Emeraldspike3>())
            {
                Lighting.AddLight(projectile.Center, Color.Silver.ToVector3() * (2f - glowspeed));
            }
            if (projectile.type == ModContent.ProjectileType<Emeraldspike3_2>())
            {
                Lighting.AddLight(projectile.Center, Color.Silver.ToVector3() * (3f - glowspeed));
            }
            Color drawColor = Color.Lerp(lightColor, WhatColor, 300f);
            drawColor = Color.Lerp(drawColor, Color.Transparent, glowspeed);
            if (projectile.spriteDirection == -1)
            {
                spriteEffects = SpriteEffects.FlipHorizontally;
            }
            Main.spriteBatch.Draw(texture, startPos, rectangle, (drawColor), rotation, origin, scale, spriteEffects, 0f);
        }

        public static void Emeraldspikedraw(Projectile projectile, Texture2D texture, Color lightColor, Color WhatColor, bool doesanimate, int startposX, int startposY, int NumFrames)
        {
            SpriteEffects spriteEffects = SpriteEffects.None;
            int frameHeight = texture.Height / NumFrames;
            Rectangle rectangle = texture.Frame(verticalFrames: NumFrames, frameY: 0);
            if (doesanimate)
            {
                int frameY = frameHeight * (projectile.frame);
                rectangle = texture.Frame(verticalFrames: NumFrames, frameY: (projectile.frame));
            }
            Vector2 origin = rectangle.Size() / 2f;
            float rotation = projectile.rotation;
            float scale = projectile.scale;
            Vector2 startPos = projectile.Center - Main.screenPosition;
            startPos.X += startposX;
            startPos.Y -= startposY;
            Color drawColor = Color.Lerp(lightColor, WhatColor, 300f);
            drawColor = Color.Lerp(drawColor, Color.Transparent, .50f);
            if (projectile.spriteDirection == -1)
            {
                spriteEffects = SpriteEffects.FlipHorizontally;
            }
            Main.spriteBatch.Draw(texture, startPos, rectangle, (drawColor), rotation, origin, scale, spriteEffects, 0f);
        }

        public static void Rupeedraw(Projectile projectile, Texture2D texture, Color lightColor, Color WhatColor, int NumFrames)
        {
            SpriteEffects spriteEffects = SpriteEffects.None;
            int frameHeight = texture.Height / NumFrames;
            int frameY = frameHeight * (projectile.frame);
            Rectangle rectangle = texture.Frame(verticalFrames: NumFrames, frameY: (projectile.frame));
            Vector2 origin = rectangle.Size() / 2f;
            float rotation = projectile.rotation;
            float scale = projectile.scale;
            Vector2 startPos = projectile.Center - Main.screenPosition;
            startPos.X += -5;
            startPos.Y += 0;
            Color drawColor = Color.Lerp(lightColor, WhatColor, 300f);
            drawColor = Color.Lerp(drawColor, Color.Transparent, .30f);
            Main.spriteBatch.Draw(texture, startPos, rectangle, (drawColor), rotation, origin, scale, spriteEffects, 0f);
        }

        public static void RupeeGlowandFadedraw(Projectile projectile, Texture2D texture, Color lightColor, Color WhatColor, int numframes)
        {
            if (rupeeAlpha4Counter)
            {
                rupeeAlpha4 -= 0.04f;
            }
            if (rupeeAlpha4 <= 0)
            {
                rupeeAlpha4Counter = false;
            }
            if (!rupeeAlpha4Counter)
            {
                rupeeAlpha4 += 0.004f;
            }
            if (rupeeAlpha4 >= 1)
            {
                rupeeAlpha4Counter = true;
            }
            SpriteEffects spriteEffects = SpriteEffects.None;
            int frameHeight = texture.Height / numframes;
            int frameY = frameHeight * (projectile.frame);
            Rectangle rectangle = texture.Frame(verticalFrames: numframes, frameY: (projectile.frame));
            Vector2 origin = rectangle.Size() / 2f;
            float rotation = projectile.rotation;
            float scale = projectile.scale;
            Vector2 startPos = projectile.Center - Main.screenPosition;
            startPos.X += -5;
            startPos.Y += 0;
            if (projectile.type == ModContent.ProjectileType<BouncingShard>())
            {
                Lighting.AddLight(projectile.Center, Color.Green.ToVector3() * (2f - rupeeAlpha4));
            }
            if (projectile.type == ModContent.ProjectileType<BouncingShard2>())
            {
                Lighting.AddLight(projectile.Center, Color.Purple.ToVector3() * (2f - rupeeAlpha4));
            }
            if (projectile.type == ModContent.ProjectileType<BouncingShard3>())
            {
                Lighting.AddLight(projectile.Center, Color.Silver.ToVector3() * (2f - rupeeAlpha4));
            }
            Color drawColor = Color.Lerp(lightColor, WhatColor, 300f);
            drawColor = Color.Lerp(drawColor, Color.Transparent, rupeeAlpha4);
            Main.spriteBatch.Draw(texture, startPos, rectangle, (drawColor), rotation, origin, scale, spriteEffects, 0f);
        }

        public static void SariaBubbleFaceSpawner(Projectile projectile, bool sleep, int canmove, bool cursed, int mood)
        {
            if (Main.myPlayer != projectile.owner) return;
            if (!(projectile.ModProjectile is Saria saria)) return;

            Player player = Main.player[projectile.owner];

            // --- Idle smile state machine ---
            // Gate: only while IdleAnimator is in free-eye roaming mode and Saria is not moving.
            Vector2 infrontofSaria = projectile.Center;
            infrontofSaria.X += (50 * projectile.spriteDirection);
            float between = Vector2.Distance(player.Center, infrontofSaria);

            // "cansee": player is within 50px of the point in front of Saria (not exactly on it),
            // facing toward her, Saria is stopped, and player is not pressing movement keys.
            bool playerNotMoving = !player.controlLeft && !player.controlRight;
            bool cansee = between < 50 && between > 0
                          && player.direction != projectile.spriteDirection
                          && canmove <= 0f
                          && playerNotMoving;

            bool eyeRoaming = saria.IdleAnimator.IsEyeFreeMode;
            bool roamJustStarted = eyeRoaming && !saria.WasEyeRoaming;
            bool roamJustEnded   = !eyeRoaming && saria.WasEyeRoaming;

            // Standing timer: counts up while player holds no movement keys, resets on movement.
            // Must reach 60 ticks (1 second) before a smile interaction can trigger.
            if (playerNotMoving)
                saria.PlayerStandingTimer = Math.Min(saria.PlayerStandingTimer + 1, 120);
            else
                saria.PlayerStandingTimer = 0;

            // On roam start: reset all smile state for a fresh interaction window.
            // If the player is already facing away when roam starts, count it immediately.
            if (roamJustStarted)
            {
                saria.SmileLockedUntilRoamReset = false;
                saria.SmileInteractionActive = false;
                saria.SmileAngerTimer = 0;
                saria.PlayerHasLookedAway = (player.direction == projectile.spriteDirection);
            }

            // On roam end: clear everything so the next roam session starts fresh.
            if (roamJustEnded)
            {
                saria.SmileLockedUntilRoamReset = false;
                saria.SmileInteractionActive = false;
                saria.SmileAngerTimer = 0;
                saria.PlayerHasLookedAway = false;
            }

            saria.WasEyeRoaming = eyeRoaming;

            // Track when the player has turned away (faces same direction as Saria)
            if (eyeRoaming && !saria.SmileInteractionActive && player.direction == projectile.spriteDirection)
                saria.PlayerHasLookedAway = true;

            // --- Trigger ---
            bool moodOk = saria.Mood == (int)MoodState.Normal || saria.Mood == (int)MoodState.Happy;
            bool smileTrigger = eyeRoaming
                && cansee
                && saria.PlayerHasLookedAway
                && !saria.SmileLockedUntilRoamReset
                && !saria.SmileInteractionActive
                && moodOk
                && saria.MoodPriority < 1
                && !sleep
                && saria.PlayerStandingTimer >= 60
                && !player.HasBuff(ModContent.BuffType<Sickness>())
                && !player.HasBuff(ModContent.BuffType<BloodmoonBuff>())
                && !player.HasBuff(ModContent.BuffType<StatLower>())
                && saria.ChannelTime < 20
                && projectile.ai[0] == 0;

            if (smileTrigger)
            {
                saria.SmileInteractionActive = true;
                saria.SmileAngerTimer = 0;
                saria.PlayerHasLookedAway = false;
                // Lock this roam session — only one interaction per roam visit.
                // Player must let free roam end and start again for a second interaction.
                saria.SmileLockedUntilRoamReset = true;
                // Only start a new happy mood if she isn't already happy from another source.
                if (saria.Mood != (int)MoodState.Happy)
                    saria.SetMoodFor(MoodState.Happy, 600, priority: 1);
                float radius = (float)Math.Sqrt(Main.rand.Next(1 * 1));
                double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                Dust.NewDust(new Vector2((projectile.Center.X + 40) + radius * (float)Math.Cos(angle), (projectile.Center.Y - 10) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<HeartDust>(), 0f, 0f, 0, default(Color), 1.5f);
                projectile.netUpdate = true;
            }

            // --- Active interaction: 1.5-second facing-away anger timer ---
            if (saria.SmileInteractionActive)
            {
                // Cancel cleanly (no anger) if Saria moves out of idle.
                bool sariaMoving = Math.Abs(projectile.velocity.X) > 0.5f || Math.Abs(projectile.velocity.Y) > 0.5f;
                if (sariaMoving)
                {
                    saria.SmileInteractionActive = false;
                    saria.SmileAngerTimer = 0;
                    projectile.netUpdate = true;
                }
                else
                {
                    // Anger ticks while player is facing AWAY from Saria's position.
                    // "Away" means the player's facing direction points in the same direction
                    // as the vector from Saria to the player (i.e., they have their back to her).
                    bool playerFacingAway = player.Center.X != projectile.Center.X
                        && player.direction == Math.Sign(player.Center.X - projectile.Center.X);
                    if (playerFacingAway)
                        saria.SmileAngerTimer++;
                    else
                        saria.SmileAngerTimer = 0;

                    // 90 ticks = 1.5 seconds
                    if (saria.SmileAngerTimer >= 90)
                    {
                        saria.SetMoodFor(MoodState.Angry, 300, priority: 2);
                        SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Error"), projectile.Center);
                        saria.SmileLockedUntilRoamReset = true;
                        saria.SmileInteractionActive = false;
                        saria.SmileAngerTimer = 0;
                        projectile.netUpdate = true;
                    }

                    // Natural expiry: happy mood timer ran out, interaction ends cleanly
                    if (saria.Mood != (int)MoodState.Happy)
                        saria.SmileInteractionActive = false;
                }
            }

            const int PeriodTimerReset = 7200;

            // --- Cursed event (bloodmoon / eclipse) — priority 20, immediate ---
            bool isCursedEvent = player.HasBuff(ModContent.BuffType<BloodmoonBuff>()) || player.HasBuff(ModContent.BuffType<EclipseBuff>());
            if (isCursedEvent && saria.Mood != (int)MoodState.Cursed)
            {
                saria.SetMoodFor(MoodState.Cursed, 3000, priority: 20);
                saria.PeriodTimerValue = PeriodTimerReset;
                projectile.netUpdate = true;
            }
            else if (!isCursedEvent && saria.Mood == (int)MoodState.Cursed)
            {
                saria.SetMoodFor(MoodState.Normal, 1, priority: 21);
                projectile.netUpdate = true;
            }

            // Competitive food signal active → show Competitive face while signal exists
            if (player.ownedProjectileCounts[ModContent.ProjectileType<Competitivetime>()] >= 1)
            {
                projectile.ShowBubbleFace(SariaExtensions1.BubbleFaceType.Competitive, 5);
            }

            // Eclipse — apply buff + dust (mood handled by cursed event above)
            if (player.active && Main.eclipse && !player.HasBuff(ModContent.BuffType<Soothing>()))
            {
                player.AddBuff(ModContent.BuffType<EclipseBuff>(), 20);
                projectile.SneezeDust(ModContent.DustType<Blood>(), 30, 1, -10, 3, -12);
                projectile.SneezeDust(ModContent.DustType<BlackSmoke>(), 20, 6, -10, 3, -12);
            }

            // Sickness → Sad (once per PeriodTimerValue)
            if (player.HasBuff(ModContent.BuffType<Sickness>()) && saria.PeriodTimerValue == 0 && !sleep)
            {
                saria.SetMoodFor(MoodState.Sad, 420, priority: 1);
                saria.PeriodTimerValue = PeriodTimerReset;
                projectile.netUpdate = true;
            }

            // Extinguished
            if (player.HasBuff(ModContent.BuffType<Extinguished>()) && saria.PeriodTimerValue == 0 && !sleep)
            {
                saria.SetMoodFor(MoodState.Sad, 420, priority: 1);
                saria.PeriodTimerValue = PeriodTimerReset;
                projectile.netUpdate = true;
            }

            // StatLower
            if (player.HasBuff(ModContent.BuffType<StatLower>()) && saria.PeriodTimerValue == 0 && !sleep)
            {
                saria.SetMoodFor(MoodState.Sad, 420, priority: 1);
                saria.PeriodTimerValue = PeriodTimerReset;
                projectile.netUpdate = true;
            }

            // StatRaise
            if (player.HasBuff(ModContent.BuffType<StatRaise>()) && saria.PeriodTimerValue == 0 && !sleep)
            {
                saria.SetMoodFor(MoodState.Happy, 420, priority: 1);
                saria.PeriodTimerValue = PeriodTimerReset;
                projectile.netUpdate = true;
            }

        }
    }
}
