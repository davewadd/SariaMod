using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;
using SariaMod.Buffs;

using Terraria;
using SariaMod.Items.zPearls;

using SariaMod.Items.LilHarpy;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.Amber
{
	public class MothFood : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Ancient BloodMoth food");
			Tooltip.SetDefault("Food Ancient Moths cannot resist!\nConsumable");
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
			item.shoot = ModContent.ProjectileType<Mothdust>();

		}
		public override bool AltFunctionUse(Player player)
		{
			return false;
		}
		public override void Update(ref float gravity, ref float maxFallSpeed)
		{
			
			Lighting.AddLight(item.Center, Color.OrangeRed.ToVector3() * 2f);
		}
		
		public override bool CanUseItem(Player player)
		{ 
			if (player.altFunctionUse ==2)
            {
				item.consumable = false;
				return false;
            }
			if (player.altFunctionUse !=2)
            {
				item.consumable = true;
				return true;
            }
			
			else
			{
				return true;
			}
            }
		public override void AddRecipes()
		{
			{
				ModRecipe recipe = new ModRecipe(mod);
				recipe.AddIngredient(ModContent.ItemType<LargeXpPearl>(), 2);
				recipe.AddIngredient(ItemID.TissueSample, 1);
				recipe.SetResult(this, 1);
				recipe.AddRecipe();
			}
			{
				ModRecipe recipe = new ModRecipe(mod);
				recipe.AddIngredient(ModContent.ItemType<LargeXpPearl>(), 2);
				recipe.AddIngredient(ItemID.ShadowScale, 1);
				recipe.SetResult(this, 1);
				recipe.AddRecipe();
			}
		}
	}
	
}
