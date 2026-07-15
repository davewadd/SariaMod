using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SariaMod.Buffs;
using SariaMod.Dusts;
using SariaMod.Items.Strange;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.Ruby
{
    /// <summary>
    /// Visual-only fire ring attached to RovaCenter.
    /// It fades in and spins inward while the first fire charge sound plays,
    /// then expands back out of the core when the second charge sound begins.
    /// </summary>
    public class RovaRing : ModProjectile
    {
        private const int FrameCount = 3;
        private const int FrameDurationTicks = 6;
        private const int SpiralDurationTicks = 120;
        private const int CircleFormationStartTicks = 96;
        private const int CircleFormationDurationTicks = 66;
        private const int ChargeFire2RingDurationTicks = 36;
        private const int SpiralArmCount = 8;
        private const float TargetDiameter = 224f;
        private const float OuterSpiralRadius = 190f;
        private const float InnerCircleRadius = 34f;
        private int AnimationTimer;
        private float RingRotation;
        private bool GlobBurstPlayed;
        private int PostChargeTimer;
        private int ChargeFire2RingTimer = -1;
        private bool FireUpgrade2Active;

        private readonly List<RovaLavaGlob> SpiralGlobs = new List<RovaLavaGlob>();
        private readonly List<RovaLavaGlob> LavaGlobs = new List<RovaLavaGlob>();

        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Saria");
            Main.projFrames[base.Projectile.type] = FrameCount;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(AnimationTimer);
            writer.Write(ChargeFire2RingTimer);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            AnimationTimer = reader.ReadInt32();
            ChargeFire2RingTimer = reader.ReadInt32();
        }

        public override void SetDefaults()
        {
            Projectile.width = 86;
            Projectile.height = 86;
            Projectile.netImportant = true;
            Projectile.alpha = 0;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 2;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.minionSlots = 0f;
        }

        public override bool? CanCutTiles()
        {
            return false;
        }

        public override bool? CanHitNPC(NPC target)
        {
            return false;
        }

        public override bool MinionContactDamage()
        {
            return false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            DrawChargeRing();
            return false;
        }

        public override void AI()
        {
            int centerIndex = (int)Projectile.ai[0];
            if (centerIndex < 0
                || centerIndex >= Main.maxProjectiles
                || !Main.projectile[centerIndex].active
                || Main.projectile[centerIndex].owner != Projectile.owner
                || Main.projectile[centerIndex].ModProjectile is not RovaCenter rovaCenter)
            {
                Projectile.Kill();
                return;
            }

            Projectile center = Main.projectile[centerIndex];
            FireUpgrade2Active = rovaCenter.HasRovaSentryPersistenceUpgrade;
            Projectile.Center = center.Center;
            Projectile.timeLeft = 2;
            AnimationTimer++;

            Projectile.frame = (AnimationTimer / FrameDurationTicks) % FrameCount;

            // The texture itself collapses into the core and fades as it becomes tiny.
            float inwardProgress = SmoothStep(MathHelper.Clamp(AnimationTimer / 150f, 0f, 1f));
            float fadeProgress = MathHelper.Clamp(AnimationTimer / 30f, 0f, 1f);
            RingRotation += MathHelper.Lerp(0.12f, 0.05f, inwardProgress);
            Projectile.rotation = RingRotation;
            Projectile.scale = MathHelper.Lerp(1.35f, 0.04f, inwardProgress);

            if (ChargeFire2RingTimer >= 0
                && ChargeFire2RingTimer < ChargeFire2RingDurationTicks)
            {
                ChargeFire2RingTimer++;
            }

            if (!GlobBurstPlayed && rovaCenter.ChargeFire2StartedValue)
            {
                SpawnLavaGlobBurst();
                GlobBurstPlayed = true;
                PostChargeTimer = 0;
                ChargeFire2RingTimer = 0;
                SpiralGlobs.Clear();
                Projectile.netUpdate = true;
            }

            if (Main.netMode != NetmodeID.Server)
            {
                if (!GlobBurstPlayed)
                {
                    if (SpiralGlobs.Count < 56 && Main.rand.NextBool(2))
                    {
                        RovaLavaGlobVisual.SpawnInward(
                            SpiralGlobs,
                            Projectile.Center,
                            1,
                            OuterSpiralRadius,
                            InnerCircleRadius,
                            104f,
                            3f,
                            8f,
                            useFireUpgrade2Palette: FireUpgrade2Active,
                            defaultOuterColor: new Color(255, 66, 10, 225));
                    }
                }
                else
                {
                    PostChargeTimer++;

                    // Keep a small ambient stream around RovaCenter for the
                    // entire time it exists. The beam gets the denser stream,
                    // while cooldown and charge preparation use only a few.
                    bool beamActive = HasActiveBeam(centerIndex);
                    int globLimit = beamActive ? 10 : 4;
                    bool shouldSpawn = beamActive
                        ? Main.rand.NextBool(7)
                        : Main.rand.NextBool(15);
                    if (SpiralGlobs.Count < globLimit && shouldSpawn)
                    {
                        RovaLavaGlobVisual.SpawnInward(
                            SpiralGlobs,
                            Projectile.Center,
                            1,
                            74f,
                            8f,
                            54f,
                            2.5f,
                            5.5f,
                            useFireUpgrade2Palette: FireUpgrade2Active,
                            defaultOuterColor: new Color(255, 66, 10, 225));
                    }
                }

                RovaLavaGlobVisual.Update(SpiralGlobs);
            }

            RovaLavaGlobVisual.Update(LavaGlobs);

            // Spawn fire particles in a ring
            if (!GlobBurstPlayed && Main.rand.NextBool(3))
            {
                float ringRadius = TargetDiameter * 0.5f * Projectile.scale;
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                Vector2 offset = new Vector2((float)Math.Cos(angle) * ringRadius, (float)Math.Sin(angle) * ringRadius);
                Vector2 dustPos = Projectile.Center + offset;
                Vector2 dustVel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * -1f;
                int dustType = Main.rand.NextBool(3)
                    ? ModContent.DustType<SmokeDust6>()
                    : ModContent.DustType<FlameDust2>();
                Dust d = Dust.NewDustPerfect(dustPos, dustType, dustVel, 0, default, Main.rand.NextFloat(0.8f, 1.5f));
                d.noGravity = true;
            }

            // Add orange/red light at center
            Lighting.AddLight(Projectile.Center, new Color(255, 120, 30).ToVector3() * 1.5f * fadeProgress);
        }

        private void DrawChargeRing()
        {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Texture2D ringTexture = TextureAssets.Projectile[Projectile.type].Value;
            int frameHeight = ringTexture.Height / FrameCount;
            Rectangle sourceRect = new Rectangle(0, frameHeight * Projectile.frame, ringTexture.Width, frameHeight);
            float pulse = 0.85f + (float)Math.Sin(Main.GameUpdateCount * 0.18f) * 0.15f;
            float fadeIn = MathHelper.Clamp(AnimationTimer / 30f, 0f, 1f);
            float fadeOut = 1f - MathHelper.Clamp((AnimationTimer - 92f) / 58f, 0f, 1f);
            float ringAlpha = fadeIn * fadeOut;
            float ringScale = TargetDiameter / Math.Max(ringTexture.Width, frameHeight) * Projectile.scale;

            if (!GlobBurstPlayed)
            {
                DrawLavaSweep(drawPos, fadeIn);
            }

            float spiralGlobAlpha = !GlobBurstPlayed
                ? fadeIn
                : GetPostChargeGlobAlpha();
            if (FireUpgrade2Active)
            {
                RovaLavaGlobVisual.DrawStoredColors(
                    SpiralGlobs,
                    Main.screenPosition,
                    new Color(255, 218, 72, 240),
                    spiralGlobAlpha);
            }
            else
            {
                RovaLavaGlobVisual.Draw(
                    SpiralGlobs,
                    Main.screenPosition,
                    new Color(255, 66, 10, 225),
                    new Color(255, 218, 72, 240),
                    spiralGlobAlpha);
            }

            if (ringAlpha > 0.005f)
            {
                Main.spriteBatch.Draw(
                    ringTexture,
                    drawPos,
                    sourceRect,
                    Color.White * (pulse * ringAlpha),
                    Projectile.rotation,
                    new Vector2(ringTexture.Width / 2f, frameHeight / 2f),
                    ringScale,
                    SpriteEffects.None,
                    0f);
            }

            DrawChargeFire2ExpansionRing(
                ringTexture,
                drawPos,
                sourceRect,
                frameHeight);

            DrawLavaGlobs(drawPos);
        }

        private void DrawChargeFire2ExpansionRing(
            Texture2D ringTexture,
            Vector2 drawPos,
            Rectangle sourceRect,
            int frameHeight)
        {
            if (ChargeFire2RingTimer < 0
                || ChargeFire2RingTimer >= ChargeFire2RingDurationTicks)
            {
                return;
            }

            float progress = MathHelper.Clamp(
                ChargeFire2RingTimer / (float)ChargeFire2RingDurationTicks,
                0f,
                1f);
            float growth = SmoothStep(progress);
            float fadeIn = SmoothStep(MathHelper.Clamp(progress / 0.18f, 0f, 1f));
            float fadeOut = 1f - SmoothStep(MathHelper.Clamp(
                (progress - 0.16f) / 0.84f,
                0f,
                1f));
            float alpha = fadeIn * fadeOut;
            if (alpha <= 0.005f)
                return;

            float textureScale = TargetDiameter / Math.Max(ringTexture.Width, frameHeight);
            float expansionScale = MathHelper.Lerp(0.025f, 1.12f, growth);
            float pulse = 0.94f
                + (float)Math.Sin(ChargeFire2RingTimer * 0.42f) * 0.06f;

            Main.spriteBatch.Draw(
                ringTexture,
                drawPos,
                sourceRect,
                Color.White * (alpha * pulse),
                Projectile.rotation - progress * 1.15f,
                new Vector2(ringTexture.Width / 2f, frameHeight / 2f),
                textureScale * expansionScale,
                SpriteEffects.None,
                0f);
        }

        private float GetPostChargeGlobAlpha()
        {
            if (HasActiveBeam((int)Projectile.ai[0]))
                return 0.8f;

            if (PostChargeTimer < 120)
                return MathHelper.Clamp(1f - PostChargeTimer / 120f, 0f, 1f);

            return 0.35f;
        }

        private void DrawLavaSweep(Vector2 drawPos, float alpha)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            float circleFormation = SmoothStep(MathHelper.Clamp(
                (AnimationTimer - CircleFormationStartTicks) / (float)CircleFormationDurationTicks,
                0f,
                1f));
            // Matches the arrival angle of each spiral head so every arc visibly
            // grows out of the spot that reached the inner radius.
            float circleRotation = MathHelper.TwoPi * 1.25f + AnimationTimer * 0.018f;

            for (int i = 0; i < SpiralArmCount; i++)
            {
                float stagger = i * 2f;
                float travel = SmoothStep(MathHelper.Clamp((AnimationTimer - stagger) / (float)SpiralDurationTicks, 0f, 1f));
                float baseAngle = i * MathHelper.TwoPi / SpiralArmCount;

                // Each arriving star point grows its own arc. The eight arcs join
                // into the complete inner circle instead of the circle appearing at once.
                float armArrival = SmoothStep(MathHelper.Clamp((travel - 0.72f) / 0.28f, 0f, 1f));
                float arcProgress = Math.Min(armArrival, circleFormation);
                int arcSegments = 5;
                for (int segment = 0; segment < arcSegments; segment++)
                {
                    float segmentStart = segment / (float)arcSegments;
                    if (segmentStart >= arcProgress)
                        break;

                    float segmentEnd = Math.Min((segment + 0.78f) / arcSegments, arcProgress);
                    float angleA = circleRotation + baseAngle + segmentStart * MathHelper.TwoPi / SpiralArmCount;
                    float angleB = circleRotation + baseAngle + segmentEnd * MathHelper.TwoPi / SpiralArmCount;
                    Vector2 pointA = drawPos + angleA.ToRotationVector2() * InnerCircleRadius;
                    Vector2 pointB = drawPos + angleB.ToRotationVector2() * InnerCircleRadius;
                    DrawLine(pixel, pointA, pointB, new Color(255, 100, 18, 205) * alpha, 2.5f);
                }
            }
        }

        private static void DrawLine(Texture2D pixel, Vector2 start, Vector2 end, Color color, float width)
        {
            Vector2 segment = end - start;
            if (segment.LengthSquared() <= 0.01f)
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

        private void SpawnLavaGlobBurst()
        {
            RovaLavaGlobVisual.SpawnOutward(LavaGlobs, Projectile.Center, 30, InnerCircleRadius, 1.8f, 4.8f, 28f, 48f, 4f, 10f);
        }

        private void DrawLavaGlobs(Vector2 drawPos)
        {
            RovaLavaGlobVisual.Draw(
                LavaGlobs,
                Main.screenPosition,
                new Color(255, 76, 10, 220),
                new Color(255, 205, 62, 235),
                0.9f);
        }

        private static bool HasActiveBeam(int centerIndex)
        {
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile projectile = Main.projectile[i];
                if (projectile.active
                    && projectile.ModProjectile is RovaBeam
                    && projectile.owner == Main.projectile[centerIndex].owner
                    && (int)projectile.ai[0] == centerIndex)
                {
                    return true;
                }
            }

            return false;
        }

        private static float SmoothStep(float value)
        {
            return value * value * (3f - 2f * value);
        }
    }
}
