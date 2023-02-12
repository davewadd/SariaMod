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
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.zPearls
{


	public class XpPearl : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault(" Xp Pearl");
			Tooltip.SetDefault("Can be used for crafting or leveling Saria");
		}


		public override void SetDefaults()
		{
			base.item.width = 26;
			base.item.height = 22;
			item.useTime = 36;
			item.useAnimation = 36;
			base.item.maxStack = 999;
			item.useStyle = ItemUseStyleID.HoldingOut;
			item.UseSound = SoundID.Item3;
			item.noMelee = true;
			item.summon = true;
			base.item.value = 0;
			base.item.consumable = true;
			item.rare = ItemRarityID.Expert;
			item.shoot = ModContent.ProjectileType<XpProjectile>();
		}

		public override bool CanUseItem(Player player)
		{

			if (player.HasBuff(ModContent.BuffType<SariaBuff>()))
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
			

			// Here you can change where the minion is spawned. Most vanilla minions spawn at the cursor position.
			position = Main.MouseWorld;
			return true;
		}
		public override void AddRecipes()
		{
			{
				ModRecipe recipe = new ModRecipe(mod);
				recipe.AddIngredient(ModContent.ItemType<MediumXpPearl>(), 5);
				recipe.SetResult(this, 5);
				recipe.AddRecipe();
			}
		}
	}

}