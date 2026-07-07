using Terraria;
using Terraria.ModLoader;

namespace SariaMod.Buffs
{
    /// <summary>
    /// Cooldown debuff for the hookshot/longshot sweetspot dash.
    /// While active, the auto-dash won't trigger - timer expiration acts as early release.
    /// Sweetspot timing (frames 161-181) still works and will refresh this debuff.
    /// </summary>
    public class DashDeBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Dash Cooldown");
            Description.SetDefault("Cannot auto-dash with hookshot. Time your sweetspot!");
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = false;
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = false;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            // Visual indicator could be added here if desired
        }
    }
}