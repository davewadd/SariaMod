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
using Terraria.Utilities;
using SariaMod.Netcode.HookshotNetworking;

namespace SariaMod.Items.Bands
{
    /// <summary>
    /// HOOKSHOT - Precision Grappling Hook
    /// 60-block range, 50 combat damage
    /// </summary>
    public class HookShot : BaseHookshotItem
    {
        protected override string DisplayNameText => "Hookshot";
        protected override string TooltipText => 
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
            "\n" +
            "[c/AAAAAA:60 block range | Restores flight time on enemy hook]";
        protected override int ProjectileType => ModContent.ProjectileType<HookshotProjectile>();
        protected override int BaseDamage => 50; // Item damage (HookDamager does 25 = this/2)
        protected override int SellPriceGold => 1;
        protected override int Rarity => ItemRarityID.Green;


        public override void AddRecipes()
        {
        }

    }

    /// <summary>
    /// Global item to prevent prefixes on hookshot
    /// </summary>
    public class HookshotGlobalItem : GlobalItem
    {
        public override bool? PrefixChance(Item item, int pre, UnifiedRandom rand)
        {
            if (item.type == ModContent.ItemType<HookShot>() || item.type == ModContent.ItemType<Longshot>())
                return false;
            return base.PrefixChance(item, pre, rand);
        }
    }

    /// <summary>
    /// Player data for Hookshot functionality
    /// </summary>
    public class HookshotPlayer : ModPlayer
    {
        public bool isHoldingHookshot = false;
        public bool wasHoldingHookshot = false;
        public bool hasHookshotEquipped = false;
        public bool hasLongshotEquipped = false;
        public Vector2 slingshotVelocity = Vector2.Zero;
        public bool shouldApplySlingshot = false;
        public int bodyAnimFrame = 0;
        public int bodyAnimTimer = 0;
        public bool isAnimatingForward = true;
        public int slingshotCooldown = 0;
        public SlotId loopingSoundSlot = SlotId.Invalid;
        public int loopSoundTimer = 0;
        private const int LoopSoundInterval = 15;
        public bool isForceHoldingHookshot = false;
        public Item previousHeldItem = null;
        public int previousSelectedItem = -1;
        public int forceHoldItemType = -1;
        public float syncedArmRotation = 0f;
        public bool hasActiveHookForArm = false;
        public int syncedDirection = 1;
        public bool needsPostPullSync = false;
        public int postPullSyncTimer = 0;
        private const int PostPullSyncInterval = 5;
        public int fallDamageProtectionTimer = 0;
        private const int FallDamageProtectionDuration = 60;
        public bool isFrozenByOwnHookshot = false;
        public Vector2 frozenPosition = Vector2.Zero;
        public int playerFreezeTimer = 0;
        public const int MaxPlayerFreezeDuration = 3600;
        public int hookedNPCIndex = -1;
        
        // Flag to indicate player is being pulled to an enemy (combat mode dash)
        // When true, player should pass through platforms but not solid tiles
        public bool isPullingToEnemy = false;
        public Vector2 pullTargetPosition = Vector2.Zero;
        
        // Direction lock for when player becomes incapacitated
        private int lockedDirection = 1;
        private bool wasIncapacitated = false;

        public override void ResetEffects()
        {
            wasHoldingHookshot = isHoldingHookshot;
            isHoldingHookshot = false;
            hasHookshotEquipped = false;
            hasLongshotEquipped = false;
        }
        
        public override void PreUpdate()
        {
            // Check if player is holding hookshot or longshot
            bool isHoldingHookshotItem = Player.HeldItem.type == ModContent.ItemType<HookShot>() || 
                                         Player.HeldItem.type == ModContent.ItemType<Longshot>() ||
                                         isForceHoldingHookshot;
            
            if (!isHoldingHookshotItem)
                return;
            
            bool isIncapacitated = Player.dead || Player.frozen || Player.stoned || 
                                   Player.webbed || Player.noItems || Player.CCed;
            
            // If we just became incapacitated, lock the current direction
            if (isIncapacitated && !wasIncapacitated)
            {
                lockedDirection = Player.direction;
            }
            // If incapacitated, store direction at start of frame (before Terraria changes it)
            else if (!isIncapacitated)
            {
                lockedDirection = Player.direction;
            }
            
            wasIncapacitated = isIncapacitated;
        }

