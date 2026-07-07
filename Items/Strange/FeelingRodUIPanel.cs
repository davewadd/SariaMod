using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI;

namespace SariaMod.Items.Strange
{
    /// <summary>
    /// Floating panel for the FeelingRod that lets the developer configure and apply
    /// a mood state to the player's Saria projectile.
    /// Three rows: Mood selector, Timer selector, Lock toggle.
    /// </summary>
    public class FeelingRodUIPanel : UIState
    {
        // Panel layout
        private const int PanelWidth  = 280;
        private const int PanelHeight = 190;

        // Arrow button size
        private const int ArrowW = 28;
        private const int ArrowH = 26;

        // Row vertical positions relative to panel top
        private const int Row1Y = 36;   // Mood
        private const int Row2Y = 82;   // Timer
        private const int Row3Y = 128;  // Lock

        // Colors (match debug panel palette)
        private static readonly Color PanelBg     = new Color(30, 30, 35, 220);
        private static readonly Color PanelBorder  = new Color(70, 70, 80, 255);
        private static readonly Color BtnBg        = new Color(50, 50, 58, 230);
        private static readonly Color BtnBgHover   = new Color(70, 70, 82, 240);
        private static readonly Color BtnBgActive  = new Color(40, 90, 40, 230);
        private static readonly Color BtnBorderAct = new Color(80, 200, 80, 255);
        private static readonly Color TitleColor   = new Color(255, 180, 220);
        private static readonly Color LabelColor   = new Color(200, 200, 200);
        private static readonly Color ValueColor   = new Color(240, 240, 240);

        // Mood names aligned with MoodState enum order
        private static readonly MoodState[] Moods =
        {
            MoodState.Normal,
            MoodState.Happy,
            MoodState.Sad,
            MoodState.Angry,
            MoodState.Cursed,
        };

        public override void Draw(SpriteBatch sb)
        {
            if (!FeelingRodUISystem.IsOpen)
                return;

            // Panel anchored to screen center
            int px = (Main.screenWidth  - PanelWidth)  / 2;
            int py = (Main.screenHeight - PanelHeight) / 2;

            Rectangle panelRect = new Rectangle(px, py, PanelWidth, PanelHeight);

            // Block game mouse when hovering panel
            if (panelRect.Contains(Main.mouseX, Main.mouseY))
                Main.LocalPlayer.mouseInterface = true;

            // Background + border
            DrawRect(sb, panelRect, PanelBg, PanelBorder);

            // Title
            string title = "Feeling Rod — Mood Tester";
            Vector2 titleSz = FontAssets.MouseText.Value.MeasureString(title) * 0.75f;
            Utils.DrawBorderString(sb, title,
                new Vector2(px + (PanelWidth - titleSz.X) * 0.5f, py + 6f),
                TitleColor, 0.75f);

            // ── Row 1: Mood ──────────────────────────────────────────────────────
            int moodIdx = Array.IndexOf(Moods, FeelingRodUISystem.SelectedMood);
            if (moodIdx < 0) moodIdx = 0;

            DrawLabel(sb, px + 10, py + Row1Y + 4, "Mood:", LabelColor);

            // Left arrow
            Rectangle moodLeft = new Rectangle(px + 80, py + Row1Y, ArrowW, ArrowH);
            if (DrawArrow(sb, moodLeft, "<"))
            {
                moodIdx = (moodIdx - 1 + Moods.Length) % Moods.Length;
                FeelingRodUISystem.SelectedMood = Moods[moodIdx];
            }

            // Value box
            string moodName = FeelingRodUISystem.SelectedMood.ToString();
            Rectangle moodBox = new Rectangle(moodLeft.Right + 2, py + Row1Y, 100, ArrowH);
            DrawRect(sb, moodBox, new Color(20, 20, 25, 200), PanelBorder);
            Vector2 moodSz = FontAssets.MouseText.Value.MeasureString(moodName) * 0.8f;
            Utils.DrawBorderString(sb, moodName,
                new Vector2(moodBox.X + (moodBox.Width - moodSz.X) * 0.5f,
                            moodBox.Y + (moodBox.Height - moodSz.Y) * 0.5f),
                ValueColor, 0.8f);

            // Right arrow
            Rectangle moodRight = new Rectangle(moodBox.Right + 2, py + Row1Y, ArrowW, ArrowH);
            if (DrawArrow(sb, moodRight, ">"))
            {
                moodIdx = (moodIdx + 1) % Moods.Length;
                FeelingRodUISystem.SelectedMood = Moods[moodIdx];
            }

            // ── Row 2: Timer ─────────────────────────────────────────────────────
            DrawLabel(sb, px + 10, py + Row2Y + 4, "Timer:", LabelColor);

            // Left arrow (−60 ticks, or −600 with Shift)
            bool timerShift = Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftShift)
                           || Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.RightShift);
            Rectangle timerLeft = new Rectangle(px + 80, py + Row2Y, ArrowW, ArrowH);
            if (DrawArrow(sb, timerLeft, "<"))
                FeelingRodUISystem.SelectedTimer = Math.Max(60, FeelingRodUISystem.SelectedTimer - (timerShift ? 600 : 60));

