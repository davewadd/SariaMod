using Microsoft.Xna.Framework;
using SariaMod.Dusts;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.Sapphire
{
    internal static class HealBubbleSpawnHelper
    {
        public const int HealingPerRequest = 10;

        private const int MaxLiveBubblesPerOwner = 20;
        private const int MaxStoredHealth = 250;
        private static readonly bool[] HasShownOverflowPop = new bool[Main.maxPlayers];
        private static readonly ulong[] LastOverflowPopUpdate = new ulong[Main.maxPlayers];

        public static void SpawnOrOverflow(IEntitySource source, Vector2 position, Vector2 velocity, int damage, float knockback, int owner)
        {
            if (owner != Main.myPlayer || owner < 0 || owner >= Main.maxPlayers)
            {
                return;
            }

            int healBubbleType = ModContent.ProjectileType<HealBubble>();
            int liveBubbleCount = 0;

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile projectile = Main.projectile[i];
                if (projectile.active && projectile.owner == owner && projectile.type == healBubbleType)
                {
                    liveBubbleCount++;
                    if (liveBubbleCount >= MaxLiveBubblesPerOwner)
                    {
                        CreditOverflow(owner, HealingPerRequest);
                        return;
                    }
                }
            }

            int projectileIndex = Projectile.NewProjectile(source, position, velocity, healBubbleType, damage, knockback, owner, HealingPerRequest);
            if (projectileIndex < 0 || projectileIndex >= Main.maxProjectiles)
            {
                CreditOverflow(owner, HealingPerRequest);
                return;
            }

            Projectile spawnedProjectile = Main.projectile[projectileIndex];
            if (!spawnedProjectile.active || spawnedProjectile.owner != owner || spawnedProjectile.type != healBubbleType)
            {
                CreditOverflow(owner, HealingPerRequest);
            }
        }

        public static void PopAndCredit(Vector2 position, int owner, int healingValue)
        {
            ShowCursorPop(position);

            if (owner == Main.myPlayer && owner >= 0 && owner < Main.maxPlayers)
            {
                AddStoredHealth(Main.player[owner], healingValue);
            }
        }

        private static void CreditOverflow(int owner, int healingValue)
        {
            AddStoredHealth(Main.player[owner], healingValue);

            ulong currentUpdate = Main.GameUpdateCount;
            if (HasShownOverflowPop[owner] && LastOverflowPopUpdate[owner] == currentUpdate)
            {
                return;
            }

            HasShownOverflowPop[owner] = true;
            LastOverflowPopUpdate[owner] = currentUpdate;
            ShowCursorPop(Main.MouseWorld + new Vector2(-20f, -5f));
        }

        private static void AddStoredHealth(Player player, int healingValue)
        {
            FairyPlayer modPlayer = player.Fairy();
            modPlayer.StoredHealth = System.Math.Clamp(
                modPlayer.StoredHealth + healingValue,
                0,
                MaxStoredHealth);
        }

        private static void ShowCursorPop(Vector2 position)
        {
            if (Main.dedServ)
            {
                return;
            }

            for (int i = 0; i < 30; i++)
            {
                Vector2 speed = Main.rand.NextVector2CircularEdge(.5f, .5f);
                Dust dust = Dust.NewDustPerfect(position, ModContent.DustType<HealingDust>(), speed * 15, Scale: 1f);
                dust.noGravity = true;
            }

            SoundEngine.PlaySound(SoundID.Item86, position);
        }
    }
}
