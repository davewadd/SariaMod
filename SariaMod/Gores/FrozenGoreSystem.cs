using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using SariaMod.Buffs;
using SariaMod.TileGlow;
using SariaMod.Dusts;
using System;

namespace SariaMod.Gores
{
    public class FrozenGoreSystem : ModSystem
    {
        private class FrozenGoreData
        {
            public Gore GoreReference;
            public int StartTick;
            public int Duration;

            public FrozenGoreData(Gore gore, int startTick, int duration)
            {
                GoreReference = gore;
                StartTick = startTick;
                Duration = duration;
            }
        }

        private static List<FrozenGoreData> trackedGores = new List<FrozenGoreData>();
        
        // HashSet for O(1) gore tracking lookups instead of O(n) list search
        private static HashSet<Gore> trackedGoreSet = new HashSet<Gore>();
        
        // Records Main.GameUpdateCount at the moment each gore is created.  Used by
        // TrackGoresNearPosition to reject gores from previous frames so only gores
        // that were freshly spawned by THIS NPC's death are ever tracked.
        private static Dictionary<Gore, int> _goreCreationTick = new Dictionary<Gore, int>();

        // Sound throttling - only play up to 3 ice-break sounds per frame
        private const int MaxSoundsPerFrame = 3;
        private static int soundsPlayedThisFrame = 0;

        private static readonly Vector3 FrozenLightColor = new Vector3(0.6f, 0.85f, 1f);

        internal static Color GetFrozenGoreDrawColor(Vector2 worldPosition)
        {
            Color lightColor = Lighting.GetColor(
                (int)(worldPosition.X / 16f),
                (int)(worldPosition.Y / 16f));
            return FrozenNPCVisualManager.ApplyFrozenPalette(lightColor);
        }

        internal static Color GetFrozenGoreGlowColor(float progress)
        {
            return TileGlowManager.GetGlowColor(MathHelper.Clamp(progress, 0f, 1f), 0f) * 0.8f;
        }

        internal static void AddFrozenGoreLight(Vector2 worldPosition, float intensity)
        {
            Lighting.AddLight(worldPosition, FrozenLightColor * Math.Max(0f, intensity));
        }

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
        
        /// <summary>
        /// Clear tracked gores when world loads to prevent stuck states
        /// </summary>
        public override void OnWorldLoad()
        {
            ClearAllTrackedGores();
        }

        /// <summary>
        /// Clear tracked gores when leaving world to prevent persistence
        /// </summary>
        public override void OnWorldUnload()
        {
            ClearAllTrackedGores();
        }

        /// <summary>
        /// Clear tracked gores before the world is saved and quit
        /// </summary>
        public override void PreSaveAndQuit()
        {
            ClearAllTrackedGores();
        }

        /// <summary>
        /// Clear all tracked frozen gores
        /// </summary>
        public static void ClearAllTrackedGores()
        {
            trackedGores?.Clear();
            trackedGoreSet?.Clear();
        }
        
        /// <summary>
        /// Track a gore that should be recolored as frozen
        /// </summary>
        public static void TrackFrozenGore(Gore gore)
        {
            // Check HashSet first (O(1) instead of O(n))
            if (trackedGoreSet.Contains(gore))
                return; // Already tracked
            
            int currentTick = (int)Main.GameUpdateCount;
            trackedGores.Add(new FrozenGoreData(gore, currentTick, TileGlowManager.DefaultGlowDuration));
            trackedGoreSet.Add(gore);
        }
        
        /// <summary>
        /// Check if a gore is tracked for frozen recoloring
        /// O(1) HashSet lookup instead of O(n) list iteration
        /// </summary>
        private static bool IsGoreTracked(Gore gore)
        {
            return trackedGoreSet.Contains(gore);
        }

