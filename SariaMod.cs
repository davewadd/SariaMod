using System;
using System.IO;
using System.Reflection;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;
using Terraria.ID;
using SariaMod;
using SariaMod.Buffs;
using Terraria.ModLoader.IO;
using SariaMod.Gores;
using SariaMod.Items.Strange;
using SariaMod.Netcode.SariaSoundSync;
using SariaMod.TileGlow;
using SariaMod.Netcode.HookshotNetworking;
using SariaMod.Netcode.FireSoundSync;
using SariaMod.Diagnostics;
using MonoMod.RuntimeDetour.HookGen;

namespace SariaMod
{
    public class SariaMod : Mod
    {
        public static SariaMod Instance { get; private set; }
        // Server-side: tracks the last player who sent SyncSariaLevel so we know who to route SyncSariaLevelTo back to.
        private static int _lastSariaLevelJoiner = -1;
        // Number of header bytes GetPacket() writes before mod code writes anything.
        // Probed lazily on first outgoing packet so it runs when netID is valid.
        internal static int PacketHeaderSize = -1; // -1 = not yet probed

        /// <summary>How many times the sandstorm should auto-restart after ending. Server-authoritative; synced to all clients.</summary>
        public static int SandstormRepeatCount = 0;
        public override void Load()
        {
            Instance = this;
            Diagnostics.SariaDebug.Initialize();

            var sendMethod = typeof(ModPacket).GetMethod(
                "Send",
                BindingFlags.Public | BindingFlags.Instance,
                null,
                new[] { typeof(int), typeof(int) },
                null);
            if (sendMethod != null)
                HookEndpointManager.Add(sendMethod, (Action<SendDelegate, ModPacket, int, int>)OnModPacketSend);
        }
        public override void Unload()
        {
            // Set the static instance back to null when the mod is unloaded.
            // This is good practice to prevent memory leaks and issues on reload.
            Instance = null;
        }

