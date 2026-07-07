using Terraria;
using Terraria.Audio;
using Terraria.ID;
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
    public class Overcharged : ModBuff
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Overcharged");
            Description.SetDefault("Saria has now become overcharged!\nHer attacks now have added effects ");
            Main.debuff[base.Type] = true;
            Main.pvpBuff[base.Type] = false;
            Main.buffNoSave[base.Type] = false;
            Main.buffNoTimeDisplay[base.Type] = false;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            if (player.buffTime[buffIndex] <= 10)
            {
                if (!player.HasBuff(ModContent.BuffType<Drained>()))
                {
                    player.AddBuff(ModContent.BuffType<Drained>(), 30000);
                }
            }
            if (player.buffTime[buffIndex] == 3750 || player.buffTime[buffIndex] == 3000 || player.buffTime[buffIndex] == 2000 || player.buffTime[buffIndex] == 1000 || player.buffTime[buffIndex] == 900 || player.buffTime[buffIndex] == 800 || player.buffTime[buffIndex] == 700 || player.buffTime[buffIndex] == 600 || player.buffTime[buffIndex] == 500 || player.buffTime[buffIndex] == 400 || player.buffTime[buffIndex] == 300 || player.buffTime[buffIndex] == 200)
            {
                SoundEngine.PlaySound(SoundID.DD2_DarkMageCastHeal, player.Center);
            }
        }
    }
}