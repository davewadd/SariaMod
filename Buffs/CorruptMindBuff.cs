using Microsoft.Xna.Framework;
using SariaMod.Dusts;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
namespace SariaMod.Buffs
{
    public class CorruptMindBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Corrupt Mind");
            Description.SetDefault("This candle has a slight trace of Saria's curse, did she create this by accident?");
            Main.debuff[base.Type] = false;
            Main.buffNoSave[base.Type] = true;
            Main.buffNoTimeDisplay[base.Type] = true;
        }
        private const int sphereRadius = 30;
        public override void Update(Player player, ref int buffIndex)
        {
            FairyPlayer modPlayer = player.Fairy();
            SariaModUtilities.Fairy(player).CorruptMind = true;
            if (Main.rand.NextBool(4))
            {
                float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius * sphereRadius));
                double angle = Main.rand.NextDouble() * 2.0 * Math.PI;
                Dust.NewDust(new Vector2(player.Center.X + radius * (float)Math.Cos(angle), player.Center.Y + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<BlackSmoke>(), 0f, 0f, 0, default(Color), 1.5f);
            }
        }
    }
}