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


	public class CraftingRecipes : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault(" New Recipes ");
			Tooltip.SetDefault("");
		}




		public override void AddRecipes()
		{
			{
				ModRecipe recipe = new ModRecipe(mod);
				recipe.AddIngredient(ItemID.BottledWater, 1);
				recipe.AddIngredient(ModContent.ItemType<XpPearl>(), 1);
				recipe.AddTile(TileID.CookingPots);
				recipe.SetResult(ItemID.SummoningPotion, 1);
				recipe.AddRecipe();
			}
			{
				ModRecipe recipe = new ModRecipe(mod);
				recipe.AddIngredient(ItemID.BottledWater, 1);
				recipe.AddIngredient(ModContent.ItemType<XpPearl>(), 1);
				recipe.AddTile(TileID.CookingPots);
				recipe.SetResult(ItemID.WrathPotion, 1);
				recipe.AddRecipe();
			}
			{
				ModRecipe recipe = new ModRecipe(mod);
				recipe.AddIngredient(ItemID.BottledWater, 1);
				recipe.AddIngredient(ModContent.ItemType<XpPearl>(), 1);
				recipe.AddTile(TileID.CookingPots);
				recipe.SetResult(ItemID.WormholePotion, 1);
				recipe.AddRecipe();
			}
			{
				ModRecipe recipe = new ModRecipe(mod);
				recipe.AddIngredient(ItemID.TurtleShell, 1);
				recipe.AddIngredient(ItemID.IceBlock, 50);
				recipe.AddTile(TileID.IceMachine);
				recipe.SetResult(ItemID.FrozenTurtleShell, 1);
				recipe.AddRecipe();
			}
			{
				ModRecipe recipe = new ModRecipe(mod);
				recipe.AddIngredient(ItemID.BottledWater, 1);
				recipe.AddIngredient(ItemID.Feather, 1);
				recipe.AddIngredient(ModContent.ItemType<XpPearl>(), 2);
				recipe.AddTile(TileID.CookingPots);
				recipe.SetResult(ItemID.FeatherfallPotion, 1);
				recipe.AddRecipe();
			}
			{
				ModRecipe recipe = new ModRecipe(mod);
				recipe.AddIngredient(ItemID.BottledWater, 1);
				recipe.AddIngredient(ModContent.ItemType<XpPearl>(), 2);
				recipe.AddTile(TileID.CookingPots);
				recipe.SetResult(ItemID.HunterPotion, 1);
				recipe.AddRecipe();
			}
			{
				ModRecipe recipe = new ModRecipe(mod);
				recipe.AddIngredient(ItemID.BottledWater, 1);
				recipe.AddIngredient(ModContent.ItemType<XpPearl>(), 1);
				recipe.AddTile(TileID.CookingPots);
				recipe.SetResult(ItemID.EndurancePotion, 1);
				recipe.AddRecipe();
			}
			{
				ModRecipe recipe = new ModRecipe(mod);
				recipe.AddIngredient(ItemID.StoneBlock, 20);
				recipe.AddIngredient(ItemID.LifeCrystal, 1);
				recipe.AddTile(TileID.WorkBenches);
				recipe.SetResult(ItemID.HeartStatue, 1);
				recipe.AddRecipe();
			}
			{
				ModRecipe recipe = new ModRecipe(mod);
				recipe.AddIngredient(ItemID.BottledWater, 1);
				recipe.AddIngredient(ModContent.ItemType<XpPearl>(), 1);
				recipe.AddTile(TileID.CookingPots);
				recipe.SetResult(ItemID.BattlePotion, 1);
				recipe.AddRecipe();
			}
			{
				ModRecipe recipe = new ModRecipe(mod);
				recipe.AddIngredient(ItemID.Torch, 1);
				recipe.AddIngredient(ItemID.Sapphire, 1);
				recipe.SetResult(ItemID.WaterCandle, 1);
				recipe.AddRecipe();
			}
			{
				ModRecipe recipe4 = new ModRecipe(mod);
				recipe4.AddIngredient(ItemID.LesserHealingPotion, 3);
				recipe4.AddTile(TileID.CookingPots);
				recipe4.SetResult(ItemID.HealingPotion, 1);
				recipe4.AddRecipe();
			}
			{
				ModRecipe recipe5 = new ModRecipe(mod);
				recipe5.AddIngredient(ItemID.HealingPotion, 4);
				recipe5.AddTile(TileID.CookingPots);
				recipe5.SetResult(ItemID.GreaterHealingPotion, 1);
				recipe5.AddRecipe();
			}
			{
				ModRecipe recipe6 = new ModRecipe(mod);
				recipe6.AddIngredient(ItemID.GreaterHealingPotion, 2);
				recipe6.AddTile(TileID.CookingPots);
				recipe6.SetResult(ItemID.SuperHealingPotion, 1);
				recipe6.AddRecipe();
			}
			{
				ModRecipe recipe7 = new ModRecipe(mod);
				recipe7.AddIngredient(ItemID.Sapphire, 3);
				recipe7.AddIngredient(ItemID.Book, 1);
				recipe7.AddTile(TileID.Bookcases);
				recipe7.SetResult(ItemID.WaterBolt, 1);
				recipe7.AddRecipe();
			}
			{
				ModRecipe recipe8 = new ModRecipe(mod);
				recipe8.AddIngredient(ItemID.Wood, 1);
				recipe8.SetResult(ItemID.BorealWood, 1);
				recipe8.AddTile(TileID.WorkBenches);
				recipe8.AddRecipe();
			}
			{
				ModRecipe recipe8 = new ModRecipe(mod);
				recipe8.AddIngredient(ItemID.BorealWood, 1);
				recipe8.AddTile(TileID.WorkBenches);
				recipe8.SetResult(ItemID.Wood, 1);
				recipe8.AddRecipe();
			}
			{
				ModRecipe recipe8 = new ModRecipe(mod);
				recipe8.AddIngredient(ItemID.BorealWood, 1);
				recipe8.AddTile(TileID.WorkBenches);
				recipe8.SetResult(ItemID.Ebonwood, 1);
				recipe8.AddRecipe();
			}
			{
				ModRecipe recipe8 = new ModRecipe(mod);
				recipe8.AddIngredient(ItemID.BorealWood, 1);
				recipe8.AddTile(TileID.WorkBenches);
				recipe8.SetResult(ItemID.RichMahogany, 1);
				recipe8.AddRecipe();
			}
			{
				ModRecipe recipe8 = new ModRecipe(mod);
				recipe8.AddIngredient(ItemID.Mushroom, 1);
				recipe8.AddTile(TileID.WorkBenches);
				recipe8.SetResult(ItemID.BorealWood, 1);
				recipe8.AddRecipe();
			}
			{
				ModRecipe recipe8 = new ModRecipe(mod);
				recipe8.AddIngredient(ItemID.GlowingMushroom, 1);
				recipe8.AddTile(TileID.WorkBenches);
				recipe8.SetResult(ItemID.BorealWood, 1);
				recipe8.AddRecipe();
			}
			{
				ModRecipe recipe8 = new ModRecipe(mod);
				recipe8.AddIngredient(ItemID.DirtBlock, 1);
				recipe8.AddTile(TileID.Sand);
				recipe8.SetResult(ItemID.SandBlock, 1);
				recipe8.AddRecipe();
			}
			{
				ModRecipe recipe8 = new ModRecipe(mod);
				recipe8.AddIngredient(ItemID.DirtBlock, 1);
				recipe8.AddTile(TileID.SnowBlock);
				recipe8.SetResult(ItemID.SnowBlock, 1);
				recipe8.AddRecipe();
			}
			{
				ModRecipe recipe8 = new ModRecipe(mod);
				recipe8.AddIngredient(ItemID.SnowBlock, 1);
				recipe8.AddTile(TileID.SnowBlock);
				recipe8.SetResult(ItemID.IceBlock, 1);
				recipe8.AddRecipe();
			}
			{
				ModRecipe recipe8 = new ModRecipe(mod);
				recipe8.AddIngredient(ItemID.DirtBlock, 1);
				recipe8.AddTile(TileID.Cloud);
				recipe8.SetResult(ItemID.Cloud, 1);
				recipe8.AddRecipe();
			}
		}
	}

}