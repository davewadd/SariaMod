using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;

namespace SariaMod.Items.zTalking
{
    /// <summary>
    /// Inline section-based text editor that renders directly in the dialogue panel's text area.
    /// Sections appear as they would in-game. Clicking a section activates it for editing.
    /// </summary>
    internal sealed class EditorTextSectionPanel
    {
        // ============================================================
        // SECTION DATA
        // ============================================================
        private readonly List<EditorTextSection> _sections = new();

        /// <summary>Index of the currently active (editable) section, or -1 if none.</summary>
        private int _activeSectionIndex = -1;

        /// <summary>True when a section is active and the user is typing into it.</summary>
        public bool IsTyping => _activeSectionIndex >= 0;

        /// <summary>Read-only access to sections for building dialogue output.</summary>
        public IReadOnlyList<EditorTextSection> Sections => _sections;

        // ============================================================
        // PARSED / RENDERED STATE
        // ============================================================
        private struct SectionChar
        {
            public char Character;
            public Color TextColor;
            public int SectionIndex;
        }

        private readonly List<SectionChar> _allChars = new();
        private readonly List<List<SectionChar>> _wrappedLines = new();

        // Per-character screen positions (filled during Draw for click detection)
        private readonly List<Vector2> _charPositions = new();
        private readonly List<Vector2> _charSizes = new();
        private int _totalRenderedChars;

        // Text area bounds (set by parent each frame)
        private Vector2 _textAreaOrigin;
        private float _textAreaMaxWidth;
        private float _textAreaMaxHeight;
        private float _textScale;
        private float _lineHeight;
        private Rectangle _textAreaHit;

        // ============================================================
        // SECTION PROPERTY TOOLBAR
        // ============================================================
        private const float ToolbarHeight = 18f;
        private const float ToolbarItemWidth = 32f;
        private Rectangle _toolbarHit;
        private Rectangle _speedDownHit;
        private Rectangle _speedUpHit;
        private Rectangle _mouthToggleHit;
        private Rectangle _waitDownHit;
        private Rectangle _waitUpHit;
        private Rectangle _deleteHit;
        private Rectangle _addHit;
        private Rectangle _colorPrevHit;
        private Rectangle _colorNextHit;
        private int _hoveredToolbarButton = -1;

        private static readonly string[] ColorNames = new[]
        {
            "White", "Pink", "LightBlue", "Green", "Yellow",
            "Orange", "Red", "Purple", "Cyan", "Gold", "Gray"
        };

        // ============================================================
        // DRAG REORDER
        // ============================================================
        private bool _isDragging;
        private int _dragSectionIndex = -1;
        private Vector2 _dragStartMouse;
        private int _dragInsertIndex = -1;

        // Empty section placeholder hit areas (for clicking invisible sections)
        private readonly List<(int SectionIndex, Rectangle Rect)> _emptySectionHits = new();

        // ============================================================
        // CLICK STATE
        // ============================================================
        private bool _wasMouseDown;
        private bool _mouseDownThisFrame;
        private bool _mouseReleasedThisFrame;
        private int _clickCooldown;

        // ============================================================
        // CALLBACKS
        // ============================================================
        public Action<string> OnStatusChanged;

        // ============================================================
        // PUBLIC API
        // ============================================================

        /// <summary>Clear all sections and reset state.</summary>
        public void Clear()
        {
            _sections.Clear();
            _activeSectionIndex = -1;
            _isDragging = false;
            _dragSectionIndex = -1;
            Reparse();
        }

        /// <summary>Add a section at the end of the list.</summary>
        public void AddSection(EditorTextSection section)
        {
            _sections.Add(section);
            Reparse();
        }

        /// <summary>Replace all sections (used when loading a node).</summary>
        public void SetSections(List<EditorTextSection> sections)
        {
            _sections.Clear();
            if (sections != null)
            {
                foreach (var s in sections)
                    _sections.Add(s.Clone());
            }
            _activeSectionIndex = -1;
            Reparse();
        }

        /// <summary>Compose all sections into a tagged dialogue string.</summary>
        public string ComposeTaggedText()
        {
            if (_sections.Count == 0)
                return "";

            var parts = new List<string>();
            foreach (var s in _sections)
            {
                if (string.IsNullOrEmpty(s.Text))
                    continue;

                string p = "";
                if (!string.IsNullOrWhiteSpace(s.Color) && !s.Color.Equals("White", StringComparison.OrdinalIgnoreCase))
                    p += $"[color:{s.Color}]";
                if (s.Speed > 0 && s.Speed != 2)
                    p += $"[speed:{s.Speed}]";
                if (!s.Mouth)
                    p += "[/mouth]";

                p += s.Text;

                if (!s.Mouth)
                    p += "[mouth]";
                if (s.WaitFrames > 0)
                    p += $"[wait:{s.WaitFrames}]";

                parts.Add(p);
            }

            return string.Join(string.Empty, parts);
        }

        /// <summary>Check if mouse is over the text area or toolbar.</summary>
        public bool ContainsMouse()
        {
            Point m = new(Main.mouseX, Main.mouseY);
            if (_textAreaHit.Contains(m)) return true;
            if (_activeSectionIndex >= 0 && _toolbarHit.Contains(m)) return true;
            if (_addHit.Width > 0 && _addHit.Contains(m)) return true;
            return false;
        }

