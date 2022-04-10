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
using SariaMod.Items.Platinum;
using SariaMod.Items.Strange;
using SariaMod.Items.zPearls;
using SariaMod.Items.Playerattack;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.Sapphire
{


	public class SapphireTome : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Sapphire Tome");
			Tooltip.SetDefault("Calls on Saria, the Champion of Foresight!\nRequires 4 minion slots\nUsing the tome again will set a sentry");
			ItemID.Sets.GamepadWholeScreenUseRange[item.type] = true; // This lets the player target anywhere on the whole screen while using a controller.
			ItemID.Sets.LockOnIgnoresCollision[item.type] = true;
		}

		public override void SetDefaults()
		{
			item.damage = 12;
			item.knockBack = 13f;
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
			item.buffType = ModContent.BuffType<SapphireSariaBuff>();
			// No buffTime because otherwise the item tooltip would say something like "1 minute duration"
			item.shoot = ModContent.ProjectileType<SapphireSariaMinion>();
		}
		
		public override bool CanUseItem(Player player)
		{

			if (player.altFunctionUse != 2 && (player.ownedProjectileCounts[ModContent.ProjectileType<SapphireSariaMinion>()] > 0f) && (player.ownedProjectileCounts[ModContent.ProjectileType<Psybeam>()] < 3f))
			{
				item.UseSound = SoundID.Item43;
				item.shoot = ModContent.ProjectileType<Psybeam>();
				return true;
			}

			else if (player.altFunctionUse != 2 && (player.ownedProjectileCounts[ModContent.ProjectileType<SapphireSariaMinion>()] <= 0f))
			{

				item.UseSound = SoundID.Item44;
				item.shoot = ModContent.ProjectileType<SapphireSariaMinion>();
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
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ModContent.ItemType<StrangeTome>(), 1);
			recipe.AddIngredient(ItemID.Sapphire, 5);
			recipe.AddIngredient(ModContent.ItemType<XpPearl>(), 75);
			recipe.AddTile(ModContent.TileType<Tiles.SapphireBookcase>());
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}

}