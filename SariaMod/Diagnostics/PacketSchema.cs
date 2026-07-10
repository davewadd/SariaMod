using System;
using System.Collections.Generic;
using System.IO;
using Terraria.ModLoader;
using SariaMod.Netcode;
using SariaMod.Netcode.FireSoundSync;
using SariaMod.Netcode.SariaSoundSync;
using SariaMod.Netcode.HookshotNetworking;
using SariaMod.TileGlow;

namespace SariaMod.Diagnostics
{
    /// <summary>
    /// Describes how to read the fields of every outgoing mod packet so the
    /// sent-packet inspector can show per-field byte breakdowns automatically.
    ///
    /// This is the ONE place that knows each packet's field layout.
    /// When you add a new packet, add a schema entry here — nowhere else.
    ///
    /// "Replay" simply reads the already-written bytes back in the same order
    /// they were written, recording each field into LastSentPacketRecord.
    /// </summary>
    public static class PacketSchema
    {
        // A field descriptor: a name and a reader delegate.
        private delegate object FieldReader(BinaryReader r);
        private readonly struct FieldDef
        {
            public readonly string  Name;
            public readonly int     Bytes;
            public readonly FieldReader Read;
            public FieldDef(string name, int bytes, FieldReader read)
            { Name = name; Bytes = bytes; Read = read; }
        }

        // A schema is an ordered list of fields that mirrors the Write() calls.
        private static readonly Dictionary<byte, Func<BinaryReader, bool>> _schemas = new();

