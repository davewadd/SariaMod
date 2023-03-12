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


	public class MediumXpPearl : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault(" MediumXp Pearl");
			Tooltip.SetDefault("Can only be used to level Saria");
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
			item.shoot = ModContent.ProjectileType<XpProjectile2>();
		}
		public override void Update(ref float gravity, ref float maxFallSpeed)
		{

			Lighting.AddLight(item.Center, Color.Blue.ToVector3() * 2f);
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
				recipe.AddIngredient(ModContent.ItemType<LargeXpPearl>(), 1);
				recipe.SetResult(this, 5);
				recipe.AddRecipe();
			}
		}
	}

}