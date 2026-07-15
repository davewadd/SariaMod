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
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.TileGlow
{
    public class TileHeatManager : ModSystem
    {
        private static Dictionary<(int x, int y), TileHeatData> heatedTiles = new Dictionary<(int x, int y), TileHeatData>();
        private static List<(int x, int y)> tilesToRemove = new List<(int x, int y)>();
        private static Dictionary<int, int> playerHeatDamageCooldowns = new Dictionary<int, int>();
        private static Dictionary<int, int> npcHeatDamageCooldowns = new Dictionary<int, int>();
        private static List<int> cooldownKeysToRemove = new List<int>();

        public const int DefaultHeatDuration = 1200;

        private static readonly Color BrightYellow = new Color(255, 255, 100);
        private static readonly Color HotOrange = new Color(255, 140, 30);
        private static readonly Color DeepRed = new Color(180, 30, 10);
        private static readonly Color DarkRed = new Color(100, 10, 0);
        private static readonly Color EmberWhite = new Color(255, 220, 160);

        public static TileHeatManager Instance { get; private set; }

        public override void Load()
        {
            Instance = this;
        }

        public override void Unload()
        {
            heatedTiles?.Clear();
            heatedTiles = null;
            tilesToRemove?.Clear();
            tilesToRemove = null;
            playerHeatDamageCooldowns?.Clear();
            playerHeatDamageCooldowns = null;
            npcHeatDamageCooldowns?.Clear();
            npcHeatDamageCooldowns = null;
            cooldownKeysToRemove?.Clear();
            cooldownKeysToRemove = null;
            Instance = null;
        }

        public override void OnWorldLoad()
        {
            ClearAllHeat();
        }

        public override void OnWorldUnload()
        {
            ClearAllHeat();
        }

        public override void PreUpdatePlayers()
        {
            DecayCooldowns(playerHeatDamageCooldowns);
            DecayCooldowns(npcHeatDamageCooldowns);
            UpdateHeatedTiles();
            ApplyHeatedTileEffects();
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
                    if (!adjacentTile.HasTile || adjacentTile.IsActuated)
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
                    if (adjacentTile.HasTile && !adjacentTile.IsActuated && IsTileExposedToAir(checkX, checkY))
                        return 1;
                }
            }

            return 2;
        }

        public override void PostDrawTiles()
        {
            if (Main.dedServ)
                return;

            if (heatedTiles == null || heatedTiles.Count == 0)
                return;

            int currentTick = (int)Main.GameUpdateCount;

            Main.spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.Additive,
                SamplerState.PointClamp,
                DepthStencilState.None,
                RasterizerState.CullNone,
                null,
                Main.GameViewMatrix.ZoomMatrix
            );

            foreach (var kvp in heatedTiles)
            {
                var data = kvp.Value;

                if (data.IsExpired(currentTick))
                    continue;

                int x = kvp.Key.x;
                int y = kvp.Key.y;

                Vector2 screenPos = new Vector2(x * 16f, y * 16f) - Main.screenPosition;
                if (screenPos.X < -16 || screenPos.X > Main.screenWidth + 16 ||
                    screenPos.Y < -16 || screenPos.Y > Main.screenHeight + 16)
                    continue;

                Tile tile = Main.tile[x, y];
                if (!tile.HasTile)
                    continue;

                float progress = data.GetProgress(currentTick);
                float normalizedDist = data.NormalizedDistance;
                int buriedDepth = GetBuriedDepth(x, y);

                float fadeStartProgress = MathHelper.Lerp(0.70f, 0.30f, normalizedDist);

                float intensity;
                if (progress < fadeStartProgress)
                {
                    intensity = MathHelper.Lerp(1f, 0.7f, normalizedDist);
                }
                else
                {
                    float fadeProgress = (progress - fadeStartProgress) / (1f - fadeStartProgress);
                    float baseIntensity = MathHelper.Lerp(1f, 0.7f, normalizedDist);
                    intensity = MathHelper.Lerp(baseIntensity, 0f, fadeProgress);
                }

                Color heatColor;
                if (buriedDepth >= 2)
                {
                    heatColor = DarkRed;
                }
                else if (buriedDepth == 1)
                {
                    Color normalColor = GetHeatColor(progress, normalizedDist);
                    heatColor = Color.Lerp(normalColor, DarkRed, 0.6f);
                }
                else
                {
                    heatColor = GetHeatColor(progress, normalizedDist);
                }

                float alpha = MathHelper.Clamp(intensity * MathHelper.Lerp(1.25f, 0.75f, normalizedDist), 0f, 1f);
                Color drawColor = heatColor * alpha;

                DrawTileHeat(tile, x, y, screenPos, drawColor);
                DrawMoltenBloom(x, y, screenPos, intensity, normalizedDist, buriedDepth);
            }

            Main.spriteBatch.End();
        }

        private void DrawMoltenBloom(int tileX, int tileY, Vector2 screenPos, float intensity, float normalizedDist, int buriedDepth)
        {
            if (buriedDepth >= 2 || intensity <= 0.25f)
                return;

            Texture2D pixel = TextureAssets.MagicPixel.Value;
            int drawX = (int)screenPos.X;
            int drawY = (int)screenPos.Y;
            float exposedMultiplier = buriedDepth == 0 ? 1f : 0.45f;
            float hotAmount = MathHelper.Clamp((intensity - 0.25f) / 0.75f, 0f, 1f);
            float distanceFalloff = MathHelper.Lerp(1f, 0.45f, normalizedDist);
            float pulse = 0.65f + (float)Math.Sin(Main.GameUpdateCount * 0.12f + tileX * 0.73f + tileY * 0.31f) * 0.35f;

            Color outer = Color.Lerp(DeepRed, HotOrange, distanceFalloff) * (0.18f + hotAmount * 0.22f) * exposedMultiplier;
            Main.spriteBatch.Draw(pixel, new Rectangle(drawX, drawY, 16, 16), outer);

            if (hotAmount <= 0.35f)
                return;

            Color core = Color.Lerp(HotOrange, BrightYellow, hotAmount) * (0.14f + pulse * 0.24f) * distanceFalloff * exposedMultiplier;
            Main.spriteBatch.Draw(pixel, new Rectangle(drawX + 3, drawY + 3, 10, 10), core);

            if (normalizedDist < 0.45f)
            {
                Color whiteCore = EmberWhite * (0.08f + pulse * 0.12f) * hotAmount * exposedMultiplier;
                Main.spriteBatch.Draw(pixel, new Rectangle(drawX + 6, drawY + 6, 4, 4), whiteCore);
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
                    if (!tile.HasTile) continue;
                    if (tile.LiquidAmount > 0) continue;
                    if (tile.IsActuated) continue;

                    float tileWorldX = x * 16f + 8f;
                    float tileWorldY = y * 16f + 8f;
                    float distance = Vector2.Distance(worldCenter, new Vector2(tileWorldX, tileWorldY));

                    if (distance > radius) continue;

                    var key = (x, y);

                    if (heatedTiles.TryGetValue(key, out TileHeatData existing))
                    {
                        if (distance < existing.DistanceFromCenter
                            || damage > existing.Damage
                            || (owner >= 0 && owner == existing.Owner))
                        {
                            heatedTiles[key] = new TileHeatData(currentTick, duration, distance, radius, owner, damage);
                        }
                    }
                    else
                    {
                        heatedTiles[key] = new TileHeatData(currentTick, duration, distance, radius, owner, damage);
                    }
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
                    if (!neighbor.HasTile || !Main.tileSolid[neighbor.TileType])
                        continue;

                    float horizontal = Math.Abs(dx);
                    float vertical = Math.Abs(dy) * 1.6f;
                    float distance = horizontal + vertical;
                    float maxRadius = horizontal <= 3f ? 6f : 9f;
                    ApplyHeatToTile(nx, ny, distance, maxRadius, duration, owner, damage);
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
            if (!tile.HasTile || tile.LiquidAmount > 0 || tile.IsActuated)
                return;

            int currentTick = (int)Main.GameUpdateCount;
            var key = (x, y);

            if (heatedTiles.TryGetValue(key, out TileHeatData existing))
            {
                if (distanceFromCenter < existing.DistanceFromCenter
                    || damage > existing.Damage
                    || (owner >= 0 && owner == existing.Owner))
                {
                    heatedTiles[key] = new TileHeatData(currentTick, duration, distanceFromCenter, maxRadius, owner, damage);
                }
            }
            else
            {
                heatedTiles[key] = new TileHeatData(currentTick, duration, distanceFromCenter, maxRadius, owner, damage);
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

                float progress = data.GetProgress(currentTick);
                float normalizedDist = data.NormalizedDistance;
                int buriedDepth = GetBuriedDepth(x, y);

                float fadeStartProgress = MathHelper.Lerp(0.70f, 0.30f, normalizedDist);

                float intensity;
                if (progress < fadeStartProgress)
                {
                    intensity = MathHelper.Lerp(1f, 0.7f, normalizedDist);
                }
                else
                {
                    float fadeProgress = (progress - fadeStartProgress) / (1f - fadeStartProgress);
                    float baseIntensity = MathHelper.Lerp(1f, 0.7f, normalizedDist);
                    intensity = MathHelper.Lerp(baseIntensity, 0f, fadeProgress);
                }

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

                    float redAmount = (heatColor.R / 255f) - ((heatColor.G + heatColor.B) / 510f);
                    lightMultiplier = MathHelper.Lerp(0.7f, 2.8f, Math.Max(0, redAmount));
                }

                if (allowVisuals)
                {
                    Vector3 lightColor = heatColor.ToVector3() * intensity * lightMultiplier;
                    Lighting.AddLight(new Vector2(x * 16f + 8f, y * 16f + 8f), lightColor);
                }

                // Spawn flame dust on exposed tiles during the hot phase
                if (allowVisuals && buriedDepth == 0 && intensity > 0.3f)
                {
                    SpawnHeatSurfaceParticles(x, y, intensity, normalizedDist, heatColor);
                }

                if (intensity > 0.5f && HasAirBelow(x, y) && data.Owner >= 0 && data.Owner < Main.maxPlayers && Main.rand.NextFloat() < 0.0045f * intensity)
                {
                    SpawnFallingEmber(x, y, data);
                }
            }

            foreach (var key in tilesToRemove)
            {
                heatedTiles.Remove(key);
            }
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
            }

            if (Main.rand.NextFloat() < 0.0075f * heatAmount)
            {
                Vector2 sparkPos = surfaceBase + new Vector2(Main.rand.NextFloat(1f, 15f), Main.rand.NextFloat(0f, 12f));
                Vector2 sparkVel = new Vector2(Main.rand.NextFloat(-0.8f, 0.8f), Main.rand.NextFloat(-2.4f, -0.8f));
                Dust spark = Dust.NewDustPerfect(sparkPos, ModContent.DustType<SmokeDust6>(), sparkVel, 0, default, Main.rand.NextFloat(0.8f, 1.4f));
                spark.noGravity = true;
                Lighting.AddLight(sparkPos, 0.7f, 0.24f, 0.02f);
            }

            if (Main.rand.NextFloat() < 0.0055f * heatAmount)
            {
                Vector2 smokePos = surfaceBase + new Vector2(Main.rand.NextFloat(2f, 14f), Main.rand.NextFloat(-4f, 6f));
                Vector2 smokeVel = new Vector2(Main.rand.NextFloat(-0.25f, 0.25f), Main.rand.NextFloat(-0.7f, -0.15f));
                Dust smoke = Dust.NewDustPerfect(smokePos, ModContent.DustType<SmokeDust>(), smokeVel, 0, default, Main.rand.NextFloat(0.55f, 1.25f));
                smoke.noGravity = true;
                smoke.fadeIn = 0.25f;
            }
        }

        private static void SpawnFallingEmber(int x, int y, TileHeatData data)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            if (!Main.player[data.Owner].active)
                return;

            int damage = Math.Max(1, data.Damage / 4);
            Vector2 position = new Vector2(x * 16f + 8f, y * 16f + 18f);
            Vector2 velocity = new Vector2(Main.rand.NextFloat(-0.4f, 0.4f), Main.rand.NextFloat(1.6f, 3.2f));
            Projectile.NewProjectile(
                new EntitySource_Misc("SariaTileHeat"),
                position,
                velocity,
                ModContent.ProjectileType<RovaEmber>(),
                damage,
                0f,
                data.Owner
            );
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

                if (TryGetHottestHeatInArea(player.Hitbox, 0, out TileHeatData contactHeat, out float contactIntensity) && contactIntensity >= 0.5f)
                {
                    if (!player.HasBuff(ModContent.BuffType<Veil>()))
                    {
                        player.buffImmune[ModContent.BuffType<Burning2>()] = false;
                        player.AddBuff(ModContent.BuffType<Burning2>(), 2, quiet: false);
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            TryDamagePlayerFromHeat(player, contactHeat);
                        }
                    }
                }
                else if (TryGetHottestHeatInArea(player.Hitbox, 48, out _, out float nearbyIntensity) && nearbyIntensity >= 0.35f)
                {
                    if (!IsPlayerFireProtected(player))
                    {
                        player.buffImmune[BuffID.OnFire] = false;
                        player.AddBuff(BuffID.OnFire, 2, quiet: false);
                    }
                }
            }
        }

        private static void ApplyNPCTileHeatEffects()
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc == null || !npc.active || npc.friendly || npc.lifeMax <= 0 || npc.dontTakeDamage)
                    continue;

                if (TryGetHottestHeatInArea(npc.Hitbox, 0, out TileHeatData contactHeat, out float contactIntensity) && contactIntensity >= 0.5f)
                {
                    npc.buffImmune[ModContent.BuffType<Burning2>()] = false;
                    npc.AddBuff(ModContent.BuffType<Burning2>(), 2);
                    TryDamageNPCFromHeat(npc, contactHeat);
                }
                else if (TryGetHottestHeatInArea(npc.Hitbox, 48, out _, out float nearbyIntensity) && nearbyIntensity >= 0.35f)
                {
                    npc.buffImmune[BuffID.OnFire] = false;
                    npc.AddBuff(BuffID.OnFire, 2);
                }
            }
        }

        private static bool TryGetHottestHeatInArea(Rectangle area, int padding, out TileHeatData hottestHeat, out float hottestIntensity)
        {
            hottestHeat = default;
            hottestIntensity = 0f;

            if (heatedTiles == null || heatedTiles.Count == 0)
                return false;

            Rectangle expandedArea = area;
            expandedArea.Inflate(padding, padding);
            int currentTick = (int)Main.GameUpdateCount;

            foreach (var kvp in heatedTiles)
            {
                Rectangle tileRect = new Rectangle(kvp.Key.x * 16, kvp.Key.y * 16, 16, 16);
                if (!tileRect.Intersects(expandedArea))
                    continue;

                float intensity = GetHeatIntensity(kvp.Value, currentTick);
                if (intensity <= hottestIntensity)
                    continue;

                hottestIntensity = intensity;
                hottestHeat = kvp.Value;
            }

            return hottestIntensity > 0f;
        }

        public static bool IsPlayerFireProtected(Player player)
        {
            return player.HasBuff(ModContent.BuffType<Veil>())
                || player.HasBuff(BuffID.ObsidianSkin)
                || player.lavaImmune
                || player.fireWalk;
        }

        private static void TryDamagePlayerFromHeat(Player player, TileHeatData heatData)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient || playerHeatDamageCooldowns == null)
                return;

            playerHeatDamageCooldowns.TryGetValue(player.whoAmI, out int cooldown);
            if (cooldown > 0)
                return;

            int damage = Math.Max(1, player.statLifeMax2 / 15);
            player.Hurt(PlayerDeathReason.ByCustomReason(player.name + " was scorched by Rova's fire."), damage, 0, false, false, false, -1);
            playerHeatDamageCooldowns[player.whoAmI] = 60;
        }

        private static void TryDamageNPCFromHeat(NPC npc, TileHeatData heatData)
        {
            if (npcHeatDamageCooldowns == null)
                return;

            npcHeatDamageCooldowns.TryGetValue(npc.whoAmI, out int cooldown);
            if (cooldown > 0)
                return;

            int damage = Math.Max(1, heatData.Damage / 5);
            npc.StrikeNPC(damage, 0f, 0);
            npcHeatDamageCooldowns[npc.whoAmI] = 60;

            if (Main.netMode == NetmodeID.Server)
            {
                NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, npc.whoAmI);
            }
        }

        private static void DecayCooldowns(Dictionary<int, int> cooldowns)
        {
            if (cooldowns == null || cooldowns.Count == 0 || cooldownKeysToRemove == null)
                return;

            cooldownKeysToRemove.Clear();
            List<int> keys = new List<int>(cooldowns.Keys);
            foreach (int key in keys)
            {
                int next = cooldowns[key] - 1;
                if (next <= 0)
                    cooldownKeysToRemove.Add(key);
                else
                    cooldowns[key] = next;
            }

            foreach (int key in cooldownKeysToRemove)
            {
                cooldowns.Remove(key);
            }
        }

        public override void PostDrawInterface(SpriteBatch spriteBatch)
        {
            if (Main.dedServ || Main.gameMenu || Main.LocalPlayer == null || !Main.LocalPlayer.active)
                return;

            float intensity = 0f;
            if (TryGetHottestHeatInArea(Main.LocalPlayer.Hitbox, 96, out _, out float heatIntensity))
            {
                intensity = heatIntensity;
            }

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile projectile = Main.projectile[i];
                if (!projectile.active || projectile.ModProjectile is not RovaCenter)
                    continue;

                float distance = Vector2.Distance(Main.LocalPlayer.Center, projectile.Center);
                if (distance >= 112f)
                    continue;

                intensity = Math.Max(intensity, 1f - distance / 112f);
            }

            if (intensity <= 0f)
                return;

            float alpha = MathHelper.Clamp(intensity * 0.16f, 0f, 0.16f);
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
            heatedTiles?.Clear();
            playerHeatDamageCooldowns?.Clear();
            npcHeatDamageCooldowns?.Clear();
        }
    }
}