        static PacketSchema()
        {
            // --- helpers ---
            static byte   RB(BinaryReader r) => r.ReadByte();
            static bool   RBool(BinaryReader r) => r.ReadBoolean();
            static short  RS(BinaryReader r) => r.ReadInt16();
            static int    RI(BinaryReader r) => r.ReadInt32();
            static float  RF(BinaryReader r) => r.ReadSingle();

            // Register a flat (non-dispatched) packet by listing field names and sizes.
            void Flat(byte id, params (string name, int bytes, FieldReader read)[] fields)
            {
                _schemas[id] = (r) =>
                {
                    foreach (var (name, bytes, read) in fields)
                    {
                        try
                        {
                            object val = read(r);
                            LastSentPacketRecord.AddField(name, val?.ToString() ?? "null", bytes);
                        }
                        catch { return false; }
                    }
                    return true;
                };
            }

            // ----------------------------------------------------------------
            // 253 = PsychicField: sariaIdentity(4), chargeIdentity(4), positionX(4), positionY(4)
            // ----------------------------------------------------------------
            Flat(PsychicFieldNetworking.PacketId,
                 ("sariaIdentity",  4, r => RI(r)),
                 ("chargeIdentity", 4, r => RI(r)),
                 ("positionX",      4, r => RF(r)),
                 ("positionY",      4, r => RF(r)));

            // ----------------------------------------------------------------
            // SoundMessageType packets  (byte ID 0-11, 200)
            // ----------------------------------------------------------------
            // 0 = PlaySound: npcWhoAmI(4), soundIndex(4)
            Flat(0,  ("npcWhoAmI",  4, r => RI(r)),
                     ("soundIndex", 4, r => RI(r)));

            // 1 = RemoveBuff: npcWhoAmI(4)
            Flat(1,  ("npcWhoAmI", 4, r => RI(r)));

            // 2 = PlayFrozenHitEffect: npcWhoAmI(4)
            Flat(2,  ("npcWhoAmI", 4, r => RI(r)));

            // 3 = SyncRainSoundState: rainState(1)
            Flat(3,  ("rainState", 1, r => RBool(r)));

            // 4 = StartRain: isResponse(1) [+ wasRaining(1) if response — handled via sub-dispatch below]
            _schemas[4] = (r) =>
            {
                try
                {
                    bool isResponse = r.ReadBoolean();
                    LastSentPacketRecord.AddField("isResponse", isResponse.ToString(), 1);
                    if (isResponse)
                    {
                        bool wasRaining = r.ReadBoolean();
                        LastSentPacketRecord.AddField("wasRaining", wasRaining.ToString(), 1);
                    }
                    return true;
                }
                catch { return false; }
            };

            // 5 = SyncBuff: playerIndex(4), buffType(4), buffTime(4)
            Flat(5,  ("playerIndex", 4, r => RI(r)),
                     ("buffType",    4, r => RI(r)),
                     ("buffTime",    4, r => RI(r)));

            // 6 = SyncProjectileState: projWhoAmI(4), frame(4), spriteDir(4), frameCounter(4)
            Flat(6,  ("projWhoAmI",   4, r => RI(r)),
                     ("frame",        4, r => RI(r)),
                     ("spriteDir",    4, r => RI(r)),
                     ("frameCounter", 4, r => RI(r)));

            // 7 = SyncSariaLevel: playerIndex(4), sariaLevel(4), sariaXp(4)
            Flat(7,  ("playerIndex", 4, r => RI(r)),
                     ("sariaLevel",  4, r => RI(r)),
                     ("sariaXp",     4, r => RI(r)));

            // 8 = SyncSariaLevelTo: senderIndex(4), sariaLevel(4), sariaXp(4)
            Flat(8,  ("senderIndex", 4, r => RI(r)),
                     ("sariaLevel",  4, r => RI(r)),
                     ("sariaXp",     4, r => RI(r)));

            // 9 = RainOcarinaEffect: no payload
            _schemas[9] = (_) => true;

            // 10 = SetTime: targetHour(4)
            Flat(10, ("targetHour", 4, r => RF(r)));

            // 11 = SyncTimeTransition: startHour(4), targetHour(4), sourcePlayer(1)
            Flat(11, ("startHour",   4, r => RF(r)),
                     ("targetHour",  4, r => RF(r)),
                     ("sourcePlayer",1, r => RB(r)));

            // 12 = StartSandstorm: isResponse(1) [+ wasSandstorming(1) if response]
            _schemas[12] = (r) =>
            {
                try
                {
                    bool isResponse = r.ReadBoolean();
                    LastSentPacketRecord.AddField("isResponse", isResponse.ToString(), 1);
                    if (isResponse)
                    {
                        bool wasSandstorming = r.ReadBoolean();
                        LastSentPacketRecord.AddField("wasSandstorming", wasSandstorming.ToString(), 1);
                    }
                    return true;
                }
                catch { return false; }
            };

            // 13 = SandstormOcarinaEffect: no payload
            _schemas[13] = (_) => true;

            // 200 = SyncFogBreath: playerIndex(1), showFog(1)
            Flat(200, ("playerIndex", 1, r => RB(r)),
                      ("showFog",     1, r => RBool(r)));

            // ----------------------------------------------------------------
            // 248 = PlayerDebuffSync — subtype dispatch
            // ----------------------------------------------------------------
            _schemas[PlayerDebuffSyncNetworking.PacketId] = (r) =>
            {
                try
                {
                    byte sub = r.ReadByte();
                    LastSentPacketRecord.AddField("subType", sub.ToString(), 1);
                    // Both subtypes: playerIndex(1), isFrozen(1)
                    byte playerIndex = r.ReadByte();
                    bool isFrozen    = r.ReadBoolean();
                    LastSentPacketRecord.AddField("playerIndex", playerIndex.ToString(), 1);
                    LastSentPacketRecord.AddField("isFrozen",    isFrozen.ToString(),    1);
                    return true;
                }
                catch { return false; }
            };

            // ----------------------------------------------------------------
            // 249 = FireSoundSync: owner(1), identity(2), soundId(1)
            // ----------------------------------------------------------------
            Flat(FireSoundSyncMessage.PacketId,
                 ("owner",    1, r => RB(r)),
                 ("identity", 2, r => RS(r)),
                 ("soundId",  1, r => RB(r)));

            // ----------------------------------------------------------------
            // 250 = SariaSoundSync: owner(1), identity(2), soundId(1)
            // ----------------------------------------------------------------
            Flat(SariaSoundSyncMessage.PacketId,
                 ("owner",    1, r => RB(r)),
                 ("identity", 2, r => RS(r)),
                 ("soundId",  1, r => RB(r)));

            // ----------------------------------------------------------------
            // 251 = IceDome: npcWhoAmI(4), randomSize(4), excludePlayer(1)
            // ----------------------------------------------------------------
            Flat(IceDomeNetworking.PacketId,
                 ("npcWhoAmI",     4, r => RI(r)),
                 ("randomSize",    4, r => RI(r)),
                 ("excludePlayer", 1, r => RB(r)));

            // ----------------------------------------------------------------
            // 252 = FrozenNPC — subtype dispatch
            // ----------------------------------------------------------------
            _schemas[FrozenNPCNetworking.PacketId] = (r) =>
            {
                try
                {
                    byte sub = r.ReadByte();
                    LastSentPacketRecord.AddField("subType", sub.ToString(), 1);
                    if (sub == 0) // FreezeNPC
                    {
                        int npcWhoAmI     = r.ReadInt32();
                        byte ownerPlayer  = r.ReadByte();
                        LastSentPacketRecord.AddField("npcWhoAmI",   npcWhoAmI.ToString(),    4);
                        LastSentPacketRecord.AddField("ownerPlayer", ownerPlayer.ToString(),  1);
                    }
                    else if (sub == 1) // SyncTimer
                    {
                        int npcWhoAmI  = r.ReadInt32();
                        int timerValue = r.ReadInt32();
                        LastSentPacketRecord.AddField("npcWhoAmI",  npcWhoAmI.ToString(),  4);
                        LastSentPacketRecord.AddField("timerValue", timerValue.ToString(), 4);
                    }
                    return true;
                }
                catch { return false; }
            };

            // ----------------------------------------------------------------
            // 254 = HookshotSync — subtype dispatch
            // ----------------------------------------------------------------
            _schemas[HookshotSyncMessage.PacketId] = (r) =>
            {
                try
                {
                    byte sub = r.ReadByte();
                    LastSentPacketRecord.AddField("subType", ((HookshotPacketType)sub).ToString(), 1);
                    switch ((HookshotPacketType)sub)
                    {
                        case HookshotPacketType.ArmSync:
                            // playerId(1), rotation(4), hasHook(1), isHolding(1), direction(1)
                            RecordFields(r,("playerId",  1, r => RB(r)),
                                           ("rotation",  4, r => RF(r)),
                                           ("hasHook",   1, r => RBool(r)),
                                           ("isHolding", 1, r => RBool(r)),
                                           ("direction", 1, r => RB(r)));
                            break;
                        case HookshotPacketType.PlaySound:
                            // soundType(1), posX(4), posY(4)
                            RecordFields(r,("soundType", 1, r => RB(r)),
                                           ("posX",      4, r => RF(r)),
                                           ("posY",      4, r => RF(r)));
                            break;
                        case HookshotPacketType.FreezeEnemy:
                        case HookshotPacketType.FreezeEnd:
                            // npcIndex(4)
                            RecordFields(r,("npcIndex", 4, r => RI(r)));
                            break;
                        case HookshotPacketType.KnockbackEnemy:
                            // npcIndex(4), knockX(4), knockY(4), damage(4), direction(4)
                            RecordFields(r,("npcIndex",  4, r => RI(r)),
                                           ("knockX",    4, r => RF(r)),
                                           ("knockY",    4, r => RF(r)),
                                           ("damage",    4, r => RI(r)),
                                           ("direction", 4, r => RI(r)));
                            break;
                        case HookshotPacketType.LaunchEnemy:
                            // npcIndex(4), velX(4), velY(4)
                            RecordFields(r,("npcIndex", 4, r => RI(r)),
                                           ("velX",     4, r => RF(r)),
                                           ("velY",     4, r => RF(r)));
                            break;
                        case HookshotPacketType.ChipDamage:
                            // npcIndex(4), damage(4)
                            RecordFields(r,("npcIndex", 4, r => RI(r)),
                                           ("damage",   4, r => RI(r)));
                            break;
                        case HookshotPacketType.TileImpact:
                            // hookX(4), hookY(4), dustX(4), dustY(4)
                            RecordFields(r,("hookX", 4, r => RF(r)),
                                           ("hookY", 4, r => RF(r)),
                                           ("dustX", 4, r => RF(r)),
                                           ("dustY", 4, r => RF(r)));
                            break;
                        case HookshotPacketType.PlayerPullPosition:
                            // pullX(4), pullY(4), playerId(1)
                            RecordFields(r,("pullX",    4, r => RF(r)),
                                           ("pullY",    4, r => RF(r)),
                                           ("playerId", 1, r => RB(r)));
                            break;
                        case HookshotPacketType.HookAttach:
                            // hookX(4), hookY(4), playerX(4), playerY(4), playerId(1)
                            RecordFields(r,("hookX",    4, r => RF(r)),
                                           ("hookY",    4, r => RF(r)),
                                           ("playerX",  4, r => RF(r)),
                                           ("playerY",  4, r => RF(r)),
                                           ("playerId", 1, r => RB(r)));
                            break;
                        case HookshotPacketType.HookRetract:
                            // hookX(4), hookY(4), ownerId(1)
                            RecordFields(r,("hookX",   4, r => RF(r)),
                                           ("hookY",   4, r => RF(r)),
                                           ("ownerId", 1, r => RB(r)));
                            break;
                        case HookshotPacketType.PlayerPullComplete:
                            // posX(4), posY(4), velX(4), velY(4), playerId(1)
                            RecordFields(r,("posX",     4, r => RF(r)),
                                           ("posY",     4, r => RF(r)),
                                           ("velX",     4, r => RF(r)),
                                           ("velY",     4, r => RF(r)),
                                           ("playerId", 1, r => RB(r)));
                            break;
                        case HookshotPacketType.LoopSoundSync:
                            // playerId(1), isPlaying(1), posX(4), posY(4)
                            RecordFields(r,("playerId",  1, r => RB(r)),
                                           ("isPlaying", 1, r => RBool(r)),
                                           ("posX",      4, r => RF(r)),
                                           ("posY",      4, r => RF(r)));
                            break;
                        case HookshotPacketType.FreezeStart:
                            // playerIndex(4), npcIndex(4), playerX(4), playerY(4), npcX(4), npcY(4)
                            RecordFields(r,("playerIndex", 4, r => RI(r)),
                                           ("npcIndex",   4, r => RI(r)),
                                           ("playerX",    4, r => RF(r)),
                                           ("playerY",    4, r => RF(r)),
                                           ("npcX",       4, r => RF(r)),
                                           ("npcY",       4, r => RF(r)));
                            break;
                        case HookshotPacketType.DealDamageToNPC:
                            // npcIndex(4), damage(4), knockback(4), hitDir(4)
                            RecordFields(r,("npcIndex",  4, r => RI(r)),
                                           ("damage",    4, r => RI(r)),
                                           ("knockback", 4, r => RF(r)),
                                           ("hitDir",    4, r => RI(r)));
                            break;
                        case HookshotPacketType.NPCKilled:
                            // npcIndex(4), killerIndex(4)
                            RecordFields(r,("npcIndex",   4, r => RI(r)),
                                           ("killerIndex",4, r => RI(r)));
                            break;
                    }
                    return true;
                }
                catch { return false; }
            };

            // ----------------------------------------------------------------
            // TileGlow (same PacketId as FrozenNPC = 252) — handled above via FrozenNPC id
            // If TileGlow has its own ID, add it here.
            // ----------------------------------------------------------------
            _schemas[TileHeatNetworking.PacketId] = (r) =>
            {
                try
                {
                    byte sub = r.ReadByte();
                    LastSentPacketRecord.AddField("subType", sub.ToString(), 1);

                    if (sub == 0)
                    {
                        RecordFields(r,
                            ("centerX", 4, r => RF(r)),
                            ("centerY", 4, r => RF(r)),
                            ("radius", 4, r => RF(r)),
                            ("duration", 4, r => RI(r)));
                    }
                    else if (sub == 1)
                    {
                        RecordFields(r,
                            ("x", 4, r => RI(r)),
                            ("y", 4, r => RI(r)),
                            ("distFromCenter", 4, r => RF(r)),
                            ("maxRadius", 4, r => RF(r)),
                            ("duration", 4, r => RI(r)));
                    }

                    return true;
                }
                catch { return false; }
            };
        }

        // Helper used inside the hookshot schema lambda
        private static void RecordFields(BinaryReader r,
            params (string name, int bytes, FieldReader read)[] fields)
        {
            foreach (var (name, bytes, read) in fields)
            {
                object val = read(r);
                LastSentPacketRecord.AddField(name, val?.ToString() ?? "null", bytes);
            }
        }

        /// <summary>
        /// Called from the <c>ModPacket.Send</c> hook.
        /// Reads the raw payload bytes back and records each field into
        /// <see cref="LastSentPacketRecord"/>. The record was already <c>Begin</c>-ed
        /// with the packet name before this is called.
        /// </summary>
        public static void Replay(byte firstByte, BinaryReader reader)
        {
            if (_schemas.TryGetValue(firstByte, out var schema))
                schema(reader);
            // If no schema is registered, the record still shows the packet name
            // and total bytes — just no per-field breakdown.
        }
    }
}
