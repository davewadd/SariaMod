using SariaMod.Items.Ruby;
using Terraria;
using Terraria.ModLoader;
namespace SariaMod.Buffs
{
    public class WillOWispBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Will-O-Wisp");
            Description.SetDefault("The Will-O-Wisps will protect you from projectile attacks. Will burn any other enemies. Can stack up to 8!\n Wisps will also provide Warmth in cold places.");
            Main.debuff[base.Type] = false;
            Main.buffNoSave[base.Type] = true;
            Main.buffNoTimeDisplay[base.Type] = true;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            FairyPlayer modPlayer = player.Fairy();
            if (player.ownedProjectileCounts[ModContent.ProjectileType<WillOWisp>()] > 0f)
            {
                player.buffTime[buffIndex] = 18000;
            }
            else
            {
                player.DelBuff(buffIndex);
                buffIndex--;
            }
        }
    }
}