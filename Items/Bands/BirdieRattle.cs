using Microsoft.Xna.Framework;
using SariaMod.Items.LilHarpy;
using SariaMod.Buffs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.Bands
{
	public class BirdieRattle : ModItem
	{
		public override void SetStaticDefaults() {
			// DisplayName and Tooltip are automatically set from the .lang files, but below is how it is done normally.
			// DisplayName.SetDefault("Paper Airplane");
			base.Tooltip.SetDefault("Summons a baby harpy in the pet slot\nActually helps fight!\nDamage starts at 10 but multiplies by the Player's minion count");
			// Tooltip.SetDefault("Summons a Paper Airplane to follow aimlessly behind you");
		}

		public override void SetDefaults() {
			
			Item.CloneDefaults(ItemID.ZephyrFish);
			Item.shoot = ModContent.ProjectileType<BabyHarpy>();
			Item.buffType = ModContent.BuffType<BabyHarpyBuff>();
			Item.noMelee = true;
			
		}

		public override void AddRecipes()
		{
			{
				Recipe recipe = CreateRecipe();
				recipe.AddIngredient(ItemID.Feather, 5);
				recipe.Register();
			}
		}

		public override void UseStyle(Player player, Rectangle heldItemFrame) {
			if (player.whoAmI == Main.myPlayer && player.itemTime == 0) {
				player.AddBuff(Item.buffType, 3600, true);
			}
		}
	}
}