        /// <summary>
        /// Scans Main.gore[] for active non-IceGore gores near a world position and tracks them.
        /// Called from GlobalNPC.HitEffect AFTER vanilla NPC.HitEffect has already spawned death gores.
        /// </summary>
        public static void TrackGoresNearPosition(Vector2 center, float radius)
        {
            float radiusSq = radius * radius;
            int tracked = 0;
            int currentTick = (int)Main.GameUpdateCount;
            int iceGore1 = ModContent.GoreType<IceGore1>();
            int iceGore2 = ModContent.GoreType<IceGore2>();
            int iceGore3 = ModContent.GoreType<IceGore3>();
            for (int i = 0; i < Main.maxGore; i++)
            {
                Gore gore = Main.gore[i];
                if (gore == null || !gore.active) continue;
                if (trackedGoreSet.Contains(gore)) continue;
                int type = gore.type;
                if (type == iceGore1 || type == iceGore2 || type == iceGore3) continue;
                // Only consider gores born THIS tick — rejects old gores still
                // flying around from earlier (non-frozen) enemy deaths.
                if (!_goreCreationTick.TryGetValue(gore, out int createdTick) || createdTick != currentTick) continue;
                Vector2 goreCenter = gore.position + new Vector2(gore.Width * 0.5f, gore.Height * 0.5f);
                if (Vector2.DistanceSquared(goreCenter, center) > radiusSq) continue;
                TrackFrozenGore(gore);
                tracked++;
            }
        }

        /// <summary>
        /// Spawns a ring of snow particles around a gore position based on texture size
        /// </summary>
        private static void SpawnSnowRingForGore(Gore gore)
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

            // Use the same source-rect logic as vanilla Gore.cs so any texture layout works
            Rectangle frame = gore.Frame.GetSourceRectangle(texture);
            int frameWidth = frame.Width;
            int frameHeight = frame.Height;

            float radius = Math.Max(frameWidth, frameHeight) * 0.5f * gore.scale;
            
            int particleCount = (int)(radius * 0.5f);
            particleCount = Math.Clamp(particleCount, 6, 30);

            Vector2 goreCenter = gore.position + new Vector2(frameWidth / 2f, frameHeight / 2f);

            for (int i = 0; i < particleCount; i++)
            {
                if (!VisualDustLimiter.TryReserveHalfCapacitySlot())
                {
                    break;
                }

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
                
                Dust dust = Dust.NewDustPerfect(dustPos, ModContent.DustType<Snow2>(), velocity, 0, default, Main.rand.NextFloat(1f, 1.5f));
                dust.noGravity = true;
            }
        }

        /// <summary>
        /// Reset per-frame sound counter
        /// </summary>
        public override void PostUpdateEverything()
        {
            if (Main.dedServ)
                return;

            // Reset sound counter each frame
            soundsPlayedThisFrame = 0;

            // Remove stale entries from the creation-tick dictionary so it doesn't grow unbounded.
            // Any gore that is no longer active can be evicted.
            if (_goreCreationTick != null)
            {
                var stale = new List<Gore>();
                foreach (var kvp in _goreCreationTick)
                    if (!kvp.Key.active) stale.Add(kvp.Key);
                foreach (var g in stale)
                    _goreCreationTick.Remove(g);
            }
        }

        // Lightweight hook — only records the tick a gore was born so TrackGoresNearPosition
        // can reject gores that were already on the ground from earlier kills.
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

        // Hook Main.DrawGore to intercept vanilla gore drawing
        private void Hook_Main_DrawGore(On.Terraria.Main.orig_DrawGore orig, Main self)
        {
            // Use HashSet for O(1) lookups instead of iterating the list
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
            
            DrawFrozenGores();
        }
        
