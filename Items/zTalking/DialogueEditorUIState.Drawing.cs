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
    internal sealed partial class DialogueEditorUIState : UIState
    {
        private void DrawBackground(SpriteBatch spriteBatch, Vector2 panelPos, float scale)
        {
            string panelPath = "SariaMod/Items/zTalking/SariaPanel";
            Texture2D panelTexture;
            try { panelTexture = ModContent.Request<Texture2D>(panelPath).Value; }
            catch { return; }

            if (panelTexture != null)
            {
                Vector2 origin = new Vector2(panelTexture.Width, panelTexture.Height) / 2f;
                spriteBatch.Draw(panelTexture, panelPos, null, Color.White, 0f, origin, scale, SpriteEffects.None, 0f);
            }

            if (_isPreviewPlaying)
                return;

            Vector2 footerPos = panelPos + new Vector2(0, 82) * scale;
            Utils.DrawBorderString(spriteBatch, "EDITOR: Type to edit | Enter=save runtime | Bottom buttons to save/exit", footerPos, Color.LightGray, 0.55f * scale, 0.5f, 0.5f);
        }

        private void DrawEditorGreetingPortrait(SpriteBatch spriteBatch, Vector2 panelPos, float scale)
        {
            string greetingPath = $"SariaMod/Items/zTalking/Greetings{_transformPreviewIndex + 1}";
            Texture2D greetingTexture;
            try { greetingTexture = ModContent.Request<Texture2D>(greetingPath).Value; }
            catch { greetingTexture = ModContent.Request<Texture2D>("SariaMod/Items/zTalking/Greetings1").Value; }

            if (greetingTexture != null)
            {
                Vector2 greetingPos = panelPos + DialogueUIState.GreetingPortraitOffset * scale;
                Vector2 origin = new Vector2(greetingTexture.Width, greetingTexture.Height) / 2f;
                spriteBatch.Draw(greetingTexture, greetingPos, null, Color.White, 0f, origin, scale, SpriteEffects.None, 0f);
            }
        }

        private void DrawEditorGreetingOverHead(SpriteBatch spriteBatch, Vector2 panelPos, float scale)
        {
            if (_transformPreviewIndex == 1)
            {
                string overHeadPath = "SariaMod/Items/zTalking/Greetings2OverHead";
                Texture2D overHeadTexture;
                try { overHeadTexture = ModContent.Request<Texture2D>(overHeadPath).Value; }
                catch { return; }

                if (overHeadTexture != null)
                {
                    Vector2 overHeadPos = panelPos + DialogueUIState.GreetingPortraitOffset * scale;
                    Vector2 origin = new Vector2(overHeadTexture.Width, overHeadTexture.Height) / 2f;
                    spriteBatch.Draw(overHeadTexture, overHeadPos, null, Color.White, 0f, origin, scale, SpriteEffects.None, 0f);
                }
            }
            else if (_transformPreviewIndex == 2)
            {
                string overHeadPath = "SariaMod/Items/zTalking/Greetings3OverHead";
                Texture2D overHeadTexture;
                try { overHeadTexture = ModContent.Request<Texture2D>(overHeadPath).Value; }
                catch { return; }

                if (overHeadTexture != null)
                {
                    Vector2 overHeadPos = panelPos + DialogueUIState.GreetingPortraitOffset * scale;
                    Vector2 origin = new Vector2(overHeadTexture.Width, overHeadTexture.Height) / 2f;

                    ulong randShakeEffect = (Main.GameUpdateCount / 8) ^ (ulong)((long)overHeadPos.Y << 20 | (long)(uint)overHeadPos.X);
                    float shakeX = Utils.RandomInt(ref randShakeEffect, -4, -3) * 0.07f;
                    float shakeY = Utils.RandomInt(ref randShakeEffect, -4, 3) * 0.07f;
                    Vector2 shimmerPos = overHeadPos + new Vector2(shakeY, shakeX) * scale;

                    spriteBatch.End();
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);

                    spriteBatch.Draw(overHeadTexture, shimmerPos, null, Color.White, 0f, origin, scale, SpriteEffects.None, 0f);

                    spriteBatch.End();
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);
                }
            }
            else if (_transformPreviewIndex == 3)
            {
                bool sparksActive = false;
                int sariaType = ModContent.ProjectileType<Saria>();
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile proj = Main.projectile[i];
                    if (proj != null && proj.active && proj.type == sariaType && proj.owner == Main.myPlayer && proj.ModProjectile is Saria sparksSaria)
                    {
                        sparksActive = sparksSaria.SpecialAnimateValue > 0;
                        break;
                    }
                }

                if (sparksActive)
                {
                    string sparksPath = "SariaMod/Items/zTalking/SariaSparksPortrait";
                    Texture2D sparksTexture;
                    try { sparksTexture = ModContent.Request<Texture2D>(sparksPath).Value; }
                    catch { sparksTexture = null; }

                    if (sparksTexture != null)
                    {
                        int sparksFrame = (int)Main.GameUpdateCount / 3 % 14;
                        Rectangle sparksRect = sparksTexture.Frame(verticalFrames: 14, frameY: sparksFrame);
                        Vector2 sparksOrigin = sparksRect.Size() / 2f;
                        Vector2 sparksPos = panelPos + DialogueUIState.SparksPortraitOffset * scale;

                        Color sparksColor = Color.Lerp(Color.White, Color.LightBlue, 2f);
                        sparksColor *= 0.85f;

                        spriteBatch.End();
                        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);

                        spriteBatch.Draw(sparksTexture, sparksPos, sparksRect, sparksColor, 0f, sparksOrigin, scale, SpriteEffects.None, 0f);

                        spriteBatch.End();
                        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);
                    }
                }
            }
            else if (_transformPreviewIndex == 4)
            {
                string overHeadPath = "SariaMod/Items/zTalking/Greetings5OverHead";
                Texture2D overHeadTexture;
                try { overHeadTexture = ModContent.Request<Texture2D>(overHeadPath).Value; }
                catch { overHeadTexture = null; }

                if (overHeadTexture != null)
                {
                    Vector2 overHeadPos = panelPos + DialogueUIState.GreetingPortraitOffset * scale;
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

                string overHeadPath2 = "SariaMod/Items/zTalking/Greetings5OverHead2";
                Texture2D overHeadTexture2;
                try { overHeadTexture2 = ModContent.Request<Texture2D>(overHeadPath2).Value; }
                catch { overHeadTexture2 = null; }

                if (overHeadTexture2 != null)
                {
                    Vector2 overHeadPos2 = panelPos + DialogueUIState.GreetingPortraitOffset * scale;
                    Vector2 origin2 = new Vector2(overHeadTexture2.Width, overHeadTexture2.Height) / 2f;

                    Color drawColor2 = Color.White;
                    drawColor2 = Color.Lerp(drawColor2, Color.FloralWhite, 30f);
                    drawColor2 = Color.Lerp(drawColor2, Color.Transparent, SariaDrawingExtensions.alpha3);

                    spriteBatch.End();
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);

                    spriteBatch.Draw(overHeadTexture2, overHeadPos2, null, drawColor2, 0f, origin2, scale, SpriteEffects.None, 0f);

                    spriteBatch.End();
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);
                }
            }
        }

        private void DrawFieldOverlay(SpriteBatch spriteBatch, Vector2 panelPos, float scale)
        {
            // Always draw the toggle button
            DrawDebugToggleButton(spriteBatch);

            // Only draw the debug panel if visible
            if (!_debugPanelVisible)
            {
                // Still draw status line even when panel is hidden
                if (_statusTimer > 0 && !string.IsNullOrEmpty(_statusLine))
                {
                    Vector2 pos = panelPos + new Vector2(0, -95) * scale;
                    Utils.DrawBorderString(spriteBatch, _statusLine, pos, Color.Cyan * 0.9f, 0.6f * scale, 0.5f, 0.5f);
                }
                return;
            }

            // Calculate debug panel position (independent of main panel)
            float debugPanelX = panelPos.X + _debugPanelOffset.X;
            float debugPanelY = panelPos.Y + _debugPanelOffset.Y;

            // Update hit rectangle for debug panel dragging
            _debugPanelHit = new Rectangle(
                (int)debugPanelX,
                (int)debugPanelY,
                (int)DebugPanelWidth,
                (int)DebugPanelHeight);

            // Draw translucent background panel
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Color panelBgColor = new Color(0, 0, 0, 100); // Mostly translucent black
            Color panelBorderColor = new Color(80, 80, 80, 150);

            // Panel background
            spriteBatch.Draw(pixel, _debugPanelHit, panelBgColor);

            // Panel border
            spriteBatch.Draw(pixel, new Rectangle(_debugPanelHit.X, _debugPanelHit.Y, _debugPanelHit.Width, 2), panelBorderColor);
            spriteBatch.Draw(pixel, new Rectangle(_debugPanelHit.X, _debugPanelHit.Bottom - 2, _debugPanelHit.Width, 2), panelBorderColor);
            spriteBatch.Draw(pixel, new Rectangle(_debugPanelHit.X, _debugPanelHit.Y, 2, _debugPanelHit.Height), panelBorderColor);
            spriteBatch.Draw(pixel, new Rectangle(_debugPanelHit.Right - 2, _debugPanelHit.Y, 2, _debugPanelHit.Height), panelBorderColor);

            // Draw panel title/drag hint
            Color titleColor = _isDraggingDebugPanel ? Color.Yellow : Color.Gray;
            Utils.DrawBorderString(spriteBatch, "[ Node Fields - Drag to Move ]",
                new Vector2(_debugPanelHit.Center.X, _debugPanelHit.Y + 10),
                titleColor, 0.45f, 0.5f, 0f);

            // Position text inside the panel with padding
            const float paddingX = 8f;
            const float paddingY = 24f; // Extra top padding for title
            Vector2 textStart = new Vector2(debugPanelX + paddingX, debugPanelY + paddingY);

            float lineHeight = 11f;
            float txtScale = 0.5f;

            string dialoguePreview = (_selectedFieldIndex == 1 || (_textSectionPanel?.Sections.Count ?? 0) > 0)
                ? BuildDialogueFromSectionsOrRaw()
                : _dialogueText;

            string[] values = new[]
            {
                _nodeId,
                dialoguePreview,
                _faceSetName,
                _sequenceToken,
                _exitTargets,
                _enableExit.ToString(),
                _autoAdvanceFrames,
                _autoAdvanceTargets,
                _b1Label,
                _btn1Targets,
                _enableBtn1.ToString(),
                $"" ,
                _b2Label,
                _btn2Targets,
                _enableBtn2.ToString(),
                $"" ,
                _b3Label,
                _btn3Targets,
                _enableBtn3.ToString(),
                _enableBack.ToString(),
                _defaultSpeed,
                _defaultColor,
                _animateMouth,
                _priorityMode,
                _cutscenePriority
            };

            for (int i = 0; i < _fieldNames.Length; i++)
            {
                Color nameColor = i == _selectedFieldIndex ? Color.Yellow : Color.LightGray;
                string line = $"{_fieldNames[i]}: {values[i]}";

                if (i == 1 && line.Length > 50)
                    line = line.Substring(0, 50) + "...";
                else if (line.Length > 55)
                    line = line.Substring(0, 55) + "...";

                // Truncate long lines with ellipsis
                if (line.Length > 60)
                    line = line.Substring(0, 60) + "...";

                Utils.DrawBorderString(spriteBatch, line, textStart + new Vector2(0, i * lineHeight), nameColor, txtScale, 0f, 0f);
            }

            // Status line stays relative to main panel
            if (_statusTimer > 0 && !string.IsNullOrEmpty(_statusLine))
            {
                Vector2 pos = panelPos + new Vector2(0, -95) * scale;
                Utils.DrawBorderString(spriteBatch, _statusLine, pos, Color.Cyan * 0.9f, 0.6f * scale, 0.5f, 0.5f);
            }
        }

        private void DrawDebugToggleButton(SpriteBatch spriteBatch)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;

            // Button colors - blue theme
            Color bgColor = _debugPanelVisible ? new Color(40, 80, 140) : new Color(30, 60, 120);
            Color borderColor = new Color(60, 120, 180);

            // Highlight on hover
            if (_hoveredButton == 300)
            {
                bgColor = _debugPanelVisible ? new Color(60, 100, 160) : new Color(50, 80, 150);
                borderColor = new Color(100, 160, 220);
            }

            // Draw button background
            spriteBatch.Draw(pixel, _debugToggleButtonHit, bgColor);

            // Draw border
            spriteBatch.Draw(pixel, new Rectangle(_debugToggleButtonHit.X, _debugToggleButtonHit.Y, _debugToggleButtonHit.Width, 2), borderColor);
            spriteBatch.Draw(pixel, new Rectangle(_debugToggleButtonHit.X, _debugToggleButtonHit.Bottom - 2, _debugToggleButtonHit.Width, 2), borderColor);
            spriteBatch.Draw(pixel, new Rectangle(_debugToggleButtonHit.X, _debugToggleButtonHit.Y, 2, _debugToggleButtonHit.Height), borderColor);
            spriteBatch.Draw(pixel, new Rectangle(_debugToggleButtonHit.Right - 2, _debugToggleButtonHit.Y, 2, _debugToggleButtonHit.Height), borderColor);

            // Draw arrow indicator (< when visible, > when hidden)
            string arrow = _debugPanelVisible ? ">" : "<";
            Vector2 arrowPos = new Vector2(_debugToggleButtonHit.Center.X, _debugToggleButtonHit.Center.Y);
            Utils.DrawBorderString(spriteBatch, arrow, arrowPos, Color.White, 0.8f, 0.5f, 0.5f);

            // Draw label vertically
            string label = "DBG";
            float labelY = _debugToggleButtonHit.Y + 8;
            for (int i = 0; i < label.Length; i++)
            {
                Utils.DrawBorderString(spriteBatch, label[i].ToString(),
                    new Vector2(_debugToggleButtonHit.Center.X, labelY + i * 12),
                    Color.LightBlue, 0.5f, 0.5f, 0f);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!_isActive)
                return;

            // Tick alpha pulse counters once per frame (frame-guarded, safe to call from anywhere)
            SariaDrawingExtensions.UpdateAlphaCounters();

            // Switch to PointClamp for crisp pixel art rendering (all dialogue UI textures)
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);

            float scale = GetUIScale();
            Vector2 screenCenter = new Vector2(Main.screenWidth, Main.screenHeight) / 2f;
            Vector2 panelPos = screenCenter + _currentPanelOffset * scale;
            Vector2 adjustedPanelPos = panelPos + DialogueUIState.BackgroundOffset * scale;

            if (_isPreviewPlaying)
            {
                DrawPreview(spriteBatch, adjustedPanelPos, scale);

                // Restore default sampler state before returning to framework
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);
                base.Draw(spriteBatch);
                return;
            }

            // Layer 1: Background (SariaPanel)
            DrawBackground(spriteBatch, adjustedPanelPos, scale);

            // Layer 2: Greeting portrait
            DrawEditorGreetingPortrait(spriteBatch, adjustedPanelPos, scale);

            // Layer 3: Portrait (eyes, mouth, extra, sparks)
            DrawEditorPortrait(spriteBatch, adjustedPanelPos, scale);

            // Layer 4: Greeting overhead overlays (transform-specific effects)
            DrawEditorGreetingOverHead(spriteBatch, adjustedPanelPos, scale);

            // Layer 5: Editor overlays
            DrawTargetBoxes(spriteBatch, adjustedPanelPos, scale);
            DrawFieldOverlay(spriteBatch, adjustedPanelPos, scale);

            // Layer 5.5: Inline section text editor (renders in dialogue text area)
            _textSectionPanel?.Draw(spriteBatch, scale);

            // Layer 6: Dialogue buttons
            DrawButtons(spriteBatch, adjustedPanelPos, scale);

            // Layer 7: Face arrows
            DrawFaceArrows(spriteBatch, adjustedPanelPos, scale);

            // Layer 7.5: Transform arrows (above face arrows)
            DrawTransformArrows(spriteBatch, adjustedPanelPos, scale);

            // Layer 8: Preview toggle button
            DrawPreviewToggleButton(spriteBatch, adjustedPanelPos, scale);

            // Layer 9: Button editor panel
            _buttonEditorPanel?.Draw(spriteBatch, scale);

            // Layer 10: Bottom-docked buttons
            _bottomButtonPanel?.Draw(spriteBatch);

            // Restore default sampler state before returning to framework
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);
            base.Draw(spriteBatch);
        }

        private void DrawPreview(SpriteBatch spriteBatch, Vector2 panelPos, float scale)
        {
            DrawBackground(spriteBatch, panelPos, scale);
            DrawEditorGreetingPortrait(spriteBatch, panelPos, scale);
            DrawPreviewPortrait(spriteBatch, panelPos, scale);
            DrawEditorGreetingOverHead(spriteBatch, panelPos, scale);
            DrawPreviewDialogueText(spriteBatch, panelPos, scale);
            DrawPreviewButtons(spriteBatch, panelPos, scale);
            DrawPreviewToggleButton(spriteBatch, panelPos, scale);
        }

        private void DrawPreviewButtons(SpriteBatch spriteBatch, Vector2 panelPos, float scale)
        {
            DrawPreviewButton(spriteBatch, "SariaMod/Items/zTalking/ExitChoiceUI",
                panelPos + DialogueUIState.ExitButtonOffset * scale, "", _enableExit, 3, scale);

            DrawPreviewButton(spriteBatch, "SariaMod/Items/zTalking/BackChoiceUI",
                panelPos + DialogueUIState.BackButtonOffset * scale, "", _enableBack, 3, scale);

            DrawPreviewButton(spriteBatch, "SariaMod/Items/zTalking/SmallChoiceUI",
                panelPos + DialogueUIState.Button1Offset * scale, _b1Label ?? "", _enableBtn1, 3, scale);

            DrawPreviewButton(spriteBatch, "SariaMod/Items/zTalking/SmallChoiceUI",
                panelPos + DialogueUIState.Button2Offset * scale, _b2Label ?? "", _enableBtn2, 3, scale);

            DrawPreviewButton(spriteBatch, "SariaMod/Items/zTalking/SmallChoiceUI",
                panelPos + DialogueUIState.Button3Offset * scale, _b3Label ?? "", _enableBtn3, 3, scale);
        }

        private void DrawPreviewButton(SpriteBatch spriteBatch, string texturePath, Vector2 position, String label, bool isEnabled, int numFrames, float scale)
        {
            Texture2D texture = ModContent.Request<Texture2D>(texturePath).Value;
            if (texture == null) return;

            int frameHeight = texture.Height / numFrames;
            int frameIndex = isEnabled ? 0 : 2;

            Rectangle sourceRect = new(0, frameIndex * frameHeight, texture.Width, frameHeight);
            Vector2 origin = new(sourceRect.Width, sourceRect.Height);
            origin /= 2f;
            Color buttonColor = isEnabled ? Color.White : Color.Gray * 0.6f;

            spriteBatch.Draw(texture, position, sourceRect, buttonColor, 0f, origin, scale, SpriteEffects.None, 0f);

            if (!string.IsNullOrEmpty(label))
            {
                Color labelColor = isEnabled ? Color.White : Color.Gray * 0.6f;
                Utils.DrawBorderString(spriteBatch, label, position, labelColor, 0.65f * scale, 0.5f, 0.5f);
            }
        }

        private void DrawButtons(SpriteBatch spriteBatch, Vector2 panelPos, float scale)
        {
            DrawButton(spriteBatch, "SariaMod/Items/zTalking/ExitChoiceUI", panelPos + DialogueUIState.ExitButtonOffset * scale, "", _hoveredButton == 4, _enableExit, 3, scale);
            DrawButton(spriteBatch, "SariaMod/Items/zTalking/BackChoiceUI", panelPos + DialogueUIState.BackButtonOffset * scale, "", _hoveredButton == 3, _enableBack, 3, scale);

            DrawButton(spriteBatch, "SariaMod/Items/zTalking/SmallChoiceUI", panelPos + DialogueUIState.Button1Offset * scale, _b1Label ?? "", _hoveredButton == 0, _enableBtn1, 3, scale);
            DrawButton(spriteBatch, "SariaMod/Items/zTalking/SmallChoiceUI", panelPos + DialogueUIState.Button2Offset * scale, _b2Label ?? "", _hoveredButton == 1, _enableBtn2, 3, scale);
            DrawButton(spriteBatch, "SariaMod/Items/zTalking/SmallChoiceUI", panelPos + DialogueUIState.Button3Offset * scale, _b3Label ?? "", _hoveredButton == 2, _enableBtn3, 3, scale);
        }

        private void DrawButton(SpriteBatch spriteBatch, string texturePath, Vector2 position, string label, bool isHovered, bool isEnabled, int numFrames, float scale)
        {
            Texture2D texture = ModContent.Request<Texture2D>(texturePath).Value;
            int frameHeight = texture.Height / numFrames;
            int frameIndex = !isEnabled ? 2 : isHovered ? 1 : 0;

            Rectangle sourceRect = new(0, frameIndex * frameHeight, texture.Width, frameHeight);
            Vector2 origin = new(sourceRect.Width, sourceRect.Height);
            origin /= 2f;
            Color buttonColor = isEnabled ? Color.White : Color.Gray * 0.6f;

            spriteBatch.Draw(texture, position, sourceRect, buttonColor, 0f, origin, scale, SpriteEffects.None, 0f);

            if (!string.IsNullOrEmpty(label))
                Utils.DrawBorderString(spriteBatch, label, position, Color.White, 0.65f * scale, 0.5f, 0.5f);
        }

        private void DrawFaceArrows(SpriteBatch spriteBatch, Vector2 panelPos, float scale)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Color c = Color.LimeGreen * 0.9f;

            Vector2 prev = panelPos + FacePrevOffset * scale;
            Vector2 next = panelPos + FaceNextOffset * scale;

            DrawTriangle(spriteBatch, pixel, prev, 8 * scale, c, left: true);
            DrawTriangle(spriteBatch, pixel, next, 8 * scale, c, left: false);
        }

        private void DrawTransformArrows(SpriteBatch spriteBatch, Vector2 panelPos, float scale)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Color c = new Color(160, 80, 220) * 0.9f;

            Vector2 prev = panelPos + TransformPrevOffset * scale;
            Vector2 next = panelPos + TransformNextOffset * scale;

            DrawSpiral(spriteBatch, pixel, prev, 8 * scale, c, left: true, _hoveredButton == 104);
            DrawSpiral(spriteBatch, pixel, next, 8 * scale, c, left: false, _hoveredButton == 105);

            // Draw form label between the two arrows
            Vector2 labelPos = panelPos + new Vector2((TransformPrevOffset.X + TransformNextOffset.X) / 2f, TransformPrevOffset.Y) * scale;
            Utils.DrawBorderString(spriteBatch, $"Form {_transformPreviewIndex + 1}", labelPos, new Color(200, 140, 255), 0.5f * scale, 0.5f, 0.5f);
        }

        private static void DrawTriangle(SpriteBatch sb, Texture2D pixel, Vector2 center, float size, Color color, bool left)
        {
            float dir = left ? -1f : 1f;
            sb.Draw(pixel, new Rectangle((int)(center.X + dir * (-size * 0.5f)), (int)(center.Y - size * 0.5f), (int)(size * 0.75f), (int)(size * 0.2f)), color);
            sb.Draw(pixel, new Rectangle((int)(center.X + dir * (-size * 0.25f)), (int)(center.Y - size * 0.15f), (int)(size * 0.9f), (int)(size * 0.2f)), color);
            sb.Draw(pixel, new Rectangle((int)(center.X + dir * (0)), (int)(center.Y + size * 0.2f), (int)(size * 0.75f), (int)(size * 0.2f)), color);
        }

        private static void DrawSpiral(SpriteBatch sb, Texture2D pixel, Vector2 center, float size, Color color, bool left, bool hovered)
        {
            Color drawColor = hovered ? Color.Lerp(color, Color.White, 0.35f) : color;
            float dir = left ? -1f : 1f;
            float s = size;

            // Outer arm
            sb.Draw(pixel, new Rectangle((int)(center.X + dir * (-s * 0.5f)), (int)(center.Y - s * 0.55f), (int)(s * 0.85f), (int)Math.Max(1, s * 0.15f)), drawColor);
            // Curl down
            sb.Draw(pixel, new Rectangle((int)(center.X + dir * (s * 0.2f)), (int)(center.Y - s * 0.55f), (int)Math.Max(1, s * 0.15f), (int)(s * 0.5f)), drawColor);
            // Inner arm
            sb.Draw(pixel, new Rectangle((int)(center.X + dir * (-s * 0.25f)), (int)(center.Y - s * 0.1f), (int)(s * 0.55f), (int)Math.Max(1, s * 0.15f)), drawColor);
            // Curl up to center
            sb.Draw(pixel, new Rectangle((int)(center.X + dir * (-s * 0.25f)), (int)(center.Y - s * 0.1f), (int)Math.Max(1, s * 0.15f), (int)(s * 0.45f)), drawColor);
            // Center dot
            sb.Draw(pixel, new Rectangle((int)(center.X + dir * (-s * 0.15f)), (int)(center.Y + s * 0.2f), (int)Math.Max(1, s * 0.2f), (int)Math.Max(1, s * 0.15f)), drawColor);
        }

        private static void DrawBox(SpriteBatch sb, Texture2D pixel, Rectangle r, Color border)
        {
            sb.Draw(pixel, r, Color.Black * 0.55f);
            sb.Draw(pixel, new Rectangle(r.X, r.Y, r.Width, 1), border * 0.9f);
            sb.Draw(pixel, new Rectangle(r.X, r.Bottom - 1, r.Width, 1), border * 0.9f);
            sb.Draw(pixel, new Rectangle(r.X, r.Y, 1, r.Height), border * 0.9f);
            sb.Draw(pixel, new Rectangle(r.Right - 1, r.Y, 1, r.Height), border * 0.9f);
        }

        private void DrawTargetBoxes(SpriteBatch spriteBatch, Vector2 panelPos, float scale)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;

            Rectangle exitBox = new Rectangle((int)(panelPos.X + DialogueUIState.ExitButtonOffset.X * scale - 49 * scale), (int)(panelPos.Y + (DialogueUIState.ExitButtonOffset.Y + 30) * scale), (int)(98 * scale), (int)(14 * scale));
            Rectangle backBox = new Rectangle((int)(panelPos.X + DialogueUIState.BackButtonOffset.X * scale - 49 * scale), (int)(panelPos.Y + (DialogueUIState.BackButtonOffset.Y + 30) * scale), (int)(98 * scale), (int)(14 * scale));
            Rectangle b1Box = new Rectangle((int)(panelPos.X + DialogueUIState.Button1Offset.X * scale - 49 * scale), (int)(panelPos.Y + (DialogueUIState.Button1Offset.Y + 30) * scale), (int)(98 * scale), (int)(14 * scale));
            Rectangle b2Box = new Rectangle((int)(panelPos.X + DialogueUIState.Button2Offset.X * scale - 49 * scale), (int)(panelPos.Y + (DialogueUIState.Button2Offset.Y + 30) * scale), (int)(98 * scale), (int)(14 * scale));
            Rectangle b3Box = new Rectangle((int)(panelPos.X + DialogueUIState.Button3Offset.X * scale - 49 * scale), (int)(panelPos.Y + (DialogueUIState.Button3Offset.Y + 30) * scale), (int)(98 * scale), (int)(14 * scale));

            Rectangle autoBox = new Rectangle(
                (int)(panelPos.X + AutoAdvanceBoxOffset.X * scale - (AutoAdvanceBoxSize.X * scale) / 2f),
                (int)(panelPos.Y + AutoAdvanceBoxOffset.Y * scale - (AutoAdvanceBoxSize.Y * scale) / 2f),
                (int)(AutoAdvanceBoxSize.X * scale),
                (int)(AutoAdvanceBoxSize.Y * scale));

            Rectangle nodeFinder = new Rectangle(
                (int)(panelPos.X + NodeFinderBoxOffset.X * scale - (NodeFinderBoxSize.X * scale) / 2f),
                (int)(panelPos.Y + NodeFinderBoxOffset.Y * scale - (NodeFinderBoxSize.Y * scale) / 2f),
                (int)(NodeFinderBoxSize.X * scale),
                (int)(NodeFinderBoxSize.Y * scale));

            _exitBoxHit = exitBox;
            _backBoxHit = backBox;
            _b1BoxHit = b1Box;
            _b2BoxHit = b2Box;
            _b3BoxHit = b3Box;
            _autoBoxHit = autoBox;
            _nodeFinderHit = nodeFinder;

            DrawBox(spriteBatch, pixel, exitBox, Color.White);
            DrawBox(spriteBatch, pixel, backBox, Color.White);
            DrawBox(spriteBatch, pixel, b1Box, Color.White);
            DrawBox(spriteBatch, pixel, b2Box, Color.White);
            DrawBox(spriteBatch, pixel, b3Box, Color.White);
            DrawBox(spriteBatch, pixel, autoBox, Color.White);

            DrawBox(spriteBatch, pixel, nodeFinder, _editingNodeFinder ? Color.Yellow : (_hoveredButton == 206 ? Color.Cyan : Color.White));
            Utils.DrawBorderString(spriteBatch, "Find: " + (_nodeFinderId ?? "") + "  (Enter=load)", new Vector2(nodeFinder.X + 3, nodeFinder.Center.Y), Color.White, 0.45f * scale, 0f, 0.5f);

            string exitTxt = string.IsNullOrWhiteSpace(_exitTargets) ? "Exit: (default)" : _exitTargets;
            if (!_enableExit) exitTxt = "(OFF) " + exitTxt;
            if (!string.IsNullOrWhiteSpace(_sequenceToken))
                exitTxt = $"{exitTxt} | Seq: {_sequenceToken}";

            Utils.DrawBorderString(spriteBatch, exitTxt, new Vector2(exitBox.X + 3, exitBox.Center.Y), Color.White, 0.45f * scale, 0f, 0.5f);
            Utils.DrawBorderString(spriteBatch, "BACK", new Vector2(backBox.Center.X, backBox.Center.Y), _enableBack ? Color.LightGray : Color.Gray, 0.45f * scale, 0.5f, 0.5f);
            Utils.DrawBorderString(spriteBatch, (_enableBtn1 ? "" : "(OFF) ") + (_btn1Targets ?? ""), new Vector2(b1Box.X + 3, b1Box.Center.Y), Color.White, 0.45f * scale, 0f, 0.5f);
            Utils.DrawBorderString(spriteBatch, (_enableBtn2 ? "" : "(OFF) ") + (_btn2Targets ?? ""), new Vector2(b2Box.X + 3, b2Box.Center.Y), Color.White, 0.45f * scale, 0f, 0.5f);
            Utils.DrawBorderString(spriteBatch, (_enableBtn3 ? "" : "(OFF) ") + (_btn3Targets ?? ""), new Vector2(b3Box.X + 3, b3Box.Center.Y), Color.White, 0.45f * scale, 0f, 0.5f);
        }

        private void DrawPreviewPortrait(SpriteBatch spriteBatch, Vector2 panelPos, float scale)
        {
            string faceSet = string.IsNullOrWhiteSpace(_faceSetName) ? "Default" : _faceSetName.Trim();
            Vector2 eyesPos = panelPos + (_transformPreviewIndex == 6 ? DialogueUIState.Eyes7Offset : DialogueUIState.EyesOffset) * scale;
            DrawEditorEyes(spriteBatch, eyesPos, scale, faceSet, _previewEyeFrame, _previewIsCurrentlySpeaking);
            DrawEditorMouth(spriteBatch, panelPos + DialogueUIState.MouthOffset * scale, scale, faceSet, _previewMouthFrame);

            var extra = DialogueFaceSetRegistry.TryResolveExtraTexture(faceSet, _transformPreviewIndex);
            if (extra != null)
            {
                Vector2 origin = new Vector2(extra.Width, extra.Height) / 2f;
                spriteBatch.Draw(extra, panelPos, null, Color.White, 0f, origin, scale, SpriteEffects.None, 0f);
            }

            DrawEditorSparks(spriteBatch, panelPos, scale);
        }

        private void DrawEditorPortrait(SpriteBatch spriteBatch, Vector2 panelPos, float scale)
        {
            string faceSet = string.IsNullOrWhiteSpace(_faceSetName) ? "Default" : _faceSetName.Trim();
            Vector2 eyesPos = panelPos + (_transformPreviewIndex == 6 ? DialogueUIState.Eyes7Offset : DialogueUIState.EyesOffset) * scale;
            DrawEditorEyes(spriteBatch, eyesPos, scale, faceSet, 0, false);
            DrawEditorMouth(spriteBatch, panelPos + DialogueUIState.MouthOffset * scale, scale, faceSet, 0);

            var extra = DialogueFaceSetRegistry.TryResolveExtraTexture(faceSet, _transformPreviewIndex);
            if (extra != null)
            {
                Vector2 origin = new Vector2(extra.Width, extra.Height) / 2f;
                spriteBatch.Draw(extra, panelPos, null, Color.White, 0f, origin, scale, SpriteEffects.None, 0f);
            }

            DrawEditorSparks(spriteBatch, panelPos, scale);
        }

        private void DrawEditorEyes(SpriteBatch spriteBatch, Vector2 position, float scale, string faceSetName, int eyeFrame, bool isSpeaking)
        {
            Texture2D eyeTexture = DialogueFaceSetRegistry.TryResolveEyesTexture(faceSetName, _transformPreviewIndex);
            if (eyeTexture == null) return;

            int numFrames = 4;
            int frameHeight = eyeTexture.Height / numFrames;
            Rectangle sourceRect = new Rectangle(0, eyeFrame * frameHeight, eyeTexture.Width, frameHeight);
            Vector2 origin = new Vector2(sourceRect.Width, sourceRect.Height) / 2f;

            if (_transformPreviewIndex == 4)
            {
                var baseSet = DialogueFaceSetRegistry.Get(faceSetName);
                string basePath = $"SariaMod/Items/zTalking/{baseSet.EyesPrefix}1";
                if (ModContent.RequestIfExists(basePath, out ReLogic.Content.Asset<Texture2D> baseAsset))
                {
                    Texture2D baseEyes = baseAsset.Value;
                    int baseFrameH = baseEyes.Height / numFrames;
                    Rectangle baseRect = new Rectangle(0, eyeFrame * baseFrameH, baseEyes.Width, baseFrameH);
                    Vector2 baseOrigin = new Vector2(baseRect.Width, baseRect.Height) / 2f;
                    spriteBatch.Draw(baseEyes, position, baseRect, Color.White, 0f, baseOrigin, scale, SpriteEffects.None, 0f);
                }

                Color glowColor = Color.White;
                glowColor = Color.Lerp(glowColor, Color.FloralWhite, 30f);
                glowColor = Color.Lerp(glowColor, Color.Transparent, SariaDrawingExtensions.alpha3);

                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);

                spriteBatch.Draw(eyeTexture, position, sourceRect, glowColor, 0f, origin, scale, SpriteEffects.None, 0f);

                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);
            }
            else if (_transformPreviewIndex == 2)
            {
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);

                spriteBatch.Draw(eyeTexture, position, sourceRect, Color.White, 0f, origin, scale, SpriteEffects.None, 0f);

                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);
            }
            else if (_transformPreviewIndex == 6)
            {
                Rectangle singleRect = new Rectangle(0, 0, eyeTexture.Width, eyeTexture.Height);
                Vector2 singleOrigin = new Vector2(singleRect.Width, singleRect.Height) / 2f;

                // Speaking intensity: drives blend toward new effect + jitter
                float jitterTarget = isSpeaking ? 1f : 0f;
                _previewPoeWaveStrength = MathHelper.Lerp(_previewPoeWaveStrength, jitterTarget, 0.05f);

                _previewPoeWavePhase += 0.09f;
                if (_previewPoeWavePhase > MathF.PI * 20f)
                    _previewPoeWavePhase -= MathF.PI * 20f;

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
                        float baseWave = MathF.Sin(_previewPoeWavePhase + t * MathF.PI * 5f);
                        float baseAlpha = 1f - 0.45f * (baseWave * 0.5f + 0.5f);

                        // Speaking glow: broad sweeping ripples
                        float speakWave = MathF.Sin(_previewPoeWavePhase + t * MathF.PI * 2.5f);
                        float speakAlpha = 1f - 0.6f * (speakWave * 0.5f + 0.5f);

                        // Blend: idle = old glow, speaking = new glow
                        float alpha = MathHelper.Lerp(baseAlpha, speakAlpha, _previewPoeWaveStrength);

                        // Horizontal jitter: only when speaking
                        float jitterAmount = _previewPoeWaveStrength * 0.6f;
                        ulong rowSeed = jitterSeed * 31ul + (ulong)row * 7ul;
                        float jitter = ((float)(rowSeed % 1000u) / 500f - 1f) * jitterAmount * scale;

                        Rectangle rowRect = new Rectangle(0, row, texW, 1);
                        Vector2 rowPos = new Vector2(topLeftX + jitter, topLeftY + row * scale);
                        Color rowColor = Color.White * alpha;

                        spriteBatch.Draw(eyeTexture, rowPos, rowRect, rowColor, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
                    }
                }
            }
            else
            {
                spriteBatch.Draw(eyeTexture, position, sourceRect, Color.White, 0f, origin, scale, SpriteEffects.None, 0f);
            }
        }

        private void DrawEditorMouth(SpriteBatch spriteBatch, Vector2 position, float scale, string faceSetName, int mouthFrame)
        {
            if (_transformPreviewIndex == 6) return;

            Texture2D mouthTexture = DialogueFaceSetRegistry.TryResolveMouthTexture(faceSetName, _transformPreviewIndex);
            if (mouthTexture == null) return;

            int numFrames = 5;
            int frameHeight = mouthTexture.Height / numFrames;
            Rectangle sourceRect = new Rectangle(0, mouthFrame * frameHeight, mouthTexture.Width, frameHeight);
            Vector2 origin = new Vector2(sourceRect.Width, sourceRect.Height) / 2f;

            spriteBatch.Draw(mouthTexture, position, sourceRect, Color.White, 0f, origin, scale, SpriteEffects.None, 0f);
        }

        private void DrawEditorSparks(SpriteBatch spriteBatch, Vector2 panelPos, float scale)
        {
            if (_transformPreviewIndex != 3) return;

            bool sparksActive = false;
            int sariaType = ModContent.ProjectileType<Saria>();
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];
                if (proj != null && proj.active && proj.type == sariaType && proj.owner == Main.myPlayer && proj.ModProjectile is Saria sparksSaria)
                {
                    sparksActive = sparksSaria.SpecialAnimateValue > 0;
                    break;
                }
            }

            if (!sparksActive) return;

            string sparksPath = "SariaMod/Items/zTalking/SariaSparksPortrait";
            Texture2D sparksTexture;
            try { sparksTexture = ModContent.Request<Texture2D>(sparksPath).Value; }
            catch { sparksTexture = null; }

            if (sparksTexture == null) return;

            int sparksFrame = (int)Main.GameUpdateCount / 3 % 14;
            Rectangle sparksRect = sparksTexture.Frame(verticalFrames: 14, frameY: sparksFrame);
            Vector2 sparksOrigin = sparksRect.Size() / 2f;
            Vector2 sparksPos = panelPos + DialogueUIState.SparksPortraitOffset * scale;

            Color sparksColor = Color.Lerp(Color.White, Color.LightBlue, 2f);
            sparksColor *= 0.85f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);

            spriteBatch.Draw(sparksTexture, sparksPos, sparksRect, sparksColor, 0f, sparksOrigin, scale, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);
        }

        private void DrawPreviewDialogueText(SpriteBatch spriteBatch, Vector2 panelPos, float scale)
        {
            if (_previewWrappedLines == null || _previewWrappedLines.Count == 0)
                return;

            Vector2 textStart = panelPos + GetTextOffset() * scale;

            float baseTextScale = DialogueUIState.TextScale * scale;
            float maxHeightPx = GetTextMaxHeight() * scale;

            int totalLines = _previewWrappedLines.Count;
            float lineHeightAtBase = DialogueUIState.LineHeightBase * scale;
            float textScale = ComputeTextScaleToFit(baseTextScale, lineHeightAtBase, totalLines, maxHeightPx);

            float lineHeight = DialogueUIState.LineHeightBase * (textScale / baseTextScale) * scale;
            DynamicSpriteFont font = FontAssets.MouseText.Value;

            Vector2 currentPos = textStart;
            int charIndex = 0;

            foreach (var line in _previewWrappedLines)
            {
                float xOffset = 0;
                foreach (var cc in line)
                {
                    if (charIndex >= _previewColoredText.Count)
                        return;

                    string charStr = cc.Character.ToString();
                    Vector2 charSize = font.MeasureString(charStr) * textScale;

                    Utils.DrawBorderString(spriteBatch, charStr, currentPos + new Vector2(xOffset, 0), _previewColoredText[charIndex].TextColor, textScale, 0f, 0f);
                    xOffset += charSize.X;
                    charIndex++;
                }
                currentPos.Y += lineHeight;
            }
        }

        private void DrawPreviewToggleButton(SpriteBatch spriteBatch, Vector2 panelPos, float scale)
        {
            Rectangle rect = GetPreviewButtonRect(panelPos, scale);
            Texture2D pixel = TextureAssets.MagicPixel.Value;

            Color bgColor = _isPreviewPlaying ? new Color(180, 60, 60) : new Color(60, 120, 60);
            if (_hoveredPreviewButton == 0)
                bgColor = bgColor * 1.3f;

            spriteBatch.Draw(pixel, rect, bgColor);

            Color borderColor = Color.White * 0.8f;
            spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, rect.Width, 2), borderColor);
            spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Bottom - 2, rect.Width, 2), borderColor);
            spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, 2, rect.Height), borderColor);
            spriteBatch.Draw(pixel, new Rectangle(rect.Right - 2, rect.Y, 2, rect.Height), borderColor);

            string label = _isPreviewPlaying ? "Stop" : "Play";
            Vector2 textPos = new(rect.Center.X, rect.Center.Y);
            Utils.DrawBorderString(spriteBatch, label, textPos, Color.White, 0.6f * scale, 0.5f, 0.5f);
        }
    }
}
