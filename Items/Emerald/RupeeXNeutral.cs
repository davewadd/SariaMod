using Microsoft.Xna.Framework;
using SariaMod.Items.Strange;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
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
using Terraria.UI;
using SariaMod;
using Terraria.GameContent.UI.Elements;
using ReLogic.Content;
using Terraria.DataStructures;
namespace SariaMod.Items.Emerald
{
    public class RupeeXNeutral : BaseRupeeXNeutral
    {
        protected override Color LightColor => Color.Green;
        protected override Color KillLightColor => Color.Green;
        protected override int StackChance => 20;
        protected override int ShardCount => 1;
        protected override int FragmentItemType => ModContent.ItemType<Items.Emerald.LivingGreenFragment>();
        protected override int RupeeXPassiveType => ModContent.ProjectileType<RupeeXPassive>();
        protected override int RupeeShardType => ModContent.ProjectileType<RupeeShard>();
        protected override string TextureName => "RupeeXNeutral";
    }
}
