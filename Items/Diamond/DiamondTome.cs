using Microsoft.Xna.Framework;




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

using Terraria;
using Terraria.ID;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace SariaMod.Items.Diamond
{


	public class DiamondTome : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Diamond Tome");
			Tooltip.SetDefault(SariaModUtilities.ColorMessage("Saria in her Strongest Form!", new Color(135, 206, 180)) + "\n" + SariaModUtilities.ColorMessage("Requires 11 minion slots", new Color(50, 200, 250)) + "\nUsing the tome after Saria is called\nwill change her ability\n~Saria will steal the power of enemies in a huge radius!\nThe power stolen will be stored in her moonblast shot.\n~Target an enemy to unleash the power stored.\n~Unleash power after the moon has three rings\nto cause a storm of power!\n " + "\n " + SariaModUtilities.ColorMessage("Super effective in:", new Color(0, 200, 250, 200)) + "\n" + SariaModUtilities.ColorMessage("Night, Space, and all evil biomes", new Color(0, 200, 250, 200)) + "\n " + "\n " + SariaModUtilities.ColorMessage("Not very effective in:", new Color(135, 206, 180)) + "\n" + SariaModUtilities.ColorMessage("~Jungle, and Glowshroom", new Color(135, 206, 180)));
			ItemID.Sets.GamepadWholeScreenUseRange[item.type] = true; // This lets the player target anywhere on the whole screen while using a controller.
			ItemID.Sets.LockOnIgnoresCollision[item.type] = true;
			
			
		}

		public override void SetDefaults()
		{
			item.damage = 1200;
			item.knockBack = 30f;
			item.mana = 1;
			
			item.width = 32;
			item.height = 32;
			item.useTime = 36;
			item.useAnimation = 36;
			item.useStyle = ItemUseStyleID.SwingThrow;
			item.value = Item.buyPrice(0, 30, 0, 0);
			item.rare = ItemRarityID.Expert;
			item.UseSound = SoundID.Item44;
			
			// These below are needed for a minion weapon
			item.noMelee = true;
			item.summon = true;
			item.buffType = ModContent.BuffType<DiamondSariaBuff>();
			// No buffTime because otherwise the item tooltip would say something like "1 minute duration"
			item.shoot = ModContent.ProjectileType<DiamondSariaMinion>();
			
		}
		public override Color? GetAlpha(Color lightColor)
		{
			return new Color(Main.DiscoB, 255, Main.DiscoG);
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
			if (player.altFunctionUse != 2 && (player.ownedProjectileCounts[ModContent.ProjectileType<DiamondSariaMinion>()] > 0f))
			{
				item.UseSound = SoundID.Item46;
				item.shoot = ModContent.ProjectileType<DSariaMinion>();
				return true;
			}


			else if (player.altFunctionUse != 2 && (player.ownedProjectileCounts[ModContent.ProjectileType<DSariaMinion>()] > 0f))
			{
				item.UseSound = SoundID.Item46;
				item.shoot = ModContent.ProjectileType<DSSariaMinion>();
				return true;
			}
			else if (player.altFunctionUse != 2 && (player.ownedProjectileCounts[ModContent.ProjectileType<DSSariaMinion>()] > 0f))
			{
				item.UseSound = SoundID.Item46;
				item.shoot = ModContent.ProjectileType<DRSariaMinion>();
				return true;
			}
			else if (player.altFunctionUse != 2 && (player.ownedProjectileCounts[ModContent.ProjectileType<DRSariaMinion>()] > 0f))
			{
				item.UseSound = SoundID.Item46;
				item.shoot = ModContent.ProjectileType<DTSariaMinion>();
				return true;
			}
			else if (player.altFunctionUse != 2 && (player.ownedProjectileCounts[ModContent.ProjectileType<DTSariaMinion>()] > 0f))
			{
				item.UseSound = SoundID.Item46;
				item.shoot = ModContent.ProjectileType<DESariaMinion>();
				return true;
			}
			else if (player.altFunctionUse != 2 && (player.ownedProjectileCounts[ModContent.ProjectileType<DESariaMinion>()] > 0f))
			{
				item.UseSound = SoundID.Item46;
				item.shoot = ModContent.ProjectileType<DASariaMinion>();
				return true;
			}
			else if (player.altFunctionUse != 2 && (player.ownedProjectileCounts[ModContent.ProjectileType<DASariaMinion>()] > 0f))
			{
				item.UseSound = SoundID.Item46;
				item.shoot = ModContent.ProjectileType<DAMSariaMinion>();
				return true;
			}
			else if (player.altFunctionUse != 2 && (player.ownedProjectileCounts[ModContent.ProjectileType<DAMSariaMinion>()] > 0f))
			{
				item.UseSound = SoundID.Item46;
				item.shoot = ModContent.ProjectileType<DiamondSariaMinion>();
				return true;
			}

			if (player.altFunctionUse != 2 && (!player.HasBuff(ModContent.BuffType<DiamondSariaBuff>())))
			{

				item.UseSound = SoundID.Item46;
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
				player.AddBuff(item.buffType, 30000);
			
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