        private delegate void SendDelegate(ModPacket self, int toClient, int ignoreClient);
        private static void OnModPacketSend(SendDelegate orig, ModPacket self, int toClient, int ignoreClient)
        {
            // Read the packet bytes before Send() seals the stream.
            // Layout: [ushort len][byte msgID=250 (mod packet)][short netID][... payload ...]
            // The payload starts with the first byte the mod code wrote (our packet ID).
            try
            {
                var stream = (System.IO.MemoryStream)self.BaseStream;
                long pos = stream.Position;
                stream.Seek(0, System.IO.SeekOrigin.Begin);
                byte[] raw = stream.ToArray();
                stream.Seek(pos, System.IO.SeekOrigin.Begin);

                // Lazy-probe the header size using a fresh throwaway packet.
                // This runs the first time any mod packet is sent, when netID is live.
                if (PacketHeaderSize < 0)
                {
                    try { PacketHeaderSize = (int)Instance.GetPacket(0).BaseStream.Position; }
                    catch { PacketHeaderSize = 4; } // fallback: ushort(0)+byte(250)+byte(netID)
                }

                // raw[0..PacketHeaderSize-1] = tModLoader header (ushort len + byte 250 + byte/short netID)
                // raw[PacketHeaderSize]       = first mod-written byte = our packet type ID
                // raw[PacketHeaderSize+1..N]  = rest of payload fields
                if (raw.Length > PacketHeaderSize)
                {
                    byte firstByte = raw[PacketHeaderSize];
                    string name = GetPacketName(firstByte);

                    LastSentPacketRecord.Begin(name);
                    LastSentPacketRecord.AddField("packetType", $"{firstByte} ({name})", 1);
                    int payloadStart = PacketHeaderSize + 1;
                    using var br = new System.IO.BinaryReader(
                        new System.IO.MemoryStream(raw, payloadStart, raw.Length - payloadStart, writable: false),
                        System.Text.Encoding.UTF8, leaveOpen: false);
                    Diagnostics.PacketSchema.Replay(firstByte, br);
                }
            }
            catch { /* never break Send() */ }

            orig(self, toClient, ignoreClient);
        }
        // ============================================================
        // PACKET INDEX REFERENCE — keep this up to date!
        //
        // RULE: Whenever you add a new packet (enum entry OR fixed PacketId),
        //       add it here with its index so nobody accidentally reuses a value.
        //
        // --- SoundMessageType enum (auto-increments from 0 unless overridden) ---
        //   0  PlaySound
        //   1  RemoveBuff
        //   2  PlayFrozenHitEffect
        //   3  SyncRainSoundState
        //   4  StartRain
        //   5  SyncBuff
        //   6  SyncProjectileState
        //   7  SyncSariaLevel
        //   8  SyncSariaLevelTo
        //   9  RainOcarinaEffect
        //  10  SetTime
        //  11  SyncTimeTransition
        //  12  StartSandstorm
        //  13  SandstormOcarinaEffect
        //  14  SyncSandstormRepeat
        //  15  SyncLinkCable
        //  16  SyncSpawnDebug
        //  ...
        // 199  <-- last free enum slot before the explicit block
        // 200  SyncFogBreath  (explicit value)
        // 201–246  free
        //
        // --- Fixed PacketId constants (hardcoded byte values) ---
        // 247  TileGlowNetworking            (SariaMod\TileGlow\TileGlowNetworking.cs)
        // 248  PlayerDebuffSyncNetworking     (Netcode\PlayerDebuffSyncNetworking.cs)
        // 249  FireSoundSyncMessage           (Netcode\FireSoundSync\FireSoundSyncMessage.cs)
        // 250  SariaSoundSyncMessage          (Netcode\SariaSoundSync\SariaSoundSyncMessage.cs)
        // 251  IceDomeNetworking              (SariaMod\Netcode\IceDomeNetworking.cs)
        // 252  FrozenNPCNetworking /
        //      FrozenGoreMarkingNetworking    (SariaMod\Netcode\FrozenGoreMarkingNetworking.cs)
        // 253  PsychicFieldNetworking      (SariaMod\Netcode\PsychicFieldNetworking.cs)
        // 254  HookshotSyncMessage            (Netcode\HookshotNetworking\HookshotSyncMessage.cs)
        // 255  TileHeatNetworking             (SariaMod\TileGlow\TileHeatNetworking.cs)
        // ============================================================
        public enum SoundMessageType : byte
        {
            PlaySound,             // 0
            RemoveBuff,            // 1
            PlayFrozenHitEffect,   // 2
            SyncRainSoundState,    // 3
            StartRain,             // 4
            SyncBuff,              // 5
            SyncProjectileState,   // 6
            SyncSariaLevel,        // 7
            SyncSariaLevelTo,      // 8  — targeted: existing player sends their level to a specific new joiner
            RainOcarinaEffect,     // 9  — sync Rain Ocarina visual effect to all players
            SetTime,               // 10 — Ocarina of Time: set world time
            SyncTimeTransition,    // 11 — sync time transition effect to all players
            StartSandstorm,        // 12 — toggle sandstorm (request/response)
            SandstormOcarinaEffect, // 13 — sync Oasis Ocarina visual effect to all players
            SyncSandstormRepeat,    // 14 — sync sandstorm repeat counter to all clients
            SyncLinkCable,          // 15 — sync a player's LinkCable state (server needs it for split spawning)
            SyncSpawnDebug,         // 16 — server → owner client: live split-spawn accounting for the debug panel
            SyncFogBreath = 200,   // 200 — sync fog breath visibility per player
        }
        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            // --- Network profiler: measure bytes actually consumed by this packet
            //     using a position delta rather than BaseStream.Length, which may
            //     reflect the full receive buffer and inflate readings. ---
            long _profilerStart = reader.BaseStream.Position;
            byte firstByte = reader.ReadByte();
            try
            {
            DispatchPacket(reader, firstByte, whoAmI);
            }
            finally
            {
                int consumed = (int)(reader.BaseStream.Position - _profilerStart);
                NetworkProfiler.RecordReceive(firstByte, consumed);
            }
        }

