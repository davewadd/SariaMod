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
    /// Base class for Hookshot and Longshot projectiles
    /// Contains all shared logic - override abstract properties for differences
    /// </summary>
    public abstract class BaseHookshotProjectile : ModProjectile
    {
        // ===== ABSTRACT PROPERTIES - Override these in derived classes =====
        protected abstract float MaxRange { get; }
        protected abstract int TimeoutFrames { get; }
        protected abstract int CombatDamage { get; }
        protected abstract int DashHitboxType { get; }
        protected abstract string BodyTexturePath { get; }
        protected abstract bool LaunchEnemyOnSuccess { get; }

        // ===== SHARED CONSTANTS =====
        protected const float ArrivalDistance = 48f;
        protected const float MinPullDistanceForSuccess = 48f;
        protected const int EnemyHoldTime = HookshotConfig.EnemyHoldTime;
        protected const int SweetspotWindowStart = HookshotConfig.SweetspotWindowStart;
        protected const int SweetspotWindowEnd = HookshotConfig.SweetspotWindowEnd;
        protected const int SweetspotCueFrame = HookshotConfig.SweetspotCueFrame;
        protected const float CombatPullSpeed = 48f;

        // ===== STATE FLAGS =====
        protected bool IsAttached
        {
            get => Projectile.ai[0] == 1f;
            set => Projectile.ai[0] = value ? 1f : 0f;
        }

        protected bool IsRetracting
        {
            get => Projectile.ai[1] == 1f;
            set => Projectile.ai[1] = value ? 1f : 0f;
        }

        protected int AttachedTimer
        {
            get => (int)Projectile.localAI[0];
            set => Projectile.localAI[0] = value;
        }

        protected bool IsCombatMode
        {
            get => Projectile.localAI[1] == 1f;
            set => Projectile.localAI[1] = value ? 1f : 0f;
        }

        // ===== COMBAT MODE STATE =====
        protected bool _isAttachedToEnemy = false;
        protected int _attachedNPCIndex = -1;
        protected Vector2 _enemyAttachOffset = Vector2.Zero;
        protected bool _isPullingThroughEnemy = false;
        protected bool _wasRightClickPressed = false;
        protected bool _earlyClickDamageMode = false;
        protected bool _dashHitboxSpawned = false;
        protected bool _hasAppliedFirstHitKnockback = false;
        protected bool _hasRestoredWingTime = false;
        protected bool _hitSweetspot = false;
        protected bool _sweetspotCuePlayed = false;
        protected int _pullTimer = 0;

        // ===== GRAPPLE MODE STATE =====
        protected bool _hasPiercedEnemyInGrappleMode = false;
        protected bool _grappleDashHitboxSpawned = false;

        // ===== GENERAL STATE =====
        protected Vector2 _playerVelocityOnFire = Vector2.Zero;
        protected Vector2 _spawnPosition = Vector2.Zero;
        protected Vector2 _playerPosOnAttach = Vector2.Zero;
        protected bool _didAttach = false;
        protected int _lastTileCollisionFrame = -1;
        protected float _initialRotation = 0f;
        protected bool _hasInitialRotation = false;

        public override string Texture => "SariaMod/Items/Bands/HookshotHook";

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.tileCollide = true;
            Projectile.timeLeft = 9999;
            Projectile.aiStyle = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.extraUpdates = 2;
            Projectile.netImportant = true;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(_isAttachedToEnemy);
            writer.Write(_attachedNPCIndex);
            writer.Write(_enemyAttachOffset.X);
            writer.Write(_enemyAttachOffset.Y);
            writer.Write(_isPullingThroughEnemy);
            writer.Write(_earlyClickDamageMode);
            writer.Write(_hasAppliedFirstHitKnockback);
            writer.Write(AttachedTimer);
            writer.Write(_hitSweetspot);
            writer.Write(_sweetspotCuePlayed);
            writer.Write(IsCombatMode);
            writer.Write(_pullTimer);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            _isAttachedToEnemy = reader.ReadBoolean();
            _attachedNPCIndex = reader.ReadInt32();
            _enemyAttachOffset.X = reader.ReadSingle();
            _enemyAttachOffset.Y = reader.ReadSingle();
            _isPullingThroughEnemy = reader.ReadBoolean();
            _earlyClickDamageMode = reader.ReadBoolean();
            _hasAppliedFirstHitKnockback = reader.ReadBoolean();
            AttachedTimer = reader.ReadInt32();
            _hitSweetspot = reader.ReadBoolean();
            _sweetspotCuePlayed = reader.ReadBoolean();
            IsCombatMode = reader.ReadBoolean();
            _pullTimer = reader.ReadInt32();
        }

        public override void OnSpawn(IEntitySource source)
        {
            Player player = Main.player[Projectile.owner];
            _playerVelocityOnFire = player.velocity;
            _spawnPosition = Projectile.Center;
            _playerPosOnAttach = Vector2.Zero;
            _didAttach = false;
            _isAttachedToEnemy = false;
            _attachedNPCIndex = -1;
            _isPullingThroughEnemy = false;
            _wasRightClickPressed = true;
            _earlyClickDamageMode = false;
            _dashHitboxSpawned = false;
            _hasAppliedFirstHitKnockback = false;
            _hasRestoredWingTime = false;
            _hasPiercedEnemyInGrappleMode = false;
            _grappleDashHitboxSpawned = false;
            _lastTileCollisionFrame = -1;
            _hitSweetspot = false;
            _sweetspotCuePlayed = false;
            _pullTimer = 0;

            if (Main.myPlayer == Projectile.owner)
            {
                SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/HookshotStart") { Volume = 0.7f }, player.Center);
                HookshotSyncMessage.SendSound(HookshotSoundType.Start, player.Center, Projectile.owner);

                HookshotPlayer hookshotPlayer = player.GetModPlayer<HookshotPlayer>();
                hookshotPlayer.loopSoundTimer = 0;
            }
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            // Kill hook if player is dead or incapacitated
            if (!player.active || player.dead || player.frozen || player.stoned || player.webbed)
            {
                Projectile.Kill();
                return;
            }

            if (Main.myPlayer == Projectile.owner)
            {
                HookshotPlayer hookshotPlayer = player.GetModPlayer<HookshotPlayer>();
                if (SoundEngine.TryGetActiveSound(hookshotPlayer.loopingSoundSlot, out var sound))
                {
                    sound.Position = Projectile.Center;
                }

                if (Projectile.Center.X > player.Center.X)
                    player.direction = 1;
                else
                    player.direction = -1;
            }

            if (IsCombatMode)
            {
                CombatModeAI(player);
            }
            else
            {
                GrappleModeAI(player);
            }

            if (IsAttached || _isAttachedToEnemy)
            {
                Vector2 fromPlayer = Projectile.Center - player.Center;
                Projectile.rotation = fromPlayer.ToRotation();
            }
            else if (Projectile.velocity != Vector2.Zero)
            {
                Projectile.rotation = Projectile.velocity.ToRotation();
            }
        }

        protected virtual void GrappleModeAI(Player player)
        {
            if (Main.myPlayer != Projectile.owner)
            {
                if (IsRetracting)
                {
                    Vector2 toPlayer = player.Center - Projectile.Center;
                    float distance = toPlayer.Length();

                    if (distance < 32f)
                    {
                        Projectile.Kill();
                        return;
                    }

                    Projectile.velocity = toPlayer.SafeNormalize(Vector2.Zero) * 32f;
                    Projectile.tileCollide = false;
                }
                else if (IsAttached)
                {
                    Projectile.velocity = Vector2.Zero;
                    Projectile.tileCollide = false;
                }
                return;
            }

            if (IsAttached)
            {
                Projectile.tileCollide = false;
                Projectile.velocity = Vector2.Zero;

                AttachedTimer++;
                if (AttachedTimer >= TimeoutFrames)
                {
                    DisconnectWithSlingshot(player);
                    return;
                }

                Vector2 toHook = Projectile.Center - player.Center;
                float distance = toHook.Length();

                if (distance > ArrivalDistance)
                {
                    Vector2 pullDirection = toHook.SafeNormalize(Vector2.Zero);
                    float pullSpeed = Math.Max(distance * 0.2f, 12f);
                    pullSpeed = Math.Min(pullSpeed, 24f);
                    player.velocity = pullDirection * pullSpeed;
                    player.fallStart = (int)(player.position.Y / 16f);

                    player.grappling[0] = Projectile.whoAmI;
                    player.grapCount = 1;

                    if (_hasPiercedEnemyInGrappleMode)
                    {
                        player.immune = true;
                        player.immuneTime = 10;
                        player.immuneNoBlink = true;
                    }

                    if (Main.netMode == NetmodeID.MultiplayerClient)
                    {
                        NetMessage.SendData(MessageID.PlayerControls, -1, -1, null, player.whoAmI);
                    }
                }
                else
                {
                    DisconnectWithSlingshot(player);
                    return;
                }

                if (Main.mouseRight)
                {
                    DisconnectWithSlingshot(player);
                    return;
                }
            }
            else if (IsRetracting)
            {
                Vector2 toPlayer = player.Center - Projectile.Center;
                float distance = toPlayer.Length();

                if (distance < 32f)
                {
                    Projectile.Kill();
                    return;
                }

                Projectile.velocity = toPlayer.SafeNormalize(Vector2.Zero) * 32f;
                Projectile.tileCollide = false;

                if (_hasPiercedEnemyInGrappleMode)
                {
                    player.immune = true;
                    player.immuneTime = 10;
                    player.immuneNoBlink = true;
                }
            }
            else
            {
                if (CheckForGrabbableTile())
                {
                    IsAttached = true;
                    _didAttach = true;
                    _playerPosOnAttach = player.Center;
                    AttachedTimer = 0;
                    Projectile.velocity = Vector2.Zero;

                    if (_hasPiercedEnemyInGrappleMode && !_grappleDashHitboxSpawned)
                    {
                        _grappleDashHitboxSpawned = true;
                        SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Hookshothit") { Volume = 0.8f }, Projectile.Center);
                        HookshotSyncMessage.SendSound(HookshotSoundType.Hit, Projectile.Center, Projectile.owner);

                        Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(),
                            player.Center,
                            Vector2.Zero,
                            DashHitboxType,
                            CombatDamage,
                            0f,
                            Projectile.owner
                        );
                    }
                    else
                    {
                        SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Hookshothit") {
Volume = 0.8f }, Projectile.Center);
                        HookshotSyncMessage.SendSound(HookshotSoundType.Hit, Projectile.Center, Projectile.owner);
                    }

                    Projectile.netUpdate = true;
                }

                float distanceFromPlayer = Vector2.Distance(Projectile.Center, player.Center);

                if (distanceFromPlayer > MaxRange)
                {
                    StartRetract(player);
                }
            }
        }

        protected virtual void CombatModeAI(Player player)
        {
            if (Main.myPlayer != Projectile.owner)
            {
                // Remote client - sync visuals based on state
                if (IsRetracting)
                {
                    Vector2 toPlayer = player.Center - Projectile.Center;
                    float distance = toPlayer.Length();

                    if (distance < 32f)
                    {
                        Projectile.Kill();
                        return;
                    }

                    Projectile.velocity = toPlayer.SafeNormalize(Vector2.Zero) * 32f;
                    Projectile.tileCollide = false;
                }
                else if (_isAttachedToEnemy && _attachedNPCIndex >= 0 && _attachedNPCIndex < Main.maxNPCs)
                {
                    NPC target = Main.npc[_attachedNPCIndex];
                    if (target.active)
                    {
                        Projectile.Center = target.Center;
                    }
                    Projectile.velocity = Vector2.Zero;
                    Projectile.tileCollide = false;

                    // Increment timer locally for visual sync (charge ring)
                    if (!_isPullingThroughEnemy)
                    {
                        AttachedTimer++;
                    }

                    // Handle pulling state for remote clients (mirage effect)
                    if (_isPullingThroughEnemy && _hitSweetspot)
                    {
                        player.armorEffectDrawShadow = true;
                        player.armorEffectDrawOutlines = true;
                    }
                }
                return;
            }

            bool rightClickNow = Main.mouseRight;
            bool rightClickJustPressed = rightClickNow && !_wasRightClickPressed;
            _wasRightClickPressed = rightClickNow;


            if (_isAttachedToEnemy)
            {
                NPC target = null;
                if (_attachedNPCIndex >= 0 && _attachedNPCIndex < Main.maxNPCs)
                {
                    target = Main.npc[_attachedNPCIndex];
                }





                bool npcDead = target == null || !target.active || target.life <= 0;

                if (npcDead)
                {
                    StartRetract(player);
                    return;
                }

                // Keep hook attached to enemy (follows them locally)
                Projectile.Center = target.Center;
                Projectile.velocity = Vector2.Zero;
                Projectile.tileCollide = false;

                AttachedTimer++;

                // Sync state frequently while attached so remote clients stay updated
                if (Main.netMode == NetmodeID.MultiplayerClient && AttachedTimer % 5 == 1)
                {
                    Projectile.netUpdate = true;
                }

                // NO FREEZING - both player and enemy move freely

                if (!_isPullingThroughEnemy)
                {
                    // Light ring is drawn in PostDraw - just add light here
                    const int transitionStart = 80;
                    float transitionProgress = 0f;

                    if (AttachedTimer >= SweetspotCueFrame)
                    {
                        transitionProgress = 1f;
                    }
                    else if (AttachedTimer > transitionStart)
                    {
                        transitionProgress = (float)(AttachedTimer - transitionStart) / (SweetspotCueFrame - transitionStart);
                    }


                    float ringRadius = MathHelper.Lerp(30f, 40f, transitionProgress);

                    // Add actual light to the world at each orb position
                    Color blueLight = new Color(0.2f, 0.5f, 1f);
                    Color redLight = new Color(1f, 0.4f, 0.2f);
                    Vector3 lightColor = Vector3.Lerp(blueLight.ToVector3(), redLight.ToVector3(), transitionProgress);
                    float lightIntensity = MathHelper.Lerp(0.4f, 0.8f, transitionProgress);

                    for (int i = 0; i < 8; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 8f + (AttachedTimer * 0.05f);
                        Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * ringRadius;
                        Vector2 lightPos = player.Center + offset;
                        Lighting.AddLight(lightPos, lightColor * lightIntensity);
                    }

                    // Play sweetspot timing cue sound at frame 160 with dramatic "POP" burst
                    if (AttachedTimer == SweetspotCueFrame && !_sweetspotCuePlayed)
                    {
                        _sweetspotCuePlayed = true;
                        SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Hookshotset") { Volume = 0.8f }, player.Center);
                        HookshotSyncMessage.SendSound(HookshotSoundType.Set, player.Center, Projectile.owner);

                        // DRAMATIC POP BURST - expanding ring of red particles
                        Color burstColor = new Color(255, 100, 50);
                        for (int i = 0; i < 16; i++)
                        {
                            float angle = MathHelper.TwoPi * i / 16f;
                            Vector2 dustVel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 5f;
                            Dust burst = Dust.NewDustPerfect(player.Center, DustID.FireworksRGB, dustVel, 0, burstColor, 2f);
                            burst.noGravity = true;
                        }
                        // Secondary inner burst for more impact
                        for (int i = 0; i < 8; i++)
                        {
                            float angle = MathHelper.TwoPi * i / 8f + MathHelper.PiOver4;
                            Vector2 dustVel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 3f;
                            Dust innerBurst = Dust.NewDustPerfect(player.Center, DustID.FireworksRGB, dustVel, 0, Color.White, 1.5f);
                            innerBurst.noGravity = true;
                        }
                        // Add a bright center flash
                        Lighting.AddLight(player.Center, 1f, 0.5f, 0.2f);
                    }

                    bool inSweetspotWindow = AttachedTimer >= SweetspotWindowStart && AttachedTimer <= SweetspotWindowEnd;
                    bool holdTimeExpired = AttachedTimer > EnemyHoldTime;
                    bool hasDashCooldown = player.HasBuff(ModContent.BuffType<DashDeBuff>());

                    if (rightClickJustPressed)
                    {
                        if (inSweetspotWindow)
                        {
                            // SWEETSPOT HIT! Double damage, invulnerability dash
                            // Works regardless of cooldown debuff
                            _hitSweetspot = true;
                            _isPullingThroughEnemy = true;
                            _playerPosOnAttach = player.Center;

                            // Set flag to allow passing through platforms during pull
                            player.GetModPlayer<HookshotPlayer>().isPullingToEnemy = true;

                            // Apply dash cooldown debuff (5 seconds)
                            player.AddBuff(ModContent.BuffType<DashDeBuff>(), HookshotConfig.DashCooldownDuration);

                            // Spawn sweetspot dash hitbox - ai[0]=1 for combat mode, ai[1]=1 for sweetspot
                            int dashDamage = CombatDamage * 2; // Double damage for sweetspot
                            Projectile.NewProjectile(
                                Projectile.GetSource_FromThis(),
                                player.Center,
                                Vector2.Zero,
                                DashHitboxType,
                                dashDamage,
                                0f,
                                Projectile.owner,
                                ai0: 1f, // Combat mode flag
                                ai1: 1f  // Sweetspot flag - triggers orange + blue dust
                            );

                            SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Hookshothit") { Volume = 0.8f }, Projectile.Center);
                            HookshotSyncMessage.SendSound(HookshotSoundType.Hit, Projectile.Center, Projectile.owner);

                            Projectile.netUpdate = true;
                        }
                        else
                        {
                            // Early release - just disengage
                            SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/HookshotStun") { Volume = 0.8f }, target.Center);
                            HookshotSyncMessage.SendSound(HookshotSoundType.Stun, target.Center, Projectile.owner);

                            StartRetract(player);
                            return;
                        }
                    }

                    if (holdTimeExpired)
                    {
                        if (hasDashCooldown)
                        {
                            // Player has dash cooldown - treat as early release (no auto-dash)
                            SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/HookshotStun") { Volume = 0.8f }, target.Center);
                            HookshotSyncMessage.SendSound(HookshotSoundType.Stun, target.Center, Projectile.owner);

                            StartRetract(player);
                            return;
                        }
                        else
                        {
                            // No cooldown - auto-pull with normal damage
                            _hitSweetspot = false;
                            _isPullingThroughEnemy = true;
                            _playerPosOnAttach = player.Center;

                            // Set flag to allow passing through platforms during pull
                            player.GetModPlayer<HookshotPlayer>().isPullingToEnemy = true;

                            // Apply dash cooldown debuff for auto-dash too
                            player.AddBuff(ModContent.BuffType<DashDeBuff>(), HookshotConfig.DashCooldownDuration);

                            // Spawn normal dash hitbox - ai[0]=1 for combat mode, ai[1]=0 for normal
                            Projectile.NewProjectile(
                                Projectile.GetSource_FromThis(),
                                player.Center,
                                Vector2.Zero,
                                DashHitboxType,
                                CombatDamage,
                                0f,
                                Projectile.owner,
                                ai0: 1f, // Combat mode flag
                                ai1: 0f  // Normal (not sweetspot)
                            );

                            SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Hookshothit") { Volume = 0.8f }, Projectile.Center);
                            HookshotSyncMessage.SendSound(HookshotSoundType.Hit, Projectile.Center, Projectile.owner);

                            Projectile.netUpdate = true;
                        }
                    }
                }
                else
                {
                    // Pulling through enemy - dash to target
                    _pullTimer++;

                    // Timeout check - disconnect if pull takes too long (8 seconds)
                    if (_pullTimer >= HookshotConfig.PullTimeoutFrames)
                    {
                        StartRetract(player);
                        return;
                    }

                    Vector2 toHook = Projectile.Center - player.Center;
                    float distance = toHook.Length();

                    // Invulnerability during dash for both normal and sweetspot
                    player.immune = true;
                    player.immuneTime = 10;
                    player.immuneNoBlink = true;

                    // Allow player to fall through platforms during pull
                    player.GoingDownWithGrapple = true;

                    // If player has water walking, they treat liquid surfaces as solid
                    // We need to bypass this so they can be pulled through liquids
                    if (player.waterWalk || player.waterWalk2)
                    {
                        player.ignoreWater = true;
                    }

                    // Sweetspot mirage/afterimage effect (similar to slimy saddle)
                    if (_hitSweetspot)
                    {
                        player.armorEffectDrawShadow = true;
                        player.armorEffectDrawOutlines = true;

                        // Extra trailing dust for sweetspot
                        if (Main.rand.NextBool(2))
                        {
                            Dust trail = Dust.NewDustPerfect(player.Center + Main.rand.NextVector2Circular(10f, 10f),
                                DustID.Torch, -player.velocity * 0.2f, 100, default, 1.2f);
                            trail.noGravity = true;

                            Dust trailBlue = Dust.NewDustPerfect(player.Center + Main.rand.NextVector2Circular(10f, 10f),
                                DustID.Electric, -player.velocity * 0.2f, 100, default, 0.8f);
                            trailBlue.noGravity = true;
                        }
                    }

                    if (distance > 32f)
                    {
                        Vector2 pullDirection = toHook.SafeNormalize(Vector2.Zero);

                        // Set pull target for platform pass-through handling in HookshotPlayer.PostUpdate
                        HookshotPlayer modPlayer = player.GetModPlayer<HookshotPlayer>();
                        modPlayer.pullTargetPosition = Projectile.Center;

                        player.velocity = pullDirection * CombatPullSpeed;
                        player.gravity = 0f;
                        player.fallStart = (int)(player.position.Y / 16f);
                        player.runAcceleration = 0f;
                        player.maxRunSpeed = 0f;

                        if (Main.netMode == NetmodeID.MultiplayerClient)
                        {
                            NetMessage.SendData(MessageID.PlayerControls, -1, -1, null, player.whoAmI);
                        }
                    }
                    else
                    {
                        // SUCCESS! Player reached the hook - launch enemy upward
                        LaunchEnemyUpward(target, player);

                        if (_playerPosOnAttach != Vector2.Zero)
                        {
                            Vector2 throughDirection = (target.Center - _playerPosOnAttach).SafeNormalize(Vector2.UnitX * player.direction);
                            player.velocity = throughDirection * 20f;
                        }

                        if (Main.netMode == NetmodeID.MultiplayerClient)
                        {
                            HookshotSyncMessage.SendPlayerPullComplete(player.position, player.velocity, player.whoAmI, player.whoAmI);
                        }



                        StartRetract(player);
                        return;
                    }
                }
            }
            else if (IsRetracting)
            {
                Vector2 toPlayer = player.Center - Projectile.Center;
                float distance = toPlayer.Length();

                if (distance < 32f)
                {
                    Projectile.Kill();
                    return;
                }

                Projectile.velocity = toPlayer.SafeNormalize(Vector2.Zero) * 32f;
                Projectile.tileCollide = false;
            }
            else
            {
                int hitNPC = FindEnemyToHook();
                if (hitNPC >= 0)
                {
                    NPC target = Main.npc[hitNPC];

                    _isAttachedToEnemy = true;
                    _attachedNPCIndex = hitNPC;
                    _enemyAttachOffset = Vector2.Zero;
                    Projectile.Center = target.Center;
                    AttachedTimer = 0;
                    Projectile.velocity = Vector2.Zero;
                    _didAttach = true;
                    _playerPosOnAttach = player.Center;
                    _hitSweetspot = false;
                    _sweetspotCuePlayed = false;

                    Projectile.netUpdate = true;

                    if (!_hasAppliedFirstHitKnockback)
                    {
                        _hasAppliedFirstHitKnockback = true;

                        // Chip damage - only 1/10th of combat damage
                        int damage = CombatDamage / 10;

                        // Spawn HookDamager to handle initial hit damage
                        Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(),
                            Projectile.Center,
                            Vector2.Zero,
                            ModContent.ProjectileType<HookDamager>(),
                            damage,
                            0f,
                            Projectile.owner,
                            ai0: hitNPC,
                            ai1: LaunchEnemyOnSuccess ? 1f : 0f
                        );

                        // NO FREEZING - enemy continues to move

                        if (!_hasRestoredWingTime)
                        {
                            _hasRestoredWingTime = true;
                            player.wingTime = player.wingTimeMax;
                            player.rocketTime = player.rocketTimeMax;
                        }

                        for (int i = 0; i < 10; i++)
                        {
                            Vector2 dustVel = Main.rand.NextVector2CircularEdge(2f, 2f);
                            Dust.NewDust(target.Center, 0, 0, DustID.Electric, dustVel.X, dustVel.Y, 100, default, 1f);
                        }

                        Projectile.netUpdate = true;
                    }

                    SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/HookshotStun") { Volume = 0.8f }, target.Center);
                    HookshotSyncMessage.SendSound(HookshotSoundType.Stun, target.Center, Projectile.owner);

                    Projectile.netUpdate = true;
                    return;
                }

                float distanceFromPlayer = Vector2.Distance(Projectile.Center, player.Center);
                if (distanceFromPlayer > MaxRange)
                {
                    StartRetract(player);
                }
            }
        }

        protected void LaunchEnemyUpward(NPC target, Player player)
        {
            if (Main.myPlayer != Projectile.owner) return;

            float horizontalDir = target.Center.X > player.Center.X ? 1f : -1f;
            Vector2 launchVelocity = new Vector2(horizontalDir * 3f, -8f);

            target.velocity = launchVelocity;

            HookshotSyncMessage.SendLaunchEnemy(target.whoAmI, launchVelocity);

            for (int i = 0; i < 10; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2CircularEdge(2f, 2f);
                Dust.NewDust(target.Center, 0, 0, DustID.Electric, dustVel.X, dustVel.Y, 100, default, 1.2f);
            }
        }

        protected int FindEnemyToHook()
        {
            float checkRadius = 24f;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.immortal)
                    continue;

                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < checkRadius + Math.Max(npc.width, npc.height) / 2f)
                {
                    return i;
                }
            }

            return -1;
        }

        protected bool CheckForGrabbableTile()
        {
            int checkRadius = 1;
            int hookTileX = (int)(Projectile.Center.X / 16);
            int hookTileY = (int)(Projectile.Center.Y / 16);

            for (int x = hookTileX - checkRadius; x <= hookTileX + checkRadius; x++)
            {
                for (int y = hookTileY - checkRadius; y <= hookTileY + checkRadius; y++)
                {
                    if (x < 0 || x >= Main.maxTilesX || y < 0 || y >= Main.maxTilesY)
                        continue;

                    Tile tile = Main.tile[x, y];

                    if (!tile.HasTile)
                        continue;

                    bool isSolid = Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType];
                    bool isPlatform = TileID.Sets.Platforms[tile.TileType];
                    bool isSolidTop = Main.tileSolidTop[tile.TileType];

                    if (isSolid || isPlatform || isSolidTop)
                    {
                        Vector2 tileCenter = new Vector2(x * 16 + 8, y * 16 + 8);
                        float dist = Vector2.Distance(Projectile.Center, tileCenter);

                        if (dist < 20f)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            if (!IsCombatMode && !IsAttached && !IsRetracting)
            {
                _hasPiercedEnemyInGrappleMode = true;
                Projectile.netUpdate = true;
            }
        }

        public override bool? CanHitNPC(NPC target)
        {
            if (IsCombatMode && _isAttachedToEnemy)
            {
                if (target.whoAmI == _attachedNPCIndex)
                {
                    return false;
                }
            }

            if (_isAttachedToEnemy)
            {
                return false;
            }

            return null;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (_lastTileCollisionFrame == Main.GameUpdateCount)
            {
                Projectile.velocity = Vector2.Zero;
                return false;
            }
            _lastTileCollisionFrame = (int)Main.GameUpdateCount;

            if (Main.myPlayer != Projectile.owner)
            {
                if (!IsAttached && !IsRetracting && !_isAttachedToEnemy)
                {
                    Projectile.velocity = Vector2.Zero;
                }
                return false;
            }

            if (!IsAttached && !IsRetracting && !_isAttachedToEnemy)
            {
                Player player = Main.player[Projectile.owner];

                Projectile.velocity = Vector2.Zero;

                if (IsCombatMode)
                {
                    SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/HookshotClang") { Volume = 0.8f }, Projectile.Center);
                    HookshotSyncMessage.SendSound(HookshotSoundType.Clang, Projectile.Center, Projectile.owner);

                    for (int i = 0; i < 8; i++)
                    {
                        Vector2 dustVel = Main.rand.NextVector2CircularEdge(2f, 2f);
                        Dust.NewDust(Projectile.Center, 0, 0, DustID.Electric, dustVel.X, dustVel.Y, 100, default, 1f);
                    }

                    HookshotSyncMessage.SendTileImpact(Projectile.Center, Projectile.Center, Projectile.owner);

                    StartRetract(player);
                }
                else
                {
                    IsAttached = true;
                    _didAttach = true;
                    _playerPosOnAttach = player.Center;
                    AttachedTimer = 0;

                    SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Hookshothit") { Volume = 0.8f }, Projectile.Center);
                    HookshotSyncMessage.SendSound(HookshotSoundType.Hit, Projectile.Center, Projectile.owner);

                    Projectile.netUpdate = true;
                }
            }

            return false;
        }

        protected void StartRetract(Player player)
        {
            // Reset sweetspot state
            _hitSweetspot = false;
            _sweetspotCuePlayed = false;

            IsRetracting = true;
            IsAttached = false;
            _isAttachedToEnemy = false;
            _isPullingThroughEnemy = false;
            _hasAppliedFirstHitKnockback = false;

            // Clear platform pass-through flags
            HookshotPlayer modPlayer = player.GetModPlayer<HookshotPlayer>();
            modPlayer.isPullingToEnemy = false;
            modPlayer.pullTargetPosition = Vector2.Zero;

            Projectile.netUpdate = true;

            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, Projectile.whoAmI);
            }

            player.grapCount = 0;

            if (Main.myPlayer == Projectile.owner && !IsCombatMode)
            {
                bool shouldClang = false;
                float distanceFromSpawn = Vector2.Distance(Projectile.Center, _spawnPosition);

                if (!_didAttach)
                {
                    if (distanceFromSpawn < 160f)
                    {
                        shouldClang = true;
                    }
                }
                else if (_playerPosOnAttach != Vector2.Zero)
                {
                    float pullDistance = Vector2.Distance(player.Center, _playerPosOnAttach);
                    if (pullDistance < MinPullDistanceForSuccess && distanceFromSpawn < 160f)
                    {
                        shouldClang = true;
                    }
                }

                if (shouldClang)
                {
                    SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/HookshotClang") { Volume = 0.8f }, Projectile.Center);
                    HookshotSyncMessage.SendSound(HookshotSoundType.Clang, Projectile.Center, Projectile.owner);

                    for (int i = 0; i < 8; i++)
                    {
                        Vector2 dustVel = Main.rand.NextVector2CircularEdge(2f, 2f);
                        Dust.NewDust(Projectile.Center, 0, 0, DustID.Electric, dustVel.X, dustVel.Y, 100, default, 1f);
                    }

                    HookshotSyncMessage.SendTileImpact(Projectile.Center, Projectile.Center, Projectile.owner);
                }
            }
        }

        protected void DisconnectWithSlingshot(Player player)
        {
            HookshotPlayer modPlayer = player.GetModPlayer<HookshotPlayer>();

            modPlayer.fallDamageProtectionTimer = 60;

            if (modPlayer.slingshotCooldown <= 0)
            {
                Vector2 slingshot = player.velocity * 0.5f;
                modPlayer.slingshotVelocity = slingshot;
                modPlayer.shouldApplySlingshot = true;
            }

            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                modPlayer.needsPostPullSync = true;
                modPlayer.postPullSyncTimer = 0;
                HookshotSyncMessage.SendPlayerPullComplete(player.position, player.velocity, player.whoAmI, player.whoAmI);
            }

            StartRetract(player);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Player player = Main.player[Projectile.owner];

            Texture2D bodyTexture = ModContent.Request<Texture2D>(BodyTexturePath).Value;
            float scale = 0.5f;

            Vector2 toHook = Projectile.Center - player.MountedCenter;
            float armRotation = toHook.ToRotation();

            bool pointingLeft = Math.Abs(armRotation) > MathHelper.PiOver2;
            int armSide = pointingLeft ? -1 : 1;

            Vector2 shoulderOffset = new Vector2(-5 * armSide, -2);
            Vector2 shoulderPos = player.MountedCenter + shoulderOffset;
            float armLength = 10f;
            Vector2 handPos = shoulderPos + armRotation.ToRotationVector2() * armLength;

            float bodyHalfLength = bodyTexture != null ? (bodyTexture.Width / 2f) * scale : 12f;
            Vector2 chainStart = handPos + armRotation.ToRotationVector2() * bodyHalfLength;
            Vector2 hookCenter = Projectile.Center;

            DrawChain(chainStart, hookCenter);
            DrawHookHead(lightColor);

            return false;
        }

        protected void DrawChain(Vector2 start, Vector2 end)
        {
            Texture2D chainTexture = ModContent.Request<Texture2D>("SariaMod/Items/Bands/HookShotChain").Value;
            if (chainTexture == null) return;

            Vector2 direction = end - start;
            float distance = direction.Length();

            if (distance < 1f) return;

            direction.Normalize();
            float rotation = direction.ToRotation();

            float scale = 0.5f;
            float chainWidth = chainTexture.Width * scale;
            if (chainWidth < 1f) chainWidth = 1f;

            int numChains = (int)(distance / chainWidth) + 1;

            for (int i = 0; i < numChains; i++)
            {
                Vector2 chainPos = start + direction * (i * chainWidth + chainWidth / 2f);

                Vector2 screenPos = chainPos - Main.screenPosition;
                if (screenPos.X > -100 && screenPos.X < Main.screenWidth + 100 &&
                    screenPos.Y > -100 && screenPos.Y < Main.screenHeight + 100)
                {
                    Color chainColor = Lighting.GetColor((int)(chainPos.X / 16), (int)(chainPos.Y / 16));

                    Main.EntitySpriteDraw(
                        chainTexture,
                        screenPos,
                        null,
                        chainColor,
                        rotation,
                        new Vector2(chainTexture.Width / 2f, chainTexture.Height / 2f),
                        scale,
                        SpriteEffects.None,
                        0
                    );
                }
            }
        }

        protected void DrawHookHead(Color lightColor)
        {
            Texture2D headTexture = ModContent.Request<Texture2D>("SariaMod/Items/Bands/HookshotHook").Value;
            if (headTexture == null) return;

            Vector2 origin = new Vector2(0, headTexture.Height / 2f);
            float scale = 0.5f;

            float drawRotation = Projectile.rotation;
            if (IsRetracting)
            {
                if (_hasInitialRotation)
                {
                    drawRotation = _initialRotation;
                }
            }
            else if (!_hasInitialRotation && Projectile.velocity != Vector2.Zero)
            {
                _initialRotation = Projectile.rotation;
                _hasInitialRotation = true;
            }

            Main.EntitySpriteDraw(
                headTexture,
                Projectile.Center - Main.screenPosition,
                null,
                lightColor,
                drawRotation,
                origin,
                scale,
                SpriteEffects.None,
                0
            );
        }

        public override void PostDraw(Color lightColor)
        {
            // Only draw the charge ring when attached to enemy in combat mode and not pulling yet
            if (!IsCombatMode || !_isAttachedToEnemy || _isPullingThroughEnemy)
                return;

            Player player = Main.player[Projectile.owner];

            // Calculate transition progress
            const int transitionStart = 80;
            float transitionProgress = 0f;

            if (AttachedTimer >= SweetspotCueFrame)
            {
                transitionProgress = 1f;
            }
            else if (AttachedTimer > transitionStart)
            {
                transitionProgress = (float)(AttachedTimer - transitionStart) / (SweetspotCueFrame - transitionStart);
            }

            // Interpolate values
            float orbScale = MathHelper.Lerp(0.15f, 0.3f, transitionProgress);
            float ringRadius = MathHelper.Lerp(30f, 40f, transitionProgress);

            // Color interpolation: Blue -> Red
            Color blueColor = new Color(50, 150, 255);
            Color redColor = new Color(255, 100, 50);
            Color currentColor = Color.Lerp(blueColor, redColor, transitionProgress);

            // Use the circular glow texture (Extra[91] is a soft circular glow)
            Texture2D glowTexture = TextureAssets.Extra[91].Value;
            Vector2 glowOrigin = glowTexture.Size() / 2f;

            // Draw 8 light orbs in a ring around player
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f + (AttachedTimer * 0.05f);
                Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * ringRadius;
                Vector2 orbPos = player.Center + offset - Main.screenPosition;

                // Draw outer glow (larger, more transparent)
                Color glowColor = currentColor * 0.5f;
                Main.spriteBatch.Draw(
                    glowTexture,
                    orbPos,
                    null,
                    glowColor,
                    0f,
                    glowOrigin,
                    orbScale * 1.5f,
                    SpriteEffects.None,
                    0f
                );

                // Draw inner core (smaller, brighter)
                Color coreColor = Color.Lerp(currentColor, Color.White, 0.6f);
                Main.spriteBatch.Draw(
                    glowTexture,
                    orbPos,
                    null,
                    coreColor,
                    0f,
                    glowOrigin,
                    orbScale * 0.6f,
                    SpriteEffects.None,
                    0f
                );
            }

            // DEBUG: Draw red rectangle during sweetspot window
            if (HookshotConfig.ShowSweetspotDebugRectangle)
            {
                bool inSweetspotWindow = AttachedTimer >= SweetspotWindowStart && AttachedTimer <= SweetspotWindowEnd;
                if (inSweetspotWindow)
                {
                    Texture2D pixelTexture = TextureAssets.MagicPixel.Value;
                    Vector2 rectPos = player.Center - Main.screenPosition;

                    // Draw a pulsing red rectangle around the player
                    float pulse = (float)Math.Sin(AttachedTimer * 0.3f) * 0.3f + 0.7f; // Pulse between 0.4 and 1.0
                    Color rectColor = new Color(255, 50, 50) * pulse * 0.6f;

                    int rectWidth = 80;
                    int rectHeight = 100;

                    // Draw rectangle outline (4 sides)
                    int thickness = 4;

                    // Top
                    Main.spriteBatch.Draw(pixelTexture, new Rectangle((int)(rectPos.X - rectWidth/2), (int)(rectPos.Y - rectHeight/2), rectWidth, thickness), rectColor);
                    // Bottom
                    Main.spriteBatch.Draw(pixelTexture, new Rectangle((int)(rectPos.X - rectWidth/2), (int)(rectPos.Y + rectHeight/2 - thickness), rectWidth, thickness), rectColor);
                    // Left
                    Main.spriteBatch.Draw(pixelTexture, new Rectangle((int)(rectPos.X - rectWidth/2), (int)(rectPos.Y - rectHeight/2), thickness, rectHeight), rectColor);
                    // Right
                    Main.spriteBatch.Draw(pixelTexture, new Rectangle((int)(rectPos.X + rectWidth/2 - thickness), (int)(rectPos.Y - rectHeight/2), thickness, rectHeight), rectColor);
                }
            }
        }

        public override void Kill(int timeLeft)
        {
            Player player = Main.player[Projectile.owner];
            player.grapCount = 0;

            // Clear platform pass-through flags
            HookshotPlayer modPlayer = player.GetModPlayer<HookshotPlayer>();
            modPlayer.isPullingToEnemy = false;
            modPlayer.pullTargetPosition = Vector2.Zero;

            if (Main.myPlayer == Projectile.owner)
            {
                HookshotPlayer hookshotPlayer = player.GetModPlayer<HookshotPlayer>();
                if (SoundEngine.TryGetActiveSound(hookshotPlayer.loopingSoundSlot, out var sound))
                {
                    sound.Stop();
                    hookshotPlayer.loopingSoundSlot = SlotId.Invalid;
                }

                SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Hookshotset") { Volume = 0.7f }, player.Center);
                HookshotSyncMessage.SendSound(HookshotSoundType.Set, player.Center, Projectile.owner);
            }
        }
    }
}