        // ============================================================
        // UPDATE (called each frame by parent)
        // ============================================================

        /// <summary>
        /// Set the text area layout for this frame.
        /// Called before UpdateInput so hit detection is accurate.
        /// </summary>
        public void UpdateLayout(Vector2 panelPos, float scale)
        {
            float uiScale = scale;
            Vector2 textOffset = GetTextOffset();
            _textAreaOrigin = panelPos + textOffset * uiScale;
            _textAreaMaxWidth = GetTextMaxWidth() * uiScale;
            _textAreaMaxHeight = GetTextMaxHeight() * uiScale;
            _textScale = DialogueUIState.TextScale * uiScale;
            _lineHeight = DialogueUIState.LineHeightBase * uiScale;

            _textAreaHit = new Rectangle(
                (int)_textAreaOrigin.X,
                (int)(_textAreaOrigin.Y - 4 * uiScale),
                (int)_textAreaMaxWidth,
                (int)(_textAreaMaxHeight + 8 * uiScale));

            // Add button always visible at bottom-right of text area
            float addBtnSize = 14f * uiScale;
            _addHit = new Rectangle(
                (int)(_textAreaOrigin.X + _textAreaMaxWidth - addBtnSize),
                (int)(_textAreaOrigin.Y + _textAreaMaxHeight + 2 * uiScale),
                (int)addBtnSize,
                (int)addBtnSize);

            // Toolbar above text area when a section is active
            if (_activeSectionIndex >= 0)
            {
                float toolbarWidth = _textAreaMaxWidth;
                float toolbarY = _textAreaOrigin.Y - (ToolbarHeight + 6) * uiScale;
                _toolbarHit = new Rectangle(
                    (int)_textAreaOrigin.X,
                    (int)toolbarY,
                    (int)toolbarWidth,
                    (int)(ToolbarHeight * uiScale));

                float btnW = ToolbarItemWidth * uiScale;
                float btnH = ToolbarHeight * uiScale;
                float x = _toolbarHit.X;
                float y = _toolbarHit.Y;

                // Layout: [ColorPrev][ColorNext] [Spd-][Spd+] [Mouth] [Wait-][Wait+] [Del]
                _colorPrevHit = new Rectangle((int)x, (int)y, (int)(btnW * 0.6f), (int)btnH);
                x += btnW * 0.6f + 1;
                _colorNextHit = new Rectangle((int)x, (int)y, (int)(btnW * 0.6f), (int)btnH);
                x += btnW * 0.6f + 4;
                _speedDownHit = new Rectangle((int)x, (int)y, (int)(btnW * 0.5f), (int)btnH);
                x += btnW * 0.5f + 1;
                _speedUpHit = new Rectangle((int)x, (int)y, (int)(btnW * 0.5f), (int)btnH);
                x += btnW * 0.5f + 4;
                _mouthToggleHit = new Rectangle((int)x, (int)y, (int)(btnW * 0.7f), (int)btnH);
                x += btnW * 0.7f + 4;
                _waitDownHit = new Rectangle((int)x, (int)y, (int)(btnW * 0.5f), (int)btnH);
                x += btnW * 0.5f + 1;
                _waitUpHit = new Rectangle((int)x, (int)y, (int)(btnW * 0.5f), (int)btnH);
                x += btnW * 0.5f + 4;
                _deleteHit = new Rectangle((int)x, (int)y, (int)(btnW * 0.6f), (int)btnH);
            }
            else
            {
                _toolbarHit = Rectangle.Empty;
            }
        }

        /// <summary>Handle mouse and keyboard input. Returns true if input was consumed.</summary>
        public bool UpdateInput()
        {
            bool mouseDown = Main.mouseLeft;
            _mouseDownThisFrame = mouseDown && !_wasMouseDown;
            _mouseReleasedThisFrame = !mouseDown && _wasMouseDown;
            if (_clickCooldown > 0) _clickCooldown--;

            bool consumed = false;

            if (_clickCooldown <= 0)
            {
                consumed |= UpdateToolbarHover();
                consumed |= UpdateDragReorder(mouseDown);
                consumed |= UpdateClickDetection();
                consumed |= UpdateToolbarClicks();
            }

            if (_activeSectionIndex >= 0)
                consumed |= UpdateKeyboardInput();

            _wasMouseDown = mouseDown;
            return consumed;
        }

