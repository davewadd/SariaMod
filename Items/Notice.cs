using Microsoft.Xna.Framework;



using SariaMod.Items;

using System;
using SariaMod.Buffs;
using SariaMod.Dusts;
using Terraria;
using Terraria.ID;
using SariaMod.Items.Strange;
using SariaMod.Items.Sapphire;
using Terraria.ModLoader;

namespace SariaMod.Items
{


    public class Notice : ModProjectile
    {
       

        public const float DistanceToCheck = 1100f;

        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Mother");
            Main.projFrames[base.projectile.type] = 1;
            Main.projPet[projectile.type] = true;
             ProjectileID.Sets.MinionSacrificable[base.projectile.type] = false;
            ProjectileID.Sets.MinionTargettingFeature[base.projectile.type] = true;
        }
        public override bool? CanCutTiles()
        {
            return false;
        }
        public override bool MinionContactDamage()
        {
           
            {
                return false;
            }
        }
       

        public override void SetDefaults()
        {
           
            base.projectile.width = 11;
            base.projectile.height = 27;
            
            base.projectile.netImportant = true;
            base.projectile.friendly = true;
            
            base.projectile.ignoreWater = false;
            base.projectile.usesLocalNPCImmunity = true;
             base.projectile.localNPCHitCooldown = 50;
                base.projectile.minionSlots = 0f;
            base.projectile.timeLeft = 100;
            base.projectile.penetrate = -1;
            base.projectile.tileCollide = false;
            base.projectile.minion = true;
        }
       
        public override void AI()
        {

            Player player = Main.player[base.projectile.owner];
            FairyPlayer modPlayer = player.Fairy();

            if (projectile.timeLeft >= 200)
            {
                Main.PlaySound(SoundID.Item30, base.projectile.Center);
            }




            if (projectile.timeLeft == 100)
            {
                Main.PlaySound(base.mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Notice"), base.projectile.Center);
            }

            Projectile mother = Main.projectile[(int)base.projectile.ai[0]];
            if (!mother.active)
            {
                base.projectile.Kill();
                return;
            }
           if (player.ownedProjectileCounts[ModContent.ProjectileType<Anger>()] >= 1f || (mother.velocity.X >= .5f))
            {
                projectile.Kill();
            }

                projectile.position.X = mother.Center.X;
            projectile.position.Y = mother.Center.Y-70;
            projectile.spriteDirection = mother.spriteDirection;



           










            }
    }



}