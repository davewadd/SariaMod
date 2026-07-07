using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI;

namespace SariaMod.Diagnostics
{
    /// <summary>
    /// Right-side toggle button + last-packet inspector panel.
    /// Press the "Net" button to open/close. When open it shows every field
    /// of the most recently received packet, how many bytes each field used,
    /// and the packet total received byte count.
    /// Only visible in dev builds (gated by NetworkProfilerUISystem.DebugEnabled).
    /// Open state persisted via FairyPlayer.NetworkProfilerOpen.
    /// </summary>
    public class NetworkProfilerUIPanel : UIState
    {
        private const int ButtonWidth       = 90;
        private const int ButtonHeight      = 26;
        private const int ButtonMarginRight = 8;
        private const int PanelWidth        = 340;
        private const int PanelMarginRight  = 8;
        private const int RowHeight         = 18;
        private const int LabelX            = 10;

        private static readonly Color PanelBg      = new Color(20,  25,  35,  230);
        private static readonly Color PanelBorder   = new Color(70,  90,  110, 255);
        private static readonly Color ButtonBg      = new Color(40,  50,  65,  230);
        private static readonly Color ButtonBgHover = new Color(60,  75,  95,  240);
        private static readonly Color ButtonText    = new Color(200, 220, 240);
        private static readonly Color HeaderColor   = new Color(150, 200, 255);
        private static readonly Color RowName       = new Color(220, 230, 240);
        private static readonly Color ValueColor    = new Color(140, 220, 140);
        private static readonly Color BytesColor    = new Color(255, 200, 120);
        private static readonly Color DimText       = new Color(140, 150, 170);

        private bool PanelOpen
        {
            get => Main.LocalPlayer.GetModPlayer<FairyPlayer>().NetworkProfilerOpen;
            set => Main.LocalPlayer.GetModPlayer<FairyPlayer>().NetworkProfilerOpen = value;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!NetworkProfilerUISystem.DebugEnabled)
                return;

            DrawToggleButton(spriteBatch);

            if (PanelOpen)
                DrawPanel(spriteBatch);
        }

        private void DrawToggleButton(SpriteBatch spriteBatch)
        {
            int x = Main.screenWidth - ButtonWidth - ButtonMarginRight;
            int y = (Main.screenHeight - ButtonHeight) / 2;
            Rectangle btnRect = new Rectangle(x, y, ButtonWidth, ButtonHeight);

            bool hover = btnRect.Contains(Main.mouseX, Main.mouseY);
            if (hover && Main.mouseLeft && Main.mouseLeftRelease)
            {
                PanelOpen = !PanelOpen;
                Main.mouseLeftRelease = false;
            }

            DrawRect(spriteBatch, btnRect, hover ? ButtonBgHover : ButtonBg, PanelBorder);

            string label    = PanelOpen ? "Net [x]" : "Net";
            Vector2 tsz     = FontAssets.MouseText.Value.MeasureString(label) * 0.75f;
            Vector2 textPos = new Vector2(x + (ButtonWidth - tsz.X) / 2f, y + (ButtonHeight - tsz.Y) / 2f);
            Utils.DrawBorderString(spriteBatch, label, textPos, ButtonText, 0.75f);

            if (hover)
                Main.LocalPlayer.mouseInterface = true;
        }