        // ============================================================
        // PARSING — Compose sections into renderable character list
        // ============================================================
        private void Reparse()
        {
            _allChars.Clear();
            _wrappedLines.Clear();

            for (int si = 0; si < _sections.Count; si++)
            {
                var s = _sections[si];
                Color color = Color.White;
                if (!string.IsNullOrWhiteSpace(s.Color) && DialogueUIState.NamedColors.TryGetValue(s.Color, out Color c))
                    color = c;

                foreach (char ch in s.Text)
                {
                    _allChars.Add(new SectionChar
                    {
                        Character = ch,
                        TextColor = color,
                        SectionIndex = si
                    });
                }
            }

            // Word-wrap
            DynamicSpriteFont font = FontAssets.MouseText.Value;
            float maxW = _textAreaMaxWidth > 0 ? _textAreaMaxWidth : DialogueUIState.TextMaxWidth * 1.5f;
            float scale = _textScale > 0 ? _textScale : DialogueUIState.TextScale * 1.5f;

            List<SectionChar> currentLine = new();
            List<SectionChar> currentWord = new();
            float currentLineWidth = 0;

            float MeasureChar(char ch) => font.MeasureString(ch.ToString()).X * scale;

            float MeasureWord(List<SectionChar> word)
            {
                float w = 0;
                foreach (var sc in word) w += MeasureChar(sc.Character);
                return w;
            }

            foreach (var sc in _allChars)
            {
                if (sc.Character == ' ')
                {
                    float ww = MeasureWord(currentWord);
                    if (currentLineWidth + ww > maxW && currentLine.Count > 0)
                    {
                        _wrappedLines.Add(new List<SectionChar>(currentLine));
                        currentLine.Clear();
                        currentLineWidth = 0;
                    }
                    currentLine.AddRange(currentWord);
                    currentLineWidth += ww;
                    currentLine.Add(sc);
                    currentLineWidth += MeasureChar(' ');
                    currentWord.Clear();
                }
                else if (sc.Character == '\n')
                {
                    currentLine.AddRange(currentWord);
                    _wrappedLines.Add(new List<SectionChar>(currentLine));
                    currentLine.Clear();
                    currentWord.Clear();
                    currentLineWidth = 0;
                }
                else
                {
                    currentWord.Add(sc);
                }
            }

            if (currentWord.Count > 0)
            {
                float ww = MeasureWord(currentWord);
                if (currentLineWidth + ww > maxW && currentLine.Count > 0)
                {
                    _wrappedLines.Add(new List<SectionChar>(currentLine));
                    currentLine.Clear();
                }
                currentLine.AddRange(currentWord);
            }
            if (currentLine.Count > 0)
                _wrappedLines.Add(currentLine);
        }

        // ============================================================
        // CLICK DETECTION — Map mouse to section
        // ============================================================
        private bool UpdateClickDetection()
        {
            if (!_mouseReleasedThisFrame) return false;
            if (_isDragging) return false;

            Point m = new(Main.mouseX, Main.mouseY);

            // Add button
            if (_addHit.Contains(m))
            {
                var newSection = new EditorTextSection();
                _sections.Add(newSection);
                _activeSectionIndex = _sections.Count - 1;
                Reparse();
                OnStatusChanged?.Invoke($"Section {_activeSectionIndex + 1} created. Type to edit.");
                SoundEngine.PlaySound(SoundID.MenuOpen);
                _clickCooldown = 10;
                return true;
            }

            if (!_textAreaHit.Contains(m))
            {
                // Don't deactivate if clicking on toolbar or add button
                if (_activeSectionIndex >= 0 && _toolbarHit != Rectangle.Empty && _toolbarHit.Contains(m))
                    return false; // Let UpdateToolbarClicks handle it
                if (_addHit.Width > 0 && _addHit.Contains(m))
                    return false; // Already handled above, safety fallback

                // Click outside text area: deactivate
                if (_activeSectionIndex >= 0)
                {
                    _activeSectionIndex = -1;
                    OnStatusChanged?.Invoke("Section deactivated.");
                    _clickCooldown = 8;
                    return true;
                }
                return false;
            }

            // Find which section was clicked by checking rendered char positions
            int clickedSection = FindSectionAtMouse(m);

            if (clickedSection >= 0)
            {
                if (_activeSectionIndex == clickedSection)
                {
                    // Already active — keep it active (user may be repositioning cursor)
                    return true;
                }

                _activeSectionIndex = clickedSection;
                OnStatusChanged?.Invoke($"Section {clickedSection + 1} active. Type to edit, Esc to deselect.");
                SoundEngine.PlaySound(SoundID.MenuTick);
                _clickCooldown = 8;
                return true;
            }

            // Clicked in text area but not on any section character
            if (_activeSectionIndex >= 0)
            {
                _activeSectionIndex = -1;
                OnStatusChanged?.Invoke("Section deactivated.");
                _clickCooldown = 8;
                return true;
            }

            return false;
        }

        private int FindSectionAtMouse(Point mousePos)
        {
            if (_charPositions.Count == 0 && _emptySectionHits.Count == 0) return -1;

            // Check rendered character bounding areas first
            int idx = 0;
            foreach (var line in _wrappedLines)
            {
                foreach (var sc in line)
                {
                    if (idx < _charPositions.Count && idx < _charSizes.Count)
                    {
                        Vector2 pos = _charPositions[idx];
                        Vector2 size = _charSizes[idx];
                        Rectangle charRect = new((int)pos.X, (int)(pos.Y - size.Y * 0.2f), (int)Math.Max(size.X, 4), (int)(size.Y * 1.2f));
                        if (charRect.Contains(mousePos))
                            return sc.SectionIndex;
                    }
                    idx++;
                }
            }

            // Check empty section placeholder areas
            foreach (var (sectionIndex, rect) in _emptySectionHits)
            {
                if (rect.Contains(mousePos))
                    return sectionIndex;
            }

            return -1;
        }

