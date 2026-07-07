using Microsoft.Xna.Framework;
using SariaMod.Buffs;
using SariaMod.Items.Emerald;
using SariaMod.Items.Strange;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
namespace SariaMod.Items.zPearls
{
    public class SariasConfect : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Saria's Confect");
            Tooltip.SetDefault("Frozen Yogurt mixed with pure crystalized magic!\nCauses Saria to enter a supercharged state\ngiving all attacks an added effect!");
        }
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.useTime = 36;
            Item.useAnimation = 36;
            Item.useStyle = ItemUseStyleID.DrinkLong;
            base.Item.width = 26;
            base.Item.height = 22;
            base.Item.maxStack = 999;
            base.Item.value = 0;
            base.Item.consumable = true;
            Item.rare = ItemRarityID.Expert;
            Item.UseSound = SoundID.Item3;
            Item.noMelee = true;
            Item.holdStyle = ItemHoldStyleID.HoldFront;
            Item.value = Item.sellPrice(0, 15, 0, 0);
            Item.DamageType = DamageClass.Summon;
            Item.shoot = ModContent.ProjectileType<Competitivetime>();
            Item.buffType = (BuffID.WellFed3);
            Item.buffTime = 20000;
        }
        public override bool CanUseItem(Player player)
        {
            if ((!player.HasBuff(ModContent.BuffType<Drained>())) && (!player.HasBuff(ModContent.BuffType<Sickness>())))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // This is needed so the buff that keeps your minion alive and allows you to despawn it properly applies
            player.AddBuff(Item.buffType, 50000);
            // Here you can change where the minion is spawned. Most vanilla minions spawn at the cursor position.
            position = Main.MouseWorld;
            return true;
        }
        public override void AddRecipes()
        {
            {
                Recipe recipe = CreateRecipe();
                recipe.AddIngredient(ModContent.ItemType<MediumXpPearl>(), 1);
                recipe.AddIngredient(ModContent.ItemType<FrozenYogurt>(), 1);
                recipe.AddIngredient(ItemID.ManaPotion, 3);
                recipe.AddIngredient(ItemID.SnowBlock, 5);
                recipe.Register();
            }
            {
                Recipe recipe = CreateRecipe();
                recipe.AddIngredient(ModContent.ItemType<LivingPurpleShard>(), 1);
                recipe.AddIngredient(ModContent.ItemType<FrozenYogurt>(), 1);
                recipe.AddIngredient(ItemID.ManaPotion, 3);
                recipe.AddIngredient(ItemID.SnowBlock, 5);
                recipe.Register();
            }
        }
    }
}