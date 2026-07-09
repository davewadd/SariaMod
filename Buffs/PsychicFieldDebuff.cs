using Microsoft.Xna.Framework;
using SariaMod.Dusts;
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

        public override void Update(NPC npc, ref int buffIndex)
        {
            // The multiplier is set by PsychicFieldProjectile.RefreshEnemyBuffs each tick while
            // the NPC stays inside a field. Once the NPC leaves, the stored value persists as
            // the debuff timer ticks down (linger period).
            // FairyGlobalNPC.ResetEffects clears psychicFieldMultiplier only when the buff expires.

            if (Main.rand.NextBool(3))
            {
                Vector2 dustPosition = npc.position + new Vector2(Main.rand.NextFloat(npc.width), Main.rand.NextFloat(npc.height));
                Dust dust = Dust.NewDustPerfect(dustPosition, ModContent.DustType<Psychic2>(), Vector2.Zero, Scale: 1.15f);
                dust.noGravity = true;
                dust.velocity = (dustPosition - npc.Center).SafeNormalize(Vector2.UnitY) * 1.4f;
            }
        }
    }
}
