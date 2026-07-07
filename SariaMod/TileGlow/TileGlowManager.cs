using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using SariaMod.Dusts;

namespace SariaMod.TileGlow
{
    public class TileGlowManager : ModSystem
    {
        /// <summary>
        /// Dictionary to store glow data for each tile position
        /// Key: (x, y) tile coordinates
        /// </summary>
        private static Dictionary<(int x, int y), TileGlowData> glowingTiles = new Dictionary<(int x, int y), TileGlowData>();
        
        /// <summary>
        /// List of tiles to remove after iteration
        /// </summary>
        private static List<(int x, int y)> tilesToRemove = new List<(int x, int y)>();
        
        /// <summary>
        /// Flag to track if we just loaded the world (to prevent stuck states)
        /// </summary>
        private static bool justLoadedWorld = false;
        
        /// <summary>
        /// Default glow duration in ticks (1500 = 25 seconds at 60fps)
        /// </summary>
        public const int DefaultGlowDuration = 1500;
        
        /// <summary>
        /// Color phases for the glow effect
        /// </summary>
        private static readonly Color DeepBlue = new Color(10, 30, 100);
        private static readonly Color MediumBlue = new Color(20, 60, 160);
        private static readonly Color LightBlue = new Color(80, 160, 255);
        private static readonly Color BrightWhite = new Color(255, 255, 255);
        
        public static TileGlowManager Instance { get; private set; }
        
        public override void Load()
        {
            Instance = this;
        }
        
        public override void Unload()
        {
            glowingTiles?.Clear();
            glowingTiles = null;
            tilesToRemove?.Clear();
            tilesToRemove = null;
            Instance = null;
        }
        
        /// <summary>
        /// Clear glowing tiles when world loads to prevent stuck states
        /// </summary>
        public override void OnWorldLoad()
        {
            // Immediately clear any stuck tiles from previous sessions
            ClearAllGlows();
        }

        /// <summary>
        /// Clear glowing tiles when leaving world to prevent persistence
        /// </summary>
        public override void OnWorldUnload()
        {
            // Clear tiles when leaving world
            ClearAllGlows();
        }

        public override void PreUpdatePlayers()
        {
            if (Main.dedServ)
                return;
            
            UpdateGlowingTiles();
        }
        
        /// <summary>
        /// Check if a tile is exposed to air (has at least one adjacent empty tile)
        /// </summary>
        private static bool IsTileExposedToAir(int x, int y)
        {
            // Check all 8 surrounding tiles plus the 4 cardinal directions at distance 2
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
        
        /// <summary>
        /// Check how buried a tile is (0 = exposed, 1 = one layer deep, 2+ = deeply buried)
        /// </summary>
        private static int GetBuriedDepth(int x, int y)
        {
            // If directly exposed, depth is 0
            if (IsTileExposedToAir(x, y))
                return 0;
            
            // Check if any adjacent tile is exposed (depth 1)
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
            
            // Deeply buried (2+ layers)
            return 2;
        }
        
        /// <summary>
        /// Draw colored overlays on glowing tiles using the actual tile texture
        /// </summary>
        public override void PostDrawTiles()
        {
            // Only skip on dedicated servers - host & play needs to draw the glow
            if (Main.dedServ)
                return;
                
            if (glowingTiles == null || glowingTiles.Count == 0)
                return;
            
            int currentTick = (int)Main.GameUpdateCount;
            
            // Begin sprite batch with additive blending for glow effect
            Main.spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.Additive,
                SamplerState.PointClamp,
                DepthStencilState.None,
                RasterizerState.CullNone,
                null,
                Main.GameViewMatrix.ZoomMatrix
            );
            
            foreach (var kvp in glowingTiles)
            {
                var data = kvp.Value;
                
                if (data.IsExpired(currentTick))
                    continue;
                
                int x = kvp.Key.x;
                int y = kvp.Key.y;
                
                // Check if tile is on screen
                Vector2 screenPos = new Vector2(x * 16f, y * 16f) - Main.screenPosition;
                if (screenPos.X < -16 || screenPos.X > Main.screenWidth + 16 ||
                    screenPos.Y < -16 || screenPos.Y > Main.screenHeight + 16)
                    continue;
                
                // Get tile
                Tile tile = Main.tile[x, y];
                if (!tile.HasTile)
                    continue;
                
                // Calculate color and intensity
                float progress = data.GetProgress(currentTick);
                float normalizedDist = data.NormalizedDistance;
                
                // Check how buried the tile is
                int buriedDepth = GetBuriedDepth(x, y);
                
                float fadeStartProgress = MathHelper.Lerp(0.75f, 0.35f, normalizedDist);
                
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
                
                // Get color based on buried depth
                Color glowColor;
                if (buriedDepth >= 2)
                {
                    // Deeply buried - always dark blue
                    glowColor = DeepBlue;
                }
                else if (buriedDepth == 1)
                {
                    // One layer deep - darker version of normal color
                    Color normalColor = GetGlowColor(progress, normalizedDist);
                    glowColor = Color.Lerp(normalColor, DeepBlue, 0.6f);
                }
                else
                {
                    // Exposed to air - normal color
                    glowColor = GetGlowColor(progress, normalizedDist);
                }
                
                float alpha = intensity * 1f;
                Color drawColor = glowColor * alpha;
                
                // Draw the tile with its actual texture, tinted with our glow color
                DrawTileGlow(tile, x, y, screenPos, drawColor);
            }
            
            Main.spriteBatch.End();
        }
        
