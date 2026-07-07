using Terraria;
using Terraria.ModLoader;
namespace SariaMod.Buffs
{
    public class PassiveHealing : ModBuff
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("PassiveHealing");
            Description.SetDefault("The water helps Saria to heal your wounds");
            Main.debuff[base.Type] = false;
            Main.pvpBuff[base.Type] = true;
            Main.buffNoSave[base.Type] = true;
            Main.buffNoTimeDisplay[base.Type] = true;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            player.GetModPlayer<FairyPlayer>().PassiveHealing = true;
        }
    }
}