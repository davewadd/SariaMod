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
        // // Typewriter
        private string _fullText = "";
        private int _charIndex = 0;
        private int _frameCounter = 0;
        private int _currentSpeed = 2;
        private int _waitFrames = 0;
        private bool _isTextComplete = false;
        private List<ColoredChar> _coloredText = new List<ColoredChar>();
        private List<List<ColoredChar>> _wrappedLines = new List<List<ColoredChar>>();
        private int _currentLineIndex = 0;
        private int _currentCharInLine = 0;

        // // MOUTH ANIMATION
        private bool _isSpeakingThisFrame = false;
        private bool _isSilentMode = false;
        private int _baseMouthSpeed = 6;
        private int _currentMouthSpeed = 6;
        private bool _isCurrentlySpeaking = false; // Tracks if we're in an active speaking section

        private struct ColoredChar
        {
            public char Character;
            public Color TextColor;
            public bool IsSilent;
            public bool MouthEnabled;
            public ColoredChar(char c, Color color, bool silent = false, bool mouthEnabled = true)
            {
                Character = c;
                TextColor = color;
                IsSilent = silent;
                MouthEnabled = mouthEnabled;
            }
        }

        // ============================================================
        // WORD WRAP
        // ============================================================
        private void PreprocessText()
        {
            _wrappedLines.Clear();
            if (string.IsNullOrEmpty(_fullText)) return;

            float scale = GetUIScale() * TextScale;
            float maxWidth = GetTextMaxWidth() * GetUIScale();
            DynamicSpriteFont font = FontAssets.MouseText.Value;

            List<ColoredChar> allChars = new List<ColoredChar>();
            Color currentColor = Color.White;
            bool currentSilent = false;
            bool currentMouthEnabled = true;
            int i = 0;

            while (i < _fullText.Length)
            {
                char c = _fullText[i];
                if (c == '[')
                {
                    int tagEnd = _fullText.IndexOf(']', i);
                    if (tagEnd > i)
                    {
                        string tag = _fullText.Substring(i + 1, tagEnd - i - 1);
                        i = tagEnd + 1;

                        if (tag.StartsWith("color:", StringComparison.OrdinalIgnoreCase))
                        {
                            string colorName = tag.Substring(6);
                            if (NamedColors.TryGetValue(colorName, out Color newColor))
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

                allChars.Add(new ColoredChar(c, currentColor, currentSilent, currentMouthEnabled));
                i++;
            }

            List<ColoredChar> currentLine = new List<ColoredChar>();
            List<ColoredChar> currentWord = new List<ColoredChar>();
            float currentLineWidth = 0;

            foreach (var cc in allChars)
            {
                if (cc.Character == ' ')
                {
                    float wordWidth = MeasureWord(font, currentWord, scale);
                    if (currentLineWidth + wordWidth > maxWidth && currentLine.Count > 0)
                    {
                        _wrappedLines.Add(new List<ColoredChar>(currentLine));
                        currentLine.Clear();
                        currentLineWidth = 0;
                    }
                    currentLine.AddRange(currentWord);
                    currentLineWidth += wordWidth;
                    currentLine.Add(cc);
                    currentLineWidth += font.MeasureString(" ").X * scale;
                    currentWord.Clear();
                }
                else if (cc.Character == '\n')
                {
                    currentLine.AddRange(currentWord);
                    _wrappedLines.Add(new List<ColoredChar>(currentLine));
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
                float wordWidth = MeasureWord(font, currentWord, scale);
                if (currentLineWidth + wordWidth > maxWidth && currentLine.Count > 0)
                {
                    _wrappedLines.Add(new List<ColoredChar>(currentLine));
                    currentLine.Clear();
                }
                currentLine.AddRange(currentWord);
            }
            if (currentLine.Count > 0)
                _wrappedLines.Add(currentLine);
        }

        private float MeasureWord(DynamicSpriteFont font, List<ColoredChar> word, float scale)
        {
            float width = 0;
            foreach (var cc in word)
                width += font.MeasureString(cc.Character.ToString()).X * scale;
            return width;
        }

        // ============================================================
        // TYPEWRITER
        // ============================================================
        private void ResetTypewriter()
        {
            _coloredText.Clear();
            _wrappedLines.Clear();
            _charIndex = 0;
            _frameCounter = 0;
            _currentSpeed = 2;
            _waitFrames = 0;
            _isTextComplete = false;
            _currentLineIndex = 0;
            _currentCharInLine = 0;
            _isSilentMode = false;
            _isSpeakingThisFrame = false;
            _isCurrentlySpeaking = false;
            _mouthFrame = 0;
            _mouthAnimTimer = 0;
            _currentMouthSpeed = _baseMouthSpeed;
        }

        private void UpdateTypewriter()
        {
            if (Main.gamePaused) return;
            if (_isTextComplete || _wrappedLines.Count == 0) return;

            if (_waitFrames > 0)
            {
                _waitFrames--;
                _isSpeakingThisFrame = false;
                _isCurrentlySpeaking = false;
                return;
            }

            _frameCounter++;
            if (_frameCounter < _currentSpeed)
            {
                return;
            }
            _frameCounter = 0;

            if (_currentLineIndex < _wrappedLines.Count)
            {
                var line = _wrappedLines[_currentLineIndex];
                if (_currentCharInLine < line.Count)
                {
                    var cc = line[_currentCharInLine];
                    _coloredText.Add(cc);
                    _currentCharInLine++;

                    bool shouldSpeak = !cc.IsSilent && cc.MouthEnabled && cc.Character != ' ';

                    if (shouldSpeak)
                    {
                        _isSpeakingThisFrame = true;
                        _isCurrentlySpeaking = true;

                        // Play typewriter sound with smooth overlap and weighted pitch variance
                        if (_soundCooldown <= 0)
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
                            _soundCooldown = 3;
                        }
                    }
                    else
                    {
                        _isSpeakingThisFrame = false;
                        if (cc.IsSilent || !cc.MouthEnabled)
                            _isCurrentlySpeaking = false;
                    }
                }
                else
                {
                    _currentLineIndex++;
                    _currentCharInLine = 0;
                }
            }

            CheckForTagsAtPosition();

            _currentMouthSpeed = _baseMouthSpeed + Math.Max(0, (_currentSpeed - 2) * 2);

            int totalChars = 0;
            foreach (var line in _wrappedLines) totalChars += line.Count;
            if (_coloredText.Count >= totalChars)
            {
                _isTextComplete = true;
                _isCurrentlySpeaking = false;
            }
        }

        private void CheckForTagsAtPosition()
        {
            while (_charIndex < _fullText.Length)
            {
                if (_fullText[_charIndex] != '[') { _charIndex++; break; }

                int tagEnd = _fullText.IndexOf(']', _charIndex);
                if (tagEnd <= _charIndex) break;

                string tag = _fullText.Substring(_charIndex + 1, tagEnd - _charIndex - 1);
                _charIndex = tagEnd + 1;

                if (tag.StartsWith("wait:", StringComparison.OrdinalIgnoreCase))
                {
                    if (int.TryParse(tag.Substring(5), out int waitTime))
                        _waitFrames = waitTime;
                }
                else if (tag.StartsWith("speed:", StringComparison.OrdinalIgnoreCase))
                {
                    if (int.TryParse(tag.Substring(6), out int newSpeed))
                        _currentSpeed = Math.Max(1, newSpeed);
                }
                // Mouth on/off is handled in PreprocessText by per-character flags.
            }
        }
    }
}