        public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
        {
            if (isHoldingHookshot)
                HookshotSyncMessage.SendArmSync(Player, syncedArmRotation, hasActiveHookForArm, isHoldingHookshot, Player.direction);
        }

        public override void PostUpdate()
        {
            // Handle platform and liquid pass-through during combat pull
            if (isPullingToEnemy && pullTargetPosition != Vector2.Zero)
            {
                Vector2 toTarget = pullTargetPosition - Player.Center;
                float distance = toTarget.Length();
                
                // If player velocity was reduced (hit something), check if it was a platform or liquid surface
                if (distance > 48f && Player.velocity.Length() < HookshotConfig.CombatPullSpeed * 0.5f)
                {
                    // Check tiles at player's position for platforms or liquids
                    bool onlyPassableBlocking = true;  // Only platforms or liquid surfaces blocking
                    bool somethingBlocking = false;
                    
                    Vector2 pullDir = toTarget.SafeNormalize(Vector2.Zero);
                    Vector2 checkPos = Player.position + pullDir * 16f;
                    
                    int startX = (int)(checkPos.X / 16f);
                    int endX = (int)((checkPos.X + Player.width) / 16f);
                    int startY = (int)(checkPos.Y / 16f);
                    int endY = (int)((checkPos.Y + Player.height) / 16f);
                    
                    bool hasWaterWalking = Player.waterWalk || Player.waterWalk2;
                    
                    for (int x = startX; x <= endX; x++)
                    {
                        for (int y = startY; y <= endY; y++)
                        {
                            if (x < 0 || x >= Main.maxTilesX || y < 0 || y >= Main.maxTilesY)
                                continue;
                            
                            Tile tile = Main.tile[x, y];
                            
                            // Check for liquid surface (water walking makes this solid)
                            if (hasWaterWalking && tile.LiquidAmount > 0)
                            {
                                // Check if this is the surface of the liquid (tile above has less/no liquid)
                                if (y > 0)
                                {
                                    Tile tileAbove = Main.tile[x, y - 1];
                                    if (tileAbove.LiquidAmount < tile.LiquidAmount)
                                    {
                                        somethingBlocking = true;
                                        continue; // Liquid surface is passable during pull
                                    }
                                }
                            }
                            
                            if (!tile.HasTile)
                                continue;
                            
                            bool isSolidBlock = Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType];
                            bool isPlatform = Main.tileSolidTop[tile.TileType];
                            
                            if (isSolidBlock)
                            {
                                onlyPassableBlocking = false;
                                somethingBlocking = true;
                                break;
                            }
                            else if (isPlatform)
                            {
                                somethingBlocking = true;
                            }
                        }
                        if (!onlyPassableBlocking) break;
                    }
                    
                    // If only platforms/liquid surfaces are blocking, push the player through
                    if (somethingBlocking && onlyPassableBlocking)
                    {
                        Player.position += pullDir * HookshotConfig.CombatPullSpeed;
                        Player.velocity = pullDir * HookshotConfig.CombatPullSpeed;
                    }
                }
            }

            
            bool hasDashHitbox = Player.ownedProjectileCounts[ModContent.ProjectileType<HookshotDashHitbox>()] > 0 ||
                                 Player.ownedProjectileCounts[ModContent.ProjectileType<LongshotDashHitbox>()] > 0;
            
            bool hasActiveHookProj = Player.ownedProjectileCounts[ModContent.ProjectileType<HookshotProjectile>()] > 0 ||
                                     Player.ownedProjectileCounts[ModContent.ProjectileType<LongshotProjectile>()] > 0;

            if (hasDashHitbox)
                fallDamageProtectionTimer = FallDamageProtectionDuration;

            if (fallDamageProtectionTimer > 0)
            {
                fallDamageProtectionTimer--;
                Player.fallStart = (int)(Player.position.Y / 16f);
            }

            if (hasActiveHookProj)
                Player.fallStart = (int)(Player.position.Y / 16f);

