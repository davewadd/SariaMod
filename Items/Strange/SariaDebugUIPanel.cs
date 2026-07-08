using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using Terraria;
using Terraria.GameInput;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI;
using SariaMod.Items.zTalking;

namespace SariaMod.Items.Strange
{
    /// <summary>
    /// Debug UI panel that shows Saria's internal state values.
    /// Single scrollable panel — mouse-wheel or drag the scrollbar to scroll.
    /// Panel open state persists via FairyPlayer.DebugPanelOpen.
    /// </summary>
    public class SariaDebugUIPanel : UIState
    {

        // ── Upgrade toggle labels (indices 1–24, fixed mapping) ──────────────────
        private static readonly (int Index, string Label)[] UpgradeLabels =
        {
            (1,  "Base Upg 1"),   (2,  "Base Upg 2"),   (3,  "Base Upg 3"),
            (4,  "Psychic Upg 1"), (5,  "Psychic Upg 2"), (6,  "Psychic Upg 3"),
            (7,  "Water Upg 1"),   (8,  "Water Upg 2"),   (9,  "Water Upg 3"),
            (10, "Fire Upg 1"),    (11, "Fire Upg 2"),    (12, "Fire Upg 3"),
            (13, "Elec Upg 1"),    (14, "Elec Upg 2"),    (15, "Elec Upg 3"),
            (16, "Rock Upg 1"),    (17, "Rock Upg 2"),    (18, "Rock Upg 3"),
            (19, "Bug Upg 1"),     (20, "Bug Upg 2"),     (21, "Bug Upg 3"),
            (22, "Ghost Upg 1"),   (23, "Ghost Upg 2"),   (24, "Ghost Upg 3"),
        };
        // ── Toggle / eye button constants ─────────────────────────────────────────
        private const int ButtonWidth        = 80;
        private const int ButtonHeight       = 26;
        private const int ButtonMarginLeft   = 16;
        private const int ButtonMarginBottom = 50;
        private const int EyeButtonWidth      = 40;
        private const int EyeButtonMarginLeft = 16 + 80 + 6;

        // ── Panel geometry ────────────────────────────────────────────────────────
        private const int PanelWidth        = 260;
        private const int PanelHeight       = 696;  // capped visible height
        private const int PanelMarginLeft   = 16;
        private const int PanelMarginBottom = 84;
        private const int ContentStartY     = 30;   // pixels below panel top where content begins

        // Measured content height (FirstRowY=4 + all rows/seps/padding):
        //   16 stat rows×26 + dist row(30) + sep(9) + 6 probe rows×26 + det-right(30)
        //   + sep(9) + label(26)
        //   + 13 biome rows×26 + meteor(28) + sep(9) + 7 depth rows×26 + rain(28)
        //   + sep(9) + 7 candle rows×26 + bottom pad(12) = ~1385
        //   Round up so MaxScroll = ContentTotalH - VisibleH is always enough.
        //   +52 for the two region trackers (Near Player / Near Saria).
        //   +52 for the two deep spawn-cap diagnostic rows (Global NPCs / Fed(P/S)).
        private const int ContentTotalH = 3072;

        // ── Row layout ────────────────────────────────────────────────────────────
        private const int RowHeight = 26;
        private const int LabelX    = 12;
        private const int ValueX    = 170;
        private const int FirstRowY = 4;

        // ── Scrollbar ─────────────────────────────────────────────────────────────
        private const int ScrollBarWidth = 8;
        private const int ScrollBarPad   = 2;
        private static readonly Color ScrollTrackColor = new Color(20, 20, 25, 180);
        private static readonly Color ScrollThumbColor = new Color(100, 110, 130, 220);
        private static readonly Color ScrollThumbHover = new Color(140, 160, 200, 240);

        private float _scrollY             = 0f;
        private bool  _scrollDragging      = false;
        private float _scrollDragStartY    = 0f;
        private float _scrollDragStartVal  = 0f;

        // ── Colors ────────────────────────────────────────────────────────────────
        private static readonly Color PanelBg             = new Color(30, 30, 35, 220);
        private static readonly Color PanelBorder         = new Color(70, 70, 80, 255);
        private static readonly Color ButtonBg            = new Color(50, 50, 58, 230);
        private static readonly Color ButtonBgHover       = new Color(70, 70, 82, 240);
        private static readonly Color ButtonText          = new Color(200, 200, 200);
        private static readonly Color FrameColor          = new Color(120, 200, 255);
        private static readonly Color EatingColor         = new Color(255, 200, 100);
        private static readonly Color SleepColor          = new Color(160, 130, 255);
        private static readonly Color CanMoveColor        = new Color(100, 255, 140);
        private static readonly Color SneezeColor         = new Color(255, 140, 140);
        private static readonly Color MoveTimerColor      = new Color(200, 220, 130);
        private static readonly Color MoodColor           = new Color(255, 180, 220);
        private static readonly Color ChannelTimeColor    = new Color(255, 160, 80);
        private static readonly Color ChannelStateColor   = new Color(80, 200, 255);
        private static readonly Color IsChargingColor     = new Color(255, 220, 60);
        private static readonly Color BloodSneezeColor    = new Color(200, 60, 60);
        private static readonly Color SneezeBiomeRateColor= new Color(255, 100, 100);
        private static readonly Color CantAttackColor     = new Color(255, 120, 160);
        private static readonly Color SicknessBarColor    = new Color(100, 220, 180);
        private static readonly Color PeriodTimerColor    = new Color(180, 140, 255);
        private static readonly Color DecayChangeColor    = new Color(255, 200, 80);
        private static readonly Color ValueColor          = new Color(240, 240, 240);
        private static readonly Color ProbeActiveColor    = new Color(100, 255, 140);
        private static readonly Color ProbeInactiveColor  = new Color(180, 180, 180);
        private static readonly Color ProbeWallColor      = new Color(255, 160, 80);
        private static readonly Color FollowColor         = new Color(180, 80, 255);
        private static readonly Color CursedColor         = new Color(140, 60, 200);
        private static Color DialogueActiveColor   => SariaText.ForesightMint;
        private static Color DialogueInactiveColor => SariaText.StalfosGrey;
        private static Color DialogueTimerColor    => SariaText.ZoraBlue;
        private static Color DialogueSelectorColor => SariaText.RupeeViolet;

