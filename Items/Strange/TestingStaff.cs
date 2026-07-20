using Microsoft.Xna.Framework;
using SariaMod.Items.Ruby;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.Strange
{
    public class TestingStaff : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Testing Staff");
            Tooltip.SetDefault(MiscUtilities.ColorMessage(
                "Left-click to fire the selected test projectile\nRight-click to open or close the projectile menu",
                new Color(0, 200, 250, 200)));
            ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true; // This lets the player target anywhere on the whole screen while using a controller.
            ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
        }

        public override void SetDefaults()
        {
            Item.knockBack = 13f;
            Item.mana = 0;
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
            Item.shootSpeed = TestingStaffUISystem.SelectedShootSpeed;
            Item.shoot = TestingStaffUISystem.SelectedProjectileType;
            Item.buffTime = 20;
        }

        public override void Update(ref float gravity, ref float maxFallSpeed)
        {
            Lighting.AddLight(Item.Center, Color.SeaShell.ToVector3() * 2f);
        }

        public override bool AltFunctionUse(Player player) => true;

        public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
        {
            int selectedDamage = TestingStaffUISystem.GetDamage(player.Fairy());
            damage.Base += selectedDamage - Item.damage;
        }

        public override bool CanUseItem(Player player)
        {
            bool openingMenu = player.altFunctionUse == 2;
            Item.autoReuse = !openingMenu;
            Item.mana = 0;

            // Right-click is a real item use so UseItem reliably receives the toggle.
            // Auto-reuse is disabled for this mode to produce one toggle per press.
            if (openingMenu)
            {
                Item.shoot = ProjectileID.None;
                return base.CanUseItem(player);
            }

            Item.shoot = TestingStaffUISystem.SelectedProjectileType;
            Item.shootSpeed = TestingStaffUISystem.SelectedShootSpeed;

            // Clicking any part of the testing menu should operate the menu, not fire
            // the staff underneath it.
            if (Main.myPlayer == player.whoAmI && TestingStaffUISystem.IsMouseOverPanel())
                return false;

            return base.CanUseItem(player);
        }

        public override bool? UseItem(Player player)
        {
            if (player.altFunctionUse == 2 && Main.myPlayer == player.whoAmI)
                TestingStaffUISystem.ToggleUI();

            return true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position,
                                   Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2 || Main.myPlayer != player.whoAmI)
                return false;

            int projectileType = TestingStaffUISystem.SelectedProjectileType;
            float shootSpeed = TestingStaffUISystem.SelectedShootSpeed;
            if (!EruptionProjectileLimitGlobal.CanSpawn(player.whoAmI, projectileType))
                return false;

            Vector2 spawnPosition;
            Vector2 shotVelocity;
            if (shootSpeed <= 0f)
            {
                spawnPosition = Main.MouseWorld;
                shotVelocity = Vector2.Zero;
            }
            else
            {
                spawnPosition = position;
                Vector2 direction = Main.MouseWorld - spawnPosition;
                if (direction.LengthSquared() <= 0.0001f)
                    direction = new Vector2(player.direction, 0f);
                else
                    direction.Normalize();

                shotVelocity = direction * shootSpeed;
            }

            TestingStaffUISystem.GetSpawnAI(player, projectileType, out float ai0, out float ai1);
            int projectileIndex = Projectile.NewProjectile(source, spawnPosition, shotVelocity, projectileType,
                damage, knockback, player.whoAmI, ai0, ai1);
            if (Main.projectile.IndexInRange(projectileIndex))
                Main.projectile[projectileIndex].originalDamage = damage;

            return false;
        }
    }
}
