using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SariaMod.Items.zPearls
{
    /// <summary>
    /// ModSystem to handle the remote time transition effect for other players.
    /// When one player uses the Ocarina of Time, all players see the clock effect.
    /// </summary>
    public class OcarinaTimeSystem : ModSystem
    {
        public override void PostUpdateWorld()
        {
            // Update remote transition effect on clients
            if (Main.netMode != Terraria.ID.NetmodeID.Server)
            {
                OcarinaOfTimeUI.UpdateRemoteTransition();
            }
        }
        
        public override void PostDrawInterface(SpriteBatch spriteBatch)
        {
            // Draw remote transition effect for other players
            if (Main.netMode != Terraria.ID.NetmodeID.Server)
            {
                OcarinaOfTimeUI.DrawRemoteTransitionEffect(spriteBatch);
            }
        }
    }
}