        /// <summary>
        /// Draw the glow using the tile's actual texture so transparent pixels are preserved
        /// </summary>
        private void DrawTileGlow(Tile tile, int tileX, int tileY, Vector2 screenPos, Color color)
        {
            int tileType = tile.TileType;
            
            // Get the tile texture
            Texture2D tileTexture = TextureAssets.Tile[tileType].Value;
            if (tileTexture == null)
                return;
            
            // Calculate the source rectangle for this specific tile frame
            int frameX = tile.TileFrameX;
            int frameY = tile.TileFrameY;
            
            // Handle tile animation if applicable
            // Main.tileFrame stores the current animation frame for animated tiles
            // For most animated tiles, this adds to frameY in increments based on the tile's frame height
            if (Main.tileFrame[tileType] > 0)
            {
                // Get the animation frame offset
                // For standard tiles, each frame is 18 pixels (16 + 2 padding)
                // But for multi-tile objects, we need to account for the full object height
                int animFrameOffset = GetAnimationFrameOffset(tileType, tileX, tileY);
                frameY += Main.tileFrame[tileType] * animFrameOffset;
            }
            
            // Validate source rectangle bounds to prevent drawing outside the texture
            // This fixes issues with animated tiles like campfires
            int sourceWidth = 16;
            int sourceHeight = 16;
            
            // Clamp to texture bounds
            if (frameX < 0) frameX = 0;
            if (frameY < 0) frameY = 0;
            if (frameX + sourceWidth > tileTexture.Width)
            {
                // Frame is outside texture bounds - skip drawing
                return;
            }
            if (frameY + sourceHeight > tileTexture.Height)
            {
                // Frame is outside texture bounds - this can happen with animated tiles
                // Try to wrap or skip
                return;
            }
            
            // Get slope type
            var slope = tile.Slope;
            bool isHalfBlock = tile.IsHalfBlock;
            
            // Handle sloped and half-block tiles
            if (slope != SlopeType.Solid || isHalfBlock)
            {
                DrawSlopedOrHalfTileGlow(tile, tileTexture, frameX, frameY, slope, isHalfBlock, screenPos, color);
                return;
            }
            
            // Standard tile - let the texture's transparency handle everything
            Rectangle sourceRect = new Rectangle(frameX, frameY, sourceWidth, sourceHeight);
            Rectangle destRect = new Rectangle((int)screenPos.X, (int)screenPos.Y, 16, 16);
            
            // Draw the tile texture with color tint
            Main.spriteBatch.Draw(tileTexture, destRect, sourceRect, color);
        }
        
