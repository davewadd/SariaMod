using System;
using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using SariaMod.Items.Bands;

namespace SariaMod.Netcode.HookshotNetworking
{
    /// <summary>
    /// Packet types for hookshot multiplayer sync
    /// </summary>
    public enum HookshotPacketType : byte
    {
        ArmSync = 0,
        PlaySound = 1,
        FreezeEnemy = 2,
        KnockbackEnemy = 3,
        LaunchEnemy = 4,
        ChipDamage = 5,
        TileImpact = 6,         // Sync dust effects for clang (failed hook)
        PlayerPullPosition = 7, // Sync player position during pull
        HookAttach = 8,         // Sync exact hook position and player position on successful attach
        HookRetract = 9,        // Force hook to retract immediately on all clients
        PlayerPullComplete = 10, // Sync player position and velocity when pull completes
        LoopSoundSync = 11,     // Sync looping sound state (playing/stopped)
        FreezeStart = 12,       // NEW: Start freeze state for player and NPC
        FreezeEnd = 13,         // NEW: End freeze state for player and NPC
        DealDamageToNPC = 14,   // Server-authoritative NPC damage (prevents resurrection bug)
        NPCKilled = 15          // Notify clients that NPC was killed by hookshot
    }

    /// <summary>
    /// Sound types for hookshot
    /// </summary>
    public enum HookshotSoundType : byte
    {
        Start = 0,
        Hit = 1,
        Clang = 2,
        Set = 3,
        Loop = 4,
        Stun = 5  // NEW: Played when hook latches to enemy
    }

    /// <summary>
    /// Handles all hookshot networking packets
    /// </summary>
    public static class HookshotSyncMessage
    {
        internal const byte PacketId = 254; // Unique packet ID (250=Saria, 251=IceDome, 252=FrozenGore/TileGlow, 253=FrozenTimer)

        #region Send Methods

        /// <summary>
        /// Send arm rotation and direction sync to other clients - called every frame for smooth sync
        /// </summary>
        public static void SendArmSync(Player player, float rotation, bool hasHook, bool isHolding, int direction)
        {
            if (Main.netMode == NetmodeID.SinglePlayer) return;

            ModPacket packet = SariaMod.Instance.GetPacket();
            packet.Write(PacketId);
            packet.Write((byte)HookshotPacketType.ArmSync);
            packet.Write((byte)player.whoAmI);
            packet.Write(rotation);
            packet.Write(hasHook);
            packet.Write(isHolding);
            packet.Write((sbyte)direction); // Add direction to packet
            packet.Send(-1, player.whoAmI);
        }

        /// <summary>
        /// Send a hookshot sound to other clients
        /// </summary>
        public static void SendSound(HookshotSoundType soundType, Vector2 position, int fromWho = -1)
        {
            if (Main.netMode == NetmodeID.SinglePlayer) return;

            ModPacket packet = SariaMod.Instance.GetPacket();
            packet.Write(PacketId);
            packet.Write((byte)HookshotPacketType.PlaySound);
            packet.Write((byte)soundType);
            packet.Write(position.X);
            packet.Write(position.Y);
            packet.Send(-1, fromWho);
        }

        /// <summary>
        /// Send a packet to freeze an enemy in place
        /// </summary>
        public static void SendFreezeEnemy(int npcIndex, int fromWho = -1, int toWho = -1)
        {
            if (Main.netMode == NetmodeID.SinglePlayer) return;

            ModPacket packet = SariaMod.Instance.GetPacket();
            packet.Write(PacketId);
            packet.Write((byte)HookshotPacketType.FreezeEnemy);
            packet.Write(npcIndex);
            packet.Send(toWho, fromWho);
        }

        /// <summary>
        /// Send a packet to knockback an enemy (first hit effect)
        /// </summary>
        public static void SendKnockbackEnemy(int npcIndex, Vector2 knockbackDir, int damage, int fromWho = -1, int toWho = -1)
        {
            if (Main.netMode == NetmodeID.SinglePlayer) return;

            ModPacket packet = SariaMod.Instance.GetPacket();
            packet.Write(PacketId);
            packet.Write((byte)HookshotPacketType.KnockbackEnemy);
            packet.Write(npcIndex);
            packet.Write(knockbackDir.X);
            packet.Write(knockbackDir.Y);
            packet.Write(damage);
            packet.Send(toWho, fromWho);
        }

