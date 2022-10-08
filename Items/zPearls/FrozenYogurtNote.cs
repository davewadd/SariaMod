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


	public class FrozenYogurtNote : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Note to Frozen Yogurt");
			Tooltip.SetDefault("May heal Saria!\nIngredients include:\nMediumXpPearls, 2\nSuper Mana Potion, 3\nSnow or Ice, 5\nAnywhere");
		}


		public override void SetDefaults()
		{
			base.item.width = 26;
			base.item.height = 22;
			base.item.maxStack = 999;
			item.rare = ItemRarityID.Orange;
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