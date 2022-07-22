using Microsoft.Xna.Framework;

using SariaMod.Items;
using SariaMod.Items.zPearls;
using System;
using SariaMod.Buffs;
using Terraria;
using Terraria.ID;
using SariaMod.Items.Strange;
using SariaMod.Items.Topaz;
using SariaMod.Items.Diamond;
using SariaMod.Dusts;
using SariaMod.Items.Sapphire;
using Terraria.ModLoader;

namespace SariaMod.Items.Topaz
{


    public class TSSariaMinion : ModProjectile
    {
       

        public const float DistanceToCheck = 1100f;
        private const int sphereRadius3 = 1;
        private const int sphereRadius2 = 6;
        private const int sphereRadius4 = 32;
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

        public override void SetDefaults()
        {
           
            base.projectile.width = 96;
            base.projectile.height = 78;
            
            base.projectile.netImportant = true;
            base.projectile.friendly = true;
            
            base.projectile.ignoreWater = false;
            base.projectile.usesLocalNPCImmunity = true;
             base.projectile.localNPCHitCooldown = 50;
                base.projectile.minionSlots = 6f;
            base.projectile.timeLeft = 1800;
            base.projectile.penetrate = -1;
            base.projectile.tileCollide = false;
            base.projectile.timeLeft *= 5;
            base.projectile.minion = true;
        }
    
