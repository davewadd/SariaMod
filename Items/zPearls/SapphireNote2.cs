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


	public class SapphireNote2 : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Note to Sapphire Tome");
			Tooltip.SetDefault("Craft a Sapphire Tome!\nIngredients include:\nStrangeTome, 1\nSapphire, 5\nXpPearl, 75\nAt a Sapphire Bookcase");
		}


		public override void SetDefaults()
		{
			base.item.width = 26;
			base.item.height = 22;
			base.item.maxStack = 999;
			item.rare = ItemRarityID.Cyan;
			base.item.value = 0;
		}


		public override void AddRecipes()
		{
			{
				ModRecipe recipe = new ModRecipe(mod);
				recipe.AddTile(ModContent.TileType<Tiles.SapphireBookcase>());
				recipe.SetResult(this, 1);
				recipe.AddRecipe();
			}
			
		}
	}

}