        // ── SariaText palette aliases ─────────────────────────────────────────────
        private static Color ST_KokiriGreen    => SariaText.KokiriGreen;
        private static Color ST_ZoraBlue       => SariaText.ZoraBlue;
        private static Color ST_GerudoOrange   => SariaText.GerudoOrange;
        private static Color ST_ThunderGold    => SariaText.ThunderGold;
        private static Color ST_RupeeViolet    => SariaText.RupeeViolet;
        private static Color ST_LurantisPink   => SariaText.LurantisPink;
        private static Color ST_PoeLavender    => SariaText.PoeLavender;
        private static Color ST_ForesightMint  => SariaText.ForesightMint;
        private static Color ST_InfoCyan       => SariaText.InfoCyan;
        private static Color ST_PureWhite      => SariaText.PureWhite;
        private static Color ST_StalfosGrey    => SariaText.StalfosGrey;
        private static Color ST_SkullkidRed    => SariaText.SkullkidRed;

        // ── Helpers ───────────────────────────────────────────────────────────────
        private int   PanelTop  => Main.screenHeight - PanelMarginBottom - PanelHeight;
        private int   VisibleH  => PanelHeight - ContentStartY;
        private float MaxScroll => Math.Max(0f, ContentTotalH - VisibleH);

        private bool PanelOpen
        {
            get => Main.LocalPlayer.GetModPlayer<FairyPlayer>().DebugPanelOpen;
            set => Main.LocalPlayer.GetModPlayer<FairyPlayer>().DebugPanelOpen = value;
        }

        // ═════════════════════════════════════════════════════════════════════════
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!SariaDebugUISystem.DebugEnabled)
                return;

            if (Main.keyState.IsKeyDown(Keys.X) && !Main.oldKeyState.IsKeyDown(Keys.X))
                PanelOpen = !PanelOpen;

            DrawEyeButton(spriteBatch);

            Player player = Main.LocalPlayer;
            if (player.ownedProjectileCounts[ModContent.ProjectileType<Saria>()] <= 0)
                return;

