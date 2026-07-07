using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace SariaMod.Items.Strange
{
    /// <summary>
    /// Owner-side A* tile pathfinder for Saria's Follow behaviour.
    /// Searches the tile grid for a walkable route from a start world position to a
    /// goal world position, fitting a player-sized footprint and treating only hard
    /// solid tiles as obstacles. Platforms, furniture and doors are passable.
    /// Pure calculation only — no movement, drawing or netcode lives here.
    /// </summary>
    public static class SariaPathfinder
    {
        // Footprint dimensions — public so CalcWallSafeClose can reference them without
        // duplicating the magic numbers.
        public const int FootprintWidthConst  = 2;
        public const int FootprintHeightConst = 3;

        private const float CardinalCost = 1f;
        private const float DiagonalCost = 1.41421356f;

        // Four-tier movement bias (penalty only affects A* ordering; no allowance cost):
        //   Liquid surface → 0f             (most preferred: walk along liquid top)
        //   Solid ground   → GroundPenalty  (normal floor)
        //   Platform       → PlatformPenalty (stand-on-top tile; preferred over open air)
        //   Air            → AirPenalty     (last resort)
        private const float AirPenalty      = 2.0f;
        private const float PlatformPenalty = 1.2f;
        private const float GroundPenalty   = 0.5f;

        // Safety cap on processed nodes so a blocked / unreachable goal can never
        // stall the game on the owner's client.
        private const int MaxExpandedNodes = 12000;

        // How far NudgeToOpen will spiral when the start or goal sits inside a wall.
        private const int NudgeRadius = 8;

        private static readonly Point[] Neighbors =
        {
            new Point( 1,  0), new Point(-1,  0), new Point( 0,  1), new Point( 0, -1),
            new Point( 1,  1), new Point( 1, -1), new Point(-1,  1), new Point(-1, -1),
        };

        /// <summary>
        /// Returns true if the given tile blocks movement. Out-of-world counts as solid.
        /// Platforms / stand-on-top tiles (tables, etc.), non-solid furniture, actuated
        /// tiles and doors are all treated as passable.
        /// </summary>
        public static bool IsTileBlocked(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Main.maxTilesX || y >= Main.maxTilesY)
                return true; // world boundary counts as solid

            Tile tile = Main.tile[x, y];
            if (!tile.HasTile || tile.IsActuated)
                return false;

            ushort type = tile.TileType;

            // Platforms and other stand-on-top tiles (tables, work benches) — not obstacles.
            if (Main.tileSolidTop[type])
                return false;

            // Anything that is not a full solid tile is passable (most furniture).
            if (!Main.tileSolid[type])
                return false;

            // Doors are explicitly passable.
            if (type == TileID.ClosedDoor || type == TileID.TallGateClosed)
                return false;

            return true;
        }

        /// <summary>
        /// Returns true if a footprint of the given tile size, anchored with its
        /// top-left at (originX, originY), is entirely clear of blocking tiles.
        /// </summary>
        public static bool FootprintFits(int originX, int originY, int wTiles, int hTiles)
        {
            for (int x = originX; x < originX + wTiles; x++)
                for (int y = originY; y < originY + hTiles; y++)
                    if (IsTileBlocked(x, y))
                        return false;
            return true;
        }

        /// <summary>
        /// Computes a walkable A* path from startWorld to goalWorld for a footprint of
        /// the given tile dimensions. Returns a list of world-space footprint-center
        /// positions (start -> goal), or null if no path within the allowance exists.
        /// </summary>
        public static List<Vector2> FindPath(Vector2 startWorld, Vector2 goalWorld,
            int footprintWidthTiles, int footprintHeightTiles, float allowanceTiles,
            bool secondForm = false)
        {
            if (footprintWidthTiles < 1) footprintWidthTiles = 1;
            if (footprintHeightTiles < 1) footprintHeightTiles = 1;

            Point start = WorldCenterToOrigin(startWorld, footprintWidthTiles, footprintHeightTiles);
            Point goal  = WorldCenterToOrigin(goalWorld,  footprintWidthTiles, footprintHeightTiles);

            // Nudge the start out of any solid it may be embedded in so the search can begin.
            if (!FootprintFits(start.X, start.Y, footprintWidthTiles, footprintHeightTiles))
            {
                start = NudgeToOpen(start, footprintWidthTiles, footprintHeightTiles);
                if (start.X == int.MinValue)
                    return null;
            }

            // If the goal itself is blocked, snap it to the nearest open footprint.
            if (!FootprintFits(goal.X, goal.Y, footprintWidthTiles, footprintHeightTiles))
            {
                goal = NudgeToOpen(goal, footprintWidthTiles, footprintHeightTiles);
                if (goal.X == int.MinValue)
                    return null;
            }

            if (start == goal)
                return new List<Vector2> { OriginToWorldCenter(start, footprintWidthTiles, footprintHeightTiles) };

            var open     = new PriorityQueue<Point, float>();
            var cameFrom = new Dictionary<Point, Point>();
            var gScore   = new Dictionary<Point, float>(); // penalized cost — drives A* ordering
            var rawG     = new Dictionary<Point, float>(); // raw tile distance — drives budget check
            var closed   = new HashSet<Point>();

            gScore[start] = 0f;
            rawG[start]   = 0f;
            open.Enqueue(start, Heuristic(start, goal));

            bool found = false;
            int expanded = 0;

            while (open.Count > 0)
            {
                Point current = open.Dequeue();

                // Lazy deletion: skip stale duplicates already finalized.
                if (!closed.Add(current))
                    continue;

                if (current == goal)
                {
                    found = true;
                    break;
                }

                if (++expanded > MaxExpandedNodes)
                    break;

                float curG    = gScore[current];
                float curRawG = rawG[current];

                for (int n = 0; n < Neighbors.Length; n++)
                {
                    Point step = Neighbors[n];
                    Point next = new Point(current.X + step.X, current.Y + step.Y);

                    if (closed.Contains(next))
                        continue;

                    if (!FootprintFits(next.X, next.Y, footprintWidthTiles, footprintHeightTiles))
                        continue;

                    bool diagonal = step.X != 0 && step.Y != 0;

                    // Corner-cut prevention: both orthogonal cells must be open too.
                    if (diagonal &&
                        (!FootprintFits(current.X + step.X, current.Y, footprintWidthTiles, footprintHeightTiles) ||
                         !FootprintFits(current.X, current.Y + step.Y, footprintWidthTiles, footprintHeightTiles)))
                        continue;

                    float rawStep  = diagonal ? DiagonalCost : CardinalCost;
                    float stepCost = rawStep
                                     + GetSupportPenalty(next.X, next.Y, footprintWidthTiles, footprintHeightTiles, secondForm);

                    float tentativeRawG = curRawG + rawStep;
                    if (tentativeRawG > allowanceTiles)   // budget uses raw tile distance, not penalized cost
                        continue;

                    float tentativeG = curG + stepCost;
                    if (!gScore.TryGetValue(next, out float knownG) || tentativeG < knownG)
                    {
                        gScore[next]   = tentativeG;
                        rawG[next]     = tentativeRawG;
                        cameFrom[next] = current;
                        open.Enqueue(next, tentativeG + Heuristic(next, goal));
                    }
                }
            }

            if (!found)
                return null;

            // Reconstruct the origin chain, then convert to world-space centers.
            var originPath = new List<Point>();
            Point node = goal;
            originPath.Add(node);
            while (cameFrom.TryGetValue(node, out Point prev))
            {
                node = prev;
                originPath.Add(node);
            }
            originPath.Reverse();

            var worldPath = new List<Vector2>(originPath.Count);
            for (int i = 0; i < originPath.Count; i++)
                worldPath.Add(OriginToWorldCenter(originPath[i], footprintWidthTiles, footprintHeightTiles));

            return worldPath;
        }

        // Octile-distance heuristic (admissible for 8-directional movement).
        private static float Heuristic(Point a, Point b)
        {
            int dx = Math.Abs(a.X - b.X);
            int dy = Math.Abs(a.Y - b.Y);
            int min = Math.Min(dx, dy);
            int max = Math.Max(dx, dy);
            return DiagonalCost * min + CardinalCost * (max - min);
        }

        // Converts a world-space center position to the footprint's top-left tile origin.
        private static Point WorldCenterToOrigin(Vector2 worldCenter, int wTiles, int hTiles)
        {
            int tileX = (int)Math.Floor(worldCenter.X / 16f) - wTiles / 2;
            int tileY = (int)Math.Floor(worldCenter.Y / 16f) - hTiles / 2;
            return new Point(tileX, tileY);
        }

        // Converts a footprint origin back to the world-space center of that footprint.
        private static Vector2 OriginToWorldCenter(Point origin, int wTiles, int hTiles)
        {
            return new Vector2(
                (origin.X + wTiles  * 0.5f) * 16f,
                (origin.Y + hTiles * 0.5f) * 16f);
        }

        // Returns the A* movement-bias penalty for the given footprint position.
        // Four tiers in increasing cost — A* naturally routes via the cheapest:
        //   Liquid surface (feet dry, liquid in the tile below) → 0f
        //   Solid ground (blocking tile directly beneath)       → GroundPenalty
        //   Platform (stand-on-top tile directly beneath)       → PlatformPenalty
        //   Air (nothing below)                                 → AirPenalty
        // Second form (Transform == 1) treats water as open space.
        // Non-water liquids (lava, honey) always get the surface discount.
        private static float GetSupportPenalty(int originX, int originY, int wTiles, int hTiles, bool secondForm)
        {
            int groundRow = originY + hTiles;
            int bottomRow = originY + hTiles - 1;

            bool hasLiquidSurface = false;
            bool hasGround        = false;
            bool hasPlatform      = false;

            for (int x = originX; x < originX + wTiles; x++)
            {
                if (IsTileBlocked(x, groundRow))
                {
                    hasGround = true;
                    continue;
                }

                if (x >= 0 && x < Main.maxTilesX &&
                    groundRow >= 0 && groundRow < Main.maxTilesY &&
                    bottomRow >= 0 && bottomRow < Main.maxTilesY)
                {
                    Tile below = Main.tile[x, groundRow];

                    // Liquid surface check (unchanged from before).
                    if (below.LiquidAmount > 0)
                    {
                        bool belowIsWater = below.LiquidType == 0;
                        if (!(secondForm && belowIsWater) && Main.tile[x, bottomRow].LiquidAmount == 0)
                            hasLiquidSurface = true;
                    }

                    // Platform check: stand-on-top tile that isn’t blocking (IsTileBlocked
                    // already returned false for it). Only counts when the footprint’s own
                    // bottom row isn’t already inside liquid, so she doesn’t prefer a
                    // platform she’d have to sink through liquid to reach.
                    if (below.HasTile && Main.tileSolidTop[below.TileType]
                        && Main.tile[x, bottomRow].LiquidAmount == 0)
                        hasPlatform = true;
                }
            }

            if (hasLiquidSurface) return 0f;
            if (hasGround)        return GroundPenalty;
            if (hasPlatform)      return PlatformPenalty;
            return AirPenalty;
        }

        // Spiral-searches outward for the nearest footprint origin that fits.
        // Returns Point(int.MinValue, int.MinValue) when none is found within NudgeRadius.
        public static Point NudgeToOpen(Point origin, int wTiles, int hTiles)
        {
            for (int r = 1; r <= NudgeRadius; r++)
            {
                for (int dx = -r; dx <= r; dx++)
                {
                    for (int dy = -r; dy <= r; dy++)
                    {
                        if (Math.Abs(dx) != r && Math.Abs(dy) != r)
                            continue; // only walk the ring at exactly distance r

                        int nx = origin.X + dx;
                        int ny = origin.Y + dy;
                        if (FootprintFits(nx, ny, wTiles, hTiles))
                            return new Point(nx, ny);
                    }
                }
            }
            return new Point(int.MinValue, int.MinValue);
        }

        // Same spiral search as NudgeToOpen but, within each radius ring, picks the
        // candidate whose footprint center is closest to the given world-space priority
        // target. Falls back to the plain nearest when no candidate fits.
        public static Point NudgeTowardTarget(Point origin, int wTiles, int hTiles, Vector2 priorityWorld)
        {
            for (int r = 1; r <= NudgeRadius; r++)
            {
                Point best  = new Point(int.MinValue, int.MinValue);
                float bestD = float.MaxValue;

                for (int dx = -r; dx <= r; dx++)
                {
                    for (int dy = -r; dy <= r; dy++)
                    {
                        if (Math.Abs(dx) != r && Math.Abs(dy) != r)
                            continue; // ring only

                        int nx = origin.X + dx;
                        int ny = origin.Y + dy;
                        if (!FootprintFits(nx, ny, wTiles, hTiles))
                            continue;

                        // World-space center of this candidate footprint.
                        Vector2 candidateWorld = new Vector2(
                            (nx + wTiles  * 0.5f) * 16f,
                            (ny + hTiles * 0.5f) * 16f);

                        float d = Vector2.DistanceSquared(candidateWorld, priorityWorld);
                        if (d < bestD)
                        {
                            bestD = d;
                            best  = new Point(nx, ny);
                        }
                    }
                }

                if (best.X != int.MinValue)
                    return best;
            }
            return new Point(int.MinValue, int.MinValue);
        }
    }
}
