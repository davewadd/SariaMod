using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;

namespace SariaMod.Items.zTalking
{
    internal sealed class DialogueFaceSet
    {
        public string Name { get; }
        public string EyesPrefix { get; }
        public string MouthPrefix { get; }
        public string ExtraPrefix { get; }

        public DialogueFaceSet(string name, string eyesPrefix, string mouthPrefix, string extraPrefix = "")
        {
            Name = name ?? "Default";
            EyesPrefix = eyesPrefix ?? "Default-Eyes";
            MouthPrefix = mouthPrefix ?? "Default-Mouth";
            ExtraPrefix = extraPrefix ?? string.Empty;
        }
    }

    internal static class DialogueFaceSetRegistry
    {
        private static readonly Dictionary<string, DialogueFaceSet> _sets = new(StringComparer.OrdinalIgnoreCase);
        private static readonly List<string> _registrationOrder = new();

        // Per-transform mouth color palettes applied to ALL mouth types.
        // Key = transform index (0-based). Transform 0 uses the mouth set's original colors.
        // The recoloring auto-detects source colors from each mouth set's base texture
        // (form 1) and swaps them to these target colors for forms 2-6.
        private static readonly Dictionary<int, (Color Border, Color Inner)> MouthTransformPalettes = new()
        {
            { 1, (new Color(0x03, 0x19, 0x2A), new Color(0x95, 0xB8, 0xBE)) },
            { 2, (new Color(0x56, 0x2B, 0x2B), new Color(0x91, 0x52, 0x46)) },
            { 3, (new Color(0x2A, 0x1A, 0x03), new Color(0xC0, 0xBF, 0x92)) },
            { 4, (new Color(0x03, 0x19, 0x2A), new Color(0xCB, 0xB7, 0xF7)) },
            { 5, (new Color(0x5A, 0x21, 0x46), new Color(0xFF, 0xAD, 0xC6)) },
        };

        // Cache for recolored mouth textures to avoid re-creating each frame
        private static readonly Dictionary<(string, int), Texture2D> _recoloredMouthCache = new();

        static DialogueFaceSetRegistry()
        {
            // Built-in default. Authors can register more in Mod.Load or via a system.
            Register(new DialogueFaceSet("Default", "Default-Eyes", "Default-Mouth", ""));
            Register(new DialogueFaceSet("Shocked", "Shocked-Eyes", "Shocked-Mouth", ""));
        }

        public static IReadOnlyList<string> RegisteredNamesInOrder => _registrationOrder;

        public static void Register(DialogueFaceSet set)
        {
            if (set == null || string.IsNullOrWhiteSpace(set.Name))
                return;

            string key = set.Name.Trim();
            _sets[key] = set;

            if (!_registrationOrder.Contains(key))
                _registrationOrder.Add(key);
        }

        public static DialogueFaceSet Get(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                name = "Default";

            if (_sets.TryGetValue(name.Trim(), out var set))
                return set;

            return _sets["Default"];
        }

        public static Texture2D TryResolveEyesTexture(string faceSetName, int transform)
            => TryResolveVariantTexture(Get(faceSetName).EyesPrefix, transform);

        public static Texture2D TryResolveMouthTexture(string faceSetName, int transform)
        {
            var set = Get(faceSetName);

            // Palette-based recoloring for all mouth types (forms 2-6)
            // Auto-detects the two most common colors in the base (form 1) texture
            // and swaps them to the per-transform target palette.
            if (MouthTransformPalettes.TryGetValue(transform, out var palette))
            {
                var cacheKey = (set.MouthPrefix, transform);
                if (_recoloredMouthCache.TryGetValue(cacheKey, out var cached))
                    return cached;

                try
                {
                    // Load the form 1 base texture with ImmediateLoad to ensure pixel data
                    // is available for GetData (async-loaded textures may return placeholders
                    // whose pixel data is all-transparent, causing auto-detection to fail).
                    string basePath = $"SariaMod/Items/zTalking/{set.MouthPrefix}1";
                    Texture2D baseTexture = ModContent.Request<Texture2D>(basePath, AssetRequestMode.ImmediateLoad).Value;
                    if (baseTexture != null)
                    {
                        Texture2D recolored = RecolorTexture(baseTexture, palette.Border, palette.Inner);
                        if (recolored != null)
                        {
                            _recoloredMouthCache[cacheKey] = recolored;
                            return recolored;
                        }
                    }
                }
                catch (Exception ex)
                {
                    SariaMod.Instance?.Logger?.Warn($"Mouth recolor failed for transform {transform}: {ex.Message}");
                }

                // Recoloring failed: return per-form fallback WITHOUT caching
                // so the next access retries recoloring (base texture may not have been ready)
                var fallbackTexture = TryResolveVariantTexture(set.MouthPrefix, transform);
                if (fallbackTexture != null)
                    SariaMod.Instance?.Logger?.Info($"Mouth recolor: using per-form fallback for transform {transform} (will retry)");
                return fallbackTexture;
            }

            // Fallback: load per-form texture file if it exists
            return TryResolveVariantTexture(set.MouthPrefix, transform);
        }

