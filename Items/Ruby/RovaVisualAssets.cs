using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;

namespace SariaMod.Items.Ruby
{
    internal static class RovaVisualAssets
    {
        private static Texture2D _portal;
        private static Texture2D _beam;
        private static Texture2D _ring;
        private static Texture2D _ember;

        public static Texture2D Portal => _portal ??= Load("SariaMod/Items/Ruby/RovaCenter_AI_PLACEHOLDER_v1");
        public static Texture2D Beam => _beam ??= Load("SariaMod/Items/Ruby/RovaBeam_AI_PLACEHOLDER_v1");
        public static Texture2D Ring => _ring ??= Load("SariaMod/Items/Ruby/RovaRing_AI_PLACEHOLDER_v1");
        public static Texture2D Ember => _ember ??= Load("SariaMod/Items/Ruby/RovaEmber_AI_PLACEHOLDER_v1");

        public static void Unload()
        {
            _portal = null;
            _beam = null;
            _ring = null;
            _ember = null;
        }

        private static Texture2D Load(string assetPath)
        {
            try
            {
                Texture2D texture = ModContent.Request<Texture2D>(assetPath, AssetRequestMode.ImmediateLoad).Value;
                return texture.Width > 2 && texture.Height > 2 ? texture : null;
            }
            catch
            {
                return null;
            }
        }
    }
}
