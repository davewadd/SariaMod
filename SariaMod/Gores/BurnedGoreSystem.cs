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
        private class BurnedGoreData
        {
            public Gore GoreReference;
            public int StartTick;
            public int Duration;

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

        private const int MaxSoundsPerFrame = 3;
        private static int soundsPlayedThisFrame = 0;

        public override void Load()
        {
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
        }

        public static void TrackBurnedGore(Gore gore)
        {
            if (trackedGoreSet.Contains(gore))
                return;

            int currentTick = (int)Main.GameUpdateCount;
            trackedGores.Add(new BurnedGoreData(gore, currentTick, TileHeatManager.DefaultHeatDuration));
            trackedGoreSet.Add(gore);
        }

        /// <summary>
        /// Scans Main.gore[] for active gores near a world position and tracks them for burning.
        /// </summary>
        public static void TrackGoresNearPosition(Vector2 center, float radius)
        {
            float radiusSq = radius * radius;
            int currentTick = (int)Main.GameUpdateCount;
            for (int i = 0; i < Main.maxGore; i++)
            {
                Gore gore = Main.gore[i];
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

            int particleCount = (int)(radius * 0.15f);
            particleCount = System.Math.Clamp(particleCount, 2, 8);

            Vector2 goreCenter = gore.position + new Vector2(frameWidth / 2f, frameHeight / 2f);

            for (int i = 0; i < particleCount; i++)
            {
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

            soundsPlayedThisFrame = 0;

            if (_goreCreationTick != null)
            {
                var stale = new List<Gore>();
                foreach (var kvp in _goreCreationTick)
                    if (!kvp.Key.active) stale.Add(kvp.Key);
                foreach (var g in stale)
                    _goreCreationTick.Remove(g);
            }
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
                    _goreCreationTick[g] = (int)Main.GameUpdateCount;
            }
            return idx;
        }

        private Gore StampGoreCreationTick_Perfect(
            On.Terraria.Gore.orig_NewGorePerfect_IEntitySource_Vector2_Vector2_int_float orig,
            IEntitySource source, Vector2 position, Vector2 velocity, int type, float scale)
        {
            Gore g = orig(source, position, velocity, type, scale);
            if (g != null)
                _goreCreationTick[g] = (int)Main.GameUpdateCount;
            return g;
        }

        private void Hook_Main_DrawGore(On.Terraria.Main.orig_DrawGore orig, Main self)
        {
            List<int> modifiedGoreAlphas = new List<int>();
            List<int> modifiedGoreIndices = new List<int>();

            for (int i = 0; i < Main.maxGore; i++)
            {
                Gore gore = Main.gore[i];
                if (gore != null && gore.active && trackedGoreSet.Contains(gore))
                {
                    modifiedGoreIndices.Add(i);
                    modifiedGoreAlphas.Add(gore.alpha);
                    gore.alpha = 255;
                }
            }

            orig(self);

            for (int i = 0; i < modifiedGoreIndices.Count; i++)
            {
                Main.gore[modifiedGoreIndices[i]].alpha = modifiedGoreAlphas[i];
            }

            DrawBurnedGores();
        }

        private void DrawBurnedGores()
        {
            if (Main.dedServ || trackedGores == null || trackedGores.Count == 0)
                return;

            SpriteBatch spriteBatch = Main.spriteBatch;
            int currentTick = (int)Main.GameUpdateCount;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

            for (int i = trackedGores.Count - 1; i >= 0; i--)
            {
                BurnedGoreData data = trackedGores[i];
                Gore gore = data.GoreReference;

                float progress = (currentTick - data.StartTick) / (float)data.Duration;
                bool shouldExpire = !gore.active || progress >= 1f;

                if (shouldExpire)
                {
                    SpawnSmokeRingForGore(gore);

                    if (soundsPlayedThisFrame < MaxSoundsPerFrame)
                    {
                        SoundEngine.PlaySound(SoundID.Item20, gore.position);
                        soundsPlayedThisFrame++;
                    }

                    trackedGoreSet.Remove(gore);
                    trackedGores.RemoveAt(i);
                    continue;
                }

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
                    Color burnedColor = new Color(60, 15, 5, 255);
                    Color lightColor = Lighting.GetColor((int)(gore.position.X / 16f), (int)(gore.position.Y / 16f));

                    Color finalColor = new Color(
                        (burnedColor.R * lightColor.R) / 255,
                        (burnedColor.G * lightColor.G) / 255,
                        (burnedColor.B * lightColor.B) / 255,
                        255
                    );

                    spriteBatch.Draw(texture, drawPos, sourceRect, finalColor, gore.rotation, origin, gore.scale, SpriteEffects.None, 0f);

                    if (Main.rand.NextBool(120))
                    {
                        Vector2 dustPos = gore.position + new Vector2(Main.rand.NextFloat(frameWidth), Main.rand.NextFloat(frameHeight));
                        Vector2 dustVel = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1.5f, 0.5f));
                        Dust smoke = Dust.NewDustPerfect(dustPos, ModContent.DustType<SmokeDust>(), dustVel, 0, default, 1.0f);
                        smoke.noGravity = true;
                    }

                    if (Main.rand.NextBool(1200))
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

            for (int i = trackedGores.Count - 1; i >= 0; i--)
            {
                BurnedGoreData data = trackedGores[i];
                Gore gore = data.GoreReference;

                if (!gore.active) continue;

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
                }
            }

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
        }
    }
}
