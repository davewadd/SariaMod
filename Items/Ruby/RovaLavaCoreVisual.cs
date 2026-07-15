using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace SariaMod.Items.Ruby
{
    /// <summary>
    /// Procedural lava body for RovaCenter. The surface is sampled as a shaded
    /// sphere with animated, domain-warped lava bands so it has real depth
    /// without relying on a texture.
    /// </summary>
    internal static class RovaLavaCoreVisual
    {
        private const float CoreRadius = 30f;
        private static readonly Dictionary<int, LavaCoreState> States = new Dictionary<int, LavaCoreState>();

        private sealed class LavaCoreState
        {
            public int Age;
            public float Phase;
        }

        public static void Update(Projectile projectile)
        {
            if (Main.dedServ)
                return;

            LavaCoreState state = GetState(projectile.whoAmI);
            state.Age++;
            state.Phase += 0.06f;

            float sunlightPulse = 0.88f + (float)Math.Sin(state.Age * 0.055f) * 0.12f;
            Lighting.AddLight(
                projectile.Center,
                new Vector3(1f, 0.58f, 0.12f) * (3.35f * sunlightPulse));
        }

        public static void Draw(Projectile projectile)
        {
            if (Main.dedServ)
                return;

            LavaCoreState state = GetState(projectile.whoAmI);
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Vector2 center = projectile.Center - Main.screenPosition;
            float goldenPulse = 0.5f + 0.5f * (float)Math.Sin(state.Age * 0.055f);

            DrawAnimatedSeekerCore(center, state.Age, state.Phase, goldenPulse);
            DrawFlowingSpiralRibbons(pixel, center, state.Phase, goldenPulse);
            float furnaceRadius = MathHelper.Lerp(5.4f, 8.2f, goldenPulse);
            RovaLavaGlobVisual.DrawSoftGlob(
                pixel,
                center,
                furnaceRadius,
                new Color(255, 211, 42) * MathHelper.Lerp(0.74f, 0.96f, goldenPulse));
            RovaLavaGlobVisual.DrawSoftGlob(
                pixel,
                center,
                furnaceRadius * 0.52f,
                new Color(255, 252, 196) * MathHelper.Lerp(0.78f, 1f, goldenPulse));
        }

        public static void Remove(int projectileIndex)
        {
            States.Remove(projectileIndex);
        }

        private static void DrawAnimatedSeekerCore(
            Vector2 center,
            int age,
            float flowTime,
            float goldenPulse)
        {
            const int frameCount = 8;
            const int frameDuration = 3;
            Texture2D texture = TextureAssets.Projectile[ModContent.ProjectileType<RubyPsychicSeeker>()].Value;
            int frameHeight = texture.Height / frameCount;
            int frame = age / frameDuration % frameCount;
            Rectangle source = new Rectangle(0, frame * frameHeight, texture.Width, frameHeight);
            Vector2 origin = new Vector2(texture.Width * 0.5f, frameHeight * 0.5f);
            float scale = MathHelper.Lerp(1.78f, 1.9f, goldenPulse);
            float rotation = flowTime * 0.62f;

            // Four quarter-turned samples average the deliberately lopsided
            // seeker artwork into a stable circular orb while retaining its
            // hand-drawn lava texture and animated edge.
            for (int sample = 0; sample < 4; sample++)
            {
                float sampleRotation = rotation + sample * MathHelper.PiOver2;
                Main.spriteBatch.Draw(
                    texture,
                    center,
                    source,
                    new Color(255, 92, 8, 88) * 0.34f,
                    sampleRotation,
                    origin,
                    scale * 1.1f,
                    SpriteEffects.None,
                    0f);

                Main.spriteBatch.Draw(
                    texture,
                    center,
                    source,
                    Color.White * 0.42f,
                    sampleRotation,
                    origin,
                    scale,
                    SpriteEffects.None,
                    0f);
            }
        }

        private static void DrawFlowingSpiralRibbons(
            Texture2D pixel,
            Vector2 center,
            float flowTime,
            float goldenPulse)
        {
            const int armCount = 3;
            const int segmentCount = 48;
            float outerRadius = CoreRadius * 0.79f;
            float innerRadius = 3.5f;

            for (int arm = 0; arm < armCount; arm++)
            {
                float armOffset = arm * MathHelper.TwoPi / armCount;
                for (int segment = 0; segment < segmentCount; segment++)
                {
                    float progressA = segment / (float)segmentCount;
                    float progressB = (segment + 1f) / segmentCount;
                    float dash = 0.5f + 0.5f * (float)Math.Sin(
                        (progressA - flowTime * 0.2f) * MathHelper.TwoPi * 4f + arm * 0.8f);
                    if (dash < 0.28f)
                        continue;

                    float angleA = armOffset
                        + progressA * 4.9f
                        + flowTime * 1.18f
                        + (float)Math.Sin(progressA * 8f + flowTime) * 0.08f;
                    float angleB = armOffset
                        + progressB * 4.9f
                        + flowTime * 1.18f
                        + (float)Math.Sin(progressB * 8f + flowTime) * 0.08f;
                    float radiusA = MathHelper.Lerp(outerRadius, innerRadius, progressA);
                    float radiusB = MathHelper.Lerp(outerRadius, innerRadius, progressB);
                    Vector2 pointA = center + angleA.ToRotationVector2() * radiusA;
                    Vector2 pointB = center + angleB.ToRotationVector2() * radiusB;
                    Color color = LerpColor(
                        new Color(255, 92, 3),
                        new Color(255, 238, 92),
                        MathHelper.Clamp(progressA * 1.12f, 0f, 1f));
                    float alpha = MathHelper.Lerp(0.28f, 0.72f, dash)
                        * MathHelper.Lerp(0.82f, 1f, goldenPulse);
                    float width = MathHelper.Lerp(2.2f, 4.1f, progressA);

                    DrawRibbonSegment(pixel, pointA, pointB, width, color * alpha);
                    if (progressA > 0.58f)
                    {
                        DrawRibbonSegment(
                            pixel,
                            pointA,
                            pointB,
                            width * 0.38f,
                            new Color(255, 249, 177) * (alpha * 0.72f));
                    }
                }
            }
        }

        private static void DrawRibbonSegment(
            Texture2D pixel,
            Vector2 start,
            Vector2 end,
            float width,
            Color color)
        {
            Vector2 segment = end - start;
            if (segment.LengthSquared() < 0.1f)
                return;

            Main.spriteBatch.Draw(
                pixel,
                start,
                new Rectangle(0, 0, 1, 1),
                color,
                segment.ToRotation(),
                new Vector2(0f, 0.5f),
                new Vector2(segment.Length(), width),
                SpriteEffects.None,
                0f);
        }

        private static Color LerpColor(Color from, Color to, float amount)
        {
            amount = MathHelper.Clamp(amount, 0f, 1f);
            return new Color(
                (byte)MathHelper.Lerp(from.R, to.R, amount),
                (byte)MathHelper.Lerp(from.G, to.G, amount),
                (byte)MathHelper.Lerp(from.B, to.B, amount),
                255);
        }

        private static LavaCoreState GetState(int projectileIndex)
        {
            if (!States.TryGetValue(projectileIndex, out LavaCoreState state))
            {
                state = new LavaCoreState();
                States[projectileIndex] = state;
            }

            return state;
        }
    }
}
