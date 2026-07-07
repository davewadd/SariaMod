using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace SariaMod.Items.Strange
{
    /// <summary>
    /// Registers the FeelingRod UI layer and holds the shared settings state.
    /// Only active in dev builds (TestItemRecipes must exist in the assembly).
    /// </summary>
    public class FeelingRodUISystem : ModSystem
    {
        // ── Static settings (read by FeelingRod.UseItem and FeelingRodUIPanel) ──
        public static bool        IsOpen        { get; set; } = false;
        public static MoodState   SelectedMood     { get; set; } = MoodState.Happy;
        public static int         SelectedTimer    { get; set; } = 300;
        public static int         SelectedPriority { get; set; } = 1;

        private static bool _enabled;
        private static FeelingRodUIPanel _panel;
        private UserInterface             _interface;

        public override void Load()
        {
            if (Main.dedServ)
                return;

            // Only available in dev builds that ship TestItemRecipes
            _enabled = GetType().Assembly.GetType("SariaMod.TestItemRecipes") != null;
            if (!_enabled)
                return;

            _panel     = new FeelingRodUIPanel();
            _panel.Activate();
            _interface = new UserInterface();
            _interface.SetState(_panel);
        }

        public override void Unload()
        {
            _panel     = null;
            _interface = null;
            IsOpen     = false;
        }

        public override void UpdateUI(GameTime gameTime)
        {
            if (!_enabled || _interface == null)
                return;
            if (IsOpen)
                _interface.Update(gameTime);
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            if (!_enabled)
                return;

            int mouseTextIndex = layers.FindIndex(l => l.Name.Equals("Vanilla: Mouse Text"));
            if (mouseTextIndex < 0)
                return;

            layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                "SariaMod: FeelingRod UI",
                delegate
                {
                    if (IsOpen && _interface != null)
                        _interface.Draw(Main.spriteBatch, new GameTime());
                    return true;
                },
                InterfaceScaleType.UI));
        }

        // ── Public helpers ────────────────────────────────────────────────────────

        public static void ToggleUI() => IsOpen = !IsOpen;

        /// <summary>
        /// Finds the local player's Saria projectile and calls SetMoodFor with
        /// the currently configured settings.
        /// </summary>
        public static void ApplyMoodToSaria(Player player)
        {
            int sariaType = ModContent.ProjectileType<Saria>();
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (!p.active || p.type != sariaType || p.owner != player.whoAmI)
                    continue;

                if (p.ModProjectile is Saria saria)
                {
                    saria.SetMoodFor(SelectedMood, SelectedTimer, SelectedPriority);
                    // Visual feedback in chat
                    string msg = $"[FeelingRod] Applied: {SelectedMood}, {SelectedTimer}t, priority={SelectedPriority}";
                    Main.NewText(msg, new Color(255, 180, 220));
                }
                break;
            }
        }
    }
}
