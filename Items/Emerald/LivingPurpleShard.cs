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
	public class LivingPurpleShard : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Living Purple Shard");
			Tooltip.SetDefault("Can be sold for a Decent price\n");
		}


		public override void SetDefaults()
		{

			base.Item.width = 26;
			base.Item.height = 22;
			base.Item.maxStack = 999;
			Item.value = Item.buyPrice(0, 0, 500, 0);

			Item.rare = ItemRarityID.Red;
			Item.consumable = true;
			Item.useTime = 36;
			Item.useAnimation = 15;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.UseSound = SoundID.Item45;
			Item.autoReuse = false;
			// These below are needed for a minion weapon
			Item.noMelee = true;

			Item.DamageType = DamageClass.Summon;
			Item.shootSpeed = 1;
			// No buffTime because otherwise the item tooltip would say something like "1 minute duration"
			Item.shoot = ModContent.ProjectileType<PurpleRupee>();

		}
		public override bool AltFunctionUse(Player player)
		{
			return false;
		}
		

		public override bool CanUseItem(Player player)
		{
			if (player.altFunctionUse == 2)
			{
				Item.consumable = false;
				return false;
			}
			if (player.altFunctionUse != 2)
			{
				Item.consumable = true;
				return true;
			}

			else
			{
				return true;
			}
		}
		public override void Update(ref float gravity, ref float maxFallSpeed)
		{
			
			Lighting.AddLight(Item.Center, Color.Purple.ToVector3() * 2f);
		}
		
		
		public override void AddRecipes()
		{
			{
				Recipe recipe = CreateRecipe();
				recipe.AddIngredient(ModContent.ItemType<LivingGreenShard>(), 8);
				recipe.AddIngredient(ModContent.ItemType<LargeXpPearl>(), 1);
				recipe.Register();
			}
			{
				Recipe recipe = CreateRecipe(2);
				recipe.AddIngredient(ModContent.ItemType<LivingSilverShard>(), 1);
				recipe.AddIngredient(ModContent.ItemType<XpPearl>(), 5);
				recipe.Register();
			}
		}
	}
}
