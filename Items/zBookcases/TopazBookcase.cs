using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.zBookcases
{
	public class TopazBookcase : ModItem
	{
		public override void SetStaticDefaults()
		{
			Tooltip.SetDefault("Contains books on particularly durable minerals.");
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
				recipe.AddIngredient(ItemID.DemonScythe, 1);
				recipe.AddIngredient(ItemID.Book, 3);
				recipe.AddIngredient(ItemID.TurtleShell, 5);
				recipe.AddIngredient(ItemID.FrozenTurtleShell, 5);
				recipe.AddIngredient(ItemID.OrichalcumBar, 15);
				recipe.AddTile(ModContent.TileType<Tiles.RubyBookcase>());
				recipe.SetResult(this);
				recipe.AddRecipe();
			}
			{
				ModRecipe recipe2 = new ModRecipe(base.mod);
				recipe2.AddIngredient(ItemID.Topaz, 5);
				recipe2.AddIngredient(ItemID.CrystalShard, 20);
				recipe2.AddIngredient(ItemID.DemonScythe, 1);
				recipe2.AddIngredient(ItemID.Book, 3);
				recipe2.AddIngredient(ItemID.TurtleShell, 5);
				recipe2.AddIngredient(ItemID.FrozenTurtleShell, 5);
				recipe2.AddIngredient(ItemID.PalladiumBar, 15);
				recipe2.AddTile(ModContent.TileType<Tiles.RubyBookcase>());
				recipe2.SetResult(this);
				recipe2.AddRecipe();
			}
		}
	}
}