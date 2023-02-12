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
			item.useAnimation = 15;
			item.useStyle = ItemUseStyleID.SwingThrow;
			item.value = Item.buyPrice(0, 30, 0, 0);
			item.rare = ItemRarityID.Expert;
			item.UseSound = SoundID.Item44;
			
			// These below are needed for a minion weapon
			item.noMelee = true;
			item.summon = true;
			item.buffType = ModContent.BuffType<DiamondSariaBuff>();
			// No buffTime because otherwise the item tooltip would say something like "1 minute duration"
			item.shoot = ModContent.ProjectileType<Transform>();
			
		}
		public override Color? GetAlpha(Color lightColor)
		{
			return new Color(Main.DiscoB, 255, Main.DiscoG);
		}
		public override void Update(ref float gravity, ref float maxFallSpeed)
		{

			Lighting.AddLight(item.Center, Color.OrangeRed.ToVector3() * 2f);
		}
		public override bool CanUseItem(Player player)
		{

			if (player.HasBuff(ModContent.BuffType<SariaBuff>()))
			{
				return false;
			}
			
			if (player.HasBuff(ModContent.BuffType<DiamondSariaBuff>()))
			{
				return true;
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

		public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
		{
			// This is needed so the buff that keeps your minion alive and allows you to despawn it properly applies
			player.AddBuff(item.buffType, 30000);
			if (player.altFunctionUse != 2 && (player.ownedProjectileCounts[ModContent.ProjectileType<Saria>()] <= 0f))
			{
				{
					Projectile.NewProjectile(player.Center + Utils.NextVector2CircularEdge(Main.rand, 8f, 8f), Utils.NextVector2Circular(Main.rand, 12f, 12f), ModContent.ProjectileType<Saria>(), damage, knockBack, player.whoAmI);
				}
			}
			else if (player.altFunctionUse != 2 && (player.ownedProjectileCounts[ModContent.ProjectileType<Saria>()] > 0f))
			{
				{
					Projectile.NewProjectile(player.Center + Utils.NextVector2CircularEdge(Main.rand, 8f, 8f), Utils.NextVector2Circular(Main.rand, 12f, 12f), ModContent.ProjectileType<Transform>(), damage, knockBack, player.whoAmI);
					for (int j = 0; j < 72; j++)
					{
						Dust dust = Dust.NewDustPerfect(player.Center, 113);
						dust.velocity = ((float)Math.PI * 2f * Vector2.Dot(((float)j / 72f * ((float)Math.PI * 2f)).ToRotationVector2(), player.velocity.SafeNormalize(Vector2.UnitY).RotatedBy((float)j / 72f * ((float)Math.PI * -2f)))).ToRotationVector2();
						dust.velocity = dust.velocity.RotatedBy((float)j / 36f * ((float)Math.PI * 2f)) * 8f;
						dust.noGravity = true;
						dust.scale = 1.9f;
					}
				}
			}
			return false;
		}



		public override void AddRecipes()
		{
			
		}
	}

}