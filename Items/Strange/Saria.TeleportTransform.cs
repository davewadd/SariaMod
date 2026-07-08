using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using SariaMod.Buffs;
using SariaMod.Dusts;
using SariaMod.Items.Amber;
using SariaMod.Items.Amethyst;
using SariaMod.Items.Bands;
using SariaMod.Items.Emerald;
using SariaMod.Items.Ruby;
using SariaMod.Items.Sapphire;
using SariaMod.Items.Topaz;
using SariaMod.Items.zPearls;
using SariaMod.Items.zTalking;
using SariaMod.Items.Strange;
using Terraria.Localization;
using System;
using Terraria.Map;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Events;
using Terraria.ID;
using Terraria.ModLoader;
using System.Linq;
using SariaMod.Diagnostics;
using SariaMod.Netcode.SariaSoundSync;
using ReLogic.Utilities;
namespace SariaMod.Items.Strange
{
    public partial class Saria
    {
        public const int TransformDuration = 180;
        public int PendingTransform = -1;
        public int TransformTimer;
        public bool IsTransforming => TransformTimer > 0;

        // --- Transform visual effect draw-side state ---
        private const int TransformGrowTicks      = 45;  // ticks for sphere to grow to full size
        private const int TransformPopTicks        = 20;  // ticks for the pop burst to play after timer = 0
        private float _transformSphereScale        = 0f;
        private float _transformPulsePhase         = 0f;
        private float _transformWavePhase          = 0f;  // drives the liquid-edge ripple
        private int   _transformPopCountdown       = 0;
        private int   _prevTransformTimer          = -1;  // -1 = uninitialized; prevents false pop on first frame
        private int   _prevTransformTimerRemote     = -1;  // non-owner edge detection for sounds
        private int   _prevTeleportTimerRemote      = -1;  // non-owner edge detection for teleport sounds/visuals
        private SlotId _transformLoopSlot;                  // tracks the currently playing TransformLoop instance
        private int   _transformLoopAge             = -1;  // ticks since the loop last started; -1 = not playing
        private struct TransformGlob { public float Angle, Distance, SpawnDistance, Speed, MaxSize; }
        private readonly List<TransformGlob> _transformGlobs = new List<TransformGlob>();
        private struct TransformPillar { public float Angle, RotSpeed, Length, MaxLength, Width, Life, MaxLife; }
        private readonly List<TransformPillar> _transformPillars = new List<TransformPillar>();

        // --- Teleport visual effect state (pink sphere at source + destination) ---
        private float  _tpSphereScale  = 0f;
        private float  _tpPulsePhase   = 0f;
        private float  _tpWavePhase    = 0f;
        private int    _tpActiveDuration = 0;                // duration the current teleport wind-up was started with
        private SlotId _tpLoopSlot;                          // currently playing TransformLoop instance for teleport wind-up (source)
        private SlotId _tpDestLoopSlot;                       // currently playing TransformLoop instance at the destination
        private readonly List<TransformGlob>   _tpSourceGlobs   = new List<TransformGlob>();
        private readonly List<TransformPillar> _tpSourcePillars = new List<TransformPillar>();
        private readonly List<TransformGlob>   _tpDestGlobs     = new List<TransformGlob>();
        private readonly List<TransformPillar> _tpDestPillars   = new List<TransformPillar>();
        // Cached world positions at the moment a teleport wind-up starts on a non-owner client.
        // Needed because by the time tpJustEnded fires, the position netUpdate has already snapped
        // Projectile.Center to the destination and _inWallEscapeTarget has been zeroed.
        private Vector2 _tpCachedSrc  = Vector2.Zero;
        private Vector2 _tpCachedDest = Vector2.Zero;

