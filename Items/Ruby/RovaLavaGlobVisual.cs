using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;

namespace SariaMod.Items.Ruby
{
    public struct RovaLavaGlob
    {
        public Vector2 Anchor;
        public Vector2 AnchorVelocity;
        public Vector2 Gravity;
        public float Angle;
        public float Distance;
        public float RadialSpeed;
        public float AngularSpeed;
        public float Age;
        public float Life;
        public float MaxSize;
        public Vector2[] Trail;
        public int TrailIndex;
        public float BeamDistance;
        public float BeamSpeed;
        public float BeamSideOffset;
        public float BeamSideAmplitude;
        public float BeamSidePhase;
        public Color Color;
    }

    public static class RovaLavaGlobVisual
    {
        public static void SpawnInward(
            List<RovaLavaGlob> globs,
            Vector2 anchor,
            int count,
            float outerRadius,
            float innerRadius,
            float life,
            float minSize = 4f,
            float maxSize = 10f,
            bool useFireUpgrade2Palette = false,
            Color? defaultOuterColor = null)
        {
            float travelDistance = Math.Max(1f, outerRadius - innerRadius);
            for (int i = 0; i < count; i++)
            {
                float globLife = life * Main.rand.NextFloat(0.8f, 1.2f);
                RovaLavaGlob glob = new RovaLavaGlob
                {
                    Anchor = anchor,
                    AnchorVelocity = Vector2.Zero,
                    Angle = Main.rand.NextFloat(MathHelper.TwoPi),
                    Distance = outerRadius + Main.rand.NextFloat(-8f, 8f),
                    RadialSpeed = -(travelDistance / globLife) * Main.rand.NextFloat(0.85f, 1.15f),
                    AngularSpeed = Main.rand.NextFloat(0.035f, 0.085f),
                    Age = 0f,
                    Life = globLife,
                    MaxSize = Main.rand.NextFloat(minSize, maxSize),
                    Color = useFireUpgrade2Palette
                        ? RollFireUpgrade2Color(defaultOuterColor ?? new Color(255, 66, 10, 225))
                        : default
                };
                InitializeTrail(ref glob);
                globs.Add(glob);
            }
        }

        public static void SpawnOutward(
            List<RovaLavaGlob> globs,
            Vector2 anchor,
            int count,
            float radius,
            float minSpeed,
            float maxSpeed,
            float minLife,
            float maxLife,
            float minSize = 4f,
            float maxSize = 10f)
        {
            for (int i = 0; i < count; i++)
            {
                RovaLavaGlob glob = new RovaLavaGlob
                {
                    Anchor = anchor,
                    AnchorVelocity = Vector2.Zero,
                    Angle = i * MathHelper.TwoPi / Math.Max(1, count) + Main.rand.NextFloat(-0.08f, 0.08f),
                    Distance = radius + Main.rand.NextFloat(-3f, 3f),
                    RadialSpeed = Main.rand.NextFloat(minSpeed, maxSpeed),
                    AngularSpeed = Main.rand.NextFloat(0.025f, 0.075f),
                    Age = 0f,
                    Life = Main.rand.NextFloat(minLife, maxLife),
                    MaxSize = Main.rand.NextFloat(minSize, maxSize)
                };
                InitializeTrail(ref glob);
                globs.Add(glob);
            }
        }

        public static void SpawnAlongBeam(
            List<RovaLavaGlob> globs,
            Vector2 beamStart,
            Vector2 beamDirection,
            float beamLength,
            int count = 1,
            float minimumBeamDistance = 0f,
            bool useFireUpgrade2Palette = false,
            Color? defaultOuterColor = null)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 direction = beamDirection.SafeNormalize(Vector2.UnitX);
                float speed = Main.rand.NextFloat(8f, 12f);
                float beamDistance = minimumBeamDistance + Main.rand.NextFloat(0f, 18f);
                Vector2 anchor = beamStart + direction * beamDistance;
                float sideOffset = Main.rand.NextFloat(-7f, 7f);

