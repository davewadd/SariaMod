using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SariaMod.Buffs;
using SariaMod.Dusts;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace SariaMod.Items.Strange
{
    public class LocatorPellet : ModProjectile
    {
        private const int PelletLifetime = 600;
        private const int StartupDuration = 100;
        private const int StartupEndTimeLeft = PelletLifetime - StartupDuration;

        private class GreenLightTriangle
        {
            public Vector2 Position;
            public float Rotation;
            public float Size;
            public float Age;
            public float Lifetime;
        }

        private List<GreenLightTriangle> greenTriangles = new List<GreenLightTriangle>();
        private int triangleSpawnTimer;

        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Locator Pellet");
            Main.projFrames[base.Projectile.type] = 1;
            ProjectileID.Sets.MinionShot[base.Projectile.type] = true;
            ProjectileID.Sets.TrailingMode[base.Projectile.type] = 0;
            ProjectileID.Sets.TrailCacheLength[base.Projectile.type] = 30;
        }
        public override void SetDefaults()
        {
            base.Projectile.width = 20;
            base.Projectile.height = 20;
            base.Projectile.netImportant = true;
            base.Projectile.friendly = true;
            base.Projectile.ignoreWater = true;
            base.Projectile.usesLocalNPCImmunity = true;
            base.Projectile.localNPCHitCooldown = 7;
            base.Projectile.minionSlots = 0f;
            base.Projectile.extraUpdates = 1;
            base.Projectile.penetrate = 1;
            base.Projectile.tileCollide = false;
            base.Projectile.timeLeft = PelletLifetime;
            base.Projectile.minion = true;
        }
        public override bool? CanCutTiles()
        {
            return false;
        }
        public override bool MinionContactDamage()
        {
            return true;
        }
        private const int sphereRadius = 3;
        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            Player player = Main.player[Projectile.owner];
            target.buffImmune[BuffID.CursedInferno] = false;
            target.buffImmune[BuffID.Confused] = false;
            target.buffImmune[BuffID.Slow] = false;
            target.buffImmune[BuffID.ShadowFlame] = false;
            target.buffImmune[BuffID.Ichor] = false;
            target.buffImmune[ModContent.BuffType<Burning2>()] = false;
            target.buffImmune[BuffID.Frostburn] = false;
            target.buffImmune[BuffID.Poisoned] = false;
            target.buffImmune[BuffID.Venom] = false;
            target.buffImmune[BuffID.Electrified] = false;
            target.AddBuff(BuffID.Slow, 300);
            target.AddBuff(ModContent.BuffType<SariaCurse2>(), 50);

            SoundEngine.PlaySound(SoundID.DD2_WitherBeastDeath, base.Projectile.Center);
            FairyPlayer modPlayer = player.Fairy();
            modPlayer.SariaXp++;
            for (int j = 0; j < 1; j++)
            {
                if (Main.myPlayer == Projectile.owner)
                {
                    global::SariaMod.Gores.LocatorShard.Spawn(
                        Projectile.GetSource_FromThis(),
                        base.Projectile.Center + Utils.RandomVector2(Main.rand, 0f, 0f),
                        Vector2.One.RotatedByRandom(MathHelper.TwoPi) * 4f,
                        Projectile.owner);
                }
            }
            knockback /= 4;
            Projectile.Kill();
        }
        public override void AI()
        {
            Projectile.scale = .5f;
            Projectile.knockBack = 10;

            // Calculate projectile speed for spawn rate scaling
            float speed = Projectile.velocity.Length();

            // Spawn colored dust triangles along trail - spawn less when moving slower
            triangleSpawnTimer++;
            int spawnThreshold = speed < 20f ? 8 : 5; // Slower = less frequent
            
            if (triangleSpawnTimer >= spawnThreshold && Projectile.oldPos.Length > 0)
            {
                triangleSpawnTimer = 0;
                
                // Pick random position along trail
                int randomTrailIndex = Main.rand.Next(Projectile.oldPos.Length);
                Vector2 trailPos = Projectile.oldPos[randomTrailIndex] + Projectile.Size * 0.5f;
                
                if (trailPos != Vector2.Zero)
                {
                    float randomSize = Main.rand.NextFloat(1f, 2.5f);
                    float randomRotation = Main.rand.NextFloat(0f, MathHelper.TwoPi);
                    
                    // Randomly cycle between green, cyan, and purple
                    Color[] colors = { Color.LimeGreen, Color.Cyan, new Color(200, 100, 255) }; // purple
                    Color triangleColor = colors[Main.rand.Next(colors.Length)];

                    LocatorLightShapeVisuals.SpawnSmallDustTriangle(
                        trailPos,
                        Projectile.velocity * 0.15f,
                        randomRotation,
                        randomSize,
                        triangleColor);
                }
            }

            // Spawn Locator-style lightning dust only after the startup phase.
            if (Projectile.timeLeft <= StartupEndTimeLeft && Main.rand.NextBool(12))
            {
                float lightningSize = Main.rand.NextFloat(4f, 9f);
                Vector2 startPos = Projectile.Center + Main.rand.NextVector2Circular(6f, 6f);
                Vector2 currentPos = startPos;
                
                int segments = Main.rand.Next(4, 8);
                for (int seg = 0; seg < segments; seg++)
                {
                    float angle = Projectile.velocity.ToRotation() + Main.rand.NextFloat(-0.8f, 0.8f);
                    Vector2 nextPos = currentPos + new Vector2(
                        (float)Math.Cos(angle) * lightningSize,
                        (float)Math.Sin(angle) * lightningSize
                    );
                    
                    int points = 3;
                    for (int p = 0; p <= points; p++)
                    {
                        float t = p / (float)points;
                        Vector2 boltPos = Vector2.Lerp(currentPos, nextPos, t);
                        
                        float fade = 1f - (seg / (float)segments);
                        Color purpleColor = Color.Lerp(Color.DarkViolet, Color.MediumPurple, t);

                        if (!VisualDustLimiter.TryReserveHalfCapacitySlot())
                        {
                            continue;
                        }

                        Dust lightning = Dust.NewDustPerfect(
                            boltPos,
                            267,
                            Projectile.velocity * 0.25f,
                            0,
                            purpleColor,
                            Main.rand.NextFloat(1.0f, 1.6f)
                        );
                        lightning.noGravity = true;
                        lightning.fadeIn = 1.5f;
                        lightning.color = purpleColor * fade;
                    }
                    
                    currentPos = nextPos;
                }
            }

            bool startupComplete = Projectile.timeLeft <= StartupEndTimeLeft;
            if (!startupComplete)
            {
                Projectile.rotation += 0.095f;
            }

            float overlapVelocity = 0.04f;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile other = Main.projectile[i];
                if (i != Projectile.whoAmI && other.active && other.owner == Projectile.owner && Math.Abs(Projectile.position.X - other.position.X) + Math.Abs(Projectile.position.Y - other.position.Y) < Projectile.width)
                {
                    if (Projectile.position.X < other.position.X) Projectile.velocity.X -= overlapVelocity;
                    else Projectile.velocity.X += overlapVelocity;
                    if (Projectile.position.Y < other.position.Y) Projectile.velocity.Y -= overlapVelocity;
                    else Projectile.velocity.Y += overlapVelocity;
                }
            }

            if (Projectile.timeLeft == PelletLifetime)
            {
                SoundEngine.PlaySound(SoundID.DD2_SkyDragonsFuryShot, base.Projectile.Center);
            }

            if (!startupComplete)
            {
                Projectile.friendly = false;
                Projectile.velocity *= 0.985f;
                return;
            }

            NPC target = Projectile.Center.ClosestNPCAt(1000f);
            Projectile.friendly = target != null;
            if (target == null)
            {
                Projectile closestField = FindClosestField();
                if (closestField != null)
                {
                    OrbitFieldClockwise(closestField);
                }
                else
                {
                    Projectile.velocity *= 0.985f;
                }
                return;
            }

            Projectile.ai[0] = 0f;
            Projectile.ai[1] = 0f;
            const float homingSpeed = 70f;
            const float inertia = 20f;
            Vector2 desiredVelocity = Projectile.DirectionTo(target.Center) * homingSpeed;
            Projectile.velocity = (Projectile.velocity * (inertia - 2f) + desiredVelocity) / inertia;
            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        private Projectile FindClosestField()
        {
            Projectile closestField = null;
            float closestDistanceSquared = float.MaxValue;

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile field = Main.projectile[i];
                if (!PsychicFieldSystem.IsActiveField(field))
                {
                    continue;
                }

                float distanceSquared = Vector2.DistanceSquared(Projectile.Center, field.Center);
                if (distanceSquared < closestDistanceSquared)
                {
                    closestDistanceSquared = distanceSquared;
                    closestField = field;
                }
            }

            return closestField;
        }

        private void OrbitFieldClockwise(Projectile field)
        {
            int fieldMarker = field.whoAmI + 1;
            Vector2 radialOffset = Projectile.Center - field.Center;
            float distance = radialOffset.Length();

            if ((int)Projectile.ai[1] != fieldMarker || Projectile.ai[0] <= 0f)
            {
                Projectile.ai[0] = MathHelper.Clamp(distance, 96f, PsychicFieldSystem.FieldRadius * 0.85f);
                Projectile.ai[1] = fieldMarker;
                Projectile.netUpdate = true;
            }

            Vector2 radialDirection = radialOffset.SafeNormalize(Vector2.UnitX);
            Vector2 clockwiseTangent = radialDirection.RotatedBy(MathHelper.PiOver2);
            float radialError = distance - Projectile.ai[0];
            Vector2 radialCorrection = -radialDirection * MathHelper.Clamp(radialError * 0.12f, -8f, 8f);

            const float orbitSpeed = 22f;
            const float orbitInertia = 20f;
            Vector2 desiredVelocity = clockwiseTangent * orbitSpeed + radialCorrection;
            Projectile.velocity = (Projectile.velocity * (orbitInertia - 1f) + desiredVelocity) / orbitInertia;
            Projectile.rotation = Projectile.velocity.ToRotation();
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D trailTexture = TextureAssets.MagicPixel.Value;
            Vector2 centerOffset = Projectile.Size * 0.5f;

            if (Projectile.timeLeft >= PelletLifetime - 5)
            {
                DrawHexagonHeadWithSparks();
                return false;
            }

            // Draw trail
            for (int i = 1; i < Projectile.oldPos.Length; i++)
            {
                Vector2 p0 = Projectile.oldPos[i - 1];
                Vector2 p1 = Projectile.oldPos[i];

                if (p0 == Vector2.Zero || p1 == Vector2.Zero)
                {
                    break;
                }

                float distFromCurrent = Vector2.Distance(p0 + centerOffset, Projectile.Center);
                if (distFromCurrent > 5000f)
                {
                    break;
                }

                p0 += centerOffset;
                p1 += centerOffset;

                Vector2 diff = p0 - p1;
                float len = diff.Length();
                if (len <= 0.001f)
                {
                    continue;
                }

                float t = i / (float)Projectile.oldPos.Length;
                float inverseLerp = 1f - t;
                float frontFlatten = (float)Math.Pow(inverseLerp, 2.5);

                float width = MathHelper.Lerp(6f, 1f, t) * Projectile.scale;
                width *= MathHelper.Lerp(frontFlatten, 1f, t * 0.15f);

                float alpha = MathHelper.Lerp(0.9f, 0f, t);

                Color c = Color.Lerp(Color.DeepPink, Color.HotPink, t * 0.5f);
                c = Color.Lerp(c, Color.LightPink, t);
                c = Color.Lerp(c, Color.Transparent, t);
                c *= alpha;

                float rotation = diff.ToRotation();
                Vector2 drawPos = (p1 + p0) * 0.5f - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
                Vector2 scale = new Vector2(len, width);

                Main.spriteBatch.Draw(
                    trailTexture,
                    drawPos,
                    new Rectangle(0, 0, 1, 1),
                    Projectile.GetAlpha(c),
                    rotation,
                    new Vector2(0.5f, 0.5f),
                    scale,
                    SpriteEffects.None,
                    0f);

                Lighting.AddLight((p1 + p0) * 0.5f, Color.DeepPink.ToVector3() * (0.4f * alpha));

                // Draw connecting segments to smooth out heavy rotation
                if (i > 1)
                {
                    Vector2 p_prev = Projectile.oldPos[i - 2] + centerOffset;
                    Vector2 connector = p1 - p_prev;
                    float connLen = connector.Length();
                    
                    if (connLen > 0.1f && connLen < 200f)
                    {
                        Vector2 connMid = (p_prev + p1) * 0.5f - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
                        float connRot = connector.ToRotation();
                        float connWidth = width * 0.6f;
                        Color connColor = c * 0.7f;
                        
                        Main.spriteBatch.Draw(
                            trailTexture,
                            connMid,
                            new Rectangle(0, 0, 1, 1),
                            Projectile.GetAlpha(connColor),
                            connRot,
                            new Vector2(0.5f, 0.5f),
                            new Vector2(connLen, connWidth),
                            SpriteEffects.None,
                            0f);
                    }
                }
            }

            // Draw hexagon head with electric sparks
            DrawHexagonHeadWithSparks();

            return false;
        }

        private void DrawGreenLightTriangles(Texture2D pixelTexture)
        {
            foreach (GreenLightTriangle tri in greenTriangles)
            {
                float ageProgress = tri.Age / tri.Lifetime;
                float alpha = MathHelper.Lerp(1f, 0f, ageProgress);
                
                Vector2 screenPos = tri.Position - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
                float size = tri.Size;
                float rot = tri.Rotation;
                
                Vector2 p1 = screenPos + new Vector2((float)Math.Cos(rot), (float)Math.Sin(rot)) * size;
                Vector2 p2 = screenPos + new Vector2((float)Math.Cos(rot + MathHelper.TwoPi / 3f), (float)Math.Sin(rot + MathHelper.TwoPi / 3f)) * size;
                Vector2 p3 = screenPos + new Vector2((float)Math.Cos(rot + MathHelper.TwoPi * 2f / 3f), (float)Math.Sin(rot + MathHelper.TwoPi * 2f / 3f)) * size;
                
                Color greenColor = Color.LimeGreen * alpha;
                
                DrawTriangleLine(p1, p2, greenColor, 1.5f, pixelTexture);
                DrawTriangleLine(p2, p3, greenColor, 1.5f, pixelTexture);
                DrawTriangleLine(p3, p1, greenColor, 1.5f, pixelTexture);
                
                Lighting.AddLight(tri.Position, greenColor.ToVector3() * 0.3f);
            }
        }

        private void DrawTriangleLine(Vector2 a, Vector2 b, Color color, float width, Texture2D texture)
        {
            Vector2 diff = b - a;
            float len = diff.Length();
            if (len <= 0.001f)
                return;

            float rot = diff.ToRotation();
            Vector2 mid = (a + b) * 0.5f;

            Main.spriteBatch.Draw(
                texture,
                mid,
                new Rectangle(0, 0, 1, 1),
                Projectile.GetAlpha(color),
                rot,
                new Vector2(0.5f, 0.5f),
                new Vector2(len, width),
                SpriteEffects.None,
                0f);
        }

        private void DrawHexagonHeadWithSparks()
        {
            Texture2D trailTexture = TextureAssets.MagicPixel.Value;
            float t = Main.GlobalTimeWrappedHourly;

            Vector2 hexCenter = Projectile.Center;
            float hexRadius = 8f * Projectile.scale;
            float polyRotation = t * 4.5f;
            float lineWidth = 1.75f * Projectile.scale;

            Color hexColor = Color.Lerp(Color.DeepPink, Color.HotPink, (float)Math.Sin(t * 2.2f) * 0.5f + 0.5f);
            
            const int sides = 6;
            Span<Vector2> pts = stackalloc Vector2[sides];
            for (int i = 0; i < sides; i++)
            {
                float a = polyRotation + MathHelper.TwoPi * (i / (float)sides);
                pts[i] = hexCenter + new Vector2(hexRadius, 0f).RotatedBy(a);
            }

            Color filledColor = Color.MediumPurple * 0.7f;
            
            for (int i = 1; i < sides - 1; i++)
            {
                DrawLightLine(hexCenter, pts[i], filledColor * 0.5f, lineWidth * 2f, trailTexture);
            }

            for (int i = 0; i < sides; i++)
            {
                Vector2 a = pts[i];
                Vector2 b = pts[(i + 1) % sides];
                DrawLightLine(a, b, hexColor, lineWidth, trailTexture);
            }

            Color innerColor = Color.Lerp(hexColor, Color.White, 0.5f) * 0.9f;
            for (int i = 0; i < sides; i++)
            {
                Vector2 a = pts[i];
                Vector2 b = pts[(i + 1) % sides];
                DrawLightLine(a, b, innerColor, lineWidth * 0.45f, trailTexture);
            }

            Lighting.AddLight(hexCenter, hexColor.ToVector3() * 0.65f);

            DrawExpandingHexagon(hexCenter, hexRadius, polyRotation, sides, trailTexture, lineWidth);
        }

        private void DrawExpandingHexagon(Vector2 center, float baseRadius, float rotation, int sides, Texture2D texture, float baseLineWidth)
        {
            float t = Main.GlobalTimeWrappedHourly;
            
            float animationTime = (PelletLifetime - Projectile.timeLeft) * 0.01f;
            float expandProgress = animationTime % 1.5f;
            
            float expandingRadius = MathHelper.Lerp(baseRadius, baseRadius * 2.5f, expandProgress);
            float expandingAlpha = MathHelper.Clamp(1f - (expandProgress * 0.8f), 0f, 1f);
            
            Color expandingColor;
            if (expandProgress < 0.5f)
            {
                float colorT = expandProgress * 2f;
                Color startColor = Color.Lerp(Color.DeepPink, Color.HotPink, (float)Math.Sin(t * 2.2f) * 0.5f + 0.5f);
                expandingColor = Color.Lerp(startColor, Color.White, colorT);
            }
            else
            {
                float colorT = (expandProgress - 0.5f) * 2f;
                expandingColor = Color.Lerp(Color.White, Color.MediumPurple, colorT);
            }
            
            expandingColor *= expandingAlpha;
            
            Span<Vector2> expandPts = stackalloc Vector2[sides];
            for (int i = 0; i < sides; i++)
            {
                float a = rotation + MathHelper.TwoPi * (i / (float)sides);
                expandPts[i] = center + new Vector2(expandingRadius, 0f).RotatedBy(a);
            }

            for (int i = 0; i < sides; i++)
            {
                Vector2 a = expandPts[i];
                Vector2 b = expandPts[(i + 1) % sides];
                DrawLightLine(a, b, expandingColor, baseLineWidth * 0.8f, texture);
            }

            Color innerExpanding = Color.Lerp(expandingColor, Color.White, 0.4f) * (expandingAlpha * 0.6f);
            for (int i = 0; i < sides; i++)
            {
                Vector2 a = expandPts[i];
                Vector2 b = expandPts[(i + 1) % sides];
                DrawLightLine(a, b, innerExpanding, baseLineWidth * 0.3f, texture);
            }

            Lighting.AddLight(center, expandingColor.ToVector3() * expandingAlpha * 0.4f);
        }

        private void DrawLightLine(Vector2 a, Vector2 b, Color color, float width, Texture2D texture)
        {
            Vector2 diff = b - a;
            float len = diff.Length();
            if (len <= 0.001f)
                return;

            float rot = diff.ToRotation();
            Vector2 mid = (a + b) * 0.5f - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);

            Main.spriteBatch.Draw(
                texture,
                mid,
                new Rectangle(0, 0, 1, 1),
                Projectile.GetAlpha(color),
                rot,
                new Vector2(0.5f, 0.5f),
                new Vector2(len, width),
                SpriteEffects.None,
                0f);
        }
    }
}
