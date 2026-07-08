using Microsoft.Xna.Framework;
using System.Linq;
using System.Reflection;
using System.Text;
using Terraria;
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.GameContent.Events;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ID;
using System;
using System.IO;
using Terraria.ModLoader;
using Terraria.ObjectData;
using SariaMod.Items;
using SariaMod.Buffs;
using SariaMod.Items.Strange;
using SariaMod.Dusts;
using SariaMod.Items.Amber;
using SariaMod.Items.Amethyst;
using SariaMod.Items.Bands;
using SariaMod.Items.Emerald;
using SariaMod.Items.Ruby;
using SariaMod.Items.Sapphire;
using SariaMod.Items.Topaz;
using SariaMod.Items.zPearls;
using SariaMod.Items.zTalking;
using Terraria.Localization;
using Terraria.Audio;
using Terraria.UI;
using SariaMod;
using Terraria.GameContent.UI.Elements;
using ReLogic.Content;
using Terraria.DataStructures;
namespace SariaMod
{
    public static class SariaMovementExtensions
    {
        public static bool NewIdlePosition(Projectile projectile, int howclose)
        {
            Player player = Main.player[projectile.owner];
            Vector2 PointtoCheck = player.Center;
            PointtoCheck.Y -= 10;
            PointtoCheck.X += howclose * player.direction;
            bool NoDetectWall = Collision.CanHitLine(player.position, -5, 1, PointtoCheck, 1, 1);
            if (NoDetectWall)
            {
                return true;
            }
            else return false;
        }

        // Scans the tile grid outward from the player in their facing direction and returns
        // the furthest pixel distance Saria can safely stand without clipping into a solid tile.
        // Requires 2+ consecutive solid, non-slope, non-halfblock tiles vertically (excludes
        // stairs and ramps). Runs every tick regardless of movement state.
        // Result is always in [0, logicalClose] — it never exceeds the logical idle distance.
        public static float CalcWallSafeClose(Projectile projectile, float logicalClose)
        {
            Player player = Main.player[projectile.owner];
            const float sariaMargin = 10f;

            float originX = player.Center.X;
            float idleY   = player.Center.Y - 10f;
            int   dir     = player.direction;   // +1 right, -1 left

            // Scan from the row at Saria's feet upward so the 2-tile check covers torso+head.
            int rowFeet  = (int)(idleY / 16f) + 1;
            int playerCol = (int)(originX / 16f);

            // Scan far enough to cover logicalClose plus one extra tile for safety
            int scanTiles = (int)Math.Ceiling((logicalClose + 16f) / 16f) + 1;

            float wallResult = logicalClose; // existing wall-column result
            for (int i = 1; i <= scanTiles; i++)
            {
                int col = Math.Clamp(playerCol + dir * i, 0, Main.maxTilesX - 1);

                // Only treat as a wall if 2+ consecutive solid non-slope non-halfblock tiles
                if (!HasWallColumn(col, rowFeet - 2, 2))
                    continue;

                // Pixel X of the wall face visible to the player
                float wallFaceX = dir > 0
                    ? col * 16f           // left face of tile
                    : (col + 1) * 16f;   // right face of tile

                float dist = Math.Abs(wallFaceX - originX) - sariaMargin;
                wallResult = Math.Clamp(dist, 0f, logicalClose);
                break;
            }

            // Additive footprint check: independently verify Saria's full body (2×3 tiles)
            // fits at the candidate idle column. Catches mixed geometry (half-tiles, slopes,
            // single-tile gaps) that the wall-column scan misses because it requires 2+
            // consecutive fully-solid tiles.
            // Tracks the current Y row as the scan walks forward; each step may only shift
            // the Y origin by 1 tile at a time (up or down), so stairs/steps are reachable
            // but a 2-tile vertical jump in one step is correctly treated as impassable.
            const int fpW    = SariaPathfinder.FootprintWidthConst;
            const int fpH    = SariaPathfinder.FootprintHeightConst;

            int  currentOriginY = (int)Math.Floor(idleY / 16f) - fpH / 2;
            float fitResult     = logicalClose;

            for (int i = 1; i <= scanTiles; i++)
            {
                float candidateX = originX + dir * (i * 16f);
                int   fpOriginX  = (int)Math.Floor(candidateX / 16f) - fpW / 2;

                // Try same Y, then one tile up (-1), then one tile down (+1).
                // Only allow a shift if the previous step's Y was already valid at that offset,
                // i.e. we step at most 1 tile vertically from where we currently are.
                int? nextOriginY = null;
                foreach (int dy in new[] { 0, -1, 1 })
                {
                    int tryY = currentOriginY + dy;
                    if (SariaPathfinder.FootprintFits(fpOriginX, tryY, fpW, fpH))
                    {
                        nextOriginY = tryY;
                        break;
                    }
                }

                if (nextOriginY == null)
                {
                    // No single-tile Y shift works — column is blocked.
                    float blockedFaceX = dir > 0
                        ? fpOriginX * 16f
                        : (fpOriginX + fpW) * 16f;
                    float dist = Math.Abs(blockedFaceX - originX) - sariaMargin;
                    fitResult = Math.Clamp(dist, 0f, logicalClose);
                    break;
                }

                currentOriginY = nextOriginY.Value;
            }

            // Use whichever constraint is tighter.
            return Math.Min(wallResult, fitResult);
        }

        private static bool IsSolidTile(int x, int y)
        {
            if ((uint)x >= (uint)Main.maxTilesX || (uint)y >= (uint)Main.maxTilesY)
                return true; // treat world boundary as solid
            Tile t = Main.tile[x, y];
            return t.HasTile
                && Main.tileSolid[t.TileType]
                && !Main.tileSolidTop[t.TileType]
                && t.Slope == SlopeType.Solid
                && !t.IsHalfBlock;
        }

        // Returns true if column col has at least 2 consecutive solid (non-slope, non-halfblock)
        // tiles starting at row rowStart and scanning downward.
        private static bool HasWallColumn(int col, int rowStart, int height = 2)
        {
            int count = 0;
            for (int r = rowStart; r < rowStart + height + 1 && count < height; r++)
            {
                if (IsSolidTile(col, r))
                    count++;
                else
                    count = 0;
            }
            return count >= height;
        }
    }
}
