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
    public class TriforceofCourage : ModBuff
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Triforce Piece of Courage!");
            Description.SetDefault("If you don't hear from me in a month, send Link!\n Defense and weapon size are much higher at the cost of speed!");
            Main.debuff[base.Type] = true;
            Main.pvpBuff[base.Type] = true;
            Main.buffNoSave[base.Type] = true;
            Main.buffNoTimeDisplay[base.Type] = true;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            player.moveSpeed -= 0.7f;
            // Increase the player's attack speed by 25%
            // This affects all damage types (melee, ranged, etc.)
            player.GetDamage(DamageClass.Generic) += 0.20f;
            player.GetAttackSpeed(DamageClass.Melee) -= .92f;
            player.GetKnockback(DamageClass.Melee) += 10f;
        }
    }
}