        public override void AI()
        {

            Player player = Main.player[base.projectile.owner];
            FairyPlayer modPlayer = player.Fairy();
            float sneezespot = 5;
            float dustspot = 14;
            float dustspeed = 40;
            //////////////////////////////faces start
            Vector2 idlePosition2 = player.Center;
            float minionPositionOffsetX2 = ((60 + projectile.minionPos / 80) * player.direction) - 15;
            idlePosition2.Y -= 15f;
            idlePosition2.X += minionPositionOffsetX2;
            Vector2 vectorToIdlePosition3 = idlePosition2 - projectile.Center;
            float distanceToIdlePosition3 = vectorToIdlePosition3.Length();
            if ((player.ownedProjectileCounts[ModContent.ProjectileType<SariasSong>()] >= 1f) && (player.ownedProjectileCounts[ModContent.ProjectileType<Happiness>()] <= 0f) && ((player.ownedProjectileCounts[ModContent.ProjectileType<Sad>()] > 0f) || (!player.HasBuff(ModContent.BuffType<BloodmoonBuff>()))))
            {
                player.AddBuff(ModContent.BuffType<Soothing>(), 12000);
                Projectile.NewProjectile(base.projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 1f, ModContent.ProjectileType<Happiness>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
            }
            if ((!player.HasBuff(ModContent.BuffType<Sickness>()) && (player.ownedProjectileCounts[ModContent.ProjectileType<Sad>()] >= 1f)))
            {
                if (player.ownedProjectileCounts[ModContent.ProjectileType<Happiness>()] <= 0f)
                {
                    {
                        Projectile.NewProjectile(base.projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 1f, ModContent.ProjectileType<Happiness>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
                    }
                }
            }
            if (player.statLife == player.statLifeMax2 && (projectile.frame >= 20 && projectile.frame <= 60 && projectile.ai[0] == 0 && (player.ownedProjectileCounts[ModContent.ProjectileType<SmileTime>()] <= 0f) && (!player.HasBuff(ModContent.BuffType<Sickness>()) && (!player.HasBuff(ModContent.BuffType<BloodmoonBuff>()) && (!player.HasBuff(ModContent.BuffType<StatLower>()) && (player.ownedProjectileCounts[ModContent.ProjectileType<Happiness>()] <= 0f) && (player.ownedProjectileCounts[ModContent.ProjectileType<Anger>()] <= 0f) && (player.ownedProjectileCounts[ModContent.ProjectileType<Sad>()] <= 0f) && (player.ownedProjectileCounts[ModContent.ProjectileType<Smile>()] <= 0f) && player.velocity.X == 0)) && projectile.spriteDirection != player.direction && (distanceToIdlePosition3 <= 10))))
            {
                float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius3 * sphereRadius3));
                double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                Projectile.NewProjectile(base.projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 1f, ModContent.ProjectileType<SmileTime>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
                Projectile.NewProjectile(base.projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 1f, ModContent.ProjectileType<Smile>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
                Dust.NewDust(new Vector2((projectile.Center.X + dustspeed) + radius * (float)Math.Cos(angle), (projectile.Center.Y - 10) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<HeartDust>(), 0f, 0f, 0, default(Color), 1.5f);
            }
            if ((player.ownedProjectileCounts[ModContent.ProjectileType<Smile>()] >= 1f) && (player.ownedProjectileCounts[ModContent.ProjectileType<Anger>()] <= 0f) && projectile.spriteDirection == player.direction)
            {
                Projectile.NewProjectile(base.projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 1f, ModContent.ProjectileType<Anger>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
            }
            if ((player.HasBuff(ModContent.BuffType<Sickness>())))
            {
                if (player.ownedProjectileCounts[ModContent.ProjectileType<Sad>()] <= 0f)
                {
                    {
                        Projectile.NewProjectile(base.projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 1f, ModContent.ProjectileType<Sad>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
                    }
                }
            }
            if (player.ownedProjectileCounts[ModContent.ProjectileType<Happiness>()] >= 1f)
            {
                if (Main.rand.NextBool(30))//controls the speed of when the sparkles spawn
                {
                    float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius4 * sphereRadius4));
                    double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                    if (projectile.spriteDirection > 0)
                    {
                        dustspeed = 18;
                    }
                    if (projectile.spriteDirection < 0)
                    {
                        dustspeed = 3;
                    }
                    Dust.NewDust(new Vector2((projectile.Center.X + dustspeed) + radius * (float)Math.Cos(angle), (projectile.Center.Y - 10) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<HeartDust>(), 0f, 0f, 0, default(Color), 1.5f);
                }
            }
            //////////////////////////////faces end
            if (projectile.frame == 90)
            {
                if (player.ownedProjectileCounts[ModContent.ProjectileType<Nerf>()] <= 0f)
                {
                    {
                        Projectile.NewProjectile(base.projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 1f, ModContent.ProjectileType<Nerf>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
                    }
                }
            }
            if (projectile.frame == 62 && (!player.HasBuff(ModContent.BuffType<StatLower>())) && (!player.HasBuff(ModContent.BuffType<Sickness>())))
            {
                if (Main.rand.NextBool())//controls the speed of when the sparkles spawn
                {
                    float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius3 * sphereRadius3));
                    double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                    if (projectile.spriteDirection > 0)
                    {
                        sneezespot = 18;
                    }
                    if (projectile.spriteDirection < 0)
                    {
                        sneezespot = 3;
                    }
                    Dust.NewDust(new Vector2((projectile.Center.X + sneezespot) + radius * (float)Math.Cos(angle), (projectile.Center.Y - 10) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<Sneeze>(), 0f, 0f, 0, default(Color), 1.5f);
                }
            }
            if (projectile.frame == 62 && ((player.HasBuff(ModContent.BuffType<StatLower>())) || (player.HasBuff(ModContent.BuffType<Sickness>()))))
            {
                if (Main.rand.NextBool())//controls the speed of when the sparkles spawn
                {
                    float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius3 * sphereRadius3));
                    double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                    if (projectile.spriteDirection > 0)
                    {
                        sneezespot = 18;
                    }
                    if (projectile.spriteDirection < 0)
                    {
                        sneezespot = 3;
                    }
                    Dust.NewDust(new Vector2((projectile.Center.X + sneezespot) + radius * (float)Math.Cos(angle), (projectile.Center.Y - 10) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<Blood>(), 0f, 0f, 0, default(Color), 1.5f);
                }
                if (Main.rand.NextBool(30))//controls the speed of when the sparkles spawn
                {
                    float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius3 * sphereRadius3));
                    double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                    if (projectile.spriteDirection > 0)
                    {
                        sneezespot = 18;
                    }
                    if (projectile.spriteDirection < 0)
                    {
                        sneezespot = 3;
                    }
                    Dust.NewDust(new Vector2((projectile.Center.X + sneezespot) + radius * (float)Math.Cos(angle), (projectile.Center.Y - 16) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<Blood>(), 0f, 0f, 0, default(Color), 1.5f);
                }
            }
            if ((Main.player[Main.myPlayer].active && Main.bloodMoon) && ((!player.HasBuff(ModContent.BuffType<Soothing>()))))
            {
                player.AddBuff(ModContent.BuffType<BloodmoonBuff>(), 20);
                if (Main.rand.NextBool(30))//controls the speed of when the sparkles spawn
                {
                    float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius3 * sphereRadius3));
                    double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                    if (projectile.spriteDirection > 0)
                    {
                        sneezespot = 18;
                    }
                    if (projectile.spriteDirection < 0)
                    {
                        sneezespot = 3;
                    }
                    Dust.NewDust(new Vector2((projectile.Center.X + sneezespot) + radius * (float)Math.Cos(angle), (projectile.Center.Y - 10) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<Blood>(), 0f, 0f, 0, default(Color), 1.5f);
                }
                if (Main.rand.NextBool(20))//controls the speed of when the sparkles spawn
                {
                    float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius2 * sphereRadius2));
                    double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                    if (projectile.spriteDirection > 0)
                    {
                        sneezespot = 18;
                    }
                    if (projectile.spriteDirection < 0)
                    {
                        sneezespot = 3;
                    }
                    Dust.NewDust(new Vector2((projectile.Center.X + sneezespot) + radius * (float)Math.Cos(angle), (projectile.Center.Y - 10) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<BlackSmoke>(), 0f, 0f, 0, default(Color), 1.5f);
                }
            }
            if (projectile.frame >= 76)
            {
                dustspeed = 5;
            }
            if (Main.rand.NextBool((int)dustspeed))//controls the speed of when the sparkles spawn
            {
                float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius2 * sphereRadius2));
                double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                if (projectile.spriteDirection > 0)
                {
                    dustspot = 18;
                }
                if (projectile.spriteDirection < 0)
                {
                    dustspot = 3;
                }
                Dust.NewDust(new Vector2((projectile.Center.X + dustspot) + radius * (float)Math.Cos(angle), (projectile.Center.Y + 34) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<Psychic2>(), 0f, 0f, 0, default(Color), 1.5f);
            }//end of dust stuff
            if (base.projectile.localAI[0] == 0f)
            {
                base.projectile.Fairy().spawnedPlayerMinionDamageValue = player.MinionDamage();
                base.projectile.Fairy().spawnedPlayerMinionProjectileDamageValue = base.projectile.damage;
                for (int j = 0; j < 1; j++) //set to 2
                {
                    Projectile.NewProjectile(base.projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 1f, ModContent.ProjectileType<Ztarget>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
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
                player.ClearBuff(ModContent.BuffType<TopazSariaBuff>());
                projectile.Kill();
            }
            if (player.HasBuff(ModContent.BuffType<TopazSariaBuff>()))
            {
                projectile.timeLeft = 2;
            }
            if (!player.HasBuff(ModContent.BuffType<TopazSariaBuff>()))
            {
                projectile.Kill();
            }
            if ((player.HasBuff(ModContent.BuffType<TopazSariaBuff>()) && (player.ownedProjectileCounts[ModContent.ProjectileType<TRSariaMinion>()] > 0f)))
            {
                projectile.Kill();
            }
            NPC target = base.projectile.Center.MinionHoming(500f, player);// the distance she targets enemies
            if (target != null && projectile.ai[0] == 0 && ((player.ownedProjectileCounts[ModContent.ProjectileType<Bubble>()] < 1f)))
            {
                projectile.ai[0] = 1;
            }
            //Statraise and lower
            if ((Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneDesert || Main.player[Main.myPlayer].ZoneSnow || Main.player[Main.myPlayer].ZoneJungle) || Main.player[Main.myPlayer].ZoneGlowshroom)
            {
                if (!player.HasBuff(ModContent.BuffType<StatLower>()))
                {
                    Main.PlaySound(base.mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/StatLower"), projectile.Center);
                    for (int j = 0; j < 1; j++) //set to 2
                    {
                        Projectile.NewProjectile(base.projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 1f, ModContent.ProjectileType<PowerDown>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
                    }
                    player.AddBuff(ModContent.BuffType<StatLower>(), 20);
                }
                if (player.HasBuff(ModContent.BuffType<StatLower>()))
                {
                    player.AddBuff(ModContent.BuffType<StatLower>(), 20);
                }
            }
           if ((Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneUnderworldHeight || Main.player[Main.myPlayer].ZoneRain || Main.player[Main.myPlayer].ZoneBeach || Main.player[Main.myPlayer].ZoneMeteor || Main.player[Main.myPlayer].ZoneWaterCandle) && (!player.HasBuff(ModContent.BuffType<StatLower>())))
            {
                if (!player.HasBuff(ModContent.BuffType<StatRaise>()))
                {
                    Main.PlaySound(base.mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/StatRaise"), projectile.Center);
                    for (int j = 0; j < 1; j++) //set to 2
                    {
                        Projectile.NewProjectile(base.projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 1f, ModContent.ProjectileType<PowerUp>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
                    }
                    player.AddBuff(ModContent.BuffType<StatRaise>(), 20);
                }
                if (player.HasBuff(ModContent.BuffType<StatRaise>()))
                {
                    player.AddBuff(ModContent.BuffType<StatRaise>(), 20);
                }
            }

            Vector2 idlePosition = player.Center;

            float nothing = 1;
            float speed = 2;
            
            

            bool foundTarget = false;

            float minionPositionOffsetX = ((60 + projectile.minionPos / 80) * player.direction)-15;
            idlePosition.Y -= 15f;
            idlePosition.X += minionPositionOffsetX;
            Vector2 vectorToIdlePosition = idlePosition - projectile.Center;
            
            float distanceToIdlePosition = vectorToIdlePosition.Length();
           
            Lighting.AddLight(projectile.Center, Color.LightBlue.ToVector3() * 0.78f);
            
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
           
            if (player.statLife < (player.statLifeMax2)/4 && !player.HasBuff(ModContent.BuffType<HealpulseBuff>()))
            {
                player.statLife += 500;
                player.AddBuff(ModContent.BuffType<HealpulseBuff>(), 8000);
                Main.PlaySound(base.mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Healpulse"), player.Center);
                for (int j = 0; j < 1; j++) //set to 2
                {
                    Projectile.NewProjectile(base.projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 1f, ModContent.ProjectileType<Heal>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
                    Projectile.NewProjectile(base.projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 1f, ModContent.ProjectileType<Sad>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
                }
            }
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
                        if (base.projectile.frame == 58 && player.statLife < ((player.statLifeMax2) - (player.statLifeMax2 / 4)) && !player.HasBuff(ModContent.BuffType<HealpulseBuff>()))
                        {

                            player.statLife += 500;
                            player.AddBuff(ModContent.BuffType<HealpulseBuff>(), 8000);
                            Main.PlaySound(base.mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Healpulse"), player.Center);
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
                  
                    if (base.projectile.frame == 90)
                    {
                        target = base.projectile.Center.MinionHoming(500f, player);// the distance she targets enemies
                        Main.PlaySound(SoundID.Item77, base.projectile.Center);
                        for (int j = 0; j < 10; j++) //set to 2
                        {
                            Projectile.NewProjectile(base.projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ProjectileType<Bubble>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
                        }
                    }

                    if (base.projectile.frame > 96) //stop when done
                    {
                        base.projectile.frame = 10;
                        projectile.ai[0] = 3;
                    }
                }
                {
                    if (projectile.ai[0] == 1 && (player.ownedProjectileCounts[ModContent.ProjectileType<Bubble>()] < 1f)) //this is set when a target is found
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