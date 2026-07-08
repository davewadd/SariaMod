using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Utilities;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using SariaMod.Netcode.HookshotNetworking;
using SariaMod.Buffs;

namespace SariaMod.Items.Bands
{
    /// <summary>
    /// Base class for Hookshot and Longshot items
    /// Override the abstract properties to define differences
    /// </summary>
    public abstract class BaseHookshotItem : ModItem
    {
        // ===== ABSTRACT PROPERTIES - Override these in derived classes =====
        protected abstract string DisplayNameText { get; }
        protected abstract string TooltipText { get; }
        protected abstract int ProjectileType { get; }
        protected abstract int BaseDamage { get; }
        protected abstract int SellPriceGold { get; }
        protected abstract int Rarity { get; }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault(DisplayNameText);
            Tooltip.SetDefault(TooltipText);
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.useTime = 10;
            Item.useAnimation = 10;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.shootSpeed = 28f;
            Item.shoot = ProjectileType;
            Item.value = Item.sellPrice(0, SellPriceGold, 0, 0);
            Item.rare = Rarity;
            Item.damage = BaseDamage;
            Item.DamageType = DamageClass.Melee;
            Item.knockBack = 4f;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.autoReuse = false;
        }


        public override bool AltFunctionUse(Player player) => true;

        public override bool CanUseItem(Player player)
        {
            // Can't use if player is incapacitated
            if (player.dead || player.frozen || player.stoned || player.webbed || player.noItems || player.CCed)
                return false;

            return player.ownedProjectileCounts[ProjectileType] < 1;
        }

        public override void HoldItem(Player player)
        {
            HookshotPlayer modPlayer = player.GetModPlayer<HookshotPlayer>();

            // Check if player is incapacitated - can't use items at all
            // This covers frozen, stoned, webbed, cursed, and any mod that sets these flags
            bool isIncapacitated = player.dead || player.frozen || player.stoned ||
                                   player.webbed || player.noItems || player.CCed;

            if (isIncapacitated)
            {
                // ACTIVELY disable the composite arm - just skipping SetCompositeArmFront doesn't reset it
                player.SetCompositeArmFront(false, Player.CompositeArmStretchAmount.Full, 0f);

                // LOCK the player direction - don't let them flip to cursor while incapacitated
                // Use the synced direction to maintain the direction they were facing
                player.direction = modPlayer.syncedDirection;

                // If we were holding before, send sync to notify other clients we stopped
                if (modPlayer.isHoldingHookshot && Main.myPlayer == player.whoAmI && Main.netMode != NetmodeID.SinglePlayer)
                {
                    modPlayer.isHoldingHookshot = false;
                    modPlayer.hasActiveHookForArm = false;
                    modPlayer.SendArmSync(); // Tell other clients to stop showing the arm
                }
                modPlayer.isHoldingHookshot = false;
                return;
            }

            modPlayer.isHoldingHookshot = true;

            if (Main.myPlayer != player.whoAmI)
                return;

            if (!modPlayer.wasHoldingHookshot)
            {
                SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Hookshotset") with { Volume = 0.8f }, player.Center);
                HookshotSyncMessage.SendSound(HookshotSoundType.Set, player.Center, player.whoAmI);
            }

            if (Main.MouseWorld.X > player.Center.X)
                player.direction = 1;
            else
                player.direction = -1;

            // Store the direction for syncing and for locking when incapacitated
            modPlayer.syncedDirection = player.direction;

            Vector2 toMouse = Main.MouseWorld - player.MountedCenter;
            float rotation = toMouse.ToRotation();

            modPlayer.syncedArmRotation = rotation;
            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, rotation - MathHelper.PiOver2);

            if (Main.netMode != NetmodeID.SinglePlayer)
            {
                modPlayer.SendArmSync();
            }
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            bool isCombatMode = player.altFunctionUse == 2;

            int proj = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            if (proj >= 0 && proj < Main.maxProjectiles)
            {
                Main.projectile[proj].localAI[1] = isCombatMode ? 1f : 0f;
            }

            return false;
        }
    }
}
