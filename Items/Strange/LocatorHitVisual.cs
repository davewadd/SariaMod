using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.Strange
{
    public class LocatorHitVisual : ModProjectile
    {
        private int effectTimer;
        private float hexagonMaxRadius;
        private float innerHexagonRadius;
        private float maxRadiusScaled;
        
        private class PsychicBall
        {
            public Vector2 Position;
            public Vector2 StartPosition;
            public Vector2 Direction;
            public float Speed;
            public float MaxDistance;
            public float Lifetime;
            public float MaxLifetime;
            public Vector2[] Trail = new Vector2[10];
            public int TrailIndex = 0;
            public bool ReturnsToSaria = false;
            public Vector2 SariaPosition = Vector2.Zero;
        }
        
        private PsychicBall[] psychicBalls;

        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Locator Hit");
            ProjectileID.Sets.MinionShot[base.Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            base.Projectile.width = 20;
            base.Projectile.height = 20;
            base.Projectile.friendly = false;
            base.Projectile.tileCollide = false;
            base.Projectile.ignoreWater = true;
            base.Projectile.timeLeft = 60;
            base.Projectile.penetrate = -1;
        }

        public override bool? CanHitNPC(NPC target)
        {
            return false;
        }

        public override bool? CanCutTiles()
        {
            return false;
        }

        public override void AI()
        {
            // Initialize on first frame
            if (Projectile.timeLeft == 60)
            {
                SoundEngine.PlaySound(SoundID.DD2_WitherBeastDeath, Projectile.Center);
                
                // Scale max size based on nearby enemy size
                NPC targetEnemy = null;
                float closestDist = 200f;
                foreach (NPC npc in Main.npc)
                {
                    if (npc.active && !npc.friendly)
                    {
                        float dist = Vector2.Distance(Projectile.Center, npc.Center);
                        if (dist < closestDist)
                        {
                            closestDist = dist;
                            targetEnemy = npc;
                        }
                    }
                }
                
                if (targetEnemy != null)
                {
                    // Scale based on enemy hitbox size - much smaller now
                    float enemySize = (targetEnemy.width + targetEnemy.height) * 0.5f;
                    maxRadiusScaled = MathHelper.Clamp(enemySize * 0.4f, 30f, 60f);
                }
                else
                {
                    maxRadiusScaled = 45f;
                }
                
                // Spawn psychic balls (3-6 balls at random angles)
                int ballCount = Main.rand.Next(3, 7);
                psychicBalls = new PsychicBall[ballCount];
                for (int i = 0; i < ballCount; i++)
                {
                    float angle = Main.rand.NextFloat(0f, MathHelper.TwoPi);
                    Vector2 direction = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                    
                    psychicBalls[i] = new PsychicBall
                    {
                        Position = Projectile.Center,
                        StartPosition = Projectile.Center,
                        Direction = direction,
                        Speed = Main.rand.NextFloat(3f, 6f),
                        MaxDistance = Main.rand.NextFloat(80f, 150f),
                        Lifetime = 0f,
                        MaxLifetime = 60f,
                        TrailIndex = 0,
                        ReturnsToSaria = false,
                        SariaPosition = Vector2.Zero
                    };
                    
                    // Initialize trail
                    for (int t = 0; t < 10; t++)
                    {
                        psychicBalls[i].Trail[t] = Projectile.Center;
                    }
                }
            }

            // Keep projectile stationary at impact point
            Projectile.velocity = Vector2.Zero;

            effectTimer++;

            // Update psychic balls
            if (psychicBalls != null)
            {
                foreach (PsychicBall ball in psychicBalls)
                {
                    ball.Lifetime += 1f;
                    float ballProgress = ball.Lifetime / ball.MaxLifetime;
                    
                    // Normal out-and-back with orbital sweep
                    float distanceProgress;
                    float sweepAngle = 0f;
                    
                    if (ballProgress < 0.5f)
                    {
                        distanceProgress = ballProgress * 2f; // 0 to 1 (going out)
                    }
                    else
                    {
                        // Coming back with orbital sweep
                        float returnProgress = (ballProgress - 0.5f) * 2f; // 0 to 1
                        distanceProgress = 1f - returnProgress; // 1 to 0
                        
                        // Orbital sweep angle - rotates as it returns
                        sweepAngle = returnProgress * MathHelper.TwoPi * 1.5f;
                    }
                    
                    float currentDistance = ball.MaxDistance * distanceProgress;
                    
                    // Apply sweep rotation to direction
                    Vector2 currentDirection = ball.Direction.RotatedBy(sweepAngle);
                    ball.Position = ball.StartPosition + currentDirection * currentDistance;
                    
                    // Update trail
                    ball.Trail[ball.TrailIndex] = ball.Position;
                    ball.TrailIndex = (ball.TrailIndex + 1) % 10;
                }
            }

            // Faster animation (60 frames instead of 120)
            float effectProgress = 1f - (Projectile.timeLeft / 60f);
            
            if (effectProgress < 0.75f)
            {
                // First 75%: grow from 0 to max with shake
                float growthProgress = effectProgress / 0.75f;
                float easedProgress = 1f - (float)Math.Pow(1f - growthProgress, 3);
                
                // Outer hexagon grows faster
                float outerTarget = MathHelper.Lerp(0f, maxRadiusScaled, easedProgress);
                float outerShake = (float)Math.Sin(effectTimer * 0.5f) * MathHelper.Lerp(12f, 1f, easedProgress);
                hexagonMaxRadius = outerTarget + outerShake;
                
                // Inner hexagon grows slower and offset
                float innerGrowthProgress = MathHelper.Clamp(growthProgress - 0.2f, 0f, 1f) / 0.8f;
                float innerTarget = MathHelper.Lerp(0f, maxRadiusScaled * 0.55f, easedProgress * 0.7f);
                float innerShake = (float)Math.Sin(effectTimer * 0.4f + 1f) * MathHelper.Lerp(8f, 0.5f, innerGrowthProgress);
                innerHexagonRadius = innerTarget + innerShake;
            }
            else
            {
                // Last 25%: shrink and fade
                float shrinkProgress = (effectProgress - 0.75f) / 0.25f;
                hexagonMaxRadius = MathHelper.Lerp(maxRadiusScaled, 0f, shrinkProgress);
                innerHexagonRadius = MathHelper.Lerp(maxRadiusScaled * 0.55f, 0f, shrinkProgress);
            }

            Lighting.AddLight(Projectile.Center, Color.DeepPink.ToVector3() * 0.5f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // Draw trails first
            DrawPsychicBallTrails();
            
            // Draw lightning effect
            DrawLightning();
            
            // Draw hexagon effect
            DrawHexagon();
            
            // Draw psychic balls
            DrawPsychicBalls();

            return false;
        }
        
        private void DrawPsychicBallTrails()
        {
            if (psychicBalls == null)
                return;
            
            Texture2D trailTexture = TextureAssets.MagicPixel.Value;
            
            foreach (PsychicBall ball in psychicBalls)
            {
                float ballProgress = ball.Lifetime / ball.MaxLifetime;
                float ballAlpha = 1f - ballProgress;
                
                // Draw trail - tapers to point at the end, brighter but no lighting
                for (int i = 0; i < 9; i++)
                {
                    Vector2 pos1 = ball.Trail[i];
                    Vector2 pos2 = ball.Trail[(i + 1) % 10];
                    
                    Vector2 diff = pos2 - pos1;
                    float len = diff.Length();
                    if (len < 0.1f) continue;
                    
                    float rot = diff.ToRotation();
                    Vector2 mid = (pos1 + pos2) * 0.5f - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
                    
                    // Trail tapers: thinner at start, thick at end, more transparent
                    float trailAlpha = ballAlpha * MathHelper.Lerp(0.3f, 0.7f, i / 9f);
                    float trailWidth = MathHelper.Lerp(1f, 8f, i / 9f);
                    
                    // Color transitions from purple to pink along the trail
                    float trailColorT = i / 9f;
                    Color trailColor = Color.Lerp(Color.MediumPurple, Color.HotPink, trailColorT);
                    trailColor *= trailAlpha;
                    
                    Main.spriteBatch.Draw(
                        trailTexture,
                        mid,
                        new Rectangle(0, 0, 1, 1),
                        trailColor,
                        rot,
                        new Vector2(0.5f, 0.5f),
                        new Vector2(len, trailWidth),
                        SpriteEffects.None,
                        0f);
                }
            }
        }
        
        private void DrawPsychicBalls()
        {
            if (psychicBalls == null)
                return;
            
            // Use circle texture for round appearance
            Texture2D circleTexture = TextureAssets.Extra[98].Value;
            
            foreach (PsychicBall ball in psychicBalls)
            {
                float ballProgress = ball.Lifetime / ball.MaxLifetime;
                float ballAlpha = 1f - ballProgress;
                
                // Color transitions from purple to pink over lifetime
                Color ballColor = Color.Lerp(Color.MediumPurple, Color.HotPink, ballProgress);
                
                Vector2 screenPos = ball.Position - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
                
                // Much larger, more pronounced glow - brighter but more transparent
                float glowSize = 24f;
                
                // Apply alpha - much more transparent (50% of previous)
                Color outerGlow = ballColor * 0.5f;
                outerGlow.A = (byte)(255 * ballAlpha * 0.5f);
                
                Color midGlow = ballColor * 0.8f;
                midGlow.A = (byte)(255 * ballAlpha * 0.6f);
                
                Color coreGlow = ballColor;
                coreGlow.A = (byte)(255 * ballAlpha * 0.8f);
                
                // Draw outer glow (much larger)
                Main.spriteBatch.Draw(
                    circleTexture,
                    screenPos,
                    null,
                    outerGlow,
                    0f,
                    new Vector2(circleTexture.Width * 0.5f, circleTexture.Height * 0.5f),
                    glowSize * 1.5f / (float)circleTexture.Width,
                    SpriteEffects.None,
                    0f);
                
                // Draw mid glow
                Main.spriteBatch.Draw(
                    circleTexture,
                    screenPos,
                    null,
                    midGlow,
                    0f,
                    new Vector2(circleTexture.Width * 0.5f, circleTexture.Height * 0.5f),
                    glowSize * 0.9f / (float)circleTexture.Width,
                    SpriteEffects.None,
                    0f);
                
                // Draw bright core
                Main.spriteBatch.Draw(
                    circleTexture,
                    screenPos,
                    null,
                    coreGlow,
                    0f,
                    new Vector2(circleTexture.Width * 0.5f, circleTexture.Height * 0.5f),
                    glowSize * 0.6f / (float)circleTexture.Width,
                    SpriteEffects.None,
                    0f);
                
                // No lighting from the orbs
            }
        }

        private void DrawHexagon()
        {
            if (hexagonMaxRadius <= 0.5f)
                return;

            Texture2D trailTexture = TextureAssets.MagicPixel.Value;
            float effectProgress = 1f - (Projectile.timeLeft / 60f);
            
            // Keep hexagon fully visible during growth, fade only during shrink
            float hexAlpha = effectProgress < 0.75f ? 0.6f : MathHelper.Lerp(0.6f, 0f, (effectProgress - 0.75f) / 0.25f);

            // Cycle through cyan, purple, white
            float t = Main.GlobalTimeWrappedHourly;
            float colorCycle = (float)(0.5 + 0.5 * Math.Sin(t * 2.2f));
            Color hexColor = Color.Lerp(Color.Cyan, Color.MediumPurple, colorCycle);
            
            // Add white highlights
            float whiteAmount = (float)Math.Abs(Math.Sin(t * 3.5f)) * 0.3f;
            hexColor = Color.Lerp(hexColor, Color.White, whiteAmount);
            hexColor *= hexAlpha;

            float polyRotation = t * 4.5f;
            float lineWidth = 4.5f;

            const int sides = 6;
            Span<Vector2> pts = stackalloc Vector2[sides];
            for (int i = 0; i < sides; i++)
            {
                float a = polyRotation + MathHelper.TwoPi * (i / (float)sides);
                pts[i] = Projectile.Center + new Vector2(hexagonMaxRadius, 0f).RotatedBy(a);
            }

            // Draw outer hexagon - bright lines
            for (int i = 0; i < sides; i++)
            {
                Vector2 a = pts[i];
                Vector2 b = pts[(i + 1) % sides];
                DrawLightLine(a, b, hexColor, lineWidth, trailTexture);
            }

            // Draw outer hexagon - inner subtle lines
            Color innerOuter = Color.Lerp(hexColor, Color.White, 0.5f) * 0.9f;
            for (int i = 0; i < sides; i++)
            {
                Vector2 a = pts[i];
                Vector2 b = pts[(i + 1) % sides];
                DrawLightLine(a, b, innerOuter, lineWidth * 0.5f, trailTexture);
            }

            // Draw inner hexagon with separate radius
            if (innerHexagonRadius > 0.5f)
            {
                Span<Vector2> innerPts = stackalloc Vector2[sides];
                for (int i = 0; i < sides; i++)
                {
                    float a = polyRotation + MathHelper.TwoPi * (i / (float)sides);
                    innerPts[i] = Projectile.Center + new Vector2(innerHexagonRadius, 0f).RotatedBy(a);
                }

                // Draw inner hexagon - bright lines
                for (int i = 0; i < sides; i++)
                {
                    Vector2 a = innerPts[i];
                    Vector2 b = innerPts[(i + 1) % sides];
                    DrawLightLine(a, b, hexColor, lineWidth * 0.8f, trailTexture);
                }

                // Draw inner hexagon - inner subtle lines
                for (int i = 0; i < sides; i++)
                {
                    Vector2 a = innerPts[i];
                    Vector2 b = innerPts[(i + 1) % sides];
                    DrawLightLine(a, b, innerOuter, lineWidth * 0.4f, trailTexture);
                }
            }

            // Hexagon adds small amount of light based on its color
            Lighting.AddLight(Projectile.Center, hexColor.ToVector3() * hexAlpha * 0.2f);
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

        private void DrawLightning()
        {
            float effectProgress = 1f - (Projectile.timeLeft / 60f);
            if (effectProgress > 0.9f) return; // Don't draw lightning in final 10%
            
            Texture2D trailTexture = TextureAssets.MagicPixel.Value;
            float lightningAlpha = effectProgress < 0.5f ? 1f : MathHelper.Lerp(1f, 0f, (effectProgress - 0.5f) * 2f);
            
            // Create only 1-2 lightning bolts instead of 4
            int boltCount = Main.rand.Next(1, 3); // 1 or 2 bolts
            for (int b = 0; b < boltCount; b++)
            {
                float boltAngle = Main.rand.NextFloat(0f, MathHelper.TwoPi);
                Vector2 boltDir = new Vector2((float)Math.Cos(boltAngle), (float)Math.Sin(boltAngle));
                
                // Create jagged lightning path with curves
                Vector2 currentPos = Projectile.Center;
                float boltDistance = 120f;
                int segments = 8;
                
                for (int seg = 0; seg < segments; seg++)
                {
                    // Curved path: bend the direction slightly each segment
                    float curveAngle = (float)Math.Sin(seg * 0.5f) * 0.15f;
                    Vector2 curvedDir = boltDir.RotatedBy(curveAngle);
                    
                    // Random jitter for jagged appearance
                    float jitter = Main.rand.NextFloat(-12f, 12f);
                    Vector2 perpendicular = new Vector2(-curvedDir.Y, curvedDir.X);
                    
                    Vector2 nextPos = currentPos + curvedDir * (boltDistance / segments) + perpendicular * jitter;
                    
                    // Draw line segment - tapers to point at tip
                    Vector2 diff = nextPos - currentPos;
                    float len = diff.Length();
                    
                    float rot = diff.ToRotation();
                    Vector2 mid = (currentPos + nextPos) * 0.5f - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
                    
                    // Taper: thick at base, thin at tip
                    float segmentAlpha = lightningAlpha * MathHelper.Lerp(1f, 0.3f, seg / (float)segments);
                    float boltWidth = MathHelper.Lerp(5f, 0.5f, seg / (float)segments);
                    Color lightColor = Color.DarkViolet;
                    lightColor *= segmentAlpha;
                    
                    Main.spriteBatch.Draw(
                        trailTexture,
                        mid,
                        new Rectangle(0, 0, 1, 1),
                        lightColor,
                        rot,
                        new Vector2(0.5f, 0.5f),
                        new Vector2(len, boltWidth),
                        SpriteEffects.None,
                        0f);
                    
                    // Draw secondary shimmer line (thinner, darker purple)
                    Color shimmerColor = Color.MediumPurple * segmentAlpha * 0.4f;
                    float shimmerWidth = boltWidth * 0.5f;
                    
                    Main.spriteBatch.Draw(
                        trailTexture,
                        mid,
                        new Rectangle(0, 0, 1, 1),
                        shimmerColor,
                        rot,
                        new Vector2(0.5f, 0.5f),
                        new Vector2(len, shimmerWidth),
                        SpriteEffects.None,
                        0f);
                    
                    // Add lighting at segment
                    Lighting.AddLight(currentPos, lightColor.ToVector3() * segmentAlpha * 0.7f);
                    
                    currentPos = nextPos;
                }
            }
        }
    }
}
