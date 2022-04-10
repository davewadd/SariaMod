using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.zBookcases
{
	public class EmeraldBookcase : ModItem
	{
		public override void SetStaticDefaults() {
			Tooltip.SetDefault("Contains books on rare minerals.");
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
			item.createTile = ModContent.TileType<Tiles.EmeraldBookcase>();
		}

		public override void AddRecipes() 
			{
				{
					ModRecipe recipe = new ModRecipe(mod);

					recipe.AddIngredient(ItemID.Emerald, 5);
					recipe.AddIngredient(ItemID.BlessedApple, 1);
					recipe.AddIngredient(ItemID.TitaniumBar, 20);
				recipe.AddIngredient(ItemID.Book, 3);
				recipe.AddIngredient(ItemID.OpticStaff, 1);
				recipe.AddTile(ModContent.TileType<Tiles.TopazBookcase>());
				recipe.SetResult(this);
					recipe.AddRecipe();
				}
				{
					ModRecipe recipe2 = new ModRecipe(base.mod);
					recipe2.AddIngredient(ItemID.Emerald, 5);
					recipe2.AddIngredient(ItemID.BlessedApple, 1);
					recipe2.AddIngredient(ItemID.AdamantiteBar, 20);
				recipe2.AddIngredient(ItemID.Book, 3);
				recipe2.AddIngredient(ItemID.OpticStaff, 1);
				recipe2.AddTile(ModContent.TileType<Tiles.TopazBookcase>());
				recipe2.SetResult(this);
					recipe2.AddRecipe();
				}
			}
		}
}