using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace SariaMod.Diagnostics
{
    /// <summary>
    /// Lightweight per-category network traffic tracker.
    ///
    /// Tracks bytes/packets RECEIVED through <see cref="SariaMod.HandlePacket"/>,
    /// plus optional bytes/packets SENT (call <see cref="RecordSend"/> at each
    /// <c>packet.Send(...)</c> site you want to attribute).
    ///
    /// Uses a 60-slot ring buffer advanced once per UI tick (~60Hz), so the sum
    /// across the buffer == bytes/sec. Cheap (a couple of int adds per packet).
    ///
    /// Not thread-safe by design — Terraria mod packet handling and UI ticks both
    /// run on the main thread, so no locking is needed.
    /// </summary>
    public static class NetworkProfiler
    {
        public const int WindowSize = 60; // 60 slots ≈ 1 second at 60 FPS

        public class Stats
        {
            public string Name;
            public readonly int[] RxBytesRing = new int[WindowSize];
            public readonly int[] RxPacketsRing = new int[WindowSize];
            public readonly int[] TxBytesRing = new int[WindowSize];
            public readonly int[] TxPacketsRing = new int[WindowSize];

            public long TotalRxBytes;
            public long TotalRxPackets;
            public long TotalTxBytes;
            public long TotalTxPackets;

            public int RxBytesPerSec
            {
                get
                {
                    int sum = 0;
                    for (int i = 0; i < WindowSize; i++) sum += RxBytesRing[i];
                    return sum;
                }
            }

            public int RxPacketsPerSec
            {
                get
                {
                    int sum = 0;
                    for (int i = 0; i < WindowSize; i++) sum += RxPacketsRing[i];
                    return sum;
                }
            }

            public int TxBytesPerSec
            {
                get
                {
                    int sum = 0;
                    for (int i = 0; i < WindowSize; i++) sum += TxBytesRing[i];
                    return sum;
                }
            }

            public int TxPacketsPerSec
            {
                get
                {
                    int sum = 0;
                    for (int i = 0; i < WindowSize; i++) sum += TxPacketsRing[i];
                    return sum;
                }
            }
        }

        private static readonly Dictionary<byte, Stats> _byId = new();
        private static int _ringHead;

        // Friendly names for known packet IDs. Anything not in this map shows up
        // as "Unknown(<id>)" so unexpected traffic is still visible.
        private static readonly Dictionary<byte, string> _knownNames = new()
        {
            { 0,   "Sound: PlaySound" },
            { 1,   "Sound: RemoveBuff" },
            { 2,   "Sound: PlayFrozenHitEffect" },
            { 3,   "Sound: SyncRainSoundState" },
            { 4,   "Sound: StartRain" },
            { 5,   "Sound: SyncBuff" },
            { 6,   "Sound: SyncProjectileState" },
            { 7,   "Sound: SyncSariaLevel" },
            { 8,   "Sound: RainOcarinaEffect" },
            { 9,   "Sound: SetTime" },
            { 10,  "Sound: SyncTimeTransition" },
            { 248, "PlayerDebuffSync" },
            { 249, "FireSoundSync" },
            { 250, "SariaSoundSync" },
            { 251, "IceDome" },
            { 252, "FrozenGore / TileGlow" },
            { 253, "FrozenNPCTimer" },
            { 254, "HookshotSync" },
        };

        private static Stats GetOrCreate(byte packetId)
        {
            if (!_byId.TryGetValue(packetId, out Stats s))
            {
                s = new Stats
                {
                    Name = _knownNames.TryGetValue(packetId, out string n) ? n : $"Unknown({packetId})"
                };
                _byId[packetId] = s;
            }
            return s;
        }

        /// <summary>
        /// Called from <see cref="SariaMod.HandlePacket"/> for every incoming packet.
        /// </summary>
        public static void RecordReceive(byte packetId, int bytes)
        {
            if (bytes <= 0) return;
            Stats s = GetOrCreate(packetId);
            s.RxBytesRing[_ringHead] += bytes;
            s.RxPacketsRing[_ringHead]++;
            s.TotalRxBytes += bytes;
            s.TotalRxPackets++;
        }

        /// <summary>
        /// Optional: call right before/after <c>packet.Send(...)</c> to attribute
        /// outgoing bytes to a category. Pass the same <see cref="ModPacket"/> you
        /// just wrote — its underlying <see cref="MemoryStream"/> length is the
        /// payload size that will hit the wire.
        /// </summary>
        public static void RecordSend(byte packetId, ModPacket packet)
        {
            if (packet?.BaseStream is not MemoryStream ms) return;
            // Subtract the 2-byte mod-index header tModLoader prepends to every ModPacket.
            // This aligns TX measurements with RX measurements (which are taken after the
            // engine has already stripped that header), so both sides show the same count.
            int bytes = Math.Max(0, (int)ms.Length - 2);
            if (bytes <= 0) return;
            Stats s = GetOrCreate(packetId);
            s.TxBytesRing[_ringHead] += bytes;
            s.TxPacketsRing[_ringHead]++;
            s.TotalTxBytes += bytes;
            s.TotalTxPackets++;
        }

        /// <summary>
        /// Advance the ring buffer one slot. Call once per UI update tick.
        /// </summary>
        public static void Tick()
        {
            _ringHead = (_ringHead + 1) % WindowSize;
            // Clear the slot we're about to write into so old samples are dropped.
            foreach (Stats s in _byId.Values)
            {
                s.RxBytesRing[_ringHead] = 0;
                s.RxPacketsRing[_ringHead] = 0;
                s.TxBytesRing[_ringHead] = 0;
                s.TxPacketsRing[_ringHead] = 0;
            }
        }

        /// <summary>Snapshot for the UI panel, sorted by RX+TX bytes/sec descending.</summary>
        public static List<Stats> GetSnapshot()
        {
            var list = new List<Stats>(_byId.Values);
            list.Sort((a, b) => (b.RxBytesPerSec + b.TxBytesPerSec).CompareTo(a.RxBytesPerSec + a.TxBytesPerSec));
            return list;
        }

        /// <summary>Reset all counters (e.g., when player wants a fresh measurement).</summary>
        public static void ResetAll()
        {
            _byId.Clear();
            _ringHead = 0;
        }

        /// <summary>
        /// Returns the combined RX+TX bytes/sec and packet count for all packet IDs
        /// that are attributed to the Saria minion system. Useful for the per-Saria
        /// debug overlay drawn in <c>Saria.PostDraw</c>.
        /// </summary>
        public static (int BytesPerSec, int PacketsPerSec) GetSariaAggregate()
        {
            int bytes = 0, pkts = 0;
            // 250 = SariaSoundSync, 6 = SyncProjectileState, 7 = SyncSariaLevel
            foreach (byte id in new byte[] { 250, 6, 7 })
            {
                if (_byId.TryGetValue(id, out Stats s))
                {
                    bytes += s.RxBytesPerSec + s.TxBytesPerSec;
                    pkts  += s.RxPacketsPerSec + s.TxPacketsPerSec;
                }
            }
            return (bytes, pkts);
        }

        /// <summary>Human-readable byte size string (shared between profiler UI and in-world overlays).</summary>
        public static string FormatBytes(int b)
        {
            if (b < 1024) return b + " B";
            if (b < 1024 * 1024) return (b / 1024f).ToString("F1") + " KB";
            return (b / (1024f * 1024f)).ToString("F1") + " MB";
        }

        // =====================================================================
        //  SARIA INSTANCE WEIGHT ANALYSIS
        // =====================================================================

        /// <summary>
        /// Total bytes written by <c>SendExtraAI</c> per sync:
        ///   Main fields: 19 ints (76 B) + 9 bools (9 B) = 85 B
        ///   IdleAnimator: 15 ints (60 B) + 1 float (4 B) + 8 bools (8 B) = 72 B
        ///   Grand total: 157 bytes, 52 synced variables.
        /// </summary>
        public const int SariaExtraAIPayloadBytes = 157;

        /// <summary>Number of distinct variables written in <c>SendExtraAI</c>.</summary>
        public const int SariaExtraAISyncedVarCount = 52;

        /// <summary>
        /// Per-frame snapshot of one active Saria projectile instance with all cost metrics.
        /// </summary>
        public struct SariaInstanceInfo
        {
            /// <summary>
            /// Local-only slot index in <c>Main.projectile[]</c>. Differs between
            /// host and each client — not useful for cross-machine identification.
            /// </summary>
            public int WhoAmI;

            /// <summary>
            /// Cross-client stable identifier assigned at spawn. The same Saria will
            /// have the same <c>identity</c> on every machine, unlike <c>whoAmI</c>.
            /// </summary>
            public int Identity;

            public string OwnerName;
            public bool IsLocalOwner;

            /// <summary>Sum of loaded overlay texture RGBA bytes (GPU VRAM estimate).</summary>
            public int TextureVramBytes;

            /// <summary>Bytes written per <c>SendExtraAI</c> call (static constant).</summary>
            public int ExtraAIPayloadBytes;

            /// <summary>Number of fields synced in <c>SendExtraAI</c> (static constant).</summary>
            public int SyncedVarCount;

            /// <summary>This instance's allocated share of aggregate Saria net traffic.</summary>
            public int NetBytesPerSec;
            public int NetPacketsPerSec;
        }

        // All Saria overlay texture paths — used for VRAM estimation.
        private static readonly string[] _sariaOverlayPaths;

        static NetworkProfiler()
        {
            var paths = new List<string>
            {
                "SariaMod/Items/Strange/GlobalSariaAnimations/SariaFeetIdle",
                "SariaMod/Items/Strange/GlobalSariaAnimations/IdleFaces/SariaNormalFaceIdle",
                "SariaMod/Items/Strange/GlobalSariaAnimations/IdleFaces/SariaNormalFaceIdleBack",
                "SariaMod/Items/Strange/GlobalSariaAnimations/IdleFaces/6SariaNormalFaceIdle",
                "SariaMod/Items/Strange/GlobalSariaAnimations/IdleFaces/6SariaNormalFaceIdleBack",
                "SariaMod/Items/Strange/GlobalSariaAnimations/IdleFaces/6SariaIdleEyeBackground",
                "SariaMod/Items/Strange/GlobalSariaAnimations/IdleFaces/7SariaNormalFaceIdle",
                "SariaMod/Items/Strange/GlobalSariaAnimations/IdleFaces/7SariaNormalFaceIdleBack",
                "SariaMod/Items/Strange/GlobalSariaAnimations/IdleFaces/7SariaNormalFaceIdleBackground",
            };
            for (int n = 1; n <= 7; n++)
            {
                paths.Add($"SariaMod/Items/Strange/{n}SariaAnimations/{n}SariaLegs");
                paths.Add($"SariaMod/Items/Strange/{n}SariaAnimations/{n}SariaArmRight");
                paths.Add($"SariaMod/Items/Strange/{n}SariaAnimations/{n}SariaArmLeft");
            }
            _sariaOverlayPaths = paths.ToArray();
        }

        /// <summary>
        /// Estimates the total GPU VRAM consumed by loaded Saria textures (main sheet +
        /// all overlay sheets currently in memory). Returns raw bytes (RGBA).
        /// </summary>
        public static int EstimateSariaVram(int sariaType)
        {
            int total = 0;

            // Main projectile sheet
            if (sariaType >= 0 && sariaType < TextureAssets.Projectile.Length)
            {
                var asset = TextureAssets.Projectile[sariaType];
                if (asset?.IsLoaded == true && asset.Value != null)
                    total += asset.Value.Width * asset.Value.Height * 4;
            }

            // Overlay sheets — only count those already resident in VRAM
            foreach (string path in _sariaOverlayPaths)
            {
                try
                {
                    var asset = ModContent.Request<Texture2D>(path);
                    if (asset?.IsLoaded == true && asset.Value != null)
                        total += asset.Value.Width * asset.Value.Height * 4;
                }
                catch { }
            }

            return total;
        }

        /// <summary>
        /// Returns a <see cref="SariaInstanceInfo"/> for every currently active Saria
        /// projectile. Net traffic is divided evenly among all active instances.
        /// </summary>
        public static List<SariaInstanceInfo> GetSariaInstances()
        {
            var result = new List<SariaInstanceInfo>();
            if (Main.projectile == null) return result;

            int sariaType = ModContent.ProjectileType<Items.Strange.Saria>();

            // Count active instances first so we can split traffic evenly
            int count = 0;
            for (int i = 0; i < Main.maxProjectiles; i++)
                if (Main.projectile[i].active && Main.projectile[i].type == sariaType)
                    count++;

            if (count == 0) return result;

            var (aggBytes, aggPkts) = GetSariaAggregate();
            int perInstBytes = aggBytes / count;
            int perInstPkts  = aggPkts  / count;
            int vram = EstimateSariaVram(sariaType);

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (!p.active || p.type != sariaType) continue;

                int owner = p.owner;
                string ownerName = (owner >= 0 && owner < Main.player.Length && Main.player[owner].active)
                    ? Main.player[owner].name
                    : $"Player {owner}";

                result.Add(new SariaInstanceInfo
                {
                    WhoAmI              = i,
                    Identity            = p.identity,
                    OwnerName           = ownerName,
                    IsLocalOwner        = owner == Main.myPlayer,
                    TextureVramBytes    = vram,
                    ExtraAIPayloadBytes = SariaExtraAIPayloadBytes,
                    SyncedVarCount      = SariaExtraAISyncedVarCount,
                    NetBytesPerSec      = perInstBytes,
                    NetPacketsPerSec    = perInstPkts,
                });
            }

            return result;
        }
    }

    /// <summary>
    /// Stores a breakdown of the last received packet for display in the inspector panel.
    /// Each field records its name, value as a string, and how many bytes it occupies.
    /// </summary>
    /// <summary>Shared field entry used by LastPacketRecord and LastSentPacketRecord.</summary>
    public struct PacketField
    {
        public string Name;
        public string Value;
        public int Bytes;
    }

    public static class LastPacketRecord
    {
        public static string PacketName   { get; private set; } = "(none yet)";
        public static int    TotalBytes   { get; private set; } = 0;
        public static List<PacketField> Fields { get; private set; } = new List<PacketField>();

        /// <summary>Call at the start of a packet handler, before reading any fields.</summary>
        public static void Begin(string packetName)
        {
            PacketName = packetName;
            TotalBytes = 0;
            Fields     = new List<PacketField>();
        }

        /// <summary>Add one field after reading it from the BinaryReader.</summary>
        public static void AddField(string name, string value, int bytes)
        {
            Fields.Add(new PacketField { Name = name, Value = value, Bytes = bytes });
            TotalBytes += bytes;
        }
    }

    /// <summary>Mirrors LastPacketRecord but for the most recently sent packet.</summary>
    public static class LastSentPacketRecord
    {
        public static string PacketName   { get; private set; } = "(none yet)";
        public static int    TotalBytes   { get; private set; } = 0;
        public static List<PacketField> Fields { get; private set; } = new List<PacketField>();

        /// <summary>Call before writing the first field of a packet.</summary>
        public static void Begin(string packetName)
        {
            PacketName = packetName;
            TotalBytes = 0;
            Fields     = new List<PacketField>();
        }

        /// <summary>Add one field after writing it to the ModPacket.</summary>
        public static void AddField(string name, string value, int bytes)
        {
            Fields.Add(new PacketField { Name = name, Value = value, Bytes = bytes });
            TotalBytes += bytes;
        }
    }

    // =========================================================================
    //  GENERAL PACKET RECORDING HELPERS
    // =========================================================================

    /// <summary>
    /// A <see cref="BinaryReader"/> wrapper that automatically records every
    /// Read* call into <see cref="LastPacketRecord"/> so all packet handlers
    /// gain full-field diagnostics without any per-handler boilerplate.
    ///
    /// Usage in DispatchPacket — one line at the top, no other changes needed:
    ///   var reader = new RecordingBinaryReader(rawReader.BaseStream, packetName);
    /// </summary>
    public sealed class RecordingBinaryReader : BinaryReader
    {
        public RecordingBinaryReader(System.IO.Stream stream, string packetName)
            : base(stream, System.Text.Encoding.UTF8, leaveOpen: true)
        {
            LastPacketRecord.Begin(packetName);
        }

        private T Rec<T>(string typeName, T value, int bytes)
        {
            LastPacketRecord.AddField(typeName, value?.ToString() ?? "null", bytes);
            return value;
        }

        public override byte   ReadByte()    => Rec("byte",   base.ReadByte(),    1);
        public override sbyte  ReadSByte()   => Rec("sbyte",  base.ReadSByte(),   1);
        public override bool   ReadBoolean() => Rec("bool",   base.ReadBoolean(), 1);
        public override short  ReadInt16()   => Rec("short",  base.ReadInt16(),   2);
        public override ushort ReadUInt16()  => Rec("ushort", base.ReadUInt16(),  2);
        public override int    ReadInt32()   => Rec("int",    base.ReadInt32(),   4);
        public override uint   ReadUInt32()  => Rec("uint",   base.ReadUInt32(),  4);
        public override long   ReadInt64()   => Rec("long",   base.ReadInt64(),   8);
        public override float  ReadSingle()  => Rec("float",  base.ReadSingle(),  4);
        public override double ReadDouble()  => Rec("double", base.ReadDouble(),  8);
        public override string ReadString()
        {
            var v = base.ReadString();
            LastPacketRecord.AddField("string", v, System.Text.Encoding.UTF8.GetByteCount(v) + 1);
            return v;
        }

        // Named overloads — optional, but let callers label a field meaningfully
        public byte   ReadByte(string name)    { var v = base.ReadByte();    LastPacketRecord.AddField(name, v.ToString(), 1);    return v; }
        public bool   ReadBoolean(string name) { var v = base.ReadBoolean(); LastPacketRecord.AddField(name, v.ToString(), 1);    return v; }
        public short  ReadInt16(string name)   { var v = base.ReadInt16();   LastPacketRecord.AddField(name, v.ToString(), 2);    return v; }
        public ushort ReadUInt16(string name)  { var v = base.ReadUInt16();  LastPacketRecord.AddField(name, v.ToString(), 2);    return v; }
        public int    ReadInt32(string name)   { var v = base.ReadInt32();   LastPacketRecord.AddField(name, v.ToString(), 4);    return v; }
        public float  ReadSingle(string name)  { var v = base.ReadSingle();  LastPacketRecord.AddField(name, v.ToString("F3"), 4); return v; }
        public string ReadString(string name)  { var v = base.ReadString();  LastPacketRecord.AddField(name, v, System.Text.Encoding.UTF8.GetByteCount(v) + 1); return v; }
    }

    /// <summary>
    /// A <see cref="System.IO.MemoryStream"/> wrapper that intercepts every
    /// <see cref="System.IO.Stream.Write"/> call and records the byte count
    /// into <see cref="LastSentPacketRecord"/> automatically.
    ///
    /// It is injected by <see cref="RecordingPacketFactory.CreatePacket"/> which
    /// wraps <c>SariaMod.GetPacket()</c>. No send site needs to change at all.
    /// </summary>
    public sealed class RecordingBinaryWriter : System.IO.BinaryWriter
    {
        // BinaryWriter wraps a Stream. We shadow the underlying MemoryStream so
        // ModPacket can still reach it, but every primitive Write() is intercepted.

        public RecordingBinaryWriter(System.IO.Stream stream)
            : base(stream, System.Text.Encoding.UTF8, leaveOpen: true) { }

        private void Rec(string typeName, string value, int bytes)
            => LastSentPacketRecord.AddField(typeName, value, bytes);

        public override void Write(byte v)   { base.Write(v); Rec("byte",   v.ToString(), 1); }
        public override void Write(sbyte v)  { base.Write(v); Rec("sbyte",  v.ToString(), 1); }
        public override void Write(bool v)   { base.Write(v); Rec("bool",   v.ToString(), 1); }
        public override void Write(short v)  { base.Write(v); Rec("short",  v.ToString(), 2); }
        public override void Write(ushort v) { base.Write(v); Rec("ushort", v.ToString(), 2); }
        public override void Write(int v)    { base.Write(v); Rec("int",    v.ToString(), 4); }
        public override void Write(uint v)   { base.Write(v); Rec("uint",   v.ToString(), 4); }
        public override void Write(long v)   { base.Write(v); Rec("long",   v.ToString(), 8); }
        public override void Write(float v)  { base.Write(v); Rec("float",  v.ToString("F3"), 4); }
        public override void Write(double v) { base.Write(v); Rec("double", v.ToString("F3"), 8); }
        public override void Write(string v) { base.Write(v); Rec("string", v, System.Text.Encoding.UTF8.GetByteCount(v) + 1); }
    }
}
