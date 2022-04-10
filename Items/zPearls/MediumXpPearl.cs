using Microsoft.Xna.Framework;
using FairyMod.FaiPlayer;
using FairyMod.Projectiles;
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


	public class MediumXpPearl : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault(" MediumXp Pearl");
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
				recipe.AddIngredient(ModContent.ItemType<XpPearl>(), 50);
				recipe.SetResult(this, 1);
				recipe.AddRecipe();
			}
			{
				ModRecipe recipe2 = new ModRecipe(mod);
				recipe2.AddIngredient(ItemID.GoldCoin, 25);
				recipe2.SetResult(this, 1);
				recipe2.AddRecipe();
			}
			{
				ModRecipe recipe3 = new ModRecipe(mod);
				recipe3.AddIngredient(ItemID.PlatinumCoin, 1);
				recipe3.SetResult(this, 4);
				recipe3.AddRecipe();
			}
		}
	}

}