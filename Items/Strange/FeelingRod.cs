using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.Strange
{
    public class FeelingRod : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("FeelingRod");
            Tooltip.SetDefault(MiscUtilities.ColorMessage("Left-click: open mood tester\nRight-click: apply settings to Saria\nMiddle-click: teleport Saria to cursor",
            new Color(255, 180, 220)));
            ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true;
            ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.value = 0;
            Item.rare = ItemRarityID.Cyan;
            Item.noMelee = true;
        }

        public override bool AltFunctionUse(Player player) => true;

        public override bool? UseItem(Player player)
        {
            if (Main.myPlayer != player.whoAmI)
                return true;

            if (player.altFunctionUse == 2)
            {
                // Right-click: apply the configured mood to the player's Saria
                FeelingRodUISystem.ApplyMoodToSaria(player);
            }
            else
            {
                // Left-click: toggle the mood tester UI
                FeelingRodUISystem.ToggleUI();
            }

            return true;
        }

        private int _middleClickCooldown = 0;

        public override void HoldItem(Player player)
        {
            if (_middleClickCooldown > 0)
                _middleClickCooldown--;

            if (Main.myPlayer != player.whoAmI) return;
            if (_middleClickCooldown > 0) return;
            if (!Main.mouseMiddle || !Main.mouseMiddleRelease) return;

            // Find this player's Saria projectile and force-teleport her to the cursor.
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];
                if (!proj.active || proj.owner != player.whoAmI) continue;
                if (proj.ModProjectile is Saria saria)
                {
                    saria.StartForcedTeleport(Main.MouseWorld);
                    _middleClickCooldown = 30;
                    break;
                }
            }
        }
    }
}
