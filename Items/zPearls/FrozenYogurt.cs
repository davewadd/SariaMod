using Microsoft.Xna.Framework;


using SariaMod.Buffs;
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


	public class FrozenYogurt : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Frozen Yogurt");
			Tooltip.SetDefault("Saria's favorite food!\nWhen used, will cure Saria of sickness");
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
			item.buffType = ModContent.BuffType<Soothing>();
			item.shoot = ModContent.ProjectileType<FrozenYogurtSignal>();
		}
		public override bool CanUseItem(Player player)
		{
			if (player.HasBuff(ModContent.BuffType<SariaBuff>()))
			{
				return true;
			}
			return false;
		}
		public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
		{
			// This is needed so the buff that keeps your minion alive and allows you to despawn it properly applies
			player.AddBuff(item.buffType, 44000);

			// Here you can change where the minion is spawned. Most vanilla minions spawn at the cursor position.
			position = Main.MouseWorld;
			return true;
		}

		public override void AddRecipes()
		{
			{
				ModRecipe recipe = new ModRecipe(mod);
				recipe.AddIngredient(ModContent.ItemType<XpPearl>(), 1);
				recipe.AddIngredient(ItemID.LesserManaPotion, 3);
				recipe.AddIngredient(ItemID.SnowBlock, 5);
				
				recipe.SetResult(this, 2);
				recipe.AddRecipe();
			}
			{
				ModRecipe recipe = new ModRecipe(mod);
				recipe.AddIngredient(ModContent.ItemType<LivingGreenShard>(), 1);
				recipe.AddIngredient(ItemID.LesserManaPotion, 3);
				recipe.AddIngredient(ItemID.SnowBlock, 5);

				recipe.SetResult(this, 2);
				recipe.AddRecipe();
			}
		}
	}

}