        /// <summary>
        /// Updates teleport glow-sphere state. Scale grows 0→1 over the teleport wind-up duration.
        /// <summary>
        /// Starts the teleport wind-up to a specific world position.
        /// Safe to call from outside (e.g. FeelingRod). Owner-only — caller must guard with Main.myPlayer == Projectile.owner.
        /// Uses IdleTeleportDuration (2 seconds) as the wind-up time.
        /// </summary>
        public void StartForcedTeleport(Vector2 worldTarget)
        {
            if (_inWallTeleportTimer > 0 || _pathTeleportTimer > 0 || _idleTeleportTimer > 0)
                return; // already in a teleport wind-up

            _idleTeleportTarget  = worldTarget;
            _idleTeleportTimer   = IdleTeleportDuration;
            StartTeleportWindUp(worldTarget, IdleTeleportDuration);
        }


        /// <summary>
        /// Initiates the teleport wind-up: locks the escape target, starts the countdown timer,
        /// plays the sting + loop sound, and spawns the initial dust burst.
        /// Callers must set any scenario-specific fields (_pathTeleportTimer, _idleTeleportTimer, etc.)
        /// BEFORE calling this method.
        /// </summary>
        private void StartTeleportWindUp(Vector2 position, int duration)
        {
            _inWallEscapeTarget  = position;
            _inWallTeleportTimer = duration;
            _tpActiveDuration    = duration;
            Projectile.netUpdate = true;

            if (Main.netMode != NetmodeID.Server)
            {
                SoundEngine.PlaySound(SoundID.Item4, Projectile.Center);
                if (SoundEngine.TryGetActiveSound(_tpLoopSlot, out ActiveSound tpOld))
                    tpOld.Stop();
                _tpLoopSlot = SoundEngine.PlaySound(
                    new SoundStyle("SariaMod/Sounds/TransformLoop") { MaxInstances = 1, Volume = 0.7f },
                    Projectile.Center);
                for (int _i = 0; _i < 20; _i++)
                {
                    Vector2 _vel = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(1.5f, 4f);
                    Dust _d = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<AbsorbPsychic>(), _vel, Scale: 1.4f);
                    _d.noGravity = true;
                }
            }
        }

        /// <summary>
        /// Spawns the teleport burst visual effect (large + small dust rings) at the given world position.

        /// </summary>
        private void SpawnTeleportBurst(Vector2 position)
        {
            if (Main.netMode == NetmodeID.Server) return;
            for (int _i = 0; _i < 70; _i++)
            {
                Vector2 _vel = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(4f, 12f);
                Dust _d = Dust.NewDustPerfect(position, ModContent.DustType<AbsorbPsychic>(), _vel, Scale: 1.8f);
                _d.noGravity = true;
            }
            for (int _i = 0; _i < 25; _i++)
            {
                Vector2 _vel = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(0.5f, 3f);
                Dust _d = Dust.NewDustPerfect(position, ModContent.DustType<AbsorbPsychic>(), _vel, Scale: 1.2f);
                _d.noGravity = true;
            }
        }

        /// <summary>
        /// Ages, rotates, and despawns expired pillars in the given list.
        /// Spawns new pillars to keep the count at 3 (non-server only).
        /// Shared by TickTransformPhase and TickTeleportPhase.

