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

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.Ruby
{


	public class RubyTome : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Ruby Tome");
			Tooltip.SetDefault(SariaModUtilities.ColorMessage("Calls on Saria, the Champion of Foresight!", new Color(135, 206, 180)) + "\n" + SariaModUtilities.ColorMessage("Requires 5 minion slots", new Color(50, 200, 250)) + "\nUsing the tome after Saria is called\nwill change her ability\n~Eruption will Bomb your enemies!\n~Smaller flames will be generated after the explosion\n " + "\n " + SariaModUtilities.ColorMessage("Super effective in:", new Color(0, 200, 250, 200)) + "\n" + SariaModUtilities.ColorMessage("~Snow, Glowshroom, Jungle, Dungeon, and Holy", new Color(0, 200, 250, 200)) + "\n " + "\n " + SariaModUtilities.ColorMessage("Not very effective in:", new Color(135, 206, 180)) + "\n" + SariaModUtilities.ColorMessage(" ~Ocean, Rain, Hell, Meteor, and Sandstorm", new Color(135, 206, 180)));

			ItemID.Sets.GamepadWholeScreenUseRange[item.type] = true; // This lets the player target anywhere on the whole screen while using a controller.
			ItemID.Sets.LockOnIgnoresCollision[item.type] = true;
		}

		public override void SetDefaults()
		{
			item.damage = 26;
			item.knockBack = 20f;
			item.mana = 1;
			item.width = 32;
			item.height = 32;
			item.useTime = 36;
			item.useAnimation = 15;
			item.useStyle = ItemUseStyleID.SwingThrow;
			item.value = Item.buyPrice(0, 30, 0, 0);
			item.rare = ItemRarityID.Cyan;
			item.UseSound = SoundID.Item46;

			// These below are needed for a minion weapon
			item.noMelee = true;
			item.summon = true;
			item.buffType = ModContent.BuffType<RubySariaBuff>();
			// No buffTime because otherwise the item tooltip would say something like "1 minute duration"
			item.shoot = ModContent.ProjectileType<RubySariaMinion>();
			
		}
		public override void Update(ref float gravity, ref float maxFallSpeed)
		{

			Lighting.AddLight(item.Center, Color.Red.ToVector3() * 2f);
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
			if (player.altFunctionUse != 2 && (player.ownedProjectileCounts[ModContent.ProjectileType<RubySariaMinion>()] > 0f))
			{
				item.UseSound = SoundID.Item46;
				item.shoot = ModContent.ProjectileType<RSariaMinion>();
				return true;
			}


			else if (player.altFunctionUse != 2 && (player.ownedProjectileCounts[ModContent.ProjectileType<RSariaMinion>()] > 0f))
			{
				item.UseSound = SoundID.Item46;
				item.shoot = ModContent.ProjectileType<RSSariaMinion>();
				return true;
			}
			else if (player.altFunctionUse != 2 && (player.ownedProjectileCounts[ModContent.ProjectileType<RSSariaMinion>()] > 0f))
			{
				item.UseSound = SoundID.Item46;
				item.shoot = ModContent.ProjectileType<RubySariaMinion>();
				return true;
			}
			
			if (player.altFunctionUse != 2 && (!player.HasBuff(ModContent.BuffType<RubySariaBuff>())))
			{

				item.UseSound = SoundID.Item46;
				item.shoot = ModContent.ProjectileType<RubySariaMinion>();
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
				recipe.AddIngredient(ModContent.ItemType<SapphireTome>(), 1);
				recipe.AddIngredient(ItemID.Ruby, 5);
				recipe.AddIngredient(ModContent.ItemType<XpPearl>(), 400);
				recipe.AddTile(ModContent.TileType<Tiles.RubyBookcase>());
				recipe.SetResult(this);
				recipe.AddRecipe();
			}
			{
				ModRecipe recipe2 = new ModRecipe(mod);
				recipe2.AddIngredient(ModContent.ItemType<SapphireTome>(), 1);
				recipe2.AddIngredient(ItemID.Ruby, 5);
				recipe2.AddIngredient(ModContent.ItemType<MediumXpPearl>(), 8);
				recipe2.AddTile(ModContent.TileType<Tiles.RubyBookcase>());
				recipe2.SetResult(this);
				recipe2.AddRecipe();
			}
		}
	}

}