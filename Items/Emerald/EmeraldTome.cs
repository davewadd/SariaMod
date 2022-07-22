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
using SariaMod.Items.zPearls;
using SariaMod.Items.Barrier;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.Emerald
{


	public class EmeraldTome : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Emerald Tome");
			Tooltip.SetDefault(SariaModUtilities.ColorMessage("Calls on Saria, the Champion of Foresight!", new Color(135, 206, 180)) + "\n" + SariaModUtilities.ColorMessage("Requires 7 minion slots", new Color(50, 200, 250)) + "\nUsing the tome after Saria is called\nwill change her ability\n~GemStorm will cause Saria to place a gem cluster under your enemies!\n~Gem Clusters will continue to damage enemies after being created!\n~Rupees may also break from clusters upon hitting enemies\nwhich can be collected and used.\n " + "\n " + SariaModUtilities.ColorMessage("Super effective in:", new Color(0, 200, 250, 200)) + "\n" + SariaModUtilities.ColorMessage("~Underground", new Color(0, 200, 250, 200)) + "\n " + "\n " + SariaModUtilities.ColorMessage("Not very effective in:", new Color(135, 206, 180)) + "\n" + SariaModUtilities.ColorMessage("~Space, and Ocean", new Color(135, 206, 180)));
			ItemID.Sets.GamepadWholeScreenUseRange[item.type] = true; // This lets the player target anywhere on the whole screen while using a controller.
			ItemID.Sets.LockOnIgnoresCollision[item.type] = true;
		}

		public override void SetDefaults()
		{
			item.damage = 70;
			
			item.mana = 1;
			item.width = 32;
			item.height = 32;
			item.useTime = 36;
			item.useAnimation = 36;
			item.useStyle = ItemUseStyleID.SwingThrow;
			item.value = Item.buyPrice(0, 30, 0, 0);
			item.rare = ItemRarityID.Cyan;
			item.UseSound = SoundID.Item44;
			item.knockBack = 20f;

			// These below are needed for a minion weapon
			item.noMelee = true;
			item.summon = true;
			item.buffType = ModContent.BuffType<EmeraldSariaBuff>();
			
			// No buffTime because otherwise the item tooltip would say something like "1 minute duration"
			item.shoot = ModContent.ProjectileType<EmeraldSariaMinion>();

		}
		public override void Update(ref float gravity, ref float maxFallSpeed)
		{

			Lighting.AddLight(item.Center, Color.LimeGreen.ToVector3() * 2f);
		}
		public override bool AltFunctionUse(Player player)
		{
			return true;
		}
		public override bool CanUseItem(Player player)
		{
			if (player.altFunctionUse != 2)
			{
				if ((player.ownedProjectileCounts[ModContent.ProjectileType<Nerf>()] > 0f))
				{
					item.useTime = 250;

				}
				if (player.ownedProjectileCounts[ModContent.ProjectileType<Nerf>()] <= 0f)
				{
					item.useTime = 36;

				}
			}
			if (player.altFunctionUse == 2)
			{
				item.useTime = 36;
			}
			if (player.altFunctionUse != 2 && (player.ownedProjectileCounts[ModContent.ProjectileType<EmeraldSariaMinion>()] > 0f))
			{
				item.UseSound = SoundID.Item46;
				item.shoot = ModContent.ProjectileType<ESariaMinion>();
				return true;
			}


			else if (player.altFunctionUse != 2 && (player.ownedProjectileCounts[ModContent.ProjectileType<ESariaMinion>()] > 0f))
			{
				item.UseSound = SoundID.Item46;
				item.shoot = ModContent.ProjectileType<ESSariaMinion>();
				return true;
			}
			else if (player.altFunctionUse != 2 && (player.ownedProjectileCounts[ModContent.ProjectileType<ESSariaMinion>()] > 0f))
			{
				item.UseSound = SoundID.Item46;
				item.shoot = ModContent.ProjectileType<ERSariaMinion>();
				return true;
			}
			else if (player.altFunctionUse != 2 && (player.ownedProjectileCounts[ModContent.ProjectileType<ERSariaMinion>()] > 0f))
			{
				item.UseSound = SoundID.Item46;
				item.shoot = ModContent.ProjectileType<ETSariaMinion>();
				return true;
			}
			else if (player.altFunctionUse != 2 && (player.ownedProjectileCounts[ModContent.ProjectileType<ETSariaMinion>()] > 0f))
			{
				item.UseSound = SoundID.Item46;
				item.shoot = ModContent.ProjectileType<EmeraldSariaMinion>();
				return true;
			}


			if (player.altFunctionUse != 2 && (!player.HasBuff(ModContent.BuffType<EmeraldSariaBuff>())))
			{

				item.UseSound = SoundID.Item46;
				item.shoot = ModContent.ProjectileType<EmeraldSariaMinion>();
				if (player.HasBuff(ModContent.BuffType<SariaBuff>()))
				{
					return false;
				}
				if (player.HasBuff(ModContent.BuffType<SapphireSariaBuff>()))
				{
					return false;
				}
				if (player.HasBuff(ModContent.BuffType<RubySariaBuff>()))
				{
					return false;
				}
				if (player.HasBuff(ModContent.BuffType<TopazSariaBuff>()))
				{
					return false;
				}
				if (player.HasBuff(ModContent.BuffType<EmeraldSariaBuff>()))
				{
					return false;
				}
				if (player.HasBuff(ModContent.BuffType<AmberSariaBuff>()))
				{
					return false;
				}
				if (player.HasBuff(ModContent.BuffType<AmethystSariaBuff>()))
				{
					return false;
				}
				if (player.HasBuff(ModContent.BuffType<DiamondSariaBuff>()))
				{
					return false;
				}
				if (player.HasBuff(ModContent.BuffType<PlatinumSariaBuff>()))
				{
					return false;
				}
				if (player.HasBuff(ModContent.BuffType<PlatinumPurpleSariaBuff>()))
				{
					return false;
				}
				if (player.HasBuff(ModContent.BuffType<PlatinumBlueSariaBuff>()))
				{
					return false;
				}
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
			player.AddBuff(item.buffType, 30000);
			
				// Here you can change where the minion is spawned. Most vanilla minions spawn at the cursor position.


				position = Main.MouseWorld;
				
			
			return true;
		}



		public override void AddRecipes()
		{
			{
				ModRecipe recipe = new ModRecipe(mod);

				recipe.AddIngredient(ItemID.Emerald, 5);
				recipe.AddIngredient(ModContent.ItemType<TopazTome>(), 1);
				recipe.AddIngredient(ModContent.ItemType<MediumXpPearl>(), 200);
				recipe.AddTile(ModContent.TileType<Tiles.EmeraldBookcase>());
				recipe.SetResult(this);
				recipe.AddRecipe();
			}
			{
				ModRecipe recipe2 = new ModRecipe(base.mod);
				recipe2.AddIngredient(ItemID.Emerald, 5);
				recipe2.AddIngredient(ModContent.ItemType<TopazTome>(), 1);
				recipe2.AddIngredient(ModContent.ItemType<LargeXpPearl>(), 4);
				recipe2.AddTile(ModContent.TileType<Tiles.EmeraldBookcase>());
				recipe2.SetResult(this);
				recipe2.AddRecipe();
			}
		}
	}
}