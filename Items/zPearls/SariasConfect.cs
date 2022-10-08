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


	public class SariasConfect : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Saria's Confect");
			Tooltip.SetDefault("Frozen Yogurt mixed with pure crystalized magic!\nCauses Saria to enter a supercharged state\ngiving all attacks an added effect!");
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
			item.buffType = ModContent.BuffType<Overcharged>();
			item.shoot = ModContent.ProjectileType<Competitivetime>();

		}
		public override bool CanUseItem(Player player)
		{
			
			if (player.HasBuff(ModContent.BuffType<SariaBuff>())&& (!player.HasBuff(ModContent.BuffType<Drained>())) && (!player.HasBuff(ModContent.BuffType<Sickness>())))
			{
				return true;
			}
			if (player.HasBuff(ModContent.BuffType<SapphireSariaBuff>()) && (!player.HasBuff(ModContent.BuffType<Drained>())) && (!player.HasBuff(ModContent.BuffType<Sickness>())))
			{
				return true;
			}
			if (player.HasBuff(ModContent.BuffType<RubySariaBuff>()) && (!player.HasBuff(ModContent.BuffType<Drained>())) && (!player.HasBuff(ModContent.BuffType<Sickness>())))
			{
				return true;
			}
			if (player.HasBuff(ModContent.BuffType<TopazSariaBuff>()) && (!player.HasBuff(ModContent.BuffType<Drained>())) && (!player.HasBuff(ModContent.BuffType<Sickness>())))
			{
				return true;
			}
			if (player.HasBuff(ModContent.BuffType<EmeraldSariaBuff>()) && (!player.HasBuff(ModContent.BuffType<Drained>())) && (!player.HasBuff(ModContent.BuffType<Sickness>())))
			{
				return true;
			}
			if (player.HasBuff(ModContent.BuffType<AmberSariaBuff>()) && (!player.HasBuff(ModContent.BuffType<Drained>())) && (!player.HasBuff(ModContent.BuffType<Sickness>())))
			{
				return true;
			}
			if (player.HasBuff(ModContent.BuffType<AmethystSariaBuff>()) && (!player.HasBuff(ModContent.BuffType<Drained>())) && (!player.HasBuff(ModContent.BuffType<Sickness>())))
			{
				return true;
			}
			if (player.HasBuff(ModContent.BuffType<DiamondSariaBuff>()) && (!player.HasBuff(ModContent.BuffType<Drained>())) && (!player.HasBuff(ModContent.BuffType<Sickness>())))
			{
				return true;
			}
			else
			{

				return false;
			}
		}
		public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
		{
			// This is needed so the buff that keeps your minion alive and allows you to despawn it properly applies
			player.AddBuff(item.buffType, 15000);

			// Here you can change where the minion is spawned. Most vanilla minions spawn at the cursor position.
			position = Main.MouseWorld;
			return true;
		}

		public override void AddRecipes()
		{
			{
				ModRecipe recipe = new ModRecipe(mod);
				recipe.AddIngredient(ModContent.ItemType<LargeXpPearl>(), 2);
				recipe.AddIngredient(ModContent.ItemType<FrozenYogurt>(), 1);
				recipe.AddIngredient(ItemID.SuperManaPotion, 3);
				recipe.AddIngredient(ItemID.SnowBlock, 5);
				recipe.SetResult(this);
				recipe.AddRecipe();
			}
		}
	}

}