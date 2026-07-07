using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace SariaMod.Items.zBookcases
{
    public class FrozenYogurtNote : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Note to Frozen Yogurt");
            Tooltip.SetDefault("May heal Saria!\nIngredients include:\nXpPearls, 1 of LivingGreedShard, 1\nLesser Mana Potion, 3\nSnow or Ice, 5\nAnywhere");
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