        /// <summary>
        /// Get the animation frame offset for a tile type.
        /// For multi-tile objects like campfires, this returns the full height of the object in pixels.
        /// </summary>
        private int GetAnimationFrameOffset(int tileType, int tileX, int tileY)
        {
            // Special handling for known problematic animated tiles
            // Campfire and similar tiles have complex animation layouts
            
            // Check TileObjectData for the tile to get proper frame dimensions
            var tileData = Terraria.ObjectData.TileObjectData.GetTileData(tileType, 0);
            if (tileData != null)
            {
                // Multi-tile animated objects: frame offset is the full object height
                // TileObjectData.CoordinateFullHeight gives us the total height of one animation frame
                int fullHeight = tileData.CoordinateFullHeight;
                if (fullHeight > 0)
                {
                    return fullHeight;
                }
                
                // Fallback: calculate from coordinate heights
                int totalHeight = 0;
                if (tileData.CoordinateHeights != null)
                {
                    foreach (int h in tileData.CoordinateHeights)
                    {
                        totalHeight += h + tileData.CoordinatePadding;
                    }
                }
                if (totalHeight > 0)
                {
                    return totalHeight;
                }
            }
            
            // Default: standard single-tile animation (16 pixels + 2 padding)
            return 18;
        }
        
        /// <summary>
        /// Draw glow for sloped or half-block tiles with correct orientation
        /// </summary>
        private void DrawSlopedOrHalfTileGlow(Tile tile, Texture2D texture, int frameX, int frameY,
            SlopeType slope, bool isHalfBlock, Vector2 screenPos, Color color)
        {
            // Validate bounds first
            if (frameX < 0 || frameY < 0 || frameX + 16 > texture.Width || frameY + 16 > texture.Height)
                return;
            
            if (isHalfBlock && slope == SlopeType.Solid)
            {
                // Half block - draw bottom half
                if (frameY + 16 > texture.Height) return;
                
                Rectangle sourceRect = new Rectangle(frameX, frameY + 8, 16, 8);
                Rectangle destRect = new Rectangle((int)screenPos.X, (int)screenPos.Y + 8, 16, 8);
                Main.spriteBatch.Draw(texture, destRect, sourceRect, color);
                return;
            }
            
            // For slopes, draw row by row with correct orientation
            // Flipped on X axis to align properly
            for (int row = 0; row < 16; row++)
            {
                if (frameY + row >= texture.Height) break;
                
                int clipLeft = 0;
                int clipRight = 0;
                
                // Row 0 is TOP of tile, row 15 is BOTTOM
                // Flipped X axis from previous version
                switch (slope)
                {
                    case SlopeType.SlopeDownRight: // ◢
                        clipLeft = 15 - row;
                        break;
                    case SlopeType.SlopeDownLeft: // ◣
                        clipRight = 15 - row;
                        break;
                    case SlopeType.SlopeUpRight: // ◥
                        clipLeft = row;
                        break;
                    case SlopeType.SlopeUpLeft: // ◤
                        clipRight = row;
                        break;
                }
                
                int width = 16 - clipLeft - clipRight;
                
                if (width > 0 && frameX + clipLeft + width <= texture.Width)
                {
                    Rectangle srcRow = new Rectangle(
                        frameX + clipLeft,
                        frameY + row,
                        width,
                        1
                    );
                    
                    Rectangle destRow = new Rectangle(
                        (int)screenPos.X + clipLeft,
                        (int)screenPos.Y + row,
                        width,
                        1
                    );
                    
                    Main.spriteBatch.Draw(texture, destRow, srcRow, color);
                }
            }
        }
        
