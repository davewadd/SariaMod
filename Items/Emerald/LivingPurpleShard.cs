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
			Tooltip.SetDefault("Can be sold for a High price\n Can be crafted into a Purple Gem Ball! \n glass, 3\nIron Bar, 3\n Living Purple Shard, 8\n LargeXpPearl, 1 \n at a Strange Bookcase!\nWhen used, the gem will heal the pink fairy's shield!");
		}


		public override void SetDefaults()
		{

			base.item.width = 26;
			base.item.height = 22;
			base.item.maxStack = 999;
			item.value = Item.buyPrice(20, 0, 0, 0);

			item.rare = ItemRarityID.Red;
			item.consumable = true;
			item.useTime = 36;
			item.useAnimation = 15;
			item.useStyle = ItemUseStyleID.SwingThrow;
			item.UseSound = SoundID.Item45;
			item.autoReuse = false;
			// These below are needed for a minion weapon
			item.noMelee = true;

			item.summon = true;
			item.shootSpeed = 1;
			// No buffTime because otherwise the item tooltip would say something like "1 minute duration"
			item.shoot = ModContent.ProjectileType<PurpleRupee>();

		}
		public override bool AltFunctionUse(Player player)
		{
			return false;
		}
		

		public override bool CanUseItem(Player player)
		{
			if (player.altFunctionUse == 2)
			{
				item.consumable = false;
				return false;
			}
			if (player.altFunctionUse != 2)
			{
				item.consumable = true;
				return true;
			}

			else
			{
				return true;
			}
		}
		public override void Update(ref float gravity, ref float maxFallSpeed)
		{
			
			Lighting.AddLight(item.Center, Color.Purple.ToVector3() * 2f);
		}
		
		
		public override void AddRecipes()
		{
			{
				ModRecipe recipe = new ModRecipe(mod);
				recipe.AddIngredient(ModContent.ItemType<LivingGreenShard>(), 8);
				recipe.AddIngredient(ModContent.ItemType<LargeXpPearl>(), 1);
				recipe.SetResult(this);
				recipe.AddRecipe();
			}

		}
	}
}