        /// <summary>
        /// Send a packet to launch an enemy upward
        /// </summary>
        public static void SendLaunchEnemy(int npcIndex, Vector2 launchVelocity, int fromWho = -1, int toWho = -1)
        {
            if (Main.netMode == NetmodeID.SinglePlayer) return;

            ModPacket packet = SariaMod.Instance.GetPacket();
            packet.Write(PacketId);
            packet.Write((byte)HookshotPacketType.LaunchEnemy);
            packet.Write(npcIndex);
            packet.Write(launchVelocity.X);
            packet.Write(launchVelocity.Y);
            packet.Send(toWho, fromWho);
        }

        /// <summary>
        /// Send a packet to deal chip damage to an enemy
        /// </summary>
        public static void SendChipDamage(int npcIndex, int damage, int fromWho = -1, int toWho = -1)
        {
            if (Main.netMode == NetmodeID.SinglePlayer) return;

            ModPacket packet = SariaMod.Instance.GetPacket();
            packet.Write(PacketId);
            packet.Write((byte)HookshotPacketType.ChipDamage);
            packet.Write(npcIndex);
            packet.Write(damage);
            packet.Send(toWho, fromWho);
        }

        /// <summary>
        /// Send a packet to sync hook position and dust on tile impact (clang/failed hook)
        /// </summary>
        public static void SendTileImpact(Vector2 hookPosition, Vector2 dustPosition, int fromWho = -1)
        {
            if (Main.netMode == NetmodeID.SinglePlayer) return;

            ModPacket packet = SariaMod.Instance.GetPacket();
            packet.Write(PacketId);
            packet.Write((byte)HookshotPacketType.TileImpact);
            packet.Write(hookPosition.X);
            packet.Write(hookPosition.Y);
            packet.Write(dustPosition.X);
            packet.Write(dustPosition.Y);
            packet.Send(-1, fromWho);
        }

        /// <summary>
        /// Send a packet to sync player position during pull
        /// </summary>
        public static void SendPlayerPullPosition(Vector2 pullPosition, int playerId, int fromWho = -1)
        {
            if (Main.netMode == NetmodeID.SinglePlayer) return;

            ModPacket packet = SariaMod.Instance.GetPacket();
            packet.Write(PacketId);
            packet.Write((byte)HookshotPacketType.PlayerPullPosition);
            packet.Write(pullPosition.X);
            packet.Write(pullPosition.Y);
            packet.Write((byte)playerId);
            packet.Send(-1, fromWho);
        }

        /// <summary>
        /// Send a packet to sync exact hook position and player position on successful attachment
        /// Called once when hook successfully attaches to a tile
        /// </summary>
        public static void SendHookAttach(Vector2 hookPosition, Vector2 playerPosition, int playerId, int fromWho = -1)
        {
            if (Main.netMode == NetmodeID.SinglePlayer) return;

            ModPacket packet = SariaMod.Instance.GetPacket();
            packet.Write(PacketId);
            packet.Write((byte)HookshotPacketType.HookAttach);
            packet.Write(hookPosition.X);
            packet.Write(hookPosition.Y);
            packet.Write(playerPosition.X);
            packet.Write(playerPosition.Y);
            packet.Write((byte)playerId);
            packet.Send(-1, fromWho);
        }

        /// <summary>
        /// Send a packet to force hook retract on all clients immediately
        /// </summary>
        public static void SendHookRetract(Vector2 hookPosition, int ownerId, int fromWho = -1)
        {
            if (Main.netMode == NetmodeID.SinglePlayer) return;

            ModPacket packet = SariaMod.Instance.GetPacket();
            packet.Write(PacketId);
            packet.Write((byte)HookshotPacketType.HookRetract);
            packet.Write(hookPosition.X);
            packet.Write(hookPosition.Y);
            packet.Write((byte)ownerId);
            packet.Send(-1, fromWho);
        }

