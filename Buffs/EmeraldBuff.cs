using SariaMod.Items.Ruby;
using Terraria;
using Terraria.ModLoader;
using SariaMod.Items.Emerald;
namespace SariaMod.Buffs
{
    public class EmeraldBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Living Rupee");
            Description.SetDefault("The Emeralds that hover around you can be clicked to activate\n Everytime you stop the shard you click will summon a barrier around you!\n Closing the buff turns the floating Emeralds into items to be reused with Saria.");
            Main.debuff[base.Type] = false;
            Main.buffNoSave[base.Type] = true;
            Main.buffNoTimeDisplay[base.Type] = true;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            FairyPlayer modPlayer = player.Fairy();
            if ((player.ownedProjectileCounts[ModContent.ProjectileType<RupeeXPassive>()] > 0f) || (player.ownedProjectileCounts[ModContent.ProjectileType<RupeeXPassive2>()] > 0f) || (player.ownedProjectileCounts[ModContent.ProjectileType<RupeeXPassive3>()] > 0f))
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