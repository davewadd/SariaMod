using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.zBookcases
{
	public class AmethystBookcase : ModItem
	{
		public override void SetStaticDefaults() {
			Tooltip.SetDefault("Contains books on spiritual minerals.");
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
			item.createTile = ModContent.TileType<Tiles.AmethystBookcase>();
		}

		public override void AddRecipes() {
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.AmethystGemsparkBlock, 5);
			recipe.AddIngredient(ItemID.SpectreBar, 15);
			recipe.AddIngredient(ItemID.LifeCrystal, 5);
			recipe.AddIngredient(ItemID.Book, 3);
			recipe.AddIngredient(ItemID.BlackFairyDust, 3);
			recipe.AddIngredient(ItemID.SpookyWood, 200);
			recipe.AddIngredient(ItemID.MothronWings, 1);
			recipe.AddIngredient(ItemID.CursedCampfire, 5);
			recipe.AddTile(ModContent.TileType<Tiles.AmberBookcase>());
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}