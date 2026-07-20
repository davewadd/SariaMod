using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using SariaMod.Dusts;
using SariaMod.Items.Ruby;
using SariaMod.TileGlow;
using System;

namespace SariaMod.Gores
{
    public class BurnedGoreSystem : ModSystem
    {
        // Shared by burned gores and the living Charred visual so both effects stay
        // in the same palette family.
        public static readonly Color BurnedPaletteColor = new Color(60, 15, 5, 255);

        private readonly struct BurnedGoreData
        {
            public readonly Gore GoreReference;
            public readonly int StartTick;
            public readonly int Duration;

            public BurnedGoreData(Gore gore, int startTick, int duration)
            {
                GoreReference = gore;
                StartTick = startTick;
                Duration = duration;
            }
        }

        private static List<BurnedGoreData> trackedGores = new List<BurnedGoreData>();
        private static HashSet<Gore> trackedGoreSet = new HashSet<Gore>();
        private static Dictionary<Gore, int> _goreCreationTick = new Dictionary<Gore, int>();
        private static List<Gore> _goresCreatedThisTick = new List<Gore>();
        private static List<Gore> _modifiedGoreReferences = new List<Gore>();
        private static List<int> _modifiedGoreAlphas = new List<int>();

        // Charred death visuals are cosmetic. Keep their expensive custom redraws
        // bounded when a beam kills a large crowd at once. Overflow pieces fall
        // back to Terraria's normal gore update and disappear quickly.
        internal const int MaxTrackedGores = 128;
        private const int OverflowDissipationTicks = 30;
        private const int MaxAdditiveGlowsPerFrame = 96;
        private const int MaxBurnedGoreDustsPerFrame = 24;
        private const float ScreenCullPadding = 96f;

        private const int MaxSoundsPerFrame = 3;
        private static int soundsPlayedThisFrame = 0;
        private static int burnedGoreDustsSpawnedThisFrame = 0;
        private static ulong lastVisualBudgetResetTick = ulong.MaxValue;
        private static ulong lastTrackingCleanupTick = ulong.MaxValue;
        private static int _creationRegistryTick = -1;

        public override void Load()
        {
            trackedGores ??= new List<BurnedGoreData>();
            trackedGoreSet ??= new HashSet<Gore>();
            _goreCreationTick ??= new Dictionary<Gore, int>();
            _goresCreatedThisTick ??= new List<Gore>();
            _modifiedGoreReferences ??= new List<Gore>();
            _modifiedGoreAlphas ??= new List<int>();

            On.Terraria.Main.DrawGore += Hook_Main_DrawGore;
            On.Terraria.Gore.NewGore_IEntitySource_Vector2_Vector2_int_float += StampGoreCreationTick;
            On.Terraria.Gore.NewGorePerfect_IEntitySource_Vector2_Vector2_int_float += StampGoreCreationTick_Perfect;
        }

        public override void Unload()
        {
            On.Terraria.Main.DrawGore -= Hook_Main_DrawGore;
            On.Terraria.Gore.NewGore_IEntitySource_Vector2_Vector2_int_float -= StampGoreCreationTick;
            On.Terraria.Gore.NewGorePerfect_IEntitySource_Vector2_Vector2_int_float -= StampGoreCreationTick_Perfect;
            trackedGores?.Clear();
            trackedGores = null;
            trackedGoreSet?.Clear();
            trackedGoreSet = null;
            _goreCreationTick?.Clear();
            _goreCreationTick = null;
            _goresCreatedThisTick?.Clear();
            _goresCreatedThisTick = null;
            _modifiedGoreReferences?.Clear();
            _modifiedGoreReferences = null;
            _modifiedGoreAlphas?.Clear();
            _modifiedGoreAlphas = null;
        }

        public override void OnWorldLoad()
        {
            ClearAllTrackedGores();
        }

        public override void OnWorldUnload()
        {
            ClearAllTrackedGores();
        }

        public override void PreSaveAndQuit()
        {
            ClearAllTrackedGores();
        }

