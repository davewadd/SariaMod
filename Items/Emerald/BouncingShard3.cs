using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using SariaMod.Items.Strange;
using Terraria.Audio;
using Terraria.Graphics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.GameContent.Events;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria.ObjectData;
using SariaMod.Items;
using SariaMod.Buffs;
using SariaMod.Items.zTalking;
using SariaMod.Dusts;
using SariaMod.Items.Amber;
using SariaMod.Items.Amethyst;
using SariaMod.Items.Bands;
using SariaMod.Items.Emerald;
using SariaMod.Items.Ruby;
using SariaMod.Items.Sapphire;
using SariaMod.Items.Topaz;
using SariaMod.Items.zPearls;
using Terraria.Localization;
namespace SariaMod.Items.Emerald
{
    public class BouncingShard3 : BaseBouncingShard
    {
        protected override Color LightColor => Color.Silver;
        protected override int InitialTimeLeft => 2300;
        protected override Color TrailColor => Color.Silver;
        protected override string ShardTextureName => "RupeeShard3";
        protected override string MaskTextureName => "RupeeShardMask3";
    }
}
