using Microsoft.Xna.Framework; 


using System;
using SariaMod.Items.Sapphire;
using SariaMod.Items.Ruby;
using SariaMod.Items.Topaz;
using SariaMod.Items.Emerald;
using SariaMod.Items.Amber;
using SariaMod.Items.Amethyst;
 
using SariaMod.Items.Platinum;
using SariaMod.Items.zPearls;

using SariaMod.Items.zBookcases;
using SariaMod.Items.Strange;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.Emerald
{


	public class SilverGemBall : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Silver Gem Ball");
			Tooltip.SetDefault(SariaModUtilities.ColorMessage("Calls a living Silver fairy to Heal the player!", new Color(135, 206, 180)));
			ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true; // This lets the player target anywhere on the whole screen while using a controller.
			ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
		}

		public override void SetDefaults()
		{

			Item.knockBack = 13f;
			Item.mana = 1;
			Item.width = 32;
			Item.height = 32;
			base.Item.useTime = (base.Item.useAnimation = 10);
			Item.useStyle = 1;
			Item.value = Item.buyPrice(0, 30, 0, 0);
			Item.rare = ItemRarityID.Expert;
			Item.UseSound = SoundID.Item46;
			Item.shootSpeed = 8;
			Item.noUseGraphic = true;
			// These below are needed for a minion weapon
			Item.noMelee = true;
			Item.DamageType = DamageClass.Summon;

			// No buffTime because otherwise the item tooltip would say something like "1 minute duration"
			Item.shoot = ModContent.ProjectileType<SilverBallReturn>();
		}
		public override void Update(ref float gravity, ref float maxFallSpeed)
		{

			Lighting.AddLight(Item.Center, Color.Gray.ToVector3() * 2f);
		}

		public override bool CanUseItem(Player player)
		{


			if (player.ownedProjectileCounts[ModContent.ProjectileType<SilverGemBallProjectile>()] > 0f || player.ownedProjectileCounts[ModContent.ProjectileType<SilverGemBallProjectile2>()] > 0f || player.ownedProjectileCounts[ModContent.ProjectileType<ReturnBallSilver>()] > 0f)
			{
				return false;
			}
			return true;

		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			// This is needed so the buff that keeps your minion alive and allows you to despawn it properly applies

			if (player.altFunctionUse != 2 && (player.ownedProjectileCounts[ModContent.ProjectileType<EmeraldfairySilver>()] <= 0f))
			{
				{
					Projectile.NewProjectile(Item.GetSource_FromThis(), position.X + 0, position.Y + 0, 0, 0, ModContent.ProjectileType<SilverGemBallProjectile>(), (int)(damage), 0f, player.whoAmI);
				}
			}
			else if (player.altFunctionUse != 2 && (player.ownedProjectileCounts[ModContent.ProjectileType<EmeraldfairySilver>()] > 0f))
			{
				{
					Projectile.NewProjectile(Item.GetSource_FromThis(), position.X + 0, position.Y + 0, 0, 0, ModContent.ProjectileType<SilverBallReturn>(), (int)(damage), 0f, player.whoAmI);

				}
			}
			return false;
		}







		public override void AddRecipes()
		{
			{
				Recipe recipe = CreateRecipe();
				recipe.AddIngredient(ItemID.Glass, 3);
				recipe.AddIngredient(ItemID.IronBar, 3);
				recipe.AddIngredient(ModContent.ItemType<LivingSilverShard>(), 100);
				recipe.AddIngredient(ModContent.ItemType<LargeXpPearl>(), 1);
				recipe.AddTile(ModContent.TileType<Tiles.StrangeBookcase>());
				recipe.Register();
			}
			
		}
}

}