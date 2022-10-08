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
using SariaMod.Buffs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.zPearls
{


	public class SoaringConcoction : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Soaring Concoction");
			Tooltip.SetDefault("Frozen Yogurt mixed with Rare magic!\nheavily increases flight time\n-Don't fly too close to the sun");
		}


		public override void SetDefaults()
		{

			item.width = 32;
			item.height = 32;
			item.useTime = 36;
			item.useAnimation = 36;
			item.useStyle = ItemUseStyleID.HoldingOut;
			base.item.width = 26;
			base.item.height = 22;
			base.item.maxStack = 999;
			base.item.value = 0;
			base.item.consumable = true;
			item.rare = ItemRarityID.Expert;
			item.UseSound = SoundID.Item3;
			item.noMelee = true;
			item.summon = true;
			item.buffType = ModContent.BuffType<AerialAceBuff>();
			item.buffTime = 10000;
		}
		public override bool CanUseItem(Player player)
		{

			return true;
		}
		

		public override void AddRecipes()
		{
			{
				ModRecipe recipe = new ModRecipe(mod);
				recipe.AddIngredient(ModContent.ItemType<RareXpPearl>(), 1);
				recipe.AddIngredient(ModContent.ItemType<FrozenYogurt>(), 1);
				recipe.AddIngredient(ItemID.SuperManaPotion, 3);
				recipe.AddIngredient(ItemID.SnowBlock, 5);
				recipe.SetResult(this);
				recipe.AddRecipe();
			}
		}
	}

}