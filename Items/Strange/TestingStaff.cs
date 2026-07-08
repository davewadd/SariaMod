using Microsoft.Xna.Framework;
using SariaMod.Buffs;
using SariaMod.Gores;
using SariaMod.Items.Topaz;
using SariaMod.Items.Ruby;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using SariaMod.Items.Sapphire;
namespace SariaMod.Items.Strange
{
    public class TestingStaff : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("XPStaff");
            Tooltip.SetDefault(MiscUtilities.ColorMessage("Shows the Level of XP Saria has when used\nRight-click to spawn a frozen leaf gore for testing", new Color(0, 200, 250, 200)));
            ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true; // This lets the player target anywhere on the whole screen while using a controller.
            ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
        }
        public override void SetDefaults()
        {
            Item.knockBack = 13f;
            Item.mana = 1;
            Item.width = 32;
            Item.height = 32;
            base.Item.useTime = (base.Item.useAnimation = 10);
            Item.useStyle = 1;
            Item.value = Item.sellPrice(0, 30, 0, 0);
            Item.rare = ItemRarityID.Cyan;
            Item.autoReuse = true;
            // These below are needed for a minion weapon
            Item.noMelee = true;
            Item.damage = 80;
            Item.DamageType = DamageClass.Summon;
            Item.shootSpeed = 4;
            Item.shoot = ModContent.ProjectileType<ColdWaveCenter>();
            Item.buffTime = 20;
        }
        public override void Update(ref float gravity, ref float maxFallSpeed)
        {
            Lighting.AddLight(Item.Center, Color.SeaShell.ToVector3() * 2f);
        }

        public override bool AltFunctionUse(Player player) => true;

        public override bool? UseItem(Player player)
        {
            // Right-click: spawn a vanilla leaf gore at the cursor and immediately apply
            // the frozen visual effect, simulating an enemy dropping a leaf-type gore.
            if (player.altFunctionUse == 2 && Main.myPlayer == player.whoAmI)
            {
                Vector2 spawnPos = Main.MouseWorld;
                Vector2 spawnVel = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-2f, 0f));
                int goreIdx = Gore.NewGore(player.GetSource_ItemUse(Item), spawnPos, spawnVel, GoreID.TreeLeaf_Normal);
                if (goreIdx >= 0 && goreIdx < Main.maxGore && Main.gore[goreIdx] != null)
                    FrozenGoreSystem.TrackFrozenGore(Main.gore[goreIdx]);
            }
            return true;
        }
    }
}