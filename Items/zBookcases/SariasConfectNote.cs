using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace SariaMod.Items.zBookcases
{
    public class SariasConfectNote : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Note to Saria's Confect");
            Tooltip.SetDefault("May make Saria stronger!\nIngredients include:\nMediumXpPearls, 1 or LivingPurpleShard, 1\nFrozen Yogurt, 1\nMana Potion, 3\nSnow or Ice, 5\nAnywhere");
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