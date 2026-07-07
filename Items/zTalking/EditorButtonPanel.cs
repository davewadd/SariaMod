using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;

namespace SariaMod.Items.zTalking
{
    /// <summary>
    /// Button Editor Panel - positioned to the LEFT of the Mock Dialogue Panel.
    /// Contains 5 rows for configuring: Button1, Button2, Button3, Back, Exit
    /// 
    /// POSITIONING MATH (Left-Anchored, Relative to Mock Panel):
    /// ?????????????????????????????????????????????????????????????????
    /// The panel is positioned relative to the Mock Panel's LEFT edge.
    /// 
    /// Given:
    ///   - MockPanelCenter = screen center + panel offset (scaled)
    ///   - MockPanelWidth = 380 (scaled)
    ///   - ButtonPanelWidth = 220 (scaled)
    ///   - Gap = 10px (scaled)
    /// 
    /// Calculation:
    ///   - MockPanelLeftEdge = MockPanelCenter.X - (MockPanelWidth * scale / 2)
    ///   - ButtonPanelRightEdge = MockPanelLeftEdge - Gap * scale
    ///   - ButtonPanelCenter.X = ButtonPanelRightEdge - (ButtonPanelWidth * scale / 2)
    /// 
    /// This ensures the Button Editor Panel stays adjacent (10px gap) to the
    /// Mock Panel regardless of screen resolution or UI scale changes.
    /// ?????????????????????????????????????????????????????????????????
    /// 
    /// LAYOUT (5 Rows):
    /// Each row contains:
    ///   - Toggle (Yes/No) - enables/disables the button
    ///   - Node Input - target node ID
    ///   - Label Input (Button1-3 only) - display text
    /// 
    /// A separator line divides the main buttons (1-3) from utility buttons (Back, Exit).
    /// </summary>
    internal sealed class EditorButtonPanel
    {
        // Panel dimensions (unscaled)
        private const float PanelWidth = 220f;
        private const float PanelHeight = 200f;
        private const float Gap = 10f;

        // Row dimensions
        private const float RowHeight = 32f;
        private const float ToggleWidth = 40f;
        private const float NodeInputWidth = 80f;
        private const float LabelInputWidth = 80f;
        private const float RowPadding = 4f;

        // Reference to parent panel dimensions
        private const float MockPanelWidth = 380f;

        // Row data structure
        private struct ButtonRowData
        {
            public string Name;
            public bool HasLabelInput;
            public Rectangle ToggleHit;
            public Rectangle NodeHit;
            public Rectangle LabelHit;
        }

        private readonly ButtonRowData[] _rows = new ButtonRowData[5];

        // Hover state
        private int _hoveredRow = -1;
        private int _hoveredField = -1; // 0=Toggle, 1=Node, 2=Label

        // Active editing
        private int _editingRow = -1;
        private int _editingField = -1;

        // References to editor state (will be set via callbacks)
        public Func<int, bool> GetButtonEnabled { get; set; }
        public Action<int, bool> SetButtonEnabled { get; set; }
        public Func<int, string> GetButtonTarget { get; set; }
        public Action<int, string> SetButtonTarget { get; set; }
        public Func<int, string> GetButtonLabel { get; set; }
        public Action<int, string> SetButtonLabel { get; set; }

        // Status callback
        public Action<string> OnStatusChanged { get; set; }

        // Panel bounds for mouse detection
        private Rectangle _panelBounds;

        public EditorButtonPanel()
        {
            // Initialize row metadata
            _rows[0] = new ButtonRowData { Name = "Button 1", HasLabelInput = true };
            _rows[1] = new ButtonRowData { Name = "Button 2", HasLabelInput = true };
            _rows[2] = new ButtonRowData { Name = "Button 3", HasLabelInput = true };
            _rows[3] = new ButtonRowData { Name = "Back", HasLabelInput = false };
            _rows[4] = new ButtonRowData { Name = "Exit", HasLabelInput = false };
        }

