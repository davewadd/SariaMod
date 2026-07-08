using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using SariaMod.Buffs;
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
using SariaMod.Items.Strange;
using Terraria.Localization;
using System;
using Terraria.Map;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Events;
using Terraria.ID;
using Terraria.ModLoader;
using System.Linq;
using SariaMod.Diagnostics;
using SariaMod.Netcode.SariaSoundSync;
using ReLogic.Utilities;
namespace SariaMod.Items.Strange
{
    public partial class Saria
    {
        public bool SariaZoneSnow;
        public bool SariaZoneJungle;
        public bool SariaZoneCorrupt;
        public bool SariaZoneCrimson;
        public bool SariaZoneHallow;
        public bool SariaZoneDesert;
        public bool SariaZoneBeach;        // near world edge + surface + enough sand tiles
        public bool SariaZoneDungeon;       // enough dungeon brick tiles nearby
        public bool SariaZoneSandstorm;     // sandstorm event is active
        public bool SariaZoneUndergroundDesert; // desert tiles at underground depth
        public bool SariaZoneGlowingMushroom;
        public bool SariaZoneGraveyard;
        public bool SariaZoneMeteor;
        public bool SariaZoneForest;        // true when no other dominant biome is active
        // Depth layers (Y-position based, mirrors player Zone depth bands)
        public bool SariaZoneSkyHeight;
        public bool SariaZoneSpace;         // above sky — mirrors player.InSpace()
        public bool SariaZoneOverworld;     // surface layer
        public bool SariaZoneUnderground;   // below surface, above dirt layer
        public bool SariaZoneDirtLayer;     // dirt layer depth band
        public bool SariaZoneRockLayer;     // rock layer depth band
        public bool SariaZoneUnderworld;    // underworld / hell depth
        public bool SariaZoneRain;
        // Environment / nearby objects
        public bool SariaHasCampfire;
        public bool SariaHasHeartLantern;
        public bool SariaHasStarInBottle;
        public bool SariaHasWaterCandle;   // net positive water candles nearby
        public bool SariaHasPeaceCandle;   // net positive peace candles nearby
        public bool SariaHasCalmMindCandle;  // Calming Candle tile nearby
        public bool SariaHasReajCandle;      // Reaj Candle tile nearby

        private Vector2 _lastBiomeScanPos;
        private const float BiomeScanMoveThreshold = 16f; // 1 tile in world units

        // Modded biomes active at Saria's location.
        // Populated each scan by temporarily relocating the owner to Saria's center and
        // calling ModBiome.IsBiomeActive(player) for every registered ModBiome.
        // Read by SariaSpawnSystem to make the Saria NPC-spawn pass biome-correct.
        private readonly HashSet<int> _sariaActiveModBiomeTypes = new();
        public IReadOnlyCollection<int> SariaActiveModBiomeTypes => _sariaActiveModBiomeTypes;

        // Bitmask layout for Send/ReceiveExtraAI
        // ushort 1: tile-count biomes
        private const ushort ZoneBit_Snow           = 1 << 0;
        private const ushort ZoneBit_Jungle         = 1 << 1;
        private const ushort ZoneBit_Corrupt        = 1 << 2;
        private const ushort ZoneBit_Crimson        = 1 << 3;
        private const ushort ZoneBit_Hallow         = 1 << 4;
        private const ushort ZoneBit_Desert         = 1 << 5;
        private const ushort ZoneBit_GlowingMushroom= 1 << 6;
        private const ushort ZoneBit_Graveyard      = 1 << 7;
        private const ushort ZoneBit_Meteor         = 1 << 8;
        private const ushort ZoneBit_Forest         = 1 << 9;
        private const ushort ZoneBit_Rain           = 1 << 10;
        private const ushort ZoneBit_Beach          = 1 << 11;
        private const ushort ZoneBit_Dungeon        = 1 << 12;
        private const ushort ZoneBit_Sandstorm      = 1 << 13;
        private const ushort ZoneBit_UndergroundDesert = 1 << 14;
        // ushort 2: depth layers + environment
        private const ushort DepthBit_SkyHeight     = 1 << 0;
        private const ushort DepthBit_Space         = 1 << 1;
        private const ushort DepthBit_Overworld     = 1 << 2;
        private const ushort DepthBit_Underground   = 1 << 3;
        private const ushort DepthBit_DirtLayer     = 1 << 4;
        private const ushort DepthBit_RockLayer     = 1 << 5;
        private const ushort DepthBit_Underworld    = 1 << 6;
        private const ushort EnvBit_Campfire        = 1 << 7;
        private const ushort EnvBit_HeartLantern    = 1 << 8;
        private const ushort EnvBit_StarInBottle    = 1 << 9;
        private const ushort EnvBit_WaterCandle     = 1 << 10;
        private const ushort EnvBit_PeaceCandle     = 1 << 11;
        private const ushort EnvBit_CalmMindCandle  = 1 << 12;
        private const ushort EnvBit_ReajCandle      = 1 << 13;

