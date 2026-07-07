using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using SariaMod.Buffs;
namespace SariaMod.Items.Emerald
{
    public class RupeeXPassive3 : BaseRupeeXPassive
    {
        protected override string PassiveTexturePath => "SariaMod/Items/Emerald/RupeeXPassive3";
        protected override string MaskTexturePath => "SariaMod/Items/Emerald/RupeeMask3";
        protected override int[] OtherPassiveTypesForPositioning => new int[]
        {
            ModContent.ProjectileType<RupeeXPassive>(),
            ModContent.ProjectileType<RupeeXPassive2>()
        };
        protected override int DefaultDamage => 30;
        protected override int SpikeType => ModContent.ProjectileType<Emeraldspike3>();
        protected override int Change1Type => ModContent.ProjectileType<Change1_3>();
        protected override int Change2Type => ModContent.ProjectileType<Change2_3>();
        protected override string ActivateSound => "SariaMod/Sounds/SilverRupee3";
        protected override int FrameThreshold1 => 20;
        protected override int FrameThreshold2 => 30;
        protected override int[] DeactivationTargetTypes => new int[]
        {
            ModContent.ProjectileType<RupeeXPassive>(),
            ModContent.ProjectileType<RupeeXPassive2>()
        };
        protected override int ShardItemType => ModContent.ItemType<Items.Emerald.LivingSilverShard>();
        protected override int FragmentItemType => ModContent.ItemType<Items.Emerald.LivingSilverFragment>();
        protected override Color KillLightColor => Color.Purple;
        protected override int FragmentStackChance => 60;
        protected override int KillBuffType => ModContent.BuffType<SilverRupeeBlock>();
        protected override int KillBuffDuration => 3200;
        protected override int[] KillPropagationPassiveTypes => new int[]
        {
            ModContent.ProjectileType<RupeeXPassive>(),
            ModContent.ProjectileType<RupeeXPassive2>()
        };
        protected override int[] KillPropagationChangeTypes => new int[]
        {
            ModContent.ProjectileType<Change1>(),
            ModContent.ProjectileType<Change1_2>()
        };
        protected override void OnBeforeSpikeSpawn(Player player)
        {
            if (Main.myPlayer == Projectile.owner && Damage <= 20) Projectile.NewProjectile(Projectile.GetSource_FromThis(), player.Center.X + 0, player.Center.Y - 60, 0, 0, ModContent.ProjectileType<BufferProj>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
        }
    }
}
