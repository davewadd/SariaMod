using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI;
using SariaMod.Items.Strange;

namespace SariaMod.Items.zTalking
{
    /// <summary>
    /// Draggable meter bar that shows Saria's sneeze cooldown as a 0–100% fill.
    /// Visible only while the dialogue UI is open.
    ///
    /// MeterBar.png is a 3-frame vertical spritesheet:
    ///   Frame 0 (top)    — decorative border drawn last (on top of everything)
    ///   Frame 1 (middle) — underlay drawn first (background)
    ///   Frame 2 (bottom) — 1-pixel-wide red sliver; 100 copies fill the bar
    ///
    /// BarFillLeft / BarFillTop: pixel offset inside the image where the
    /// fill region starts.  Adjust these to align with the interior of the
    /// frame art if needed.
    /// </summary>
    public class MeterBarUIState : UIState
    {
        private const string TexPath = "SariaMod/Items/zTalking/MeterBar";

        // ── Scale: 2x renders the entire bar at double size ────────────────
        private const float Scale = 1f;
        // ────────────────────────────────────────────────────────────────────

        // ── Adjustable: where inside the image the fill region starts ──────
        private const int BarFillLeft = 0;
        private const int BarFillTop  = 0;
        // ────────────────────────────────────────────────────────────────────

        // Tooltip label shown on hover
        private const string TooltipLabel    = "Saria's Condition";
        private const string TooltipSubLabel = "";

        // Default screen-centre offset (pixels, unscaled)
        private static Vector2 _savedPos = new Vector2(0f, 250f);

        private Vector2 _currentPos;
        private bool    _isDragging;
        private Vector2 _dragStartMouse;
        private Vector2 _dragStartPanel;
        private bool    _prevMouseDown;
        private bool    _isHovered;

        public override void OnInitialize()
        {
            if (Main.LocalPlayer?.ModPlayers.Length > 0)
            {
                var fp = Main.LocalPlayer.GetModPlayer<FairyPlayer>();
                if (fp != null)
                    _savedPos = new Vector2(fp.MeterBarPosX, fp.MeterBarPosY);
            }
            _currentPos = _savedPos;
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private static Texture2D GetTex() =>
            ModContent.Request<Texture2D>(TexPath, AssetRequestMode.ImmediateLoad).Value;

        /// <summary>Returns the top-left pixel position of the panel on screen, clamped to screen bounds.</summary>
        private Vector2 GetPanelTopLeft(Texture2D tex)
        {
            int frameH = tex.Height / 3;
            Vector2 center = new Vector2(Main.screenWidth, Main.screenHeight) / 2f;
            Vector2 topLeft = center + _currentPos - new Vector2(tex.Width * Scale / 2f, frameH * Scale / 2f);
            // Clamp so the bar never goes off-screen
            topLeft.X = Math.Clamp(topLeft.X, 0f, Main.screenWidth  - tex.Width  * Scale);
            topLeft.Y = Math.Clamp(topLeft.Y, 0f, Main.screenHeight - frameH     * Scale);
            return topLeft;
        }

        // ── Sickness bar data ─────────────────────────────────────────────────

        private const float SicknessBarMax = 12000f;

        private static float GetSicknessData()
        {
            int sariaType = ModContent.ProjectileType<Saria>();
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];
                if (!proj.active || proj.owner != Main.myPlayer || proj.type != sariaType)
                    continue;

                if (proj.ModProjectile is Saria saria)
                    return saria.SicknessBar / SicknessBarMax;
            }
            return 1f; // default full when Saria not present
        }

        private static int GetSicknessDecayChange()
        {
            int sariaType = ModContent.ProjectileType<Saria>();
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];
                if (!proj.active || proj.owner != Main.myPlayer || proj.type != sariaType)
                    continue;

