using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace SariaMod.Items.Strange
{
    /// <summary>
    /// Reusable image-driven color swap utility.
    /// Accepts a 2-pixel-wide swap image: each row maps left pixel (source)
    /// to right pixel (destination).
    /// - Both A=0 → row ignored.
    /// - Left has color, right A=0 → hides that color (becomes transparent).
    /// - Left A=0, right has color → transparent pixels become that color.
    /// Destination pixels preserve full RGBA, including semi-transparent colors.
    ///
    /// Usage:
    ///   var map = TextureColorSwap.LoadSwapMap("SariaMod/ColorsForSaria5Faces");
    ///   var result = TextureColorSwap.Apply(originalTexture, map, cacheGroup: 3);
    /// </summary>
    public static class TextureColorSwap
    {
        // Cache: (originalTexture, cacheGroup) → processed texture
        private static readonly Dictionary<(Texture2D, int), Texture2D> _cache = new();

        /// <summary>
        /// Loads a swap map from a 2-pixel-wide image asset.
        /// Left column = source colors, right column = destination colors.
        /// Rows where BOTH pixels are fully transparent (A=0) are skipped.
        /// Left has color + right A=0 → hides that color (maps to transparent).
        /// Left A=0 + right has color → maps fully transparent pixels to that color.
        /// Destination pixels preserve full RGBA, including semi-transparent colors.
        /// </summary>
        public static Dictionary<Color, Color> LoadSwapMap(string assetPath)
        {
            var log = ModContent.GetInstance<SariaMod>()?.Logger;
            var map = new Dictionary<Color, Color>();

            if (!ModContent.HasAsset(assetPath))
            {
                log?.Warn($"[TextureColorSwap] Asset NOT FOUND: {assetPath}");
                return map;
            }

            Texture2D swapTex = ModContent.Request<Texture2D>(assetPath, ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            if (swapTex == null || swapTex.Width < 2)
            {
                log?.Warn($"[TextureColorSwap] Asset null or too narrow ({swapTex?.Width}x{swapTex?.Height}): {assetPath}");
                return map;
            }

            log?.Info($"[TextureColorSwap] Loading {assetPath} — size {swapTex.Width}x{swapTex.Height}");

            int halfW = swapTex.Width / 2;
            Color[] pixels = new Color[swapTex.Width * swapTex.Height];
            Main.RunOnMainThread(() => swapTex.GetData(pixels)).GetAwaiter().GetResult();

            for (int y = 0; y < swapTex.Height; y++)
            {
                Color src = pixels[y * swapTex.Width];           // left column
                Color dst = pixels[y * swapTex.Width + halfW];   // right column

                // Skip only when both sides are fully transparent
                if (src.A == 0 && dst.A == 0)
                {
                    log?.Debug($"  Row {y}: SKIP (both transparent)");
                    continue;
                }

                bool added = map.TryAdd(src, dst);
                log?.Info($"  Row {y}: ({src.R},{src.G},{src.B},{src.A}) → ({dst.R},{dst.G},{dst.B},{dst.A}) {(added ? "OK" : "DUPLICATE-SKIPPED")}");
            }

            log?.Info($"[TextureColorSwap] Loaded {map.Count} swap entries from {assetPath}");
            return map;
        }

        /// <summary>
        /// Applies a color swap map to a texture. Results are cached per
        /// (texture, cacheGroup) pair. Destination colors preserve full RGBA.
        /// </summary>
        public static Texture2D Apply(Texture2D original, Dictionary<Color, Color> swapMap, int cacheGroup, bool skipTransparentDest = false)
        {
            if (original == null || swapMap == null || swapMap.Count == 0)
                return original;

            // Use a separate cache slot when skipping transparent destinations
            int effectiveGroup = skipTransparentDest ? cacheGroup + 1000 : cacheGroup;
            var cacheKey = (original, effectiveGroup);
            if (_cache.TryGetValue(cacheKey, out var cached))
                return cached;

            int pixelCount = original.Width * original.Height;
            Color[] pixels = new Color[pixelCount];
            original.GetData(pixels);

            bool anyChanged = false;
            int changeCount = 0;
            for (int i = 0; i < pixelCount; i++)
            {
                if (swapMap.TryGetValue(pixels[i], out Color replacement))
                {
                    if (skipTransparentDest && replacement.A == 0)
                        continue;
                    pixels[i] = replacement;
                    anyChanged = true;
                    changeCount++;
                }
            }

            var log = ModContent.GetInstance<SariaMod>()?.Logger;
            if (!anyChanged)
            {
                // Log unique colors in the texture to help diagnose mismatches
                var uniqueColors = new HashSet<Color>(pixels);
                var sample = string.Join(", ", uniqueColors.Where(c => c.A > 0).Take(15).Select(c => $"({c.R},{c.G},{c.B},{c.A})"));
                log?.Warn($"[TextureColorSwap] NO matches in {original.Name ?? "?"} ({original.Width}x{original.Height}), group {cacheGroup}. Map has {swapMap.Count} entries. Sample opaque colors: {sample}");
                _cache[cacheKey] = original;
                return original;
            }

            log?.Info($"[TextureColorSwap] Swapped {changeCount}/{pixelCount} pixels in {original.Name ?? "?"}, group {cacheGroup}");
            var processed = new Texture2D(Main.graphics.GraphicsDevice, original.Width, original.Height);
            processed.SetData(pixels);
            _cache[cacheKey] = processed;
            return processed;
        }

        /// <summary>
        /// Clears cached results for a specific original texture (all groups).
        /// </summary>
        public static void InvalidateCache(Texture2D original)
        {
            var toRemove = new List<(Texture2D, int)>();
            foreach (var kvp in _cache)
            {
                if (kvp.Key.Item1 == original)
                    toRemove.Add(kvp.Key);
            }

            foreach (var key in toRemove)
            {
                if (_cache.TryGetValue(key, out var tex) && tex != key.Item1)
                    tex?.Dispose();
                _cache.Remove(key);
            }
        }

        /// <summary>
        /// Disposes all cached textures. Call on mod unload.
        /// </summary>
        public static void ClearAll()
        {
            foreach (var kvp in _cache)
            {
                if (kvp.Value != kvp.Key.Item1)
                    kvp.Value?.Dispose();
            }
            _cache.Clear();
        }
    }
}
