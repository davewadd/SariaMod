using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using SariaMod.Buffs;
namespace SariaMod.Items.Emerald
{
    public class RupeeXPassive2 : BaseRupeeXPassive
    {
        protected override string PassiveTexturePath => "SariaMod/Items/Emerald/RupeeXPassive2";
        protected override string MaskTexturePath => "SariaMod/Items/Emerald/RupeeMask2";
        protected override int[] OtherPassiveTypesForPositioning => new int[]
        {
            ModContent.ProjectileType<RupeeXPassive>()
        };
        protected override int DefaultDamage => 10;
        protected override int SpikeType => ModContent.ProjectileType<Emeraldspike2>();
        protected override int Change1Type => ModContent.ProjectileType<Change1_2>();
        protected override int Change2Type => ModContent.ProjectileType<Change2_2>();
        protected override string ActivateSound => "SariaMod/Sounds/SilverRupee2";
        protected override int[] DeactivationTargetTypes => new int[]
        {
            ModContent.ProjectileType<RupeeXPassive>(),
            ModContent.ProjectileType<RupeeXPassive3>()
        };
        protected override int ShardItemType => ModContent.ItemType<Items.Emerald.LivingPurpleShard>();
        protected override int FragmentItemType => ModContent.ItemType<Items.Emerald.LivingPurpleFragment>();
        protected override Color KillLightColor => Color.Purple;
        protected override int FragmentStackChance => 40;
        protected override int KillBuffType => ModContent.BuffType<PurpleRupeeBlock>();
        protected override int KillBuffDuration => 3200;
        protected override int[] KillPropagationPassiveTypes => new int[]
        {
            ModContent.ProjectileType<RupeeXPassive>(),
            ModContent.ProjectileType<RupeeXPassive3>()
        };
        protected override int[] KillPropagationChangeTypes => new int[]
        {
            ModContent.ProjectileType<Change1>(),
            ModContent.ProjectileType<Change1_3>()
        };
    }
}