        /// <summary>
        /// Send a packet to sync player position and velocity when pull completes
        /// </summary>
        public static void SendPlayerPullComplete(Vector2 position, Vector2 velocity, int playerId, int fromWho = -1)
        {
            if (Main.netMode == NetmodeID.SinglePlayer) return;

            ModPacket packet = SariaMod.Instance.GetPacket();
            packet.Write(PacketId);
            packet.Write((byte)HookshotPacketType.PlayerPullComplete);
            packet.Write(position.X);
            packet.Write(position.Y);
            packet.Write(velocity.X);
            packet.Write(velocity.Y);
            packet.Write((byte)playerId);
            packet.Send(-1, fromWho);
        }

        /// <summary>
        /// Send a packet to sync loop sound state (start/stop) for a player
        /// </summary>
        public static void SendLoopSoundSync(int playerId, bool isPlaying, Vector2 position, int fromWho = -1)
        {
            if (Main.netMode == NetmodeID.SinglePlayer) return;

            ModPacket packet = SariaMod.Instance.GetPacket();
            packet.Write(PacketId);
            packet.Write((byte)HookshotPacketType.LoopSoundSync);
            packet.Write((byte)playerId);
            packet.Write(isPlaying);
            packet.Write(position.X);
            packet.Write(position.Y);
            packet.Send(-1, fromWho);
        }

        /// <summary>
        /// Send a packet to start the freeze state for a player and NPC
        /// This uses the state-based freeze system where all clients locally enforce the freeze
        /// </summary>
        public static void SendFreezeStart(int playerIndex, int npcIndex, Vector2 playerPos, Vector2 npcPos, int fromWho = -1)
        {
            if (Main.netMode == NetmodeID.SinglePlayer) return;

            ModPacket packet = SariaMod.Instance.GetPacket();
            packet.Write(PacketId);
            packet.Write((byte)HookshotPacketType.FreezeStart);
            packet.Write((byte)playerIndex);
            packet.Write(npcIndex);
            packet.Write(playerPos.X);
            packet.Write(playerPos.Y);
            packet.Write(npcPos.X);
            packet.Write(npcPos.Y);
            packet.Send(-1, fromWho);
        }

        /// <summary>
        /// Send a packet to end the freeze state for a player and NPC
        /// </summary>
        public static void SendFreezeEnd(int playerIndex, int npcIndex, int fromWho = -1)
        {
            if (Main.netMode == NetmodeID.SinglePlayer) return;

            ModPacket packet = SariaMod.Instance.GetPacket();
            packet.Write(PacketId);
            packet.Write((byte)HookshotPacketType.FreezeEnd);
            packet.Write((byte)playerIndex);
            packet.Write(npcIndex);
            packet.Send(-1, fromWho);
        }

        /// <summary>
        /// Send a packet to deal damage to an NPC - SERVER AUTHORITATIVE
        /// This ensures the server controls NPC death to prevent resurrection bugs in multiplayer.
        /// The server will apply the damage and broadcast the result to all clients.
        /// </summary>
        /// <param name="npcIndex">The NPC whoAmI index</param>
        /// <param name="damage">Damage amount to deal</param>
        /// <param name="knockback">Knockback amount</param>
        /// <param name="hitDirection">Direction of the hit (-1 or 1)</param>
        /// <param name="fromWho">Player whoAmI who dealt the damage</param>
        public static void SendDealDamageToNPC(int npcIndex, int damage, float knockback, int hitDirection, int fromWho)
        {
            if (Main.netMode == NetmodeID.SinglePlayer) return;

            ModPacket packet = SariaMod.Instance.GetPacket();
            packet.Write(PacketId);
            packet.Write((byte)HookshotPacketType.DealDamageToNPC);
            packet.Write(npcIndex);
            packet.Write(damage);
            packet.Write(knockback);
            packet.Write((sbyte)hitDirection);
            packet.Write((byte)fromWho);
            
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                packet.Send();
            }
            else if (Main.netMode == NetmodeID.Server)
            {
                packet.Send(-1, fromWho);
            }
        }

        /// <summary>
        /// Send notification that an NPC was killed by hookshot
        /// </summary>
        public static void SendNPCKilled(int npcIndex, int killerPlayerIndex, int fromWho = -1)
        {
            if (Main.netMode == NetmodeID.SinglePlayer) return;

            ModPacket packet = SariaMod.Instance.GetPacket();
            packet.Write(PacketId);
            packet.Write((byte)HookshotPacketType.NPCKilled);
            packet.Write(npcIndex);
            packet.Write((byte)killerPlayerIndex);
            packet.Send(-1, fromWho);
        }

