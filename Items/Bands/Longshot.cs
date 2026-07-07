using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Utilities;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using SariaMod.Netcode.HookshotNetworking;

namespace SariaMod.Items.Bands
{
    /// <summary>
    /// LONGSHOT - Upgraded Hookshot
    /// 120-block range, 100 combat damage, launches enemies upward
    /// </summary>
    public class Longshot : BaseHookshotItem
    {
        protected override string DisplayNameText => "Longshot";
        protected override string TooltipText => 
            "[c/FFA500:~~~ UPGRADED HOOKSHOT ~~~]\n" +
            "\n" +
            "[c/87CEEB:=== GRAPPLE MODE (Left Click) ===]\n" +
            "Fire hook at tiles to pull yourself toward them\n" +
            "Right click to disconnect early with momentum boost\n" +
            "[c/FFD700:Pierce enemies while grappling for a ramming dash!]\n" +
            "Grants brief invulnerability during the dash\n" +
            "\n" +
            "[c/FF6B6B:=== COMBAT MODE (Right Click) ===]\n" +
            "Hook an enemy to tether them for up to 3.3 seconds\n" +
            "[c/FFD700:Press Right Click when orbs turn red for the Sweet Spot!]\n" +
            "[c/90EE90:Sweetspot Hit: Double damage dash!]\n" +
            "Normal dash triggers automatically after waiting\n" +
            "Try to use other weapons while you wait!\n" +
            "Press right click early to retract the hook instead\n" +
            "[c/FF69B4:5 second cooldown between combat dashes]\n" +
            "[c/FFA500:Launches enemies upward on successful dash!]\n" +
            "\n" +
            "[c/AAAAAA:120 block range | Double damage | Restores flight time]";
        protected override int ProjectileType => ModContent.ProjectileType<LongshotProjectile>();
        protected override int BaseDamage => 100; // Item damage (HookDamager does 50 = this/2)
        protected override int SellPriceGold => 2;
        protected override int Rarity => ItemRarityID.Orange;


        public override void AddRecipes()
        {
        }

    }

    /// <summary>
    /// Player data for Longshot functionality - tracks holding state
    /// </summary>
    public class LongshotPlayer : ModPlayer
    {
        public bool isHoldingLongshot = false;
        public bool wasHoldingLongshot = false;
        public SlotId loopingSoundSlot = SlotId.Invalid;
        public int loopSoundTimer = 0;

        public override void ResetEffects()
        {
            wasHoldingLongshot = isHoldingLongshot;
            isHoldingLongshot = false;
        }
    }

    /// <summary>
    /// Draw layer for longshot body on player's front arm
    /// </summary>
    public class LongshotArmLayer : PlayerDrawLayer
    {
        public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.ArmOverItem);

        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        {
            Player player = drawInfo.drawPlayer;
            
            // Don't show longshot on arm if player is incapacitated
            bool isIncapacitated = player.dead || player.frozen || player.stoned || 
                                   player.webbed || player.noItems || player.CCed;
            if (isIncapacitated)
                return false;
            
            HookshotPlayer hookshotPlayer = player.GetModPlayer<HookshotPlayer>();
            bool hasActiveHook = player.ownedProjectileCounts[ModContent.ProjectileType<LongshotProjectile>()] > 0;
            
            bool isHoldingLongshotItem = player.HeldItem.type == ModContent.ItemType<Longshot>();
            bool isForceHoldingLongshot = hookshotPlayer.isForceHoldingHookshot && hookshotPlayer.forceHoldItemType == ModContent.ItemType<Longshot>();
            
            return isHoldingLongshotItem || isForceHoldingLongshot || hasActiveHook;
        }

        private static Vector2 GetHandPosition(Player player, float armRotation)
        {
            bool pointingLeft = Math.Abs(armRotation) > MathHelper.PiOver2;
            int armSide = pointingLeft ? -1 : 1;
            Vector2 shoulderOffset = new Vector2(-5 * armSide, -2);
            Vector2 shoulderPos = player.MountedCenter + shoulderOffset;
            float armLength = 10f;
            return shoulderPos + armRotation.ToRotationVector2() * armLength;
        }

        public static (Vector2 position, float rotation) GetChainAttachPoint(Player player, Vector2 targetPos)
        {
            Vector2 toTarget = targetPos - player.MountedCenter;
            float armRotation = toTarget.ToRotation();
            Vector2 handPos = GetHandPosition(player, armRotation);
            float bodyHalfLength = 12f;
            Vector2 chainAttach = handPos + armRotation.ToRotationVector2() * bodyHalfLength;
            return (chainAttach, armRotation);
        }

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            Player player = drawInfo.drawPlayer;
            
