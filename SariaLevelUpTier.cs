using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using SariaMod.Items.Emerald;
using Terraria.DataStructures;
using SariaMod.Items.Bands;
using SariaMod.Items.zPearls;
using Terraria.Audio;
using Microsoft.Xna.Framework;
using SariaMod.Buffs;
using SariaMod.Diagnostics;
using SariaMod.Dusts;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.UI.Elements;
using ReLogic.Content;
using System;
using System.Reflection;
using SariaMod.Items.Ruby;
using System.Collections.Generic;
using SariaMod.Items.Amber;
using SariaMod.Items.Amethyst;
using SariaMod.Items.Sapphire;
using SariaMod.Items.Topaz;
using SariaMod.Items.zTalking;
using Terraria.Localization;
using Terraria.Map;
using Terraria.GameContent;
using SariaMod.Items.Strange;
using SariaMod.Items.zDinner;
using SariaMod.Gores;
namespace SariaMod
{
    public class SariaLevelUpTier
    {
        public int RequiredXP { get; set; }
        public Func<bool> Condition { get; set; }
    }
}
