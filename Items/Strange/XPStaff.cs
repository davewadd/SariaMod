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
using SariaMod.Buffs;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.Strange
{


	public class XPStaff : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("XPStaff");
			Tooltip.SetDefault(SariaModUtilities.ColorMessage("Shows the Level of XP Saria has when used", new Color(0, 200, 250, 200)));
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
			Item.rare = ItemRarityID.Cyan;
			Item.UseSound = SoundID.Item46;

			// These below are needed for a minion weapon
			Item.noMelee = true;
			Item.DamageType = DamageClass.Summon;
			// No buffTime because otherwise the item tooltip would say something like "1 minute duration"
			Item.buffType = ModContent.BuffType<XPBuff>();
			Item.buffTime = 100000;
		}
		public override void Update(ref float gravity, ref float maxFallSpeed)
		{

			Lighting.AddLight(Item.Center, Color.SeaShell.ToVector3() * 2f);
		}



		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			// This is needed so the buff that keeps your minion alive and allows you to despawn it properly applies
			

			// Here you can change where the minion is spawned. Most vanilla minions spawn at the cursor position.
			position = Main.MouseWorld;
			return true;
		}



		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.LifeCrystal, 1);
			recipe.AddIngredient(ItemID.Wood, 5);
			recipe.AddTile(ModContent.TileType<Tiles.StrangeBookcase>());
			recipe.Register();
		}
	}

}