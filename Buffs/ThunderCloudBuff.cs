using SariaMod.Items.Topaz;
using Terraria;
using Terraria.ModLoader;
namespace SariaMod.Buffs
{
    public class ThunderCloudBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("ThunderCloud");
            Description.SetDefault("The ThunderCloud will attack any enemy from any distance if it can see it! Will consume 25% Mana to fire bolt.");
            Main.debuff[base.Type] = false;
            Main.buffNoSave[base.Type] = true;
            Main.buffNoTimeDisplay[base.Type] = true;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            FairyPlayer modPlayer = player.Fairy();
            if (((player.ownedProjectileCounts[ModContent.ProjectileType<LightningCloud>()] > 0f)))
            {
                player.buffTime[buffIndex] = 4;
            }
            else
            {
                player.DelBuff(buffIndex);
                buffIndex--;
            }
        }
    }
}