        public static Texture2D TryResolveExtraTexture(string faceSetName, int transform)
        {
            var set = Get(faceSetName);
            if (string.IsNullOrWhiteSpace(set.ExtraPrefix))
                return null;

            return TryResolveVariantTexture(set.ExtraPrefix, transform);
        }

        private static Texture2D TryResolveVariantTexture(string prefix, int transform)
        {
            // transform is 0-based in dialogue UI; want suffix {transform+1}
            int suffix = Math.Max(1, transform + 1);

            // Example: "Sad-Eyes{suffix}". Fallback to "Sad-Eyes1".
            string preferred = $"SariaMod/Items/zTalking/{prefix}{suffix}";
            if (ModContent.RequestIfExists(preferred, out Asset<Texture2D> preferredAsset))
                return preferredAsset.Value;

            string fallback = $"SariaMod/Items/zTalking/{prefix}1";
            if (ModContent.RequestIfExists(fallback, out Asset<Texture2D> fallbackAsset))
                return fallbackAsset.Value;

            return null;
        }

        private static Texture2D RecolorTexture(Texture2D source, Color newBorder, Color newInner)
        {
            int pixelCount = source.Width * source.Height;
            Color[] pixels = new Color[pixelCount];
            source.GetData(pixels);

            // Auto-detect the two source colors from the base texture by frequency.
            // Most common non-transparent color = border, second = inner fill.
            uint topKey = 0, secondKey = 0;
            int topCount = 0, secondCount = 0;
            var colorCounts = new Dictionary<uint, int>();

            for (int i = 0; i < pixelCount; i++)
            {
                if (pixels[i].A == 0)
                    continue;

                uint key = pixels[i].PackedValue;
                colorCounts.TryGetValue(key, out int count);
                colorCounts[key] = count + 1;
            }

            foreach (var kv in colorCounts)
            {
                if (kv.Value > topCount)
                {
                    secondKey = topKey;
                    secondCount = topCount;
                    topKey = kv.Key;
                    topCount = kv.Value;
                }
                else if (kv.Value > secondCount)
                {
                    secondKey = kv.Key;
                    secondCount = kv.Value;
                }
            }

            if (topCount == 0 || secondCount == 0)
            {
                SariaMod.Instance?.Logger?.Warn("Mouth recolor: failed to detect two source colors from base texture");
                return null;
            }

            // Swap all pixels matching the two detected source colors
            uint newBorderPacked = new Color(newBorder.R, newBorder.G, newBorder.B, 255).PackedValue;
            uint newInnerPacked = new Color(newInner.R, newInner.G, newInner.B, 255).PackedValue;

            for (int i = 0; i < pixelCount; i++)
            {
                uint pv = pixels[i].PackedValue;
                if (pv == topKey)
                    pixels[i] = new Color { PackedValue = newBorderPacked };
                else if (pv == secondKey)
                    pixels[i] = new Color { PackedValue = newInnerPacked };
            }

            Texture2D result = new Texture2D(Main.graphics.GraphicsDevice, source.Width, source.Height);
            result.SetData(pixels);

            SariaMod.Instance?.Logger?.Info(
                $"Mouth recolor: detected border=({new Color { PackedValue = topKey }.R},{new Color { PackedValue = topKey }.G},{new Color { PackedValue = topKey }.B}) x{topCount}, "
                + $"inner=({new Color { PackedValue = secondKey }.R},{new Color { PackedValue = secondKey }.G},{new Color { PackedValue = secondKey }.B}) x{secondCount} "
                + $"-> border=({newBorder.R},{newBorder.G},{newBorder.B}), inner=({newInner.R},{newInner.G},{newInner.B})");

            return result;
        }
    }
}
