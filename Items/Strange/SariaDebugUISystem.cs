using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI;
using SariaMod.Items.zTalking;

namespace SariaMod.Items.Strange
{
    /// <summary>
    /// Registers the Saria debug panel UI layer.
    /// Draws after "Vanilla: Mouse Text" so it appears on top.
    /// Only active when TestItemRecipes exists in the mod assembly (dev builds).
    /// </summary>
    public class SariaDebugUISystem : ModSystem
    {
        private static SariaDebugUIPanel _debugPanel;
        private UserInterface _debugInterface;
        private static bool _debugEnabled;
        private static bool _hitboxVisible;
        private static string _debugStartNodeOverride = "start";

        public static bool DebugEnabled => _debugEnabled;
        public static bool HitboxVisible
        {
            get => _hitboxVisible;
            set => _hitboxVisible = value;
        }

        /// <summary>
        /// The node ID that will be passed to DisplayDialogue when clicking Saria in debug mode.
        /// Defaults to "start". Editable via the debug panel.
        /// </summary>
        public static string DebugStartNodeOverride
        {
            get => _debugStartNodeOverride;
            set => _debugStartNodeOverride = value ?? "start";
        }

        /// <summary>
        /// Mirrors the node-selection logic in Saria.cs so the debug panel can show,
        /// without opening anything, exactly which node would be used if the player
        /// clicked Saria right now.
        /// Returns a string like "Pending → forest_cave_night" or "start".
        /// </summary>
        public static string ResolveNextDialogueNode(Player player)
        {
            if (player == null) return "?";

            var tracker = player.GetModPlayer<SariaInteractionTrackerPlayer>();
            var bestPending = tracker.GetBestAvailableCutscene();

            if (bestPending != null)
                return $"Pending → {bestPending.TargetNodeID}";

            var fairyPlayer = player.GetModPlayer<FairyPlayer>();
            if (InteractionManager.CanTriggerInteractive(fairyPlayer))
            {
                string interactiveID = InteractionManager.GetRandomInteractiveDialogue();
                if (!string.IsNullOrEmpty(interactiveID))
                    return $"Interactive → {interactiveID}";
            }

            return _debugStartNodeOverride;
        }

        public override void Load()
        {
            if (Main.dedServ)
                return;

            _debugEnabled = GetType().Assembly.GetType("SariaMod.TestItemRecipes") != null;
            if (!_debugEnabled)
                return;

            _debugPanel = new SariaDebugUIPanel();
            _debugPanel.Activate();

            _debugInterface = new UserInterface();
            _debugInterface.SetState(_debugPanel);

            On.Terraria.Main.DrawProjectiles += Hook_DrawProjectileHitboxes;
        }

        public override void Unload()
        {
            On.Terraria.Main.DrawProjectiles -= Hook_DrawProjectileHitboxes;
            _debugPanel = null;
            _debugInterface = null;
            _debugEnabled = false;
            _hitboxVisible = false;
            _debugStartNodeOverride = "start";
        }

        public override void UpdateUI(GameTime gameTime)
        {
            if (!_debugEnabled)
                return;

            if (_debugInterface?.CurrentState != null)
            {
                _debugInterface.Update(gameTime);
            }
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            if (!_debugEnabled)
                return;

            int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
            if (mouseTextIndex != -1)
            {
                layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                    "SariaMod: Debug Panel",
                    delegate
                    {
                        _debugInterface?.Draw(Main.spriteBatch, new GameTime());
                        return true;
                    },
                    InterfaceScaleType.UI
                ));
            }
        }
            private void Hook_DrawProjectileHitboxes(On.Terraria.Main.orig_DrawProjectiles orig, Main self)
            {
                orig(self);

                if (!_debugEnabled || !_hitboxVisible)
                    return;

                SpriteBatch spriteBatch = Main.spriteBatch;
                Texture2D pixel = TextureAssets.MagicPixel.Value;
                Color hitboxColor = Color.Green * 0.35f;
                Color borderColor = new Color(0, 255, 0, 200);

                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile p = Main.projectile[i];
                    if (!p.active)
                        continue;

                    Rectangle hitbox = p.Hitbox;
                    // Convert world coords to screen coords
                    Rectangle screenRect = new Rectangle(
                        (int)(hitbox.X - Main.screenPosition.X),
                        (int)(hitbox.Y - Main.screenPosition.Y),
                        hitbox.Width,
                        hitbox.Height
                    );

                    // Fill
                    spriteBatch.Draw(pixel, screenRect, hitboxColor);
                    // Border
                    spriteBatch.Draw(pixel, new Rectangle(screenRect.X, screenRect.Y, screenRect.Width, 1), borderColor);
                    spriteBatch.Draw(pixel, new Rectangle(screenRect.X, screenRect.Bottom - 1, screenRect.Width, 1), borderColor);
                    spriteBatch.Draw(pixel, new Rectangle(screenRect.X, screenRect.Y, 1, screenRect.Height), borderColor);
                    spriteBatch.Draw(pixel, new Rectangle(screenRect.Right - 1, screenRect.Y, 1, screenRect.Height), borderColor);
                }

                // Saria-specific overlay: idle dot, probe rects, rings, trail dots.
                // Drawn here so it stays visible even when Saria is off-screen.
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile p = Main.projectile[i];
                    if (!p.active || p.type != ModContent.ProjectileType<Saria>())
                        continue;
                    if (p.ModProjectile is Saria saria)
                        saria.DrawDebugOverlay(spriteBatch);
                }

                spriteBatch.End();
            }
        }
    }
