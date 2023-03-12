using Microsoft.Xna.Framework;



using SariaMod.Items;
using SariaMod.Items.zPearls;
using System;
using SariaMod.Buffs;
using Terraria;
using Terraria.ID;
using SariaMod.Items.Strange;
using SariaMod.Items.Sapphire;
using SariaMod.Items.Ruby;
using SariaMod.Dusts;
using Terraria.ModLoader;

namespace SariaMod.Items
{


    public class DuskBallReturn : ModProjectile
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
          
                return false;
            
        }
       
     
        public override void SetDefaults()
        {

            base.projectile.width = 96;
            base.projectile.height = 78;

            base.projectile.netImportant = true;
            base.projectile.friendly = true;

            base.projectile.ignoreWater = false;
            base.projectile.usesLocalNPCImmunity = true;
             base.projectile.localNPCHitCooldown = 50;
          
            base.projectile.timeLeft = 1800;
            base.projectile.penetrate = -1;
            base.projectile.tileCollide = false;
            projectile.alpha = 300;
            base.projectile.minion = true;
        }
       
        public override void AI()
        {
          
           
            Player player = Main.player[base.projectile.owner];
            FairyPlayer modPlayer = player.Fairy();
           
            //////////////////////////////faces start
          




            
        }
    }



}