        /// <summary>
        /// Calculate panel position based on Mock Panel position.
        /// Panel is anchored to the LEFT of the Mock Panel with a 10px gap.
        /// </summary>
        public void UpdateLayout(Vector2 mockPanelCenter, float scale)
        {
            // Calculate Mock Panel's left edge
            float mockPanelLeftEdge = mockPanelCenter.X - (MockPanelWidth * scale / 2f);

            // Position Button Panel to the left with gap
            float buttonPanelRightEdge = mockPanelLeftEdge - (Gap * scale);
            float buttonPanelCenterX = buttonPanelRightEdge - (PanelWidth * scale / 2f);

            // Vertically align with Mock Panel center
            float buttonPanelCenterY = mockPanelCenter.Y;

            Vector2 panelCenter = new Vector2(buttonPanelCenterX, buttonPanelCenterY);

            // Calculate panel bounds
            _panelBounds = new Rectangle(
                (int)(panelCenter.X - PanelWidth * scale / 2f),
                (int)(panelCenter.Y - PanelHeight * scale / 2f),
                (int)(PanelWidth * scale),
                (int)(PanelHeight * scale));

            // Calculate row hit rectangles
            float rowY = _panelBounds.Y + (RowPadding * scale);
            float scaledRowHeight = RowHeight * scale;
            float scaledToggleWidth = ToggleWidth * scale;
            float scaledNodeWidth = NodeInputWidth * scale;
            float scaledLabelWidth = LabelInputWidth * scale;
            float scaledPadding = RowPadding * scale;

            for (int i = 0; i < 5; i++)
            {
                float rowX = _panelBounds.X + scaledPadding;

                // Toggle button
                _rows[i].ToggleHit = new Rectangle(
                    (int)rowX,
                    (int)rowY,
                    (int)scaledToggleWidth,
                    (int)(scaledRowHeight - scaledPadding));

                // Node input
                _rows[i].NodeHit = new Rectangle(
                    (int)(rowX + scaledToggleWidth + scaledPadding),
                    (int)rowY,
                    (int)scaledNodeWidth,
                    (int)(scaledRowHeight - scaledPadding));

                // Label input (only for first 3 rows)
                if (_rows[i].HasLabelInput)
                {
                    _rows[i].LabelHit = new Rectangle(
                        (int)(rowX + scaledToggleWidth + scaledNodeWidth + scaledPadding * 2),
                        (int)rowY,
                        (int)scaledLabelWidth,
                        (int)(scaledRowHeight - scaledPadding));
                }
                else
                {
                    _rows[i].LabelHit = Rectangle.Empty;
                }

                rowY += scaledRowHeight;

                // Add extra spacing after row 2 (separator between main buttons and utility buttons)
                if (i == 2)
                    rowY += scaledPadding * 2;
            }
        }

        public void UpdateHover()
        {
            Point mousePos = new Point(Main.mouseX, Main.mouseY);
            int previousRow = _hoveredRow;
            int previousField = _hoveredField;

            _hoveredRow = -1;
            _hoveredField = -1;

            if (!_panelBounds.Contains(mousePos))
                return;

            for (int i = 0; i < 5; i++)
            {
                if (_rows[i].ToggleHit.Contains(mousePos))
                {
                    _hoveredRow = i;
                    _hoveredField = 0;
                    break;
                }
                if (_rows[i].NodeHit.Contains(mousePos))
                {
                    _hoveredRow = i;
                    _hoveredField = 1;
                    break;
                }
                if (_rows[i].HasLabelInput && _rows[i].LabelHit.Contains(mousePos))
                {
                    _hoveredRow = i;
                    _hoveredField = 2;
                    break;
                }
            }

            if (_hoveredRow != -1 && (_hoveredRow != previousRow || _hoveredField != previousField))
                SoundEngine.PlaySound(SoundID.MenuTick);
        }