            // Value box — show ticks and seconds
            int ticks = FeelingRodUISystem.SelectedTimer;
            string timerStr = $"{ticks}t ({ticks / 60}s)";
            Rectangle timerBox = new Rectangle(timerLeft.Right + 2, py + Row2Y, 100, ArrowH);
            DrawRect(sb, timerBox, new Color(20, 20, 25, 200), PanelBorder);
            Vector2 timerSz = FontAssets.MouseText.Value.MeasureString(timerStr) * 0.72f;
            Utils.DrawBorderString(sb, timerStr,
                new Vector2(timerBox.X + (timerBox.Width - timerSz.X) * 0.5f,
                            timerBox.Y + (timerBox.Height - timerSz.Y) * 0.5f),
                ValueColor, 0.72f);

            // Right arrow (+60 ticks, or +600 with Shift)
            Rectangle timerRight = new Rectangle(timerBox.Right + 2, py + Row2Y, ArrowW, ArrowH);
            if (DrawArrow(sb, timerRight, ">"))
                FeelingRodUISystem.SelectedTimer = Math.Min(36000, FeelingRodUISystem.SelectedTimer + (timerShift ? 600 : 60));

            // Shift-click hint for larger increments (drawn below the timer row to avoid overlap)
            string timerHint = "Hold Shift: ±600t / ±5 pri";
            Utils.DrawBorderString(sb, timerHint,
                new Vector2(px + 80f, py + Row2Y + ArrowH + 4f),
                new Color(130, 130, 130), 0.65f);

            // ── Row 3: Priority ──────────────────────────────────────────────────
            DrawLabel(sb, px + 10, py + Row3Y + 4, "Priority:", LabelColor);

