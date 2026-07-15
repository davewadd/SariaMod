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

        public override void PostDraw(Color lightColor)
        {
            // Owner-only LinkCable destination marker. In the LinkCable dot-placement
            // mode only the owning player sees the marked location; the A* trail stays
            // hidden here (it is shown solely through the debug overlay). Drawn in
            // PostDraw so it is visible in normal gameplay without debug visuals on.
            if (_linkCableFollow && Main.myPlayer == Projectile.owner && _followMarkedPosition != Vector2.Zero)
            {
                Texture2D pixelMark = TextureAssets.MagicPixel.Value;
                Vector2 markScreen  = _followMarkedPosition - Main.screenPosition;

                // Dark red filled square (12×12) at the target position.
                Main.spriteBatch.Draw(pixelMark,
                    new Rectangle((int)markScreen.X - 6, (int)markScreen.Y - 6, 12, 12),
                    null, new Color(160, 20, 20, 220));

                // SariaIcon.png centered above the dot.
                Texture2D markIcon = ModContent.Request<Texture2D>("SariaMod/SariaIcon").Value;
                Vector2 markIconOrigin = new Vector2(markIcon.Width * 0.5f, markIcon.Height);
                Main.spriteBatch.Draw(markIcon,
                    new Vector2(markScreen.X, markScreen.Y - 8),
                    null, Color.White * 0.9f,
                    0f, markIconOrigin, 0.75f, SpriteEffects.None, 0f);
            }

            {
                Player player = Main.player[Projectile.owner];
                FairyPlayer modPlayer = player.Fairy();
                float sneezespot = 5;
                {
                    Vector2 drawPosition;
                    Vector2 mouse = Main.MouseWorld;
                    mouse.X += 10f;
                    mouse.Y -= 5f;
                    float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius3 * sphereRadius3));
                    double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                    Vector2 startPos2 = Projectile.Center;
                    float radius2 = ((Projectile.Center.Y - 10) + radius * (float)Math.Sin(angle));
                    startPos2.Y = radius2;
                    if (Projectile.spriteDirection > 0)
                    {
                        sneezespot = 18;
                    }
                    if (Projectile.spriteDirection < 0)
                    {
                        sneezespot = 3;
                    }
                    startPos2.X += sneezespot;
                    float between = Vector2.Distance(mouse, startPos2);
                    bool Rightclick = (player.HeldItem.type == ModContent.ItemType<HealBall>() && Main.mouseLeft && (Main.myPlayer == Projectile.owner));
                    if (between > 30)
                    {
                        SelectSound = false;
                    }

                    // Check for pending cutscenes (Persistent Icon Logic)
                    var tracker = player.GetModPlayer<SariaInteractionTrackerPlayer>();
                    var bestPending = tracker.GetBestAvailableCutscene();
                    bool hasAny = tracker.HasAnyPendingCutscenes();

                    if (hasAny && !SariaUISystem.IsDialogueActive)
                    {
                        if (bestPending != null)
                        {
                            // Condition met -> Orange (Normal SariaTimed)
                            Projectile.SariaBubbleFaces(((Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/SariaTimed").Value), false, 60, 3, -50, lightColor);
                        }
                        else
                        {
                            // Condition NOT met -> Grey (SariaTimedGrey)
                            // Using SariaTimedGrey texture instead of tinting
                            Projectile.SariaBubbleFaces(((Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/SariaTimedGrey").Value), false, 60, 3, -50, lightColor);
                        }
                    }

                    if (ChangeForm > 0 && (Main.myPlayer == Projectile.owner))
                    {
                        if (between <= 30 && (Main.myPlayer == Projectile.owner))
                        {
                            player.noThrow = 2;
                            player.cursorItemIconEnabled = true;
                            player.cursorItemIconID = ModContent.ItemType<Items.Bands.Blank>();
                            player.cursorItemIconText = (MiscUtilities.ColorMessage("Saria", new Color(135, 206, 180)));
                            {
                                // Check for pending cutscenes
                                // Reusing variables from above: 'tracker', 'bestPending', 'hasAny'
                                // var tracker2 = player.GetModPlayer<SariaInteractionTrackerPlayer>();
                                // var pending = tracker2.GetActivePendingCutscene();

                                bool showTimed = hasAny; // Show timed icon if any exist

                                if (showTimed)
                                {
                                    if (bestPending != null)
                                    {
                                        Projectile.SariaBubbleFaces(((Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/SariaTimed").Value), false, 60, 3, -50, lightColor);
                                    }
                                    else
                                    {
                                        Projectile.SariaBubbleFaces(((Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/SariaTimedGrey").Value), false, 60, 3, -50, lightColor);
                                    }
                                }
                                else
                                            // Only draw SariaTalk if NOT showing SariaTimed
                                            if (!hasAny)
                                {
                                    Projectile.SariaBubbleFaces(((Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/SariaTalk").Value), false, 60, 3, -50, lightColor);
                                }

                                if (!SelectSound)
                                {
                                    SoundEngine.PlaySound(SoundID.MenuTick);
                                    SelectSound = true;
                                }
                            }
                        }
                        if (between <= 30 && Rightclick && Eating <= 0)
                        {
                            if (!SariaUISystem.IsDialogueActive)
                            {
                                SariaTalking = true;
                                ChangeForm = 0; // Close the form change overlay when opening dialogue
                                Projectile.netUpdate = true;
                                SoundEngine.PlaySound(SoundID.MenuOpen);

                                // Check pending cutscene for interaction
                                if (bestPending != null)
                                {
                                    // Condition met: Go to Pending node
                                    SariaUISystem.DisplayDialogue("Pending", Projectile);
                                }
                                else if (InteractionManager.CanTriggerInteractive(modPlayer))
                                {
                                    string interactiveID = InteractionManager.GetRandomInteractiveDialogue();
                                    if (!string.IsNullOrEmpty(interactiveID))
                                    {
                                        InteractionManager.RegisterInteractiveDialogue(interactiveID);
                                        InteractionManager.IsInteractiveSession = true;
                                        SariaUISystem.DisplayDialogue(interactiveID, Projectile);
                                    }
                                    else
                                    {
                                        // No interactive dialogue configured — fall back to normal dialogue
                                        string startNode = SariaDebugUISystem.DebugEnabled
                                            ? SariaDebugUISystem.DebugStartNodeOverride
                                            : "start";
                                        SariaUISystem.DisplayDialogue(startNode, Projectile);
                                    }
                                }
                                else
                                {
                                    // Default behavior — use debug override if available
                                    string startNode = SariaDebugUISystem.DebugEnabled
                                        ? SariaDebugUISystem.DebugStartNodeOverride
                                        : "start";
                                    SariaUISystem.DisplayDialogue(startNode, Projectile);
                                }
                            }
                        }
                    }
                    // Tick transform phase once before all transform draws.
                    TickTransformPhase();

                    Projectile.SariaBubbleFaceLoader((int)ChangeForm, (int)Eating, lightColor);
                    Color sariaLightColor = ApplyColdWaterFormChilledVisuals(lightColor);
                    Projectile.SariaFeetandArmDraw((int)Transform, (int)Eating, sariaLightColor);

                    // Idle feet pass (bottommost — drawn before body so glow stays underneath)
                    if (Projectile.frame <= SariaIdleAnimator.IdleFrameMax && Eating <= 0)
                        IdleAnimator.DrawFeetPass(Projectile, sariaLightColor);

                    // Body pass (behind faces) — body, eat, hair, scar, body masks only
                    Projectile.SariaBodyDraw((int)Transform, (int)Eating, (int)IsCharging, (int)ChannelState, (int)SpecialAnimate, sariaLightColor, armsOnly: false);

                    // Idle legs pass (behind faces)
                    if (Projectile.frame <= SariaIdleAnimator.IdleFrameMax && Eating <= 0)
                        IdleAnimator.DrawLegsPass(Projectile, Transform, sariaLightColor);

                    // Emit campfire-like light for the flaming hair when in Transform 2 and hair is visible.
                    if (Transform == 2)
                    {
                        Vector2 lightPos = Projectile.Center + new Vector2(0f, -20f);
                        Lighting.AddLight(lightPos, Color.Orange.ToVector3() * 1.2f);
                    }
                    Projectile.SariaHornDraw((int)Transform, sariaLightColor);

                    // Owner drives the displayed mood through the blink gate.
                    // Non-owner clients get DisplayedMood pushed directly from the synced packet value.
                    if (Projectile.owner == Main.myPlayer)
                        IdleAnimator.UpdateDisplayedMood(Mood);

                    // Idle underlays — drawn BEFORE the face base layer
                    if (Projectile.frame <= SariaIdleAnimator.IdleFrameMax && Eating <= 0)
                    {
                        IdleAnimator.DrawAngryIdleUnderlay(Projectile, Transform, Mood, Cursed, sariaLightColor);
                        IdleAnimator.DrawMouthIdleUnderlay(Projectile, Transform, Mood, Cursed, sariaLightColor);
                    }

                    // Faces and chest pieces
                    Projectile.SariaSmallFacesOrWhencursed((int)Transform, (bool)Sleep, (int)Eating, (int)IsCharging, (bool)Cursed, (int)ChannelState, (int)Mood, sariaLightColor, IdleAnimator);

                    // Idle eye overlays — drawn AFTER the face base layer
                    if (Projectile.frame <= SariaIdleAnimator.IdleFrameMax && Eating <= 0)
                    {
                        IdleAnimator.DrawIdleEyes(Projectile, Transform, Mood, Cursed, sariaLightColor);
                        IdleAnimator.DrawHappyIdleEyes(Projectile, Transform, Mood, Cursed, sariaLightColor);
                        IdleAnimator.DrawSadIdleEyes(Projectile, Transform, Mood, Cursed, sariaLightColor);
                        IdleAnimator.DrawAngryIdleEyes(Projectile, Transform, Mood, Cursed, sariaLightColor);
                    }

                    // Arms pass (over faces and chest) — direction arms + their masks
                    Projectile.SariaBodyDraw((int)Transform, (int)Eating, (int)IsCharging, (int)ChannelState, (int)SpecialAnimate, sariaLightColor, armsOnly: true);

                    // Idle arms pass (over faces and chest)
                    if (Projectile.frame <= SariaIdleAnimator.IdleFrameMax && Eating <= 0)
                        IdleAnimator.DrawArmsPass(Projectile, Transform, sariaLightColor);

                    // Cursed shadowy overlay — drawn over all body parts, arms, and legs
                    // so it covers every form's glows and all animation states.
                    if (Mood == (int)MoodState.Cursed)
                    {
                        Color cursedTint = Color.Lerp(sariaLightColor, new Color(60, 0, 80, 200), 0.55f);
                        Projectile.SariaBodyDraw((int)Transform, (int)Eating, (int)IsCharging, (int)ChannelState, (int)SpecialAnimate, cursedTint, armsOnly: false);
                        Projectile.SariaBodyDraw((int)Transform, (int)Eating, (int)IsCharging, (int)ChannelState, (int)SpecialAnimate, cursedTint, armsOnly: true);
                        if (Projectile.frame <= SariaIdleAnimator.IdleFrameMax && Eating <= 0)
                        {
                            IdleAnimator.DrawFeetPass(Projectile, cursedTint);
                            IdleAnimator.DrawLegsPass(Projectile, Transform, cursedTint);
                            IdleAnimator.DrawArmsPass(Projectile, Transform, cursedTint);
                        }
                    }

                    // Attack arm top pass — drawn over direction arms and idle arms
                    Projectile.SariaAttackArmTopDraw(Transform, (int)IsCharging, (int)ChannelState, (int)Eating, Sleep, Cursed, sariaLightColor);

                    Projectile.SariaChargingAnimation((int)Transform, (bool)Sleep, (int)Eating, (int)IsCharging, (bool)Cursed, (int)ChannelState, (int)Mood, sariaLightColor);
                    Projectile.SariaEatDraw((int)Transform, (int)Eating, sariaLightColor, IdleAnimator);
                    Projectile.SariaSleepDraw((int)Transform, (bool)Sleep, sariaLightColor, IdleAnimator);

                    // Sparks overlay — drawn last so it appears over all body parts,
                    // masks, arms, faces, and other overlays.
                    if (Transform == 3 && SpecialAnimate > 0)
                    {
                        Projectile.SariaSparksDraw(TextureAssets.Projectile[ModContent.ProjectileType<SariaSparks>()].Value, sariaLightColor);
                    }

                    if (XpTimer && Main.myPlayer == Projectile.owner)
                    {
                        Projectile.SariaDrawInterface(lightColor, SariaExtensions1.InterfaceType.XPBar);
                        Projectile.SariaDrawInterface(lightColor, SariaExtensions1.InterfaceType.NextBoss);
                    }

                    // Transformation glow sphere — drawn absolutely last so it overlays
                    // every body layer, arm, face, and UI element.
                    DrawTransformGlowSphere();

                    // Source teleport sphere — drawn on Saria when wind-up is active.
                    // The destination sphere is drawn by SariaModSystem.PostDrawTiles
                    // so it renders even when Saria is far off-screen.
                    DrawTeleportSourceSphere();

                    // Debug: per-Saria bandwidth label, visible on non-owner clients
                    // when the network profiler panel is open/enabled.
                    if (Diagnostics.NetworkProfilerUISystem.DebugEnabled && Main.myPlayer != Projectile.owner)
                    {
                        var (netBytes, netPkts) = Diagnostics.NetworkProfiler.GetSariaAggregate();
                        string label = $"Net {Diagnostics.NetworkProfiler.FormatBytes(netBytes)}/s  {netPkts}p/s";
                        Vector2 labelPos = Projectile.Top - Main.screenPosition - new Vector2(0, 14);
                        Vector2 labelSize = Terraria.GameContent.FontAssets.MouseText.Value.MeasureString(label) * 0.65f;
                        labelPos.X -= labelSize.X * 0.5f;
                        Color labelColor = netBytes > 1024 ? new Color(255, 160, 80) : new Color(120, 220, 120);
                        Utils.DrawBorderString(Main.spriteBatch, label, labelPos, labelColor, 0.65f);
                    }

                    }

                // Debug overlay is now drawn by SariaDebugUISystem.Hook_DrawProjectileHitboxes
                // via DrawDebugOverlay() so it stays visible when Saria is off-screen.
            }
        }


        /// <summary>
        /// Compares two A* paths for equality (same length and matching tile-center
        /// waypoints). Treats null and empty as equal so cleared paths don't spam sync.
        /// </summary>
        private static bool FollowPathsEqual(System.Collections.Generic.List<Vector2> a, System.Collections.Generic.List<Vector2> b)
        {
            int countA = a?.Count ?? 0;
            int countB = b?.Count ?? 0;
            if (countA != countB)
                return false;
            for (int i = 0; i < countA; i++)
                if (a[i] != b[i])
                    return false;
            return true;
        }

        /// </summary>
        public void DrawDebugOverlay(SpriteBatch spriteBatch)
        {
            // Marked location — drawn for all clients so everyone sees the chosen target dot.
            if ((Follow || _linkCableFollow) && _followMarkedPosition != Vector2.Zero)
            {
                Texture2D pixelMark = TextureAssets.MagicPixel.Value;
                Vector2 markScreen  = _followMarkedPosition - Main.screenPosition;

                // Dark red filled square (12×12) at the target position.
                spriteBatch.Draw(pixelMark,
                    new Rectangle((int)markScreen.X - 6, (int)markScreen.Y - 6, 12, 12),
                    null, new Color(160, 20, 20, 220));

                // SariaIcon.png centered above the dot.
                Texture2D icon = ModContent.Request<Texture2D>("SariaMod/SariaIcon").Value;
                Vector2 iconOrigin = new Vector2(icon.Width * 0.5f, icon.Height);
                spriteBatch.Draw(icon,
                    new Vector2(markScreen.X, markScreen.Y - 8),
                    null, Color.White * 0.9f,
                    0f, iconOrigin, 0.75f, SpriteEffects.None, 0f);
            }

            // A* pink dotted trail — drawn for all clients (synced via Extra AI).
            // A pink dot sits on each tile node, joined by a pink dotted line.
            if ((Follow || _linkCableFollow) && _followPath.Count > 0)
            {
                Texture2D pixelPath = TextureAssets.MagicPixel.Value;
                Color pathColor = new Color(255, 105, 200, 230);

                // Dotted connector segments between consecutive nodes.
                const float dotSpacing = 6f;
                for (int i = 0; i < _followPath.Count - 1; i++)
                {
                    Vector2 a = _followPath[i]     - Main.screenPosition;
                    Vector2 b = _followPath[i + 1] - Main.screenPosition;
                    Vector2 seg = b - a;
                    float len = seg.Length();
                    if (len <= 0f)
                        continue;
                    Vector2 dir = seg / len;
                    for (float d = 0f; d < len; d += dotSpacing)
                    {
                        Vector2 p = a + dir * d;
                        spriteBatch.Draw(pixelPath,
                            new Rectangle((int)p.X - 1, (int)p.Y - 1, 2, 2),
                            null, pathColor);
                    }
                }

                // Pink node dot on each tile.
                for (int i = 0; i < _followPath.Count; i++)
                {
                    Vector2 nodeScreen = _followPath[i] - Main.screenPosition;
                    spriteBatch.Draw(pixelPath,
                        new Rectangle((int)nodeScreen.X - 3, (int)nodeScreen.Y - 3, 6, 6),
                        null, pathColor);
                }
            }

            // Everything below is owner-only debug visualisation.
            if (Main.myPlayer != Projectile.owner)
                return;

            Player player = Main.player[Projectile.owner];
            Texture2D pixel = TextureAssets.MagicPixel.Value;

            // InWall escape target — pink dot with SariaIcon, shown when she is stuck in geometry.
            if (_inWallEscapeTarget != Vector2.Zero)
            {
                Texture2D pixelEscape = TextureAssets.MagicPixel.Value;
                Vector2 escScreen = _inWallEscapeTarget - Main.screenPosition;

                // During teleport wind-up the dot pulses; before that it is dim.
                float pulse = _inWallTeleportTimer > 0
                    ? 0.5f + 0.5f * (float)Math.Sin(Main.GameUpdateCount * 0.25f)
                    : 0.45f;
                Color dotColor  = new Color(255, 80, 200) * pulse;
                Color iconColor = new Color(255, 80, 200) * (pulse * 0.9f);

                spriteBatch.Draw(pixelEscape,
                    new Rectangle((int)escScreen.X - 5, (int)escScreen.Y - 5, 10, 10),
                    null, dotColor);

                Texture2D escIcon = ModContent.Request<Texture2D>("SariaMod/SariaIcon").Value;
                Vector2 escIconOrigin = new Vector2(escIcon.Width * 0.5f, escIcon.Height);
                spriteBatch.Draw(escIcon,
                    new Vector2(escScreen.X, escScreen.Y - 8),
                    null, iconColor,
                    0f, escIconOrigin, 0.75f, SpriteEffects.None, 0f);
            }

            // Convert world positions to screen positions once.
            Vector2 idleScreenPos  = _debugIdlePosition - Main.screenPosition;
            Vector2 playerScreenPos = player.Center     - Main.screenPosition;

            // Red dot: idle position.
            spriteBatch.Draw(pixel,
                new Rectangle((int)idleScreenPos.X - 4, (int)idleScreenPos.Y - 4, 8, 8),
                null, new Color(255, 0, 0, 200));

            // Ground-probe visualizer.
            // Hitbox probe  — pink/red outline: lit when embedded in a tile (push-up fires).
            // Ground line   — green outline:    lit when feet are touching a surface (stable).
            // Scan probe    — yellow outline:   lit when a tile is within 1.5 tiles below (settle-down eligible).
            void DrawOutline(Rectangle r, Color c)
            {
                Vector2 o = Main.screenPosition;
                spriteBatch.Draw(pixel, new Rectangle(r.X - (int)o.X, r.Y          - (int)o.Y, r.Width,  1),         c);
                spriteBatch.Draw(pixel, new Rectangle(r.X - (int)o.X, r.Bottom     - (int)o.Y, r.Width,  1),         c);
                spriteBatch.Draw(pixel, new Rectangle(r.X - (int)o.X, r.Y          - (int)o.Y, 1,        r.Height),  c);
                spriteBatch.Draw(pixel, new Rectangle(r.Right - (int)o.X, r.Y      - (int)o.Y, 1,        r.Height),  c);
            }

            {
                Vector2 dbgSpritePos = new Vector2(
                    (float)Math.Round(Projectile.position.X),
                    (float)Math.Round(Projectile.position.Y));

                for (int di = 0; di < _detectorConfigs.Length; di++)
                {
                    SariaDetectorConfig dcfg = _detectorConfigs[di];
                    SariaDetector.GetFacingDir(dcfg.RotationDegrees, out int difx, out int dify);
                    SariaDetector.GetProbeRects(in dcfg, dbgSpritePos, difx, dify,
                        out Rectangle pinkR, out Rectangle greenR, out Rectangle yellowR,
                        Projectile.width, Projectile.spriteDirection);

                    bool dp = _detectorResults[di].Pink;
                    bool dg = _detectorResults[di].Green;
                    bool dy = _detectorResults[di].Yellow;

                    Color pinkActive   = new Color(255,  60, 180, 230); Color pinkDim   = new Color(255,  60, 180,  60);
                    Color greenActive  = new Color( 60, 230,  80, 230); Color greenDim  = new Color( 60, 230,  80,  60);
                    Color yellowActive = new Color(255, 220,   0, 200); Color yellowDim = new Color(255, 220,   0,  50);

                    if (dcfg.PinkDepth > 0)
                        DrawOutline(pinkR,   dp ? pinkActive   : pinkDim);
                    if (dcfg.GreenDepth > 0)
                        DrawOutline(greenR,  dg ? greenActive  : greenDim);
                    if (dcfg.HasPullLine && dcfg.PullLength > 0)
                        DrawOutline(yellowR, dy ? yellowActive : yellowDim);
                }

                // Orange bounding box spanning both wall probe pink rects ([2] left, [3] right).
                {
                    SariaDetector.GetFacingDir(_detectorConfigs[2].RotationDegrees, out int owlx, out int owly);
                    SariaDetector.GetFacingDir(_detectorConfigs[3].RotationDegrees, out int owrx, out int owry);
                    SariaDetector.GetProbeRects(in _detectorConfigs[2], dbgSpritePos, owlx, owly,
                        out Rectangle orangeWallL, out _, out _);
                    SariaDetector.GetProbeRects(in _detectorConfigs[3], dbgSpritePos, owrx, owry,
                        out Rectangle orangeWallR, out _, out _);

                    int owLeft   = orangeWallL.Left;
                    int owRight  = orangeWallR.Right - 2;
                    int owTop    = Math.Min(orangeWallL.Top,    orangeWallR.Top);
                    int owBottom = Math.Max(orangeWallL.Bottom, orangeWallR.Bottom);
                    DrawOutline(
                        new Rectangle(owLeft, owTop, owRight - owLeft, owBottom - owTop),
                        new Color(255, 140, 0, 200));
                }

                // Body-fit box: outer left of left-wall green → outer right of right-wall green,
                // top Y of the side green rects → bottom of the feet green rect.
                // This represents the actual collision footprint.
                {
                    SariaDetector.GetFacingDir(_detectorConfigs[0].RotationDegrees, out int f0x, out int f0y);
                    SariaDetector.GetFacingDir(_detectorConfigs[2].RotationDegrees, out int f2x, out int f2y);
                    SariaDetector.GetFacingDir(_detectorConfigs[3].RotationDegrees, out int f3x, out int f3y);

                    SariaDetector.GetProbeRects(in _detectorConfigs[0], dbgSpritePos, f0x, f0y,
                        out _, out Rectangle greenFeet,  out _,
                        Projectile.width, Projectile.spriteDirection);
                    SariaDetector.GetProbeRects(in _detectorConfigs[2], dbgSpritePos, f2x, f2y,
                        out _, out Rectangle greenWallL, out _);
                    SariaDetector.GetProbeRects(in _detectorConfigs[3], dbgSpritePos, f3x, f3y,
                        out _, out Rectangle greenWallR, out _);

                    int boxLeft   = greenWallL.Right;
                    int boxRight  = greenWallR.Left - 2;
                    int boxTop    = Math.Min(greenWallL.Y, greenWallR.Y);
                    int boxBottom = greenFeet.Bottom - 2;

                    DrawOutline(
                        new Rectangle(boxLeft, boxTop, boxRight - boxLeft, boxBottom - boxTop),
                        new Color(255, 255, 255, 200));
                }
            }

            // White ring: radius = TileCollisionRadius — ground-probe active inside, disabled outside (non-cursed).
            {
                bool insideTileRadius = Mood != (int)MoodState.Cursed
                    && Vector2.Distance(Projectile.Center, player.Center) <= TileCollisionRadius;
                const int tileRingSegments = 36;
                Color tileRingColor = insideTileRadius
                    ? new Color(180, 220, 255, 180)  // light blue — probe active
                    : new Color(255, 255, 255, 100);  // white — probe disabled
                for (int seg = 0; seg < tileRingSegments; seg++)
                {
                    float a1 = seg       * MathHelper.TwoPi / tileRingSegments;
                    float a2 = (seg + 1) * MathHelper.TwoPi / tileRingSegments;
                    Vector2 rp1 = playerScreenPos + new Vector2((float)Math.Cos(a1), (float)Math.Sin(a1)) * TileCollisionRadius;
                    Vector2 rp2 = playerScreenPos + new Vector2((float)Math.Cos(a2), (float)Math.Sin(a2)) * TileCollisionRadius;
                    Vector2 rdiff = rp2 - rp1;
                    float   rlen  = rdiff.Length();
                    if (rlen > 0f)
                    {
                        float rangle = (float)Math.Atan2(rdiff.Y, rdiff.X);
                        spriteBatch.Draw(pixel, rp1, new Rectangle(0, 0, 1, 1), tileRingColor,
                            rangle, Vector2.Zero, new Vector2(rlen, 1.5f), SpriteEffects.None, 0f);
                    }
                }
            }

            // Blue/red ring: radius = CursedSeparationRadius — matches the AI separation threshold exactly.
            // Saria follows the trail while she is outside this ring.
            if (Mood == (int)MoodState.Cursed)
            {
                float ringRadius = CursedSeparationRadius;
                const int ringSegments = 48;
                Color ringColor = _cursedSeparated
                    ? new Color(255, 80, 80, 180)   // red — currently separated / following trail
                    : new Color(80, 160, 255, 180);  // blue — inside ring / following direct
                for (int seg = 0; seg < ringSegments; seg++)
                {
                    float a1 = seg       * MathHelper.TwoPi / ringSegments;
                    float a2 = (seg + 1) * MathHelper.TwoPi / ringSegments;
                    Vector2 rp1 = idleScreenPos + new Vector2((float)Math.Cos(a1), (float)Math.Sin(a1)) * ringRadius;
                    Vector2 rp2 = idleScreenPos + new Vector2((float)Math.Cos(a2), (float)Math.Sin(a2)) * ringRadius;
                    Vector2 rdiff = rp2 - rp1;
                    float   rlen  = rdiff.Length();
                    if (rlen > 0f)
                    {
                        float rangle = (float)Math.Atan2(rdiff.Y, rdiff.X);
                        spriteBatch.Draw(pixel, rp1, new Rectangle(0, 0, 1, 1), ringColor,
                            rangle, Vector2.Zero, new Vector2(rlen, 1.5f), SpriteEffects.None, 0f);
                    }
                }
            }

            // Follow trail dots — yellow markers with permanent number labels.
            if (Follow && _followTrailDots.Count > 0)
            {
                Color dotColor   = new Color(255, 220, 0, 220);
                Color labelColor = new Color(255, 255, 255, 230);
                for (int di = 0; di < _followTrailDots.Count; di++)
                {
                    Vector2 dotScreen = _followTrailDots[di].Position - Main.screenPosition;
                    spriteBatch.Draw(pixel,
                        new Rectangle((int)dotScreen.X - 3, (int)dotScreen.Y - 3, 6, 6),
                        null, dotColor);

                    string label = _followTrailDots[di].Number.ToString();
                    Vector2 labelSize = Terraria.GameContent.FontAssets.MouseText.Value.MeasureString(label) * 0.55f;
                    Utils.DrawBorderString(spriteBatch, label,
                        new Vector2(dotScreen.X - labelSize.X * 0.5f, dotScreen.Y - 3 - labelSize.Y),
                        labelColor, 0.55f);
                }
            }
        }
    }
}
