using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace SariaMod.Diagnostics
{
    /// <summary>
    /// Registers the network profiler UI layer and advances the profiler ring
    /// buffer once per UI tick. Mirrors the gating used by
    /// <c>SariaDebugUISystem</c> — only loads in dev builds (detected by the
    /// presence of <c>SariaMod.TestItemRecipes</c>).
    /// </summary>
    public class NetworkProfilerUISystem : ModSystem
    {
        private static NetworkProfilerUIPanel _panel;
        private UserInterface _ui;
        private static bool _enabled;

        public static bool DebugEnabled => _enabled;

        public override void Load()
        {
            if (Main.dedServ)
                return;

            _enabled = GetType().Assembly.GetType("SariaMod.TestItemRecipes") != null;
            if (!_enabled)
                return;

            _panel = new NetworkProfilerUIPanel();
            _panel.Activate();

            _ui = new UserInterface();
            _ui.SetState(_panel);
        }

        public override void Unload()
        {
            _panel = null;
            _ui = null;
            _enabled = false;
        }

        public override void UpdateUI(GameTime gameTime)
        {
            if (!_enabled)
                return;

            // Advance the ring buffer one slot per UI tick (~60 Hz) so the
            // sum across the buffer equals "per second" stats.
            NetworkProfiler.Tick();

            if (_ui?.CurrentState != null)
                _ui.Update(gameTime);
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            if (!_enabled)
                return;

            int idx = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
            if (idx != -1)
            {
                layers.Insert(idx, new LegacyGameInterfaceLayer(
                    "SariaMod: Network Profiler",
                    delegate
                    {
                        _ui?.Draw(Main.spriteBatch, new GameTime());
                        return true;
                    },
                    InterfaceScaleType.UI
                ));
            }
        }
    }
}
