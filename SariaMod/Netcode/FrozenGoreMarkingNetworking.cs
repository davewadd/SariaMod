using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using SariaMod.Gores;

namespace SariaMod.Netcode
{
    /// <summary>
    /// Unified packet for all frozen-NPC visual state.
    /// Replaces the old FrozenGoreMarkingNetworking (252) + FrozenNPCTimerNetworking (253).
    /// Sub-type FreezeNPC: first-time freeze — marks NPC blue and starts timer.
    /// Sub-type SyncTimer: re-freeze timer reset only (no redundant mark).
    /// IceDomeNetworking (251 / RandomSize) stays separate.
    /// </summary>
    public static class FrozenNPCNetworking
    {
        public const byte PacketId = 252;

        private enum SubType : byte
        {
            FreezeNPC  = 0,   // mark + start timer
            SyncTimer  = 1,   // timer reset only (re-freeze)
        }

        // ── Send ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Send a first-time freeze packet: marks the NPC blue and starts its timer on all clients.
        /// Pass the projectile owner's player index so the server does not echo it back to them.
        /// </summary>
        public static void SendFreezeNPC(int npcWhoAmI, int ownerPlayerIndex)
        {
            if (Main.netMode == NetmodeID.SinglePlayer) return;

            ModPacket packet = SariaMod.Instance.GetPacket();
            packet.Write(PacketId);
            packet.Write((byte)SubType.FreezeNPC);
            packet.Write(npcWhoAmI);
            packet.Write((byte)ownerPlayerIndex);

            if (Main.netMode == NetmodeID.MultiplayerClient)
                packet.Send();                  // client → server, server will relay
            else
                packet.Send(-1, ownerPlayerIndex); // server → all except owner
        }

        /// <summary>
        /// Send a timer-reset packet for an NPC that is already marked frozen.
        /// Call this instead of the old SendSyncFrozenTimer.
        /// </summary>
        public static void SendSyncTimer(int npcWhoAmI, int timerValue)
        {
            if (Main.netMode == NetmodeID.SinglePlayer) return;
            if (npcWhoAmI < 0 || npcWhoAmI >= Main.npc.Length) return;

            ModPacket packet = SariaMod.Instance.GetPacket();
            packet.Write(PacketId);
            packet.Write((byte)SubType.SyncTimer);
            packet.Write(npcWhoAmI);
            packet.Write(timerValue);

            if (Main.netMode == NetmodeID.MultiplayerClient)
                packet.Send();
            else
                packet.Send(-1, -1);
        }

        // ── Handle ───────────────────────────────────────────────────────────

        public static void HandlePacket(BinaryReader reader, int whoAmI)
        {
            SubType sub = (SubType)reader.ReadByte();

            switch (sub)
            {
                case SubType.FreezeNPC:  HandleFreezeNPC(reader, whoAmI);  break;
                case SubType.SyncTimer:  HandleSyncTimer(reader, whoAmI);  break;
            }
        }

        private static void HandleFreezeNPC(BinaryReader reader, int whoAmI)
        {
            int npcWhoAmI   = reader.ReadInt32();
            int ownerPlayer = reader.ReadByte();

            if (Main.netMode == NetmodeID.Server)
            {
                if (npcWhoAmI < 0 || npcWhoAmI >= Main.maxNPCs) return;
                NPC npc = Main.npc[npcWhoAmI];
                if (!npc.active) return;
                if (!npc.TryGetGlobalNPC(out FairyGlobalNPC fairyNPC)) return;

                // Deduplicate: if freezeInitiatorPlayer is already set, a FreezeNPC for this
                // NPC was already processed this freeze event. Drop the duplicate.
                // freezeInitiatorPlayer resets to -1 when the NPC is no longer frozen.
                if (fairyNPC.freezeInitiatorPlayer >= 0) return;

                // Record who caused the freeze so IceDomeNetworking can exclude them correctly.
                fairyNPC.freezeInitiatorPlayer = ownerPlayer;

                // Relay to all clients except the projectile owner (they already applied it locally)
                ModPacket packet = SariaMod.Instance.GetPacket();
                packet.Write(PacketId);
                packet.Write((byte)SubType.FreezeNPC);
                packet.Write(npcWhoAmI);
                packet.Write((byte)ownerPlayer);
                packet.Send(-1, ownerPlayer);
            }
            else
            {
                // Client: apply mark locally (no network send — we ARE the receiver)
                FrozenNPCVisualManager.MarkNPCAsFrozenLocal(npcWhoAmI);
            }
        }

        private static void HandleSyncTimer(BinaryReader reader, int whoAmI)
        {
            int npcWhoAmI  = reader.ReadInt32();
            int timerValue = reader.ReadInt32();

            if (npcWhoAmI < 0 || npcWhoAmI >= Main.npc.Length) return;

            if (Main.netMode == NetmodeID.Server)
            {
                ModPacket packet = SariaMod.Instance.GetPacket();
                packet.Write(PacketId);
                packet.Write((byte)SubType.SyncTimer);
                packet.Write(npcWhoAmI);
                packet.Write(timerValue);
                packet.Send(-1, whoAmI);
            }
            else
            {
                FrozenNPCVisualManager.SyncFrozenTimer(npcWhoAmI, timerValue);
            }
        }
    }

    // Keep old name as a thin alias so existing call sites outside this PR still compile.
    // Can be removed once all callers are updated.
    [System.Obsolete("Use FrozenNPCNetworking instead.")]
    public static class FrozenGoreMarkingNetworking
    {
        public const byte PacketId = FrozenNPCNetworking.PacketId;
        public static void SendMarkNPCFrozen(int npcWhoAmI) => FrozenNPCNetworking.SendFreezeNPC(npcWhoAmI, Main.myPlayer);
        public static void HandlePacket(BinaryReader reader, int whoAmI) => FrozenNPCNetworking.HandlePacket(reader, whoAmI);
    }
}

