using Microsoft.Xna.Framework;
using System.Linq;
using System.Reflection;
using System.Text;
using Terraria;
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.GameContent.Events;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ID;
using System;
using System.IO;
using Terraria.ModLoader;
using Terraria.ObjectData;
using SariaMod.Items;
using SariaMod.Buffs;
using SariaMod.Items.Strange;
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
using Terraria.Localization;
using Terraria.Audio;
using Terraria.UI;
using SariaMod;
using Terraria.GameContent.UI.Elements;
using ReLogic.Content;
using Terraria.DataStructures;

namespace SariaMod.Items.Strange
{
    public static class SariaExtensions1
    {
        public static float alpha1;
        public static bool alpha1Counter;
        public static float alpha2;
        public static bool alpha2Counter;
        public static float alpha3;
        public static bool alpha3Counter;

        /// <summary>
        /// Macro-level cycle timer for the Mask2 electric effect.
        /// Ticked once per frame in UpdateAlphaCounters.
        /// </summary>
        public static int electricCycleTimer;
        /// <summary>
        /// Computed each frame from electricCycleTimer.
        /// 0 = invisible, 1 = fully active. Controls both opacity and jitter magnitude.
        /// </summary>
        public static float electricIntensity;

        // Cycle phase durations (frames at 60fps)
        internal const int ElectricActiveFrames = 90;    // 1.5 sec — full jitter
        internal const int ElectricFadeOutFrames = 60;   // 1 sec — jitter slows + fades
        internal const int ElectricOffFrames = 150;      // 2.5 sec — invisible
        internal const int ElectricFadeInFrames = 30;    // 0.5 sec — ramps back in
        internal static readonly int ElectricCycleTotal =
            ElectricActiveFrames + ElectricFadeOutFrames + ElectricOffFrames + ElectricFadeInFrames;


        // --- Bubble Face State ---
        /// <summary>Which floating emotion bubble is currently shown above Saria's head.</summary>
        public enum BubbleFaceType { None, Notice, Competitive, Smile, Anger, Sad, Flash, Cursed }

        // Per-projectile (by whoAmI): active face + remaining display ticks + per-instance draw tick.
        // drawTick increments every time SariaBubbleFaceLoader runs; used by faces that need
        // play-once frame progression (Flash) rather than the looping GameUpdateCount animation.
        private static readonly Dictionary<int, (BubbleFaceType face, int timer, int drawTick)> _bubbleFaceState = new();

        /// <summary>
        /// Shows a bubble face above Saria for <paramref name="duration"/> ticks.
        /// Replaces any currently active face. Pass <see cref="BubbleFaceType.None"/> to clear early.
        /// </summary>
        public static void ShowBubbleFace(this Projectile projectile, BubbleFaceType face, int duration)
        {
            if (face == BubbleFaceType.None || duration <= 0)
            {
                _bubbleFaceState.Remove(projectile.whoAmI);
                return;
            }
            // Preserve existing drawTick when refreshing the same face (e.g. bridge loop),
            // so the animation doesn't reset every frame.
            int existingTick = 0;
            if (_bubbleFaceState.TryGetValue(projectile.whoAmI, out var existing) && existing.face == face)
                existingTick = existing.drawTick;
            _bubbleFaceState[projectile.whoAmI] = (face, duration, existingTick);
        }

        private static void TickBubbleFace(int whoAmI)
        {
            if (!_bubbleFaceState.TryGetValue(whoAmI, out var state)) return;
            int next = state.timer - 1;
            if (next <= 0)
                _bubbleFaceState.Remove(whoAmI);
            else
                _bubbleFaceState[whoAmI] = (state.face, next, state.drawTick + 1);
        }

        private static bool TryGetBubbleFaceState(int whoAmI, out BubbleFaceType face, out int drawTick)
        {
            if (_bubbleFaceState.TryGetValue(whoAmI, out var state))
            {
                face = state.face;
                drawTick = state.drawTick;
                return true;
            }
            face = BubbleFaceType.None;
            drawTick = 0;
            return false;
        }

        /// <summary>
        /// Triggers the Flash bubble face visual: shows the play-once animation
        /// and plays the impact sounds. Owner-only. FlashBarrier is spawned by
        /// the caller (Saria.cs) so it can be positioned correctly and tracked
        /// via FlashCooldownTimer.
        /// </summary>
        public static void TriggerFlash(this Projectile saria)
        {
            if (Main.myPlayer != saria.owner) return;

            // Set visual state — 1000 ticks matches original timeLeft (200 * 5)
            saria.ShowBubbleFace(BubbleFaceType.Flash, 1000);

            SoundEngine.PlaySound(SoundID.Item74, saria.Center);
            SoundEngine.PlaySound(SoundID.DD2_DarkMageHealImpact, saria.Center);
        }


        public enum InterfaceType
        {
            XPBar,
            NextBoss
        }

