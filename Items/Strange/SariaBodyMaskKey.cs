using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using ReLogic.Content;

namespace SariaMod.Items.Strange
{
    /// <summary>
    /// Runtime body-mask extraction for Form 4 (Transform 3).
    ///
    /// Instead of pre-made mask textures for every body part, a single
    /// reference image (<c>4SariaMask1Swap</c>) defines which pixel colors
    /// should be extracted from any body texture. At runtime the system
    /// scans the body texture, keeps pixels whose colors appear in the
    /// reference, and makes everything else transparent. The result is
    /// drawn as the Mask1 glow overlay.
    ///
    /// Usage:
    ///   var mask = SariaBodyMaskKey.GetBodyMask(bodyTexture);
    ///   if (mask != null)
    ///       projectile.SariaMaindraw(mask, true, false, false, 1, 1, lightColor);
    /// </summary>
    public class SariaBodyMaskKey : ILoadable
    {
        private static HashSet<Color> _maskColors;
        private static bool _colorsLoaded;

        private static readonly Dictionary<Texture2D, Texture2D> _cache = new();

        private static Dictionary<Color, Color> _mask2SwapMap;
        private static bool _mask2ColorsLoaded;

        private static readonly Dictionary<Texture2D, Texture2D> _mask2Cache = new();

        private static Dictionary<Color, Color> _mask3SwapMap;
        private static bool _mask3ColorsLoaded;

        private static readonly Dictionary<Texture2D, Texture2D> _mask3Cache = new();

        // Form 5 (Transform 4) — both masks are Dictionary color swaps
        private static Dictionary<Color, Color> _form5Mask1SwapMap;
        private static bool _form5Mask1Loaded;
        private static readonly Dictionary<Texture2D, Texture2D> _form5Mask1Cache = new();

        private static Dictionary<Color, Color> _form5Mask2SwapMap;
        private static bool _form5Mask2Loaded;
        private static readonly Dictionary<Texture2D, Texture2D> _form5Mask2Cache = new();

        public void Load(Mod mod) { }

        public void Unload()
        {
            foreach (var kvp in _cache)
            {
                kvp.Value?.Dispose();
            }
            _cache.Clear();
            _maskColors = null;
            _colorsLoaded = false;

            foreach (var kvp in _mask2Cache)
            {
                kvp.Value?.Dispose();
            }
            _mask2Cache.Clear();
            _mask2SwapMap = null;
            _mask2ColorsLoaded = false;

            foreach (var kvp in _mask3Cache)
            {
                kvp.Value?.Dispose();
            }
            _mask3Cache.Clear();
            _mask3SwapMap = null;
            _mask3ColorsLoaded = false;

            foreach (var kvp in _form5Mask1Cache)
            {
                kvp.Value?.Dispose();
            }
            _form5Mask1Cache.Clear();
            _form5Mask1SwapMap = null;
            _form5Mask1Loaded = false;

            foreach (var kvp in _form5Mask2Cache)
            {
                kvp.Value?.Dispose();
            }
            _form5Mask2Cache.Clear();
            _form5Mask2SwapMap = null;
            _form5Mask2Loaded = false;
        }

        private static void EnsureColorsLoaded()
        {
            if (_colorsLoaded) return;
            _colorsLoaded = true;

            _maskColors = new HashSet<Color>();
            const string path = "SariaMod/Items/Strange/4SariaAnimations/4SariaMask1Swap";
            if (!ModContent.HasAsset(path)) return;

            var swapTex = ModContent.Request<Texture2D>(path, AssetRequestMode.ImmediateLoad).Value;
            Color[] pixels = new Color[swapTex.Width * swapTex.Height];
            swapTex.GetData(pixels);
            foreach (var px in pixels)
            {
                if (px.A > 0)
                    _maskColors.Add(px);
            }
        }

