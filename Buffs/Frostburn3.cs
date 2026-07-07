using Microsoft.Xna.Framework;
using SariaMod.Dusts;
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
    public class Frostburn3 : ModBuff
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("ArcticSpace");
            Description.SetDefault("Space boils your blood with cold");
            Main.debuff[base.Type] = true;
            Main.noWindowBorder = true;
            Main.pvpBuff[base.Type] = true;
            Main.buffNoSave[base.Type] = true;
            Main.buffNoTimeDisplay[base.Type] = false;
        }
        private const int sphereRadius = 30;
        private const int sphereRadius2 = 15;
        public override void Update(Player player, ref int buffIndex)
        {
            player.GetModPlayer<FairyPlayer>().Frostburn2 = true;
            if (Main.rand.NextBool(2))
            {
                float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius * sphereRadius));
                double angle = Main.rand.NextDouble() * 2.0 * Math.PI;
                Dust.NewDust(new Vector2(player.Center.X + radius * (float)Math.Cos(angle), player.Center.Y + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<FrostburnDust>(), 0f, 0f, 0, default(Color), 1.5f);
            }
            if (Main.rand.NextBool(20))
            {
                float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius2 * sphereRadius2));
                double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                Dust.NewDust(new Vector2((player.Center.X) + radius * (float)Math.Cos(angle), (player.Center.Y) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<Cold>(), 0f, 0f, 0, default(Color), 1.5f);
            }
        }
        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.GetGlobalNPC<FairyGlobalNPC>().Frostburn2 = true;
            if (Main.rand.NextBool(2))
            {
                float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius * sphereRadius));
                double angle = Main.rand.NextDouble() * 2.0 * Math.PI;
                Dust.NewDust(new Vector2(npc.Center.X + radius * (float)Math.Cos(angle), npc.Center.Y + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<FrostburnDust>(), 0f, 0f, 0, default(Color), 1.5f);
            }
            if (Main.rand.NextBool(20))
            {
                float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius2 * sphereRadius2));
                double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                Dust.NewDust(new Vector2((npc.Center.X) + radius * (float)Math.Cos(angle), (npc.Center.Y) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<Cold>(), 0f, 0f, 0, default(Color), 1.5f);
            }
        }
    }
}