        /// <summary>
        /// Apply glow to tiles in a radius around a point
        /// </summary>
        public static void ApplyGlowInRadius(Vector2 worldCenter, float radius, int duration = DefaultGlowDuration)
        {
            if (glowingTiles == null)
                return;
                
            int centerTileX = (int)(worldCenter.X / 16f);
            int centerTileY = (int)(worldCenter.Y / 16f);
            int tileRadius = (int)(radius / 16f) + 1;
            
            int currentTick = (int)Main.GameUpdateCount;
            
            for (int x = centerTileX - tileRadius; x <= centerTileX + tileRadius; x++)
            {
                for (int y = centerTileY - tileRadius; y <= centerTileY + tileRadius; y++)
                {
                    // Bounds check
                    if (x < 0 || x >= Main.maxTilesX || y < 0 || y >= Main.maxTilesY)
                        continue;
                    
                    Tile tile = Main.tile[x, y];
                    
                    // Skip if no solid tile
                    if (!tile.HasTile)
                        continue;
                    
                    // Skip liquids
                    if (tile.LiquidAmount > 0)
                        continue;
                    
                    // Skip actuated tiles
                    if (tile.IsActuated)
                        continue;
                    
                    // Calculate distance from center
                    float tileWorldX = x * 16f + 8f;
                    float tileWorldY = y * 16f + 8f;
                    float distance = Vector2.Distance(worldCenter, new Vector2(tileWorldX, tileWorldY));
                    
                    // Skip if outside radius
                    if (distance > radius)
                        continue;
                    
                    var key = (x, y);
                    
                    // Only add/update if this would extend the glow or increase intensity
                    if (glowingTiles.TryGetValue(key, out TileGlowData existing))
                    {
                        // If new glow would be closer to center, update it
                        if (distance < existing.DistanceFromCenter)
                        {
                            glowingTiles[key] = new TileGlowData(currentTick, duration, distance, radius);
                        }
                    }
                    else
                    {
                        glowingTiles[key] = new TileGlowData(currentTick, duration, distance, radius);
                    }
                }
            }
        }
        
        /// <summary>
        /// Apply glow to a single tile (used for network sync)
        /// </summary>
        public static void ApplyGlowToTile(int x, int y, float distanceFromCenter, float maxRadius, int duration = DefaultGlowDuration)
        {
            if (glowingTiles == null)
                return;
                
            if (x < 0 || x >= Main.maxTilesX || y < 0 || y >= Main.maxTilesY)
                return;
            
            Tile tile = Main.tile[x, y];
            
            if (!tile.HasTile || tile.LiquidAmount > 0 || tile.IsActuated)
                return;
            
            int currentTick = (int)Main.GameUpdateCount;
            var key = (x, y);
            
            if (glowingTiles.TryGetValue(key, out TileGlowData existing))
            {
                if (distanceFromCenter < existing.DistanceFromCenter)
                {
                    glowingTiles[key] = new TileGlowData(currentTick, duration, distanceFromCenter, maxRadius);
                }
            }
            else
            {
                glowingTiles[key] = new TileGlowData(currentTick, duration, distanceFromCenter, maxRadius);
            }
        }
        
        /// <summary>
        /// Update all glowing tiles and remove expired ones
        /// </summary>
        private void UpdateGlowingTiles()
        {
            if (glowingTiles == null || glowingTiles.Count == 0)
                return;
            
            int currentTick = (int)Main.GameUpdateCount;
            tilesToRemove.Clear();
            
            foreach (var kvp in glowingTiles)
            {
                var data = kvp.Value;
                
                // Check if expired
                if (data.IsExpired(currentTick))
                {
                    tilesToRemove.Add(kvp.Key);
                    continue;
                }
                
                int x = kvp.Key.x;
                int y = kvp.Key.y;
                
                // Calculate color and add light
                float progress = data.GetProgress(currentTick);
                float normalizedDist = data.NormalizedDistance;
                
                // Check how buried the tile is
                int buriedDepth = GetBuriedDepth(x, y);
                
                // Tiles closer to center fade out later
                float fadeStartProgress = MathHelper.Lerp(0.75f, 0.35f, normalizedDist);
                
                // Calculate intensity based on progress and distance
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
                
                // Get the color based on progress and buried depth
                Color glowColor;
                float lightMultiplier;
                
                if (buriedDepth >= 2)
                {
                    // Deeply buried - always dark blue, minimal light
                    glowColor = DeepBlue;
                    lightMultiplier = 0.3f;
                }
                else if (buriedDepth == 1)
                {
                    // One layer deep - darker, reduced light
                    Color normalColor = GetGlowColor(progress, normalizedDist);
                    glowColor = Color.Lerp(normalColor, DeepBlue, 0.6f);
                    lightMultiplier = 0.8f;
                }
                else
                {
                    // Exposed to air - check if it's a blue phase for full light
                    glowColor = GetGlowColor(progress, normalizedDist);
                    
                    // Reduce light for white/bright phases, keep it for blue phases
                    float blueAmount = (glowColor.B / 255f) - ((glowColor.R + glowColor.G) / 510f);
                    lightMultiplier = MathHelper.Lerp(0.5f, 1.5f, Math.Max(0, blueAmount));
                }
                
                // Add light to the tile
                Vector3 lightColor = glowColor.ToVector3() * intensity * lightMultiplier;
                Lighting.AddLight(new Vector2(x * 16f + 8f, y * 16f + 8f), lightColor);
                
                // Spawn cold dust and snow twinkle on exposed tiles occasionally
                if (buriedDepth == 0 && intensity > 0.3f)
                {
                    // Cold4 dust - only spawn on tiles with white-ish light (during bright phase)
                    // Check if the glow color is more white than blue
                    bool isWhitePhase = glowColor.R > 150 && glowColor.G > 150 && glowColor.B > 150;
                    
                    if (isWhitePhase && Main.rand.NextBool(1000))
                    {
                        Vector2 dustPos = new Vector2(x * 16f + Main.rand.Next(16), y * 16f + Main.rand.Next(16));
                        Vector2 dustVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-1f, -0.3f));
                        Dust cold = Dust.NewDustPerfect(dustPos, ModContent.DustType<Cold4>(), dustVel, 0, default, 1f);
                        cold.noGravity = true;
                    }
                    
                    // Snow twinkle effect - occasional sparkle
                    if (Main.rand.NextBool(400))
                    {
                        Vector2 sparklePos = new Vector2(x * 16f + Main.rand.Next(16), y * 16f + Main.rand.Next(16));
                        Dust snow = Dust.NewDustPerfect(sparklePos, ModContent.DustType<Snow2>(), Vector2.Zero, 0, default, Main.rand.NextFloat(0.8f, 1.5f));
                        snow.noGravity = true;
                        snow.velocity = Vector2.Zero;
                        snow.fadeIn = 0.5f;
                        
                        // Bright white twinkle light
                        Lighting.AddLight(sparklePos, 0.5f, 0.55f, 0.6f);
                    }
                }
            }
            
