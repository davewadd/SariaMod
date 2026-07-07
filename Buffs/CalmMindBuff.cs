using SariaMod.Items.Ruby;
using Terraria;
using Terraria.ModLoader;
namespace SariaMod.Buffs
{
    public class CalmMindBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Calm Mind");
            Description.SetDefault("The fragrance of this candle brings peace to all entities, even to common enemies");
            Main.debuff[base.Type] = false;
            Main.buffNoSave[base.Type] = true;
            Main.buffNoTimeDisplay[base.Type] = true;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            FairyPlayer modPlayer = player.Fairy();
            SariaModUtilities.Fairy(player).CalmMind = true;
        }
    }
}