        public static void ClearAllTrackedGores()
        {
            trackedGores?.Clear();
            trackedGoreSet?.Clear();
            _goreCreationTick?.Clear();
            _goresCreatedThisTick?.Clear();
            _modifiedGoreReferences?.Clear();
            _modifiedGoreAlphas?.Clear();
            _creationRegistryTick = -1;
            lastVisualBudgetResetTick = ulong.MaxValue;
            lastTrackingCleanupTick = ulong.MaxValue;
            soundsPlayedThisFrame = 0;
            burnedGoreDustsSpawnedThisFrame = 0;
        }

        public static void TrackBurnedGore(Gore gore)
        {
            if (gore == null || !gore.active || trackedGoreSet.Contains(gore))
                return;

            int currentTick = (int)Main.GameUpdateCount;
            EnsureTrackedCleanupForCurrentTick(currentTick);

            if (trackedGores.Count >= MaxTrackedGores)
            {
                // Keep the newest death readable. The oldest custom piece returns
                // to a short, translucent vanilla dissipation without producing a
                // completion ring or sound. Removing its birth stamp prevents a
                // later same-tick radius query from cycling it back into the cap.
                BurnedGoreData oldest = trackedGores[0];
                Gore oldestGore = oldest.GoreReference;
                trackedGores.RemoveAt(0);
                if (oldestGore != null)
                {
                    trackedGoreSet.Remove(oldestGore);
                    _goreCreationTick.Remove(oldestGore);
                    BeginOverflowDissipation(oldestGore);
                }
            }

            trackedGores.Add(new BurnedGoreData(gore, currentTick, TileHeatManager.DefaultHeatDuration));
            trackedGoreSet.Add(gore);
        }

        private static void BeginOverflowDissipation(Gore gore)
        {
            if (gore == null || !gore.active)
                return;

            if (gore.timeLeft <= 0 || gore.timeLeft > OverflowDissipationTicks)
                gore.timeLeft = OverflowDissipationTicks;
            gore.alpha = Math.Max(gore.alpha, 160);
        }

        /// <summary>
        /// Scans only gores born during the current update near a world position and
        /// tracks them for burning.
        /// </summary>
        public static void TrackGoresNearPosition(Vector2 center, float radius)
        {
            float radiusSq = radius * radius;
            int currentTick = (int)Main.GameUpdateCount;
            if (_creationRegistryTick != currentTick)
                return;

            for (int i = 0; i < _goresCreatedThisTick.Count; i++)
            {
                Gore gore = _goresCreatedThisTick[i];
                if (gore == null || !gore.active) continue;
                if (trackedGoreSet.Contains(gore)) continue;
                if (!_goreCreationTick.TryGetValue(gore, out int createdTick) || createdTick != currentTick) continue;
                Vector2 goreCenter = gore.position + new Vector2(gore.Width * 0.5f, gore.Height * 0.5f);
                if (Vector2.DistanceSquared(goreCenter, center) > radiusSq) continue;
                TrackBurnedGore(gore);
            }
        }

        private static void SpawnSmokeRingForGore(Gore gore)
        {
            if (Main.dedServ)
                return;

            Texture2D texture = null;
            if (gore.type >= 0 && gore.type < TextureAssets.Gore.Length)
            {
                texture = TextureAssets.Gore[gore.type].Value;
            }

            if (texture == null)
                return;

            Rectangle frame = gore.Frame.GetSourceRectangle(texture);
            int frameWidth = frame.Width;
            int frameHeight = frame.Height;

            float radius = Math.Max(frameWidth, frameHeight) * 0.5f * gore.scale;

            int particleCount = (int)(radius * 0.08f);
            particleCount = System.Math.Clamp(particleCount, 1, 4);

            Vector2 goreCenter = gore.position + new Vector2(frameWidth / 2f, frameHeight / 2f);

            for (int i = 0; i < particleCount; i++)
            {
                if (!TryReserveBurnedGoreDustSlot())
                    break;

                double angle = (i / (double)particleCount) * 2.0 * Math.PI;
                double randomAngle = angle + Main.rand.NextFloat(-0.2f, 0.2f);
                float randomRadius = radius * Main.rand.NextFloat(0.85f, 1.15f);

                Vector2 dustPos = goreCenter + new Vector2(
                    (float)(Math.Cos(randomAngle) * randomRadius),
                    (float)(Math.Sin(randomAngle) * randomRadius)
                );

                Vector2 velocity = new Vector2(
                    (float)(Math.Cos(randomAngle) * Main.rand.NextFloat(1f, 2.5f)),
                    (float)(Math.Sin(randomAngle) * Main.rand.NextFloat(1f, 2.5f))
                );

                Dust.NewDustPerfect(dustPos, ModContent.DustType<SmokeDust>(), velocity, 0, default, Main.rand.NextFloat(1f, 1.5f));
            }
        }