        /// <summary>
        /// Generates a mask texture from the given body texture by keeping
        /// only pixels whose colors appear in the <c>4SariaMask1Swap</c>
        /// reference image. Everything else becomes transparent.
        /// Returns null if no matching pixels are found or the swap image
        /// is not available.
        /// </summary>
        public static Texture2D GetBodyMask(Texture2D bodyTex)
        {
            if (bodyTex == null) return null;

            EnsureColorsLoaded();
            if (_maskColors == null || _maskColors.Count == 0) return null;

            if (_cache.TryGetValue(bodyTex, out var cached))
                return cached;

            int pixelCount = bodyTex.Width * bodyTex.Height;
            Color[] pixels = new Color[pixelCount];
            bodyTex.GetData(pixels);

            bool anyKept = false;
            int matchCount = 0;
            for (int i = 0; i < pixelCount; i++)
            {
                if (pixels[i].A > 0 && _maskColors.Contains(pixels[i]))
                {
                    anyKept = true;
                    matchCount++;
                }
                else
                {
                    pixels[i] = Color.Transparent;
                }
            }

            if (!anyKept)
            {
                _cache[bodyTex] = null;
                return null;
            }

            var mask = new Texture2D(Main.graphics.GraphicsDevice, bodyTex.Width, bodyTex.Height);
            mask.SetData(pixels);
            _cache[bodyTex] = mask;
            return mask;
        }

        /// <summary>
        /// Clears the cache for a specific body texture.
        /// Call if a texture is hot-reloaded during development.
        /// </summary>
        public static void InvalidateCache(Texture2D bodyTex)
        {
            if (_cache.TryGetValue(bodyTex, out var tex))
            {
                tex?.Dispose();
                _cache.Remove(bodyTex);
            }
            if (_mask2Cache.TryGetValue(bodyTex, out var tex2))
            {
                tex2?.Dispose();
                _mask2Cache.Remove(bodyTex);
            }
            if (_mask3Cache.TryGetValue(bodyTex, out var tex3))
            {
                tex3?.Dispose();
                _mask3Cache.Remove(bodyTex);
            }
            if (_form5Mask1Cache.TryGetValue(bodyTex, out var tex5_1))
            {
                tex5_1?.Dispose();
                _form5Mask1Cache.Remove(bodyTex);
            }
            if (_form5Mask2Cache.TryGetValue(bodyTex, out var tex5_2))
            {
                tex5_2?.Dispose();
                _form5Mask2Cache.Remove(bodyTex);
            }
        }

        private static void EnsureMask2ColorsLoaded()
        {
            if (_mask2ColorsLoaded) return;
            _mask2ColorsLoaded = true;

            _mask2SwapMap = new Dictionary<Color, Color>();
            const string path = "SariaMod/Items/Strange/4SariaAnimations/4SariaMask2Swap";
            if (!ModContent.HasAsset(path)) return;

            var swapTex = ModContent.Request<Texture2D>(path, AssetRequestMode.ImmediateLoad).Value;
            Color[] pixels = new Color[swapTex.Width * swapTex.Height];
            swapTex.GetData(pixels);

            // The swap image is split vertically: left half = source colors,
            // right half = destination colors. Each row maps left→right.
            int halfW = swapTex.Width / 2;
            for (int y = 0; y < swapTex.Height; y++)
            {
                for (int x = 0; x < halfW; x++)
                {
                    Color src = pixels[y * swapTex.Width + x];
                    Color dst = pixels[y * swapTex.Width + x + halfW];
                    if (src.A > 0 && dst.A > 0)
                    {
                        _mask2SwapMap.TryAdd(src, dst);
                    }
                }
            }
        }

        /// <summary>
        /// Generates a mask texture from the given body texture by finding
        /// pixels whose colors appear as source colors in the <c>4SariaMask2Swap</c>
        /// reference image and replacing them with the corresponding destination
        /// colors. Everything else becomes transparent.
        /// Returns null if no matching pixels are found or the swap image
        /// is not available.
        /// </summary>
        public static Texture2D GetBodyMask2(Texture2D bodyTex)
        {
            if (bodyTex == null) return null;

            EnsureMask2ColorsLoaded();
            if (_mask2SwapMap == null || _mask2SwapMap.Count == 0) return null;

            if (_mask2Cache.TryGetValue(bodyTex, out var cached))
                return cached;

            int pixelCount = bodyTex.Width * bodyTex.Height;
            Color[] pixels = new Color[pixelCount];
            bodyTex.GetData(pixels);

            bool anyKept = false;
            int matchCount = 0;
            for (int i = 0; i < pixelCount; i++)
            {
                if (pixels[i].A > 0 && _mask2SwapMap.TryGetValue(pixels[i], out Color dest))
                {
                    pixels[i] = dest;
                    anyKept = true;
                    matchCount++;
                }
                else
                {
                    pixels[i] = Color.Transparent;
                }
            }

            if (!anyKept)
            {
                _mask2Cache[bodyTex] = null;
                return null;
            }

            var mask = new Texture2D(Main.graphics.GraphicsDevice, bodyTex.Width, bodyTex.Height);
            mask.SetData(pixels);
            _mask2Cache[bodyTex] = mask;
            return mask;
        }