            bool priorityShift = Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftShift)
                               || Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.RightShift);
            Rectangle priorityLeft = new Rectangle(px + 80, py + Row3Y, ArrowW, ArrowH);
            if (DrawArrow(sb, priorityLeft, "<"))
                FeelingRodUISystem.SelectedPriority = Math.Max(0, FeelingRodUISystem.SelectedPriority - (priorityShift ? 5 : 1));

            string priorityStr = FeelingRodUISystem.SelectedPriority.ToString();
            Rectangle priorityBox = new Rectangle(priorityLeft.Right + 2, py + Row3Y, 60, ArrowH);
            DrawRect(sb, priorityBox, new Color(20, 20, 25, 200), PanelBorder);
            Vector2 prioritySz = FontAssets.MouseText.Value.MeasureString(priorityStr) * 0.85f;
            Utils.DrawBorderString(sb, priorityStr,
                new Vector2(priorityBox.X + (priorityBox.Width - prioritySz.X) * 0.5f,
                            priorityBox.Y + (priorityBox.Height - prioritySz.Y) * 0.5f),
                ValueColor, 0.85f);

            Rectangle priorityRight = new Rectangle(priorityBox.Right + 2, py + Row3Y, ArrowW, ArrowH);
            if (DrawArrow(sb, priorityRight, ">"))
                FeelingRodUISystem.SelectedPriority = Math.Min(25, FeelingRodUISystem.SelectedPriority + (priorityShift ? 5 : 1));

            // ── Footer hint ──────────────────────────────────────────────────────
            string hint = "Right-click FeelingRod to apply";
            Utils.DrawBorderString(sb, hint,
                new Vector2(px + 10f, py + PanelHeight - 20f),
                new Color(160, 160, 160), 0.68f);

            // ── Close button ─────────────────────────────────────────────────────
            Rectangle closeBtn = new Rectangle(px + PanelWidth - 26, py + 4, 22, 22);
            bool closeHover = closeBtn.Contains(Main.mouseX, Main.mouseY);
            DrawRect(sb, closeBtn, closeHover ? new Color(160, 60, 60, 230) : BtnBg, PanelBorder);
            Vector2 xSz = FontAssets.MouseText.Value.MeasureString("x") * 0.85f;
            Utils.DrawBorderString(sb, "x",
                new Vector2(closeBtn.X + (closeBtn.Width - xSz.X) * 0.5f,
                            closeBtn.Y + (closeBtn.Height - xSz.Y) * 0.5f),
                closeHover ? Color.White : new Color(200, 150, 150), 0.85f);
            if (closeHover)
            {
                Main.LocalPlayer.mouseInterface = true;
                if (Main.mouseLeft && Main.mouseLeftRelease)
                {
                    FeelingRodUISystem.IsOpen = false;
                    Main.mouseLeftRelease = false;
                }
            }

            base.Draw(sb);
        }

        // ── Helpers ──────────────────────────────────────────────────────────────

        /// <summary>Draws an arrow button. Returns true on click (left-release).</summary>
        private bool DrawArrow(SpriteBatch sb, Rectangle rect, string label)
        {
            bool hover = rect.Contains(Main.mouseX, Main.mouseY);
            DrawRect(sb, rect, hover ? BtnBgHover : BtnBg, PanelBorder);
            Vector2 sz = FontAssets.MouseText.Value.MeasureString(label) * 0.85f;
            Utils.DrawBorderString(sb, label,
                new Vector2(rect.X + (rect.Width - sz.X) * 0.5f,
                            rect.Y + (rect.Height - sz.Y) * 0.5f),
                LabelColor, 0.85f);
            if (hover)
            {
                Main.LocalPlayer.mouseInterface = true;
                // Shift held → larger increment (handled by caller when true)
                if (Main.mouseLeft && Main.mouseLeftRelease)
                {
                    Main.mouseLeftRelease = false;
                    return true;
                }
            }
            return false;
        }

        private void DrawLabel(SpriteBatch sb, int x, int y, string text, Color color)
        {
            Utils.DrawBorderString(sb, text, new Vector2(x, y), color, 0.8f);
        }

        private static void DrawRect(SpriteBatch sb, Rectangle rect, Color fill, Color border)
        {
            Texture2D pix = TextureAssets.MagicPixel.Value;
            sb.Draw(pix, rect, fill);
            // top
            sb.Draw(pix, new Rectangle(rect.X, rect.Y, rect.Width, 1), border);
            // bottom
            sb.Draw(pix, new Rectangle(rect.X, rect.Bottom - 1, rect.Width, 1), border);
            // left
            sb.Draw(pix, new Rectangle(rect.X, rect.Y, 1, rect.Height), border);
            // right
            sb.Draw(pix, new Rectangle(rect.Right - 1, rect.Y, 1, rect.Height), border);
        }
    }
}
