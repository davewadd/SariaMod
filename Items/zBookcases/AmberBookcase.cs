using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.zBookcases
{
	public class AmberBookcase : ModItem
	{
		public override void SetStaticDefaults() {
			Tooltip.SetDefault("Contains books on prehistoric minerals.\n...The pages are sticky...");
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
			item.createTile = ModContent.TileType<Tiles.AmberBookcase>();
		}

		public override void AddRecipes() {
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.Amber, 5);
			recipe.AddIngredient(ItemID.AmberMosquito, 1);
			recipe.AddIngredient(ItemID.HeartLantern, 3);
			recipe.AddIngredient(ItemID.PapyrusScarab, 1);
			recipe.AddIngredient(ItemID.MagicHoneyDropper, 3);
			recipe.AddIngredient(ItemID.Book, 3);
			recipe.AddIngredient(ItemID.LizardEgg, 1);
			recipe.AddTile(ModContent.TileType<Tiles.EmeraldBookcase>());
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}