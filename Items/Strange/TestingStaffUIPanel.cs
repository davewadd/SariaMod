using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.UI;

namespace SariaMod.Items.Strange
{
    public class TestingStaffUIPanel : UIState
    {
        private const int PanelWidth = 300;
        private const int PanelHeight = 164;
        private const int PanelGapFromPlayer = 42;
        private const int ScreenMargin = 8;

        private const int HeaderHeight = 26;
        private const int SpeedSectionY = 30;
        private const int SpeedSectionHeight = 39;
        private const int ProjectileSectionY = 76;
        private const int ListViewportY = 96;
        private const int ListViewportHeight = 60;
        private const int ProjectileRowHeight = 26;

        private const int ScrollBarWidth = 7;
        private const int ScrollBarPadding = 2;

        private static readonly Color PanelBackground = new Color(27, 29, 36, 232);
        private static readonly Color PanelBorder = new Color(83, 88, 105, 255);
        private static readonly Color SectionBackground = new Color(38, 41, 50, 235);
        private static readonly Color ButtonBackground = new Color(55, 59, 70, 240);
        private static readonly Color ButtonHover = new Color(78, 84, 101, 245);
        private static readonly Color SelectedBackground = new Color(90, 64, 31, 245);
        private static readonly Color SelectedBorder = new Color(255, 190, 72, 255);
        private static readonly Color ScrollTrackColor = new Color(20, 22, 28, 220);
        private static readonly Color ScrollThumbColor = new Color(105, 113, 135, 235);
        private static readonly Color ScrollThumbHover = new Color(151, 166, 204, 245);
        private static readonly Color TitleColor = new Color(255, 209, 116);
        private static readonly Color LabelColor = new Color(205, 210, 222);
        private static readonly Color MutedColor = new Color(139, 145, 160);
        private static readonly Color ValueColor = new Color(247, 247, 247);
        private static readonly RasterizerState ScissorRasterizerState = new RasterizerState
        {
            ScissorTestEnable = true,
        };

        private float scrollY;
        private bool scrollDragging;
        private float scrollDragStartY;
        private float scrollDragStartValue;
        private ulong lastScrollFrame = ulong.MaxValue;
        private Point anchoredPanelPosition;
        private bool hasAnchoredPanelPosition;
        private bool leftMouseWasDown;

        private float ContentHeight => TestingStaffUISystem.ProjectileOptionCount * ProjectileRowHeight;
        private float MaxScroll => Math.Max(0f, ContentHeight - ListViewportHeight);

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (!TestingStaffUISystem.IsOpen)
                return;

            // Track our own press edge. Terraria's shared mouseLeftRelease flag
            // can already be consumed by the held item or another UI layer.
            bool leftMousePressed = Main.mouseLeft && !leftMouseWasDown;
            leftMouseWasDown = Main.mouseLeft;

            Rectangle panelBounds = GetPanelBounds();
            Point mouse = GetMouseUiPosition().ToPoint();
            if (!panelBounds.Contains(mouse))
                return;

            Main.LocalPlayer.mouseInterface = true;

            if (!leftMousePressed)
                return;

            Rectangle closeButton = GetCloseButton(panelBounds);
            if (closeButton.Contains(mouse))
            {
                TestingStaffUISystem.CloseUI();
                Main.mouseLeftRelease = false;
                return;
            }

            Rectangle speedSection = GetSpeedSection(panelBounds);
            Rectangle minusButton = GetMinusButton(speedSection);
            Rectangle plusButton = GetPlusButton(speedSection);
            Rectangle damageSection = GetDamageSection(panelBounds);
            Rectangle damageMinusButton = GetMinusButton(damageSection);
            Rectangle damagePlusButton = GetPlusButton(damageSection);
            bool shift = Main.keyState.IsKeyDown(Keys.LeftShift) || Main.keyState.IsKeyDown(Keys.RightShift);
            int adjustmentStep = shift ? 5 : 1;

            if (minusButton.Contains(mouse))
            {
                TestingStaffUISystem.AdjustShootSpeed(-adjustmentStep);
                Main.mouseLeftRelease = false;
                return;
            }