            // Remove expired tiles
            foreach (var key in tilesToRemove)
            {
                glowingTiles.Remove(key);
            }
        }
        
        /// <summary>
        /// Get the glow color based on progress and distance
        /// White (center) -> Dark Blue -> Transparent (normal)
        /// </summary>
        public static Color GetGlowColor(float progress, float normalizedDistance)
        {
            // Phase timings - center tiles stay white much longer
            float whitePhaseEnd = MathHelper.Lerp(0.5f, 0.1f, normalizedDistance);
            float bluePhaseEnd = MathHelper.Lerp(0.75f, 0.45f, normalizedDistance);
            
            // Peak color by distance - center is pure white, edges start more blue
            float distSquared = normalizedDistance * normalizedDistance;
            Color peakColor = Color.Lerp(BrightWhite, LightBlue, distSquared);
            
            Color result;
            
            if (progress < whitePhaseEnd)
            {
                // White/bright phase
                float rampUpEnd = whitePhaseEnd * 0.15f;
                if (progress < rampUpEnd)
                {
                    // Very quick ramp from blue to peak
                    float t = progress / rampUpEnd;
                    t = t * t; // Ease in
                    result = Color.Lerp(MediumBlue, peakColor, t);
                }
                else
                {
                    // Hold at peak color
                    result = peakColor;
                }
            }
            else if (progress < bluePhaseEnd)
            {
                // Transition from peak to deep dark blue
                float t = (progress - whitePhaseEnd) / (bluePhaseEnd - whitePhaseEnd);
                t = t * t; // Ease out - slower start, faster end
                result = Color.Lerp(peakColor, DeepBlue, t);
            }
            else
            {
                // Fade from deep blue to transparent
                float t = (progress - bluePhaseEnd) / (1f - bluePhaseEnd);
                t = (float)Math.Sqrt(t); // Ease in - gradual fade
                result = Color.Lerp(DeepBlue, Color.Transparent, t);
            }
            
            return result;
        }
        
        /// <summary>
        /// Get the count of currently glowing tiles (for debugging)
        /// </summary>
        public static int GetGlowingTileCount()
        {
            return glowingTiles?.Count ?? 0;
        }
        
        /// <summary>
        /// Clear all glowing tiles
        /// </summary>
        public static void ClearAllGlows()
        {
            glowingTiles?.Clear();
        }
    }
}