        private void DrawPanel(SpriteBatch spriteBatch)
        {
            var rxFields = LastPacketRecord.Fields;
            var txFields = LastSentPacketRecord.Fields;
            int rxRows   = Math.Max(rxFields.Count, 1);
            int txRows   = Math.Max(txFields.Count, 1);

            // RX section: title + subtitle + col-header + rows + footer
            int rxHeight = RowHeight * 2 + RowHeight + rxRows * RowHeight + RowHeight;
            // TX section: same layout
            int txHeight = RowHeight * 2 + RowHeight + txRows * RowHeight + RowHeight;
            // gap between sections
            const int GapHeight = 10;

            int panelHeight = 10 + rxHeight + GapHeight + txHeight + 6;

            int btnX = Main.screenWidth - ButtonWidth - ButtonMarginRight;
            int x    = btnX - PanelMarginRight - PanelWidth;
            int y    = (Main.screenHeight - panelHeight) / 2;
            Rectangle panelRect = new Rectangle(x, y, PanelWidth, panelHeight);

            if (panelRect.Contains(Main.mouseX, Main.mouseY))
                Main.LocalPlayer.mouseInterface = true;

            DrawRect(spriteBatch, panelRect, PanelBg, PanelBorder);

            int curY = y + 6;

            // ?? RX Section ??????????????????????????????????????????????
            curY = DrawPacketSection(spriteBatch, x, curY,
                "Last Received Packet",
                LastPacketRecord.PacketName, LastPacketRecord.TotalBytes,
                rxFields, "received");

            // Visual gap + divider line
            curY += GapHeight / 2;
            DrawRect(spriteBatch, new Rectangle(x + LabelX, curY, PanelWidth - LabelX * 2, 1),
                Color.Transparent, new Color(70, 90, 110, 180));
            curY += GapHeight / 2;

            // ?? TX Section ??????????????????????????????????????????????
            DrawPacketSection(spriteBatch, x, curY,
                "Last Sent Packet",
                LastSentPacketRecord.PacketName, LastSentPacketRecord.TotalBytes,
                txFields, "sent");
        }

        private int DrawPacketSection(
            SpriteBatch spriteBatch, int x, int startY,
            string title, string packetName, int totalBytes,
            System.Collections.Generic.List<PacketField> fields,
            string noneLabel)
        {
            int curY = startY;

            Utils.DrawBorderString(spriteBatch, title,
                new Vector2(x + LabelX, curY), HeaderColor, 0.82f);
            curY += RowHeight;

            Utils.DrawBorderString(spriteBatch,
                "Packet: " + packetName + "   Total: " + totalBytes + " B",
                new Vector2(x + LabelX, curY), DimText, 0.7f);
            curY += RowHeight;

            // Column headers
            Utils.DrawBorderString(spriteBatch, "Field", new Vector2(x + LabelX, curY), HeaderColor, 0.7f);
            Utils.DrawBorderString(spriteBatch, "Value", new Vector2(x + 160,    curY), HeaderColor, 0.7f);
            Utils.DrawBorderString(spriteBatch, "Bytes", new Vector2(x + 295,    curY), HeaderColor, 0.7f);
            curY += RowHeight;

            if (fields.Count == 0)
            {
                Utils.DrawBorderString(spriteBatch, "(no packet " + noneLabel + " yet)",
                    new Vector2(x + LabelX, curY), DimText, 0.7f);
                curY += RowHeight;
            }
            else
            {
                foreach (var field in fields)
                {
                    Utils.DrawBorderString(spriteBatch, field.Name,
                        new Vector2(x + LabelX, curY), RowName,    0.7f);
                    Utils.DrawBorderString(spriteBatch, field.Value,
                        new Vector2(x + 160,    curY), ValueColor, 0.7f);
                    Utils.DrawBorderString(spriteBatch, field.Bytes + " B",
                        new Vector2(x + 295,    curY), BytesColor, 0.7f);
                    curY += RowHeight;
                }
            }

            Utils.DrawBorderString(spriteBatch,
                "Total " + noneLabel + " bytes: " + totalBytes,
                new Vector2(x + LabelX, curY + 2), DimText, 0.68f);
            curY += RowHeight;

            return curY;
        }

        private void DrawRect(SpriteBatch spriteBatch, Rectangle rect, Color fill, Color border)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            spriteBatch.Draw(pixel, rect, fill);
            if (border != Color.Transparent)
            {
                spriteBatch.Draw(pixel, new Rectangle(rect.X,                  rect.Y,                   rect.Width, 1),           border);
                spriteBatch.Draw(pixel, new Rectangle(rect.X,                  rect.Y + rect.Height - 1, rect.Width, 1),           border);
                spriteBatch.Draw(pixel, new Rectangle(rect.X,                  rect.Y,                   1,          rect.Height), border);
                spriteBatch.Draw(pixel, new Rectangle(rect.X + rect.Width - 1, rect.Y,                   1,          rect.Height), border);
            }
        }
    }
}
