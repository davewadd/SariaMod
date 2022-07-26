using Microsoft.Xna.Framework;






using System;
using SariaMod.Items.Sapphire;
using SariaMod.Items.Ruby;
using SariaMod.Items.Topaz;
using SariaMod.Items.Emerald;
using SariaMod.Items.Amber;
using SariaMod.Items.Amethyst;
using SariaMod.Items.Diamond;
using SariaMod.Items.Platinum;
using SariaMod.Items.Strange;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.zPearls
{


	public class BandOfSummoningNote : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Note to increasing minion count!");
			Tooltip.SetDefault("Craft a Band of Summoning!\nIngredients include:\nJungleSpores, 3\nRuby, 1\nXpPearl, 15\nAt a Strange Bookcase");
		}


		public override void SetDefaults()
		{
			base.item.width = 26;
			base.item.height = 22;
			base.item.maxStack = 999;
			item.rare = ItemRarityID.Green;
			base.item.value = 0;
		}


		public override void AddRecipes()
		{
			{
				ModRecipe recipe = new ModRecipe(mod);
				recipe.AddTile(ModContent.TileType<Tiles.StrangeBookcase>());
				recipe.SetResult(this, 1);
				recipe.AddRecipe();
			}
			
		}
	}

}