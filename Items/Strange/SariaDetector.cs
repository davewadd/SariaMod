using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.Strange
{
    // Config for one collision detector probe.
    // Anchor is always the outermost surface edge in the facing direction.
    // Rotation: 0° = DOWN, 90° = RIGHT, 180° = UP, 270° = LEFT.
    public readonly struct SariaDetectorConfig
    {
        // Offset from sprite top-left to the probe anchor (surface edge point).
        public readonly Vector2 AnchorOffset;
        // Cardinal direction this probe faces (0/90/180/270 degrees).
        public readonly float   RotationDegrees;
        // Cross-axis width of the probe rectangles.
        public readonly int     Width;
        // Depth of the green (contact) zone measured inward from the anchor.
        public readonly int     GreenDepth;
        // Depth of the pink (embedded) zone measured inward from the green zone.
        public readonly int     PinkDepth;
        // Static priority — higher priority suppresses lower when active (Rule 1).
        public readonly int     Priority;
        // Cancel group — if ≥2 probes in the same non-zero group both fire Pink,
        // all probes in that group are suppressed (Rule 2).
        public readonly int     CancelGroup;
        // Whether this probe has a yellow (settle-down) zone.
        public readonly bool    HasPullLine;
        // Depth of the yellow zone measured outward from the anchor.
        public readonly int     PullLength;
        // When true, IsActive fires on Pink OR Green (green counts as active).
        // When false, IsActive requires Pink only.
        public readonly bool    GreenIsActive;
        // Passed to Collision.SolidCollision as acceptTopSurfaces.
        public readonly bool    AcceptTopSurfaces;
        // When true (and AcceptTopSurfaces is also true), solidTop tiles that are
        // NOT real platforms (e.g. tables, workbenches) are ignored by the probe.
        public readonly bool    ExcludeFurnitureTops;
        // Priority used for yellow (settle-down) correction suppression.
        // Works on the same basis as Priority: if any active probe has a higher
        // Priority than this probe's YellowPriority, the yellow correction is
        // suppressed. Set to 0 for probes without a pull line (no effect).
        public readonly int     YellowPriority;
        // When true, the detector checks whether the innermost pixel line of the
        // Green zone (i.e. the boundary shared with Pink) is solid. If it is,
        // GreenPaused is set on the result and velocity into that wall is zeroed.
        // Priority is NOT affected — this is purely a velocity gate, not a suppressor.
        // Only meaningful on wall (horizontal) probes.
        public readonly bool    GreenPause;
        // When true, the anchor X position is mirrored horizontally when spriteDirection == -1.
        // Use for vertical probes (feet, head) so they follow the sprite's facing side.
        public readonly bool    FlipWithDirection;

        public SariaDetectorConfig(
            Vector2 anchorOffset,
            float   rotationDegrees,
            int     width,
            int     greenDepth,
            int     pinkDepth,
            int     priority,
            int     cancelGroup,
            bool    hasPullLine,
            int     pullLength,
            bool    greenIsActive,
            bool    acceptTopSurfaces = false,
            bool    excludeFurnitureTops = false,
            int     yellowPriority = 0,
            bool    greenPause = false,
            bool    flipWithDirection = false)
        {
            AnchorOffset         = anchorOffset;
            RotationDegrees      = rotationDegrees;
            Width                = width;
            GreenDepth           = greenDepth;
            PinkDepth            = pinkDepth;
            Priority             = priority;
            CancelGroup          = cancelGroup;
            HasPullLine          = hasPullLine;
            PullLength           = pullLength;
            GreenIsActive        = greenIsActive;
            AcceptTopSurfaces    = acceptTopSurfaces;
            ExcludeFurnitureTops = excludeFurnitureTops;
            YellowPriority       = yellowPriority;
            GreenPause           = greenPause;
            FlipWithDirection    = flipWithDirection;
        }
    }

    // Result of evaluating one detector probe for a single AI tick.
    public struct SariaDetectorResult
    {
        // Pink zone fired (sprite is embedded in a tile).
        public bool Pink;
        // Green zone fired (sprite is touching a surface).
        public bool Green;
        // Yellow zone fired (tile is nearby but not yet touching).
        public bool Yellow;
        // Derived active flag (see GreenIsActive in config).
        public bool IsActive;
        // Set true by Rule 1 or Rule 2 — corrections are skipped.
        public bool Suppressed;
        // Set true when this probe's YellowPriority is below the highest active
        // probe's Priority — yellow settle-down correction is skipped.
        public bool YellowSuppressed;
        // Set true when GreenPause is enabled on the config AND the innermost
        // pixel line of the Green zone (the Pink boundary) is solid.
        // Signals that velocity into this wall should be gated externally.
        public bool GreenPaused;
    }

    public static class SariaDetector
    {
        // Converts rotation degrees to integer facing direction components.
        // 0° = DOWN (0,+1), 90° = RIGHT (+1,0), 180° = UP (0,-1), 270° = LEFT (-1,0).
        public static void GetFacingDir(float degrees, out int ifx, out int ify)
        {
            float rad = MathHelper.ToRadians(degrees);
            ifx = (int)Math.Round(Math.Sin(rad));
            ify = (int)Math.Round(Math.Cos(rad));
        }

        // Returns the three probe rectangles in world space for a given config.
        // spritePos is Projectile.position (top-left corner of the sprite).
        public static void GetProbeRects(
            in SariaDetectorConfig cfg,
            Vector2 spritePos,
            int ifx, int ify,
            out Rectangle pink,
            out Rectangle green,
            out Rectangle yellow,
            int spriteWidth = 0,
            int spriteDirection = 1)
        {
            int anchorX = (int)cfg.AnchorOffset.X;
            if (cfg.FlipWithDirection && spriteDirection == -1 && spriteWidth > 0)
                anchorX = spriteWidth - anchorX - cfg.Width;
            int ax = (int)spritePos.X + anchorX;
            int ay = (int)spritePos.Y + (int)cfg.AnchorOffset.Y;
            int W  = cfg.Width;

            if (ify == 1) // DOWN: inward = up (Y decreases)
            {
                pink   = new Rectangle(ax, ay - cfg.GreenDepth - cfg.PinkDepth, W, cfg.PinkDepth);
                green  = new Rectangle(ax, ay - cfg.GreenDepth,                  W, cfg.GreenDepth);
                yellow = new Rectangle(ax, ay,                                   W, cfg.PullLength);
            }
            else if (ify == -1) // UP: inward = down (Y increases)
            {
                pink   = new Rectangle(ax, ay + cfg.GreenDepth,                  W, cfg.PinkDepth);
                green  = new Rectangle(ax, ay,                                   W, cfg.GreenDepth);
                yellow = new Rectangle(ax, ay - cfg.PullLength,                  W, cfg.PullLength);
            }
            else if (ifx == 1) // RIGHT: inward = left (X decreases)
            {
                pink   = new Rectangle(ax - cfg.GreenDepth - cfg.PinkDepth, ay, cfg.PinkDepth, W);
                green  = new Rectangle(ax - cfg.GreenDepth,                 ay, cfg.GreenDepth, W);
                yellow = new Rectangle(ax,                                  ay, cfg.PullLength, W);
            }
            else // LEFT (ifx == -1): inward = right (X increases)
            {
                pink   = new Rectangle(ax + cfg.GreenDepth, ay, cfg.PinkDepth,  W);
                green  = new Rectangle(ax,                  ay, cfg.GreenDepth, W);
                yellow = new Rectangle(ax - cfg.PullLength, ay, cfg.PullLength, W);
            }
        }

        // Custom solid-collision check that accepts real platforms but rejects furniture
        // (tables, workbenches, etc.) even though both have tileSolidTop[] = true.
        // Fully-solid and half-block tiles are always solid.
        // Sloped solid tiles are only solid when Saria is moving in the ascending direction,
        // matching the same direction-aware logic used for sloped platforms below.
        // Platforms are scanned separately so furniture (also tileSolidTop) stays excluded.
        private static bool SolidCollisionExcludeFurniture(Vector2 pos, int w, int h, float velocityX = 0f)
        {
            // Manual scan for solid geometry with sub-tile diagonal surface detection.
            // For fully-solid, half-block, and ceiling-slope (SlopeUpLeft/SlopeUpRight) tiles:
            //   always solid — bounding-box intersection is correct.
            // For floor-slope tiles (SlopeDownLeft / SlopeDownRight):
            //   use the actual diagonal surface line instead of the full bounding box.
            //   A probe overlaps solid only when its bottommost pixel falls AT or BELOW
            //   the surface at the shallowest column in the overlap range.
            //   This gives sub-tile precision (smooth stepping), prevents phase-through in
            //   BOTH directions, and correctly fires when resting (no direction gate).
            //
            // SlopeDownLeft : high-left side.
            //   surfaceY(cx) = tileTop + (cx - tileLeft)          [low Y = high on screen]
            //   minSurfaceY  = surfaceY at leftmost overlap column (left = shallowest)
            // SlopeDownRight: high-right side.
            //   surfaceY(cx) = tileTop + 15 - (cx - tileLeft)
            //   minSurfaceY  = surfaceY at rightmost overlap column (right = shallowest)
            {
                int probeLeft        = (int)pos.X;
                int probeRight       = (int)pos.X + w;       // exclusive
                int probeTop         = (int)pos.Y;
                int probeBottomPixel = (int)pos.Y + h - 1;   // last (lowest) row of probe

                int sLeft   = probeLeft        / 16;
                int sRight  = (probeRight - 1) / 16;
                int sTop    = probeTop         / 16;
                int sBottom = probeBottomPixel / 16;

                for (int tx = sLeft; tx <= sRight; tx++)
                {
                    for (int ty = sTop; ty <= sBottom; ty++)
                    {
                        if (tx < 0 || ty < 0 || tx >= Main.maxTilesX || ty >= Main.maxTilesY)
                            return true; // world boundary counts as solid
                        Tile tile = Main.tile[tx, ty];
                        if (!tile.HasTile || !Main.tileSolid[tile.TileType] || Main.tileSolidTop[tile.TileType])
                            continue;
                        // Doors and tall gates are passable — skip them so the ground
                        // probe and wall detectors don't treat them as solid obstacles.
                        ushort tType = tile.TileType;
                        if (tType == TileID.ClosedDoor || tType == TileID.TallGateClosed)
                            continue;

                        int tileLeft = tx * 16;
                        int tileTop  = ty * 16;

                        if (tile.Slope == SlopeType.Solid && !tile.IsHalfBlock)
                        {
                            // Fully solid (not half-block): always solid.
                            return true;
                        }

                        if (tile.Slope == SlopeType.SlopeUpLeft || tile.Slope == SlopeType.SlopeUpRight)
                        {
                            // Ceiling-face slope: always solid (solid from below).
                            return true;
                        }

                        if (tile.IsHalfBlock)
                        {
                            // Half-block: solid only in the lower 8 pixels of the tile space.
                            int halfBlockSurface = tileTop + 8;
                            if (probeBottomPixel >= halfBlockSurface)
                                return true;
                            continue;
                        }

                        // SlopeDownLeft / SlopeDownRight: diagonal surface intersection.
                        // Skip if the probe sits entirely below the tile (in air below it).
                        if (probeTop > tileTop + 15)
                            continue;

                        // Find the columns where probe and tile both overlap.
                        int xLeft  = Math.Max(probeLeft,    tileLeft);
                        int xRight = Math.Min(probeRight - 1, tileLeft + 15); // inclusive
                        if (xLeft > xRight)
                            continue;

                        int minSurfaceY;
                        if (tile.Slope == SlopeType.SlopeDownLeft)
                        {
                            // High-left: surface rises going left, so min surface Y is at leftmost column.
                            minSurfaceY = tileTop + (xLeft - tileLeft);
                        }
                        else // SlopeDownRight
                        {
                            // High-right: surface rises going right, so min surface Y is at rightmost column.
                            minSurfaceY = tileTop + 15 - (xRight - tileLeft);
                        }

                        if (probeBottomPixel >= minSurfaceY)
                            return true;
                    }
                }
            }

            // Additionally accept real platforms (stand-on-top, but not furniture).
            // Flat platforms: one-way from above only.
            // Sloped platforms: only solid when Saria is moving in the ascending direction.
            int left   = (int)pos.X / 16;
            int right  = ((int)pos.X + w - 1) / 16;
            int top    = (int)pos.Y / 16;
            int bottom = ((int)pos.Y + h - 1) / 16;

            for (int tx = left; tx <= right; tx++)
            {
                for (int ty = top; ty <= bottom; ty++)
                {
                    if (tx < 0 || ty < 0 || tx >= Main.maxTilesX || ty >= Main.maxTilesY)
                        continue;
                    Tile tile = Main.tile[tx, ty];
                    if (!tile.HasTile || !TileID.Sets.Platforms[tile.TileType])
                        continue;

                    if (tile.Slope == (SlopeType)0)
                    {
                        // Flat platform: one-way from above only.
                        // Half-platforms have their surface at tileTop+8 instead of tileTop.
                        int surfaceY = tile.IsHalfBlock ? ty * 16 + 8 : ty * 16;
                        if ((int)pos.Y + h - 1 >= surfaceY && (int)pos.Y <= surfaceY)
                            return true;
                    }
                    else
                    {
                        // Sloped platform: only solid when moving in the ascending direction.
                        // SlopeDownLeft  (1): high-right, low-left  → ascending = moving RIGHT (velocityX > 0)
                        // SlopeDownRight (2): high-left,  low-right → ascending = moving LEFT  (velocityX < 0)
                        bool ascending =
                            (tile.Slope == SlopeType.SlopeDownRight  && velocityX > 0f) ||
                            (tile.Slope == SlopeType.SlopeDownLeft && velocityX < 0f);
                        if (ascending)
                            return true;
                    }
                }
            }
            return false;
        }

        // Returns true if any tile fully or partially within rect contains a non-water
        // liquid (lava, honey, shimmer). Used to suppress the yellow settle-down pull
        // so Saria is never drifted into harmful liquid by the foot probe.
        private static bool RectContainsNonWaterLiquid(Rectangle rect)
        {
            int tLeft   = rect.X          / 16;
            int tRight  = (rect.Right - 1) / 16;
            int tTop    = rect.Y           / 16;
            int tBottom = (rect.Bottom - 1) / 16;
            for (int tx = tLeft; tx <= tRight; tx++)
            {
                for (int ty = tTop; ty <= tBottom; ty++)
                {
                    if (tx < 0 || ty < 0 || tx >= Main.maxTilesX || ty >= Main.maxTilesY)
                        continue;
                    Tile t = Main.tile[tx, ty];
                    if (t.LiquidAmount > 0 && t.LiquidType != LiquidID.Water)
                        return true;
                }
            }
            return false;
        }

        // Evaluates one detector at the current sprite position.
        public static SariaDetectorResult Evaluate(
            in SariaDetectorConfig cfg,
            Vector2 spritePos,
            bool enabled,
            float velocityX = 0f,
            int spriteWidth = 0,
            int spriteDirection = 1)
        {
            var result = new SariaDetectorResult();
            if (!enabled)
                return result;

            GetFacingDir(cfg.RotationDegrees, out int ifx, out int ify);
            GetProbeRects(in cfg, spritePos, ifx, ify,
                out Rectangle pinkR, out Rectangle greenR, out Rectangle yellowR,
                spriteWidth, spriteDirection);

            if (cfg.PinkDepth > 0)
                result.Pink = cfg.AcceptTopSurfaces && cfg.ExcludeFurnitureTops
                    ? SolidCollisionExcludeFurniture(new Vector2(pinkR.X, pinkR.Y), pinkR.Width, pinkR.Height, velocityX)
                    : Collision.SolidCollision(new Vector2(pinkR.X, pinkR.Y), pinkR.Width, pinkR.Height, cfg.AcceptTopSurfaces);

            if (cfg.GreenDepth > 0)
                result.Green = cfg.AcceptTopSurfaces && cfg.ExcludeFurnitureTops
                    ? SolidCollisionExcludeFurniture(new Vector2(greenR.X, greenR.Y), greenR.Width, greenR.Height, velocityX)
                    : Collision.SolidCollision(new Vector2(greenR.X, greenR.Y), greenR.Width, greenR.Height, cfg.AcceptTopSurfaces);

            if (cfg.HasPullLine && cfg.PullLength > 0)
            {
                result.Yellow = cfg.AcceptTopSurfaces && cfg.ExcludeFurnitureTops
                    ? SolidCollisionExcludeFurniture(new Vector2(yellowR.X, yellowR.Y), yellowR.Width, yellowR.Height, velocityX)
                    : Collision.SolidCollision(new Vector2(yellowR.X, yellowR.Y), yellowR.Width, yellowR.Height, false);
                // Suppress the pull if the zone contains non-water liquid (lava, honey,
                // shimmer). Water is intentionally passable — normal wading is fine.
                if (result.Yellow && RectContainsNonWaterLiquid(yellowR))
                    result.Yellow = false;
            }

            result.IsActive = cfg.GreenIsActive
                ? (result.Pink || result.Green)
                : result.Pink;

            // GreenPause: check only the innermost 1px line of the Green zone.
            // This is the boundary shared with Pink — solid here means Saria is
            // right at the wall face. Priority is not affected.
            if (cfg.GreenPause && cfg.GreenDepth > 0)
            {
                GetProbeRects(in cfg, spritePos, ifx, ify,
                    out Rectangle pinkR2, out Rectangle greenR2, out _,
                    spriteWidth, spriteDirection);
                // Inner line of green = the face touching pink.
                // For LEFT probe (ifx==-1): inner edge is the RIGHT side of greenR → x = greenR.Right-1
                // For RIGHT probe (ifx==+1): inner edge is the LEFT side of greenR  → x = greenR.X
                Rectangle innerLine;
                if (ifx == 1)
                    innerLine = new Rectangle(greenR2.X, greenR2.Y, 1, greenR2.Height);
                else if (ifx == -1)
                    innerLine = new Rectangle(greenR2.Right - 1, greenR2.Y, 1, greenR2.Height);
                else
                    innerLine = Rectangle.Empty;

                if (innerLine != Rectangle.Empty)
                    result.GreenPaused = Collision.SolidCollision(
                        new Vector2(innerLine.X, innerLine.Y), innerLine.Width, innerLine.Height, false);
            }

            return result;
        }

        // Evaluates all detectors, applies the two suppression rules, then
        // applies position and velocity corrections to the projectile.
        //
        // suppressYellow: pass Sleep (or any condition that should disable settle-down).
        public static void Apply(
            SariaDetectorConfig[]  configs,
            SariaDetectorResult[]  results,
            Vector2                spritePos,
            bool                   enabled,
            bool                   suppressYellow,
            ref Vector2            position,
            ref Vector2            velocity,
            int                    spriteWidth = 0,
            int                    spriteDirection = 1)
        {
            int count = configs.Length;

            // Step 1: Evaluate all detectors.
            for (int i = 0; i < count; i++)
                results[i] = Evaluate(in configs[i], spritePos, enabled, velocity.X, spriteWidth, spriteDirection);

            if (!enabled)
                return;

            // Step 2 (Rule 2): Cancel groups.
            // For each non-zero group, count Pink hits. If ≥2, suppress ALL in group.
            int maxGroup = 0;
            for (int i = 0; i < count; i++)
                if (configs[i].CancelGroup > maxGroup)
                    maxGroup = configs[i].CancelGroup;

            for (int g = 1; g <= maxGroup; g++)
            {
                int pinkCount = 0;
                for (int i = 0; i < count; i++)
                    if (configs[i].CancelGroup == g && results[i].Pink)
                        pinkCount++;

                if (pinkCount >= 2)
                    for (int i = 0; i < count; i++)
                        if (configs[i].CancelGroup == g)
                            results[i].Suppressed = true;
            }

            // Step 3 (Rule 1): Static priority.
            // Find the highest priority among active, non-suppressed detectors.
            // Suppress all active detectors below that priority.
            int maxPriority = -1;
            for (int i = 0; i < count; i++)
                if (!results[i].Suppressed && results[i].IsActive)
                    if (configs[i].Priority > maxPriority)
                        maxPriority = configs[i].Priority;

            if (maxPriority >= 0)
                for (int i = 0; i < count; i++)
                    if (!results[i].Suppressed && results[i].IsActive)
                        if (configs[i].Priority < maxPriority)
                            results[i].Suppressed = true;

            // Step 3b: Yellow priority suppression.
            // Suppress yellow settle-down corrections whose YellowPriority is below
            // the highest active probe's Priority. This prevents the feet yellow from
            // firing while the head probe (or walls) is actively pushing.
            for (int i = 0; i < count; i++)
            {
                if (!results[i].Yellow || !configs[i].HasPullLine)
                    continue;
                if (configs[i].YellowPriority < maxPriority)
                    results[i].YellowSuppressed = true;
            }

            // Step 4: Apply corrections for each non-suppressed detector.
            for (int i = 0; i < count; i++)
            {
                if (results[i].Suppressed)
                    continue;

                SariaDetectorConfig cfg = configs[i];
                SariaDetectorResult res = results[i];

                GetFacingDir(cfg.RotationDegrees, out int ifx, out int ify);

                if (res.Pink)
                {
                    // Push sprite inward (opposite of facing direction) by 1px.
                    position.X -= ifx;
                    position.Y -= ify;
                    // Zero velocity in the outward (facing) direction.
                    if (ify > 0 && velocity.Y > 0f) velocity.Y = 0f;
                    if (ify < 0 && velocity.Y < 0f) velocity.Y = 0f;
                    if (ifx > 0 && velocity.X > 0f) velocity.X = 0f;
                    if (ifx < 0 && velocity.X < 0f) velocity.X = 0f;
                }
                else if (res.Green && res.IsActive)
                {
                    // Surface contact — zero outward velocity only.
                    if (ify > 0 && velocity.Y > 0f) velocity.Y = 0f;
                    if (ify < 0 && velocity.Y < 0f) velocity.Y = 0f;
                    if (ifx > 0 && velocity.X > 0f) velocity.X = 0f;
                    if (ifx < 0 && velocity.X < 0f) velocity.X = 0f;
                }
                else if (res.Yellow && cfg.HasPullLine && !suppressYellow && !res.YellowSuppressed)
                {
                    // Tile is close but not yet touching — drift toward it by 0.5px.
                    position.X += ifx * 0.5f;
                    position.Y += ify * 0.5f;
                }
            }

            // Step 5: Velocity-only correction for suppressed Pink probes.
            // Suppression correctly skips the position push, but velocity in the
            // penetration direction must still be zeroed to prevent drift into the
            // suppressed solid (which causes 1px oscillation with the active probe).
            for (int i = 0; i < count; i++)
            {
                if (!results[i].Suppressed || !results[i].Pink)
                    continue;
                GetFacingDir(configs[i].RotationDegrees, out int ifx2, out int ify2);
                if (ify2 > 0 && velocity.Y > 0f) velocity.Y = 0f;
                if (ify2 < 0 && velocity.Y < 0f) velocity.Y = 0f;
                if (ifx2 > 0 && velocity.X > 0f) velocity.X = 0f;
                if (ifx2 < 0 && velocity.X < 0f) velocity.X = 0f;
            }
        }
    }
}
