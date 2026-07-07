using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using SariaMod;

namespace SariaMod.Items.zTalking
{
    public class DialogueEditorUISystem : ModSystem
    {
        internal static DialogueEditorUIState EditorUI;
        private UserInterface _editorInterface;

        public static bool IsEditorActive => EditorUI?.IsActive ?? false;

        private bool _launcherWasMouseDown;
        private int _launcherClickCooldown;

        public override void Load()
        {
            if (Main.dedServ)
                return;

            EditorUI = new DialogueEditorUIState();
            EditorUI.Activate();

            _editorInterface = new UserInterface();
            _editorInterface.SetState(EditorUI);
        }

        public override void Unload()
        {
            EditorUI = null;
            _editorInterface = null;
        }

        public static void OpenEditor()
        {
            if (EditorUI == null)
                return;
            EditorUI.Open();
        }

        public static void CloseEditor()
        {
            if (EditorUI == null)
                return;
            EditorUI.Close();
        }

        public override void UpdateUI(GameTime gameTime)
        {
            if (Main.gamePaused)
                return;

            if (_launcherClickCooldown > 0)
                _launcherClickCooldown--;

            if (_editorInterface?.CurrentState != null && EditorUI?.IsActive == true)
            {
                EditorUI.HandleHotkeys();
                _editorInterface.Update(gameTime);
            }
        }

        private static Rectangle GetLauncherRect()
        {
            const int w = 120;
            const int h = 18;
            int x = (Main.screenWidth - w) / 2;
            int y = 8;
            return new Rectangle(x, y, w, h);
        }

        private static Rectangle GetResetButtonRect()
        {
            const int w = 100;
            const int h = 18;
            // Position to the left of the launcher with a small gap
            Rectangle launcher = GetLauncherRect();
            int x = launcher.X - w - 10; 
            int y = launcher.Y;
            return new Rectangle(x, y, w, h);
        }

        private static Rectangle GetResetInteractionsButtonRect()
        {
            const int w = 120;
            const int h = 18;
            // Position to the right of the launcher with a small gap
            Rectangle launcher = GetLauncherRect();
            int x = launcher.Right + 10;
            int y = launcher.Y;
            return new Rectangle(x, y, w, h);
        }

        private void UpdateLauncherClick(Rectangle rect)
        {
            if (EditorUI?.IsActive == true)
            {
                _launcherWasMouseDown = Main.mouseLeft;
                return;
            }

            bool hover = rect.Contains(Main.mouseX, Main.mouseY);
            if (hover)
                Main.LocalPlayer.mouseInterface = true;

            bool mouseDown = Main.mouseLeft;
            bool released = !mouseDown && _launcherWasMouseDown;
            
            // Only update mouse down state if we are not handling the reset button click in the same frame
            // But since they are separate rects, it should be fine.
            // We need to track mouse state globally for this system though.
            
            if (!released || _launcherClickCooldown > 0)
            {
                _launcherWasMouseDown = mouseDown;
                return;
            }

            if (hover)
            {
                OpenEditor();
                SoundEngine.PlaySound(SoundID.MenuOpen);
                _launcherClickCooldown = 12;
            }
            _launcherWasMouseDown = mouseDown;
        }

        private void UpdateResetButtonClick(Rectangle rect)
        {
            if (EditorUI?.IsActive == true) return;

            bool hover = rect.Contains(Main.mouseX, Main.mouseY);
            if (hover)
                Main.LocalPlayer.mouseInterface = true;

            // Reuse the same mouse state tracking or add a new one?
            // Since they are in the same layer callback, we can share the state if we are careful.
            // However, UpdateLauncherClick updates _launcherWasMouseDown at the end.
            // So we should probably check click state based on that.
            
            // Actually, let's just use the same logic pattern but be careful about order.
            // We will move the _launcherWasMouseDown update to the end of the delegate.
            
            bool mouseDown = Main.mouseLeft;
            bool released = !mouseDown && _launcherWasMouseDown;

            if (!released || _launcherClickCooldown > 0)
                return;

            if (hover)
            {
                var tracker = Main.LocalPlayer.GetModPlayer<SariaInteractionTrackerPlayer>();
                tracker.ResetCutscene("ZoraIntro");
                tracker.ResetCutscene("HallowIntro");
                // tracker.ResetCutscene("ThirdCutsceneID"); // Ready for 3rd
                
                Main.NewText("Cutscenes Reset!", Color.Orange);
                SoundEngine.PlaySound(SoundID.MenuTick);
                _launcherClickCooldown = 12;
            }
        }

