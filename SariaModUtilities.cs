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
    public static class SariaModUtilities
    {
        public static float alpha1;
        public static bool alpha1Counter;
        public static float alpha2;
        public static bool alpha2Counter;
        public static float alpha3;
        public static bool alpha3Counter;
        public static int alpha3Phase;      // 0=rise, 1=hold, 2=flicker, 3=fade down
        public static int alpha3Timer;      // ticks remaining in current phase
        public static int alpha3FlickerCount; // flickers remaining in phase 2
        public static float alpha4;
        public static bool alpha4Counter;

        // Frame-guard so multiple callers per frame don't double-tick the alphas
        private static uint _lastAlphaUpdateFrame;

        /// <summary>
        /// Ticks alpha1/2/3 once per game frame with an asymmetric pulse:
        /// fade-in (alpha decreasing ? visible) is slightly quicker than
        /// fade-out (alpha increasing ? transparent).
        /// Safe to call from many places; only the first call per frame does work.
        /// </summary>
        public static void UpdateAlphaCounters()
        {
            if (Main.GameUpdateCount == _lastAlphaUpdateFrame) return;
            _lastAlphaUpdateFrame = Main.GameUpdateCount;

            // Alpha4 — shared pulse for Form 5 eye glow + dialogue mask3 (~12s full cycle)
            // Middle ground: dialogue gets a bit longer, Saria eyes get a bit more common
            if (alpha4Counter)
                alpha4 -= 0.008f;   // fade in (~2.1s visible?gone)
            else
                alpha4 += 0.001f;   // fade out (~16.7s gone?visible)

            if (alpha4 <= 0f) { alpha4 = 0f; alpha4Counter = false; }
            if (alpha4 >= 1f) { alpha4 = 1f; alpha4Counter = true; }

            // Electric mask cycle (Mask2)
            SariaExtensions1.electricCycleTimer = (SariaExtensions1.electricCycleTimer + 1) % SariaExtensions1.ElectricCycleTotal;
            int t = SariaExtensions1.electricCycleTimer;
            int activeEnd = SariaExtensions1.ElectricActiveFrames;
            int fadeOutEnd = activeEnd + SariaExtensions1.ElectricFadeOutFrames;
            int offEnd = fadeOutEnd + SariaExtensions1.ElectricOffFrames;
            // int fadeInEnd = offEnd + ElectricFadeInFrames == ElectricCycleTotal

            if (t < activeEnd)
            {
                // Active phase — full intensity
                SariaExtensions1.electricIntensity = 1f;
            }
            else if (t < fadeOutEnd)
            {
                // Fade-out phase — 1?0 over ElectricFadeOutFrames
                SariaExtensions1.electricIntensity = 1f - (float)(t - activeEnd) / SariaExtensions1.ElectricFadeOutFrames;
            }
            else if (t < offEnd)
            {
                // Off phase — invisible
                SariaExtensions1.electricIntensity = 0f;
            }
            else
            {
                // Fade-in phase — 0?1 over ElectricFadeInFrames
                SariaExtensions1.electricIntensity = (float)(t - offEnd) / SariaExtensions1.ElectricFadeInFrames;
            }
        }

        public static void StartSandstorm()
        {
            typeof(Sandstorm).GetMethod("StartSandstorm", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
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
        public static void StopSandstorm()
        {
            Sandstorm.Happening = false;
        }
        public static FairyPlayer Fairy(this Player player)
        {
            return player.GetModPlayer<FairyPlayer>();
        }
        public static FairyProjectile Fairy(this Projectile proj)
        {
            return proj.GetGlobalProjectile<FairyProjectile>();
        }
        public static int CountProjectiles(int Type)
        {
            return Main.projectile.Count((Projectile proj) => proj.type == Type && proj.active);
        }
        public static bool InSpace(this Player player)
        {
            float x = (float)Main.maxTilesX / 4200f;
            x *= x;
            return (float)((double)(player.position.Y / 16f - (60f + 10f * x)) / (Main.worldSurface / 6.0)) < 1f;
        }
        public static void HealingProjectile(Projectile projectile, int healing, int playerToHeal, int timeCheck = 120)
        {
            Player player = Main.LocalPlayer;
            Vector2 playerVector = player.Center - projectile.Center;
            float playerDist = playerVector.Length();
            if (player.Hitbox.Intersects(projectile.Hitbox))
            {
                {
                    player.HealEffect(healing, broadcast: false);
                    player.statLife += healing;
                    if (player.statLife > player.statLifeMax2)
                    {
                        player.statLife = player.statLifeMax2;
                    }
                    NetMessage.SendData(66, -1, -1);
                }
            }
        }
        public static void HealingProjectile2(Projectile projectile, int healing, int playerToHeal, float homingVelocity, float N, bool autoHomes = true, int timeCheck = 120)
        {
            Player player = Main.player[playerToHeal];
            float homingSpeed = homingVelocity;
            player.HealEffect(healing, broadcast: false);
            player.statLife += healing;
            if (player.statLife > player.statLifeMax2)
            {
                player.statLife = player.statLifeMax2;
            }
            NetMessage.SendData(66, -1, -1, null, playerToHeal, healing);
        }
        public static string ColorMessage(string msg, Color color)
        {
            StringBuilder stringBuilder = new StringBuilder(msg.Length + 12);
            stringBuilder.Append("[c/").Append(color.Hex4()).Append(':')
                .Append(msg)
                .Append(']');
            return stringBuilder.ToString();
        }
        public static void LightHitWire(int type, int i, int j, int tileX, int tileY)
        {
            int x = i - Main.tile[i, j].TileFrameX / 18 % tileX;
            int y = j - Main.tile[i, j].TileFrameY / 18 % tileY;
            int tileXX18 = 18 * tileX;
            for (int l = x; l < x + tileX; l++)
            {
                for (int m = y; m < y + tileY; m++)
                {
                    if (Main.tile[l, m].HasTile && Main.tile[l, m].TileType == type)
                    {
                        if (Main.tile[l, m].TileFrameX < tileXX18)
                            Main.tile[l, m].TileFrameX += (short)(tileXX18);
                        else
                            Main.tile[l, m].TileFrameX -= (short)(tileXX18);
                    }
                }
            }
            if (Wiring.running)
            {
                for (int k = 0; k < tileX; k++)
                {
                    for (int l = 0; l < tileY; l++)
                        Wiring.SkipWire(x + k, y + l);
                }
            }
        }
        public static void SummonRupeeShard(this Projectile projectile, int ProjectileType, int CrystalState)
        {
            Player player = Main.player[projectile.owner];
            Projectile.NewProjectile(projectile.GetSource_FromThis(), projectile.position.X + 100, projectile.position.Y - 60, 0, 0, ProjectileType, (int)(projectile.damage), 0f, projectile.owner, player.whoAmI, projectile.whoAmI);
        }
        public static void SariaStatRaise(this Projectile projectile)
        {
            Player player = Main.player[projectile.owner];
            Saria saria = projectile.ModProjectile as Saria;
            if (!player.HasBuff(ModContent.BuffType<StatRaise>()))
            {
                if (saria != null && saria.StatRaiseSoundCooldown == 0)
                {
                    SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/StatRaise"), projectile.Center);
                    saria.StatRaiseSoundCooldown = Saria.StatSoundCooldownMax;
                }
                for (int j = 0; j < 1; j++) //set to 2
                {
                    if (Main.myPlayer == projectile.owner) Projectile.NewProjectile(projectile.GetSource_FromThis(), projectile.position.X + 0, projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<PowerUp>(), (int)(projectile.damage), 0f, projectile.owner, player.whoAmI, projectile.whoAmI);
                }
                player.AddBuff(ModContent.BuffType<StatRaise>(), 20);
            }
            if (player.HasBuff(ModContent.BuffType<StatRaise>()))
            {
                player.AddBuff(ModContent.BuffType<StatRaise>(), 20);
            }
        }
        public static void SariaStatLower(this Projectile projectile)
        {
            Player player = Main.player[projectile.owner];
            Saria saria = projectile.ModProjectile as Saria;
            if (!player.HasBuff(ModContent.BuffType<StatLower>()))
            {
                if (saria != null && saria.StatLowerSoundCooldown == 0)
                {
                    SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/StatLower"), projectile.Center);
                    saria.StatLowerSoundCooldown = Saria.StatSoundCooldownMax;
                }
                for (int j = 0; j < 1; j++) //set to 2
                {
                    if (Main.myPlayer == projectile.owner) Projectile.NewProjectile(projectile.GetSource_FromThis(), projectile.position.X + 0, projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<PowerDown>(), (int)(projectile.damage), 0f, projectile.owner, player.whoAmI, projectile.whoAmI);
                }
                player.AddBuff(ModContent.BuffType<StatLower>(), 20);
            }
            if (player.HasBuff(ModContent.BuffType<StatLower>()))
            {
                player.AddBuff(ModContent.BuffType<StatLower>(), 20);
            }
        }
        public static void AttackCircleDust(this Projectile projectile, int dusttype, int Severity, int Speed, float Width, float lenght, float Scale)
        {
            for (int i = 0; i < Severity; i++)
            {
                Vector2 speed = Main.rand.NextVector2CircularEdge(Width, lenght);
                Dust d = Dust.NewDustPerfect(projectile.Center, dusttype, speed * 15, Scale: Scale);
                d.noGravity = true;
            }
        }
        public static void AttackDust(this Projectile projectile, int dusttype, int Severity, int Range)
        {
            if (Main.rand.NextBool(Severity))
            {
                float radius = (float)Math.Sqrt(Main.rand.Next(Range * Range));
                double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                Dust.NewDust(new Vector2(projectile.Center.X + radius * (float)Math.Cos(angle), projectile.Center.Y + radius * (float)Math.Sin(angle)), 0, 0, dusttype, 0f, 0f, 0, default(Color), 1.5f);
            }
        }
        public static void AttackDust2(this Projectile projectile)
        {
            Player player = Main.player[projectile.owner];
            if ((player.HasBuff(ModContent.BuffType<Overcharged>())) && !player.ZoneSnow)
            {
                projectile.AttackDust(ModContent.DustType<StaticDustNormalPurple>(), 1, 34);
                Lighting.AddLight(projectile.Center, Color.Purple.ToVector3());
            }
            else if (player.ZoneSnow)
            {
                projectile.AttackDust(ModContent.DustType<StaticDustNormalPink>(), 1, 34);
                Lighting.AddLight(projectile.Center, Color.Pink.ToVector3());
            }
            else if ((player.HasBuff(ModContent.BuffType<StatRaise>())))
            {
                projectile.AttackDust(ModContent.DustType<StaticDustNormalBlue>(), 1, 34);
                Lighting.AddLight(projectile.Center, Color.Blue.ToVector3());
            }
            else if (player.HasBuff(ModContent.BuffType<StatLower>()))
            {
                projectile.AttackDust(ModContent.DustType<StaticDustNormalRed>(), 1, 34);
                Lighting.AddLight(projectile.Center, Color.Red.ToVector3());
            }
            else
            {
                projectile.AttackDust(ModContent.DustType<StaticDustNormal>(), 1, 34);
                Lighting.AddLight(projectile.Center, Color.Yellow.ToVector3());
            }
        }
        public static void RockDust(this Projectile projectile, int dusttype, int Severity, int Range1, int Range2, int dustspotY, int sneezespotGreater, int sneezespotLesser)
        {
            float sneezespot = 5;
            if (Main.rand.NextBool(Severity))
            {
                float radius = (float)Math.Sqrt(Main.rand.Next(Range1 * Range2));
                double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                if (projectile.spriteDirection > 0)
                {
                    sneezespot = sneezespotGreater;
                }
                if (projectile.spriteDirection < 0)
                {
                    sneezespot = sneezespotLesser;
                }
                Dust.NewDust(new Vector2((projectile.Center.X + sneezespot) + radius * (float)Math.Cos(angle), (projectile.Center.Y + dustspotY) + radius * (float)Math.Sin(angle)), 0, 0, dusttype, 0f, 0f, 0, default(Color), 1.5f);
            }
        }

        /// <summary>
        /// Spawns dust only at positions that correspond to visible (non-transparent)
        /// pixels in the given mask texture's current frame. This keeps sparkles
        /// confined to the actual glowing body parts instead of floating in empty space.
        /// </summary>
        public static void RockDustOnVisiblePixels(this Projectile projectile, Texture2D maskTexture,
            int dustType, int severity, int totalFrames, int currentFrame,
            bool doesFlip = false, int startPosX = 0)
        {
            if (!Main.rand.NextBool(severity)) return;
            if (maskTexture == null) return;

            int frameHeight = maskTexture.Height / totalFrames;
            int frameY = frameHeight * Math.Clamp(currentFrame, 0, totalFrames - 1);

            // Read only the current frame's pixels
            Color[] pixels = new Color[maskTexture.Width * frameHeight];
            maskTexture.GetData(0, new Rectangle(0, frameY, maskTexture.Width, frameHeight), pixels, 0, pixels.Length);

            // Group visible pixels by row so we can pick a uniform random Y first,
            // then a random X within that row. This prevents the selection from
            // being biased toward wide areas (dress/legs) and spreads dust evenly
            // from head to feet.
            var rowMap = new Dictionary<int, List<int>>();
            for (int y = 0; y < frameHeight; y++)
            {
                for (int x = 0; x < maskTexture.Width; x++)
                {
                    if (pixels[y * maskTexture.Width + x].A > 0)
                    {
                        if (!rowMap.ContainsKey(y))
                            rowMap[y] = new List<int>();
                        rowMap[y].Add(x);
                    }
                }
            }

            if (rowMap.Count == 0) return;

            // Pick a random row (uniform vertical distribution), then random X in that row
            var rows = new List<int>(rowMap.Keys);
            int py = rows[Main.rand.Next(rows.Count)];
            var xList = rowMap[py];
            int px = xList[Main.rand.Next(xList.Count)];

            // Convert pixel position to world position relative to projectile center
            // Origin is center of frame
            float originX = maskTexture.Width / 2f;
            float originY = frameHeight / 2f;
            float offsetX = px - originX;
            float offsetY = py - originY;

            // Handle horizontal flip
            if (doesFlip && projectile.spriteDirection == -1)
                offsetX = -offsetX;

            offsetX += startPosX;

            float worldX = projectile.Center.X + offsetX;
            float worldY = projectile.Center.Y + offsetY + 1f; // +1 matches the startPos.Y += 1 in draw

            Dust.NewDust(new Vector2(worldX, worldY), 0, 0, dustType, 0f, 0f, 0, default(Color), 1.5f);
        }
        public static void SneezeDust(this Projectile projectile, int dusttype, int Severity, int Range, int dustspotY, int sneezespotGreater, int sneezespotLesser)
        {
            float sneezespot = 5;
            if (Main.rand.NextBool(Severity))
            {
                float radius = (float)Math.Sqrt(Main.rand.Next(Range * Range));
                double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                if (projectile.spriteDirection > 0)
                {
                    sneezespot = sneezespotGreater;
                }
                if (projectile.spriteDirection < 0)
                {
                    sneezespot = sneezespotLesser;
                }
                Dust.NewDust(new Vector2((projectile.Center.X + sneezespot) + radius * (float)Math.Cos(angle), (projectile.Center.Y + dustspotY) + radius * (float)Math.Sin(angle)), 0, 0, dusttype, 0f, 0f, 0, default(Color), 1.5f);
            }
        }
        public static void FrameChargeElectricitydraw(this Projectile projectile, Texture2D texture, Color lightColor, bool nottoscreen, int startPosX = 0, int startPosY = 0)
        {
            Vector2 startPos = new Vector2(Main.screenWidth + 0, Main.screenHeight - 0) / 2f + new Vector2(0f, 0f);
            if (nottoscreen)
            {
                startPos = projectile.Center - Main.screenPosition + new Vector2(0f, projectile.gfxOffY);
            }
            int frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<PinkCharge>()];
            int frameY = frameHeight * Main.projFrames[ModContent.ProjectileType<PinkCharge>()];
            Rectangle rectangle = texture.Frame(verticalFrames: 14, frameY: (int)Main.GameUpdateCount / 3 % 14);
            Color drawColor = Color.Lerp(lightColor, Color.WhiteSmoke, 100f);
            drawColor = Color.Lerp(drawColor, Color.LightPink, 0);
            Vector2 origin = rectangle.Size() / 2f;
            float rotation = projectile.rotation;
            float scale = projectile.scale;
            SpriteEffects spriteEffects = SpriteEffects.None;
            startPos.X += startPosX;
            startPos.Y += startPosY;
            Main.spriteBatch.Draw(texture, startPos, rectangle, projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
        }
        public static void FrameChargedraw(this Projectile projectile, Texture2D texture, Color lightColor, bool nottoscreen, bool Eightframes, int startPosX = 0, int startPosY = 0)
        {
            Vector2 startPos = new Vector2(Main.screenWidth + 0, Main.screenHeight - 0) / 2f + new Vector2(0f, 0f);
            if (nottoscreen)
            {
                startPos = projectile.Center - Main.screenPosition + new Vector2(0f, projectile.gfxOffY);
            }
            int frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<PinkCharge>()];
            int frameY = frameHeight * Main.projFrames[ModContent.ProjectileType<PinkCharge>()];
            Rectangle rectangle = texture.Frame(verticalFrames: 4, frameY: (int)Main.GameUpdateCount / 6 % 4);
            if (Eightframes)
            {
                frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<BlueCharge>()];
                frameY = frameHeight * Main.projFrames[ModContent.ProjectileType<BlueCharge>()];
                rectangle = texture.Frame(verticalFrames: 8, frameY: (int)Main.GameUpdateCount / 8 % 8);
            }
            Color drawColor = Color.Lerp(lightColor, Color.WhiteSmoke, 100f);
            drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
            Vector2 origin = rectangle.Size() / 2f;
            float rotation = projectile.rotation;
            float scale = projectile.scale;
            SpriteEffects spriteEffects = SpriteEffects.None;
            startPos.X += startPosX;
            startPos.Y += startPosY;
            Main.spriteBatch.Draw(texture, startPos, rectangle, projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
        }
       
        public static void SariaBubbleFaces(this Projectile projectile, Texture2D texture, bool shoulditflip, int FrameSpeed, int NumFrames, int startPosY, Color lightColor)
        {
            Vector2 startPos = projectile.Center - Main.screenPosition + new Vector2(0f, projectile.gfxOffY);
            int frameHeight = texture.Height / NumFrames;
            int frameY = frameHeight * NumFrames;
            Color drawColor = Color.Lerp(lightColor, Color.WhiteSmoke, 20f);
            drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
            Rectangle rectangle = texture.Frame(verticalFrames: NumFrames, frameY: (int)Main.GameUpdateCount / FrameSpeed % NumFrames);
            Vector2 origin = rectangle.Size() / 2;
            float rotation = projectile.rotation;
            float scale = projectile.scale;
            SpriteEffects spriteEffects = SpriteEffects.None;
            startPos.Y += startPosY;
            if (projectile.spriteDirection == -1)
            {
                startPos.X += 0;
                if (shoulditflip)
                {
                    spriteEffects = SpriteEffects.FlipHorizontally;
                }
            }
            if (projectile.spriteDirection == 1)
            {
                startPos.X += 0;
            }
            Main.spriteBatch.Draw(texture, startPos, rectangle, projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
        }
        // Static dictionary to track trail decay timers per projectile (by whoAmI)
        private static Dictionary<int, float> _trailDecayTimers = new Dictionary<int, float>();

        public static void SariaMaindraw(this Projectile projectile, Texture2D texture, bool Glowinthedark, bool ShoulditFlip, bool DoesitTrail, int startPosY, int HowlongisTrail, Color lightColor, int startPosX = 0, bool pointSample = false, float alphaScale = 1f)
        {
            Vector2 startPos = projectile.Center - Main.screenPosition + new Vector2(0f, projectile.gfxOffY);
            int frameHeight = texture.Height / Main.projFrames[projectile.type];
            int frameY = frameHeight * projectile.frame;
            Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
            Vector2 origin = rectangle.Size() / 2f;
            float rotation = projectile.rotation;
            float scale = projectile.scale;
            SpriteEffects spriteEffects = SpriteEffects.None;
            Color drawColor = lightColor;
            if (Glowinthedark)
            {
                drawColor = Color.Lerp(lightColor, Color.GhostWhite, 20f);
            }
            if (alphaScale < 1f)
            {
                drawColor *= alphaScale;
            }
            startPos.Y += startPosY;
            startPos.X += startPosX;
            if (ShoulditFlip)
            {
                if (projectile.spriteDirection == -1)
                {
                    spriteEffects = SpriteEffects.FlipHorizontally;
                }
            }
            if (!DoesitTrail)
            {
                if (pointSample)
                {
                    Main.spriteBatch.End();
                    Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                }
                Main.spriteBatch.Draw(texture, startPos, rectangle, projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                if (pointSample)
                {
                    Main.spriteBatch.End();
                    Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                }
            }
            if (DoesitTrail)
            {
                // Manage trail decay timer for this projectile
                if (!_trailDecayTimers.ContainsKey(projectile.whoAmI))
                {
                    _trailDecayTimers[projectile.whoAmI] = 0f;
                }

                // Update trail timer based on velocity
                if (projectile.velocity.Length() > 0.1f)
                {
                    _trailDecayTimers[projectile.whoAmI] = 30f; // Reset timer when moving
                }
                else if (_trailDecayTimers[projectile.whoAmI] > 0f)
                {
                    _trailDecayTimers[projectile.whoAmI]--; // Decay timer when stopped
                }

                float currentTimer = _trailDecayTimers[projectile.whoAmI];

                // Draw trail only if timer > 0 (moving or recently stopped)
                if (currentTimer > 0f)
                {
                    // Calculate fade factor: 1.0 when moving, gradually fades to 0
                    float trailFadeFactor = MathHelper.Clamp(currentTimer / 30f, 0f, 1f);

                    // Draw trail segments with interpolation for smooth flowing tail
                    for (int i = 1; i < HowlongisTrail; i++)
                    {
                        if (projectile.oldPos[i] == Vector2.Zero)
                            continue;

                        Vector2 currentPos = projectile.oldPos[i];
                        Vector2 previousPos = (i > 0) ? projectile.oldPos[i - 1] : projectile.Center;
                        
                        if (previousPos == Vector2.Zero)
                            previousPos = currentPos;

                        // Interpolate between positions for smooth connected trail
                        int interpolationSteps = 3;
                        for (int t = 0; t <= interpolationSteps; t++)
                        {
                            float lerpAmount = (float)t / interpolationSteps;
                            Vector2 interpolatedPos = Vector2.Lerp(previousPos, currentPos, lerpAmount);
                            
                            // Calculate completion ratio for the interpolated position
                            float completionRatio = ((float)i - 1 + lerpAmount) / (float)HowlongisTrail;

                            Vector2 trailPos = interpolatedPos + projectile.Size * 0.5f - Main.screenPosition;
                            trailPos.Y += startPosY;

                            // Cone effect: scale decreases smoothly toward the tail end
                            float trailScale = scale * MathHelper.Lerp(1f, 0.3f, completionRatio);

                            // Use original lerp with transparent for proper fading, with additional fade-out
                            Color trailColor = Color.Lerp(drawColor, Color.DeepPink, completionRatio);
                            trailColor = Color.Lerp(trailColor, Color.Transparent, completionRatio);
                            
                            // Apply additional fade when stopping
                            trailColor = Color.Lerp(Color.Transparent, trailColor, trailFadeFactor);

                            Main.spriteBatch.Draw(texture, trailPos, rectangle, projectile.GetAlpha(trailColor), rotation, origin, trailScale, spriteEffects, 0f);
                        }
                    }
                }

                // Clean up timer for inactive projectiles
                if (!projectile.active && _trailDecayTimers.ContainsKey(projectile.whoAmI))
                {
                    _trailDecayTimers.Remove(projectile.whoAmI);
                }

                // Draw main sprite on top
                Main.spriteBatch.Draw(texture, startPos, rectangle, projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
            }
        }
        /// <summary>
        /// Draws animated sparks with layered electric effects:
        /// shimmer copies for crackling discharge, additive glow for
        /// luminance, and dynamic flickering electric light emission.
        /// </summary>
        public static void SariaSparksDraw(this Projectile projectile, Texture2D texture, Color lightColor)
        {
            UpdateAlphaCounters();

            Vector2 startPos = projectile.Center - Main.screenPosition + new Vector2(0f, projectile.gfxOffY);
            Rectangle rectangle = texture.Frame(verticalFrames: 14, frameY: (int)Main.GameUpdateCount / 3 % 14);
            Vector2 origin = rectangle.Size() / 2f;
            float rotation = projectile.rotation;
            float scale = projectile.scale;
            SpriteEffects spriteEffects = SpriteEffects.None;
            if (projectile.spriteDirection == -1)
            {
                spriteEffects = SpriteEffects.FlipHorizontally;
            }
            startPos.Y += 1;

            // --- 1) Base spark draw (bright, glow-enabled) ---
            Color baseColor = Color.Lerp(lightColor, Color.GhostWhite, 20f);
            Main.spriteBatch.Draw(texture, startPos, rectangle, projectile.GetAlpha(baseColor), rotation, origin, scale, spriteEffects, 0f);

            // --- 2) Shimmer copies: electric crackling flicker ---
            // Multiple offset copies with electric tint and alpha=0 (pseudo-additive)
            // create a discharge / arcing effect. Sparks scatter in all directions.
            ulong randSeed = (Main.GameUpdateCount / 4) ^ (ulong)projectile.whoAmI;
            for (int c = 0; c < 4; c++)
            {
                float shakeX = Utils.RandomInt(ref randSeed, -12, 13) * 0.15f;
                float shakeY = Utils.RandomInt(ref randSeed, -12, 13) * 0.15f;
                Vector2 shimmerPos = startPos + new Vector2(shakeX, shakeY);
                Color shimmerColor = new Color(40, 100, 140, 0);
                Main.spriteBatch.Draw(texture, shimmerPos, rectangle, projectile.GetAlpha(shimmerColor), rotation, origin, scale, spriteEffects, 0f);
            }

            // --- 3) Additive glow pass: bright electric core ---
            // Pulsating intensity makes the sparks crackle. Color shifts cyan?blue.
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            float glowIntensity = MathHelper.Lerp(0.15f, 0.4f, 1f - alpha1);
            Color glowColor = Color.Lerp(Color.DeepSkyBlue, Color.Cyan, 1f - alpha2) * glowIntensity;
            Main.spriteBatch.Draw(texture, startPos, rectangle, projectile.GetAlpha(glowColor), rotation, origin, scale * 1.03f, spriteEffects, 0f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            // --- 4) Dynamic flickering electric light ---
            // Light oscillates in intensity and shifts cyan?blue-white.
            float lightPulse = MathHelper.Lerp(0.35f, 0.7f, 1f - alpha1);
            Vector3 sparkLight = Vector3.Lerp(Color.DeepSkyBlue.ToVector3(), Color.LightCyan.ToVector3(), 1f - alpha2) * lightPulse;
            Lighting.AddLight(projectile.Center, sparkLight);
        }
        /// <summary>
        /// Draws a body-mask overlay with an electrical effect:
        /// rapid pulsating visibility, shimmer copies for arcing
        /// discharge, and an additive glow pass for bright electric
        /// luminance. Uses the same frame/flip logic as SariaMaindraw.
        /// </summary>
        public static void SariaElectricMaskDraw(this Projectile projectile, Texture2D texture, bool ShoulditFlip, Color lightColor, int startPosY = 1)
        {
            UpdateAlphaCounters();

            float intensity = SariaExtensions1.electricIntensity;
            if (intensity <= 0f) return; // off phase — skip everything

            Vector2 startPos = projectile.Center - Main.screenPosition + new Vector2(0f, projectile.gfxOffY);
            int frameHeight = texture.Height / Main.projFrames[projectile.type];
            int frameY = frameHeight * projectile.frame;
            Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
            Vector2 origin = rectangle.Size() / 2f;
            float rotation = projectile.rotation;
            float scale = projectile.scale;
            SpriteEffects spriteEffects = SpriteEffects.None;
            if (ShoulditFlip && projectile.spriteDirection == -1)
            {
                spriteEffects = SpriteEffects.FlipHorizontally;
            }
            startPos.Y += startPosY;

            // --- 1) Pulsating base draw (rapid flicker via alpha3, scaled by cycle intensity) ---
            Color baseColor = Color.Lerp(lightColor, Color.GhostWhite, 20f);
            float electricPulse = MathHelper.Lerp(0.3f, 1f, 1f - alpha3) * intensity;
            baseColor *= electricPulse;
            Main.spriteBatch.Draw(texture, startPos, rectangle, projectile.GetAlpha(baseColor), rotation, origin, scale, spriteEffects, 0f);

            // --- 2) Shimmer copies: electrical arcing ---
            // Jitter magnitude scales with intensity so arcs slow down as it fades.
            ulong randSeed = (Main.GameUpdateCount / 3) ^ (ulong)projectile.whoAmI;
            for (int c = 0; c < 3; c++)
            {
                float shakeX = Utils.RandomInt(ref randSeed, -8, 9) * 0.12f * intensity;
                float shakeY = Utils.RandomInt(ref randSeed, -8, 9) * 0.12f * intensity;
                Vector2 shimmerPos = startPos + new Vector2(shakeX, shakeY);
                Color shimmerColor = new Color(30, 90, 130, 0) * electricPulse;
                Main.spriteBatch.Draw(texture, shimmerPos, rectangle, projectile.GetAlpha(shimmerColor), rotation, origin, scale, spriteEffects, 0f);
            }

            // --- 3) Additive glow pass: bright electric core ---
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            float glowIntensity = MathHelper.Lerp(0.1f, 0.35f, 1f - alpha3) * intensity;
            Color glowColor = Color.Lerp(Color.DeepSkyBlue, Color.Cyan, 1f - alpha2) * glowIntensity;
            Main.spriteBatch.Draw(texture, startPos, rectangle, projectile.GetAlpha(glowColor), rotation, origin, scale * 1.02f, spriteEffects, 0f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }
        public static void FlatImageDraw(this Projectile projectile, Texture2D texture, Color lightColor, int startPosX = 0, int startPosY = 0)
        {
            Vector2 startPos = new Vector2(Main.screenWidth + 0, Main.screenHeight - 0) / 2f + new Vector2(0f, 0f);
            int frameHeight = texture.Height / 1;
            int frameY = frameHeight * 1;
            Color drawColor = Color.Lerp(lightColor, Color.LightBlue, 90f);
            drawColor = Color.Lerp(drawColor, Color.GhostWhite, 90);
            drawColor = Color.Lerp(drawColor, Color.Transparent, .75f);
            Rectangle rectangle = texture.Frame(verticalFrames: 1, frameY: (int)Main.GameUpdateCount / 6 % 1);
            Vector2 origin = rectangle.Size() / 2f;
            float rotation = projectile.rotation;
            float scale = projectile.scale * 1.25f;
            startPos.X += (startPosX + 1.5f);
            startPos.Y += startPosY;
            SpriteEffects spriteEffects = SpriteEffects.None;
            Main.spriteBatch.Draw(texture, startPos, rectangle, projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
        }
        public static void VisualSetUpDraw(this Projectile projectile, Texture2D texture, Color lightColor, int startPosX = 0, int startPosY = 0)
        {
            Vector2 startPos = new Vector2(Main.screenWidth + 0, Main.screenHeight - 0) / 2f + new Vector2(0f, 0f);
            int frameHeight = texture.Height / 1;
            int frameY = frameHeight * 1;
            Color drawColor = Color.Lerp(lightColor, Color.Yellow, 80f);
            drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
            Rectangle rectangle = texture.Frame(verticalFrames: 1, frameY: (int)Main.GameUpdateCount / 6 % 1);
            Vector2 origin = rectangle.Size() / 2f;
            float rotation = projectile.rotation;
            float scale = projectile.scale;
            startPos.X += startPosX;
            startPos.Y += startPosY;
            SpriteEffects spriteEffects = SpriteEffects.None;
            Main.spriteBatch.Draw(texture, startPos, rectangle, projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
        }
         public static void SariaEyesGlowandFadedraw(this Projectile projectile, Texture2D texture, Color lightColor, Color WhatColor)
        {
            // alpha3=0 means FULL GLOW, alpha3=1 means INVISIBLE
            // Phase 0: Drop quickly to 0 (glow appears fast)
            // Phase 1: Hold at 0 (full glow stays visible)
            // Phase 2: Flicker — bump up ~20% (dim) then snap back to 0 (bright)
            // Phase 3: Rise slowly to 1 (glow fades away)
            switch (alpha3Phase)
            {
                case 0: // glow appears fast (alpha3 drops to 0)
                    alpha3 -= 0.02f;
                    if (alpha3 <= 0f)
                    {
                        alpha3 = 0f;
                        alpha3Phase = 1;
                        alpha3Timer = 360; // hold full glow ~6 seconds at 60fps
                    }
                    break;
                case 1: // hold at full glow
                    alpha3Timer--;
                    if (alpha3Timer <= 0)
                    {
                        alpha3Phase = 2;
                        alpha3Counter = false; // start dimming
                        alpha3Timer = 0;
                    }
                    break;
                case 2: // flicker — go up ~20% (dim), snap back to full, then fade
                    if (!alpha3Counter) // dimming up toward 0.2
                    {
                        alpha3 += 0.01f;
                        if (alpha3 >= 0.2f)
                        {
                            alpha3 = 0.2f;
                            alpha3Counter = true; // snap back to full
                        }
                    }
                    else // snapping back to full glow
                    {
                        alpha3 -= 0.04f;
                        if (alpha3 <= 0f)
                        {
                            alpha3 = 0f;
                            alpha3Phase = 3; // done flickering, start slow fade
                        }
                    }
                    break;
                case 3: // slow fade away (alpha3 rises to 1)
                    alpha3 += 0.001f;
                    if (alpha3 >= 1f)
                    {
                        alpha3 = 1f;
                        alpha3Phase = 0; // restart cycle
                    }
                    break;
            }
            Vector2 startPos = projectile.Center - Main.screenPosition + new Vector2(0f, projectile.gfxOffY);
            int frameHeight = texture.Height / Main.projFrames[projectile.type];
            int frameY = frameHeight * projectile.frame;
            Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
            Vector2 origin = rectangle.Size() / 2f;
            float rotation = projectile.rotation;
            float scale = projectile.scale;
            SpriteEffects spriteEffects = SpriteEffects.None;
            if (projectile.spriteDirection == -1)
            {
                spriteEffects = SpriteEffects.FlipHorizontally;
            }
            Lighting.AddLight(projectile.Center, Color.DeepPink.ToVector3() * (1f - alpha3));
            Color drawColor = Color.Lerp(lightColor, WhatColor, 300f);
            drawColor = Color.Lerp(drawColor, Color.Transparent, alpha3);
            float light = 80.15f * alpha1;
            startPos.Y += 1;
            startPos.X += 0;
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            Main.spriteBatch.Draw(texture, startPos, rectangle, (drawColor), rotation, origin, scale, spriteEffects, 0f);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }
        public static void DialogueUEyeMaskdraw(this Projectile projectile, Texture2D texture, Color lightColor, Vector2 startPos2, int NumFrames, int WhichFrame)
        {
            Player player = Main.player[projectile.owner];
            int owner = player.whoAmI;
            Vector2 startPos = startPos2;
            int frameHeight = texture.Height / NumFrames;
            int frameY = frameHeight * NumFrames;
            Rectangle rectangle = texture.Frame(verticalFrames: NumFrames, frameY: WhichFrame);
            Vector2 origin = rectangle.Size() / NumFrames;
            float rotation = projectile.rotation;
            float scale = projectile.scale;
            SpriteEffects spriteEffects = SpriteEffects.None;
            Color drawColor = lightColor;
            drawColor = Color.Lerp(drawColor, Color.AntiqueWhite, 20f);
            drawColor = Color.Lerp(drawColor, Color.Transparent, alpha3);
            Main.spriteBatch.Draw(texture, startPos, rectangle, (drawColor), rotation, origin, scale, spriteEffects, 0f);
        }
        public static void DialogueUIMask3draw(this Projectile projectile, Color lightColor, int startPosX = 0, int startPosY = 0)
        {
            Player player = Main.player[projectile.owner];
            int owner = player.whoAmI;
            Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/zTalking/Greetings2EmeraldMask3");
            Vector2 startPos = new Vector2(Main.screenWidth + 0, Main.screenHeight + 0) / 2f + new Vector2(0f, 0f);
            int frameHeight = texture.Height / Main.projFrames[projectile.type];
            int frameY = frameHeight * projectile.frame;
            Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
            Vector2 origin = rectangle.Size() / 2f;
            float rotation = projectile.rotation;
            float scale = projectile.scale;
            SpriteEffects spriteEffects = SpriteEffects.None;
            Color drawColor = lightColor;
            drawColor = Color.Lerp(drawColor, Color.FloralWhite, 30f);
            drawColor = Color.Lerp(drawColor, Color.Transparent, alpha3);
            startPos.X += startPosX;
            startPos.Y += startPosY;
            Main.spriteBatch.Draw(texture, startPos, rectangle, (drawColor), rotation, origin, scale, spriteEffects, 0f);
        }
        public static void DialogueUIMask2draw(this Projectile projectile, Color lightColor, int startPosX = 0, int startPosY = 0)
        {
            Player player = Main.player[projectile.owner];
            int owner = player.whoAmI;
            Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/zTalking/Greetings2EmeraldMask2");
            Vector2 startPos = new Vector2(Main.screenWidth + 0, Main.screenHeight + 0) / 2f + new Vector2(0f, 0f);
            int frameHeight = texture.Height / Main.projFrames[projectile.type];
            int frameY = frameHeight * projectile.frame;
            Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
            Vector2 origin = rectangle.Size() / 2f;
            float rotation = projectile.rotation;
            float scale = projectile.scale;
            SpriteEffects spriteEffects = SpriteEffects.None;
            Color drawColor = lightColor;
            drawColor = Color.Lerp(drawColor, Color.FloralWhite, 30f);
            drawColor = Color.Lerp(drawColor, Color.Transparent, alpha1);
            startPos.X += startPosX;
            startPos.Y += startPosY;
            Main.spriteBatch.Draw(texture, startPos, rectangle, (drawColor), rotation, origin, scale, spriteEffects, 0f);
        }
        public static void DialogueUIMaskdraw(this Projectile projectile, Color lightColor, int startPosX = 0, int startPosY = 0)
        {
            Player player = Main.player[projectile.owner];
            int owner = player.whoAmI;
            Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/zTalking/Greetings2EmeraldMask");
            Vector2 startPos = new Vector2(Main.screenWidth + 0, Main.screenHeight + 0) / 2f + new Vector2(0f, 0f);
            int frameHeight = texture.Height / Main.projFrames[projectile.type];
            int frameY = frameHeight * projectile.frame;
            Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
            Vector2 origin = rectangle.Size() / 2f;
            float rotation = projectile.rotation;
            float scale = projectile.scale;
            SpriteEffects spriteEffects = SpriteEffects.None;
            Color drawColor = lightColor;
            drawColor = Color.Lerp(drawColor, Color.FloralWhite, 30f);
            drawColor = Color.Lerp(drawColor, Color.Transparent, alpha2);
            startPos.X += startPosX;
            startPos.Y += startPosY;
            Main.spriteBatch.Draw(texture, startPos, rectangle, (drawColor), rotation, origin, scale, spriteEffects, 0f);
        }
        public static void DialogueUIFireMaskdraw(this Projectile projectile, Color lightColor, Texture2D texture, int i, int j, int startPosX = 0, int startPosY = 0)
        {
            Player player = Main.player[projectile.owner];
            int owner = player.whoAmI;
            Vector2 startPos = new Vector2(Main.screenWidth + 0, Main.screenHeight + 0) / 2f + new Vector2(0f, 0f);
            int frameHeight = texture.Height / Main.projFrames[projectile.type];
            int frameY = frameHeight * projectile.frame;
            Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
            Vector2 origin = rectangle.Size() / 2f;
            float rotation = projectile.rotation;
            float scale = projectile.scale;
            SpriteEffects spriteEffects = SpriteEffects.None;
            Color drawColor = lightColor;
            drawColor = Color.Lerp(drawColor, Color.AntiqueWhite, 20f);
            ulong randShakeEffect = (Main.GameUpdateCount / 8) ^ (ulong)((long)j << 20 | (long)(uint)i);
            float drawPositionX = i * 1 - (int)Main.screenPosition.X - (texture.Width - 16f) / 2f;
            float drawPositionY = j * 1 - (int)Main.screenPosition.Y;
            float shakeX = Utils.RandomInt(ref randShakeEffect, -4, -3) * 0.07f;
            float shakeY = Utils.RandomInt(ref randShakeEffect, -4, 3) * 0.07f;
            startPos.Y += (startPosY + (1 + shakeX));
            startPos.X += (startPosX + (+0 + shakeY));
            Main.spriteBatch.Draw(texture, startPos, rectangle, (drawColor), rotation, origin, scale, spriteEffects, 0f);
        }
        public static void Saria5GlowMaskdraw(this Projectile projectile, Texture2D texture, Color lightColor, bool counter1, bool counter2, bool doesFlip = false, int startPosX = 0)
        {
            if (alpha1Counter) alpha1 -= 0.001f;
            if (alpha1 <= 0f) { alpha1 = 0f; alpha1Counter = false; }
            if (!alpha1Counter) alpha1 += 0.001f;
            if (alpha1 >= 1f) { alpha1 = 1f; alpha1Counter = true; }

            if (alpha2Counter) alpha2 -= 0.002f;
            if (alpha2 <= 0f) { alpha2 = 0f; alpha2Counter = false; }
            if (!alpha2Counter) alpha2 += 0.002f;
            if (alpha2 >= 1f) { alpha2 = 1f; alpha2Counter = true; }

            Vector2 startPos = projectile.Center - Main.screenPosition + new Vector2(0f, projectile.gfxOffY);
            int frameHeight = texture.Height / Main.projFrames[projectile.type];
            int frameY = frameHeight * projectile.frame;
            Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
            Vector2 origin = rectangle.Size() / 2f;
            float rotation = projectile.rotation;
            float scale = projectile.scale;
            SpriteEffects spriteEffects = SpriteEffects.None;
            if (doesFlip && projectile.spriteDirection == -1)
            {
                spriteEffects = SpriteEffects.FlipHorizontally;
            }
            Lighting.AddLight(projectile.Center, Color.DeepPink.ToVector3() * (1f - alpha1));
            Lighting.AddLight(projectile.Center, Color.Green.ToVector3() * (1f - alpha2));
            Color drawColor = Color.Lerp(lightColor, Color.FloralWhite, 30f);
            if (counter1)
            {
                drawColor = Color.Lerp(drawColor, Color.Transparent, alpha1);
                // Use 5SariaBody for dust spawning so particles cover the full silhouette evenly
                var bodyTex = ModContent.Request<Texture2D>("SariaMod/Items/Strange/5SariaAnimations/5SariaBody").Value;
                projectile.RockDustOnVisiblePixels(bodyTex, ModContent.DustType<RockSparkle>(), 20,
                    Main.projFrames[projectile.type], projectile.frame, doesFlip, startPosX);
            }
            if (counter2)
            {
                drawColor = Color.Lerp(drawColor, Color.Transparent, alpha2);
            }
            startPos.Y += 1;
            startPos.X += startPosX;
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            Main.spriteBatch.Draw(texture, startPos, rectangle, (drawColor), rotation, origin, scale, spriteEffects, 0f);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }
        public static void Saria3GlowMaskdraw(this Projectile projectile, Texture2D texture, int i, int j, bool ShoulditFlip, Color lightColor)
        {
            Vector2 startPos = projectile.Center - Main.screenPosition + new Vector2(0f, projectile.gfxOffY);
            int frameHeight = texture.Height / Main.projFrames[projectile.type];
            int frameY = frameHeight * projectile.frame;
            Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
            Vector2 origin = rectangle.Size() / 2f;
            float rotation = projectile.rotation;
            float scale = projectile.scale;
            SpriteEffects spriteEffects = SpriteEffects.None;
            Color drawColor = Color.Lerp(lightColor, Color.LightYellow, 30f);
            if (ShoulditFlip)
            {
                if (projectile.spriteDirection == -1)
                {
                    spriteEffects = SpriteEffects.FlipHorizontally;
                }
            }
            Lighting.AddLight(projectile.Center, Color.Yellow.ToVector3() * .2f);
            ulong randShakeEffect = (Main.GameUpdateCount / 8) ^ (ulong)((long)j << 20 | (long)(uint)i);
            float drawPositionX = i * 1 - (int)Main.screenPosition.X - (projectile.width - 16f) / 2f;
            float drawPositionY = j * 1 - (int)Main.screenPosition.Y;
            float shakeX = Utils.RandomInt(ref randShakeEffect, -4, -3) * 0.07f;
            float shakeY = Utils.RandomInt(ref randShakeEffect, -4, 3) * 0.07f;
            startPos.Y += (-15 + shakeX);
            startPos.X += (+0 + shakeY);
            Main.spriteBatch.Draw(texture, startPos, rectangle, projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
        }
        /// <summary>
        /// Draws animated fire hair with layered flame effects:
        /// shimmer copies for heat haze, additive glow for luminance,
        /// and dynamic flickering light emission.
        /// </summary>
        public static void SariaFireHairDraw(this Projectile projectile, Texture2D texture, bool ShoulditFlip, int startPosY, Color lightColor)
        {
            UpdateAlphaCounters();

            Vector2 startPos = projectile.Center - Main.screenPosition + new Vector2(0f, projectile.gfxOffY);
            int frameHeight = texture.Height / Main.projFrames[projectile.type];
            int frameY = frameHeight * projectile.frame;
            Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
            Vector2 origin = rectangle.Size() / 2f;
            float rotation = projectile.rotation;
            float scale = projectile.scale;
            SpriteEffects spriteEffects = SpriteEffects.None;
            if (ShoulditFlip && projectile.spriteDirection == -1)
            {
                spriteEffects = SpriteEffects.FlipHorizontally;
            }
            startPos.Y += startPosY;

            // --- 1) Base fire draw (bright, glow-enabled) ---
            Color baseColor = Color.Lerp(lightColor, Color.GhostWhite, 20f);
            Main.spriteBatch.Draw(texture, startPos, rectangle, projectile.GetAlpha(baseColor), rotation, origin, scale, spriteEffects, 0f);

            // --- 2) Shimmer copies: Terraria-style flame flicker ---
            // Multiple offset copies with warm fire tint and alpha=0 (pseudo-additive)
            // create heat haze / shimmering edges. Bias shake upward since fire rises.
            ulong randSeed = (Main.GameUpdateCount / 6) ^ (ulong)projectile.whoAmI;
            for (int c = 0; c < 4; c++)
            {
                float shakeX = Utils.RandomInt(ref randSeed, -10, 11) * 0.12f;
                float shakeY = Utils.RandomInt(ref randSeed, -12, 5) * 0.15f;
                Vector2 shimmerPos = startPos + new Vector2(shakeX, shakeY);
                Color shimmerColor = new Color(120, 80, 30, 0);
                Main.spriteBatch.Draw(texture, shimmerPos, rectangle, projectile.GetAlpha(shimmerColor), rotation, origin, scale, spriteEffects, 0f);
            }

            // --- 3) Additive glow pass: bright luminous core ---
            // Pulsating intensity makes the fire breathe. Color shifts orange?gold.
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            float glowIntensity = MathHelper.Lerp(0.2f, 0.45f, 1f - alpha1);
            Color glowColor = Color.Lerp(Color.OrangeRed, Color.Gold, 1f - alpha2) * glowIntensity;
            Main.spriteBatch.Draw(texture, startPos, rectangle, projectile.GetAlpha(glowColor), rotation, origin, scale * 1.02f, spriteEffects, 0f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            // --- 4) Dynamic flickering fire light ---
            // Light oscillates in intensity and shifts orange?yellow.
            // Positioned above center since fire illuminates upward.
            float lightPulse = MathHelper.Lerp(0.3f, 0.55f, 1f - alpha1);
            Vector3 fireLight = Vector3.Lerp(Color.Orange.ToVector3(), Color.Yellow.ToVector3(), 1f - alpha2) * lightPulse;
            Vector2 lightPos = projectile.Center;
            lightPos.Y -= 10f;
            Lighting.AddLight(lightPos, fireLight);
        }
        public static void DrawFlameEffect(Texture2D flameTexture, int i, int j, int offsetX = 0, int offsetY = 0)
        {
            Tile tile = Main.tile[i, j];
            Vector2 zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange, Main.offScreenRange);
            int width = 16;
            int height = 16;
            int yOffset = TileObjectData.GetTileData(tile).DrawYOffset;
            ulong randShakeEffect = Main.TileFrameSeed ^ (ulong)((long)j << 32 | (long)(uint)i);
            float drawPositionX = i * 16 - (int)Main.screenPosition.X - (width - 16f) / 2f;
            float drawPositionY = j * 16 - (int)Main.screenPosition.Y;
            for (int c = 0; c < 7; c++)
            {
                float shakeX = Utils.RandomInt(ref randShakeEffect, -10, 11) * 0.15f;
                float shakeY = Utils.RandomInt(ref randShakeEffect, -10, 1) * 0.35f;
                Main.spriteBatch.Draw(flameTexture, new Vector2(drawPositionX + shakeX, drawPositionY + shakeY + yOffset) + zero, new Rectangle(tile.TileFrameX + offsetX, tile.TileFrameY + offsetY, width, height), new Color(100, 100, 100, 0), 0f, default(Vector2), 1f, SpriteEffects.None, 0f);
            }
        }
        public static void DrawFlameSparks(int dustType, int rarity, int i, int j)
        {
            if (!Main.gamePaused && Main.instance.IsActive && (!Lighting.UpdateEveryFrame || Main.rand.NextBool(4)))
            {
                if (Main.rand.NextBool(rarity))
                {
                    int dust = Dust.NewDust(new Vector2(i * 16 + 4, j * 16 + 2), 4, 4, dustType, 0f, 0f, 100, default, 1f);
                    if (Main.rand.Next(3) != 0)
                        Main.dust[dust].noGravity = true;
                    // Prevent lag.
                    Main.dust[dust].noLightEmittence = true;
                    Main.dust[dust].velocity *= 0.3f;
                    Main.dust[dust].velocity.Y = Main.dust[dust].velocity.Y - 1.5f;
                }
            }
        }
        public static void BlueRingofdust(Projectile projectile)
        {
            Player player = Main.player[projectile.owner];
            for (int j = 0; j < 72; j++)
            {
                Dust dust = Dust.NewDustPerfect(projectile.Center, 113);
                dust.velocity = ((float)Math.PI * 2f * Vector2.Dot(((float)j / 72f * ((float)Math.PI * 2f)).ToRotationVector2(), player.velocity.SafeNormalize(Vector2.UnitY).RotatedBy((float)j / 72f * ((float)Math.PI * -2f)))).ToRotationVector2();
                dust.velocity = dust.velocity.RotatedBy((float)j / 36f * ((float)Math.PI * 2f)) * 8f;
                dust.noGravity = true;
                dust.scale *= 3.9f;
            }
        }
        public static void SariaBaseDamage(this Projectile projectile)
        {
            Player player = Main.player[projectile.owner];
            FairyPlayer modPlayer = player.Fairy();
            if (modPlayer.Sarialevel == 6)
            {
                projectile.damage = 900 + (modPlayer.SariaXp / 20);
                projectile.netUpdate = true;
            }
            else if (modPlayer.Sarialevel == 5)
            {
                projectile.damage = 200 + (modPlayer.SariaXp / 342);
                projectile.netUpdate = true;
            }
            else if (modPlayer.Sarialevel == 4)
            {
                projectile.damage = 75 + (modPlayer.SariaXp / 640);
                projectile.netUpdate = true;
            }
            else if (modPlayer.Sarialevel == 3)
            {
                projectile.damage = 50 + (modPlayer.SariaXp / 1600);
                projectile.netUpdate = true;
            }
            else if (modPlayer.Sarialevel == 2)
            {
                projectile.damage = 26 + (modPlayer.SariaXp / 833);
                projectile.netUpdate = true;
            }
            else if (modPlayer.Sarialevel == 1)
            {
                projectile.damage = 15 + (modPlayer.SariaXp / 818);
                projectile.netUpdate = true;
            }
            else
            {
                projectile.damage = 10 + (modPlayer.SariaXp / 600);
                projectile.netUpdate = true;
            }
            if (player.HasBuff(ModContent.BuffType<StatRaise>()))
            {
                projectile.damage += (projectile.damage) / 4;
            }
            else if (player.HasBuff(ModContent.BuffType<StatLower>()))
            {
                projectile.damage /= 2;
            }
        }
        public static NPC MinionHoming(this Vector2 origin, float maxDistanceToCheck, Player owner, bool ignoreTiles = true)
        {
            if (owner == null || owner.whoAmI < 0 || owner.whoAmI > 255 || owner.MinionAttackTargetNPC < 0 || owner.MinionAttackTargetNPC > 200)
            {
                return origin.ClosestNPCAt(maxDistanceToCheck, ignoreTiles);
            }
            NPC npc = Main.npc[owner.MinionAttackTargetNPC];
            bool canHit = true;
            if (!ignoreTiles)
            {
                origin = owner.Center;
                canHit = Collision.CanHit(origin, 1, 1, npc.Center, 1, 1);
            }
            if (owner.HasMinionAttackTargetNPC && canHit)
            {
                return npc;
            }
            return origin.ClosestNPCAt(maxDistanceToCheck, ignoreTiles);
        }
        public static NPC ClosestNPCAt(this Vector2 origin, float maxDistanceToCheck, bool ignoreTiles = true, bool bossPriority = false)
        {
            NPC closestTarget = null;
            float distance = maxDistanceToCheck;
            if (bossPriority)
            {
                bool bossFound = false;
                for (int index2 = 0; index2 < Main.npc.Length; index2++)
                {
                    if ((bossFound && !Main.npc[index2].boss && Main.npc[index2].type != NPCID.WallofFleshEye) || !Main.npc[index2].CanBeChasedBy())
                    {
                        continue;
                    }
                    float extraDistance2 = Main.npc[index2].width / 2 + Main.npc[index2].height / 2;
                    bool canHit2 = true;
                    if (extraDistance2 < distance && !ignoreTiles)
                    {
                        canHit2 = Collision.CanHit(origin, 1, 1, Main.npc[index2].Center, 1, 1);
                    }
                    if (Vector2.Distance(origin, Main.npc[index2].Center) < distance + extraDistance2 && canHit2)
                    {
                        if (Main.npc[index2].boss || Main.npc[index2].type == NPCID.WallofFleshEye)
                        {
                            bossFound = true;
                        }
                        distance = Vector2.Distance(origin, Main.npc[index2].Center);
                        closestTarget = Main.npc[index2];
                    }
                }
            }
            else
            {
                for (int index = 0; index < Main.npc.Length; index++)
                {
                    if (Main.npc[index].CanBeChasedBy())
                    {
                        float extraDistance = Main.npc[index].width / 2 + Main.npc[index].height / 2;
                        bool canHit = true;
                        if (extraDistance < distance && !ignoreTiles)
                        {
                            canHit = Collision.CanHit(origin, 1, 1, Main.npc[index].Center, 1, 1);
                        }
                        if (Vector2.Distance(origin, Main.npc[index].Center) < distance + extraDistance && canHit)
                        {
                            distance = Vector2.Distance(origin, Main.npc[index].Center);
                            closestTarget = Main.npc[index];
                        }
                    }
                }
            }
            return closestTarget;
        }
    }
}