        /// </summary>
        private void TickPillarList(List<TransformPillar> list)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                TransformPillar p = list[i];
                p.Life  -= 1f;
                p.Angle += p.RotSpeed;
                list[i]  = p;
                if (p.Life <= 0f) list.RemoveAt(i);
            }
            if (Main.netMode != NetmodeID.Server)
            {
                while (list.Count < 3)
                {
                    float sizeRoll = Main.rand.NextFloat(1f);
                    float maxLen   = sizeRoll < 0.60f
                        ? Main.rand.NextFloat(280f, 400f)
                        : Main.rand.NextFloat(150f, 250f);
                    float baseWidth = sizeRoll < 0.60f
                        ? Main.rand.NextFloat(10f, 18f)
                        : Main.rand.NextFloat(7f, 11f);
                    float life = Main.rand.NextFloat(90f, 160f);
                    list.Add(new TransformPillar
                    {
                        Angle     = Main.rand.NextFloat(MathHelper.TwoPi),
                        RotSpeed  = Main.rand.NextFloat(0.003f, 0.009f) * (Main.rand.NextBool() ? 1f : -1f),
                        Length    = 0f,
                        MaxLength = maxLen,
                        Width     = baseWidth,
                        Life      = life,
                        MaxLife   = life,
                    });
                }
            }
        }

        /// <summary>
        /// Drives both the source (Saria) and destination sphere via shared phase values.
        /// Called once per PostDraw before any teleport draw calls.

        /// </summary>
        private void TickTeleportPhase()
        {
            if (_inWallTeleportTimer <= 0)
            {
                // Not in teleport phase — clear everything.
                _tpSphereScale   = 0f;
                _tpPulsePhase    = 0f;
                _tpActiveDuration = 0;
                _tpSourceGlobs.Clear();
                _tpSourcePillars.Clear();
                _tpDestGlobs.Clear();
                _tpDestPillars.Clear();
                if (SoundEngine.TryGetActiveSound(_tpLoopSlot,     out ActiveSound orphan))  orphan.Stop();
                if (SoundEngine.TryGetActiveSound(_tpDestLoopSlot, out ActiveSound orphan2)) orphan2.Stop();
                return;
            }

            // Scale: grows from 0 at timer start to 1 at timer = 0.
            // Use the stored active duration so both 30-tick (in-wall) and
            // 300-tick (path-blocked) wind-ups reach full scale at the end.
            int activeDur = _tpActiveDuration > 0 ? _tpActiveDuration : InWallTeleportDuration;
            float progress = Math.Clamp(
                (float)(activeDur - _inWallTeleportTimer) / activeDur,
                0f, 1f);
            _tpSphereScale  = progress;
            _tpPulsePhase  += 0.08f;
            _tpWavePhase   += 0.12f;

            if (Main.netMode == NetmodeID.Server) return;

            // Re-fire the source loop every 181 ticks so it covers the full wind-up duration.
            // On the very first tick (activeDur - timer == 0) the loop was already started
            // at the call site; subsequent re-fires keep it seamless.
            int elapsed = activeDur - _inWallTeleportTimer;
            bool isLoopRetrigger = elapsed > 0 && (elapsed % 181 == 0);
            if (isLoopRetrigger)
            {
                if (SoundEngine.TryGetActiveSound(_tpLoopSlot, out ActiveSound prev)) prev.Stop();
                _tpLoopSlot = SoundEngine.PlaySound(
                    new SoundStyle("SariaMod/Sounds/TransformLoop") { MaxInstances = 1, Volume = 0.7f },
                    Projectile.Center);
            }

            // Keep source loop tethered to Saria's position.
            if (SoundEngine.TryGetActiveSound(_tpLoopSlot, out ActiveSound tpActive))
                tpActive.Position = Projectile.Center;

            // Destination loop: play at the escape target so the player hears it near
            // their camera even when Saria is far away.
            // "Far" = escape target is more than one screen width from Saria.
            bool hasDest = _inWallEscapeTarget != Vector2.Zero;
            bool destFar = hasDest &&
                           Vector2.Distance(Projectile.Center, _inWallEscapeTarget) > Main.screenWidth;
            if (hasDest)
            {
                bool destLoopNeeded = !SoundEngine.TryGetActiveSound(_tpDestLoopSlot, out ActiveSound destActive);
                if (destLoopNeeded || isLoopRetrigger)
                {
                    if (SoundEngine.TryGetActiveSound(_tpDestLoopSlot, out ActiveSound prev2)) prev2.Stop();
                    // If far, play without position so attenuation doesn't kill it;
                    // if close, anchor it to the destination in world space.
                    _tpDestLoopSlot = destFar
                        ? SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/TransformLoop") { MaxInstances = 1, Volume = 0.5f })
                        : SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/TransformLoop") { MaxInstances = 1, Volume = 0.5f }, _inWallEscapeTarget);
                }
                else if (!destFar && SoundEngine.TryGetActiveSound(_tpDestLoopSlot, out ActiveSound destPos))
                {
                    destPos.Position = _inWallEscapeTarget;
                }
            }
            else
            {
                // No destination — stop dest loop if it somehow lingered.
                if (SoundEngine.TryGetActiveSound(_tpDestLoopSlot, out ActiveSound stale)) stale.Stop();
            }

            // Tick and spawn pillars for source.
            TickPillarList(_tpSourcePillars);
            // Tick and spawn pillars for destination.
            TickPillarList(_tpDestPillars);
        }

        /// <summary>
        /// Updates transform phase values for this frame (pop detection, scale/alpha/pulse).
        /// Must be called once per PostDraw before any transform draw calls.

        /// </summary>
        private void TickTransformPhase()
        {
            bool justPopped = (_prevTransformTimer > 0 && TransformTimer == 0);
            _prevTransformTimer = TransformTimer;

            if (justPopped)
                _transformPopCountdown = TransformPopTicks;

            bool isActive = IsTransforming || _transformPopCountdown > 0;
            if (!isActive)
            {
                _transformSphereScale = 0f;
                _transformPulsePhase  = 0f;
                _transformGlobs.Clear();
                _transformPillars.Clear();
                return;
            }

            if (IsTransforming)
            {
                float growProgress = Math.Clamp((TransformDuration - TransformTimer) / (float)TransformGrowTicks, 0f, 1f);
                _transformSphereScale = growProgress;
                _transformPulsePhase += 0.08f;
                _transformWavePhase  += 0.12f;  // inner ripple advances fastest

                TickPillarList(_transformPillars);
            }
            else if (_transformPopCountdown > 0)
            {
                _transformSphereScale = 0f;
                _transformPopCountdown--;

                // Burst dust on first pop frame only
                if (_transformPopCountdown == TransformPopTicks - 1 && Main.netMode != NetmodeID.Server)
                {
                    // Fast outward burst — 75% white, 25% pink (AbsorbPsychic)
                    for (int i = 0; i < 70; i++)
                    {
                        Vector2 vel = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(4f, 12f);
                        int dustType = Main.rand.NextBool(4) ? ModContent.DustType<AbsorbPsychic>() : DustID.Cloud;
                        Dust d = Dust.NewDustPerfect(Projectile.Center, dustType, vel, Scale: 1.8f);
                        d.noGravity = true;
                    }
                    // Slow inner burst — 75% white, 25% pink
                    for (int i = 0; i < 25; i++)
                    {
                        Vector2 vel = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(0.5f, 3f);
                        int dustType = Main.rand.NextBool(4) ? ModContent.DustType<AbsorbPsychic>() : DustID.Cloud;
                        Dust d = Dust.NewDustPerfect(Projectile.Center, dustType, vel, Scale: 1.2f);
                        d.noGravity = true;
                    }
                }
            }
        }

        /// <summary>
        /// Draws the transformation glow sphere. Called absolutely last in PostDraw so it
        /// renders on top of every body layer, arm, face, and UI element.

        /// </summary>
        private void DrawTransformGlowSphere()
        {
            if (_transformSphereScale <= 0f)
            {
                _transformGlobs.Clear();
                return;
            }
            DrawGlowSphere(Projectile.Center, _transformSphereScale, Color.White,
                           ref _transformPulsePhase, ref _transformWavePhase,
                           _transformGlobs, _transformPillars);
        }

        /// <summary>
        /// Shared glow-sphere drawing routine used by both transform and teleport spheres.
        /// Draws concentric soft circles, absorption globs, and light pillars in additive blend.
        /// Phase state evolves independently per sphere type via ref parameters.
        /// Must be called while the SpriteBatch is in AlphaBlend state;
        /// this method switches to Additive internally and restores AlphaBlend before returning.

        /// </summary>
        private void DrawGlowSphere(Vector2 center, float scale, Color color,
                                    ref float pulsePhase, ref float wavePhase,
                                    List<TransformGlob> globs, List<TransformPillar> pillars)
        {
            if (scale <= 0f)
            {
                globs.Clear();
                return;
            }

            Vector2 screenPos = center - Main.screenPosition - new Vector2(0f, 8f);
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            const float baseRadius = 90f;
            float s = scale;

            // ref parameters cannot be captured by local functions — copy to locals
            float localWavePhase  = wavePhase;
            float localPulsePhase = pulsePhase;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);

            // waveFreq    = number of ripple peaks around the edge
            // waveAmp     = max pixel distortion of the edge radius
            // phaseOffset = added to the wave argument — use radius-proportional values so crests appear to travel outward
            // edgeFalloff = exponent for the alpha fade: 2 = soft, higher = sharper edge
            // aspectX     = horizontal stretch multiplier (>1 = wider)
            // aspectTop   = vertical stretch upward  (<1 = less tall)
            // aspectBot   = vertical stretch downward (<1 = less tall)
            void DrawSoftCircle(Vector2 centre, float radius, float alphaScale,
                                float waveFreq = 0f, float waveAmp = 0f, float phaseOffset = 0f,
                                float edgeFalloff = 2f,
                                float aspectX = 1f, float aspectTop = 1.4f, float aspectBot = 1.6f)
            {
                const int step = 3;
                float radiusTop    = radius * aspectTop;
                float radiusBottom = radius * aspectBot;

                for (float y = -radiusTop; y <= radiusBottom; y += step)
                {
                    float normY  = y < 0f ? y / radiusTop : y / radiusBottom;
                    float sinArg = (float)Math.Atan2(y, radius);
                    float ripple = waveAmp > 0f
                        ? waveAmp * (float)Math.Sin(waveFreq * sinArg + localWavePhase + phaseOffset)
                        : 0f;
                    float radiusX = (radius + ripple) * aspectX;
                    float halfW   = radiusX * (float)Math.Sqrt(Math.Max(0f, 1f - normY * normY));
                    if (halfW < 1f) continue;
                    float t        = Math.Abs(normY);
                    float rowAlpha = (1f - (float)Math.Pow(t, edgeFalloff)) * alphaScale;
                    if (rowAlpha < 0.005f) continue;
                    Main.spriteBatch.Draw(pixel,
                        new Rectangle((int)(centre.X - halfW), (int)(centre.Y + y), (int)(halfW * 2f), step),
                        null, color * rowAlpha);
                }
            }

            // All rings share the same waveFreq=6 so the shape is consistent.
            // phaseOffset increases with radius so crests appear to travel outward from the center.
            // Amplitude and alpha decrease with each ring — wave energy dissipates as it spreads.
            // The center ring always leads (offset 0); each outer ring is offset by +1.2 radians.
            const float freq       = 6f;
            const float offsetStep = 1.2f;  // radians between rings — tune to taste

            // Center (anchor) — full amplitude, stays fixed as the wave source
            DrawSoftCircle(screenPos, baseRadius * s * 0.40f, s * 1.2f,  waveFreq: freq, waveAmp: 7f,  phaseOffset: 0f,              edgeFalloff: 2f);
            // Mid ring — wider, less tall
            DrawSoftCircle(screenPos, baseRadius * s * 0.70f, s * 0.70f, waveFreq: freq, waveAmp: 5.5f, phaseOffset: offsetStep,      edgeFalloff: 2.5f, aspectX: 1.25f, aspectTop: 1.1f, aspectBot: 1.2f);
            // Outer ring — wider, less tall
            DrawSoftCircle(screenPos, baseRadius * s * 1.08f, s * 0.40f, waveFreq: freq, waveAmp: 4f,  phaseOffset: offsetStep * 2f,  edgeFalloff: 3.5f, aspectX: 1.35f, aspectTop: 1.0f, aspectBot: 1.1f);
            // Halo — wider, less tall, breathing edge
            float haloFalloff = 3.5f + (float)(Math.Sin(localPulsePhase) * 0.5 + 0.5) * 2.5f;
            DrawSoftCircle(screenPos, baseRadius * s * 1.32f, s * 0.32f, waveFreq: freq, waveAmp: 2.5f, phaseOffset: offsetStep * 3f, edgeFalloff: haloFalloff, aspectX: 1.45f, aspectTop: 0.9f, aspectBot: 1.0f);
            // Spike fringe — almost no wave energy left; high falloff so only faint spikes survive
            float spikeFalloff = 7.0f + (float)(Math.Sin(localPulsePhase * 0.6f) * 0.5 + 0.5) * 3.0f;
            DrawSoftCircle(screenPos, baseRadius * s * 1.52f, s * 0.18f, waveFreq: freq, waveAmp: 1.5f, phaseOffset: offsetStep * 4f, edgeFalloff: spikeFalloff);

            // --- Absorption globs: spawn at edge, drift inward, grow then vanish ---
            void DrawGlobSpot(Vector2 pos, float radius, float alpha)
            {
                if (radius < 0.5f || alpha < 0.005f) return;
                int r = Math.Max(1, (int)radius);
                for (int dy = -r; dy <= r; dy++)
                {
                    float normY    = r > 0 ? dy / (float)r : 0f;
                    float halfW    = (float)Math.Sqrt(Math.Max(0f, 1f - normY * normY)) * r;
                    if (halfW < 0.5f) continue;
                    float rowAlpha = (1f - normY * normY) * alpha;
                    if (rowAlpha < 0.005f) continue;
                    Main.spriteBatch.Draw(pixel,
                        new Rectangle((int)(pos.X - halfW), (int)(pos.Y + dy), Math.Max(1, (int)(halfW * 2f)), 1),
                        null, color * rowAlpha);
                }
            }

            // Spawn new globs at the outer edge each frame
            if (Main.netMode != NetmodeID.Server && globs.Count < 60)
            {
                int spawnCount = Main.rand.Next(1, 3);
                for (int i = 0; i < spawnCount; i++)
                {
                    float angle     = Main.rand.NextFloat(MathHelper.TwoPi);
                    float spawnDist = baseRadius * s * Main.rand.NextFloat(1.35f, 1.75f);
                    globs.Add(new TransformGlob
                    {
                        Angle         = angle,
                        Distance      = spawnDist,
                        SpawnDistance = spawnDist,
                        Speed         = Main.rand.NextFloat(0.4f, 1.2f),
                        MaxSize       = Main.rand.NextFloat(3f, 9f) * s,
                    });
                }
            }

            // Move globs inward; bell-curve size grows then shrinks to zero (absorbed)
            for (int i = globs.Count - 1; i >= 0; i--)
            {
                TransformGlob g = globs[i];
                g.Distance     -= g.Speed;
                globs[i] = g;

                if (g.Distance <= 3f)
                {
                    globs.RemoveAt(i);
                    continue;
                }

                float progress  = Math.Clamp(1f - g.Distance / g.SpawnDistance, 0f, 1f);
                float sizeScale = 4f * progress * (1f - progress); // bell: 0 → peak at 50% → 0
                float drawSize  = g.MaxSize * sizeScale;
                float alpha     = sizeScale * 0.85f * s;

                // Squish Y slightly to follow the oval contour
                Vector2 globPos = screenPos + new Vector2(
                    (float)Math.Cos(g.Angle) * g.Distance,
                    (float)Math.Sin(g.Angle) * g.Distance * 0.75f);

                DrawGlobSpot(globPos, drawSize, alpha);
            }
            // --- End absorption globs ---

            // --- Light pillars ---
            foreach (TransformPillar p in pillars)
            {
                float lifeT    = p.Life / p.MaxLife;                          // 1→0 over lifetime
                // Length: grows to full in first 25% of life, then holds at max
                float growT  = Math.Clamp((1f - lifeT) / 0.25f, 0f, 1f);         // 0→1 in first 25%
                float length = p.MaxLength * growT;
                // Alpha envelope: fade in during grow phase, hold, fade out in last 30%
                float envelope = lifeT < 0.3f ? lifeT / 0.3f : 1f;
                if (length < 2f) continue;

                // Wider and longer beams are more translucent — big beams are ghostly, thin beams are bright.
                // Width range ~7-18px → widthFactor ~0.35-0.75 (inverted: wider = lower alpha)
                float widthFactor  = Math.Clamp(1f - (p.Width - 7f) / 20f,  0.30f, 0.85f);
                // Length range ~150-400px → lengthFactor ~0.55-0.90 (inverted: longer = lower alpha)
                float lengthFactor = Math.Clamp(1f - (p.MaxLength - 150f) / 500f, 0.45f, 0.90f);
                float alpha = envelope * s * 0.75f * widthFactor * lengthFactor;
                if (alpha < 0.005f) continue;

                // Draw pillar as scanline slices along its axis.
                // Width flares linearly from 0 at the root to p.Width*3 at the tip.
                float cos = (float)Math.Cos(p.Angle);
                float sin = (float)Math.Sin(p.Angle);
                const int sliceStep = 2;
                for (float d = 0f; d < length; d += sliceStep)
                {
                    float t        = d / length;                            // 0 at root, 1 at tip
                    float halfW    = (p.Width * 0.5f + p.Width * 2.5f * t); // flares outward
                    // Fade only kicks in past 50% — thin beams stay full-bright near the root
                    float fadeFactor = t < 0.5f ? 1f : (float)Math.Pow(1f - ((t - 0.5f) / 0.5f), 2.5f);
                    float rowAlpha = alpha * fadeFactor;
                    if (rowAlpha < 0.005f) continue;

                    // Centre of this slice in screen space
                    float cx = screenPos.X + cos * d;
                    float cy = screenPos.Y + sin * d * 0.75f;               // squish Y to match oval

                    // Perpendicular to the pillar axis
                    float px = -sin;
                    float py =  cos * 0.75f;

                    int rx = (int)(cx - px * halfW);
                    int ry = (int)(cy - py * halfW);
                    int rw = Math.Max(1, (int)(px * halfW * 2f));
                    int rh = Math.Max(1, (int)(py * halfW * 2f));

                    // Normalise into a proper rectangle
                    if (rw < 0) { rx += rw; rw = -rw; }
                    if (rh < 0) { ry += rh; rh = -rh; }
                    if (rw == 0) rw = 1;
                    if (rh == 0) rh = 1;

                    Main.spriteBatch.Draw(pixel, new Rectangle(rx, ry, rw, rh), null, color * rowAlpha);
                }
            }
            // --- End light pillars ---

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
        }

        /// <summary>
        /// Draws the teleport destination sphere in world space.
        /// Called from SariaModSystem.PostDrawTiles so it renders even when
        /// Saria herself is off-screen.

        /// </summary>
        public void DrawTeleportDestination(SpriteBatch spriteBatch)
        {
            if (_tpSphereScale <= 0f || _inWallEscapeTarget == Vector2.Zero) return;
            // PostDrawTiles runs with no active SpriteBatch — start one.
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                Main.DefaultSamplerState, DepthStencilState.None,
                Main.Rasterizer, null, Main.Transform);
            DrawTeleportGlowSphere(_inWallEscapeTarget, _tpSphereScale, _tpDestGlobs, _tpDestPillars);
            // DrawTeleportGlowSphere ends in AlphaBlend — close that batch too.
            spriteBatch.End();
        }

        /// <summary>
        /// Draws the teleport source sphere. Called from PostDraw (Saria visible on screen).

        /// </summary>
        private void DrawTeleportSourceSphere()
        {
            if (_tpSphereScale <= 0f) return;
            DrawTeleportGlowSphere(Projectile.Center, _tpSphereScale, _tpSourceGlobs, _tpSourcePillars);
        }
        /// Pass the source or destination position and the corresponding glob/pillar lists.
        /// Must be called while the SpriteBatch is in its normal AlphaBlend state;
        /// this method switches to Additive internally and restores AlphaBlend before returning.

        /// </summary>
        private void DrawTeleportGlowSphere(Vector2 worldCenter,
                                            float s,
                                            List<TransformGlob>   globs,
                                            List<TransformPillar> pillars)
        {
            if (s <= 0f)
            {
                globs.Clear();
                return;
            }
            DrawGlowSphere(worldCenter, s, new Color(255, 80, 200),
                           ref _tpPulsePhase, ref _tpWavePhase,
                           globs, pillars);
        }
    }
}
