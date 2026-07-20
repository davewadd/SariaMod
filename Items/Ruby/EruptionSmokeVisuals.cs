using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace SariaMod.Items.Ruby
{
    internal enum EruptionSmokeKind
    {
        Yellow,
        YellowOrange,
        Red,
        Smoke
    }

    internal sealed class EruptionSmokeVisuals
    {
        public const int LifetimeTicks = 128;
        public const float GoldenAngle = 2.39996323f;
        private const int GlobalMaximumPuffs = 512;
        private const float ProjectileIdentityPhaseStep = 0.7548777f;
        private static readonly Rectangle SmokeSourceRectangle = new Rectangle(0, 1, 84, 86);
        private static readonly Queue<SmokePuff> globalPuffOrder = new Queue<SmokePuff>();
        private static int globalActivePuffCount;
        private static Texture2D yellowTexture;
        private static Texture2D yellowOrangeTexture;
        private static Texture2D redTexture;
        private static Texture2D smokeTexture;

        private sealed class SmokePuff
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public int Alpha;
            public EruptionSmokeKind Kind;
            public bool Active;
        }

        private readonly List<SmokePuff> smokePuffs;
        private readonly int maximumPuffs;

        public EruptionSmokeVisuals(int maximumPuffs)
        {
            this.maximumPuffs = System.Math.Max(1, maximumPuffs);
            smokePuffs = new List<SmokePuff>(Main.dedServ ? 0 : this.maximumPuffs);
        }

        public void Spawn(Vector2 position, Vector2 velocity, EruptionSmokeKind kind)
        {
            if (Main.dedServ)
            {
                return;
            }

            RemoveInactiveLocalPuffs();
            while (smokePuffs.Count >= maximumPuffs)
            {
                Deactivate(smokePuffs[0]);
                smokePuffs.RemoveAt(0);
            }

            MakeRoomInGlobalBudget();
            SmokePuff smokePuff = new SmokePuff
            {
                Position = position,
                Velocity = velocity,
                Scale = 0.5f,
                Alpha = 0,
                Kind = kind,
                Active = true
            };
            smokePuffs.Add(smokePuff);
            globalPuffOrder.Enqueue(smokePuff);
            globalActivePuffCount++;
        }

        public static Vector2 CreateDustLikeVelocity()
        {
            if (Main.dedServ)
            {
                return Vector2.Zero;
            }

            return new Vector2(
                Main.rand.Next(-20, 21) * 0.1f,
                Main.rand.Next(-20, 21) * 0.1f);
        }

        public static float CreatePatternPhase(int tick, int projectileIdentity)
        {
            return tick * GoldenAngle + projectileIdentity * ProjectileIdentityPhaseStep;
        }

        public static Vector2 CreateSunflowerOffset(
            int sampleIndex,
            int sampleCount,
            float radius,
            float rotationOffset)
        {
            if (sampleCount <= 0)
            {
                return Vector2.Zero;
            }

            int radialIndex = sampleIndex * 17 % sampleCount;
            float radiusProgress = (radialIndex + 0.5f) / sampleCount;
            float angle = rotationOffset + sampleIndex * GoldenAngle;
            return Vector2.UnitX.RotatedBy(angle) * ((float)System.Math.Sqrt(radiusProgress) * radius);
        }

        public void SpawnEvenRing(
            Vector2 position,
            float speed,
            EruptionSmokeKind kind,
            int count,
            float rotationOffset,
            float maxAngleJitter = 0f,
            float speedVariation = 0f,
            float maxSpawnJitter = 0f)
        {
            if (Main.dedServ || count <= 0)
            {
                return;
            }

            float angleStep = MathHelper.TwoPi / count;
            for (int i = 0; i < count; i++)
            {
                float angleJitter = maxAngleJitter > 0f
                    ? Main.rand.NextFloat(-maxAngleJitter, maxAngleJitter)
                    : 0f;
                float speedMultiplier = speedVariation > 0f
                    ? Main.rand.NextFloat(1f - speedVariation, 1f + speedVariation)
                    : 1f;
                Vector2 spawnOffset = maxSpawnJitter > 0f
                    ? Main.rand.NextVector2Circular(maxSpawnJitter, maxSpawnJitter)
                    : Vector2.Zero;
                Vector2 velocity = Vector2.UnitX.RotatedBy(
                    rotationOffset + angleStep * i + angleJitter) * (speed * speedMultiplier);
                Spawn(position + spawnOffset, velocity, kind);
            }
        }

        public void Update()
        {
            for (int i = smokePuffs.Count - 1; i >= 0; i--)
            {
                SmokePuff smokePuff = smokePuffs[i];
                if (!smokePuff.Active)
                {
                    smokePuffs.RemoveAt(i);
                    continue;
                }

                smokePuff.Position += smokePuff.Velocity;
                smokePuff.Velocity *= 0.98f;
                smokePuff.Scale *= 1.01f;
                smokePuff.Alpha += 2;

                if (smokePuff.Alpha >= 256)
                {
                    Deactivate(smokePuff);
                    smokePuffs.RemoveAt(i);
                    continue;
                }

                if (IsNearScreen(smokePuff))
                {
                    AddLight(smokePuff);
                }
            }

            RemoveInactiveGlobalQueueHead();
        }

        public void Draw()
        {
            if (Main.dedServ || smokePuffs.Count == 0)
            {
                return;
            }

            EnsureTexturesLoaded();

            foreach (SmokePuff smokePuff in smokePuffs)
            {
                if (!smokePuff.Active)
                {
                    continue;
                }

                Texture2D texture = smokePuff.Kind switch
                {
                    EruptionSmokeKind.Yellow => yellowTexture,
                    EruptionSmokeKind.YellowOrange => yellowOrangeTexture,
                    EruptionSmokeKind.Red => redTexture,
                    _ => smokeTexture
                };

                Vector2 drawPosition = smokePuff.Position - Main.screenPosition;
                float drawRadius = System.Math.Max(SmokeSourceRectangle.Width, SmokeSourceRectangle.Height)
                    * smokePuff.Scale
                    * 0.5f;
                if (drawPosition.X < -drawRadius
                    || drawPosition.X > Main.screenWidth + drawRadius
                    || drawPosition.Y < -drawRadius
                    || drawPosition.Y > Main.screenHeight + drawRadius)
                {
                    continue;
                }

                float opacity = MathHelper.Clamp(1f - smokePuff.Alpha / 255f, 0f, 1f);
                Main.spriteBatch.Draw(
                    texture,
                    drawPosition,
                    SmokeSourceRectangle,
                    Color.White * opacity,
                    0f,
                    SmokeSourceRectangle.Size() * 0.5f,
                    smokePuff.Scale,
                    SpriteEffects.None,
                    0f);
            }
        }

        public void Clear()
        {
            for (int i = 0; i < smokePuffs.Count; i++)
            {
                Deactivate(smokePuffs[i]);
            }

            smokePuffs.Clear();
            RemoveInactiveGlobalQueueHead();
        }

        private static void AddLight(SmokePuff smokePuff)
        {
            Vector3 light = smokePuff.Kind switch
            {
                EruptionSmokeKind.Yellow => Color.Yellow.ToVector3() * 3f,
                EruptionSmokeKind.YellowOrange => Color.OrangeRed.ToVector3(),
                EruptionSmokeKind.Red => Color.OrangeRed.ToVector3(),
                _ => Color.OrangeRed.ToVector3() * 0.2f
            };

            Lighting.AddLight(smokePuff.Position, light);
        }

        internal static void ResetGlobalState()
        {
            globalPuffOrder.Clear();
            globalActivePuffCount = 0;
            yellowTexture = null;
            yellowOrangeTexture = null;
            redTexture = null;
            smokeTexture = null;
        }

        private void RemoveInactiveLocalPuffs()
        {
            for (int i = smokePuffs.Count - 1; i >= 0; i--)
            {
                if (!smokePuffs[i].Active)
                {
                    smokePuffs.RemoveAt(i);
                }
            }
        }

        private static void MakeRoomInGlobalBudget()
        {
            RemoveInactiveGlobalQueueHead();
            while (globalActivePuffCount >= GlobalMaximumPuffs && globalPuffOrder.Count > 0)
            {
                SmokePuff oldest = globalPuffOrder.Dequeue();
                Deactivate(oldest);
                RemoveInactiveGlobalQueueHead();
            }
        }

        private static void Deactivate(SmokePuff smokePuff)
        {
            if (!smokePuff.Active)
            {
                return;
            }

            smokePuff.Active = false;
            globalActivePuffCount = System.Math.Max(0, globalActivePuffCount - 1);
        }

        private static void RemoveInactiveGlobalQueueHead()
        {
            while (globalPuffOrder.Count > 0 && !globalPuffOrder.Peek().Active)
            {
                globalPuffOrder.Dequeue();
            }
        }

        private static bool IsNearScreen(SmokePuff smokePuff)
        {
            Vector2 drawPosition = smokePuff.Position - Main.screenPosition;
            float drawRadius = System.Math.Max(SmokeSourceRectangle.Width, SmokeSourceRectangle.Height)
                * smokePuff.Scale
                * 0.5f;
            return drawPosition.X >= -drawRadius
                && drawPosition.X <= Main.screenWidth + drawRadius
                && drawPosition.Y >= -drawRadius
                && drawPosition.Y <= Main.screenHeight + drawRadius;
        }

        private static void EnsureTexturesLoaded()
        {
            yellowTexture ??= ModContent.Request<Texture2D>("SariaMod/Dusts/SmokeDust5Yellow").Value;
            yellowOrangeTexture ??= ModContent.Request<Texture2D>("SariaMod/Dusts/SmokeDust5Yellorange").Value;
            redTexture ??= ModContent.Request<Texture2D>("SariaMod/Dusts/SmokeDust5Red").Value;
            smokeTexture ??= ModContent.Request<Texture2D>("SariaMod/Dusts/SmokeDust5").Value;
        }
    }
}
