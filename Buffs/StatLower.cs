using Terraria;
using Terraria.ModLoader;
namespace SariaMod.Buffs
{
    /*
	 * This file contains all the code necessary for a All
	 * - ModItem
	 *     the weapon which you use to summon the All with
	 * - ModBuff
	 *     the icon you can click on to despawn the All
	 * - ModProjectile 
	 *     the All itself
	 *     
	 * It is not recommended to put all these classes in the same file. For demonstrations sake they are all compacted together so you get a better overwiew.
	 * To get a better understanding of how everything works together, and how to code All AI, read the guide: https://github.com/tModLoader/tModLoader/wiki/Basic-All-Guide
	 * This is NOT an in-depth guide to advanced All AI
	 */
    public class StatLower : ModBuff
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("LowStats");
            Description.SetDefault("Saria feels weak in this current environment!\n Defense is drastically lowered!");
            Main.debuff[base.Type] = true;
            Main.pvpBuff[base.Type] = true;
            Main.buffNoSave[base.Type] = true;
            Main.buffNoTimeDisplay[base.Type] = true;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            player.GetModPlayer<FairyPlayer>().Statlowered = true;
        }
    }
}