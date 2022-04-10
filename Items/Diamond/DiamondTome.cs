using Microsoft.Xna.Framework;
using FairyMod.FaiPlayer;
using FairyMod.Projectiles;
using System;
using SariaMod.Items.Sapphire;
using SariaMod.Items.Ruby;
using SariaMod.Items.Topaz;
using SariaMod.Items.Emerald;
using SariaMod.Items.Amber;
using SariaMod.Items.Amethyst;
using SariaMod.Items.Diamond;
using SariaMod.Items.Barrier;
using SariaMod.Items.Platinum;
using SariaMod.Items.Strange;
using SariaMod.Items.zPearls;
using SariaMod.Items.Playerattack;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.Diamond
{


	public class DiamondTome : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Diamond Tome");
			Tooltip.SetDefault("Summons Saria in her strongest form!\nUsing the tome again will set a sentry\nRequires 10 minion slots\nUsing the tome after Saria is called\nwill cause her to place a Psychic Barrier");
			ItemID.Sets.GamepadWholeScreenUseRange[item.type] = true; // This lets the player target anywhere on the whole screen while using a controller.
			ItemID.Sets.LockOnIgnoresCollision[item.type] = true;
		}

		public override void SetDefaults()
		{
			item.damage = 1000;
			item.knockBack = 30f;
			item.mana = 1;
			item.width = 32;
			item.height = 32;
			item.useTime = 36;
			item.useAnimation = 36;
			item.useStyle = ItemUseStyleID.SwingThrow;
			item.value = Item.buyPrice(0, 30, 0, 0);
			item.rare = ItemRarityID.Cyan;
			item.UseSound = SoundID.Item44;

			// These below are needed for a minion weapon
			item.noMelee = true;
			item.summon = true;
			item.buffType = ModContent.BuffType<DiamondSariaBuff>();
			// No buffTime because otherwise the item tooltip would say something like "1 minute duration"
			item.shoot = ModContent.ProjectileType<DiamondSariaMinion>();
			
		}
		public override bool AltFunctionUse(Player player)
		{
			return true;
		}
		public override bool CanUseItem(Player player)
		{
			if (player.altFunctionUse == 2 && (player.ownedProjectileCounts[ModContent.ProjectileType<DiamondSariaMinion>()] > 0f) && (player.ownedProjectileCounts[ModContent.ProjectileType<BarrierMinion>()] <= 1))
			{
				SariaModUtilities.KillShootProjectile(player, ModContent.ProjectileType<BarrierMinion>());

				item.UseSound = base.mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Barrier");
				item.shoot = ModContent.ProjectileType<BarrierMinion>();
				return true;
			}
			else if (player.altFunctionUse != 2 && (player.ownedProjectileCounts[ModContent.ProjectileType<DiamondSariaMinion>()] > 0f) && (player.ownedProjectileCounts[ModContent.ProjectileType<Psybeam>()] < 6f) && (player.ownedProjectileCounts[ModContent.ProjectileType<Psybeam>()] >= 3f))
			{
								item.UseSound = SoundID.Item46;
				item.shoot = ModContent.ProjectileType<Psybeam>();
				return true;
			}

			else if (player.altFunctionUse != 2 && (player.ownedProjectileCounts[ModContent.ProjectileType<DiamondSariaMinion>()] > 0f) && (player.ownedProjectileCounts[ModContent.ProjectileType<Psybeam>()] < 3f))
			{
				item.UseSound = SoundID.Item43;
				item.shoot = ModContent.ProjectileType<Psybeam>();
				return true;
			}

			else if (player.altFunctionUse != 2 && (player.ownedProjectileCounts[ModContent.ProjectileType<DiamondSariaMinion>()] <= 0f))
			{

				item.UseSound = SoundID.Item44;
				item.shoot = ModContent.ProjectileType<DiamondSariaMinion>();
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
				player.AddBuff(item.buffType, 2);
			
			// Here you can change where the minion is spawned. Most vanilla minions spawn at the cursor position.
			position = Main.MouseWorld;
				return true;
			}

		

		public override void AddRecipes()
		{
			{
				ModRecipe recipe = new ModRecipe(mod);

				recipe.AddIngredient(ItemID.DiamondGemsparkBlock, 5);
				recipe.AddIngredient(ModContent.ItemType<LargeXpPearl>(), 500);
				recipe.AddIngredient(ModContent.ItemType<StrangeTome>(), 1);
				recipe.AddIngredient(ModContent.ItemType<SapphireTome>(), 1);
				recipe.AddIngredient(ModContent.ItemType<RubyTome>(), 1);
				recipe.AddIngredient(ModContent.ItemType<TopazTome>(), 1);
				recipe.AddIngredient(ModContent.ItemType<EmeraldTome>(), 1);
				recipe.AddIngredient(ModContent.ItemType<AmberTome>(), 1);
				recipe.AddIngredient(ModContent.ItemType<AmethystTome>(), 1);
				recipe.AddTile(ModContent.TileType<Tiles.DiamondBookcase>());
				recipe.SetResult(this);
				recipe.AddRecipe();
			}
			{
				ModRecipe recipe2 = new ModRecipe(mod);

				recipe2.AddIngredient(ItemID.DiamondGemsparkBlock, 5);
				recipe2.AddIngredient(ModContent.ItemType<RareXpPearl>(), 10);
				recipe2.AddIngredient(ModContent.ItemType<StrangeTome>(), 1);
				recipe2.AddIngredient(ModContent.ItemType<SapphireTome>(), 1);
				recipe2.AddIngredient(ModContent.ItemType<RubyTome>(), 1);
				recipe2.AddIngredient(ModContent.ItemType<TopazTome>(), 1);
				recipe2.AddIngredient(ModContent.ItemType<EmeraldTome>(), 1);
				recipe2.AddIngredient(ModContent.ItemType<AmberTome>(), 1);
				recipe2.AddIngredient(ModContent.ItemType<AmethystTome>(), 1);
				recipe2.AddTile(ModContent.TileType<Tiles.DiamondBookcase>());
				recipe2.SetResult(this);
				recipe2.AddRecipe();
			}
		}
	}

}