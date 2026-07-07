using SariaMod.Items.LilHarpy;
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
    public class BabyHarpyBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Baby Harpy");
            Description.SetDefault("Cuteness from above!");
            Main.debuff[base.Type] = false;
            Main.pvpBuff[base.Type] = true;
            Main.buffNoSave[base.Type] = false;
            Main.buffNoTimeDisplay[base.Type] = true;
            Main.vanityPet[Type] = true;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            if (((player.ownedProjectileCounts[ModContent.ProjectileType<BabyHarpy>()] > 0f)))
            {
                player.buffTime[buffIndex] = 18000;
            }
            player.buffTime[buffIndex] = 18000;
            bool petProjectileNotSpawned = player.ownedProjectileCounts[ModContent.ProjectileType<BabyHarpy>()] <= 0;
            if (petProjectileNotSpawned && player.whoAmI == Main.myPlayer)
            {
                Projectile.NewProjectile(player.GetSource_FromThis(), player.position.X + 0, player.position.Y + 0, 0, 0, ModContent.ProjectileType<BabyHarpy>(), (int)(0), 0f, player.whoAmI);
            }
        }
    }
}