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
using SariaMod.Items.zBookcases;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.zPearls
{


	public class BlankOcarina : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Blank Ocarina");
			Tooltip.SetDefault("Try using it in different places to learn powerful songs");
		}


		public override void SetDefaults()
		{
			base.item.width = 26;
			base.item.height = 22;
			base.item.maxStack = 1;
			item.useTime = 36;
			item.useStyle = ItemUseStyleID.HoldingOut;
			base.item.consumable = true;
			item.shoot = ModContent.ProjectileType<EmptyNote>();
			item.UseSound = base.mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/SongCorrect");
		}
		

		public override void AddRecipes()
		{
			{
				ModRecipe recipe = new ModRecipe(mod);
				recipe.AddIngredient(ModContent.ItemType<ForestOcarina>(), 1);
				recipe.SetResult(this, 1);
				recipe.AddRecipe();
			}
			{
				ModRecipe recipe = new ModRecipe(mod);
				recipe.AddIngredient(ItemID.Wood, 12);
				recipe.AddIngredient(ItemID.ManaCrystal, 1);
				recipe.SetResult(this, 1);
				recipe.AddRecipe();
			}
			{
				ModRecipe recipe = new ModRecipe(mod);
				recipe.AddIngredient(ModContent.ItemType<TimeOcarina>(), 1);
				recipe.SetResult(this, 1);
				recipe.AddRecipe();
			}
			{
				ModRecipe recipe = new ModRecipe(mod);
				recipe.AddIngredient(ModContent.ItemType<RainOcarina>(), 1);
				recipe.SetResult(this, 1);
				recipe.AddRecipe();
			}
			{
				ModRecipe recipe = new ModRecipe(mod);
				recipe.AddIngredient(ModContent.ItemType<SandOcarina>(), 1);
				recipe.SetResult(this, 1);
				recipe.AddRecipe();
			}
			{
				ModRecipe recipe = new ModRecipe(mod);
				recipe.AddIngredient(ModContent.ItemType<BloodOcarina>(), 1);
				recipe.SetResult(this, 1);
				recipe.AddRecipe();
			}
			{
				ModRecipe recipe = new ModRecipe(mod);
				recipe.AddIngredient(ModContent.ItemType<EclipseOcarina>(), 1);
				recipe.SetResult(this, 1);
				recipe.AddRecipe();
			}
			{
				ModRecipe recipe = new ModRecipe(mod);
				recipe.AddIngredient(ModContent.ItemType<ForestOcarina>(), 1);
				recipe.SetResult(this, 1);
				recipe.AddRecipe();
			}
		}
	}

}