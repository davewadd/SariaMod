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


    public class Happiness : ModProjectile
    {
       

        public const float DistanceToCheck = 1100f;

        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Mother");
            Main.projFrames[base.projectile.type] = 2;
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
           
            base.projectile.width = 96;
            base.projectile.height = 78;
            
            base.projectile.netImportant = true;
            base.projectile.friendly = true;
            
            base.projectile.ignoreWater = false;
            base.projectile.usesLocalNPCImmunity = true;
             base.projectile.localNPCHitCooldown = 50;
                base.projectile.minionSlots = 0f;
            base.projectile.timeLeft = 80;
            base.projectile.penetrate = -1;
            base.projectile.tileCollide = false;
            base.projectile.timeLeft *= 5;
            base.projectile.minion = true;
        }
       
        public override void AI()
        {

            Player player = Main.player[base.projectile.owner];
            FairyPlayer modPlayer = player.Fairy();
           
           
           
           
             
            

            
            Projectile mother = Main.projectile[(int)base.projectile.ai[0]];
            if (!mother.active)
            {
                base.projectile.Kill();
                return;
            }
           
           
            projectile.position.X = mother.Center.X;
            projectile.position.Y = mother.Center.Y-70;
            projectile.spriteDirection = mother.spriteDirection;



            int frameSpeed = 60; //reduced by half due to framecounter speedup
            projectile.frameCounter += 2;
            if (projectile.frameCounter >= frameSpeed)
            {
                base.projectile.frameCounter++;
                if (base.projectile.frameCounter > 2)
                {
                    base.projectile.frame++;
                    base.projectile.frameCounter = 0;
                }
                if (base.projectile.frame >= Main.projFrames[base.projectile.type])
                {
                    base.projectile.frame = 0;
                }
            }










            }
    }



}