        private static void EnsureMask3ColorsLoaded()
        {
            if (_mask3ColorsLoaded) return;
            _mask3ColorsLoaded = true;

            _mask3SwapMap = new Dictionary<Color, Color>();
            const string path = "SariaMod/Items/Strange/4SariaAnimations/4SariaMask3Swap";
            if (!ModContent.HasAsset(path)) return;

            var swapTex = ModContent.Request<Texture2D>(path, AssetRequestMode.ImmediateLoad).Value;
            Color[] pixels = new Color[swapTex.Width * swapTex.Height];
            swapTex.GetData(pixels);

            // The swap image is split vertically: left half = source colors,
            // right half = destination colors. Each row maps left→right.
            int halfW = swapTex.Width / 2;
            for (int y = 0; y < swapTex.Height; y++)
            {
                for (int x = 0; x < halfW; x++)
                {
                    Color src = pixels[y * swapTex.Width + x];
                    Color dst = pixels[y * swapTex.Width + x + halfW];
                    if (src.A > 0 && dst.A > 0)
                    {
                        _mask3SwapMap.TryAdd(src, dst);
                    }
                }
            }
        }

        /// <summary>
        /// Generates a mask texture from the given body texture by keeping
        /// only pixels whose colors appear in the <c>4SariaMask3Swap</c>
        /// reference image. Everything else becomes transparent.
        /// Used for the random 1-in-40 flicker overlay on direction arms.
        /// Returns null if no matching pixels are found or the swap image
        /// is not available.
        /// </summary>
        public static Texture2D GetBodyMask3(Texture2D bodyTex)
        {
            if (bodyTex == null) return null;

            EnsureMask3ColorsLoaded();
            if (_mask3SwapMap == null || _mask3SwapMap.Count == 0) return null;

            if (_mask3Cache.TryGetValue(bodyTex, out var cached))
                return cached;

            int pixelCount = bodyTex.Width * bodyTex.Height;
            Color[] pixels = new Color[pixelCount];
            bodyTex.GetData(pixels);

            bool anyKept = false;
            int matchCount = 0;
            for (int i = 0; i < pixelCount; i++)
            {
                if (pixels[i].A > 0 && _mask3SwapMap.TryGetValue(pixels[i], out Color dest))
                {
                    pixels[i] = dest;
                    anyKept = true;
                    matchCount++;
                }
                else
                {
                    pixels[i] = Color.Transparent;
                }
            }

            if (!anyKept)
            {
                _mask3Cache[bodyTex] = null;
                return null;
            }

            var mask = new Texture2D(Main.graphics.GraphicsDevice, bodyTex.Width, bodyTex.Height);
            mask.SetData(pixels);
            _mask3Cache[bodyTex] = mask;
            return mask;
        }

        // --- Form 5 Mask1 (Dictionary color swap from 5SariaMask1Swap) ---

        private static void EnsureForm5Mask1Loaded()
        {
            if (_form5Mask1Loaded) return;
            _form5Mask1Loaded = true;

            _form5Mask1SwapMap = new Dictionary<Color, Color>();
            const string path = "SariaMod/Items/Strange/5SariaAnimations/5SariaMask1Swap";
            if (!ModContent.HasAsset(path)) return;

            var swapTex = ModContent.Request<Texture2D>(path, AssetRequestMode.ImmediateLoad).Value;
            Color[] pixels = new Color[swapTex.Width * swapTex.Height];
            swapTex.GetData(pixels);

            int halfW = swapTex.Width / 2;
            for (int y = 0; y < swapTex.Height; y++)
            {
                for (int x = 0; x < halfW; x++)
                {
                    Color src = pixels[y * swapTex.Width + x];
                    Color dst = pixels[y * swapTex.Width + x + halfW];
                    if (src.A > 0 && dst.A > 0)
                    {
                        _form5Mask1SwapMap.TryAdd(src, dst);
                    }
                }
            }
        }

