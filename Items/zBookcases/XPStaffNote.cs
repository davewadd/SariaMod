using Microsoft.Xna.Framework;






using System;
using SariaMod.Items.Sapphire;
using SariaMod.Items.Ruby;
using SariaMod.Items.Topaz;
using SariaMod.Items.Emerald;
using SariaMod.Items.Amber;
using SariaMod.Items.Amethyst;
 
using SariaMod.Items.Platinum;
using SariaMod.Items.Strange;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.zBookcases
{


	public class XPStaffNote : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Staff Note");
			Tooltip.SetDefault("Craft an XP staff!\nIngredients include:\nWood, 5\nLifeCrystal\n at a strangebookcase");
		}


		public override void SetDefaults()
		{
			base.Item.width = 26;
			base.Item.height = 22;
			base.Item.maxStack = 999;
			Item.rare = ItemRarityID.White;
			base.Item.value = 0;
		}


		public override void AddRecipes()
		{
			{
				Recipe recipe = CreateRecipe(1);
				recipe.Register();
			}
			
		}
	}

}