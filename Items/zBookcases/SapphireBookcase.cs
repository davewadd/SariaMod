using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.zBookcases
{
	public class SapphireBookcase : ModItem
	{
		public override void SetStaticDefaults() {
			Tooltip.SetDefault("Contains books on water abundant minerals.");
		}

		public override void SetDefaults() {
			item.width = 28;
			item.height = 14;
			item.maxStack = 99;
			item.useTurn = true;
			item.autoReuse = true;
			item.useAnimation = 15;
			item.useTime = 10;
			item.useStyle = ItemUseStyleID.SwingThrow;
			item.consumable = true;
			item.value = 150;
			item.createTile = ModContent.TileType<Tiles.SapphireBookcase>();
		}

		public override void AddRecipes() {
			ModRecipe recipe = new ModRecipe(mod);

			recipe.AddIngredient(ItemID.Sapphire, 5);
			recipe.AddIngredient(ItemID.KingSlimeTrophy, 1);
			recipe.AddIngredient(ItemID.WaterBolt, 1);
			recipe.AddIngredient(ItemID.Book, 3);
			recipe.AddIngredient(ItemID.Seashell, 12);
			recipe.AddIngredient(ItemID.FrostburnArrow, 500);
			recipe.AddTile(ModContent.TileType<Tiles.StrangeBookcase>());
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}