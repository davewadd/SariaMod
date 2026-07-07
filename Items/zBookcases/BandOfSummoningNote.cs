using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace SariaMod.Items.zBookcases
{
    public class BandOfSummoningNote : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Note to increasing minion count!");
            Tooltip.SetDefault("Craft a Charm of Summoning!\nIngredients include:\nJungleSpores, 3\nRuby, 1\nXpPearl, 3\nAt a Strange Bookcase");
        }
        public override void SetDefaults()
        {
            base.Item.width = 26;
            base.Item.height = 22;
            base.Item.maxStack = 999;
            Item.rare = ItemRarityID.Green;
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