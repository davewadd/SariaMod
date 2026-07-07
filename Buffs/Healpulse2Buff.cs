using Terraria;
using Terraria.ModLoader;
namespace SariaMod.Buffs
{
    public class Healpulse2Buff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Healpulse");
            Description.SetDefault("Saria thought you could use some healing while resting");
            Main.debuff[base.Type] = true;
            Main.pvpBuff[base.Type] = true;
            Main.buffNoSave[base.Type] = true;
            Main.buffNoTimeDisplay[base.Type] = false;
        }
        public override void Update(Player player, ref int buffIndex)
        {
        }
    }
}