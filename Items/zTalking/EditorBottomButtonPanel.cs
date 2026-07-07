using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.UI;

namespace SariaMod.Items.zTalking
{
    /// <summary>
    /// Bottom-docked button panel for the Dialogue Editor.
    /// Contains: Exit Creator (left), Save (center), Save As (right)
    /// 
    /// HORIZONTAL CENTERING MATH:
    /// ?????????????????????????????????????????????????????????????????
    /// To center a group of 3 buttons as a single row:
    /// 
    /// Given:
    ///   - ButtonWidth = width of each button (e.g., 100px)
    ///   - ButtonGap = space between buttons (e.g., 20px)
    ///   - TotalGroupWidth = 3 * ButtonWidth + 2 * ButtonGap
    ///                     = 3 * 100 + 2 * 20 = 340px
    /// 
    /// To center the group on screen:
    ///   - GroupLeftEdge = (ScreenWidth - TotalGroupWidth) / 2
    ///   - Button positions from GroupLeftEdge:
    ///       Exit Creator: GroupLeftEdge + 0
    ///       Save:         GroupLeftEdge + ButtonWidth + ButtonGap
    ///       Save As:      GroupLeftEdge + 2 * (ButtonWidth + ButtonGap)
    /// 
    /// Alternatively, position relative to screen center:
    ///   - Save button X = ScreenWidth / 2 (centered)
    ///   - Exit Creator X = ScreenWidth / 2 - (ButtonWidth + ButtonGap)
    ///   - Save As X = ScreenWidth / 2 + (ButtonWidth + ButtonGap)
    /// ?????????????????????????????????????????????????????????????????
    /// </summary>
    internal sealed class EditorBottomButtonPanel
    {
        // Button dimensions
        private const float ButtonWidth = 100f;
        private const float ButtonHeight = 28f;
        private const float ButtonGap = 20f;
        
        // Vertical offset from bottom of screen (negative = up from bottom)
        private const float BottomOffset = 80f;

        // Hit rectangles
        private Rectangle _exitCreatorHit;
        private Rectangle _saveHit;
        private Rectangle _saveAsHit;

        // Hover state
        private int _hoveredButton = -1; // 0 = Exit Creator, 1 = Save, 2 = Save As

        // Button IDs for external reference
        public const int ExitCreatorButtonId = 300;
        public const int SaveButtonId = 301;
        public const int SaveAsButtonId = 302;

        // Callbacks
        public Action OnExitCreatorClicked { get; set; }
        public Action OnSaveClicked { get; set; }
        public Action OnSaveAsClicked { get; set; }

        public int HoveredButton => _hoveredButton;

        /// <summary>
        /// Calculate button positions anchored to bottom of screen.
        /// Uses Top.Set(-BottomOffset, 1f) equivalent logic.
        /// </summary>
        public void UpdateLayout()
        {
            // Vertical position: 100% height minus offset
            float buttonY = Main.screenHeight - BottomOffset;

            // Horizontal centering: Save button is centered, others offset by (ButtonWidth + ButtonGap)
            float centerX = Main.screenWidth / 2f;
            float offsetAmount = ButtonWidth + ButtonGap;

            // Exit Creator (left of Save)
            _exitCreatorHit = new Rectangle(
                (int)(centerX - offsetAmount - ButtonWidth / 2f),
                (int)(buttonY - ButtonHeight / 2f),
                (int)ButtonWidth,
                (int)ButtonHeight);

            // Save (centered)
            _saveHit = new Rectangle(
                (int)(centerX - ButtonWidth / 2f),
                (int)(buttonY - ButtonHeight / 2f),
                (int)ButtonWidth,
                (int)ButtonHeight);

            // Save As (right of Save)
            _saveAsHit = new Rectangle(
                (int)(centerX + offsetAmount - ButtonWidth / 2f),
                (int)(buttonY - ButtonHeight / 2f),
                (int)ButtonWidth,
                (int)ButtonHeight);
        }

        public void UpdateHover()
        {
            Point mousePos = new Point(Main.mouseX, Main.mouseY);
            int previousHover = _hoveredButton;
            _hoveredButton = -1;

            if (_exitCreatorHit.Contains(mousePos))
                _hoveredButton = 0;
            else if (_saveHit.Contains(mousePos))
                _hoveredButton = 1;
            else if (_saveAsHit.Contains(mousePos))
                _hoveredButton = 2;

            if (_hoveredButton != -1 && _hoveredButton != previousHover)
                SoundEngine.PlaySound(SoundID.MenuTick);
        }

        public bool HandleClick()
        {
            if (_hoveredButton == -1)
                return false;

            switch (_hoveredButton)
            {
                case 0:
                    OnExitCreatorClicked?.Invoke();
                    return true;
                case 1:
                    OnSaveClicked?.Invoke();
                    return true;
                case 2:
                    OnSaveAsClicked?.Invoke();
                    return true;
            }

            return false;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;

            // Draw Exit Creator button
            DrawButton(spriteBatch, pixel, _exitCreatorHit, "Exit Creator", 
                _hoveredButton == 0 ? Color.OrangeRed : Color.DarkRed, Color.White);

            // Draw Save button
            DrawButton(spriteBatch, pixel, _saveHit, "Save",
                _hoveredButton == 1 ? Color.DeepSkyBlue : Color.DodgerBlue, Color.White);

            // Draw Save As button
            DrawButton(spriteBatch, pixel, _saveAsHit, "Save As",
                _hoveredButton == 2 ? Color.LimeGreen : Color.ForestGreen, Color.White);
        }

        private static void DrawButton(SpriteBatch sb, Texture2D pixel, Rectangle rect, string label, Color fillColor, Color textColor)
        {
            // Fill
            sb.Draw(pixel, rect, fillColor * 0.85f);

            // Border
            Color borderColor = Color.White * 0.9f;
            sb.Draw(pixel, new Rectangle(rect.X, rect.Y, rect.Width, 2), borderColor);
            sb.Draw(pixel, new Rectangle(rect.X, rect.Bottom - 2, rect.Width, 2), borderColor);
            sb.Draw(pixel, new Rectangle(rect.X, rect.Y, 2, rect.Height), borderColor);
            sb.Draw(pixel, new Rectangle(rect.Right - 2, rect.Y, 2, rect.Height), borderColor);

            // Label
            Vector2 center = new Vector2(rect.Center.X, rect.Center.Y);
            Utils.DrawBorderString(sb, label, center, textColor, 0.8f, 0.5f, 0.5f);
        }

        /// <summary>
        /// Check if mouse is over any button (to block other UI interactions).
        /// </summary>
        public bool ContainsMouse()
        {
            Point mousePos = new Point(Main.mouseX, Main.mouseY);
            return _exitCreatorHit.Contains(mousePos) 
                || _saveHit.Contains(mousePos) 
                || _saveAsHit.Contains(mousePos);
        }
    }
}