        /// </summary>
        private void UpdateSariaZones()
        {
            if (Main.myPlayer != Projectile.owner)
                return;

            // Only re-scan when Saria has moved at least one tile from the last scan position.
            // When she is stationary the zone flags from the previous scan are preserved as-is,
            // matching the "last biome she landed on" behaviour the user requested.
            if (Vector2.DistanceSquared(Projectile.Center, _lastBiomeScanPos) < BiomeScanMoveThreshold * BiomeScanMoveThreshold)
                return;

            _lastBiomeScanPos = Projectile.Center;

            Point tilePos = new Point((int)(Projectile.Center.X / 16f), (int)(Projectile.Center.Y / 16f));
            SceneMetrics metrics = new SceneMetrics();
            SceneMetricsScanSettings settings = new SceneMetricsScanSettings
            {
                BiomeScanCenterPositionInWorld = Projectile.Center,
                ScanOreFinderData = false,
            };
            metrics.ScanAndExportToMain(settings);

            SariaZoneSnow            = metrics.EnoughTilesForSnow;
            SariaZoneJungle          = metrics.EnoughTilesForJungle;
            SariaZoneCorrupt         = metrics.EnoughTilesForCorruption;
            SariaZoneCrimson         = metrics.EnoughTilesForCrimson;
            SariaZoneHallow          = metrics.EnoughTilesForHallow;
            SariaZoneDesert          = metrics.EnoughTilesForDesert;
            // Beach: world-edge proximity + surface depth + at least 300 nearby sand tiles.
            // SceneMetrics deliberately excludes beach sand from SandTileCount (to avoid triggering
            // the desert biome), so we scan the tiles manually within a 60-tile radius instead.
            int beachMargin = (int)(Main.maxTilesX * 0.0905);
            bool nearEdge = (tilePos.X < beachMargin || tilePos.X > Main.maxTilesX - beachMargin)
                            && tilePos.Y < Main.worldSurface;
            int manualSandCount = 0;
            if (nearEdge)
            {
                const int BeachScanRadius = 60;
                int x0 = Math.Max(0, tilePos.X - BeachScanRadius);
                int x1 = Math.Min(Main.maxTilesX - 1, tilePos.X + BeachScanRadius);
                int y0 = Math.Max(0, tilePos.Y - BeachScanRadius);
                int y1 = Math.Min(Main.maxTilesY - 1, tilePos.Y + BeachScanRadius);
                for (int bx = x0; bx <= x1; bx++)
                    for (int by = y0; by <= y1; by++)
                    {
                        Tile t = Main.tile[bx, by];
                        if (t != null && t.HasTile && TileID.Sets.isDesertBiomeSand[t.TileType])
                            manualSandCount++;
                    }
            }
            SariaZoneBeach = nearEdge && manualSandCount >= 300;
            int sariaWall = Main.tile[tilePos.X, tilePos.Y].WallType;
            bool hasDungeonWall = sariaWall == WallID.BlueDungeonUnsafe
                                  || sariaWall == WallID.GreenDungeonUnsafe
                                  || sariaWall == WallID.PinkDungeonUnsafe;
            SariaZoneDungeon         = metrics.DungeonTileCount >= 250 && hasDungeonWall;
            SariaZoneSandstorm       = Terraria.GameContent.Events.Sandstorm.Happening && SariaZoneDesert;
            SariaZoneUndergroundDesert = metrics.EnoughTilesForDesert
                                       && (SariaZoneUnderground || SariaZoneDirtLayer || SariaZoneRockLayer);
            SariaZoneGlowingMushroom = metrics.EnoughTilesForGlowingMushroom;
            SariaZoneGraveyard       = metrics.EnoughTilesForGraveyard;
            SariaZoneMeteor          = metrics.EnoughTilesForMeteor;
            SariaZoneForest          = !SariaZoneJungle && !SariaZoneSnow && !SariaZoneDesert
                                       && !SariaZoneCorrupt && !SariaZoneCrimson && !SariaZoneHallow
                                       && !SariaZoneGlowingMushroom && !SariaZoneGraveyard;
            // Depth layers — matching vanilla player zone thresholds exactly.
            // worldSurface, rockLayer, UnderworldLayer are already in tile coordinates.
            // InSpace()   : tileY < worldSurface * 0.3
            // ZoneSkyHeight: tileY < worldSurface * 0.6  (overlaps Space; Space takes priority)
            // ZoneOverworldHeight: tileY >= worldSurface * 0.6 && < worldSurface
            // ZoneDirtLayerHeight: tileY >= worldSurface && < rockLayer
            // ZoneRockLayerHeight: tileY >= rockLayer && < UnderworldLayer
            // ZoneUnderworldHeight: tileY >= UnderworldLayer
            int y = tilePos.Y;
            float spaceX = (float)Main.maxTilesX / 4200f;
            spaceX *= spaceX;
            SariaZoneSpace = (float)((y - (50f + 10f * spaceX)) / (Main.worldSurface / 5.0)) < 1f;
            SariaZoneSkyHeight   = y < Main.worldSurface * 0.50 && !SariaZoneSpace;
            SariaZoneOverworld   = y >= Main.worldSurface * 0.5 && y < Main.worldSurface;
            SariaZoneUnderground = y >= Main.worldSurface;
            SariaZoneDirtLayer   = y >= Main.worldSurface       && y < Main.rockLayer;
            SariaZoneRockLayer   = y >= Main.rockLayer          && y < Main.UnderworldLayer;
            SariaZoneUnderworld  = y >= Main.UnderworldLayer;
            SariaZoneRain        = Main.raining && !SariaZoneSpace
                                   && !SariaZoneUnderground
                                   && !SariaZoneDirtLayer
                                   && !SariaZoneRockLayer
                                   && !SariaZoneUnderworld;
            // Environment
            SariaHasCampfire         = metrics.HasCampfire;
            SariaHasHeartLantern     = metrics.HasHeartLantern;
            SariaHasStarInBottle     = metrics.HasStarInBottle;
            SariaHasWaterCandle      = metrics.WaterCandleCount > 0;
            SariaHasPeaceCandle      = metrics.PeaceCandleCount > 0;
            RefreshCandleEnvironment();

            // --- Modded biome detection via temporary player relocation ---
            // Briefly move the owner to Saria's center (same trick used by SariaSpawnSystem for
            // NPC spawning) so that ModBiome.IsBiomeActive(player) evaluates Saria's tile environment.
            // SceneMetrics was already scanned at Saria's position above, so tile-count based
            // mod biomes will see the correct data.  Vanilla zone flags on the player are also
            // patched from those results so biomes that cross-check player.ZoneJungle etc. work too.
            Player owner = Main.player[Projectile.owner];
            Vector2 savedPos      = owner.position;
            bool savedJungle      = owner.ZoneJungle;
            bool savedSnow        = owner.ZoneSnow;
            bool savedCrimson     = owner.ZoneCrimson;
            bool savedCorrupt     = owner.ZoneCorrupt;
            bool savedHallow      = owner.ZoneHallow;
            bool savedDesert      = owner.ZoneDesert;
            bool savedMushroom    = owner.ZoneGlowshroom;
            bool savedGraveyard   = owner.ZoneGraveyard;
            bool savedMeteor      = owner.ZoneMeteor;
            bool savedBeach       = owner.ZoneBeach;
            bool savedDungeon     = owner.ZoneDungeon;
            bool savedSky         = owner.ZoneSkyHeight;
            bool savedOverworld   = owner.ZoneOverworldHeight;
            bool savedDirt        = owner.ZoneDirtLayerHeight;
            bool savedRock        = owner.ZoneRockLayerHeight;
            bool savedUnderworld  = owner.ZoneUnderworldHeight;

            try
            {
                // Relocate owner to Saria's center (no velocity change needed for a pure logic query).
                owner.position = Projectile.Center - new Vector2(owner.width * 0.5f, owner.height * 0.5f);

                // Apply Saria's vanilla zone flags so mod biomes that read player.ZoneJungle etc.
                // see the right area rather than the player's actual location.
                owner.ZoneJungle         = SariaZoneJungle;
                owner.ZoneSnow           = SariaZoneSnow;
                owner.ZoneCrimson        = SariaZoneCrimson;
                owner.ZoneCorrupt        = SariaZoneCorrupt;
                owner.ZoneHallow         = SariaZoneHallow;
                owner.ZoneDesert         = SariaZoneDesert;
                owner.ZoneGlowshroom      = SariaZoneGlowingMushroom;
                owner.ZoneGraveyard      = SariaZoneGraveyard;
                owner.ZoneMeteor         = SariaZoneMeteor;
                owner.ZoneBeach          = SariaZoneBeach;
                owner.ZoneDungeon        = SariaZoneDungeon;
                owner.ZoneSkyHeight      = SariaZoneSkyHeight;
                owner.ZoneOverworldHeight  = SariaZoneOverworld;
                owner.ZoneDirtLayerHeight  = SariaZoneDirtLayer;
                owner.ZoneRockLayerHeight  = SariaZoneRockLayer;
                owner.ZoneUnderworldHeight = SariaZoneUnderworld;

                _sariaActiveModBiomeTypes.Clear();
                foreach (ModBiome biome in ModContent.GetContent<ModBiome>())
                {
                    try
                    {
                        if (biome.IsBiomeActive(owner))
                            _sariaActiveModBiomeTypes.Add(biome.Type);
                    }
                    catch { /* defensive: guard against misbehaving mod biomes */ }
                }
            }
            finally
            {
                // Always restore the owner's real state before any rendering or net code runs.
                owner.position            = savedPos;
                owner.ZoneJungle          = savedJungle;
                owner.ZoneSnow            = savedSnow;
                owner.ZoneCrimson         = savedCrimson;
                owner.ZoneCorrupt         = savedCorrupt;
                owner.ZoneHallow          = savedHallow;
                owner.ZoneDesert          = savedDesert;
                owner.ZoneGlowshroom      = savedMushroom;
                owner.ZoneGraveyard       = savedGraveyard;
                owner.ZoneMeteor          = savedMeteor;
                owner.ZoneBeach           = savedBeach;
                owner.ZoneDungeon         = savedDungeon;
                owner.ZoneSkyHeight         = savedSky;
                owner.ZoneOverworldHeight   = savedOverworld;
                owner.ZoneDirtLayerHeight   = savedDirt;
                owner.ZoneRockLayerHeight   = savedRock;
                owner.ZoneUnderworldHeight  = savedUnderworld;
            }

            Projectile.netUpdate = true;
        }

