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

namespace SariaMod.Items.zPearls
{


    public class XpProjectile2 : ModProjectile
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
            Player player = Main.player[base.projectile.owner];
            FairyPlayer modPlayer = player.Fairy();
            NPC target = base.projectile.Center.MinionHoming(500f, player);
            if (target != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        
        private const int sphereRadius3 = 1;
        private const int sphereRadius2 = 6;
        private const int sphereRadius4 = 32;
        private const int sphereRadius = 100;
        public override void SetDefaults()
        {

            base.projectile.width = 96;
            base.projectile.height = 78;

            base.projectile.netImportant = true;
            base.projectile.friendly = true;
            projectile.alpha = 300;
            base.projectile.ignoreWater = false;
            base.projectile.usesLocalNPCImmunity = true;
             base.projectile.localNPCHitCooldown = 50;
            base.projectile.minionSlots = 0f;
            base.projectile.timeLeft = 1800;
            base.projectile.penetrate = -1;
            base.projectile.tileCollide = false;
            
            base.projectile.minion = true;
        }
       
        public override void AI()
        {
            float sneezespot = 5;
            float dustspot = 14;
            float dustspeed = 40;
           
            Player player = Main.player[base.projectile.owner];
            FairyPlayer modPlayer = player.Fairy();
           
            //////////////////////////////faces start
            Vector2 idlePosition2 = player.Center;
            float minionPositionOffsetX2 = ((60 + projectile.minionPos / 80) * player.direction) - 15;
            idlePosition2.Y -= 15f;
            idlePosition2.X += minionPositionOffsetX2;
            Vector2 vectorToIdlePosition3 = idlePosition2 - projectile.Center;
            float distanceToIdlePosition3 = vectorToIdlePosition3.Length();
            


                Vector2 idlePosition = player.Center;

            float nothing = 1;
            float speed = 2;



            bool foundTarget = false;

            float minionPositionOffsetX = ((60 + projectile.minionPos / 80) * player.direction) - 15;
            idlePosition.Y -= 15f;
            idlePosition.X += minionPositionOffsetX;
            Vector2 vectorToIdlePosition = idlePosition - projectile.Center;

            float distanceToIdlePosition = vectorToIdlePosition.Length();

            Lighting.AddLight(projectile.Center, Color.OrangeRed.ToVector3() * 3f);

            Vector2 direction = idlePosition - projectile.Center;
           

            if (foundTarget)
            {
                {
                    speed = 2;
                    projectile.velocity = (((projectile.velocity * (13 - speed) + direction) / 20) * nothing);

                }
            }
            if (!foundTarget)
            {
                nothing = 1;

            }
            if ((base.projectile.frame >= 20) && (base.projectile.frame <= 69) && (distanceToIdlePosition <= 500) && (Math.Abs(projectile.velocity.X) <= .5) && (player.statLife >= player.statLifeMax2))
            {
                nothing = 0;
            }
            else
            {
                nothing = 1;
            }
            projectile.velocity = ((projectile.velocity * (13 - speed) + direction) / 20) * nothing;

          
            if (!foundTarget)
            {
                if (projectile.velocity.X >= 0.25)
                {
                    projectile.spriteDirection = 1;
                }
                if (projectile.velocity.X <= -0.25)
                {
                    projectile.spriteDirection = -1;
                }
            }






       
        }
    }



}