        // ============================================================
        // KEYBOARD INPUT
        // ============================================================
        private bool UpdateKeyboardInput()
        {
            if (_activeSectionIndex < 0 || _activeSectionIndex >= _sections.Count)
                return false;

            // Escape: deactivate section
            if (Main.keyState.IsKeyDown(Keys.Escape) && !Main.oldKeyState.IsKeyDown(Keys.Escape))
            {
                _activeSectionIndex = -1;
                OnStatusChanged?.Invoke("Section deactivated.");
                return true;
            }

            // Enter: deactivate section (commit)
            if (Main.keyState.IsKeyDown(Keys.Enter) && !Main.oldKeyState.IsKeyDown(Keys.Enter))
            {
                OnStatusChanged?.Invoke($"Section {_activeSectionIndex + 1} locked.");
                _activeSectionIndex = -1;
                return true;
            }

            // Backspace: use key state since Main.GetInputText processes it internally
            // (GetInputText("") returns "" for backspace, never "\b")
            if (Main.keyState.IsKeyDown(Keys.Back) && !Main.oldKeyState.IsKeyDown(Keys.Back))
            {
                var bsSection = _sections[_activeSectionIndex];
                if (!string.IsNullOrEmpty(bsSection.Text))
                {
                    bsSection.Text = bsSection.Text[..^1];
                    Reparse();
                }
                return true;
            }

            // Tab: use key state (same reason as backspace)
            if (Main.keyState.IsKeyDown(Keys.Tab) && !Main.oldKeyState.IsKeyDown(Keys.Tab))
            {
                if (_activeSectionIndex < _sections.Count - 1)
                    _activeSectionIndex++;
                else
                {
                    _sections.Add(new EditorTextSection());
                    _activeSectionIndex = _sections.Count - 1;
                    Reparse();
                }
                OnStatusChanged?.Invoke($"Section {_activeSectionIndex + 1} active.");
                SoundEngine.PlaySound(SoundID.MenuTick);
                return true;
            }

            string typed = Main.GetInputText(string.Empty);
            if (string.IsNullOrEmpty(typed))
                return true; // Still consuming input even if nothing typed

            // Filter out control characters that GetInputText may pass through
            if (typed.Equals("\r", StringComparison.Ordinal) || typed.Equals("\n", StringComparison.Ordinal)
                || typed.Equals("\t", StringComparison.Ordinal) || typed.Equals("\b", StringComparison.Ordinal))
                return true;

            var section = _sections[_activeSectionIndex];
            section.Text += typed;
            Reparse();
            return true;
        }

        // ============================================================
        // TOOLBAR
        // ============================================================
        private bool UpdateToolbarHover()
        {
            int prev = _hoveredToolbarButton;
            _hoveredToolbarButton = -1;

            Point m = new(Main.mouseX, Main.mouseY);

            // Add button hover works even when no section is active
            if (_addHit.Width > 0 && _addHit.Contains(m))
            {
                _hoveredToolbarButton = 8;
                if (_hoveredToolbarButton != prev)
                    SoundEngine.PlaySound(SoundID.MenuTick);
                return true;
            }

            if (_activeSectionIndex < 0) return false;
            if (_colorPrevHit.Contains(m)) _hoveredToolbarButton = 0;
            else if (_colorNextHit.Contains(m)) _hoveredToolbarButton = 1;
            else if (_speedDownHit.Contains(m)) _hoveredToolbarButton = 2;
            else if (_speedUpHit.Contains(m)) _hoveredToolbarButton = 3;
            else if (_mouthToggleHit.Contains(m)) _hoveredToolbarButton = 4;
            else if (_waitDownHit.Contains(m)) _hoveredToolbarButton = 5;
            else if (_waitUpHit.Contains(m)) _hoveredToolbarButton = 6;
            else if (_deleteHit.Contains(m)) _hoveredToolbarButton = 7;
            else if (_addHit.Contains(m)) _hoveredToolbarButton = 8;

            if (_hoveredToolbarButton != -1 && _hoveredToolbarButton != prev)
                SoundEngine.PlaySound(SoundID.MenuTick);

            return _hoveredToolbarButton != -1;
        }

        private bool UpdateToolbarClicks()
        {
            if (!_mouseReleasedThisFrame) return false;
            if (_activeSectionIndex < 0 || _activeSectionIndex >= _sections.Count) return false;
            if (_hoveredToolbarButton < 0) return false;

            var section = _sections[_activeSectionIndex];

            switch (_hoveredToolbarButton)
            {
                case 0: // Color prev
                    CycleColor(section, -1);
                    break;
                case 1: // Color next
                    CycleColor(section, 1);
                    break;
                case 2: // Speed down
                    section.Speed = Math.Max(1, section.Speed - 1);
                    OnStatusChanged?.Invoke($"Speed: {section.Speed}");
                    break;
                case 3: // Speed up
                    section.Speed = Math.Min(10, section.Speed + 1);
                    OnStatusChanged?.Invoke($"Speed: {section.Speed}");
                    break;
                case 4: // Mouth toggle
                    section.Mouth = !section.Mouth;
                    OnStatusChanged?.Invoke($"Mouth: {(section.Mouth ? "On" : "Off")}");
                    break;
                case 5: // Wait down
                    section.WaitFrames = Math.Max(0, section.WaitFrames - 10);
                    OnStatusChanged?.Invoke($"Wait: {section.WaitFrames}f");
                    break;
                case 6: // Wait up
                    section.WaitFrames = Math.Min(600, section.WaitFrames + 10);
                    OnStatusChanged?.Invoke($"Wait: {section.WaitFrames}f");
                    break;
                case 7: // Delete
                    _sections.RemoveAt(_activeSectionIndex);
                    if (_activeSectionIndex >= _sections.Count)
                        _activeSectionIndex = _sections.Count - 1;
                    Reparse();
                    OnStatusChanged?.Invoke("Section deleted.");
                    break;
                default:
                    return false;
            }

            Reparse();
            SoundEngine.PlaySound(SoundID.MenuTick);
            _clickCooldown = 8;
            return true;
        }

