using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;
using SariaMod.Buffs;

using Terraria;

using SariaMod.Items.zPearls;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.Emerald
{
	public class LivingSilverShard : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Living Silver Shard");
			Tooltip.SetDefault("Can be sold for an insanely High price\nCan also be crafted into a Silver Gem Ball!  \n glass, 3\nIron Bar, 3\n Living Silver Shard, 4\n LargeXpPearl, 1 \n at a Strange Bookcase!");
		}


		public override void SetDefaults()
		{

			base.Item.width = 26;
			base.Item.height = 22;
			base.Item.maxStack = 999;
			Item.value = Item.buyPrice(0, 100, 0, 0);

			Item.rare = ItemRarityID.Expert;
			

		}

		public override void Update(ref float gravity, ref float maxFallSpeed)
		{

			Lighting.AddLight(Item.Center, Color.Silver.ToVector3() * 3f);
		}
	
		

		public override void AddRecipes()
		{
			{
				Recipe recipe = CreateRecipe();
				recipe.AddIngredient(ModContent.ItemType<LivingPurpleShard>(), 3);
				recipe.AddIngredient(ModContent.ItemType<LargeXpPearl>(), 1);
				recipe.AddIngredient(ItemID.SilverBar, 1);
				recipe.Register();
			}
		}
	}
}