            Saria sariaProj = null;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.type == ModContent.ProjectileType<Saria>() && p.owner == player.whoAmI)
                {
                    sariaProj = p.ModProjectile as Saria;
                    break;
                }
            }

            DrawToggleButton(spriteBatch);

            if (!PanelOpen || sariaProj == null)
                return;

            int panelX = PanelMarginLeft;
            int panelY = PanelTop;
            Rectangle panelRect = new Rectangle(panelX, panelY, PanelWidth, PanelHeight);

            // ── Mouse-wheel scroll ────────────────────────────────────────────────
            if (panelRect.Contains(Main.mouseX, Main.mouseY))
            {
                int wheel = PlayerInput.ScrollWheelDelta;
                if (wheel != 0)
                    _scrollY = Math.Clamp(_scrollY - Math.Sign(wheel) * RowHeight * 3, 0, MaxScroll);
                Main.LocalPlayer.mouseInterface = true;
            }

            DrawPanel(spriteBatch, sariaProj);
        }

        // ═════════════════════════════════════════════════════════════════════════
        private void DrawPanel(SpriteBatch spriteBatch, Saria saria)
        {
            int panelX = PanelMarginLeft;
            int panelY = PanelTop;
            Rectangle panelRect = new Rectangle(panelX, panelY, PanelWidth, PanelHeight);

            // ── Panel shell (always visible, outside scissor) ─────────────────────
            DrawRect(spriteBatch, panelRect, PanelBg, PanelBorder);

            string title = "Saria Debug";
            Vector2 titleSize = FontAssets.MouseText.Value.MeasureString(title) * 0.8f;
            Utils.DrawBorderString(spriteBatch, title,
                new Vector2(panelX + (PanelWidth - titleSize.X) / 2f, panelY + 2f),
                new Color(180, 180, 180), 0.8f);
            DrawRect(spriteBatch, new Rectangle(panelX + 4, panelY + 22, PanelWidth - 8, 1),
                     PanelBorder, Color.Transparent);

            // ── Scrollbar geometry ────────────────────────────────────────────────
            int trackX = panelX + PanelWidth - ScrollBarWidth - ScrollBarPad;
            int trackY = panelY + ContentStartY;
            int trackH = VisibleH;
            int thumbH = Math.Max(20, (int)((float)trackH / ContentTotalH * trackH));
            float thumbFrac = MaxScroll > 0 ? _scrollY / MaxScroll : 0f;
            int thumbY = trackY + (int)(thumbFrac * (trackH - thumbH));

            Rectangle trackRect = new Rectangle(trackX, trackY, ScrollBarWidth, trackH);
            Rectangle thumbRect = new Rectangle(trackX, thumbY, ScrollBarWidth, thumbH);
            bool thumbHovered   = thumbRect.Contains(Main.mouseX, Main.mouseY);

            // Scrollbar drag
            if (thumbHovered && Main.mouseLeft && !_scrollDragging)
            {
                _scrollDragging     = true;
                _scrollDragStartY   = Main.mouseY;
                _scrollDragStartVal = _scrollY;
            }
            if (_scrollDragging)
            {
                if (Main.mouseLeft)
                {
                    float scrollPerPixel = MaxScroll / Math.Max(1f, trackH - thumbH);
                    _scrollY = Math.Clamp(
                        _scrollDragStartVal + (Main.mouseY - _scrollDragStartY) * scrollPerPixel,
                        0, MaxScroll);
                    Main.LocalPlayer.mouseInterface = true;
                }
                else _scrollDragging = false;
            }

            // ── Begin scissored content region ────────────────────────────────────
            // Map the content rect from UI space → device pixel space.
            Vector2 tl = Vector2.Transform(new Vector2(panelX, panelY + ContentStartY), Main.UIScaleMatrix);
            Vector2 br = Vector2.Transform(new Vector2(panelX + PanelWidth, panelY + PanelHeight), Main.UIScaleMatrix);
            Rectangle deviceClip = new Rectangle(
                (int)tl.X, (int)tl.Y,
                Math.Max(1, (int)(br.X - tl.X)),
                Math.Max(1, (int)(br.Y - tl.Y)));

            Rectangle prevScissor = spriteBatch.GraphicsDevice.ScissorRectangle;
            spriteBatch.End();
            spriteBatch.GraphicsDevice.ScissorRectangle = deviceClip;
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                SamplerState.LinearClamp, null,
                new RasterizerState { ScissorTestEnable = true },
                null, Main.UIScaleMatrix);

            // ── All rows (shifted up by _scrollY) ────────────────────────────────
            int scroll = (int)_scrollY;
            int x      = panelX;
            int rowY   = panelY + ContentStartY + FirstRowY - scroll;
            Projectile proj = saria.Projectile;

            // Stats
            DrawStatRow(spriteBatch, x, rowY, "Frame",           proj.frame.ToString(),                   FrameColor);          rowY += RowHeight;
            DrawStatRow(spriteBatch, x, rowY, "Eating",          saria.Eating.ToString(),                 EatingColor);         rowY += RowHeight;
            DrawStatRow(spriteBatch, x, rowY, "Sleep",           saria.Sleep ? "Yes" : "No",              SleepColor);          rowY += RowHeight;
            DrawStatRow(spriteBatch, x, rowY, "CanMove",         saria.CanMove.ToString(),                CanMoveColor);        rowY += RowHeight;
            DrawStatRow(spriteBatch, x, rowY, "MoveTimer",       saria.MoveTimerValue.ToString(),         MoveTimerColor);      rowY += RowHeight;

            bool sneezeQueued = saria.IdleAnimator.IsSneezeQueued;
            string sneezeText = sneezeQueued
                ? "QUEUED"
                : $"{saria.IdleAnimator.SneezeTimer:F0} / {saria.IdleAnimator.SneezeThreshold:F0}";
            DrawStatRow(spriteBatch, x, rowY, "Sneeze",          sneezeText,                              SneezeColor);         rowY += RowHeight;

            DrawStatRow(spriteBatch, x, rowY, "BloodSneeze",     saria.BloodSneezeValue ? "Yes" : "No",  BloodSneezeColor);    rowY += RowHeight;
            DrawStatRow(spriteBatch, x, rowY, "SneezeBiomeRate", saria.IdleAnimator.SneezeBiomeRate.ToString("F2"), SneezeBiomeRateColor); rowY += RowHeight;
            DrawStatRow(spriteBatch, x, rowY, "Mood",            saria.Mood.ToString(),                   MoodColor);           rowY += RowHeight;
            DrawStatRow(spriteBatch, x, rowY, "Cursed",          saria.CursedValue ? "Yes" : "No",        saria.CursedValue ? CursedColor : new Color(140, 140, 140)); rowY += RowHeight;
            DrawStatRow(spriteBatch, x, rowY, "ChannelTime",     saria.ChannelTime.ToString(),            ChannelTimeColor);    rowY += RowHeight;
            DrawStatRow(spriteBatch, x, rowY, "ChannelState",    saria.ChannelState.ToString(),           ChannelStateColor);   rowY += RowHeight;
            DrawStatRow(spriteBatch, x, rowY, "IsCharging",      saria.IsCharging.ToString(),             IsChargingColor);     rowY += RowHeight;
            DrawStatRow(spriteBatch, x, rowY, "CantAttackTimer", saria.CantAttackTimer.ToString(),        CantAttackColor);     rowY += RowHeight;
            DrawStatRow(spriteBatch, x, rowY, "SicknessBar",     $"{saria.SicknessBar} / 12000",          SicknessBarColor);    rowY += RowHeight;
            DrawStatRow(spriteBatch, x, rowY, "PeriodTimer",     saria.PeriodTimerValue.ToString(),       PeriodTimerColor);    rowY += RowHeight;
            string decaySign = saria.SicknessDecayChange >= 0 ? "+" : "";
            DrawStatRow(spriteBatch, x, rowY, "DecayRate",       $"{decaySign}{saria.SicknessDecayChange}/6s", DecayChangeColor); rowY += RowHeight;
            float dist = Vector2.Distance(proj.Center, Main.player[proj.owner].Center);
            DrawStatRow(spriteBatch, x, rowY, "Dist to Player",  $"{dist:F1}",                            new Color(180, 220, 255)); rowY += RowHeight;

            // Region entity trackers. Attribution here is IDENTICAL to the gate's
            // (CountRegionDisplay = CountRegionSlots rules): vanilla's cap predicate,
            // box membership, nearest-anchor tiebreak in the overlap — so these rows
            // finally agree with what the split actually counts. Each value shows
            // "slots (bodies)": vanilla's cap math runs on npcSlots, and many enemies
            // weigh 0.5/0.75 slots, so 6 visible enemies can legitimately read 5.0 —
            // that's vanilla semantics, not an accounting bug. In multiplayer the
            // caps/fed/global values are pushed from the SERVER (where SpawnNPC and the
            // gate actually run) via SyncSpawnDebug ~1/s; while that feed is fresh the
            // synced values are shown, otherwise the local fallback (single player, or
            // LinkCable off long enough for the feed to lapse).
            int sariaCap   = SariaSpawnSystem.LastSariaCap;
            int playerCap  = SariaSpawnSystem.LastPlayerCap;
            bool serverFed = SariaSpawnSystem.ServerDebugFresh;
            // Caps/fed/merged live where the gate actually runs. In single player that is
            // THIS process, so the statics are always current. On a multiplayer CLIENT
            // they are only ever written by SyncSpawnDebug packets — once that feed
            // lapses (LinkCable off, split idle server-side) the statics just hold
            // whatever the last packet said, silently drifting out of date while events
            // (full moon, sandstorm...) move the real cap. That stale-but-confident
            // display is exactly the "cap doesn't always reflect accurately" report:
            // show "?" instead of a number we cannot vouch for.
            bool capsKnown = Main.netMode != Terraria.ID.NetmodeID.MultiplayerClient || serverFed;
            SariaSpawnSystem.CountRegionDisplay(Main.player[proj.owner].Center, proj.Center,
                out float nearPlayer, out int nearPlayerBodies, out float nearSaria, out int nearSariaBodies);
            if (serverFed)
            {
                // Region slot totals come from the server's own gate computation; the
                // local body tally still gives the "how many do I actually see" number.
                nearPlayer = SariaSpawnSystem.LastPlayerRegionSlots;
                nearSaria  = SariaSpawnSystem.LastSariaRegionSlots;
            }
            // Merged mode: owner and Saria are close enough that the gate runs ONE
            // shared budget (population = both attributions summed, target = larger
            // cap). Present the same math the gate uses so the rows agree with it.
            // Only honored while the merged flag itself is trustworthy (capsKnown) —
            // a stale merged=true from a lapsed feed must not restyle the rows.
            if (SariaSpawnSystem.LastRegionsMerged && capsKnown)
            {
                float mergedPop = nearPlayer + nearSaria;
                int mergedBodies = nearPlayerBodies + nearSariaBodies;
                int mergedCap = Math.Max(playerCap, sariaCap);
                DrawStatRow(spriteBatch, x, rowY, "Near Both",
                    $"{mergedPop:F1} ({mergedBodies}) / {mergedCap}",
                    mergedPop >= mergedCap ? new Color(255, 140, 120) : new Color(200, 190, 255));         rowY += RowHeight;
                DrawStatRow(spriteBatch, x, rowY, "  (merged)",
                    $"P {nearPlayer:F1} + S {nearSaria:F1}",
                    new Color(160, 150, 210));                                                             rowY += RowHeight;
            }
            else
            {
                DrawStatRow(spriteBatch, x, rowY, "Near Player",
                    $"{nearPlayer:F1} ({nearPlayerBodies}) / {(capsKnown ? playerCap.ToString() : "?")}",
                    capsKnown && nearPlayer >= playerCap ? new Color(255, 140, 120) : new Color(180, 220, 255));  rowY += RowHeight;
                DrawStatRow(spriteBatch, x, rowY, "Near Saria",
                    $"{nearSaria:F1} ({nearSariaBodies}) / {(capsKnown ? sariaCap.ToString() : "?")}",
                    capsKnown && nearSaria >= sariaCap ? new Color(255, 140, 120) : new Color(140, 255, 180));    rowY += RowHeight;
            }
            float globalSlots = serverFed ? SariaSpawnSystem.LastGlobalSlotCount : SariaSpawnSystem.CountAllSlots();
            DrawStatRow(spriteBatch, x, rowY, "Global NPCs",
                $"{globalSlots:F1} / {Main.maxNPCs}",
                new Color(210, 210, 140));                                                                 rowY += RowHeight;
            DrawStatRow(spriteBatch, x, rowY, "Fed(P/S)",
                capsKnown
                    ? $"{SariaSpawnSystem.LastPlayerFedMaxSpawns} / {SariaSpawnSystem.LastSariaFedMaxSpawns}{(serverFed ? " *srv" : "")}"
                    : "? / ?",
                new Color(210, 210, 140));                                                                 rowY += RowHeight;
            DrawStatRow(spriteBatch, x, rowY, "FollowSight",
                saria.FollowSight ? "true" : "false",
                saria.FollowSight ? new Color(120, 255, 200) : ProbeInactiveColor);                                               rowY += RowHeight;
            DrawStatRow(spriteBatch, x, rowY, "Follow",
                saria.Follow ? $"true ({saria.FollowTrailDotCount}/60)" : "false",
                saria.Follow ? FollowColor : ProbeInactiveColor);                                                                 rowY += RowHeight;
            DrawStatRow(spriteBatch, x, rowY, "CursedSeparated",
                saria.CursedSeparated ? "true" : "false",
                saria.CursedSeparated ? new Color(80, 180, 255) : ProbeInactiveColor);                                            rowY += RowHeight + 4;

            // Probe / detection
            DrawSep(spriteBatch, x, rowY); rowY += 9;
            DrawStatRow(spriteBatch, x, rowY, "InWall",
                saria.InWall ? "true" : "false",
                saria.InWall ? new Color(255, 80, 80) : ProbeInactiveColor);                                                      rowY += RowHeight;
            DrawStatRow(spriteBatch, x, rowY, "OverallCov",
                $"{saria.DbgOverallCoverage * 100f:F0}%",
                saria.DbgOverallCoverage >= 0.3f ? new Color(255, 140, 40) : ProbeInactiveColor);                                  rowY += RowHeight;
            DrawStatRow(spriteBatch, x, rowY, "OrangeCov",
                $"{saria.DbgOrangeCoverage * 100f:F0}%",
                saria.DbgOrangeCoverage >= 0.4f ? new Color(255, 140, 0) : ProbeInactiveColor);                                    rowY += RowHeight;
            string probeText; Color probeColor;
            if (saria.DbgOutOfBounds)           { probeText = "OFF (OOB)"; probeColor = ProbeWallColor; }
            else if (saria.DbgProbesActive) { probeText = "true";     probeColor = ProbeActiveColor; }
            else                                 { probeText = "false";    probeColor = ProbeInactiveColor; }
            DrawStatRow(spriteBatch, x, rowY, "Probe Active",   probeText,                               probeColor);           rowY += RowHeight;
            DrawStatRow(spriteBatch, x, rowY, "Det: Down",
                saria.DbgDetDown ? "true" : "false",
                saria.DbgDetDown ? ProbeActiveColor : ProbeInactiveColor,
                DetZone(saria.DbgDetDown, saria.DbgDetDownGreen));                                                                rowY += RowHeight;
            DrawStatRow(spriteBatch, x, rowY, "Det: Down (Y)",
                saria.DbgDetDownYellow ? "true" : "false",
                saria.DbgDetDownYellow ? new Color(255, 220, 0) : ProbeInactiveColor);                                           rowY += RowHeight;
            DrawStatRow(spriteBatch, x, rowY, "Det: Up",
                saria.DbgDetUp ? "true" : "false",
                saria.DbgDetUp ? ProbeActiveColor : ProbeInactiveColor,
                DetZone(saria.DbgDetUp, saria.DbgDetUpGreen));                                                                    rowY += RowHeight;
            DrawStatRow(spriteBatch, x, rowY, "Det: Left",
                saria.DbgDetLeft ? "true" : "false",
                saria.DbgDetLeft ? ProbeWallColor : ProbeInactiveColor,
                DetZone(saria.DbgDetLeft, saria.DbgDetLeftGreen));                                                                rowY += RowHeight;
            DrawStatRow(spriteBatch, x, rowY, "Det: Right",
                saria.DbgDetRight ? "true" : "false",
                saria.DbgDetRight ? ProbeWallColor : ProbeInactiveColor,
                DetZone(saria.DbgDetRight, saria.DbgDetRightGreen));                                                              rowY += RowHeight + 4;

            // Biome / zones header
            DrawSep(spriteBatch, x, rowY); rowY += 9;
            Utils.DrawBorderString(spriteBatch, "Saria Biome / Zones",
                new Vector2(x + LabelX, rowY), new Color(180, 255, 180), 0.75f);                         rowY += RowHeight;

            DrawBiomeRow(spriteBatch, x, rowY, "Forest",      saria.SariaZoneForest);            rowY += RowHeight;
            DrawBiomeRow(spriteBatch, x, rowY, "Snow",        saria.SariaZoneSnow);              rowY += RowHeight;
            DrawBiomeRow(spriteBatch, x, rowY, "Jungle",      saria.SariaZoneJungle);            rowY += RowHeight;
            DrawBiomeRow(spriteBatch, x, rowY, "Corrupt",     saria.SariaZoneCorrupt);           rowY += RowHeight;
            DrawBiomeRow(spriteBatch, x, rowY, "Crimson",     saria.SariaZoneCrimson);           rowY += RowHeight;
            DrawBiomeRow(spriteBatch, x, rowY, "Hallow",      saria.SariaZoneHallow);            rowY += RowHeight;
            DrawBiomeRow(spriteBatch, x, rowY, "Desert",      saria.SariaZoneDesert);            rowY += RowHeight;
            DrawBiomeRow(spriteBatch, x, rowY, "Beach",       saria.SariaZoneBeach);             rowY += RowHeight;
            DrawBiomeRow(spriteBatch, x, rowY, "Dungeon",     saria.SariaZoneDungeon);           rowY += RowHeight;
            DrawBiomeRow(spriteBatch, x, rowY, "Sandstorm",   saria.SariaZoneSandstorm);         rowY += RowHeight;
            DrawBiomeRow(spriteBatch, x, rowY, "Ug.Desert",   saria.SariaZoneUndergroundDesert); rowY += RowHeight;
            DrawBiomeRow(spriteBatch, x, rowY, "GlowMushroom",saria.SariaZoneGlowingMushroom);   rowY += RowHeight;
            DrawBiomeRow(spriteBatch, x, rowY, "Graveyard",   saria.SariaZoneGraveyard);         rowY += RowHeight;
            DrawBiomeRow(spriteBatch, x, rowY, "Meteor",      saria.SariaZoneMeteor);            rowY += RowHeight + 2;

            DrawSep(spriteBatch, x, rowY); rowY += 9;
            DrawBiomeRow(spriteBatch, x, rowY, "Space",       saria.SariaZoneSpace);       rowY += RowHeight;
            DrawBiomeRow(spriteBatch, x, rowY, "SkyHeight",   saria.SariaZoneSkyHeight);   rowY += RowHeight;
            DrawBiomeRow(spriteBatch, x, rowY, "Overworld",   saria.SariaZoneOverworld);   rowY += RowHeight;
            DrawBiomeRow(spriteBatch, x, rowY, "Underground", saria.SariaZoneUnderground); rowY += RowHeight;
            DrawBiomeRow(spriteBatch, x, rowY, "DirtLayer",   saria.SariaZoneDirtLayer);   rowY += RowHeight;
            DrawBiomeRow(spriteBatch, x, rowY, "RockLayer",   saria.SariaZoneRockLayer);   rowY += RowHeight;
            DrawBiomeRow(spriteBatch, x, rowY, "Underworld",  saria.SariaZoneUnderworld);  rowY += RowHeight;
            DrawBiomeRow(spriteBatch, x, rowY, "Rain",        saria.SariaZoneRain);        rowY += RowHeight + 2;

            DrawSep(spriteBatch, x, rowY); rowY += 9;
            DrawBiomeRow(spriteBatch, x, rowY, "Campfire",     saria.SariaHasCampfire);     rowY += RowHeight;
            DrawBiomeRow(spriteBatch, x, rowY, "HeartLantern", saria.SariaHasHeartLantern); rowY += RowHeight;
            DrawBiomeRow(spriteBatch, x, rowY, "StarInBottle", saria.SariaHasStarInBottle); rowY += RowHeight;
            DrawBiomeRow(spriteBatch, x, rowY, "WaterCandle",  saria.SariaHasWaterCandle);  rowY += RowHeight;
            DrawBiomeRow(spriteBatch, x, rowY, "PeaceCandle",  saria.SariaHasPeaceCandle);  rowY += RowHeight;
            DrawBiomeRow(spriteBatch, x, rowY, "ReajCandle",   saria.SariaHasReajCandle);    rowY += RowHeight;
            DrawBiomeRow(spriteBatch, x, rowY, "CalmingCandle",saria.SariaHasCalmMindCandle);rowY += RowHeight;

            // Dialogue state
            DrawSep(spriteBatch, x, rowY); rowY += 9;
            Utils.DrawBorderString(spriteBatch, "Dialogue",
                new Vector2(x + LabelX, rowY), new Color(200, 180, 255), 0.75f);             rowY += RowHeight;

            bool dlgActive   = SariaUISystem.IsDialogueActive;
            bool ctActive    = SariaUISystem.IsCutsceneActive;
            string activeId  = SariaUISystem.ActiveCutsceneID ?? "—";
            FairyPlayer fp   = Main.LocalPlayer.GetModPlayer<FairyPlayer>();

            DrawStatRow(spriteBatch, x, rowY, "IsDialogue",
                dlgActive ? "YES" : "no",
                dlgActive ? DialogueActiveColor : DialogueInactiveColor);                     rowY += RowHeight;
            DrawStatRow(spriteBatch, x, rowY, "IsCutscene",
                ctActive ? "YES" : "no",
                ctActive ? DialogueActiveColor : DialogueInactiveColor);                      rowY += RowHeight;
            DrawStatRow(spriteBatch, x, rowY, "ActiveCutscene",
                activeId,
                DialogueSelectorColor);                                                        rowY += RowHeight;
            DrawDualRow(spriteBatch, x, rowY, "InteractionCooldown",
                fp.totalTalkingTime.ToString(), fp.smallTalkingTime.ToString(),
                ST_LurantisPink,
                ST_GerudoOrange,
                ST_SkullkidRed);                                                               rowY += RowHeight;
            DrawStatRow(spriteBatch, x, rowY, "DialogueTimer",
                SariaUISystem.TalkingTimer.ToString(),
                DialogueTimerColor);                                                           rowY += RowHeight;
            bool canInteract = InteractionManager.CanTriggerInteractive(fp);
            DrawStatRow(spriteBatch, x, rowY, "CanInteract",
                canInteract ? "YES" : "no",
                canInteract ? DialogueActiveColor : DialogueInactiveColor);                    rowY += RowHeight;

            // Sandstorm repeat counter
            DrawSep(spriteBatch, x, rowY); rowY += 9;
            int repeatCount = SariaMod.SandstormRepeatCount;
            DrawStatRow(spriteBatch, x, rowY, "SandstormRepeat",
                repeatCount.ToString(),
                repeatCount > 0 ? new Color(255, 200, 80) : new Color(140, 140, 140));        rowY += RowHeight;

            // Pending cutscene timers
            DrawSep(spriteBatch, x, rowY); rowY += 9;
            Utils.DrawBorderString(spriteBatch, "PendingCutscenes",
                new Vector2(x + LabelX, rowY), new Color(255, 210, 120), 0.75f);            rowY += RowHeight;
            var tracker = Main.LocalPlayer.GetModPlayer<SariaInteractionTrackerPlayer>();
            if (tracker.PendingCutscenes.Count == 0)
            {
                DrawStatRow(spriteBatch, x, rowY, "  (none)", "", new Color(100, 100, 100)); rowY += RowHeight;
            }
            else
            {
                foreach (var pc in tracker.PendingCutscenes)
                {
                    int secsLeft = (int)(pc.RemainingTime / 60.0);
                    Color timeColor = secsLeft > 120 ? new Color(120, 220, 120)
                                    : secsLeft > 30  ? new Color(255, 200, 80)
                                    : new Color(255, 80, 80);
                    DrawStatRow(spriteBatch, x, rowY,
                        pc.ID ?? "?",
                        $"{secsLeft}s",
                        timeColor);                                                          rowY += RowHeight;
                }
            }

            // Upgrades testing toggle
            DrawSep(spriteBatch, x, rowY); rowY += 9;
            Utils.DrawBorderString(spriteBatch, "Upgrades (Testing Toggle)",
                new Vector2(x + LabelX, rowY), new Color(255, 200, 80), 0.75f);             rowY += RowHeight;

            Utils.DrawBorderString(spriteBatch, "── Base (All Forms) ──",
                new Vector2(x + LabelX + 4, rowY), new Color(180, 180, 200), 0.7f);        rowY += RowHeight;
            for (int i = 0; i < 3; i++)
                rowY = DrawUpgradeToggle(spriteBatch, x, rowY, UpgradeLabels[i].Index, UpgradeLabels[i].Label, fp);

            string[] formHeaders = { "── Psychic ──", "── Water ──", "── Fire ──",
                "── Electric ──", "── Rock ──", "── Bug ──", "── Ghost ──" };
            int startIdx = 3;
            foreach (string header in formHeaders)
            {
                Utils.DrawBorderString(spriteBatch, header,
                    new Vector2(x + LabelX + 4, rowY), new Color(180, 180, 200), 0.7f);   rowY += RowHeight;
                for (int j = 0; j < 3; j++)
                {
                    rowY = DrawUpgradeToggle(spriteBatch, x, rowY,
                        UpgradeLabels[startIdx].Index, UpgradeLabels[startIdx].Label, fp);
                    startIdx++;
                }
            }

            // ── Restore scissor
            spriteBatch.End();
            spriteBatch.GraphicsDevice.ScissorRectangle = prevScissor;
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                SamplerState.LinearClamp, null, null, null, Main.UIScaleMatrix);

            DrawRect(spriteBatch, trackRect, ScrollTrackColor, Color.Transparent);
            DrawRect(spriteBatch, thumbRect,
                     thumbHovered || _scrollDragging ? ScrollThumbHover : ScrollThumbColor,
                     Color.Transparent);
        }

        // ── Sub-draw helpers ──────────────────────────────────────────────────────
        private void DrawToggleButton(SpriteBatch spriteBatch)
        {
            int x = ButtonMarginLeft;
            int y = Main.screenHeight - ButtonMarginBottom - ButtonHeight;
            Rectangle btn = new Rectangle(x, y, ButtonWidth, ButtonHeight);
            bool hover = btn.Contains(Main.mouseX, Main.mouseY);
            if (hover && Main.mouseLeft && Main.mouseLeftRelease) { PanelOpen = !PanelOpen; Main.mouseLeftRelease = false; }
            DrawRect(spriteBatch, btn, hover ? ButtonBgHover : ButtonBg, PanelBorder);
            string label = PanelOpen ? "Debug [x]" : "Debug";
            Vector2 sz = FontAssets.MouseText.Value.MeasureString(label) * 0.75f;
            Utils.DrawBorderString(spriteBatch, label,
                new Vector2(x + (ButtonWidth - sz.X) / 2f, y + (ButtonHeight - sz.Y) / 2f), ButtonText, 0.75f);
            if (hover) Main.LocalPlayer.mouseInterface = true;
        }

        private void DrawEyeButton(SpriteBatch spriteBatch)
        {
            int x = EyeButtonMarginLeft;
            int y = Main.screenHeight - ButtonMarginBottom - ButtonHeight;
            Rectangle btn = new Rectangle(x, y, EyeButtonWidth, ButtonHeight);
            bool hover  = btn.Contains(Main.mouseX, Main.mouseY);
            bool active = SariaDebugUISystem.HitboxVisible;
            if (hover && Main.mouseLeft && Main.mouseLeftRelease) { SariaDebugUISystem.HitboxVisible = !active; Main.mouseLeftRelease = false; }
            DrawRect(spriteBatch, btn,
                active ? new Color(40, 90, 40, 230) : (hover ? ButtonBgHover : ButtonBg),
                active ? new Color(80, 200, 80, 255) : PanelBorder);
            Vector2 sz = FontAssets.MouseText.Value.MeasureString("👁") * 0.85f;
            Utils.DrawBorderString(spriteBatch, "👁",
                new Vector2(x + (EyeButtonWidth - sz.X) / 2f, y + (ButtonHeight - sz.Y) / 2f),
                active ? new Color(120, 255, 120) : ButtonText, 0.85f);
            if (hover) Main.LocalPlayer.mouseInterface = true;
        }

        private void DrawBiomeRow(SpriteBatch spriteBatch, int panelX, int rowY, string label, bool active)
        {
            Utils.DrawBorderString(spriteBatch, label,
                new Vector2(panelX + LabelX, rowY),
                active ? new Color(140, 230, 140) : new Color(140, 140, 140), 0.8f);
            Utils.DrawBorderString(spriteBatch, active ? "YES" : "no",
                new Vector2(panelX + ValueX, rowY),
                active ? new Color(100, 255, 120) : new Color(90, 90, 90), 0.8f);
        }

        private void DrawStatRow(SpriteBatch spriteBatch, int panelX, int rowY,
                                  string label, string value, Color labelColor, string zone = null)
        {
            Utils.DrawBorderString(spriteBatch, label, new Vector2(panelX + LabelX, rowY), labelColor, 0.8f);
            Utils.DrawBorderString(spriteBatch, value, new Vector2(panelX + ValueX,  rowY), ValueColor, 0.8f);
            if (zone != null)
            {
                Color zc = zone switch { "Both" => new Color(255,180,60), "Pink" => new Color(255,120,200), "Green" => new Color(100,255,140), _ => ProbeInactiveColor };
                Utils.DrawBorderString(spriteBatch, zone, new Vector2(panelX + ValueX + 48, rowY), zc, 0.8f);
            }
        }

        private void DrawDualRow(SpriteBatch spriteBatch, int panelX, int rowY,
                                   string label, string val1, string val2, Color labelColor,
                                   Color val1Color = default, Color val2Color = default)
        {
            if (val1Color == default) val1Color = ST_StalfosGrey;
            if (val2Color == default) val2Color = ST_StalfosGrey;
            Utils.DrawBorderString(spriteBatch, label, new Vector2(panelX + LabelX, rowY), labelColor, 0.8f);
            Utils.DrawBorderString(spriteBatch, val1,  new Vector2(panelX + ValueX, rowY), val1Color, 0.8f);
            Vector2 v1Size = FontAssets.MouseText.Value.MeasureString(val1) * 0.8f;
            Utils.DrawBorderString(spriteBatch, $"/ {val2}", new Vector2(panelX + ValueX + v1Size.X + 4, rowY), val2Color, 0.8f);
        }

        private int DrawUpgradeToggle(SpriteBatch spriteBatch, int panelX, int rowY, int upgradeIndex, string label, FairyPlayer fp)
        {
            bool val = GetUpgradeBool(fp, upgradeIndex);
            Rectangle btn = new Rectangle(panelX + ValueX - 10, rowY, 50, RowHeight);
            bool hover = btn.Contains(Main.mouseX, Main.mouseY);
            if (hover && Main.mouseLeft && Main.mouseLeftRelease)
            {
                SetUpgradeBool(fp, upgradeIndex, !val);
                Main.mouseLeftRelease = false;
            }
            Utils.DrawBorderString(spriteBatch, label,
                new Vector2(panelX + LabelX, rowY),
                val ? new Color(100, 255, 120) : new Color(140, 140, 140), 0.8f);
            string status = val ? "ON" : "OFF";
            Color statusColor = val ? new Color(80, 220, 80) : new Color(120, 120, 120);
            DrawRect(spriteBatch, btn, hover ? ButtonBgHover : ButtonBg, statusColor);
            Vector2 sz = FontAssets.MouseText.Value.MeasureString(status) * 0.75f;
            Utils.DrawBorderString(spriteBatch, status,
                new Vector2(btn.X + (btn.Width - sz.X) / 2f, btn.Y + (btn.Height - sz.Y) / 2f),
                statusColor, 0.75f);
            if (hover) Main.LocalPlayer.mouseInterface = true;
            return rowY + RowHeight;
        }

        private static bool GetUpgradeBool(FairyPlayer fp, int index)
        {
            return index switch
            {
                1  => fp.SariaUpgrade1,  2  => fp.SariaUpgrade2,  3  => fp.SariaUpgrade3,
                4  => fp.SariaUpgrade4,  5  => fp.SariaUpgrade5,  6  => fp.SariaUpgrade6,
                7  => fp.SariaUpgrade7,  8  => fp.SariaUpgrade8,  9  => fp.SariaUpgrade9,
                10 => fp.SariaUpgrade10, 11 => fp.SariaUpgrade11, 12 => fp.SariaUpgrade12,
                13 => fp.SariaUpgrade13, 14 => fp.SariaUpgrade14, 15 => fp.SariaUpgrade15,
                16 => fp.SariaUpgrade16, 17 => fp.SariaUpgrade17, 18 => fp.SariaUpgrade18,
                19 => fp.SariaUpgrade19, 20 => fp.SariaUpgrade20, 21 => fp.SariaUpgrade21,
                22 => fp.SariaUpgrade22, 23 => fp.SariaUpgrade23, 24 => fp.SariaUpgrade24,
                _ => false,
            };
        }

        private static void SetUpgradeBool(FairyPlayer fp, int index, bool value)
        {
            switch (index)
            {
                case 1:  fp.SariaUpgrade1 = value; break;
                case 2:  fp.SariaUpgrade2 = value; break;
                case 3:  fp.SariaUpgrade3 = value; break;
                case 4:  fp.SariaUpgrade4 = value; break;
                case 5:  fp.SariaUpgrade5 = value; break;
                case 6:  fp.SariaUpgrade6 = value; break;
                case 7:  fp.SariaUpgrade7 = value; break;
                case 8:  fp.SariaUpgrade8 = value; break;
                case 9:  fp.SariaUpgrade9 = value; break;
                case 10: fp.SariaUpgrade10 = value; break;
                case 11: fp.SariaUpgrade11 = value; break;
                case 12: fp.SariaUpgrade12 = value; break;
                case 13: fp.SariaUpgrade13 = value; break;
                case 14: fp.SariaUpgrade14 = value; break;
                case 15: fp.SariaUpgrade15 = value; break;
                case 16: fp.SariaUpgrade16 = value; break;
                case 17: fp.SariaUpgrade17 = value; break;
                case 18: fp.SariaUpgrade18 = value; break;
                case 19: fp.SariaUpgrade19 = value; break;
                case 20: fp.SariaUpgrade20 = value; break;
                case 21: fp.SariaUpgrade21 = value; break;
                case 22: fp.SariaUpgrade22 = value; break;
                case 23: fp.SariaUpgrade23 = value; break;
                case 24: fp.SariaUpgrade24 = value; break;
            }
        }

        private void DrawSep(SpriteBatch spriteBatch, int panelX, int rowY)
            => DrawRect(spriteBatch, new Rectangle(panelX + 4, rowY, PanelWidth - 8, 1), PanelBorder, Color.Transparent);

        private static string DetZone(bool pink, bool green)
        {
            if (pink && green) return "Both";
            if (pink)          return "Pink";
            if (green)         return "Green";
            return "None";
        }

        private void DrawRect(SpriteBatch spriteBatch, Rectangle rect, Color fill, Color border)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            spriteBatch.Draw(pixel, rect, fill);
            if (border != Color.Transparent)
            {
                spriteBatch.Draw(pixel, new Rectangle(rect.X,          rect.Y,          rect.Width, 1), border);
                spriteBatch.Draw(pixel, new Rectangle(rect.X,          rect.Bottom - 1, rect.Width, 1), border);
                spriteBatch.Draw(pixel, new Rectangle(rect.X,          rect.Y,          1, rect.Height), border);
                spriteBatch.Draw(pixel, new Rectangle(rect.Right - 1,  rect.Y,          1, rect.Height), border);
            }
        }
    }
}