        public override void PostUpdateEverything()
        {
            if (Main.dedServ)
                return;

            ResetVisualBudgetsIfNeeded();
            RemoveFinishedTrackedGores((int)Main.GameUpdateCount, true);

            // Birth stamps older than this update can never match a future death.
            // Clearing both reusable containers avoids an ever-growing registry and
            // replaces the old per-frame temporary-list allocation.
            _goreCreationTick.Clear();
            _goresCreatedThisTick.Clear();
            _creationRegistryTick = -1;
        }

        private static void ResetVisualBudgetsIfNeeded()
        {
            ulong currentTick = Main.GameUpdateCount;
            if (lastVisualBudgetResetTick == currentTick)
                return;

            lastVisualBudgetResetTick = currentTick;
            soundsPlayedThisFrame = 0;
            burnedGoreDustsSpawnedThisFrame = 0;
        }

        private static void EnsureTrackedCleanupForCurrentTick(int currentTick)
        {
            ulong updateTick = Main.GameUpdateCount;
            if (lastTrackingCleanupTick == updateTick)
                return;

            lastTrackingCleanupTick = updateTick;
            ResetVisualBudgetsIfNeeded();
            RemoveFinishedTrackedGores(currentTick, true);
        }

        private static bool TryReserveBurnedGoreDustSlot()
        {
            ResetVisualBudgetsIfNeeded();
            if (burnedGoreDustsSpawnedThisFrame >= MaxBurnedGoreDustsPerFrame
                || !VisualDustLimiter.TryReserveHalfCapacitySlot())
            {
                return false;
            }

            burnedGoreDustsSpawnedThisFrame++;
            return true;
        }

        private static void RemoveFinishedTrackedGores(int currentTick, bool emitCompletionEffects)
        {
            for (int i = trackedGores.Count - 1; i >= 0; i--)
            {
                BurnedGoreData data = trackedGores[i];
                Gore gore = data.GoreReference;
                bool expired = gore == null
                    || !gore.active
                    || currentTick - data.StartTick >= data.Duration;
                if (!expired)
                    continue;

                if (emitCompletionEffects && gore != null)
                {
                    SpawnSmokeRingForGore(gore);
                    if (soundsPlayedThisFrame < MaxSoundsPerFrame)
                    {
                        SoundEngine.PlaySound(SoundID.Item20, gore.position);
                        soundsPlayedThisFrame++;
                    }
                }

                if (gore != null)
                    trackedGoreSet.Remove(gore);
                trackedGores.RemoveAt(i);
            }
        }

        private static void RecordGoreCreation(Gore gore)
        {
            if (gore == null || !gore.active)
                return;

            // Main.gore[] reuses mutable Gore objects. If a tracked slot is reused
            // before the regular update cleanup, the new generation must not inherit
            // the previous piece's charred start time or membership.
            if (trackedGoreSet.Remove(gore))
            {
                _goreCreationTick.Remove(gore);
                for (int i = trackedGores.Count - 1; i >= 0; i--)
                {
                    if (ReferenceEquals(trackedGores[i].GoreReference, gore))
                        trackedGores.RemoveAt(i);
                }
            }

            int currentTick = (int)Main.GameUpdateCount;
            if (_creationRegistryTick != currentTick)
            {
                _goreCreationTick.Clear();
                _goresCreatedThisTick.Clear();
                _creationRegistryTick = currentTick;
            }

            if (_goreCreationTick.TryGetValue(gore, out int existingTick)
                && existingTick == currentTick)
            {
                return;
            }

            _goreCreationTick[gore] = currentTick;
            _goresCreatedThisTick.Add(gore);
        }

