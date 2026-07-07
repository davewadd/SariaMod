using SariaMod.Items.Bands;
using SariaMod.Items.zPearls;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using SariaMod.Items.Emerald;
using Microsoft.Xna.Framework;
using SariaMod.Buffs;
using SariaMod.Dusts;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.UI.Elements;
using ReLogic.Content;
using Terraria.DataStructures;
using System;
namespace SariaMod
{
    public class FairyGlobalBuff : GlobalBuff
    {
        public override void Update(int type, NPC npc, ref int buffIndex)
        {
            if (type == ModContent.BuffType<MeteorSpikeDebuff>() && npc.HasBuff(ModContent.BuffType<MeteorLaunchDebuff>()))
            {
                npc.DelBuff(buffIndex);
                buffIndex--;
            }
        }
    }
}
