using Microsoft.Xna.Framework;
using SariaMod.Items.zPearls;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using SariaMod;
using SariaMod.Items.zTalking;
using SariaMod.Buffs;
namespace SariaMod.Items.Emerald
{
    public class LivingPurpleShard : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Living Purple Shard");
            Tooltip.SetDefault("Can be sold for a Decent price\nWhen used can grant you a weak hovering gem\nCan be used with Saria to gain a much stronger hovering gem\nCannot be used if you have the PurpleRupeeBlock");
        }
        public override void SetDefaults()
        {
            base.Item.width = 26;
            base.Item.height = 22;
            base.Item.maxStack = 999;
            Item.knockBack = 13f;
            Item.damage = 10;
            base.Item.useTime = (base.Item.useAnimation = 10);
            Item.useStyle = 1;
            Item.value = Item.sellPrice(0, 0, 156, 0);
            Item.shootSpeed = 8;
            Item.noUseGraphic = true;
            Item.consumable = true;
            // These below are needed for a minion weapon
            Item.noMelee = true;
            Item.DamageType = DamageClass.Summon;
            Item.rare = ItemRarityID.Purple;
            Item.shoot = ModContent.ProjectileType<RupeeXPassive2>();
            Item.buffType = ModContent.BuffType<EmeraldBuff>();
            Item.buffTime = 10000;
        }
        public override bool AltFunctionUse(Player player)
        {
            return false;
        }
        public override void Update(ref float gravity, ref float maxFallSpeed)
        {
            Lighting.AddLight(Item.Center, Color.Purple.ToVector3() * 2f);
        }
        public override bool CanUseItem(Player player)
        {
            if ((player.ownedProjectileCounts[ModContent.ProjectileType<RupeeXPassive2>()] > 0f) || player.HasBuff(ModContent.BuffType<PurpleRupeeBlock>()))
            {
                return false;
            }
            return true;
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
                recipe.AddIngredient(ModContent.ItemType<LivingPurpleFragment>(), 3);
                recipe.Register();
            }
        }
    }
}