        private int StampGoreCreationTick(
            On.Terraria.Gore.orig_NewGore_IEntitySource_Vector2_Vector2_int_float orig,
            IEntitySource source, Vector2 position, Vector2 velocity, int type, float scale)
        {
            int idx = orig(source, position, velocity, type, scale);
            if (idx >= 0 && idx < Main.maxGore)
            {
                Gore g = Main.gore[idx];
                if (g != null && g.active)
                    RecordGoreCreation(g);
            }
            return idx;
        }

        private Gore StampGoreCreationTick_Perfect(
            On.Terraria.Gore.orig_NewGorePerfect_IEntitySource_Vector2_Vector2_int_float orig,
            IEntitySource source, Vector2 position, Vector2 velocity, int type, float scale)
        {
            Gore g = orig(source, position, velocity, type, scale);
            if (g != null)
                RecordGoreCreation(g);
            return g;
        }

        private void Hook_Main_DrawGore(On.Terraria.Main.orig_DrawGore orig, Main self)
        {
            if (trackedGores == null || trackedGores.Count == 0)
            {
                orig(self);
                return;
            }

            _modifiedGoreReferences.Clear();
            _modifiedGoreAlphas.Clear();
            for (int i = 0; i < trackedGores.Count; i++)
            {
                Gore gore = trackedGores[i].GoreReference;
                if (gore != null && gore.active)
                {
                    _modifiedGoreReferences.Add(gore);
                    _modifiedGoreAlphas.Add(gore.alpha);
                    gore.alpha = 255;
                }
            }

            try
            {
                orig(self);
            }
            finally
            {
                for (int i = 0; i < _modifiedGoreReferences.Count; i++)
                    _modifiedGoreReferences[i].alpha = _modifiedGoreAlphas[i];
            }

            DrawBurnedGores();
        }

        private static bool IsRoughlyOnScreen(Gore gore)
        {
            if (gore == null || !gore.active)
                return false;

            float scale = Math.Max(0.1f, gore.scale);
            float width = Math.Max(16f, gore.Width * scale);
            float height = Math.Max(16f, gore.Height * scale);
            float left = Main.screenPosition.X - ScreenCullPadding;
            float top = Main.screenPosition.Y - ScreenCullPadding;
            float right = Main.screenPosition.X + Main.screenWidth + ScreenCullPadding;
            float bottom = Main.screenPosition.Y + Main.screenHeight + ScreenCullPadding;
            return gore.position.X + width >= left
                && gore.position.X <= right
                && gore.position.Y + height >= top
                && gore.position.Y <= bottom;
        }