        private void CycleColor(EditorTextSection section, int delta)
        {
            int idx = Array.IndexOf(ColorNames, section.Color);
            if (idx < 0) idx = 0;
            idx = (idx + delta + ColorNames.Length) % ColorNames.Length;
            section.Color = ColorNames[idx];
            OnStatusChanged?.Invoke($"Color: {section.Color}");
        }

        // ============================================================
        // DRAG REORDER
        // ============================================================
        private bool UpdateDragReorder(bool mouseDown)
        {
            if (_activeSectionIndex < 0) return false;

            Point m = new(Main.mouseX, Main.mouseY);

            // Start drag: mouse held on active section text for a moment
            if (_mouseDownThisFrame && _textAreaHit.Contains(m) && !_isDragging)
            {
                int clickedSection = FindSectionAtMouse(m);
                if (clickedSection == _activeSectionIndex && _sections.Count > 1)
                {
                    _isDragging = true;
                    _dragSectionIndex = _activeSectionIndex;
                    _dragStartMouse = new Vector2(Main.mouseX, Main.mouseY);
                    _dragInsertIndex = _activeSectionIndex;
                    return true;
                }
            }

            if (_isDragging && mouseDown)
            {
                // Determine insert position based on mouse Y relative to section boundaries
                _dragInsertIndex = FindInsertIndexAtMouse(m);
                return true;
            }

            if (_isDragging && !mouseDown)
            {
                // Complete reorder
                if (_dragInsertIndex >= 0 && _dragInsertIndex != _dragSectionIndex)
                {
                    var moving = _sections[_dragSectionIndex];
                    _sections.RemoveAt(_dragSectionIndex);
                    int insertAt = _dragInsertIndex > _dragSectionIndex ? _dragInsertIndex - 1 : _dragInsertIndex;
                    insertAt = Math.Clamp(insertAt, 0, _sections.Count);
                    _sections.Insert(insertAt, moving);
                    _activeSectionIndex = insertAt;
                    Reparse();
                    OnStatusChanged?.Invoke($"Section moved to position {insertAt + 1}.");
                    SoundEngine.PlaySound(SoundID.MenuTick);
                }
                _isDragging = false;
                _dragSectionIndex = -1;
                _clickCooldown = 10;
                return true;
            }

            return false;
        }

        private int FindInsertIndexAtMouse(Point mousePos)
        {
            // Find the section boundary closest to the mouse Y position
            // by checking where each section's characters start/end in screen space
            if (_charPositions.Count == 0 || _sections.Count <= 1)
                return _dragSectionIndex;

            // Build section Y ranges from rendered characters
            float[] sectionMidY = new float[_sections.Count];
            int[] sectionCharCount = new int[_sections.Count];

            int idx = 0;
            foreach (var line in _wrappedLines)
            {
                foreach (var sc in line)
                {
                    if (idx < _charPositions.Count)
                    {
                        sectionMidY[sc.SectionIndex] += _charPositions[idx].Y;
                        sectionCharCount[sc.SectionIndex]++;
                    }
                    idx++;
                }
            }

            for (int i = 0; i < _sections.Count; i++)
            {
                if (sectionCharCount[i] > 0)
                    sectionMidY[i] /= sectionCharCount[i];
            }

            // Find nearest slot
            int best = 0;
            float bestDist = float.MaxValue;
            for (int i = 0; i <= _sections.Count; i++)
            {
                float slotY;
                if (i == 0)
                    slotY = sectionMidY[0] - _lineHeight;
                else if (i >= _sections.Count)
                    slotY = sectionMidY[_sections.Count - 1] + _lineHeight;
                else
                    slotY = (sectionMidY[i - 1] + sectionMidY[i]) / 2f;

                float dist = Math.Abs(mousePos.Y - slotY);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = i;
                }
            }

            return best;
        }

        // ============================================================
        // DRAWING
        // ============================================================

