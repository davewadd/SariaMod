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
using SariaMod.Items.zPearls;

using SariaMod.Items.zBookcases;
using SariaMod.Items.Strange;
using Terraria;
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
			ItemID.Sets.GamepadWholeScreenUseRange[item.type] = true; // This lets the player target anywhere on the whole screen while using a controller.
			ItemID.Sets.LockOnIgnoresCollision[item.type] = true;
		}

		public override void SetDefaults()
		{

			item.knockBack = 13f;
			item.mana = 1;
			item.width = 32;
			item.height = 32;
			base.item.useTime = (base.item.useAnimation = 10);
			item.useStyle = 1;
			item.value = Item.buyPrice(0, 30, 0, 0);
			item.rare = ItemRarityID.Expert;
			item.UseSound = SoundID.Item46;
			item.shootSpeed = 8;
			item.noUseGraphic = true;
			// These below are needed for a minion weapon
			item.noMelee = true;
			item.summon = true;

			// No buffTime because otherwise the item tooltip would say something like "1 minute duration"
			item.shoot = ModContent.ProjectileType<SilverBallReturn>();
		}
		public override void Update(ref float gravity, ref float maxFallSpeed)
		{

			Lighting.AddLight(item.Center, Color.Gray.ToVector3() * 2f);
		}

		public override bool CanUseItem(Player player)
		{


			if (player.ownedProjectileCounts[ModContent.ProjectileType<SilverGemBallProjectile>()] > 0f || player.ownedProjectileCounts[ModContent.ProjectileType<SilverGemBallProjectile2>()] > 0f || player.ownedProjectileCounts[ModContent.ProjectileType<ReturnBallSilver>()] > 0f)
			{
				return false;
			}
			return true;

		}

		public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
		{
			// This is needed so the buff that keeps your minion alive and allows you to despawn it properly applies

			if (player.altFunctionUse != 2 && (player.ownedProjectileCounts[ModContent.ProjectileType<EmeraldfairySilver>()] <= 0f))
			{
				{
					Projectile.NewProjectile(position, new Vector2(speedX, speedY), ModContent.ProjectileType<SilverGemBallProjectile>(), damage, knockBack, player.whoAmI);
				}
			}
			else if (player.altFunctionUse != 2 && (player.ownedProjectileCounts[ModContent.ProjectileType<EmeraldfairySilver>()] > 0f))
			{
				{
					Projectile.NewProjectile(player.Center + Utils.NextVector2CircularEdge(Main.rand, 8f, 8f), Utils.NextVector2Circular(Main.rand, 12f, 12f), ModContent.ProjectileType<SilverBallReturn>(), damage, knockBack, player.whoAmI);

				}
			}
			return false;
		}







		public override void AddRecipes()
		{
			{
				ModRecipe recipe = new ModRecipe(mod);
				recipe.AddIngredient(ItemID.Glass, 3);
				recipe.AddIngredient(ItemID.IronBar, 3);
				recipe.AddIngredient(ModContent.ItemType<LivingSilverShard>(), 4);
				recipe.AddIngredient(ModContent.ItemType<LargeXpPearl>(), 1);
				recipe.AddTile(ModContent.TileType<Tiles.StrangeBookcase>());
				recipe.SetResult(this);
				recipe.AddRecipe();
			}
			
		}
}

}