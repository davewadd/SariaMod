using Microsoft.Xna.Framework;
using SariaMod.Buffs;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Gores
{
    public class LocatorShard : ModGore
    {
        // The old projectile used timeLeft = 150 with extraUpdates = 1, so it lasted 75 ticks.
        private const int Lifetime = 75;

        public static void Spawn(IEntitySource source, Vector2 position, Vector2 velocity, int owner)
        {
            if (Main.netMode == NetmodeID.Server)
            {
                return;
            }

            if (owner < 0 || owner >= Main.maxPlayers)
            {
                return;
            }

            Player player = Main.player[owner];
            if (!player.active || !player.HasBuff(ModContent.BuffType<StatLower>()))
            {
                return;
            }

            int goreType = ModContent.GoreType<LocatorShard>();
            Gore gore = Gore.NewGorePerfect(source, position, velocity, goreType);
            gore.timeLeft = Lifetime;
            gore.alpha = 0;
        }

        public override bool Update(Gore gore)
        {
            // Match the old projectile's extraUpdates = 1 movement without collision or gravity.
            gore.position += gore.velocity * 2f;
            Lighting.AddLight(gore.position, Color.HotPink.ToVector3() * 2f);

            gore.timeLeft--;
            int oldProjectileTimeLeft = gore.timeLeft * 2;
            if (oldProjectileTimeLeft < 85)
            {
                gore.alpha = 255 - (byte)MathHelper.Clamp(oldProjectileTimeLeft * 3f, 0f, 255f);
            }

            if (gore.timeLeft <= 0)
            {
                gore.active = false;
            }

            return false;
        }
    }
}
