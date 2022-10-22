using Microsoft.Xna.Framework;

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
using SariaMod.Items.Amber;
using Terraria.ModLoader;

namespace SariaMod.Items.Diamond
{


    public class DASariaMinion : ModProjectile
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
            target.AddBuff(BuffID.Venom, 300);
            target.AddBuff(BuffID.Poisoned, 300);
            
            if (player.HasBuff(ModContent.BuffType<StatRaise>()))
            {
                damage = damage;
            }
           else  if (player.HasBuff(ModContent.BuffType<StatLower>()))
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
                base.projectile.minionSlots = 0f;
            base.projectile.timeLeft = 1800;
            base.projectile.penetrate = -1;
            base.projectile.tileCollide = false;
            
            base.projectile.minion = true;
            projectile.minionPos = 1;
        }
    
        public override void AI()
        {

            Player player = Main.player[base.projectile.owner];
            FairyPlayer modPlayer = player.Fairy();
            float sneezespot = 5;
            float dustspot = 14;
            float dustspeed = 40;
            if (projectile.timeLeft < 1799)
            {
                base.projectile.minionSlots = 11f;
            }
            //////////////////////////////faces start
            Vector2 idlePosition2 = player.Center;
            float minionPositionOffsetX2 = ((60 + projectile.minionPos / 80) * player.direction) - 15;
            idlePosition2.Y -= 15f;
            idlePosition2.X += minionPositionOffsetX2;
            Vector2 vectorToIdlePosition3 = idlePosition2 - projectile.Center;
            float distanceToIdlePosition3 = vectorToIdlePosition3.Length();
            if ((player.ownedProjectileCounts[ModContent.ProjectileType<FrozenYogurtSignal>()] >= 1f) && (player.ownedProjectileCounts[ModContent.ProjectileType<Happiness>()] <= 0f) && ((player.ownedProjectileCounts[ModContent.ProjectileType<Sad>()] > 0f) || ((!player.HasBuff(ModContent.BuffType<BloodmoonBuff>())) || (!player.HasBuff(ModContent.BuffType<EclipseBuff>())))))
            {
                 
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
            if (player.statLife == player.statLifeMax2 && (projectile.frame >= 20 && projectile.frame <= 60 && projectile.ai[0] == 0 && (player.ownedProjectileCounts[ModContent.ProjectileType<SmileTime>()] <= 0f) && (!player.HasBuff(ModContent.BuffType<Sickness>()) && (!player.HasBuff(ModContent.BuffType<BloodmoonBuff>()) && !player.HasBuff(ModContent.BuffType<EclipseBuff>()) && (!player.HasBuff(ModContent.BuffType<StatLower>()) && (player.ownedProjectileCounts[ModContent.ProjectileType<Happiness>()] <= 0f) && (player.ownedProjectileCounts[ModContent.ProjectileType<Anger>()] <= 0f) && (player.ownedProjectileCounts[ModContent.ProjectileType<Sad>()] <= 0f) && (player.ownedProjectileCounts[ModContent.ProjectileType<Smile>()] <= 0f) && player.velocity.X == 0)) && projectile.spriteDirection != player.direction && (distanceToIdlePosition3 <= 10))))
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
            if (player.ownedProjectileCounts[ModContent.ProjectileType<Competitivetime>()] == 1f && player.ownedProjectileCounts[ModContent.ProjectileType<Competitive>()] <= 0f)
            {
                {
                    Main.PlaySound(base.mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/StatRaise"), projectile.Center);
                    for (int j = 0; j < 1; j++) //set to 2
                    {
                        Projectile.NewProjectile(base.projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 1f, ModContent.ProjectileType<PowerUp>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
                    }
                    Projectile.NewProjectile(base.projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 1f, ModContent.ProjectileType<Competitive>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
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
            if (projectile.frame == 62 && (!player.HasBuff(ModContent.BuffType<StatLower>())) && (!player.HasBuff(ModContent.BuffType<Sickness>()) && (!player.HasBuff(ModContent.BuffType<BloodmoonBuff>())) && (!player.HasBuff(ModContent.BuffType<EclipseBuff>()))))
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
            if (projectile.frame == 62 && ((player.HasBuff(ModContent.BuffType<StatLower>())) || (player.HasBuff(ModContent.BuffType<Sickness>()) || (player.HasBuff(ModContent.BuffType<BloodmoonBuff>()) || (player.HasBuff(ModContent.BuffType<EclipseBuff>()))))))
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
            }
            if (((Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneSnow) && !(Main.player[Main.myPlayer].behindBackWall && player.HasBuff((BuffID.Campfire)))) || (Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneSkyHeight) && !(Main.player[Main.myPlayer].behindBackWall && player.HasBuff((BuffID.Campfire))) || (Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneDesert && !Main.dayTime) && !(Main.player[Main.myPlayer].behindBackWall && player.HasBuff((BuffID.Campfire))) || (Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneRain && !Main.player[Main.myPlayer].ZoneJungle && !(Main.player[Main.myPlayer].ZoneDesert && Main.dayTime)) && !(Main.player[Main.myPlayer].behindBackWall && player.HasBuff((BuffID.Campfire))))
            {
                if (projectile.velocity.X <= 1)
                {
                    if (Main.rand.NextBool(50))
                    {
                        float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius3 * sphereRadius3));
                        double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                        if (projectile.spriteDirection > 0)
                        {
                            sneezespot = 25;
                        }
                        if (projectile.spriteDirection < 0)
                        {
                            sneezespot = -2;
                        }
                        for (int j = 0; j < 2; j++)
                        {
                            Dust.NewDust(new Vector2((projectile.Center.X + sneezespot) + radius * (float)Math.Cos(angle), (projectile.Center.Y - 10) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<Fog>(), 0f, 0f, 0, default(Color), 1.5f);

                        }
                    }
                }
                else if (projectile.velocity.X > 1)
                {
                    if (Main.rand.NextBool(10))
                    {
                        float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius3 * sphereRadius3));
                        double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                        if (projectile.spriteDirection > 0)
                        {
                            sneezespot = 25;
                        }
                        if (projectile.spriteDirection < 0)
                        {
                            sneezespot = -2;
                        }
                        for (int j = 0; j < 2; j++)
                        {
                            Dust.NewDust(new Vector2((projectile.Center.X + sneezespot) + radius * (float)Math.Cos(angle), (projectile.Center.Y - 10) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<Fog>(), 0f, 0f, 0, default(Color), 1.5f);

                        }
                    }
                }

            }//end of dust stuff
            if (base.projectile.localAI[0] == 0f)
            {
                base.projectile.Fairy().spawnedPlayerMinionDamageValue = player.MinionDamage();
                base.projectile.Fairy().spawnedPlayerMinionProjectileDamageValue = base.projectile.damage;
                for (int j = 0; j < 1; j++) //set to 2
                {
                    Projectile.NewProjectile(base.projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 1f, ModContent.ProjectileType<ZtargetD7>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
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
                player.ClearBuff(ModContent.BuffType<DiamondSariaBuff>());
                projectile.Kill();
            }
            if (player.HasBuff(ModContent.BuffType<DiamondSariaBuff>()) && projectile.timeLeft <= 10)
            {
                projectile.timeLeft = 500;
            }
            if (!player.HasBuff(ModContent.BuffType<DiamondSariaBuff>()))
            {
                projectile.Kill();
            }
            if ((player.HasBuff(ModContent.BuffType<DiamondSariaBuff>()) && (player.ownedProjectileCounts[ModContent.ProjectileType<DAMSariaMinion>()] > 0f)))
            {
                projectile.Kill();
            }
            NPC target = base.projectile.Center.MinionHoming(500f, player);// the distance she targets enemies
            if (target != null && projectile.ai[0] == 0 && ((player.ownedProjectileCounts[ModContent.ProjectileType<AmberRed>()] < 1f)) && ((player.ownedProjectileCounts[ModContent.ProjectileType<AmberGreen>()] < 1f)) && ((player.ownedProjectileCounts[ModContent.ProjectileType<AmberPurple>()] < 1f)) && ((player.ownedProjectileCounts[ModContent.ProjectileType<AmberBlack1>()] < 1f)) && ((player.ownedProjectileCounts[ModContent.ProjectileType<AmberBlack2>()] < 1f)) && ((player.ownedProjectileCounts[ModContent.ProjectileType<BlackMoth>()] < 2f)))
            {
                projectile.ai[0] = 1;
            }
            //Flashupdate stuff
            for (int i = 0; i < 1000; i++)
            {

                float between = Vector2.Distance(Main.projectile[i].Center, player.Center);
                if (between <= 100)
                {


                    if (Main.projectile[i].active && i != base.projectile.whoAmI && ((!Main.projectile[i].friendly && Main.projectile[i].hostile) || (Main.projectile[i].trap)))
                    {
                        if ((!player.HasBuff(ModContent.BuffType<Sickness>()) && (!player.HasBuff(ModContent.BuffType<BloodmoonBuff>()) && (!player.HasBuff(ModContent.BuffType<EclipseBuff>()) && (player.ownedProjectileCounts[ModContent.ProjectileType<Sad>()] <= 0f) && (player.ownedProjectileCounts[ModContent.ProjectileType<Flash>()] <= 0f) && (player.ownedProjectileCounts[ModContent.ProjectileType<FlashCooldown>()] <= 0f)))))
                        {

                            Projectile.NewProjectile(base.projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 1f, ModContent.ProjectileType<Flash>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
                            Main.PlaySound(SoundID.Item76, base.projectile.Center);

                        }
                    }
                }
            }
            if (player.ownedProjectileCounts[ModContent.ProjectileType<Flash>()] > 0f)
            {
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
                    Dust.NewDust(new Vector2((projectile.Center.X + sneezespot) + radius * (float)Math.Cos(angle), (projectile.Center.Y - 10) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<Psychic>(), 0f, 0f, 0, default(Color), 1.5f);
                }
            }
            if ((Main.player[Main.myPlayer].active && Main.eclipse) && ((!player.HasBuff(ModContent.BuffType<Soothing>()))))
            {
                player.AddBuff(ModContent.BuffType<EclipseBuff>(), 20);
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
                if (player.ownedProjectileCounts[ModContent.ProjectileType<period>()] <= 0f)
                {
                    Projectile.NewProjectile(base.projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 1f, ModContent.ProjectileType<Anger>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
                    Projectile.NewProjectile(base.projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 1f, ModContent.ProjectileType<period>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
                }
            }
            if (player.HasBuff(ModContent.BuffType<BloodmoonBuff>()) && (player.ownedProjectileCounts[ModContent.ProjectileType<period>()] <= 0f))

            {
                Projectile.NewProjectile(base.projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 1f, ModContent.ProjectileType<Anger>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
                Projectile.NewProjectile(base.projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 1f, ModContent.ProjectileType<period>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
            }
            if (player.HasBuff(ModContent.BuffType<StatLower>()) && (player.ownedProjectileCounts[ModContent.ProjectileType<period>()] <= 0f))

            {
                Projectile.NewProjectile(base.projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 1f, ModContent.ProjectileType<Sad2>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
                Projectile.NewProjectile(base.projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 1f, ModContent.ProjectileType<period>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
            }


            //end of Flashupdate stuff
            //Statraise and lower
            if ((Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneSkyHeight || Main.player[Main.myPlayer].ZoneBeach))
            {
                if (!player.HasBuff(ModContent.BuffType<StatRaise>()) && !player.HasBuff(ModContent.BuffType<StatLower>()))
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
            if ((Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneUndergroundDesert || Main.player[Main.myPlayer].ZoneUnderworldHeight || Main.player[Main.myPlayer].ZoneRockLayerHeight))
            {
                if (!player.HasBuff(ModContent.BuffType<StatRaise>()) && !player.HasBuff(ModContent.BuffType<StatLower>()))
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
                if (projectile.frame >= Main.projFrames[ModContent.ProjectileType<TopazSariaMinion>()]) //error here! you had the wrong projectile id, so the animation did not use the right frames
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

                    if (base.projectile.frame == 90)
                    {
                        if (player.ownedProjectileCounts[ModContent.ProjectileType<GreenPoint>()] > 0f)
                        {
                            Main.PlaySound(SoundID.Item77, base.projectile.Center);
                            target = base.projectile.Center.MinionHoming(500f, player);// the distance she targets enemies
                            Main.PlaySound(SoundID.Item77, base.projectile.Center);
                            Projectile.NewProjectile(base.projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ProjectileType<AmberGreen>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);

                        }
                        else if (player.ownedProjectileCounts[ModContent.ProjectileType<GreenPoint>()] <= 0f)
                        {
                            if ((projectile.timeLeft > 100 && projectile.timeLeft < 2000 && (player.ownedProjectileCounts[ModContent.ProjectileType<GreenMoth>()] <= 0f) && (player.ownedProjectileCounts[ModContent.ProjectileType<AmberGreen>()] <= 0f) && (player.ownedProjectileCounts[ModContent.ProjectileType<GreenMothGiant>()] <= 0f) && (player.ownedProjectileCounts[ModContent.ProjectileType<GreenMothGoliath>()] <= 0f) && ((player.ownedProjectileCounts[ModContent.ProjectileType<RedMoth>()] == 1f) || (player.ownedProjectileCounts[ModContent.ProjectileType<RedMothGiant>()] == 1f)) && ((player.ownedProjectileCounts[ModContent.ProjectileType<PurpleMoth>()] == 1f) || (player.ownedProjectileCounts[ModContent.ProjectileType<PurpleMothGiant>()] == 1f))))
                            {
                                {
                                    Main.PlaySound(SoundID.Item77, base.projectile.Center);
                                    target = base.projectile.Center.MinionHoming(500f, player);// the distance she targets enemies
                                    Main.PlaySound(SoundID.Item77, base.projectile.Center);
                                    Projectile.NewProjectile(base.projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ProjectileType<AmberGreen>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);

                                }
                            }
                            else
                            {
                                if ((projectile.timeLeft >= 50 && projectile.timeLeft <= 450) && (player.ownedProjectileCounts[ModContent.ProjectileType<PurpleMoth>()] <= 0f) && (player.ownedProjectileCounts[ModContent.ProjectileType<PurpleMothGiant>()] <= 0f) && ((player.ownedProjectileCounts[ModContent.ProjectileType<RedMoth>()] <= 0f) && (player.ownedProjectileCounts[ModContent.ProjectileType<RedMothGiant>()] <= 0f)))
                                {

                                    Projectile.NewProjectile(base.projectile.Center + new Vector2(-250f, 370f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ProjectileType<AmberRed>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);

                                }
                                if ((projectile.timeLeft >= 50 && projectile.timeLeft <= 250) && ((player.ownedProjectileCounts[ModContent.ProjectileType<RedMoth>()] == 1f) || (player.ownedProjectileCounts[ModContent.ProjectileType<RedMothGiant>()] == 1f)) && ((player.ownedProjectileCounts[ModContent.ProjectileType<PurpleMoth>()] <= 0f) && (player.ownedProjectileCounts[ModContent.ProjectileType<PurpleMothGiant>()] <= 0f)))
                                {
                                    Projectile.NewProjectile(base.projectile.Center + new Vector2(250f, 370f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ProjectileType<AmberPurple>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);

                                }
                                target = base.projectile.Center.MinionHoming(500f, player);// the distance she targets enemies
                                Main.PlaySound(SoundID.Item77, base.projectile.Center);
                                for (int j = 0; j < 1; j++) //set to 2
                                {
                                    Projectile.NewProjectile(base.projectile.Center + new Vector2(-500f, 370f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ProjectileType<AmberBlack1>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
                                    Projectile.NewProjectile(base.projectile.Center + new Vector2(500f, 370f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ProjectileType<AmberBlack2>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
                                }
                            }
                        }
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