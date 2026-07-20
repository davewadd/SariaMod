using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.Ruby
{
    /// <summary>
    /// Resolves parent projectiles by their owner-scoped network identity in
    /// multiplayer. Single player keeps using direct projectile slots so the
    /// existing local behavior is unchanged.
    /// </summary>
    internal static class RovaProjectileLink
    {
        internal static int GetHandle(Projectile projectile)
        {
            return Main.netMode == NetmodeID.SinglePlayer
                ? projectile.whoAmI
                : projectile.identity;
        }

        internal static Projectile Find<T>(int owner, int handle) where T : ModProjectile
        {
            if (Main.netMode == NetmodeID.SinglePlayer)
            {
                if (handle >= 0 && handle < Main.maxProjectiles)
                {
                    Projectile direct = Main.projectile[handle];
                    if (direct.active && direct.owner == owner && direct.ModProjectile is T)
                        return direct;
                }

                return null;
            }

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile candidate = Main.projectile[i];
                if (candidate.active
                    && candidate.owner == owner
                    && candidate.identity == handle
                    && candidate.ModProjectile is T)
                {
                    return candidate;
                }
            }

            return null;
        }

        internal static Projectile Find<T>(int owner, int handle, ref int cachedIndex) where T : ModProjectile
        {
            if (cachedIndex >= 0 && cachedIndex < Main.maxProjectiles)
            {
                Projectile cached = Main.projectile[cachedIndex];
                if (Matches<T>(cached, owner, handle))
                    return cached;
            }

            Projectile resolved = Find<T>(owner, handle);
            cachedIndex = resolved?.whoAmI ?? -1;
            return resolved;
        }

        internal static bool Matches<T>(Projectile candidate, int owner, int handle) where T : ModProjectile
        {
            if (candidate == null || !candidate.active || candidate.owner != owner || candidate.ModProjectile is not T)
                return false;

            return Main.netMode == NetmodeID.SinglePlayer
                ? candidate.whoAmI == handle
                : candidate.identity == handle;
        }

        /// <summary>
        /// Synchronizes authoritative Rova state. Dedicated servers are never
        /// the owner of player-owned projectiles, so Projectile.netUpdate is
        /// ignored for them by Terraria's projectile update loop.
        /// </summary>
        internal static void SyncState(Projectile projectile)
        {
            if (projectile == null || !projectile.active)
                return;

            if (Main.netMode == NetmodeID.Server)
            {
                NetMessage.SendData(
                    MessageID.SyncProjectile,
                    -1,
                    -1,
                    null,
                    projectile.whoAmI);
            }
            else if (Main.netMode == NetmodeID.MultiplayerClient
                && Main.myPlayer == projectile.owner)
            {
                projectile.netUpdate = true;
            }
        }
    }
}