        public bool HandleClick()
        {
            if (_hoveredRow == -1)
                return false;

            // Toggle field - toggle enabled state
            if (_hoveredField == 0)
            {
                bool current = GetButtonEnabled?.Invoke(_hoveredRow) ?? true;
                SetButtonEnabled?.Invoke(_hoveredRow, !current);
                OnStatusChanged?.Invoke($"{_rows[_hoveredRow].Name} {(!current ? "Enabled" : "Disabled")}");
                SoundEngine.PlaySound(SoundID.MenuTick);
                return true;
            }

            // Node or Label field - start editing
            if (_hoveredField == 1 || _hoveredField == 2)
            {
                _editingRow = _hoveredRow;
                _editingField = _hoveredField;
                string fieldName = _hoveredField == 1 ? "Target Node" : "Label";
                OnStatusChanged?.Invoke($"Editing {_rows[_hoveredRow].Name} {fieldName} (Tab to finish)");
                return true;
            }

            return false;
        }

        public bool HandleKeyboardInput(string typed)
        {
            if (_editingRow == -1 || _editingField == -1)
                return false;

            // Tab exits editing
            if (typed.Equals("\t", StringComparison.Ordinal) || typed.Equals("\r", StringComparison.Ordinal))
            {
                _editingRow = -1;
                _editingField = -1;
                OnStatusChanged?.Invoke("Editing complete");
                return true;
            }

            // Get current value
            string current = _editingField == 1
                ? (GetButtonTarget?.Invoke(_editingRow) ?? "")
                : (GetButtonLabel?.Invoke(_editingRow) ?? "");

            // Backspace
            if (typed.Equals("\b", StringComparison.Ordinal))
            {
                if (!string.IsNullOrEmpty(current))
                {
                    current = current[..^1];
                    if (_editingField == 1)
                        SetButtonTarget?.Invoke(_editingRow, current);
                    else
                        SetButtonLabel?.Invoke(_editingRow, current);
                }
                return true;
            }

            // Append typed character
            current += typed;
            if (_editingField == 1)
                SetButtonTarget?.Invoke(_editingRow, current);
            else
                SetButtonLabel?.Invoke(_editingRow, current);

            return true;
        }

        public bool IsEditing => _editingRow != -1;

        public bool ContainsMouse()
        {
            return _panelBounds.Contains(new Point(Main.mouseX, Main.mouseY));
        }

        public void Draw(SpriteBatch spriteBatch, float scale)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;

            // Draw panel background
            spriteBatch.Draw(pixel, _panelBounds, Color.Black * 0.85f);

            // Draw panel border
            Color borderColor = Color.Gray * 0.9f;
            spriteBatch.Draw(pixel, new Rectangle(_panelBounds.X, _panelBounds.Y, _panelBounds.Width, 2), borderColor);
            spriteBatch.Draw(pixel, new Rectangle(_panelBounds.X, _panelBounds.Bottom - 2, _panelBounds.Width, 2), borderColor);
            spriteBatch.Draw(pixel, new Rectangle(_panelBounds.X, _panelBounds.Y, 2, _panelBounds.Height), borderColor);
            spriteBatch.Draw(pixel, new Rectangle(_panelBounds.Right - 2, _panelBounds.Y, 2, _panelBounds.Height), borderColor);

            // Draw title
            Vector2 titlePos = new Vector2(_panelBounds.Center.X, _panelBounds.Y - 10 * scale);
            Utils.DrawBorderString(spriteBatch, "Button Editor", titlePos, Color.White, 0.7f * scale, 0.5f, 1f);

            // Draw separator line between Button3 and Back
            float separatorY = _rows[2].ToggleHit.Bottom + (RowPadding * scale);
            spriteBatch.Draw(pixel,
                new Rectangle(_panelBounds.X + 4, (int)separatorY, _panelBounds.Width - 8, 2),
                Color.DarkGray);

            // Draw each row
            for (int i = 0; i < 5; i++)
            {
                DrawRow(spriteBatch, i, scale);
            }
        }

