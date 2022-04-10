using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.zBookcases
{
	public class RubyBookcase : ModItem
	{
		public override void SetStaticDefaults() {
			Tooltip.SetDefault("Contains books on heat resistant minerals.");
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
			item.createTile = ModContent.TileType<Tiles.RubyBookcase>();
		}

		public override void AddRecipes() {
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.Ruby, 5);
			recipe.AddIngredient(ItemID.Book, 3);
			recipe.AddIngredient(ItemID.HellstoneBar, 5);
			recipe.AddIngredient(ItemID.LavaBucket, 5);
			recipe.AddIngredient(ItemID.Fireblossom, 3);
			recipe.AddTile(ModContent.TileType<Tiles.SapphireBookcase>());
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}