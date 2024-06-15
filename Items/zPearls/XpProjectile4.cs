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


    public class XpProjectile4 : ModProjectile
    {


        public const float DistanceToCheck = 1100f;

        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Mother");
            Main.projFrames[base.Projectile.type] = 1;
            Main.projPet[Projectile.type] = true;
            ProjectileID.Sets.MinionSacrificable[base.Projectile.type] = false;
            ProjectileID.Sets.MinionTargettingFeature[base.Projectile.type] = true;
        }
        public override bool? CanCutTiles()
        {
            return false;
        }
        public override bool MinionContactDamage()
        {
            Player player = Main.player[base.Projectile.owner];
            FairyPlayer modPlayer = player.Fairy();
            NPC target = base.Projectile.Center.MinionHoming(500f, player);
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

            base.Projectile.width = 96;
            base.Projectile.height = 78;

            base.Projectile.netImportant = true;
            base.Projectile.friendly = true;
            Projectile.alpha = 300;
            base.Projectile.ignoreWater = false;
            base.Projectile.usesLocalNPCImmunity = true;
             base.Projectile.localNPCHitCooldown = 50;
            base.Projectile.minionSlots = 0f;
            base.Projectile.timeLeft = 1800;
            base.Projectile.penetrate = -1;
            base.Projectile.tileCollide = false;
            
            base.Projectile.minion = true;
        }
       
        public override void AI()
        {
            float sneezespot = 5;
            float dustspot = 14;
            float dustspeed = 40;
           
            Player player = Main.player[base.Projectile.owner];
            FairyPlayer modPlayer = player.Fairy();
           
            //////////////////////////////faces start
            Vector2 idlePosition2 = player.Center;
            float minionPositionOffsetX2 = ((60 + Projectile.minionPos / 80) * player.direction) - 15;
            idlePosition2.Y -= 15f;
            idlePosition2.X += minionPositionOffsetX2;
            Vector2 vectorToIdlePosition3 = idlePosition2 - Projectile.Center;
            float distanceToIdlePosition3 = vectorToIdlePosition3.Length();
            


                Vector2 idlePosition = player.Center;

            float nothing = 1;
            float speed = 2;



            bool foundTarget = false;

            float minionPositionOffsetX = ((60 + Projectile.minionPos / 80) * player.direction) - 15;
            idlePosition.Y -= 15f;
            idlePosition.X += minionPositionOffsetX;
            Vector2 vectorToIdlePosition = idlePosition - Projectile.Center;

            float distanceToIdlePosition = vectorToIdlePosition.Length();

            Lighting.AddLight(Projectile.Center, Color.OrangeRed.ToVector3() * 3f);

            Vector2 direction = idlePosition - Projectile.Center;
           

            if (foundTarget)
            {
                {
                    speed = 2;
                    Projectile.velocity = (((Projectile.velocity * (13 - speed) + direction) / 20) * nothing);

                }
            }
            if (!foundTarget)
            {
                nothing = 1;

            }
            if ((base.Projectile.frame >= 20) && (base.Projectile.frame <= 69) && (distanceToIdlePosition <= 500) && (Math.Abs(Projectile.velocity.X) <= .5) && (player.statLife >= player.statLifeMax2))
            {
                nothing = 0;
            }
            else
            {
                nothing = 1;
            }
            Projectile.velocity = ((Projectile.velocity * (13 - speed) + direction) / 20) * nothing;

          
            if (!foundTarget)
            {
                if (Projectile.velocity.X >= 0.25)
                {
                    Projectile.spriteDirection = 1;
                }
                if (Projectile.velocity.X <= -0.25)
                {
                    Projectile.spriteDirection = -1;
                }
            }






       
        }
    }



}