        #endregion

        #region Receive/Handle Methods

        /// <summary>
        /// Handle incoming hookshot packets - called from SariaMod.HandlePacket
        /// </summary>
        public static void HandlePacket(BinaryReader reader, int whoAmI)
        {
            HookshotPacketType packetType = (HookshotPacketType)reader.ReadByte();

            switch (packetType)
            {
                case HookshotPacketType.ArmSync:
                    HandleArmSync(reader, whoAmI);
                    break;
                case HookshotPacketType.PlaySound:
                    HandlePlaySound(reader, whoAmI);
                    break;
                case HookshotPacketType.FreezeEnemy:
                    HandleFreezeEnemy(reader, whoAmI);
                    break;
                case HookshotPacketType.KnockbackEnemy:
                    HandleKnockbackEnemy(reader, whoAmI);
                    break;
                case HookshotPacketType.LaunchEnemy:
                    HandleLaunchEnemy(reader, whoAmI);
                    break;
                case HookshotPacketType.ChipDamage:
                    HandleChipDamage(reader, whoAmI);
                    break;
                case HookshotPacketType.TileImpact:
                    HandleTileImpact(reader, whoAmI);
                    break;
                case HookshotPacketType.PlayerPullPosition:
                    HandlePlayerPullPosition(reader, whoAmI);
                    break;
                case HookshotPacketType.HookAttach:
                    HandleHookAttach(reader, whoAmI);
                    break;
                case HookshotPacketType.HookRetract:
                    HandleHookRetract(reader, whoAmI);
                    break;
                case HookshotPacketType.PlayerPullComplete:
                    HandlePlayerPullComplete(reader, whoAmI);
                    break;
                case HookshotPacketType.LoopSoundSync:
                    HandleLoopSoundSync(reader, whoAmI);
                    break;
                case HookshotPacketType.FreezeStart:
                    HandleFreezeStart(reader, whoAmI);
                    break;
                case HookshotPacketType.FreezeEnd:
                    HandleFreezeEnd(reader, whoAmI);
                    break;
                case HookshotPacketType.DealDamageToNPC:
                    HandleDealDamageToNPC(reader, whoAmI);
                    break;
                case HookshotPacketType.NPCKilled:
                    HandleNPCKilled(reader, whoAmI);
                    break;
            }
        }

        private static void HandleArmSync(BinaryReader reader, int whoAmI)
        {
            int playerIndex = reader.ReadByte();
            float rotation = reader.ReadSingle();
            bool hasHook = reader.ReadBoolean();
            bool isHolding = reader.ReadBoolean();
            int direction = reader.ReadSByte();

            if (playerIndex >= 0 && playerIndex < Main.maxPlayers)
            {
                Player player = Main.player[playerIndex];
                if (player.active)
                {
                    HookshotPlayer modPlayer = player.GetModPlayer<HookshotPlayer>();
                    modPlayer.syncedArmRotation = rotation;
                    modPlayer.hasActiveHookForArm = hasHook;
                    modPlayer.isHoldingHookshot = isHolding;
                    modPlayer.syncedDirection = direction;

                    if (hasHook || isHolding)
                    {
                        player.direction = direction;
                        player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, rotation - MathHelper.PiOver2);
                    }
                }
            }

