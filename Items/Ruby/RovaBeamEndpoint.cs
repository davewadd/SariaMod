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
    /// Rounded molten cap at the tile-impact end of RovaBeam.
    /// </summary>
    public class RovaBeamEndpoint : ModProjectile
    {
        private const int MaxCapGlobs = 16;
        private const int MaxFlyingGlobs = 36;
        private const int MinimumCeilingEmberDelay = 14;
        private const int MaximumCeilingEmberDelay = 24;
        private readonly List<EndpointCapGlob> CapGlobs = new List<EndpointCapGlob>();
        private readonly List<EndpointFlyingGlob> FlyingGlobs = new List<EndpointFlyingGlob>();
        private int CapGlobSpawnTimer;
        private int FlyingGlobSpawnTimer;
        private int CeilingEmberSpawnTimer;
        private int CeilingEmberSpawnDelay = 18;
        private bool CapGlobsInitialized;
        private bool PersistentCapColorsInitialized;
        private bool HasPreviousEndpointPosition;
        private Vector2 PreviousEndpointPosition;
        private Color PersistentCapCenterColor;
        private Color PersistentCapNegativeColor;
        private Color PersistentCapPositiveColor;

        private struct EndpointCapGlob
        {
            public float Age;
            public float Life;
            public float MaxRadius;
            public float SideOffset;
            public float BackOffset;
            public float SideVelocity;
            public float PulsePhase;
            public Color Color;
        }

        private struct EndpointFlyingGlob
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Age;
            public float Life;
            public float MaxRadius;
            public float PulsePhase;
            public Color Color;
        }

        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Rova Beam End");
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 4800;
        }

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
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
            if (beamIndex < 0
                || beamIndex >= Main.maxProjectiles
                || !Main.projectile[beamIndex].active
                || Main.projectile[beamIndex].owner != Projectile.owner
                || Main.projectile[beamIndex].ModProjectile is not RovaBeam beam)
            {
                Projectile.Kill();
                return;
            }

            Vector2 endpointPosition = beam.GetBeamEndpointPosition();
            Vector2 endpointMovement = HasPreviousEndpointPosition
                ? endpointPosition - PreviousEndpointPosition
                : Vector2.Zero;
            Projectile.Center = endpointPosition;
            PreviousEndpointPosition = endpointPosition;
            HasPreviousEndpointPosition = true;
            Projectile.velocity = Vector2.Zero;
            Projectile.timeLeft = 2;

            if (Main.netMode != NetmodeID.Server)
                UpdateCapGlobs(beam, endpointMovement);

            if (Main.netMode != NetmodeID.MultiplayerClient)
                UpdateCeilingEmberDrops(beam);

            Lighting.AddLight(
                Projectile.Center,
                new Color(255, 205, 38).ToVector3() * 1.35f * beam.GetBeamEndpointAlpha());
        }

        private void UpdateCeilingEmberDrops(RovaBeam beam)
        {
            if (beam.IsBeamEnding || !beam.HasSurfaceContact())
            {
                CeilingEmberSpawnTimer = 0;
                return;
            }

            Vector2 beamDirection = beam.GetBeamDirection().SafeNormalize(Vector2.UnitX);
            Vector2 surfaceNormal = ResolveVisualSurfaceNormal(beam, beamDirection);
            if (surfaceNormal.Y < 0.75f)
            {
                CeilingEmberSpawnTimer = 0;
                return;
            }

            CeilingEmberSpawnTimer++;
            if (CeilingEmberSpawnTimer < CeilingEmberSpawnDelay)
                return;

            CeilingEmberSpawnTimer = 0;
            CeilingEmberSpawnDelay = Main.rand.Next(
                MinimumCeilingEmberDelay,
                MaximumCeilingEmberDelay + 1);

            Vector2 surfaceTangent = surfaceNormal.RotatedBy(MathHelper.PiOver2);
            Vector2 spawnPosition = Projectile.Center
                + surfaceNormal * 4f
                + surfaceTangent * Main.rand.NextFloat(-11f, 11f);
            Vector2 velocity = surfaceNormal * Main.rand.NextFloat(1.5f, 2.8f)
                + surfaceTangent * Main.rand.NextFloat(-0.75f, 0.75f);
            int damage = Math.Max(1, beam.Projectile.damage / 4);

            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                spawnPosition,
                velocity,
                ModContent.ProjectileType<RovaEmber>(),
                damage,
                0f,
                Projectile.owner);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            int beamIndex = (int)Projectile.ai[0];
            if (beamIndex < 0
                || beamIndex >= Main.maxProjectiles
                || !Main.projectile[beamIndex].active
                || Main.projectile[beamIndex].ModProjectile is not RovaBeam beam)
            {
                return false;
            }

            float alpha = beam.GetBeamEndpointAlpha();
            if (alpha <= 0.005f)
                return false;

            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            float widthScale = beam.GetBeamEndpointWidthScale();
            Vector2 direction = beam.GetBeamDirection().SafeNormalize(Vector2.UnitX);
            bool attachedToSurface = beam.HasSurfaceContact();
            Vector2 surfaceNormal = attachedToSurface
                ? ResolveVisualSurfaceNormal(beam, direction)
                : -direction;
            Vector2 surfaceTangent = attachedToSurface
                ? surfaceNormal.RotatedBy(MathHelper.PiOver2)
                : direction.RotatedBy(MathHelper.PiOver2);
            float time = (float)Main.GameUpdateCount;
            bool restingOnGround = attachedToSurface && surfaceNormal.Y < -0.75f;
            float surfaceOffset = restingOnGround ? -4f : 3.5f;
            Vector2 capCenter = drawPosition
                + surfaceNormal * (attachedToSurface ? surfaceOffset * widthScale : 0f);

            // Keep a solid rounded plug directly over the beam's flat final
            // segment. The larger molten buildup is allowed to sink into a
            // ground tile below it without exposing the beam end.
            DrawBeamEndcap(
                pixel,
                drawPosition,
                widthScale,
                alpha,
                time);

            DrawPersistentCap(
                pixel,
                capCenter,
                surfaceTangent,
                surfaceNormal,
                attachedToSurface,
                widthScale,
                alpha,
                time);

            DrawCapGlobs(
                pixel,
                capCenter,
                direction,
                surfaceTangent,
                surfaceNormal,
                attachedToSurface,
                widthScale,
                alpha);
            DrawFlyingGlobs(pixel, widthScale, alpha);
            return false;
        }

        private static void DrawBeamEndcap(
            Texture2D pixel,
            Vector2 center,
            float widthScale,
            float alpha,
            float time)
        {
            float pulse = 0.94f + (float)Math.Sin(time * 0.27f) * 0.06f;
            float radius = 10.5f * widthScale * pulse;
            RovaLavaGlobVisual.DrawSoftGlob(
                pixel,
                center,
                radius,
                new Color(255, 205, 38) * (0.98f * alpha));
            RovaLavaGlobVisual.DrawSoftGlob(
                pixel,
                center,
                radius * 0.5f,
                new Color(255, 239, 86) * alpha);
        }

        private void UpdateCapGlobs(RovaBeam beam, Vector2 endpointMovement)
        {
            if (!PersistentCapColorsInitialized)
            {
                PersistentCapColorsInitialized = true;
                PersistentCapCenterColor = RovaBeam.GetCoreSplashColor();
                PersistentCapNegativeColor = RovaBeam.GetCoreSplashColor();
                PersistentCapPositiveColor = RovaBeam.GetCoreSplashColor();
            }

            if (!CapGlobsInitialized && !beam.IsBeamEnding)
            {
                CapGlobsInitialized = true;
                for (int i = 0; i < 9; i++)
                    SpawnCapGlob(prewarm: true);
            }

            for (int i = CapGlobs.Count - 1; i >= 0; i--)
            {
                EndpointCapGlob glob = CapGlobs[i];
                glob.Age++;
                glob.SideOffset += glob.SideVelocity;
                if (glob.Age >= glob.Life)
                {
                    CapGlobs.RemoveAt(i);
                    continue;
                }

                CapGlobs[i] = glob;
            }

            UpdateFlyingGlobs();

            if (beam.IsBeamEnding)
            {
                FlyingGlobSpawnTimer = 0;
                return;
            }

            float movementSpeed = endpointMovement.Length();
            if (FlyingGlobs.Count < MaxFlyingGlobs)
            {
                FlyingGlobSpawnTimer++;
                int spawnInterval = movementSpeed >= 6f
                    ? 1
                    : movementSpeed > 0.65f
                        ? 2
                        : 4;
                if (FlyingGlobSpawnTimer >= spawnInterval)
                {
                    FlyingGlobSpawnTimer = 0;
                    Vector2 beamDirection = beam.GetBeamDirection().SafeNormalize(Vector2.UnitX);
                    Vector2 surfaceNormal = beam.HasSurfaceContact()
                        ? ResolveVisualSurfaceNormal(beam, beamDirection)
                        : -beamDirection;
                    Vector2 surfaceTangent = surfaceNormal.RotatedBy(MathHelper.PiOver2);
                    Vector2 movementDirection = movementSpeed > 0.65f
                        ? endpointMovement / movementSpeed
                        : Vector2.Zero;
                    int spawnCount = movementSpeed >= 12f ? 2 : 1;
                    for (int i = 0; i < spawnCount && FlyingGlobs.Count < MaxFlyingGlobs; i++)
                    {
                        SpawnFlyingGlob(surfaceNormal, surfaceTangent, movementDirection);
                    }
                }
            }
            else
            {
                FlyingGlobSpawnTimer = 0;
            }

            if (CapGlobs.Count >= MaxCapGlobs)
                return;

            CapGlobSpawnTimer++;
            if (CapGlobSpawnTimer >= 2)
            {
                CapGlobSpawnTimer = 0;
                SpawnCapGlob(prewarm: false);
            }
        }

        private void SpawnCapGlob(bool prewarm)
        {
            float side = Main.rand.NextBool() ? -1f : 1f;
            float life = Main.rand.NextFloat(34f, 62f);
            float sideOffset = Main.rand.NextBool(6)
                ? Main.rand.NextFloat(-2f, 2f)
                : side * Main.rand.NextFloat(3f, 13f);
            CapGlobs.Add(new EndpointCapGlob
            {
                Age = prewarm ? Main.rand.NextFloat(0f, life * 0.72f) : 0f,
                Life = life,
                MaxRadius = Main.rand.NextFloat(8.5f, 13.8f),
                SideOffset = sideOffset,
                BackOffset = Main.rand.NextFloat(-5f, 2f),
                SideVelocity = side * Main.rand.NextFloat(0.02f, 0.07f),
                PulsePhase = Main.rand.NextFloat(MathHelper.TwoPi),
                Color = RovaBeam.GetCoreSplashColor()
            });
        }

        private void SpawnFlyingGlob(
            Vector2 surfaceNormal,
            Vector2 surfaceTangent,
            Vector2 movementDirection)
        {
            FlyingGlobs.Add(new EndpointFlyingGlob
            {
                Position = Projectile.Center
                    + surfaceTangent * Main.rand.NextFloat(-6f, 6f)
                    + surfaceNormal * Main.rand.NextFloat(0f, 3f),
                Velocity = surfaceNormal * Main.rand.NextFloat(1.4f, 4.2f)
                    + movementDirection * Main.rand.NextFloat(0.45f, 2.4f)
                    + surfaceTangent * Main.rand.NextFloat(-0.9f, 0.9f),
                Age = 0f,
                Life = Main.rand.NextFloat(28f, 48f),
                MaxRadius = Main.rand.NextFloat(5.8f, 10.8f),
                PulsePhase = Main.rand.NextFloat(MathHelper.TwoPi),
                Color = RovaBeam.GetCoreSplashColor()
            });
        }

        private void UpdateFlyingGlobs()
        {
            for (int i = FlyingGlobs.Count - 1; i >= 0; i--)
            {
                EndpointFlyingGlob glob = FlyingGlobs[i];
                glob.Age++;
                glob.Position += glob.Velocity;
                glob.Velocity *= 0.985f;
                glob.Velocity.Y += 0.075f;
                if (glob.Age >= glob.Life)
                {
                    FlyingGlobs.RemoveAt(i);
                    continue;
                }

                FlyingGlobs[i] = glob;
            }
        }

        private static Vector2 ResolveVisualSurfaceNormal(
            RovaBeam beam,
            Vector2 beamDirection)
        {
            Vector2 collisionNormal = beam.GetSurfaceNormal().SafeNormalize(-beamDirection);
            Point contactTile = beam.GetSurfaceContactTileCoordinates();
            if (contactTile.X < 0
                || contactTile.X >= Main.maxTilesX
                || contactTile.Y < 0
                || contactTile.Y >= Main.maxTilesY)
            {
                return collisionNormal;
            }

            bool topExposed = !IsSolidVisualSurfaceTile(contactTile.X, contactTile.Y - 1);
            bool bottomExposed = !IsSolidVisualSurfaceTile(contactTile.X, contactTile.Y + 1);
            bool leftExposed = !IsSolidVisualSurfaceTile(contactTile.X - 1, contactTile.Y);
            bool rightExposed = !IsSolidVisualSurfaceTile(contactTile.X + 1, contactTile.Y);
            float horizontalTravel = Math.Abs(beamDirection.X);
            float verticalTravel = Math.Abs(beamDirection.Y);

            // At a grass ledge or tile corner, the ray can technically enter
            // through the tile's side even though it is visibly landing on the
            // open ground above it. Prefer that exposed top for downward
            // diagonals, and mirror the rule for ceilings. Shallow impacts
            // continue to read as true wall hits.
            if (beamDirection.Y > 0.001f
                && topExposed
                && verticalTravel >= horizontalTravel * 0.65f)
            {
                return -Vector2.UnitY;
            }

            if (beamDirection.Y < -0.001f
                && bottomExposed
                && verticalTravel >= horizontalTravel * 0.65f)
            {
                return Vector2.UnitY;
            }

            if (beamDirection.X > 0.001f && leftExposed)
                return -Vector2.UnitX;

            if (beamDirection.X < -0.001f && rightExposed)
                return Vector2.UnitX;

            return collisionNormal;
        }

        private static bool IsSolidVisualSurfaceTile(int tileX, int tileY)
        {
            if (tileX < 0
                || tileX >= Main.maxTilesX
                || tileY < 0
                || tileY >= Main.maxTilesY)
            {
                return false;
            }

            Tile tile = Main.tile[tileX, tileY];
            return tile.HasTile
                && Main.tileSolid[tile.TileType]
                && !TileID.Sets.Platforms[tile.TileType];
        }

        private void DrawCapGlobs(
            Texture2D pixel,
            Vector2 capCenter,
            Vector2 direction,
            Vector2 surfaceTangent,
            Vector2 surfaceNormal,
            bool attachedToSurface,
            float widthScale,
            float alpha)
        {
            foreach (EndpointCapGlob glob in CapGlobs)
            {
                float progress = MathHelper.Clamp(glob.Age / glob.Life, 0f, 1f);
                float envelope = MathHelper.Clamp(
                    Math.Min(progress * 6f, (1f - progress) * 3.5f),
                    0f,
                    1f);
                float bubbleWave = 0.5f
                    + (float)Math.Sin(glob.Age * 0.19f + glob.PulsePhase) * 0.5f;
                float pulse = 0.78f + bubbleWave * 0.34f;
                float globRadius = glob.MaxRadius * envelope * pulse * widthScale;
                if (globRadius < 0.5f)
                    continue;

                float drawAlpha = alpha * envelope;
                if (attachedToSurface)
                {
                    float surfaceWobble = (float)Math.Sin(
                        glob.Age * 0.11f + glob.PulsePhase * 1.31f) * 2.2f;
                    float surfaceLift = 0.65f
                        + bubbleWave * 1.55f
                        + glob.BackOffset * 0.1f;
                    Vector2 position = capCenter
                        + surfaceTangent * ((glob.SideOffset + surfaceWobble) * widthScale)
                        + surfaceNormal * (surfaceLift * widthScale);

                    // A few attached globs lift out of the pool and curl
                    // around the beam cap before fading. Most remain low so
                    // the grounded buildup never uncovers the beam endpoint.
                    if (glob.BackOffset > 0f)
                    {
                        float riseProgress = MathHelper.SmoothStep(0f, 1f, progress);
                        float arcDirection = glob.SideVelocity >= 0f ? 1f : -1f;
                        float arcAmount = (float)Math.Sin(progress * MathHelper.Pi)
                            * glob.MaxRadius
                            * 0.72f
                            * arcDirection;
                        position += surfaceNormal
                            * (riseProgress * glob.MaxRadius * 1.25f * widthScale);
                        position += surfaceTangent * (arcAmount * widthScale);
                        globRadius *= 0.92f
                            + (float)Math.Sin(progress * MathHelper.Pi) * 0.18f;
                    }

                    DrawBubblingSurfaceGlob(
                        pixel,
                        position,
                        surfaceTangent,
                        surfaceNormal,
                        globRadius,
                        glob.Age,
                        glob.PulsePhase,
                        glob.Color,
                        drawAlpha);
                }
                else
                {
                    // Free-air endpoint globs stay circular. Surface contact is
                    // the only state that builds an irregular, tangent-spread blob.
                    Vector2 position = capCenter
                        + direction * (glob.BackOffset * widthScale)
                        + surfaceTangent * (glob.SideOffset * widthScale)
                        + surfaceNormal * (bubbleWave * 4.5f * widthScale);
                    RovaLavaGlobVisual.DrawSoftGlob(
                        pixel,
                        position,
                        globRadius,
                        glob.Color * drawAlpha);
                    RovaLavaGlobVisual.DrawSoftGlob(
                        pixel,
                        position,
                        globRadius * 0.42f,
                        RovaBeam.CoreSplashInnerColor * (drawAlpha * 0.88f));
                }
            }
        }

        private void DrawFlyingGlobs(Texture2D pixel, float widthScale, float alpha)
        {
            foreach (EndpointFlyingGlob glob in FlyingGlobs)
            {
                float progress = MathHelper.Clamp(glob.Age / glob.Life, 0f, 1f);
                float envelope = MathHelper.Clamp(
                    Math.Min(progress * 5f, (1f - progress) * 3f),
                    0f,
                    1f);
                float pulse = 0.88f
                    + (float)Math.Sin(glob.Age * 0.23f + glob.PulsePhase) * 0.12f;
                float radius = glob.MaxRadius * envelope * pulse * widthScale;
                if (radius < 0.5f)
                    continue;

                Vector2 position = glob.Position - Main.screenPosition;
                Vector2 velocityDirection = glob.Velocity.SafeNormalize(-Vector2.UnitY);
                Vector2 trailingPosition = position - velocityDirection * (radius * 0.72f);
                float drawAlpha = alpha * envelope;
                RovaLavaGlobVisual.DrawSoftGlob(
                    pixel,
                    trailingPosition,
                    radius * 0.58f,
                    glob.Color * (drawAlpha * 0.72f));
                RovaLavaGlobVisual.DrawSoftGlob(
                    pixel,
                    position,
                    radius,
                    glob.Color * drawAlpha);
                RovaLavaGlobVisual.DrawSoftGlob(
                    pixel,
                    position,
                    radius * 0.42f,
                    RovaBeam.CoreSplashInnerColor * (drawAlpha * 0.88f));
            }
        }

        private void DrawPersistentCap(
            Texture2D pixel,
            Vector2 capCenter,
            Vector2 surfaceTangent,
            Vector2 surfaceNormal,
            bool attachedToSurface,
            float widthScale,
            float alpha,
            float time)
        {
            Color centerColor = PersistentCapCenterColor.A > 0
                ? PersistentCapCenterColor
                : new Color(255, 229, 66);
            if (!attachedToSurface)
            {
                float pulse = 0.9f + (float)Math.Sin(time * 0.18f) * 0.12f;
                float radius = 14.5f * widthScale * pulse;
                RovaLavaGlobVisual.DrawSoftGlob(
                    pixel,
                    capCenter,
                    radius,
                    centerColor * (0.96f * alpha));
                RovaLavaGlobVisual.DrawSoftGlob(
                    pixel,
                    capCenter,
                    radius * 0.43f,
                    RovaBeam.CoreSplashInnerColor * (0.88f * alpha));
                return;
            }

            float centerPulse = 0.92f + (float)Math.Sin(time * 0.18f) * 0.09f;
            DrawBubblingSurfaceGlob(
                pixel,
                capCenter,
                surfaceTangent,
                surfaceNormal,
                10.8f * widthScale * centerPulse,
                time,
                0.35f,
                centerColor,
                0.96f * alpha);

            for (int side = -1; side <= 1; side += 2)
            {
                float sideWave = (float)Math.Sin(time * 0.16f + side * 1.7f);
                float sideRadius = 8.8f * widthScale * (0.92f + sideWave * 0.1f);
                Vector2 sidePosition = capCenter
                    + surfaceTangent * (side * (10.5f + sideWave * 1.1f) * widthScale)
                    + surfaceNormal * ((0.65f + (sideWave + 1f) * 0.45f) * widthScale);
                Color sideColor = side < 0
                    ? PersistentCapNegativeColor
                    : PersistentCapPositiveColor;
                if (sideColor.A <= 0)
                    sideColor = new Color(255, 229, 66);

                DrawBubblingSurfaceGlob(
                    pixel,
                    sidePosition,
                    surfaceTangent,
                    surfaceNormal,
                    sideRadius,
                    time,
                    side * 1.85f,
                    sideColor,
                    0.9f * alpha);
            }
        }

        private static void DrawBubblingSurfaceGlob(
            Texture2D pixel,
            Vector2 center,
            Vector2 surfaceTangent,
            Vector2 surfaceNormal,
            float radius,
            float time,
            float phase,
            Color outerColor,
            float alpha)
        {
            if (radius < 0.5f || alpha <= 0.005f)
                return;

            surfaceTangent = surfaceTangent.SafeNormalize(Vector2.UnitX);
            surfaceNormal = surfaceNormal.SafeNormalize(-Vector2.UnitY);
            float leftWave = (float)Math.Sin(time * 0.14f + phase);
            float rightWave = (float)Math.Sin(time * 0.17f + phase + 2.1f);
            float bubbleWave = (float)Math.Sin(time * 0.21f + phase + 4.2f);
            Vector2 bodyCenter = center
                + surfaceTangent * (leftWave * radius * 0.06f);
            Vector2 leftCenter = bodyCenter
                - surfaceTangent * (radius * (0.34f + rightWave * 0.04f))
                + surfaceNormal * (radius * (0.04f + leftWave * 0.05f));
            Vector2 rightCenter = bodyCenter
                + surfaceTangent * (radius * (0.37f + leftWave * 0.05f))
                + surfaceNormal * (radius * (0.01f + rightWave * 0.04f));
            Vector2 bubbleCenter = bodyCenter
                + surfaceTangent * (bubbleWave * radius * 0.18f)
                + surfaceNormal * (radius * (0.31f + bubbleWave * 0.08f));
            float bodyRadius = radius * (0.74f + bubbleWave * 0.04f);
            float leftRadius = radius * (0.58f + leftWave * 0.08f);
            float rightRadius = radius * (0.62f + rightWave * 0.08f);
            float bubbleRadius = radius * (0.43f + bubbleWave * 0.07f);

            RovaLavaGlobVisual.DrawSoftGlob(
                pixel,
                leftCenter,
                leftRadius,
                outerColor * (alpha * 0.9f));
            RovaLavaGlobVisual.DrawSoftGlob(
                pixel,
                rightCenter,
                rightRadius,
                outerColor * (alpha * 0.92f));
            RovaLavaGlobVisual.DrawSoftGlob(
                pixel,
                bodyCenter,
                bodyRadius,
                outerColor * alpha);
            RovaLavaGlobVisual.DrawSoftGlob(
                pixel,
                bubbleCenter,
                bubbleRadius,
                outerColor * (alpha * 0.94f));
            RovaLavaGlobVisual.DrawSoftGlob(
                pixel,
                bodyCenter + surfaceNormal * (bodyRadius * 0.08f),
                bodyRadius * 0.42f,
                RovaBeam.CoreSplashInnerColor * (alpha * 0.88f));
            RovaLavaGlobVisual.DrawSoftGlob(
                pixel,
                bubbleCenter,
                bubbleRadius * 0.4f,
                RovaBeam.CoreSplashInnerColor * (alpha * 0.78f));
        }
    }
}