        private void DrawRow(SpriteBatch spriteBatch, int rowIndex, float scale)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            var row = _rows[rowIndex];

            bool isEnabled = GetButtonEnabled?.Invoke(rowIndex) ?? true;
            string nodeValue = GetButtonTarget?.Invoke(rowIndex) ?? "";
            string labelValue = row.HasLabelInput ? (GetButtonLabel?.Invoke(rowIndex) ?? "") : "";

            bool isEditingNode = _editingRow == rowIndex && _editingField == 1;
            bool isEditingLabel = _editingRow == rowIndex && _editingField == 2;

            // Row name label (left of toggle)
            Vector2 namePos = new Vector2(row.ToggleHit.X - 2, row.ToggleHit.Center.Y);
            Utils.DrawBorderString(spriteBatch, row.Name, namePos, Color.LightGray, 0.5f * scale, 1f, 0.5f);

            // Toggle button
            Color toggleBg = isEnabled ? Color.DarkGreen : Color.DarkRed;
            if (_hoveredRow == rowIndex && _hoveredField == 0)
                toggleBg = toggleBg * 1.3f;
            DrawInputBox(spriteBatch, pixel, row.ToggleHit, toggleBg, isEnabled ? "YES" : "NO", Color.White, scale);

            // Node input
            Color nodeBg = isEditingNode ? Color.DarkBlue : Color.DarkSlateGray;
            if (_hoveredRow == rowIndex && _hoveredField == 1)
                nodeBg = nodeBg * 1.3f;
            string nodeDisplay = nodeValue + (isEditingNode ? "_" : "");
            DrawInputBox(spriteBatch, pixel, row.NodeHit, nodeBg, nodeDisplay, Color.White, scale, "Node");

            // Label input (if applicable)
            if (row.HasLabelInput)
            {
                Color labelBg = isEditingLabel ? Color.DarkBlue : Color.DarkSlateGray;
                if (_hoveredRow == rowIndex && _hoveredField == 2)
                    labelBg = labelBg * 1.3f;
                string labelDisplay = labelValue + (isEditingLabel ? "_" : "");
                DrawInputBox(spriteBatch, pixel, row.LabelHit, labelBg, labelDisplay, Color.White, scale, "Label");
            }

            // Grey out entire row if disabled
            if (!isEnabled)
            {
                // Draw semi-transparent overlay over node/label
                spriteBatch.Draw(pixel, row.NodeHit, Color.Black * 0.4f);
                if (row.HasLabelInput)
                    spriteBatch.Draw(pixel, row.LabelHit, Color.Black * 0.4f);
            }
        }

        private static void DrawInputBox(SpriteBatch sb, Texture2D pixel, Rectangle rect, Color bgColor, string text, Color textColor, float scale, string placeholder = null)
        {
            // Background
            sb.Draw(pixel, rect, bgColor);

            // Border
            Color borderColor = Color.Gray * 0.7f;
            sb.Draw(pixel, new Rectangle(rect.X, rect.Y, rect.Width, 1), borderColor);
            sb.Draw(pixel, new Rectangle(rect.X, rect.Bottom - 1, rect.Width, 1), borderColor);
            sb.Draw(pixel, new Rectangle(rect.X, rect.Y, 1, rect.Height), borderColor);
            sb.Draw(pixel, new Rectangle(rect.Right - 1, rect.Y, 1, rect.Height), borderColor);

            // Text
            Vector2 textPos = new Vector2(rect.X + 3, rect.Center.Y);
            string displayText = string.IsNullOrEmpty(text) && placeholder != null ? placeholder : text;
            Color displayColor = string.IsNullOrEmpty(text) && placeholder != null ? Color.Gray * 0.6f : textColor;
            Utils.DrawBorderString(sb, displayText, textPos, displayColor, 0.45f * scale, 0f, 0.5f);
        }

        public void StopEditing()
        {
            _editingRow = -1;
            _editingField = -1;
        }
    }
}