        /// <summary>
        /// Rescans Saria's candle environment LIVE — no movement gating, no owner gating.
        /// The half-extents match the vanilla buff-scan area a real player standing at her
        /// position would get (falling back to the old 30-tile radius if the screen size
        /// is unavailable, e.g. dedicated servers), so the debug panel's detection range
        /// and the spawn system's application range always agree.

        /// </summary>
        public void RefreshCandleEnvironment()
        {
            int halfW = Math.Max(30, Main.buffScanAreaWidth / 2);
            int halfH = Math.Max(30, Main.buffScanAreaHeight / 2);
            int centerX = (int)(Projectile.Center.X / 16f);
            int centerY = (int)(Projectile.Center.Y / 16f);
            int calmType = ModContent.TileType<Tiles.CalmingCandleTile>();
            int reajType = ModContent.TileType<Tiles.ReajCandleTile>();
            bool calm = false, reaj = false;
            for (int x = centerX - halfW; x <= centerX + halfW && !(calm && reaj); x++)
            {
                for (int y = centerY - halfH; y <= centerY + halfH; y++)
                {
                    if (!WorldGen.InWorld(x, y))
                        continue;
                    Tile t = Main.tile[x, y];
                    if (t == null || !t.HasTile)
                        continue;
                    if (t.TileType == calmType) calm = true;
                    else if (t.TileType == reajType) reaj = true;
                    if (calm && reaj)
                        break;
                }
            }
            SariaHasCalmMindCandle = calm;
            SariaHasReajCandle = reaj;
        }


        /// <summary>
        /// Scans tiles within a square radius around Saria for a given tile type.
        /// Only valid on the owner's client — always returns false on non-owners.
        /// </summary>
        public bool ScanTilesInRadius(int tileType, int radius)
        {
            if (Main.myPlayer != Projectile.owner)
                return false;

            int centerX = (int)(Projectile.Center.X / 16f);
            int centerY = (int)(Projectile.Center.Y / 16f);

            for (int x = centerX - radius; x <= centerX + radius; x++)
            {
                for (int y = centerY - radius; y <= centerY + radius; y++)
                {
                    if (WorldGen.InWorld(x, y) && Main.tile[x, y].HasTile && Main.tile[x, y].TileType == tileType)
                        return true;
                }
            }
            return false;
        }
    }
}