            if (plusButton.Contains(mouse))
            {
                TestingStaffUISystem.AdjustShootSpeed(adjustmentStep);
                Main.mouseLeftRelease = false;
                return;
            }

            if (damageMinusButton.Contains(mouse))
            {
                TestingStaffUISystem.AdjustDamage(-adjustmentStep);
                Main.mouseLeftRelease = false;
                return;
            }

            if (damagePlusButton.Contains(mouse))
            {
                TestingStaffUISystem.AdjustDamage(adjustmentStep);
                Main.mouseLeftRelease = false;
                return;
            }

            Rectangle listViewport = GetListViewport(panelBounds);
            int hoveredOption = GetHoveredOption(listViewport, mouse);
            if (hoveredOption >= 0)
                TestingStaffUISystem.SelectProjectile(hoveredOption);

            // Consume every click inside the panel so the held item cannot fire
            // through controls or through empty list space.
            Main.mouseLeftRelease = false;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!TestingStaffUISystem.IsOpen)
                return;

            Rectangle panelBounds = GetPanelBounds();
            Rectangle listViewport = GetListViewport(panelBounds);
            Point mouse = GetMouseUiPosition().ToPoint();
            bool panelHovered = panelBounds.Contains(mouse);

            HandleScrollbarDrag(listViewport, mouse);

            if (panelHovered || scrollDragging)
            {
                // Match SariaDebugUIPanel's rolling draw-time lock as a backup.
                // Current-frame wheel movement is applied only from
                // ModSystem.PostUpdateInput, after Terraria polls fresh input.
                PlayerInput.LockVanillaMouseScroll("SariaMod/TestingStaffProjectileList");
                Main.LocalPlayer.mouseInterface = true;
            }

            DrawRectangle(spriteBatch, panelBounds, PanelBackground, PanelBorder);

            string title = "Testing Staff";
            DrawCenteredText(spriteBatch, title,
                new Rectangle(panelBounds.X + 6, panelBounds.Y + 3, panelBounds.Width - 36, HeaderHeight - 4),
                TitleColor, 0.76f);

            Rectangle closeButton = GetCloseButton(panelBounds);
            bool closeHovered = closeButton.Contains(mouse);
            DrawRectangle(spriteBatch, closeButton,
                closeHovered ? new Color(155, 59, 59, 245) : ButtonBackground, PanelBorder);
            DrawCenteredText(spriteBatch, "x", closeButton,
                closeHovered ? Color.White : new Color(225, 165, 165), 0.78f);

            DrawSpeedSection(spriteBatch, panelBounds, mouse);
            DrawDamageSection(spriteBatch, panelBounds, mouse);

            Utils.DrawBorderString(spriteBatch, "Projectile",
                new Vector2(panelBounds.X + 8, panelBounds.Y + ProjectileSectionY), LabelColor, 0.68f);
            Utils.DrawBorderString(spriteBatch, "scroll",
                new Vector2(panelBounds.Right - 49, panelBounds.Y + ProjectileSectionY + 1), MutedColor, 0.52f);

            DrawRectangle(spriteBatch, listViewport, SectionBackground, PanelBorder);
            DrawProjectileList(spriteBatch, listViewport, mouse);
            DrawScrollbar(spriteBatch, listViewport, mouse);