        public static void SendPacket(this Player player, ModPacket packet, bool server)
        {
            // Client: Send the packet only to the host.
            if (!server)
                packet.Send();
            // Server: Send the packet to every OTHER client.
            else
                packet.Send(-1, player.whoAmI);
        }
        internal static void SetUpCandle(ModTile mt, bool lavaImmune = false, int offset = -4)
        {
            Main.tileLighted[mt.Type] = true;
            Main.tileFrameImportant[mt.Type] = true;
            Main.tileLavaDeath[mt.Type] = !lavaImmune;
            Main.tileWaterDeath[mt.Type] = false;
            TileObjectData.newTile.CopyFrom(TileObjectData.StyleOnTable1x1);
            TileObjectData.newTile.CoordinateHeights = new int[1] { 20 };
            TileObjectData.newTile.LavaDeath = !lavaImmune;
            TileObjectData.newTile.DrawYOffset = offset;
            TileObjectData.addTile(mt.Type);
            mt.AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTorch);
        }
        private static Asset<Texture2D> GetXPBarTexture(FairyPlayer modPlayer)
        {
            return modPlayer.XPBarLevel switch
            {
                1 => ModContent.Request<Texture2D>("SariaMod/Items/Strange/SariaXpBar/SariaXPBar2"),
                2 => ModContent.Request<Texture2D>("SariaMod/Items/Strange/SariaXpBar/SariaXPBar3"),
                3 => ModContent.Request<Texture2D>("SariaMod/Items/Strange/SariaXpBar/SariaXPBar4"),
                4 => ModContent.Request<Texture2D>("SariaMod/Items/Strange/SariaXpBar/SariaXPBar5"),
                5 => ModContent.Request<Texture2D>("SariaMod/Items/Strange/SariaXpBar/SariaXPBar6"),
                6 => ModContent.Request<Texture2D>("SariaMod/Items/Strange/SariaXpBar/SariaXPBar7"),
                7 => ModContent.Request<Texture2D>("SariaMod/Items/Strange/SariaXpBar/SariaXPBar8"),
                8 => ModContent.Request<Texture2D>("SariaMod/Items/Strange/SariaXpBar/SariaXPBar9"),
                _ => ModContent.Request<Texture2D>("SariaMod/Items/Strange/SariaXpBar/SariaXPBar1")
            };
        }

        private static Asset<Texture2D> GetNextBossTexture(FairyPlayer modPlayer)
        {
            return modPlayer.Sarialevel switch
            {
                1 => ModContent.Request<Texture2D>("SariaMod/Items/Bands/QueenBee"),
                2 => ModContent.Request<Texture2D>("SariaMod/Items/Bands/WallOfFlesh"),
                3 => ModContent.Request<Texture2D>("SariaMod/Items/Bands/Retinazer"),
                4 => ModContent.Request<Texture2D>("SariaMod/Items/Bands/Plantera"),
                5 => ModContent.Request<Texture2D>("SariaMod/Items/Bands/TheDuke"),
                6 => ModContent.Request<Texture2D>("SariaMod/Items/zTalking/Blank"),
                _ => ModContent.Request<Texture2D>("SariaMod/Items/Bands/KingSlime")
            };
        }
        public static void SariaDrawInterface(this Projectile projectile, Color lightColor, InterfaceType type)
        {
            Player player = Main.player[projectile.owner];
            FairyPlayer modPlayer = player.Fairy();

            if (Main.myPlayer != projectile.owner)
            {
                return;
            }

            // Fix: Access the .Value property to get the Texture2D from the Asset<Texture2D>
            Texture2D texture = type switch
            {
                InterfaceType.XPBar => GetXPBarTexture(modPlayer).Value,
                InterfaceType.NextBoss => GetNextBossTexture(modPlayer).Value,
                _ => ModContent.Request<Texture2D>("SariaMod/Items/zTalking/Blank").Value // Default/fallback
            };

            Vector2 startPos = projectile.Center - Main.screenPosition + new Vector2(0f, projectile.gfxOffY);
            Vector2 offset = type switch
            {
                InterfaceType.XPBar => new Vector2(0f, 60f),
                InterfaceType.NextBoss => new Vector2(43f, 60f),
                _ => Vector2.Zero
            };

            Color drawColor = Color.Lerp(lightColor, Color.LightPink, 20f);
            drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);

            Rectangle rectangle = new Rectangle(0, 0, texture.Width, texture.Height);
            Vector2 origin = rectangle.Size() / 2f;