            // Hookshot Laser — spawn a non-damaging aiming laser when holding hookshot/longshot with no active hook
            if (Main.myPlayer == Player.whoAmI)
            {
                bool holdingHookshotItem = Player.HeldItem.type == ModContent.ItemType<HookShot>()
                    || Player.HeldItem.type == ModContent.ItemType<Longshot>()
                    || isForceHoldingHookshot;

                bool wantsLaser = holdingHookshotItem && !hasActiveHookProj && !Player.dead;
                bool hasLaser = Player.ownedProjectileCounts[ModContent.ProjectileType<HookshotLaser>()] > 0;

                if (wantsLaser && !hasLaser)
                {
                    Projectile.NewProjectile(
                        Player.GetSource_FromThis(),
                        Player.Center,
                        Vector2.UnitX * Player.direction,
                        ModContent.ProjectileType<HookshotLaser>(),
                        0, 0f, Player.whoAmI
                    );
                }
            }

            if (isFrozenByOwnHookshot)
            {
                playerFreezeTimer++;
                if (playerFreezeTimer > MaxPlayerFreezeDuration)
                {
                    ClearPlayerFreeze();
                }
                else
                {
                    Player.velocity *= 0.1f;
                    Player.fallStart = (int)(Player.position.Y / 16f);
                    if (frozenPosition != Vector2.Zero)
                    {
                        float drift = Vector2.Distance(Player.Center, frozenPosition);
                        if (drift > 4f)
                            Player.Center = frozenPosition;
                    }
                }
            }

            Item equippedHook = Player.miscEquips[4];
            if (equippedHook.type == ModContent.ItemType<HookShot>())
                hasHookshotEquipped = true;
            else if (equippedHook.type == ModContent.ItemType<Longshot>())
                hasLongshotEquipped = true;

            if (isForceHoldingHookshot)
            {
                bool hasActiveHookshotProj = Player.ownedProjectileCounts[ModContent.ProjectileType<HookshotProjectile>()] > 0;
                bool hasActiveLongshotProj = Player.ownedProjectileCounts[ModContent.ProjectileType<LongshotProjectile>()] > 0;
                
                if (!hasActiveHookshotProj && !hasActiveLongshotProj)
                    ReleaseForceHold();
            }

            if (slingshotCooldown > 0)
                slingshotCooldown--;

            if (shouldApplySlingshot && slingshotCooldown <= 0)
            {
                // Cap the slingshot velocity to prevent exploits
                if (slingshotVelocity.Length() > 15f)
                    slingshotVelocity = slingshotVelocity.SafeNormalize(Vector2.Zero) * 15f;
                
                Player.velocity += slingshotVelocity;
                slingshotCooldown = 30; // Half second cooldown
            }
            shouldApplySlingshot = false;
            slingshotVelocity = Vector2.Zero;

            // Handle post-pull position sync (only for local player in multiplayer)
            if (Main.myPlayer == Player.whoAmI && Main.netMode == NetmodeID.MultiplayerClient && needsPostPullSync)
            {
                // Check if player is providing any input or has landed
                bool hasInput = Player.controlLeft || Player.controlRight || Player.controlJump || 
                               Player.controlUp || Player.controlDown || Player.controlUseItem;
                bool isGrounded = Player.velocity.Y == 0 && Player.oldVelocity.Y == 0;
                
                if (hasInput || isGrounded)
                {
                    // Stop syncing - player has input or landed
                    needsPostPullSync = false;
                    postPullSyncTimer = 0;
                }
                else
                {
                    // Continue syncing position every 5 frames
                    postPullSyncTimer++;
                    if (postPullSyncTimer >= PostPullSyncInterval)
                    {
                        postPullSyncTimer = 0;
                        HookshotSyncMessage.SendPlayerPullComplete(Player.position, Player.velocity, Player.whoAmI, Player.whoAmI);
                    }
                }
            }

            bool hasActiveHook = hasActiveHookProj;
            hasActiveHookForArm = hasActiveHook;
            
            // Check if player is incapacitated - don't show arm visuals
            bool isIncapacitated = Player.dead || Player.frozen || Player.stoned || 
                                   Player.webbed || Player.noItems || Player.CCed;
            
            // Check if player is holding hookshot or longshot
            bool isHoldingHookshotItem = Player.HeldItem.type == ModContent.ItemType<HookShot>() || 
                                         Player.HeldItem.type == ModContent.ItemType<Longshot>() ||
                                         isForceHoldingHookshot;
            
