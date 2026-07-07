using Terraria;
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
    public class Frozen2 : ModBuff
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Frozen");
            Description.SetDefault("The Cold Air Freezes your body!");
            Main.debuff[base.Type] = true;
            Main.pvpBuff[base.Type] = true;
            Main.buffNoSave[base.Type] = true;
            Main.buffNoTimeDisplay[base.Type] = false;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            player.frozen = true;
        }
        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.AddBuff(BuffID.Frozen, 2);
        }
    }
}