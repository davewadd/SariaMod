using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.Strange
{
    public class PsychicJumpPlatformProjectile : ModProjectile
    {
        public const float BackHalf = 0f;
        public const float FrontHalf = 1f;
        public const int PlaySoundFlag = 1;
        public const int DrawAroundProjectileFlag = 2;

        private const int Lifetime = 28;
        private const int HalfRingSegments = 18;

        private bool IsFrontHalf => Projectile.ai[0] == FrontHalf;
        private float GravityDirection => Projectile.ai[1] < 0f ? -1f : 1f;
        private int EffectFlags => Math.Max(0, (int)Math.Abs(Projectile.ai[1]) - 1);
        private bool ShouldPlaySound => (EffectFlags & PlaySoundFlag) != 0;
        private bool DrawAroundProjectile => (EffectFlags & DrawAroundProjectileFlag) != 0;

        public override string Texture => "SariaMod/Items/Strange/Ztarget2";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Psychic Jump Platform");
        }

        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = Lifetime;
            Projectile.hide = true;
        }

        public override bool? CanDamage() => false;
        public override bool? CanCutTiles() => false;

        public static void SpawnPair(
            IEntitySource source,
            Vector2 position,
            int owner,
            float gravityDirection,
            int effectFlags)
        {
            SpawnHalf(source, position, owner, gravityDirection, BackHalf, effectFlags);
            SpawnHalf(source, position, owner, gravityDirection, FrontHalf, effectFlags);
        }

        private static void SpawnHalf(
            IEntitySource source,
            Vector2 position,
            int owner,
            float gravityDirection,
            float half,
            int effectFlags)
        {
            float encodedOptions = (effectFlags + 1) * (gravityDirection < 0f ? -1f : 1f);
            Terraria.Projectile.NewProjectile(
                source,
                position,
                Vector2.Zero,
                ModContent.ProjectileType<PsychicJumpPlatformProjectile>(),
                0,
                0f,
                owner,
                half,
                encodedOptions);
        }

        public override void AI()
        {
            Projectile.velocity = Vector2.Zero;

            if (IsFrontHalf && ShouldPlaySound && Projectile.localAI[0] == 0f && !Main.dedServ)
            {
                SoundEngine.PlaySound(
                    SoundID.Item4 with
                    {
                        Volume = 0.32f,
                        Pitch = 0.55f,
                        PitchVariance = 0.1f,
                        MaxInstances = 3
                    },
                    Projectile.Center);
            }

            Projectile.localAI[0]++;
            if (IsFrontHalf && TryGetRingState(0f, 1f, out Vector2 centerOffset, out float radiusX, out _, out float opacity))
            {
                Vector2 ringCenter = Projectile.Center + centerOffset;
                float lightIntensity = DrawAroundProjectile ? 1.15f : 0.85f;
                Vector3 light = Color.HotPink.ToVector3() * (lightIntensity * opacity);
                Lighting.AddLight(ringCenter, light);
                Lighting.AddLight(ringCenter + Vector2.UnitX * radiusX * 0.65f, light * 0.5f);
                Lighting.AddLight(ringCenter - Vector2.UnitX * radiusX * 0.65f, light * 0.5f);
            }
        }

        public override void DrawBehind(
            int index,
            List<int> behindNPCsAndTiles,
            List<int> behindNPCs,
            List<int> behindProjectiles,
            List<int> overPlayers,
            List<int> overWiresUI)
        {
            if (IsFrontHalf)
            {
                if (DrawAroundProjectile)
                {
                    overWiresUI.Add(index);
                }
                else
                {
                    overPlayers.Add(index);
                }
            }
            else
            {
                behindProjectiles.Add(index);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            DrawRing(pixel, 0f, 1f);
            DrawRing(pixel, 0.12f, 0.65f);
            DrawRing(pixel, 0.25f, 0.42f);
            return false;
        }

        private void DrawRing(Texture2D pixel, float delay, float scale)
        {
            if (!TryGetRingState(delay, scale, out Vector2 centerOffset, out float radiusX, out float radiusY, out float opacity))
            {
                return;
            }

            Vector2 center = Projectile.Center + centerOffset - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
            if (DrawAroundProjectile)
            {
                Color brightCore = Color.Lerp(Color.HotPink, Color.White, 0.35f);
                DrawEllipseHalf(pixel, center, radiusX, radiusY, Color.DeepPink * (0.72f * opacity), 6f);
                DrawEllipseHalf(pixel, center, radiusX, radiusY, Color.HotPink * opacity, 3.2f);
                DrawEllipseHalf(pixel, center, radiusX, radiusY, brightCore * opacity, 1.2f);
            }
            else
            {
                DrawEllipseHalf(pixel, center, radiusX, radiusY, Color.DarkMagenta * (0.58f * opacity), 6f);
                DrawEllipseHalf(pixel, center, radiusX, radiusY, Color.DeepPink * (0.88f * opacity), 3.2f);
                DrawEllipseHalf(pixel, center, radiusX, radiusY, Color.HotPink * opacity, 1.2f);
            }
        }

        private bool TryGetRingState(
            float delay,
            float scale,
            out Vector2 centerOffset,
            out float radiusX,
            out float radiusY,
            out float opacity)
        {
            float totalProgress = MathHelper.Clamp((Lifetime - Projectile.timeLeft) / (float)Lifetime, 0f, 1f);
            if (totalProgress < delay)
            {
                centerOffset = Vector2.Zero;
                radiusX = 0f;
                radiusY = 0f;
                opacity = 0f;
                return false;
            }

            float progress = MathHelper.Clamp((totalProgress - delay) / (1f - delay), 0f, 1f);
            float easedProgress = 1f - (float)Math.Pow(1f - progress, 3f);
            radiusX = MathHelper.Lerp(5f, 42f, easedProgress) * scale;
            radiusY = MathHelper.Lerp(2f, 8f, easedProgress) * MathHelper.Lerp(0.8f, 1f, scale);
            float travelDistance = DrawAroundProjectile
                ? 18f + 6f * (1f - scale)
                : 30f + 10f * (1f - scale);
            centerOffset = Vector2.UnitY * GravityDirection * MathHelper.Lerp(0f, travelDistance, easedProgress);

            float fadeIn = MathHelper.Clamp(progress / 0.1f, 0f, 1f);
            float fadeOut = 1f - MathHelper.SmoothStep(0.42f, 1f, progress);
            opacity = fadeIn * fadeOut * MathHelper.Lerp(0.58f, 1f, scale);
            return opacity > 0f;
        }

        private void DrawEllipseHalf(
            Texture2D pixel,
            Vector2 center,
            float radiusX,
            float radiusY,
            Color color,
            float width)
        {
            float startAngle = IsFrontHalf ? 0f : MathHelper.Pi;
            Vector2 previous = center + new Vector2(
                (float)Math.Cos(startAngle) * radiusX,
                (float)Math.Sin(startAngle) * radiusY);

            for (int i = 1; i <= HalfRingSegments; i++)
            {
                float angle = startAngle + MathHelper.Pi * i / HalfRingSegments;
                Vector2 next = center + new Vector2(
                    (float)Math.Cos(angle) * radiusX,
                    (float)Math.Sin(angle) * radiusY);
                DrawLine(pixel, previous, next, color, width);
                previous = next;
            }
        }

        private static void DrawLine(
            Texture2D pixel,
            Vector2 start,
            Vector2 end,
            Color color,
            float width)
        {
            Vector2 difference = end - start;
            float length = difference.Length();
            if (length <= 0.001f)
            {
                return;
            }

            Main.spriteBatch.Draw(
                pixel,
                (start + end) * 0.5f,
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
