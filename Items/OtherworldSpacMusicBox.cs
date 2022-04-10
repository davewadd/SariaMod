using Terraria.ID;
using Terraria.ModLoader;

using SariaMod.Tiles;

namespace SariaMod.Items

{
	public class OtherworldSpacMusicBox : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("OtherworldSpace Music Box");
		}

		public override void SetDefaults()
		{
			item.useStyle = ItemUseStyleID.SwingThrow;
			item.useTurn = true;
			item.useAnimation = 15;
			item.useTime = 10;
			item.autoReuse = true;
			item.consumable = true;
			item.createTile = ModContent.TileType<Tiles.OtherworldSpacMusicBoxTile>();
			item.width = 24;
			item.height = 24;
			item.rare = ItemRarityID.LightRed;
			item.value = 100000;
			item.accessory = true;
		}


		public override void AddRecipes()
		{
			ModRecipe modRecipe = new ModRecipe(base.mod);
			modRecipe.AddIngredient(ItemID.MusicBox, 1);
			modRecipe.AddTile(TileID.WorkBenches);
			modRecipe.SetResult(this);
			modRecipe.AddRecipe();
		}
	}
}
