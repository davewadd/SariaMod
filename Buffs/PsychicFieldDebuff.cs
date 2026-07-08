using Microsoft.Xna.Framework;
using SariaMod.Dusts;
using Terraria;
using Terraria.ModLoader;

namespace SariaMod.Buffs
{
    public class PsychicFieldDebuff : ModBuff
    {
        public override string Texture => "SariaMod/Buffs/SariaCurse2";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Psychic Field");
            Description.SetDefault("Psychic force is pulling extra strikes through space");
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
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
