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


	public class PaperNote : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Paper Note");
			Tooltip.SetDefault("Craft a Strange Bookcase!\nIngredients include:\nManaCrystal, 5\nBorealWoodBookcase, 1\nHunterPotion, 3\nFeatherfallPotion, 3\nDiamond, 1\nSlimeStaff, 1");
		}


		public override void SetDefaults()
		{
			base.item.width = 26;
			base.item.height = 22;
			base.item.maxStack = 999;
			item.rare = ItemRarityID.White;
			base.item.value = 0;
		}


		public override void AddRecipes()
		{
			{
				ModRecipe recipe = new ModRecipe(mod);
				recipe.SetResult(this, 1);
				recipe.AddRecipe();
			}
			
		}
	}

}