            if (Main.netMode == NetmodeID.Server)
            {
                ModPacket packet = SariaMod.Instance.GetPacket();
                packet.Write(PacketId);
                packet.Write((byte)HookshotPacketType.ArmSync);
                packet.Write((byte)playerIndex);
                packet.Write(rotation);
                packet.Write(hasHook);
                packet.Write(isHolding);
                packet.Write((sbyte)direction);
                packet.Send(-1, whoAmI);
            }
        }

        private static void HandlePlaySound(BinaryReader reader, int whoAmI)
        {
            HookshotSoundType soundType = (HookshotSoundType)reader.ReadByte();
            float posX = reader.ReadSingle();
            float posY = reader.ReadSingle();
            Vector2 position = new Vector2(posX, posY);

            string soundPath = soundType switch
            {
                HookshotSoundType.Start => "SariaMod/Sounds/HookshotStart",
                HookshotSoundType.Hit => "SariaMod/Sounds/Hookshothit",
                HookshotSoundType.Clang => "SariaMod/Sounds/HookshotClang",
                HookshotSoundType.Set => "SariaMod/Sounds/Hookshotset",
                HookshotSoundType.Loop => "SariaMod/Sounds/HookshotLoop",
                HookshotSoundType.Stun => "SariaMod/Sounds/HookshotStun",
                _ => "SariaMod/Sounds/HookshotStart"
            };

            float volume = soundType switch
            {
                HookshotSoundType.Loop => 0.5f,
                HookshotSoundType.Clang => 0.6f,
                HookshotSoundType.Stun => 0.8f,
                _ => 0.7f
            };
            SoundEngine.PlaySound(new SoundStyle(soundPath) { Volume = volume }, position);
            
            // Spawn hit sparks for Stun sound (when hook latches to enemy)
            if (soundType == HookshotSoundType.Stun && !Main.dedServ)
            {
                for (int i = 0; i < 10; i++)
                {
                    Vector2 dustVel = Main.rand.NextVector2CircularEdge(2f, 2f);
                    Dust.NewDust(position, 0, 0, DustID.Electric, dustVel.X, dustVel.Y, 100, default, 1f);
                }
            }

            // If server, relay to other clients
            if (Main.netMode == NetmodeID.Server)
            {
                ModPacket packet = SariaMod.Instance.GetPacket();
                packet.Write(PacketId);
                packet.Write((byte)HookshotPacketType.PlaySound);
                packet.Write((byte)soundType);
                packet.Write(posX);
                packet.Write(posY);
                packet.Send(-1, whoAmI);
            }
        }

        private static void HandleFreezeEnemy(BinaryReader reader, int whoAmI)
        {
            int npcIndex = reader.ReadInt32();

            if (npcIndex >= 0 && npcIndex < Main.maxNPCs)
            {
                NPC npc = Main.npc[npcIndex];
                if (npc.active)
                {
                    npc.velocity = Vector2.Zero;

                    // If server, relay to other clients
                    if (Main.netMode == NetmodeID.Server)
                    {
                        SendFreezeEnemy(npcIndex, whoAmI);
                        NetMessage.SendData(MessageID.SyncNPC, -1, whoAmI, null, npcIndex);
                    }
                }
            }
        }

        private static void HandleKnockbackEnemy(BinaryReader reader, int whoAmI)
        {
            int npcIndex = reader.ReadInt32();
            float knockbackX = reader.ReadSingle();
            float knockbackY = reader.ReadSingle();
            int damage = reader.ReadInt32();

            if (npcIndex >= 0 && npcIndex < Main.maxNPCs)
            {
                NPC npc = Main.npc[npcIndex];
                if (npc.active)
                {
                    Vector2 knockbackDir = new Vector2(knockbackX, knockbackY);
                    int hitDirection = knockbackDir.X > 0 ? 1 : -1;

                    // Apply damage and knockback
                    npc.StrikeNPC(damage, 8f, hitDirection);
                    npc.velocity = knockbackDir * 4f;

                    // Visual feedback
                    for (int i = 0; i < 8; i++)
                    {
                        Vector2 dustVel = Main.rand.NextVector2CircularEdge(2f, 2f);
                        Dust.NewDust(npc.Center, 0, 0, DustID.Electric, dustVel.X, dustVel.Y, 100, default, 1f);
                    }

                    // If server, relay to other clients
                    if (Main.netMode == NetmodeID.Server)
                    {
                        SendKnockbackEnemy(npcIndex, knockbackDir, damage, whoAmI);
                        NetMessage.SendData(MessageID.SyncNPC, -1, whoAmI, null, npcIndex);
                    }
                }
            }
        }

        private static void HandleLaunchEnemy(BinaryReader reader, int whoAmI)
        {
            int npcIndex = reader.ReadInt32();
            float velX = reader.ReadSingle();
            float velY = reader.ReadSingle();

            if (npcIndex >= 0 && npcIndex < Main.maxNPCs)
            {
                NPC npc = Main.npc[npcIndex];
                if (npc.active)
                {
                    npc.velocity = new Vector2(velX, velY);

                    // Visual feedback for launch
                    for (int i = 0; i < 10; i++){
                        Vector2 dustVel = Main.rand.NextVector2CircularEdge(2f, 2f);
                        Dust.NewDust(npc.Center, 0, 0, DustID.Electric, dustVel.X, dustVel.Y, 100, default, 1.2f);
                    }

                    // If server, relay to other clients
                    if (Main.netMode == NetmodeID.Server)
                    {
                        SendLaunchEnemy(npcIndex, new Vector2(velX, velY), whoAmI);
                        NetMessage.SendData(MessageID.SyncNPC, -1, whoAmI, null, npcIndex);
                    }
                }
            }
        }

        private static void HandleChipDamage(BinaryReader reader, int whoAmI)
        {
            int npcIndex = reader.ReadInt32();
            int damage = reader.ReadInt32();

            if (npcIndex >= 0 && npcIndex < Main.maxNPCs)
            {
                NPC npc = Main.npc[npcIndex];
                if (npc.active)
                {
                    npc.StrikeNPC(damage, 0f, 0);

                    // Visual feedback
                    for (int i = 0; i < 3; i++)
                    {
                        Vector2 dustVel = Main.rand.NextVector2CircularEdge(1f, 1f);
                        Dust.NewDust(npc.Center, 0, 0, DustID.Electric, dustVel.X, dustVel.Y, 100, default, 0.5f);
                    }

                    // If server, relay to other clients
                    if (Main.netMode == NetmodeID.Server)
                    {
                        SendChipDamage(npcIndex, damage, whoAmI);
                        NetMessage.SendData(MessageID.SyncNPC, -1, whoAmI, null, npcIndex);
                    }
                }
            }
        }

        private static void HandleTileImpact(BinaryReader reader, int whoAmI)
        {
            float hookPosX = reader.ReadSingle();
            float hookPosY = reader.ReadSingle();
            float dustPosX = reader.ReadSingle();
            float dustPosY = reader.ReadSingle();
            Vector2 hookPosition = new Vector2(hookPosX, hookPosY);
            Vector2 dustPosition = new Vector2(dustPosX, dustPosY);

            // Visual feedback for tile impact
            Dust.NewDust(hookPosition, 0, 0, DustID.Electric, 0f, 0f, 100, default, 1.5f);
            for (int i = 0; i < 10; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2CircularEdge(2f, 2f);
                Dust.NewDust(dustPosition, 0, 0, DustID.Electric, dustVel.X, dustVel.Y, 100, default, 1f);
            }

            // If server, relay to other clients
            if (Main.netMode == NetmodeID.Server)
            {
                SendTileImpact(hookPosition, dustPosition, whoAmI);
            }
        }

        private static void HandlePlayerPullPosition(BinaryReader reader, int whoAmI)
        {
            float pullPosX = reader.ReadSingle();
            float pullPosY = reader.ReadSingle();
            int playerId = reader.ReadByte();
            Vector2 pullPosition = new Vector2(pullPosX, pullPosY);

            // Don't manipulate player position - Terraria's built-in player sync handles this
            // This packet was causing phantom pulls by trying to move players on other clients
            // The owner controls their own position, and Terraria syncs it naturally

            // If server, relay to other clients
            if (Main.netMode == NetmodeID.Server)
            {
                SendPlayerPullPosition(pullPosition, playerId, whoAmI);
            }
        }

        private static void HandleHookAttach(BinaryReader reader, int whoAmI)
        {
            float hookPosX = reader.ReadSingle();
            float hookPosY = reader.ReadSingle();
            float playerPosX = reader.ReadSingle();
            float playerPosY = reader.ReadSingle();
            int playerId = reader.ReadByte();
            Vector2 hookPosition = new Vector2(hookPosX, hookPosY);
            Vector2 playerPosition = new Vector2(playerPosX, playerPosY);

            // Don't manipulate player position - just store the hook position for visual sync
            // The player's actual position is handled by Terraria's built-in player sync
            // This packet is purely informational for other clients to know where the hook attached

            // If server, relay to other clients
            if (Main.netMode == NetmodeID.Server)
            {
                SendHookAttach(hookPosition, playerPosition, playerId, whoAmI);
            }
        }

        private static void HandleHookRetract(BinaryReader reader, int whoAmI)
        {
            float hookPosX = reader.ReadSingle();
            float hookPosY = reader.ReadSingle();
            int ownerId = reader.ReadByte();
            Vector2 hookPosition = new Vector2(hookPosX, hookPosY);

            // Find the hookshot projectile owned by this player and force it to retract position
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];
                if (proj.active && proj.owner == ownerId && 
                    (proj.type == ModContent.ProjectileType<HookshotProjectile>() ||
                     proj.type == ModContent.ProjectileType<LongshotProjectile>()))
                {
                    // Snap projectile to the correct position and stop it
                    proj.Center = hookPosition;
                    proj.velocity = Vector2.Zero;
                    break;
                }
            }

            // If server, relay to other clients
            if (Main.netMode == NetmodeID.Server)
            {
                SendHookRetract(hookPosition, ownerId, whoAmI);
            }
        }

        private static void HandlePlayerPullComplete(BinaryReader reader, int whoAmI)
        {
            float posX = reader.ReadSingle();
            float posY = reader.ReadSingle();
            float velX = reader.ReadSingle();
            float velY = reader.ReadSingle();
            int playerId = reader.ReadByte();
            Vector2 position = new Vector2(posX, posY);
            Vector2 velocity = new Vector2(velX, velY);

            // Find the player and set their position and velocity
            if (playerId >= 0 && playerId < Main.maxPlayers)
            {
                Player player = Main.player[playerId];
                if (player.active)
                {
                    player.position = position;
                    player.velocity = velocity;
                }
            }

            // If server, relay to other clients
            if (Main.netMode == NetmodeID.Server)
            {
                SendPlayerPullComplete(position, velocity, playerId, whoAmI);
            }
        }

        private static void HandleLoopSoundSync(BinaryReader reader, int whoAmI)
        {
            int playerId = reader.ReadByte();
            bool isPlaying = reader.ReadBoolean();
            float posX = reader.ReadSingle();
            float posY = reader.ReadSingle();
            Vector2 position = new Vector2(posX, posY);

            // Only play/stop sound for other players (not the local player who originated it)
            if (playerId != Main.myPlayer && isPlaying)
            {
                SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/HookshotLoop") { Volume = 0.5f }, position);
            }

            // If server, relay to other clients
            if (Main.netMode == NetmodeID.Server)
            {
                SendLoopSoundSync(playerId, isPlaying, position, whoAmI);
            }
        }

        private static void HandleFreezeStart(BinaryReader reader, int whoAmI)
        {
            int playerIndex = reader.ReadByte();
            int npcIndex = reader.ReadInt32();
            float playerPosX = reader.ReadSingle();
            float playerPosY = reader.ReadSingle();
            float npcPosX = reader.ReadSingle();
            float npcPosY = reader.ReadSingle();
            Vector2 playerPos = new Vector2(playerPosX, playerPosY);
            Vector2 npcPos = new Vector2(npcPosX, npcPosY);

            // Apply freeze to the NPC using GlobalNPC
            if (npcIndex >= 0 && npcIndex < Main.maxNPCs)
            {
                NPC npc = Main.npc[npcIndex];
                if (npc.active)
                {
                    HookshotFreezeGlobalNPC freezeNPC = npc.GetGlobalNPC<HookshotFreezeGlobalNPC>();
                    freezeNPC.StartFreeze(npc, playerIndex);
                    freezeNPC.frozenPosition = npcPos; // Use synced position
                }
            }

            // Apply freeze to the player
            if (playerIndex >= 0 && playerIndex < Main.maxPlayers)
            {
                Player player = Main.player[playerIndex];
                if (player.active)
                {
                    HookshotPlayer modPlayer = player.GetModPlayer<HookshotPlayer>();
                    modPlayer.isFrozenByOwnHookshot = true;
                    modPlayer.frozenPosition = playerPos;
                    modPlayer.playerFreezeTimer = 0;
                    modPlayer.hookedNPCIndex = npcIndex;
                }
            }

            // If server, relay to other clients
            if (Main.netMode == NetmodeID.Server)
            {
                SendFreezeStart(playerIndex, npcIndex, playerPos, npcPos, whoAmI);
            }
        }

        private static void HandleFreezeEnd(BinaryReader reader, int whoAmI)
        {
            int playerIndex = reader.ReadByte();
            int npcIndex = reader.ReadInt32();

            // Clear freeze on NPC
            if (npcIndex >= 0 && npcIndex < Main.maxNPCs)
            {
                NPC npc = Main.npc[npcIndex];
                if (npc.active)
                {
                    HookshotFreezeGlobalNPC freezeNPC = npc.GetGlobalNPC<HookshotFreezeGlobalNPC>();
                    freezeNPC.ClearFreeze(npc);
                }
            }

            // Clear freeze on player
            if (playerIndex >= 0 && playerIndex < Main.maxPlayers)
            {
                Player player = Main.player[playerIndex];
                if (player.active)
                {
                    HookshotPlayer modPlayer = player.GetModPlayer<HookshotPlayer>();
                    modPlayer.ClearPlayerFreeze();
                }
            }

            // If server, relay to other clients
            if (Main.netMode == NetmodeID.Server)
            {
                SendFreezeEnd(playerIndex, npcIndex, whoAmI);
            }
        }

        private static void HandleDealDamageToNPC(BinaryReader reader, int whoAmI)
        {
            int npcIndex = reader.ReadInt32();
            int damage = reader.ReadInt32();
            float knockback = reader.ReadSingle();
            int hitDirection = reader.ReadSByte();
            int fromPlayer = reader.ReadByte();

            if (npcIndex >= 0 && npcIndex < Main.maxNPCs)
            {
                NPC npc = Main.npc[npcIndex];
                if (npc.active)
                {
                    // SERVER: Apply damage authoritatively
                    if (Main.netMode == NetmodeID.Server)
                    {
                        // Apply damage
                        npc.life -= damage;
                        
                        // Show damage number
                        npc.HitEffect(hitDirection, damage);
                        
                        // Check if NPC should die
                        if (npc.life <= 0)
                        {
                            npc.life = 0;
                            
                            // Clear freeze state
                            HookshotFreezeGlobalNPC freezeNPC = npc.GetGlobalNPC<HookshotFreezeGlobalNPC>();
                            if (freezeNPC.isFrozenByHookshot)
                            {
                                freezeNPC.ClearFreeze(npc);
                            }
                            
                            // This triggers death animation, loot drops, etc.
                            npc.checkDead();
                            
                            // Notify all clients
                            SendNPCKilled(npcIndex, fromPlayer, -1);
                        }
                        
                        // Sync NPC to all clients
                        NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, npcIndex);
                    }
                    // CLIENT: Visual feedback only
                    else
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            Vector2 dustVel = Main.rand.NextVector2CircularEdge(2f, 2f);
                            Dust.NewDust(npc.Center, 0, 0, DustID.Electric, dustVel.X, dustVel.Y, 100, default, 0.8f);
                        }
                    }
                }
            }
        }

        private static void HandleNPCKilled(BinaryReader reader, int whoAmI)
        {
            int npcIndex = reader.ReadInt32();
            int killerPlayerIndex = reader.ReadByte();

            if (npcIndex >= 0 && npcIndex < Main.maxNPCs)
            {
                NPC npc = Main.npc[npcIndex];
                
                HookshotFreezeGlobalNPC freezeNPC = npc.GetGlobalNPC<HookshotFreezeGlobalNPC>();
                if (freezeNPC.isFrozenByHookshot)
                {
                    freezeNPC.ClearFreeze(npc);
                }
                
                if (killerPlayerIndex >= 0 && killerPlayerIndex < Main.maxPlayers)
                {
                    Player killer = Main.player[killerPlayerIndex];
                    if (killer.active)
                    {
                        HookshotPlayer modPlayer = killer.GetModPlayer<HookshotPlayer>();
                        if (modPlayer.hookedNPCIndex == npcIndex)
                        {
                            modPlayer.ClearPlayerFreeze();
                            modPlayer.hookedNPCIndex = -1;
                        }
                    }
                }
                
                if (Main.netMode == NetmodeID.Server)
                {
                    SendNPCKilled(npcIndex, killerPlayerIndex, whoAmI);
                }
            }
        }

        #endregion
    }
}