            // If incapacitated, ACTIVELY disable the composite arm and LOCK direction
            if (isIncapacitated)
            {
                Player.SetCompositeArmFront(false, Player.CompositeArmStretchAmount.Full, 0f);
                bodyAnimFrame = 0;
                bodyAnimTimer = 0;
                isAnimatingForward = true;
                
                // LOCK direction when incapacitated and holding hookshot/longshot
                // Use lockedDirection which was captured in PreUpdate before Terraria changed it
                if (isHoldingHookshotItem)
                {
                    Player.direction = lockedDirection;
                }
                
                return;
            }
            
            // LOCAL PLAYER: Calculate everything and sync to others
            if (Main.myPlayer == Player.whoAmI)
            {
                if (hasActiveHook)
                {
                    // Animate only when hook is deployed
                    bodyAnimTimer++;
                    if (bodyAnimTimer >= 5)
                    {
                        bodyAnimTimer = 0;
                        if (isAnimatingForward)
                        {
                            bodyAnimFrame++;
                            if (bodyAnimFrame >= 2)
                            {
                                bodyAnimFrame = 2;
                                isAnimatingForward = false;
                            }
                        }
                        else
                        {
                            bodyAnimFrame--;
                            if (bodyAnimFrame <= 0)
                            {
                                bodyAnimFrame = 0;
                                isAnimatingForward = true;
                            }
                        }
                    }
                    
                    // Find hook position and calculate arm rotation
                    Vector2 hookPos = Player.Center;
                    for (int i = 0; i < Main.maxProjectiles; i++)
                    {
                        Projectile proj = Main.projectile[i];
                        if (proj.active && proj.owner == Player.whoAmI && 
                            (proj.type == ModContent.ProjectileType<HookshotProjectile>() ||
                             proj.type == ModContent.ProjectileType<LongshotProjectile>()))
                        {
                            hookPos = proj.Center;
                            break;
                        }
                    }
                    
                    // Calculate and store arm rotation
                    Vector2 toHook = hookPos - Player.MountedCenter;
                    syncedArmRotation = toHook.ToRotation();
                    
                    // Store the current direction for syncing and for locking when incapacitated
                    syncedDirection = Player.direction;
                    
                    // Apply arm rotation locally
                    Player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, syncedArmRotation - MathHelper.PiOver2);
                    
                    // Sync arm rotation to other clients EVERY FRAME
                    if (Main.netMode != NetmodeID.SinglePlayer)
                    {
                        SendArmSync();
                    }
                }
                else if (isHoldingHookshot)
                {
                    // Holding but no hook - arm rotation is set in HoldItem, just apply it
                    Player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, syncedArmRotation - MathHelper.PiOver2);
                    
                    // Store the current direction for locking when incapacitated
                    syncedDirection = Player.direction;
                }
                else
                {
                    // Reset animation state when hook is not out and not holding (or incapacitated)
                    bodyAnimFrame = 0;
                    bodyAnimTimer = 0;
                    isAnimatingForward = true;
                }
                
