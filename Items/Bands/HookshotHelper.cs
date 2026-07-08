using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Utilities;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using SariaMod.Netcode.HookshotNetworking;
using SariaMod.Buffs;

namespace SariaMod.Items.Bands
{
    /// <summary>
    /// Shared helper methods for hookshot functionality
    /// </summary>
    public static class HookshotHelper
    {
        public static int FindEnemyToHook(Vector2 position, float checkRadius = 24f)
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.immortal)
                    continue;

                float dist = Vector2.Distance(position, npc.Center);
                if (dist < checkRadius + Math.Max(npc.width, npc.height) / 2f)
                {
                    return i;
                }
            }
            return -1;
        }

        public static Projectile FindOwnerHookProjectile(int ownerIndex)
        {
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];
                if (proj.active && proj.owner == ownerIndex)
                {
                    if (proj.type == ModContent.ProjectileType<HookshotProjectile>() ||
                        proj.type == ModContent.ProjectileType<LongshotProjectile>())
                    {
                        return proj;
                    }
                }
            }
            return null;
        }

        public static bool IsHookAttachedToNPC(Projectile hookProj, int npcIndex)
        {
            if (hookProj == null || !hookProj.active)
                return false;

            if (npcIndex < 0 || npcIndex >= Main.maxNPCs)
                return false;

            NPC npc = Main.npc[npcIndex];
            if (!npc.active)
                return false;

            return Vector2.Distance(hookProj.Center, npc.Center) < 32f;
        }
    }
}