            Main.spriteBatch.Draw(texture, startPos + offset, null, projectile.GetAlpha(drawColor), projectile.rotation, origin, projectile.scale, SpriteEffects.None, 0f);
        }
        public static void SariaChargingAnimation(this Projectile projectile, int Transform, bool Sleep, int Eating, int isCharging, bool Cursed, int ChannelState, int Mood, Color lightColor)
        {
            Player player = Main.player[projectile.owner];
            bool isRight = (projectile.spriteDirection == 1);

            if (!Sleep && Eating <= 0 && ChannelState > 0 && (projectile.frame < 20 || isCharging >= 1))
            {
                int formNumber = Transform + 1;
                string dirSuffix = isRight ? "Right" : "Left";
                string baseTexturePath = $"SariaMod/Items/Strange/{formNumber}SariaAnimations/{formNumber}SariaCharging{dirSuffix}";

                // Full charging body texture (forms 4+ have these)
                if (ModContent.HasAsset(baseTexturePath))
                {
                    projectile.SariaMaindraw(ModContent.Request<Texture2D>(baseTexturePath).Value, true, false, false, 1, 1, lightColor);
                }

                projectile.SariaSmallChargeSetup(Transform, isRight, lightColor);
                projectile.SariaMaindraw(ModContent.Request<Texture2D>($"{baseTexturePath}Mask1").Value, true, false, false, 1, 1, lightColor);

                // Charging eye overlay (forms 5+ have these)
                string chargingEyesPath = $"SariaMod/Items/Strange/{formNumber}SariaAnimations/{formNumber}SariaChargingEyes";
                if (ModContent.HasAsset(chargingEyesPath))
                {
                    projectile.SariaMaindraw(ModContent.Request<Texture2D>(chargingEyesPath).Value, true, false, false, 1, 1, lightColor);
                }
            }
        }
        public static void SariaBodyDraw(this Projectile projectile, int Transform, int Eating, int isCharging, int ChannelState, int SpecialAnimate, Color lightColor, bool armsOnly = false)
        {
            Player player = Main.player[projectile.owner];

            bool IsEating = (Eating == 3 || Eating == 4) && projectile.frame <= 60;
            bool ThistoRight = (projectile.spriteDirection == 1 && !IsEating);
            bool ThistoLeft = (projectile.spriteDirection == -1 && !IsEating);
            bool isChargingActive = ChannelState > 0 && (projectile.frame < 20 || isCharging >= 1);
            bool idleArms = projectile.frame <= SariaIdleAnimator.IdleFrameMax && Eating <= 0;

            switch (Transform)
            {
                case 0:
                    if (!armsOnly)
                    {
                        if (IsEating)
                            projectile.SariaMaindraw(ModContent.Request<Texture2D>("SariaMod/Items/Strange/1SariaAnimations/1SariaEat").Value, false, false, false, 1, 1, lightColor);
                        if (ThistoRight || ThistoLeft)
                            projectile.SariaMaindraw(ModContent.Request<Texture2D>("SariaMod/Items/Strange/1SariaAnimations/1SariaBody").Value, false, true, false, 1, 1, lightColor);
                    }
                    if (armsOnly)
                    {
                        if (ThistoRight && !idleArms)
                            projectile.SariaMaindraw(ModContent.Request<Texture2D>("SariaMod/Items/Strange/1SariaAnimations/1SariaRight").Value, false, false, false, 1, 1, lightColor);
                        if (ThistoLeft && !idleArms && !isChargingActive)
                            projectile.SariaMaindraw(ModContent.Request<Texture2D>("SariaMod/Items/Strange/1SariaAnimations/1SariaLeft").Value, false, false, false, 1, 1, lightColor, startPosX: -2);
                    }
                    break;
                case 1:
                    if (!armsOnly)
                    {
                        if (IsEating)
                        {
                            bool submerged = projectile.IsMostlyInNonLavaLiquid();
                            projectile.SariaMaindraw(ModContent.Request<Texture2D>("SariaMod/Items/Strange/2SariaAnimations/2SariaEat").Value, submerged, false, false, 1, 1, lightColor);
                        }
                        if (ThistoRight || ThistoLeft)
                        {
                            bool glowBody = projectile.IsMostlyInNonLavaLiquid();
                            projectile.SariaMaindraw(ModContent.Request<Texture2D>("SariaMod/Items/Strange/2SariaAnimations/2SariaBody").Value, glowBody, true, false, 1, 1, lightColor);
                        }
                    }
                    if (armsOnly)
                    {
                        bool liquid = projectile.IsMostlyInNonLavaLiquid();
                        if (ThistoRight && !idleArms)
                            projectile.SariaMaindraw(ModContent.Request<Texture2D>("SariaMod/Items/Strange/2SariaAnimations/2SariaRight").Value, liquid, false, false, 1, 1, lightColor);
                        if (ThistoLeft && !idleArms && !isChargingActive)
                            projectile.SariaMaindraw(ModContent.Request<Texture2D>("SariaMod/Items/Strange/2SariaAnimations/2SariaLeft").Value, liquid, false, false, 1, 1, lightColor, startPosX: -2);
                    }
                    break;
                default:
                {
                    int formNumber = Transform + 1;
                    string baseDir = $"SariaMod/Items/Strange/{formNumber}SariaAnimations/{formNumber}";
                    bool form5Glow = Transform == 4;
                    string dirSuffix = ThistoRight ? "Right" : "Left";
                    string bodyPath = $"{baseDir}SariaBody";

                    if (!armsOnly)
                    {
                        // Form 3 fire hair (behind body)
                        if (Transform == 2)
                        {
                            if (IsEating)
                                projectile.SariaFireHairDraw(ModContent.Request<Texture2D>("SariaMod/Items/Strange/3SariaAnimations/3SariaFlamingHairEat").Value, false, -15, lightColor);
                            if (ThistoRight || ThistoLeft)
                                projectile.SariaFireHairDraw(ModContent.Request<Texture2D>("SariaMod/Items/Strange/3SariaAnimations/3SariaFlamingHair").Value, true, -15, lightColor);
                        }

                        // Body
                        if (IsEating)
                            projectile.SariaMaindraw(ModContent.Request<Texture2D>($"{baseDir}SariaEat").Value, form5Glow, false, false, 1, 1, lightColor);
                        if ((ThistoRight || ThistoLeft) && ModContent.HasAsset(bodyPath))
                            projectile.SariaMaindraw(ModContent.Request<Texture2D>(bodyPath).Value, form5Glow, true, false, 1, 1, lightColor);

                        // Form 4 body mask (over body, under arms)
                        if (Transform == 3 && (ThistoRight || ThistoLeft) && ModContent.HasAsset(bodyPath))
                        {
                            var bodyTex = ModContent.Request<Texture2D>(bodyPath).Value;
                            var bm1 = SariaBodyMaskKey.GetBodyMask(bodyTex);
                            if (bm1 != null) projectile.SariaMaindraw(bm1, true, true, false, 1, 1, lightColor);
                            var bm2 = SariaBodyMaskKey.GetBodyMask2(bodyTex);
                            if (bm2 != null) projectile.SariaElectricMaskDraw(bm2, true, lightColor);
                            if (Main.rand.NextBool(40))
                            {
                                var bm3 = SariaBodyMaskKey.GetBodyMask3(bodyTex);
                                if (bm3 != null) projectile.SariaMaindraw(bm3, true, true, false, 1, 1, lightColor);
                            }
                        }

                        // Form 3 scar (over body, under arms)
                        if (Transform == 2 && (ThistoRight || ThistoLeft))
                        {
                            string scarDir = projectile.spriteDirection == 1 ? "Right" : "Left";
                            int scarOffsetX = projectile.spriteDirection == 1 ? 0 : -8;
                            projectile.SariaMaindraw(ModContent.Request<Texture2D>($"SariaMod/Items/Strange/3SariaAnimations/3SariaScar{scarDir}").Value, false, true, false, 1, 1, lightColor, startPosX: scarOffsetX);
                        }

                        // Form 4 eat masks
                        if (Transform == 3 && IsEating)
                        {
                            var eatTex = ModContent.Request<Texture2D>($"{baseDir}SariaEat").Value;
                            var em1 = SariaBodyMaskKey.GetBodyMask(eatTex);
                            if (em1 != null) projectile.SariaMaindraw(em1, true, false, false, 1, 1, lightColor);
                            var em2 = SariaBodyMaskKey.GetBodyMask2(eatTex);
                            if (em2 != null) projectile.SariaElectricMaskDraw(em2, false, lightColor);
                            if (Main.rand.NextBool(40))
                            {
                                var em3 = SariaBodyMaskKey.GetBodyMask3(eatTex);
                                if (em3 != null) projectile.SariaMaindraw(em3, true, false, false, 1, 1, lightColor);
                            }
                        }

                        // Form 5 body masks + eat masks
                        if (Transform == 4)
                        {
                            if ((ThistoRight || ThistoLeft) && ModContent.HasAsset(bodyPath))
                            {
                                var bodyTex = ModContent.Request<Texture2D>(bodyPath).Value;
                                var bm1 = SariaBodyMaskKey.GetForm5Mask1(bodyTex);
                                if (bm1 != null) projectile.Saria5GlowMaskdraw(bm1, lightColor, true, false, true);
                                var bm2 = SariaBodyMaskKey.GetForm5Mask2(bodyTex);
                                if (bm2 != null) projectile.Saria5GlowMaskdraw(bm2, lightColor, false, true, true);
                            }
                            if (IsEating)
                            {
                                var eatTex = ModContent.Request<Texture2D>($"{baseDir}SariaEat").Value;
                                var em1 = SariaBodyMaskKey.GetForm5Mask1(eatTex);
                                if (em1 != null) projectile.Saria5GlowMaskdraw(em1, lightColor, true, false);
                                var em2 = SariaBodyMaskKey.GetForm5Mask2(eatTex);
                                if (em2 != null) projectile.Saria5GlowMaskdraw(em2, lightColor, false, true);
                            }
                        }
                    }

                    // Arms pass — drawn over faces when armsOnly=true
                    if (armsOnly)
                    {
                        if (ThistoRight && !idleArms)
                            projectile.SariaMaindraw(ModContent.Request<Texture2D>($"{baseDir}SariaRight").Value, form5Glow, false, false, 1, 1, lightColor);
                        if (ThistoLeft && !idleArms && !isChargingActive)
                            projectile.SariaMaindraw(ModContent.Request<Texture2D>($"{baseDir}SariaLeft").Value, form5Glow, false, false, 1, 1, lightColor, startPosX: -2);

                        // Form 4 direction arm masks (same order: Mask1 glow, Mask2 electric, Mask3 rare spark)
                        if (Transform == 3 && ((ThistoRight && !idleArms) || (ThistoLeft && !idleArms && !isChargingActive)))
                        {
                            var dirTex = ModContent.Request<Texture2D>($"{baseDir}Saria{dirSuffix}").Value;
                            var dm1 = SariaBodyMaskKey.GetBodyMask(dirTex);
                            if (dm1 != null) projectile.SariaMaindraw(dm1, true, false, false, 1, 1, lightColor);
                            var dm2 = SariaBodyMaskKey.GetBodyMask2(dirTex);
                            if (dm2 != null) projectile.SariaElectricMaskDraw(dm2, false, lightColor);
                            if (Main.rand.NextBool(40))
                            {
                                var dm3 = SariaBodyMaskKey.GetBodyMask3(dirTex);
                                if (dm3 != null) projectile.SariaMaindraw(dm3, true, false, false, 1, 1, lightColor);
                            }
                        }

                        // Form 5 direction arm masks (pulsing pink/green glow, left arm -2 X offset)
                        if (Transform == 4 && ((ThistoRight && !idleArms) || (ThistoLeft && !idleArms && !isChargingActive)))
                        {
                            int dirOffsetX = ThistoLeft ? -2 : 0;
                            var dirTex = ModContent.Request<Texture2D>($"{baseDir}Saria{dirSuffix}").Value;
                            var dm1 = SariaBodyMaskKey.GetForm5Mask1(dirTex);
                            if (dm1 != null) projectile.Saria5GlowMaskdraw(dm1, lightColor, true, false, startPosX: dirOffsetX);
                            var dm2 = SariaBodyMaskKey.GetForm5Mask2(dirTex);
                            if (dm2 != null) projectile.Saria5GlowMaskdraw(dm2, lightColor, false, true, startPosX: dirOffsetX);
                        }
                    }
                    break;
                }
            }
        }
        public static void SariaEatDraw(this Projectile projectile, int Transform, int Eating, Color lightColor, SariaIdleAnimator idleAnimator = null)
        {
            if (Eating == 3 || Eating == 4)
            {
                // Logic for eyes during eating animations
                if (Transform != 6 && Transform != 5 && Transform != 2)
                {
                    var originalEatEyes = ModContent.Request<Texture2D>("SariaMod/Items/Strange/GlobalSariaAnimations/SariaEatEyes").Value;
                    var eatEyesTex = SariaFaceColorKey.GetProcessedFace(originalEatEyes, Transform);
                    projectile.SariaMaindraw(eatEyesTex, true, false, false, 1, 1, lightColor, pointSample: true);
                    if (Transform == 4)
                    {
                        var glowTex = SariaFaceColorKey.GetForm5GlowFace(originalEatEyes);
                        projectile.SariaEyesGlowandFadedraw(glowTex, lightColor, Color.White);
                    }
                }
                if (Transform == 2)
                {
                    var originalEat3 = ModContent.Request<Texture2D>($"SariaMod/Items/Strange/{Transform + 1}SariaAnimations/{Transform + 1}SariaEatEyes").Value;
                    var eat3Tex = SariaFaceColorKey.GetProcessedFace(originalEat3, Transform);
                    projectile.SariaMaindraw(eat3Tex, true, false, false, 1, 1, lightColor, pointSample: true);
                }
                if (Transform == 5)
                {
                    projectile.SariaMaindraw(ModContent.Request<Texture2D>($"SariaMod/Items/Strange/{Transform + 1}SariaAnimations/{Transform + 1}SariaEatEyes").Value, true, false, false, 1, 1, lightColor, pointSample: true);
                }
                if (Transform == 6)
                {
                    var eatTex7 = ModContent.Request<Texture2D>($"SariaMod/Items/Strange/{Transform + 1}SariaAnimations/{Transform + 1}SariaEatEyes").Value;
                    if (idleAnimator != null)
                        SariaIdleAnimator.DrawForm7EyeRowWave(projectile, eatTex7, projectile.frame, Main.projFrames[projectile.type], idleAnimator.Form7WavePhase, idleAnimator.Form7EyeAlpha, new Vector2(0f, 1f), true, lightColor);
                    else
                        projectile.SariaMaindraw(eatTex7, true, false, false, 1, 1, lightColor, pointSample: true);
                }
            }

            // Logic for the eating aura/effect
            if (Eating == 3)
            {
                // Use string interpolation for the Eating 3 texture
                projectile.SariaMaindraw(ModContent.Request<Texture2D>($"SariaMod/Items/Strange/GlobalSariaAnimations/SariaEat3").Value, true, false, false, 1, 1, lightColor);
            }
            if (Eating == 4)
            {
                // Use string interpolation for the Eating 4 texture
                projectile.SariaMaindraw(ModContent.Request<Texture2D>($"SariaMod/Items/Strange/GlobalSariaAnimations/SariaEat2").Value, true, false, false, 1, 1, lightColor);
            }
        }
        public static bool IsLineSegmentPartiallyWalled(Vector2 start, Vector2 end, float percentageRequired)
        {
            Point startTile = (start / 16f).ToPoint();
            Point endTile = (end / 16f).ToPoint();

            int dx = Math.Abs(endTile.X - startTile.X);
            int dy = Math.Abs(endTile.Y - startTile.Y);
            int sx = (startTile.X < endTile.X) ? 1 : -1;
            int sy = (startTile.Y < endTile.Y) ? 1 : -1;
            int err = dx - dy;

            int totalTiles = 0;
            int wallTiles = 0;

            while (true)
            {
                totalTiles++;
                Tile tile = Framing.GetTileSafely(startTile.X, startTile.Y);

                // Check for a background wall
                if (tile.WallType > 0)
                {
                    wallTiles++;
                }

                if (startTile.X == endTile.X && startTile.Y == endTile.Y)
                {
                    break;
                }

                int e2 = 2 * err;
                if (e2 > -dy)
                {
                    err -= dy;
                    startTile.X += sx;
                }
                if (e2 < dx)
                {
                    err += dx;
                    startTile.Y += sy;
                }
            }
            if (totalTiles == 0) return false;
            return (float)wallTiles / totalTiles >= percentageRequired;
        }


        /// <summary>
        /// Checks if a projectile has cover, based on your specific logic.
        /// </summary>
        /// <param name="projectile">The projectile instance to check for cover.</param>
        /// <returns>True if the projectile is determined to have cover, otherwise false.</returns>
        public static bool HasCover(this Projectile projectile)
        {
            Player player = Main.player[projectile.owner];

            Vector2 upWardPosition = projectile.Center;
            upWardPosition.Y -= 550f;
            Vector2 upWardPositionCover = projectile.Center;
            float minionPositionOffCover = ((20 + projectile.minionPos / 80) * player.direction) - 15;
            upWardPositionCover.Y -= 50f;
            upWardPositionCover.X += minionPositionOffCover;

            // Check for any obstruction blocking the line of sight (this only checks solid tiles).
            bool canHitLine = Collision.CanHitLine(upWardPositionCover, projectile.width / 2, projectile.height, upWardPosition, 0, 1);

            // If a solid tile blocks the line, the minion has cover.
            if (!canHitLine)
            {
                return true;
            }

            // If the line is clear of solid tiles, perform the vertical check for walls.
            // This check goes from the max height point down to the projectile's center.
            if (IsLineSegmentPartiallyWalled(upWardPosition, projectile.Center, 0.75f))
            {
                return true;
            }

            // If neither condition for cover is met, the minion is exposed.
            return false;
        }
        public static void SariaSleepDraw(this Projectile projectile, int Transform, bool Sleeping, Color lightColor, SariaIdleAnimator idleAnimator = null)
        {
            if (Sleeping && Transform != 5 && Transform != 6)
            {
                var originalSleep = ModContent.Request<Texture2D>("SariaMod/Items/Strange/GlobalSariaAnimations/SariaSleep").Value;
                var sleepTex = SariaFaceColorKey.GetProcessedFace(originalSleep, Transform);
                projectile.SariaMaindraw(sleepTex, true, true, false, 1, 1, lightColor, pointSample: true);
                if (Transform == 4)
                {
                    var glowTex = SariaFaceColorKey.GetForm5GlowFace(originalSleep);
                    projectile.SariaEyesGlowandFadedraw(glowTex, lightColor, Color.White);
                }
            }
            if (Sleeping && Transform == 5)
            {
                projectile.SariaMaindraw(ModContent.Request<Texture2D>("SariaMod/Items/Strange/6SariaAnimations/6SariaSleep").Value, true, true, false, 1, 1, lightColor, pointSample: true);
            }
            if (Sleeping && Transform == 6)
            {
                var sleepTex7 = ModContent.Request<Texture2D>("SariaMod/Items/Strange/7SariaAnimations/7SariaSleep").Value;
                if (idleAnimator != null)
                    SariaIdleAnimator.DrawForm7EyeRowWave(projectile, sleepTex7, projectile.frame, Main.projFrames[projectile.type], idleAnimator.Form7WavePhase, idleAnimator.Form7EyeAlpha, new Vector2(0f, 1f), true, lightColor);
                else
                    projectile.SariaMaindraw(sleepTex7, true, true, false, 1, 1, lightColor, pointSample: true);
            }
        }
        public static void SariaBubbleFaceLoader(this Projectile projectile, int changeform, int eating, Color lightColor)
        {
            Player player = Main.player[projectile.owner];

            // Notice bridge — still driven by the Notice projectile
            if (player.ownedProjectileCounts[ModContent.ProjectileType<Notice>()] >= 1)
            {
                projectile.ShowBubbleFace(BubbleFaceType.Notice, 3);
            }

            // Tick the timer and read the current face — all drawing from here
            // uses only internal state and texture paths, no projectile coupling.
            TickBubbleFace(projectile.whoAmI);
            if (!TryGetBubbleFaceState(projectile.whoAmI, out BubbleFaceType activeFace, out int drawTick)) return;

            if (activeFace == BubbleFaceType.None) return;

            // Notice draws for all clients regardless of ownership or changeform
            if (activeFace == BubbleFaceType.Notice)
            {
                projectile.SariaBubbleFaces(ModContent.Request<Texture2D>("SariaMod/Items/Notice").Value, true, 1, 1, -50, lightColor);
                return;
            }

            // All other faces are owner-only and suppressed while the form-change UI is open
            if (changeform > 0 || Main.myPlayer != projectile.owner) return;

            switch (activeFace)
            {
                case BubbleFaceType.Competitive:
                    if (eating == 4)
                        projectile.SariaBubbleFaces(ModContent.Request<Texture2D>("SariaMod/Items/Competitive").Value, true, 60, 2, -50, lightColor);
                    break;
                case BubbleFaceType.Smile:
                    projectile.SariaBubbleFaces(ModContent.Request<Texture2D>("SariaMod/Items/Smile").Value, true, 60, 2, -50, lightColor);
                    break;
                case BubbleFaceType.Anger:
                    projectile.SariaBubbleFaces(ModContent.Request<Texture2D>("SariaMod/Items/Anger").Value, true, 60, 2, -50, lightColor);
                    break;
                case BubbleFaceType.Sad:
                    projectile.SariaBubbleFaces(ModContent.Request<Texture2D>("SariaMod/Items/Sad").Value, true, 60, 2, -50, lightColor);
                    break;
                case BubbleFaceType.Cursed:
                    projectile.SariaBubbleFaces(ModContent.Request<Texture2D>("SariaMod/Items/Cursed").Value, true, 60, 2, -50, lightColor);
                    break;
                case BubbleFaceType.Flash:
                {
                    // Flash loops continuously until FlashCooldownTimer expires and the state is cleared.
                    // drawTick is the per-instance counter incremented every draw tick.
                    const int flashFrameSpeed = 5;
                    const int flashFrameCount = 11;
                    int flashFrame = (drawTick / flashFrameSpeed) % flashFrameCount;
                    Texture2D flashTex = ModContent.Request<Texture2D>("SariaMod/Items/Flash").Value;
                    Vector2 flashPos = projectile.Center - Main.screenPosition + new Vector2(0f, projectile.gfxOffY);
                    flashPos.Y -= 50f;
                    int fh = flashTex.Height / flashFrameCount;
                    Rectangle flashRect = new Rectangle(0, fh * flashFrame, flashTex.Width, fh);
                    Color flashColor = Color.Lerp(lightColor, Color.WhiteSmoke, 20f);
                    SpriteEffects flashFx = projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                    Main.spriteBatch.Draw(flashTex, flashPos, flashRect, projectile.GetAlpha(flashColor), projectile.rotation, flashRect.Size() / 2f, projectile.scale, flashFx, 0f);
                    break;
                }
            }
        }
        public static void SariaFeetandArmDraw(this Projectile projectile, int Transform, int eating, Color lightColor)
        {
            bool isRight = (projectile.spriteDirection == 1);
            string dirSuffix = isRight ? "Right" : "Left";

            // Feet drawing logic
            string feetTexturePath = (eating <= 2 || eating == 5) ? "SariaMod/Items/Strange/GlobalSariaAnimations/SariaFeet" : "SariaMod/Items/Strange/GlobalSariaAnimations/SariaFeetEating";
            projectile.SariaMaindraw(ModContent.Request<Texture2D>(feetTexturePath).Value, true, true, true, 4, 25, lightColor);

            // Default stump glow drawing logic (universal across all forms)
            string armTexturePath = $"SariaMod/Items/Strange/GlobalSariaAnimations/SariaArm{dirSuffix}";
            float psychicAlpha = SariaPsychicEyes.GetOpacity(projectile.whoAmI);
            if (psychicAlpha > 0f)
                projectile.SariaMaindraw(ModContent.Request<Texture2D>(armTexturePath).Value, true, false, true, 1, 30, lightColor, alphaScale: psychicAlpha);

            // Conditional attack arm drawing logic
            switch (Transform)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                    string attackArmTexturePath = $"SariaMod/Items/Strange/{Transform + 1}SariaAnimations/{Transform + 1}SariaAttackArm{dirSuffix}";
                    projectile.SariaMaindraw(ModContent.Request<Texture2D>(attackArmTexturePath).Value, true, false, true, 1, 3, lightColor);
                    break;
            }
        }
        public static void SariaHornDraw(this Projectile projectile, int Transform, Color lightColor)
        {
            if (Transform == 6) return; // ghost form has no horn
            var originalTex = ModContent.Request<Texture2D>("SariaMod/Items/Strange/GlobalSariaAnimations/SariaHorn").Value;
            var processedTex = SariaFaceColorKey.GetProcessedFace(originalTex, Transform);
            projectile.SariaMaindraw(processedTex, true, true, false, 1, 1, lightColor, pointSample: true);
            if (Transform == 4)
            {
                var glowTex = SariaFaceColorKey.GetForm5GlowFace(originalTex);
                projectile.SariaEyesGlowandFadedraw(glowTex, lightColor, Color.White);
            }
        }
        public static void SariaSmallFacesOrWhencursed(this Projectile projectile, int Transform, bool Sleep, int Eating, int isCharging, bool Cursed, int ChannelState, int Mood, Color lightColor, SariaIdleAnimator idleAnimator = null)
        {
            Player player = Main.player[projectile.owner];
            bool isRight = (projectile.spriteDirection == 1);

            // Draw regular face and attack arms if not sleeping and not eating
            if (!Sleep && (Eating <= 2 || Eating == 5))
            {
                if (!Cursed)
                {
                    string faceTextureName;
                    string form5FaceTextureName = null;
                    string form6FaceTextureName = null;
                    string form7FaceTextureName = null;

                    // Logic to determine face based on Mood
                    if (Mood == (int)MoodState.Happy)
                    {
                        faceTextureName = "SariaHappy";
                        form5FaceTextureName = "5SariaHappy";
                        form6FaceTextureName = "6SariaHappy";
                        form7FaceTextureName = "7SariaHappy";
                    }
                    else if (Mood == (int)MoodState.Sad || player.HasBuff(ModContent.BuffType<Extinguished>()))
                    {
                        faceTextureName = "SariaSad";
                        form5FaceTextureName = "5SariaSad";
                        form6FaceTextureName = "6SariaSad";
                        form7FaceTextureName = "7SariaSad";
                    }
                    else if (Mood == (int)MoodState.Angry)
                    {
                        faceTextureName = "SariaAngry";
                        form6FaceTextureName = "6SariaAngry";
                        form7FaceTextureName = "7SariaAngry";
                    }
                    else
                    {
                        faceTextureName = "SariaNormalFace";
                        form5FaceTextureName = "5SariaNormalFace";
                        form6FaceTextureName = "6SariaNormalFace";
                        form7FaceTextureName = "7SariaNormalFace";
                    }

                    // Drawing logic for faces
                    // Forms 6/7 (Transform 5/6) have their own face sheets.
                    // Form 3 (Transform 2) uses the Global faces with SariaFaceColorKey colorizing.
                    if (Transform != 5 && Transform != 6)
                    {
                        var originalFaceTex = ModContent.Request<Texture2D>($"SariaMod/Items/Strange/GlobalSariaAnimations/{faceTextureName}").Value;
                        var faceTex = SariaFaceColorKey.GetProcessedFace(originalFaceTex, Transform);
                        projectile.SariaMaindraw(faceTex, true, true, false, 1, 1, lightColor, pointSample: true);
                        SariaPsychicEyes.DrawPsychicEyeOverlay(projectile, originalFaceTex);
                        SariaPsychicEyes.DrawBloodSneezeEyeOverlay(projectile, originalFaceTex);
                        if (Transform == 4 && form5FaceTextureName != null)
                        {
                            var glowTex = SariaFaceColorKey.GetForm5GlowFace(originalFaceTex);
                            projectile.SariaEyesGlowandFadedraw(glowTex, lightColor, Color.White);
                        }
                    }
                    if (Transform == 5)
                    {
                        if (form6FaceTextureName != null)
                        {
                            var faceTex6 = ModContent.Request<Texture2D>($"SariaMod/Items/Strange/6SariaAnimations/{form6FaceTextureName}").Value;
                            projectile.SariaMaindraw(faceTex6, true, true, false, 1, 1, lightColor, pointSample: true);
                            SariaPsychicEyes.DrawPsychicEyeOverlay(projectile, faceTex6, 5);
                            SariaPsychicEyes.DrawBloodSneezeEyeOverlay(projectile, faceTex6);
                        }
                    }
                    if (Transform == 6)
                    {
                        if (form7FaceTextureName != null)
                        {
                            var faceTex7 = ModContent.Request<Texture2D>($"SariaMod/Items/Strange/7SariaAnimations/{form7FaceTextureName}").Value;
                            if (idleAnimator != null)
                                SariaIdleAnimator.DrawForm7EyeRowWave(projectile, faceTex7, projectile.frame, Main.projFrames[projectile.type], idleAnimator.Form7WavePhase, idleAnimator.Form7EyeAlpha, new Vector2(0f, 1f), true, lightColor);
                            else
                                projectile.SariaMaindraw(faceTex7, true, true, false, 1, 1, lightColor, pointSample: true);
                            SariaPsychicEyes.DrawPsychicEyeOverlay(projectile, faceTex7, 6);
                            SariaPsychicEyes.DrawBloodSneezeEyeOverlay(projectile, faceTex7);
                        }
                    }

                    }
                    if (Cursed)
                {
                    projectile.SariaMaindraw(ModContent.Request<Texture2D>("SariaMod/Items/Strange/GlobalSariaAnimations/SariaShader").Value, true, true, false, 1, 1, lightColor);
                    projectile.SariaMaindraw(ModContent.Request<Texture2D>("SariaMod/Items/Strange/GlobalSariaAnimations/SariaCursed").Value, true, true, false, 1, 1, lightColor);
                }
            }

        }

        // Cache to avoid repeated expensive tile sampling each tick per projectile
        private static readonly Dictionary<int, (int tick, bool result)> _liquidCheckCache = new Dictionary<int, (int, bool)>();
        private static readonly Dictionary<int, (int tick, bool result)> _liquidTopHalfCache = new Dictionary<int, (int, bool)>();
        public static bool IsMostlyInNonLavaLiquid(this Projectile projectile)
        {
            // Avoid running on server; lighting is client-side only.
            if (Main.netMode == NetmodeID.Server)
                return false;

            // Quick rejects to avoid sampling when obviously not submerged.
            if (!projectile.wet || projectile.lavaWet)
            {
                // Cache false for this tick to avoid re-checks.
                int quickTick = (int)(Main.GameUpdateCount & int.MaxValue);
                _liquidCheckCache[projectile.whoAmI] = (quickTick, false);
                return false;
            }

            int currentTick = (int)(Main.GameUpdateCount & int.MaxValue);
            if (_liquidCheckCache.TryGetValue(projectile.whoAmI, out var cached) && cached.tick == currentTick)
            {
                return cached.result;
            }

            int samplesX = 4;
            int samplesY = 4;
            int hits = 0;
            int total = samplesX * samplesY;
            for (int sx = 0; sx < samplesX; sx++)
            {
                for (int sy = 0; sy < samplesY; sy++)
                {
                    float sampleX = projectile.position.X + (sx + 0.5f) * projectile.width / samplesX;
                    float sampleY = projectile.position.Y + (sy + 0.5f) * projectile.height / samplesY;
                    Point tilePoint = (new Vector2(sampleX, sampleY) / 16f).ToPoint();
                    Tile tile = Framing.GetTileSafely(tilePoint.X, tilePoint.Y);
                    // Consider tile "wet" only if it's at least half-full, and ensure it's not lava (LiquidType == 1).
                    if (tile != null && tile.LiquidAmount >= 128 && tile.LiquidType == 0)
                    {
                        hits++;
                    }
                }
            }
            bool result = hits >= total / 2;
            _liquidCheckCache[projectile.whoAmI] = (currentTick, result);

            // Clean up cache for inactive projectiles occasionally
            if (!projectile.active && _liquidCheckCache.ContainsKey(projectile.whoAmI))
                _liquidCheckCache.Remove(projectile.whoAmI);

            return result;
        }
        public static bool IsTopHalfMostlyInNonLavaLiquid(this Projectile projectile)
        {
            // Quick reject if completely lava
            if (projectile.lavaWet)
            {
                int quickTickL = (int)(Main.GameUpdateCount & int.MaxValue);
                _liquidTopHalfCache[projectile.whoAmI] = (quickTickL, false);
                return false;
            }

            int currentTick = (int)(Main.GameUpdateCount & int.MaxValue);
            if (_liquidTopHalfCache.TryGetValue(projectile.whoAmI, out var cached) && cached.tick == currentTick)
            {
                return cached.result;
            }

            // Determine sampling density from projectile size (width and height) so behavior scales with size
            int samplesX = Math.Clamp(projectile.width / 16, 1, 8);
            int samplesY = Math.Clamp((projectile.height / 2) / 16, 1, 8); // only top half

            int hits = 0;
            int total = samplesX * samplesY;
            for (int sx = 0; sx < samplesX; sx++)
            {
                for (int sy = 0; sy < samplesY; sy++)
                {
                    float sampleX = projectile.position.X + (sx + 0.5f) * projectile.width / samplesX;
                    float sampleY = projectile.position.Y + (sy + 0.5f) * (projectile.height * 0.5f) / samplesY; // top half
                    Point tilePoint = (new Vector2(sampleX, sampleY) / 16f).ToPoint();
                    Tile tile = Framing.GetTileSafely(tilePoint.X, tilePoint.Y);
                    // Consider any liquid (not only half-full) and ensure it's not lava (LiquidType == 1)
                    if (tile != null && tile.LiquidAmount > 0 && tile.LiquidType == 0)
                    {
                        hits++;
                    }
                }
            }
            bool result = hits >= total / 2;
            _liquidTopHalfCache[projectile.whoAmI] = (currentTick, result);
            if (!projectile.active && _liquidTopHalfCache.ContainsKey(projectile.whoAmI))
                _liquidTopHalfCache.Remove(projectile.whoAmI);
            return result;
        }

        /// <summary>
        /// Draws the attack arm on top of direction arms and idle arms.
        /// Call from PostDraw after SariaBodyDraw(armsOnly:true) and DrawArmsPass.
        /// Skipped during charging (charging has its own arm), eating, sleeping, and cursed.
        /// </summary>
        public static void SariaAttackArmTopDraw(this Projectile projectile, int Transform, int isCharging, int ChannelState, int Eating, bool Sleep, bool Cursed, Color lightColor)
        {
            if (Sleep || Cursed) return;
            bool isEating = (Eating == 3 || Eating == 4) && projectile.frame <= 60;
            if (isEating || Eating > 2 && Eating != 5) return;
            bool isChargingActive = ChannelState > 0 && (projectile.frame < 20 || isCharging >= 1);
            if (isChargingActive) return;

            bool isRight = projectile.spriteDirection == 1;
            string dirSuffix = isRight ? "Right" : "Left";
            string attackArmTexturePath = $"SariaMod/Items/Strange/{Transform + 1}SariaAnimations/{Transform + 1}SariaAttackArm{dirSuffix}";
            projectile.SariaMaindraw(ModContent.Request<Texture2D>(attackArmTexturePath).Value, true, false, true, 1, 3, lightColor);
        }

        public static bool TryGetOwnedZtargetRealTarget(this Projectile projectile, float maxRange, out Vector2 targetCenter)
        {
            targetCenter = default;

            int type = ModContent.ProjectileType<ZtargetReal>();
            float bestDist = maxRange;
            bool found = false;

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (!p.active)
                    continue;

                if (p.type != type)
                    continue;

                if (p.owner != projectile.owner)
                    continue;

                float dist = Vector2.Distance(projectile.Center, p.Center);
                if (dist >= bestDist)
                    continue;

                bestDist = dist;
                targetCenter = p.Center;
                found = true;
            }

            return found;
        }
    }
}