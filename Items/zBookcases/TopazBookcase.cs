using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.zBookcases
{
	public class TopazBookcase : ModItem
	{
		public override void SetStaticDefaults()
		{
			Tooltip.SetDefault("Contains books on particularly conductive minerals.");
		}

		public override void SetDefaults()
		{
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
			item.createTile = ModContent.TileType<Tiles.TopazBookcase>();
		}

		public override void AddRecipes()
		{
			{
				ModRecipe recipe = new ModRecipe(mod);
				recipe.AddIngredient(ItemID.Topaz, 5);
				recipe.AddIngredient(ItemID.CrystalShard, 20);
				recipe.AddIngredient(ItemID.NimbusRod, 1);
				recipe.AddIngredient(ItemID.Book, 3);
				recipe.AddIngredient(ItemID.RainCloud, 5);
				recipe.AddIngredient(ItemID.CobaltBar, 15);
				recipe.AddTile(ModContent.TileType<Tiles.RubyBookcase>());
				recipe.SetResult(this);
				recipe.AddRecipe();
			}
			{
				ModRecipe recipe = new ModRecipe(mod);
				recipe.AddIngredient(ItemID.Topaz, 5);
				recipe.AddIngredient(ItemID.CrystalShard, 20);
				recipe.AddIngredient(ItemID.NimbusRod, 1);
				recipe.AddIngredient(ItemID.Book, 3);
				recipe.AddIngredient(ItemID.RainCloud, 5);
				recipe.AddIngredient(ItemID.PalladiumBar, 15);
				recipe.AddTile(ModContent.TileType<Tiles.RubyBookcase>());
				recipe.SetResult(this);
				recipe.AddRecipe();
			}


		}
	}
}