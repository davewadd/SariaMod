using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria.ModLoader;

namespace SariaMod.Items.Strange
{
    /// <summary>
    /// Per-form face color key system.
    /// Color swap pairs are loaded from 2-pixel-wide PNG images at runtime
    /// via <see cref="TextureColorSwap"/>. Destination pixels can be
    /// semi-transparent (any alpha value is preserved).
    ///
    /// Swap images:
    ///   ColorsForSaria1-2-4Faces.png  → Forms 1,2,4 (dummy pixel removal)
    ///   ColorsForSaria3Faces.png      → Form 3 (fire-form recolor)
    ///   ColorsForSaria5Faces.png      → Form 5 glow overlay (pink glow)
    ///
    /// Usage:
    ///   var processed = SariaFaceColorKey.GetProcessedFace(originalTex, transform);
    ///   projectile.SariaMaindraw(processed, ...);
    ///
    ///   var glow = SariaFaceColorKey.GetForm5GlowFace(originalGlobalTex);
    ///   projectile.SariaEyesGlowandFadedraw(glow, lightColor, Color.White);
    /// </summary>
    public class SariaFaceColorKey : ModSystem
    {
        // ── Form group IDs for cache keying ──
        private const int GroupForms124 = 0;
        private const int GroupForm3 = 1;
        private const int GroupNone = 2;
        private const int GroupForm5Glow = 3;
        private const int GroupForm6 = 4;

        // ── Swap maps loaded from 2-pixel-wide PNGs ──
        private static Dictionary<Color, Color> _forms124Map;
        private static Dictionary<Color, Color> _form3Map;
        private static Dictionary<Color, Color> _form5GlowMap;
        private static Dictionary<Color, Color> _form6Map;

        public override void PostSetupContent()
        {
            _forms124Map = TextureColorSwap.LoadSwapMap("SariaMod/ColorsForSaria1-2-4Faces");
            _form3Map = TextureColorSwap.LoadSwapMap("SariaMod/ColorsForSaria3Faces");
            _form5GlowMap = TextureColorSwap.LoadSwapMap("SariaMod/ColorsForSaria5Faces");
            _form6Map = TextureColorSwap.LoadSwapMap("SariaMod/ColorsForSaria6Faces");
        }

        public override void Unload()
        {
            TextureColorSwap.ClearAll();
            _forms124Map = null;
            _form3Map = null;
            _form5GlowMap = null;
            _form6Map = null;
        }

        private static int GetBaseGroup(int transform) => transform switch
        {
            0 or 1 or 3 or 4 => GroupForms124,
            2 => GroupForm3,
            5 => GroupForm6,
            _ => GroupNone,
        };

        private static Dictionary<Color, Color> GetMapForGroup(int group) => group switch
        {
            GroupForms124 => _forms124Map,
            GroupForm3 => _form3Map,
            GroupForm5Glow => _form5GlowMap,
            GroupForm6 => _form6Map,
            _ => null,
        };

        /// <summary>
        /// Returns a processed copy of the face texture with per-form color
        /// swaps applied. Results are cached per (texture, formGroup) pair.
        /// </summary>
        public static Texture2D GetProcessedFace(Texture2D original, int transform, bool skipTransparentDest = false)
        {
            if (original == null) return null;
            int group = GetBaseGroup(transform);
            return TextureColorSwap.Apply(original, GetMapForGroup(group), group, skipTransparentDest);
        }

        /// <summary>
        /// Returns a processed copy of the Global face texture with Form 5
        /// glow color swaps applied (pinks). Used as the glow overlay for
        /// Transform 4 in place of the 5SariaAnimations face sheet.
        /// </summary>
        public static Texture2D GetForm5GlowFace(Texture2D original)
        {
            if (original == null) return null;
            return TextureColorSwap.Apply(original, _form5GlowMap, GroupForm5Glow);
        }

        /// <summary>
        /// Clears the cache for a specific original texture (all form groups).
        /// Call if a texture is hot-reloaded during development.
        /// </summary>
        public static void InvalidateCache(Texture2D original)
        {
            TextureColorSwap.InvalidateCache(original);
        }
    }
}