        private void DispatchPacket(BinaryReader rawReader, byte firstByte, int whoAmI)
        {
            // Wrap the raw stream in a RecordingBinaryReader so every Read* call
            // automatically populates LastPacketRecord for the inspector UI.
            RecordingBinaryReader reader = new RecordingBinaryReader(rawReader.BaseStream, GetPacketName(firstByte));
            if (firstByte == SariaSoundSyncMessage.PacketId)
            {
                // If the server receives a sound event from a client, rebroadcast to all OTHER clients (not the sender).
                if (Main.netMode == NetmodeID.Server)
                {
                    byte owner = reader.ReadByte();
                    short identity = reader.ReadInt16();
                    byte soundId = reader.ReadByte();

                    ModPacket packet = GetPacket();
                    packet.Write(SariaSoundSyncMessage.PacketId);
                    packet.Write(owner);
                    packet.Write(identity);
                    packet.Write(soundId);
                    NetworkProfiler.RecordSend(SariaSoundSyncMessage.PacketId, packet);
                    packet.Send(-1, whoAmI); // exclude the sender — they already played the sound locally

                    // On host-and-play the server IS a client too, but only play if the server is not the owner.
                    // The owner already played the sound on their own machine before sending.
                    if (!Main.dedServ && owner != Main.myPlayer)
                    {
                        SariaSoundSyncMessage.PlaySound(owner, identity, (SariaSoundId)soundId);
                    }
                    return;
                }

                SariaSoundSyncMessage.Receive(reader);
                return;
            }

            if (firstByte == Netcode.PlayerDebuffSyncNetworking.PacketId)
            {
                Netcode.PlayerDebuffSyncNetworking.HandlePacket(reader, whoAmI);
                return;
            }

            if (firstByte == Netcode.IceDomeNetworking.PacketId)
            {
                Netcode.IceDomeNetworking.HandlePacket(reader);
                return;
            }

            if (firstByte == Netcode.FrozenNPCNetworking.PacketId)
            {
                Netcode.FrozenNPCNetworking.HandlePacket(reader, whoAmI);
                return;
            }

            if (firstByte == TileGlowNetworking.PacketId)
            {
                TileGlowNetworking.HandlePacket(reader, whoAmI);
                return;
            }

            if (firstByte == TileHeatNetworking.PacketId)
            {
                TileHeatNetworking.HandlePacket(reader, whoAmI);
                return;
            }

            if (firstByte == HookshotSyncMessage.PacketId)
            {
                HookshotSyncMessage.HandlePacket(reader, whoAmI);
                return;
            }

            if (firstByte == Netcode.PsychicFieldNetworking.PacketId)
            {
                Netcode.PsychicFieldNetworking.HandlePacket(reader, whoAmI);
                return;
            }

            if (firstByte == FireSoundSyncMessage.PacketId)
            {
                if (Main.netMode == NetmodeID.Server)
                {
                    byte owner = reader.ReadByte();
                    short identity = reader.ReadInt16();
                    byte soundId = reader.ReadByte();

                    ModPacket packet = GetPacket();
                    packet.Write(FireSoundSyncMessage.PacketId);
                    packet.Write(owner);
                    packet.Write(identity);
                    packet.Write(soundId);
                    NetworkProfiler.RecordSend(FireSoundSyncMessage.PacketId, packet);
                    packet.Send(-1, whoAmI); // exclude the sender — they already played the sound locally

                    if (!Main.dedServ && owner != Main.myPlayer)
                    {
                        FireSoundSyncMessage.PlaySound(owner, identity, (FireSoundId)soundId);
                    }
                    return;
                }

                FireSoundSyncMessage.Receive(reader);
                return;
            }

            SoundMessageType type = (SoundMessageType)firstByte;
            
            if (type == SoundMessageType.PlaySound)
            {
                int npcWhoAmI = reader.ReadInt32();
                int soundIndex = reader.ReadInt32();
                NPC npc = Main.npc[npcWhoAmI];
                PlaySound(npc.Center, soundIndex);
            }
            else if (type == SoundMessageType.SyncTimeTransition)
            {
                // Handle Ocarina of Time visual effect sync
                float startHour = reader.ReadSingle();
                float targetHour = reader.ReadSingle();
                byte sourcePlayer = reader.ReadByte(); // Who started the time transition
                
                if (Main.netMode == NetmodeID.Server)
                {
                    // Server received request - broadcast to all clients
                    ModPacket packet = Instance.GetPacket();
                    packet.Write((byte)SoundMessageType.SyncTimeTransition);
                    packet.Write(startHour);
                    packet.Write(targetHour);
                    packet.Write(sourcePlayer);
                    NetworkProfiler.RecordSend((byte)SoundMessageType.SyncTimeTransition, packet);
                    packet.Send(-1, whoAmI); // Send to all except the sender (they already spawned it)
                }
                else if (Main.netMode == NetmodeID.MultiplayerClient)
                {
                    // Client received broadcast - spawn the effect projectile on the local player
                    Player localPlayer = Main.player[Main.myPlayer];
                    if (localPlayer.active && !localPlayer.dead)
                    {
                        // Check if we don't already have the effect active
                        bool hasEffect = false;
                        for (int i = 0; i < Main.maxProjectiles; i++)
                        {
                            if (Main.projectile[i].active && 
                                Main.projectile[i].type == ModContent.ProjectileType<Items.zPearls.OcarinaOfTimeNote>() &&
                                Main.projectile[i].owner == Main.myPlayer)
                            {
                                hasEffect = true;
                                break;
                            }
                        }
                        
                        if (!hasEffect)
                        {
                            // Spawn the effect projectile with time data in ai[]
                            int projIndex = Projectile.NewProjectile(
                                localPlayer.GetSource_FromThis(),
                                localPlayer.Center,
                                Vector2.Zero,
                                ModContent.ProjectileType<Items.zPearls.OcarinaOfTimeNote>(),
                                0,
                                0f,
                                Main.myPlayer,
                                startHour,  // ai[0] = startHour
                                targetHour  // ai[1] = targetHour
                            );
                            
                            // Store source player ID in localAI[1] so we can set their animation
                            if (projIndex >= 0 && projIndex < Main.maxProjectiles)
                            {
                                Main.projectile[projIndex].localAI[0] = 1f; // 1 = received from network
                                Main.projectile[projIndex].localAI[1] = sourcePlayer; // Source player who used ocarina
                            }
                        }
                    }
                }
            }
            else if (type == SoundMessageType.RemoveBuff) // Handle the new buff removal message
            {
                int npcWhoAmI = reader.ReadInt32();
                if (Main.netMode == NetmodeID.Server)
                {
                    // On the server, apply the removal.
                    NPC npc = Main.npc[npcWhoAmI];
                    int buffIndex = npc.FindBuffIndex(ModContent.BuffType<EnemyFrozen>());
                    if (buffIndex != -1)
                    {
                        npc.DelBuff(buffIndex);
                        npc.netUpdate = true; // Sync the buff removal to all clients.
                    }
                }
            }
            else if (type == SoundMessageType.PlayFrozenHitEffect)
            {
                int npcWhoAmI = reader.ReadInt32();
                NPC npc = Main.npc[npcWhoAmI];
                if (npc.active)
                {
                    // Play sound and create gore on the client
                    int backGoreType = ModContent.GoreType<IceGore2>();
                    for (int G = 0; G < 3; G++)
                    {
                        Gore B = Gore.NewGorePerfect(npc.GetSource_FromThis(), npc.position, new Vector2(Main.rand.Next(-6, 7), Main.rand.Next(-6, 7)), backGoreType, 2f);
                        B.light = .5f;
                        SoundEngine.PlaySound(SoundID.Item27, npc.Center);
                    }
                }
            }
            else if (type == SoundMessageType.SyncRainSoundState)
            {
                bool newRainState = reader.ReadBoolean();
                // This ensures the packet is handled on the correct side.
                // In this case, the packet is sent from the server/host to clients.
                if (Main.netMode == NetmodeID.MultiplayerClient)
                {
                    // Find the local player's ModPlayer instance.
                    FairyPlayerMiscEffects modPlayer = Main.player[Main.myPlayer].GetModPlayer<FairyPlayerMiscEffects>();
                    // Call the method in your ModPlayer to update the rain state.
                    modPlayer.ReceiveRainSoundState(newRainState);
                }
                if (Main.netMode != NetmodeID.MultiplayerClient && Main.netMode == NetmodeID.SinglePlayer)
                    {
                        // Find the local player's ModPlayer instance.
                        FairyPlayerMiscEffects modPlayer = Main.player[Main.myPlayer].GetModPlayer<FairyPlayerMiscEffects>();
                        // Call the method in your ModPlayer to update the rain state.
                        modPlayer.ReceiveRainSoundState(newRainState);
                    }
            }
            else if (type == SoundMessageType.StartRain)
            {
                // First byte after message type indicates if this is a request (0) or response (1)
                bool isResponse = reader.ReadBoolean();
                
                if (!isResponse && Main.netMode == NetmodeID.Server)
                {
                    // Server received a REQUEST from a client to toggle rain
                    bool wasRaining = Main.raining;
                    if (Main.raining)
                    {
                        Main.StopRain();
                    }
                    else
                    {
                        Main.StartRain();
                    }
                    
                    // Sync the world data (rain state) to all clients using Terraria's built-in system
                    NetMessage.SendData(MessageID.WorldData);

                    // Broadcast the RESPONSE to all clients for the text message
                    ModPacket responsePacket = Instance.GetPacket();
                    responsePacket.Write((byte)SoundMessageType.StartRain);
                    responsePacket.Write(true); // This is a response
                    responsePacket.Write(wasRaining); // Send what the state WAS (so client knows what changed)
                    NetworkProfiler.RecordSend((byte)SoundMessageType.StartRain, responsePacket);
                    responsePacket.Send(-1, -1);
                }
                else if (isResponse && Main.netMode == NetmodeID.MultiplayerClient)
                {
                    // Client received a RESPONSE - display the appropriate message
                    bool wasRaining = reader.ReadBoolean();
                    if (wasRaining)
                    {
                        Main.NewText("The storm passes for now.", 50, 100, 150);
                    }
                    else
                    {
                        Main.NewText("Another Storm! You played the Ocarina again didn't you?", 50, 100, 150);
                    }
                }
            }
            else if (type == SoundMessageType.RainOcarinaEffect)
            {
                // Handle Rain Ocarina visual effect sync
                if (Main.netMode == NetmodeID.Server)
                {
                    // Server received request - broadcast to all clients
                    ModPacket packet = Instance.GetPacket();
                    packet.Write((byte)SoundMessageType.RainOcarinaEffect);
                    NetworkProfiler.RecordSend((byte)SoundMessageType.RainOcarinaEffect, packet);
                    packet.Send(-1, whoAmI); // Send to all except the sender (they already spawned it)
                }
                else if (Main.netMode == NetmodeID.MultiplayerClient)
                {
                    // Client received broadcast - spawn the effect on the local player
                    Player localPlayer = Main.player[Main.myPlayer];
                    if (localPlayer.active && !localPlayer.dead)
                    {
                        // Check if we don't already have the effect active
                        bool hasEffect = false;
                        for (int i = 0; i < Main.maxProjectiles; i++)
                        {
                            if (Main.projectile[i].active && 
                                Main.projectile[i].type == ModContent.ProjectileType<Items.zPearls.RainOcarinaNote>() &&
                                Main.projectile[i].owner == Main.myPlayer)
                            {
                                hasEffect = true;
                                break;
                            }
                        }
                        
                        if (!hasEffect)
                        {
                            Projectile.NewProjectile(
                                localPlayer.GetSource_FromThis(),
                                localPlayer.Center,
                                Vector2.Zero,
                                ModContent.ProjectileType<Items.zPearls.RainOcarinaNote>(),
                                0,
                                0f,
                                Main.myPlayer
                            );
                        }
                    }
                }
            }
            else if (type == SoundMessageType.StartSandstorm)
            {
                // First byte after message type indicates if this is a request (0) or response (1)
                bool isResponse = reader.ReadBoolean();

                if (!isResponse && Main.netMode == NetmodeID.Server)
                {
                    // Server received a REQUEST from a client to toggle sandstorm
                    bool wasSandstorming = Terraria.GameContent.Events.Sandstorm.Happening;
                    if (wasSandstorming)
                    {
                        Terraria.GameContent.Events.Sandstorm.StopSandstorm();
                    }
                    else
                    {
                        Terraria.GameContent.Events.Sandstorm.StartSandstorm();
                    }

                    // Sync the world data (sandstorm state) to all clients using Terraria's built-in system
                    NetMessage.SendData(MessageID.WorldData);

                    // Broadcast the RESPONSE to all clients for the text message
                    ModPacket responsePacket = Instance.GetPacket();
                    responsePacket.Write((byte)SoundMessageType.StartSandstorm);
                    responsePacket.Write(true); // This is a response
                    responsePacket.Write(wasSandstorming); // Send what the state WAS (so client knows what changed)
                    NetworkProfiler.RecordSend((byte)SoundMessageType.StartSandstorm, responsePacket);
                    responsePacket.Send(-1, -1);
                }
                else if (isResponse && Main.netMode == NetmodeID.MultiplayerClient)
                {
                    // Client received a RESPONSE - display the appropriate message
                    bool wasSandstorming = reader.ReadBoolean();
                    if (wasSandstorming)
                    {
                        Main.NewText("The sands settle...", 200, 170, 100);
                    }
                    else
                    {
                        Main.NewText("The deserts air begins to stir...", 200, 170, 100);
                    }
                }
            }
            else if (type == SoundMessageType.SandstormOcarinaEffect)
            {
                // Handle Oasis Ocarina visual effect sync
                if (Main.netMode == NetmodeID.Server)
                {
                    // Server received request - broadcast to all clients
                    ModPacket packet = Instance.GetPacket();
                    packet.Write((byte)SoundMessageType.SandstormOcarinaEffect);
                    NetworkProfiler.RecordSend((byte)SoundMessageType.SandstormOcarinaEffect, packet);
                    packet.Send(-1, whoAmI); // Send to all except the sender (they already spawned it)
                }
                else if (Main.netMode == NetmodeID.MultiplayerClient)
                {
                    // Client received broadcast - spawn the effect on the local player
                    Player localPlayer = Main.player[Main.myPlayer];
                    if (localPlayer.active && !localPlayer.dead)
                    {
                        // Check if we don't already have the effect active
                        bool hasEffect = false;
                        for (int i = 0; i < Main.maxProjectiles; i++)
                        {
                            if (Main.projectile[i].active &&
                                Main.projectile[i].type == ModContent.ProjectileType<Items.zPearls.SandstormOcarinaNote>() &&
                                Main.projectile[i].owner == Main.myPlayer)
                            {
                                hasEffect = true;
                                break;
                            }
                        }

                        if (!hasEffect)
                        {
                            Projectile.NewProjectile(
                                localPlayer.GetSource_FromThis(),
                                localPlayer.Center,
                                Vector2.Zero,
                                ModContent.ProjectileType<Items.zPearls.SandstormOcarinaNote>(),
                                0,
                                0f,
                                Main.myPlayer
                            );
                        }
                    }
                }
            }
            else if (type == SoundMessageType.SyncSandstormRepeat)
            {
                int newCount = reader.ReadInt32();
                SandstormRepeatCount = newCount;

                if (Main.netMode == NetmodeID.Server)
                {
                    // Broadcast the updated count to all clients
                    ModPacket broadcast = Instance.GetPacket();
                    broadcast.Write((byte)SoundMessageType.SyncSandstormRepeat);
                    broadcast.Write(newCount);
                    broadcast.Send(-1, whoAmI);
                }
            }
            else if (type == SoundMessageType.SyncLinkCable)
            {
                // LinkCable state sync. The SERVER is the machine that runs NPC.SpawnNPC in
                // multiplayer, so the split spawn system (SariaSpawnSystem/ApplyRegionGate)
                // reads FairyPlayer.LinkCable on the server — without this packet the flag
                // only ever flipped on the toggling client and split spawning silently never
                // engaged on dedicated servers. Other clients receive it too so any
                // LinkCable-conditional presentation stays consistent everywhere.
                int playerIndex = reader.ReadInt32("playerIndex");
                bool linkCable = reader.ReadBoolean("linkCable");

                if (playerIndex >= 0 && playerIndex < Main.player.Length)
                {
                    FairyPlayer fp = Main.player[playerIndex].GetModPlayer<FairyPlayer>();
                    fp.LinkCable = linkCable;
                    if (!linkCable)
                        fp.LinkCableTarget = Vector2.Zero;

                    Diagnostics.SariaDebug.LogSilent("SpawnGate",
                        $"SyncLinkCable received: player[{playerIndex}] LinkCable={linkCable} netMode={Main.netMode}");

                    if (Main.netMode == NetmodeID.Server)
                    {
                        // Relay to all other clients (sender already applied it locally).
                        ModPacket relay = Instance.GetPacket();
                        relay.Write((byte)SoundMessageType.SyncLinkCable);
                        relay.Write(playerIndex);
                        relay.Write(linkCable);
                        NetworkProfiler.RecordSend((byte)SoundMessageType.SyncLinkCable, relay);
                        relay.Send(-1, whoAmI);
                    }
                }
            }
            else if (type == SoundMessageType.SyncSpawnDebug)
            {
                // Server → client: the split-spawn accounting lives where NPC.SpawnNPC
                // actually runs (the SERVER in multiplayer), so every cap/fed/slot static
                // the debug panel reads was stale or zero on clients — the panel showed
                // locally-recomputed numbers that never matched what the server's gate
                // was really doing (the "5 where it should have been 6" report). The
                // server pushes its authoritative values to the owner's client on the
                // same ~1s cadence as the split log line; the panel prefers these while
                // they are fresh (see SariaSpawnSystem.ServerDebugFresh).
                SariaSpawnSystem.LastPlayerCap            = reader.ReadInt32("playerCap");
                SariaSpawnSystem.LastSariaCap             = reader.ReadInt32("sariaCap");
                SariaSpawnSystem.LastPlayerFedMaxSpawns   = reader.ReadInt32("playerFed");
                SariaSpawnSystem.LastSariaFedMaxSpawns    = reader.ReadInt32("sariaFed");
                SariaSpawnSystem.LastPlayerRegionSlots    = reader.ReadSingle("playerSlots");
                SariaSpawnSystem.LastSariaRegionSlots     = reader.ReadSingle("sariaSlots");
                SariaSpawnSystem.LastGlobalSlotCount      = reader.ReadSingle("globalSlots");
                SariaSpawnSystem.LastRegionsMerged        = reader.ReadBoolean("merged");
                SariaSpawnSystem.LastServerDebugSyncTime  = Main.GameUpdateCount;
            }
            else if (type == SoundMessageType.SyncBuff)
            {
                int playerIndex = reader.ReadInt32();
                int buffType = reader.ReadInt32();
                int buffTime = reader.ReadInt32();
                // Ensure the buff is only added on the client that received the packet
                if (Main.netMode == NetmodeID.MultiplayerClient)
                {
                    // Apply the buff to the correct player
                    if (Main.player[playerIndex].active)
                    {
                        // The 'quiet' flag prevents the buff from trying to resend a network packet
                        // which would cause an infinite loop.
                        Main.player[playerIndex].AddBuff(buffType, buffTime, quiet: true);
                    }
                }
            }
            else if (type == SariaMod.SoundMessageType.SyncProjectileState)
            {
                int projWhoAmI = reader.ReadInt32();
                int frame = reader.ReadInt32();
                int direction = reader.ReadInt32();
                int frameCounter = reader.ReadInt32(); // ADD THIS LINE
                if (Main.netMode == NetmodeID.MultiplayerClient && projWhoAmI >= 0 && projWhoAmI < Main.maxProjectiles)
                {
                    Projectile projectile = Main.projectile[projWhoAmI];
                    if (projectile.active && projectile.type == ModContent.ProjectileType<Saria>())
                    {
                        projectile.frame = frame;
                        projectile.spriteDirection = direction;
                        projectile.frameCounter = frameCounter; // ADD THIS LINE
                    }
                }
            }
            else if (type == SoundMessageType.SyncSariaLevel)
            {
                int playerIndex = reader.ReadInt32("playerIndex");
                int sariaLevel  = reader.ReadInt32("sariaLevel");
                int sariaXp     = reader.ReadInt32("sariaXp");

                if (playerIndex >= 0 && playerIndex < Main.player.Length)
                {
                    // Always store the received values locally.
                    FairyPlayer modPlayer = Main.player[playerIndex].GetModPlayer<FairyPlayer>();
                    if (modPlayer != null)
                    {
                        modPlayer.Sarialevel = sariaLevel;
                        modPlayer.SariaXp = sariaXp;
                    }

                    if (Main.netMode == NetmodeID.Server)
                    {
                        // Remember who just joined so we can route SyncSariaLevelTo replies back to them.
                        _lastSariaLevelJoiner = whoAmI;
                        // Server relays to all other clients so they learn about the sender.
                        ModPacket relay = Instance.GetPacket();
                        relay.Write((byte)SoundMessageType.SyncSariaLevel);
                        relay.Write(playerIndex);
                        relay.Write(sariaLevel);
                        relay.Write(sariaXp);
                        NetworkProfiler.RecordSend((byte)SoundMessageType.SyncSariaLevel, relay);
                        relay.Send(-1, whoAmI);
                    }
                    else if (Main.netMode == NetmodeID.MultiplayerClient)
                    {
                        // A new player just announced themselves — send our own level back to them only.
                        FairyPlayer self = Main.LocalPlayer.GetModPlayer<FairyPlayer>();
                        ModPacket reply = Instance.GetPacket();
                        reply.Write((byte)SoundMessageType.SyncSariaLevelTo);
                        reply.Write(Main.myPlayer);
                        reply.Write(self.Sarialevel);
                        reply.Write(self.SariaXp);
                        // no targetIndex written: server already knows the joiner from _lastSariaLevelJoiner
                        reply.Send();
                    }
                }
            }
            else if (type == SoundMessageType.SyncSariaLevelTo)
            {
                // An existing player is sending their level to a specific new joiner.
                // Packet contains only 3 fields in both directions: no targetIndex ever sent.
                // Server uses _lastSariaLevelJoiner (set when SyncSariaLevel arrived) to route the forward.
                int senderIndex  = reader.ReadInt32("senderIndex");
                int sariaLevel2  = reader.ReadInt32("sariaLevel");
                int sariaXp2     = reader.ReadInt32("sariaXp");

                if (Main.netMode == NetmodeID.Server)
                {
                    // Forward to the joiner we recorded when their SyncSariaLevel arrived.
                    ModPacket forward = Instance.GetPacket();
                    forward.Write((byte)SoundMessageType.SyncSariaLevelTo);
                    forward.Write(senderIndex);
                    forward.Write(sariaLevel2);
                    forward.Write(sariaXp2);
                    NetworkProfiler.RecordSend((byte)SoundMessageType.SyncSariaLevelTo, forward);
                    forward.Send(_lastSariaLevelJoiner);
                }
                else
                {
                    // We are the target — store the sender's level.
                    if (senderIndex >= 0 && senderIndex < Main.player.Length)
                    {
                        FairyPlayer fp = Main.player[senderIndex].GetModPlayer<FairyPlayer>();
                        if (fp != null)
                        {
                            fp.Sarialevel = sariaLevel2;
                            fp.SariaXp    = sariaXp2;
                        }
                    }
                }
            }
            else if (type == SoundMessageType.SetTime)
            {
                // Ocarina of Time - Set world time
                float targetHour = reader.ReadSingle();

                if (Main.netMode == NetmodeID.Server)
                {
                    // Server: Set the time and sync to all clients
                    SetGameTimeFromHour(targetHour);

                    // Sync world data to all clients
                    NetMessage.SendData(MessageID.WorldData);
                }
            }
            else if (type == SoundMessageType.SyncFogBreath)
            {
                int playerIndex = reader.ReadByte();
                bool showFog = reader.ReadBoolean();

                if (playerIndex >= 0 && playerIndex < Main.maxPlayers && Main.player[playerIndex].active)
                {
                    Main.player[playerIndex].GetModPlayer<FairyPlayer>().ShowFogBreath = showFog;
                }

                // Server forwards to all other clients
                if (Main.netMode == NetmodeID.Server)
                {
                    ModPacket packet = GetPacket();
                    packet.Write((byte)SoundMessageType.SyncFogBreath);
                    packet.Write((byte)playerIndex);
                    packet.Write(showFog);
                    NetworkProfiler.RecordSend((byte)SoundMessageType.SyncFogBreath, packet);
                    packet.Send(-1, whoAmI);
                }
            }
            // Note: SyncTimeTransition is now handled by PacketId 247 above
        }
        public static SariaMod GetInstance()
        {
            return ModContent.GetInstance<SariaMod>();
        }
        // Generic helper method to play any sound based on its index.
        public static void PlaySound(Vector2 position, int soundIndex)
        {
            string soundPath;
            if (soundIndex == 6)
            {
                soundPath = "SariaMod/Sounds/Death6";
            }
            else if (soundIndex == 7)
            {
                soundPath = "SariaMod/Sounds/BeatMe";
            }
            else 
            {
                soundPath = "SariaMod/Sounds/Die" + soundIndex;
            }
            SoundEngine.PlaySound(new SoundStyle(soundPath), position);
        }
        public static void PlayFrozenHitEffect(int npcWhoAmI)
        {
            if (Main.netMode == NetmodeID.Server)
            {
                ModPacket packet = Instance.GetPacket();
                packet.Write((byte)SoundMessageType.PlayFrozenHitEffect);
                packet.Write(npcWhoAmI);
                NetworkProfiler.RecordSend((byte)SoundMessageType.PlayFrozenHitEffect, packet);
                packet.Send(-1, -1); // Send to all clients
            }
        }
        
