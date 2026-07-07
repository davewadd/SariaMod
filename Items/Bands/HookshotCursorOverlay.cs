using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace SariaMod.Items.Bands
{
    /// <summary>
    /// Legacy overlay — replaced by HookshotLaser projectile.
    /// Kept as empty shell to avoid reference errors.
    /// </summary>
    public class HookshotCursorOverlay : ModSystem
    {
        public override void PostDrawInterface(SpriteBatch spriteBatch)
        {
            // Intentionally empty — aiming laser is now handled by HookshotLaser projectile.
        }
    }
}