        public void Draw(SpriteBatch spriteBatch, float uiScale)
        {
            _charPositions.Clear();
            _charSizes.Clear();
            _emptySectionHits.Clear();
            _totalRenderedChars = 0;

            if (_wrappedLines.Count == 0 && _sections.Count == 0)
            {
                // Draw placeholder + add hint
                DrawAddButton(spriteBatch, uiScale);
                return;
            }

            DynamicSpriteFont font = FontAssets.MouseText.Value;

            // Compute text scale to fit (mirrors DialogueUIState logic)
            float baseTextScale = _textScale;
            int totalLines = Math.Max(1, _wrappedLines.Count);
            float textScale = ComputeTextScaleToFit(baseTextScale, _lineHeight, totalLines, _textAreaMaxHeight);
            float lineHeight = DialogueUIState.LineHeightBase * (textScale / baseTextScale) * uiScale;

            Vector2 currentPos = _textAreaOrigin;

            // First pass: find active section character ranges for outline
            int activeStart = -1, activeEnd = -1;
            if (_activeSectionIndex >= 0)
            {
                int ci = 0;
                foreach (var line in _wrappedLines)
                {
                    foreach (var sc in line)
                    {
                        if (sc.SectionIndex == _activeSectionIndex)
                        {
                            if (activeStart < 0) activeStart = ci;
                            activeEnd = ci;
                        }
                        ci++;
                    }
                }
            }

            // Draw characters line by line, tracking positions
            int charIdx = 0;
            foreach (var line in _wrappedLines)
            {
                float xOffset = 0;
                foreach (var sc in line)
                {
                    string charStr = sc.Character.ToString();
                    Vector2 charSize = font.MeasureString(charStr) * textScale;
                    Vector2 charPos = currentPos + new Vector2(xOffset, 0);

                    _charPositions.Add(charPos);
                    _charSizes.Add(charSize);

                    // Draw highlight background for active section
                    if (_activeSectionIndex >= 0 && sc.SectionIndex == _activeSectionIndex)
                    {
                        Texture2D pixel = TextureAssets.MagicPixel.Value;
                        Rectangle hlRect = new(
                            (int)charPos.X,
                            (int)(charPos.Y - 1),
                            (int)Math.Max(charSize.X, 2),
                            (int)(charSize.Y + 2));
                        spriteBatch.Draw(pixel, hlRect, Color.White * 0.12f);
                    }

                    // Draw the character
                    Color drawColor = sc.TextColor;
                    if (_isDragging && sc.SectionIndex == _dragSectionIndex)
                        drawColor *= 0.4f; // Ghost the dragged section

                    Utils.DrawBorderString(spriteBatch, charStr, charPos, drawColor, textScale, 0f, 0f);
                    xOffset += charSize.X;
                    charIdx++;
                }
                currentPos.Y += lineHeight;
            }
            _totalRenderedChars = charIdx;

            // Draw empty section placeholders (blinking cursor markers)
            DrawEmptySectionPlaceholders(spriteBatch, font, textScale, lineHeight, uiScale);

            // Draw outline border around active section characters
            if (_activeSectionIndex >= 0 && activeStart >= 0 && activeEnd >= 0)
            {
                DrawSectionOutline(spriteBatch, activeStart, activeEnd, uiScale);
            }
            else if (_activeSectionIndex >= 0 && activeStart < 0)
            {
                // Active section is empty — draw outline around its placeholder
                foreach (var (sectionIndex, rect) in _emptySectionHits)
                {
                    if (sectionIndex == _activeSectionIndex)
                    {
                        Texture2D px = TextureAssets.MagicPixel.Value;
                        int pad = (int)(2 * uiScale);
                        Rectangle outline = new(
                            rect.X - pad, rect.Y - pad,
                            rect.Width + pad * 2, rect.Height + pad * 2);
                        Color borderColor = Color.Cyan * 0.6f;
                        int thickness = Math.Max(1, (int)(1 * uiScale));
                        spriteBatch.Draw(px, new Rectangle(outline.X, outline.Y, outline.Width, thickness), borderColor);
                        spriteBatch.Draw(px, new Rectangle(outline.X, outline.Bottom - thickness, outline.Width, thickness), borderColor);
                        spriteBatch.Draw(px, new Rectangle(outline.X, outline.Y, thickness, outline.Height), borderColor);
                        spriteBatch.Draw(px, new Rectangle(outline.Right - thickness, outline.Y, thickness, outline.Height), borderColor);
                        break;
                    }
                }
            }

            // Draw drag insert indicator
            if (_isDragging && _dragInsertIndex >= 0)
            {
                DrawDragInsertIndicator(spriteBatch, uiScale);
            }

            // Draw toolbar when section is active
            if (_activeSectionIndex >= 0)
            {
                DrawToolbar(spriteBatch, uiScale);
            }

            // Draw add button
            DrawAddButton(spriteBatch, uiScale);
        }

        private void DrawEmptySectionPlaceholders(SpriteBatch spriteBatch, DynamicSpriteFont font, float textScale, float lineHeight, float uiScale)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            bool blink = (Main.GameUpdateCount / 20) % 2 == 0;