        /// <summary>
        /// Helper method to convert hour (0-24) to Terraria game time format.
        /// Used by Ocarina of Time for time manipulation.
        /// </summary>
        public static void SetGameTimeFromHour(float hour)
        {
            // Clamp hour to 0-24
            hour = Math.Clamp(hour, 0f, 24f);
            if (hour >= 24f) hour = 0f;
            
            // Convert hour to Terraria time
            // Terraria: dayTime=true from 4:30 AM (0 ticks) to 7:30 PM (54000 ticks)
            // Terraria: dayTime=false from 7:30 PM (0 ticks) to 4:30 AM (32400 ticks)
            if (hour >= 4.5f && hour < 19.5f)
            {
                // Daytime (4:30 AM to 7:30 PM)
                Main.dayTime = true;
                Main.time = (hour - 4.5f) / 15.0 * 54000.0;
            }
            else
            {
                // Nighttime (7:30 PM to 4:30 AM)
                Main.dayTime = false;
                float nightHour = hour >= 19.5f ? hour - 19.5f : hour + 4.5f;
                Main.time = nightHour / 9.0 * 32400.0;
            }
        }
                    /// <summary>
                    /// Returns a human-readable name for a packet ID byte.
                    /// </summary>
                    private static string GetPacketName(byte id)
                    {
                        if (id == SariaSoundSyncMessage.PacketId)               return "SariaSoundSync";
                        if (id == Netcode.PlayerDebuffSyncNetworking.PacketId)  return "PlayerDebuffSync";
                        if (id == Netcode.IceDomeNetworking.PacketId)           return "IceDome";
                        if (id == Netcode.FrozenNPCNetworking.PacketId)         return "FrozenNPC";
                        if (id == TileGlowNetworking.PacketId)                  return "TileGlow";
                        if (id == TileHeatNetworking.PacketId)                  return "TileHeat";
                        if (id == HookshotSyncMessage.PacketId)                 return "HookshotSync";
                        if (id == Netcode.PsychicFieldNetworking.PacketId)      return "PsychicField";
                        if (id == FireSoundSyncMessage.PacketId)                return "FireSoundSync";

                        if (Enum.IsDefined(typeof(SoundMessageType), id))
                            return ((SoundMessageType)id).ToString();

                        return $"Unknown(0x{id:X2})";
                    }

