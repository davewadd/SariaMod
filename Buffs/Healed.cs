using Terraria;
using Terraria.ModLoader;
namespace SariaMod.Buffs
{
    public class Healed : ModBuff
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Healed");
            Description.SetDefault("Saria's aqua veil has healed you slightly");
            Main.debuff[base.Type] = false;
            Main.pvpBuff[base.Type] = true;
            Main.buffNoSave[base.Type] = true;
            Main.buffNoTimeDisplay[base.Type] = true;
        }
        public static int healing;
        public override void Update(Player player, ref int buffIndex)
        {
        }
    }
}