using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Map;
using Terraria.ModLoader;
using Terraria.UI;

namespace SariaMod.Items.Ruby
{
    public class RovaCenterMapLayer : ModMapLayer
    {
        public override void Draw(ref MapOverlayDrawContext context, ref string text)
        {
            Texture2D icon = TextureAssets.Extra[98].Value;

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile center = Main.projectile[i];
                if (!center.active || center.owner != Main.myPlayer || center.ModProjectile is not RovaCenter)
                    continue;

                var result = context.Draw(
                    icon,
                    center.Center / 16f,
                    Color.Red,
                    new SpriteFrame(1, 1, 0, 0),
                    0.18f,
                    0.18f,
                    Alignment.Center);

                if (!result.IsMouseOver)
                    continue;

                text = "Rova (click to unsummon)";

                if (Main.mouseLeft && Main.mouseLeftRelease)
                {
                    center.Kill();
                    Main.mouseLeftRelease = false;
                }
            }
        }
    }
}
