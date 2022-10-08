using Microsoft.Xna.Framework;


using System;
using SariaMod.Items.Sapphire;
using SariaMod.Items.Ruby;
using SariaMod.Items.Topaz;
using SariaMod.Items.Emerald;
using SariaMod.Items.Amber;
using SariaMod.Items.Amethyst;
using SariaMod.Items.Barrier;
using SariaMod.Items.Diamond;
using SariaMod.Items.Platinum;
using SariaMod.Items.Strange;
using SariaMod.Items.zPearls;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.Amethyst
{


	public class AmethystTome : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Amethyst Tome");
			Tooltip.SetDefault(SariaModUtilities.ColorMessage("Calls on Saria, the Champion of Foresight!", new Color(135, 206, 180)) + "\n" + SariaModUtilities.ColorMessage("Requires 10 minion slots", new Color(50, 200, 250)) + "\nUsing the tome after Saria is called\nwill change her ability\n~ShadowSneak will send a high damage shadow slithering to your enemies!\n~Saria will now Curse enemies that get near\nstealing their ability to go through walls!\n " + "\n " + SariaModUtilities.ColorMessage("Super effective in:", new Color(0, 200, 250, 200)) + "\n" + SariaModUtilities.ColorMessage("Night, Dungeon, and all evil biomes", new Color(0, 200, 250, 200)) + "\n " + "\n " + SariaModUtilities.ColorMessage("Not very effective in:", new Color(135, 206, 180)) + "\n" + SariaModUtilities.ColorMessage("~Overworld during the day", new Color(135, 206, 180)));
			ItemID.Sets.GamepadWholeScreenUseRange[item.type] = true; // This lets the player target anywhere on the whole screen while using a controller.
			ItemID.Sets.LockOnIgnoresCollision[item.type] = true;
		}

		public override void SetDefaults()
		{
			item.damage = 900;
			item.knockBack = 30f;
			item.mana = 1;
			item.width = 32;
			item.height = 32;
			item.useTime = 36;
			item.useAnimation = 15;
			item.useStyle = ItemUseStyleID.SwingThrow;
			item.value = Item.buyPrice(0, 30, 0, 0);
			item.rare = ItemRarityID.Cyan;
			item.UseSound = SoundID.Item44;

			// These below are needed for a minion weapon
			item.noMelee = true;
			item.summon = true;
			item.buffType = ModContent.BuffType<AmethystSariaBuff>();
			// No buffTime because otherwise the item tooltip would say something like "1 minute duration"
			item.shoot = ModContent.ProjectileType<AmethystSariaMinion>();
			
		}
		public override void Update(ref float gravity, ref float maxFallSpeed)
		{

			Lighting.AddLight(item.Center, Color.OrangeRed.ToVector3() * 2f);
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
			if (player.altFunctionUse != 2 && (player.ownedProjectileCounts[ModContent.ProjectileType<AmethystSariaMinion>()] > 0f))
			{
				item.UseSound = SoundID.Item46;
				item.shoot = ModContent.ProjectileType<AMSariaMinion>();
				return true;
			}


			else if (player.altFunctionUse != 2 && (player.ownedProjectileCounts[ModContent.ProjectileType<AMSariaMinion>()] > 0f))
			{
				item.UseSound = SoundID.Item46;
				item.shoot = ModContent.ProjectileType<AMSSariaMinion>();
				return true;
			}
			else if (player.altFunctionUse != 2 && (player.ownedProjectileCounts[ModContent.ProjectileType<AMSSariaMinion>()] > 0f))
			{
				item.UseSound = SoundID.Item46;
				item.shoot = ModContent.ProjectileType<AMRSariaMinion>();
				return true;
			}
			else if (player.altFunctionUse != 2 && (player.ownedProjectileCounts[ModContent.ProjectileType<AMRSariaMinion>()] > 0f))
			{
				item.UseSound = SoundID.Item46;
				item.shoot = ModContent.ProjectileType<AMTSariaMinion>();
				return true;
			}
			else if (player.altFunctionUse != 2 && (player.ownedProjectileCounts[ModContent.ProjectileType<AMTSariaMinion>()] > 0f))
			{
				item.UseSound = SoundID.Item46;
				item.shoot = ModContent.ProjectileType<AMESariaMinion>();
				return true;
			}
			else if (player.altFunctionUse != 2 && (player.ownedProjectileCounts[ModContent.ProjectileType<AMESariaMinion>()] > 0f))
			{
				item.UseSound = SoundID.Item46;
				item.shoot = ModContent.ProjectileType<AMASariaMinion>();
				return true;
			}
			else if (player.altFunctionUse != 2 && (player.ownedProjectileCounts[ModContent.ProjectileType<AMASariaMinion>()] > 0f))
			{
				item.UseSound = SoundID.Item46;
				item.shoot = ModContent.ProjectileType<AmethystSariaMinion>();
				return true;
			}


			if (player.altFunctionUse != 2 && (!player.HasBuff(ModContent.BuffType<EmeraldSariaBuff>())))
			{

				item.UseSound = SoundID.Item46;
				item.shoot = ModContent.ProjectileType<AmethystSariaMinion>();
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

				recipe.AddIngredient(ItemID.AmethystGemsparkBlock, 5);
				recipe.AddIngredient(ModContent.ItemType<LargeXpPearl>(), 100);
				recipe.AddIngredient(ModContent.ItemType<AmberTome>(), 1);
				recipe.AddTile(ModContent.TileType<Tiles.AmethystBookcase>());
				recipe.SetResult(this);
				recipe.AddRecipe();
			}
			{
				ModRecipe recipe2 = new ModRecipe(mod);

				recipe2.AddIngredient(ItemID.AmethystGemsparkBlock, 5);
				recipe2.AddIngredient(ModContent.ItemType<RareXpPearl>(), 2);
				recipe2.AddIngredient(ModContent.ItemType<AmberTome>(), 1);
				recipe2.AddTile(ModContent.TileType<Tiles.AmethystBookcase>());
				recipe2.SetResult(this);
				recipe2.AddRecipe();
			}
		}
	}

}