using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using SariaMod.Gores;
using SariaMod.Buffs;

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
        /// Send a first-time hard-freeze request. The server applies the authoritative
        /// EnemyFrozen buff while the owner keeps a quiet local prediction.
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
        /// Refresh the short Sapphire chilled visual on the other clients.
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
                if (whoAmI < 0 || whoAmI >= Main.maxPlayers || !Main.player[whoAmI].active) return;
                NPC npc = Main.npc[npcWhoAmI];
                if (!npc.active || npc.boss) return;
                if (!npc.TryGetGlobalNPC(out FairyGlobalNPC fairyNPC)) return;

                // Keep the existing payload for compatibility, but derive authority
                // from the sender instead of trusting the claimed owner byte.
                ownerPlayer = whoAmI;
                int frozenBuffType = ModContent.BuffType<EnemyFrozen>();
                bool alreadyFrozen = npc.HasBuff(frozenBuffType);

                if (!alreadyFrozen)
                {
                    npc.buffImmune[frozenBuffType] = false;
                    npc.AddBuff(frozenBuffType, EnemyFrozen.MaximumBuffTime, true);
                }

                if (!alreadyFrozen || fairyNPC.freezeInitiatorPlayer < 0)
                {
                    fairyNPC.SetFreezeInitiator(ownerPlayer);
                    npc.netUpdate = true;
                }
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
                if (whoAmI < 0 || whoAmI >= Main.maxPlayers || !Main.player[whoAmI].active) return;
                if (!Main.npc[npcWhoAmI].active) return;

                ModPacket packet = SariaMod.Instance.GetPacket();
                packet.Write(PacketId);
                packet.Write((byte)SubType.SyncTimer);
                packet.Write(npcWhoAmI);
                packet.Write(0);
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

