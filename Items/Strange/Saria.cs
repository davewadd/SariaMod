using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SariaMod.Items.Topaz;
using SariaMod.Items.Emerald;
using SariaMod.Items.Amber;
using SariaMod.Items.Amethyst;
using SariaMod.Items.Diamond;
using SariaMod.Items.Ruby;
using SariaMod.Items;
using SariaMod.Items.zPearls;
using System;
using SariaMod.Items.Bands;
using SariaMod.Buffs;
using SariaMod.Dusts;
using Terraria;
using Terraria.ID;
using SariaMod.Items.Strange;
using SariaMod.Items.Sapphire;
using Terraria.ModLoader;

namespace SariaMod.Items.Strange
{


    public class Saria : ModProjectile
    {
       

        public const float DistanceToCheck = 1100f;
        private static int Transform;
        private static int GemTimer;
        private static int BugTimer;
        private static int SwarmTimer;
        private static int SicknessTimer;
        private static int Mood;
        private static int MoodTimer;
        private static int MoveTimer;
        private static int SleepHeal;
        private static int Heal;
        private static int Sleep;
        private static int TimeAsleep;
        private static int XpTimer;
        private static int Cursed;
        
        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Mother");
            Main.projFrames[base.projectile.type] = 99;
            Main.projPet[projectile.type] = true;
             ProjectileID.Sets.MinionSacrificable[base.projectile.type] = false;
            ProjectileID.Sets.MinionShot[base.projectile.type] = false;
            ProjectileID.Sets.MinionTargettingFeature[base.projectile.type] = true;
            ProjectileID.Sets.TrailingMode[base.projectile.type] = 0;
            ProjectileID.Sets.TrailCacheLength[base.projectile.type] = 30;
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
            if (target != null && TimeAsleep <= 10)
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
            float Timer = 40;
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
            modPlayer.SariaXp+= 2;
            if (Transform == 0)
            {
                target.AddBuff(ModContent.BuffType<SariaCurse2>(), 200);
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
            else if (Transform == 1)
            {
                Heal++;
                target.buffImmune[ModContent.BuffType<Frostburn2>()] = false;
                target.AddBuff(ModContent.BuffType<Frostburn2>(), 200);
                if (Heal>= Timer)
                {
                    Projectile.NewProjectile(base.projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ProjectileType<HealBubble>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
                }
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
            else if (Transform == 2)
            {
                target.buffImmune[ModContent.BuffType<Burning2>()] = false;
                target.AddBuff(ModContent.BuffType<Burning2>(), 200);
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
            else if (Transform == 3)
            {
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
            else if (Transform == 4)
            {
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
            else if (Transform == 5)
            {
                target.AddBuff(BuffID.Venom, 300);
                target.AddBuff(BuffID.Poisoned, 300);

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
            else if (Transform == 6)
            {
                target.buffImmune[ModContent.BuffType<SariaCurse>()] = false;
                target.AddBuff(ModContent.BuffType<SariaCurse>(), 2000);
                if (!player.HasBuff(ModContent.BuffType<StatLower>()))
                {
                    Projectile.NewProjectile(target.Center + new Vector2(10f, 2f), Vector2.One.RotatedByRandom(6.2831854820251465) * 0f, ModContent.ProjectileType<ShadowClaw>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
                }
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
            
            base.projectile.minion = false;
        }
        private const int sphereRadius3 = 1;
        private const int sphereRadius2 = 6;
        private const int sphereRadius4 = 32;
        private const int sphereRadius = 100;

        public override void AI()
        {

            Player player = Main.player[base.projectile.owner];
            FairyPlayer modPlayer = player.Fairy();
           
            
            int owner = player.whoAmI;
            int GiantMoth = ModContent.ProjectileType<DuskBallProjectile>();
            for (int i = 0; i < 1000; i++)
            {
                if ((player.ownedProjectileCounts[ModContent.ProjectileType<GreenMothGoliath>()] <= 0f) && (player.ownedProjectileCounts[ModContent.ProjectileType<GreenMothGiant>()] <= 0f) && (player.ownedProjectileCounts[ModContent.ProjectileType<GreenMoth>()] <= 0f))
                {
                    if (Main.projectile[i].active && i != base.projectile.whoAmI && ((Main.projectile[i].type == GiantMoth && Main.projectile[i].owner == owner && Main.projectile[i].timeLeft == 10)))
                    {

                        {
                            Main.PlaySound(base.mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Pokeball"), Main.projectile[i].Center);
                            Projectile.NewProjectile(Main.projectile[i].Center + Utils.NextVector2CircularEdge(Main.rand, 8f, 8f), Utils.NextVector2Circular(Main.rand, 12f, 12f), ModContent.ProjectileType<GreenMothGoliath2>(), projectile.damage, projectile.knockBack, player.whoAmI);
                        }

                    }

                }
            }
            int XpProjectile = ModContent.ProjectileType<XpProjectile>();
            for (int i = 0; i < 1000; i++)
            {
                if (Main.projectile[i].active && i != base.projectile.whoAmI && ((Main.projectile[i].type == XpProjectile && Main.projectile[i].owner == owner)))
                {

                    {
                        modPlayer.SariaXp += 100;
                        Main.projectile[i].Kill();
                    }

                }

            }
            int XpProjectile2 = ModContent.ProjectileType<XpProjectile2>();
            for (int i = 0; i < 1000; i++)
            {
                if (Main.projectile[i].active && i != base.projectile.whoAmI && ((Main.projectile[i].type == XpProjectile2 && Main.projectile[i].owner == owner)))
                {

                    {
                        modPlayer.SariaXp += 500;
                        Main.projectile[i].Kill();
                    }

                }

            }
            int XpProjectile3 = ModContent.ProjectileType<XpProjectile3>();
            for (int i = 0; i < 1000; i++)
            {
                if (Main.projectile[i].active && i != base.projectile.whoAmI && ((Main.projectile[i].type == XpProjectile3 && Main.projectile[i].owner == owner)))
                {

                    {
                        modPlayer.SariaXp += 2500;
                        Main.projectile[i].Kill();
                    }

                }

            }
            int XpProjectile4 = ModContent.ProjectileType<XpProjectile4>();
            for (int i = 0; i < 1000; i++)
            {
                if (Main.projectile[i].active && i != base.projectile.whoAmI && ((Main.projectile[i].type == XpProjectile4 && Main.projectile[i].owner == owner)))
                {

                    {
                        modPlayer.SariaXp += 12500;
                        Main.projectile[i].Kill();
                    }

                }

            }
            int Lightning = ModContent.ProjectileType<LightningLocator>();
            for (int i = 0; i < 1000; i++)
            {
                if (Transform != 3)
                { 
                    if (Main.projectile[i].active && i != base.projectile.whoAmI && ((Main.projectile[i].type == Lightning && Main.projectile[i].owner == owner)))
                    {

                        {
                            Main.projectile[i].Kill();
                        }

                    }

                }
            }
            //////////////Transformation Timer
            ///
            int Timer = 3;

            
            if (modPlayer.Sarialevel == 6)
            {
                projectile.damage = 900 + (modPlayer.SariaXp / 40);
            }
            else if (modPlayer.Sarialevel == 5)
            {
                projectile.damage = 200 + (modPlayer.SariaXp / 342);
            }
            else if (modPlayer.Sarialevel == 4)
            {
                projectile.damage = 75 + (modPlayer.SariaXp / 640);
            }
            else if (modPlayer.Sarialevel == 3)
            {
                projectile.damage = 50 + (modPlayer.SariaXp / 1600);
            }
            else if (modPlayer.Sarialevel == 2)
            {
                projectile.damage = 26 + (modPlayer.SariaXp / 833);
            }

            else if (modPlayer.Sarialevel == 1)
            {
                projectile.damage = 15 + (modPlayer.SariaXp / 818);
            }
            else
            {
                projectile.damage = 10 + (modPlayer.SariaXp/600);
            }
            
                if (player.HasBuff(ModContent.BuffType<XPBuff>()))
                {
                XpTimer = 2;
               
            }
            if (player.ownedProjectileCounts[ModContent.ProjectileType<Transform>()] > 0f)
            { 
                Transform++;
                
               
                int VeilBubble = ModContent.ProjectileType<Transform>();
                for (int i = 0; i < 1000; i++)
                {
                    if (Main.projectile[i].active && i != base.projectile.whoAmI && ((Main.projectile[i].type == VeilBubble && Main.projectile[i].owner == owner)))
                    {

                        {
                            Main.projectile[i].Kill();
                        }

                    }

                }
                if (modPlayer.Sarialevel == 0 && Transform >= 1)
                {
                    Transform = 0;
                }
                if (modPlayer.Sarialevel == 1 && Transform >= 2)
                {
                    Transform = 0;
                }
                if (modPlayer.Sarialevel == 2 && Transform >= 3)
                {
                    Transform = 0;
                }
                if (modPlayer.Sarialevel == 3 && Transform >= 4)
                {
                    Transform = 0;
                }
                if (modPlayer.Sarialevel == 4 && Transform >= 5)
                {
                    Transform = 0;
                }
                if (modPlayer.Sarialevel == 5 && Transform >= 6)
                {
                    Transform = 0;
                }
                if (modPlayer.Sarialevel == 6 && Transform >= 7)
                {
                    Transform = 0;
                }
                if (Transform > 8)
                {
                    Transform = 0;
                }
            }
            GemTimer--;
            {
                if (GemTimer <= 0)
                {
                    GemTimer = 500;
                }
            }
            BugTimer--;
            {
                if (BugTimer <= 0)
                {
                    BugTimer = 500;
                }
            }
            if (SwarmTimer < 1200)
            {
                SwarmTimer++;
            }
            if (XpTimer >= 1)
            {
                XpTimer--;
            }
            SicknessTimer++;
            {
                if (SicknessTimer >= 180000 && !player.HasBuff(ModContent.BuffType<Soothing>()))
                {
                    player.AddBuff(ModContent.BuffType<Sickness>(), 30000);
                    SicknessTimer = 0;
                }
                if (player.HasBuff(ModContent.BuffType<Soothing>()))
                {
                    SicknessTimer = 0;
                }
            }
            if (Mood < 0 && MoodTimer >= Timer)
            {
                Mood++;
            }
            if (Mood > 0 && MoodTimer >= Timer)
            {
                Mood--;
            }
            if ((Math.Abs(projectile.velocity.X) >= 0.5f) || (Math.Abs(projectile.velocity.Y) >= 0.5f))
            {
                MoveTimer = 0;
            }
            if ((Math.Abs(projectile.velocity.X) < 0.5f) && (Math.Abs(projectile.velocity.Y) < 0.5f))
            {
                if (MoveTimer < 2500)
                {
                    MoveTimer++;
                }
                if (Mood <= -1200)
                {
                    MoveTimer++;
                }
            }
            if (MoveTimer == 0)
            {
                TimeAsleep = 0;
                SleepHeal = 0;
            }
            if (TimeAsleep >= 200 && SleepHeal<= 0)
            {
                Main.PlaySound(base.mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Healpulse"), player.Center);
                player.AddBuff(ModContent.BuffType<Soothing>(), 44000);
                Mood = 0;
                SleepHeal = 1;
                if (player.HasBuff(ModContent.BuffType<Drained>()))
                {
                    player.ClearBuff(ModContent.BuffType<Drained>());
                }
                

            }
            if (player.HasBuff(ModContent.BuffType<BloodmoonBuff>()) || player.HasBuff(ModContent.BuffType<EclipseBuff>()))
            {
                Cursed = 1;
            }
            else
            {
                Cursed = 0;
            }
            if (TimeAsleep >= 500)
            {
                if (!player.HasBuff(ModContent.BuffType<Overcharged>()))
                {
                    player.AddBuff(ModContent.BuffType<Overcharged>(), 30000);
                    Main.PlaySound(base.mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/StatRaise"), projectile.Center);
                    for (int j = 0; j < 1; j++) //set to 2
                    {
                        Projectile.NewProjectile(base.projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 1f, ModContent.ProjectileType<PowerUp>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
                    }
                }
                Mood = 0;
                MoveTimer = 0;
               
            }
            
                if (Mood >= 1200)
            {
                SicknessTimer--;
            }
            if (Mood >= 2400)
            {
                SicknessTimer--;
            }
            if (Mood <= -1200)
            {
                SicknessTimer++;
            }
            if (Mood <= -2400)
            {
                SicknessTimer++;
            }
            
            /////////////// End of Transformation Timer
            ///


            
            {
                
               
            }
            float sneezespot = 5;
            float dustspot = 14;
            float dustspeed = 40;
            if (Transform == 2)
            {
                if (Main.rand.NextBool(30))//controls the speed of when the sparkles spawn
                {
                    float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius * sphereRadius));
                    double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                    Dust.NewDust(new Vector2(projectile.Center.X + radius * (float)Math.Cos(angle), (projectile.Center.Y - 10) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<FlameDustSaria>(), 0f, 0f, 0, default(Color), 1.5f);
                }
            }
            if (Transform == 6)
            {
                if (Main.rand.NextBool(30))//controls the speed of when the sparkles spawn
                {
                    float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius * sphereRadius));
                    double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                    Dust.NewDust(new Vector2(projectile.Center.X + radius * (float)Math.Cos(angle), (projectile.Center.Y - 10) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<ShadowFlameDust>(), 0f, 0f, 0, default(Color), 1.5f);
                }
            }
            //////////////////////////////faces start
            Vector2 idlePosition2 = player.Center;
            float minionPositionOffsetX2 = ((60 + projectile.minionPos / 80) * player.direction) - 15;
            idlePosition2.Y -= 15f;
            idlePosition2.X += minionPositionOffsetX2;
            Vector2 vectorToIdlePosition3 = idlePosition2 - projectile.Center;
            float distanceToIdlePosition3 = vectorToIdlePosition3.Length();
            if ((player.ownedProjectileCounts[ModContent.ProjectileType<FrozenYogurtSignal>()] >= 1f) && (player.ownedProjectileCounts[ModContent.ProjectileType<Happiness>()] <= 0f) && ((player.ownedProjectileCounts[ModContent.ProjectileType<Sad>()] > 0f) || (!player.HasBuff(ModContent.BuffType<BloodmoonBuff>()))))
            {
                Mood += 600;
                Projectile.NewProjectile(base.projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 1f, ModContent.ProjectileType<Happiness>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
            }
            
            if (Sleep <= 0 && player.statLife == player.statLifeMax2 && (projectile.frame >= 20 && projectile.frame <= 60 && projectile.ai[0] == 0 && (player.ownedProjectileCounts[ModContent.ProjectileType<SmileTime>()] <= 0f) && (!player.HasBuff(ModContent.BuffType<Sickness>()) && (!player.HasBuff(ModContent.BuffType<BloodmoonBuff>()) && (!player.HasBuff(ModContent.BuffType<StatLower>()) && (player.ownedProjectileCounts[ModContent.ProjectileType<Happiness>()] <= 0f) && (player.ownedProjectileCounts[ModContent.ProjectileType<Anger>()] <= 0f) && (player.ownedProjectileCounts[ModContent.ProjectileType<Sad>()] <= 0f) && (player.ownedProjectileCounts[ModContent.ProjectileType<Smile>()] <= 0f) && player.velocity.X == 0)) && projectile.spriteDirection != player.direction && (distanceToIdlePosition3 <= 10))))
            {
                float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius3 * sphereRadius3));
                double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                Mood += 600;
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
                SicknessTimer = 0;
                Mood = -4800;
                if (player.ownedProjectileCounts[ModContent.ProjectileType<Sad>()] <= 0f && (player.ownedProjectileCounts[ModContent.ProjectileType<period>()] <= 0f))
                {
                    {
                        Projectile.NewProjectile(base.projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 1f, ModContent.ProjectileType<Sad>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
                        Projectile.NewProjectile(base.projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 1f, ModContent.ProjectileType<period>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
                    }
                }
            }
            if ((player.HasBuff(ModContent.BuffType<Soothing>())))
            {
                if (Mood <= 200)
                {
                    Mood++;
                    Mood++;
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
                    Mood = 3700;
                }
            }
            //////////////////////////////faces end

            if (projectile.frame == 62 && (Sleep <= 0) && (!player.HasBuff(ModContent.BuffType<StatLower>())) && (!player.HasBuff(ModContent.BuffType<Sickness>()) && (!player.HasBuff(ModContent.BuffType<BloodmoonBuff>())) && (!player.HasBuff(ModContent.BuffType<EclipseBuff>()))))
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
            if (projectile.frame == 62 && (Sleep <= 0) && ((player.HasBuff(ModContent.BuffType<StatLower>())) || (player.HasBuff(ModContent.BuffType<Sickness>()) || (player.HasBuff(ModContent.BuffType<BloodmoonBuff>()) || (player.HasBuff(ModContent.BuffType<EclipseBuff>()))))))
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
                    Projectile.NewProjectile(base.projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 1f, ModContent.ProjectileType<Ztarget>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
                }
                base.projectile.localAI[0] = 1f;
            }

            if (player.MinionDamage() != base.projectile.Fairy().spawnedPlayerMinionDamageValue)
            {
                int trueDamage = (int)((float)base.projectile.Fairy().spawnedPlayerMinionProjectileDamageValue / base.projectile.Fairy().spawnedPlayerMinionDamageValue * player.MinionDamage());
                base.projectile.damage = trueDamage;
            }
            if (player.dead)
            {
                modPlayer.SariaXp /= 2;
            }
            
            if (player.dead || !player.active)
            {
                for (int j = 0; j < 72; j++)
                {
                    Dust dust = Dust.NewDustPerfect(projectile.Center, 113);
                    dust.velocity = ((float)Math.PI * 2f * Vector2.Dot(((float)j / 72f * ((float)Math.PI * 2f)).ToRotationVector2(), player.velocity.SafeNormalize(Vector2.UnitY).RotatedBy((float)j / 72f * ((float)Math.PI * -2f)))).ToRotationVector2();
                    dust.velocity = dust.velocity.RotatedBy((float)j / 36f * ((float)Math.PI * 2f)) * 8f;
                    dust.noGravity = true;
                    dust.scale *= 3.9f;
                }
                Projectile.NewProjectile(projectile.Center, Utils.NextVector2Circular(Main.rand, 0, 2), ModContent.ProjectileType<HealBallProjectile2>(), projectile.damage, projectile.knockBack, player.whoAmI);
                projectile.Kill();
            }
           
            if (player.HasBuff(ModContent.BuffType<SariaBuff>()))
            {
                projectile.timeLeft = 2;
            }
            if (!player.HasBuff(ModContent.BuffType<SariaBuff>()))
            {
                for (int j = 0; j < 72; j++)
                {
                    Dust dust = Dust.NewDustPerfect(projectile.Center, 113);
                    dust.velocity = ((float)Math.PI * 2f * Vector2.Dot(((float)j / 72f * ((float)Math.PI * 2f)).ToRotationVector2(), player.velocity.SafeNormalize(Vector2.UnitY).RotatedBy((float)j / 72f * ((float)Math.PI * -2f)))).ToRotationVector2();
                    dust.velocity = dust.velocity.RotatedBy((float)j / 36f * ((float)Math.PI * 2f)) * 8f;
                    dust.noGravity = true;
                    dust.scale *= 3.9f;
                    
                }
                Projectile.NewProjectile(projectile.Center, Utils.NextVector2Circular(Main.rand, 0, 2), ModContent.ProjectileType<HealBallProjectile2>(), projectile.damage, projectile.knockBack, player.whoAmI);
                projectile.Kill();
            }

            NPC target = base.projectile.Center.MinionHoming(500f, player);// the distance she targets enemies
            if (target != null && projectile.ai[0] == 0 && Transform != 1 && Sleep <= 0)
            {
                if (Transform != 5)
                {
                    projectile.ai[0] = 1;
                }
                else if (Transform == 5 && SwarmTimer >= 1000)
                {
                    projectile.ai[0] = 1;
                }

            }
            else if (target != null && projectile.ai[0] == 0 && Transform == 1 && player.ownedProjectileCounts[ModContent.ProjectileType<Bubble>()] < 1f && Sleep <= 0)
            {
                projectile.ai[0] = 1;
            }
            if (Transform == 6)
            {
                for (int b = 0; b < Main.maxNPCs; b++)
                {
                    NPC npc = Main.npc[b];
                    float between2 = Vector2.Distance(npc.Center, projectile.Center);
                    // Reasonable distance away so it doesn't target across multiple screens
                    if (between2 < 1200f && npc.friendly == false && target != null)
                    {
                        if (!npc.HasBuff(ModContent.BuffType<SariaCurse>()))
                        {
                            npc.buffImmune[ModContent.BuffType<SariaCurse>()] = false;
                            npc.AddBuff(ModContent.BuffType<SariaCurse>(), 2000);
                        }
                        if (between2 < 500f)
                        {
                            if ((player.HasBuff(ModContent.BuffType<Overcharged>())))
                            {
                                if (!npc.HasBuff(ModContent.BuffType<SariaCurse3>()))
                                {
                                    npc.buffImmune[ModContent.BuffType<SariaCurse3>()] = false;
                                    npc.AddBuff(ModContent.BuffType<SariaCurse3>(), 500);
                                    if (npc.HasBuff(ModContent.BuffType<SariaCurse3>()))
                                        if (!player.HasBuff(ModContent.BuffType<StatLower>()))
                                    {
                                        Projectile.NewProjectile(npc.Center + new Vector2(10f, 2f), Vector2.One.RotatedByRandom(6.2831854820251465) * 0f, ModContent.ProjectileType<ShadowClaw>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
                                    }
                                }
                            }
                        }

                    }
                }
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
                if (player.ownedProjectileCounts[ModContent.ProjectileType<period>()] <= 0f && Sleep <= 0)
                {
                    Mood = -3600;
                    Projectile.NewProjectile(base.projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 1f, ModContent.ProjectileType<Anger>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
                    Projectile.NewProjectile(base.projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 1f, ModContent.ProjectileType<period>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
                }
            }
            if (player.HasBuff(ModContent.BuffType<BloodmoonBuff>()) && (player.ownedProjectileCounts[ModContent.ProjectileType<period>()] <= 0f) && Sleep <= 0)

            {
                Mood = -3600;
                Projectile.NewProjectile(base.projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 1f, ModContent.ProjectileType<Anger>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
                Projectile.NewProjectile(base.projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 1f, ModContent.ProjectileType<period>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
            }
            if (player.HasBuff(ModContent.BuffType<StatLower>()) && (player.ownedProjectileCounts[ModContent.ProjectileType<period>()] <= 0f) && Sleep <= 0 && Cursed <= 0)

            {
                Mood -= 600;
                Projectile.NewProjectile(base.projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 1f, ModContent.ProjectileType<Sad2>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
                Projectile.NewProjectile(base.projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 1f, ModContent.ProjectileType<period>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
            }
            if (player.HasBuff(ModContent.BuffType<StatRaise>()) && (player.ownedProjectileCounts[ModContent.ProjectileType<period2>()] <= 0f) && Sleep <= 0 && Cursed <= 0)

            {
                Mood += 600;
                Projectile.NewProjectile(base.projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 1f, ModContent.ProjectileType<Smile2>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
                Projectile.NewProjectile(base.projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 1f, ModContent.ProjectileType<period2>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
            }


            //end of Flashupdate stuff

           
            if (projectile.frame >= 84 && projectile.frame <= 95 && Transform == 1)
            {
                if (Main.rand.NextBool(8))//controls the speed of when the sparkles spawn
                {
                    float radius = (float)Math.Sqrt(Main.rand.Next(34 * 34));
                    double angle = Main.rand.NextDouble() * 5.0 * Math.PI;


                    {
                        Dust.NewDust(new Vector2(projectile.Center.X + radius * (float)Math.Cos(angle), projectile.Center.Y + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<BubbleDust>(), 0f, 0f, 0, default(Color), 1.5f);
                    }
                }
            }
            if (projectile.frame >= 84 && projectile.frame <= 95 && Transform == 2)
            {
                if (Main.rand.NextBool(8))//controls the speed of when the sparkles spawn
                {
                    float radius = (float)Math.Sqrt(Main.rand.Next(34 * 34));
                    double angle = Main.rand.NextDouble() * 5.0 * Math.PI;


                    Dust.NewDust(new Vector2(projectile.Center.X + radius * (float)Math.Cos(angle), projectile.Center.Y + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<FlameDust>(), 0f, 0f, 0, default(Color), 1.5f);
                }
            }
            if (projectile.frame >= 84 && projectile.frame <= 95 && Transform == 3)
            {
                if (Main.rand.NextBool())//controls the speed of when the sparkles spawn
                {
                    float radius = (float)Math.Sqrt(Main.rand.Next(34 * 34));
                    double angle = Main.rand.NextDouble() * 5.0 * Math.PI;


                    Dust.NewDust(new Vector2(projectile.Center.X + radius * (float)Math.Cos(angle), projectile.Center.Y + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<StaticDust>(), 0f, 0f, 0, default(Color), 1.5f);
                }
            }
            if (projectile.frame >= 84 && projectile.frame <= 95 && Transform == 6)
            {
                if (Main.rand.NextBool())//controls the speed of when the sparkles spawn
                {
                    float radius = (float)Math.Sqrt(Main.rand.Next(34 * 34));
                    double angle = Main.rand.NextDouble() * 5.0 * Math.PI;


                    Dust.NewDust(new Vector2(projectile.Center.X + radius * (float)Math.Cos(angle), projectile.Center.Y + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<ShadowFlameDust>(), 0f, 0f, 0, default(Color), 1.5f);
                }
            }

            //Statraise and lower
            if (Transform == 0)
            {


                if ((Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneCrimson || Main.player[Main.myPlayer].ZoneCorrupt))
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
                if ((Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneSkyHeight || Main.player[Main.myPlayer].ZoneGlowshroom || Main.player[Main.myPlayer].ZoneJungle && !Main.player[Main.myPlayer].ZoneCrimson && !Main.player[Main.myPlayer].ZoneCorrupt) && (!player.HasBuff(ModContent.BuffType<StatLower>())))
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
            }
            if (Transform == 1)
            {
                if ((Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneDesert || Main.player[Main.myPlayer].ZoneJungle) || Main.player[Main.myPlayer].ZoneGlowshroom)
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
            }
            if (Transform == 2)
            {
                if ((Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneBeach || (Main.player[Main.myPlayer].ZoneRain && !Main.player[Main.myPlayer].ZoneSnow) || Main.player[Main.myPlayer].ZoneSandstorm))
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
                if ((Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneSnow || Main.player[Main.myPlayer].ZoneGlowshroom || Main.player[Main.myPlayer].ZoneJungle || Main.player[Main.myPlayer].ZoneDungeon || Main.player[Main.myPlayer].ZoneHoly && (!player.HasBuff(ModContent.BuffType<StatLower>()))))
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
            }
            if (Transform == 3)
            {
                if ((Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneUndergroundDesert || Main.player[Main.myPlayer].ZoneUnderworldHeight || Main.player[Main.myPlayer].ZoneRockLayerHeight || Main.player[Main.myPlayer].ZoneUnderworldHeight))
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
                if ((Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneBeach || Main.player[Main.myPlayer].ZoneRain))
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
            }
            if (Transform == 4)
            {
                if ((Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneSkyHeight || Main.player[Main.myPlayer].ZoneRain || Main.player[Main.myPlayer].ZoneBeach))
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
            }
            if (Transform == 5)
            {
               
            }
            if (Transform == 6)
            {
                if ((Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneOverworldHeight && Main.dayTime && !Main.player[Main.myPlayer].ZoneCrimson && !Main.player[Main.myPlayer].ZoneCorrupt))
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
                if ((Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneCorrupt || Main.player[Main.myPlayer].ZoneCrimson || Main.player[Main.myPlayer].ZoneDungeon || !Main.dayTime))
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
            }

            ////////////// Statraise and lower end
            ///







           

            Vector2 idlePosition = player.Center;

            float nothing = 1;
            float speed = 2;



            bool foundTarget = false;

            float minionPositionOffsetX = ((60 + projectile.minionPos / 80) * player.direction) - 15;
            idlePosition.Y -= 15f;
            idlePosition.X += minionPositionOffsetX;
            Vector2 vectorToIdlePosition = idlePosition - projectile.Center;

            float distanceToIdlePosition = vectorToIdlePosition.Length();



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
            if (MoveTimer >= 350 && (base.projectile.frame >= 0) && (base.projectile.frame <= 75) && (distanceToIdlePosition <= 180) && (Math.Abs(projectile.velocity.X) <= .5) && (player.statLife >= player.statLifeMax2) || Sleep >= 1)
            {
                nothing = 0;
            }
            else
            {
                nothing = 1;

            }
            if (Sleep >= 1 && (distanceToIdlePosition > 180))
            {
                MoveTimer = 0;
                
            }
            projectile.velocity = ((projectile.velocity * (13 - speed) + direction) / 20) * nothing;
        
            if (player.statLife < (player.statLifeMax2)/4 && !player.HasBuff(ModContent.BuffType<HealpulseBuff>()))
            {
                Mood -= 600;
                player.statLife += 500;
                player.AddBuff(ModContent.BuffType<HealpulseBuff>(), 8000);
                
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

            if (projectile.frame == 65 && Sleep > 0 && MoveTimer >= 550)
            {
                if (Main.rand.NextBool(40))
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
                    Dust.NewDust(new Vector2((projectile.Center.X + sneezespot) + radius * (float)Math.Cos(angle), (projectile.Center.Y - 10) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<Z>(), 0f, 0f, 0, default(Color), 1.5f);
                }
            }

           
            if (MoveTimer >= 2500 && projectile.frame == 59)
            {
                if (Main.rand.NextBool(2))
                {
                    Sleep = 1;
                }
            }



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
                      
                        if (Sleep > 0 && MoveTimer > 550)
                        {

                            if (projectile.frame == 60)
                            {

                                projectile.frame = 62;
                                TimeAsleep += 5;
                                Main.PlaySound(base.mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Hover"), base.projectile.Center);

                            }
                            if (projectile.frame >= 66 && MoveTimer >= 550)
                            {
                                TimeAsleep += 5;
                                projectile.frame = 62;
                                Main.PlaySound(base.mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Hover"), base.projectile.Center);

                            }
                           
                        }
                        if (projectile.frame == 66 && (player.ownedProjectileCounts[ModContent.ProjectileType<Notice>()] <= 0f) && Sleep > 0)
                        {
                            Projectile.NewProjectile(base.projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 1f, ModContent.ProjectileType<Notice>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
                        }
                        if (base.projectile.frame == 63 && Sleep <= 0)
                        {
                            Main.PlaySound(base.mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Step2"), base.projectile.Center);
                        }
                        if (base.projectile.frame >= 76)
                        {
                            base.projectile.frame = 0;
                            if (Sleep > 0)
                            {
                                Sleep = 0;
                            }
                            Main.PlaySound(base.mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Step1"), base.projectile.Center);
                        }
                        if (base.projectile.frame == 58 && player.statLife < ((player.statLifeMax2) - (player.statLifeMax2 / 4)) && !player.HasBuff(ModContent.BuffType<HealpulseBuff>()))
                        {
                            Mood -= 600;
                            player.statLife += 500;
                            if (!player.HasBuff(ModContent.BuffType<HealpulseBuff>()))
                            {
                                
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







                    

                    ///////////Transform Attacks
                    {
                        if (Transform == 0)
                        //Main.NewText("Frame: " + base.projectile.frame);
                        {

                            if (base.projectile.frame == 85)
                            {
                                Projectile.NewProjectile(base.projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ProjectileType<Locator>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
                            }
                            if (base.projectile.frame == 86)
                            {
                                Projectile.NewProjectile(base.projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ProjectileType<Locator>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
                            }
                            if (base.projectile.frame == 87)
                            {
                                Projectile.NewProjectile(base.projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ProjectileType<Locator>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
                            }
                            if (base.projectile.frame == 88)
                            {
                                Projectile.NewProjectile(base.projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ProjectileType<Locator>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
                            }
                            if (base.projectile.frame == 89)
                            {
                                Projectile.NewProjectile(base.projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ProjectileType<Locator>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
                            }


                            if (base.projectile.frame == 90)
                            {
                                target = base.projectile.Center.MinionHoming(500f, player);// the distance she targets enemies
                                Main.PlaySound(SoundID.Item77, base.projectile.Center);
                                for (int j = 0; j < 1; j++) //set to 2
                                {
                                    Projectile.NewProjectile(base.projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ProjectileType<Locator>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
                                }
                            }
                        }
                        else if (Transform == 1)
                        {
                            if (base.projectile.frame == 90 && player.ownedProjectileCounts[ModContent.ProjectileType<Bubble>()] < 1f)
                            {
                                target = base.projectile.Center.MinionHoming(500f, player);// the distance she targets enemies
                                Main.PlaySound(SoundID.Item77, base.projectile.Center);

                                if (player.HasBuff(ModContent.BuffType<Overcharged>()))
                                {
                                    for (int j = 0; j < 12; j++) //set to 2
                                    {
                                        Projectile.NewProjectile(base.projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ProjectileType<Bubble>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
                                    }
                                }
                                else
                                {
                                    for (int j = 0; j < 8; j++) //set to 2
                                    {
                                        Projectile.NewProjectile(base.projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ProjectileType<Bubble>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
                                    }
                                }
                               
                            }
                        }
                        
                        else if (Transform == 2)
                        {
                            if (base.projectile.frame == 90)
                            {
                                Main.PlaySound(SoundID.Item77, base.projectile.Center);
                                Projectile.NewProjectile(base.projectile.Center + new Vector2(0f, 0f), Vector2.One.RotatedByRandom(6) * 3f, ModContent.ProjectileType<RubyPsychicSeeker>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
                            }
                        }
                        else if (Transform == 3)
                        {
                            if (base.projectile.frame == 90 && (player.ownedProjectileCounts[ModContent.ProjectileType<LightningLocator>()] == 0f))
                            {
                                target = base.projectile.Center.MinionHoming(500f, player);// the distance she targets enemies
                                Main.PlaySound(SoundID.Item77, base.projectile.Center);
                                for (int j = 0; j < 1; j++) //set to 2
                                {
                                    Projectile.NewProjectile(base.projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ProjectileType<LightningLocator>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
                                }
                            }
                            else if (base.projectile.frame == 90 && (player.ownedProjectileCounts[ModContent.ProjectileType<LightningLocator>()] > 0f))
                            {
                                target = base.projectile.Center.MinionHoming(500f, player);// the distance she targets enemies
                                Main.PlaySound(SoundID.Item77, base.projectile.Center);
                                for (int j = 0; j < 1; j++) //set to 2
                                {
                                    Projectile.NewProjectile(base.projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ProjectileType<Static2>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
                                }
                            }
                            if (projectile.frame == 92)
                            {
                                for (int j = 0; j < 1; j++) //set to 2
                                {
                                    Projectile.NewProjectile(base.projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ProjectileType<Static2>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
                                }
                            }
                        }
                        else if (Transform == 4)
                        {
                            if (base.projectile.frame == 90 && (player.ownedProjectileCounts[ModContent.ProjectileType<Rupee>()] <= 0f))
                            {
                                if (GemTimer > 100 && GemTimer < 200)
                                {
                                    {

                                        target = base.projectile.Center.MinionHoming(500f, player);// the distance she targets enemies
                                        Main.PlaySound(SoundID.Item77, base.projectile.Center);
                                        Projectile.NewProjectile(base.projectile.Center + new Vector2(0, 370f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ProjectileType<Specialrupee>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
                                    }
                                }
                                else
                                {

                                    target = base.projectile.Center.MinionHoming(500f, player);// the distance she targets enemies
                                    Main.PlaySound(SoundID.Item77, base.projectile.Center);
                                    for (int j = 0; j < 1; j++) //set to 2
                                    {
                                        Projectile.NewProjectile(base.projectile.Center + new Vector2(0, 370f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ProjectileType<Rupee>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
                                    }
                                }
                            }
                        }
                        else if (Transform == 5)
                        {
                            if (base.projectile.frame == 90 && SwarmTimer >= 1000)
                            {
                                
                                if (player.ownedProjectileCounts[ModContent.ProjectileType<DuskBallProjectile>()] <= 0f)
                                {
                                    if (((Main.rand.NextBool(60)) && (player.ownedProjectileCounts[ModContent.ProjectileType<GreenMoth>()] <= 0f) && (player.ownedProjectileCounts[ModContent.ProjectileType<GreenMothGoliath2>()] <= 0f) && (player.ownedProjectileCounts[ModContent.ProjectileType<AmberGreen>()] <= 0f) && (player.ownedProjectileCounts[ModContent.ProjectileType<GreenMothGiant>()] <= 0f) && (player.ownedProjectileCounts[ModContent.ProjectileType<GreenMothGoliath>()] <= 0f) && ((player.ownedProjectileCounts[ModContent.ProjectileType<RedMoth>()] == 1f) || (player.ownedProjectileCounts[ModContent.ProjectileType<RedMothGiant>()] == 1f)) && ((player.ownedProjectileCounts[ModContent.ProjectileType<PurpleMoth>()] == 1f) || (player.ownedProjectileCounts[ModContent.ProjectileType<PurpleMothGiant>()] == 1f))))
                                    {
                                        {
                                            modPlayer.SariaXp++;
                                            Main.PlaySound(SoundID.Item77, base.projectile.Center);
                                            target = base.projectile.Center.MinionHoming(500f, player);// the distance she targets enemies
                                            Main.PlaySound(SoundID.Item77, base.projectile.Center);
                                            Projectile.NewProjectile(base.projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ProjectileType<AmberGreen>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);

                                        }
                                    }
                                    else
                                    {
                                        if ((BugTimer >= 50 && BugTimer <= 350) && ((player.ownedProjectileCounts[ModContent.ProjectileType<RedMoth>()] <= 0f) && (player.ownedProjectileCounts[ModContent.ProjectileType<RedMothGiant>()] <= 0f)))
                                        {
                                            modPlayer.SariaXp++;
                                            Projectile.NewProjectile(base.projectile.Center + new Vector2(-250f, 370f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ProjectileType<AmberRed>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);

                                        }
                                        if ((BugTimer >= 50 && BugTimer <= 250) && ((player.ownedProjectileCounts[ModContent.ProjectileType<PurpleMoth>()] <= 0f) && (player.ownedProjectileCounts[ModContent.ProjectileType<PurpleMothGiant>()] <= 0f)))
                                        {
                                            modPlayer.SariaXp++;
                                            Projectile.NewProjectile(base.projectile.Center + new Vector2(250f, 370f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ProjectileType<AmberPurple>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);

                                        }
                                        target = base.projectile.Center.MinionHoming(500f, player);// the distance she targets enemies
                                        Main.PlaySound(SoundID.Item77, base.projectile.Center);
                                        for (int j = 0; j < 1; j++) //set to 2
                                        {
                                            modPlayer.SariaXp++;
                                            Projectile.NewProjectile(base.projectile.Center + new Vector2(-500f, 370f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ProjectileType<AmberBlack1>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
                                            Projectile.NewProjectile(base.projectile.Center + new Vector2(500f, 370f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ProjectileType<AmberBlack2>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
                                        }
                                    }
                                    SwarmTimer = 0;
                                }
                            }
                        }
                        else if (Transform == 6)
                        {
                            if (base.projectile.frame == 90)
                            {
                                target = base.projectile.Center.MinionHoming(500f, player);// the distance she targets enemies
                                Main.PlaySound(SoundID.Item77, base.projectile.Center);

                                target = base.projectile.Center.MinionHoming(500f, player);// the distance she targets enemies
                                Main.PlaySound(SoundID.Item77, base.projectile.Center);
                                for (int j = 0; j < 1; j++) //set to 2
                                {
                                    Projectile.NewProjectile(base.projectile.Center + new Vector2(0f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ProjectileType<Shadowmelt>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
                                }
                            }
                        }
                    }
                    //////////////////// End of Transform attacks
                    ///










                    if (base.projectile.frame > 96) //stop when done
                    {
                        base.projectile.frame = 10;
                        projectile.ai[0] = 3;
                    }
                }
                
                if (projectile.ai[0] == 1 && Transform != 1 ) //this is set when a target is found
                {

                    base.projectile.frame = 83; //animation setup
                    Main.PlaySound(base.mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Step1"), base.projectile.Center);
                    projectile.ai[0] = 2; //next phase

                }
               else  if (projectile.ai[0] == 1 && Transform == 1 && (player.ownedProjectileCounts[ModContent.ProjectileType<Bubble>()] < 1f)) //this is set when a target is found
                {

                    base.projectile.frame = 83; //animation setup
                    Main.PlaySound(base.mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Step1"), base.projectile.Center);
                    projectile.ai[0] = 2; //next phase

                }
               



            }
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Player player = Main.player[base.projectile.owner];
            FairyPlayer modPlayer = player.Fairy();
            {

                
                Vector2 drawPosition;

                for (int i = 1; i < 25; i++)
                {
                    Texture2D texture = Main.projectileTexture[ModContent.ProjectileType<SariaFeet>()];
                    Vector2 startPos = base.projectile.oldPos[i] + base.projectile.Size * 0.5f - Main.screenPosition;
                    int frameHeight = texture.Height / Main.projFrames[base.projectile.type];
                    int frameY = frameHeight * base.projectile.frame;
                    float completionRatio = (float)i / (float)base.projectile.oldPos.Length;
                    Color drawColor = Color.Lerp(lightColor, Color.LightPink, 20f);
                    drawColor = Color.Lerp(drawColor, Color.DarkViolet, completionRatio);
                    drawColor = Color.Lerp(drawColor, Color.Transparent, completionRatio);
                    Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                    Vector2 origin = rectangle.Size() / 2f;
                    float rotation = base.projectile.rotation;
                    float scale = base.projectile.scale;
                    SpriteEffects spriteEffects = SpriteEffects.None;
                    startPos.Y += 1;
                    startPos.X += +17;

                    if (base.projectile.spriteDirection == -1)
                    {
                        spriteEffects = SpriteEffects.FlipHorizontally;
                    }
                    Main.spriteBatch.Draw(texture, startPos, rectangle, base.projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, layerDepth: 0f);

                }
                for (int i = 1; i < 30; i++)
                {
                    Texture2D texture = Main.projectileTexture[ModContent.ProjectileType<SariaArm>()];
                    Vector2 startPos = base.projectile.oldPos[i] + base.projectile.Size * 0.5f - Main.screenPosition;
                    int frameHeight = texture.Height / Main.projFrames[base.projectile.type];
                    int frameY = frameHeight * base.projectile.frame;
                    float completionRatio = (float)i / (float)base.projectile.oldPos.Length;
                    Color drawColor = Color.Lerp(lightColor, Color.LightPink, 20f);
                    drawColor = Color.Lerp(drawColor, Color.DarkViolet, completionRatio);
                    drawColor = Color.Lerp(drawColor, Color.Transparent, completionRatio);
                    Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                    Vector2 origin = rectangle.Size() / 2f;
                    float rotation = base.projectile.rotation;
                    float scale = base.projectile.scale;
                    SpriteEffects spriteEffects = SpriteEffects.None;
                    startPos.Y += 1;
                    startPos.X += +17;
                    if (base.projectile.spriteDirection == -1)
                    {
                        spriteEffects = SpriteEffects.FlipHorizontally;
                    }
                    Main.spriteBatch.Draw(texture, startPos, rectangle, base.projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, layerDepth: 0f);

                }
                ////////////// Transform Ability
                if (Transform == 0)
                {
                    Texture2D texture = Main.projectileTexture[ModContent.ProjectileType<Saria>()];
                    Vector2 startPos = base.projectile.Center - Main.screenPosition + new Vector2(0f, base.projectile.gfxOffY);
                    int frameHeight = texture.Height / Main.projFrames[base.projectile.type];
                    int frameY = frameHeight * base.projectile.frame;
                    Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                    Vector2 origin = rectangle.Size() / 2f;
                    float rotation = base.projectile.rotation;
                    float scale = base.projectile.scale;
                    SpriteEffects spriteEffects = SpriteEffects.None;
                    startPos.Y += 1;
                    startPos.X += +17;
                    if (base.projectile.spriteDirection == -1)
                    {
                        spriteEffects = SpriteEffects.FlipHorizontally;
                    }
                    Main.spriteBatch.Draw(texture, startPos, rectangle, base.projectile.GetAlpha(lightColor), rotation, origin, scale, spriteEffects, 0f);

                }
                else if (Transform == 1)
                {
                    Texture2D texture = Main.projectileTexture[ModContent.ProjectileType<Saria2>()];
                    Vector2 startPos = base.projectile.Center - Main.screenPosition + new Vector2(0f, base.projectile.gfxOffY);
                    int frameHeight = texture.Height / Main.projFrames[base.projectile.type];
                    int frameY = frameHeight * base.projectile.frame;
                    Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                    Vector2 origin = rectangle.Size() / 2f;
                    float rotation = base.projectile.rotation;
                    float scale = base.projectile.scale;
                    SpriteEffects spriteEffects = SpriteEffects.None;
                    startPos.Y += 1;
                    startPos.X += +17;
                    if (base.projectile.spriteDirection == -1)
                    {
                        spriteEffects = SpriteEffects.FlipHorizontally;
                    }
                    Main.spriteBatch.Draw(texture, startPos, rectangle, base.projectile.GetAlpha(lightColor), rotation, origin, scale, spriteEffects, 0f);

                }
                else if (Transform == 2)
                {
                    Texture2D texture = Main.projectileTexture[ModContent.ProjectileType<Saria3>()];
                    Vector2 startPos = base.projectile.Center - Main.screenPosition + new Vector2(0f, base.projectile.gfxOffY);
                    int frameHeight = texture.Height / Main.projFrames[base.projectile.type];
                    int frameY = frameHeight * base.projectile.frame;
                    Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                    Vector2 origin = rectangle.Size() / 2f;
                    float rotation = base.projectile.rotation;
                    float scale = base.projectile.scale;
                    SpriteEffects spriteEffects = SpriteEffects.None;
                    startPos.Y += 1;
                    startPos.X += +17;
                    if (base.projectile.spriteDirection == -1)
                    {
                        spriteEffects = SpriteEffects.FlipHorizontally;
                    }
                    Main.spriteBatch.Draw(texture, startPos, rectangle, base.projectile.GetAlpha(lightColor), rotation, origin, scale, spriteEffects, 0f);

                }
                else if (Transform == 3)
                {
                    Texture2D texture = Main.projectileTexture[ModContent.ProjectileType<Saria4>()];
                    Vector2 startPos = base.projectile.Center - Main.screenPosition + new Vector2(0f, base.projectile.gfxOffY);
                    int frameHeight = texture.Height / Main.projFrames[base.projectile.type];
                    int frameY = frameHeight * base.projectile.frame;
                    Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                    Vector2 origin = rectangle.Size() / 2f;
                    float rotation = base.projectile.rotation;
                    float scale = base.projectile.scale;
                    SpriteEffects spriteEffects = SpriteEffects.None;
                    startPos.Y += 1;
                    startPos.X += +17;
                    if (base.projectile.spriteDirection == -1)
                    {
                        spriteEffects = SpriteEffects.FlipHorizontally;
                    }
                    Main.spriteBatch.Draw(texture, startPos, rectangle, base.projectile.GetAlpha(lightColor), rotation, origin, scale, spriteEffects, 0f);

                }
                else if (Transform == 4)
                {
                    Texture2D texture = Main.projectileTexture[ModContent.ProjectileType<Saria5>()];
                    Vector2 startPos = base.projectile.Center - Main.screenPosition + new Vector2(0f, base.projectile.gfxOffY);
                    int frameHeight = texture.Height / Main.projFrames[base.projectile.type];
                    int frameY = frameHeight * base.projectile.frame;
                    Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                    Vector2 origin = rectangle.Size() / 2f;
                    float rotation = base.projectile.rotation;
                    float scale = base.projectile.scale;
                    SpriteEffects spriteEffects = SpriteEffects.None;
                    startPos.Y += 1;
                    startPos.X += +17;
                    if (base.projectile.spriteDirection == -1)
                    {
                        spriteEffects = SpriteEffects.FlipHorizontally;
                    }
                    Main.spriteBatch.Draw(texture, startPos, rectangle, base.projectile.GetAlpha(lightColor), rotation, origin, scale, spriteEffects, 0f);

                }
                else if (Transform == 5)
                {
                    Texture2D texture = Main.projectileTexture[ModContent.ProjectileType<Saria6>()];
                    Vector2 startPos = base.projectile.Center - Main.screenPosition + new Vector2(0f, base.projectile.gfxOffY);
                    int frameHeight = texture.Height / Main.projFrames[base.projectile.type];
                    int frameY = frameHeight * base.projectile.frame;
                    Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                    Vector2 origin = rectangle.Size() / 2f;
                    float rotation = base.projectile.rotation;
                    float scale = base.projectile.scale;
                    SpriteEffects spriteEffects = SpriteEffects.None;
                    startPos.Y += 1;
                    startPos.X += +17;
                    if (base.projectile.spriteDirection == -1)
                    {
                        spriteEffects = SpriteEffects.FlipHorizontally;
                    }
                    Main.spriteBatch.Draw(texture, startPos, rectangle, base.projectile.GetAlpha(lightColor), rotation, origin, scale, spriteEffects, 0f);

                }
                else if (Transform == 6)
                {
                    Texture2D texture = Main.projectileTexture[ModContent.ProjectileType<Saria7>()];
                    Vector2 startPos = base.projectile.Center - Main.screenPosition + new Vector2(0f, base.projectile.gfxOffY);
                    int frameHeight = texture.Height / Main.projFrames[base.projectile.type];
                    int frameY = frameHeight * base.projectile.frame;
                    Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                    Vector2 origin = rectangle.Size() / 2f;
                    float rotation = base.projectile.rotation;
                    float scale = base.projectile.scale;
                    SpriteEffects spriteEffects = SpriteEffects.None;
                    startPos.Y += 1;
                    startPos.X += +17;
                    if (base.projectile.spriteDirection == -1)
                    {
                        spriteEffects = SpriteEffects.FlipHorizontally;
                    }
                    Main.spriteBatch.Draw(texture, startPos, rectangle, base.projectile.GetAlpha(lightColor), rotation, origin, scale, spriteEffects, 0f);

                }
                ///////////faces
                ///
                if (Sleep <= 0)
                {
                    if (Cursed <= 0)
                    {
                        if (Transform != 7 && Transform != 6 && Mood > -1200 && Mood < 3600)
                        {
                            Texture2D texture = Main.projectileTexture[ModContent.ProjectileType<SariaNormalFace>()];
                            Vector2 startPos = base.projectile.Center - Main.screenPosition + new Vector2(0f, base.projectile.gfxOffY);
                            int frameHeight = texture.Height / Main.projFrames[base.projectile.type];
                            int frameY = frameHeight * base.projectile.frame;
                            Color drawColor = Color.Lerp(lightColor, Color.LightPink, 20f);
                            drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                            Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                            Vector2 origin = rectangle.Size() / 2f;
                            float rotation = base.projectile.rotation;
                            float scale = base.projectile.scale;
                            SpriteEffects spriteEffects = SpriteEffects.None;
                            startPos.Y += 1;
                            startPos.X += +17;
                            if (base.projectile.spriteDirection == -1)
                            {
                                spriteEffects = SpriteEffects.FlipHorizontally;
                            }
                            Main.spriteBatch.Draw(texture, startPos, rectangle, base.projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                        }
                        if (Transform == 6 && Mood > -1200 && Mood < 3600)
                        {
                            Texture2D texture = Main.projectileTexture[ModContent.ProjectileType<Saria7NormalFace>()];
                            Vector2 startPos = base.projectile.Center - Main.screenPosition + new Vector2(0f, base.projectile.gfxOffY);
                            int frameHeight = texture.Height / Main.projFrames[base.projectile.type];
                            int frameY = frameHeight * base.projectile.frame;
                            Color drawColor = Color.Lerp(lightColor, Color.LightBlue, 20f);
                            drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                            Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                            Vector2 origin = rectangle.Size() / 2f;
                            float rotation = base.projectile.rotation;
                            float scale = base.projectile.scale;
                            SpriteEffects spriteEffects = SpriteEffects.None;
                            startPos.Y += 1;
                            startPos.X += +17;
                            if (base.projectile.spriteDirection == -1)
                            {
                                spriteEffects = SpriteEffects.FlipHorizontally;
                            }
                            Main.spriteBatch.Draw(texture, startPos, rectangle, base.projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                        }
                        if (Transform != 7 && Transform != 6 && Mood >= 2400 && Mood < 3600)
                        {
                            Texture2D texture = Main.projectileTexture[ModContent.ProjectileType<SariaHappy>()];
                            Vector2 startPos = base.projectile.Center - Main.screenPosition + new Vector2(0f, base.projectile.gfxOffY);
                            int frameHeight = texture.Height / Main.projFrames[base.projectile.type];
                            int frameY = frameHeight * base.projectile.frame;
                            Color drawColor = Color.Lerp(lightColor, Color.LightPink, 20f);
                            drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                            Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                            Vector2 origin = rectangle.Size() / 2f;
                            float rotation = base.projectile.rotation;
                            float scale = base.projectile.scale;
                            SpriteEffects spriteEffects = SpriteEffects.None;
                            startPos.Y += 1;
                            startPos.X += +17;
                            if (base.projectile.spriteDirection == -1)
                            {
                                spriteEffects = SpriteEffects.FlipHorizontally;
                            }
                            Main.spriteBatch.Draw(texture, startPos, rectangle, base.projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                        }
                        if (Transform == 6 && Mood >= 2400 && Mood < 3600)
                        {
                            Texture2D texture = Main.projectileTexture[ModContent.ProjectileType<Saria7Happy>()];
                            Vector2 startPos = base.projectile.Center - Main.screenPosition + new Vector2(0f, base.projectile.gfxOffY);
                            int frameHeight = texture.Height / Main.projFrames[base.projectile.type];
                            int frameY = frameHeight * base.projectile.frame;
                            Color drawColor = Color.Lerp(lightColor, Color.LightBlue, 20f);
                            drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                            Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                            Vector2 origin = rectangle.Size() / 2f;
                            float rotation = base.projectile.rotation;
                            float scale = base.projectile.scale;
                            SpriteEffects spriteEffects = SpriteEffects.None;
                            startPos.Y += 1;
                            startPos.X += +17;
                            if (base.projectile.spriteDirection == -1)
                            {
                                spriteEffects = SpriteEffects.FlipHorizontally;
                            }
                            Main.spriteBatch.Draw(texture, startPos, rectangle, base.projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                        }
                        if (Transform != 6 && Transform != 7 && Mood >= 3600)
                        {
                            Texture2D texture = Main.projectileTexture[ModContent.ProjectileType<SariaPumped>()];
                            Vector2 startPos = base.projectile.Center - Main.screenPosition + new Vector2(0f, base.projectile.gfxOffY);
                            int frameHeight = texture.Height / Main.projFrames[base.projectile.type];
                            int frameY = frameHeight * base.projectile.frame;
                            Color drawColor = Color.Lerp(lightColor, Color.LightBlue, 20f);
                            drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                            Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                            Vector2 origin = rectangle.Size() / 2f;
                            float rotation = base.projectile.rotation;
                            float scale = base.projectile.scale;
                            SpriteEffects spriteEffects = SpriteEffects.None;
                            startPos.Y += 1;
                            startPos.X += +17;
                            if (base.projectile.spriteDirection == -1)
                            {
                                spriteEffects = SpriteEffects.FlipHorizontally;
                            }
                            Main.spriteBatch.Draw(texture, startPos, rectangle, base.projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                        }
                        if (Transform == 6 && Mood >= 3600)
                        {
                            Texture2D texture = Main.projectileTexture[ModContent.ProjectileType<Saria7Pumped>()];
                            Vector2 startPos = base.projectile.Center - Main.screenPosition + new Vector2(0f, base.projectile.gfxOffY);
                            int frameHeight = texture.Height / Main.projFrames[base.projectile.type];
                            int frameY = frameHeight * base.projectile.frame;
                            Color drawColor = Color.Lerp(lightColor, Color.LightBlue, 20f);
                            drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                            Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                            Vector2 origin = rectangle.Size() / 2f;
                            float rotation = base.projectile.rotation;
                            float scale = base.projectile.scale;
                            SpriteEffects spriteEffects = SpriteEffects.None;
                            startPos.Y += 1;
                            startPos.X += +17;
                            if (base.projectile.spriteDirection == -1)
                            {
                                spriteEffects = SpriteEffects.FlipHorizontally;
                            }
                            Main.spriteBatch.Draw(texture, startPos, rectangle, base.projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                        }
                        if (Transform != 6 && Transform != 7 && ((Mood <= -1200 && Mood > -2400) || Mood <= -3600))
                        {
                            Texture2D texture = Main.projectileTexture[ModContent.ProjectileType<SariaSad>()];
                            Vector2 startPos = base.projectile.Center - Main.screenPosition + new Vector2(0f, base.projectile.gfxOffY);
                            int frameHeight = texture.Height / Main.projFrames[base.projectile.type];
                            int frameY = frameHeight * base.projectile.frame;
                            Color drawColor = Color.Lerp(lightColor, Color.LightPink, 20f);
                            drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                            Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                            Vector2 origin = rectangle.Size() / 2f;
                            float rotation = base.projectile.rotation;
                            float scale = base.projectile.scale;
                            SpriteEffects spriteEffects = SpriteEffects.None;
                            startPos.Y += 1;
                            startPos.X += +17;
                            if (base.projectile.spriteDirection == -1)
                            {
                                spriteEffects = SpriteEffects.FlipHorizontally;
                            }
                            Main.spriteBatch.Draw(texture, startPos, rectangle, base.projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                        }
                        if (Transform == 6 && ((Mood <= -1200 && Mood > -2400) || Mood <= -3600))
                        {
                            Texture2D texture = Main.projectileTexture[ModContent.ProjectileType<Saria7Sad>()];
                            Vector2 startPos = base.projectile.Center - Main.screenPosition + new Vector2(0f, base.projectile.gfxOffY);
                            int frameHeight = texture.Height / Main.projFrames[base.projectile.type];
                            int frameY = frameHeight * base.projectile.frame;
                            Color drawColor = Color.Lerp(lightColor, Color.LightBlue, 20f);
                            drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                            Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                            Vector2 origin = rectangle.Size() / 2f;
                            float rotation = base.projectile.rotation;
                            float scale = base.projectile.scale;
                            SpriteEffects spriteEffects = SpriteEffects.None;
                            startPos.Y += 1;
                            startPos.X += +17;
                            if (base.projectile.spriteDirection == -1)
                            {
                                spriteEffects = SpriteEffects.FlipHorizontally;
                            }
                            Main.spriteBatch.Draw(texture, startPos, rectangle, base.projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                        }
                        if (Transform != 6 && Transform != 7 && Mood <= -2400 && Mood > -3600)
                        {
                            Texture2D texture = Main.projectileTexture[ModContent.ProjectileType<SariaAngry>()];
                            Vector2 startPos = base.projectile.Center - Main.screenPosition + new Vector2(0f, base.projectile.gfxOffY);
                            int frameHeight = texture.Height / Main.projFrames[base.projectile.type];
                            int frameY = frameHeight * base.projectile.frame;
                            Color drawColor = Color.Lerp(lightColor, Color.LightBlue, 20f);
                            drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                            Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                            Vector2 origin = rectangle.Size() / 2f;
                            float rotation = base.projectile.rotation;
                            float scale = base.projectile.scale;
                            SpriteEffects spriteEffects = SpriteEffects.None;
                            startPos.Y += 1;
                            startPos.X += +17;
                            if (base.projectile.spriteDirection == -1)
                            {
                                spriteEffects = SpriteEffects.FlipHorizontally;
                            }
                            Main.spriteBatch.Draw(texture, startPos, rectangle, base.projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                        }
                        if (Transform == 6 && Mood <= -2400 && Mood > -3600)
                        {
                            Texture2D texture = Main.projectileTexture[ModContent.ProjectileType<Saria7Angry>()];
                            Vector2 startPos = base.projectile.Center - Main.screenPosition + new Vector2(0f, base.projectile.gfxOffY);
                            int frameHeight = texture.Height / Main.projFrames[base.projectile.type];
                            int frameY = frameHeight * base.projectile.frame;
                            Color drawColor = Color.Lerp(lightColor, Color.LightBlue, 20f);
                            drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                            Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                            Vector2 origin = rectangle.Size() / 2f;
                            float rotation = base.projectile.rotation;
                            float scale = base.projectile.scale;
                            SpriteEffects spriteEffects = SpriteEffects.None;
                            startPos.Y += 1;
                            startPos.X += +17;
                            if (base.projectile.spriteDirection == -1)
                            {
                                spriteEffects = SpriteEffects.FlipHorizontally;
                            }
                            Main.spriteBatch.Draw(texture, startPos, rectangle, base.projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                        }

                        for (int i = 1; i < 5; i++)
                        {
                            Texture2D texture = Main.projectileTexture[ModContent.ProjectileType<SariaArm2>()];
                            Vector2 startPos = base.projectile.oldPos[i] + base.projectile.Size * 0.5f - Main.screenPosition;
                            int frameHeight = texture.Height / Main.projFrames[base.projectile.type];
                            int frameY = frameHeight * base.projectile.frame;
                            float completionRatio = (float)i / (float)base.projectile.oldPos.Length;
                            Color drawColor = Color.Lerp(lightColor, Color.LightPink, 20f);
                            drawColor = Color.Lerp(drawColor, Color.HotPink, completionRatio);

                            Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                            Vector2 origin = rectangle.Size() / 2f;
                            float rotation = base.projectile.rotation;
                            float scale = base.projectile.scale;
                            SpriteEffects spriteEffects = SpriteEffects.None;
                            startPos.Y += 1;
                            startPos.X += +17;
                            if (base.projectile.spriteDirection == -1)
                            {
                                spriteEffects = SpriteEffects.FlipHorizontally;
                            }
                            Main.spriteBatch.Draw(texture, startPos, rectangle, base.projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, layerDepth: 0f);

                        }
                    }
                    if (Cursed >= 1)
                    {
                        {
                            Texture2D texture = Main.projectileTexture[ModContent.ProjectileType<SariaShader>()];
                            Vector2 startPos = base.projectile.Center - Main.screenPosition + new Vector2(0f, base.projectile.gfxOffY);
                            int frameHeight = texture.Height / Main.projFrames[base.projectile.type];
                            int frameY = frameHeight * base.projectile.frame;
                            Color drawColor = Color.Lerp(lightColor, Color.LightBlue, 20f);
                            drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                            Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                            Vector2 origin = rectangle.Size() / 2f;
                            float rotation = base.projectile.rotation;
                            float scale = base.projectile.scale;
                            SpriteEffects spriteEffects = SpriteEffects.None;
                            startPos.Y += 1;
                            startPos.X += +17;
                            if (base.projectile.spriteDirection == -1)
                            {
                                spriteEffects = SpriteEffects.FlipHorizontally;
                            }
                            Main.spriteBatch.Draw(texture, startPos, rectangle, base.projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                        }
                        {
                            Texture2D texture = Main.projectileTexture[ModContent.ProjectileType<SariaCursed>()];
                            Vector2 startPos = base.projectile.Center - Main.screenPosition + new Vector2(0f, base.projectile.gfxOffY);
                            int frameHeight = texture.Height / Main.projFrames[base.projectile.type];
                            int frameY = frameHeight * base.projectile.frame;
                            Color drawColor = Color.Lerp(lightColor, Color.LightBlue, 20f);
                            drawColor = Color.Lerp(drawColor, Color.MediumVioletRed, 0);
                            Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                            Vector2 origin = rectangle.Size() / 2f;
                            float rotation = base.projectile.rotation;
                            float scale = base.projectile.scale;
                            SpriteEffects spriteEffects = SpriteEffects.None;
                            startPos.Y += 1;
                            startPos.X += +17;
                            if (base.projectile.spriteDirection == -1)
                            {
                                spriteEffects = SpriteEffects.FlipHorizontally;
                            }
                            Main.spriteBatch.Draw(texture, startPos, rectangle, base.projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                        }
                    }
                }
                if (Sleep > 0 && Transform != 6 && Transform != 7)
                {
                    Texture2D texture = Main.projectileTexture[ModContent.ProjectileType<SariaSleep>()];
                    Vector2 startPos = base.projectile.Center - Main.screenPosition + new Vector2(0f, base.projectile.gfxOffY);
                    int frameHeight = texture.Height / Main.projFrames[base.projectile.type];
                    int frameY = frameHeight * base.projectile.frame;
                    Color drawColor = Color.Lerp(lightColor, Color.LightPink, 20f);
                    drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                    Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                    Vector2 origin = rectangle.Size() / 2f;
                    float rotation = base.projectile.rotation;
                    float scale = base.projectile.scale;
                    SpriteEffects spriteEffects = SpriteEffects.None;
                    startPos.Y += 1;
                    startPos.X += +17;
                    if (base.projectile.spriteDirection == -1)
                    {
                        spriteEffects = SpriteEffects.FlipHorizontally;
                    }
                    Main.spriteBatch.Draw(texture, startPos, rectangle, base.projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                }
                if (Sleep > 0 && Transform == 6)
                {
                    Texture2D texture = Main.projectileTexture[ModContent.ProjectileType<Saria7Sleep>()];
                    Vector2 startPos = base.projectile.Center - Main.screenPosition + new Vector2(0f, base.projectile.gfxOffY);
                    int frameHeight = texture.Height / Main.projFrames[base.projectile.type];
                    int frameY = frameHeight * base.projectile.frame;
                    Color drawColor = Color.Lerp(lightColor, Color.LightPink, 20f);
                    drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                    Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                    Vector2 origin = rectangle.Size() / 2f;
                    float rotation = base.projectile.rotation;
                    float scale = base.projectile.scale;
                    SpriteEffects spriteEffects = SpriteEffects.None;
                    startPos.Y += 1;
                    startPos.X += +17;
                    if (base.projectile.spriteDirection == -1)
                    {
                        spriteEffects = SpriteEffects.FlipHorizontally;
                    }
                    Main.spriteBatch.Draw(texture, startPos, rectangle, base.projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                }
                /////XP Bars
                if (XpTimer >= 1)
                {
                    if (modPlayer.XPBarLevel == 0)
                    {
                        Texture2D texture = Main.projectileTexture[ModContent.ProjectileType<SariaXPBar1>()];
                        Vector2 startPos = base.projectile.Center - Main.screenPosition + new Vector2(0f, base.projectile.gfxOffY);
                        int frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<SariaXPBar1>()];
                        int frameY = frameHeight;
                        Color drawColor = Color.Lerp(lightColor, Color.LightPink, 20f);
                        drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                        Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                        Vector2 origin = rectangle.Size() / 2f;
                        float rotation = base.projectile.rotation;
                        float scale = base.projectile.scale;
                        SpriteEffects spriteEffects = SpriteEffects.None;
                        startPos.Y += 60;
                        startPos.X += +17;
                        Main.spriteBatch.Draw(texture, startPos, null, base.projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                    }
                    if (modPlayer.XPBarLevel == 1)
                    {
                        Texture2D texture = Main.projectileTexture[ModContent.ProjectileType<SariaXPBar2>()];
                        Vector2 startPos = base.projectile.Center - Main.screenPosition + new Vector2(0f, base.projectile.gfxOffY);
                        int frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<SariaXPBar2>()];
                        int frameY = frameHeight;
                        Color drawColor = Color.Lerp(lightColor, Color.LightPink, 20f);
                        drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                        Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                        Vector2 origin = rectangle.Size() / 2f;
                        float rotation = base.projectile.rotation;
                        float scale = base.projectile.scale;
                        SpriteEffects spriteEffects = SpriteEffects.None;
                        startPos.Y += 60;
                        startPos.X += +17;
                        Main.spriteBatch.Draw(texture, startPos, null, base.projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                    }
                    if (modPlayer.XPBarLevel == 2)
                    {
                        Texture2D texture = Main.projectileTexture[ModContent.ProjectileType<SariaXPBar3>()];
                        Vector2 startPos = base.projectile.Center - Main.screenPosition + new Vector2(0f, base.projectile.gfxOffY);
                        int frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<SariaXPBar3>()];
                        int frameY = frameHeight;
                        Color drawColor = Color.Lerp(lightColor, Color.LightPink, 20f);
                        drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                        Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                        Vector2 origin = rectangle.Size() / 2f;
                        float rotation = base.projectile.rotation;
                        float scale = base.projectile.scale;
                        SpriteEffects spriteEffects = SpriteEffects.None;
                        startPos.Y += 60;
                        startPos.X += +17;
                        Main.spriteBatch.Draw(texture, startPos, null, base.projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                    }
                    if (modPlayer.XPBarLevel == 3)
                    {
                        Texture2D texture = Main.projectileTexture[ModContent.ProjectileType<SariaXPBar4>()];
                        Vector2 startPos = base.projectile.Center - Main.screenPosition + new Vector2(0f, base.projectile.gfxOffY);
                        int frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<SariaXPBar4>()];
                        int frameY = frameHeight;
                        Color drawColor = Color.Lerp(lightColor, Color.LightPink, 20f);
                        drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                        Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                        Vector2 origin = rectangle.Size() / 2f;
                        float rotation = base.projectile.rotation;
                        float scale = base.projectile.scale;
                        SpriteEffects spriteEffects = SpriteEffects.None;
                        startPos.Y += 60;
                        startPos.X += +17;
                        Main.spriteBatch.Draw(texture, startPos, null, base.projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                    }
                    if (modPlayer.XPBarLevel == 4)
                    {
                        Texture2D texture = Main.projectileTexture[ModContent.ProjectileType<SariaXPBar5>()];
                        Vector2 startPos = base.projectile.Center - Main.screenPosition + new Vector2(0f, base.projectile.gfxOffY);
                        int frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<SariaXPBar5>()];
                        int frameY = frameHeight;
                        Color drawColor = Color.Lerp(lightColor, Color.LightPink, 20f);
                        drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                        Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                        Vector2 origin = rectangle.Size() / 2f;
                        float rotation = base.projectile.rotation;
                        float scale = base.projectile.scale;
                        SpriteEffects spriteEffects = SpriteEffects.None;
                        startPos.Y += 60;
                        startPos.X += +17;
                        Main.spriteBatch.Draw(texture, startPos, null, base.projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                    }
                    if (modPlayer.XPBarLevel == 5)
                    {
                        Texture2D texture = Main.projectileTexture[ModContent.ProjectileType<SariaXPBar6>()];
                        Vector2 startPos = base.projectile.Center - Main.screenPosition + new Vector2(0f, base.projectile.gfxOffY);
                        int frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<SariaXPBar6>()];
                        int frameY = frameHeight;
                        Color drawColor = Color.Lerp(lightColor, Color.LightPink, 20f);
                        drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                        Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                        Vector2 origin = rectangle.Size() / 2f;
                        float rotation = base.projectile.rotation;
                        float scale = base.projectile.scale;
                        SpriteEffects spriteEffects = SpriteEffects.None;
                        startPos.Y += 60;
                        startPos.X += +17;
                        Main.spriteBatch.Draw(texture, startPos, null, base.projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                    }
                    if (modPlayer.XPBarLevel == 6)
                    {
                        Texture2D texture = Main.projectileTexture[ModContent.ProjectileType<SariaXPBar7>()];
                        Vector2 startPos = base.projectile.Center - Main.screenPosition + new Vector2(0f, base.projectile.gfxOffY);
                        int frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<SariaXPBar7>()];
                        int frameY = frameHeight;
                        Color drawColor = Color.Lerp(lightColor, Color.LightPink, 20f);
                        drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                        Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                        Vector2 origin = rectangle.Size() / 2f;
                        float rotation = base.projectile.rotation;
                        float scale = base.projectile.scale;
                        SpriteEffects spriteEffects = SpriteEffects.None;
                        startPos.Y += 60;
                        startPos.X += +17;
                        Main.spriteBatch.Draw(texture, startPos, null, base.projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                    }
                    if (modPlayer.XPBarLevel == 7)
                    {
                        Texture2D texture = Main.projectileTexture[ModContent.ProjectileType<SariaXPBar8>()];
                        Vector2 startPos = base.projectile.Center - Main.screenPosition + new Vector2(0f, base.projectile.gfxOffY);
                        int frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<SariaXPBar8>()];
                        int frameY = frameHeight;
                        Color drawColor = Color.Lerp(lightColor, Color.LightPink, 20f);
                        drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                        Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                        Vector2 origin = rectangle.Size() / 2f;
                        float rotation = base.projectile.rotation;
                        float scale = base.projectile.scale;
                        SpriteEffects spriteEffects = SpriteEffects.None;
                        startPos.Y += 60;
                        startPos.X += +17;
                        Main.spriteBatch.Draw(texture, startPos, null, base.projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                    }
                    if (modPlayer.XPBarLevel == 8)
                    {
                        Texture2D texture = Main.projectileTexture[ModContent.ProjectileType<SariaXPBar9>()];
                        Vector2 startPos = base.projectile.Center - Main.screenPosition + new Vector2(0f, base.projectile.gfxOffY);
                        int frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<SariaXPBar9>()];
                        int frameY = frameHeight;
                        Color drawColor = Color.Lerp(lightColor, Color.LightPink, 20f);
                        drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                        Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                        Vector2 origin = rectangle.Size() / 2f;
                        float rotation = base.projectile.rotation;
                        float scale = base.projectile.scale;
                        SpriteEffects spriteEffects = SpriteEffects.None;
                        startPos.Y += 60;
                        startPos.X += +17;
                        Main.spriteBatch.Draw(texture, startPos, null, base.projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                    }
                   if (modPlayer.Sarialevel == 0)
                    {
                        Texture2D texture = Main.projectileTexture[ModContent.ProjectileType<KingSlime>()];
                        Vector2 startPos = base.projectile.Center - Main.screenPosition + new Vector2(0f, base.projectile.gfxOffY);
                        int frameHeight = texture.Height;
                        int frameY = frameHeight;
                        Color drawColor = Color.Lerp(lightColor, Color.LightPink, 20f);
                        drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                        Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                        Vector2 origin = rectangle.Size() / 2f;
                        float rotation = base.projectile.rotation;
                        float scale = base.projectile.scale;
                        SpriteEffects spriteEffects = SpriteEffects.None;
                        startPos.Y += 60;
                        startPos.X += +60;
                        Main.spriteBatch.Draw(texture, startPos, null, base.projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                    }
                    if (modPlayer.Sarialevel == 1)
                    {
                        Texture2D texture = Main.projectileTexture[ModContent.ProjectileType<QueenBee>()];
                        Vector2 startPos = base.projectile.Center - Main.screenPosition + new Vector2(0f, base.projectile.gfxOffY);
                        int frameHeight = texture.Height;
                        int frameY = frameHeight;
                        Color drawColor = Color.Lerp(lightColor, Color.LightPink, 20f);
                        drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                        Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                        Vector2 origin = rectangle.Size() / 2f;
                        float rotation = base.projectile.rotation;
                        float scale = base.projectile.scale;
                        SpriteEffects spriteEffects = SpriteEffects.None;
                        startPos.Y += 60;
                        startPos.X += +60;
                        Main.spriteBatch.Draw(texture, startPos, null, base.projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                    }
                    if (modPlayer.Sarialevel == 2)
                    {
                        Texture2D texture = Main.projectileTexture[ModContent.ProjectileType<WallOfFlesh>()];
                        Vector2 startPos = base.projectile.Center - Main.screenPosition + new Vector2(0f, base.projectile.gfxOffY);
                        int frameHeight = texture.Height;
                        int frameY = frameHeight;
                        Color drawColor = Color.Lerp(lightColor, Color.LightPink, 20f);
                        drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                        Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                        Vector2 origin = rectangle.Size() / 2f;
                        float rotation = base.projectile.rotation;
                        float scale = base.projectile.scale;
                        SpriteEffects spriteEffects = SpriteEffects.None;
                        startPos.Y += 60;
                        startPos.X += +60;
                        Main.spriteBatch.Draw(texture, startPos, null, base.projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                    }
                    if (modPlayer.Sarialevel == 3)
                    {
                        Texture2D texture = Main.projectileTexture[ModContent.ProjectileType<Retinazer>()];
                        Vector2 startPos = base.projectile.Center - Main.screenPosition + new Vector2(0f, base.projectile.gfxOffY);
                        int frameHeight = texture.Height;
                        int frameY = frameHeight;
                        Color drawColor = Color.Lerp(lightColor, Color.LightPink, 20f);
                        drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                        Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                        Vector2 origin = rectangle.Size() / 2f;
                        float rotation = base.projectile.rotation;
                        float scale = base.projectile.scale;
                        SpriteEffects spriteEffects = SpriteEffects.None;
                        startPos.Y += 60;
                        startPos.X += +60;
                        Main.spriteBatch.Draw(texture, startPos, null, base.projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                    }
                    if (modPlayer.Sarialevel == 4)
                    {
                        Texture2D texture = Main.projectileTexture[ModContent.ProjectileType<Plantera>()];
                        Vector2 startPos = base.projectile.Center - Main.screenPosition + new Vector2(0f, base.projectile.gfxOffY);
                        int frameHeight = texture.Height;
                        int frameY = frameHeight;
                        Color drawColor = Color.Lerp(lightColor, Color.LightPink, 20f);
                        drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                        Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                        Vector2 origin = rectangle.Size() / 2f;
                        float rotation = base.projectile.rotation;
                        float scale = base.projectile.scale;
                        SpriteEffects spriteEffects = SpriteEffects.None;
                        startPos.Y += 60;
                        startPos.X += +60;
                        Main.spriteBatch.Draw(texture, startPos, null, base.projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                    }
                    if (modPlayer.Sarialevel == 5)
                    {
                        Texture2D texture = Main.projectileTexture[ModContent.ProjectileType<TheDuke>()];
                        Vector2 startPos = base.projectile.Center - Main.screenPosition + new Vector2(0f, base.projectile.gfxOffY);
                        int frameHeight = texture.Height;
                        int frameY = frameHeight;
                        Color drawColor = Color.Lerp(lightColor, Color.LightPink, 20f);
                        drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                        Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                        Vector2 origin = rectangle.Size() / 2f;
                        float rotation = base.projectile.rotation;
                        float scale = base.projectile.scale;
                        SpriteEffects spriteEffects = SpriteEffects.None;
                        startPos.Y += 60;
                        startPos.X += +60;
                        Main.spriteBatch.Draw(texture, startPos, null, base.projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                    }
                }
                return false;

            }



        }
    }



}