                    /// <summary>
                    /// Logs a received packet header to chat/console.
                    /// Note on byte counts: RecordSend subtracts the 2-byte tModLoader framing header
                    /// so TX and RX measurements both reflect the same payload-only byte count.
                    /// </summary>
                    private static void DebugLogPacketReceived(byte id, int fromWho)
                    {
                        string name = GetPacketName(id);
                        string side = Main.netMode == NetmodeID.Server ? "[Server]" : "[Client]";
                        string msg  = $"{side} RX packet: {name} (id={id}) from whoAmI={fromWho}  [TX byte count is +2 due to mod framing header]";

                        if (Main.netMode == NetmodeID.Server)
                            Console.WriteLine("[SariaMod] " + msg);
                        else
                            Main.NewText(msg, 100, 200, 255);
                    }

                    /// <summary>
                    /// Logs specific payload values read from inside a packet handler.
                    /// Call after reading all values to show exactly what was received.
                    /// </summary>
                    public static void DebugLogPacketDetails(string details)
                    {
                        string side = Main.netMode == NetmodeID.Server ? "[Server]" : "[Client]";
                        string msg  = $"{side}   -> {details}";

                        if (Main.netMode == NetmodeID.Server)
                            Console.WriteLine("[SariaMod] " + msg);
                        else
                            Main.NewText(msg, 150, 230, 180);
                    }
                }
            }