                if (proj.ModProjectile is Saria saria)
                    return saria.SicknessDecayChange;
            }
            return 0;
        }

        // ── Update (dragging + hover) ─────────────────────────────────────────

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // Sync position from player data when not dragging (handles player switching)
            if (!_isDragging)
            {
                var fp = Main.LocalPlayer?.GetModPlayer<FairyPlayer>();
                if (fp != null)
                    _currentPos = new Vector2(fp.MeterBarPosX, fp.MeterBarPosY);
            }

            Texture2D tex     = GetTex();
            int       frameH  = tex.Height / 3;
            Vector2   topLeft = GetPanelTopLeft(tex);
            Rectangle rect    = new Rectangle((int)topLeft.X, (int)topLeft.Y, (int)(tex.Width * Scale), (int)(frameH * Scale));
            Point     mouse   = new Point(Main.mouseX, Main.mouseY);

            bool mouseDown      = Main.mouseLeft;
            bool mouseJustDown  = mouseDown && !_prevMouseDown;

            _isHovered = rect.Contains(mouse);

            // Start drag
            if (mouseJustDown && _isHovered)
            {
                _isDragging     = true;
                _dragStartMouse = new Vector2(Main.mouseX, Main.mouseY);
                _dragStartPanel = _currentPos;
                Main.blockMouse = true;
            }

            // Continue drag
            if (_isDragging && mouseDown)
            {
                Vector2 delta = new Vector2(Main.mouseX, Main.mouseY) - _dragStartMouse;
                _currentPos  = _dragStartPanel + delta;
                _savedPos    = _currentPos;
                Main.blockMouse = true;
            }

            // Release drag — clamp saved position to screen
            if (!mouseDown)
            {
                if (_isDragging)
                {
                    Texture2D clampTex  = GetTex();
                    int clampFrameH     = clampTex.Height / 3;
                    Vector2 clampCenter = new Vector2(Main.screenWidth, Main.screenHeight) / 2f;
                    Vector2 clampTL     = clampCenter + _currentPos - new Vector2(clampTex.Width * Scale / 2f, clampFrameH * Scale / 2f);
                    clampTL.X   = Math.Clamp(clampTL.X, 0f, Main.screenWidth  - clampTex.Width  * Scale);
                    clampTL.Y   = Math.Clamp(clampTL.Y, 0f, Main.screenHeight - clampFrameH     * Scale);
                    _currentPos = clampTL - clampCenter + new Vector2(clampTex.Width * Scale / 2f, clampFrameH * Scale / 2f);
                    _savedPos   = _currentPos;

                    // Persist to player so the position survives game close
                    var fp = Main.LocalPlayer?.GetModPlayer<FairyPlayer>();
                    if (fp != null)
                    {
                        fp.MeterBarPosX = _savedPos.X;
                        fp.MeterBarPosY = _savedPos.Y;
                    }
                }
                _isDragging = false;
            }

            _prevMouseDown = mouseDown;
        }

        // ── Draw ─────────────────────────────────────────────────────────────

        protected override void DrawSelf(SpriteBatch sb)
        {
            Texture2D tex    = GetTex();
            int       frameH = tex.Height / 3;
            int       frameW = tex.Width;

            // Source rectangles for each frame
            var underlayRect = new Rectangle(0, frameH,     frameW, frameH); // frame 1 — underlay
            var borderRect   = new Rectangle(0, 0,           frameW, frameH); // frame 0 — border
            // frame 2 — full-width fill bar; we clip its source width to fill%

            Vector2 origin = GetPanelTopLeft(tex);

            // Compute fill percentage from SicknessBar
            float fill    = MathHelper.Clamp(GetSicknessData(), 0f, 1f);
            int   percent = (int)Math.Round(fill * 100f);
            // Lerp fill color from red (empty) → yellow (half) → green (full)
            Color fillColor = fill < 0.5f
                ? Color.Lerp(Color.Red, Color.Yellow, fill * 2f)
                : Color.Lerp(Color.Yellow, Color.LimeGreen, (fill - 0.5f) * 2f);

            // 1. Underlay (background) — drawn first, behind everything
            sb.Draw(tex, origin, underlayRect, Color.White, 0f, Vector2.Zero, Scale, SpriteEffects.None, 0f);

            // 2. Decorative border — drawn over underlay, under fill
            sb.Draw(tex, origin, borderRect, Color.White, 0f, Vector2.Zero, Scale, SpriteEffects.None, 0f);

            // 3. Fill — frame 2 is a 2px-wide seed at (14,14) sized (2×12).
            //    Stretch it across the underlay interior: X=16..211 (195px wide), Y offset=14, height=12.
            if (percent > 0)
            {
                int   totalFillW = 195;
                int   fillW      = (int)(totalFillW * fill);
                var   fillSrc    = new Rectangle(14, frameH * 2 + 14, 2, 12);
                var   fillDest   = new Rectangle((int)(origin.X + 16), (int)(origin.Y + 14), fillW, 12);
                sb.Draw(tex, fillDest, fillSrc, fillColor);
            }

            // 4. Percentage text centred over the whole panel
            DynamicSpriteFont font     = FontAssets.MouseText.Value;
            string            text     = $"{percent}%";
            Vector2           textSize = font.MeasureString(text);
            Vector2           textPos  = origin + new Vector2(frameW * Scale / 2f, frameH * Scale / 2f) - textSize / 2f;
            sb.DrawString(font, text, textPos, Color.White);

            // 5. Hover tooltip — show label and current decay/recovery rate
            if (_isHovered && !_isDragging)
            {
                int   change    = GetSicknessDecayChange();
                string rateStr  = change >= 0 ? $"+{change} per 6s" : $"{change} per 6s";
                Main.hoverItemName = $"{TooltipLabel}\n{rateStr}";
            }
        }
    }
}