        private void DrawBurnedGores()
        {
            if (Main.dedServ || trackedGores == null || trackedGores.Count == 0)
                return;

            SpriteBatch spriteBatch = Main.spriteBatch;
            int currentTick = (int)Main.GameUpdateCount;

            bool hasVisibleGore = false;
            for (int i = 0; i < trackedGores.Count; i++)
            {
                if (IsRoughlyOnScreen(trackedGores[i].GoreReference))
                {
                    hasVisibleGore = true;
                    break;
                }
            }

            if (!hasVisibleGore)
                return;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

            for (int i = trackedGores.Count - 1; i >= 0; i--)
            {
                BurnedGoreData data = trackedGores[i];
                Gore gore = data.GoreReference;

                float progress = (currentTick - data.StartTick) / (float)data.Duration;
                if (!IsRoughlyOnScreen(gore) || progress >= 1f)
                    continue;

                Texture2D texture = null;
                if (gore.type >= 0 && gore.type < TextureAssets.Gore.Length)
                {
                    texture = TextureAssets.Gore[gore.type].Value;
                }

                if (texture != null)
                {
                    Rectangle sourceRect = gore.Frame.GetSourceRectangle(texture);
                    int frameWidth = sourceRect.Width;
                    int frameHeight = sourceRect.Height;

                    Vector2 origin = new Vector2(frameWidth / 2f, frameHeight / 2f);
                    Vector2 drawPos = gore.position + origin - Main.screenPosition;

                    float fadeMultiplier = 1f - progress;
                    float baseIntensity = fadeMultiplier * 0.8f;

                    float lightIntensity = baseIntensity;

                    float lightR = 1.0f * lightIntensity;
                    float lightG = 0.4f * lightIntensity;
                    float lightB = 0.05f * lightIntensity;

                    Vector2 goreCenter = gore.position + new Vector2(frameWidth / 2f, frameHeight / 2f);
                    Lighting.AddLight(goreCenter, lightR, lightG, lightB);

                    // Blackened/dark red tint for fire-burned gore
                    Color lightColor = Lighting.GetColor((int)(gore.position.X / 16f), (int)(gore.position.Y / 16f));

                    Color finalColor = new Color(
                        (BurnedPaletteColor.R * lightColor.R) / 255,
                        (BurnedPaletteColor.G * lightColor.G) / 255,
                        (BurnedPaletteColor.B * lightColor.B) / 255,
                        255
                    );

                    spriteBatch.Draw(texture, drawPos, sourceRect, finalColor, gore.rotation, origin, gore.scale, SpriteEffects.None, 0f);

                    if (Main.rand.NextBool(240) && TryReserveBurnedGoreDustSlot())
                    {
                        Vector2 dustPos = gore.position + new Vector2(Main.rand.NextFloat(frameWidth), Main.rand.NextFloat(frameHeight));
                        Vector2 dustVel = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1.5f, 0.5f));
                        Dust smoke = Dust.NewDustPerfect(dustPos, ModContent.DustType<SmokeDust>(), dustVel, 0, default, 1.0f);
                        smoke.noGravity = true;
                    }

                    if (Main.rand.NextBool(2400) && TryReserveBurnedGoreDustSlot())
                    {
                        Vector2 dustPos = gore.position + new Vector2(Main.rand.NextFloat(frameWidth), Main.rand.NextFloat(frameHeight));
                        Dust d = Dust.NewDustPerfect(dustPos, ModContent.DustType<SmokeDust7>(), Vector2.Zero, 0, default, 1f);
                        d.noGravity = true;
                    }
                }
            }

            spriteBatch.End();

            // Additive glow pass for fire embers
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

            int additiveGlowsDrawn = 0;
            for (int i = trackedGores.Count - 1; i >= 0; i--)
            {
                if (additiveGlowsDrawn >= MaxAdditiveGlowsPerFrame)
                    break;

                BurnedGoreData data = trackedGores[i];
                Gore gore = data.GoreReference;

                if (!IsRoughlyOnScreen(gore)) continue;

                float progress = (currentTick - data.StartTick) / (float)data.Duration;
                if (progress >= 1f) continue;

                Texture2D texture = null;
                if (gore.type >= 0 && gore.type < TextureAssets.Gore.Length)
                {
                    texture = TextureAssets.Gore[gore.type].Value;
                }

                if (texture != null)
                {
                    Rectangle sourceRect = gore.Frame.GetSourceRectangle(texture);
                    int frameWidth = sourceRect.Width;
                    int frameHeight = sourceRect.Height;

                    Vector2 origin = new Vector2(frameWidth / 2f, frameHeight / 2f);
                    Vector2 drawPos = gore.position + origin - Main.screenPosition;

                    // Fire ember glow: bright orange with fade
                    Color glowColor = Color.Lerp(new Color(255, 160, 30), new Color(255, 60, 10), progress);

                    spriteBatch.Draw(texture, drawPos, sourceRect, glowColor * 0.6f * (1f - progress), gore.rotation, origin, gore.scale, SpriteEffects.None, 0f);
                    additiveGlowsDrawn++;
                }
            }

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
        }
    }
}
