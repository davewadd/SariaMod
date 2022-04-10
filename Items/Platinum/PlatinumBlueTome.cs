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
using SariaMod.Items.Barrier;
using SariaMod.Items.Strange;
using SariaMod.Items.zPearls;
using SariaMod.Items.Playerattack;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.Platinum
{


	public class PlatinumBlueTome : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Platinum Tome (Blue Variant)");
			Tooltip.SetDefault("Summons Saria in her strongest form!\nUsing the tome again will set a sentry\nRequires 18 minion slots\nUsing the tome after Saria is called\nwill cause her to place a Psychic Barrier");
			ItemID.Sets.GamepadWholeScreenUseRange[item.type] = true; // This lets the player target anywhere on the whole screen while using a controller.
			ItemID.Sets.LockOnIgnoresCollision[item.type] = true;
		}

		public override void SetDefaults()
		{
			item.damage = 10000;
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
			item.buffType = ModContent.BuffType<PlatinumBlueSariaBuff>();
			// No buffTime because otherwise the item tooltip would say something like "1 minute duration"
			item.shoot = ModContent.ProjectileType<PlatinumBlueSariaMinion>();
			
		}
		public override bool AltFunctionUse(Player player)
		{
			return true;
		}
		public override bool CanUseItem(Player player)
		{
			if (player.altFunctionUse == 2 && (player.ownedProjectileCounts[ModContent.ProjectileType<PlatinumBlueSariaMinion>()] > 0f) && (player.ownedProjectileCounts[ModContent.ProjectileType<BarrierMinion>()] <= 1))
			{
				SariaModUtilities.KillShootProjectile(player, ModContent.ProjectileType<BarrierMinion>());

				item.UseSound = base.mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Barrier");
				item.shoot = ModContent.ProjectileType<BarrierMinion>();
				return true;
			}
			else if (player.altFunctionUse != 2 && (player.ownedProjectileCounts[ModContent.ProjectileType<PlatinumBlueSariaMinion>()] > 0f) && (player.ownedProjectileCounts[ModContent.ProjectileType<Psybeam>()] < 20f) && (player.ownedProjectileCounts[ModContent.ProjectileType<Psybeam>()] >= 17f))
			{
				item.UseSound = SoundID.Item46;
				item.shoot = ModContent.ProjectileType<Psybeam>();
				return true;
			}

			else if (player.altFunctionUse != 2 && (player.ownedProjectileCounts[ModContent.ProjectileType<PlatinumBlueSariaMinion>()] > 0f) && (player.ownedProjectileCounts[ModContent.ProjectileType<Psybeam>()] < 17f))
			{
				item.UseSound = SoundID.Item43;
				item.shoot = ModContent.ProjectileType<Psybeam>();
				item.autoReuse = true;
				return true;
			}

			else if (player.altFunctionUse != 2 && (player.ownedProjectileCounts[ModContent.ProjectileType<PlatinumBlueSariaMinion>()] <= 0f))
			{

				item.UseSound = SoundID.Item44;
				item.shoot = ModContent.ProjectileType<PlatinumBlueSariaMinion>();
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

			recipe.AddIngredient(ModContent.ItemType<PlatinumTome>(), 1);
			recipe.AddIngredient(ModContent.ItemType<RareXpPearl>(), 250);
			recipe.AddIngredient(ItemID.CorruptionKey, 1);
			recipe.AddIngredient(ItemID.HallowedKey, 1);
			recipe.AddTile(ModContent.TileType<Tiles.PlatinumBookcase>());
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}

}