using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using System;

namespace SariaMod.Effects
{
    /// <summary>
    /// Reusable charge dust effect utilities.
    /// Creates a ring of dust particles that fade from one color to another over time.
    /// </summary>
    public static class ChargeDust
    {
        /// <summary>
        /// Spawns a ring of dust particles around a position that fade from blue to red.
        /// </summary>
        /// <param name="center">Center position of the ring</param>
        /// <param name="currentFrame">Current frame/timer value</param>
        /// <param name="transitionStartFrame">Frame when color transition begins</param>
        /// <param name="transitionEndFrame">Frame when color transition is complete</param>
        /// <param name="orbCount">Number of orbs in the ring (default 8)</param>
        /// <param name="rotationSpeed">How fast the ring rotates (default 0.05f)</param>
        public static void SpawnChargeRing(
            Vector2 center,
            int currentFrame,
            int transitionStartFrame,
            int transitionEndFrame,
            int orbCount = 8,
            float rotationSpeed = 0.05f)
        {
            SpawnChargeRing(
                center,
                currentFrame,
                transitionStartFrame,
                transitionEndFrame,
                new Color(50, 150, 255),  // Default blue
                new Color(255, 100, 50),   // Default red
                0.8f, 1.5f,                // Scale range
                30f, 40f,                  // Radius range
                orbCount,
                rotationSpeed
            );
        }

        /// <summary>
        /// Spawns a ring of dust particles around a position with full customization.
        /// </summary>
        public static void SpawnChargeRing(
            Vector2 center,
            int currentFrame,
            int transitionStartFrame,
            int transitionEndFrame,
            Color startColor,
            Color endColor,
            float startScale,
            float endScale,
            float startRadius,
            float endRadius,
            int orbCount = 8,
            float rotationSpeed = 0.05f)
        {
            // Calculate transition progress
            float transitionProgress = 0f;

            if (currentFrame >= transitionEndFrame)
            {
                transitionProgress = 1f;
            }
            else if (currentFrame > transitionStartFrame)
            {
                transitionProgress = (float)(currentFrame - transitionStartFrame) / (transitionEndFrame - transitionStartFrame);
            }

            // Interpolate values based on progress
            float dustScale = MathHelper.Lerp(startScale, endScale, transitionProgress);
            float ringRadius = MathHelper.Lerp(startRadius, endRadius, transitionProgress);
            Color currentColor = Color.Lerp(startColor, endColor, transitionProgress);

            // Spawn orbs in a ring
            for (int i = 0; i < orbCount; i++)
            {
                float angle = MathHelper.TwoPi * i / orbCount + (currentFrame * rotationSpeed);
                Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * ringRadius;
                Vector2 dustPos = center + offset;

                Dust d = Dust.NewDustPerfect(dustPos, DustID.FireworksRGB, Vector2.Zero, 0, currentColor, dustScale);
                d.noGravity = true;
                d.noLight = false;
            }
        }

        /// <summary>
        /// Spawns a burst of dust particles expanding outward from a center point.
        /// </summary>
        public static void SpawnChargeBurst(
            Vector2 center,
            Color color,
            int particleCount = 16,
            float speed = 4f,
            float scale = 1.8f)
        {
            for (int i = 0; i < particleCount; i++)
            {
                float angle = MathHelper.TwoPi * i / particleCount;
                Vector2 dustVel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * speed;
                Dust burst = Dust.NewDustPerfect(center, DustID.FireworksRGB, dustVel, 0, color, scale);
                burst.noGravity = true;
            }
        }

        /// <summary>
        /// Gets the interpolated color for a charge effect at a given frame.
        /// </summary>
        public static Color GetChargeColor(
            int currentFrame,
            int transitionStartFrame,
            int transitionEndFrame,
            Color startColor,
            Color endColor)
        {
            float progress = 0f;

            if (currentFrame >= transitionEndFrame)
            {
                progress = 1f;
            }
            else if (currentFrame > transitionStartFrame)
            {
                progress = (float)(currentFrame - transitionStartFrame) / (transitionEndFrame - transitionStartFrame);
            }

            return Color.Lerp(startColor, endColor, progress);
        }

        /// <summary>
        /// Default blue color for charge effects
        /// </summary>
        public static Color DefaultBlue => new Color(50, 150, 255);

        /// <summary>
        /// Default red color for charge effects
        /// </summary>
        public static Color DefaultRed => new Color(255, 100, 50);
    }
}
