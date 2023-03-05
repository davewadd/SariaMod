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


	public class SariaThirdFormNote : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Saria's Ruby Form");
			Tooltip.SetDefault("~Eruption will Bomb your enemies!\n~Smaller flames will be generated after the explosion\n " + "\n " + SariaModUtilities.ColorMessage("Saria can protect you from immense heat!", new Color(0, 200, 250, 200)) + "\n" + SariaModUtilities.ColorMessage("~Snow, Glowshroom, Jungle, Dungeon, and Holy", new Color(0, 200, 250, 200)) + "\n " + "\n " + SariaModUtilities.ColorMessage("Not very effective in:", new Color(135, 206, 180)) + "\n" + SariaModUtilities.ColorMessage(" ~Ocean, Rain, Hell, Meteor, and Sandstorm", new Color(135, 206, 180)));
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