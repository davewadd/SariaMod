using Microsoft.Xna.Framework;
using SariaMod.Items.zPearls;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using SariaMod;
using SariaMod.Items.zTalking;
namespace SariaMod.Items.Emerald
{
    public class HealShard : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Living Green Shard");
            Tooltip.SetDefault("You should'nt even have this");
        }
        public override void SetDefaults()
        {
            Item.useTime = 36;
            Item.useAnimation = 36;
            Item.useStyle = ItemUseStyleID.DrinkLong;
            base.Item.width = 26;
            base.Item.height = 22;
            base.Item.maxStack = 9999;
            base.Item.consumable = true;
            Item.damage = 1;
            base.Item.useTime = (base.Item.useAnimation = 10);
            Item.value = Item.sellPrice(0, 30, 0, 0);
            Item.rare = ItemRarityID.Master;
            Item.shootSpeed = 8;
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.DamageType = DamageClass.Summon;
            Item.rare = ItemRarityID.Lime;
            Item.healLife = 300;
            Item.potion = true;
            Item.potionDelay = 2;
        }
        public override void Update(ref float gravity, ref float maxFallSpeed)
        {
            Lighting.AddLight(Item.Center, Color.LimeGreen.ToVector3() * 2f);
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // This is needed so the buff that keeps your minion alive and allows you to despawn it properly applies
                if (player.controlQuickHeal)
            {
                return true;
            }
            // Here you can change where the minion is spawned. Most vanilla minions spawn at the cursor position.
            position = Main.MouseWorld;
            return true;
        }
        }
}
