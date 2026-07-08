using Microsoft.Xna.Framework;
using System;
using ReLogic.Graphics;
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
        private void UpdatePreviewHover()
        {
            float scale = GetUIScale();
            Vector2 screenCenter = new Vector2(Main.screenWidth, Main.screenHeight) / 2f;
            Vector2 panelPos = screenCenter + _currentPanelOffset * scale;
            Vector2 adjustedPanelPos = panelPos + DialogueUIState.BackgroundOffset * scale;

            Rectangle playRect = GetPreviewButtonRect(adjustedPanelPos, scale);
            int prev = _hoveredPreviewButton;
            _hoveredPreviewButton = -1;

            if (playRect.Contains(new Point(Main.mouseX, Main.mouseY)))
                _hoveredPreviewButton = 0;

            if (_hoveredPreviewButton != -1 && _hoveredPreviewButton != prev)
                SoundEngine.PlaySound(SoundID.MenuTick);
        }

        private void UpdatePreviewClicks()
        {
            if (_clickCooldown > 0) return;
            if (!_mouseReleasedThisFrame) return;

            if (_hoveredPreviewButton == 0)
            {
                StopPreview();
                _clickCooldown = 10;
            }
        }

        private void UpdateEditorPreviewButtonHover(Vector2 panelPos, float scale)
        {
            Rectangle playRect = GetPreviewButtonRect(panelPos, scale);
            int prev = _hoveredPreviewButton;
            _hoveredPreviewButton = -1;

            if (playRect.Contains(new Point(Main.mouseX, Main.mouseY)))
                _hoveredPreviewButton = 0;

            if (_hoveredPreviewButton != -1 && _hoveredPreviewButton != prev)
                SoundEngine.PlaySound(SoundID.MenuTick);
        }

        private void UpdateEditorPreviewButtonClick(Vector2 panelPos, float scale)
        {
            if (_clickCooldown > 0) return;
            if (!_mouseReleasedThisFrame) return;

            Rectangle playRect = GetPreviewButtonRect(panelPos, scale);
            if (!playRect.Contains(new Point(Main.mouseX, Main.mouseY)))
                return;

            StartPreview();
            _clickCooldown = 10;
        }

        private void StartPreview()
        {
            _isPreviewPlaying = true;

            ResetPreviewTypewriter();
            _previewFullText = BuildDialogueFromSectionsOrRaw();
            PreprocessPreviewText();
        }

        private void StopPreview()
        {
            _isPreviewPlaying = false;
            _hoveredPreviewButton = -1;
            ResetPreviewTypewriter();
        }

        private void ResetPreviewTypewriter()
        {
            _previewColoredText.Clear();
            _previewWrappedLines.Clear();
            _previewFullText = "";
            _previewCharIndex = 0;
            _previewFrameCounter = 0;
            _previewCurrentSpeed = 2;
            _previewWaitFrames = 0;
            _previewTextComplete = false;
            _previewCurrentLineIndex = 0;
            _previewCurrentCharInLine = 0;
            _previewIsCurrentlySpeaking = false;
            _previewMouthFrame = 0;
            _previewMouthAnimTimer = 0;
            _previewEyeFrame = 0;
            _previewEyeAnimTimer = 0;
            _previewIsBlinking = false;
            _previewBlinkFrameIndex = 0;
            _previewNextBlinkTime = Main.rand.Next(120, 300);
            _previewSoundCooldown = 0;
        }

        private void PreprocessPreviewText()
        {
            _previewWrappedLines.Clear();
            if (string.IsNullOrEmpty(_previewFullText))
                return;

            float scale = GetUIScale() * DialogueUIState.TextScale;
            float maxWidth = GetTextMaxWidth() * GetUIScale();
            DynamicSpriteFont font = FontAssets.MouseText.Value;

            List<DialogueUIChar> allChars = new();
            Color currentColor = Color.White;
            bool currentSilent = false;
            bool currentMouthEnabled = true;
            int i = 0;

            while (i < _previewFullText.Length)
            {
                char c = _previewFullText[i];
                if (c == '[')
                {
                    int tagEnd = _previewFullText.IndexOf(']', i);
                    if (tagEnd > i)
                    {
                        string tag = _previewFullText.Substring(i + 1, tagEnd - i - 1);
                        i = tagEnd + 1;

                        if (tag.StartsWith("color:", StringComparison.OrdinalIgnoreCase))
                        {
                            string colorName = tag.Substring(6);
                            if (DialogueUIState.NamedColors.TryGetValue(colorName, out Color newColor))
                                currentColor = newColor;
                        }
                        else if (tag.Equals("silent", StringComparison.OrdinalIgnoreCase))
                        {
                            currentSilent = true;
                        }
                        else if (tag.Equals("/silent", StringComparison.OrdinalIgnoreCase))
                        {
                            currentSilent = false;
                        }
                        else if (tag.Equals("mouth", StringComparison.OrdinalIgnoreCase))
                        {
                            currentMouthEnabled = true;
                        }
                        else if (tag.Equals("/mouth", StringComparison.OrdinalIgnoreCase))
                        {
                            currentMouthEnabled = false;
                        }
                        continue;
                    }
                }

                allChars.Add(new DialogueUIChar(c, currentColor, currentSilent, currentMouthEnabled));
                i++;
            }

            List<DialogueUIChar> currentLine = new();
            List<DialogueUIChar> currentWord = new();
            float currentLineWidth = 0;

            float measureChar(char ch) => font.MeasureString(ch.ToString()).X * scale;

            float measureWord(List<DialogueUIChar> word)
            {
                float width = 0;
                foreach (var cc in word)
                    width += measureChar(cc.Character);
                return width;
            }

            foreach (var cc in allChars)
            {
                if (cc.Character == ' ')
                {
                    float wordWidth = measureWord(currentWord);
                    if (currentLineWidth + wordWidth > maxWidth && currentLine.Count > 0)
                    {
                        _previewWrappedLines.Add(new List<DialogueUIChar>(currentLine));
                        currentLine.Clear();
                        currentLineWidth = 0;
                    }

                    currentLine.AddRange(currentWord);
                    currentLineWidth += wordWidth;
                    currentLine.Add(cc);
                    currentLineWidth += measureChar(' ');
                    currentWord.Clear();
                }
                else if (cc.Character == '\n')
                {
                    currentLine.AddRange(currentWord);
                    _previewWrappedLines.Add(new List<DialogueUIChar>(currentLine));
                    currentLine.Clear();
                    currentWord.Clear();
                    currentLineWidth = 0;
                }
                else
                {
                    currentWord.Add(cc);
                }
            }

            if (currentWord.Count > 0)
            {
                float wordWidth = measureWord(currentWord);
                if (currentLineWidth + wordWidth > maxWidth && currentLine.Count > 0)
                {
                    _previewWrappedLines.Add(new List<DialogueUIChar>(currentLine));
                    currentLine.Clear();
                }
                currentLine.AddRange(currentWord);
            }

            if (currentLine.Count > 0)
                _previewWrappedLines.Add(currentLine);
        }

        private void UpdatePreviewTypewriter()
        {
            if (_previewTextComplete || _previewWrappedLines.Count == 0)
                return;

            // Decrement sound cooldown
            if (_previewSoundCooldown > 0)
                _previewSoundCooldown--;

            if (_previewWaitFrames > 0)
            {
                _previewWaitFrames--;
                _previewIsCurrentlySpeaking = false;
                return;
            }

            _previewFrameCounter++;
            if (_previewFrameCounter < Math.Max(1, _previewCurrentSpeed))
                return;

            _previewFrameCounter = 0;

            if (_previewCurrentLineIndex < _previewWrappedLines.Count)
            {
                var line = _previewWrappedLines[_previewCurrentLineIndex];
                if (_previewCurrentCharInLine < line.Count)
                {
                    var cc = line[_previewCurrentCharInLine];
                    _previewColoredText.Add(cc);
                    _previewCurrentCharInLine++;

                    bool shouldSpeak = !cc.IsSilent && cc.MouthEnabled && cc.Character != ' ';
                    _previewIsCurrentlySpeaking = shouldSpeak;

                    // Play typewriter sound with smooth overlap and weighted pitch variance
                    if (shouldSpeak && _previewSoundCooldown <= 0)
                    {
                        // Weighted randomization: 80% default pitch, 20% lower pitch
                        float pitchVariance = 0f;
                        float volumeLevel = 1f;
                        int roll = Main.rand.Next(100);
                        if (roll >= 80)
                        {
                            // Lower pitch: random between -0.05f and -0.15f
                            pitchVariance = -0.05f - (Main.rand.NextFloat() * 0.10f);
                            // Reduce volume slightly for lowered pitch (more natural feel)
                            volumeLevel = 0.9f;
                        }

                        // MaxInstances = 3 allows sound tails to overlap for smoother transitions
                        // SoundLimitBehavior.ReplaceOldest ensures older sounds fade out gracefully
                        SoundStyle talkingSound = new SoundStyle("SariaMod/Sounds/SariaTalking")
                        {
                            Pitch = pitchVariance,
                            Volume = volumeLevel,
                            MaxInstances = 3,
                            SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest
                        };
                        SoundEngine.PlaySound(talkingSound, Main.LocalPlayer.Center);
                        _previewSoundCooldown = 3;
                    }
                }
                else
                {
                    _previewCurrentLineIndex++;
                    _previewCurrentCharInLine = 0;
                }
            }

            // Parse tags to control wait/speed during preview
            while (_previewCharIndex < _previewFullText.Length)
            {
                if (_previewFullText[_previewCharIndex] != '[') { _previewCharIndex++; break; }

                int tagEnd = _previewFullText.IndexOf(']', _previewCharIndex);
                if (tagEnd <= _previewCharIndex) break;

                string tag = _previewFullText.Substring(_previewCharIndex + 1, tagEnd - _previewCharIndex - 1);
                _previewCharIndex = tagEnd + 1;

                if (tag.StartsWith("wait:", StringComparison.OrdinalIgnoreCase))
                {
                    if (int.TryParse(tag.Substring(5), out int waitTime))
                        _previewWaitFrames = waitTime;
                }
                else if (tag.StartsWith("speed:", StringComparison.OrdinalIgnoreCase))
                {
                    if (int.TryParse(tag.Substring(6), out int newSpeed))
                        _previewCurrentSpeed = Math.Max(1, newSpeed);
                }
            }

            int totalChars = 0;
            foreach (var l in _previewWrappedLines) totalChars += l.Count;

            if (_previewColoredText.Count >= totalChars)
            {
                _previewTextComplete = true;
                _previewIsCurrentlySpeaking = false;
            }
        }

        private void UpdatePreviewEyeAnimation()
        {
            _previewEyeAnimTimer++;

            if (!_previewIsBlinking)
            {
                if (_previewEyeAnimTimer >= _previewNextBlinkTime)
                {
                    _previewIsBlinking = true;
                    _previewBlinkFrameIndex = 0;
                    _previewEyeAnimTimer = 0;
                }
            }
            else
            {
                if (_previewEyeAnimTimer >= 4)
                {
                    _previewEyeAnimTimer = 0;
                    _previewBlinkFrameIndex++;
                    if (_previewBlinkFrameIndex >= 4)
                    {
                        _previewIsBlinking = false;
                        _previewEyeFrame = 0;
                        _previewNextBlinkTime = Main.rand.Next(120, 300);
                    }
                    else
                    {
                        _previewEyeFrame = _previewBlinkFrameIndex;
                    }
                }
            }
        }

        private void UpdatePreviewMouthAnimation()
        {
            bool animateMouth = !string.Equals(_animateMouth, "false", StringComparison.OrdinalIgnoreCase);
            if (!animateMouth)
            {
                _previewMouthFrame = 0;
                _previewMouthAnimTimer = 0;
                return;
            }

            int baseSpeed = 6;
            int mouthSpeed = baseSpeed + Math.Max(0, (_previewCurrentSpeed - 2) * 2);

            bool shouldAnimate = !_previewTextComplete && _previewWaitFrames <= 0 && _previewIsCurrentlySpeaking;
            if (shouldAnimate)
            {
                _previewMouthAnimTimer++;
                if (_previewMouthAnimTimer >= mouthSpeed)
                {
                    _previewMouthAnimTimer = 0;
                    _previewMouthFrame = (_previewMouthFrame + 1) % 5;
                }
            }
            else
            {
                _previewMouthFrame = 0;
                _previewMouthAnimTimer = 0;
            }
        }
    }
}
