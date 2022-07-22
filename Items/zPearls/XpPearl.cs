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


	public class XpPearl : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault(" Xp Pearl");
			Tooltip.SetDefault("Can be used to upgrade Saria");
		}


		public override void SetDefaults()
		{
			base.item.width = 26;
			base.item.height = 22;
			base.item.maxStack = 999;
			base.item.value = 0;
		}


		public override void AddRecipes()
		{
			{
				ModRecipe recipe = new ModRecipe(mod);
				recipe.AddIngredient(ItemID.SilverCoin, 50);
				recipe.SetResult(this);
				recipe.AddRecipe();
			}
			{
				ModRecipe recipe2 = new ModRecipe(mod);
				recipe2.AddIngredient(ItemID.GoldCoin, 1);
				recipe2.SetResult(this, 2);
				recipe2.AddRecipe();
			}
			{
				ModRecipe recipe3 = new ModRecipe(mod);
				recipe3.AddIngredient(ItemID.PlatinumCoin, 1);
				recipe3.SetResult(this, 200);
				recipe3.AddRecipe();
			}
		}
	}

}