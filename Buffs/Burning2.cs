using Microsoft.Xna.Framework;
using SariaMod.Dusts;
using SariaMod.Gores;
using System;
using Terraria;
using Terraria.ModLoader;
namespace SariaMod.Buffs
{
    /*
	 * This file contains all the code necessary for a minion
	 * - ModItem
	 *     the weapon which you use to summon the minion with
	 * - ModBuff
	 *     the icon you can click on to despawn the minion
	 * - ModProjectile 
	 *     the minion itself
	 *     
	 * It is not recommended to put all these classes in the same file. For demonstrations sake they are all compacted together so you get a better overwiew.
	 * To get a better understanding of how everything works together, and how to code minion AI, read the guide: https://github.com/tModLoader/tModLoader/wiki/Basic-Minion-Guide
	 * This is NOT an in-depth guide to advanced minion AI
	 */
    public class Burning2 : ModBuff
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Burning");
            Description.SetDefault("The Hot Air burns your body!");
            Main.debuff[base.Type] = true;
            Main.pvpBuff[base.Type] = true;
            Main.buffNoSave[base.Type] = true;
            Main.buffNoTimeDisplay[base.Type] = false;
        }
        private const int sphereRadius = 30;

        public override void Update(Player player, ref int buffIndex)
        {
            player.GetModPlayer<FairyPlayer>().Burning2 = true;
            Lighting.AddLight(player.Center, new Vector3(1f, 0.32f, 0.04f) * 0.65f);
            // Keep the yellow motes as an accent. The red-orange smoke mote now
            // appears at the same rate so the particle mix reads as fire instead
            // of a cloud of mostly yellow dust.
            if (Main.rand.NextBool(8) && VisualDustLimiter.TryReserveHalfCapacitySlot())
            {
                float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius * sphereRadius));
                double angle = Main.rand.NextDouble() * 2.0 * Math.PI;
                Dust.NewDust(new Vector2(player.Center.X + radius * (float)Math.Cos(angle), player.Center.Y + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<FlameDust>(), 0f, 0f, 0, default(Color), 1.5f);
            }
            if (Main.rand.NextBool(8) && VisualDustLimiter.TryReserveHalfCapacitySlot())
            {
                float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius * sphereRadius));
                double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                Dust.NewDust(new Vector2(player.Center.X + radius * (float)Math.Cos(angle), (player.Center.Y - 15) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<SmokeDust3>(), 0f, 0f, 0, default(Color), 1.5f);
            }
        }
        public override void Update(NPC npc, ref int buffIndex)
        {
            {
                npc.GetGlobalNPC<FairyGlobalNPC>().Burning2 = true;
                // Refresh Charred at the same point tModLoader confirms Burning2 is
                // active. This avoids depending on a separate NPC.UpdateNPC hook and
                // guarantees the visual state exists before the NPC is drawn.
                CharredNPCVisualManager.RefreshCharredEffect(npc.whoAmI);
                if (Main.rand.NextBool(8) && VisualDustLimiter.TryReserveHalfCapacitySlot())
                {
                    float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius * sphereRadius));
                    double angle = Main.rand.NextDouble() * 2.0 * Math.PI;
                    Dust.NewDust(new Vector2(npc.Center.X + radius * (float)Math.Cos(angle), npc.Center.Y + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<FlameDust>(), 0f, 0f, 0, default(Color), 1.5f);
                }
                if (Main.rand.NextBool(8) && VisualDustLimiter.TryReserveHalfCapacitySlot())
                {
                    float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius * sphereRadius));
                    double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                    Dust.NewDust(new Vector2(npc.Center.X + radius * (float)Math.Cos(angle), (npc.Center.Y - 15) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<SmokeDust3>(), 0f, 0f, 0, default(Color), 1.5f);
                }
            }
        }
    }
}
