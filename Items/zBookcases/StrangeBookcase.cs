using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace SariaMod.Items.zBookcases
{
    public class StrangeBookcase : ModItem
    {
        public override void SetStaticDefaults()
        {
            Tooltip.SetDefault("A bookcase full of strange materials.\n...are these from another world?\nYou can make many new notes here!");
        }
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 14;
            Item.maxStack = 99;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.value = 150;
            Item.createTile = ModContent.TileType<Tiles.StrangeBookcase>();
        }
        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.BorealWoodBookcase, 1);
            recipe.AddIngredient(ItemID.HunterPotion, 3);
            recipe.AddIngredient(ItemID.FeatherfallPotion, 3);
            recipe.AddIngredient(ItemID.Diamond, 1);
            recipe.AddIngredient(ItemID.SlimeStaff, 1);
            recipe.AddTile(TileID.WorkBenches);
            recipe.Register();
        }
    }
}