            base.Draw(spriteBatch);
        }

        public void CaptureScrollInput()
        {
            if (!TestingStaffUISystem.IsOpen || Main.gameMenu)
                return;

            Rectangle panelBounds = GetPanelBounds();
            Point mouse = GetMouseUiPosition().ToPoint();
            if (!panelBounds.Contains(mouse))
                return;

            // SariaDebugUIPanel uses this lock while hovered. Calling it from
            // PostUpdateInput additionally captures the current input cycle
            // before the player updates their active hotbar slot.
            PlayerInput.LockVanillaMouseScroll("SariaMod/TestingStaffProjectileList");
            Main.LocalPlayer.mouseInterface = true;
            ApplyScrollWheel(PlayerInput.ScrollWheelDelta);
        }

        private void DrawSpeedSection(SpriteBatch spriteBatch, Rectangle panelBounds, Point mouse)
        {
            Rectangle speedSection = GetSpeedSection(panelBounds);
            DrawRectangle(spriteBatch, speedSection, SectionBackground, PanelBorder);
            Utils.DrawBorderString(spriteBatch, "Shoot Speed",
                new Vector2(speedSection.X + 7, speedSection.Y + 5), LabelColor, 0.68f);

            Rectangle minusButton = GetMinusButton(speedSection);
            Rectangle plusButton = GetPlusButton(speedSection);
            Rectangle speedValue = GetValueBox(speedSection);
            DrawButton(spriteBatch, minusButton, "-", minusButton.Contains(mouse));
            DrawRectangle(spriteBatch, speedValue, new Color(22, 24, 30, 240), PanelBorder);
            DrawCenteredText(spriteBatch, TestingStaffUISystem.SelectedShootSpeed.ToString(),
                speedValue, ValueColor, 0.72f);
            DrawButton(spriteBatch, plusButton, "+", plusButton.Contains(mouse));

            string speedHint = TestingStaffUISystem.SelectedShootSpeed == 0
                ? "0: cursor"
                : "Shift: +5";
            Utils.DrawBorderString(spriteBatch, speedHint,
                new Vector2(speedSection.Right - 53, speedSection.Y + 6), MutedColor, 0.50f);
        }

        private void DrawDamageSection(SpriteBatch spriteBatch, Rectangle panelBounds, Point mouse)
        {
            Rectangle damageSection = GetDamageSection(panelBounds);
            DrawRectangle(spriteBatch, damageSection, SectionBackground, PanelBorder);
            Utils.DrawBorderString(spriteBatch, "Damage",
                new Vector2(damageSection.X + 7, damageSection.Y + 5), LabelColor, 0.68f);

            Rectangle minusButton = GetMinusButton(damageSection);
            Rectangle plusButton = GetPlusButton(damageSection);
            Rectangle damageValue = GetValueBox(damageSection);
            DrawButton(spriteBatch, minusButton, "-", minusButton.Contains(mouse));
            DrawRectangle(spriteBatch, damageValue, new Color(22, 24, 30, 240), PanelBorder);
            DrawCenteredText(spriteBatch, TestingStaffUISystem.SelectedDamage.ToString(),
                damageValue, ValueColor, 0.72f);
            DrawButton(spriteBatch, plusButton, "+", plusButton.Contains(mouse));
            Utils.DrawBorderString(spriteBatch, "Shift: +5",
                new Vector2(damageSection.Right - 53, damageSection.Y + 6), MutedColor, 0.50f);
        }

        private bool ApplyScrollWheel(int wheel)
        {
            if (wheel == 0 || lastScrollFrame == Main.GameUpdateCount)
                return false;

            lastScrollFrame = Main.GameUpdateCount;
            float previousScroll = scrollY;
            scrollY = Math.Clamp(
                scrollY - Math.Sign(wheel) * ProjectileRowHeight * 3,
                0f,
                MaxScroll);

            if (Math.Abs(previousScroll - scrollY) <= 0.01f)
                return false;

            SoundEngine.PlaySound(SoundID.MenuTick);
            return true;
        }

        public void ResetInteractionState()
        {
            scrollDragging = false;
            lastScrollFrame = ulong.MaxValue;
            leftMouseWasDown = Main.mouseLeft;
            anchoredPanelPosition = CalculateInitialPanelPosition();
            hasAnchoredPanelPosition = true;
        }

        private void HandleScrollbarDrag(Rectangle listViewport, Point mouse)
        {
            Rectangle thumb = GetScrollThumb(listViewport);
            if (thumb.Contains(mouse) && Main.mouseLeft && !scrollDragging)
            {
                scrollDragging = true;
                scrollDragStartY = mouse.Y;
                scrollDragStartValue = scrollY;
            }

            if (!scrollDragging)
                return;

            if (!Main.mouseLeft)
            {
                scrollDragging = false;
                return;
            }

            Rectangle track = GetScrollTrack(listViewport);
            float scrollPerPixel = MaxScroll / Math.Max(1f, track.Height - thumb.Height);
            scrollY = Math.Clamp(
                scrollDragStartValue + (mouse.Y - scrollDragStartY) * scrollPerPixel,
                0f,
                MaxScroll);
            Main.LocalPlayer.mouseInterface = true;
        }

        private void DrawProjectileList(SpriteBatch spriteBatch, Rectangle listViewport, Point mouse)
        {
            Vector2 topLeft = Vector2.Transform(listViewport.TopLeft(), Main.UIScaleMatrix);
            Vector2 bottomRight = Vector2.Transform(listViewport.BottomRight(), Main.UIScaleMatrix);
            Rectangle deviceClip = new Rectangle(
                (int)Math.Floor(topLeft.X),
                (int)Math.Floor(topLeft.Y),
                Math.Max(1, (int)Math.Ceiling(bottomRight.X) - (int)Math.Floor(topLeft.X)),
                Math.Max(1, (int)Math.Ceiling(bottomRight.Y) - (int)Math.Floor(topLeft.Y)));

            Rectangle previousScissor = spriteBatch.GraphicsDevice.ScissorRectangle;
            Rectangle graphicsBounds = new Rectangle(
                0,
                0,
                spriteBatch.GraphicsDevice.Viewport.Width,
                spriteBatch.GraphicsDevice.Viewport.Height);
            deviceClip = Rectangle.Intersect(deviceClip, graphicsBounds);
            if (previousScissor.Width > 0 && previousScissor.Height > 0)
                deviceClip = Rectangle.Intersect(deviceClip, previousScissor);
            if (deviceClip.Width <= 0 || deviceClip.Height <= 0)
                return;

            spriteBatch.End();
            spriteBatch.GraphicsDevice.ScissorRectangle = deviceClip;
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                SamplerState.LinearClamp, null,
                ScissorRasterizerState,
                null, Main.UIScaleMatrix);

            int firstVisibleOption = Math.Max(0, (int)Math.Floor(scrollY / ProjectileRowHeight));
            int lastVisibleOption = Math.Min(
                TestingStaffUISystem.ProjectileOptionCount - 1,
                (int)Math.Ceiling((scrollY + ListViewportHeight) / ProjectileRowHeight));
            for (int optionIndex = firstVisibleOption; optionIndex <= lastVisibleOption; optionIndex++)
            {
                Rectangle rowBounds = GetOptionRowBounds(listViewport, optionIndex);
                if (rowBounds.Bottom <= listViewport.Top || rowBounds.Top >= listViewport.Bottom)
                    continue;

                bool selected = optionIndex == TestingStaffUISystem.SelectedProjectileIndex;
                bool hovered = listViewport.Contains(mouse) && rowBounds.Contains(mouse);
                Color fill = selected ? SelectedBackground : hovered ? ButtonHover : ButtonBackground;
                Color border = selected ? SelectedBorder : PanelBorder;
                DrawRectangle(spriteBatch, rowBounds, fill, border);

                if (selected)
                {
                    Utils.DrawBorderString(spriteBatch, ">",
                        new Vector2(rowBounds.X + 5, rowBounds.Y + 4), SelectedBorder, 0.62f);
                }

                DrawCenteredText(spriteBatch, TestingStaffUISystem.GetProjectileName(optionIndex),
                    new Rectangle(rowBounds.X + 15, rowBounds.Y, rowBounds.Width - 24, rowBounds.Height),
                    selected ? TitleColor : LabelColor, selected ? 0.71f : 0.66f);
            }

            spriteBatch.End();
            spriteBatch.GraphicsDevice.ScissorRectangle = previousScissor;
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                SamplerState.LinearClamp, null, null, null, Main.UIScaleMatrix);
        }

        private void DrawScrollbar(SpriteBatch spriteBatch, Rectangle listViewport, Point mouse)
        {
            Rectangle track = GetScrollTrack(listViewport);
            Rectangle thumb = GetScrollThumb(listViewport);
            bool thumbHovered = thumb.Contains(mouse) || scrollDragging;
            DrawRectangle(spriteBatch, track, ScrollTrackColor, Color.Transparent);
            DrawRectangle(spriteBatch, thumb,
                thumbHovered ? ScrollThumbHover : ScrollThumbColor,
                Color.Transparent);
        }

        private int GetHoveredOption(Rectangle listViewport, Point mouse)
        {
            if (!listViewport.Contains(mouse))
                return -1;

            for (int optionIndex = 0; optionIndex < TestingStaffUISystem.ProjectileOptionCount; optionIndex++)
            {
                if (GetOptionRowBounds(listViewport, optionIndex).Contains(mouse))
                    return optionIndex;
            }

            return -1;
        }

        private Rectangle GetOptionRowBounds(Rectangle listViewport, int optionIndex)
        {
            return new Rectangle(
                listViewport.X + 2,
                listViewport.Y + (int)(optionIndex * ProjectileRowHeight - scrollY) + 1,
                listViewport.Width - ScrollBarWidth - ScrollBarPadding * 3,
                ProjectileRowHeight - 2);
        }

        private static Rectangle GetScrollTrack(Rectangle listViewport)
        {
            return new Rectangle(
                listViewport.Right - ScrollBarWidth - ScrollBarPadding,
                listViewport.Y + ScrollBarPadding,
                ScrollBarWidth,
                listViewport.Height - ScrollBarPadding * 2);
        }

        private Rectangle GetScrollThumb(Rectangle listViewport)
        {
            Rectangle track = GetScrollTrack(listViewport);
            if (MaxScroll <= 0f)
                return track;

            int thumbHeight = Math.Max(14, (int)(ListViewportHeight / ContentHeight * track.Height));
            float scrollFraction = scrollY / MaxScroll;
            int thumbY = track.Y + (int)(scrollFraction * (track.Height - thumbHeight));
            return new Rectangle(track.X, thumbY, track.Width, thumbHeight);
        }

        public bool ContainsMouse()
        {
            if (!TestingStaffUISystem.IsOpen || Main.gameMenu)
                return false;

            return GetPanelBounds().Contains(GetMouseUiPosition().ToPoint());
        }

        public Rectangle GetPanelBounds()
        {
            if (!hasAnchoredPanelPosition)
            {
                anchoredPanelPosition = CalculateInitialPanelPosition();
                hasAnchoredPanelPosition = true;
            }

            GetUiScreenLimits(out int minX, out int minY, out int maxX, out int maxY);
            int panelX = Math.Clamp(anchoredPanelPosition.X, minX, Math.Max(minX, maxX));
            int panelY = Math.Clamp(anchoredPanelPosition.Y, minY, Math.Max(minY, maxY));
            anchoredPanelPosition = new Point(panelX, panelY);

            return new Rectangle(panelX, panelY, PanelWidth, PanelHeight);
        }

        private static Point CalculateInitialPanelPosition()
        {
            Player player = Main.LocalPlayer;
            Matrix inverseUi = Matrix.Invert(Main.UIScaleMatrix);

            Vector2 playerScreen = player.Center - Main.screenPosition;
            playerScreen = Vector2.Transform(playerScreen, Main.GameViewMatrix.ZoomMatrix);
            Vector2 playerUi = Vector2.Transform(playerScreen, inverseUi);

            GetUiScreenLimits(out int minX, out int minY, out int maxX, out int maxY);
            int panelX = (int)playerUi.X + PanelGapFromPlayer;
            if (panelX > maxX)
                panelX = (int)playerUi.X - PanelWidth - PanelGapFromPlayer;

            int panelY = (int)playerUi.Y - PanelHeight / 2;
            panelX = Math.Clamp(panelX, minX, Math.Max(minX, maxX));
            panelY = Math.Clamp(panelY, minY, Math.Max(minY, maxY));

            return new Point(panelX, panelY);
        }

        private static void GetUiScreenLimits(out int minX, out int minY, out int maxX, out int maxY)
        {
            Matrix inverseUi = Matrix.Invert(Main.UIScaleMatrix);
            Vector2 originalScreenSize = new Vector2(
                PlayerInput.OriginalScreenSize.X,
                PlayerInput.OriginalScreenSize.Y);
            Vector2 uiTopLeft = Vector2.Transform(Vector2.Zero, inverseUi);
            Vector2 uiBottomRight = Vector2.Transform(originalScreenSize, inverseUi);
            minX = (int)uiTopLeft.X + ScreenMargin;
            minY = (int)uiTopLeft.Y + ScreenMargin;
            maxX = (int)uiBottomRight.X - PanelWidth - ScreenMargin;
            maxY = (int)uiBottomRight.Y - PanelHeight - ScreenMargin;
        }

        private static Vector2 GetMouseUiPosition()
        {
            Matrix inverseUi = Matrix.Invert(Main.UIScaleMatrix);
            // Main.mouseX/Y are temporarily converted by Terraria while a UI layer
            // is drawing. PlayerInput.MouseX/Y stay in raw screen coordinates, so
            // transforming them exactly once keeps UpdateUI, PostUpdateInput, Draw,
            // and item-use hit tests on the same panel bounds at every UI scale.
            return Vector2.Transform(
                new Vector2(PlayerInput.MouseX, PlayerInput.MouseY),
                inverseUi);
        }

        private static Rectangle GetCloseButton(Rectangle panelBounds)
        {
            return new Rectangle(panelBounds.Right - 25, panelBounds.Y + 4, 20, 19);
        }

        private static Rectangle GetSpeedSection(Rectangle panelBounds)
        {
            int sectionWidth = (panelBounds.Width - 18) / 2;
            return new Rectangle(
                panelBounds.X + 6,
                panelBounds.Y + SpeedSectionY,
                sectionWidth,
                SpeedSectionHeight);
        }

        private static Rectangle GetDamageSection(Rectangle panelBounds)
        {
            Rectangle speedSection = GetSpeedSection(panelBounds);
            return new Rectangle(
                speedSection.Right + 6,
                speedSection.Y,
                speedSection.Width,
                speedSection.Height);
        }

        private static Rectangle GetMinusButton(Rectangle sectionBounds)
        {
            int controlsWidth = 94;
            int controlsX = sectionBounds.X + (sectionBounds.Width - controlsWidth) / 2;
            return new Rectangle(controlsX, sectionBounds.Y + 18, 24, 17);
        }

        private static Rectangle GetValueBox(Rectangle sectionBounds)
        {
            Rectangle minus = GetMinusButton(sectionBounds);
            return new Rectangle(minus.Right + 3, minus.Y, 40, minus.Height);
        }

        private static Rectangle GetPlusButton(Rectangle sectionBounds)
        {
            Rectangle value = GetValueBox(sectionBounds);
            return new Rectangle(value.Right + 3, value.Y, 24, value.Height);
        }

        private static Rectangle GetListViewport(Rectangle panelBounds)
        {
            return new Rectangle(
                panelBounds.X + 7,
                panelBounds.Y + ListViewportY,
                panelBounds.Width - 14,
                ListViewportHeight);
        }

        private static void DrawButton(SpriteBatch spriteBatch, Rectangle bounds, string text, bool hovered)
        {
            DrawRectangle(spriteBatch, bounds, hovered ? ButtonHover : ButtonBackground, PanelBorder);
            DrawCenteredText(spriteBatch, text, bounds, LabelColor, 0.72f);
        }

        private static void DrawCenteredText(SpriteBatch spriteBatch, string text, Rectangle bounds, Color color, float scale)
        {
            Vector2 textSize = FontAssets.MouseText.Value.MeasureString(text) * scale;
            Vector2 textPosition = new Vector2(
                bounds.X + (bounds.Width - textSize.X) * 0.5f,
                bounds.Y + (bounds.Height - textSize.Y) * 0.5f);
            Utils.DrawBorderString(spriteBatch, text, textPosition, color, scale);
        }

        private static void DrawRectangle(SpriteBatch spriteBatch, Rectangle bounds, Color fill, Color border)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            spriteBatch.Draw(pixel, bounds, fill);
            if (border == Color.Transparent)
                return;

            spriteBatch.Draw(pixel, new Rectangle(bounds.X, bounds.Y, bounds.Width, 1), border);
            spriteBatch.Draw(pixel, new Rectangle(bounds.X, bounds.Bottom - 1, bounds.Width, 1), border);
            spriteBatch.Draw(pixel, new Rectangle(bounds.X, bounds.Y, 1, bounds.Height), border);
            spriteBatch.Draw(pixel, new Rectangle(bounds.Right - 1, bounds.Y, 1, bounds.Height), border);
        }
    }
}
