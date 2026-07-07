using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
namespace SariaMod.Items.zPearls
{
    public class TMEmpower : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("TM Empower");
            Tooltip.SetDefault("A rare Item used to make Saria remember every Unlock\nGrants every ability and charged attack up to her Amethyst Form\nAlso grants the TM Points used to earn them");
        }
        public override void SetDefaults()
        {
            base.Item.width = 26;
            base.Item.height = 22;
            Item.useTime = 36;
            Item.useAnimation = 36;
            base.Item.maxStack = 999;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.UseSound = SoundID.Item163;
            Item.noMelee = true;
            Item.DamageType = DamageClass.Summon;
            base.Item.value = 0;
            base.Item.consumable = true;
            Item.value = Item.sellPrice(50, 0, 0, 0);
            Item.rare = ItemRarityID.Expert;
            Item.shoot = ModContent.ProjectileType<TMEmpowerProjectile>();
        }
        public override void Update(ref float gravity, ref float maxFallSpeed)
        {
            Lighting.AddLight(Item.Center, Color.Gold.ToVector3() * 2f);
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Here you can change where the minion is spawned. Most vanilla minions spawn at the cursor position.
            position = Main.MouseWorld;
            return true;
        }
        public override void AddRecipes()
        {
        }
    }
}
