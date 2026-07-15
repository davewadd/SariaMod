using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace SariaMod.Items.Sapphire
{
    internal static class SapphireBarrierHealing
    {
        internal static void HealFriendlyNPCs(Rectangle barrierHitbox, int healAmount)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient || healAmount <= 0)
            {
                return;
            }

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active
                    || (!npc.townNPC && !npc.friendly)
                    || npc.lifeMax <= 0
                    || npc.life <= 0
                    || npc.life >= npc.lifeMax
                    || npc.dontTakeDamage
                    || npc.immortal
                    || !npc.Hitbox.Intersects(barrierHitbox))
                {
                    continue;
                }

                npc.GetGlobalNPC<FairyGlobalNPC>().TryHealFromSapphireBarrier(npc, healAmount);
            }
        }
    }
}
