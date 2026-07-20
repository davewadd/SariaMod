using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;

namespace SariaMod.Items.Strange
{
    internal sealed class LocatorLightShapeVisuals
    {
        private const int MaxTriangles = 32;

        private sealed class LightTriangle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Rotation;
            public float Size;
            public float Age;
            public float Lifetime;
            public Color InnerColor;
        }

        private readonly List<LightTriangle> triangles = new List<LightTriangle>();

        public void SpawnTriangle(Vector2 position, Vector2 velocity, float rotation, float size, Color innerColor)
        {
            if (triangles.Count >= MaxTriangles)
            {
                triangles.RemoveAt(0);
            }

            triangles.Add(new LightTriangle
            {
                Position = position,
                Velocity = velocity,
                Rotation = rotation,
                Size = size,
                Age = 0f,
                Lifetime = 90f,
                InnerColor = innerColor
            });
        }

        public static void SpawnSmallDustTriangle(Vector2 position, Vector2 velocity, float rotation, float size, Color color)
        {
            const int dustsPerTriangle = 9;
            if (!VisualDustLimiter.TryReserveHalfCapacitySlots(dustsPerTriangle))
            {
                return;
            }

            Vector2 point1 = new Vector2(size, 0f).RotatedBy(rotation);
            Vector2 point2 = new Vector2(size, 0f).RotatedBy(rotation + MathHelper.TwoPi / 3f);
            Vector2 point3 = new Vector2(size, 0f).RotatedBy(rotation + MathHelper.TwoPi * 2f / 3f);

            for (int edge = 0; edge < 3; edge++)
            {
                float progress = edge / 3f;
                SpawnTriangleDust(position + Vector2.Lerp(point1, point2, progress), velocity, color);
                SpawnTriangleDust(position + Vector2.Lerp(point2, point3, progress), velocity, color);
                SpawnTriangleDust(position + Vector2.Lerp(point3, point1, progress), velocity, color);
            }
        }

        private static void SpawnTriangleDust(Vector2 position, Vector2 velocity, Color color)
        {
            Dust dust = Dust.NewDustPerfect(position, 267, velocity, 0, color, 0.8f);
            dust.noGravity = true;
            dust.fadeIn = 1f;
        }

        public void Update()
        {
            for (int i = triangles.Count - 1; i >= 0; i--)
            {
                LightTriangle triangle = triangles[i];
                triangle.Age++;
                triangle.Position += triangle.Velocity;
                triangle.Velocity *= 0.96f;
                triangle.Rotation += 0.025f;

                if (triangle.Age >= triangle.Lifetime)
                {
                    triangles.RemoveAt(i);
                }
            }
        }

        public void Draw(Projectile projectile, Texture2D pixelTexture)
        {
            DrawTriangles(projectile, pixelTexture);
        }

        private void DrawTriangles(Projectile projectile, Texture2D pixelTexture)
        {
            foreach (LightTriangle triangle in triangles)
            {
                float ageProgress = MathHelper.Clamp(triangle.Age / triangle.Lifetime, 0f, 1f);
                float opacity = (1f - ageProgress) * MathHelper.Clamp(triangle.Age / 3f, 0f, 1f);
                float size = triangle.Size * MathHelper.Lerp(0.9f, 1.12f, ageProgress);

                Vector2 center = triangle.Position - Main.screenPosition + new Vector2(0f, projectile.gfxOffY);
                Vector2 p1 = center + new Vector2(size, 0f).RotatedBy(triangle.Rotation);
                Vector2 p2 = center + new Vector2(size, 0f).RotatedBy(triangle.Rotation + MathHelper.TwoPi / 3f);
                Vector2 p3 = center + new Vector2(size, 0f).RotatedBy(triangle.Rotation + MathHelper.TwoPi * 2f / 3f);

                Color outline = Color.Lerp(triangle.InnerColor, Color.Black, 0.62f) * opacity;
                Color inner = triangle.InnerColor * opacity;
                float outlineWidth = MathHelper.Clamp(size * 0.28f, 2.5f, 5f);
                float innerWidth = MathHelper.Clamp(outlineWidth * 0.48f, 1.2f, 2.4f);

                DrawLine(p1, p2, outline, outlineWidth, pixelTexture);
                DrawLine(p2, p3, outline, outlineWidth, pixelTexture);
                DrawLine(p3, p1, outline, outlineWidth, pixelTexture);
                DrawLine(p1, p2, inner, innerWidth, pixelTexture);
                DrawLine(p2, p3, inner, innerWidth, pixelTexture);
                DrawLine(p3, p1, inner, innerWidth, pixelTexture);

                Lighting.AddLight(triangle.Position, triangle.InnerColor.ToVector3() * (0.45f * opacity));
            }
        }

        private static void DrawLine(Vector2 a, Vector2 b, Color color, float width, Texture2D texture)
        {
            Vector2 difference = b - a;
            float length = difference.Length();
            if (length <= 0.001f)
            {
                return;
            }

            Main.spriteBatch.Draw(
                texture,
                (a + b) * 0.5f,
                new Rectangle(0, 0, 1, 1),
                color,
                difference.ToRotation(),
                new Vector2(0.5f, 0.5f),
                new Vector2(length, width),
                SpriteEffects.None,
                0f);
        }
    }
}
