using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using SariaMod.Items.Strange;

namespace SariaMod.Items.zTalking
{
    public partial class DialogueUIState : UIState
    {
        // ============================================================
        // DRAWING
        // ============================================================
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!_isActive || _currentNode == null) return;
            // Also draw during close animation even after _isActive will be cleared
            if (_panelAnimState == PanelAnimState.Closing && _animAlpha <= 0f) return;

            // Tick alpha pulse counters once per frame (frame-guarded, safe to call from anywhere)
            SariaDrawingExtensions.UpdateAlphaCounters();

            // Switch to PointClamp for crisp pixel art rendering (all dialogue UI textures)
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);

            float scale = GetUIScale();
            float a = _animAlpha; // 0..1 overall panel alpha from animation
            Vector2 screenCenter = new Vector2(Main.screenWidth, Main.screenHeight) / 2f;
            Vector2 panelPos = screenCenter + _currentPanelOffset * scale;

            // Apply visual-only animation offset (never persisted) + background offset
            Vector2 adjustedPanelPos = panelPos + BackgroundOffset * scale + _animVisualOffset * scale;

            DrawBackground(spriteBatch, adjustedPanelPos, scale, a);
            DrawGreetingPortrait(spriteBatch, adjustedPanelPos, scale, a);
            DrawPortrait(spriteBatch, adjustedPanelPos, scale, a);
            DrawGreetingOverHead(spriteBatch, adjustedPanelPos, scale, a);
            DrawDialogueText(spriteBatch, adjustedPanelPos, scale, a);
            // Hide button labels and text while opening; show blank button shells only
            bool labelsVisible = _panelAnimState == PanelAnimState.Idle;
            DrawButtons(spriteBatch, adjustedPanelPos, scale, a, labelsVisible);

            // Show cutscene indicator (only when fully open)
            if (_isCutsceneMode && labelsVisible)
            {
                Vector2 cutscenePos = adjustedPanelPos + new Vector2(0, -100) * scale;
                Utils.DrawBorderString(spriteBatch, "~ CUTSCENE ~", cutscenePos, Color.Gold * 0.8f * a, 0.7f * scale, 0.5f, 0.5f);
            }

            if (_isEnding && _isTextComplete && labelsVisible)
            {
                float secondsLeft = _exitCountdown / 60f;
                string exitText = $"Closing in {secondsLeft:F1}s...";
                Vector2 exitPos = adjustedPanelPos + new Vector2(0, 70) * scale;
                Utils.DrawBorderString(spriteBatch, exitText, exitPos, Color.Gray * 0.7f * a, 0.8f * scale, 0.5f, 0.5f);
            }

            // Restore default sampler state before returning to framework
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);
            base.Draw(spriteBatch);
        }

        private void DrawBackground(SpriteBatch spriteBatch, Vector2 panelPos, float scale, float alpha)
        {
            string panelPath = "SariaMod/Items/zTalking/SariaPanel";
            Texture2D panelTexture;
            try { panelTexture = ModContent.Request<Texture2D>(panelPath).Value; }
            catch { return; }

            if (panelTexture != null)
            {
                Vector2 origin = new Vector2(panelTexture.Width, panelTexture.Height) / 2f;
                spriteBatch.Draw(panelTexture, panelPos, null, Color.White * alpha, 0f, origin, scale, SpriteEffects.None, 0f);
            }
        }

        private void DrawGreetingPortrait(SpriteBatch spriteBatch, Vector2 panelPos, float scale, float alpha)
        {
            string greetingPath = $"SariaMod/Items/zTalking/Greetings{_sariaTransform + 1}";
            Texture2D greetingTexture;
            try { greetingTexture = ModContent.Request<Texture2D>(greetingPath).Value; }
            catch { greetingTexture = ModContent.Request<Texture2D>("SariaMod/Items/zTalking/Greetings1").Value; }

            if (greetingTexture != null)
            {
                Vector2 greetingPos = panelPos + GreetingPortraitOffset * scale;
                Vector2 origin = new Vector2(greetingTexture.Width, greetingTexture.Height) / 2f;
                spriteBatch.Draw(greetingTexture, greetingPos, null, Color.White * alpha, 0f, origin, scale, SpriteEffects.None, 0f);
            }
        }

        private void DrawGreetingOverHead(SpriteBatch spriteBatch, Vector2 panelPos, float scale, float alpha)
        {
            if (_sariaTransform == 1)
            {
                // Greetings2OverHead appears on the second transform (Zora form)
                string overHeadPath = "SariaMod/Items/zTalking/Greetings2OverHead";
                Texture2D overHeadTexture;
                try { overHeadTexture = ModContent.Request<Texture2D>(overHeadPath).Value; }
                catch { return; }

                if (overHeadTexture != null)
                {
                    Vector2 overHeadPos = panelPos + GreetingPortraitOffset * scale;
                    Vector2 origin = new Vector2(overHeadTexture.Width, overHeadTexture.Height) / 2f;
                    spriteBatch.Draw(overHeadTexture, overHeadPos, null, Color.White * alpha, 0f, origin, scale, SpriteEffects.None, 0f);
                }
            }
            else if (_sariaTransform == 2)
            {
                // Greetings3OverHead appears on the third transform (Gerudo form) - Twinrova fire hair
                string overHeadPath = "SariaMod/Items/zTalking/Greetings3OverHead";
                Texture2D overHeadTexture;
                try { overHeadTexture = ModContent.Request<Texture2D>(overHeadPath).Value; }
                catch { return; }

                if (overHeadTexture != null)
                {
                    Vector2 overHeadPos = panelPos + GreetingPortraitOffset * scale;
                    Vector2 origin = new Vector2(overHeadTexture.Width, overHeadTexture.Height) / 2f;

                    // Flame shimmer effect
                    ulong randShakeEffect = (Main.GameUpdateCount / 8) ^ (ulong)((long)overHeadPos.Y << 20 | (long)(uint)overHeadPos.X);
                    float shakeX = Utils.RandomInt(ref randShakeEffect, -4, -3) * 0.07f;
                    float shakeY = Utils.RandomInt(ref randShakeEffect, -4, 3) * 0.07f;
                    Vector2 shimmerPos = overHeadPos + new Vector2(shakeY, shakeX) * scale;

                    // Swap to NonPremultiplied so the texture's straight alpha renders with exact original colors
                    spriteBatch.End();
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);

                    spriteBatch.Draw(overHeadTexture, shimmerPos, null, Color.White * alpha, 0f, origin, scale, SpriteEffects.None, 0f);

                    // Restore original blend state for remaining UI draws
                    spriteBatch.End();
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);
                }
            }
            else if (_sariaTransform == 3)
            {
                // SariaSparksPortrait overlay for electric form — only when sparks are active on Saria
                bool sparksActive = false;
                if (_sariaProjectile != null && _sariaProjectile.active && _sariaProjectile.ModProjectile is Saria sparksSaria)
                    sparksActive = sparksSaria.SpecialAnimateValue > 0;

                if (sparksActive)
                {
                    string sparksPath = "SariaMod/Items/zTalking/SariaSparksPortrait";
                    Texture2D sparksTexture;
                    try { sparksTexture = ModContent.Request<Texture2D>(sparksPath).Value; }
                    catch { sparksTexture = null; }

                    if (sparksTexture != null)
                    {
                        // 14 frames, advance every 3 game ticks (matches SariaSparksDraw)
                        int sparksFrame = (int)Main.GameUpdateCount / 3 % 14;
                        Rectangle sparksRect = sparksTexture.Frame(verticalFrames: 14, frameY: sparksFrame);
                        Vector2 sparksOrigin = sparksRect.Size() / 2f;
                        Vector2 sparksPos = panelPos + SparksPortraitOffset * scale;

                        Color sparksColor = Color.Lerp(Color.White, Color.LightBlue, 2f);
                        sparksColor *= 0.85f * alpha;

                        spriteBatch.End();
                        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);

                        spriteBatch.Draw(sparksTexture, sparksPos, sparksRect, sparksColor, 0f, sparksOrigin, scale, SpriteEffects.None, 0f);

                        spriteBatch.End();
                        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);
                    }
                }
            }
            else if (_sariaTransform == 4)
            {
                // Greetings5OverHead with DialogueUIMaskdraw-style alpha2 fade
                string overHeadPath = "SariaMod/Items/zTalking/Greetings5OverHead";
                Texture2D overHeadTexture;
                try { overHeadTexture = ModContent.Request<Texture2D>(overHeadPath).Value; }
                catch { overHeadTexture = null; }

                if (overHeadTexture != null)
                {
                    Vector2 overHeadPos = panelPos + GreetingPortraitOffset * scale;
                    Vector2 origin = new Vector2(overHeadTexture.Width, overHeadTexture.Height) / 2f;

                    Color drawColor = Color.White;
                    drawColor = Color.Lerp(drawColor, Color.FloralWhite, 30f);
                    drawColor = Color.Lerp(drawColor, Color.Transparent, SariaDrawingExtensions.alpha2);

                    spriteBatch.End();
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);

                    spriteBatch.Draw(overHeadTexture, overHeadPos, null, drawColor, 0f, origin, scale, SpriteEffects.None, 0f);

                    spriteBatch.End();
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);
                }

                // Greetings5OverHead2
                string overHeadPath2 = "SariaMod/Items/zTalking/Greetings5OverHead2";
                Texture2D overHeadTexture2;
                try { overHeadTexture2 = ModContent.Request<Texture2D>(overHeadPath2).Value; }
                catch { overHeadTexture2 = null; }

                if (overHeadTexture2 != null)
                {
                    Vector2 overHeadPos2 = panelPos + GreetingPortraitOffset * scale;
                    Vector2 origin2 = new Vector2(overHeadTexture2.Width, overHeadTexture2.Height) / 2f;

                    Color drawColor2 = Color.White;
                    drawColor2 = Color.Lerp(drawColor2, Color.FloralWhite, 30f);
                    drawColor2 = Color.Lerp(drawColor2, Color.Transparent, SariaDrawingExtensions.alpha3);
                    drawColor2 *= alpha;

                    spriteBatch.End();
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);

                    spriteBatch.Draw(overHeadTexture2, overHeadPos2, null, drawColor2, 0f, origin2, scale, SpriteEffects.None, 0f);

                    spriteBatch.End();
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);
                }
            }
        }

        private void DrawPortrait(SpriteBatch spriteBatch, Vector2 panelPos, float scale, float alpha)
        {
            string faceSet = _currentNode?.FaceSetName;
            Vector2 eyesPos = panelPos + (_sariaTransform == 6 ? Eyes7Offset : EyesOffset) * scale;
            DrawEyes(spriteBatch, eyesPos, scale, faceSet, alpha);
            DrawMouth(spriteBatch, panelPos + MouthOffset * scale, scale, faceSet, alpha);

            var extra = DialogueFaceSetRegistry.TryResolveExtraTexture(faceSet, _sariaTransform);
            if (extra != null)
            {
                Vector2 origin = new Vector2(extra.Width, extra.Height) / 2f;
                spriteBatch.Draw(extra, panelPos, null, Color.White * alpha, 0f, origin, scale, SpriteEffects.None, 0f);
            }

            // SariaSparksPortrait overlay for electric form (transform 3) — synced to in-world sparks
            if (_sariaTransform == 3)
            {
                bool sparksActive = false;
                if (_sariaProjectile != null && _sariaProjectile.active && _sariaProjectile.ModProjectile is Saria sparksSaria)
                    sparksActive = sparksSaria.SpecialAnimateValue > 0;

                if (sparksActive)
                {
                    string sparksPath = "SariaMod/Items/zTalking/SariaSparksPortrait";
                    Texture2D sparksTexture;
                    try { sparksTexture = ModContent.Request<Texture2D>(sparksPath).Value; }
                    catch { sparksTexture = null; }

                    if (sparksTexture != null)
                    {
                        // 14 frames, advance every 3 game ticks (matches SariaSparksDraw)
                        int sparksFrame = (int)Main.GameUpdateCount / 3 % 14;
                        Rectangle sparksRect = sparksTexture.Frame(verticalFrames: 14, frameY: sparksFrame);
                        Vector2 sparksOrigin = sparksRect.Size() / 2f;
                        Vector2 sparksPos = panelPos + SparksPortraitOffset * scale;

                        Color sparksColor = Color.Lerp(Color.White, Color.LightBlue, 2f);
                        sparksColor *= 0.85f * alpha;

                        spriteBatch.End();
                        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);

                        spriteBatch.Draw(sparksTexture, sparksPos, sparksRect, sparksColor, 0f, sparksOrigin, scale, SpriteEffects.None, 0f);

                        spriteBatch.End();
                        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);
                    }
                }
            }
        }

        private void DrawEyes(SpriteBatch spriteBatch, Vector2 position, float scale, string faceSetName, float alpha)
        {
            Texture2D eyeTexture = DialogueFaceSetRegistry.TryResolveEyesTexture(faceSetName, _sariaTransform);
            if (eyeTexture == null) return;

            int numFrames = 4;
            int frameHeight = eyeTexture.Height / numFrames;
            Rectangle sourceRect = new Rectangle(0, _eyeFrame * frameHeight, eyeTexture.Width, frameHeight);
            Vector2 origin = new Vector2(sourceRect.Width, sourceRect.Height) / 2f;

            if (_sariaTransform == 4)
            {
                // Form 5: draw base eyes underneath first (uses current face set)
                var baseSet = DialogueFaceSetRegistry.Get(faceSetName);
                string basePath = $"SariaMod/Items/zTalking/{baseSet.EyesPrefix}1";
                if (ModContent.RequestIfExists(basePath, out ReLogic.Content.Asset<Texture2D> baseAsset))
                {
                    Texture2D baseEyes = baseAsset.Value;
                    int baseFrameH = baseEyes.Height / numFrames;
                    Rectangle baseRect = new Rectangle(0, _eyeFrame * baseFrameH, baseEyes.Width, baseFrameH);
                    Vector2 baseOrigin = new Vector2(baseRect.Width, baseRect.Height) / 2f;
                    spriteBatch.Draw(baseEyes, position, baseRect, Color.White * alpha, 0f, baseOrigin, scale, SpriteEffects.None, 0f);
                }

                // Then draw face-set-specific Eyes5 glow overlay with alpha3 fade
                // Uses Additive blending so the glow brightens the base eyes without darkening white areas
                // Uses alpha3 to match SariaEyesGlowandFadedraw
                Color glowColor = Color.White;
                glowColor = Color.Lerp(glowColor, Color.FloralWhite, 30f);
                glowColor = Color.Lerp(glowColor, Color.Transparent, SariaDrawingExtensions.alpha3);
                glowColor *= alpha;

                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);

                spriteBatch.Draw(eyeTexture, position, sourceRect, glowColor, 0f, origin, scale, SpriteEffects.None, 0f);

                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);
            }
            else if (_sariaTransform == 2)
            {
                // Form 3: NonPremultiplied for correct transparent pixels on Default-Eyes3
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);

                spriteBatch.Draw(eyeTexture, position, sourceRect, Color.White * alpha, 0f, origin, scale, SpriteEffects.None, 0f);

                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);
            }
            else if (_sariaTransform == 6)
            {
                // Form 7: old-style broad glow (constant) + new static ripples & jitter (speaking)
                Rectangle singleRect = new Rectangle(0, 0, eyeTexture.Width, eyeTexture.Height);
                Vector2 singleOrigin = new Vector2(singleRect.Width, singleRect.Height) / 2f;

                // Speaking intensity: drives blend toward new effect + jitter
                float jitterTarget = _isSpeakingThisFrame ? 1f : 0f;
                _poeWaveStrength = MathHelper.Lerp(_poeWaveStrength, jitterTarget, 0.05f);

                // Phase always advances so the glow never freezes
                _poeWavePhase += 0.09f;
                if (_poeWavePhase > MathF.PI * 20f)
                    _poeWavePhase -= MathF.PI * 20f;

                {
                    int texH = eyeTexture.Height;
                    int texW = eyeTexture.Width;
                    float topLeftX = position.X - singleOrigin.X * scale;
                    float topLeftY = position.Y - singleOrigin.Y * scale;

                    ulong jitterSeed = Main.GameUpdateCount;

                    for (int row = 0; row < texH; row++)
                    {
                        float t = 1f - (float)row / texH;

                        // Base glow: dense static ripples, always on
                        float baseWave = MathF.Sin(_poeWavePhase + t * MathF.PI * 5f);
                        float baseAlpha = 1f - 0.45f * (baseWave * 0.5f + 0.5f);

                        // Speaking glow: broad sweeping ripples
                        float speakWave = MathF.Sin(_poeWavePhase + t * MathF.PI * 2.5f);
                        float speakAlpha = 1f - 0.6f * (speakWave * 0.5f + 0.5f);

                        // Blend: idle = old glow, speaking = new glow
                        float rowAlpha = MathHelper.Lerp(baseAlpha, speakAlpha, _poeWaveStrength);

                        // Horizontal jitter: only when speaking
                        float jitterAmount = _poeWaveStrength * 0.6f;
                        ulong rowSeed = jitterSeed * 31ul + (ulong)row * 7ul;
                        float jitter = ((float)(rowSeed % 1000u) / 500f - 1f) * jitterAmount * scale;

                        Rectangle rowRect = new Rectangle(0, row, texW, 1);
                        Vector2 rowPos = new Vector2(topLeftX + jitter, topLeftY + row * scale);
                        Color rowColor = Color.White * (rowAlpha * alpha);

                        spriteBatch.Draw(eyeTexture, rowPos, rowRect, rowColor, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
                    }
                }
            }
            else
            {
                spriteBatch.Draw(eyeTexture, position, sourceRect, Color.White * alpha, 0f, origin, scale, SpriteEffects.None, 0f);
            }
        }

        private void DrawMouth(SpriteBatch spriteBatch, Vector2 position, float scale, string faceSetName, float alpha)
        {
            // Seventh form (transform 6) has no mouth
            if (_sariaTransform == 6) return;

            Texture2D mouthTexture = DialogueFaceSetRegistry.TryResolveMouthTexture(faceSetName, _sariaTransform);
            if (mouthTexture == null) return;

            int numFrames = 5;
            int frameHeight = mouthTexture.Height / numFrames;
            Rectangle sourceRect = new Rectangle(0, _mouthFrame * frameHeight, mouthTexture.Width, frameHeight);
            Vector2 origin = new Vector2(sourceRect.Width, sourceRect.Height) / 2f;

            spriteBatch.Draw(mouthTexture, position, sourceRect, Color.White * alpha, 0f, origin, scale, SpriteEffects.None, 0f);
        }

        private void DrawDialogueText(SpriteBatch spriteBatch, Vector2 panelPos, float scale, float alpha)
        {
            if (_coloredText.Count == 0) return;

            Vector2 textStart = panelPos + GetTextOffset() * scale;

            float baseTextScale = TextScale * scale;

            float maxHeightPx = GetTextMaxHeight() * scale;

            int totalLines = _wrappedLines?.Count ?? 0;
            float lineHeightAtBase = LineHeightBase * scale;
            float textScale = ComputeTextScaleToFit(baseTextScale, lineHeightAtBase, totalLines, maxHeightPx);

            float lineHeight = LineHeightBase * (textScale / baseTextScale) * scale;
            DynamicSpriteFont font = FontAssets.MouseText.Value;

            Vector2 currentPos = textStart;
            int charIndex = 0;

            foreach (var line in _wrappedLines)
            {
                float xOffset = 0;
                foreach (var cc in line)
                {
                    if (charIndex >= _coloredText.Count) return;
                    string charStr = cc.Character.ToString();
                    Vector2 charSize = font.MeasureString(charStr) * textScale;

                    Utils.DrawBorderString(spriteBatch, charStr, currentPos + new Vector2(xOffset, 0), _coloredText[charIndex].TextColor * alpha, textScale, 0f, 0f);
                    xOffset += charSize.X;
                    charIndex++;
                }
                currentPos.Y += lineHeight;
            }
        }

        private void DrawButtons(SpriteBatch spriteBatch, Vector2 panelPos, float scale, float alpha, bool labelsVisible)
        {
            // Always draw the exit button; disable/grey it if not enabled.
            DrawButton(spriteBatch, "SariaMod/Items/zTalking/ExitChoiceUI",
                panelPos + ExitButtonOffset * scale, labelsVisible ? "" : null, _hoveredButton == 4, _buttonEnabled[4] && !_isEnding, 3, scale, alpha);

            DrawButton(spriteBatch, "SariaMod/Items/zTalking/BackChoiceUI",
                panelPos + BackButtonOffset * scale, labelsVisible ? "" : null, _hoveredButton == 3, _buttonEnabled[3], 3, scale, alpha);

            DrawButtonWithWrappedText(spriteBatch, "SariaMod/Items/zTalking/SmallChoiceUI",
                panelPos + Button1Offset * scale,
                labelsVisible ? (_currentNode?.Button1Label ?? "") : "",
                _hoveredButton == 0, _buttonEnabled[0], 3, scale, alpha, labelsVisible);

            DrawButtonWithWrappedText(spriteBatch, "SariaMod/Items/zTalking/SmallChoiceUI",
                panelPos + Button2Offset * scale,
                labelsVisible ? (_currentNode?.Button2Label ?? "") : "",
                _hoveredButton == 1, _buttonEnabled[1], 3, scale, alpha, labelsVisible);

            DrawButtonWithWrappedText(spriteBatch, "SariaMod/Items/zTalking/SmallChoiceUI",
                panelPos + Button3Offset * scale,
                labelsVisible ? (_currentNode?.Button3Label ?? "") : "",
                _hoveredButton == 2, _buttonEnabled[2], 3, scale, alpha, labelsVisible);
        }

        private void DrawButton(SpriteBatch spriteBatch, string texturePath, Vector2 position, string label, bool isHovered, bool isEnabled, int numFrames, float scale, float alpha)
        {
            Texture2D texture = ModContent.Request<Texture2D>(texturePath).Value;
            if (texture == null) return;

            int frameHeight = texture.Height / numFrames;
            int frameIndex;
            if (!isEnabled)
                frameIndex = 2;
            else if (isHovered)
                frameIndex = 1;
            else
                frameIndex = 0;

            Rectangle sourceRect = new Rectangle(0, frameIndex * frameHeight, texture.Width, frameHeight);
            Vector2 origin = new Vector2(sourceRect.Width, sourceRect.Height) / 2f;
            Color buttonColor = (isEnabled ? Color.White : Color.Gray * 0.6f) * alpha;

            spriteBatch.Draw(texture, position, sourceRect, buttonColor, 0f, origin, scale, SpriteEffects.None, 0f);

            if (!string.IsNullOrEmpty(label))
            {
                Vector2 labelPos = position;
                if (isHovered && isEnabled)
                {
                    labelPos.X += 2 * scale;
                    labelPos.Y += 2 * scale;
                }
                Color labelColor = (isEnabled ? Color.White : Color.Gray * 0.6f) * alpha;
                Utils.DrawBorderString(spriteBatch, label, labelPos, labelColor, 0.75f * scale, 0.5f, 0.5f);
            }
        }

        private void DrawButtonWithWrappedText(SpriteBatch spriteBatch, string texturePath, Vector2 position, string label, bool isHovered, bool isEnabled, int numFrames, float scale, float alpha, bool showLabel)
        {
            Texture2D texture = ModContent.Request<Texture2D>(texturePath).Value;
            if (texture == null) return;

            int frameHeight = texture.Height / numFrames;
            int frameIndex;
            if (!isEnabled)
                frameIndex = 2;
            else if (isHovered)
                frameIndex = 1;
            else
                frameIndex = 0;

            Rectangle sourceRect = new Rectangle(0, frameIndex * frameHeight, texture.Width, frameHeight);
            Vector2 origin = new Vector2(sourceRect.Width, sourceRect.Height) / 2f;
            Color buttonColor = (isEnabled ? Color.White : Color.Gray * 0.6f) * alpha;

            spriteBatch.Draw(texture, position, sourceRect, buttonColor, 0f, origin, scale, SpriteEffects.None, 0f);

            if (!showLabel || string.IsNullOrEmpty(label)) return;

            Vector2 labelPos = position;
            if (isHovered && isEnabled)
            {
                labelPos.X += 2 * scale;
                labelPos.Y += 2 * scale;
            }

            Color labelColor = (isEnabled ? Color.White : Color.Gray * 0.6f) * alpha;
            DynamicSpriteFont font = FontAssets.MouseText.Value;

            float maxWidth = ButtonLabelMaxWidth * scale;
            float maxHeight = 26 * scale;

            // Desired layout:
            // - 1 word: centered
            // - 2 words: stack unless they fit side-by-side
            // - 3+ words: normal word wrap
            string[] words = label.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            float baseScale = ButtonLabelScale * scale;
            float finalScale = baseScale;

            List<string> lines = new List<string>();

            if (words.Length == 1)
            {
                lines.Add(words[0]);
            }
            else if (words.Length == 2)
            {
                string sideBySide = words[0] + " " + words[1];
                if (font.MeasureString(sideBySide).X * baseScale <= maxWidth)
                {
                    lines.Add(sideBySide);
                }
                else
                {
                    lines.Add(words[0]);
                    lines.Add(words[1]);
                }
            }
            else
            {
                string current = "";
                foreach (var w in words)
                {
                    string test = string.IsNullOrEmpty(current) ? w : current + " " + w;
                    if (font.MeasureString(test).X * baseScale <= maxWidth)
                    {
                        current = test;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(current))
                            lines.Add(current);
                        current = w;
                    }
                }
                if (!string.IsNullOrEmpty(current))
                    lines.Add(current);

                if (lines.Count > 5)
                {
                    // Hard limit: collapse remainder into last line
                    lines = lines.GetRange(0, 5);
                }
            }

            // Shrink to fit width/height as a centered block
            int attempts = 0;
            while (attempts < 8)
            {
                float lineHeight = ButtonLineHeight * scale * (finalScale / baseScale);
                float totalHeight = lines.Count * lineHeight;

                bool tooWide = false;
                foreach (var l in lines)
                {
                    if (font.MeasureString(l).X * finalScale > maxWidth)
                    {
                        tooWide = true;
                        break;
                    }
                }

                if (!tooWide && totalHeight <= maxHeight)
                    break;

                finalScale *= 0.9f;
                if (finalScale < ButtonLabelMinScale * scale)
                {
                    finalScale = ButtonLabelMinScale * scale;
                    break;
                }

                attempts++;
            }

            float actualLineHeight2 = ButtonLineHeight * scale * (finalScale / baseScale);
            float totalTextHeight = lines.Count * actualLineHeight2;
            float startY = labelPos.Y - (totalTextHeight / 2f) + (actualLineHeight2 / 2f);

            for (int i = 0; i < lines.Count; i++)
            {
                Vector2 linePos = new Vector2(labelPos.X, startY + i * actualLineHeight2);
                Utils.DrawBorderString(spriteBatch, lines[i], linePos, labelColor, finalScale, 0.5f, 0.5f);
            }
        }

        private Vector2 GetTextOffset()
        {
            try
            {
                return new Vector2(FairyConfig.Instance?.DialogueTextOffsetX ?? TextOffset.X,
                    FairyConfig.Instance?.DialogueTextOffsetY ?? TextOffset.Y);
            }
            catch
            {
                return TextOffset;
            }
        }

        private float GetTextMaxWidth()
        {
            try { return FairyConfig.Instance?.DialogueTextMaxWidth ?? TextMaxWidth; }
            catch { return TextMaxWidth; }
        }

        private float GetTextMaxHeight()
        {
            try { return FairyConfig.Instance?.DialogueTextMaxHeight ?? 48f; }
            catch { return 48f; }
        }

        private float ComputeTextScaleToFit(float baseTextScale, float scaledLineHeight, int totalLines, float maxHeight)
        {
            if (totalLines <= 0)
                return baseTextScale;

            float clampMin = 0.45f;

            float fitForLines(int allowedLines)
            {
                float requiredHeight = allowedLines * scaledLineHeight;
                if (requiredHeight <= 0)
                    return baseTextScale;

                float ratio = maxHeight / requiredHeight;
                return baseTextScale * MathHelper.Clamp(ratio, clampMin, 1f);
            }

            // Stage 1: if it already fits within 3 lines at base scale, keep it.
            if (totalLines <= 3)
                return baseTextScale;

            // Stage 2: shrink to fit within the configured height for 3 lines.
            float s3 = fitForLines(3);
            if (totalLines <= 4)
                return s3;

            // Stage 3: once text is small enough, allow an extra line if needed.
            float s4 = fitForLines(4);
            if (totalLines <= 5)
                return s4;

            // Stage 4: last resort allow 5 lines.
            return fitForLines(5);
        }
    }
}
