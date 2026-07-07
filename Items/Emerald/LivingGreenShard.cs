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
    public class LivingGreenShard : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Living Green Shard");
            Tooltip.SetDefault("Can be sold for a small price\nWhen used can grant you a weak hovering gem\nCan be used with Saria to gain a much stronger hovering gem");
        }
        public override void SetDefaults()
        {
            base.Item.width = 26;
            base.Item.height = 22;
            base.Item.maxStack = 999;
            Item.value = Item.sellPrice(0, 0, 40, 0);
            Item.knockBack = 13f;
            Item.damage = 20;
            Item.consumable = true;
            base.Item.useTime = (base.Item.useAnimation = 10);
            Item.useStyle = 1;
            Item.shootSpeed = 8;
            Item.noUseGraphic = true;
            // These below are needed for a minion weapon
            Item.noMelee = true;
            Item.DamageType = DamageClass.Summon;
            Item.rare = ItemRarityID.Lime;
            Item.shoot = ModContent.ProjectileType<RupeeXPassive>();
            Item.buffType = ModContent.BuffType<EmeraldBuff>();
            Item.buffTime = 10000;
        }
        public override void Update(ref float gravity, ref float maxFallSpeed)
        {
            Lighting.AddLight(Item.Center, Color.LimeGreen.ToVector3() * 2f);
        }
        public override bool CanUseItem(Player player)
        {
            if ((player.ownedProjectileCounts[ModContent.ProjectileType<RupeeXPassive>()] > 0f))
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
                recipe.AddIngredient(ModContent.ItemType<LivingGreenFragment>(), 3);
                recipe.Register();
            }
        }
    }
}
