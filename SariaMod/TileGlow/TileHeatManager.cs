using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SariaMod.Buffs;
using SariaMod.Dusts;
using SariaMod.Items.Ruby;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.TileGlow
{
    public class TileHeatManager : ModSystem
    {
        private static Dictionary<(int x, int y), TileHeatData> heatedTiles = new Dictionary<(int x, int y), TileHeatData>();
        private static List<(int x, int y)> tilesToRemove = new List<(int x, int y)>();
        private static Dictionary<int, int> playerHeatDamageCooldowns = new Dictionary<int, int>();
        private static List<int> cooldownKeysToRemove = new List<int>();
        private static Dictionary<(int x, int y), VisualHeatState> visualHeatStates = new Dictionary<(int x, int y), VisualHeatState>();
        private static List<(int x, int y)> visualStatesToRemove = new List<(int x, int y)>();
        private static List<(int x, int y)> visualStateKeys = new List<(int x, int y)>();
        private static List<VisibleHeatEntry> visibleHeatEntries = new List<VisibleHeatEntry>();
        private static List<AdjacentHeatSpillEntry> visibleHeatSpillEntries = new List<AdjacentHeatSpillEntry>();
        private static Dictionary<long, int> visibleHeatSpillIndices = new Dictionary<long, int>();
        private static Dictionary<(int x, int y), int> buriedDepthCache = new Dictionary<(int x, int y), int>();
        private static int[] heatFieldProjectileIndices;
        private static bool[] heatFieldOwnerNeeded;
        private static Rectangle[] heatFieldBoundsByOwner;
        private static int[] heatFieldDamageByOwner;
        private static Texture2D moltenBoundaryAtlas;
        private static Texture2D moltenHighlightAtlas;
        private static Texture2D adjacentHeatSpillAtlas;
        private static int heatParticlesSpawnedThisTick;
        private static int lastFallingEmberSpawnTick = -1000000;
        private static CenterOverheatState[] centerOverheatStates;
        private static bool[] centerObservedThisTick;
        private static bool[] centerHasActiveBeam;
        private static RovaEmber[] centerOverheatEmberInstances;
        private static RovaEmber[] tileHeatEmberInstances;
        private static int[] centerEmberCountsByOwner;
        private static int[] tileEmberCountsByOwner;
        private static Dictionary<(int owner, int handle), int> centerSlotsByHandle = new Dictionary<(int owner, int handle), int>();
        private static RovaBeam[] trackedPlatformHeatBeams;
        private static List<int> activeRovaBeamIndices = new List<int>();
        private static List<Point>[] pendingPlatformHeatTiles;
        private static HashSet<long>[] pendingPlatformHeatKeys;
        private static int[] pendingPlatformHeatOwners;
        private static int[] pendingPlatformHeatDamage;
        private static Dictionary<long, BeamHeatSpreadSample> platformHeatSpreadSamples = new Dictionary<long, BeamHeatSpreadSample>();
        private static List<Point> platformHeatCenters = new List<Point>();
        private static float localScreenHeatIntensity;
        private static bool heatDistortionFilterRegistered;

        public const int DefaultHeatDuration = 1200;
        public const int HeatDamageIntervalTicks = 60;
        public const int HeatCollisionOverhangPixels = 4;
        private const int PlayerHeatProximityPaddingPixels = 48;
        // Fresh heat falls from 1.0 at its center to 0.70 at the outer edge.
        // Requiring at least 0.75 leaves the faint perimeter non-damaging.
        public const float HeatDamageIntensityThreshold = 0.75f;

        private static readonly Color BrightYellow = new Color(255, 255, 100);
        private static readonly Color HotOrange = new Color(255, 140, 30);
        private static readonly Color DeepRed = new Color(180, 30, 10);
        private static readonly Color DarkRed = new Color(100, 10, 0);
        private static readonly Color EmberWhite = new Color(255, 220, 160);
        private static readonly Color LavaRed = new Color(255, 46, 6);
        private static readonly Color LavaOrange = new Color(255, 104, 8);
        private static readonly Color MoltenShadowRed = new Color(68, 3, 2);
        private static readonly Color MoltenBodyRed = new Color(168, 12, 4);
        private static readonly Color MoltenHighlightRed = new Color(255, 34, 5);
        private static readonly Color CoolingBlack = new Color(12, 4, 3);

        private const float MoltenVisualThreshold = 0.20f;
        private const float VisualHeatRiseRate = 0.14f;
        private const float VisualHeatReheatRiseRate = 0.30f;
        private const float VisualHeatFallRate = 0.035f;
        private const float VisualHeatRemovalThreshold = 0.004f;
        private const int MinimumMoltenOverhangPixels = 2;
        private const int MaximumMoltenOverhangPixels = HeatCollisionOverhangPixels;
        private const float MinimumMoltenContourFade = 0.40f;
        private const float MoltenNeighborCoverageRatio = 0.88f;
        private const int MaxHeatParticlesPerTick = 4;
        private const int FallingEmberSpawnIntervalTicks = 8;
        // Heat begins storing after two seconds. The first 48-point release is
        // ready after about 2.8 seconds of uninterrupted firing.
        private const int CenterOverheatWarmupTicks = 120;
        private const int CenterOverheatCapacity = 480;
        private const int CenterOverheatEmberCost = 48;
        private const int CenterOverheatEmberIntervalTicks = 18;
        private const int MaxCenterEmbersPerOwner = 6;
        private const int MaxCenterEmbersGlobal = 32;
        private const int BoundaryAtlasSize = 10;
        private const int HighlightFrameSize = 16;
        private const int HighlightFrameCount = 16;
        private const int AdjacentHeatSpillFrameSize = 16;
        private const int AdjacentHeatSpillDirectionCount = 4;
        private const int AdjacentHeatSpillNativeRatioLevels = 64;
        private const int AdjacentHeatSpillDepthPixels = 8;
        private const float AdjacentHeatSpillStrength = 0.50f;
        private const byte SpillFromLeft = 1;
        private const byte SpillFromRight = 2;
        private const byte SpillFromTop = 4;
        private const byte SpillFromBottom = 8;
        private const int BeamPlatformHeatIntervalTicks = 8;
        private const float RovaBeamHeatCollisionWidth = 14f;
        private const int HeatedTileScreenEffectRangePixels = 128;
        private const float RovaBeamScreenEffectRangePixels = 144f;
        private const float RovaCenterScreenEffectRangePixels = 112f;
        // Proportional attack/release keeps the perceived fade duration the
        // same at every distance. Each factor closes 95% of the remaining gap
        // over the named duration instead of completing weak targets early.
        private const float ScreenHeatRiseSmoothing = 0.006221674f; // 95% in 8 seconds
        private const float ScreenHeatFallSmoothing = 0.004152095f; // 95% in 12 seconds
        private const float ScreenHeatSnapThreshold = 0.0001f;
        private const float MaximumScreenTintAlpha = 0.16f;
        private const float MaximumHeatDistortionIntensity = 1.5f;
        private const string RovaHeatDistortionFilterKey = "SariaMod:RovaHeatDistortion";
        internal const int MaxBeamPlatformBatchTiles = 4096;

        private struct VisualHeatState
        {
            public float Intensity;
            public float NormalizedDistance;
            public float Reveal;

            public VisualHeatState(float intensity, float normalizedDistance, float reveal)
            {
                Intensity = intensity;
                NormalizedDistance = normalizedDistance;
                Reveal = reveal;
            }
        }

        private struct VisibleHeatEntry
        {
            public int X;
            public int Y;
            public Tile Tile;
            public Vector2 ScreenPosition;
            public VisualHeatState State;
            public int BuriedDepth;

            public VisibleHeatEntry(
                int x,
                int y,
                Tile tile,
                Vector2 screenPosition,
                VisualHeatState state,
                int buriedDepth)
            {
                X = x;
                Y = y;
                Tile = tile;
                ScreenPosition = screenPosition;
                State = state;
                BuriedDepth = buriedDepth;
            }
        }

        private struct AdjacentHeatSpillEntry
        {
            public int X;
            public int Y;
            public Tile Tile;
            public Vector2 ScreenPosition;
            public byte DirectionMask;
            public byte NativeRatioLevel;
            public ushort SourceEdgeCoverage;
            public Color Tint;
            public float Opacity;
            public float DesiredDisplayedRed;
            public float NativeDisplayedRed;

            public AdjacentHeatSpillEntry(
                int x,
                int y,
                Tile tile,
                Vector2 screenPosition,
                byte directionMask,
                byte nativeRatioLevel,
                ushort sourceEdgeCoverage,
                Color tint,
                float opacity,
                float desiredDisplayedRed,
                float nativeDisplayedRed)
            {
                X = x;
                Y = y;
                Tile = tile;
                ScreenPosition = screenPosition;
                DirectionMask = directionMask;
                NativeRatioLevel = nativeRatioLevel;
                SourceEdgeCoverage = sourceEdgeCoverage;
                Tint = tint;
                Opacity = opacity;
                DesiredDisplayedRed = desiredDisplayedRed;
                NativeDisplayedRed = nativeDisplayedRed;
            }
        }

        private struct CenterOverheatState
        {
            public int Owner;
            public int Handle;
            public int FiringTicks;
            public int StoredHeat;
            public int EmberCooldown;
        }

        private struct BeamHeatSpreadSample
        {
            public float DistanceFromCenter;
            public float MaxRadius;

            public BeamHeatSpreadSample(float distanceFromCenter, float maxRadius)
            {
                DistanceFromCenter = distanceFromCenter;
                MaxRadius = maxRadius;
            }

            public float NormalizedDistance => MaxRadius > 0f
                ? DistanceFromCenter / MaxRadius
                : 1f;
        }

        public static TileHeatManager Instance { get; private set; }

        public override void Load()
        {
            heatFieldProjectileIndices = new int[Main.maxPlayers];
            heatFieldOwnerNeeded = new bool[Main.maxPlayers];
            heatFieldBoundsByOwner = new Rectangle[Main.maxPlayers];
            heatFieldDamageByOwner = new int[Main.maxPlayers];
            centerOverheatStates = new CenterOverheatState[Main.maxProjectiles];
            centerObservedThisTick = new bool[Main.maxProjectiles];
            centerHasActiveBeam = new bool[Main.maxProjectiles];
            centerOverheatEmberInstances = new RovaEmber[Main.maxProjectiles];
            tileHeatEmberInstances = new RovaEmber[Main.maxProjectiles];
            centerEmberCountsByOwner = new int[Main.maxPlayers];
            tileEmberCountsByOwner = new int[Main.maxPlayers];
            trackedPlatformHeatBeams = new RovaBeam[Main.maxProjectiles];
            activeRovaBeamIndices ??= new List<int>();
            pendingPlatformHeatTiles = new List<Point>[Main.maxProjectiles];
            pendingPlatformHeatKeys = new HashSet<long>[Main.maxProjectiles];
            pendingPlatformHeatOwners = new int[Main.maxProjectiles];
            pendingPlatformHeatDamage = new int[Main.maxProjectiles];
            platformHeatSpreadSamples ??= new Dictionary<long, BeamHeatSpreadSample>();
            platformHeatCenters ??= new List<Point>();
            centerSlotsByHandle ??= new Dictionary<(int owner, int handle), int>();
            visibleHeatEntries ??= new List<VisibleHeatEntry>();
            visibleHeatSpillEntries ??= new List<AdjacentHeatSpillEntry>();
            visibleHeatSpillIndices ??= new Dictionary<long, int>();
            Array.Fill(heatFieldProjectileIndices, -1);

            if (!Main.dedServ)
            {
                Filters.Scene[RovaHeatDistortionFilterKey] = new Filter(
                    new ScreenShaderData("FilterHeatDistortion")
                        .UseImage("Images/Misc/noise", 0, null)
                        .UseIntensity(0f),
                    EffectPriority.High);
                heatDistortionFilterRegistered = true;
            }

            Instance = this;
        }

        public override void Unload()
        {
            DeactivateRovaHeatDistortion();
            heatDistortionFilterRegistered = false;
            heatedTiles?.Clear();
            heatedTiles = null;
            tilesToRemove?.Clear();
            tilesToRemove = null;
            playerHeatDamageCooldowns?.Clear();
            playerHeatDamageCooldowns = null;
            cooldownKeysToRemove?.Clear();
            cooldownKeysToRemove = null;
            visualHeatStates?.Clear();
            visualHeatStates = null;
            visualStatesToRemove?.Clear();
            visualStatesToRemove = null;
            visualStateKeys?.Clear();
            visualStateKeys = null;
            visibleHeatEntries?.Clear();
            visibleHeatEntries = null;
            visibleHeatSpillEntries?.Clear();
            visibleHeatSpillEntries = null;
            visibleHeatSpillIndices?.Clear();
            visibleHeatSpillIndices = null;
            buriedDepthCache?.Clear();
            buriedDepthCache = null;
            heatFieldProjectileIndices = null;
            heatFieldOwnerNeeded = null;
            heatFieldBoundsByOwner = null;
            heatFieldDamageByOwner = null;
            centerOverheatStates = null;
            centerObservedThisTick = null;
            centerHasActiveBeam = null;
            centerOverheatEmberInstances = null;
            tileHeatEmberInstances = null;
            centerEmberCountsByOwner = null;
            tileEmberCountsByOwner = null;
            trackedPlatformHeatBeams = null;
            activeRovaBeamIndices?.Clear();
            activeRovaBeamIndices = null;
            pendingPlatformHeatTiles = null;
            pendingPlatformHeatKeys = null;
            pendingPlatformHeatOwners = null;
            pendingPlatformHeatDamage = null;
            platformHeatSpreadSamples?.Clear();
            platformHeatSpreadSamples = null;
            platformHeatCenters?.Clear();
            platformHeatCenters = null;
            centerSlotsByHandle?.Clear();
            centerSlotsByHandle = null;
            moltenBoundaryAtlas?.Dispose();
            moltenBoundaryAtlas = null;
            moltenHighlightAtlas?.Dispose();
            moltenHighlightAtlas = null;
            adjacentHeatSpillAtlas?.Dispose();
            adjacentHeatSpillAtlas = null;
            localScreenHeatIntensity = 0f;
            Instance = null;
        }

        public override void OnWorldLoad()
        {
            localScreenHeatIntensity = 0f;
            ClearAllHeat();
        }

        public override void OnWorldUnload()
        {
            localScreenHeatIntensity = 0f;
            DeactivateRovaHeatDistortion();
            ClearAllHeat();
        }

        public override void PreUpdatePlayers()
        {
            buriedDepthCache?.Clear();
            heatParticlesSpawnedThisTick = 0;
            DecayCooldowns(playerHeatDamageCooldowns);
            UpdateRovaCenterOverheatEmbers();
            UpdateHeatedTiles();
            UpdateVisualHeatStates();
            UpdateHeatFieldProjectiles();
            ApplyHeatedTileEffects();
        }

        public override void PostUpdateEverything()
        {
            UpdateLocalScreenHeatEffect();
        }

        private static bool IsTileExposedToAir(int x, int y)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;

                    int checkX = x + dx;
                    int checkY = y + dy;

                    if (checkX < 0 || checkX >= Main.maxTilesX || checkY < 0 || checkY >= Main.maxTilesY)
                        continue;

                    Tile adjacentTile = Main.tile[checkX, checkY];
                    if (!IsBurialOccludingTerrain(adjacentTile))
                        return true;
                }
            }
            return false;
        }

        private static int GetBuriedDepth(int x, int y)
        {
            if (IsTileExposedToAir(x, y))
                return 0;

            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;

                    int checkX = x + dx;
                    int checkY = y + dy;

                    if (checkX < 0 || checkX >= Main.maxTilesX || checkY < 0 || checkY >= Main.maxTilesY)
                        continue;

                    Tile adjacentTile = Main.tile[checkX, checkY];
                    if (IsBurialOccludingTerrain(adjacentTile) && IsTileExposedToAir(checkX, checkY))
                        return 1;
                }
            }

            return 2;
        }

        private static bool IsBurialOccludingTerrain(Tile tile)
        {
            if (!tile.HasTile || tile.IsActuated)
                return false;

            int tileType = tile.TileType;
            return Main.tileSolid[tileType]
                && !Main.tileSolidTop[tileType]
                && !Main.tileFrameImportant[tileType]
                && !IsPlatformTile(tile)
                && !tile.IsHalfBlock
                && tile.Slope == SlopeType.Solid;
        }

        private static int GetCachedBuriedDepth(int x, int y)
        {
            if (buriedDepthCache == null)
                return GetBuriedDepth(x, y);

            var key = (x, y);
            if (!buriedDepthCache.TryGetValue(key, out int depth))
            {
                depth = GetBuriedDepth(x, y);
                buriedDepthCache[key] = depth;
            }

            return depth;
        }

        public override void PostDrawTiles()
        {
            if (Main.dedServ)
                return;

            if (visualHeatStates == null || visualHeatStates.Count == 0)
                return;

            int currentTick = (int)Main.GameUpdateCount;
            BuildVisibleHeatEntries();
            if (visibleHeatEntries.Count == 0)
                return;
            EnsureMoltenVisualAtlases();

            // Lay down a deep red molten base, or a blackened cooling tint.
            // Both stay clipped to the original tile silhouette so only the
            // later glow contour extends into otherwise unaffected terrain.
            Main.spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.PointClamp,
                DepthStencilState.None,
                RasterizerState.CullNone,
                null,
                Main.GameViewMatrix.ZoomMatrix
            );

            for (int i = 0; i < visibleHeatEntries.Count; i++)
            {
                VisibleHeatEntry entry = visibleHeatEntries[i];
                VisualHeatState state = entry.State;
                bool isDoor = IsDoorHeatSurfaceTile(entry.Tile);

                if (state.Intensity > MoltenVisualThreshold)
                {
                    float hotAmount = GetMoltenVisualStrength(state.Intensity);
                    float moltenOpacity = GetMoltenVisualOpacity(state.Intensity) * state.Reveal;
                    float distanceFalloff = GetSpatialVisualFade(state.NormalizedDistance);
                    float buriedMultiplier = entry.BuriedDepth >= 2 ? 0.30f : entry.BuriedDepth == 1 ? 0.68f : 1f;
                    Color deepMoltenColor = Color.Lerp(MoltenShadowRed, MoltenBodyRed, hotAmount);
                    float deepMoltenAlpha = (0.24f + hotAmount * 0.42f)
                        * moltenOpacity * distanceFalloff * buriedMultiplier;
                    if (isDoor)
                        DrawTileHeat(entry.Tile, entry.X, entry.Y, entry.ScreenPosition, deepMoltenColor * deepMoltenAlpha);
                    else
                        DrawTileShapeOverlay(entry.Tile, entry.ScreenPosition, deepMoltenColor * deepMoltenAlpha);
                    continue;
                }

                float coolingAmount = MathHelper.Clamp(
                    (MoltenVisualThreshold - state.Intensity) / MoltenVisualThreshold,
                    0f,
                    1f);
                float finalFade = MathHelper.Clamp(state.Intensity / 0.05f, 0f, 1f);
                float coolingAlpha = coolingAmount * finalFade * state.Reveal
                    * GetSpatialVisualFade(state.NormalizedDistance) * 0.58f;
                if (isDoor)
                    DrawTileHeat(entry.Tile, entry.X, entry.Y, entry.ScreenPosition, CoolingBlack * coolingAlpha);
                else
                    DrawTileShapeOverlay(entry.Tile, entry.ScreenPosition, CoolingBlack * coolingAlpha);
            }

            Main.spriteBatch.End();

            Main.spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.Additive,
                SamplerState.PointClamp,
                DepthStencilState.None,
                RasterizerState.CullNone,
                null,
                Main.GameViewMatrix.ZoomMatrix
            );

            // Keep terrain textures together before switching to MagicPixel.
            // This avoids forcing a texture batch change for every heated tile.
            for (int i = 0; i < visibleHeatEntries.Count; i++)
            {
                VisibleHeatEntry entry = visibleHeatEntries[i];
                VisualHeatState state = entry.State;
                if (state.Intensity <= MoltenVisualThreshold)
                    continue;

                float intensity = state.Intensity;
                Color bodyColor = GetMoltenBodyColor(intensity);
                float textureAlpha = GetMoltenBodyVisualOpacity(state, entry.BuriedDepth);
                DrawTileHeat(entry.Tile, entry.X, entry.Y, entry.ScreenPosition, bodyColor * textureAlpha);
            }

            // Let a hot tile tint only the nearest half of an adjacent unheated
            // surface. This is visual-only: the neighbor receives no heat state,
            // damage, particles, projectile work, or network traffic.
            for (int i = 0; i < visibleHeatSpillEntries.Count; i++)
            {
                AdjacentHeatSpillEntry spill = visibleHeatSpillEntries[i];
                DrawAdjacentHeatSpill(spill);
            }

            for (int i = 0; i < visibleHeatEntries.Count; i++)
            {
                VisibleHeatEntry entry = visibleHeatEntries[i];
                VisualHeatState state = entry.State;
                if (state.Intensity <= MoltenVisualThreshold || IsDoorHeatSurfaceTile(entry.Tile))
                    continue;

                float hotAmount = GetMoltenVisualStrength(state.Intensity);
                float moltenOpacity = GetMoltenVisualOpacity(state.Intensity) * state.Reveal;
                float distanceFalloff = GetSpatialVisualFade(state.NormalizedDistance);
                float buriedMultiplier = entry.BuriedDepth >= 2 ? 0.28f : entry.BuriedDepth == 1 ? 0.62f : 1f;
                float pulse = 0.90f + (float)Math.Sin(
                    currentTick * 0.055f + entry.X * 0.41f + entry.Y * 0.23f) * 0.10f;

                Color moltenFill = Color.Lerp(MoltenBodyRed, LavaRed, hotAmount) *
                    (0.08f + hotAmount * 0.32f) * moltenOpacity
                    * distanceFalloff * buriedMultiplier * pulse;
                DrawTileShapeOverlay(entry.Tile, entry.ScreenPosition, moltenFill);
            }

            // Common full blocks and flat platforms use one atlas draw for the
            // complete moving melt line instead of several MagicPixel strips.
            for (int i = 0; i < visibleHeatEntries.Count; i++)
            {
                VisibleHeatEntry entry = visibleHeatEntries[i];
                VisualHeatState state = entry.State;
                if (state.Intensity <= MoltenVisualThreshold || !CanUseMeltHighlightAtlas(entry.Tile))
                    continue;

                float hotAmount = GetMoltenVisualStrength(state.Intensity);
                float moltenOpacity = GetMoltenVisualOpacity(state.Intensity) * state.Reveal;
                float distanceFalloff = GetSpatialVisualFade(state.NormalizedDistance);
                float buriedMultiplier = entry.BuriedDepth >= 2 ? 0.28f : entry.BuriedDepth == 1 ? 0.62f : 1f;
                DrawMoltenMeltHighlightAtlas(
                    entry.Tile,
                    entry.X,
                    entry.Y,
                    entry.ScreenPosition,
                    hotAmount,
                    moltenOpacity * distanceFalloff * buriedMultiplier,
                    currentTick);
            }

            // Irregular geometry keeps the exact row-clipped fallback.
            for (int i = 0; i < visibleHeatEntries.Count; i++)
            {
                VisibleHeatEntry entry = visibleHeatEntries[i];
                VisualHeatState state = entry.State;
                if (state.Intensity <= MoltenVisualThreshold
                    || IsDoorHeatSurfaceTile(entry.Tile)
                    || CanUseMeltHighlightAtlas(entry.Tile))
                    continue;

                float hotAmount = GetMoltenVisualStrength(state.Intensity);
                float moltenOpacity = GetMoltenVisualOpacity(state.Intensity) * state.Reveal;
                float distanceFalloff = GetSpatialVisualFade(state.NormalizedDistance);
                float buriedMultiplier = entry.BuriedDepth >= 2 ? 0.28f : entry.BuriedDepth == 1 ? 0.62f : 1f;
                DrawMoltenMeltHighlights(
                    entry.Tile,
                    entry.X,
                    entry.Y,
                    entry.ScreenPosition,
                    hotAmount,
                    moltenOpacity * distanceFalloff * buriedMultiplier,
                    currentTick);
            }

            // Draw the contour after every body tile so adjacent hot tiles form
            // one shape with no bright seams between them.
            for (int i = 0; i < visibleHeatEntries.Count; i++)
            {
                VisibleHeatEntry entry = visibleHeatEntries[i];
                if (entry.State.Intensity <= MoltenVisualThreshold
                    || IsDoorHeatSurfaceTile(entry.Tile))
                    continue;

                DrawWavyMoltenBoundary(
                    entry.X,
                    entry.Y,
                    entry.State.Intensity,
                    entry.State.Reveal,
                    entry.State.NormalizedDistance,
                    currentTick);
            }

            Main.spriteBatch.End();
        }

        private static void BuildVisibleHeatEntries()
        {
            visibleHeatEntries.Clear();
            visibleHeatSpillEntries.Clear();
            visibleHeatSpillIndices.Clear();
            foreach (var kvp in visualHeatStates)
            {
                VisualHeatState state = kvp.Value;
                if (state.Intensity <= VisualHeatRemovalThreshold
                    || GetSpatialVisualFade(state.NormalizedDistance) <= VisualHeatRemovalThreshold)
                    continue;

                int x = kvp.Key.x;
                int y = kvp.Key.y;
                if (x < 0 || x >= Main.maxTilesX || y < 0 || y >= Main.maxTilesY)
                    continue;

                Vector2 screenPosition = new Vector2(x * 16f, y * 16f) - Main.screenPosition;
                if (!IsHeatTileOnScreen(screenPosition, MaximumMoltenOverhangPixels + 2f))
                    continue;

                Tile tile = Main.tile[x, y];
                if (!IsHeatSurfaceTile(tile) && !IsDoorHeatSurfaceTile(tile))
                    continue;

                int buriedDepth = state.Intensity > MoltenVisualThreshold
                    ? GetCachedBuriedDepth(x, y)
                    : 0;
                visibleHeatEntries.Add(new VisibleHeatEntry(
                    x,
                    y,
                    tile,
                    screenPosition,
                    state,
                    buriedDepth));
            }

            BuildVisibleHeatSpillEntries();
        }

        private static void BuildVisibleHeatSpillEntries()
        {
            for (int i = 0; i < visibleHeatEntries.Count; i++)
            {
                VisibleHeatEntry source = visibleHeatEntries[i];
                VisualHeatState state = source.State;
                if (state.Intensity <= MoltenVisualThreshold || IsDoorHeatSurfaceTile(source.Tile))
                    continue;

                Color bodyColor = GetMoltenBodyColor(state.Intensity);
                float desiredDisplayedRed = GetMoltenBodyVisualRed(state, source.BuriedDepth)
                    * AdjacentHeatSpillStrength;
                if (desiredDisplayedRed <= 0.001f)
                    continue;

                GetSourceEdgeCoverages(
                    source.Tile,
                    out ushort sourceLeftCoverage,
                    out ushort sourceRightCoverage,
                    out ushort sourceTopCoverage,
                    out ushort sourceBottomCoverage);
                AddVisibleHeatSpill(
                    source.X - 1,
                    source.Y,
                    SpillFromRight,
                    sourceLeftCoverage,
                    bodyColor,
                    desiredDisplayedRed);
                AddVisibleHeatSpill(
                    source.X + 1,
                    source.Y,
                    SpillFromLeft,
                    sourceRightCoverage,
                    bodyColor,
                    desiredDisplayedRed);
                AddVisibleHeatSpill(
                    source.X,
                    source.Y - 1,
                    SpillFromBottom,
                    sourceTopCoverage,
                    bodyColor,
                    desiredDisplayedRed);
                AddVisibleHeatSpill(
                    source.X,
                    source.Y + 1,
                    SpillFromTop,
                    sourceBottomCoverage,
                    bodyColor,
                    desiredDisplayedRed);
            }

        }

        private static void AddVisibleHeatSpill(
            int tileX,
            int tileY,
            byte directionMask,
            ushort sourceEdgeCoverage,
            Color tint,
            float desiredDisplayedRed)
        {
            if (sourceEdgeCoverage == 0
                || tileX < 0
                || tileX >= Main.maxTilesX
                || tileY < 0
                || tileY >= Main.maxTilesY)
            {
                return;
            }

            Tile tile = Main.tile[tileX, tileY];
            if (!IsHeatSurfaceTile(tile))
                return;

            Vector2 screenPosition = new Vector2(tileX * 16f, tileY * 16f) - Main.screenPosition;
            if (!IsHeatTileOnScreen(screenPosition, 2f))
                return;

            int buriedDepth = GetCachedBuriedDepth(tileX, tileY);
            float buriedMultiplier = buriedDepth >= 2 ? 0.28f : buriedDepth == 1 ? 0.62f : 1f;
            desiredDisplayedRed *= buriedMultiplier;
            float nativeDisplayedRed = visualHeatStates.TryGetValue((tileX, tileY), out VisualHeatState targetState)
                ? GetMoltenBodyVisualRed(targetState, buriedDepth)
                : 0f;
            if (nativeDisplayedRed >= desiredDisplayedRed - 0.001f)
                return;

            float nativeRatio = MathHelper.Clamp(nativeDisplayedRed / desiredDisplayedRed, 0f, 1f);
            int nativeRatioLevel = (int)Math.Ceiling(nativeRatio * AdjacentHeatSpillNativeRatioLevels);
            if (nativeRatioLevel >= AdjacentHeatSpillNativeRatioLevels)
                return;

            float tintRed = Math.Max(0.001f, tint.R / 255f);
            // SpriteBatch's additive blend applies source alpha. Square-rooting
            // both scalar factors makes their on-screen red contribution linear.
            float opacity = (float)Math.Sqrt(desiredDisplayedRed / tintRed);
            if (opacity <= 0.001f)
                return;

            long packedKey = PackHeatSpillDirectionKey(tileX, tileY, directionMask);
            if (visibleHeatSpillIndices.TryGetValue(packedKey, out int existingIndex))
            {
                AdjacentHeatSpillEntry existing = visibleHeatSpillEntries[existingIndex];
                if (desiredDisplayedRed > existing.DesiredDisplayedRed)
                {
                    existing.Tint = tint;
                    existing.Opacity = opacity;
                    existing.NativeRatioLevel = (byte)nativeRatioLevel;
                    existing.SourceEdgeCoverage = sourceEdgeCoverage;
                    existing.DesiredDisplayedRed = desiredDisplayedRed;
                    existing.NativeDisplayedRed = nativeDisplayedRed;
                }

                visibleHeatSpillEntries[existingIndex] = existing;
                return;
            }

            visibleHeatSpillIndices[packedKey] = visibleHeatSpillEntries.Count;
            visibleHeatSpillEntries.Add(new AdjacentHeatSpillEntry(
                tileX,
                tileY,
                tile,
                screenPosition,
                directionMask,
                (byte)nativeRatioLevel,
                sourceEdgeCoverage,
                tint,
                opacity,
                desiredDisplayedRed,
                nativeDisplayedRed));
        }

        private static long PackHeatSpillDirectionKey(int tileX, int tileY, byte directionMask)
        {
            return ((long)tileX << 36) | ((long)(uint)tileY << 4) | directionMask;
        }

        private static void GetSourceEdgeCoverages(
            Tile tile,
            out ushort leftCoverage,
            out ushort rightCoverage,
            out ushort topCoverage,
            out ushort bottomCoverage)
        {
            leftCoverage = 0;
            rightCoverage = 0;
            topCoverage = 0;
            bottomCoverage = 0;
            if (tile.Slope == SlopeType.Solid
                && !tile.IsHalfBlock
                && !IsPlatformTile(tile))
            {
                leftCoverage = ushort.MaxValue;
                rightCoverage = ushort.MaxValue;
                topCoverage = ushort.MaxValue;
                bottomCoverage = ushort.MaxValue;
                return;
            }

            for (int localY = 0; localY < 16; localY++)
            {
                if (!TryGetGeometryRowRange(tile, localY, out int left, out int right))
                    continue;

                if (left == 0)
                    leftCoverage |= (ushort)(1 << localY);
                if (right == 15)
                    rightCoverage |= (ushort)(1 << localY);

                if (localY == 0)
                {
                    for (int localX = left; localX <= right; localX++)
                        topCoverage |= (ushort)(1 << localX);
                }
                else if (localY == 15)
                {
                    for (int localX = left; localX <= right; localX++)
                        bottomCoverage |= (ushort)(1 << localX);
                }
            }
        }

        private static void EnsureMoltenVisualAtlases()
        {
            if (moltenBoundaryAtlas == null || moltenBoundaryAtlas.IsDisposed)
            {
                moltenBoundaryAtlas = new Texture2D(
                    Main.instance.GraphicsDevice,
                    BoundaryAtlasSize,
                    BoundaryAtlasSize);
                Color[] boundaryPixels = new Color[BoundaryAtlasSize * BoundaryAtlasSize];
                for (int overhang = 1; overhang <= MaximumMoltenOverhangPixels; overhang++)
                {
                    int column = overhang - 1;
                    int row = 5 + overhang;
                    for (int step = 0; step <= overhang; step++)
                    {
                        Color color = GetBoundaryAtlasColor(step, overhang);
                        boundaryPixels[step * BoundaryAtlasSize + column] = color;
                        boundaryPixels[row * BoundaryAtlasSize + step] = color;
                    }
                }

                moltenBoundaryAtlas.SetData(boundaryPixels);
            }

            if (moltenHighlightAtlas == null || moltenHighlightAtlas.IsDisposed)
            {
                int atlasWidth = HighlightFrameSize * HighlightFrameCount;
                int atlasHeight = HighlightFrameSize * 2;
                moltenHighlightAtlas = new Texture2D(Main.instance.GraphicsDevice, atlasWidth, atlasHeight);
                Color[] highlightPixels = new Color[atlasWidth * atlasHeight];
                for (int geometryType = 0; geometryType < 2; geometryType++)
                {
                    int centerY = geometryType == 0 ? 8 : 3;
                    int atlasTop = geometryType * HighlightFrameSize;
                    for (int frame = 0; frame < HighlightFrameCount; frame++)
                    {
                        float phase = frame * MathHelper.TwoPi / HighlightFrameCount;
                        int atlasLeft = frame * HighlightFrameSize;
                        for (int localX = 0; localX < HighlightFrameSize; localX++)
                        {
                            float broadFlow = (float)Math.Sin(localX * 0.43f + phase);
                            float fineFlow = (float)Math.Sin(localX * 0.91f - phase * 0.67f);
                            int localY = centerY + (int)Math.Round(broadFlow * 1.35f + fineFlow * 0.55f);
                            int mainIndex = (atlasTop + localY) * atlasWidth + atlasLeft + localX;
                            highlightPixels[mainIndex] = MoltenHighlightRed;

                            if (((localX + frame * 3) & 7) == 0)
                            {
                                int dripY = Math.Min(HighlightFrameSize - 1, localY + 1);
                                int dripIndex = (atlasTop + dripY) * atlasWidth + atlasLeft + localX;
                                highlightPixels[dripIndex] = LavaRed * 0.62f;
                            }
                        }
                    }
                }

                moltenHighlightAtlas.SetData(highlightPixels);
            }

            if (adjacentHeatSpillAtlas == null || adjacentHeatSpillAtlas.IsDisposed)
            {
                int atlasWidth = AdjacentHeatSpillFrameSize
                    * AdjacentHeatSpillDirectionCount;
                int atlasHeight = AdjacentHeatSpillFrameSize
                    * (AdjacentHeatSpillNativeRatioLevels + 1);
                adjacentHeatSpillAtlas = new Texture2D(
                    Main.instance.GraphicsDevice,
                    atlasWidth,
                    atlasHeight);
                Color[] spillPixels = new Color[atlasWidth * atlasHeight];
                for (int nativeLevel = 0; nativeLevel <= AdjacentHeatSpillNativeRatioLevels; nativeLevel++)
                {
                    float nativeRatio = nativeLevel / (float)AdjacentHeatSpillNativeRatioLevels;
                    int frameTop = nativeLevel * AdjacentHeatSpillFrameSize;
                    for (int directionIndex = 0; directionIndex < AdjacentHeatSpillDirectionCount; directionIndex++)
                    {
                        byte direction = (byte)(1 << directionIndex);
                        int frameLeft = directionIndex * AdjacentHeatSpillFrameSize;
                        for (int localY = 0; localY < AdjacentHeatSpillFrameSize; localY++)
                        {
                            for (int localX = 0; localX < AdjacentHeatSpillFrameSize; localX++)
                            {
                                float fade = GetAdjacentHeatSpillDirectionFade(direction, localX, localY);
                                float adjustedFade = Math.Max(0f, fade - nativeRatio);
                                float encodedFade = (float)Math.Sqrt(adjustedFade);
                                spillPixels[(frameTop + localY) * atlasWidth + frameLeft + localX]
                                    = Color.White * encodedFade;
                            }
                        }
                    }
                }

                adjacentHeatSpillAtlas.SetData(spillPixels);
            }
        }

        private static float GetAdjacentHeatSpillDirectionFade(byte direction, int localX, int localY)
        {
            if (direction == SpillFromLeft)
                return GetAdjacentHeatSpillFade(localX);
            if (direction == SpillFromRight)
                return GetAdjacentHeatSpillFade(15 - localX);
            if (direction == SpillFromTop)
                return GetAdjacentHeatSpillFade(localY);
            if (direction == SpillFromBottom)
                return GetAdjacentHeatSpillFade(15 - localY);
            return 0f;
        }

        private static float GetAdjacentHeatSpillFade(int distanceFromHeatedEdge)
        {
            float amount = MathHelper.Clamp(
                1f - distanceFromHeatedEdge / (float)AdjacentHeatSpillDepthPixels,
                0f,
                1f);
            return amount * amount * (3f - 2f * amount);
        }

        private static Color GetBoundaryAtlasColor(int step, int overhang)
        {
            if (step == 0)
                return LavaRed * 0.92f;

            float edgeProgress = step / (float)overhang;
            float yellowBand = MathHelper.Clamp(
                1f - Math.Abs(edgeProgress - 0.55f) / 0.38f,
                0f,
                1f);
            Color edgeColor = Color.Lerp(
                Color.Lerp(LavaRed, LavaOrange, edgeProgress),
                BrightYellow,
                yellowBand * 0.55f);
            float outwardFade = 0.06f
                + 0.94f * (float)Math.Pow(1f - edgeProgress, 0.72f);
            return edgeColor * outwardFade;
        }

        private static bool IsHeatTileOnScreen(Vector2 screenPos, float margin)
        {
            return screenPos.X >= -16f - margin
                && screenPos.X <= Main.screenWidth + margin
                && screenPos.Y >= -16f - margin
                && screenPos.Y <= Main.screenHeight + margin;
        }

        private static float GetMoltenVisualStrength(float intensity)
        {
            float amount = MathHelper.Clamp(
                (intensity - MoltenVisualThreshold) / (1f - MoltenVisualThreshold),
                0f,
                1f);
            return amount * amount * (3f - 2f * amount);
        }

        private static float GetMoltenVisualOpacity(float intensity)
        {
            // Keep the molten layer transparent when it first crosses the
            // threshold, then bring it fully into view over the next 28% heat.
            float amount = MathHelper.Clamp(
                (intensity - MoltenVisualThreshold) / 0.28f,
                0f,
                1f);
            return amount * amount * (3f - 2f * amount);
        }

        private static float GetMoltenBodyVisualOpacity(VisualHeatState state, int buriedDepth)
        {
            if (state.Intensity <= MoltenVisualThreshold)
                return 0f;

            float hotAmount = GetMoltenVisualStrength(state.Intensity);
            float buriedMultiplier = buriedDepth >= 2 ? 0.28f : buriedDepth == 1 ? 0.62f : 1f;
            return (0.30f + hotAmount * 0.62f)
                * GetMoltenVisualOpacity(state.Intensity)
                * state.Reveal
                * GetSpatialVisualFade(state.NormalizedDistance)
                * buriedMultiplier;
        }

        private static Color GetMoltenBodyColor(float intensity)
        {
            float hotAmount = GetMoltenVisualStrength(intensity);
            Color bodyColor = Color.Lerp(MoltenShadowRed, MoltenBodyRed, hotAmount);
            return Color.Lerp(bodyColor, MoltenHighlightRed, hotAmount * hotAmount * 0.58f);
        }

        private static float GetMoltenBodyVisualRed(VisualHeatState state, int buriedDepth)
        {
            if (state.Intensity <= MoltenVisualThreshold)
                return 0f;

            float opacity = GetMoltenBodyVisualOpacity(state, buriedDepth);
            return opacity * opacity * (GetMoltenBodyColor(state.Intensity).R / 255f);
        }

        private static float GetSpatialVisualFade(float normalizedDistance)
        {
            // Preserve the molten center, then taper the outside forty-five
            // percent to a clear texture-shaped floor. A heated tile must stay
            // readable while it can still deal contact damage.
            return 0.28f + 0.72f * GetSpatialBoundaryFade(normalizedDistance);
        }

        private static float GetSpatialBoundaryFade(float normalizedDistance)
        {
            // The glow outside the actual tile geometry can fade fully away;
            // only the clipped terrain body retains the small visibility floor.
            float edgeProgress = MathHelper.Clamp(
                (normalizedDistance - 0.55f) / 0.45f,
                0f,
                1f);
            float smoothEdge = edgeProgress * edgeProgress * (3f - 2f * edgeProgress);
            return 1f - smoothEdge;
        }

        private static float GetSpatialContourFade(float normalizedDistance)
        {
            // The affected tile body remains readable at the edge. Give that
            // final tile a small contour too, so the atlas can taper the color
            // over its existing two-to-four-pixel overhang instead of ending
            // on a hard sixteen-pixel cell boundary.
            return MinimumMoltenContourFade
                + (1f - MinimumMoltenContourFade) * GetSpatialBoundaryFade(normalizedDistance);
        }

        private static bool CanUseMeltHighlightAtlas(Tile tile)
        {
            return !IsDoorHeatSurfaceTile(tile)
                && (IsFullSolidTileGeometry(tile)
                    || (IsPlatformTile(tile) && tile.Slope == SlopeType.Solid));
        }

        private static void DrawMoltenMeltHighlightAtlas(
            Tile tile,
            int tileX,
            int tileY,
            Vector2 screenPosition,
            float hotAmount,
            float opacity,
            int currentTick)
        {
            float meltAmount = MathHelper.Clamp((hotAmount - 0.52f) / 0.48f, 0f, 1f);
            meltAmount = meltAmount * meltAmount * (3f - 2f * meltAmount);
            if (meltAmount <= 0.001f || moltenHighlightAtlas == null)
                return;

            bool flatPlatform = IsPlatformTile(tile);
            int frame = (currentTick / 3 + tileX * 5 + tileY * 3) & (HighlightFrameCount - 1);
            int sourceY = flatPlatform ? HighlightFrameSize : 0;
            int height = flatPlatform ? 8 : HighlightFrameSize;
            Rectangle source = new Rectangle(
                frame * HighlightFrameSize,
                sourceY,
                HighlightFrameSize,
                height);
            Rectangle destination = new Rectangle(
                (int)screenPosition.X,
                (int)screenPosition.Y,
                HighlightFrameSize,
                height);
            float highlightAlpha = (0.10f + meltAmount * 0.24f) * opacity;
            Main.spriteBatch.Draw(
                moltenHighlightAtlas,
                destination,
                source,
                Color.White * highlightAlpha);
        }

        private static void DrawMoltenMeltHighlights(
            Tile tile,
            int tileX,
            int tileY,
            Vector2 screenPos,
            float hotAmount,
            float opacity,
            int currentTick)
        {
            float meltAmount = MathHelper.Clamp((hotAmount - 0.52f) / 0.48f, 0f, 1f);
            meltAmount = meltAmount * meltAmount * (3f - 2f * meltAmount);
            if (meltAmount <= 0.001f)
                return;

            Texture2D pixel = TextureAssets.MagicPixel.Value;
            int screenX = (int)screenPos.X;
            int screenY = (int)screenPos.Y;
            float highlightAlpha = (0.10f + meltAmount * 0.24f) * opacity;
            int meltCenterY = IsPlatformTile(tile) && tile.Slope == SlopeType.Solid ? 3 : 8;

            int runStart = -1;
            int runY = 0;
            for (int localX = 0; localX <= 16; localX++)
            {
                bool validPixel = false;
                int localY = 0;
                if (localX < 16)
                {
                    int worldX = tileX * 16 + localX;
                    float broadFlow = (float)Math.Sin(worldX * 0.22f + currentTick * 0.018f + tileY * 0.61f);
                    float fineFlow = (float)Math.Sin(worldX * 0.49f - currentTick * 0.011f + tileY * 0.27f);
                    localY = meltCenterY + (int)Math.Round(broadFlow * 1.4f + fineFlow * 0.6f);
                    validPixel = IsTileGeometryPixel(tile, localX, localY);
                }

                if (validPixel && runStart >= 0 && localY == runY)
                    continue;

                if (runStart >= 0)
                {
                    Main.spriteBatch.Draw(
                        pixel,
                        new Rectangle(screenX + runStart, screenY + runY, localX - runStart, 1),
                        MoltenHighlightRed * highlightAlpha);
                }

                runStart = validPixel ? localX : -1;
                runY = localY;
            }

            // One occasional drip keeps the melting motion without adding four
            // separate pixel draws to every hot tile.
            if (meltAmount > 0.72f)
            {
                int dripX = (tileX * 7 + tileY * 3 + currentTick / 12) & 15;
                int worldX = tileX * 16 + dripX;
                float broadFlow = (float)Math.Sin(worldX * 0.22f + currentTick * 0.018f + tileY * 0.61f);
                float fineFlow = (float)Math.Sin(worldX * 0.49f - currentTick * 0.011f + tileY * 0.27f);
                int dripY = meltCenterY + (int)Math.Round(broadFlow * 1.4f + fineFlow * 0.6f) + 1;
                if (IsTileGeometryPixel(tile, dripX, dripY))
                {
                    Main.spriteBatch.Draw(
                        pixel,
                        new Rectangle(screenX + dripX, screenY + dripY, 1, 2),
                        LavaRed * highlightAlpha * 0.62f);
                }
            }
        }

        private void DrawTileHeat(Tile tile, int tileX, int tileY, Vector2 screenPos, Color color)
        {
            int tileType = tile.TileType;

            Texture2D tileTexture = TextureAssets.Tile[tileType].Value;
            if (tileTexture == null)
                return;

            int frameX = tile.TileFrameX;
            int frameY = tile.TileFrameY;

            if (Main.tileFrame[tileType] > 0)
            {
                int animFrameOffset = GetAnimationFrameOffset(tileType, tileX, tileY);
                frameY += Main.tileFrame[tileType] * animFrameOffset;
            }

            int sourceWidth = 16;
            int sourceHeight = 16;

            if (frameX < 0) frameX = 0;
            if (frameY < 0) frameY = 0;
            if (frameX + sourceWidth > tileTexture.Width) return;
            if (frameY + sourceHeight > tileTexture.Height) return;

            var slope = tile.Slope;
            bool isHalfBlock = tile.IsHalfBlock;

            // Platforms occupy only a thin band, including when hammered into
            // stairs. Clip both the texture tint and solid overlay to that band.
            if (IsPlatformTile(tile))
            {
                if (slope == SlopeType.Solid)
                {
                    Rectangle platformSource = new Rectangle(frameX, frameY, 16, 8);
                    Rectangle platformDestination = new Rectangle(
                        (int)screenPos.X,
                        (int)screenPos.Y,
                        16,
                        8);
                    Main.spriteBatch.Draw(tileTexture, platformDestination, platformSource, color);
                    return;
                }

                for (int row = 0; row < 16; row++)
                {
                    if (!TryGetGeometryRowRange(tile, row, out int firstPixel, out int lastPixel))
                        continue;

                    int width = lastPixel - firstPixel + 1;
                    Rectangle platformSource = new Rectangle(frameX + firstPixel, frameY + row, width, 1);
                    Rectangle platformDestination = new Rectangle(
                        (int)screenPos.X + firstPixel,
                        (int)screenPos.Y + row,
                        width,
                        1);
                    Main.spriteBatch.Draw(tileTexture, platformDestination, platformSource, color);
                }

                return;
            }

            if (slope != SlopeType.Solid || isHalfBlock)
            {
                DrawSlopedOrHalfTileHeat(tile, tileTexture, frameX, frameY, slope, isHalfBlock, screenPos, color);
                return;
            }

            Rectangle sourceRect = new Rectangle(frameX, frameY, sourceWidth, sourceHeight);
            Rectangle destRect = new Rectangle((int)screenPos.X, (int)screenPos.Y, 16, 16);

            Main.spriteBatch.Draw(tileTexture, destRect, sourceRect, color);
        }

        private int GetAnimationFrameOffset(int tileType, int tileX, int tileY)
        {
            var tileData = Terraria.ObjectData.TileObjectData.GetTileData(tileType, 0);
            if (tileData != null)
            {
                int fullHeight = tileData.CoordinateFullHeight;
                if (fullHeight > 0) return fullHeight;

                int totalHeight = 0;
                if (tileData.CoordinateHeights != null)
                {
                    foreach (int h in tileData.CoordinateHeights)
                        totalHeight += h + tileData.CoordinatePadding;
                }
                if (totalHeight > 0) return totalHeight;
            }

            return 18;
        }

        private void DrawSlopedOrHalfTileHeat(Tile tile, Texture2D texture, int frameX, int frameY,
            SlopeType slope, bool isHalfBlock, Vector2 screenPos, Color color)
        {
            if (frameX < 0 || frameY < 0 || frameX + 16 > texture.Width || frameY + 16 > texture.Height)
                return;

            if (isHalfBlock && slope == SlopeType.Solid)
            {
                if (frameY + 16 > texture.Height) return;

                Rectangle sourceRect = new Rectangle(frameX, frameY + 8, 16, 8);
                Rectangle destRect = new Rectangle((int)screenPos.X, (int)screenPos.Y + 8, 16, 8);
                Main.spriteBatch.Draw(texture, destRect, sourceRect, color);
                return;
            }

            for (int row = 0; row < 16; row++)
            {
                if (frameY + row >= texture.Height) break;

                int clipLeft = 0;
                int clipRight = 0;

                switch (slope)
                {
                    case SlopeType.SlopeDownRight:
                        clipLeft = 15 - row;
                        break;
                    case SlopeType.SlopeDownLeft:
                        clipRight = 15 - row;
                        break;
                    case SlopeType.SlopeUpRight:
                        clipLeft = row;
                        break;
                    case SlopeType.SlopeUpLeft:
                        clipRight = row;
                        break;
                }

                int width = 16 - clipLeft - clipRight;

                if (width > 0 && frameX + clipLeft + width <= texture.Width)
                {
                    Rectangle srcRow = new Rectangle(frameX + clipLeft, frameY + row, width, 1);
                    Rectangle destRow = new Rectangle((int)screenPos.X + clipLeft, (int)screenPos.Y + row, width, 1);
                    Main.spriteBatch.Draw(texture, destRow, srcRow, color);
                }
            }
        }

        private static void DrawTileShapeOverlay(Tile tile, Vector2 screenPos, Color color)
        {
            if (!tile.HasTile || tile.IsActuated || color == Color.Transparent)
                return;

            Texture2D pixel = TextureAssets.MagicPixel.Value;
            int screenX = (int)screenPos.X;
            int screenY = (int)screenPos.Y;

            if (IsPlatformTile(tile))
            {
                if (tile.Slope == SlopeType.Solid)
                {
                    Main.spriteBatch.Draw(pixel, new Rectangle(screenX, screenY, 16, 8), color);
                    return;
                }

                for (int row = 0; row < 16; row++)
                {
                    if (TryGetGeometryRowRange(tile, row, out int firstPixel, out int lastPixel))
                    {
                        Main.spriteBatch.Draw(
                            pixel,
                            new Rectangle(screenX + firstPixel, screenY + row, lastPixel - firstPixel + 1, 1),
                            color);
                    }
                }

                return;
            }

            if (tile.IsHalfBlock && tile.Slope == SlopeType.Solid)
            {
                Main.spriteBatch.Draw(pixel, new Rectangle(screenX, screenY + 8, 16, 8), color);
                return;
            }

            if (tile.Slope == SlopeType.Solid)
            {
                Main.spriteBatch.Draw(pixel, new Rectangle(screenX, screenY, 16, 16), color);
                return;
            }

            for (int row = 0; row < 16; row++)
            {
                int clipLeft = 0;
                int clipRight = 0;

                switch (tile.Slope)
                {
                    case SlopeType.SlopeDownRight:
                        clipLeft = 15 - row;
                        break;
                    case SlopeType.SlopeDownLeft:
                        clipRight = 15 - row;
                        break;
                    case SlopeType.SlopeUpRight:
                        clipLeft = row;
                        break;
                    case SlopeType.SlopeUpLeft:
                        clipRight = row;
                        break;
                }

                int width = 16 - clipLeft - clipRight;
                if (width > 0)
                {
                    Main.spriteBatch.Draw(
                        pixel,
                        new Rectangle(screenX + clipLeft, screenY + row, width, 1),
                        color);
                }
            }
        }

        private static void DrawAdjacentHeatSpill(AdjacentHeatSpillEntry spill)
        {
            if (adjacentHeatSpillAtlas == null
                || spill.DirectionMask == 0
                || (spill.DirectionMask & (spill.DirectionMask - 1)) != 0
                || spill.Opacity <= 0.001f
                || spill.SourceEdgeCoverage == 0
                || !spill.Tile.HasTile
                || spill.Tile.IsActuated)
            {
                return;
            }

            Span<int> directionEntryIndices = stackalloc int[AdjacentHeatSpillDirectionCount];
            directionEntryIndices.Fill(-1);
            int firstDirectionIndex = -1;
            int directionEntryCount = 0;
            for (int directionIndex = 0; directionIndex < AdjacentHeatSpillDirectionCount; directionIndex++)
            {
                byte direction = (byte)(1 << directionIndex);
                if (!visibleHeatSpillIndices.TryGetValue(
                    PackHeatSpillDirectionKey(spill.X, spill.Y, direction),
                    out int entryIndex))
                {
                    continue;
                }

                directionEntryIndices[directionIndex] = entryIndex;
                directionEntryCount++;
                if (firstDirectionIndex < 0)
                    firstDirectionIndex = directionIndex;
            }

            // The outer draw loop visits every directional entry. Only the first
            // one for this target builds and draws the combined pixel-local field.
            if (firstDirectionIndex < 0
                || spill.DirectionMask != (byte)(1 << firstDirectionIndex))
            {
                return;
            }

            int screenX = (int)spill.ScreenPosition.X;
            int screenY = (int)spill.ScreenPosition.Y;
            if (directionEntryCount == 1
                && spill.SourceEdgeCoverage == ushort.MaxValue
                && spill.Tile.Slope == SlopeType.Solid
                && !spill.Tile.IsHalfBlock
                && !IsPlatformTile(spill.Tile))
            {
                DrawAdjacentHeatSpillRectangle(
                    screenX,
                    screenY,
                    firstDirectionIndex * AdjacentHeatSpillFrameSize,
                    spill.NativeRatioLevel * AdjacentHeatSpillFrameSize,
                    0,
                    0,
                    AdjacentHeatSpillFrameSize,
                    AdjacentHeatSpillFrameSize,
                    spill.Tint * spill.Opacity);
                return;
            }

            Span<byte> winningDirections = stackalloc byte[AdjacentHeatSpillFrameSize * AdjacentHeatSpillFrameSize];
            winningDirections.Clear();
            for (int localY = 0; localY < AdjacentHeatSpillFrameSize; localY++)
            {
                for (int localX = 0; localX < AdjacentHeatSpillFrameSize; localX++)
                {
                    if (!IsTileGeometryPixel(spill.Tile, localX, localY))
                        continue;

                    float strongestContribution = 0.001f;
                    byte winningDirection = 0;
                    for (int directionIndex = 0; directionIndex < AdjacentHeatSpillDirectionCount; directionIndex++)
                    {
                        int entryIndex = directionEntryIndices[directionIndex];
                        if (entryIndex < 0)
                            continue;

                        AdjacentHeatSpillEntry candidate = visibleHeatSpillEntries[entryIndex];
                        bool sourceCoversPixel = candidate.DirectionMask == SpillFromLeft
                            || candidate.DirectionMask == SpillFromRight
                                ? (candidate.SourceEdgeCoverage & (1 << localY)) != 0
                                : (candidate.SourceEdgeCoverage & (1 << localX)) != 0;
                        if (!sourceCoversPixel)
                            continue;

                        float fade = GetAdjacentHeatSpillDirectionFade(
                            candidate.DirectionMask,
                            localX,
                            localY);
                        float contribution = candidate.DesiredDisplayedRed * fade
                            - candidate.NativeDisplayedRed;
                        if (contribution > strongestContribution)
                        {
                            strongestContribution = contribution;
                            winningDirection = candidate.DirectionMask;
                        }
                    }

                    winningDirections[localY * AdjacentHeatSpillFrameSize + localX]
                        = winningDirection;
                }
            }

            // Greedily combine equal winners into rectangles. A single-sided
            // full block remains one draw, while complex corners and slopes use
            // only the small number of rectangles their silhouettes require.
            for (int localY = 0; localY < 16; localY++)
            {
                int localX = 0;
                while (localX < 16)
                {
                    byte direction = winningDirections[localY * 16 + localX];
                    if (direction == 0)
                    {
                        localX++;
                        continue;
                    }

                    int width = 1;
                    while (localX + width < 16
                        && winningDirections[localY * 16 + localX + width] == direction)
                    {
                        width++;
                    }

                    int height = 1;
                    bool canExtend = true;
                    while (localY + height < 16 && canExtend)
                    {
                        for (int scanX = localX; scanX < localX + width; scanX++)
                        {
                            if (winningDirections[(localY + height) * 16 + scanX] != direction)
                            {
                                canExtend = false;
                                break;
                            }
                        }

                        if (canExtend)
                            height++;
                    }

                    int directionIndex = GetAdjacentHeatSpillDirectionIndex(direction);
                    AdjacentHeatSpillEntry winner = visibleHeatSpillEntries[
                        directionEntryIndices[directionIndex]];
                    DrawAdjacentHeatSpillRectangle(
                        screenX,
                        screenY,
                        directionIndex * AdjacentHeatSpillFrameSize,
                        winner.NativeRatioLevel * AdjacentHeatSpillFrameSize,
                        localX,
                        localY,
                        width,
                        height,
                        winner.Tint * winner.Opacity);

                    for (int clearY = localY; clearY < localY + height; clearY++)
                    {
                        for (int clearX = localX; clearX < localX + width; clearX++)
                            winningDirections[clearY * 16 + clearX] = 0;
                    }

                    localX += width;
                }
            }
        }

        private static void DrawAdjacentHeatSpillRectangle(
            int screenX,
            int screenY,
            int frameLeft,
            int frameTop,
            int localX,
            int localY,
            int width,
            int height,
            Color color)
        {
            Main.spriteBatch.Draw(
                adjacentHeatSpillAtlas,
                new Rectangle(screenX + localX, screenY + localY, width, height),
                new Rectangle(frameLeft + localX, frameTop + localY, width, height),
                color);
        }

        private static int GetAdjacentHeatSpillDirectionIndex(byte direction)
        {
            if (direction == SpillFromLeft)
                return 0;
            if (direction == SpillFromRight)
                return 1;
            if (direction == SpillFromTop)
                return 2;
            if (direction == SpillFromBottom)
                return 3;
            return -1;
        }

        private static void DrawWavyMoltenBoundary(
            int tileX,
            int tileY,
            float intensity,
            float reveal,
            float normalizedDistance,
            int currentTick)
        {
            if (tileX < 0 || tileX >= Main.maxTilesX || tileY < 0 || tileY >= Main.maxTilesY)
                return;

            Tile tile = Main.tile[tileX, tileY];
            if (!tile.HasTile || tile.IsActuated)
                return;

            int worldLeft = tileX * 16;
            int worldTop = tileY * 16;
            float strength = GetMoltenVisualStrength(intensity);
            float moltenOpacity = GetMoltenVisualOpacity(intensity) * reveal
                * GetSpatialContourFade(normalizedDistance);
            if (moltenOpacity <= 0.001f)
                return;

            bool fullSolidTile = IsFullSolidTileGeometry(tile);
            float visualCoverage = GetMoltenVisualCoverage(tileX, tileY);

            if (!fullSolidTile || !IsMoltenFullSolidTile(tileX, tileY - 1, visualCoverage))
                DrawHorizontalBoundaryRuns(tile, worldLeft, worldTop, -1, strength, moltenOpacity, reveal, visualCoverage, currentTick);
            if (!fullSolidTile || !IsMoltenFullSolidTile(tileX, tileY + 1, visualCoverage))
                DrawHorizontalBoundaryRuns(tile, worldLeft, worldTop, 1, strength, moltenOpacity, reveal, visualCoverage, currentTick);
            if (!fullSolidTile || !IsMoltenFullSolidTile(tileX - 1, tileY, visualCoverage))
                DrawVerticalBoundaryRuns(tile, worldLeft, worldTop, -1, strength, moltenOpacity, reveal, visualCoverage, currentTick);
            if (!fullSolidTile || !IsMoltenFullSolidTile(tileX + 1, tileY, visualCoverage))
                DrawVerticalBoundaryRuns(tile, worldLeft, worldTop, 1, strength, moltenOpacity, reveal, visualCoverage, currentTick);
        }

        private static void DrawHorizontalBoundaryRuns(
            Tile tile,
            int worldLeft,
            int worldTop,
            int normalY,
            float strength,
            float moltenOpacity,
            float reveal,
            float visualCoverage,
            int currentTick)
        {
            int runStart = -1;
            int runWorldY = 0;
            int runOverhang = 0;
            float runTransition = 0f;

            for (int localX = 0; localX <= 16; localX++)
            {
                bool exposed = false;
                int worldY = 0;
                int overhang = 0;
                float transition = 0f;

                if (localX < 16)
                {
                    int localY = FindVerticalGeometryEdge(tile, localX, normalY < 0);
                    if (localY >= 0)
                    {
                        int worldX = worldLeft + localX;
                        worldY = worldTop + localY;
                        float neighborCoverage = GetMoltenVisualPixelCoverage(worldX, worldY + normalY);
                        transition = GetMoltenBoundaryTransition(visualCoverage, neighborCoverage);
                        exposed = transition > 0f;
                        if (exposed)
                        {
                            overhang = GetMoltenBoundaryOverhang(
                                worldX,
                                worldY,
                                0,
                                normalY,
                                reveal,
                                currentTick);
                        }
                    }
                }

                bool continuesRun = exposed
                    && runStart >= 0
                    && worldY == runWorldY
                    && overhang == runOverhang
                    && Math.Abs(transition - runTransition) <= 0.0001f;
                if (continuesRun)
                    continue;

                if (runStart >= 0)
                {
                    DrawMoltenBoundaryRun(
                        worldLeft + runStart,
                        runWorldY,
                        1,
                        0,
                        0,
                        normalY,
                        localX - runStart,
                        runOverhang,
                        strength,
                        moltenOpacity * runTransition,
                        currentTick);
                }

                if (exposed)
                {
                    runStart = localX;
                    runWorldY = worldY;
                    runOverhang = overhang;
                    runTransition = transition;
                }
                else
                {
                    runStart = -1;
                }
            }
        }

        private static void DrawVerticalBoundaryRuns(
            Tile tile,
            int worldLeft,
            int worldTop,
            int normalX,
            float strength,
            float moltenOpacity,
            float reveal,
            float visualCoverage,
            int currentTick)
        {
            int runStart = -1;
            int runWorldX = 0;
            int runOverhang = 0;
            float runTransition = 0f;

            for (int localY = 0; localY <= 16; localY++)
            {
                bool exposed = false;
                int worldX = 0;
                int overhang = 0;
                float transition = 0f;

                if (localY < 16)
                {
                    int localX = FindHorizontalGeometryEdge(tile, localY, normalX < 0);
                    if (localX >= 0)
                    {
                        worldX = worldLeft + localX;
                        int worldY = worldTop + localY;
                        float neighborCoverage = GetMoltenVisualPixelCoverage(worldX + normalX, worldY);
                        transition = GetMoltenBoundaryTransition(visualCoverage, neighborCoverage);
                        exposed = transition > 0f;
                        if (exposed)
                        {
                            overhang = GetMoltenBoundaryOverhang(
                                worldX,
                                worldY,
                                normalX,
                                0,
                                reveal,
                                currentTick);
                        }
                    }
                }

                bool continuesRun = exposed
                    && runStart >= 0
                    && worldX == runWorldX
                    && overhang == runOverhang
                    && Math.Abs(transition - runTransition) <= 0.0001f;
                if (continuesRun)
                    continue;

                if (runStart >= 0)
                {
                    DrawMoltenBoundaryRun(
                        runWorldX,
                        worldTop + runStart,
                        0,
                        1,
                        normalX,
                        0,
                        localY - runStart,
                        runOverhang,
                        strength,
                        moltenOpacity * runTransition,
                        currentTick);
                }

                if (exposed)
                {
                    runStart = localY;
                    runWorldX = worldX;
                    runOverhang = overhang;
                    runTransition = transition;
                }
                else
                {
                    runStart = -1;
                }
            }
        }

        private static int FindVerticalGeometryEdge(Tile tile, int localX, bool findTop)
        {
            return TryGetGeometryColumnRange(tile, localX, out int top, out int bottom)
                ? findTop ? top : bottom
                : -1;
        }

        private static int FindHorizontalGeometryEdge(Tile tile, int localY, bool findLeft)
        {
            return TryGetGeometryRowRange(tile, localY, out int left, out int right)
                ? findLeft ? left : right
                : -1;
        }

        private static bool IsTileGeometryPixel(Tile tile, int localX, int localY)
        {
            return TryGetGeometryRowRange(tile, localY, out int left, out int right)
                && localX >= left
                && localX <= right;
        }

        private static bool IsPlatformTile(Tile tile)
        {
            return tile.HasTile && TileID.Sets.Platforms[tile.TileType];
        }

        private static bool IsDoorHeatSurfaceTile(Tile tile)
        {
            if (!tile.HasTile || tile.IsActuated || tile.LiquidAmount > 0)
                return false;

            int tileType = tile.TileType;
            return tileType == TileID.ClosedDoor
                || tileType == TileID.OpenDoor
                || tileType == TileID.TallGateClosed
                || tileType == TileID.TallGateOpen
                || tileType == TileID.TrapdoorClosed
                || tileType == TileID.TrapdoorOpen;
        }

        private static bool IsHeatSurfaceTile(Tile tile)
        {
            if (!tile.HasTile || tile.IsActuated || tile.LiquidAmount > 0)
                return false;

            int tileType = tile.TileType;
            if (IsPlatformTile(tile))
                return true;

            // Frame-important tiles are foreground objects whose sprites can
            // span several cells, such as trees, doors, and furniture. A
            // per-cell terrain overlay turns those objects into red cutouts,
            // so heat is limited to actual solid terrain geometry.
            return Main.tileSolid[tileType]
                && !Main.tileSolidTop[tileType]
                && !Main.tileFrameImportant[tileType];
        }

        private static bool IsFullSolidTileGeometry(Tile tile)
        {
            return tile.HasTile
                && !tile.IsActuated
                && !IsPlatformTile(tile)
                && !tile.IsHalfBlock
                && tile.Slope == SlopeType.Solid;
        }

        private static bool IsMoltenFullSolidTile(int tileX, int tileY, float requiredCoverage)
        {
            if (tileX < 0 || tileX >= Main.maxTilesX
                || tileY < 0 || tileY >= Main.maxTilesY
                || visualHeatStates == null
                || !visualHeatStates.TryGetValue((tileX, tileY), out VisualHeatState state))
            {
                return false;
            }

            return state.Intensity > MoltenVisualThreshold
                && HasComparableMoltenCoverage(GetMoltenVisualCoverage(tileX, tileY, state), requiredCoverage)
                && IsFullSolidTileGeometry(Main.tile[tileX, tileY]);
        }

        private static float GetMoltenVisualCoverage(int tileX, int tileY)
        {
            if (visualHeatStates == null
                || !visualHeatStates.TryGetValue((tileX, tileY), out VisualHeatState state))
            {
                return 0f;
            }

            return GetMoltenVisualCoverage(tileX, tileY, state);
        }

        private static float GetMoltenVisualCoverage(int tileX, int tileY, VisualHeatState state)
        {
            if (state.Intensity <= MoltenVisualThreshold)
                return 0f;

            int buriedDepth = GetCachedBuriedDepth(tileX, tileY);
            float buriedMultiplier = buriedDepth >= 2 ? 0.28f : buriedDepth == 1 ? 0.62f : 1f;
            return GetMoltenVisualOpacity(state.Intensity) * state.Reveal
                * GetSpatialVisualFade(state.NormalizedDistance) * buriedMultiplier;
        }

        private static bool HasComparableMoltenCoverage(float neighborCoverage, float requiredCoverage)
        {
            return neighborCoverage > 0.10f
                && neighborCoverage >= requiredCoverage * MoltenNeighborCoverageRatio;
        }

        private static bool TryGetGeometryRowRange(Tile tile, int localY, out int left, out int right)
        {
            left = 0;
            right = 15;
            if (!tile.HasTile || tile.IsActuated || localY < 0 || localY >= 16)
                return false;

            if (IsPlatformTile(tile))
            {
                switch (tile.Slope)
                {
                    case SlopeType.SlopeDownRight:
                        left = Math.Max(0, 15 - localY);
                        right = Math.Min(15, 22 - localY);
                        break;
                    case SlopeType.SlopeDownLeft:
                        left = Math.Max(0, localY - 7);
                        right = Math.Min(15, localY);
                        break;
                    case SlopeType.SlopeUpRight:
                        left = Math.Max(0, localY);
                        right = Math.Min(15, localY + 7);
                        break;
                    case SlopeType.SlopeUpLeft:
                        left = Math.Max(0, 8 - localY);
                        right = Math.Min(15, 15 - localY);
                        break;
                    default:
                        return localY < 8;
                }

                return left <= right;
            }

            if (tile.IsHalfBlock && tile.Slope == SlopeType.Solid)
                return localY >= 8;

            switch (tile.Slope)
            {
                case SlopeType.SlopeDownRight:
                    left = 15 - localY;
                    break;
                case SlopeType.SlopeDownLeft:
                    right = localY;
                    break;
                case SlopeType.SlopeUpRight:
                    left = localY;
                    break;
                case SlopeType.SlopeUpLeft:
                    right = 15 - localY;
                    break;
            }

            return left <= right;
        }

        private static bool TryGetGeometryColumnRange(Tile tile, int localX, out int top, out int bottom)
        {
            top = 0;
            bottom = 15;
            if (!tile.HasTile || tile.IsActuated || localX < 0 || localX >= 16)
                return false;

            if (IsPlatformTile(tile))
            {
                switch (tile.Slope)
                {
                    case SlopeType.SlopeDownRight:
                        top = Math.Max(0, 15 - localX);
                        bottom = Math.Min(15, 22 - localX);
                        break;
                    case SlopeType.SlopeDownLeft:
                        top = Math.Max(0, localX);
                        bottom = Math.Min(15, localX + 7);
                        break;
                    case SlopeType.SlopeUpRight:
                        top = Math.Max(0, localX - 7);
                        bottom = Math.Min(15, localX);
                        break;
                    case SlopeType.SlopeUpLeft:
                        top = Math.Max(0, 8 - localX);
                        bottom = Math.Min(15, 15 - localX);
                        break;
                    default:
                        bottom = 7;
                        break;
                }

                return top <= bottom;
            }

            if (tile.IsHalfBlock && tile.Slope == SlopeType.Solid)
            {
                top = 8;
                return true;
            }

            switch (tile.Slope)
            {
                case SlopeType.SlopeDownRight:
                    top = 15 - localX;
                    break;
                case SlopeType.SlopeDownLeft:
                    top = localX;
                    break;
                case SlopeType.SlopeUpRight:
                    bottom = localX;
                    break;
                case SlopeType.SlopeUpLeft:
                    bottom = 15 - localX;
                    break;
            }

            return top <= bottom;
        }

        private static bool TryGetTileGeometryBounds(Tile tile, out Rectangle bounds)
        {
            if (!tile.HasTile || tile.IsActuated)
            {
                bounds = Rectangle.Empty;
                return false;
            }

            if (IsPlatformTile(tile) && tile.Slope == SlopeType.Solid)
            {
                bounds = new Rectangle(0, 0, 16, 8);
                return true;
            }

            if (tile.IsHalfBlock && tile.Slope == SlopeType.Solid)
            {
                bounds = new Rectangle(0, 8, 16, 8);
                return true;
            }

            // Solid blocks and every diagonal form span the complete tile cell.
            bounds = new Rectangle(0, 0, 16, 16);
            return true;
        }

        private static float GetMoltenVisualPixelCoverage(int worldX, int worldY)
        {
            if (worldX < 0 || worldY < 0
                || worldX >= Main.maxTilesX * 16 || worldY >= Main.maxTilesY * 16
                || visualHeatStates == null)
            {
                return 0f;
            }

            int tileX = worldX / 16;
            int tileY = worldY / 16;
            if (!visualHeatStates.TryGetValue((tileX, tileY), out VisualHeatState state))
                return 0f;

            Tile tile = Main.tile[tileX, tileY];
            return IsTileGeometryPixel(tile, worldX - tileX * 16, worldY - tileY * 16)
                ? GetMoltenVisualCoverage(tileX, tileY, state)
                : 0f;
        }

        private static float GetMoltenBoundaryTransition(float sourceCoverage, float neighborCoverage)
        {
            if (sourceCoverage <= 0.001f
                || HasComparableMoltenCoverage(neighborCoverage, sourceCoverage))
            {
                return 0f;
            }

            // Only the brighter side draws a shared boundary. Scale that skirt
            // by the actual coverage difference so a cooling or buried neighbor
            // receives a soft bridge instead of either a hard cell cut or a
            // full-strength internal outline.
            return MathHelper.Clamp(
                (sourceCoverage - neighborCoverage) / sourceCoverage,
                0f,
                1f);
        }

        private static int GetMoltenBoundaryOverhang(
            int worldX,
            int worldY,
            int normalX,
            int normalY,
            float reveal,
            int currentTick)
        {
            float alongEdge = normalX == 0 ? worldX : worldY;
            float directionPhase = normalX * 1.73f + normalY * 2.41f;
            float broadWave = (float)Math.Sin(alongEdge * 0.22f + currentTick * 0.025f + directionPhase);
            float fineWave = (float)Math.Sin(alongEdge * 0.47f - currentTick * 0.014f + directionPhase * 1.7f);
            float combinedWave = broadWave * 0.72f + fineWave * 0.28f;
            float waveAmount = MathHelper.Clamp(combinedWave * 0.5f + 0.5f, 0f, 1f);
            int overhang = MinimumMoltenOverhangPixels
                + (int)Math.Round((MaximumMoltenOverhangPixels - MinimumMoltenOverhangPixels) * waveAmount);
            float revealSweep = reveal * reveal * (3f - 2f * reveal);
            overhang = Math.Max(1, (int)Math.Ceiling(overhang * revealSweep));
            return overhang;
        }

        private static void DrawMoltenBoundaryRun(
            int worldX,
            int worldY,
            int tangentX,
            int tangentY,
            int normalX,
            int normalY,
            int length,
            int overhang,
            float strength,
            float moltenOpacity,
            int currentTick)
        {
            if (length <= 0
                || overhang <= 0
                || overhang > MaximumMoltenOverhangPixels
                || moltenOpacity <= 0.001f
                || moltenBoundaryAtlas == null)
            {
                return;
            }

            float alongEdge = normalX == 0
                ? worldX + (length - 1) * 0.5f
                : worldY + (length - 1) * 0.5f;
            float pulse = 0.92f + (float)Math.Sin(currentTick * 0.04f + alongEdge * 0.11f) * 0.08f;
            int screenX = (int)(worldX - Main.screenPosition.X);
            int screenY = (int)(worldY - Main.screenPosition.Y);
            Rectangle source;
            Rectangle destination;
            SpriteEffects effects = SpriteEffects.None;
            if (tangentX != 0)
            {
                source = new Rectangle(overhang - 1, 0, 1, overhang + 1);
                destination = new Rectangle(
                    screenX,
                    normalY < 0 ? screenY - overhang : screenY,
                    length,
                    overhang + 1);
                if (normalY < 0)
                    effects = SpriteEffects.FlipVertically;
            }
            else
            {
                source = new Rectangle(0, 5 + overhang, overhang + 1, 1);
                destination = new Rectangle(
                    normalX < 0 ? screenX - overhang : screenX,
                    screenY,
                    overhang + 1,
                    length);
                if (normalX < 0)
                    effects = SpriteEffects.FlipHorizontally;
            }

            Color boundaryTint = Color.Lerp(new Color(180, 88, 62), Color.White, strength)
                * (0.38f + strength * 0.42f) * moltenOpacity * pulse;
            Main.spriteBatch.Draw(
                moltenBoundaryAtlas,
                destination,
                source,
                boundaryTint,
                0f,
                Vector2.Zero,
                effects,
                0f);
        }

        public static void ApplyHeatInRadius(Vector2 worldCenter, float radius, int duration = DefaultHeatDuration, int owner = -1, int damage = 0)
        {
            if (heatedTiles == null)
                return;

            int centerTileX = (int)(worldCenter.X / 16f);
            int centerTileY = (int)(worldCenter.Y / 16f);
            int tileRadius = (int)(radius / 16f) + 1;

            int currentTick = (int)Main.GameUpdateCount;

            for (int x = centerTileX - tileRadius; x <= centerTileX + tileRadius; x++)
            {
                for (int y = centerTileY - tileRadius; y <= centerTileY + tileRadius; y++)
                {
                    if (x < 0 || x >= Main.maxTilesX || y < 0 || y >= Main.maxTilesY)
                        continue;

                    Tile tile = Main.tile[x, y];
                    if (!IsHeatSurfaceTile(tile))
                        continue;

                    float tileWorldX = x * 16f + 8f;
                    float tileWorldY = y * 16f + 8f;
                    float distance = Vector2.Distance(worldCenter, new Vector2(tileWorldX, tileWorldY));

                    if (distance > radius) continue;

                    var key = (x, y);

                    ApplyHeatDataUpdate(key, currentTick, duration, distance, radius, owner, damage);
                }
            }
        }

        public static void ApplyBeamImpact(int tileX, int tileY, int duration, int owner, int damage)
        {
            if (heatedTiles == null)
                return;

            if (tileX < 0 || tileX >= Main.maxTilesX || tileY < 0 || tileY >= Main.maxTilesY)
                return;

            Tile impactTile = Main.tile[tileX, tileY];
            // Preserve the beam's original impact acceptance. A foreground
            // object can stop the beam, while only valid terrain neighbors are
            // actually added to the heat map below.
            if (!impactTile.HasTile || !Main.tileSolid[impactTile.TileType])
                return;

            for (int dx = -6; dx <= 6; dx++)
            {
                for (int dy = -3; dy <= 3; dy++)
                {
                    int nx = tileX + dx;
                    int ny = tileY + dy;
                    if (nx < 0 || nx >= Main.maxTilesX || ny < 0 || ny >= Main.maxTilesY)
                        continue;

                    Tile neighbor = Main.tile[nx, ny];
                    if (!IsHeatSurfaceTile(neighbor) && !IsDoorHeatSurfaceTile(neighbor))
                        continue;

                    float horizontal = Math.Abs(dx);
                    float vertical = Math.Abs(dy) * 1.6f;
                    float distance = horizontal + vertical;
                    float maxRadius = 6f + Math.Max(0f, horizontal - 3f);
                    ApplyBeamHeatToTile(nx, ny, distance, maxRadius, duration, owner, damage);
                }
            }
        }

        private static void ApplyBeamHeatToTile(
            int x,
            int y,
            float distanceFromCenter,
            float maxRadius,
            int duration,
            int owner,
            int damage)
        {
            Tile tile = Main.tile[x, y];
            if (!IsHeatSurfaceTile(tile) && !IsDoorHeatSurfaceTile(tile))
                return;

            ApplyHeatDataUpdate(
                (x, y),
                (int)Main.GameUpdateCount,
                duration,
                distanceFromCenter,
                maxRadius,
                owner,
                damage);

            if (tile.TileType == TileID.TallGateClosed || tile.TileType == TileID.TallGateOpen)
            {
                ApplyConnectedTallGateHeat(
                    x,
                    y,
                    tile.TileType,
                    distanceFromCenter,
                    maxRadius,
                    duration,
                    owner,
                    damage);
            }
        }

        private static void ApplyConnectedTallGateHeat(
            int x,
            int y,
            ushort tileType,
            float distanceFromCenter,
            float maxRadius,
            int duration,
            int owner,
            int damage)
        {
            int currentTick = (int)Main.GameUpdateCount;
            for (int direction = -1; direction <= 1; direction += 2)
            {
                for (int step = 1; step <= 4; step++)
                {
                    int connectedY = y + direction * step;
                    if (connectedY < 0 || connectedY >= Main.maxTilesY)
                        break;

                    Tile connectedTile = Main.tile[x, connectedY];
                    if (!connectedTile.HasTile
                        || connectedTile.TileType != tileType
                        || !IsDoorHeatSurfaceTile(connectedTile))
                    {
                        break;
                    }

                    ApplyHeatDataUpdate(
                        (x, connectedY),
                        currentTick,
                        duration,
                        distanceFromCenter,
                        maxRadius,
                        owner,
                        damage);
                }
            }
        }

        public static void ApplyHeatToTile(int x, int y, float distanceFromCenter, float maxRadius, int duration = DefaultHeatDuration, int owner = -1, int damage = 0)
        {
            if (heatedTiles == null)
                return;

            if (x < 0 || x >= Main.maxTilesX || y < 0 || y >= Main.maxTilesY)
                return;

            Tile tile = Main.tile[x, y];
            if (!IsHeatSurfaceTile(tile))
                return;

            int currentTick = (int)Main.GameUpdateCount;
            var key = (x, y);

            ApplyHeatDataUpdate(key, currentTick, duration, distanceFromCenter, maxRadius, owner, damage);
        }

        public static int ApplyBeamPlatformHeat(
            Vector2 beamStart,
            Vector2 beamEnd,
            int duration = DefaultHeatDuration,
            int owner = -1,
            int damage = 0,
            List<Point> platformHits = null,
            HashSet<long> platformHitKeys = null)
        {
            if (heatedTiles == null
                || !float.IsFinite(beamStart.X)
                || !float.IsFinite(beamStart.Y)
                || !float.IsFinite(beamEnd.X)
                || !float.IsFinite(beamEnd.Y))
            {
                return 0;
            }

            Vector2 delta = beamEnd - beamStart;
            if (delta.LengthSquared() <= 0.01f)
                return 0;

            platformHeatSpreadSamples ??= new Dictionary<long, BeamHeatSpreadSample>();
            platformHeatSpreadSamples.Clear();
            platformHeatCenters ??= new List<Point>();
            platformHeatCenters.Clear();
            int heatedPlatformCount = 0;
            if (Math.Abs(delta.X) >= Math.Abs(delta.Y))
            {
                int firstTileX = Math.Max(0, (int)Math.Floor(
                    (Math.Min(beamStart.X, beamEnd.X) - RovaBeamHeatCollisionWidth) / 16f));
                int lastTileX = Math.Min(Main.maxTilesX - 1, (int)Math.Floor(
                    (Math.Max(beamStart.X, beamEnd.X) + RovaBeamHeatCollisionWidth) / 16f));

                for (int tileX = firstTileX; tileX <= lastTileX; tileX++)
                {
                    float slabLeft = tileX * 16f - RovaBeamHeatCollisionWidth;
                    float slabRight = (tileX + 1) * 16f + RovaBeamHeatCollisionWidth;
                    if (!TryGetSegmentSlabInterval(
                        beamStart.X,
                        delta.X,
                        slabLeft,
                        slabRight,
                        out float startT,
                        out float endT))
                    {
                        continue;
                    }

                    float startY = beamStart.Y + delta.Y * startT;
                    float endY = beamStart.Y + delta.Y * endT;
                    int firstTileY = Math.Max(0, (int)Math.Floor(
                        (Math.Min(startY, endY) - RovaBeamHeatCollisionWidth) / 16f));
                    int lastTileY = Math.Min(Main.maxTilesY - 1, (int)Math.Floor(
                        (Math.Max(startY, endY) + RovaBeamHeatCollisionWidth) / 16f));

                    for (int tileY = firstTileY; tileY <= lastTileY; tileY++)
                    {
                        if (TryApplyBeamPlatformHeat(
                            tileX,
                            tileY,
                            beamStart,
                            beamEnd,
                            platformHits,
                            platformHitKeys))
                        {
                            heatedPlatformCount++;
                        }
                    }
                }
            }
            else
            {
                int firstTileY = Math.Max(0, (int)Math.Floor(
                    (Math.Min(beamStart.Y, beamEnd.Y) - RovaBeamHeatCollisionWidth) / 16f));
                int lastTileY = Math.Min(Main.maxTilesY - 1, (int)Math.Floor(
                    (Math.Max(beamStart.Y, beamEnd.Y) + RovaBeamHeatCollisionWidth) / 16f));

                for (int tileY = firstTileY; tileY <= lastTileY; tileY++)
                {
                    float slabTop = tileY * 16f - RovaBeamHeatCollisionWidth;
                    float slabBottom = (tileY + 1) * 16f + RovaBeamHeatCollisionWidth;
                    if (!TryGetSegmentSlabInterval(
                        beamStart.Y,
                        delta.Y,
                        slabTop,
                        slabBottom,
                        out float startT,
                        out float endT))
                    {
                        continue;
                    }

                    float startX = beamStart.X + delta.X * startT;
                    float endX = beamStart.X + delta.X * endT;
                    int firstTileX = Math.Max(0, (int)Math.Floor(
                        (Math.Min(startX, endX) - RovaBeamHeatCollisionWidth) / 16f));
                    int lastTileX = Math.Min(Main.maxTilesX - 1, (int)Math.Floor(
                        (Math.Max(startX, endX) + RovaBeamHeatCollisionWidth) / 16f));

                    for (int tileX = firstTileX; tileX <= lastTileX; tileX++)
                    {
                        if (TryApplyBeamPlatformHeat(
                            tileX,
                            tileY,
                            beamStart,
                            beamEnd,
                            platformHits,
                            platformHitKeys))
                        {
                            heatedPlatformCount++;
                        }
                    }
                }
            }

            AccumulateBeamPlatformHeatCenters();
            ApplyAccumulatedBeamPlatformHeat(duration, owner, damage);
            return heatedPlatformCount;
        }

        internal static void ApplyBeamPlatformTileBatch(
            IReadOnlyList<Point> platformTiles,
            int duration,
            int owner,
            int damage)
        {
            if (heatedTiles == null || platformTiles == null || platformTiles.Count == 0)
                return;

            platformHeatSpreadSamples ??= new Dictionary<long, BeamHeatSpreadSample>();
            platformHeatSpreadSamples.Clear();
            platformHeatCenters ??= new List<Point>();
            platformHeatCenters.Clear();
            int tileCount = Math.Min(platformTiles.Count, MaxBeamPlatformBatchTiles);
            for (int i = 0; i < tileCount; i++)
            {
                Point point = platformTiles[i];
                if (point.X < 0
                    || point.X >= Main.maxTilesX
                    || point.Y < 0
                    || point.Y >= Main.maxTilesY)
                {
                    continue;
                }

                Tile tile = Main.tile[point.X, point.Y];
                if (!IsPlatformTile(tile) || !IsHeatSurfaceTile(tile))
                    continue;

                platformHeatCenters.Add(point);
            }

            AccumulateBeamPlatformHeatCenters();
            ApplyAccumulatedBeamPlatformHeat(duration, owner, damage);
        }

        private static bool TryGetSegmentSlabInterval(
            float segmentStart,
            float segmentDelta,
            float slabMinimum,
            float slabMaximum,
            out float startT,
            out float endT)
        {
            float firstT = (slabMinimum - segmentStart) / segmentDelta;
            float secondT = (slabMaximum - segmentStart) / segmentDelta;
            startT = Math.Max(0f, Math.Min(firstT, secondT));
            endT = Math.Min(1f, Math.Max(firstT, secondT));
            return startT <= endT;
        }

        private static bool TryApplyBeamPlatformHeat(
            int tileX,
            int tileY,
            Vector2 beamStart,
            Vector2 beamEnd,
            List<Point> platformHits,
            HashSet<long> platformHitKeys)
        {
            Tile tile = Main.tile[tileX, tileY];
            if (!IsPlatformTile(tile)
                || !IsHeatSurfaceTile(tile)
                || !DoesBeamIntersectPlatformGeometry(tile, tileX, tileY, beamStart, beamEnd))
            {
                return false;
            }

            platformHeatCenters.Add(new Point(tileX, tileY));
            if (platformHits != null
                && platformHitKeys != null
                && platformHits.Count < MaxBeamPlatformBatchTiles)
            {
                long packedKey = ((long)tileX << 32) | (uint)tileY;
                if (platformHitKeys.Add(packedKey))
                    platformHits.Add(new Point(tileX, tileY));
            }

            return true;
        }

        private static void AccumulateBeamPlatformHeatCenters()
        {
            if (platformHeatCenters == null || platformHeatCenters.Count == 0)
                return;

            platformHeatCenters.Sort(ComparePlatformHeatCenters);
            int index = 0;
            while (index < platformHeatCenters.Count)
            {
                int centerTileY = platformHeatCenters[index].Y;
                int runStartX = platformHeatCenters[index].X;
                int runEndX = runStartX;
                index++;

                while (index < platformHeatCenters.Count
                    && platformHeatCenters[index].Y == centerTileY)
                {
                    int tileX = platformHeatCenters[index].X;
                    if (tileX > runEndX + 1)
                    {
                        AccumulateBeamPlatformHeatRun(runStartX, runEndX, centerTileY);
                        runStartX = tileX;
                    }

                    runEndX = Math.Max(runEndX, tileX);
                    index++;
                }

                AccumulateBeamPlatformHeatRun(runStartX, runEndX, centerTileY);
            }
        }

        private static void AccumulateBeamPlatformHeatRun(
            int runStartX,
            int runEndX,
            int centerTileY)
        {
            int firstTileX = Math.Max(0, runStartX - 6);
            int lastTileX = Math.Min(Main.maxTilesX - 1, runEndX + 6);
            for (int dy = -3; dy <= 3; dy++)
            {
                int tileY = centerTileY + dy;
                if (tileY < 0 || tileY >= Main.maxTilesY)
                    continue;

                for (int tileX = firstTileX; tileX <= lastTileX; tileX++)
                {
                    Tile tile = Main.tile[tileX, tileY];
                    if (!IsHeatSurfaceTile(tile) && !IsDoorHeatSurfaceTile(tile))
                        continue;

                    int minimumHorizontal;
                    int maximumHorizontal;
                    if (tileX < runStartX)
                    {
                        minimumHorizontal = runStartX - tileX;
                        maximumHorizontal = Math.Min(6, runEndX - tileX);
                    }
                    else if (tileX > runEndX)
                    {
                        minimumHorizontal = tileX - runEndX;
                        maximumHorizontal = Math.Min(6, tileX - runStartX);
                    }
                    else
                    {
                        minimumHorizontal = 0;
                        maximumHorizontal = 0;
                    }

                    BeamHeatSpreadSample incoming = GetStrongestBeamHeatSpreadSample(
                        minimumHorizontal,
                        maximumHorizontal,
                        Math.Abs(dy));

                    long packedKey = ((long)tileX << 32) | (uint)tileY;
                    if (!platformHeatSpreadSamples.TryGetValue(packedKey, out BeamHeatSpreadSample existing)
                        || incoming.NormalizedDistance < existing.NormalizedDistance)
                    {
                        platformHeatSpreadSamples[packedKey] = incoming;
                    }
                }
            }
        }

        private static BeamHeatSpreadSample GetStrongestBeamHeatSpreadSample(
            int minimumHorizontal,
            int maximumHorizontal,
            int verticalDistance)
        {
            BeamHeatSpreadSample strongest = default;
            float strongestNormalizedDistance = float.MaxValue;
            for (int horizontal = minimumHorizontal; horizontal <= maximumHorizontal; horizontal++)
            {
                float maxRadius = 6f + Math.Max(0f, horizontal - 3f);
                float distance = horizontal + verticalDistance * 1.6f;
                BeamHeatSpreadSample candidate = new BeamHeatSpreadSample(distance, maxRadius);
                if (candidate.NormalizedDistance < strongestNormalizedDistance)
                {
                    strongest = candidate;
                    strongestNormalizedDistance = candidate.NormalizedDistance;
                }
            }

            return strongest;
        }

        private static int ComparePlatformHeatCenters(Point left, Point right)
        {
            int rowComparison = left.Y.CompareTo(right.Y);
            return rowComparison != 0 ? rowComparison : left.X.CompareTo(right.X);
        }

        private static void ApplyAccumulatedBeamPlatformHeat(int duration, int owner, int damage)
        {
            foreach (KeyValuePair<long, BeamHeatSpreadSample> entry in platformHeatSpreadSamples)
            {
                int tileX = (int)(entry.Key >> 32);
                int tileY = (int)entry.Key;
                BeamHeatSpreadSample sample = entry.Value;
                ApplyBeamHeatToTile(
                    tileX,
                    tileY,
                    sample.DistanceFromCenter,
                    sample.MaxRadius,
                    duration,
                    owner,
                    damage);
            }
        }

        private static bool DoesBeamIntersectPlatformGeometry(
            Tile tile,
            int tileX,
            int tileY,
            Vector2 beamStart,
            Vector2 beamEnd)
        {
            int worldLeft = tileX * 16;
            int worldTop = tileY * 16;
            float collisionPoint = 0f;

            if (tile.Slope == SlopeType.Solid)
            {
                return Collision.CheckAABBvLineCollision(
                    new Vector2(worldLeft, worldTop),
                    new Vector2(16f, 8f),
                    beamStart,
                    beamEnd,
                    RovaBeamHeatCollisionWidth,
                    ref collisionPoint);
            }

            for (int localY = 0; localY < 16; localY++)
            {
                if (!TryGetGeometryRowRange(tile, localY, out int left, out int right))
                    continue;

                if (Collision.CheckAABBvLineCollision(
                    new Vector2(worldLeft + left, worldTop + localY),
                    new Vector2(right - left + 1f, 1f),
                    beamStart,
                    beamEnd,
                    RovaBeamHeatCollisionWidth,
                    ref collisionPoint))
                {
                    return true;
                }
            }

            return false;
        }

        private static void ApplyHeatDataUpdate(
            (int x, int y) key,
            int currentTick,
            int duration,
            float distanceFromCenter,
            float maxRadius,
            int owner,
            int damage)
        {
            TileHeatData incoming = new TileHeatData(
                currentTick,
                duration,
                distanceFromCenter,
                maxRadius,
                owner,
                damage);

            if (!heatedTiles.TryGetValue(key, out TileHeatData existing))
            {
                heatedTiles[key] = incoming;
                return;
            }

            bool sameOwnedSource = owner >= 0 && owner == existing.Owner;
            if (sameOwnedSource)
            {
                // A moving beam can revisit the same tile from a farther impact
                // point. Refresh the lifetime and damage without replacing the
                // strongest spatial profile, otherwise an actively reheated tile
                // can become visually cooler while it remains damaging.
                TileHeatData strongestProfile = incoming.NormalizedDistance < existing.NormalizedDistance
                    ? incoming
                    : existing;
                int elapsedTicks = Math.Max(0, currentTick - existing.StartTick);
                int remainingDuration = Math.Max(0, existing.Duration - elapsedTicks);
                heatedTiles[key] = new TileHeatData(
                    currentTick,
                    Math.Max(remainingDuration, duration),
                    strongestProfile.DistanceFromCenter,
                    strongestProfile.MaxRadius,
                    owner,
                    Math.Max(existing.Damage, damage));
                return;
            }

            // Ownership changes only when the fresh incoming source is hotter
            // than the existing tile at this tick. A nearly expired inner hit
            // can therefore be refreshed by another player's outer hit, while
            // a high-damage but visibly cooler hit cannot dim active geometry.
            // Stable exact ties prevent owners from oscillating every packet.
            float existingIntensity = GetHeatIntensity(existing, currentTick);
            float incomingIntensity = GetHeatIntensity(incoming, currentTick);
            bool incomingIsHotter = incomingIntensity > existingIntensity + 0.0001f;
            bool equalHeatWithStrongerDamage = Math.Abs(incomingIntensity - existingIntensity) <= 0.0001f
                && damage > existing.Damage;
            if (incomingIsHotter || equalHeatWithStrongerDamage)
            {
                int elapsedTicks = Math.Max(0, currentTick - existing.StartTick);
                int remainingDuration = Math.Max(0, existing.Duration - elapsedTicks);
                heatedTiles[key] = new TileHeatData(
                    currentTick,
                    Math.Max(remainingDuration, duration),
                    distanceFromCenter,
                    maxRadius,
                    owner,
                    damage);
            }
        }

        private void UpdateHeatedTiles()
        {
            if (heatedTiles == null || heatedTiles.Count == 0)
                return;

            int currentTick = (int)Main.GameUpdateCount;
            bool allowVisuals = !Main.dedServ;
            tilesToRemove.Clear();

            foreach (var kvp in heatedTiles)
            {
                var data = kvp.Value;

                if (data.IsExpired(currentTick))
                {
                    tilesToRemove.Add(kvp.Key);
                    continue;
                }

                int x = kvp.Key.x;
                int y = kvp.Key.y;

                if (x < 0 || x >= Main.maxTilesX || y < 0 || y >= Main.maxTilesY
                    || (!IsHeatSurfaceTile(Main.tile[x, y])
                        && !IsDoorHeatSurfaceTile(Main.tile[x, y])))
                {
                    tilesToRemove.Add(kvp.Key);
                    continue;
                }

                float progress = data.GetProgress(currentTick);
                float normalizedDist = data.NormalizedDistance;
                float intensity = GetHeatIntensity(data, currentTick);
                Vector2 screenPos = new Vector2(x * 16f, y * 16f) - Main.screenPosition;
                bool updateLocalTileVisuals = allowVisuals && IsHeatTileOnScreen(screenPos, 192f);

                if (updateLocalTileVisuals)
                {
                    int buriedDepth = GetCachedBuriedDepth(x, y);
                    Color heatColor;
                    float lightMultiplier;

                    if (buriedDepth >= 2)
                    {
                        heatColor = DarkRed;
                        lightMultiplier = 0.3f;
                    }
                    else if (buriedDepth == 1)
                    {
                        Color normalColor = GetHeatColor(progress, normalizedDist);
                        heatColor = Color.Lerp(normalColor, DarkRed, 0.6f);
                        lightMultiplier = 0.8f;
                    }
                    else
                    {
                        heatColor = GetHeatColor(progress, normalizedDist);
                        lightMultiplier = 1f;
                    }

                    float moltenLightAmount = GetMoltenVisualStrength(intensity);
                    Color emittedLightColor = Color.Lerp(MoltenBodyRed, LavaRed, moltenLightAmount);
                    if (buriedDepth == 0)
                        lightMultiplier = MathHelper.Lerp(0.8f, 1.65f, moltenLightAmount);

                    Vector3 lightColor = emittedLightColor.ToVector3() * intensity * lightMultiplier;
                    Lighting.AddLight(new Vector2(x * 16f + 8f, y * 16f + 8f), lightColor);

                    if (buriedDepth == 0 && intensity > 0.3f)
                        SpawnHeatSurfaceParticles(x, y, intensity, normalizedDist, heatColor);
                }

                // Keep the original heated-surface ember source. It is capped
                // independently from the new RovaCenter overheat release, so
                // either source can remain visible without suppressing the other.
                if (Main.netMode != NetmodeID.MultiplayerClient
                    && intensity > 0.5f
                    && data.Owner >= 0
                    && data.Owner < Main.maxPlayers
                    && Main.player[data.Owner].active
                    && currentTick - lastFallingEmberSpawnTick >= FallingEmberSpawnIntervalTicks
                    && tileEmberCountsByOwner != null
                    && tileEmberCountsByOwner[data.Owner] < 12
                    && HasAirBelow(x, y)
                    && Main.rand.NextFloat() < 0.0045f * intensity
                    && SpawnFallingTileEmber(x, y, data))
                {
                    lastFallingEmberSpawnTick = currentTick;
                }

            }

            foreach (var key in tilesToRemove)
            {
                heatedTiles.Remove(key);
            }
        }

        private static void UpdateVisualHeatStates()
        {
            if (Main.dedServ || visualHeatStates == null || visualStatesToRemove == null || visualStateKeys == null)
                return;

            int currentTick = (int)Main.GameUpdateCount;
            visualStatesToRemove.Clear();

            if (heatedTiles != null)
            {
                foreach (var kvp in heatedTiles)
                {
                    float targetIntensity = GetHeatIntensity(kvp.Value, currentTick);
                    bool hadVisualState = visualHeatStates.TryGetValue(kvp.Key, out VisualHeatState state);
                    if (!hadVisualState)
                    {
                        state = new VisualHeatState(0f, kvp.Value.NormalizedDistance, 0f);
                    }

                    float rate = targetIntensity > state.Intensity
                        ? hadVisualState && targetIntensity >= HeatDamageIntensityThreshold
                            ? VisualHeatReheatRiseRate
                            : VisualHeatRiseRate
                        : VisualHeatFallRate;
                    state.Intensity = MathHelper.Lerp(state.Intensity, targetIntensity, rate);
                    float targetDistance = kvp.Value.NormalizedDistance;
                    state.NormalizedDistance = targetDistance < state.NormalizedDistance
                        ? targetDistance
                        : MathHelper.Lerp(state.NormalizedDistance, targetDistance, VisualHeatFallRate);

                    float revealTarget = targetIntensity > MoltenVisualThreshold ? 1f : 0f;
                    float revealRate = revealTarget > state.Reveal
                        ? MathHelper.Lerp(0.12f, 0.07f, kvp.Value.NormalizedDistance)
                        : 0.045f;
                    state.Reveal = MathHelper.Lerp(state.Reveal, revealTarget, revealRate);
                    visualHeatStates[kvp.Key] = state;
                }
            }

            visualStateKeys.Clear();
            visualStateKeys.AddRange(visualHeatStates.Keys);
            for (int i = 0; i < visualStateKeys.Count; i++)
            {
                (int x, int y) key = visualStateKeys[i];
                if (heatedTiles != null && heatedTiles.ContainsKey(key))
                    continue;

                VisualHeatState state = visualHeatStates[key];
                state.Intensity = MathHelper.Lerp(state.Intensity, 0f, VisualHeatFallRate);
                state.Reveal = MathHelper.Lerp(state.Reveal, 0f, 0.045f);
                if (state.Intensity <= VisualHeatRemovalThreshold)
                    visualStatesToRemove.Add(key);
                else
                    visualHeatStates[key] = state;
            }

            for (int i = 0; i < visualStatesToRemove.Count; i++)
                visualHeatStates.Remove(visualStatesToRemove[i]);
        }

        private static void UpdateHeatFieldProjectiles()
        {
            if (heatFieldProjectileIndices == null
                || heatFieldOwnerNeeded == null
                || heatFieldBoundsByOwner == null
                || heatFieldDamageByOwner == null)
            {
                return;
            }

            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            Array.Clear(heatFieldOwnerNeeded, 0, heatFieldOwnerNeeded.Length);
            Array.Clear(heatFieldBoundsByOwner, 0, heatFieldBoundsByOwner.Length);
            Array.Clear(heatFieldDamageByOwner, 0, heatFieldDamageByOwner.Length);
            int currentTick = (int)Main.GameUpdateCount;

            if (heatedTiles != null)
            {
                foreach (var kvp in heatedTiles)
                {
                    TileHeatData data = kvp.Value;
                    int owner = data.Owner;
                    if (owner < 0 || owner >= heatFieldOwnerNeeded.Length
                        || data.Damage <= 0
                        || !Main.player[owner].active
                        || GetHeatIntensity(data, currentTick) < HeatDamageIntensityThreshold)
                    {
                        continue;
                    }

                    int x = kvp.Key.x;
                    int y = kvp.Key.y;
                    if (x < 0 || x >= Main.maxTilesX || y < 0 || y >= Main.maxTilesY)
                        continue;

                    Tile tile = Main.tile[x, y];
                    if (!TryGetTileGeometryBounds(tile, out Rectangle localBounds))
                        continue;

                    Rectangle tileBounds = new Rectangle(
                        x * 16 + localBounds.Left - HeatCollisionOverhangPixels,
                        y * 16 + localBounds.Top - HeatCollisionOverhangPixels,
                        localBounds.Width + HeatCollisionOverhangPixels * 2,
                        localBounds.Height + HeatCollisionOverhangPixels * 2);
                    heatFieldBoundsByOwner[owner] = heatFieldOwnerNeeded[owner]
                        ? Rectangle.Union(heatFieldBoundsByOwner[owner], tileBounds)
                        : tileBounds;
                    heatFieldDamageByOwner[owner] = Math.Max(
                        heatFieldDamageByOwner[owner],
                        Math.Max(1, data.Damage / 5));
                    heatFieldOwnerNeeded[owner] = true;
                }
            }

            int heatFieldType = ModContent.ProjectileType<RovaHeatField>();
            for (int owner = 0; owner < heatFieldProjectileIndices.Length; owner++)
            {
                int projectileIndex = heatFieldProjectileIndices[owner];
                bool validProjectile = projectileIndex >= 0
                    && projectileIndex < Main.maxProjectiles
                    && Main.projectile[projectileIndex].active
                    && Main.projectile[projectileIndex].type == heatFieldType
                    && Main.projectile[projectileIndex].owner == owner;

                if (!validProjectile)
                {
                    projectileIndex = -1;
                    heatFieldProjectileIndices[owner] = -1;
                }

                if (!heatFieldOwnerNeeded[owner])
                {
                    if (validProjectile)
                        Main.projectile[projectileIndex].Kill();

                    heatFieldProjectileIndices[owner] = -1;
                    continue;
                }

                Rectangle bounds = heatFieldBoundsByOwner[owner];
                int damage = heatFieldDamageByOwner[owner];
                if (validProjectile)
                {
                    Projectile activeHeatField = Main.projectile[projectileIndex];
                    bool changed = activeHeatField.position.X != bounds.X
                        || activeHeatField.position.Y != bounds.Y
                        || activeHeatField.width != bounds.Width
                        || activeHeatField.height != bounds.Height
                        || activeHeatField.damage != damage;
                    activeHeatField.position = new Vector2(bounds.X, bounds.Y);
                    activeHeatField.width = Math.Max(2, bounds.Width);
                    activeHeatField.height = Math.Max(2, bounds.Height);
                    activeHeatField.damage = damage;
                    activeHeatField.velocity = Vector2.Zero;
                    activeHeatField.timeLeft = 60;
                    if (changed && Main.netMode == NetmodeID.Server)
                        activeHeatField.netUpdate = true;
                    continue;
                }

                int newIndex = Projectile.NewProjectile(
                    new EntitySource_Misc("SariaTileHeatField"),
                    bounds.Center.ToVector2(),
                    Vector2.Zero,
                    heatFieldType,
                    damage,
                    0f,
                    owner);

                if (newIndex < 0 || newIndex >= Main.maxProjectiles)
                    continue;

                Projectile heatField = Main.projectile[newIndex];
                heatField.position = new Vector2(bounds.X, bounds.Y);
                heatField.width = Math.Max(2, bounds.Width);
                heatField.height = Math.Max(2, bounds.Height);
                heatField.timeLeft = 60;
                heatField.netUpdate = true;
                heatFieldProjectileIndices[owner] = newIndex;
            }
        }

        internal static bool TryGetHeatFieldBounds(int owner, out Rectangle bounds, out int damage)
        {
            bounds = Rectangle.Empty;
            damage = 0;
            if (heatFieldOwnerNeeded == null
                || heatFieldBoundsByOwner == null
                || heatFieldDamageByOwner == null
                || owner < 0
                || owner >= heatFieldOwnerNeeded.Length
                || !heatFieldOwnerNeeded[owner])
            {
                return false;
            }

            bounds = heatFieldBoundsByOwner[owner];
            damage = heatFieldDamageByOwner[owner];
            return bounds.Width > 0 && bounds.Height > 0 && damage > 0;
        }

        private static void UpdateRovaCenterOverheatEmbers()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient
                || centerOverheatStates == null
                || centerObservedThisTick == null
                || centerHasActiveBeam == null
                || centerOverheatEmberInstances == null
                || tileHeatEmberInstances == null
                || centerEmberCountsByOwner == null
                || tileEmberCountsByOwner == null
                || trackedPlatformHeatBeams == null
                || pendingPlatformHeatTiles == null
                || pendingPlatformHeatKeys == null
                || pendingPlatformHeatOwners == null
                || pendingPlatformHeatDamage == null
                || centerSlotsByHandle == null)
            {
                return;
            }

            Array.Clear(centerObservedThisTick, 0, centerObservedThisTick.Length);
            Array.Clear(centerHasActiveBeam, 0, centerHasActiveBeam.Length);
            Array.Clear(centerEmberCountsByOwner, 0, centerEmberCountsByOwner.Length);
            Array.Clear(tileEmberCountsByOwner, 0, tileEmberCountsByOwner.Length);
            centerSlotsByHandle.Clear();
            activeRovaBeamIndices.Clear();

            int activeCenterEmberCount = 0;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile projectile = Main.projectile[i];
                RovaBeam activePlatformHeatBeam = projectile.active
                    ? projectile.ModProjectile as RovaBeam
                    : null;
                if (activePlatformHeatBeam != null)
                    activeRovaBeamIndices.Add(i);
                if (!ReferenceEquals(trackedPlatformHeatBeams[i], activePlatformHeatBeam))
                {
                    if (Main.netMode == NetmodeID.Server)
                        FlushPendingBeamPlatformTiles(i);

                    pendingPlatformHeatTiles[i]?.Clear();
                    pendingPlatformHeatKeys[i]?.Clear();
                    trackedPlatformHeatBeams[i] = activePlatformHeatBeam;
                }

                RovaEmber ember = projectile.active ? projectile.ModProjectile as RovaEmber : null;
                if (!ReferenceEquals(centerOverheatEmberInstances[i], ember))
                    centerOverheatEmberInstances[i] = null;
                if (!ReferenceEquals(tileHeatEmberInstances[i], ember))
                    tileHeatEmberInstances[i] = null;

                if (ember != null)
                {
                    int emberOwner = projectile.owner;
                    if (ReferenceEquals(centerOverheatEmberInstances[i], ember))
                    {
                        activeCenterEmberCount++;
                        if (emberOwner >= 0 && emberOwner < centerEmberCountsByOwner.Length)
                            centerEmberCountsByOwner[emberOwner]++;
                    }

                    if (ReferenceEquals(tileHeatEmberInstances[i], ember)
                        && emberOwner >= 0
                        && emberOwner < tileEmberCountsByOwner.Length)
                    {
                        tileEmberCountsByOwner[emberOwner]++;
                    }
                }

                if (!projectile.active)
                    continue;

                if (projectile.ModProjectile is not RovaCenter)
                    continue;

                int handle = RovaProjectileLink.GetHandle(projectile);
                centerObservedThisTick[i] = true;
                centerSlotsByHandle[(projectile.owner, handle)] = i;

                CenterOverheatState state = centerOverheatStates[i];
                if (state.Owner != projectile.owner || state.Handle != handle)
                {
                    state = new CenterOverheatState
                    {
                        Owner = projectile.owner,
                        Handle = handle
                    };
                    centerOverheatStates[i] = state;
                }
            }

            // Resolve every beam to its exact linked center in one projectile
            // pass. This avoids a center-by-beam nested scan and keeps the work
            // constant regardless of the number of NPCs in the fight.
            bool flushBeamPlatformHeat = Main.GameUpdateCount % BeamPlatformHeatIntervalTicks == 0;
            foreach (int i in activeRovaBeamIndices)
            {
                Projectile projectile = Main.projectile[i];
                RovaBeam beam = (RovaBeam)projectile.ModProjectile;

                if (!beam.IsBeamEnding)
                {
                    Vector2 beamStart = projectile.Center;
                    Vector2 beamEnd = beam.GetBeamEndpointPosition();
                    List<Point> platformHits = null;
                    HashSet<long> platformHitKeys = null;
                    if (Main.netMode == NetmodeID.Server)
                    {
                        platformHits = pendingPlatformHeatTiles[i] ??= new List<Point>(16);
                        platformHitKeys = pendingPlatformHeatKeys[i] ??= new HashSet<long>();
                        pendingPlatformHeatOwners[i] = projectile.owner;
                        pendingPlatformHeatDamage[i] = projectile.damage;
                    }

                    ApplyBeamPlatformHeat(
                        beamStart,
                        beamEnd,
                        DefaultHeatDuration,
                        projectile.owner,
                        projectile.damage,
                        platformHits,
                        platformHitKeys);
                }

                if (Main.netMode == NetmodeID.Server
                    && (flushBeamPlatformHeat
                        || beam.IsBeamEnding
                        || (pendingPlatformHeatTiles[i]?.Count ?? 0) >= MaxBeamPlatformBatchTiles))
                {
                    FlushPendingBeamPlatformTiles(i);
                }

                if (centerSlotsByHandle.TryGetValue(
                    (projectile.owner, (int)projectile.ai[0]),
                    out int centerSlot))
                {
                    centerHasActiveBeam[centerSlot] = true;
                }
            }

            for (int i = 0; i < centerOverheatStates.Length; i++)
            {
                if (!centerObservedThisTick[i])
                {
                    centerOverheatStates[i] = default;
                    continue;
                }

                Projectile center = Main.projectile[i];
                CenterOverheatState state = centerOverheatStates[i];
                if (centerHasActiveBeam[i])
                {
                    state.FiringTicks = Math.Min(
                        CenterOverheatWarmupTicks + CenterOverheatCapacity,
                        state.FiringTicks + 1);
                    if (state.FiringTicks > CenterOverheatWarmupTicks)
                    {
                        state.StoredHeat = Math.Min(
                            CenterOverheatCapacity,
                            state.StoredHeat + 1);
                    }

                    // Emission begins promptly once this linked beam has fully
                    // stopped, but never while any part of it remains active.
                    state.EmberCooldown = 0;
                    centerOverheatStates[i] = state;
                    continue;
                }

                state.FiringTicks = 0;
                if (state.EmberCooldown > 0)
                    state.EmberCooldown--;

                int owner = center.owner;
                bool canSpawn = state.StoredHeat >= CenterOverheatEmberCost
                    && state.EmberCooldown <= 0
                    && activeCenterEmberCount < MaxCenterEmbersGlobal
                    && owner >= 0
                    && owner < Main.maxPlayers
                    && Main.player[owner].active
                    && centerEmberCountsByOwner[owner] < MaxCenterEmbersPerOwner;
                if (canSpawn && SpawnCenterOverheatEmber(center))
                {
                    state.StoredHeat -= CenterOverheatEmberCost;
                    state.EmberCooldown = CenterOverheatEmberIntervalTicks;
                    activeCenterEmberCount++;
                    centerEmberCountsByOwner[owner]++;
                }
                else if (state.StoredHeat > 0)
                {
                    // Idle heat cools by one point per tick. A successful
                    // release pays the larger ember cost instead on that tick.
                    state.StoredHeat--;
                }

                centerOverheatStates[i] = state;
            }
        }

        private static void FlushPendingBeamPlatformTiles(int projectileIndex)
        {
            if (Main.netMode != NetmodeID.Server
                || pendingPlatformHeatTiles == null
                || pendingPlatformHeatKeys == null
                || projectileIndex < 0
                || projectileIndex >= pendingPlatformHeatTiles.Length)
            {
                return;
            }

            List<Point> platformTiles = pendingPlatformHeatTiles[projectileIndex];
            if (platformTiles == null || platformTiles.Count == 0)
                return;

            TileHeatNetworking.SendBeamPlatformTilesPacket(
                platformTiles,
                DefaultHeatDuration,
                pendingPlatformHeatOwners[projectileIndex],
                pendingPlatformHeatDamage[projectileIndex]);
            platformTiles.Clear();
            pendingPlatformHeatKeys[projectileIndex]?.Clear();
        }

        private static bool HasAirAbove(int x, int y)
        {
            int checkY = y - 1;
            if (checkY < 0)
                return false;

            Tile tile = Main.tile[x, checkY];
            return !tile.HasTile || tile.IsActuated;
        }

        private static bool HasAirBelow(int x, int y)
        {
            int checkY = y + 1;
            if (checkY >= Main.maxTilesY)
                return false;

            Tile tile = Main.tile[x, checkY];
            return !tile.HasTile || tile.IsActuated;
        }

        private static void SpawnHeatSurfaceParticles(int x, int y, float intensity, float normalizedDist, Color heatColor)
        {
            if (heatParticlesSpawnedThisTick >= MaxHeatParticlesPerTick)
                return;

            if (!HasAirAbove(x, y))
                return;

            float heatAmount = MathHelper.Clamp(intensity * MathHelper.Lerp(1f, 0.55f, normalizedDist), 0f, 1f);
            bool isHotPhase = heatColor.R > 180 && heatColor.G > 35;
            if (!isHotPhase || heatAmount <= 0.2f)
                return;

            Vector2 surfaceBase = new Vector2(x * 16f, y * 16f);

            // Heated terrain should smolder quietly instead of filling the
            // whole beam path with particles.
            if (Main.rand.NextFloat() < 0.0125f * heatAmount)
            {
                Vector2 dustPos = surfaceBase + new Vector2(Main.rand.NextFloat(2f, 14f), Main.rand.NextFloat(-2f, 8f));
                Vector2 dustVel = new Vector2(Main.rand.NextFloat(-0.45f, 0.45f), Main.rand.NextFloat(-1.45f, -0.35f));
                Dust flame = Dust.NewDustPerfect(dustPos, ModContent.DustType<FlameDust>(), dustVel, 0, default, Main.rand.NextFloat(0.9f, 1.9f));
                flame.noGravity = true;
                heatParticlesSpawnedThisTick++;
            }

            if (heatParticlesSpawnedThisTick < MaxHeatParticlesPerTick
                && Main.rand.NextFloat() < 0.0075f * heatAmount)
            {
                Vector2 sparkPos = surfaceBase + new Vector2(Main.rand.NextFloat(1f, 15f), Main.rand.NextFloat(0f, 12f));
                Vector2 sparkVel = new Vector2(Main.rand.NextFloat(-0.8f, 0.8f), Main.rand.NextFloat(-2.4f, -0.8f));
                Dust spark = Dust.NewDustPerfect(sparkPos, ModContent.DustType<SmokeDust6>(), sparkVel, 0, default, Main.rand.NextFloat(0.8f, 1.4f));
                spark.noGravity = true;
                Lighting.AddLight(sparkPos, 0.7f, 0.24f, 0.02f);
                heatParticlesSpawnedThisTick++;
            }

            if (heatParticlesSpawnedThisTick < MaxHeatParticlesPerTick
                && Main.rand.NextFloat() < 0.0055f * heatAmount)
            {
                Vector2 smokePos = surfaceBase + new Vector2(Main.rand.NextFloat(2f, 14f), Main.rand.NextFloat(-4f, 6f));
                Vector2 smokeVel = new Vector2(Main.rand.NextFloat(-0.25f, 0.25f), Main.rand.NextFloat(-0.7f, -0.15f));
                Dust smoke = Dust.NewDustPerfect(smokePos, ModContent.DustType<SmokeDust>(), smokeVel, 0, default, Main.rand.NextFloat(0.55f, 1.25f));
                smoke.noGravity = true;
                smoke.fadeIn = 0.25f;
                heatParticlesSpawnedThisTick++;
            }
        }

        private static bool SpawnCenterOverheatEmber(Projectile center)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return false;

            int owner = center.owner;
            if (owner < 0 || owner >= Main.maxPlayers || !Main.player[owner].active)
                return false;

            int damage = Math.Max(1, center.damage / 4);
            Vector2 position = center.Center + new Vector2(
                Main.rand.NextFloat(-10f, 10f),
                center.height * 0.28f);
            Vector2 velocity = new Vector2(
                Main.rand.NextFloat(-0.55f, 0.55f),
                Main.rand.NextFloat(1.6f, 3.2f));
            int projectileIndex = Projectile.NewProjectile(
                new EntitySource_Misc("SariaRovaCenterOverheat"),
                position,
                velocity,
                ModContent.ProjectileType<RovaEmber>(),
                damage,
                0f,
                owner
            );

            if (projectileIndex < 0 || projectileIndex >= Main.maxProjectiles)
                return false;

            if (centerOverheatEmberInstances != null
                && projectileIndex < centerOverheatEmberInstances.Length)
            {
                centerOverheatEmberInstances[projectileIndex] =
                    Main.projectile[projectileIndex].ModProjectile as RovaEmber;
            }

            Main.projectile[projectileIndex].netUpdate = true;
            return true;
        }

        private static bool SpawnFallingTileEmber(int x, int y, TileHeatData data)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient
                || data.Owner < 0
                || data.Owner >= Main.maxPlayers
                || !Main.player[data.Owner].active)
            {
                return false;
            }

            int damage = Math.Max(1, data.Damage / 4);
            Vector2 position = new Vector2(x * 16f + 8f, y * 16f + 18f);
            Vector2 velocity = new Vector2(
                Main.rand.NextFloat(-0.4f, 0.4f),
                Main.rand.NextFloat(1.6f, 3.2f));
            int projectileIndex = Projectile.NewProjectile(
                new EntitySource_Misc("SariaTileHeat"),
                position,
                velocity,
                ModContent.ProjectileType<RovaEmber>(),
                damage,
                0f,
                data.Owner);

            if (projectileIndex < 0 || projectileIndex >= Main.maxProjectiles)
                return false;

            if (tileHeatEmberInstances != null && projectileIndex < tileHeatEmberInstances.Length)
                tileHeatEmberInstances[projectileIndex] = Main.projectile[projectileIndex].ModProjectile as RovaEmber;
            if (tileEmberCountsByOwner != null && data.Owner < tileEmberCountsByOwner.Length)
                tileEmberCountsByOwner[data.Owner]++;

            Main.projectile[projectileIndex].netUpdate = true;
            return true;
        }

        private static float GetHeatIntensity(TileHeatData data, int currentTick)
        {
            if (data.IsExpired(currentTick))
                return 0f;

            float progress = data.GetProgress(currentTick);
            float normalizedDist = data.NormalizedDistance;
            float fadeStartProgress = MathHelper.Lerp(0.70f, 0.30f, normalizedDist);

            if (progress < fadeStartProgress)
                return MathHelper.Lerp(1f, 0.7f, normalizedDist);

            float fadeProgress = (progress - fadeStartProgress) / (1f - fadeStartProgress);
            float baseIntensity = MathHelper.Lerp(1f, 0.7f, normalizedDist);
            return MathHelper.Lerp(baseIntensity, 0f, fadeProgress);
        }

        public static Color GetHeatColor(float progress, float normalizedDistance)
        {
            float whitePhaseEnd = MathHelper.Lerp(0.45f, 0.08f, normalizedDistance);
            float yellowPhaseEnd = MathHelper.Lerp(0.60f, 0.25f, normalizedDistance);
            float orangePhaseEnd = MathHelper.Lerp(0.80f, 0.50f, normalizedDistance);

            float distSquared = normalizedDistance * normalizedDistance;
            Color peakColor = Color.Lerp(EmberWhite, BrightYellow, distSquared);

            Color result;

            if (progress < whitePhaseEnd)
            {
                float rampUpEnd = whitePhaseEnd * 0.15f;
                if (progress < rampUpEnd)
                {
                    float t = progress / rampUpEnd;
                    t = t * t;
                    result = Color.Lerp(HotOrange, peakColor, t);
                }
                else
                {
                    result = peakColor;
                }
            }
            else if (progress < yellowPhaseEnd)
            {
                float t = (progress - whitePhaseEnd) / (yellowPhaseEnd - whitePhaseEnd);
                t = t * t;
                result = Color.Lerp(peakColor, HotOrange, t);
            }
            else if (progress < orangePhaseEnd)
            {
                float t = (progress - yellowPhaseEnd) / (orangePhaseEnd - yellowPhaseEnd);
                t = t * t;
                result = Color.Lerp(HotOrange, DeepRed, t);
            }
            else
            {
                float t = (progress - orangePhaseEnd) / (1f - orangePhaseEnd);
                t = (float)Math.Sqrt(t);
                result = Color.Lerp(DeepRed, Color.Transparent, t);
            }

            return result;
        }

        /// <summary>
        /// Check if a tile position is in the heated state and above a given intensity threshold.
        /// </summary>
        public static bool IsTileHeated(int x, int y, float minIntensity = 0.5f)
        {
            if (heatedTiles == null)
                return false;

            var key = (x, y);
            if (!heatedTiles.TryGetValue(key, out TileHeatData data))
                return false;

            int currentTick = (int)Main.GameUpdateCount;
            return GetHeatIntensity(data, currentTick) >= minIntensity;
        }

        private void ApplyHeatedTileEffects()
        {
            if (heatedTiles == null || heatedTiles.Count == 0)
                return;

            ApplyPlayerHeatEffects();

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                ApplyNPCTileHeatEffects();
            }
        }

        private static void ApplyPlayerHeatEffects()
        {
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player player = Main.player[i];
                if (player == null || !player.active || player.dead)
                    continue;

                if (Main.netMode == NetmodeID.MultiplayerClient && i != Main.myPlayer)
                    continue;

                bool isFireProtected = IsPlayerFireProtected(player);
                bool isTouchingBurningHeat = !isFireProtected
                    && HasExactHeatInArea(
                        player.Hitbox,
                        HeatCollisionOverhangPixels,
                        minIntensity: PlayerTileHeatBurnRules.DirectBurnIntensityThreshold);
                bool isNearBurningHeat = !isFireProtected
                    && (isTouchingBurningHeat
                        || HasHeatInArea(
                            player.Hitbox,
                            PlayerHeatProximityPaddingPixels,
                            PlayerTileHeatBurnRules.NearbyBurnIntensityThreshold));
                int minimumBurnDurationTicks = PlayerTileHeatBurnRules.ResolveMinimumDurationTicks(
                    isTouchingBurningHeat,
                    isNearBurningHeat);

                if (minimumBurnDurationTicks > 0)
                    RefreshPlayerTileBurn(player, minimumBurnDurationTicks);

                // Player contact damage is separate from the proximity debuff.
                // Use the same shaped-tile collision and molten overhang as the
                // NPC heat field so slopes and platforms have one hit geometry.
                if (Main.netMode != NetmodeID.MultiplayerClient
                    && isTouchingBurningHeat
                    && HasExactHeatInArea(
                        player.Hitbox,
                        HeatCollisionOverhangPixels,
                        minIntensity: HeatDamageIntensityThreshold))
                {
                    TryDamagePlayerFromHeat(player);
                }
            }
        }

        private static void RefreshPlayerTileBurn(Player player, int minimumDurationTicks)
        {
            int burningType = ModContent.BuffType<Burning2>();
            player.buffImmune[burningType] = false;
            int buffIndex = player.FindBuffIndex(burningType);
            int currentDurationTicks = buffIndex >= 0 ? player.buffTime[buffIndex] : 0;
            int refreshedDurationTicks = PlayerTileHeatBurnRules.PreserveLongerDurationTicks(
                currentDurationTicks,
                minimumDurationTicks);
            if (refreshedDurationTicks == currentDurationTicks)
                return;

            player.AddBuff(burningType, refreshedDurationTicks, quiet: false);
        }

        private static void ApplyNPCTileHeatEffects()
        {
            int updatePhase = (int)Main.GameUpdateCount & 7;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                // Spread proximity-burn refreshes across eight ticks. The
                // heat-field projectile still performs immediate hit checks.
                if ((i & 7) != updatePhase)
                    continue;

                NPC npc = Main.npc[i];
                if (npc == null || !npc.active || npc.friendly || npc.lifeMax <= 0 || npc.dontTakeDamage)
                    continue;

                if (HasHeatInArea(
                    npc.Hitbox,
                    48,
                    HeatDamageIntensityThreshold))
                {
                    npc.buffImmune[ModContent.BuffType<Burning2>()] = false;
                    npc.AddBuff(ModContent.BuffType<Burning2>(), 16);
                }
            }
        }

        private static bool TryGetHottestHeatInArea(Rectangle area, int padding, out TileHeatData hottestHeat, out float hottestIntensity)
        {
            return TryGetHottestHeatInAreaCore(
                area,
                padding,
                requiredOwner: -1,
                filterOwner: false,
                exactTileGeometry: false,
                stopAtFirstMatch: false,
                minIntensity: 0f,
                out hottestHeat,
                out hottestIntensity);
        }

        private static bool HasHeatInArea(Rectangle area, int padding, float minIntensity)
        {
            return TryGetHottestHeatInAreaCore(
                area,
                padding,
                requiredOwner: -1,
                filterOwner: false,
                exactTileGeometry: false,
                stopAtFirstMatch: true,
                minIntensity,
                out _,
                out _);
        }

        private static bool HasExactHeatInArea(Rectangle area, int padding, float minIntensity)
        {
            return TryGetHottestHeatInAreaCore(
                area,
                padding,
                requiredOwner: -1,
                filterOwner: false,
                exactTileGeometry: true,
                stopAtFirstMatch: true,
                minIntensity,
                out _,
                out _);
        }

        internal static bool TryGetHottestOwnedHeatInArea(
            Rectangle area,
            int padding,
            int owner,
            float minIntensity,
            out TileHeatData hottestHeat,
            out float hottestIntensity)
        {
            return TryGetHottestHeatInAreaCore(
                area,
                padding,
                owner,
                filterOwner: true,
                exactTileGeometry: true,
                stopAtFirstMatch: false,
                minIntensity,
                out hottestHeat,
                out hottestIntensity);
        }

        private static bool TryGetHottestHeatInAreaCore(
            Rectangle area,
            int padding,
            int requiredOwner,
            bool filterOwner,
            bool exactTileGeometry,
            bool stopAtFirstMatch,
            float minIntensity,
            out TileHeatData hottestHeat,
            out float hottestIntensity)
        {
            hottestHeat = default;
            hottestIntensity = 0f;

            if (heatedTiles == null || heatedTiles.Count == 0)
                return false;

            Rectangle expandedArea = area;
            expandedArea.Inflate(padding, padding);
            if (expandedArea.Right <= 0 || expandedArea.Bottom <= 0
                || expandedArea.Left >= Main.maxTilesX * 16
                || expandedArea.Top >= Main.maxTilesY * 16)
            {
                return false;
            }

            int minTileX = Math.Max(0, (int)Math.Floor(expandedArea.Left / 16f));
            int minTileY = Math.Max(0, (int)Math.Floor(expandedArea.Top / 16f));
            int maxTileX = Math.Min(Main.maxTilesX - 1, (int)Math.Floor((expandedArea.Right - 1) / 16f));
            int maxTileY = Math.Min(Main.maxTilesY - 1, (int)Math.Floor((expandedArea.Bottom - 1) / 16f));
            int currentTick = (int)Main.GameUpdateCount;

            for (int x = minTileX; x <= maxTileX; x++)
            {
                for (int y = minTileY; y <= maxTileY; y++)
                {
                    if (!heatedTiles.TryGetValue((x, y), out TileHeatData data)
                        || (filterOwner && (data.Owner != requiredOwner || data.Damage <= 0)))
                    {
                        continue;
                    }

                    if (exactTileGeometry && !DoesTileGeometryIntersectArea(x, y, expandedArea))
                        continue;

                    float intensity = GetHeatIntensity(data, currentTick);
                    if (intensity < minIntensity || intensity <= 0f)
                        continue;

                    if (stopAtFirstMatch)
                    {
                        hottestIntensity = intensity;
                        hottestHeat = data;
                        return true;
                    }

                    if (intensity <= hottestIntensity)
                        continue;

                    hottestIntensity = intensity;
                    hottestHeat = data;
                }
            }

            return hottestIntensity > 0f;
        }

        private static bool DoesTileGeometryIntersectArea(int tileX, int tileY, Rectangle area)
        {
            Tile tile = Main.tile[tileX, tileY];
            if (!tile.HasTile || tile.IsActuated)
                return false;

            int worldLeft = tileX * 16;
            int worldTop = tileY * 16;
            int minLocalX = Math.Max(0, area.Left - worldLeft);
            int maxLocalX = Math.Min(15, area.Right - 1 - worldLeft);
            int minLocalY = Math.Max(0, area.Top - worldTop);
            int maxLocalY = Math.Min(15, area.Bottom - 1 - worldTop);
            if (minLocalX > maxLocalX || minLocalY > maxLocalY)
                return false;

            for (int localY = minLocalY; localY <= maxLocalY; localY++)
            {
                if (TryGetGeometryRowRange(tile, localY, out int geometryLeft, out int geometryRight)
                    && geometryRight >= minLocalX
                    && geometryLeft <= maxLocalX)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsPlayerFireProtected(Player player)
        {
            return player.HasBuff(ModContent.BuffType<Veil>())
                || player.HasBuff(BuffID.ObsidianSkin)
                || player.lavaImmune
                || player.fireWalk;
        }

        private static void TryDamagePlayerFromHeat(Player player)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient || playerHeatDamageCooldowns == null)
                return;

            playerHeatDamageCooldowns.TryGetValue(player.whoAmI, out int cooldown);
            if (cooldown > 0)
                return;

            int damage = Math.Max(1, player.statLifeMax2 / 15);
            player.Hurt(PlayerDeathReason.ByCustomReason(player.name + " was scorched by Rova's fire."), damage, 0, false, false, false, -1);
            playerHeatDamageCooldowns[player.whoAmI] = HeatDamageIntervalTicks;
        }

        private static void DecayCooldowns(Dictionary<int, int> cooldowns)
        {
            if (cooldowns == null || cooldowns.Count == 0 || cooldownKeysToRemove == null)
                return;

            cooldownKeysToRemove.Clear();
            cooldownKeysToRemove.AddRange(cooldowns.Keys);
            for (int i = 0; i < cooldownKeysToRemove.Count; i++)
            {
                int key = cooldownKeysToRemove[i];
                int next = cooldowns[key] - 1;
                if (next <= 0)
                    cooldowns.Remove(key);
                else
                    cooldowns[key] = next;
            }
        }

        private static void UpdateLocalScreenHeatEffect()
        {
            if (Main.dedServ || Main.gameMenu || Main.LocalPlayer == null || !Main.LocalPlayer.active)
            {
                localScreenHeatIntensity = 0f;
                DeactivateRovaHeatDistortion();
                return;
            }

            Player player = Main.LocalPlayer;
            float targetIntensity = GetHeatedTileScreenIntensity(player);

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile projectile = Main.projectile[i];
                if (!projectile.active)
                    continue;

                if (projectile.ModProjectile is RovaBeam beam)
                {
                    float beamDistance = GetDistanceFromPointToSegment(
                        player.Center,
                        projectile.Center,
                        beam.GetBeamEndpointPosition());
                    beamDistance = Math.Max(0f, beamDistance - Math.Max(player.width, player.height) * 0.5f);
                    float beamProximity = GetLinearProximity(beamDistance, RovaBeamScreenEffectRangePixels);
                    float beamIntensity = beamProximity * beam.GetBeamEndpointAlpha();
                    targetIntensity = Math.Max(targetIntensity, beamIntensity);
                    continue;
                }

                if (projectile.ModProjectile is RovaCenter center)
                {
                    float centerHeat = center.ScreenHeatIntensity;
                    if (centerHeat <= 0f)
                        continue;

                    float centerDistance = Vector2.Distance(player.Center, projectile.Center);
                    float centerProximity = GetLinearProximity(centerDistance, RovaCenterScreenEffectRangePixels);
                    targetIntensity = Math.Max(targetIntensity, centerProximity * centerHeat);
                }
            }

            targetIntensity = MathHelper.Clamp(targetIntensity, 0f, 1f);
            float smoothing = targetIntensity > localScreenHeatIntensity
                ? ScreenHeatRiseSmoothing
                : ScreenHeatFallSmoothing;
            localScreenHeatIntensity = MathHelper.Lerp(
                localScreenHeatIntensity,
                targetIntensity,
                smoothing);
            if (Math.Abs(targetIntensity - localScreenHeatIntensity) <= ScreenHeatSnapThreshold)
                localScreenHeatIntensity = targetIntensity;

            UpdateRovaHeatDistortion(player);
        }

        private static float GetHeatedTileScreenIntensity(Player player)
        {
            if (heatedTiles == null || heatedTiles.Count == 0)
                return 0f;

            Rectangle playerArea = player.Hitbox;
            Rectangle searchArea = playerArea;
            searchArea.Inflate(HeatedTileScreenEffectRangePixels, HeatedTileScreenEffectRangePixels);

            int minTileX = Math.Max(0, (int)Math.Floor(searchArea.Left / 16f));
            int minTileY = Math.Max(0, (int)Math.Floor(searchArea.Top / 16f));
            int maxTileX = Math.Min(Main.maxTilesX - 1, (int)Math.Floor((searchArea.Right - 1) / 16f));
            int maxTileY = Math.Min(Main.maxTilesY - 1, (int)Math.Floor((searchArea.Bottom - 1) / 16f));
            int currentTick = (int)Main.GameUpdateCount;
            float strongestIntensity = 0f;

            for (int x = minTileX; x <= maxTileX; x++)
            {
                for (int y = minTileY; y <= maxTileY; y++)
                {
                    if (!heatedTiles.TryGetValue((x, y), out TileHeatData heat))
                        continue;

                    float heatIntensity = GetHeatIntensity(heat, currentTick);
                    if (heatIntensity <= 0f)
                        continue;

                    Rectangle tileArea = new Rectangle(x * 16, y * 16, 16, 16);
                    float distance = GetDistanceBetweenRectangles(playerArea, tileArea);
                    float proximity = GetLinearProximity(distance, HeatedTileScreenEffectRangePixels);
                    strongestIntensity = Math.Max(strongestIntensity, heatIntensity * proximity);
                }
            }

            return strongestIntensity;
        }

        private static float GetDistanceBetweenRectangles(Rectangle first, Rectangle second)
        {
            float horizontalDistance = first.Right < second.Left
                ? second.Left - first.Right
                : second.Right < first.Left
                    ? first.Left - second.Right
                    : 0f;
            float verticalDistance = first.Bottom < second.Top
                ? second.Top - first.Bottom
                : second.Bottom < first.Top
                    ? first.Top - second.Bottom
                    : 0f;

            return (float)Math.Sqrt(
                horizontalDistance * horizontalDistance
                + verticalDistance * verticalDistance);
        }

        private static float GetDistanceFromPointToSegment(Vector2 point, Vector2 start, Vector2 end)
        {
            Vector2 segment = end - start;
            float lengthSquared = segment.LengthSquared();
            if (lengthSquared <= 0.0001f)
                return Vector2.Distance(point, start);

            float amount = Vector2.Dot(point - start, segment) / lengthSquared;
            amount = MathHelper.Clamp(amount, 0f, 1f);
            return Vector2.Distance(point, start + segment * amount);
        }

        private static float GetLinearProximity(float distance, float range)
        {
            if (range <= 0f || distance >= range)
                return 0f;

            return 1f - Math.Max(0f, distance) / range;
        }

        private static void UpdateRovaHeatDistortion(Player player)
        {
            if (!heatDistortionFilterRegistered)
                return;

            Filter heatFilter = Filters.Scene[RovaHeatDistortionFilterKey];
            bool shouldBeActive = Main.UseHeatDistortion && localScreenHeatIntensity > 0.005f;
            if (!shouldBeActive)
            {
                if (heatFilter.IsActive())
                    Filters.Scene.Deactivate(RovaHeatDistortionFilterKey);
                return;
            }

            if (!heatFilter.IsActive())
                Filters.Scene.Activate(RovaHeatDistortionFilterKey, player.Center);

            heatFilter.IsHidden = false;
            heatFilter.GetShader()
                .UseIntensity(localScreenHeatIntensity * MaximumHeatDistortionIntensity)
                .UseOpacity(localScreenHeatIntensity);
        }

        private static void DeactivateRovaHeatDistortion()
        {
            if (Main.dedServ || !heatDistortionFilterRegistered)
                return;

            Filter heatFilter = Filters.Scene[RovaHeatDistortionFilterKey];
            if (heatFilter.IsActive())
                Filters.Scene.Deactivate(RovaHeatDistortionFilterKey);
        }

        public override void PostDrawInterface(SpriteBatch spriteBatch)
        {
            if (Main.dedServ || Main.gameMenu || Main.LocalPlayer == null || !Main.LocalPlayer.active)
                return;

            if (localScreenHeatIntensity <= 0f)
                return;

            float alpha = MathHelper.Clamp(
                localScreenHeatIntensity * MaximumScreenTintAlpha,
                0f,
                MaximumScreenTintAlpha);
            if (alpha <= 0f)
                return;

            Texture2D pixel = TextureAssets.MagicPixel.Value;
            spriteBatch.Draw(pixel, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), new Color(180, 20, 0) * alpha);
        }

        public static int GetHeatedTileCount()
        {
            return heatedTiles?.Count ?? 0;
        }

        public static void ClearAllHeat()
        {
            if (heatFieldProjectileIndices != null)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    int heatFieldType = ModContent.ProjectileType<RovaHeatField>();
                    for (int i = 0; i < heatFieldProjectileIndices.Length; i++)
                    {
                        int projectileIndex = heatFieldProjectileIndices[i];
                        if (projectileIndex >= 0
                            && projectileIndex < Main.maxProjectiles
                            && Main.projectile[projectileIndex].active
                            && Main.projectile[projectileIndex].type == heatFieldType)
                        {
                            Main.projectile[projectileIndex].Kill();
                        }
                    }
                }

                Array.Fill(heatFieldProjectileIndices, -1);
            }

            if (heatFieldOwnerNeeded != null)
                Array.Clear(heatFieldOwnerNeeded, 0, heatFieldOwnerNeeded.Length);
            if (heatFieldBoundsByOwner != null)
                Array.Clear(heatFieldBoundsByOwner, 0, heatFieldBoundsByOwner.Length);
            if (heatFieldDamageByOwner != null)
                Array.Clear(heatFieldDamageByOwner, 0, heatFieldDamageByOwner.Length);
            if (centerOverheatStates != null)
                Array.Clear(centerOverheatStates, 0, centerOverheatStates.Length);
            if (centerObservedThisTick != null)
                Array.Clear(centerObservedThisTick, 0, centerObservedThisTick.Length);
            if (centerHasActiveBeam != null)
                Array.Clear(centerHasActiveBeam, 0, centerHasActiveBeam.Length);
            if (centerOverheatEmberInstances != null)
                Array.Clear(centerOverheatEmberInstances, 0, centerOverheatEmberInstances.Length);
            if (tileHeatEmberInstances != null)
                Array.Clear(tileHeatEmberInstances, 0, tileHeatEmberInstances.Length);
            if (centerEmberCountsByOwner != null)
                Array.Clear(centerEmberCountsByOwner, 0, centerEmberCountsByOwner.Length);
            if (tileEmberCountsByOwner != null)
                Array.Clear(tileEmberCountsByOwner, 0, tileEmberCountsByOwner.Length);
            if (trackedPlatformHeatBeams != null)
                Array.Clear(trackedPlatformHeatBeams, 0, trackedPlatformHeatBeams.Length);
            if (pendingPlatformHeatTiles != null)
            {
                for (int i = 0; i < pendingPlatformHeatTiles.Length; i++)
                    pendingPlatformHeatTiles[i]?.Clear();
            }
            if (pendingPlatformHeatKeys != null)
            {
                for (int i = 0; i < pendingPlatformHeatKeys.Length; i++)
                    pendingPlatformHeatKeys[i]?.Clear();
            }
            if (pendingPlatformHeatOwners != null)
                Array.Clear(pendingPlatformHeatOwners, 0, pendingPlatformHeatOwners.Length);
            if (pendingPlatformHeatDamage != null)
                Array.Clear(pendingPlatformHeatDamage, 0, pendingPlatformHeatDamage.Length);
            platformHeatSpreadSamples?.Clear();
            centerSlotsByHandle?.Clear();

            heatedTiles?.Clear();
            playerHeatDamageCooldowns?.Clear();
            visualHeatStates?.Clear();
            visualStatesToRemove?.Clear();
            visualStateKeys?.Clear();
            visibleHeatEntries?.Clear();
            visibleHeatSpillEntries?.Clear();
            visibleHeatSpillIndices?.Clear();
            buriedDepthCache?.Clear();
            heatParticlesSpawnedThisTick = 0;
            lastFallingEmberSpawnTick = -1000000;
        }
    }
}
