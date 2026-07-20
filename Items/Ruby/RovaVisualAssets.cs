using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.ModLoader;

namespace SariaMod.Items.Ruby
{
    internal static class RovaVisualAssets
    {
        private const int MaximumSoftGlobRadius = 32;
        private const int SoftGlobFrameSize = MaximumSoftGlobRadius * 2 + 1;
        private const int SoftGlobAtlasColumns = 2;
        private static Texture2D _ember;
        private static Texture2D _softGlobAtlas;
        private static bool _softGlobAtlasCreationFailed;

        public static Texture2D Ember => _ember ??= Load("SariaMod/Items/Ruby/RovaEmber_AI_PLACEHOLDER_v1");

        public static bool TryGetSoftGlobFrame(int radius, out Texture2D texture, out Rectangle source)
        {
            texture = null;
            source = Rectangle.Empty;
            if (Main.dedServ || radius < 1 || radius > MaximumSoftGlobRadius)
                return false;

            if (_softGlobAtlas?.IsDisposed == true)
            {
                _softGlobAtlas = null;
                _softGlobAtlasCreationFailed = false;
            }

            if (_softGlobAtlas == null && !_softGlobAtlasCreationFailed)
            {
                _softGlobAtlas = CreateSoftGlobAtlas();
                _softGlobAtlasCreationFailed = _softGlobAtlas == null;
            }

            if (_softGlobAtlas == null)
                return false;

            texture = _softGlobAtlas;
            int frameIndex = radius - 1;
            source = new Rectangle(
                frameIndex % SoftGlobAtlasColumns * SoftGlobFrameSize,
                frameIndex / SoftGlobAtlasColumns * SoftGlobFrameSize,
                SoftGlobFrameSize,
                SoftGlobFrameSize);
            return true;
        }

        public static void Unload()
        {
            _ember = null;
            _softGlobAtlas?.Dispose();
            _softGlobAtlas = null;
            _softGlobAtlasCreationFailed = false;
        }

        private static Texture2D CreateSoftGlobAtlas()
        {
            try
            {
                int width = SoftGlobFrameSize * SoftGlobAtlasColumns;
                int rowCount = (MaximumSoftGlobRadius + SoftGlobAtlasColumns - 1)
                    / SoftGlobAtlasColumns;
                int height = SoftGlobFrameSize * rowCount;
                Color[] pixels = new Color[width * height];
                int center = MaximumSoftGlobRadius;

                for (int radius = 1; radius <= MaximumSoftGlobRadius; radius++)
                {
                    int frameIndex = radius - 1;
                    int frameLeft = frameIndex % SoftGlobAtlasColumns * SoftGlobFrameSize;
                    int frameTop = frameIndex / SoftGlobAtlasColumns * SoftGlobFrameSize;
                    for (int y = -radius; y <= radius; y++)
                    {
                        float normalizedY = y / (float)radius;
                        float halfWidth = (float)Math.Sqrt(
                            Math.Max(0f, 1f - normalizedY * normalizedY)) * radius;
                        if (halfWidth < 0.5f)
                            continue;

                        int row = frameTop + center + y;
                        int left = frameLeft + (int)(center - halfWidth);
                        int runWidth = Math.Max(1, (int)(halfWidth * 2f));
                        Color value = Color.White * (1f - normalizedY * normalizedY);
                        for (int x = 0; x < runWidth; x++)
                            pixels[row * width + left + x] = value;
                    }
                }

                Texture2D atlas = new Texture2D(Main.graphics.GraphicsDevice, width, height);
                atlas.SetData(pixels);
                return atlas;
            }
            catch
            {
                return null;
            }
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