        /// <summary>
        /// Form 5 Mask1: Dictionary color swap using <c>5SariaMask1Swap</c>.
        /// Drawn with <c>Saria5GlowMaskdraw</c> (counter1=true, counter2=false).
        /// </summary>
        public static Texture2D GetForm5Mask1(Texture2D bodyTex)
        {
            if (bodyTex == null) return null;

            EnsureForm5Mask1Loaded();
            if (_form5Mask1SwapMap == null || _form5Mask1SwapMap.Count == 0) return null;

            if (_form5Mask1Cache.TryGetValue(bodyTex, out var cached))
                return cached;

            int pixelCount = bodyTex.Width * bodyTex.Height;
            Color[] pixels = new Color[pixelCount];
            bodyTex.GetData(pixels);

            bool anyKept = false;
            for (int i = 0; i < pixelCount; i++)
            {
                if (pixels[i].A > 0 && _form5Mask1SwapMap.TryGetValue(pixels[i], out Color dest))
                {
                    pixels[i] = dest;
                    anyKept = true;
                }
                else
                {
                    pixels[i] = Color.Transparent;
                }
            }

            if (!anyKept)
            {
                _form5Mask1Cache[bodyTex] = null;
                return null;
            }

            var result = new Texture2D(Main.graphics.GraphicsDevice, bodyTex.Width, bodyTex.Height);
            result.SetData(pixels);
            _form5Mask1Cache[bodyTex] = result;
            return result;
        }

        // --- Form 5 Mask2 (Dictionary color swap from 5SariaMask2Swap) ---

        private static void EnsureForm5Mask2Loaded()
        {
            if (_form5Mask2Loaded) return;
            _form5Mask2Loaded = true;

            _form5Mask2SwapMap = new Dictionary<Color, Color>();
            const string path = "SariaMod/Items/Strange/5SariaAnimations/5SariaMask2Swap";
            if (!ModContent.HasAsset(path)) return;

            var swapTex = ModContent.Request<Texture2D>(path, AssetRequestMode.ImmediateLoad).Value;
            Color[] pixels = new Color[swapTex.Width * swapTex.Height];
            swapTex.GetData(pixels);

            int halfW = swapTex.Width / 2;
            for (int y = 0; y < swapTex.Height; y++)
            {
                for (int x = 0; x < halfW; x++)
                {
                    Color src = pixels[y * swapTex.Width + x];
                    Color dst = pixels[y * swapTex.Width + x + halfW];
                    if (src.A > 0 && dst.A > 0)
                    {
                        _form5Mask2SwapMap.TryAdd(src, dst);
                    }
                }
            }
        }

        /// <summary>
        /// Form 5 Mask2: Dictionary color swap using <c>5SariaMask2Swap</c>.
        /// Drawn with <c>Saria5GlowMaskdraw</c> (counter1=false, counter2=true).
        /// </summary>
        public static Texture2D GetForm5Mask2(Texture2D bodyTex)
        {
            if (bodyTex == null) return null;

            EnsureForm5Mask2Loaded();
            if (_form5Mask2SwapMap == null || _form5Mask2SwapMap.Count == 0) return null;

            if (_form5Mask2Cache.TryGetValue(bodyTex, out var cached))
                return cached;

            int pixelCount = bodyTex.Width * bodyTex.Height;
            Color[] pixels = new Color[pixelCount];
            bodyTex.GetData(pixels);

            bool anyKept = false;
            for (int i = 0; i < pixelCount; i++)
            {
                if (pixels[i].A > 0 && _form5Mask2SwapMap.TryGetValue(pixels[i], out Color dest))
                {
                    pixels[i] = dest;
                    anyKept = true;
                }
                else
                {
                    pixels[i] = Color.Transparent;
                }
            }

            if (!anyKept)
            {
                _form5Mask2Cache[bodyTex] = null;
                return null;
            }

            var result = new Texture2D(Main.graphics.GraphicsDevice, bodyTex.Width, bodyTex.Height);
            result.SetData(pixels);
            _form5Mask2Cache[bodyTex] = result;
            return result;
        }
    }
}
