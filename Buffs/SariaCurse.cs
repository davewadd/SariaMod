using Microsoft.Xna.Framework;
using SariaMod.Dusts;
using System;
using Terraria;
using Terraria.Audio;
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
    public class SariaCurse : ModBuff
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("SariaCurse");
            Description.SetDefault("Saria Has Cursed You!");
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }
        private const int sphereRadius = 30;
        public override void Update(Player player, ref int buffIndex)
        {
            player.GetModPlayer<FairyPlayer>().SariaCurseD = true;
            if (Main.rand.NextBool(800))
            {
                SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Poe"), player.Center);
            }
            if (Main.rand.NextBool(2))
            {
                float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius * sphereRadius));
                double angle = Main.rand.NextDouble() * 2.0 * Math.PI;
                Dust.NewDust(new Vector2(player.Center.X + radius * (float)Math.Cos(angle), player.Center.Y + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<ShadowFlameDust>(), 0f, 0f, 0, default(Color), 1.5f);
            }
        }
        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.GetGlobalNPC<FairyGlobalNPC>().SariaCurseD = true;
            if (Main.rand.NextBool(800))
            {
                SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Poe"), npc.Center);
            }
            if (Main.rand.NextBool(2))
            {
                float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius * sphereRadius));
                double angle = Main.rand.NextDouble() * 2.0 * Math.PI;
                Dust.NewDust(new Vector2(npc.Center.X + radius * (float)Math.Cos(angle), npc.Center.Y + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<ShadowFlameDust>(), 0f, 0f, 0, default(Color), 1.5f);
            }
        }
    }
}