        private void UpdateResetInteractionsButtonClick(Rectangle rect)
        {
            if (EditorUI?.IsActive == true) return;

            bool hover = rect.Contains(Main.mouseX, Main.mouseY);
            if (hover)
                Main.LocalPlayer.mouseInterface = true;

            bool mouseDown = Main.mouseLeft;
            bool released = !mouseDown && _launcherWasMouseDown;

            if (!released || _launcherClickCooldown > 0)
                return;

            if (hover)
            {
                // 1. Reset the "last 3 seen" history (random interactions)
                InteractionManager.ClearHistory();
                
                // 2. Reset the cumulative talking timer
                DialogueUIState.ResetTotalTalkingTime();

                // 3. Reset persistent player progress (completed/lost interactions)
                var tracker = Main.LocalPlayer.GetModPlayer<SariaInteractionTrackerPlayer>();
                tracker.ResetAllInteractions();

                // 4. Reset cooldowns so interactions can trigger immediately
                var fairy = Main.LocalPlayer.GetModPlayer<FairyPlayer>();
                fairy.totalTalkingTime = 0;
                fairy.smallTalkingTime = 0;
                
                Main.NewText("ALL Interactions & Timers Reset!", Color.LightBlue);
                SoundEngine.PlaySound(SoundID.MenuTick);
                _launcherClickCooldown = 12;
            }
        }

        private static void DrawLauncher(SpriteBatch spriteBatch, Rectangle rect)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;

            bool hover = rect.Contains(Main.mouseX, Main.mouseY);
            Color border = hover ? Color.Cyan : Color.DeepSkyBlue;
            Color fill = new Color(25, 60, 140) * 0.85f;

