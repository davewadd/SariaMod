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
			Tooltip.SetDefault("Can be sold for an insanely High price\nSummons a silver fairy to heal you slowely over time!");
		}


		public override void SetDefaults()
		{

			base.item.width = 26;
			base.item.height = 22;
			base.item.maxStack = 999;
			item.value = Item.buyPrice(50, 0, 0, 0);

			item.rare = ItemRarityID.Expert;
			item.consumable = true;
			item.useTime = 36;
			item.useAnimation = 15;
			item.useStyle = ItemUseStyleID.SwingThrow;
			item.UseSound = SoundID.Item45;
			item.autoReuse = false;
			// These below are needed for a minion weapon
			item.noMelee = true;
			item.buffType = ModContent.BuffType<EmeraldFairySilverBuff>();
			item.summon = true;
			// No buffTime because otherwise the item tooltip would say something like "1 minute duration"
			item.shoot = ModContent.ProjectileType<EmeraldfairySilver>();

		}
		public override bool AltFunctionUse(Player player)
		{
			return false;
		}
		public override void Update(ref float gravity, ref float maxFallSpeed)
		{

			Lighting.AddLight(item.Center, Color.Silver.ToVector3() * 3f);
		}
		public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
		{

			// This is needed so the buff that keeps your minion alive and allows you to despawn it properly applies
			player.AddBuff(item.buffType, 2);

			// Here you can change where the minion is spawned. Most vanilla minions spawn at the cursor position.


			position = Main.MouseWorld;


			return true;
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
			}
			if (player.ownedProjectileCounts[ModContent.ProjectileType<EmeraldfairySilver>()] > 0f)
			{
				return false;
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
				recipe.AddIngredient(ModContent.ItemType<LivingPurpleShard>(), 3);
				recipe.AddIngredient(ModContent.ItemType<LargeXpPearl>(), 1);
				recipe.AddIngredient(ItemID.SilverBar, 1);
				recipe.SetResult(this);
				recipe.AddRecipe();
			}
		}
	}
}
