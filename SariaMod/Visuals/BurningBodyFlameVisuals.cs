using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SariaMod.Buffs;
using SariaMod.Gores;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod
{
    /// <summary>
    /// Draws small fire and ice status sprites directly on opaque entity pixels.
    /// These are draw effects only and never create Flame projectiles or Gore objects.
    /// </summary>
    internal static class BurningBodyFlameVisuals
    {
        private const int FlameFrameCount = 6;
        private const int MinimumBodyVisualCount = 3;
        private const float BodyVisualSizeStep = 42f;
        private const byte OpaqueAlphaThreshold = 48;
        private const int MaximumCachedMasks = 1024;
        private const int MaximumCachedMaskBytes = 32 * 1024 * 1024;
        private const float MinimumPlayerFlameSpacingSquared = 64f;
        private const float MinimumPlayerIceSpacingSquared = 49f;

        // Flame's later frames begin before the full 64-pixel cell ends. These
        // baselines keep the following frame from bleeding into the current one.
        private static readonly int[] FlameBodyBaselineSourceYs = { 34, 35, 37, 38, 38, 35 };

        // The first three anchors cover a normal body. Large NPCs progressively
        // receive the remaining anchors so the extra flames stay well spread out.
        private static readonly Vector2[] BodyAnchors =
        {
            new Vector2(0.00f, 0.22f),
            new Vector2(-0.28f, -0.06f),
            new Vector2(0.28f, -0.14f),
            new Vector2(-0.18f, -0.35f),
            new Vector2(0.22f, 0.35f),
            new Vector2(0.13f, -0.43f),
            new Vector2(-0.36f, 0.28f),
            new Vector2(-0.40f, -0.28f),
            new Vector2(0.40f, 0.10f),
            new Vector2(0.00f, 0.42f),
            new Vector2(0.00f, -0.20f),
            new Vector2(-0.10f, 0.08f)
        };

        private static readonly Dictionary<SpriteMaskKey, SpriteAlphaMask> AlphaMaskCache =
            new Dictionary<SpriteMaskKey, SpriteAlphaMask>();
        private static readonly Queue<SpriteMaskKey> AlphaMaskCacheOrder = new Queue<SpriteMaskKey>();
        private static readonly int[] PlayerBodyDrawStarts = new int[Main.maxPlayers];
        private static readonly List<DrawData>[] PlayerBodyDrawCaches = new List<DrawData>[Main.maxPlayers];
        private static Texture2D cachedFlameTexture;
        private static Texture2D cachedIceGore1Texture;
        private static Texture2D cachedIceGore2Texture;
        private static int cachedMaskBytes;

        internal static void ClearCache()
        {
            AlphaMaskCache.Clear();
            AlphaMaskCacheOrder.Clear();
            cachedFlameTexture = null;
            cachedIceGore1Texture = null;
            cachedIceGore2Texture = null;
            cachedMaskBytes = 0;
            Array.Clear(PlayerBodyDrawStarts, 0, PlayerBodyDrawStarts.Length);
            Array.Clear(PlayerBodyDrawCaches, 0, PlayerBodyDrawCaches.Length);
        }

        internal static void DrawNPCFlames(NPC npc, SpriteBatch spriteBatch, Vector2 screenPosition)
        {
            if (Main.netMode == Terraria.ID.NetmodeID.Server
                || npc == null
                || !npc.active
                || npc.IsABestiaryIconDummy
                || (!npc.GetGlobalNPC<FairyGlobalNPC>().Burning2
                    && !npc.HasBuff(ModContent.BuffType<Burning2>())))
            {
                return;
            }

            if (!TryGetNPCBodyMask(npc, out Rectangle sourceRectangle, out SpriteAlphaMask mask))
            {
                return;
            }

            float visualSize = Math.Max(mask.OpaqueBounds.Width, mask.OpaqueBounds.Height) * npc.scale;
            int flameCount = GetBodyVisualCount(visualSize);
            float flameScale = MathHelper.Clamp(0.38f + visualSize / 500f, 0.40f, 0.68f);
            List<int> usedPoints = new List<int>(flameCount);

            for (int i = 0; i < flameCount; i++)
            {
                Vector2 target = GetMaskTarget(mask, BodyAnchors[i]);
                int pointIndex = FindNearestUnusedPoint(mask.OpaqueSamples, target, usedPoints);
                if (pointIndex < 0)
                {
                    continue;
                }

                usedPoints.Add(pointIndex);
                Vector2 sourcePoint = mask.OpaqueSamples[pointIndex];
                Vector2 worldPosition = GetNPCBodyWorldPosition(npc, sourceRectangle, sourcePoint);
                DrawFlameSprite(
                    spriteBatch,
                    worldPosition - screenPosition,
                    flameScale,
                    npc.whoAmI * 17 + i * 31,
                    (npc.whoAmI + i) % 2 == 0);
            }
        }

        internal static void DrawNPCIceGores(NPC npc, SpriteBatch spriteBatch, Vector2 screenPosition)
        {
            if (Main.netMode == NetmodeID.Server
                || npc == null
                || !npc.active
                || npc.IsABestiaryIconDummy
                || npc.GetGlobalNPC<FairyGlobalNPC>().Burning2
                || npc.HasBuff(ModContent.BuffType<Burning2>())
                || CharredNPCVisualManager.HasCharredEffect(npc.whoAmI)
                || !FrozenNPCVisualManager.HasChilledGoreEffect(npc.whoAmI))
            {
                return;
            }

            Color? frozenTint = FrozenNPCVisualManager.GetFrozenTintColor(npc.whoAmI);
            if (!frozenTint.HasValue
                || !TryGetNPCBodyMask(npc, out Rectangle sourceRectangle, out SpriteAlphaMask mask))
            {
                return;
            }

            float effectStrength = frozenTint.Value.A / 255f;
            float visualSize = Math.Max(mask.OpaqueBounds.Width, mask.OpaqueBounds.Height) * npc.scale;
            int iceGoreCount = GetBodyVisualCount(visualSize);
            float iceGoreScale = MathHelper.Clamp(0.42f + visualSize / 220f, 0.48f, 0.95f);
            List<int> usedPoints = new List<int>(iceGoreCount);

            for (int i = 0; i < iceGoreCount; i++)
            {
                Vector2 target = GetMaskTarget(mask, BodyAnchors[i]);
                int pointIndex = FindNearestUnusedPoint(mask.OpaqueSamples, target, usedPoints);
                if (pointIndex < 0)
                {
                    continue;
                }

                usedPoints.Add(pointIndex);
                Vector2 worldPosition = GetNPCBodyWorldPosition(
                    npc,
                    sourceRectangle,
                    mask.OpaqueSamples[pointIndex]);
                DrawIceGoreSprite(
                    spriteBatch,
                    worldPosition,
                    worldPosition - screenPosition,
                    iceGoreScale,
                    effectStrength,
                    npc.whoAmI * 23 + i * 37,
                    (npc.whoAmI + i) % 2 == 0,
                    i % 3 != 2);
            }
        }

        internal static void AddPlayerFlames(ref PlayerDrawSet drawInfo)
        {
            Player player = drawInfo.drawPlayer;
            if (player.whoAmI < 0
                || player.whoAmI >= PlayerBodyDrawStarts.Length
                || !ReferenceEquals(PlayerBodyDrawCaches[player.whoAmI], drawInfo.DrawDataCache))
            {
                return;
            }

            int bodyDrawDataStart = PlayerBodyDrawStarts[player.whoAmI];
            int originalDrawDataCount = drawInfo.DrawDataCache.Count;
            if (bodyDrawDataStart < 0 || bodyDrawDataStart >= originalDrawDataCount)
            {
                return;
            }

            Vector2 visualTopLeft = drawInfo.Position - Main.screenPosition;
            Rectangle searchArea = new Rectangle(
                (int)Math.Floor(visualTopLeft.X - 6f),
                (int)Math.Floor(visualTopLeft.Y - 7f),
                player.width + 12,
                player.height + 8);

            float visualSize = Math.Max(searchArea.Width, searchArea.Height);
            int playerFlameCount = GetBodyVisualCount(visualSize);
            List<Vector2> usedPoints = new List<Vector2>(playerFlameCount);
            for (int i = 0; i < playerFlameCount; i++)
            {
                Vector2 anchor = BodyAnchors[i];
                Vector2 target = new Vector2(
                    searchArea.Center.X + anchor.X * searchArea.Width,
                    searchArea.Center.Y + anchor.Y * searchArea.Height);
                if (!TryFindNearestOpaquePlayerPoint(
                    target,
                    searchArea,
                    drawInfo.DrawDataCache,
                    bodyDrawDataStart,
                    originalDrawDataCount,
                    usedPoints,
                    MinimumPlayerFlameSpacingSquared,
                    out Vector2 bodyPoint))
                {
                    continue;
                }

                usedPoints.Add(bodyPoint);
                AddFlameDrawData(
                    drawInfo.DrawDataCache,
                    bodyPoint,
                    0.40f,
                    player.whoAmI * 19 + i * 29,
                    (player.whoAmI + i) % 2 == 0);
            }
        }

        internal static void AddPlayerIceGores(ref PlayerDrawSet drawInfo)
        {
            Player player = drawInfo.drawPlayer;
            if (player.whoAmI < 0
                || player.whoAmI >= PlayerBodyDrawStarts.Length
                || !ReferenceEquals(PlayerBodyDrawCaches[player.whoAmI], drawInfo.DrawDataCache))
            {
                return;
            }

            int bodyDrawDataStart = PlayerBodyDrawStarts[player.whoAmI];
            int originalDrawDataCount = drawInfo.DrawDataCache.Count;
            if (bodyDrawDataStart < 0 || bodyDrawDataStart >= originalDrawDataCount)
            {
                return;
            }

            Vector2 visualTopLeft = drawInfo.Position - Main.screenPosition;
            Rectangle searchArea = new Rectangle(
                (int)Math.Floor(visualTopLeft.X - 6f),
                (int)Math.Floor(visualTopLeft.Y - 7f),
                player.width + 12,
                player.height + 8);
            float visualSize = Math.Max(searchArea.Width, searchArea.Height);
            int iceGoreCount = GetBodyVisualCount(visualSize);
            float iceGoreScale = MathHelper.Clamp(0.42f + visualSize / 220f, 0.48f, 0.95f);
            List<Vector2> usedPoints = new List<Vector2>(iceGoreCount);
            Lighting.AddLight(player.Center, new Vector3(0.6f, 0.85f, 1f) * 0.45f);

            for (int i = 0; i < iceGoreCount; i++)
            {
                Vector2 anchor = BodyAnchors[i];
                Vector2 target = new Vector2(
                    searchArea.Center.X + anchor.X * searchArea.Width,
                    searchArea.Center.Y + anchor.Y * searchArea.Height);
                if (!TryFindNearestOpaquePlayerPoint(
                    target,
                    searchArea,
                    drawInfo.DrawDataCache,
                    bodyDrawDataStart,
                    originalDrawDataCount,
                    usedPoints,
                    MinimumPlayerIceSpacingSquared,
                    out Vector2 bodyPoint))
                {
                    continue;
                }

                usedPoints.Add(bodyPoint);
                Vector2 worldPosition = bodyPoint + Main.screenPosition;
                AddIceGoreDrawData(
                    drawInfo.DrawDataCache,
                    worldPosition,
                    bodyPoint,
                    iceGoreScale,
                    1f,
                    player.whoAmI * 31 + i * 41,
                    (player.whoAmI + i) % 2 == 0,
                    i % 3 != 2);
            }
        }

        internal static void RecordPlayerBodyDrawStart(ref PlayerDrawSet drawInfo)
        {
            int playerIndex = drawInfo.drawPlayer.whoAmI;
            if (playerIndex < 0 || playerIndex >= PlayerBodyDrawStarts.Length)
            {
                return;
            }

            PlayerBodyDrawStarts[playerIndex] = drawInfo.DrawDataCache.Count;
            PlayerBodyDrawCaches[playerIndex] = drawInfo.DrawDataCache;
        }

        private static bool TryGetNPCBodyMask(
            NPC npc,
            out Rectangle sourceRectangle,
            out SpriteAlphaMask mask)
        {
            sourceRectangle = Rectangle.Empty;
            mask = null;
            if (npc.type < 0 || npc.type >= TextureAssets.Npc.Length)
            {
                return false;
            }

            Texture2D npcTexture;
            try
            {
                npcTexture = TextureAssets.Npc[npc.type].Value;
            }
            catch
            {
                return false;
            }

            sourceRectangle = npc.frame;
            if (!IsValidSourceRectangle(npcTexture, sourceRectangle))
            {
                int frameCount = Math.Max(1, Main.npcFrameCount[npc.type]);
                sourceRectangle = npcTexture.Frame(verticalFrames: frameCount);
            }

            return TryGetMask(npcTexture, sourceRectangle, out mask)
                && mask.OpaqueSamples.Count > 0;
        }

        private static Vector2 GetNPCBodyWorldPosition(
            NPC npc,
            Rectangle sourceRectangle,
            Vector2 sourcePoint)
        {
            // Terraria anchors the standard NPC draw at its feet, not at the
            // hitbox center. Match DrawNPCDirect_Inner's near-bottom origin so
            // tall frames and sprites that extend above their hitbox stay lined up.
            Vector2 drawOrigin = new Vector2(
                sourceRectangle.Width * 0.5f,
                sourceRectangle.Height - 4f);
            Vector2 localOffset = sourcePoint - drawOrigin;

            if (npc.spriteDirection == 1)
            {
                localOffset.X *= -1f;
            }

            localOffset *= npc.scale;
            localOffset = localOffset.RotatedBy(npc.rotation);
            return npc.Bottom
                + new Vector2(0f, npc.gfxOffY + Main.NPCAddHeight(npc))
                + localOffset;
        }

        private static Vector2 GetMaskTarget(SpriteAlphaMask mask, Vector2 anchor)
        {
            return new Vector2(
                mask.OpaqueBounds.Center.X + anchor.X * mask.OpaqueBounds.Width,
                mask.OpaqueBounds.Center.Y + anchor.Y * mask.OpaqueBounds.Height);
        }

        private static int FindNearestUnusedPoint(
            List<Vector2> points,
            Vector2 target,
            List<int> usedPoints)
        {
            int bestIndex = -1;
            float bestDistanceSquared = float.MaxValue;
            for (int i = 0; i < points.Count; i++)
            {
                if (usedPoints.Contains(i))
                {
                    continue;
                }

                float distanceSquared = Vector2.DistanceSquared(points[i], target);
                if (distanceSquared < bestDistanceSquared)
                {
                    bestDistanceSquared = distanceSquared;
                    bestIndex = i;
                }
            }

            return bestIndex;
        }

        private static bool TryFindNearestOpaquePlayerPoint(
            Vector2 target,
            Rectangle searchArea,
            List<DrawData> drawDataCache,
            int bodyDrawDataStart,
            int bodyDrawDataEnd,
            List<Vector2> usedPoints,
            float minimumSpacingSquared,
            out Vector2 bodyPoint)
        {
            Point roundedTarget = target.ToPoint();
            int maximumRadius = Math.Max(searchArea.Width, searchArea.Height);
            for (int radius = 0; radius <= maximumRadius; radius += 2)
            {
                for (int yOffset = -radius; yOffset <= radius; yOffset += 2)
                {
                    for (int xOffset = -radius; xOffset <= radius; xOffset += 2)
                    {
                        if (radius > 0
                            && Math.Abs(xOffset) != radius
                            && Math.Abs(yOffset) != radius)
                        {
                            continue;
                        }

                        Vector2 candidate = new Vector2(
                            roundedTarget.X + xOffset,
                            roundedTarget.Y + yOffset);
                        if (!searchArea.Contains(candidate.ToPoint())
                            || IsTooCloseToExistingPoint(candidate, usedPoints, minimumSpacingSquared)
                            || !IsOpaqueInPlayerDrawData(
                                candidate,
                                drawDataCache,
                                bodyDrawDataStart,
                                bodyDrawDataEnd))
                        {
                            continue;
                        }

                        bodyPoint = candidate;
                        return true;
                    }
                }
            }

            bodyPoint = Vector2.Zero;
            return false;
        }

        private static bool IsTooCloseToExistingPoint(
            Vector2 candidate,
            List<Vector2> usedPoints,
            float minimumSpacingSquared)
        {
            for (int i = 0; i < usedPoints.Count; i++)
            {
                if (Vector2.DistanceSquared(candidate, usedPoints[i]) < minimumSpacingSquared)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsOpaqueInPlayerDrawData(
            Vector2 screenPoint,
            List<DrawData> drawDataCache,
            int bodyDrawDataStart,
            int bodyDrawDataEnd)
        {
            // Only the range recorded between the Skin and Head layers qualifies.
            // This excludes wings, mounts, balloons, held items and effect layers.
            for (int i = bodyDrawDataEnd - 1; i >= bodyDrawDataStart; i--)
            {
                DrawData drawData = drawDataCache[i];
                if (drawData.texture == null
                    || drawData.texture.IsDisposed
                    || drawData.color.PackedValue == 0
                    || drawData.useDestinationRectangle)
                {
                    continue;
                }

                Rectangle sourceRectangle = drawData.sourceRect
                    ?? new Rectangle(0, 0, drawData.texture.Width, drawData.texture.Height);
                if (sourceRectangle.Width < 4
                    || sourceRectangle.Height < 4
                    || !IsValidSourceRectangle(drawData.texture, sourceRectangle))
                {
                    continue;
                }

                Vector2 scale = drawData.scale;
                if (Math.Abs(scale.X) < 0.001f || Math.Abs(scale.Y) < 0.001f)
                {
                    continue;
                }

                Vector2 localPoint = (screenPoint - drawData.position).RotatedBy(-drawData.rotation);
                localPoint.X /= scale.X;
                localPoint.Y /= scale.Y;
                localPoint += drawData.origin;

                // SpriteEffects flips texture coordinates inside the same drawn
                // rectangle. It does not mirror the rectangle around its origin.
                int sourceX = (int)Math.Floor(localPoint.X);
                int sourceY = (int)Math.Floor(localPoint.Y);
                if ((drawData.effect & SpriteEffects.FlipHorizontally) != 0)
                {
                    sourceX = sourceRectangle.Width - 1 - sourceX;
                }
                if ((drawData.effect & SpriteEffects.FlipVertically) != 0)
                {
                    sourceY = sourceRectangle.Height - 1 - sourceY;
                }
                if (sourceX < 0
                    || sourceY < 0
                    || sourceX >= sourceRectangle.Width
                    || sourceY >= sourceRectangle.Height)
                {
                    continue;
                }

                if (TryGetMask(drawData.texture, sourceRectangle, out SpriteAlphaMask mask)
                    && mask.IsOpaque(sourceX, sourceY))
                {
                    return true;
                }
            }

            return false;
        }

        private static void DrawFlameSprite(
            SpriteBatch spriteBatch,
            Vector2 screenPosition,
            float scale,
            int animationOffset,
            bool flipHorizontally)
        {
            Texture2D flameTexture = GetFlameTexture();
            int frameIndex = GetFlameFrame(animationOffset);
            Rectangle sourceRectangle = flameTexture.Frame(verticalFrames: FlameFrameCount, frameY: frameIndex);
            sourceRectangle.Height = FlameBodyBaselineSourceYs[frameIndex];
            Vector2 origin = new Vector2(sourceRectangle.Width * 0.5f, sourceRectangle.Height);
            SpriteEffects effects = SpriteEffects.FlipVertically;
            if (flipHorizontally)
            {
                effects |= SpriteEffects.FlipHorizontally;
            }

            spriteBatch.Draw(
                flameTexture,
                screenPosition,
                sourceRectangle,
                new Color(255, 255, 255, 230),
                0f,
                origin,
                scale,
                effects,
                0f);
        }

        private static void AddFlameDrawData(
            List<DrawData> drawDataCache,
            Vector2 screenPosition,
            float scale,
            int animationOffset,
            bool flipHorizontally)
        {
            Texture2D flameTexture = GetFlameTexture();
            int frameIndex = GetFlameFrame(animationOffset);
            Rectangle sourceRectangle = flameTexture.Frame(verticalFrames: FlameFrameCount, frameY: frameIndex);
            sourceRectangle.Height = FlameBodyBaselineSourceYs[frameIndex];
            Vector2 origin = new Vector2(sourceRectangle.Width * 0.5f, sourceRectangle.Height);
            SpriteEffects effects = SpriteEffects.FlipVertically;
            if (flipHorizontally)
            {
                effects |= SpriteEffects.FlipHorizontally;
            }

            drawDataCache.Add(new DrawData(
                flameTexture,
                screenPosition,
                sourceRectangle,
                new Color(255, 255, 255, 230),
                0f,
                origin,
                scale,
                effects,
                0));
        }

        private static void DrawIceGoreSprite(
            SpriteBatch spriteBatch,
            Vector2 worldPosition,
            Vector2 screenPosition,
            float baseScale,
            float effectStrength,
            int visualOffset,
            bool flipHorizontally,
            bool useIceGore1)
        {
            Texture2D texture = GetIceGoreTexture(useIceGore1);
            GetIceGoreTransform(
                baseScale,
                visualOffset,
                flipHorizontally,
                out float scale,
                out float rotation,
                out SpriteEffects effects);
            Color drawColor = GetIceGoreDrawColor(worldPosition, effectStrength);
            Color glowColor = GetIceGoreGlowColor(effectStrength);

            spriteBatch.Draw(
                texture,
                screenPosition,
                null,
                drawColor,
                rotation,
                texture.Size() * 0.5f,
                scale,
                effects,
                0f);

            // NPC post-draw uses AlphaBlend. A zero-alpha color preserves the RGB
            // contribution as an additive overlay, matching the dropped-gore glow
            // without interrupting Terraria's shared NPC SpriteBatch.
            spriteBatch.Draw(
                texture,
                screenPosition,
                null,
                glowColor,
                rotation,
                texture.Size() * 0.5f,
                scale,
                effects,
                0f);

            FrozenGoreSystem.AddFrozenGoreLight(
                worldPosition,
                GetIceGoreLightIntensity(effectStrength, scale, useIceGore1));
        }

        private static void AddIceGoreDrawData(
            List<DrawData> drawDataCache,
            Vector2 worldPosition,
            Vector2 screenPosition,
            float baseScale,
            float effectStrength,
            int visualOffset,
            bool flipHorizontally,
            bool useIceGore1)
        {
            Texture2D texture = GetIceGoreTexture(useIceGore1);
            GetIceGoreTransform(
                baseScale,
                visualOffset,
                flipHorizontally,
                out float scale,
                out float rotation,
                out SpriteEffects effects);

            drawDataCache.Add(new DrawData(
                texture,
                screenPosition,
                null,
                GetIceGoreDrawColor(worldPosition, effectStrength),
                rotation,
                texture.Size() * 0.5f,
                scale,
                effects,
                0));

            drawDataCache.Add(new DrawData(
                texture,
                screenPosition,
                null,
                GetIceGoreGlowColor(effectStrength),
                rotation,
                texture.Size() * 0.5f,
                scale,
                effects,
                0));

            FrozenGoreSystem.AddFrozenGoreLight(
                worldPosition,
                GetIceGoreLightIntensity(effectStrength, scale, useIceGore1));
        }

        private static void GetIceGoreTransform(
            float baseScale,
            int visualOffset,
            bool flipHorizontally,
            out float scale,
            out float rotation,
            out SpriteEffects effects)
        {
            // Ice attached to a body remains rigid. The offset still gives each
            // shard a stable angle, but scale and rotation no longer pulse or sway.
            scale = baseScale;
            rotation = ((visualOffset % 7) - 3) * 0.14f;
            effects = flipHorizontally ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        }

        private static Color GetIceGoreDrawColor(Vector2 worldPosition, float effectStrength)
        {
            Color frozenColor = FrozenGoreSystem.GetFrozenGoreDrawColor(worldPosition);
            return Color.Lerp(Color.Transparent, frozenColor, MathHelper.Clamp(effectStrength, 0f, 1f));
        }

        private static Color GetIceGoreGlowColor(float effectStrength)
        {
            float progress = 1f - MathHelper.Clamp(effectStrength, 0f, 1f);
            Color glowColor = FrozenGoreSystem.GetFrozenGoreGlowColor(progress);
            glowColor.A = 0;
            return glowColor;
        }

        private static float GetIceGoreLightIntensity(
            float effectStrength,
            float scale,
            bool useIceGore1)
        {
            float intensity = MathHelper.Clamp(effectStrength, 0f, 1f) * 0.8f;
            return useIceGore1 ? intensity * scale : intensity;
        }

        private static int GetBodyVisualCount(float visualSize)
        {
            int sizeScaledCount = 2 + (int)(Math.Max(0f, visualSize) / BodyVisualSizeStep);
            return Math.Clamp(sizeScaledCount, MinimumBodyVisualCount, BodyAnchors.Length);
        }

        private static int GetFlameFrame(int animationOffset)
        {
            int gameTicks = (int)(Main.GameUpdateCount % 1000000);
            int frameSpeed = 5 + Math.Abs(animationOffset % 4);
            return (gameTicks / frameSpeed + Math.Abs(animationOffset)) % FlameFrameCount;
        }

        private static Texture2D GetFlameTexture()
        {
            if (cachedFlameTexture == null || cachedFlameTexture.IsDisposed)
            {
                cachedFlameTexture = ModContent.Request<Texture2D>("SariaMod/Items/Ruby/Flame").Value;
            }

            return cachedFlameTexture;
        }

        private static Texture2D GetIceGoreTexture(bool useIceGore1)
        {
            if (useIceGore1)
            {
                if (cachedIceGore1Texture == null || cachedIceGore1Texture.IsDisposed)
                {
                    cachedIceGore1Texture = ModContent.Request<Texture2D>("SariaMod/Gores/IceGore1").Value;
                }

                return cachedIceGore1Texture;
            }

            if (cachedIceGore2Texture == null || cachedIceGore2Texture.IsDisposed)
            {
                cachedIceGore2Texture = ModContent.Request<Texture2D>("SariaMod/Gores/IceGore2").Value;
            }

            return cachedIceGore2Texture;
        }

        private static bool TryGetMask(
            Texture2D texture,
            Rectangle sourceRectangle,
            out SpriteAlphaMask mask)
        {
            SpriteMaskKey key = new SpriteMaskKey(texture, sourceRectangle);
            if (AlphaMaskCache.TryGetValue(key, out mask))
            {
                return mask.Readable;
            }

            mask = SpriteAlphaMask.Create(texture, sourceRectangle);
            int newMaskBytes = mask.Alpha.Length;
            while ((AlphaMaskCache.Count >= MaximumCachedMasks
                    || cachedMaskBytes + newMaskBytes > MaximumCachedMaskBytes)
                && AlphaMaskCacheOrder.Count > 0)
            {
                SpriteMaskKey oldestKey = AlphaMaskCacheOrder.Dequeue();
                if (AlphaMaskCache.Remove(oldestKey, out SpriteAlphaMask oldestMask))
                {
                    cachedMaskBytes -= oldestMask.Alpha.Length;
                }
            }

            AlphaMaskCache[key] = mask;
            AlphaMaskCacheOrder.Enqueue(key);
            cachedMaskBytes += newMaskBytes;
            return mask.Readable;
        }

        private static bool IsValidSourceRectangle(Texture2D texture, Rectangle rectangle)
        {
            return texture != null
                && !texture.IsDisposed
                && rectangle.Width > 0
                && rectangle.Height > 0
                && rectangle.X >= 0
                && rectangle.Y >= 0
                && rectangle.Right <= texture.Width
                && rectangle.Bottom <= texture.Height;
        }

        private readonly struct SpriteMaskKey : IEquatable<SpriteMaskKey>
        {
            private readonly Texture2D texture;
            private readonly Rectangle sourceRectangle;

            internal SpriteMaskKey(Texture2D texture, Rectangle sourceRectangle)
            {
                this.texture = texture;
                this.sourceRectangle = sourceRectangle;
            }

            public bool Equals(SpriteMaskKey other)
            {
                return ReferenceEquals(texture, other.texture)
                    && sourceRectangle.Equals(other.sourceRectangle);
            }

            public override bool Equals(object obj)
            {
                return obj is SpriteMaskKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(RuntimeHelpers.GetHashCode(texture), sourceRectangle);
            }
        }

        private sealed class SpriteAlphaMask
        {
            internal bool Readable { get; }
            internal byte[] Alpha { get; }
            internal int Width { get; }
            internal Rectangle OpaqueBounds { get; }
            internal List<Vector2> OpaqueSamples { get; }

            private SpriteAlphaMask(
                bool readable,
                byte[] alpha,
                int width,
                Rectangle opaqueBounds,
                List<Vector2> opaqueSamples)
            {
                Readable = readable;
                Alpha = alpha;
                Width = width;
                OpaqueBounds = opaqueBounds;
                OpaqueSamples = opaqueSamples;
            }

            internal static SpriteAlphaMask Create(Texture2D texture, Rectangle sourceRectangle)
            {
                int pixelCount = sourceRectangle.Width * sourceRectangle.Height;
                try
                {
                    Color[] pixels = new Color[pixelCount];
                    texture.GetData(0, sourceRectangle, pixels, 0, pixelCount);
                    byte[] alpha = new byte[pixelCount];
                    int minimumX = sourceRectangle.Width;
                    int minimumY = sourceRectangle.Height;
                    int maximumX = -1;
                    int maximumY = -1;

                    for (int y = 0; y < sourceRectangle.Height; y++)
                    {
                        for (int x = 0; x < sourceRectangle.Width; x++)
                        {
                            int index = x + y * sourceRectangle.Width;
                            byte pixelAlpha = pixels[index].A;
                            alpha[index] = pixelAlpha;
                            if (pixelAlpha >= OpaqueAlphaThreshold)
                            {
                                minimumX = Math.Min(minimumX, x);
                                minimumY = Math.Min(minimumY, y);
                                maximumX = Math.Max(maximumX, x);
                                maximumY = Math.Max(maximumY, y);
                            }
                        }
                    }

                    Rectangle opaqueBounds = maximumX >= minimumX && maximumY >= minimumY
                        ? new Rectangle(
                            minimumX,
                            minimumY,
                            maximumX - minimumX + 1,
                            maximumY - minimumY + 1)
                        : Rectangle.Empty;
                    List<Vector2> samples = BuildOpaqueSamples(
                        alpha,
                        sourceRectangle.Width,
                        sourceRectangle.Height);

                    return new SpriteAlphaMask(true, alpha, sourceRectangle.Width, opaqueBounds, samples);
                }
                catch
                {
                    // Some dynamically generated textures cannot be read back. Do
                    // not guess a body position for them because that could place a
                    // flame over a transparent part of the sprite.
                    return new SpriteAlphaMask(false, Array.Empty<byte>(), 0, Rectangle.Empty, new List<Vector2>());
                }
            }

            internal bool IsOpaque(int x, int y)
            {
                int index = x + y * Width;
                return Readable
                    && x >= 0
                    && y >= 0
                    && Width > 0
                    && index >= 0
                    && index < Alpha.Length
                    && Alpha[index] >= OpaqueAlphaThreshold;
            }

            private static List<Vector2> BuildOpaqueSamples(byte[] alpha, int width, int height)
            {
                int sampleStep = Math.Max(1, Math.Min(width, height) / 24);
                List<Vector2> samples = new List<Vector2>();
                for (int y = 0; y < height; y += sampleStep)
                {
                    for (int x = 0; x < width; x += sampleStep)
                    {
                        if (alpha[x + y * width] >= OpaqueAlphaThreshold)
                        {
                            samples.Add(new Vector2(x + 0.5f, y + 0.5f));
                        }
                    }
                }

                // A very thin sprite can fall between the sampling lines. Preserve
                // the opaque-pixel guarantee by finding exact pixels in that case.
                if (samples.Count == 0)
                {
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            if (alpha[x + y * width] >= OpaqueAlphaThreshold)
                            {
                                samples.Add(new Vector2(x + 0.5f, y + 0.5f));
                            }
                        }
                    }
                }

                return samples;
            }
        }
    }

    public sealed class BurningBodyFlameVisualSystem : ModSystem
    {
        public override void Unload()
        {
            BurningBodyFlameVisuals.ClearCache();
        }
    }

    public sealed class BurningBodyFlameMaskStartLayer : PlayerDrawLayer
    {
        public override Position GetDefaultPosition()
        {
            return new BeforeParent(PlayerDrawLayers.Skin);
        }

        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        {
            return BurningBodyFlamePlayerLayer.ShouldDrawBodyVisuals(drawInfo);
        }

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            BurningBodyFlameVisuals.RecordPlayerBodyDrawStart(ref drawInfo);
        }
    }

    public sealed class BurningBodyFlamePlayerLayer : PlayerDrawLayer
    {
        public override Position GetDefaultPosition()
        {
            // Skin, legs, torso, armor and head are now available for alpha
            // testing. Later accessories and the held item cannot become roots.
            return new AfterParent(PlayerDrawLayers.Head);
        }

        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        {
            return ShouldDrawBodyVisuals(drawInfo);
        }

        internal static bool ShouldDrawFlames(PlayerDrawSet drawInfo)
        {
            Player player = drawInfo.drawPlayer;
            return IsDrawablePlayer(drawInfo)
                && IsBurning2Active(player);
        }

        internal static bool ShouldDrawIceGores(PlayerDrawSet drawInfo)
        {
            Player player = drawInfo.drawPlayer;
            FairyPlayer fairyPlayer = player.GetModPlayer<FairyPlayer>();
            return IsDrawablePlayer(drawInfo)
                && !IsBurning2Active(player)
                && (fairyPlayer.Frostburn2
                    || player.HasBuff(ModContent.BuffType<Frozen2>())
                    || player.HasBuff(BuffID.Chilled)
                    || player.frozen);
        }

        internal static bool ShouldDrawBodyVisuals(PlayerDrawSet drawInfo)
        {
            return ShouldDrawFlames(drawInfo) || ShouldDrawIceGores(drawInfo);
        }

        private static bool IsDrawablePlayer(PlayerDrawSet drawInfo)
        {
            return drawInfo.shadow <= 0f
                && drawInfo.drawPlayer.active
                && !drawInfo.drawPlayer.dead;
        }

        private static bool IsBurning2Active(Player player)
        {
            return player.GetModPlayer<FairyPlayer>().Burning2
                || player.HasBuff(ModContent.BuffType<Burning2>());
        }

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            if (ShouldDrawFlames(drawInfo))
            {
                BurningBodyFlameVisuals.AddPlayerFlames(ref drawInfo);
            }
            else if (ShouldDrawIceGores(drawInfo))
            {
                BurningBodyFlameVisuals.AddPlayerIceGores(ref drawInfo);
            }
        }
    }
}