            // Double-check incapacitation - don't draw anything if player can't use items
            bool isIncapacitated = player.dead || player.frozen || player.stoned || 
                                   player.webbed || player.noItems || player.CCed;
            if (isIncapacitated)
                return;
            
            HookshotPlayer hookshotPlayer = player.GetModPlayer<HookshotPlayer>();

            Texture2D bodyTexture = ModContent.Request<Texture2D>("SariaMod/Items/Bands/LongshotBody").Value;
            Texture2D hookTexture = ModContent.Request<Texture2D>("SariaMod/Items/Bands/HookshotHook").Value;
            
            if (bodyTexture == null) return;

            bool hasActiveHook = player.ownedProjectileCounts[ModContent.ProjectileType<LongshotProjectile>()] > 0;
            bool isLocalPlayer = player.whoAmI == Main.myPlayer;

            Vector2 targetPos;
            float armRotation;
            
            if (hasActiveHook)
            {
                targetPos = player.Center;
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile proj = Main.projectile[i];
                    if (proj.active && proj.owner == player.whoAmI && proj.type == ModContent.ProjectileType<LongshotProjectile>())
                    {
                        targetPos = proj.Center;
                        break;
                    }
                }
                Vector2 toTarget = targetPos - player.MountedCenter;
                armRotation = toTarget.ToRotation();
            }
            else if (isLocalPlayer)
            {
                targetPos = Main.MouseWorld;
                Vector2 toTarget = targetPos - player.MountedCenter;
                armRotation = toTarget.ToRotation();
            }
            else
            {
                armRotation = hookshotPlayer.syncedArmRotation;
                targetPos = player.MountedCenter + armRotation.ToRotationVector2() * 100f;
            }

            Vector2 handPos = GetHandPosition(player, armRotation);
            bool pointingLeft = Math.Abs(armRotation) > MathHelper.PiOver2;
            SpriteEffects effects = pointingLeft ? SpriteEffects.FlipVertically : SpriteEffects.None;

            int frameCount = 3;
            int frameHeight = bodyTexture.Height / frameCount;
            int currentFrame = Math.Clamp(hookshotPlayer.bodyAnimFrame, 0, frameCount - 1);
            Rectangle bodySourceRect = new Rectangle(0, currentFrame * frameHeight, bodyTexture.Width, frameHeight);

            float scale = 0.5f;
            Vector2 bodyOrigin = new Vector2(bodyTexture.Width / 2f, frameHeight / 2f);
            Color color = Lighting.GetColor((int)(player.Center.X / 16), (int)(player.Center.Y / 16));

            DrawData bodyData = new DrawData(
                bodyTexture,
                handPos - Main.screenPosition,
                bodySourceRect,
                color,
                armRotation,
                bodyOrigin,
                scale,
                effects,
                0
            );
            drawInfo.DrawDataCache.Add(bodyData);

            if (!hasActiveHook && hookTexture != null)
            {
                float bodyHalfLength = (bodyTexture.Width / 2f) * scale;
                Vector2 hookOffset = armRotation.ToRotationVector2() * bodyHalfLength;
                Vector2 hookPos = handPos + hookOffset - Main.screenPosition;
                Vector2 hookOrigin = new Vector2(0, hookTexture.Height / 2f);

                DrawData hookData = new DrawData(
                    hookTexture,
                    hookPos,
                    null,
                    color,
                    armRotation,
                    hookOrigin,
                    scale * 0.8f,
                    effects,
                    0
                );
                drawInfo.DrawDataCache.Add(hookData);
            }
        }
    }

    /// <summary>
    /// Longshot projectile - 120-block range, launches enemies upward on success
    /// </summary>
    public class LongshotProjectile : BaseHookshotProjectile
    {
        protected override float MaxRange => 1920f; // 120 blocks
        protected override int TimeoutFrames => 480; // 8 seconds
        protected override int CombatDamage => 100; // Item damage for dash hitbox (double hookshot)
        protected override int DashHitboxType => ModContent.ProjectileType<LongshotDashHitbox>();
        protected override String BodyTexturePath => "SariaMod/Items/Bands/LongshotBody";
        protected override bool LaunchEnemyOnSuccess => true; // Launches enemies upward

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Longshot");
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 4800; // Extended draw distance for longer range
        }
    }

    /// <summary>
    /// Longshot dash hitbox
    /// </summary>
    public class LongshotDashHitbox : BaseHookshotDashHitbox
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Longshot Dash");
        }
    }
}
