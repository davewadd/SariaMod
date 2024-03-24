using Microsoft.Xna.Framework; 



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


	public class XpPearl : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault(" Xp Pearl");
			Tooltip.SetDefault("Can be used for crafting or leveling Saria");
		}


		public override void SetDefaults()
		{
			base.Item.width = 26;
			base.Item.height = 22;
			Item.useTime = 36;
			Item.useAnimation = 36;
			base.Item.maxStack = 999;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.UseSound = SoundID.Item3;
			Item.noMelee = true;
			Item.DamageType = DamageClass.Summon;
			Item.value = Item.buyPrice(0, 4, 0, 0);
			base.Item.consumable = true;
			Item.rare = ItemRarityID.Expert;
			Item.shoot = ModContent.ProjectileType<XpProjectile>();
		}
		public override void Update(ref float gravity, ref float maxFallSpeed)
		{

			Lighting.AddLight(Item.Center, Color.FloralWhite.ToVector3() * 2f);
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
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			

			// Here you can change where the minion is spawned. Most vanilla minions spawn at the cursor position.
			position = Main.MouseWorld;
			return true;
		}
		public override void AddRecipes()
		{
			{
				Recipe recipe = CreateRecipe(5);
				recipe.AddIngredient(ModContent.ItemType<MediumXpPearl>(), 1);
				recipe.Register();
			}
		}
	}

}