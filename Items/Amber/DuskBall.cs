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

namespace SariaMod.Items.Amber
{


	public class DuskBall : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("DuskBall");
			Tooltip.SetDefault(SariaModUtilities.ColorMessage("Use to catch Saria's GreenMothGoliath", new Color(135, 206, 180)));
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
			item.consumable = true;
			// No buffTime because otherwise the item tooltip would say something like "1 minute duration"
			item.shoot = ModContent.ProjectileType<DuskBallProjectile3>();
		}
		public override void Update(ref float gravity, ref float maxFallSpeed)
		{

			Lighting.AddLight(item.Center, Color.Green.ToVector3() * 2f);
		}

		

		
		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.Glass, 3);
			recipe.AddIngredient(ItemID.IronBar, 3);
			recipe.AddIngredient(ModContent.ItemType<MothFood>(), 3);
			recipe.AddIngredient(ModContent.ItemType<XpPearl>(), 3);
			recipe.AddTile(ModContent.TileType<Tiles.StrangeBookcase>());
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}

}