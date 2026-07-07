using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace SariaMod.Items.zBookcases
{
    public class SariaSeventhFormNote : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Saria's Amethyst Form");
            Tooltip.SetDefault("~ShadowSneak will send a high damage shadow slithering to your enemies!\n~Saria will now Curse enemies that get near\nstealing their ability to go through walls!\n " + "\n " + SariaModUtilities.ColorMessage("Super effective in:", new Color(0, 200, 250, 200)) + "\n" + SariaModUtilities.ColorMessage("Night, Dungeon, and all evil biomes", new Color(0, 200, 250, 200)) + "\n " + "\n " + SariaModUtilities.ColorMessage("Not very effective in:", new Color(135, 206, 180)) + "\n" + SariaModUtilities.ColorMessage("~Overworld during the day", new Color(135, 206, 180)));
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