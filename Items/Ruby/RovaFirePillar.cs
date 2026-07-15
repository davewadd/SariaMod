using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.Ruby
{
    /// <summary>
    /// Procedural fire pillar that appears only when RovaBeam reaches a solid
    /// surface. The parent beam supplies the impact point and surface normal.
    /// </summary>
    public class RovaFirePillar : ModProjectile
    {
        private const int FadeTicks = 18;
        private const float PillarHeight = 64f;
        private const float PillarWidth = 48f;
        private const int ColumnSliceCount = 18;
        private static readonly Color LavaDeepColor = new Color(216, 46, 2);
        private static readonly Color LavaOuterColor = new Color(255, 92, 3);
        private static readonly Color LavaOrangeColor = new Color(255, 148, 10);
        private static readonly Color LavaGoldColor = new Color(255, 205, 38);
        private static readonly Color LavaHotColor = new Color(255, 239, 86);

        private readonly List<FlameTongue> FlameTongues = new List<FlameTongue>();
        private readonly List<PillarRibbon> Ribbons = new List<PillarRibbon>();
        private readonly List<FlareArc> FlareArcs = new List<FlareArc>();
        private readonly List<RisingEmber> Embers = new List<RisingEmber>();
        private readonly List<RovaLavaGlob> EruptionGlobs = new List<RovaLavaGlob>();
        private int ContactTimer;
        private bool Initialized;
        private bool VisualsInitialized;
        private bool Erupted;
        private Vector2 SurfacePosition;
        private Vector2 SurfaceNormal = -Vector2.UnitY;
        private int SurfaceTileX = int.MinValue;
        private int SurfaceTileY = int.MinValue;

        internal bool IsErupted => Erupted;

        private struct FlameTongue
        {
            public float BaseOffset;
            public float BaseHeight;
            public float Height;
            public float Width;
            public float Sway;
            public float Lean;
            public float Phase;
            public float Speed;
            public Color OuterColor;
            public Color InnerColor;
        }

        private struct PillarRibbon
        {
            public float StartHeight;
            public float Height;
            public float Amplitude;
            public float Turns;
            public float SideBias;
            public float Phase;
            public float Speed;
            public float Width;
            public Color Color;
        }

        private struct FlareArc
        {
            public float Height;
            public float Radius;
            public float VerticalRadius;
            public float Phase;
            public float Speed;
            public float ArcSpan;
            public float Width;
            public Color Color;
        }

        private struct RisingEmber
        {
            public float Phase;
            public float Speed;
            public float StartOffset;
            public float Drift;
            public float RiseHeight;
            public float Size;
            public float FlickerPhase;
        }

        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Rova Fire Pillar");
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 4800;
        }

        public override void SetDefaults()
        {
            Projectile.width = (int)PillarWidth;
            Projectile.height = (int)PillarHeight;
            Projectile.alpha = 0;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 2;
            Projectile.ignoreWater = true;
            Projectile.netImportant = true;
        }

        public override void AI()
        {
            int beamIndex = (int)Projectile.ai[0];
            RovaBeam beam = null;
            if (beamIndex >= 0
                && beamIndex < Main.maxProjectiles
                && Main.projectile[beamIndex].active
                && Main.projectile[beamIndex].owner == Projectile.owner
                && Main.projectile[beamIndex].ModProjectile is RovaBeam parentBeam)
            {
                beam = parentBeam;
            }

            if (!Initialized)
            {
                Initialized = true;
                SurfacePosition = Projectile.Center;
                SurfaceNormal = Projectile.velocity.SafeNormalize(-Vector2.UnitY);
                Vector2 tileProbe = SurfacePosition - SurfaceNormal;
                SurfaceTileX = (int)Math.Floor(tileProbe.X / 16f);
                SurfaceTileY = (int)Math.Floor(tileProbe.Y / 16f);
            }

            Projectile.timeLeft = 2;
            Projectile.velocity = Vector2.Zero;

            if (beam == null)
            {
                if (!Erupted && ContactTimer > 0)
                    EruptIntoGlobs();

                if (!Erupted)
                {
                    Projectile.Kill();
                    return;
                }

                RovaLavaGlobVisual.Update(EruptionGlobs);
                if (EruptionGlobs.Count == 0)
                    Projectile.Kill();
                return;
            }

            bool surfaceActive = !Erupted
                && beam.HasSurfaceContact()
                && !beam.IsBeamEnding
                && IsSameSurface(beam);

            if (surfaceActive)
            {
                SurfacePosition = beam.GetSurfaceContactPosition();
                SurfaceNormal = beam.GetSurfaceNormal().SafeNormalize(SurfaceNormal);
                ContactTimer = Math.Min(ContactTimer + 1, FadeTicks);
            }
            else
            {
                if (!Erupted && ContactTimer > 0)
                    EruptIntoGlobs();

                ContactTimer = Math.Max(ContactTimer - 1, 0);
            }

            RovaLavaGlobVisual.Update(EruptionGlobs);

            if (Erupted)
            {
                if (EruptionGlobs.Count == 0)
                    Projectile.Kill();
                return;
            }

            Projectile.Center = SurfacePosition;
            Projectile.rotation = SurfaceNormal.ToRotation() + MathHelper.PiOver2;
        }

        private bool IsSameSurface(RovaBeam beam)
        {
            Vector2 currentPosition = beam.GetSurfaceContactPosition();
            Vector2 currentNormal = beam.GetSurfaceNormal().SafeNormalize(SurfaceNormal);
            Vector2 tileProbe = currentPosition - currentNormal;
            int tileX = (int)Math.Floor(tileProbe.X / 16f);
            int tileY = (int)Math.Floor(tileProbe.Y / 16f);
            return tileX == SurfaceTileX
                && tileY == SurfaceTileY
                && Vector2.Dot(currentNormal, SurfaceNormal) >= 0.98f;
        }

        private void EruptIntoGlobs()
        {
            if (Erupted)
                return;

            Erupted = true;
            ContactTimer = 0;

            int globCount = Main.rand.Next(12, 18);
            RovaLavaGlobVisual.SpawnOutward(
                EruptionGlobs,
                SurfacePosition,
                globCount,
                radius: 0f,
                minSpeed: 0.2f,
                maxSpeed: 1.25f,
                minLife: 34f,
                maxLife: 62f,
                minSize: 4.5f,
                maxSize: 9.5f);

            Vector2 normal = SurfaceNormal.SafeNormalize(-Vector2.UnitY);
            Vector2 sideways = normal.RotatedBy(MathHelper.PiOver2);
            for (int i = 0; i < EruptionGlobs.Count; i++)
            {
                RovaLavaGlob glob = EruptionGlobs[i];
                glob.Angle = Main.rand.NextFloat(MathHelper.TwoPi);
                glob.Distance = Main.rand.NextFloat(0f, 2f);
                glob.RadialSpeed = Main.rand.NextFloat(0.05f, 0.18f);
                glob.AnchorVelocity = normal * Main.rand.NextFloat(1.8f, 4.4f)
                    + sideways * Main.rand.NextFloat(-2.2f, 2.2f);
                glob.Gravity = Vector2.UnitY * Main.rand.NextFloat(0.055f, 0.095f);
                glob.Color = RovaBeam.GetCoreSplashColor();
                EruptionGlobs[i] = glob;
            }
        }

        private void InitializeVisuals()
        {
            VisualsInitialized = true;

            // Five broad, merged tongues establish a flame silhouette without
            // reading as separate upright tentacles.
            float[] tongueOffsets = { -8f, -4f, 0f, 4f, 8f };
            float[] tongueStarts = { 8f, 3f, 0f, 4f, 9f };
            float[] tongueHeights = { 30f, 52f, 64f, 47f, 27f };
            float[] tongueWidths = { 7.2f, 8.8f, 9.6f, 8.6f, 7f };
            float[] tongueSways = { 7f, 9f, 10f, 9f, 7f };
            Color[] tongueOuterColors =
            {
                LavaOuterColor,
                LavaOuterColor,
                LavaOrangeColor,
                LavaOuterColor,
                LavaDeepColor
            };
            Color[] tongueInnerColors =
            {
                LavaGoldColor,
                LavaGoldColor,
                LavaHotColor,
                LavaGoldColor,
                LavaOrangeColor
            };

            for (int i = 0; i < tongueOffsets.Length; i++)
            {
                FlameTongues.Add(new FlameTongue
                {
                    BaseOffset = tongueOffsets[i],
                    BaseHeight = tongueStarts[i],
                    Height = tongueHeights[i],
                    Width = tongueWidths[i],
                    Sway = tongueSways[i],
                    Lean = Main.rand.NextFloat(-4f, 4f),
                    Phase = i * 0.91f + Main.rand.NextFloat(-0.28f, 0.28f),
                    Speed = Main.rand.NextFloat(0.16f, 0.28f),
                    OuterColor = tongueOuterColors[i],
                    InnerColor = tongueInnerColors[i]
                });
            }

            float[] ribbonStarts = { 3f, 8f, 14f, 20f, 26f };
            float[] ribbonHeights = { 49f, 48f, 46f, 40f, 34f };
            float[] ribbonAmplitudes = { 20f, 18f, 21f, 16f, 13f };
            float[] ribbonTurns = { 1.15f, 1.4f, 1.05f, 1.55f, 1.2f };
            Color[] ribbonColors =
            {
                LavaOuterColor,
                LavaOrangeColor,
                LavaOuterColor,
                LavaOrangeColor,
                LavaOuterColor
            };

            for (int i = 0; i < ribbonStarts.Length; i++)
            {
                Ribbons.Add(new PillarRibbon
                {
                    StartHeight = ribbonStarts[i],
                    Height = ribbonHeights[i],
                    Amplitude = ribbonAmplitudes[i],
                    Turns = ribbonTurns[i],
                    SideBias = Main.rand.NextFloat(-2f, 2f),
                    Phase = i * MathHelper.TwoPi / ribbonStarts.Length + Main.rand.NextFloat(-0.3f, 0.3f),
                    Speed = Main.rand.NextFloat(0.035f, 0.075f) * (i % 2 == 0 ? 1f : -1f),
                    Width = Main.rand.NextFloat(1.5f, 2.6f),
                    Color = ribbonColors[i]
                });
            }

            FlareArcs.Add(new FlareArc
            {
                Height = 13f,
                Radius = 23f,
                VerticalRadius = 4f,
                Phase = Main.rand.NextFloat(MathHelper.TwoPi),
                Speed = 0.085f,
                ArcSpan = 2.25f,
                Width = 2.5f,
                Color = LavaOuterColor
            });
            FlareArcs.Add(new FlareArc
            {
                Height = 31f,
                Radius = 19.5f,
                VerticalRadius = 5f,
                Phase = Main.rand.NextFloat(MathHelper.TwoPi),
                Speed = -0.105f,
                ArcSpan = 2.55f,
                Width = 2.1f,
                Color = LavaOrangeColor
            });
            FlareArcs.Add(new FlareArc
            {
                Height = 49f,
                Radius = 15.5f,
                VerticalRadius = 3.8f,
                Phase = Main.rand.NextFloat(MathHelper.TwoPi),
                Speed = 0.125f,
                ArcSpan = 2f,
                Width = 1.8f,
                Color = LavaOrangeColor
            });

            const int emberCount = 12;
            for (int i = 0; i < emberCount; i++)
            {
                float riseHeight = Main.rand.NextFloat(43f, 62f);
                if (i % 5 == 0)
                    riseHeight += Main.rand.NextFloat(6f, 10f);

                Embers.Add(new RisingEmber
                {
                    Phase = i / (float)emberCount + Main.rand.NextFloat(0f, 0.07f),
                    Speed = Main.rand.NextFloat(0.0065f, 0.0115f),
                    StartOffset = Main.rand.NextFloat(-17.5f, 17.5f),
                    Drift = Main.rand.NextFloat(2f, 5.5f),
                    RiseHeight = riseHeight,
                    Size = Main.rand.NextFloat(1.1f, 2.4f),
                    FlickerPhase = Main.rand.NextFloat(MathHelper.TwoPi)
                });
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            int beamIndex = (int)Projectile.ai[0];
            RovaBeam beam = null;
            if (beamIndex >= 0
                && beamIndex < Main.maxProjectiles
                && Main.projectile[beamIndex].active
                && Main.projectile[beamIndex].ModProjectile is RovaBeam parentBeam)
            {
                beam = parentBeam;
            }

            float beamAlpha = beam == null ? 1f : beam.GetBeamEndpointAlpha();
            if (EruptionGlobs.Count > 0)
            {
                RovaLavaGlobVisual.DrawStoredColors(
                    EruptionGlobs,
                    Main.screenPosition,
                    RovaBeam.CoreSplashInnerColor,
                    beamAlpha);
            }

            // The contact projectile remains active only to detect when the
            // beam leaves a tile and release its golden glob spray.
            return false;
        }

        // Preserved for later iteration. Intentionally unused while the fire
        // pillar itself is disabled.
        private void DrawStoredPillarVisual(RovaBeam beam)
        {

            if (Erupted)
                return;

            float growth = MathHelper.Clamp(ContactTimer / (float)FadeTicks, 0f, 1f);
            if (growth <= 0.005f)
                return;

            if (beam == null)
                return;

            float alpha = growth * beam.GetBeamEndpointAlpha();
            if (alpha <= 0.005f)
                return;

            if (!VisualsInitialized)
                InitializeVisuals();

            float lightStrength = MathHelper.Clamp(ContactTimer / (float)FadeTicks, 0f, 1f);
            Vector2 lightPosition = SurfacePosition + SurfaceNormal.SafeNormalize(-Vector2.UnitY) * 4f;
            Lighting.AddLight(
                lightPosition,
                LavaGoldColor.ToVector3() * (1.15f * lightStrength));

            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Vector2 normal = SurfaceNormal.SafeNormalize(-Vector2.UnitY);
            Vector2 perpendicular = normal.RotatedBy(MathHelper.PiOver2);
            Vector2 basePosition = SurfacePosition - Main.screenPosition + normal * 2f;
            float time = (float)Main.GameUpdateCount;

            DrawBaseEruptionSwirls(pixel, basePosition, normal, perpendicular, growth, alpha, time, false);
            DrawFlareArcs(pixel, basePosition, normal, perpendicular, growth, alpha, time, false);
            DrawRisingRibbons(pixel, basePosition, normal, perpendicular, growth, alpha, time, false);
            DrawPeakJets(pixel, basePosition, normal, perpendicular, growth, alpha, time, false);
            DrawFlameTongues(pixel, basePosition, normal, perpendicular, growth, alpha, time, false);

            // Keep the molten mound behind the main column so the pillar grows
            // through it instead of looking like a separate bowl pasted on top.
            DrawBaseFlare(pixel, basePosition, normal, perpendicular, growth, alpha, time);
            DrawMoltenColumn(pixel, basePosition, normal, perpendicular, growth, alpha, time);
            DrawBaseBubbles(pixel, basePosition, normal, perpendicular, growth, alpha, time);

            DrawFlameTongues(pixel, basePosition, normal, perpendicular, growth, alpha, time, true);
            DrawRisingRibbons(pixel, basePosition, normal, perpendicular, growth, alpha, time, true);
            DrawPeakJets(pixel, basePosition, normal, perpendicular, growth, alpha, time, true);
            DrawFlareArcs(pixel, basePosition, normal, perpendicular, growth, alpha, time, true);
            DrawBaseEruptionSwirls(pixel, basePosition, normal, perpendicular, growth, alpha, time, true);
            DrawEmbers(pixel, basePosition, normal, perpendicular, growth, alpha, time);
        }

        private static void DrawMoltenColumn(
            Texture2D pixel,
            Vector2 basePosition,
            Vector2 normal,
            Vector2 perpendicular,
            float growth,
            float alpha,
            float time)
        {
            float height = PillarHeight * growth;
            float widthGrowth = MathHelper.SmoothStep(0f, 1f, growth);

            DrawColumnLayer(
                pixel,
                basePosition,
                normal,
                perpendicular,
                height,
                widthGrowth,
                alpha * 0.82f,
                time,
                1f,
                0f,
                LavaDeepColor,
                LavaOuterColor);
            DrawColumnLayer(
                pixel,
                basePosition,
                normal,
                perpendicular,
                height,
                widthGrowth,
                alpha * 0.94f,
                time,
                0.68f,
                1.35f,
                LavaOuterColor,
                LavaOrangeColor);
            DrawColumnLayer(
                pixel,
                basePosition,
                normal,
                perpendicular,
                height,
                widthGrowth,
                alpha * 0.86f,
                time,
                0.22f,
                2.25f,
                LavaGoldColor,
                LavaHotColor);

            DrawMoltenVeins(pixel, basePosition, normal, perpendicular, growth, alpha, time);
        }

        private static void DrawColumnLayer(
            Texture2D pixel,
            Vector2 basePosition,
            Vector2 normal,
            Vector2 perpendicular,
            float height,
            float widthGrowth,
            float alpha,
            float time,
            float widthRatio,
            float lateralShift,
            Color lowerColor,
            Color upperColor)
        {
            float sliceSpacing = height / ColumnSliceCount;
            float sliceThickness = Math.Max(0.7f, sliceSpacing + 1.2f * widthGrowth);

            for (int i = 0; i <= ColumnSliceCount; i++)
            {
                float progress = i / (float)ColumnSliceCount;
                bool brokenHighlight = widthRatio < 0.4f;
                float highlightBreakup = (float)Math.Sin(i * 1.91f + time * 0.11f)
                    + (float)Math.Sin(i * 0.73f - time * 0.16f) * 0.7f;
                if (brokenHighlight && highlightBreakup < 0.12f)
                    continue;

                GetColumnShape(progress, time, out float centerOffset, out float halfWidth);
                float layerOffset = (float)Math.Sin(time * 0.11f + progress * 12.7f + widthRatio * 4.1f)
                    * lateralShift
                    * widthGrowth;
                Vector2 center = basePosition
                    + normal * (height * progress)
                    + perpendicular * (centerOffset * widthGrowth + layerOffset);
                float layerHalfWidth = halfWidth * widthRatio * widthGrowth;
                if (brokenHighlight)
                    layerHalfWidth *= 0.76f + MathHelper.Clamp(highlightBreakup, 0f, 1f) * 0.24f;
                float colorWave = 0.5f + (float)Math.Sin(time * 0.09f + progress * 17.3f + widthRatio * 3f) * 0.5f;
                Color color = Color.Lerp(lowerColor, upperColor, colorWave) * alpha;

                DrawSegment(
                    pixel,
                    center - perpendicular * layerHalfWidth,
                    perpendicular,
                    layerHalfWidth * 2f,
                    sliceThickness,
                    color);
            }

            for (int i = 1; i <= 5; i++)
            {
                float progress = 0.04f + i * 0.16f;
                GetColumnShape(progress, time, out float centerOffset, out float halfWidth);
                float layerOffset = (float)Math.Sin(time * 0.11f + progress * 12.7f + widthRatio * 4.1f)
                    * lateralShift
                    * widthGrowth;
                Vector2 position = basePosition
                    + normal * (height * progress)
                    + perpendicular * (centerOffset * widthGrowth + layerOffset);
                float radius = Math.Min(
                    halfWidth * widthRatio * widthGrowth * 0.46f,
                    1f + 7.5f * widthRatio);
                float colorWave = 0.5f + (float)Math.Sin(time * 0.08f + progress * 15f) * 0.5f;
                Color color = Color.Lerp(lowerColor, upperColor, colorWave) * (alpha * 0.78f);
                RovaLavaGlobVisual.DrawSoftGlob(pixel, position, radius, color);
            }
        }

        private static void GetColumnShape(
            float progress,
            float time,
            out float centerOffset,
            out float halfWidth)
        {
            progress = MathHelper.Clamp(progress, 0f, 1f);
            float taper = 1f - (float)Math.Pow(progress, 1.42f);
            float edgeEnvelope = (float)Math.Sin(progress * MathHelper.Pi);
            float irregularity = ((float)Math.Sin(progress * 15.2f + time * 0.075f) * 1.55f
                + (float)Math.Sin(progress * 34.1f - time * 0.115f) * 0.8f)
                * edgeEnvelope;
            halfWidth = MathHelper.Clamp(
                3f + 12f * taper + irregularity,
                2.25f,
                PillarWidth * 0.46f);

            float anchorEase = MathHelper.SmoothStep(
                0f,
                1f,
                MathHelper.Clamp(progress / 0.18f, 0f, 1f));
            centerOffset = ((float)Math.Sin(progress * 8.8f + time * 0.085f) * 2.25f
                + (float)Math.Sin(progress * 22.9f - time * 0.13f) * 1.25f)
                * anchorEase;
        }

        private static void DrawMoltenVeins(
            Texture2D pixel,
            Vector2 basePosition,
            Vector2 normal,
            Vector2 perpendicular,
            float growth,
            float alpha,
            float time)
        {
            float widthGrowth = MathHelper.SmoothStep(0f, 1f, growth);
            for (int layer = 0; layer < 2; layer++)
            {
                float layerWidth = (layer == 0 ? 3.1f : 1.25f) * widthGrowth;
                Color color = layer == 0
                    ? LavaOrangeColor * (alpha * 0.58f)
                    : LavaGoldColor * (alpha * 0.78f);

                for (int vein = 0; vein < 2; vein++)
                {
                    const int segmentCount = 11;
                    Vector2 previous = GetMoltenVeinPoint(
                        vein,
                        0.06f,
                        basePosition,
                        normal,
                        perpendicular,
                        growth,
                        widthGrowth,
                        time);

                    for (int i = 1; i <= segmentCount; i++)
                    {
                        float progress = MathHelper.Lerp(0.06f, 0.92f, i / (float)segmentCount);
                        Vector2 current = GetMoltenVeinPoint(
                            vein,
                            progress,
                            basePosition,
                            normal,
                            perpendicular,
                            growth,
                            widthGrowth,
                            time);
                        Vector2 segment = current - previous;
                        float breakup = (float)Math.Sin(i * 1.7f + vein * 2.1f + time * 0.08f);
                        if (segment.LengthSquared() > 0.2f && breakup > -0.22f)
                        {
                            float taper = 1f - progress * 0.42f;
                            DrawSegment(
                                pixel,
                                previous,
                                segment.SafeNormalize(normal),
                                segment.Length() + 0.6f,
                                layerWidth * taper,
                                color);
                        }

                        previous = current;
                    }
                }
            }
        }

        private static Vector2 GetMoltenVeinPoint(
            int vein,
            float progress,
            Vector2 basePosition,
            Vector2 normal,
            Vector2 perpendicular,
            float growth,
            float widthGrowth,
            float time)
        {
            GetColumnShape(progress, time, out float centerOffset, out float halfWidth);
            float side = vein == 0 ? -0.18f : 0.18f;
            float curl = (float)Math.Sin(
                progress * (8.2f + vein * 1.7f)
                + time * (vein == 0 ? 0.07f : -0.06f)
                + vein * 2.4f);
            float localOffset = centerOffset + halfWidth * (side + curl * 0.21f);
            return basePosition
                + normal * (PillarHeight * growth * progress)
                + perpendicular * (localOffset * widthGrowth);
        }

        private void DrawFlameTongues(
            Texture2D pixel,
            Vector2 basePosition,
            Vector2 normal,
            Vector2 perpendicular,
            float growth,
            float alpha,
            float time,
            bool front)
        {
            float widthGrowth = MathHelper.SmoothStep(0f, 1f, growth);
            for (int i = 0; i < FlameTongues.Count; i++)
            {
                FlameTongue tongue = FlameTongues[i];
                float flicker = 0.91f
                    + (float)Math.Sin(time * tongue.Speed + tongue.Phase) * 0.1f
                    + (float)Math.Sin(time * tongue.Speed * 1.83f - tongue.Phase * 0.7f) * 0.055f;
                float visibleHeight = Math.Min(
                    tongue.Height * flicker,
                    PillarHeight + 2f - tongue.BaseHeight);
                float passAlpha = alpha * (front ? 1f : 0.55f);

                DrawFlameTongueLayer(
                    pixel,
                    tongue,
                    basePosition,
                    normal,
                    perpendicular,
                    growth,
                    widthGrowth,
                    time,
                    visibleHeight,
                    1f,
                    tongue.OuterColor * (passAlpha * 0.9f),
                    front);
                DrawFlameTongueLayer(
                    pixel,
                    tongue,
                    basePosition,
                    normal,
                    perpendicular,
                    growth,
                    widthGrowth,
                    time,
                    visibleHeight,
                    0.43f,
                    tongue.InnerColor * passAlpha,
                    front);
            }
        }

        private static void DrawFlameTongueLayer(
            Texture2D pixel,
            FlameTongue tongue,
            Vector2 basePosition,
            Vector2 normal,
            Vector2 perpendicular,
            float growth,
            float widthGrowth,
            float time,
            float visibleHeight,
            float widthRatio,
            Color color,
            bool front)
        {
            const int segmentCount = 14;
            Vector2 previous = GetFlameTonguePoint(
                tongue,
                0f,
                basePosition,
                normal,
                perpendicular,
                growth,
                widthGrowth,
                time,
                visibleHeight,
                out float previousAngle);

            for (int i = 1; i <= segmentCount; i++)
            {
                float progress = i / (float)segmentCount;
                float midpoint = (i - 0.5f) / segmentCount;
                Vector2 current = GetFlameTonguePoint(
                    tongue,
                    progress,
                    basePosition,
                    normal,
                    perpendicular,
                    growth,
                    widthGrowth,
                    time,
                    visibleHeight,
                    out float angle);
                Vector2 segment = current - previous;
                float depth = (float)Math.Sin((previousAngle + angle) * 0.5f);
                if ((depth >= 0f) == front && segment.LengthSquared() > 0.2f)
                {
                    float breakup = (float)Math.Sin(
                        midpoint * 23f
                        + time * tongue.Speed * 1.7f
                        + tongue.Phase);
                    bool brokenHotInterior = widthRatio < 0.5f && breakup < -0.28f;
                    bool brokenOuterTip = widthRatio >= 0.5f
                        && midpoint > 0.55f
                        && breakup < -0.7f;
                    if (!brokenHotInterior && !brokenOuterTip)
                    {
                        float taper = (float)Math.Pow(1f - midpoint, 1.12f);
                        float belly = (float)Math.Sin(midpoint * MathHelper.Pi);
                        float widthFlicker = 0.88f
                            + (float)Math.Sin(time * 0.22f + midpoint * 18f + tongue.Phase) * 0.12f;
                        float width = (0.08f + tongue.Width * taper * (0.54f + belly * 0.5f))
                            * widthGrowth
                            * widthRatio
                            * widthFlicker;
                        if (widthRatio < 0.5f)
                        {
                            DrawPixelBlock(
                                pixel,
                                (previous + current) * 0.5f,
                                Math.Max(1f, width),
                                color);
                        }
                        else
                        {
                            DrawSegment(
                                pixel,
                                previous,
                                segment.SafeNormalize(normal),
                                segment.Length() + 0.9f,
                                width,
                                color);
                        }
                    }
                }

                previous = current;
                previousAngle = angle;
            }
        }

        private static Vector2 GetFlameTonguePoint(
            FlameTongue tongue,
            float progress,
            Vector2 basePosition,
            Vector2 normal,
            Vector2 perpendicular,
            float growth,
            float widthGrowth,
            float time,
            float visibleHeight,
            out float orbitAngle)
        {
            float join = MathHelper.SmoothStep(0f, 1f, progress);
            float orbitTurns = 0.9f + tongue.Sway * 0.035f;
            orbitAngle = tongue.Phase
                + time * tongue.Speed * 0.42f
                + progress * orbitTurns * MathHelper.TwoPi;
            float orbitEnvelope = 0.24f
                + (float)Math.Sin(progress * MathHelper.Pi) * 0.76f;
            float orbitRadius = tongue.Sway
                * MathHelper.Lerp(0.34f, 1f, join)
                * orbitEnvelope;
            float tipJitter = (float)Math.Sin(
                time * tongue.Speed * 2.6f
                + tongue.Phase * 1.7f
                + progress * 9f)
                * 0.9f
                * progress
                * progress;
            float peakJet = Math.Abs(tongue.BaseOffset) <= 4f
                ? MathHelper.SmoothStep(
                    0f,
                    1f,
                    MathHelper.Clamp((progress - 0.78f) / 0.22f, 0f, 1f))
                : 0f;
            float peakSide = Math.Cos(orbitAngle) >= 0f ? 1f : -1f;
            float localOffset = tongue.BaseOffset * MathHelper.Lerp(0.68f, 0.12f, join)
                + tongue.Lean * progress * progress * 0.28f
                + (float)Math.Cos(orbitAngle) * orbitRadius
                + tipJitter
                + peakSide * peakJet * (4f + tongue.Sway * 0.35f);
            float verticalOrbit = (float)Math.Sin(orbitAngle)
                * (0.65f + (float)Math.Sin(progress * MathHelper.Pi) * 1.35f)
                * widthGrowth;
            float localHeight = tongue.BaseHeight
                + visibleHeight * progress
                + verticalOrbit
                + peakJet * 2f;
            return basePosition
                + normal * (localHeight * growth)
                + perpendicular * (localOffset * widthGrowth);
        }

        private void DrawRisingRibbons(
            Texture2D pixel,
            Vector2 basePosition,
            Vector2 normal,
            Vector2 perpendicular,
            float growth,
            float alpha,
            float time,
            bool front)
        {
            float widthGrowth = MathHelper.SmoothStep(0f, 1f, growth);
            foreach (PillarRibbon ribbon in Ribbons)
            {
                const int segmentCount = 16;
                Vector2 previous = GetRibbonPoint(
                    ribbon,
                    0f,
                    basePosition,
                    normal,
                    perpendicular,
                    growth,
                    widthGrowth,
                    time,
                    out float previousAngle);

                for (int i = 1; i <= segmentCount; i++)
                {
                    float progress = i / (float)segmentCount;
                    float midpoint = (i - 0.5f) / segmentCount;
                    Vector2 current = GetRibbonPoint(
                        ribbon,
                        progress,
                        basePosition,
                        normal,
                        perpendicular,
                        growth,
                        widthGrowth,
                        time,
                        out float angle);
                    float depth = (float)Math.Sin((previousAngle + angle) * 0.5f);
                    Vector2 segment = current - previous;
                    if ((depth >= 0f) == front && segment.LengthSquared() > 0.2f)
                    {
                        float envelope = 0.18f + (float)Math.Sin(midpoint * MathHelper.Pi) * 0.82f;
                        float passAlpha = alpha * envelope * (front ? 0.84f : 0.34f);
                        float width = ribbon.Width
                            * widthGrowth
                            * (0.78f + (float)Math.Sin(time * 0.16f + ribbon.Phase + midpoint * 9f) * 0.22f);
                        Vector2 direction = segment.SafeNormalize(normal);
                        DrawSegment(
                            pixel,
                            previous,
                            direction,
                            segment.Length() + 0.7f,
                            width,
                            ribbon.Color * passAlpha);
                        float highlightPattern = (float)Math.Sin(
                            midpoint * 25f
                            + time * 0.12f
                            + ribbon.Phase);
                        if (highlightPattern > 0.22f)
                        {
                            DrawPixelBlock(
                                pixel,
                                (previous + current) * 0.5f,
                                Math.Max(1f, width * 0.72f),
                                LavaGoldColor * (passAlpha * 0.82f));
                        }
                    }

                    previous = current;
                    previousAngle = angle;
                }
            }
        }

        private static Vector2 GetRibbonPoint(
            PillarRibbon ribbon,
            float progress,
            Vector2 basePosition,
            Vector2 normal,
            Vector2 perpendicular,
            float growth,
            float widthGrowth,
            float time,
            out float angle)
        {
            angle = ribbon.Phase + time * ribbon.Speed + progress * ribbon.Turns * MathHelper.TwoPi;
            float radius = ribbon.Amplitude
                * widthGrowth
                * (0.72f + (float)Math.Sin(progress * MathHelper.Pi) * 0.28f);
            float localOffset = ribbon.SideBias * widthGrowth * (1f - progress)
                + (float)Math.Cos(angle) * radius;
            float localHeight = (ribbon.StartHeight + ribbon.Height * progress) * growth
                + (float)Math.Sin(angle) * 2.1f * widthGrowth;
            return basePosition + normal * localHeight + perpendicular * localOffset;
        }

        private void DrawPeakJets(
            Texture2D pixel,
            Vector2 basePosition,
            Vector2 normal,
            Vector2 perpendicular,
            float growth,
            float alpha,
            float time,
            bool front)
        {
            float widthGrowth = MathHelper.SmoothStep(0f, 1f, growth);
            for (int jet = 0; jet < 3; jet++)
            {
                int ribbonIndex = jet * 2;
                if (ribbonIndex >= Ribbons.Count)
                    break;

                bool jetIsFront = jet != 1;
                if (jetIsFront != front)
                    continue;

                float cycle = time * (0.0068f + jet * 0.0009f) + jet * 0.31f;
                cycle -= (float)Math.Floor(cycle);
                float appear = MathHelper.SmoothStep(
                    0f,
                    1f,
                    MathHelper.Clamp(cycle / 0.13f, 0f, 1f));
                float disappear = 1f - MathHelper.SmoothStep(
                    0f,
                    1f,
                    MathHelper.Clamp((cycle - 0.68f) / 0.32f, 0f, 1f));
                float jetAlpha = alpha * appear * disappear * (front ? 0.92f : 0.42f);
                if (jetAlpha <= 0.01f)
                    continue;

                PillarRibbon ribbon = Ribbons[ribbonIndex];
                Vector2 start = GetRibbonPoint(
                    ribbon,
                    0.96f,
                    basePosition,
                    normal,
                    perpendicular,
                    growth,
                    widthGrowth,
                    time,
                    out _);
                float side = jet == 0 ? -1f : 1f;
                if (jet == 2 && Math.Sin(time * 0.035f) < 0f)
                    side = -1f;

                Vector2 jetDirection = (normal * 0.42f + perpendicular * side * 0.91f)
                    .SafeNormalize(normal);
                float extension = (10f + jet * 2.5f)
                    * MathHelper.SmoothStep(
                        0f,
                        1f,
                        MathHelper.Clamp(cycle / 0.48f, 0f, 1f))
                    * growth;
                const int segmentCount = 6;
                Vector2 previous = start;
                for (int i = 1; i <= segmentCount; i++)
                {
                    float progress = i / (float)segmentCount;
                    float lift = (float)Math.Sin(progress * MathHelper.Pi) * (2.4f + jet * 0.65f);
                    Vector2 current = start
                        + jetDirection * (extension * progress)
                        + normal * lift
                        + perpendicular * (side * progress * progress * 2.2f);
                    Vector2 segment = current - previous;
                    float width = MathHelper.Lerp(3.6f, 0.8f, progress)
                        * widthGrowth;
                    DrawSegment(
                        pixel,
                        previous,
                        segment.SafeNormalize(jetDirection),
                        segment.Length() + 0.8f,
                        width,
                        (jet == 1 ? LavaOrangeColor : LavaOuterColor) * jetAlpha);

                    if (i % 2 == 0)
                    {
                        DrawPixelBlock(
                            pixel,
                            (previous + current) * 0.5f,
                            Math.Max(1f, width * 0.58f),
                            LavaGoldColor * (jetAlpha * 0.82f));
                    }

                    previous = current;
                }

                DrawPixelBlock(
                    pixel,
                    previous,
                    Math.Max(1f, 2.2f * widthGrowth),
                    LavaOrangeColor * (jetAlpha * 0.72f));
            }
        }

        private void DrawFlareArcs(
            Texture2D pixel,
            Vector2 basePosition,
            Vector2 normal,
            Vector2 perpendicular,
            float growth,
            float alpha,
            float time,
            bool front)
        {
            float widthGrowth = MathHelper.SmoothStep(0f, 1f, growth);
            foreach (FlareArc flare in FlareArcs)
            {
                const int segmentCount = 12;
                float startAngle = flare.Phase + time * flare.Speed;
                Vector2 previous = GetFlareArcPoint(
                    flare,
                    startAngle,
                    basePosition,
                    normal,
                    perpendicular,
                    growth,
                    widthGrowth);
                float previousAngle = startAngle;

                for (int i = 1; i <= segmentCount; i++)
                {
                    float progress = i / (float)segmentCount;
                    float midpoint = (i - 0.5f) / segmentCount;
                    float angle = startAngle + flare.ArcSpan * progress;
                    Vector2 current = GetFlareArcPoint(
                        flare,
                        angle,
                        basePosition,
                        normal,
                        perpendicular,
                        growth,
                        widthGrowth);
                    float depth = (float)Math.Sin((previousAngle + angle) * 0.5f);
                    Vector2 segment = current - previous;
                    if ((depth >= 0f) == front && segment.LengthSquared() > 0.15f)
                    {
                        float envelope = 0.2f + (float)Math.Sin(midpoint * MathHelper.Pi) * 0.8f;
                        float passAlpha = alpha * envelope * (front ? 0.92f : 0.38f);
                        float width = flare.Width * widthGrowth * (0.72f + envelope * 0.28f);
                        Vector2 direction = segment.SafeNormalize(perpendicular);
                        DrawSegment(
                            pixel,
                            previous,
                            direction,
                            segment.Length() + 0.8f,
                            width,
                            flare.Color * passAlpha);
                        float highlightPattern = (float)Math.Sin(
                            midpoint * 21f
                            - time * 0.14f
                            + flare.Phase);
                        if (highlightPattern > 0.3f)
                        {
                            DrawPixelBlock(
                                pixel,
                                (previous + current) * 0.5f,
                                Math.Max(1f, width * 0.68f),
                                LavaGoldColor * (passAlpha * 0.8f));
                        }
                    }

                    previous = current;
                    previousAngle = angle;
                }
            }
        }

        private static Vector2 GetFlareArcPoint(
            FlareArc flare,
            float angle,
            Vector2 basePosition,
            Vector2 normal,
            Vector2 perpendicular,
            float growth,
            float widthGrowth)
        {
            float localHeight = flare.Height * growth
                + (float)Math.Sin(angle) * flare.VerticalRadius * widthGrowth;
            float localOffset = (float)Math.Cos(angle) * flare.Radius * widthGrowth;
            return basePosition + normal * localHeight + perpendicular * localOffset;
        }

        private static void DrawBaseEruptionSwirls(
            Texture2D pixel,
            Vector2 basePosition,
            Vector2 normal,
            Vector2 perpendicular,
            float growth,
            float alpha,
            float time,
            bool front)
        {
            float widthGrowth = MathHelper.SmoothStep(0f, 1f, growth);
            const int swirlCount = 5;
            for (int swirl = 0; swirl < swirlCount; swirl++)
            {
                // Each trail has its own life cycle. It is born inside the
                // rounded base, spirals upward and outward, then fades before
                // restarting so the trails never disappear as one solid ring.
                float speed = 0.0064f + swirl * 0.00043f;
                float cycle = time * speed + swirl * 0.193f;
                cycle -= (float)Math.Floor(cycle);

                float spawnFade = MathHelper.SmoothStep(
                    0f,
                    1f,
                    MathHelper.Clamp(cycle / 0.1f, 0f, 1f));
                float endFade = 1f - MathHelper.SmoothStep(
                    0f,
                    1f,
                    MathHelper.Clamp((cycle - 0.72f) / 0.28f, 0f, 1f));
                float lifeAlpha = spawnFade * endFade;
                if (lifeAlpha <= 0.005f)
                    continue;

                float trailSpan = 0.3f + (swirl % 2) * 0.045f;
                float trailStart = Math.Max(0f, cycle - trailSpan);
                float visibleSpan = cycle - trailStart;
                if (visibleSpan <= 0.001f)
                    continue;

                float swirlWidth = 2.75f - swirl * 0.22f;
                Color swirlColor = swirl == 0
                    ? new Color(255, 104, 6)
                    : swirl == 1
                        ? new Color(255, 142, 11)
                        : swirl == 2
                            ? new Color(255, 190, 30)
                            : swirl == 3
                                ? new Color(255, 123, 7)
                                : new Color(255, 222, 72);

                const int segmentCount = 18;
                Vector2 previous = GetBaseEruptionSwirlPoint(
                    swirl,
                    trailStart,
                    basePosition,
                    normal,
                    perpendicular,
                    growth,
                    widthGrowth,
                    out float previousAngle);

                for (int i = 1; i <= segmentCount; i++)
                {
                    float segmentProgress = i / (float)segmentCount;
                    float progress = MathHelper.Lerp(trailStart, cycle, segmentProgress);
                    float midpoint = MathHelper.Lerp(
                        trailStart,
                        cycle,
                        (i - 0.5f) / segmentCount);
                    Vector2 current = GetBaseEruptionSwirlPoint(
                        swirl,
                        progress,
                        basePosition,
                        normal,
                        perpendicular,
                        growth,
                        widthGrowth,
                        out float angle);
                    float depth = (float)Math.Sin((previousAngle + angle) * 0.5f);
                    Vector2 segment = current - previous;
                    if ((depth >= 0f) == front && segment.LengthSquared() > 0.15f)
                    {
                        float trailProgress = MathHelper.Clamp(
                            (midpoint - trailStart) / visibleSpan,
                            0f,
                            1f);
                        float tailFade = MathHelper.SmoothStep(0f, 1f, trailProgress);
                        float pathEndFade = 1f - MathHelper.SmoothStep(
                            0f,
                            1f,
                            MathHelper.Clamp((midpoint - 0.74f) / 0.26f, 0f, 1f));
                        float passAlpha = alpha
                            * lifeAlpha
                            * tailFade
                            * pathEndFade
                            * (front ? 0.9f : 0.32f);
                        float width = swirlWidth
                            * widthGrowth
                            * (0.66f + tailFade * 0.34f)
                            * (0.88f + (float)Math.Sin(time * 0.13f + midpoint * 18f + swirl) * 0.12f);
                        Vector2 segmentDirection = segment.SafeNormalize(perpendicular);
                        DrawSegment(
                            pixel,
                            previous,
                            segmentDirection,
                            segment.Length() + 0.8f,
                            width,
                            swirlColor * passAlpha);
                        DrawSegment(
                            pixel,
                            previous,
                            segmentDirection,
                            segment.Length() + 0.8f,
                            width * 0.34f,
                            new Color(255, 231, 82) * (passAlpha * 0.92f));
                    }

                    previous = current;
                    previousAngle = angle;
                }
            }
        }

        private static Vector2 GetBaseEruptionSwirlPoint(
            int swirl,
            float progress,
            Vector2 basePosition,
            Vector2 normal,
            Vector2 perpendicular,
            float growth,
            float widthGrowth,
            out float angle)
        {
            float turns = 0.94f + (swirl % 3) * 0.12f;
            angle = swirl * 1.37f + progress * turns * MathHelper.TwoPi;
            float outwardProgress = MathHelper.SmoothStep(0f, 1f, progress);
            float maxRadius = 28f + swirl * 3.2f;
            float radius = MathHelper.Lerp(
                1.5f,
                maxRadius,
                (float)Math.Pow(outwardProgress, 0.82f)) * widthGrowth;
            float riseHeight = 17f + (swirl % 2) * 0.8f;
            float orbitHeight = (float)Math.Sin(angle)
                * (1.8f + swirl * 0.18f)
                * (0.18f + outwardProgress * 0.82f)
                * widthGrowth;
            float localHeight = (-4.5f + riseHeight * outwardProgress) * growth
                + orbitHeight;
            float localOffset = (float)Math.Cos(angle) * radius;

            return basePosition
                + normal * localHeight
                + perpendicular * localOffset;
        }

        private static void DrawBaseFlare(
            Texture2D pixel,
            Vector2 basePosition,
            Vector2 normal,
            Vector2 perpendicular,
            float growth,
            float alpha,
            float time)
        {
            float widthGrowth = MathHelper.SmoothStep(0f, 1f, growth);
            float pulse = 0.98f + (float)Math.Sin(time * 0.23f) * 0.045f;

            // The contact mass is centered below the tile face. Its lowest
            // point sits about 12 pixels inside the tile, covering the beam end
            // without sinking a full tile deep.
            Vector2 submergedCenter = basePosition - normal * (3.5f * widthGrowth);

            // Pixel sparks provide a broken luminous edge without turning the
            // whole base into a smooth yellow blur.
            DrawPixelEdgeGlow(
                pixel,
                submergedCenter + normal * (0.5f * growth),
                normal,
                perpendicular,
                21f * widthGrowth * pulse,
                9.5f * widthGrowth,
                widthGrowth,
                alpha,
                time);
            DrawOrientedPixelEllipse(
                pixel,
                submergedCenter,
                normal,
                perpendicular,
                19.5f * widthGrowth * pulse,
                8.8f * widthGrowth,
                LavaOuterColor * (alpha * 0.9f));
            Vector2 middleCenter = submergedCenter
                + normal * (1.7f * growth)
                + perpendicular * ((float)Math.Sin(time * 0.14f + 1.1f) * 1.35f * widthGrowth);
            DrawOrientedPixelEllipse(
                pixel,
                middleCenter,
                normal,
                perpendicular,
                14.8f * widthGrowth * pulse,
                6.4f * widthGrowth,
                LavaOrangeColor * (alpha * 0.94f));
            Vector2 hotCenter = submergedCenter
                + normal * (3.2f * growth)
                + perpendicular * ((float)Math.Sin(time * 0.19f - 0.65f) * 1.9f * widthGrowth);
            DrawOrientedPixelEllipse(
                pixel,
                hotCenter,
                normal,
                perpendicular,
                9.6f * widthGrowth * pulse,
                4.2f * widthGrowth,
                LavaGoldColor * alpha);
            DrawOrientedPixelEllipse(
                pixel,
                hotCenter + normal * (0.6f * growth),
                normal,
                perpendicular,
                4.8f * widthGrowth * pulse,
                2.2f * widthGrowth,
                LavaHotColor * alpha);

            // Short curved jets make the rounded mass look like an eruption
            // instead of a static puddle or horizontal bar.
            for (int i = -3; i <= 3; i++)
            {
                float side = i / 3f;
                float flicker = 0.86f + (float)Math.Sin(time * 0.2f + i * 1.43f) * 0.14f;
                float sideOffset = side * 18f * widthGrowth;
                Vector2 start = submergedCenter
                    + perpendicular * sideOffset
                    + normal * (2.5f * growth);
                float jetHeight = (8f + (1f - Math.Abs(side)) * 7f) * growth * flicker;
                float curl = (float)Math.Sin(time * 0.16f + i * 1.9f) * 3f * widthGrowth;
                Vector2 middle = start
                    + normal * (jetHeight * 0.48f)
                    + perpendicular * (side * 2.5f + curl * 0.35f);
                Vector2 end = start
                    + normal * jetHeight
                    + perpendicular * (side * 5f + curl);
                Vector2 firstSegment = middle - start;
                Vector2 secondSegment = end - middle;
                float outerWidth = (5.8f - Math.Abs(side) * 1.6f) * widthGrowth;

                DrawSegment(
                    pixel,
                    start,
                    firstSegment.SafeNormalize(normal),
                    firstSegment.Length() + 0.7f,
                    outerWidth,
                    LavaOuterColor * (alpha * 0.88f));
                DrawSegment(
                    pixel,
                    middle,
                    secondSegment.SafeNormalize(normal),
                    secondSegment.Length() + 0.6f,
                    outerWidth * 0.52f,
                    LavaOrangeColor * (alpha * 0.9f));
                DrawSegment(
                    pixel,
                    start,
                    firstSegment.SafeNormalize(normal),
                    firstSegment.Length() + 0.7f,
                    outerWidth * 0.34f,
                    LavaGoldColor * alpha);
            }

            for (int i = -2; i <= 2; i++)
            {
                float flicker = 0.86f + (float)Math.Sin(time * 0.19f + i * 1.7f) * 0.14f;
                float lobeLift = (float)Math.Sin(time * 0.16f + i * 1.31f) * 1.4f;
                Vector2 lobeCenter = submergedCenter
                    + perpendicular * (i * 7.2f * widthGrowth)
                    + normal * ((3.8f + (2 - Math.Abs(i)) * 1.15f + lobeLift) * growth);
                DrawOrientedPixelEllipse(
                    pixel,
                    lobeCenter,
                    normal,
                    perpendicular,
                    (5.9f - Math.Abs(i) * 0.5f) * widthGrowth * flicker,
                    (4.5f - Math.Abs(i) * 0.32f) * widthGrowth,
                    LavaOuterColor * (alpha * 0.86f));
            }
        }

        private static void DrawBaseBubbles(
            Texture2D pixel,
            Vector2 basePosition,
            Vector2 normal,
            Vector2 perpendicular,
            float growth,
            float alpha,
            float time)
        {
            float widthGrowth = MathHelper.SmoothStep(0f, 1f, growth);
            const int bubbleCount = 6;
            for (int bubble = 0; bubble < bubbleCount; bubble++)
            {
                float cycle = time * (0.0074f + bubble * 0.00052f) + bubble * 0.173f;
                cycle -= (float)Math.Floor(cycle);
                float sideOffset = -14f + bubble * (28f / (bubbleCount - 1f));

                if (cycle < 0.78f)
                {
                    float life = cycle / 0.78f;
                    float sizeEnvelope = (float)Math.Sin(life * MathHelper.Pi);
                    float radius = (2.8f + bubble % 3 * 0.75f)
                        * sizeEnvelope
                        * widthGrowth;
                    if (radius < 0.65f)
                        continue;

                    float wobble = (float)Math.Sin(time * 0.13f + bubble * 1.9f) * 1.2f;
                    Vector2 position = basePosition
                        - normal * (2f * widthGrowth)
                        + perpendicular * ((sideOffset + wobble) * widthGrowth)
                        + normal * ((1.5f + life * 6.5f) * growth);
                    Color outerColor = bubble % 2 == 0 ? LavaOuterColor : LavaOrangeColor;
                    DrawOrientedPixelEllipse(
                        pixel,
                        position,
                        normal,
                        perpendicular,
                        radius,
                        radius * 0.78f,
                        outerColor * (alpha * (0.72f + sizeEnvelope * 0.28f)));
                    if (radius >= 2.2f)
                    {
                        DrawOrientedPixelEllipse(
                            pixel,
                            position + normal * widthGrowth,
                            normal,
                            perpendicular,
                            radius * 0.46f,
                            radius * 0.34f,
                            LavaGoldColor * (alpha * sizeEnvelope * 0.84f));
                    }
                }
                else
                {
                    float pop = (cycle - 0.78f) / 0.22f;
                    float popAlpha = alpha * (1f - pop);
                    Vector2 popCenter = basePosition
                        + perpendicular * (sideOffset * widthGrowth)
                        + normal * (8f * growth);
                    for (int side = -1; side <= 1; side += 2)
                    {
                        Vector2 popPosition = popCenter
                            + perpendicular * (side * (1.5f + pop * 3.5f) * widthGrowth)
                            + normal * (pop * 3f * growth);
                        DrawPixelBlock(
                            pixel,
                            popPosition,
                            Math.Max(1f, 2f * widthGrowth * (1f - pop * 0.45f)),
                            LavaOrangeColor * popAlpha);
                    }
                }
            }
        }

        private static void DrawPixelEdgeGlow(
            Texture2D pixel,
            Vector2 center,
            Vector2 normal,
            Vector2 perpendicular,
            float perpendicularRadius,
            float normalRadius,
            float widthGrowth,
            float alpha,
            float time)
        {
            const int sparkCount = 16;
            for (int spark = 0; spark < sparkCount; spark++)
            {
                float baseAngle = spark * MathHelper.TwoPi / sparkCount;
                float angle = baseAngle + time * 0.012f;
                float flicker = 0.5f + 0.5f * (float)Math.Sin(time * 0.19f + spark * 1.73f);
                if (flicker < 0.22f)
                    continue;

                float edgeWobble = (float)Math.Sin(time * 0.11f + spark * 2.17f) * 1.25f;
                Vector2 position = center
                    + perpendicular * ((float)Math.Cos(angle) * (perpendicularRadius + edgeWobble))
                    + normal * ((float)Math.Sin(angle) * (normalRadius + edgeWobble * 0.45f));
                float pixelSize = Math.Max(1f, (spark % 3 == 0 ? 3f : 2f) * widthGrowth);
                Color color = spark % 4 == 0
                    ? LavaGoldColor
                    : LavaOrangeColor;
                DrawSegment(
                    pixel,
                    position - perpendicular * (pixelSize * 0.5f),
                    perpendicular,
                    pixelSize,
                    pixelSize,
                    color * (alpha * MathHelper.Lerp(0.12f, 0.32f, flicker)));
            }
        }

        private static void DrawOrientedPixelEllipse(
            Texture2D pixel,
            Vector2 center,
            Vector2 normal,
            Vector2 perpendicular,
            float perpendicularRadius,
            float normalRadius,
            Color color)
        {
            if (perpendicularRadius < 0.5f || normalRadius < 0.5f || color.A <= 1)
                return;

            const float pixelSize = 2f;
            int sliceCount = Math.Max(1, (int)Math.Ceiling(normalRadius / pixelSize));
            for (int i = -sliceCount; i <= sliceCount; i++)
            {
                float normalOffset = i * pixelSize;
                float normalized = normalOffset / normalRadius;
                if (Math.Abs(normalized) > 1f)
                    continue;

                float envelope = Math.Max(0f, 1f - normalized * normalized);
                float halfWidth = (float)Math.Sqrt(envelope) * perpendicularRadius;
                halfWidth = (float)Math.Floor(halfWidth / pixelSize) * pixelSize;
                if (halfWidth < pixelSize)
                    continue;

                Vector2 sliceCenter = center + normal * normalOffset;
                float edgeShade = 0.72f + envelope * 0.28f;
                DrawSegment(
                    pixel,
                    sliceCenter - perpendicular * halfWidth,
                    perpendicular,
                    halfWidth * 2f,
                    pixelSize,
                    color * edgeShade);
            }
        }

        private void DrawEmbers(
            Texture2D pixel,
            Vector2 basePosition,
            Vector2 normal,
            Vector2 perpendicular,
            float growth,
            float alpha,
            float time)
        {
            float widthGrowth = MathHelper.SmoothStep(0f, 1f, growth);
            foreach (RisingEmber ember in Embers)
            {
                float cycle = time * ember.Speed + ember.Phase;
                cycle -= (float)Math.Floor(cycle);
                float envelope = (float)Math.Sin(cycle * MathHelper.Pi);
                float flicker = 0.76f
                    + (float)Math.Sin(time * 0.31f + ember.FlickerPhase) * 0.24f;
                float emberAlpha = alpha * envelope * flicker;
                if (emberAlpha <= 0.01f)
                    continue;

                float localHeight = (3f + ember.RiseHeight * cycle) * growth;
                float drift = (float)Math.Sin(
                    ember.FlickerPhase
                    + cycle * MathHelper.TwoPi * 1.35f
                    + time * 0.027f)
                    * ember.Drift
                    * (0.25f + cycle * 0.75f);
                float localOffset = (ember.StartOffset * (1f - cycle * 0.12f) + drift) * widthGrowth;
                Vector2 position = basePosition
                    + normal * localHeight
                    + perpendicular * localOffset;
                float size = ember.Size
                    * widthGrowth
                    * (0.82f + (float)Math.Sin(time * 0.38f + ember.FlickerPhase) * 0.18f);
                float trailLength = (2f + cycle * 3.5f) * growth;

                DrawSegment(
                    pixel,
                    position - normal * trailLength,
                    normal,
                    trailLength,
                    Math.Max(0.55f, size * 0.52f),
                    new Color(255, 102, 5) * (emberAlpha * 0.72f));
                RovaLavaGlobVisual.DrawSoftGlob(
                    pixel,
                    position,
                    size,
                    new Color(255, 125, 7) * emberAlpha);
                RovaLavaGlobVisual.DrawSoftGlob(
                    pixel,
                    position,
                    size * 0.46f,
                    new Color(255, 241, 117) * emberAlpha);
            }
        }

        private static void DrawPixelBlock(
            Texture2D pixel,
            Vector2 center,
            float size,
            Color color)
        {
            if (size <= 0f || color.A <= 1)
                return;

            int pixelSize = Math.Max(1, (int)Math.Round(size));
            Rectangle destination = new Rectangle(
                (int)Math.Round(center.X - pixelSize * 0.5f),
                (int)Math.Round(center.Y - pixelSize * 0.5f),
                pixelSize,
                pixelSize);
            Main.spriteBatch.Draw(pixel, destination, color);
        }

        private static void DrawSegment(
            Texture2D pixel,
            Vector2 start,
            Vector2 direction,
            float length,
            float width,
            Color color)
        {
            if (length <= 0f || width <= 0f || color.A <= 1)
                return;

            // Keep all procedural pieces on the same integer pixel grid as the
            // scaled RovaCenter sprite instead of allowing sub-pixel smoothing.
            Vector2 pixelStart = new Vector2(
                (float)Math.Round(start.X),
                (float)Math.Round(start.Y));
            float pixelLength = Math.Max(1f, (float)Math.Round(length));
            float pixelWidth = Math.Max(1f, (float)Math.Round(width));

            Main.spriteBatch.Draw(
                pixel,
                pixelStart,
                new Rectangle(0, 0, 1, 1),
                color,
                direction.ToRotation(),
                new Vector2(0f, 0.5f),
                new Vector2(pixelLength, pixelWidth),
                SpriteEffects.None,
                0f);
        }
    }
}
