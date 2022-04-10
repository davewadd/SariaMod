using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.zBookcases
{
	public class PlatinumBookcase : ModItem
	{
		public override void SetStaticDefaults() {
			Tooltip.SetDefault("Contains books on pure minerals.");
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
			item.createTile = ModContent.TileType<Tiles.PlatinumBookcase>();
		}

		public override void AddRecipes() {
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.RodofDiscord, 1);
			recipe.AddIngredient(ItemID.PlatinumBar, 200);
			recipe.AddIngredient(ItemID.DiamondGemsparkBlock, 5);
			recipe.AddIngredient(ItemID.AmethystGemsparkBlock, 5);
			recipe.AddIngredient(ItemID.AmberGemsparkBlock, 5);
			recipe.AddIngredient(ItemID.EmeraldGemsparkBlock, 5);
			recipe.AddIngredient(ItemID.TopazGemsparkBlock, 5);
			recipe.AddIngredient(ItemID.RubyGemsparkBlock, 5);
			recipe.AddIngredient(ItemID.SapphireGemsparkBlock, 5);
			recipe.AddIngredient(ItemID.LunarBar, 200);
			recipe.AddTile(ModContent.TileType<Tiles.DiamondBookcase>());
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}