            spriteBatch.Draw(pixel, rect, fill);
            spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, rect.Width, 1), border);
            spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Bottom - 1, rect.Width, 1), border);
            spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, 1, rect.Height), border);
            spriteBatch.Draw(pixel, new Rectangle(rect.Right - 1, rect.Y, 1, rect.Height), border);

            Utils.DrawBorderString(spriteBatch, "Dialogue Creator", rect.Center.ToVector2(), Color.White, 0.65f, 0.5f, 0.5f);
        }

        private static void DrawResetButton(SpriteBatch spriteBatch, Rectangle rect)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;

            bool hover = rect.Contains(Main.mouseX, Main.mouseY);
            Color border = hover ? Color.Orange : Color.OrangeRed;
            Color fill = new Color(140, 60, 25) * 0.85f;

            spriteBatch.Draw(pixel, rect, fill);
            spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, rect.Width, 1), border);
            spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Bottom - 1, rect.Width, 1), border);
            spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, 1, rect.Height), border);
            spriteBatch.Draw(pixel, new Rectangle(rect.Right - 1, rect.Y, 1, rect.Height), border);

            Utils.DrawBorderString(spriteBatch, "Reset Cutscenes", rect.Center.ToVector2(), Color.White, 0.65f, 0.5f, 0.5f);
        }

        private static void DrawResetInteractionsButton(SpriteBatch spriteBatch, Rectangle rect)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;

            bool hover = rect.Contains(Main.mouseX, Main.mouseY);
            Color border = hover ? Color.White : Color.LightBlue;
            Color fill = new Color(60, 100, 160) * 0.85f;

            spriteBatch.Draw(pixel, rect, fill);
            spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, rect.Width, 1), border);
            spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Bottom - 1, rect.Width, 1), border);
            spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, 1, rect.Height), border);
            spriteBatch.Draw(pixel, new Rectangle(rect.Right - 1, rect.Y, 1, rect.Height), border);

            Utils.DrawBorderString(spriteBatch, "Reset Interactions", rect.Center.ToVector2(), Color.White, 0.65f, 0.5f, 0.5f);
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
            if (mouseTextIndex == -1)
                return;

            layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                "SariaMod: Dialogue Editor UI",
                delegate
                {
                    if (EditorUI?.IsActive == true)
                        _editorInterface.Draw(Main.spriteBatch, new GameTime());
                    return true;
                },
                InterfaceScaleType.UI));

            layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                "SariaMod: Dialogue Creator Launcher",
                delegate
                {
                    if (Main.dedServ)
                        return true;

                    bool enabled;
                    try { enabled = FairyConfig.Instance?.DialogueCreationPanelActive ?? false; }
                    catch { enabled = false; }

                    if (!enabled)
                        return true;

                    if (EditorUI?.IsActive == true)
                        return true;

                    Rectangle launcherRect = GetLauncherRect();
                    Rectangle resetRect = GetResetButtonRect();
                    Rectangle resetInteractionsRect = GetResetInteractionsButtonRect();
                    
                    // Handle logic
                    // We need to manage mouse state carefully here since we have two buttons
                    bool mouseDown = Main.mouseLeft;
                    bool released = !mouseDown && _launcherWasMouseDown;
                    
                    if (released && _launcherClickCooldown <= 0)
                    {
                        if (launcherRect.Contains(Main.mouseX, Main.mouseY))
                        {
                            OpenEditor();
                            SoundEngine.PlaySound(SoundID.MenuOpen);
                            _launcherClickCooldown = 12;
                        }
                        else if (resetRect.Contains(Main.mouseX, Main.mouseY))
                        {
                            var tracker = Main.LocalPlayer.GetModPlayer<SariaInteractionTrackerPlayer>();
                            tracker.ResetCutscene("ZoraIntro");
                            tracker.ResetCutscene("HallowIntro");
                            // tracker.ResetCutscene("ThirdCutsceneID"); 
                            
                            Main.NewText("Cutscenes Reset!", Color.Orange);
                            SoundEngine.PlaySound(SoundID.MenuTick);
                            _launcherClickCooldown = 12;
                        }
                        else if (resetInteractionsRect.Contains(Main.mouseX, Main.mouseY))
                        {
                            // 1. Reset the "last 3 seen" history (random interactions)
                            InteractionManager.ClearHistory();
                            
                            // 2. Reset the cumulative talking timer
                            DialogueUIState.ResetTotalTalkingTime();

                            // 3. Reset persistent player progress (completed/lost interactions)
                            var tracker = Main.LocalPlayer.GetModPlayer<SariaInteractionTrackerPlayer>();
                            tracker.ResetAllInteractions();

                            // 4. Reset cooldowns so interactions can trigger immediately
                            var fairy = Main.LocalPlayer.GetModPlayer<FairyPlayer>();
                            fairy.totalTalkingTime = 0;
                            fairy.smallTalkingTime = 0;
                            
                            Main.NewText("ALL Interactions & Timers Reset!", Color.LightBlue);
                            SoundEngine.PlaySound(SoundID.MenuTick);
                            _launcherClickCooldown = 12;
                        }
                    }
                    
                    // Update hover state for UI blocking
                    if (launcherRect.Contains(Main.mouseX, Main.mouseY) || resetRect.Contains(Main.mouseX, Main.mouseY) || resetInteractionsRect.Contains(Main.mouseX, Main.mouseY))
                    {
                        Main.LocalPlayer.mouseInterface = true;
                    }

                    _launcherWasMouseDown = mouseDown;

                    // Draw
                    DrawLauncher(Main.spriteBatch, launcherRect);
                    DrawResetButton(Main.spriteBatch, resetRect);
                    DrawResetInteractionsButton(Main.spriteBatch, resetInteractionsRect);
                    
                    return true;
                },
                InterfaceScaleType.UI));
        }
    }
}
