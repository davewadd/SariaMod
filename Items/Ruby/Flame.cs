using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SariaMod.Buffs;
using SariaMod.Dusts;
using SariaMod.TileGlow;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace SariaMod.Items.Ruby
{
    public class Flame : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Saria");
            Main.projFrames[base.Projectile.type] = 6;
            ProjectileID.Sets.MinionShot[base.Projectile.type] = true;
            ProjectileID.Sets.DrawScreenCheckFluff[base.Projectile.type] = 128;
        }
        private int Timedown;
        private int Startup;

        // The visible pixels in the old cluster span about 107 pixels. A three-tile
        // placement radius plus the sprite bodies produces an approximately
        // 128-pixel visual diameter, about one tile wider than the old effect.
        private const int FootprintRadiusTiles = 3;
        private const int MaximumFlamePlacements = 19;
        private const int MovingFlameTextureDraws = 19;
        private const int SurfaceHitboxClearancePixels = 12;
        private const int WallSideDamageReachPixels = 12;
        private const float SurfaceVisualInsetPixels = 3.25f;
        private const float SurfaceMaximumVisualInsetPixels = 7.75f;
        internal const int SparsePopulationThreshold = 15;
        private const int NormalLifetimeUpdates = 1500;
        private const int SparseLifetimeUpdates = 2250;
        private const int BurningDurationTicks = 600;
        private const float FlameContactKnockback = 2f;
        private const int DayDurationTicks = 54000;
        private const int FullDayDurationTicks = 86400;
        // Each animation frame has a slightly different visible bottom edge.
        // Matching the origin to that edge keeps every frame resting on the tile
        // instead of floating when its randomized scale changes.
        private static readonly int[] FlameBodyBaselineSourceYs = { 34, 35, 37, 38, 38, 35 };

        private readonly List<FlamePlacement> flamePlacements = new List<FlamePlacement>();
        private Point footprintCenterTile;
        private bool footprintReady;
        private bool awaitingImpactSync;
        private bool impactEffectsPlayed;
        private bool movingVisualBudgetReserved;

        private readonly struct FlamePlacement
        {
            public readonly Point TileCoordinates;
            public readonly float OffsetX;
            public readonly float OffsetY;
            public readonly float Scale;
            public readonly float Rotation;
            public readonly int FrameSpeed;
            public readonly int FrameOffset;
            public readonly bool FlipHorizontally;

            public FlamePlacement(Point tileCoordinates, float offsetX, float offsetY, float scale,
                float rotation, int frameSpeed, int frameOffset, bool flipHorizontally)
            {
                TileCoordinates = tileCoordinates;
                OffsetX = offsetX;
                OffsetY = offsetY;
                Scale = scale;
                Rotation = rotation;
                FrameSpeed = frameSpeed;
                FrameOffset = frameOffset;
                FlipHorizontally = flipHorizontally;
            }
        }

        private readonly struct FlameTileCandidate
        {
            public readonly Point TileCoordinates;
            public readonly uint SortKey;

            public FlameTileCandidate(Point tileCoordinates, uint sortKey)
            {
                TileCoordinates = tileCoordinates;
                SortKey = sortKey;
            }
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(Startup);
            writer.Write(Projectile.timeLeft);
            writer.Write(Timedown);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            // Match SendExtraAI's existing wire order. This corrects the old
            // decoder without adding or changing any synchronized fields.
            int previousTimedown = Timedown;
            Startup = reader.ReadInt32();
            Projectile.timeLeft = reader.ReadInt32();
            Timedown = reader.ReadInt32();
            awaitingImpactSync = false;

            if (previousTimedown <= 0 && Timedown > 0)
            {
                PlayImpactEffects();
            }

            if (previousTimedown > 0 && Timedown <= 0)
            {
                flamePlacements.Clear();
                footprintReady = false;
            }
        }

        private void PlayImpactEffects()
        {
            if (impactEffectsPlayed)
            {
                return;
            }

            if (!Main.dedServ)
            {
                SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Ignite"), Projectile.Center);
            }

            impactEffectsPlayed = true;
        }
        public override void SetDefaults()
        {
            base.Projectile.width = 20;
            base.Projectile.height = 20;
            base.Projectile.netImportant = true;
            base.Projectile.friendly = true;
            base.Projectile.ignoreWater = true;
            base.Projectile.usesIDStaticNPCImmunity = true;
            base.Projectile.idStaticNPCHitCooldown = TileHeatManager.HeatDamageIntervalTicks;
            base.Projectile.minionSlots = 0f;
            base.Projectile.extraUpdates = 1;
            base.Projectile.aiStyle = 1;
            Projectile.alpha = 40;
            base.Projectile.penetrate = -1;
            base.Projectile.timeLeft = NormalLifetimeUpdates;
        }
        private const int sphereRadius = 20;

        private static bool IsFlameBackingTile(int tileX, int tileY)
        {
            if (!WorldGen.InWorld(tileX, tileY, 1))
            {
                return false;
            }

            Tile tile = Framing.GetTileSafely(tileX, tileY);
            if (!tile.HasTile || tile.IsActuated)
            {
                return false;
            }

            int tileType = tile.TileType;
            if (TileID.Sets.NotReallySolid[tileType])
            {
                return false;
            }

            bool isSolidBlock = Main.tileSolid[tileType] && !Main.tileSolidTop[tileType];
            bool isPlatform = TileID.Sets.Platforms[tileType];
            return isSolidBlock || isPlatform;
        }

        private static bool HasLiteralAirAbove(int tileX, int tileY)
        {
            if (!WorldGen.InWorld(tileX, tileY - 1, 1))
            {
                return false;
            }

            Tile tileAbove = Framing.GetTileSafely(tileX, tileY - 1);
            return (!tileAbove.HasTile || tileAbove.IsActuated) && tileAbove.LiquidAmount == 0;
        }

        private static bool HasOpenSpaceAbove(int tileX, int tileY)
        {
            return !IsFlameBackingTile(tileX, tileY - 1);
        }

        private static uint MixHash(uint value)
        {
            value ^= value >> 16;
            value *= 0x7FEB352Du;
            value ^= value >> 15;
            value *= 0x846CA68Bu;
            value ^= value >> 16;
            return value;
        }

        private uint GetFootprintSeed(Point centerTile)
        {
            uint seed = MixHash(unchecked((uint)(Projectile.identity + 1)));
            seed ^= MixHash(unchecked((uint)(Projectile.owner + 1)) * 0x9E3779B9u);
            seed ^= MixHash(unchecked((uint)centerTile.X) * 0x85EBCA6Bu);
            seed ^= MixHash(unchecked((uint)centerTile.Y) * 0xC2B2AE35u);
            return MixHash(seed);
        }

        private static uint GetTileSortKey(uint seed, int tileX, int tileY)
        {
            uint xHash = MixHash(unchecked((uint)tileX) * 0x8DA6B343u);
            uint yHash = MixHash(unchecked((uint)tileY) * 0xD8163841u);
            return MixHash(seed ^ xHash ^ yHash);
        }

        private Point FindImpactTile(Vector2 oldVelocity)
        {
            Vector2 impactDirection = oldVelocity.SafeNormalize(Vector2.UnitY);
            Vector2 impactProbe = Projectile.Center + impactDirection * 14f;
            Point probeTile = impactProbe.ToTileCoordinates();
            Point bestTile = Projectile.Center.ToTileCoordinates();
            float bestDistanceSquared = float.MaxValue;
            bool foundTile = false;

            for (int offsetY = -3; offsetY <= 3; offsetY++)
            {
                for (int offsetX = -3; offsetX <= 3; offsetX++)
                {
                    int tileX = probeTile.X + offsetX;
                    int tileY = probeTile.Y + offsetY;
                    if (!IsFlameBackingTile(tileX, tileY))
                    {
                        continue;
                    }

                    Vector2 tileCenter = new Vector2(tileX * 16f + 8f, tileY * 16f + 8f);
                    float distanceSquared = Vector2.DistanceSquared(tileCenter, impactProbe);
                    if (distanceSquared < bestDistanceSquared)
                    {
                        bestDistanceSquared = distanceSquared;
                        bestTile = new Point(tileX, tileY);
                        foundTile = true;
                    }
                }
            }

            return foundTile ? bestTile : Projectile.Center.ToTileCoordinates();
        }

        private void AnchorToImpactTile(Point impactTile)
        {
            Vector2 impactCenter = new Vector2(impactTile.X * 16f + 8f, impactTile.Y * 16f + 8f);
            Projectile.Center = impactCenter;
            Projectile.velocity = Vector2.Zero;
            Projectile.tileCollide = false;
            // ai[0] becomes the shared animation epoch once the owner lands.
            // It travels with the same projectile update as the exact tile-center anchor.
            Projectile.ai[0] = GetWorldAnimationTick();
            footprintReady = false;
            BuildFlameFootprint(impactTile);
            Projectile.netUpdate = true;
        }

        private static int GetWorldAnimationTick()
        {
            int worldTick = (int)Main.time;
            return Main.dayTime ? worldTick : DayDurationTicks + worldTick;
        }

        private int GetGroundedAnimationAge()
        {
            int elapsed = GetWorldAnimationTick() - (int)Projectile.ai[0];
            return elapsed >= 0 ? elapsed : elapsed + FullDayDurationTicks;
        }

        private void EnsureFlameFootprint()
        {
            if (Timedown <= 0)
            {
                return;
            }

            Point currentCenterTile = Projectile.Center.ToTileCoordinates();
            if (!footprintReady || currentCenterTile.X != footprintCenterTile.X || currentCenterTile.Y != footprintCenterTile.Y)
            {
                BuildFlameFootprint(currentCenterTile);
            }
        }

        private void BuildFlameFootprint(Point centerTile)
        {
            flamePlacements.Clear();
            footprintCenterTile = centerTile;
            footprintReady = true;

            uint seed = GetFootprintSeed(centerTile);
            List<FlameTileCandidate> literalAirCandidates = new List<FlameTileCandidate>();
            List<FlameTileCandidate> surfaceCandidates = new List<FlameTileCandidate>();
            List<FlameTileCandidate> buriedCandidates = new List<FlameTileCandidate>();

            for (int offsetY = -FootprintRadiusTiles; offsetY <= FootprintRadiusTiles; offsetY++)
            {
                for (int offsetX = -FootprintRadiusTiles; offsetX <= FootprintRadiusTiles; offsetX++)
                {
                    if (offsetX * offsetX + offsetY * offsetY > FootprintRadiusTiles * FootprintRadiusTiles)
                    {
                        continue;
                    }

                    int tileX = centerTile.X + offsetX;
                    int tileY = centerTile.Y + offsetY;
                    if (!IsFlameBackingTile(tileX, tileY))
                    {
                        continue;
                    }

                    FlameTileCandidate candidate = new FlameTileCandidate(
                        new Point(tileX, tileY),
                        GetTileSortKey(seed, tileX, tileY));

                    if (HasLiteralAirAbove(tileX, tileY))
                    {
                        literalAirCandidates.Add(candidate);
                    }
                    else if (HasOpenSpaceAbove(tileX, tileY))
                    {
                        surfaceCandidates.Add(candidate);
                    }
                    else
                    {
                        buriedCandidates.Add(candidate);
                    }
                }
            }

            Comparison<FlameTileCandidate> comparison = (left, right) =>
            {
                int keyComparison = left.SortKey.CompareTo(right.SortKey);
                if (keyComparison != 0)
                {
                    return keyComparison;
                }

                int xComparison = left.TileCoordinates.X.CompareTo(right.TileCoordinates.X);
                return xComparison != 0
                    ? xComparison
                    : left.TileCoordinates.Y.CompareTo(right.TileCoordinates.Y);
            };

            literalAirCandidates.Sort(comparison);
            surfaceCandidates.Sort(comparison);
            buriedCandidates.Sort(comparison);
            PrioritizeCenterCandidate(literalAirCandidates, centerTile);
            PrioritizeCenterCandidate(surfaceCandidates, centerTile);
            PrioritizeCenterCandidate(buriedCandidates, centerTile);

            int placementIndex = 0;
            AddCandidatePlacements(literalAirCandidates, ref placementIndex);
            AddCandidatePlacements(surfaceCandidates, ref placementIndex);

            // Give exposed tiles a second randomized flame before filling buried
            // tiles. This makes the preference visible, not just an ordering rule.
            AddCandidatePlacements(literalAirCandidates, ref placementIndex);
            AddCandidatePlacements(surfaceCandidates, ref placementIndex);
            AddCandidatePlacements(buriedCandidates, ref placementIndex);
        }

        private static void PrioritizeCenterCandidate(List<FlameTileCandidate> candidates, Point centerTile)
        {
            for (int i = 0; i < candidates.Count; i++)
            {
                Point tileCoordinates = candidates[i].TileCoordinates;
                if (tileCoordinates.X != centerTile.X || tileCoordinates.Y != centerTile.Y)
                {
                    continue;
                }

                FlameTileCandidate centerCandidate = candidates[i];
                candidates.RemoveAt(i);
                candidates.Insert(0, centerCandidate);
                return;
            }
        }

        private void AddCandidatePlacements(List<FlameTileCandidate> candidates, ref int placementIndex)
        {
            for (int i = 0; i < candidates.Count && flamePlacements.Count < MaximumFlamePlacements; i++)
            {
                AddFlamePlacement(candidates[i], placementIndex++);
            }
        }

        private void AddFlamePlacement(FlameTileCandidate candidate, int placementIndex)
        {
            uint style = MixHash(candidate.SortKey ^ unchecked((uint)(placementIndex + 1)) * 0x9E3779B9u);

            style = MixHash(style + 0x9E3779B9u);
            float offsetX = MathHelper.Lerp(-3f, 3f, (style & 0x00FFFFFFu) / 16777216f);

            style = MixHash(style + 0x9E3779B9u);
            float offsetY = MathHelper.Lerp(-3f, 3f, (style & 0x00FFFFFFu) / 16777216f);

            style = MixHash(style + 0x9E3779B9u);
            float scaleRoll = (style & 0x00FFFFFFu) / 16777216f;
            float scale = MathHelper.Lerp(0.65f, 1.35f, scaleRoll * scaleRoll);

            style = MixHash(style + 0x9E3779B9u);
            float rotation = MathHelper.Lerp(-0.08f, 0.08f, (style & 0x00FFFFFFu) / 16777216f);

            style = MixHash(style + 0x9E3779B9u);
            int frameSpeed = 5 + (int)(style % 6u);

            style = MixHash(style + 0x9E3779B9u);
            int frameOffset = (int)(style % 6u);

            style = MixHash(style + 0x9E3779B9u);
            bool flipHorizontally = (style & 1u) != 0u;

            flamePlacements.Add(new FlamePlacement(
                candidate.TileCoordinates,
                offsetX,
                offsetY,
                scale,
                rotation,
                frameSpeed,
                frameOffset,
                flipHorizontally));
        }

        private static float GetTileSurfaceY(int tileX, int tileY, float localX)
        {
            Tile tile = Framing.GetTileSafely(tileX, tileY);
            float tileTop = tileY * 16f;
            float clampedLocalX = MathHelper.Clamp(localX, 0f, 15f);

            if (tile.IsHalfBlock)
            {
                return tileTop + 8f;
            }

            if (tile.Slope == SlopeType.SlopeDownLeft)
            {
                return tileTop + clampedLocalX;
            }

            if (tile.Slope == SlopeType.SlopeDownRight)
            {
                return tileTop + 15f - clampedLocalX;
            }

            return tileTop;
        }

        private static float GetVerticalClearancePixels(int tileX, int tileY, float surfaceY)
        {
            const int MaximumClearanceTiles = 4;
            float openTop = surfaceY - MaximumClearanceTiles * 16f;

            for (int scanY = tileY - 1; scanY >= tileY - MaximumClearanceTiles; scanY--)
            {
                if (IsFlameBackingTile(tileX, scanY))
                {
                    openTop = (scanY + 1) * 16f;
                    break;
                }
            }

            return Math.Max(0f, surfaceY - openTop);
        }

        private bool TryResolvePlacementGeometry(FlamePlacement placement, out Point tileCoordinates,
            out float localX, out bool hasOpenSpaceAbove, out float surfaceY)
        {
            tileCoordinates = placement.TileCoordinates;
            if (!IsFlameBackingTile(tileCoordinates.X, tileCoordinates.Y))
            {
                localX = 0f;
                hasOpenSpaceAbove = false;
                surfaceY = 0f;
                return false;
            }

            localX = MathHelper.Clamp(8f + placement.OffsetX, 2f, 14f);
            hasOpenSpaceAbove = HasOpenSpaceAbove(tileCoordinates.X, tileCoordinates.Y);
            surfaceY = hasOpenSpaceAbove
                ? GetTileSurfaceY(tileCoordinates.X, tileCoordinates.Y, localX)
                : tileCoordinates.Y * 16f + 8f;
            return true;
        }

        private bool TryResolveFlameVisual(FlamePlacement placement, out Point tileCoordinates,
            out float localX, out bool hasOpenSpaceAbove, out float surfaceY, out Vector2 worldPosition)
        {
            if (!TryResolvePlacementGeometry(placement, out tileCoordinates, out localX,
                out hasOpenSpaceAbove, out surfaceY))
            {
                worldPosition = Vector2.Zero;
                return false;
            }

            if (hasOpenSpaceAbove)
            {
                float depthGradient = MathHelper.Clamp((placement.OffsetY + 3f) / 6f, 0f, 1f);
                float surfaceInset = MathHelper.Lerp(
                    SurfaceVisualInsetPixels,
                    SurfaceMaximumVisualInsetPixels,
                    depthGradient);
                worldPosition = new Vector2(
                    tileCoordinates.X * 16f + localX,
                    surfaceY + surfaceInset);
            }
            else
            {
                worldPosition = new Vector2(
                    tileCoordinates.X * 16f + 8f + placement.OffsetX,
                    tileCoordinates.Y * 16f + 8f + placement.OffsetY);
            }

            return true;
        }

        private void SpawnGroundedFlameDust()
        {
            if (Main.dedServ || flamePlacements.Count <= 0)
            {
                return;
            }

            // Every currently drawable flame position gets an equal chance to emit
            // the original grounded dust pair. Dividing the old 1-in-30 rate across
            // all placements keeps the total density close to its previous value.
            int dustChance = Math.Max(1, 30 * flamePlacements.Count);
            for (int i = 0; i < flamePlacements.Count; i++)
            {
                if (!TryResolveFlameVisual(flamePlacements[i], out _, out _, out _, out _,
                    out Vector2 worldPosition) || !Main.rand.NextBool(dustChance))
                {
                    continue;
                }

                float smokeRadius = (float)Math.Sqrt(Main.rand.Next(sphereRadius * sphereRadius));
                double smokeAngle = Main.rand.NextDouble() * 5.0 * Math.PI;
                Dust.NewDust(
                    new Vector2(
                        worldPosition.X + smokeRadius * (float)Math.Cos(smokeAngle),
                        worldPosition.Y - 15f + smokeRadius * (float)Math.Sin(smokeAngle)),
                    0, 0, ModContent.DustType<SmokeDust7>(), 0f, 0f, 0, default(Color), 1.5f);

                double flameAngle = Main.rand.NextDouble() * 5.0 * Math.PI;
                Dust.NewDust(
                    new Vector2(
                        worldPosition.X + 40f * (float)Math.Cos(flameAngle),
                        worldPosition.Y - 3f + 10f * (float)Math.Sin(flameAngle)),
                    0, 0, ModContent.DustType<FlameDust2>(), 0f, 0f, 0, default(Color), 1.5f);
            }
        }

        private bool TryGetPlacementHitbox(FlamePlacement placement, out Rectangle hitbox)
        {
            if (!TryResolvePlacementGeometry(placement, out Point tileCoordinates, out float localX,
                out bool hasOpenSpaceAbove, out float surfaceY))
            {
                hitbox = Rectangle.Empty;
                return false;
            }

            int tileLeft = tileCoordinates.X * 16;
            int tileTop = tileCoordinates.Y * 16;
            if (!hasOpenSpaceAbove)
            {
                hitbox = new Rectangle(tileLeft, tileTop, 16, 16);
                return true;
            }

            int hitboxWidth = Math.Clamp((int)Math.Round(9f + placement.Scale * 4f), 8, 16);
            int placementCenterX = (int)Math.Round(tileLeft + localX);
            int hitboxLeft = Math.Clamp(placementCenterX - hitboxWidth / 2, tileLeft, tileLeft + 16 - hitboxWidth);
            float leftLocalX = hitboxLeft - tileLeft;
            float rightLocalX = leftLocalX + hitboxWidth - 1;
            float leftSurfaceY = GetTileSurfaceY(tileCoordinates.X, tileCoordinates.Y, leftLocalX);
            float rightSurfaceY = GetTileSurfaceY(tileCoordinates.X, tileCoordinates.Y, rightLocalX);
            int hitboxBottom = (int)Math.Floor(Math.Min(surfaceY, Math.Min(leftSurfaceY, rightSurfaceY))) + 1;
            int hitboxTop = hitboxBottom - SurfaceHitboxClearancePixels;
            hitbox = new Rectangle(hitboxLeft, hitboxTop, hitboxWidth, SurfaceHitboxClearancePixels);
            return true;
        }

        private bool TryGetPlacementDamageHitbox(FlamePlacement placement, out Rectangle hitbox)
        {
            if (!TryGetPlacementHitbox(placement, out hitbox))
            {
                return false;
            }

            Point tileCoordinates = placement.TileCoordinates;
            if (HasOpenSpaceAbove(tileCoordinates.X, tileCoordinates.Y))
            {
                return true;
            }

            // A flame drawn inside a wall still visibly reaches an enemy standing
            // beside an exposed wall face. Extend only into horizontally open
            // neighboring space, leaving fully buried tiles confined to themselves.
            bool openLeft = !IsFlameBackingTile(tileCoordinates.X - 1, tileCoordinates.Y);
            bool openRight = !IsFlameBackingTile(tileCoordinates.X + 1, tileCoordinates.Y);
            if (openLeft)
            {
                hitbox.X -= WallSideDamageReachPixels;
                hitbox.Width += WallSideDamageReachPixels;
            }

            if (openRight)
            {
                hitbox.Width += WallSideDamageReachPixels;
            }

            return true;
        }

        private bool FootprintTouchesWater()
        {
            if (Timedown <= 0)
            {
                return Collision.WetCollision(Projectile.position, Projectile.width, Projectile.height);
            }

            EnsureFlameFootprint();
            for (int i = 0; i < flamePlacements.Count; i++)
            {
                if (!TryGetPlacementHitbox(flamePlacements[i], out Rectangle hitbox))
                {
                    continue;
                }

                if (Collision.WetCollision(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height))
                {
                    return true;
                }
            }

            return false;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Timedown <= 0)
            {
                if (Main.myPlayer == Projectile.owner)
                {
                    Point impactTile = FindImpactTile(oldVelocity);
                    Timedown = 280;
                    AnchorToImpactTile(impactTile);
                    PlayImpactEffects();
                }
                else
                {
                    // Only the owning client chooses and synchronizes the anchor.
                    // Other simulators pause their prediction until that update arrives.
                    awaitingImpactSync = true;
                    Projectile.tileCollide = false;
                }
            }
            if (Main.rand.NextBool(30))
            {
                {
                    float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius * sphereRadius));
                    double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                    Dust.NewDust(new Vector2(Projectile.Center.X + radius * (float)Math.Cos(angle), (Projectile.Center.Y - 15) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<SmokeDust7>(), 0f, 0f, 0, default(Color), 1.5f);
                }
                {
                    float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius * sphereRadius));
                    double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                    Dust.NewDust(new Vector2(Projectile.Center.X + 40 * (float)Math.Cos(angle), (Projectile.Center.Y - 3) + 10 * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<FlameDust2>(), 0f, 0f, 0, default(Color), 1.5f);
                }
            }
            Projectile.velocity = Vector2.Zero;
            return false;
        }
        public override bool? CanCutTiles()
        {
            return false;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (Timedown <= 0)
            {
                return null;
            }

            EnsureFlameFootprint();
            for (int i = 0; i < flamePlacements.Count; i++)
            {
                if (TryGetPlacementDamageHitbox(flamePlacements[i], out Rectangle flameHitbox)
                    && flameHitbox.Intersects(targetHitbox))
                {
                    return true;
                }
            }

            return false;
        }

        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            Player player = Main.player[Projectile.owner];
            FairyPlayer modPlayer = player.Fairy();
            knockback = target.type == NPCID.TargetDummy ? 0f : FlameContactKnockback;
            target.buffImmune[BuffID.CursedInferno] = false;
            target.buffImmune[BuffID.Confused] = false;
            target.buffImmune[BuffID.Slow] = false;
            target.buffImmune[BuffID.ShadowFlame] = false;
            target.buffImmune[BuffID.Ichor] = false;
            target.buffImmune[BuffID.Frostburn] = false;
            target.buffImmune[BuffID.Poisoned] = false;
            target.buffImmune[BuffID.Venom] = false;
            target.buffImmune[BuffID.Electrified] = false;
            target.buffImmune[ModContent.BuffType<Burning2>()] = false;
            target.AddBuff(ModContent.BuffType<Burning2>(), BurningDurationTicks);
            modPlayer.SariaXp++;
        }
        public override bool MinionContactDamage()
        {
            return true;
        }

        public override bool ShouldUpdatePosition()
        {
            return Timedown <= 0 && !awaitingImpactSync;
        }

        // ai[1] is chosen by the spawning owner and synchronized by tModLoader.
        // Flame never used the old parent-projectile index stored in this slot.
        private bool UsesSparseLifetime => Projectile.ai[1] >= 0.5f;

        private int ConfiguredLifetimeUpdates => UsesSparseLifetime
            ? SparseLifetimeUpdates
            : NormalLifetimeUpdates;

        private void ConfigureLifetime()
        {
            if (Startup != 0)
            {
                return;
            }

            Startup = 1;
            Projectile.timeLeft = ConfiguredLifetimeUpdates;
        }

        public override void AI()
        {
            ConfigureLifetime();
            Player player = Main.player[Projectile.owner];
            Player player2 = Main.LocalPlayer;
            FairyPlayer modPlayer = player.Fairy();
            Projectile.SariaBaseDamage();
            Projectile.damage /= 15;
            Projectile.knockBack = FlameContactKnockback;
            if (Timedown == 0)
            {
                Projectile.tileCollide = !awaitingImpactSync;
            }
            if (Timedown > 0)
            {
                Projectile.velocity.Y = 0f;
                Projectile.velocity.X = 0f;
                EnsureFlameFootprint();
                SpawnGroundedFlameDust();
                Projectile.tileCollide = false;
            }
            bool hasRemovalAuthority = Main.netMode != NetmodeID.MultiplayerClient || Main.myPlayer == Projectile.owner;
            if (hasRemovalAuthority && FootprintTouchesWater())
            {
                for (int i = 0; i < 5; i++)
                {
                    Vector2 speed = Main.rand.NextVector2CircularEdge(1f, 1f);
                    Dust d = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<SmokeDust6>(), speed * 13, Scale: 3.5f);
                    d.noGravity = true;
                }
                for (int i = 0; i < 5; i++)
                {
                    Vector2 speed = Main.rand.NextVector2CircularEdge(1f, 1f);
                    Dust d = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<BubbleDust2>(), speed * 13, Scale: 3.5f);
                    d.noGravity = true;
                }
                SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/mist"), Projectile.Center);
                Projectile.Kill();
            }
            if (Math.Abs(Projectile.velocity.X) > 0f && Math.Abs(Projectile.velocity.Y) > 0f)
            {
                {
                    float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius * sphereRadius));
                    double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                    Dust.NewDust(new Vector2(Projectile.Center.X + radius * (float)Math.Cos(angle), (Projectile.Center.Y - 15) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<SmokeDust7>(), 0f, 0f, 0, default(Color), 1.5f);
                }
            }
            // friendly needs to be set to true so the minion can deal contact damage
            // friendly needs to be set to false so it doesn't damage things like target dummies while idling
            // Both things depend on if it has a target or not, so it's just one assignment here
            // You don't need this assignment if your minion is shooting things instead of dealing contact damage
            Lighting.AddLight(Projectile.Center, Color.OrangeRed.ToVector3() * 2f);
            // Default movement parameters (here for attacking)
            {
                float between = Vector2.Distance(player2.Center, Projectile.Center);
                // Reasonable distance away so it doesn't target across multiple screens
                if (between < 600f)
                {
                    player2.AddBuff(BuffID.Campfire, 30);
                }
            }
            int frameSpeed = 15;
            {
                base.Projectile.frameCounter++;
                if (Projectile.frameCounter >= frameSpeed)
                    if (base.Projectile.frameCounter > 6)
                    {
                        base.Projectile.frame++;
                        base.Projectile.frameCounter = 0;
                    }
                if (base.Projectile.frame >= 6)
                {
                    base.Projectile.frame = 0;
                }
            }
        }
        public override Color? GetAlpha(Color lightColor)
        {
            if (base.Projectile.timeLeft < 85)
            {
                byte b2 = (byte)(base.Projectile.timeLeft * 3);
                byte a2 = (byte)(100f * ((float)(int)b2 / 255f));
                return new Color(b2, b2, b2, a2);
            }
            return new Color(255, 255, 255, 100);
        }

        private void DrawFlameFootprint(Color lightColor)
        {
            EnsureFlameFootprint();
            Texture2D texture = TextureAssets.Projectile[ModContent.ProjectileType<Flame>()].Value;
            Color drawColor = Color.Lerp(lightColor, Color.Pink, 20f);
            drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0f);

            for (int i = 0; i < flamePlacements.Count; i++)
            {
                FlamePlacement placement = flamePlacements[i];
                if (!TryResolveFlameVisual(placement, out Point tileCoordinates, out _,
                    out bool hasOpenSpaceAbove, out float surfaceY, out Vector2 worldPosition))
                {
                    continue;
                }

                int visualAge = GetGroundedAnimationAge();
                int frameIndex = (visualAge / placement.FrameSpeed + placement.FrameOffset) % 6;
                Rectangle sourceRectangle = texture.Frame(verticalFrames: 6, frameY: frameIndex);
                int bodyBaselineSourceY = FlameBodyBaselineSourceYs[frameIndex];
                sourceRectangle.Height = bodyBaselineSourceY;
                Vector2 origin;
                float drawScale = placement.Scale;

                if (hasOpenSpaceAbove)
                {
                    float verticalClearance = GetVerticalClearancePixels(tileCoordinates.X, tileCoordinates.Y, surfaceY);
                    drawScale = Math.Min(drawScale, Math.Max(0.4f, verticalClearance / bodyBaselineSourceY));
                    origin = new Vector2(sourceRectangle.Width * 0.5f, bodyBaselineSourceY);
                }
                else
                {
                    origin = sourceRectangle.Size() * 0.5f;
                }

                // Flame.png faces downward in its unrotated source orientation.
                // Grounded flames need to face away from their supporting tile.
                SpriteEffects spriteEffects = SpriteEffects.FlipVertically;
                if (placement.FlipHorizontally)
                {
                    spriteEffects |= SpriteEffects.FlipHorizontally;
                }

                Main.spriteBatch.Draw(
                    texture,
                    worldPosition - Main.screenPosition,
                    sourceRectangle,
                    base.Projectile.GetAlpha(drawColor),
                    placement.Rotation,
                    origin,
                    drawScale,
                    spriteEffects,
                    0f);

                Lighting.AddLight(worldPosition, Color.HotPink.ToVector3() * (0.3f + drawScale * 0.15f));
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // The moving projectile retains its existing sprite and clustered
            // afterimages. Once grounded, only tile-backed placements are drawn.
            movingVisualBudgetReserved = Timedown <= 0
                && EruptionVisualBudget.TryReserveFlameTextureDraws(MovingFlameTextureDraws);
            return movingVisualBudgetReserved;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            overPlayers.Add(index);
        }
        public override void PostDraw(Color lightColor)
        {
            if (Timedown > 0)
            {
                EnsureFlameFootprint();
                if (!EruptionVisualBudget.TryReserveFlameTextureDraws(flamePlacements.Count))
                {
                    return;
                }

                DrawFlameFootprint(lightColor);
                return;
            }

            if (!movingVisualBudgetReserved)
            {
                return;
            }

            Lighting.AddLight(Projectile.Center, Color.HotPink.ToVector3() * 0.78f);

            {
                Texture2D texture = TextureAssets.Projectile[ModContent.ProjectileType<Flame>()].Value;
                Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                int frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<Flame>()];
                int frameY = frameHeight * Main.projFrames[ModContent.ProjectileType<Flame>()];
                Color drawColor = Color.Lerp(lightColor, Color.Pink, 20f);
                drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                Rectangle rectangle = texture.Frame(verticalFrames: 6, frameY: (int)Main.GameUpdateCount / 7 % 6);
                Vector2 origin = rectangle.Size() / 2f;
                float rotation = base.Projectile.rotation;
                float scale = base.Projectile.scale * 1.9f;
                SpriteEffects spriteEffects = SpriteEffects.None;
                if (base.Projectile.spriteDirection == 1)
                {
                    startPos.Y -= 18;
                    startPos.X += 13;
                }
                Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
            }
            {
                Texture2D texture = TextureAssets.Projectile[ModContent.ProjectileType<Flame>()].Value;
                Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                int frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<Flame>()];
                int frameY = frameHeight * Main.projFrames[ModContent.ProjectileType<Flame>()];
                Color drawColor = Color.Lerp(lightColor, Color.Pink, 20f);
                drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                Rectangle rectangle = texture.Frame(verticalFrames: 6, frameY: (int)Main.GameUpdateCount / 8 % 6);
                Vector2 origin = rectangle.Size() / 2f;
                float rotation = base.Projectile.rotation;
                float scale = base.Projectile.scale;
                SpriteEffects spriteEffects = SpriteEffects.None;
                if (base.Projectile.spriteDirection == 1)
                {
                    startPos.Y -= 16;
                    startPos.X -= 32;
                }
                Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
            }
            {
                Texture2D texture = TextureAssets.Projectile[ModContent.ProjectileType<Flame>()].Value;
                Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                int frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<Flame>()];
                int frameY = frameHeight * Main.projFrames[ModContent.ProjectileType<Flame>()];
                Color drawColor = Color.Lerp(lightColor, Color.Pink, 20f);
                drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                Rectangle rectangle = texture.Frame(verticalFrames: 6, frameY: (int)Main.GameUpdateCount / 6 % 6);
                Vector2 origin = rectangle.Size() / 2f;
                float rotation = base.Projectile.rotation;
                float scale = base.Projectile.scale;
                SpriteEffects spriteEffects = SpriteEffects.None;
                if (base.Projectile.spriteDirection == 1)
                {
                    startPos.Y -= 16;
                    startPos.X += 32;
                }
                Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
            }
            {
                Texture2D texture = TextureAssets.Projectile[ModContent.ProjectileType<Flame>()].Value;
                Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                int frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<Flame>()];
                int frameY = frameHeight * Main.projFrames[ModContent.ProjectileType<Flame>()];
                Color drawColor = Color.Lerp(lightColor, Color.Pink, 20f);
                drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                Rectangle rectangle = texture.Frame(verticalFrames: 6, frameY: (int)Main.GameUpdateCount / 4 % 6);
                Vector2 origin = rectangle.Size() / 2f;
                float rotation = base.Projectile.rotation;
                float scale = base.Projectile.scale * 1.3f;
                SpriteEffects spriteEffects = SpriteEffects.None;
                if (base.Projectile.spriteDirection == 1)
                {
                    startPos.Y -= 22;
                    startPos.X -= +27;
                }
                Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
            }
            {
                Texture2D texture = TextureAssets.Projectile[ModContent.ProjectileType<Flame>()].Value;
                Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                int frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<Flame>()];
                int frameY = frameHeight * Main.projFrames[ModContent.ProjectileType<Flame>()];
                Color drawColor = Color.Lerp(lightColor, Color.Pink, 20f);
                drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                Rectangle rectangle = texture.Frame(verticalFrames: 6, frameY: (int)Main.GameUpdateCount / 5 % 6);
                Vector2 origin = rectangle.Size() / 2f;
                float rotation = base.Projectile.rotation;
                float scale = base.Projectile.scale * 1.8f;
                SpriteEffects spriteEffects = SpriteEffects.None;
                if (base.Projectile.spriteDirection == 1)
                {
                    startPos.Y -= 11;
                    startPos.X -= +26;
                }
                Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
            }
            {
                Texture2D texture = TextureAssets.Projectile[ModContent.ProjectileType<Flame>()].Value;
                Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                int frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<Flame>()];
                int frameY = frameHeight * Main.projFrames[ModContent.ProjectileType<Flame>()];
                Color drawColor = Color.Lerp(lightColor, Color.Pink, 20f);
                drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                Rectangle rectangle = texture.Frame(verticalFrames: 6, frameY: (int)Main.GameUpdateCount / 7 % 6);
                Vector2 origin = rectangle.Size() / 2f;
                float rotation = base.Projectile.rotation;
                float scale = base.Projectile.scale;
                SpriteEffects spriteEffects = SpriteEffects.None;
                if (base.Projectile.spriteDirection == 1)
                {
                    startPos.Y -= 10;
                    startPos.X -= +22;
                }
                Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
            }
            {
                Texture2D texture = TextureAssets.Projectile[ModContent.ProjectileType<Flame>()].Value;
                Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                int frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<Flame>()];
                int frameY = frameHeight * Main.projFrames[ModContent.ProjectileType<Flame>()];
                Color drawColor = Color.Lerp(lightColor, Color.Pink, 20f);
                drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                Rectangle rectangle = texture.Frame(verticalFrames: 6, frameY: (int)Main.GameUpdateCount / 6 % 6);
                Vector2 origin = rectangle.Size() / 2f;
                float rotation = base.Projectile.rotation;
                float scale = base.Projectile.scale;
                SpriteEffects spriteEffects = SpriteEffects.None;
                if (base.Projectile.spriteDirection == 1)
                {
                    startPos.Y -= 10;
                    startPos.X += +22;
                }
                Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
            }
            {
                Texture2D texture = TextureAssets.Projectile[ModContent.ProjectileType<Flame>()].Value;
                Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                int frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<Flame>()];
                int frameY = frameHeight * Main.projFrames[ModContent.ProjectileType<Flame>()];
                Color drawColor = Color.Lerp(lightColor, Color.Pink, 20f);
                drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                Rectangle rectangle = texture.Frame(verticalFrames: 6, frameY: (int)Main.GameUpdateCount / 5 % 6);
                Vector2 origin = rectangle.Size() / 2f;
                float rotation = base.Projectile.rotation;
                float scale = base.Projectile.scale;
                SpriteEffects spriteEffects = SpriteEffects.None;
                if (base.Projectile.spriteDirection == 1)
                {
                    startPos.Y -= 6;
                    startPos.X -= +3;
                }
                Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
            }
            {
                Texture2D texture = TextureAssets.Projectile[ModContent.ProjectileType<Flame>()].Value;
                Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                int frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<Flame>()];
                int frameY = frameHeight * Main.projFrames[ModContent.ProjectileType<Flame>()];
                Color drawColor = Color.Lerp(lightColor, Color.Pink, 20f);
                drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                Rectangle rectangle = texture.Frame(verticalFrames: 6, frameY: (int)Main.GameUpdateCount / 6 % 6);
                Vector2 origin = rectangle.Size() / 2f;
                float rotation = base.Projectile.rotation;
                float scale = base.Projectile.scale;
                SpriteEffects spriteEffects = SpriteEffects.None;
                if (base.Projectile.spriteDirection == 1)
                {
                    startPos.Y -= 20;
                    startPos.X -= +5;
                }
                Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
            }
            {
                Texture2D texture = TextureAssets.Projectile[ModContent.ProjectileType<Flame>()].Value;
                Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                int frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<Flame>()];
                int frameY = frameHeight * Main.projFrames[ModContent.ProjectileType<Flame>()];
                Color drawColor = Color.Lerp(lightColor, Color.Pink, 20f);
                drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                Rectangle rectangle = texture.Frame(verticalFrames: 6, frameY: (int)Main.GameUpdateCount / 5 % 6);
                Vector2 origin = rectangle.Size() / 2f;
                float rotation = base.Projectile.rotation;
                float scale = base.Projectile.scale;
                SpriteEffects spriteEffects = SpriteEffects.None;
                if (base.Projectile.spriteDirection == 1)
                {
                    startPos.Y -= 14;
                    startPos.X -= +22;
                }
                Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
            }
            {
                Texture2D texture = TextureAssets.Projectile[ModContent.ProjectileType<Flame>()].Value;
                Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                int frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<Flame>()];
                int frameY = frameHeight * Main.projFrames[ModContent.ProjectileType<Flame>()];
                Color drawColor = Color.Lerp(lightColor, Color.Pink, 20f);
                drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                Rectangle rectangle = texture.Frame(verticalFrames: 6, frameY: (int)Main.GameUpdateCount / 7 % 6);
                Vector2 origin = rectangle.Size() / 2f;
                float rotation = base.Projectile.rotation;
                float scale = base.Projectile.scale;
                SpriteEffects spriteEffects = SpriteEffects.None;
                if (base.Projectile.spriteDirection == 1)
                {
                    startPos.Y -= 2;
                    startPos.X -= +16;
                }
                Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
            }
            {
                Texture2D texture = TextureAssets.Projectile[ModContent.ProjectileType<Flame>()].Value;
                Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                int frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<Flame>()];
                int frameY = frameHeight * Main.projFrames[ModContent.ProjectileType<Flame>()];
                Color drawColor = Color.Lerp(lightColor, Color.Pink, 20f);
                drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                Rectangle rectangle = texture.Frame(verticalFrames: 6, frameY: (int)Main.GameUpdateCount / 7 % 6);
                Vector2 origin = rectangle.Size() / 2f;
                float rotation = base.Projectile.rotation;
                float scale = base.Projectile.scale;
                SpriteEffects spriteEffects = SpriteEffects.None;
                if (base.Projectile.spriteDirection == 1)
                {
                    startPos.Y -= 2;
                    startPos.X += +16;
                }
                Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
            }
            {
                Texture2D texture = TextureAssets.Projectile[ModContent.ProjectileType<Flame>()].Value;
                Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                int frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<Flame>()];
                int frameY = frameHeight * Main.projFrames[ModContent.ProjectileType<Flame>()];
                Color drawColor = Color.Lerp(lightColor, Color.Pink, 20f);
                drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                Rectangle rectangle = texture.Frame(verticalFrames: 6, frameY: (int)Main.GameUpdateCount / 5 % 6);
                Vector2 origin = rectangle.Size() / 2f;
                float rotation = base.Projectile.rotation;
                float scale = base.Projectile.scale;
                SpriteEffects spriteEffects = SpriteEffects.None;
                if (base.Projectile.spriteDirection == 1)
                {
                    startPos.Y -= 7;
                    startPos.X += +17;
                }
                Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
            }
            {
                Texture2D texture = TextureAssets.Projectile[ModContent.ProjectileType<Flame>()].Value;
                Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                int frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<Flame>()];
                int frameY = frameHeight * Main.projFrames[ModContent.ProjectileType<Flame>()];
                Color drawColor = Color.Lerp(lightColor, Color.Pink, 20f);
                drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                Rectangle rectangle = texture.Frame(verticalFrames: 6, frameY: (int)Main.GameUpdateCount / 6 % 6);
                Vector2 origin = rectangle.Size() / 2f;
                float rotation = base.Projectile.rotation;
                float scale = base.Projectile.scale;
                SpriteEffects spriteEffects = SpriteEffects.None;
                if (base.Projectile.spriteDirection == 1)
                {
                    startPos.Y -= 7;
                    startPos.X -= +17;
                }
                Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
            }
            {
                Texture2D texture = TextureAssets.Projectile[ModContent.ProjectileType<Flame>()].Value;
                Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                int frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<Flame>()];
                int frameY = frameHeight * Main.projFrames[ModContent.ProjectileType<Flame>()];
                Color drawColor = Color.Lerp(lightColor, Color.Pink, 20f);
                drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                Rectangle rectangle = texture.Frame(verticalFrames: 6, frameY: (int)Main.GameUpdateCount / 6 % 6);
                Vector2 origin = rectangle.Size() / 2f;
                float rotation = base.Projectile.rotation;
                float scale = base.Projectile.scale;
                SpriteEffects spriteEffects = SpriteEffects.None;
                if (base.Projectile.spriteDirection == 1)
                {
                    startPos.Y -= 14;
                    startPos.X += +17;
                }
                Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
            }
            {
                Texture2D texture = TextureAssets.Projectile[ModContent.ProjectileType<Flame>()].Value;
                Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                int frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<Flame>()];
                int frameY = frameHeight * Main.projFrames[ModContent.ProjectileType<Flame>()];
                Color drawColor = Color.Lerp(lightColor, Color.Pink, 20f);
                drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                Rectangle rectangle = texture.Frame(verticalFrames: 6, frameY: (int)Main.GameUpdateCount / 5 % 6);
                Vector2 origin = rectangle.Size() / 2f;
                float rotation = base.Projectile.rotation;
                float scale = base.Projectile.scale;
                SpriteEffects spriteEffects = SpriteEffects.None;
                if (base.Projectile.spriteDirection == 1)
                {
                    startPos.Y -= 24;
                    startPos.X += +19;
                }
                Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
            }
            {
                Texture2D texture = TextureAssets.Projectile[ModContent.ProjectileType<Flame>()].Value;
                Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                int frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<Flame>()];
                int frameY = frameHeight * Main.projFrames[ModContent.ProjectileType<Flame>()];
                Color drawColor = Color.Lerp(lightColor, Color.Pink, 20f);
                drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                Rectangle rectangle = texture.Frame(verticalFrames: 6, frameY: (int)Main.GameUpdateCount / 7 % 6);
                Vector2 origin = rectangle.Size() / 2f;
                float rotation = base.Projectile.rotation;
                float scale = base.Projectile.scale;
                SpriteEffects spriteEffects = SpriteEffects.None;
                if (base.Projectile.spriteDirection == 1)
                {
                    startPos.Y -= 12;
                    startPos.X += +23;
                }
                Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
            }
            {
                Texture2D texture = TextureAssets.Projectile[ModContent.ProjectileType<Flame>()].Value;
                Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                int frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<Flame>()];
                int frameY = frameHeight * Main.projFrames[ModContent.ProjectileType<Flame>()];
                Color drawColor = Color.Lerp(lightColor, Color.Pink, 20f);
                drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                Rectangle rectangle = texture.Frame(verticalFrames: 6, frameY: (int)Main.GameUpdateCount / 5 % 6);
                Vector2 origin = rectangle.Size() / 2f;
                float rotation = base.Projectile.rotation;
                float scale = base.Projectile.scale * 1.2f;
                SpriteEffects spriteEffects = SpriteEffects.None;
                if (base.Projectile.spriteDirection == 1)
                {
                    startPos.Y -= 12;
                    startPos.X -= +40;
                }
                Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
            }
        }
    }
}
