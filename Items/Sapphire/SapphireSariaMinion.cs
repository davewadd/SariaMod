using Microsoft.Xna.Framework;
using FairyMod.FaiPlayer;
using SariaMod.Items;
using FairyMod.Projectiles;
using System;
using SariaMod.Buffs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.Sapphire
{


    public class SapphireSariaMinion : ModProjectile
    {
       

        public const float DistanceToCheck = 1100f;

        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Mother");
            Main.projFrames[base.projectile.type] = 83;
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
            target.AddBuff(BuffID.Confused, 300);
            target.AddBuff(BuffID.Frostburn, 300);
            target.AddBuff(BuffID.Slow, 300);
             
             
        }
       
        public override void SetDefaults()
        {
            base.projectile.width = 96;
            base.projectile.height = 78;
            
            base.projectile.netImportant = true;
            base.projectile.friendly = true;
            
            base.projectile.ignoreWater = false;
            base.projectile.usesLocalNPCImmunity = true;
            base.projectile.localNPCHitCooldown = 5;
            base.projectile.minionSlots = 1f;
            base.projectile.timeLeft = 18000;
            base.projectile.penetrate = -1;
            base.projectile.tileCollide = false;
            base.projectile.timeLeft *= 5;
            base.projectile.minion = true;
        }
    
        public override void AI()
        {

            Player player = Main.player[base.projectile.owner];
            FairyPlayer modPlayer = player.Fairy();
            if (base.projectile.localAI[0] == 0f)
            {
                base.projectile.Fairy().spawnedPlayerMinionDamageValue = player.MinionDamage();
                base.projectile.Fairy().spawnedPlayerMinionProjectileDamageValue = base.projectile.damage;
                for (int j = 0; j < 1; j++) //set to 2
                {
                    Projectile.NewProjectile(base.projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ProjectileType<SapphirePsychicSeeker>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
                }
                base.projectile.localAI[0] = 1f;
            }

            if (player.MinionDamage() != base.projectile.Fairy().spawnedPlayerMinionDamageValue)
            {
                int trueDamage = (int)((float)base.projectile.Fairy().spawnedPlayerMinionProjectileDamageValue / base.projectile.Fairy().spawnedPlayerMinionDamageValue * player.MinionDamage());
                base.projectile.damage = trueDamage;
            }
            if (player.dead || !player.active)
            {
                player.ClearBuff(ModContent.BuffType<SapphireSariaBuff>());
                projectile.Kill();
            }
            if (player.HasBuff(ModContent.BuffType<SapphireSariaBuff>()))
            {
                projectile.timeLeft = 2;
            }
            else if (!player.HasBuff(ModContent.BuffType<SapphireSariaBuff>()))
            {
                projectile.Kill();
            }
            NPC target = base.projectile.Center.MinionHoming(500f, player);// the distance she targets enemies
            if (target != null && projectile.ai[0] == 0)
            {
                projectile.ai[0] = 1;
            }
           
            Vector2 idlePosition = player.Center;

            float nothing = 1;
          
            Vector2 targetCenter = projectile.position;
            float minionPositionOffsetX = ((60 + projectile.minionPos / 80) * player.direction)+15;
            idlePosition.Y -= 15f;
            idlePosition.X += minionPositionOffsetX;
            Vector2 vectorToIdlePosition = idlePosition - projectile.Center;
            float distanceToIdlePosition = vectorToIdlePosition.Length();
            Lighting.AddLight(projectile.Center, Color.LightBlue.ToVector3() * 0.78f);
            
            Vector2 direction = idlePosition - projectile.Center;

            if ((base.projectile.frame >= 20) && (base.projectile.frame <= 69) && (distanceToIdlePosition <= 500) && (Math.Abs(projectile.velocity.X) <= .5))
            {
                nothing = 0;
            }
            else
            {
                nothing = 1;
            }
            projectile.velocity = ((projectile.velocity * (13 - 2) + direction) / 20) * nothing;
            if (target != null)
            {
                projectile.velocity.X = player.velocity.X;
                
            }
            if (player.statLife < (player.statLifeMax2)/4 && !player.HasBuff(ModContent.BuffType<HealpulseBuff>()))
            {
                player.statLife += 500;
                player.AddBuff(ModContent.BuffType<HealpulseBuff>(), 8000);
                Main.PlaySound(base.mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Healpulse"), player.Center);
            }

            if (projectile.velocity.X >= 0.25)
                {
                    projectile.spriteDirection = 1;
                }
                if (projectile.velocity.X <= -0.25)
                {
                    projectile.spriteDirection = -1;
                }
            


            int frameSpeed = 30; //reduced by half due to framecounter speedup
            projectile.frameCounter += 2;
            if (projectile.frameCounter >= frameSpeed)
            {
                projectile.frameCounter = 0;
                if (projectile.frame >= Main.projFrames[ModContent.ProjectileType<SapphireSariaMinion>()]) //error here! you had the wrong projectile id, so the animation did not use the right frames
                {
                    projectile.frame = 0;

                }
                
                if (base.projectile.frame == 34)
                {
                    Main.PlaySound(base.mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Step2"), base.projectile.Center);
                }
                if (projectile.ai[0] == 0 || projectile.ai[0] == 3 || projectile.ai[0] == 4) //only run these animations if not attacking! no longer overrides
                {
                    if ((projectile.velocity.Y) > -3f && (projectile.velocity.Y) < 4f && Math.Abs(projectile.velocity.X) <= .5) //Idle animation, notice how I have (player.velocity.Y greater than -3f and less than 4f. this DID conflict with the rising and Falling animations but this is how i fixed it.
                    { ////however you set up the attack animation, make sure that none of these other animations override it. 
                      //that's easy legit just
                        projectile.frame++;
                        if (base.projectile.frameCounter <= 70)
                        {

                            base.projectile.frameCounter = 0;
                        }
                        if (base.projectile.frame >= 70)
                        {
                            base.projectile.frame = 0;
                            Main.PlaySound(base.mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Step1"), base.projectile.Center);
                        }
                        if (base.projectile.frame == 68 && player.statLife < ((player.statLifeMax2) - (player.statLifeMax2/4)) && !player.HasBuff(ModContent.BuffType<HealpulseBuff>()))
                        {
                            player.statLife += 500;
                            player.AddBuff(ModContent.BuffType<HealpulseBuff>(),8000);
                            Main.PlaySound(base.mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Healpulse"), player.Center);
                        }
                    }
                    if ((projectile.velocity.Y) < 4f && Math.Abs(projectile.velocity.X) > 0.5f && Math.Abs(projectile.velocity.X) < 4f) //walking animation and such
                    {
                        projectile.frame++;
                        projectile.frameCounter += 3;

                        if (base.projectile.frame <= 74)
                        {

                            base.projectile.frameCounter = 0;

                        }
                        if (base.projectile.frame >= 74)
                        {
                            base.projectile.frame = 70;
                        }
                        if (base.projectile.frame < 70)
                        {
                            base.projectile.frame = 70;
                        }

                    }

                    if ((projectile.velocity.Y) < 4f && Math.Abs(projectile.velocity.X) >= 4f)//running or (floating) animation
                    {
                        projectile.frame++;

                        if (base.projectile.frameCounter < 77)
                        {

                            base.projectile.frameCounter = 0;

                        }
                        if (base.projectile.frame >= 77)
                        {
                            base.projectile.frame = 74;
                            Main.PlaySound(base.mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Hover"), base.projectile.Center);
                        }
                        if (base.projectile.frame < 74)
                        {
                            base.projectile.frame = 74;
                        }
                    }
                    if ((projectile.velocity.Y) < -3f) //rising animation
                    {
                        projectile.frame++;

                        if (base.projectile.frameCounter < 77)
                        {

                            base.projectile.frameCounter = 0;

                        }
                        if (base.projectile.frame >= 77)
                        {
                            base.projectile.frame = 74;
                        }
                        if (base.projectile.frame < 74)
                        {
                            base.projectile.frame = 74;
                            Main.PlaySound(base.mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Fly"), base.projectile.Center);
                        }
                    }

                    if (projectile.velocity.Y > 4f) //falling animation
                    {
                        projectile.frame++;

                        if (base.projectile.frameCounter < 83)
                        {

                            base.projectile.frameCounter = 0;

                        }
                        if (base.projectile.frame >= 83)
                        {
                            base.projectile.frame = 81;
                        }
                        if (base.projectile.frame < 81)
                        {
                            base.projectile.frame = 81;
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
                    base.projectile.frameCounter += 11;
                    //Main.NewText("Frame: " + base.projectile.frame);
                    if (base.projectile.frame == 78)
                    {
                        target = base.projectile.Center.MinionHoming(500f, player);// the distance she targets enemies
                        Main.PlaySound(base.mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Blade"), base.projectile.Center);
                        if (target != null) Projectile.NewProjectile(base.projectile.Center, base.projectile.DirectionTo(target.Center) * 10f, ModContent.ProjectileType<SapphirePsychicBlade>(), base.projectile.damage, 0f, base.projectile.owner); //here is the attack that she fires when attacking enemies

                    }

                    if (base.projectile.frame > 80) //stop when done
                    {
                        base.projectile.frame = 10;
                        projectile.ai[0] = 3;
                    }
                }
                if (projectile.ai[0] == 1) //this is set when a target is found
                {

                    base.projectile.frame = 77; //animation setup
                    projectile.ai[0] = 2; //next phase

                }





            }
        }
    }



}