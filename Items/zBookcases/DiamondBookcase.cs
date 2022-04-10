using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.zBookcases
{
	public class DiamondBookcase : ModItem
	{
		public override void SetStaticDefaults() {
			Tooltip.SetDefault("Contains books on highly condensed minerals.");
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
			item.createTile = ModContent.TileType<Tiles.DiamondBookcase>();
		}

		public override void AddRecipes() {
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.DiamondGemsparkBlock, 5);
			recipe.AddIngredient(ItemID.LunarBar, 20);
			recipe.AddIngredient(ItemID.FrozenKey, 1);
			recipe.AddIngredient(ItemID.JungleKey, 1);
			recipe.AddIngredient(ItemID.Book, 3);
			recipe.AddTile(ModContent.TileType<Tiles.AmethystBookcase>());
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}