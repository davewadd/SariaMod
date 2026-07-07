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
namespace SariaMod
{
    public static class SariaModUtilities2
    {
        private const int PeriodTimerReset = 7200;
        public static float alpha4;
        public static bool alpha4Counter;
        public static float alpha5;
        public static bool alpha5Counter;
        public static float alpha6;
        public static bool alpha6Counter;
        public static void BlueRingofdust(this Projectile projectile, int howmany)
        {
            Player player = Main.player[projectile.owner];
            for (int j = 0; j < howmany; j++)
            {
                Dust dust = Dust.NewDustPerfect(projectile.Center, 113);
                dust.velocity = ((float)Math.PI * 2f * Vector2.Dot(((float)j / 72f * ((float)Math.PI * 2f)).ToRotationVector2(), player.velocity.SafeNormalize(Vector2.UnitY).RotatedBy((float)j / 72f * ((float)Math.PI * -2f)))).ToRotationVector2();
                dust.velocity = dust.velocity.RotatedBy((float)j / 36f * ((float)Math.PI * 2f)) * 8f;
                dust.noGravity = true;
                dust.scale *= 3.9f;
            }
        }
        public static void SariaSmallChargeSetup(this Projectile projectile, int Transform, bool IsRight, Color lightColor)
        {
            int startpositionx = -50;
            int startpositiony = 10;
            if (IsRight)
            {
                startpositionx = 12;
                startpositiony = 10;
            }
            int formNumber = Transform + 1;
            string sparkPath = $"SariaMod/Items/Strange/{formNumber}SariaAnimations/{formNumber}ChargingSpark";
            projectile.FrameChargeElectricitydraw((Texture2D)ModContent.Request<Texture2D>(sparkPath).Value, lightColor, true, startpositionx, startpositiony);
            projectile.SariaRandomChargeCircle(Transform, IsRight);
        }
        public static void SariaRandomChargeCircle(this Projectile projectile, int transform, bool isright)
        {
            Vector2 ToSpot = projectile.Right;
            if (!isright)
            {
                ToSpot = projectile.Left;
            }
            if (transform == 0)
            {
                if (Main.rand.NextBool(30))
                {
                    for (int i = 0; i < 50; i++)
                    {
                        Vector2 speed = Main.rand.NextVector2CircularEdge(1f, 1f);
                        Dust d = Dust.NewDustPerfect(ToSpot, ModContent.DustType<AbsorbPsychic>(), speed * -5, Scale: 1.5f);
                        d.noGravity = true;
                    }
                    SoundEngine.PlaySound(SoundID.DD2_SkyDragonsFuryShot, projectile.Center);
                    Lighting.AddLight(projectile.Center, Color.HotPink.ToVector3() * 4f);
                }
            }
            if (transform == 1)
            {
                if (Main.rand.NextBool(30))
                {
                    for (int i = 0; i < 50; i++)
                    {
                        Vector2 speed = Main.rand.NextVector2CircularEdge(1f, 1f);
                        Dust d = Dust.NewDustPerfect(ToSpot, ModContent.DustType<BubbleDust2>(), speed * -6, Scale: 2.7f);
                        d.noGravity = true;
                    }
                    SoundEngine.PlaySound(SoundID.Drown, projectile.Center);
                    Lighting.AddLight(projectile.Center, Color.White.ToVector3() * 4f);
                }
            }
            if (transform == 2)
            {
                if (Main.rand.NextBool(30))
                {
                    for (int i = 0; i < 25; i++)
                    {
                        Vector2 speed = Main.rand.NextVector2CircularEdge(1f, 1f);
                        Dust d = Dust.NewDustPerfect(ToSpot, ModContent.DustType<ShadowFlameDustCharge>(), speed * 5, Scale: 4.5f);
                        d.noGravity = true;
                    }
                    for (int i = 0; i < 25; i++)
                    {
                        Vector2 speed = Main.rand.NextVector2CircularEdge(1f, 1f);
                        Dust d = Dust.NewDustPerfect(ToSpot, ModContent.DustType<SmokeDust6>(), speed * 6, Scale: 2.5f);
                        d.noGravity = true;
                    }
                    SoundEngine.PlaySound(SoundID.Item88, projectile.Center);
                    Lighting.AddLight(projectile.Center, Color.Red.ToVector3() * 4f);
                }
            }
            if (transform == 3)
            {
                if (Main.rand.NextBool(30))
                {
                    for (int i = 0; i < 50; i++)
                    {
                        Vector2 speed = Main.rand.NextVector2CircularEdge(1f, 1f);
                        Dust d = Dust.NewDustPerfect(ToSpot, ModContent.DustType<StaticDustRing>(), speed * -6, Scale: 2.7f);
                        d.noGravity = true;
                    }
                    SoundEngine.PlaySound(SoundID.NPCHit34, projectile.Center);
                    Lighting.AddLight(projectile.Center, Color.LightYellow.ToVector3() * 4f);
                }
            }
            if (transform == 4)
            {
                if (Main.rand.NextBool(30))
                {
                    for (int i = 0; i < 50; i++)
                    {
                        Vector2 speed = Main.rand.NextVector2CircularEdge(1f, 1f);
                        Dust d = Dust.NewDustPerfect(ToSpot, ModContent.DustType<RockDustRing>(), speed * -6, Scale: 2.7f);
                        d.noGravity = true;
                    }
                    SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact, projectile.Center);
                    Lighting.AddLight(projectile.Center, Color.Green.ToVector3() * 6f);
                }
            }
            if (transform == 5)
            {
                if (Main.rand.NextBool(30))
                {
                    SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact, projectile.Center);
                    Lighting.AddLight(projectile.Center, Color.Orange.ToVector3() * 6f);
                }
            }
            if (transform == 6)
            {
                if (Main.rand.NextBool(30))
                {
                    SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact, projectile.Center);
                    Lighting.AddLight(projectile.Center, Color.GhostWhite.ToVector3() * 6f);
                }
            }
        }
        public static bool NewIdlePosition(this Projectile projectile, int howclose)
        {
            Player player = Main.player[projectile.owner];
            Vector2 PointtoCheck = player.Center;
            PointtoCheck.Y -= 10;
            PointtoCheck.X += howclose * player.direction;
            bool NoDetectWall = Collision.CanHitLine(player.position, -5, 1, PointtoCheck, 1, 1);
            if (NoDetectWall)
            {
                return true;
            }
            else return false;
        }

        // Scans the tile grid outward from the player in their facing direction and returns
        // the furthest pixel distance Saria can safely stand without clipping into a solid tile.
        // Requires 2+ consecutive solid, non-slope, non-halfblock tiles vertically (excludes
        // stairs and ramps). Runs every tick regardless of movement state.
        // Result is always in [0, logicalClose] — it never exceeds the logical idle distance.
        public static float CalcWallSafeClose(this Projectile projectile, float logicalClose)
        {
            Player player = Main.player[projectile.owner];
            const float sariaMargin = 10f;

            float originX = player.Center.X;
            float idleY   = player.Center.Y - 10f;
            int   dir     = player.direction;   // +1 right, -1 left

            // Scan from the row at Saria's feet upward so the 2-tile check covers torso+head.
            int rowFeet  = (int)(idleY / 16f) + 1;
            int playerCol = (int)(originX / 16f);

            // Scan far enough to cover logicalClose plus one extra tile for safety
            int scanTiles = (int)Math.Ceiling((logicalClose + 16f) / 16f) + 1;

            float wallResult = logicalClose; // existing wall-column result
            for (int i = 1; i <= scanTiles; i++)
            {
                int col = Math.Clamp(playerCol + dir * i, 0, Main.maxTilesX - 1);

                // Only treat as a wall if 2+ consecutive solid non-slope non-halfblock tiles
                if (!HasWallColumn(col, rowFeet - 2, 2))
                    continue;

                // Pixel X of the wall face visible to the player
                float wallFaceX = dir > 0
                    ? col * 16f           // left face of tile
                    : (col + 1) * 16f;   // right face of tile

                float dist = Math.Abs(wallFaceX - originX) - sariaMargin;
                wallResult = Math.Clamp(dist, 0f, logicalClose);
                break;
            }

            // Additive footprint check: independently verify Saria's full body (2×3 tiles)
            // fits at the candidate idle column. Catches mixed geometry (half-tiles, slopes,
            // single-tile gaps) that the wall-column scan misses because it requires 2+
            // consecutive fully-solid tiles.
            // Tracks the current Y row as the scan walks forward; each step may only shift
            // the Y origin by 1 tile at a time (up or down), so stairs/steps are reachable
            // but a 2-tile vertical jump in one step is correctly treated as impassable.
            const int fpW    = SariaPathfinder.FootprintWidthConst;
            const int fpH    = SariaPathfinder.FootprintHeightConst;

            int  currentOriginY = (int)Math.Floor(idleY / 16f) - fpH / 2;
            float fitResult     = logicalClose;

            for (int i = 1; i <= scanTiles; i++)
            {
                float candidateX = originX + dir * (i * 16f);
                int   fpOriginX  = (int)Math.Floor(candidateX / 16f) - fpW / 2;

                // Try same Y, then one tile up (-1), then one tile down (+1).
                // Only allow a shift if the previous step's Y was already valid at that offset,
                // i.e. we step at most 1 tile vertically from where we currently are.
                int? nextOriginY = null;
                foreach (int dy in new[] { 0, -1, 1 })
                {
                    int tryY = currentOriginY + dy;
                    if (SariaPathfinder.FootprintFits(fpOriginX, tryY, fpW, fpH))
                    {
                        nextOriginY = tryY;
                        break;
                    }
                }

                if (nextOriginY == null)
                {
                    // No single-tile Y shift works — column is blocked.
                    float blockedFaceX = dir > 0
                        ? fpOriginX * 16f
                        : (fpOriginX + fpW) * 16f;
                    float dist = Math.Abs(blockedFaceX - originX) - sariaMargin;
                    fitResult = Math.Clamp(dist, 0f, logicalClose);
                    break;
                }

                currentOriginY = nextOriginY.Value;
            }

            // Use whichever constraint is tighter.
            return Math.Min(wallResult, fitResult);
        }

        private static bool IsSolidTile(int x, int y)
        {
            if ((uint)x >= (uint)Main.maxTilesX || (uint)y >= (uint)Main.maxTilesY)
                return true; // treat world boundary as solid
            Tile t = Main.tile[x, y];
            return t.HasTile
                && Main.tileSolid[t.TileType]
                && !Main.tileSolidTop[t.TileType]
                && t.Slope == SlopeType.Solid
                && !t.IsHalfBlock;
        }

        // Returns true if column col has at least 2 consecutive solid (non-slope, non-halfblock)
        // tiles starting at row rowStart and scanning downward.
        private static bool HasWallColumn(int col, int rowStart, int height = 2)
        {
            int count = 0;
            for (int r = rowStart; r < rowStart + height + 1 && count < height; r++)
            {
                if (IsSolidTile(col, r))
                    count++;
                else
                    count = 0;
            }
            return count >= height;
        }
        public static void EmeraldspikeGlowandFadedraw(this Projectile projectile, Texture2D texture, Color lightColor, Color WhatColor, float glowspeed, int numframes)
        {
            SpriteEffects spriteEffects = SpriteEffects.None;
            int frameHeight = texture.Height / numframes;
            Rectangle rectangle = texture.Frame(verticalFrames: numframes, frameY: 0);
            Vector2 origin = rectangle.Size() / 2f;
            float rotation = projectile.rotation;
            float scale = projectile.scale;
            Vector2 startPos = projectile.Center - Main.screenPosition;
            startPos.X += 0;
            startPos.Y += 0;
            if (projectile.type == ModContent.ProjectileType<Emeraldspike>())
            {
                Lighting.AddLight(projectile.Center, Color.Green.ToVector3() * (2f - glowspeed));
            }
            if (projectile.type == ModContent.ProjectileType<Emeraldspike2>())
            {
                Lighting.AddLight(projectile.Center, Color.Purple.ToVector3() * (2f - glowspeed));
            }
            if (projectile.type == ModContent.ProjectileType<Emeraldspike3>())
            {
                Lighting.AddLight(projectile.Center, Color.Silver.ToVector3() * (2f - glowspeed));
            }
            if (projectile.type == ModContent.ProjectileType<Emeraldspike3_2>())
            {
                Lighting.AddLight(projectile.Center, Color.Silver.ToVector3() * (3f - glowspeed));
            }
            Color drawColor = Color.Lerp(lightColor, WhatColor, 300f);
            drawColor = Color.Lerp(drawColor, Color.Transparent, glowspeed);
            if (projectile.spriteDirection == -1)
            {
                spriteEffects = SpriteEffects.FlipHorizontally;
            }
            Main.spriteBatch.Draw(texture, startPos, rectangle, (drawColor), rotation, origin, scale, spriteEffects, 0f);
        }
        public static void Emeraldspikedraw(this Projectile projectile, Texture2D texture, Color lightColor, Color WhatColor, bool doesanimate, int startposX, int startposY, int NumFrames)
        {
            SpriteEffects spriteEffects = SpriteEffects.None;
            int frameHeight = texture.Height / NumFrames;
            Rectangle rectangle = texture.Frame(verticalFrames: NumFrames, frameY: 0);
            if (doesanimate)
            {
                int frameY = frameHeight * (projectile.frame);
                rectangle = texture.Frame(verticalFrames: NumFrames, frameY: (projectile.frame));
            }
            Vector2 origin = rectangle.Size() / 2f;
            float rotation = projectile.rotation;
            float scale = projectile.scale;
            Vector2 startPos = projectile.Center - Main.screenPosition;
            startPos.X += startposX;
            startPos.Y -= startposY;
            Color drawColor = Color.Lerp(lightColor, WhatColor, 300f);
            drawColor = Color.Lerp(drawColor, Color.Transparent, .50f);
            if (projectile.spriteDirection == -1)
            {
                spriteEffects = SpriteEffects.FlipHorizontally;
            }
            Main.spriteBatch.Draw(texture, startPos, rectangle, (drawColor), rotation, origin, scale, spriteEffects, 0f);
        }
        public static void Rupeedraw(this Projectile projectile, Texture2D texture, Color lightColor, Color WhatColor, int NumFrames)
        {
            SpriteEffects spriteEffects = SpriteEffects.None;
            int frameHeight = texture.Height / NumFrames;
            int frameY = frameHeight * (projectile.frame);
            Rectangle rectangle = texture.Frame(verticalFrames: NumFrames, frameY: (projectile.frame));
            Vector2 origin = rectangle.Size() / 2f;
            float rotation = projectile.rotation;
            float scale = projectile.scale;
            Vector2 startPos = projectile.Center - Main.screenPosition;
            startPos.X += -5;
            startPos.Y += 0;
            Color drawColor = Color.Lerp(lightColor, WhatColor, 300f);
            drawColor = Color.Lerp(drawColor, Color.Transparent, .30f);
            Main.spriteBatch.Draw(texture, startPos, rectangle, (drawColor), rotation, origin, scale, spriteEffects, 0f);
        }
        public static void RupeeGlowandFadedraw(this Projectile projectile, Texture2D texture, Color lightColor, Color WhatColor, int numframes)
        {
            if (alpha4Counter)
            {
                alpha4 -= 0.04f;
            }
            if (alpha4 <= 0)
            {
                alpha4Counter = false;
            }
            if (!alpha4Counter)
            {
                alpha4 += 0.004f;
            }
            if (alpha4 >= 1)
            {
                alpha4Counter = true;
            }
            SpriteEffects spriteEffects = SpriteEffects.None;
            int frameHeight = texture.Height / numframes;
            int frameY = frameHeight * (projectile.frame);
            Rectangle rectangle = texture.Frame(verticalFrames: numframes, frameY: (projectile.frame));
            Vector2 origin = rectangle.Size() / 2f;
            float rotation = projectile.rotation;
            float scale = projectile.scale;
            Vector2 startPos = projectile.Center - Main.screenPosition;
            startPos.X += -5;
            startPos.Y += 0;
            if (projectile.type == ModContent.ProjectileType<BouncingShard>())
            {
                Lighting.AddLight(projectile.Center, Color.Green.ToVector3() * (2f - alpha4));
            }
            if (projectile.type == ModContent.ProjectileType<BouncingShard2>())
            {
                Lighting.AddLight(projectile.Center, Color.Purple.ToVector3() * (2f - alpha4));
            }
            if (projectile.type == ModContent.ProjectileType<BouncingShard3>())
            {
                Lighting.AddLight(projectile.Center, Color.Silver.ToVector3() * (2f - alpha4));
            }
            Color drawColor = Color.Lerp(lightColor, WhatColor, 300f);
            drawColor = Color.Lerp(drawColor, Color.Transparent, alpha4);
            Main.spriteBatch.Draw(texture, startPos, rectangle, (drawColor), rotation, origin, scale, spriteEffects, 0f);
        }
        public static bool IsTouchingWaterBarrier(this Projectile projectile)
        {
            Player player = Main.player[projectile.owner];
            int WaterVeil = ModContent.ProjectileType<WaterBarrier3>();
            int WaterVeil2 = ModContent.ProjectileType<WaterBarrier>();
            int owner = player.whoAmI;
                for (int l = 0; l < 1000; l++)
                {
                    if (Main.projectile[l].active && l != projectile.whoAmI && ((Main.projectile[l].type == WaterVeil || Main.projectile[l].type == WaterVeil2)))
                    {
                        if (Main.projectile[l].Hitbox.Intersects(projectile.Hitbox))
                        {
                            return true;
                        }
                    }
                }
            return false;
        }
        public static bool IsUnderThunderCloud(this Projectile projectile)
        {
            Player player = Main.player[projectile.owner];
            int CloudStrife = ModContent.ProjectileType<LightningCloud>();
                for (int l = 0; l < 1000; l++)
                {
                    if (Main.projectile[l].active && l != projectile.whoAmI && ((Main.projectile[l].type == CloudStrife)))
                    {
                        {
                            Vector2 UpWardPosition = projectile.Center;
                            int sneezespot = 18;
                            if (projectile.spriteDirection > 0)
                            {
                                sneezespot = 18;
                            }
                            if (projectile.spriteDirection < 0)
                            {
                                sneezespot = 3;
                            }
                            UpWardPosition.X += sneezespot;
                            Vector2 CloudPosition = Main.projectile[l].Center;
                            bool NoCover = Collision.CanHitLine(UpWardPosition, projectile.width / 4, projectile.height - 50, CloudPosition, 0, 1);
                            if ((Math.Abs(UpWardPosition.X - CloudPosition.X) <= 100) && (UpWardPosition.Y >= CloudPosition.Y) && (Math.Abs(UpWardPosition.Y - CloudPosition.Y) <= 1000) && NoCover)
                            {
                                return true;
                            }
                        }
                    }
                }
            return false;
        }
        public static void SariaBubbleFaceSpawner(this Projectile projectile, bool sleep, int canmove, bool cursed, int mood)
        {
            if (Main.myPlayer != projectile.owner) return;
            if (!(projectile.ModProjectile is Saria saria)) return;

            Player player = Main.player[projectile.owner];

            // --- Idle smile state machine ---
            // Gate: only while IdleAnimator is in free-eye roaming mode and Saria is not moving.
            Vector2 infrontofSaria = projectile.Center;
            infrontofSaria.X += (50 * projectile.spriteDirection);
            float between = Vector2.Distance(player.Center, infrontofSaria);

            // "cansee": player is within 50px of the point in front of Saria (not exactly on it),
            // facing toward her, Saria is stopped, and player is not pressing movement keys.
            bool playerNotMoving = !player.controlLeft && !player.controlRight;
            bool cansee = between < 50 && between > 0
                          && player.direction != projectile.spriteDirection
                          && canmove <= 0f
                          && playerNotMoving;

            bool eyeRoaming = saria.IdleAnimator.IsEyeFreeMode;
            bool roamJustStarted = eyeRoaming && !saria.WasEyeRoaming;
            bool roamJustEnded   = !eyeRoaming && saria.WasEyeRoaming;

            // Standing timer: counts up while player holds no movement keys, resets on movement.
            // Must reach 60 ticks (1 second) before a smile interaction can trigger.
            if (playerNotMoving)
                saria.PlayerStandingTimer = Math.Min(saria.PlayerStandingTimer + 1, 120);
            else
                saria.PlayerStandingTimer = 0;

            // On roam start: reset all smile state for a fresh interaction window.
            // If the player is already facing away when roam starts, count it immediately.
            if (roamJustStarted)
            {
                saria.SmileLockedUntilRoamReset = false;
                saria.SmileInteractionActive = false;
                saria.SmileAngerTimer = 0;
                saria.PlayerHasLookedAway = (player.direction == projectile.spriteDirection);
            }

            // On roam end: clear everything so the next roam session starts fresh.
            if (roamJustEnded)
            {
                saria.SmileLockedUntilRoamReset = false;
                saria.SmileInteractionActive = false;
                saria.SmileAngerTimer = 0;
                saria.PlayerHasLookedAway = false;
            }

            saria.WasEyeRoaming = eyeRoaming;

            // Track when the player has turned away (faces same direction as Saria)
            if (eyeRoaming && !saria.SmileInteractionActive && player.direction == projectile.spriteDirection)
                saria.PlayerHasLookedAway = true;

            // --- Trigger ---
            bool moodOk = saria.Mood == (int)MoodState.Normal || saria.Mood == (int)MoodState.Happy;
            bool smileTrigger = eyeRoaming
                && cansee
                && saria.PlayerHasLookedAway
                && !saria.SmileLockedUntilRoamReset
                && !saria.SmileInteractionActive
                && moodOk
                && saria.MoodPriority < 1
                && !sleep
                && saria.PlayerStandingTimer >= 60
                && !player.HasBuff(ModContent.BuffType<Sickness>())
                && !player.HasBuff(ModContent.BuffType<BloodmoonBuff>())
                && !player.HasBuff(ModContent.BuffType<StatLower>())
                && saria.ChannelTime < 20
                && projectile.ai[0] == 0;

            if (smileTrigger)
            {
                saria.SmileInteractionActive = true;
                saria.SmileAngerTimer = 0;
                saria.PlayerHasLookedAway = false;
                // Lock this roam session — only one interaction per roam visit.
                // Player must let free roam end and start again for a second interaction.
                saria.SmileLockedUntilRoamReset = true;
                // Only start a new happy mood if she isn't already happy from another source.
                if (saria.Mood != (int)MoodState.Happy)
                    saria.SetMoodFor(MoodState.Happy, 600, priority: 1);
                float radius = (float)Math.Sqrt(Main.rand.Next(1 * 1));
                double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                Dust.NewDust(new Vector2((projectile.Center.X + 40) + radius * (float)Math.Cos(angle), (projectile.Center.Y - 10) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<HeartDust>(), 0f, 0f, 0, default(Color), 1.5f);
                projectile.netUpdate = true;
            }

            // --- Active interaction: 1.5-second facing-away anger timer ---
            if (saria.SmileInteractionActive)
            {
                // Cancel cleanly (no anger) if Saria moves out of idle.
                bool sariaMoving = Math.Abs(projectile.velocity.X) > 0.5f || Math.Abs(projectile.velocity.Y) > 0.5f;
                if (sariaMoving)
                {
                    saria.SmileInteractionActive = false;
                    saria.SmileAngerTimer = 0;
                    projectile.netUpdate = true;
                }
                else
                {
                    // Anger ticks while player is facing AWAY from Saria's position.
                    // "Away" means the player's facing direction points in the same direction
                    // as the vector from Saria to the player (i.e., they have their back to her).
                    bool playerFacingAway = player.Center.X != projectile.Center.X
                        && player.direction == Math.Sign(player.Center.X - projectile.Center.X);
                    if (playerFacingAway)
                        saria.SmileAngerTimer++;
                    else
                        saria.SmileAngerTimer = 0;

                    // 90 ticks = 1.5 seconds
                    if (saria.SmileAngerTimer >= 90)
                    {
                        saria.SetMoodFor(MoodState.Angry, 300, priority: 2);
                        SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Error"), projectile.Center);
                        saria.SmileLockedUntilRoamReset = true;
                        saria.SmileInteractionActive = false;
                        saria.SmileAngerTimer = 0;
                        projectile.netUpdate = true;
                    }

                    // Natural expiry: happy mood timer ran out, interaction ends cleanly
                    if (saria.Mood != (int)MoodState.Happy)
                        saria.SmileInteractionActive = false;
                }
            }

            // --- Cursed event (bloodmoon / eclipse) — priority 20, immediate ---
            bool isCursedEvent = player.HasBuff(ModContent.BuffType<BloodmoonBuff>()) || player.HasBuff(ModContent.BuffType<EclipseBuff>());
            if (isCursedEvent && saria.Mood != (int)MoodState.Cursed)
            {
                saria.SetMoodFor(MoodState.Cursed, 3000, priority: 20);
                saria.PeriodTimerValue = PeriodTimerReset;
                projectile.netUpdate = true;
            }
            else if (!isCursedEvent && saria.Mood == (int)MoodState.Cursed)
            {
                saria.SetMoodFor(MoodState.Normal, 1, priority: 21);
                projectile.netUpdate = true;
            }

            // Competitive food signal active → show Competitive face while signal exists
            if (player.ownedProjectileCounts[ModContent.ProjectileType<Competitivetime>()] >= 1)
            {
                projectile.ShowBubbleFace(SariaExtensions1.BubbleFaceType.Competitive, 5);
            }

            // Eclipse — apply buff + dust (mood handled by cursed event above)
            if (player.active && Main.eclipse && !player.HasBuff(ModContent.BuffType<Soothing>()))
            {
                player.AddBuff(ModContent.BuffType<EclipseBuff>(), 20);
                projectile.SneezeDust(ModContent.DustType<Blood>(), 30, 1, -10, 3, -12);
                projectile.SneezeDust(ModContent.DustType<BlackSmoke>(), 20, 6, -10, 3, -12);
            }

            // Sickness → Sad (once per PeriodTimerValue)
            if (player.HasBuff(ModContent.BuffType<Sickness>()) && saria.PeriodTimerValue == 0 && !sleep)
            {
                saria.SetMoodFor(MoodState.Sad, 420, priority: 1);
                saria.PeriodTimerValue = PeriodTimerReset;
                projectile.netUpdate = true;
            }

            // Extinguished
            if (player.HasBuff(ModContent.BuffType<Extinguished>()) && saria.PeriodTimerValue == 0 && !sleep)
            {
                saria.SetMoodFor(MoodState.Sad, 420, priority: 1);
                saria.PeriodTimerValue = PeriodTimerReset;
                projectile.netUpdate = true;
            }

            // StatLower
            if (player.HasBuff(ModContent.BuffType<StatLower>()) && saria.PeriodTimerValue == 0 && !sleep)
            {
                saria.SetMoodFor(MoodState.Sad, 420, priority: 1);
                saria.PeriodTimerValue = PeriodTimerReset;
                projectile.netUpdate = true;
            }

            // StatRaise
            if (player.HasBuff(ModContent.BuffType<StatRaise>()) && saria.PeriodTimerValue == 0 && !sleep)
            {
                saria.SetMoodFor(MoodState.Happy, 420, priority: 1);
                saria.PeriodTimerValue = PeriodTimerReset;
                projectile.netUpdate = true;
            }

            }
        public static void SariaBiomeEffectivness(this Projectile projectile, int biometime, int transform)
        {
            if (Main.myPlayer != projectile.owner) return;
            Player player = Main.player[projectile.owner];
            Vector2 UpWardPosition = projectile.Center;
            bool NoCover = !projectile.HasCover();
            bool scarfImmune = player.GetModPlayer<FairyPlayer>().SoftStepShimmerImmune;
            if (!(projectile.ModProjectile is Saria saria)) return;
            if (biometime <= 0f)
            {
                if (transform == 0)
                {
                    if (!scarfImmune && (saria.SariaZoneCrimson || saria.SariaZoneCorrupt || saria.SariaZoneUnderworld || saria.SariaZoneGraveyard || saria.SariaZoneDungeon || saria.SariaHasReajCandle))
                    {
                        projectile.SariaStatLower();
                    }
                    if ((saria.SariaZoneSpace || saria.SariaZoneGlowingMushroom || saria.SariaZoneJungle || saria.SariaHasPeaceCandle || saria.SariaZoneHallow || saria.SariaHasCalmMindCandle || (saria.SariaZoneBeach && !Main.dayTime)) && (!player.HasBuff(ModContent.BuffType<StatLower>())))
                    {
                        projectile.SariaStatRaise();
                    }
                }
                if (transform == 1)
                {
                    if (saria.SariaZoneSnow && !saria.SariaHasCampfire && !player.HasBuff(BuffID.Warmth))
                    {
                        player.AddBuff(ModContent.BuffType<Frostburn2>(), 2);
                    }
                    if ((saria.SariaZoneRain && !saria.SariaZoneSpace && NoCover) || (Collision.WetCollision(projectile.position, projectile.width / 2, projectile.height / 2) && !Collision.LavaCollision(projectile.position, projectile.width / 2, projectile.height / 2)) || (projectile.IsUnderThunderCloud()) || projectile.IsTouchingWaterBarrier())
                    {
                        player.AddBuff(ModContent.BuffType<PassiveHealing>(), 2);
                    }
                    if (!scarfImmune && (saria.SariaZoneDesert || saria.SariaZoneJungle || saria.SariaZoneGlowingMushroom || saria.SariaZoneSnow))
                    {
                        projectile.SariaStatLower();
                    }
                    if ((saria.SariaZoneUnderworld || saria.SariaZoneRain || saria.SariaZoneBeach || saria.SariaZoneMeteor || saria.SariaHasWaterCandle) && (!player.HasBuff(ModContent.BuffType<StatLower>())))
                    {
                        projectile.SariaStatRaise();
                    }
                }
                if (transform == 2)
                {
                    if ((Collision.WetCollision(projectile.position, projectile.width / 2, projectile.height / 2)) && (!Collision.LavaCollision(projectile.position, projectile.width / 2, projectile.height / 2)))
                    {
                        player.AddBuff(ModContent.BuffType<Extinguished>(), 20);
                    }
                    if (((saria.SariaZoneRain && !saria.SariaZoneSpace && NoCover && !saria.SariaZoneSnow)) || projectile.IsUnderThunderCloud() || projectile.IsTouchingWaterBarrier())
                    {
                        player.AddBuff(ModContent.BuffType<Extinguished>(), 20);
                    }
                    if (player.ZoneUnderworldHeight && !player.HasBuff(ModContent.BuffType<Veil>()) && Vector2.Distance(player.Center, projectile.Center) <= 200f)
                    {
                        player.AddBuff(ModContent.BuffType<Burning2>(), 20);
                    }
                    if (!scarfImmune && (saria.SariaZoneBeach || (saria.SariaZoneRain && !saria.SariaZoneSnow) || saria.SariaZoneSandstorm))
                    {
                        projectile.SariaStatLower();
                    }
                    if ((saria.SariaZoneSnow || saria.SariaZoneGlowingMushroom || saria.SariaZoneUnderworld || saria.SariaZoneJungle || saria.SariaZoneDungeon || saria.SariaZoneHallow) && (!player.HasBuff(ModContent.BuffType<StatLower>())))
                    {
                        projectile.SariaStatRaise();
                    }
                }
                if (transform == 3)
                {
                    if (!scarfImmune && (saria.SariaZoneUndergroundDesert || saria.SariaZoneUnderworld || saria.SariaZoneRockLayer || saria.SariaZoneDirtLayer || saria.SariaZoneUnderground))
                    {
                        projectile.SariaStatLower();
                    }
                    if ((saria.SariaZoneBeach || saria.SariaZoneRain || saria.SariaZoneSkyHeight) && (!player.HasBuff(ModContent.BuffType<StatLower>())))
                    {
                        projectile.SariaStatRaise();
                    }
                }
                if (transform == 4)
                {
                    if (!scarfImmune && (saria.SariaZoneSkyHeight || saria.SariaZoneRain || saria.SariaZoneBeach || saria.SariaZoneSpace))
                    {
                        projectile.SariaStatLower();
                    }
                    if ((saria.SariaZoneUndergroundDesert || saria.SariaZoneUnderworld || saria.SariaZoneRockLayer || saria.SariaZoneUnderground) && !saria.SariaZoneJungle && (!player.HasBuff(ModContent.BuffType<StatLower>())))
                    {
                        projectile.SariaStatRaise();
                    }
                }
                if (transform == 5)
                {
                    if (!scarfImmune && (saria.SariaZoneUnderworld || saria.SariaZoneSnow || saria.SariaZoneSpace || saria.SariaZoneRain || saria.SariaZoneSandstorm || saria.SariaZoneMeteor || saria.SariaZoneBeach || saria.SariaHasReajCandle))
                    {
                        projectile.SariaStatLower();
                    }
                    if ((saria.SariaZoneJungle || saria.SariaZoneCorrupt || saria.SariaZoneCrimson || saria.SariaZoneGlowingMushroom || saria.SariaZoneGraveyard || saria.SariaZoneUnderground || saria.SariaZoneDirtLayer || saria.SariaZoneHallow || saria.SariaZoneDesert || saria.SariaZoneUndergroundDesert || !Main.dayTime || saria.SariaHasCalmMindCandle) && (!player.HasBuff(ModContent.BuffType<StatLower>())))
                    {
                        projectile.SariaStatRaise();
                    }
                }
                if (transform == 6)
                {
                    if ((saria.SariaZoneCorrupt || saria.SariaZoneCrimson || saria.SariaZoneGraveyard || saria.SariaZoneUnderworld || saria.SariaZoneDungeon || !Main.dayTime || (saria.SariaZoneHallow && saria.SariaZoneUnderground) || saria.SariaHasReajCandle))
                    {
                        projectile.SariaStatRaise();
                    }
                    if (!scarfImmune && (((saria.SariaZoneOverworld || saria.SariaZoneSkyHeight) && Main.dayTime) || saria.SariaHasCalmMindCandle) && (!player.HasBuff(ModContent.BuffType<StatRaise>())))
                    {
                        projectile.SariaStatLower();
                    }
                }
            }
        }
    }
}