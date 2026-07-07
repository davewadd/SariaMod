using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.Map;
using Terraria.ModLoader;
using Terraria.UI;

namespace SariaMod.Items.Strange
{
    /// <summary>
    /// SARIA MAP ICON - Shows Saria's location on the minimap and fullscreen map
    /// Uses SariaIcon.png texture
    /// </summary>
    public class SariaMapLayer : ModMapLayer
    {
        public override void Draw(ref MapOverlayDrawContext context, ref string text)
        {
            Texture2D iconRight;
            Texture2D iconLeft;

            try
            {
                iconRight = ModContent.Request<Texture2D>("SariaMod/SariaIcon").Value;
                iconLeft = ModContent.Request<Texture2D>("SariaMod/SariaIconLeft").Value;
            }
            catch
            {
                return;
            }

            if (iconRight == null && iconLeft == null)
                return;

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];
                if (!proj.active)
                    continue;

                if (proj.ModProjectile is not Saria)
                    continue;

                Vector2 sariaPosition = proj.Center;

                // `spriteDirection` is 1 (right) or -1 (left)
                Texture2D iconTexture = proj.spriteDirection < 0 ? iconLeft : iconRight;
                if (iconTexture == null)
                    iconTexture = iconRight ?? iconLeft;

                var drawResult = context.Draw(
                    iconTexture,
                    sariaPosition / 16f,
                    Color.White,
                    new SpriteFrame(1, 1, 0, 0),
                    1f,
                    1f,
                    Alignment.Center
                );

                if (drawResult.IsMouseOver)
                {
                    if (proj.owner >= 0 && proj.owner < Main.maxPlayers && Main.player[proj.owner].active)
                        text = $"Saria ({Main.player[proj.owner].name})";
                    else
                        text = "Saria";
                }
            }
        }
    }
}
