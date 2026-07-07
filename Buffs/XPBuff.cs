using Terraria;
using Terraria.ModLoader;
namespace SariaMod.Buffs
{
    public class XPBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("XP Checker");
            Description.SetDefault("While this buff is active you can see Saria's XP level");
            Main.debuff[base.Type] = false;
            Main.buffNoSave[base.Type] = true;
            Main.buffNoTimeDisplay[base.Type] = true;
        }
        public override void Update(Player player, ref int buffIndex)
        {
        }
    }
}