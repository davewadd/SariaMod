using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace SariaMod.Items.zBookcases
{
    public class SariaFifthFormNote : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Saria's Emerald Form");
            Tooltip.SetDefault("~GemStorm will cause Saria to place a gem cluster under your enemies!\n~Gem Clusters will continue to damage enemies after being created!\n~Rupees may also break from clusters upon hitting enemies\nwhich can be collected and used.\n " + "\n " + SariaModUtilities.ColorMessage("Super effective in:", new Color(0, 200, 250, 200)) + "\n" + SariaModUtilities.ColorMessage("~Underground", new Color(0, 200, 250, 200)) + "\n " + "\n " + SariaModUtilities.ColorMessage("Not very effective in:", new Color(135, 206, 180)) + "\n" + SariaModUtilities.ColorMessage("~Space, and Ocean, Rain", new Color(135, 206, 180)));
        }
        public override void SetDefaults()
        {
            base.Item.width = 26;
            base.Item.height = 22;
            base.Item.maxStack = 999;
            Item.rare = ItemRarityID.Orange;
            base.Item.value = 0;
        }
        public override void AddRecipes()
        {
            {
                Recipe recipe = CreateRecipe(1);
                recipe.AddTile(ModContent.TileType<Tiles.StrangeBookcase>());
                recipe.Register();
            }
        }
    }
}