                RovaLavaGlob glob = new RovaLavaGlob
                {
                    Anchor = anchor,
                    AnchorVelocity = Vector2.Zero,
                    Angle = Main.rand.NextFloat(MathHelper.TwoPi),
                    Distance = Main.rand.NextFloat(0f, 3f),
                    RadialSpeed = Main.rand.NextFloat(0.02f, 0.12f),
                    AngularSpeed = Main.rand.NextFloat(0.06f, 0.14f),
                    Age = 0f,
                    Life = MathHelper.Clamp(beamLength / speed, 24f, 100f),
                    MaxSize = Main.rand.NextFloat(2.5f, 5.5f),
                    BeamDistance = beamDistance,
                    BeamSpeed = speed,
                    BeamSideOffset = sideOffset,
                    BeamSideAmplitude = Main.rand.NextFloat(4f, 10f),
                    BeamSidePhase = Main.rand.NextFloat(MathHelper.TwoPi),
                    Color = useFireUpgrade2Palette
                        ? RollFireUpgrade2Color(defaultOuterColor ?? new Color(255, 66, 8, 220))
                        : default
                };
                InitializeTrail(ref glob);
                globs.Add(glob);
            }
        }

        private static Color RollFireUpgrade2Color(Color orangeColor)
        {
            int roll = Main.rand.Next(5);
            if (roll == 0)
                return new Color(255, 229, 66, orangeColor.A);
            if (roll == 1)
                return new Color(150, 18, 6, orangeColor.A);

            return orangeColor;
        }

        public static void Update(List<RovaLavaGlob> globs)
        {
            for (int i = globs.Count - 1; i >= 0; i--)
            {
                RovaLavaGlob glob = globs[i];
                if (glob.Trail == null)
                    InitializeTrail(ref glob);

                glob.Age++;
                glob.Anchor += glob.AnchorVelocity;
                glob.Distance += glob.RadialSpeed;
                glob.Angle += glob.AngularSpeed;
                glob.AnchorVelocity += glob.Gravity;
                glob.AnchorVelocity *= 0.985f;

                Vector2 currentPosition = GetWorldPosition(glob);
                glob.Trail[glob.TrailIndex] = currentPosition;
                glob.TrailIndex = (glob.TrailIndex + 1) % glob.Trail.Length;

                if (glob.Age >= glob.Life || glob.Distance <= 1f)
                {
                    globs.RemoveAt(i);
                    continue;
                }

                globs[i] = glob;
            }
        }

        public static void UpdateAlongBeam(
            List<RovaLavaGlob> globs,
            Vector2 beamStart,
            Vector2 beamDirection,
            float beamLength)
        {
            Vector2 direction = beamDirection.SafeNormalize(Vector2.UnitX);
            Vector2 sideways = direction.RotatedBy(MathHelper.PiOver2);

            for (int i = globs.Count - 1; i >= 0; i--)
            {
                RovaLavaGlob glob = globs[i];
                if (glob.Trail == null)
                    InitializeTrail(ref glob);

                glob.Age++;
                glob.BeamDistance += glob.BeamSpeed;
                glob.Angle += glob.AngularSpeed;
                float sideMotion = glob.BeamSideOffset
                    + (float)Math.Sin(glob.Age * 0.16f + glob.BeamSidePhase) * glob.BeamSideAmplitude;
                glob.Anchor = beamStart + direction * glob.BeamDistance + sideways * sideMotion;

                Vector2 currentPosition = GetWorldPosition(glob);
                glob.Trail[glob.TrailIndex] = currentPosition;
                glob.TrailIndex = (glob.TrailIndex + 1) % glob.Trail.Length;

                if (glob.Age >= glob.Life || glob.BeamDistance >= beamLength)
                {
                    globs.RemoveAt(i);
                    continue;
                }

                globs[i] = glob;
            }
        }

        public static void Draw(
            List<RovaLavaGlob> globs,
            Vector2 screenPosition,
            Color outerColor,
            Color innerColor,
            float alpha = 1f)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            foreach (RovaLavaGlob glob in globs)
            {
                float progress = MathHelper.Clamp(glob.Age / glob.Life, 0f, 1f);
                float envelope = 4f * progress * (1f - progress);
                float size = glob.MaxSize * envelope;
                float drawAlpha = envelope * alpha;
                Vector2 position = glob.Anchor - screenPosition + glob.Angle.ToRotationVector2() * glob.Distance;

                DrawTrail(glob, screenPosition, outerColor, drawAlpha);

                DrawSoftGlob(pixel, position, size, outerColor * drawAlpha);
                DrawSoftGlob(pixel, position, size * 0.48f, innerColor * drawAlpha);
            }
        }

        public static void DrawStoredColors(
            List<RovaLavaGlob> globs,
            Vector2 screenPosition,
            Color innerColor,
            float alpha = 1f)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            foreach (RovaLavaGlob glob in globs)
            {
                float progress = MathHelper.Clamp(glob.Age / glob.Life, 0f, 1f);
                float envelope = 4f * progress * (1f - progress);
                float size = glob.MaxSize * envelope;
                float drawAlpha = envelope * alpha;
                Vector2 position = glob.Anchor - screenPosition + glob.Angle.ToRotationVector2() * glob.Distance;
                Color outerColor = glob.Color.A > 0 ? glob.Color : new Color(255, 229, 66);

                DrawTrail(glob, screenPosition, outerColor, drawAlpha);
                DrawSoftGlob(pixel, position, size, outerColor * drawAlpha);
                DrawSoftGlob(pixel, position, size * 0.42f, innerColor * (drawAlpha * 0.88f));
            }
        }

        private static void InitializeTrail(ref RovaLavaGlob glob)
        {
            const int trailLength = 12;
            glob.Trail = new Vector2[trailLength];
            Vector2 position = GetWorldPosition(glob);
            for (int i = 0; i < trailLength; i++)
                glob.Trail[i] = position;

            glob.TrailIndex = 0;
        }

        private static Vector2 GetWorldPosition(RovaLavaGlob glob)
        {
            return glob.Anchor + glob.Angle.ToRotationVector2() * glob.Distance;
        }

        private static void DrawTrail(RovaLavaGlob glob, Vector2 screenPosition, Color color, float alpha)
        {
            if (glob.Trail == null || glob.Trail.Length < 2)
                return;

            Texture2D pixel = TextureAssets.MagicPixel.Value;
            for (int i = 0; i < glob.Trail.Length - 1; i++)
            {
                int firstIndex = (glob.TrailIndex + i) % glob.Trail.Length;
                int secondIndex = (firstIndex + 1) % glob.Trail.Length;
                Vector2 start = glob.Trail[firstIndex] - screenPosition;
                Vector2 end = glob.Trail[secondIndex] - screenPosition;
                Vector2 segment = end - start;
                if (segment.LengthSquared() < 0.25f)
                    continue;

                float progress = (i + 1f) / (glob.Trail.Length - 1f);
                float width = MathHelper.Lerp(0.7f, 4.5f, progress);
                float trailAlpha = MathHelper.SmoothStep(0f, 1f, progress) * alpha * 0.7f;
                Main.spriteBatch.Draw(
                    pixel,
                    start,
                    new Rectangle(0, 0, 1, 1),
                    color * trailAlpha,
                    segment.ToRotation(),
                    new Vector2(0f, 0.5f),
                    new Vector2(segment.Length(), width),
                    SpriteEffects.None,
                    0f);
            }
        }

        public static void DrawSoftGlob(Texture2D pixel, Vector2 position, float radius, Color color)
        {
            if (radius < 0.5f || color.A <= 1)
                return;

            int r = Math.Max(1, (int)Math.Ceiling(radius));
            for (int y = -r; y <= r; y++)
            {
                float normalizedY = y / (float)r;
                float halfWidth = (float)Math.Sqrt(Math.Max(0f, 1f - normalizedY * normalizedY)) * r;
                if (halfWidth < 0.5f)
                    continue;

                Main.spriteBatch.Draw(
                    pixel,
                    new Rectangle(
                        (int)(position.X - halfWidth),
                        (int)(position.Y + y),
                        Math.Max(1, (int)(halfWidth * 2f)),
                        1),
                    null,
                    color * (1f - normalizedY * normalizedY));
            }
        }
    }
}