            for (int si = 0; si < _sections.Count; si++)
            {
                if (!string.IsNullOrEmpty(_sections[si].Text))
                    continue;

                // Determine position: after the last character of the previous non-empty section,
                // or at the text area origin if this is the first section.
                Vector2 placeholderPos = _textAreaOrigin;

                // Walk through rendered chars to find the end of the previous section
                if (_charPositions.Count > 0)
                {
                    int lastCharOfPrev = -1;
                    int ci = 0;
                    foreach (var line in _wrappedLines)
                    {
                        foreach (var sc in line)
                        {
                            if (sc.SectionIndex < si)
                                lastCharOfPrev = ci;
                            ci++;
                        }
                    }

                    if (lastCharOfPrev >= 0 && lastCharOfPrev < _charPositions.Count)
                    {
                        Vector2 prevPos = _charPositions[lastCharOfPrev];
                        Vector2 prevSize = lastCharOfPrev < _charSizes.Count
                            ? _charSizes[lastCharOfPrev]
                            : new Vector2(4, 12);
                        placeholderPos = new Vector2(prevPos.X + prevSize.X + 2f, prevPos.Y);
                    }
                }

                // Determine the section's color for the marker
                Color sectionColor = Color.White;
                string colorName = _sections[si].Color;
                if (!string.IsNullOrWhiteSpace(colorName) && DialogueUIState.NamedColors.TryGetValue(colorName, out Color namedColor))
                    sectionColor = namedColor;

                // Measure a reference character height
                float charHeight = font.MeasureString("A").Y * textScale;
                float markerWidth = Math.Max(6f, 4f * uiScale);

                Rectangle markerRect = new(
                    (int)placeholderPos.X,
                    (int)(placeholderPos.Y - 1),
                    (int)markerWidth,
                    (int)(charHeight + 2));

                // Store for click detection
                _emptySectionHits.Add((si, markerRect));

                // Draw blinking cursor marker
                bool isActive = si == _activeSectionIndex;
                if (blink || isActive)
                {
                    Color markerColor = isActive ? Color.Cyan * 0.7f : sectionColor * 0.5f;
                    spriteBatch.Draw(pixel, markerRect, markerColor);
                }

                // Draw small section index label so the user can identify it
                string label = $"[{si + 1}]";
                Vector2 labelPos = new(markerRect.X, markerRect.Bottom + 1);
                Utils.DrawBorderString(spriteBatch, label, labelPos, isActive ? Color.Cyan : Color.Gray * 0.7f, 0.35f * uiScale, 0f, 0f);
            }
        }

        private void DrawSectionOutline(SpriteBatch spriteBatch, int startIdx, int endIdx, float uiScale)
        {
            if (startIdx < 0 || endIdx < 0 || _charPositions.Count == 0) return;

            // Find bounding box of the active section's characters
            float minX = float.MaxValue, minY = float.MaxValue;
            float maxX = float.MinValue, maxY = float.MinValue;

            for (int i = startIdx; i <= endIdx && i < _charPositions.Count; i++)
            {
                Vector2 pos = _charPositions[i];
                Vector2 size = i < _charSizes.Count ? _charSizes[i] : new Vector2(4, 12);
                minX = Math.Min(minX, pos.X);
                minY = Math.Min(minY, pos.Y - 1);
                maxX = Math.Max(maxX, pos.X + size.X);
                maxY = Math.Max(maxY, pos.Y + size.Y + 1);
            }

            if (minX >= maxX || minY >= maxY) return;

            Texture2D pixel = TextureAssets.MagicPixel.Value;
            int pad = (int)(2 * uiScale);
            Rectangle outline = new(
                (int)minX - pad,
                (int)minY - pad,
                (int)(maxX - minX) + pad * 2,
                (int)(maxY - minY) + pad * 2);

            Color borderColor = Color.Cyan * 0.6f;
            int thickness = Math.Max(1, (int)(1 * uiScale));

            spriteBatch.Draw(pixel, new Rectangle(outline.X, outline.Y, outline.Width, thickness), borderColor);
            spriteBatch.Draw(pixel, new Rectangle(outline.X, outline.Bottom - thickness, outline.Width, thickness), borderColor);
            spriteBatch.Draw(pixel, new Rectangle(outline.X, outline.Y, thickness, outline.Height), borderColor);
            spriteBatch.Draw(pixel, new Rectangle(outline.Right - thickness, outline.Y, thickness, outline.Height), borderColor);
        }

        private void DrawDragInsertIndicator(SpriteBatch spriteBatch, float uiScale)
        {
            // Draw a horizontal line where the section would be inserted
            if (_dragInsertIndex < 0 || _charPositions.Count == 0) return;

            Texture2D pixel = TextureAssets.MagicPixel.Value;
            float y;

            // Find Y position for the insert indicator
            if (_dragInsertIndex == 0 && _charPositions.Count > 0)
            {
                y = _charPositions[0].Y - 3 * uiScale;
            }
            else
            {
                // Find the last char of the section before the insert point
                int charBefore = -1;
                int ci = 0;
                foreach (var line in _wrappedLines)
                {
                    foreach (var sc in line)
                    {
                        if (sc.SectionIndex < _dragInsertIndex)
                            charBefore = ci;
                        ci++;
                    }
                }

                if (charBefore >= 0 && charBefore < _charPositions.Count && charBefore < _charSizes.Count)
                    y = _charPositions[charBefore].Y + _charSizes[charBefore].Y + 2 * uiScale;
                else
                    y = _textAreaOrigin.Y + _textAreaMaxHeight;
            }

            Rectangle indicator = new(
                (int)_textAreaOrigin.X,
                (int)y,
                (int)_textAreaMaxWidth,
                (int)Math.Max(2, 2 * uiScale));

            spriteBatch.Draw(pixel, indicator, Color.Yellow * 0.8f);
        }

        private void DrawToolbar(SpriteBatch spriteBatch, float uiScale)
        {
            if (_activeSectionIndex < 0 || _activeSectionIndex >= _sections.Count) return;

            var section = _sections[_activeSectionIndex];
            Texture2D pixel = TextureAssets.MagicPixel.Value;

            // Background
            spriteBatch.Draw(pixel, _toolbarHit, Color.Black * 0.7f);
            Color borderC = Color.Cyan * 0.4f;
            spriteBatch.Draw(pixel, new Rectangle(_toolbarHit.X, _toolbarHit.Y, _toolbarHit.Width, 1), borderC);
            spriteBatch.Draw(pixel, new Rectangle(_toolbarHit.X, _toolbarHit.Bottom - 1, _toolbarHit.Width, 1), borderC);

            float txtScale = 0.4f * uiScale;

            // Color
            DrawToolbarButton(spriteBatch, _colorPrevHit, "<", 0, txtScale);
            DrawToolbarButton(spriteBatch, _colorNextHit, ">", 1, txtScale);
            Color dispColor = Color.White;
            if (DialogueUIState.NamedColors.TryGetValue(section.Color, out Color nc))
                dispColor = nc;
            Vector2 colorLabelPos = new((_colorPrevHit.Right + _colorNextHit.X) / 2f - 2, _toolbarHit.Center.Y);
            Utils.DrawBorderString(spriteBatch, section.Color, new Vector2(_colorNextHit.Right + 4, _toolbarHit.Center.Y), dispColor, txtScale * 0.9f, 0f, 0.5f);

            // Speed
            DrawToolbarButton(spriteBatch, _speedDownHit, "-", 2, txtScale);
            DrawToolbarButton(spriteBatch, _speedUpHit, "+", 3, txtScale);
            Utils.DrawBorderString(spriteBatch, $"Spd:{section.Speed}", new Vector2(_speedDownHit.X, _toolbarHit.Y - 8 * uiScale), Color.LightGray, txtScale * 0.85f, 0f, 0f);

            // Mouth
            Color mouthColor = section.Mouth ? Color.LightGreen : Color.IndianRed;
            string mouthLabel = section.Mouth ? "M:On" : "M:Off";
            DrawToolbarButton(spriteBatch, _mouthToggleHit, mouthLabel, 4, txtScale, mouthColor);

            // Wait
            DrawToolbarButton(spriteBatch, _waitDownHit, "-", 5, txtScale);
            DrawToolbarButton(spriteBatch, _waitUpHit, "+", 6, txtScale);
            Utils.DrawBorderString(spriteBatch, $"Wait:{section.WaitFrames}f", new Vector2(_waitDownHit.X, _toolbarHit.Y - 8 * uiScale), Color.LightGray, txtScale * 0.85f, 0f, 0f);

            // Delete
            DrawToolbarButton(spriteBatch, _deleteHit, "Del", 7, txtScale, Color.IndianRed);
        }

        private void DrawToolbarButton(SpriteBatch spriteBatch, Rectangle rect, string label, int btnId, float txtScale, Color? labelColor = null)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Color bg = _hoveredToolbarButton == btnId ? Color.White * 0.2f : Color.White * 0.08f;
            spriteBatch.Draw(pixel, rect, bg);
            Color lc = labelColor ?? Color.White;
            Utils.DrawBorderString(spriteBatch, label, new Vector2(rect.Center.X, rect.Center.Y), lc, txtScale, 0.5f, 0.5f);
        }

        private void DrawAddButton(SpriteBatch spriteBatch, float uiScale)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            bool hovered = _hoveredToolbarButton == 8;
            Color bg = hovered ? Color.LimeGreen * 0.3f : Color.White * 0.1f;
            spriteBatch.Draw(pixel, _addHit, bg);

            Color borderC = hovered ? Color.LimeGreen * 0.6f : Color.White * 0.3f;
            spriteBatch.Draw(pixel, new Rectangle(_addHit.X, _addHit.Y, _addHit.Width, 1), borderC);
            spriteBatch.Draw(pixel, new Rectangle(_addHit.X, _addHit.Bottom - 1, _addHit.Width, 1), borderC);
            spriteBatch.Draw(pixel, new Rectangle(_addHit.X, _addHit.Y, 1, _addHit.Height), borderC);
            spriteBatch.Draw(pixel, new Rectangle(_addHit.Right - 1, _addHit.Y, 1, _addHit.Height), borderC);

            Utils.DrawBorderString(spriteBatch, "+", new Vector2(_addHit.Center.X, _addHit.Center.Y), Color.LimeGreen, 0.55f * uiScale, 0.5f, 0.5f);
        }

        // ============================================================
        // HELPERS
        // ============================================================
        private static Vector2 GetTextOffset()
        {
            try
            {
                return new Vector2(
                    FairyConfig.Instance?.DialogueTextOffsetX ?? DialogueUIState.TextOffset.X,
                    FairyConfig.Instance?.DialogueTextOffsetY ?? DialogueUIState.TextOffset.Y);
            }
            catch { return DialogueUIState.TextOffset; }
        }

        private static float GetTextMaxWidth()
        {
            try { return FairyConfig.Instance?.DialogueTextMaxWidth ?? DialogueUIState.TextMaxWidth; }
            catch { return DialogueUIState.TextMaxWidth; }
        }

        private static float GetTextMaxHeight()
        {
            try { return FairyConfig.Instance?.DialogueTextMaxHeight ?? 48f; }
            catch { return 48f; }
        }

        private static float ComputeTextScaleToFit(float baseTextScale, float scaledLineHeight, int totalLines, float maxHeight)
        {
            if (totalLines <= 0) return baseTextScale;
            const float clampMin = 0.45f;

            float fitForLines(int allowedLines)
            {
                float req = allowedLines * scaledLineHeight;
                if (req <= 0) return baseTextScale;
                float ratio = maxHeight / req;
                return baseTextScale * MathHelper.Clamp(ratio, clampMin, 1f);
            }

            if (totalLines <= 3) return baseTextScale;
            if (totalLines <= 4) return fitForLines(3);
            if (totalLines <= 5) return fitForLines(4);
            return fitForLines(5);
        }
    }
}
