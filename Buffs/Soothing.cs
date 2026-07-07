using Terraria;
using Terraria.ModLoader;
namespace SariaMod.Buffs
{
    public class Soothing : ModBuff
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Soothing");
            Description.SetDefault("Saria's old Wounds are at ease for now.");
            Main.debuff[base.Type] = true;
            Main.pvpBuff[base.Type] = false;
            Main.buffNoSave[base.Type] = false;
            Main.buffNoTimeDisplay[base.Type] = false;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            if (player.HasBuff(ModContent.BuffType<StatLower>()))
            {
                player.buffTime[buffIndex] -= 2;
            }
        }
    }
}