        // Draw all tracked frozen gores with light blue color
        private void DrawFrozenGores()
        {
            if (Main.dedServ || trackedGores == null || trackedGores.Count == 0)
                return;
            
            SpriteBatch spriteBatch = Main.spriteBatch;
            int currentTick = (int)Main.GameUpdateCount;
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

            for (int i = trackedGores.Count - 1; i >= 0; i--)
            {
                FrozenGoreData data = trackedGores[i];
                Gore gore = data.GoreReference;

                float progress = (currentTick - data.StartTick) / (float)data.Duration;
                bool shouldExpire = !gore.active || progress >= 1f;
                
                if (shouldExpire)
                {
                    // Spawn particles immediately (no throttling on visuals)
                    SpawnSnowRingForGore(gore);
                    
                    // Only play sound if we haven't hit the limit this frame
                    // If 3+ sounds would play at once, only the first 3 play, rest are skipped
                    if (soundsPlayedThisFrame < MaxSoundsPerFrame)
                    {
                        SoundEngine.PlaySound(SoundID.Item27, gore.position);
                        soundsPlayedThisFrame++;
                    }
                    // Sounds beyond the limit are simply skipped (not queued)
                    
                    // Remove from tracking
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
                    // Use the same source-rect logic as vanilla Gore.cs — handles any
                    // texture layout (single frame, vertical strip, multi-column grid, etc.)
                    Rectangle sourceRect = gore.Frame.GetSourceRectangle(texture);
                    int frameWidth = sourceRect.Width;
                    int frameHeight = sourceRect.Height;

                    Vector2 origin = new Vector2(frameWidth / 2f, frameHeight / 2f);
                    Vector2 drawPos = gore.position + origin - Main.screenPosition;

                    float fadeMultiplier = 1f - progress;
                    float baseIntensity = fadeMultiplier * 0.8f;

                    // IceGore1 emits stronger icy light scaled to its size
                    float lightIntensity = (gore.type == ModContent.GoreType<IceGore1>())
                        ? baseIntensity * gore.scale
                        : baseIntensity;

                    Vector2 goreCenter = gore.position + new Vector2(frameWidth / 2f, frameHeight / 2f);
                    AddFrozenGoreLight(goreCenter, lightIntensity);
                    Color finalColor = GetFrozenGoreDrawColor(gore.position);
                    
                    spriteBatch.Draw(texture, drawPos, sourceRect, finalColor, gore.rotation, origin, gore.scale, SpriteEffects.None, 0f);
                    
                    if (Main.rand.NextBool(20) && VisualDustLimiter.TryReserveHalfCapacitySlot())
                    {
                        Vector2 dustPos = gore.position + new Vector2(Main.rand.NextFloat(frameWidth), Main.rand.NextFloat(frameHeight));
                        Vector2 dustVel = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1.5f, 0.5f));
                        Dust fog = Dust.NewDustPerfect(dustPos, ModContent.DustType<Fog>(), dustVel, 0, default, 1.2f);
                        fog.noGravity = true;
                    }
                    
                    if (Main.rand.NextBool(350) && VisualDustLimiter.TryReserveHalfCapacitySlot())
                    {
                        Vector2 dustPos = gore.position + new Vector2(Main.rand.NextFloat(frameWidth), Main.rand.NextFloat(frameHeight));
                        Dust d = Dust.NewDustPerfect(dustPos, ModContent.DustType<SnowRingFog>(), Vector2.Zero, 0, default, 1f);
                        d.noGravity = true;
                    }
                    
                    if (Main.rand.NextBool(60) && VisualDustLimiter.TryReserveHalfCapacitySlot())
                    {
                        Vector2 dustPos = gore.position + new Vector2(Main.rand.NextFloat(frameWidth), Main.rand.NextFloat(frameHeight));
                        Dust d = Dust.NewDustPerfect(dustPos, ModContent.DustType<Snow2>(), Vector2.Zero, 0, default, Main.rand.NextFloat(0.5f, 1f));
                        d.noGravity = true;
                        d.velocity *= 0.1f;
                    }
                }
            }

            spriteBatch.End();
            
            // Draw glow overlay
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

            for (int i = trackedGores.Count - 1; i >= 0; i--)
            {
                FrozenGoreData data = trackedGores[i];
                Gore gore = data.GoreReference;

                if (!gore.active) continue;

                float progress = (currentTick - data.StartTick) / (float)data.Duration;
                if (progress >= 1f) continue;

                Color glowColor = GetFrozenGoreGlowColor(progress);
                
                Texture2D texture = null;
                if (gore.type >= 0 && gore.type < TextureAssets.Gore.Length)
                {
                    texture = TextureAssets.Gore[gore.type].Value;
                }

                if (texture != null)
                {
                    // Same source-rect logic as vanilla Gore.cs — works with any texture layout
                    Rectangle sourceRect = gore.Frame.GetSourceRectangle(texture);
                    int frameWidth = sourceRect.Width;
                    int frameHeight = sourceRect.Height;

                    Vector2 origin = new Vector2(frameWidth / 2f, frameHeight / 2f);
                    Vector2 drawPos = gore.position + origin - Main.screenPosition;

                    spriteBatch.Draw(texture, drawPos, sourceRect, glowColor, gore.rotation, origin, gore.scale, SpriteEffects.None, 0f);
                }
            }

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
        }
        
        /// <summary>
        /// Mark an NPC as frozen for visual effect and gore tracking
        /// </summary>
        public static void MarkNPCFrozen(NPC npc)
        {
            if (npc != null && npc.active)
            {
                FrozenNPCVisualManager.MarkNPCAsFrozen(npc.whoAmI);
            }
        }
    }
}

