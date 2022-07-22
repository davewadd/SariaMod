using Microsoft.Xna.Framework;






using System;
using SariaMod.Items.Sapphire;
using SariaMod.Items.Ruby;
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


	public class EmeraldNote1 : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Note to Emerald Bookcase");
			Tooltip.SetDefault("Craft a Emerald Bookcase!\nIngredients include:\nEmerald, 5\nBlessedApple, 1\nTitaniumBar, 20 or AdamantiteBar, 20\nBook, 3\nOpticStaff, 1\nAt a Topaz Bookcase");
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
				recipe.AddTile(ModContent.TileType<Tiles.TopazBookcase>());
				recipe.SetResult(this, 1);
				recipe.AddRecipe();
			}
			
		}
	}

}