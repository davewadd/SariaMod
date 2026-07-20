using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.Ruby
{
    internal static class EruptionVisualBudget
    {
        internal const int MaximumFlameTextureDrawsPerFrame = 256;

        private static int remainingFlameTextureDraws = MaximumFlameTextureDrawsPerFrame;

        internal static bool TryReserveFlameTextureDraws(int requestedDraws)
        {
            if (requestedDraws <= 0)
            {
                return true;
            }

            if (remainingFlameTextureDraws < requestedDraws)
            {
                return false;
            }

            remainingFlameTextureDraws -= requestedDraws;
            return true;
        }

        internal static void ResetFrame()
        {
            remainingFlameTextureDraws = MaximumFlameTextureDrawsPerFrame;
        }
    }

    public sealed class EruptionVisualLimitSystem : ModSystem
    {
        public override void PostDrawTiles()
        {
            EruptionVisualBudget.ResetFrame();
        }

        public override void OnWorldLoad()
        {
            EruptionVisualBudget.ResetFrame();
            EruptionSmokeVisuals.ResetGlobalState();
        }

        public override void OnWorldUnload()
        {
            EruptionVisualBudget.ResetFrame();
            EruptionSmokeVisuals.ResetGlobalState();
        }

        public override void Unload()
        {
            EruptionVisualBudget.ResetFrame();
            EruptionSmokeVisuals.ResetGlobalState();
        }
    }

    public sealed class EruptionProjectileLimitGlobal : GlobalProjectile
    {
        internal const int MaximumFlamesPerOwner = 9;
        internal const int MaximumExplosionsPerOwner = 1;
        internal const int MaximumExplosion2PerOwner = 1;
        internal const int MaximumExplosion3PerOwner = 5;

        public override void OnSpawn(Projectile projectile, IEntitySource source)
        {
            int limit = GetLimit(projectile.type);
            if (limit <= 0 || !HasLimitAuthority(projectile))
            {
                return;
            }

            if (CountExisting(projectile.owner, projectile.type, projectile.whoAmI) >= limit)
            {
                projectile.Kill();
            }
        }

        internal static bool CanSpawn(int owner, int projectileType)
        {
            int limit = GetLimit(projectileType);
            return limit <= 0 || CountExisting(owner, projectileType, -1) < limit;
        }

        private static int GetLimit(int projectileType)
        {
            if (projectileType == ModContent.ProjectileType<Flame>())
            {
                return MaximumFlamesPerOwner;
            }

            if (projectileType == ModContent.ProjectileType<Explosion>())
            {
                return MaximumExplosionsPerOwner;
            }

            if (projectileType == ModContent.ProjectileType<Explosion2>())
            {
                return MaximumExplosion2PerOwner;
            }

            if (projectileType == ModContent.ProjectileType<Explosion3>())
            {
                return MaximumExplosion3PerOwner;
            }

            return 0;
        }

        private static bool HasLimitAuthority(Projectile projectile)
        {
            return Main.netMode != NetmodeID.MultiplayerClient || Main.myPlayer == projectile.owner;
        }

        private static int CountExisting(int owner, int projectileType, int ignoredWhoAmI)
        {
            int count = 0;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile candidate = Main.projectile[i];
                if (candidate.active
                    && candidate.whoAmI != ignoredWhoAmI
                    && candidate.owner == owner
                    && candidate.type == projectileType)
                {
                    count++;
                }
            }

            return count;
        }
    }
}
