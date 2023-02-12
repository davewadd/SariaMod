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

namespace SariaMod.Items.zBookcases
{


	public class SariaFirstFormNote : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Saria's First Form");
			Tooltip.SetDefault("~Psyshock will pelt your enemies!\n~Enemies hit will slowely rise!\n " + "\n " + SariaModUtilities.ColorMessage("Psychic powers keep you from taking fall damage!", new Color(0, 200, 250, 200)) + "\n" + SariaModUtilities.ColorMessage("~Space, Jungle, Glowshroom", new Color(0, 200, 250, 200)) + "\n " + "\n " + SariaModUtilities.ColorMessage("Not very effective in:", new Color(135, 206, 180)) + "\n" + SariaModUtilities.ColorMessage("~all evil biomes", new Color(135, 206, 180)));
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