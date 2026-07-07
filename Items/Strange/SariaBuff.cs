using Terraria;
using Terraria.ModLoader;
namespace SariaMod.Items.Strange
{
    public class SariaBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("FairySpirit");
            Description.SetDefault("Saria now watches over you\n-Gain xp with her by fighting to gain new attacks and abilities!\n Saria will only attack while pokeball is held.\n\nYou can charge attacks by holding leftclick with the pokeball\nDon't be afraid to get creative!");
            Main.debuff[base.Type] = false;
            Main.buffNoSave[base.Type] = true;
            Main.buffNoTimeDisplay[base.Type] = true;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            FairyPlayer modPlayer = player.Fairy();
            if (player.ownedProjectileCounts[ModContent.ProjectileType<Saria>()] > 0)
            {
                if (modPlayer.Sarialevel == 0)
                {
                    player.buffTime[buffIndex] = 18000;
                    player.noFallDmg = true;
                }
                if (modPlayer.Sarialevel == 1)
                {
                    player.buffTime[buffIndex] = 18000;
                    player.detectCreature = true;
                    player.noFallDmg = true;
                }
                if (modPlayer.Sarialevel == 2)
                {
                    player.buffTime[buffIndex] = 18000;
                    player.detectCreature = true;
                    player.dangerSense = true;
                    player.noFallDmg = true;
                }
                if (modPlayer.Sarialevel == 3)
                {
                    player.buffTime[buffIndex] = 18000;
                    player.detectCreature = true;
                    player.noFallDmg = true;
                    player.dangerSense = true;
                }
                if (modPlayer.Sarialevel == 4)
                {
                    player.buffTime[buffIndex] = 18000;
                    player.detectCreature = true;
                    player.dangerSense = true;
                    player.noFallDmg = true;
                }
                if (modPlayer.Sarialevel == 5)
                {
                    player.buffTime[buffIndex] = 18000;
                    player.detectCreature = true;
                    player.dangerSense = true;
                    player.noFallDmg = true;
                }
                if (modPlayer.Sarialevel == 6)
                {
                    player.buffTime[buffIndex] = 18000;
                    player.detectCreature = true;
                    player.dangerSense = true;
                    player.noFallDmg = true;
                }
            }
            else
            {
                player.DelBuff(buffIndex);
                buffIndex--;
            }
        }
    }
}