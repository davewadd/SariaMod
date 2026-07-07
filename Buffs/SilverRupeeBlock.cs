using SariaMod.Items.Ruby;
using Terraria;
using Terraria.ModLoader;
using SariaMod.Items.Emerald;
namespace SariaMod.Buffs
{
    public class SilverRupeeBlock : ModBuff
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("SilverRupeeBlock");
            Description.SetDefault("You must Wait before you can activate this rupee again");
            Main.debuff[base.Type] = true;
            Main.pvpBuff[base.Type] = false;
            Main.buffNoSave[base.Type] = true;
            Main.buffNoTimeDisplay[base.Type] = false;
        }
        public override void Update(Player player, ref int buffIndex)
        {
        }
    }
}