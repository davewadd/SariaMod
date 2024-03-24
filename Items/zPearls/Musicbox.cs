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

namespace SariaMod.Items.zPearls
{


	public class Musicbox : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault(" Musicbox ");
			Tooltip.SetDefault("");
		}




		public override void AddRecipes()
		{
			{
				Recipe recipe = Recipe.Create(ItemID.MusicBox, 1);
				recipe.AddIngredient(ItemID.Wood, 8);
				recipe.AddIngredient(ItemID.IronBar, 1);
				recipe.Register();
			}
			{
				Recipe recipe2 = Recipe.Create(ItemID.MusicBox, 1);
				recipe2.AddIngredient(ItemID.Wood, 8);
				recipe2.AddIngredient(ItemID.LeadBar, 1);
				recipe2.Register();
			}
			{
				Recipe recipe3 = Recipe.Create(ItemID.MusicBox, 1);
				recipe3.AddIngredient(ItemID.BorealWood, 8);
				recipe3.AddIngredient(ItemID.IronBar, 1);
				recipe3.Register();
			}
			{
				Recipe recipe4 = Recipe.Create(ItemID.MusicBox, 1);
				recipe4.AddIngredient(ItemID.BorealWood, 8);
				recipe4.AddIngredient(ItemID.LeadBar, 1);
				recipe4.Register();
			}
		}
	}

}