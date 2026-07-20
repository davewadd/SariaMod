using Microsoft.Xna.Framework;
using SariaMod.Diagnostics;
using SariaMod.Items.Sapphire;
using SariaMod.Items.Strange;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Netcode
{
    /// <summary>
    /// Shares one cursor position per player between all Saria projectiles that
    /// intentionally follow or aim at that cursor. The owner and single-player
    /// paths still read Main.MouseWorld directly; only remote presentation is
    /// interpolated.
    /// </summary>
    public static class SariaCursorNetworking
    {
        public const byte PacketId = 17;

        private const int SyncIntervalTicks = 2;
        private const float WorldBoundsMargin = 2048f;
        private const float RemoteCursorLerpFactor = 0.45f;
        private const float RemoteCursorSnapDistance = 1200f;
        private const float RemoteCursorSettleDistance = 0.25f;

        private static readonly Vector2[] CachedCursor = new Vector2[Main.maxPlayers];
        private static readonly Vector2[] RemoteCursorTarget = new Vector2[Main.maxPlayers];
        private static readonly bool[] HasCachedCursor = new bool[Main.maxPlayers];
        private static readonly bool[] RemoteCursorInitialized = new bool[Main.maxPlayers];
        private static readonly bool[] HasSentOwnerCursor = new bool[Main.maxPlayers];
        private static readonly bool[] HasAcceptedServerCursor = new bool[Main.maxPlayers];
        private static readonly ulong[] LastOwnerSendTick = new ulong[Main.maxPlayers];
        private static readonly ulong[] LastServerAcceptTick = new ulong[Main.maxPlayers];
        private static readonly ulong[] LastRemoteInterpolationTick = new ulong[Main.maxPlayers];
        private static readonly bool[] WasPlayerActive = new bool[Main.maxPlayers];
        private static readonly bool[] HadAllowedFollower = new bool[Main.maxPlayers];
        private static readonly bool[] AllowedFollowerPresent = new bool[Main.maxPlayers];

        /// <summary>
        /// Called by any allowlisted local follower. Multiple followers still
        /// produce at most one cursor packet for their owner every two world ticks.
        /// </summary>
        public static void PublishLocalCursor(Projectile follower)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient
                || follower == null
                || !follower.active
                || follower.owner != Main.myPlayer
                || !IsAllowedFollower(follower))
            {
                return;
            }

            int owner = follower.owner;
            ulong currentTick = Main.GameUpdateCount;
            if (HasSentOwnerCursor[owner]
                && currentTick - LastOwnerSendTick[owner] < SyncIntervalTicks)
            {
                return;
            }

            HasSentOwnerCursor[owner] = true;
            LastOwnerSendTick[owner] = currentTick;

            Vector2 cursorPosition = Main.MouseWorld;
            CachedCursor[owner] = cursorPosition;
            HasCachedCursor[owner] = true;

            ModPacket packet = SariaMod.Instance.GetPacket();
            packet.Write(PacketId);
            packet.Write((byte)owner);
            packet.Write(cursorPosition.X);
            packet.Write(cursorPosition.Y);
            NetworkProfiler.RecordSend(PacketId, packet);
            packet.Send();
        }

        /// <summary>
        /// Returns the exact local cursor in single player/on the owning client,
        /// the exact accepted cursor on the server, and the shared interpolated
        /// cursor on observing clients.
        /// </summary>
        public static bool TryGetCursor(int owner, out Vector2 cursorPosition)
        {
            cursorPosition = default;
            if (owner < 0 || owner >= Main.maxPlayers)
                return false;

            if (Main.netMode != NetmodeID.Server && owner == Main.myPlayer)
            {
                cursorPosition = Main.MouseWorld;
                return true;
            }

            if (!HasCachedCursor[owner])
                return false;

            if (Main.netMode == NetmodeID.MultiplayerClient)
                UpdateRemoteCursor(owner);

            cursorPosition = CachedCursor[owner];
            return true;
        }

        public static void HandlePacket(BinaryReader reader, int whoAmI)
        {
            int owner = reader.ReadByte();
            Vector2 cursorPosition = new Vector2(reader.ReadSingle(), reader.ReadSingle());

            if (owner < 0 || owner >= Main.maxPlayers || !IsFiniteWorldPosition(cursorPosition))
                return;

            if (Main.netMode == NetmodeID.Server)
            {
                if (owner != whoAmI
                    || !Main.player[owner].active
                    || !HasTrackedOrStartupFollower(owner))
                    return;

                ulong currentTick = Main.GameUpdateCount;
                if (HasAcceptedServerCursor[owner]
                    && currentTick - LastServerAcceptTick[owner] < SyncIntervalTicks)
                {
                    return;
                }

                HasAcceptedServerCursor[owner] = true;
                LastServerAcceptTick[owner] = currentTick;
                CachedCursor[owner] = cursorPosition;
                HasCachedCursor[owner] = true;

                ModPacket relay = SariaMod.Instance.GetPacket();
                relay.Write(PacketId);
                relay.Write((byte)owner);
                relay.Write(cursorPosition.X);
                relay.Write(cursorPosition.Y);
                NetworkProfiler.RecordSend(PacketId, relay);
                relay.Send(-1, whoAmI);
                return;
            }

            if (Main.netMode == NetmodeID.MultiplayerClient && owner != Main.myPlayer)
            {
                RemoteCursorTarget[owner] = cursorPosition;
                if (!RemoteCursorInitialized[owner])
                {
                    CachedCursor[owner] = cursorPosition;
                    RemoteCursorInitialized[owner] = true;
                }

                HasCachedCursor[owner] = true;
            }
        }

        internal static void Reset()
        {
            for (int owner = 0; owner < Main.maxPlayers; owner++)
            {
                ResetOwner(owner);
                WasPlayerActive[owner] = false;
                HadAllowedFollower[owner] = false;
                AllowedFollowerPresent[owner] = false;
            }
        }

        internal static void ResetChangedPlayerSlots()
        {
            for (int owner = 0; owner < Main.maxPlayers; owner++)
                AllowedFollowerPresent[owner] = false;

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile projectile = Main.projectile[i];
                if (projectile.active
                    && projectile.owner >= 0
                    && projectile.owner < Main.maxPlayers
                    && IsAllowedFollower(projectile))
                {
                    AllowedFollowerPresent[projectile.owner] = true;
                }
            }

            for (int owner = 0; owner < Main.maxPlayers; owner++)
            {
                bool isActive = Main.player[owner].active;
                bool hasAllowedFollower = isActive && AllowedFollowerPresent[owner];

                if (isActive != WasPlayerActive[owner])
                {
                    ResetOwner(owner);
                    WasPlayerActive[owner] = isActive;
                    HadAllowedFollower[owner] = hasAllowedFollower;
                    continue;
                }

                // The cache was cleared when the previous follower disappeared.
                // Keep the first fresh cursor packet when a new follower appears.
                if (HadAllowedFollower[owner] && !hasAllowedFollower)
                    ResetOwner(owner);

                HadAllowedFollower[owner] = hasAllowedFollower;
            }
        }

        private static void ResetOwner(int owner)
        {
            CachedCursor[owner] = Vector2.Zero;
            RemoteCursorTarget[owner] = Vector2.Zero;
            HasCachedCursor[owner] = false;
            RemoteCursorInitialized[owner] = false;
            HasSentOwnerCursor[owner] = false;
            HasAcceptedServerCursor[owner] = false;
            LastOwnerSendTick[owner] = 0;
            LastServerAcceptTick[owner] = 0;
            LastRemoteInterpolationTick[owner] = ulong.MaxValue;
        }

        private static void UpdateRemoteCursor(int owner)
        {
            ulong currentTick = Main.GameUpdateCount;
            if (LastRemoteInterpolationTick[owner] == currentTick)
                return;

            LastRemoteInterpolationTick[owner] = currentTick;
            Vector2 currentPosition = CachedCursor[owner];
            Vector2 targetPosition = RemoteCursorTarget[owner];
            float distanceSquared = Vector2.DistanceSquared(currentPosition, targetPosition);

            if (distanceSquared > RemoteCursorSnapDistance * RemoteCursorSnapDistance
                || distanceSquared <= RemoteCursorSettleDistance * RemoteCursorSettleDistance)
            {
                CachedCursor[owner] = targetPosition;
            }
            else
            {
                CachedCursor[owner] = Vector2.Lerp(
                    currentPosition,
                    targetPosition,
                    RemoteCursorLerpFactor);
            }
        }

        private static bool HasTrackedOrStartupFollower(int owner)
        {
            if (HadAllowedFollower[owner])
                return true;

            // A projectile can spawn after PreUpdatePlayers. Scan only for that
            // startup packet; steady-state packets use the tracked flag above.
            if (!HasAllowedOwnedFollower(owner))
                return false;

            if (!WasPlayerActive[owner])
            {
                ResetOwner(owner);
                WasPlayerActive[owner] = true;
            }

            HadAllowedFollower[owner] = true;
            AllowedFollowerPresent[owner] = true;
            return true;
        }

        private static bool HasAllowedOwnedFollower(int owner)
        {
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile projectile = Main.projectile[i];
                if (projectile.active
                    && projectile.owner == owner
                    && IsAllowedFollower(projectile))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsAllowedFollower(Projectile projectile)
        {
            return projectile.ModProjectile is Ztarget4
                or ZtargetReal
                or Locator
                or HealCursor
                or HealCursorVisual
                or HealBarrier
                or HealBubble;
        }

        private static bool IsFiniteWorldPosition(Vector2 position)
        {
            if (float.IsNaN(position.X)
                || float.IsNaN(position.Y)
                || float.IsInfinity(position.X)
                || float.IsInfinity(position.Y))
            {
                return false;
            }

            float worldWidth = Main.maxTilesX * 16f;
            float worldHeight = Main.maxTilesY * 16f;
            return position.X >= -WorldBoundsMargin
                && position.Y >= -WorldBoundsMargin
                && position.X <= worldWidth + WorldBoundsMargin
                && position.Y <= worldHeight + WorldBoundsMargin;
        }
    }

    internal sealed class SariaCursorResetSystem : ModSystem
    {
        public override void PreUpdatePlayers()
        {
            SariaCursorNetworking.ResetChangedPlayerSlots();
        }

        public override void OnWorldLoad()
        {
            SariaCursorNetworking.Reset();
        }

        public override void OnWorldUnload()
        {
            SariaCursorNetworking.Reset();
        }
    }
}
