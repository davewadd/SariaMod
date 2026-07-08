using Microsoft.Xna.Framework;

namespace SariaMod
{
    /// <summary>
    /// Named color palette and shorthand helpers for Terraria [c/RRGGBB:...] chat tags.
    /// Each form color is named after one of Saria's transformations.
    /// <para>Usage: <c>SariaText.Kokiri("some text")</c> or <c>SariaText.Dye("text", SariaText.ThunderGold)</c></para>
    /// </summary>
    public static class SariaText
    {
        // ── Form Colors ──────────────────────────────────────────────

        // Form 1 – Fairy / Psychic  (base Gardevoir)
        public static readonly Color KokiriGreen    = new Color(106, 214, 127);

        // Form 2 – Zora / Water  (Sapphire Form)
        public static readonly Color ZoraBlue       = new Color(80, 200, 250);

        // Form 3 – Gerudo / Twinrova / Fire  (Ruby Form)
        public static readonly Color GerudoOrange   = new Color(255, 155, 84);

        // Form 4 – Thunder / Electric  (Topaz Form)
        public static readonly Color ThunderGold    = new Color(255, 215, 80);

        // Form 5 – Rupee / Rock  (Emerald Form)
        public static readonly Color RupeeViolet    = new Color(203, 183, 247);

        // Form 6 – Lurantis / Bug  (Amber Form)
        public static readonly Color LurantisPink   = new Color(255, 173, 198);

        // Form 7 – Poe / Ghost  (Amethyst Form)
        public static readonly Color PoeLavender    = new Color(184, 154, 219);

        // ── Utility Colors ───────────────────────────────────────────

        // The mint color used for "Saria, the Champion of Foresight"
        // and form-select overlay text — matches existing Color(135, 206, 180).
        public static readonly Color ForesightMint  = new Color(135, 206, 180);

        // Bright cyan used for info lines in form notes and the HealBall
        // — matches existing Color(0, 200, 250).
        public static readonly Color InfoCyan       = new Color(0, 200, 250);

        // Soft white for generic highlighted text.
        public static readonly Color PureWhite      = new Color(255, 255, 255);

        // Muted grey for inactive or disabled UI states.
        public static readonly Color StalfosGrey    = new Color(110, 110, 110);

        // Vibrant red for warnings, danger states, and Skull Kid references.
        public static readonly Color SkullkidRed    = new Color(220, 50, 50);

        // ── Per-form shorthand ───────────────────────────────────────

        public static string Kokiri(string text)    => MiscUtilities.ColorMessage(text, KokiriGreen);
        public static string Zora(string text)      => MiscUtilities.ColorMessage(text, ZoraBlue);
        public static string Gerudo(string text)    => MiscUtilities.ColorMessage(text, GerudoOrange);
        public static string Thunder(string text)   => MiscUtilities.ColorMessage(text, ThunderGold);
        public static string Rupee(string text)     => MiscUtilities.ColorMessage(text, RupeeViolet);
        public static string Lurantis(string text)  => MiscUtilities.ColorMessage(text, LurantisPink);
        public static string Poe(string text)       => MiscUtilities.ColorMessage(text, PoeLavender);

        // ── Utility shorthand ────────────────────────────────────────

        public static string Foresight(string text) => MiscUtilities.ColorMessage(text, ForesightMint);
        public static string Info(string text)      => MiscUtilities.ColorMessage(text, InfoCyan);

        // ── Generic ──────────────────────────────────────────────────

        /// <summary>Color any text with an arbitrary Color value.</summary>
        public static string Dye(string text, Color color) => MiscUtilities.ColorMessage(text, color);
    }
}
