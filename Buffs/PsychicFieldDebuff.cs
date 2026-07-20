using Terraria;
using Terraria.ModLoader;

namespace SariaMod.Buffs
{
    public class PsychicFieldDebuff : ModBuff
    {
        /// <summary>
        /// How long (in ticks) the debuff lingers after the NPC leaves the field.
        /// 240 ticks = 4 seconds at 60fps.
        /// </summary>
        public const int LingerDuration = 240;

        public override string Texture => "SariaMod/Buffs/SariaCurse2";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Psychic Field");
            Description.SetDefault("Psychic force is pulling extra strikes through space");
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = true;
            // NOT Main.buffNoTimeDisplay — we want the timer to tick down for linger effect
        }
    }
}