                // Handle looping sound with timer-based approach
                if (hasActiveHook)
                {
                    loopSoundTimer++;
                    if (loopSoundTimer >= LoopSoundInterval)
                    {
                        loopSoundTimer = 0;
                        SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/HookshotLoop") { Volume = 0.5f }, Player.Center);
                        
                        // Sync loop sound to other clients
                        if (Main.netMode == NetmodeID.MultiplayerClient)
                        {
                            HookshotSyncMessage.SendLoopSoundSync(Player.whoAmI, true, Player.Center, Player.whoAmI);
                        }
                    }
                }
                else
                {
                    loopSoundTimer = 0;
                }
            }
            // REMOTE PLAYER: Just apply synced values, no calculations
            else
            {
                if (hasActiveHookForArm || isHoldingHookshot)
                {
                    // FORCE the synced direction - this overrides any movement-based direction changes
                    Player.direction = syncedDirection;
                    
                    // Apply synced arm rotation - don't calculate anything
                    Player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, syncedArmRotation - MathHelper.PiOver2);
                    
                    // Animation for remote players
                    if (hasActiveHookForArm)
                    {
                        bodyAnimTimer++;
                        if (bodyAnimTimer >= 5)
                        {
                            bodyAnimTimer = 0;
                            if (isAnimatingForward)
                            {
                                bodyAnimFrame++;
                                if (bodyAnimFrame >= 2)
                                {
                                    bodyAnimFrame = 2;
                                    isAnimatingForward = false;
                                }
                            }
                            else
                            {
                                bodyAnimFrame--;
                                if (bodyAnimFrame <= 0)
                                {
                                    bodyAnimFrame = 0;
                                    isAnimatingForward = true;
                                }
                            }
                        }
                    }
                }
                else
                {
                    // Reset animation state
                    bodyAnimFrame = 0;
                    bodyAnimTimer = 0;
                    isAnimatingForward = true;
                }
            }
        }

        public void SendArmSync()
        {
            HookshotSyncMessage.SendArmSync(Player, syncedArmRotation, hasActiveHookForArm, isHoldingHookshot, Player.direction);
        }

        public static void HandleArmSync(BinaryReader reader)
        {
            int playerIndex = reader.ReadByte();
            float rotation = reader.ReadSingle();
            bool hasHook = reader.ReadBoolean();
            bool isHolding = reader.ReadBoolean();
            
            if (playerIndex >= 0 && playerIndex < Main.maxPlayers)
            {
                Player player = Main.player[playerIndex];
                if (player.active)
                {
                    HookshotPlayer modPlayer = player.GetModPlayer<HookshotPlayer>();
                    modPlayer.syncedArmRotation = rotation;
                    modPlayer.hasActiveHookForArm = hasHook;
                    modPlayer.isHoldingHookshot = isHolding;
                    
                    // Check if player is incapacitated
                    bool isIncapacitated = player.dead || player.frozen || player.stoned || 
                                           player.webbed || player.noItems || player.CCed;
                    
                    if (isIncapacitated)
                    {
                        // ACTIVELY disable the arm when incapacitated
                        player.SetCompositeArmFront(false, Player.CompositeArmStretchAmount.Full, 0f);
                    }
                    else if (hasHook || isHolding)
                    {
                        player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, rotation - MathHelper.PiOver2);
                    }
                }
            }
        }

        public override void ProcessTriggers(Terraria.GameInput.TriggersSet triggersSet)
        {
            if (triggersSet.Grapple)
            {
                bool hasActiveHook = Player.ownedProjectileCounts[ModContent.ProjectileType<HookshotProjectile>()] > 0 ||
                                     Player.ownedProjectileCounts[ModContent.ProjectileType<LongshotProjectile>()] > 0;
                
                if (!hasActiveHook)
                {
                    if (hasLongshotEquipped)
                        FireFromGrappleSlot(ModContent.ItemType<Longshot>(), ModContent.ProjectileType<LongshotProjectile>());
                    else if (hasHookshotEquipped)
                        FireFromGrappleSlot(ModContent.ItemType<HookShot>(), ModContent.ProjectileType<HookshotProjectile>());
                }
            }
        }

        public void StartForceHold(int itemType)
        {
            if (isForceHoldingHookshot) return;
            
            previousSelectedItem = Player.selectedItem;
            previousHeldItem = Player.inventory[Player.selectedItem].Clone();
            forceHoldItemType = itemType;
            isForceHoldingHookshot = true;
            
            Item tempItem = new Item();
            tempItem.SetDefaults(itemType);
            Player.inventory[Player.selectedItem] = tempItem;
            isHoldingHookshot = true;
        }

        public void ReleaseForceHold()
        {
            if (!isForceHoldingHookshot) return;
            
            if (previousHeldItem != null && previousSelectedItem >= 0 && previousSelectedItem < Player.inventory.Length)
                Player.inventory[previousSelectedItem] = previousHeldItem.Clone();
            
            isForceHoldingHookshot = false;
            previousHeldItem = null;
            previousSelectedItem = -1;
            forceHoldItemType = -1;
        }

        private void FireFromGrappleSlot(int itemType, int projectileType)
        {
            StartForceHold(itemType);
            
            Vector2 direction = (Main.MouseWorld - Player.Center).SafeNormalize(Vector2.UnitX);
            Vector2 velocity = direction * 28f;
            
            if (direction.X > 0)
                Player.direction = 1;
            else
                Player.direction = -1;
            
            int damage = itemType == ModContent.ItemType<Longshot>() ? 100 : 50;
            
            Projectile.NewProjectile(
                Player.GetSource_ItemUse(Player.miscEquips[4]),
                Player.Center,
                velocity,
                projectileType,
                damage, 
                4f,
                Player.whoAmI
            );
            
            SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/HookshotStart") { Volume = 0.7f }, Player.Center);
            HookshotSyncMessage.SendSound(HookshotSoundType.Start, Player.Center, Player.whoAmI);
        }

        public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
        {
            ReleaseForceHold();
        }

        public override void Initialize()
        {
            isForceHoldingHookshot = false;
            previousHeldItem = null;
            previousSelectedItem = -1;
            forceHoldItemType = -1;
        }

        public void ClearPlayerFreeze()
        {
            isFrozenByOwnHookshot = false;
            frozenPosition = Vector2.Zero;
            playerFreezeTimer = 0;
        }
        
        public void FreezePlayer()
        {
            isFrozenByOwnHookshot = true;
            frozenPosition = Player.Center;
            playerFreezeTimer = 0;
        }
    }

    /// <summary>
    /// Draw layer for hookshot body on player's front arm
    /// </summary>
    public class HookshotArmLayer : PlayerDrawLayer
    {
        public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.ArmOverItem);

        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        {
            Player player = drawInfo.drawPlayer;
            HookshotPlayer modPlayer = player.GetModPlayer<HookshotPlayer>();
            
            // Don't show hookshot on arm if player is incapacitated
            bool isIncapacitated = player.dead || player.frozen || player.stoned || 
                                   player.webbed || player.noItems || player.CCed;
            if (isIncapacitated)
                return false;
            
            bool hasActiveLongshotHook = player.ownedProjectileCounts[ModContent.ProjectileType<LongshotProjectile>()] > 0;
            bool isHoldingLongshotItem = player.HeldItem.type == ModContent.ItemType<Longshot>();
            bool isForceHoldingLongshot = modPlayer.isForceHoldingHookshot && modPlayer.forceHoldItemType == ModContent.ItemType<Longshot>();
            
            if (hasActiveLongshotHook || isHoldingLongshotItem || isForceHoldingLongshot)
                return false;
            
            bool hasActiveHook = player.ownedProjectileCounts[ModContent.ProjectileType<HookshotProjectile>()] > 0;
            bool isHoldingHookshotItem = player.HeldItem.type == ModContent.ItemType<HookShot>();
            bool isForceHoldingHookshot = modPlayer.isForceHoldingHookshot && modPlayer.forceHoldItemType == ModContent.ItemType<HookShot>();
            bool otherPlayerHasHook = modPlayer.hasActiveHookForArm && !hasActiveLongshotHook;
            
            return isHoldingHookshotItem || isForceHoldingHookshot || hasActiveHook || otherPlayerHasHook;
        }

        /// <summary>
        /// Calculate the hookshot body position for chain attachment - at the front tip of the gauntlet
        /// </summary>
        public static (Vector2 position, float rotation) GetChainAttachPoint(Player player, Vector2 targetPos)
        {
            // Calculate arm rotation toward target
            Vector2 toTarget = targetPos - player.MountedCenter;
            float armRotation = toTarget.ToRotation();
            
            // Get the hand position based on arm rotation (matching vanilla arm positioning)
            Vector2 handPos = GetHandPosition(player, armRotation);
            
            // Chain attaches at front of gauntlet
            float bodyHalfLength = 12f;
            Vector2 chainAttach = handPos + armRotation.ToRotationVector2() * bodyHalfLength;
            
            return (chainAttach, armRotation);
        }

        /// <summary>
        /// Calculate the player's hand/forearm position based on arm rotation
        /// This matches how vanilla composite arms work
        /// </summary>
        private static Vector2 GetHandPosition(Player player, float armRotation)
        {
            // Use arm rotation to determine which side the arm should be on
            // This prevents flicking when player direction changes
            bool pointingLeft = Math.Abs(armRotation) > MathHelper.PiOver2;
            int armSide = pointingLeft ? -1 : 1;
            
            // The arm pivot is at the shoulder, which is offset from MountedCenter
            Vector2 shoulderOffset = new Vector2(
                -5 * armSide, // Shoulder position based on arm direction, not player direction
                -2 // Shoulder is slightly above center
            );
            Vector2 shoulderPos = player.MountedCenter + shoulderOffset;
            
            // Arm length from shoulder to hand (vanilla uses about 10-12 pixels for the forearm)
            float armLength = 10f;
            
            // Hand position extends from shoulder in the direction of arm rotation
            Vector2 handPos = shoulderPos + armRotation.ToRotationVector2() * armLength;
            
            return handPos;
        }

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            Player player = drawInfo.drawPlayer;
            
            // Double-check incapacitation - don't draw anything if player can't use items
            bool isIncapacitated = player.dead || player.frozen || player.stoned || 
                                   player.webbed || player.noItems || player.CCed;
            if (isIncapacitated)
                return;
            
            HookshotPlayer modPlayer = player.GetModPlayer<HookshotPlayer>();

            Texture2D bodyTexture = ModContent.Request<Texture2D>("SariaMod/Items/Bands/HookShotBody").Value;
            Texture2D hookTexture = ModContent.Request<Texture2D>("SariaMod/Items/Bands/HookshotHook").Value;
            
            if (bodyTexture == null) return;

            bool hasActiveHook = player.ownedProjectileCounts[ModContent.ProjectileType<HookshotProjectile>()] > 0;
            bool isLocalPlayer = player.whoAmI == Main.myPlayer;

            // Get target position (mouse for local player, hook for active hook, or synced rotation for other players)
            Vector2 targetPos;
            float armRotation;
            
            if (hasActiveHook)
            {
                // Find the active hook projectile
                targetPos = player.Center;
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile proj = Main.projectile[i];
                    if (proj.active && proj.owner == player.whoAmI && proj.type == ModContent.ProjectileType<HookshotProjectile>())
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
                // Local player aiming with mouse
                targetPos = Main.MouseWorld;
                Vector2 toTarget = targetPos - player.MountedCenter;
                armRotation = toTarget.ToRotation();
            }
            else
            {
                // Other player - use synced rotation
                armRotation = modPlayer.syncedArmRotation;
                targetPos = player.MountedCenter + armRotation.ToRotationVector2() * 100f;
            }

            // Get the hand position where the gauntlet should be centered
            Vector2 handPos = GetHandPosition(player, armRotation);

            // Determine sprite effects - flip vertically when pointing left so texture stays upright
            bool pointingLeft = Math.Abs(armRotation) > MathHelper.PiOver2;
            SpriteEffects effects = pointingLeft ? SpriteEffects.FlipVertically : SpriteEffects.None;

            // Body frame animation (only animates when hook is out)
            int frameCount = 3;
            int frameHeight = bodyTexture.Height / frameCount;
            int currentFrame = Math.Clamp(modPlayer.bodyAnimFrame, 0, frameCount - 1);
            Rectangle bodySourceRect = new Rectangle(0, currentFrame * frameHeight, bodyTexture.Width, frameHeight);

            float scale = 0.5f;
            Vector2 bodyOrigin = new Vector2(bodyTexture.Width / 2f, frameHeight / 2f);
            Color color = Lighting.GetColor((int)(player.Center.X / 16), (int)(player.Center.Y / 16));

            // Draw body centered on the hand/forearm position
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

            // Draw hook at front of gauntlet (only when not fired)
            if (!hasActiveHook && hookTexture != null)
            {
                // Hook extends from the front of the gauntlet
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
    /// Hookshot projectile - 60-block range, no enemy launch
    /// </summary>
    public class HookshotProjectile : BaseHookshotProjectile
    {
        protected override float MaxRange => 960f; // 60 blocks
        protected override int TimeoutFrames => 360; // 6 seconds
        protected override int CombatDamage => 50; // Item damage for dash hitbox
        protected override int DashHitboxType => ModContent.ProjectileType<HookshotDashHitbox>();
        protected override String BodyTexturePath => "SariaMod/Items/Bands/HookShotBody";
        protected override bool LaunchEnemyOnSuccess => false;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Hookshot");
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 2400;
        }
    }

    /// <summary>
    /// Hookshot dash hitbox
    /// </summary>
    public class HookshotDashHitbox : BaseHookshotDashHitbox
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Hookshot Dash");
        }
    }
}
