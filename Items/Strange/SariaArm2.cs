using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using SariaMod.Items;
using SariaMod.Items.zPearls;
using System;
using SariaMod.Buffs;
using Terraria;
using Terraria.ID;
using SariaMod.Items.Topaz;
using SariaMod.Dusts;
using SariaMod.Items.Strange;
using SariaMod.Items.Sapphire;
using SariaMod.Items.Emerald;
using Terraria.ModLoader;

namespace SariaMod.Items.Strange
{


    public class SariaArm2 : ModProjectile
    {


        public const float DistanceToCheck = 1100f;
        private const int sphereRadius3 = 1;
        private const int sphereRadius2 = 6;
        private const int sphereRadius4 = 42;
        private const int sphereRadius = 100;
        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Mother");
            Main.projFrames[base.projectile.type] = 99;
            Main.projPet[projectile.type] = true;
            ProjectileID.Sets.MinionSacrificable[base.projectile.type] = false;
            ProjectileID.Sets.MinionTargettingFeature[base.projectile.type] = true;
            ProjectileID.Sets.TrailingMode[base.projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[base.projectile.type] = 30;
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
            base.projectile.timeLeft = 1800;
            base.projectile.penetrate = -1;
            base.projectile.tileCollide = false;

            base.projectile.minion = true;
        }

        public override void AI()
        {

            Player player = Main.player[base.projectile.owner];
            FairyPlayer modPlayer = player.Fairy();
           
        
        



            int frameSpeed = 30; //reduced by half due to framecounter speedup
            projectile.frameCounter += 2;
            if (projectile.frameCounter >= frameSpeed)
            {
                projectile.frameCounter = 0;
                if (projectile.frame >= Main.projFrames[ModContent.ProjectileType<Saria>()]) //error here! you had the wrong projectile id, so the animation did not use the right frames
                {
                    projectile.frame = 0;

                }

                if (base.projectile.frame == 34)
                {
                    Main.PlaySound(base.mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Step2"), base.projectile.Center);
                }
                if (projectile.ai[0] == 0 || projectile.ai[0] == 3 || projectile.ai[0] == 4) //only run these animations if not attacking! no longer overrides
                {
                    if ((projectile.velocity.Y) > -3f && (projectile.velocity.Y) < 4f && Math.Abs(projectile.velocity.X) <= .5) //Idle animation, notice how I have (projectile.velocity.Y greater than -3f and less than 4f. this DID conflict with the rising and Falling animations but this is how i fixed it.
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
                        if (base.projectile.frame == 58 && player.statLife < ((player.statLifeMax2) - (player.statLifeMax2 / 4)) && !player.HasBuff(ModContent.BuffType<HealpulseBuff>()))
                        {
                            player.statLife += 500;
                            if (!player.HasBuff(ModContent.BuffType<HealpulseBuff>()))
                            {
                                Main.PlaySound(base.mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Healpulse"), player.Center);
                                for (int j = 0; j < 1; j++) //set to 2
                                {
                                    Projectile.NewProjectile(base.projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 1f, ModContent.ProjectileType<Heal>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
                                    Projectile.NewProjectile(base.projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 1f, ModContent.ProjectileType<Sad>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
                                }
                            }
                            player.AddBuff(ModContent.BuffType<HealpulseBuff>(), 8000);

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
                    if (base.projectile.frame == 90 && player.HasMinionAttackTargetNPC)
                    {
                       

                    }
                    if (base.projectile.frame == 90 && !player.HasMinionAttackTargetNPC)
                    {
                       
                    }

                    if (base.projectile.frame > 96) //stop when done
                    {
                        base.projectile.frame = 10;
                        projectile.ai[0] = 3;
                    }
                }
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