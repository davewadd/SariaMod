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


    public class Transform : ModProjectile
    {


        public const float DistanceToCheck = 1100f;

        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Mother");
            Main.projFrames[base.projectile.type] = 99;
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
        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            Player player = Main.player[base.projectile.owner];
            FairyPlayer modPlayer = player.Fairy();
            target.buffImmune[BuffID.CursedInferno] = false;
            target.buffImmune[BuffID.Confused] = false;
            target.buffImmune[BuffID.Slow] = false;
            target.buffImmune[BuffID.ShadowFlame] = false;
            target.buffImmune[BuffID.Ichor] = false;
            target.buffImmune[BuffID.OnFire] = false;
            target.buffImmune[BuffID.Frostburn] = false;
            target.buffImmune[BuffID.Poisoned] = false;
            target.buffImmune[BuffID.Venom] = false;
            target.buffImmune[BuffID.Electrified] = false;

            target.AddBuff(BuffID.Electrified, 300);

            if (player.HasBuff(ModContent.BuffType<StatRaise>()))
            {
                damage = damage;
            }
            else if (player.HasBuff(ModContent.BuffType<StatLower>()))
            {
                damage /= 4;

            }
            else
            {
                damage /= 2;
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



            if (projectile.frame >= 83 && projectile.frame <= 100)
            {
                ;
            }



            int frameSpeed = 30; //reduced by half due to framecounter speedup
            projectile.frameCounter += 2;
            if (projectile.frameCounter >= frameSpeed)
            {
                projectile.frameCounter = 0;
               

                if (base.projectile.frame == 34)
                {
                    Main.PlaySound(base.mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Step2"), base.projectile.Center);
                }
                if (projectile.ai[0] == 0 || projectile.ai[0] == 3 || projectile.ai[0] == 4) //only run these animations if not attacking! no longer overrides
                {
                    if ((projectile.velocity.Y) > -3f && (projectile.velocity.Y) < 4f && Math.Abs(projectile.velocity.X) <= .5) //Idle animation, notice how I have (
                                                                                                                                //
                                                                                                                                //.Y greater than -3f and less than 4f. this DID conflict with the rising and Falling animations but this is how i fixed it.
                    { ////however you set up the attack animation, make sure that none of these other animations override it. 
                      //that's easy legit just
                        projectile.frame++;
                        if (base.projectile.frameCounter <= 76)
                        {

                            base.projectile.frameCounter = 0;
                        }
                        if (base.projectile.frame == 63)
                        {
                            Main.PlaySound(base.mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Step2"), base.projectile.Center);
                        }
                        if (base.projectile.frame >= 76)
                        {
                            base.projectile.frame = 0;
                            Main.PlaySound(base.mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Step1"), base.projectile.Center);
                        }
                      
                    }
                    if ((projectile.velocity.Y) < 4f && Math.Abs(projectile.velocity.X) > 0.5f && Math.Abs(projectile.velocity.X) < 4f) //walking animation and such
                    {
                        projectile.frame++;
                        projectile.frameCounter += 3;

                        if (base.projectile.frame <= 80)
                        {

                            base.projectile.frameCounter = 0;

                        }
                        if (base.projectile.frame >= 80)
                        {
                            base.projectile.frame = 76;
                        }
                        if (base.projectile.frame < 76)
                        {
                            base.projectile.frame = 76;
                        }

                    }

                    if ((projectile.velocity.Y) < 4f && Math.Abs(projectile.velocity.X) >= 4f)//running or (floating) animation
                    {
                        projectile.frame++;

                        if (base.projectile.frameCounter < 83)
                        {

                            base.projectile.frameCounter = 0;

                        }
                        if (base.projectile.frame >= 83)
                        {
                            base.projectile.frame = 80;
                            Main.PlaySound(base.mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Hover"), base.projectile.Center);
                        }
                        if (base.projectile.frame < 80)
                        {

                            base.projectile.frame = 80;
                        }
                    }
                    if ((projectile.velocity.Y) < -3f) //rising animation
                    {
                        projectile.frame++;

                        if (base.projectile.frameCounter < 83)
                        {

                            base.projectile.frameCounter = 0;

                        }
                        if (base.projectile.frame >= 83)
                        {
                            base.projectile.frame = 80;
                        }
                        if (base.projectile.frame < 80)
                        {
                            base.projectile.frame = 80;
                            Main.PlaySound(base.mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Fly"), base.projectile.Center);
                        }
                    }

                    if (projectile.velocity.Y > 4f) //falling animation
                    {
                        projectile.frame++;

                        if (base.projectile.frameCounter < 99)
                        {

                            base.projectile.frameCounter = 0;

                        }
                        if (base.projectile.frame >= 99)
                        {
                            base.projectile.frame = 97;
                        }
                        if (base.projectile.frame < 97)
                        {
                            base.projectile.frame = 97;
                        }
                    }
                }

                //Main.NewText(projectile.ai[0] + " is state, " + projectile.ai[1] + " is timer. Test");

                if (projectile.ai[0] == 4)
                {
                    projectile.ai[1] -= 1; //reduce timer
                    if (projectile.ai[1] == 0)
                    {
                        projectile.ai[0] = 0; //once at 0, back to normal behavior
                    }
                }

                if (projectile.ai[0] == 3) //recovery setup
                {

                    projectile.ai[1] = 4; //4 cycles recovery between shots, adjust this for how long she waits between swipes
                    projectile.ai[0] = 4;
                }

                if (projectile.ai[0] == 2)
                {
                    base.projectile.frame++; //increment attack frame
                    base.projectile.frameCounter += 15;
                    if (base.projectile.frame == 84)
                    {
                        Main.PlaySound(base.mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Hover"), base.projectile.Center);
                    }
                    if (base.projectile.frame == 86)
                    {
                        Main.PlaySound(base.mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Step2"), base.projectile.Center);
                    }
                    if (base.projectile.frame == 89)
                    {
                        base.projectile.frameCounter = 1;
                    }
                    if (base.projectile.frame > 90)
                    {
                        base.projectile.frameCounter = 15;
                    }
                    if (base.projectile.frame == 93)
                    {
                        Main.PlaySound(base.mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Hover"), base.projectile.Center);
                    }
                    //Main.NewText("Frame: " + base.projectile.frame);

                   

                    if (base.projectile.frame > 96) //stop when done
                    {
                        base.projectile.frame = 10;
                        projectile.ai[0] = 3;
                    }
                }
                {
                    if (projectile.ai[0] == 1) //this is set when a target is found
                    {

                        base.projectile.frame = 83; //animation setup
                        Main.PlaySound(base.mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Step1"), base.projectile.Center);
                        projectile.ai[0] = 2; //next phase

                    }

                }




            }
        }
    }



}