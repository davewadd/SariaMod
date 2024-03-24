using Microsoft.Xna.Framework;


using SariaMod.Buffs;
using System;
using SariaMod.Items.Sapphire;
using SariaMod.Items.Ruby;
using SariaMod.Items.Topaz;
using SariaMod.Items.Emerald;
using SariaMod.Items.Amber;
using SariaMod.Items.Amethyst;
 
using SariaMod.Items.Platinum;
using SariaMod.Items.Strange;
using Terraria;
using Terraria.DataStructures;
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

			Item.width = 32;
			Item.height = 32;
			Item.useTime = 36;
			Item.useAnimation = 36;
			Item.useStyle = ItemUseStyleID.Shoot;
			base.Item.width = 26;
			base.Item.height = 22;
			base.Item.maxStack = 999;
			base.Item.value = 0;
			base.Item.consumable = true;
			Item.rare = ItemRarityID.Expert;
			Item.UseSound = SoundID.Item3;
			Item.noMelee = true;
			Item.value = Item.buyPrice(0, 8, 0, 0);
			Item.DamageType = DamageClass.Summon;
			Item.buffType = ModContent.BuffType<Soothing>();
			Item.shoot = ModContent.ProjectileType<FrozenYogurtSignal>();
		}
		public override bool CanUseItem(Player player)
		{
			if (player.HasBuff(ModContent.BuffType<SariaBuff>()))
			{
				return true;
			}
			return false;
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			// This is needed so the buff that keeps your minion alive and allows you to despawn it properly applies
			player.AddBuff(Item.buffType, 44000);

			// Here you can change where the minion is spawned. Most vanilla minions spawn at the cursor position.
			position = Main.MouseWorld;
			return true;
		}

		public override void AddRecipes()
		{
			{
				Recipe recipe = CreateRecipe(2);
				recipe.AddIngredient(ModContent.ItemType<XpPearl>(), 1);
				recipe.AddIngredient(ItemID.LesserManaPotion, 3);
				recipe.AddIngredient(ItemID.SnowBlock, 5);
				recipe.Register();
			}
			{
				Recipe recipe = CreateRecipe(2);
				recipe.AddIngredient(ModContent.ItemType<LivingGreenShard>(), 1);
				recipe.AddIngredient(ItemID.LesserManaPotion, 3);
				recipe.AddIngredient(ItemID.SnowBlock, 5);
				recipe.Register();
			}
		}
	}

}