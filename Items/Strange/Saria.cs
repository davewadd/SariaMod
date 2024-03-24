using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SariaMod.Items.Topaz;
using SariaMod.Items.Emerald;
using SariaMod.Items.Amber;
using SariaMod.Items.Amethyst;
using SariaMod.Items.Ruby;
using SariaMod.Items;
using SariaMod.Items.zPearls;
using System;
using SariaMod.Items.Bands;
using SariaMod.Buffs;
using SariaMod.Dusts;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using SariaMod.Items.Strange;
using SariaMod.Items.Sapphire;
using Terraria.ModLoader;
using System.IO;

namespace SariaMod.Items.Strange
{


    public class Saria : ModProjectile
    {
       

        public const float DistanceToCheck = 1100f;
       private   int Transform;
        private   int BugTimer;
        private int SwarmTimer;
       private   int SicknessTimer;
       private   int Mood;
       private  int MoodTimer;
       private  int MoveTimer;
       private  int SleepHeal;
       private  int Sleep;
       private  int TimeAsleep;
       private  int XpTimer;
       private  int Cursed;
        private int ChannelTime;
        private int Eating;
        private int Holding;
        private int ToEat;

        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Mother");
            Main.projFrames[base.Projectile.type] = 99;
            Main.projPet[Projectile.type] = true;
             ProjectileID.Sets.MinionSacrificable[base.Projectile.type] = false;
            ProjectileID.Sets.MinionShot[base.Projectile.type] = false;
            ProjectileID.Sets.MinionTargettingFeature[base.Projectile.type] = true;
            ProjectileID.Sets.TrailingMode[base.Projectile.type] = 0;
            ProjectileID.Sets.TrailCacheLength[base.Projectile.type] = 30;
        }
        public override bool? CanCutTiles()
        {
            return false;
        }
        public override bool? CanHitNPC(NPC target)
        {
            return target.CanBeChasedBy(base.Projectile);
        }
        public override bool MinionContactDamage()
        {
            Player player = Main.player[base.Projectile.owner];
            FairyPlayer modPlayer = player.Fairy();
            NPC target = base.Projectile.Center.MinionHoming(500f, player);
            if (target != null && TimeAsleep <= 10)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(Transform);
            writer.Write(MoveTimer);
            writer.Write(TimeAsleep);
            writer.Write(Mood);
            writer.Write(Cursed);
            writer.Write(XpTimer);
            writer.Write(Sleep);
            writer.Write(SleepHeal);
            writer.Write(SicknessTimer);
            writer.Write(MoodTimer);
            writer.Write(BugTimer);
            writer.Write(SwarmTimer);
            writer.Write(ChannelTime);
            writer.Write(Eating);
            writer.Write(Holding);
            writer.Write(ToEat);
        }
        
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            Transform = (int)reader.ReadInt32();
            MoveTimer = (int)reader.ReadInt32();
            TimeAsleep = (int)reader.ReadInt32();
            Mood = (int)reader.ReadInt32();
            Cursed = (int)reader.ReadInt32();
            XpTimer = (int)reader.ReadInt32();
            Sleep = (int)reader.ReadInt32();
            SleepHeal = (int)reader.ReadInt32();
            SicknessTimer = (int)reader.ReadInt32();
            MoodTimer = (int)reader.ReadInt32();
            BugTimer = (int)reader.ReadInt32();
            SwarmTimer = (int)reader.ReadInt32();
            ChannelTime = (int)reader.ReadInt32();
            Eating = (int)reader.ReadInt32();
            Holding = (int)reader.ReadInt32();
            ToEat = (int)reader.ReadInt32();
        }
        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            float Timer = 40;
            Player player = Main.player[base.Projectile.owner];
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
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + 0, 0, 0, ProjectileID.SpiritHeal, 20, 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                target.buffImmune[ModContent.BuffType<Frostburn2>()] = false;
                target.AddBuff(ModContent.BuffType<Frostburn2>(), 200);
                
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
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.position.X + 10, target.position.Y + 2, 0, 0, ModContent.ProjectileType<ShadowClaw>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
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
           
            base.Projectile.width = 96;
            base.Projectile.height = 78;
            
            base.Projectile.netImportant = true;
            base.Projectile.friendly = true;
            Projectile.netUpdate = true;
            base.Projectile.ignoreWater = false;
            base.Projectile.usesLocalNPCImmunity = true;
             base.Projectile.localNPCHitCooldown = 50;
                base.Projectile.minionSlots = 0f;
            base.Projectile.timeLeft = 1800;
            base.Projectile.penetrate = -1;
            base.Projectile.tileCollide = false;
            base.Projectile.minion = false;
        }
        private const int sphereRadius3 = 1;
        private const int sphereRadius2 = 6;
        private const int sphereRadius4 = 32;
        private const int sphereRadius = 100;

        public override void AI()
        {
            
            { 
                Player player = Main.player[base.Projectile.owner];
            FairyPlayer modPlayer = player.Fairy();
               
                int owner = player.whoAmI;
            int GiantMoth = ModContent.ProjectileType<DuskBallProjectile>();
            for (int i = 0; i < 1000; i++)
            {
                if ((player.ownedProjectileCounts[ModContent.ProjectileType<GreenMothGoliath>()] <= 0f) && (player.ownedProjectileCounts[ModContent.ProjectileType<GreenMothGiant>()] <= 0f) && (player.ownedProjectileCounts[ModContent.ProjectileType<GreenMoth>()] <= 0f))
                {
                    if (Main.projectile[i].active && i != base.Projectile.whoAmI && ((Main.projectile[i].type == GiantMoth && Main.projectile[i].owner == owner && Main.projectile[i].timeLeft == 10)))
                    {

                        {
                            SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Pokeball"),Main.projectile[i].Center);
                            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Main.projectile[i].position.X + 0, Main.projectile[i].position.Y + 0, 0, 0, ModContent.ProjectileType<GreenMothGoliath2>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                        }

                    }

                }
            }
            ///Channeling
            if (player.channel == true && player.HeldItem.type == ModContent.ItemType<HealBall>())
                {
                    ChannelTime++;
                    Projectile.netUpdate = true;
                }
            
            int XpProjectile = ModContent.ProjectileType<XpProjectile>();
            for (int i = 0; i < 1000; i++)
            {
                if (Main.projectile[i].active && i != base.Projectile.whoAmI && ((Main.projectile[i].type == XpProjectile && Main.projectile[i].owner == owner)))
                {

                    {
                        modPlayer.SariaXp += 100;
                            Projectile.netUpdate = true;
                            Main.projectile[i].Kill();
                    }

                }

            }
            int XpProjectile2 = ModContent.ProjectileType<XpProjectile2>();
            for (int i = 0; i < 1000; i++)
            {
                if (Main.projectile[i].active && i != base.Projectile.whoAmI && ((Main.projectile[i].type == XpProjectile2 && Main.projectile[i].owner == owner)))
                {

                    {
                        modPlayer.SariaXp += 500;
                            Projectile.netUpdate = true;
                            Main.projectile[i].Kill();
                    }

                }

            }
            int XpProjectile3 = ModContent.ProjectileType<XpProjectile3>();
            for (int i = 0; i < 1000; i++)
            {
                if (Main.projectile[i].active && i != base.Projectile.whoAmI && ((Main.projectile[i].type == XpProjectile3 && Main.projectile[i].owner == owner)))
                {

                    {
                        modPlayer.SariaXp += 2500;
                            Projectile.netUpdate = true;
                            Main.projectile[i].Kill();
                    }

                }

            }
            int XpProjectile4 = ModContent.ProjectileType<XpProjectile4>();
            for (int i = 0; i < 1000; i++)
            {
                if (Main.projectile[i].active && i != base.Projectile.whoAmI && ((Main.projectile[i].type == XpProjectile4 && Main.projectile[i].owner == owner)))
                {

                    {
                        modPlayer.SariaXp += 12500;
                            Projectile.netUpdate = true;
                            Main.projectile[i].Kill();
                    }

                }

            }
            int Lightning = ModContent.ProjectileType<LightningLocator>();
            for (int i = 0; i < 1000; i++)
            {
                if (Transform != 3)
                { 
                    if (Main.projectile[i].active && i != base.Projectile.whoAmI && ((Main.projectile[i].type == Lightning && Main.projectile[i].owner == owner)))
                    {

                        {
                                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Main.projectile[i].position.X + 16, Main.projectile[i].position.Y + 16, 0, 0, ModContent.ProjectileType<LightningLocator2>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                                Main.projectile[i].Kill();
                        }

                    }

                }
            }
                int Static2 = ModContent.ProjectileType<Static2>();
                for (int i = 0; i < 1000; i++)
                {
                    if (Transform != 3)
                    {
                        if (Main.projectile[i].active && i != base.Projectile.whoAmI && ((Main.projectile[i].type == Static2 && Main.projectile[i].owner == owner)))
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
                
                        Projectile.knockBack = 10;
            if (modPlayer.Sarialevel == 6)
            {
                Projectile.damage = 900 + (modPlayer.SariaXp / 40);
                    Projectile.netUpdate = true;
                }
            else if (modPlayer.Sarialevel == 5)
            {
                Projectile.damage = 200 + (modPlayer.SariaXp / 342);
                    Projectile.netUpdate = true;
                }
            else if (modPlayer.Sarialevel == 4)
            {
                Projectile.damage = 75 + (modPlayer.SariaXp / 640);
                    Projectile.netUpdate = true;
                }
            else if (modPlayer.Sarialevel == 3)
            {
                Projectile.damage = 50 + (modPlayer.SariaXp / 1600);
                    Projectile.netUpdate = true;
                }
            else if (modPlayer.Sarialevel == 2)
            {
                Projectile.damage = 26 + (modPlayer.SariaXp / 833);
                    Projectile.netUpdate = true;
                }

            else if (modPlayer.Sarialevel == 1)
            {
                Projectile.damage = 15 + (modPlayer.SariaXp / 818);
                    Projectile.netUpdate = true;
                }
            else
            {
                Projectile.damage = 10 + (modPlayer.SariaXp/600);
                    Projectile.netUpdate = true;
                }
                if (player.HasBuff(ModContent.BuffType<XPBuff>()))
                {
                XpTimer = 2;
                    Projectile.netUpdate = true;

                }
                if (player.ownedProjectileCounts[ModContent.ProjectileType<Transform>()] > 0f && !player.channel)
                {
                    


                    int VeilBubble = ModContent.ProjectileType<Transform>();
                    for (int i = 0; i < 1000; i++)
                    {
                        if (Main.projectile[i].active && i != base.Projectile.whoAmI && !player.channel && ((Main.projectile[i].type == VeilBubble && Main.projectile[i].owner == owner)))
                        {
                            ///channeltime > 0 is suspiscious
                            if (ChannelTime > 0 && ChannelTime <= 20)
                            {
                                Main.projectile[i].Kill();
                                Transform++;
                                Projectile.netUpdate =  true;
                                ChannelTime = 0;
                                Main.NewText(Transform);
                            }
                            if (ChannelTime > 20)
                            {
                                
                                ChannelTime = 0;
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

                BugTimer--;
            {
                if (BugTimer <= 0)
                {
                    BugTimer = 500;
                        Projectile.netUpdate = true;
                    }
            }
            if (SwarmTimer < 1200)
            {
                SwarmTimer++;
                    Projectile.netUpdate = true;
                }
            if (XpTimer >= 1)
            {
                XpTimer--;
                    Projectile.netUpdate = true;
                }
            SicknessTimer++;
            {
                if (SicknessTimer >= 180000 && !player.HasBuff(ModContent.BuffType<Soothing>()))
                {
                    player.AddBuff(ModContent.BuffType<Sickness>(), 30000);
                    SicknessTimer = 0;
                        Projectile.netUpdate = true;
                    }
                if (player.HasBuff(ModContent.BuffType<Soothing>()))
                {
                    SicknessTimer = 0;
                        Projectile.netUpdate = true;
                    }
            }
            if (Mood < 0 && MoodTimer >= Timer)
            {
                Mood++;
                    Projectile.netUpdate = true;
                }
            if (Mood > 0 && MoodTimer >= Timer)
            {
                Mood--;
                    Projectile.netUpdate = true;
                }
            if ((Math.Abs(Projectile.velocity.X) >= 0.5f) || (Math.Abs(Projectile.velocity.Y) >= 0.5f))
            {
                MoveTimer = 0;
                    Projectile.netUpdate = true;
                }
            if ((Math.Abs(Projectile.velocity.X) < 0.5f) && (Math.Abs(Projectile.velocity.Y) < 0.5f))
            {
                if (MoveTimer < 8000)
                {
                    MoveTimer+= 1;
                        Projectile.netUpdate = true;
                    }
                if (Mood < 0)
                {
                    MoveTimer+=4;
                        Projectile.netUpdate = true;
                    }
            }
            if (MoveTimer == 0)
            {
                TimeAsleep = 0;
                SleepHeal = 0;
                    Projectile.netUpdate = true;
                }
            if (TimeAsleep >= 200 && SleepHeal<= 0)
            {
                SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Healpulse"), player.Center);
                player.AddBuff(ModContent.BuffType<Soothing>(), 44000);
                Mood = 0;
                SleepHeal = 1;
                if (player.HasBuff(ModContent.BuffType<Drained>()))
                {
                    player.ClearBuff(ModContent.BuffType<Drained>());
                }
                    Projectile.netUpdate = true;

                }
            if (player.HasBuff(ModContent.BuffType<BloodmoonBuff>()) || player.HasBuff(ModContent.BuffType<EclipseBuff>()))
            {
                Cursed = 1;
                    Projectile.netUpdate = true;
                }
            else
            {
                Cursed = 0;
                    Projectile.netUpdate = true;
                }
            if (TimeAsleep >= 500)
            {
                if (player.HasBuff(ModContent.BuffType<Drained>()))
                {
                    player.ClearBuff(ModContent.BuffType<Drained>());
                }
                if (MoveTimer >= 500)
                {
                    player.AddBuff(ModContent.BuffType<Overcharged>(), 30000);
                    SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/StatRaise"), Projectile.Center);
                    for (int j = 0; j < 1; j++) //set to 2
                    {
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + 0, 0, 0, ModContent.ProjectileType<PowerUp>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                    }
                }
                Mood = 0;
                MoveTimer = 0;
                    Projectile.netUpdate = true;
                }
            
                if (Mood >= 1200)
            {
                SicknessTimer--;
                    Projectile.netUpdate = true;
                }
            if (Mood >= 2400)
            {
                SicknessTimer--;
                    Projectile.netUpdate = true;
                }
            if (Mood <= -1200)
            {
                SicknessTimer++;
                    Projectile.netUpdate = true;
                }
            if (Mood <= -2400)
            {
                SicknessTimer++;
                    Projectile.netUpdate = true;
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
                    Dust.NewDust(new Vector2(Projectile.Center.X + radius * (float)Math.Cos(angle), (Projectile.Center.Y - 10) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<FlameDustSaria>(), 0f, 0f, 0, default(Color), 1.5f);
                }
            }
            if (Transform == 6)
            {
                if (Main.rand.NextBool(30))//controls the speed of when the sparkles spawn
                {
                    float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius * sphereRadius));
                    double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                    Dust.NewDust(new Vector2(Projectile.Center.X + radius * (float)Math.Cos(angle), (Projectile.Center.Y - 10) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<ShadowFlameDust>(), 0f, 0f, 0, default(Color), 1.5f);
                }
            }
            //////////////////////////////faces start
            Vector2 idlePosition2 = player.Center;
            float minionPositionOffsetX2 = ((60 + Projectile.minionPos / 80) * player.direction) - 15;
            idlePosition2.Y -= 15f;
            idlePosition2.X += minionPositionOffsetX2;
            Vector2 vectorToIdlePosition3 = idlePosition2 - Projectile.Center;
            float distanceToIdlePosition3 = vectorToIdlePosition3.Length();
            if ((player.ownedProjectileCounts[ModContent.ProjectileType<FrozenYogurtSignal>()] >= 1f) && (player.ownedProjectileCounts[ModContent.ProjectileType<Happiness>()] <= 0f) && ((player.ownedProjectileCounts[ModContent.ProjectileType<Sad>()] > 0f) || (!player.HasBuff(ModContent.BuffType<BloodmoonBuff>()))))
            {
                Mood += 600;
                    Projectile.netUpdate = true;
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<Happiness>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
            }
            
            if (Sleep <= 0 && player.statLife == player.statLifeMax2 && (Projectile.frame >= 20 && Projectile.frame <= 60 && Projectile.ai[0] == 0 && (player.ownedProjectileCounts[ModContent.ProjectileType<SmileTime>()] <= 0f) && (!player.HasBuff(ModContent.BuffType<Sickness>()) && (!player.HasBuff(ModContent.BuffType<BloodmoonBuff>()) && (!player.HasBuff(ModContent.BuffType<StatLower>()) && (player.ownedProjectileCounts[ModContent.ProjectileType<Happiness>()] <= 0f) && (player.ownedProjectileCounts[ModContent.ProjectileType<Anger>()] <= 0f) && (player.ownedProjectileCounts[ModContent.ProjectileType<Sad>()] <= 0f) && (player.ownedProjectileCounts[ModContent.ProjectileType<Smile>()] <= 0f) && player.velocity.X == 0)) && Projectile.spriteDirection != player.direction && (distanceToIdlePosition3 <= 10))))
            {
                float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius3 * sphereRadius3));
                double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                Mood += 600;
                    Projectile.netUpdate = true;
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<SmileTime>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<Smile>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                Dust.NewDust(new Vector2((Projectile.Center.X + dustspeed) + radius * (float)Math.Cos(angle), (Projectile.Center.Y - 10) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<HeartDust>(), 0f, 0f, 0, default(Color), 1.5f);
            }
            if ((player.ownedProjectileCounts[ModContent.ProjectileType<Smile>()] >= 1f) && (player.ownedProjectileCounts[ModContent.ProjectileType<Anger>()] <= 0f) && Projectile.spriteDirection == player.direction)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<Anger>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
            }
            if ((player.HasBuff(ModContent.BuffType<Sickness>())))
            {
                SicknessTimer = 0;
                Mood = -4800;
                    Projectile.netUpdate = true;
                    if (player.ownedProjectileCounts[ModContent.ProjectileType<Sad>()] <= 0f && (player.ownedProjectileCounts[ModContent.ProjectileType<period>()] <= 0f))
                {
                    {
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<Sad>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<period>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                    }
                }
            }
            if ((player.HasBuff(ModContent.BuffType<Soothing>())))
            {
                if (Mood <= 200)
                {
                    Mood++;
                    Mood++;
                        Projectile.netUpdate = true;
                    }
            }
            
            if (player.ownedProjectileCounts[ModContent.ProjectileType<Competitivetime>()] == 1f && player.ownedProjectileCounts[ModContent.ProjectileType<Competitive>()] <= 0f)
            {
                {
                    SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/StatRaise"), Projectile.Center);
                    for (int j = 0; j < 1; j++) //set to 2
                    {
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<PowerUp>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                    }
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<Competitive>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                    Mood = 3700;
                        Projectile.netUpdate = true;
                    }
            }
            //////////////////////////////faces end

            if (Projectile.frame == 62 && (Sleep <= 0) && (!player.HasBuff(ModContent.BuffType<StatLower>())) && (!player.HasBuff(ModContent.BuffType<Sickness>()) && (!player.HasBuff(ModContent.BuffType<BloodmoonBuff>())) && (!player.HasBuff(ModContent.BuffType<EclipseBuff>()))))
            {
                if (Main.rand.NextBool())//controls the speed of when the sparkles spawn
                {
                    float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius3 * sphereRadius3));
                    double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                    if (Projectile.spriteDirection > 0)
                    {
                        sneezespot = 18;
                    }
                    if (Projectile.spriteDirection < 0)
                    {
                        sneezespot = 3;
                    }
                    Dust.NewDust(new Vector2((Projectile.Center.X + sneezespot) + radius * (float)Math.Cos(angle), (Projectile.Center.Y - 10) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<Sneeze>(), 0f, 0f, 0, default(Color), 1.5f);
                }
            }
            if (Projectile.frame == 62 && (Sleep <= 0) && ((player.HasBuff(ModContent.BuffType<StatLower>())) || (player.HasBuff(ModContent.BuffType<Sickness>()) || (player.HasBuff(ModContent.BuffType<BloodmoonBuff>()) || (player.HasBuff(ModContent.BuffType<EclipseBuff>()))))))
            {
                if (Main.rand.NextBool())//controls the speed of when the sparkles spawn
                {
                    float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius3 * sphereRadius3));
                    double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                    if (Projectile.spriteDirection > 0)
                    {
                        sneezespot = 18;
                    }
                    if (Projectile.spriteDirection < 0)
                    {
                        sneezespot = 3;
                    }
                    Dust.NewDust(new Vector2((Projectile.Center.X + sneezespot) + radius * (float)Math.Cos(angle), (Projectile.Center.Y - 10) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<Blood>(), 0f, 0f, 0, default(Color), 1.5f);
                }
                if (Main.rand.NextBool(30))//controls the speed of when the sparkles spawn
                {
                    float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius3 * sphereRadius3));
                    double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                    if (Projectile.spriteDirection > 0)
                    {
                        sneezespot = 18;
                    }
                    if (Projectile.spriteDirection < 0)
                    {
                        sneezespot = 3;
                    }
                    Dust.NewDust(new Vector2((Projectile.Center.X + sneezespot) + radius * (float)Math.Cos(angle), (Projectile.Center.Y - 16) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<Blood>(), 0f, 0f, 0, default(Color), 1.5f);
                }
            }
            if ((Main.player[Main.myPlayer].active && Main.bloodMoon) && ((!player.HasBuff(ModContent.BuffType<Soothing>()))))
            {
                player.AddBuff(ModContent.BuffType<BloodmoonBuff>(), 20);
                if (Main.rand.NextBool(30))//controls the speed of when the sparkles spawn
                {
                    float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius3 * sphereRadius3));
                    double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                    if (Projectile.spriteDirection > 0)
                    {
                        sneezespot = 18;
                    }
                    if (Projectile.spriteDirection < 0)
                    {
                        sneezespot = 3;
                    }
                    Dust.NewDust(new Vector2((Projectile.Center.X + sneezespot) + radius * (float)Math.Cos(angle), (Projectile.Center.Y - 10) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<Blood>(), 0f, 0f, 0, default(Color), 1.5f);
                }
                if (Main.rand.NextBool(20))//controls the speed of when the sparkles spawn
                {
                    float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius2 * sphereRadius2));
                    double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                    if (Projectile.spriteDirection > 0)
                    {
                        sneezespot = 18;
                    }
                    if (Projectile.spriteDirection < 0)
                    {
                        sneezespot = 3;
                    }
                    Dust.NewDust(new Vector2((Projectile.Center.X + sneezespot) + radius * (float)Math.Cos(angle), (Projectile.Center.Y - 10) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<BlackSmoke>(), 0f, 0f, 0, default(Color), 1.5f);
                }
            }
            if (Projectile.frame >= 76)
            {
                dustspeed = 5;
            }
            if (Main.rand.NextBool((int)dustspeed))//controls the speed of when the sparkles spawn
            {
                float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius2 * sphereRadius2));
                double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                if (Projectile.spriteDirection > 0)
                {
                    dustspot = 18;
                }
                if (Projectile.spriteDirection < 0)
                {
                    dustspot = 3;
                }
                Dust.NewDust(new Vector2((Projectile.Center.X + dustspot) + radius * (float)Math.Cos(angle), (Projectile.Center.Y + 34) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<Psychic2>(), 0f, 0f, 0, default(Color), 1.5f);
            }
            if (((Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneSnow) && !(Main.player[Main.myPlayer].behindBackWall && player.HasBuff((BuffID.Campfire)))) || (Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneSkyHeight) && !(Main.player[Main.myPlayer].behindBackWall && player.HasBuff((BuffID.Campfire))) || (Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneDesert && !Main.dayTime) && !(Main.player[Main.myPlayer].behindBackWall && player.HasBuff((BuffID.Campfire))) || (Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneRain && !Main.player[Main.myPlayer].ZoneJungle && !(Main.player[Main.myPlayer].ZoneDesert && Main.dayTime)) && !(Main.player[Main.myPlayer].behindBackWall && player.HasBuff((BuffID.Campfire))))
            {
                if (Projectile.velocity.X <= 1)
                {
                    if (Main.rand.NextBool(50))
                    {
                        float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius3 * sphereRadius3));
                        double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                        if (Projectile.spriteDirection > 0)
                        {
                            sneezespot = 25;
                        }
                        if (Projectile.spriteDirection < 0)
                        {
                            sneezespot = -2;
                        }
                        for (int j = 0; j < 2; j++)
                        {
                            Dust.NewDust(new Vector2((Projectile.Center.X + sneezespot) + radius * (float)Math.Cos(angle), (Projectile.Center.Y - 10) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<Fog>(), 0f, 0f, 0, default(Color), 1.5f);

                        }
                    }
                }
                else if (Projectile.velocity.X > 1)
                {
                    if (Main.rand.NextBool(10))
                    {
                        float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius3 * sphereRadius3));
                        double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                        if (Projectile.spriteDirection > 0)
                        {
                            sneezespot = 25;
                        }
                        if (Projectile.spriteDirection < 0)
                        {
                            sneezespot = -2;
                        }
                        for (int j = 0; j < 2; j++)
                        {
                            Dust.NewDust(new Vector2((Projectile.Center.X + sneezespot) + radius * (float)Math.Cos(angle), (Projectile.Center.Y - 10) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<Fog>(), 0f, 0f, 0, default(Color), 1.5f);

                        }
                    }
                }

            }//end of dust stuff
          
            if (Projectile.localAI[0] == 0f)
            {
               
                Projectile.Fairy().spawnedPlayerMinionProjectileDamageValue = Projectile.damage;
                for (int j = 0; j < 1; j++) //set to 2
                {
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<PowerUp>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<Ztarget>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);

                }
                Projectile.localAI[0] = 1f;
            }

           
            if (player.dead)
            {
                modPlayer.SariaXp /= 2;
            }
            
            if (player.dead || !player.active)
            {
                for (int j = 0; j < 72; j++)
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.Center, 113);
                    dust.velocity = ((float)Math.PI * 2f * Vector2.Dot(((float)j / 72f * ((float)Math.PI * 2f)).ToRotationVector2(), player.velocity.SafeNormalize(Vector2.UnitY).RotatedBy((float)j / 72f * ((float)Math.PI * -2f)))).ToRotationVector2();
                    dust.velocity = dust.velocity.RotatedBy((float)j / 36f * ((float)Math.PI * 2f)) * 8f;
                    dust.noGravity = true;
                    dust.scale *= 3.9f;
                }
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<HealBallProjectile2>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);

                
                Projectile.Kill();
            }
           
            if (player.HasBuff(ModContent.BuffType<SariaBuff>()))
            {
                Projectile.timeLeft = 2;
            }
            if (!player.HasBuff(ModContent.BuffType<SariaBuff>()) && Projectile.timeLeft <= 3)
            {
                for (int j = 0; j < 72; j++)
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.Center, 113);
                    dust.velocity = ((float)Math.PI * 2f * Vector2.Dot(((float)j / 72f * ((float)Math.PI * 2f)).ToRotationVector2(), player.velocity.SafeNormalize(Vector2.UnitY).RotatedBy((float)j / 72f * ((float)Math.PI * -2f)))).ToRotationVector2();
                    dust.velocity = dust.velocity.RotatedBy((float)j / 36f * ((float)Math.PI * 2f)) * 8f;
                    dust.noGravity = true;
                    dust.scale *= 3.9f;
                    
                }
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 60, Projectile.position.Y + 30, 0, 0, ModContent.ProjectileType<HealBallProjectile2>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                
                Projectile.Kill();
            }
           /// Ai Stuff
            NPC target = base.Projectile.Center.MinionHoming(500f, player);// the distance she targets enemies
            if (target != null && Projectile.ai[0] == 0 && ChannelTime <= 20 && Transform != 1 && Transform != 4 && Transform != 3 && Sleep <= 0 && ToEat <= 0 && Eating <= 0 && player.HeldItem.type == ModContent.ItemType<HealBall>())
            {
                if (Transform != 5)
                {
                    Projectile.ai[0] = 1;
                }
                else if (Transform == 5 && SwarmTimer >= 1000)
                {
                    Projectile.ai[0] = 1;
                }

            }
                else if (target != null && Projectile.ai[0] == 0 && ChannelTime <= 20 && Transform != 1 && Transform != 4 && Transform == 3 && player.ownedProjectileCounts[ModContent.ProjectileType<Static2>()] < 3f && !player.HasMinionAttackTargetNPC && Sleep <= 0 && ToEat <= 0 && Eating <= 0 && player.HeldItem.type == ModContent.ItemType<HealBall>())
            {
                    Projectile.ai[0] = 1;
                }
                else if (target != null && Projectile.ai[0] == 0 && ChannelTime <= 20 && Transform != 1 && Transform != 4 && Transform == 3 && player.HasMinionAttackTargetNPC && Sleep <= 0 && ToEat <= 0 && Eating <= 0 && player.HeldItem.type == ModContent.ItemType<HealBall>())
                {
                    Projectile.ai[0] = 1;
                }
            ///transform emerald
                else if (target != null && Projectile.ai[0] == 0 && ChannelTime <= 20 && Transform != 1 && Transform == 4 && Transform != 3 && !player.HasMinionAttackTargetNPC && Sleep <= 0 && ToEat <= 0 && Eating <= 0 && player.ownedProjectileCounts[ModContent.ProjectileType<Specialrupee>()] <= 0f && player.ownedProjectileCounts[ModContent.ProjectileType<Rupee>()] <= 0f && player.HeldItem.type == ModContent.ItemType<HealBall>())
                {
                    Projectile.ai[0] = 1;
                }
                else if (target != null && Projectile.ai[0] == 0 && ChannelTime <= 20 && Transform != 1 && Transform == 4 && Transform != 3 && player.HasMinionAttackTargetNPC && Sleep <= 0 && ToEat <= 0 && Eating <= 0 && player.HeldItem.type == ModContent.ItemType<HealBall>())
                {
                    Projectile.ai[0] = 1;
                }
          
                else if (target != null && Projectile.ai[0] == 0 && ChannelTime <= 20 && Transform == 1 && Transform != 4 && Transform != 3 && player.ownedProjectileCounts[ModContent.ProjectileType<Bubble2>()] < 1f && player.ownedProjectileCounts[ModContent.ProjectileType<Bubble>()] < 1f && Sleep <= 0 && ToEat <=0 && Eating <= 0 && player.HeldItem.type == ModContent.ItemType<HealBall>())
            {
                Projectile.ai[0] = 1;
            }
               
                if (target != null && Projectile.ai[0] == 0 && ChannelTime <= 20 && Transform == 1 && Transform != 3 && player.HasMinionAttackTargetNPC &&  player.ownedProjectileCounts[ModContent.ProjectileType<Bubble2>()] >= 1f && player.ownedProjectileCounts[ModContent.ProjectileType<Bubble>()] <= 0f && Sleep <= 0 && ToEat <= 0 && Eating <= 0 && player.HeldItem.type == ModContent.ItemType<HealBall>())
                {
                    Projectile.ai[0] = 1;
                }
                
                    if (Transform == 6 && player.ownedProjectileCounts[ModContent.ProjectileType<CoolDown>()] <= 0f)
            {
                for (int b = 0; b < Main.maxNPCs; b++)
                {
                    NPC npc = Main.npc[b];
                    float between2 = Vector2.Distance(npc.Center, Projectile.Center);
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
                                            Projectile.NewProjectile(Projectile.GetSource_FromThis(), npc.position.X + 0, npc.position.Y + -24, 0, 0, ModContent.ProjectileType<ShadowClaw>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                                                Projectile.netUpdate = true;
                                            }
                                }
                            }
                        }

                    }
                }
            }
                    /////end
            //Flashupdate stuff
            for (int i = 0; i < 1000; i++)
            {

                float between = Vector2.Distance(Main.projectile[i].Center, player.Center);
                if (between <= 100)
                {


                    if (Main.projectile[i].active && i != base.Projectile.whoAmI && ((!Main.projectile[i].friendly && Main.projectile[i].hostile) || (Main.projectile[i].trap)))
                    {
                        if ((!player.HasBuff(ModContent.BuffType<Sickness>()) && (!player.HasBuff(ModContent.BuffType<BloodmoonBuff>()) && (!player.HasBuff(ModContent.BuffType<EclipseBuff>()) && (player.ownedProjectileCounts[ModContent.ProjectileType<Sad>()] <= 0f) && (player.ownedProjectileCounts[ModContent.ProjectileType<Flash>()] <= 0f) && (player.ownedProjectileCounts[ModContent.ProjectileType<FlashCooldown>()] <= 0f)))))
                        {
                            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<Flash>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                            SoundEngine.PlaySound(SoundID.Item76, base.Projectile.Center);

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
                    if (Projectile.spriteDirection > 0)
                    {
                        sneezespot = 18;
                    }
                    if (Projectile.spriteDirection < 0)
                    {
                        sneezespot = 3;
                    }
                    Dust.NewDust(new Vector2((Projectile.Center.X + sneezespot) + radius * (float)Math.Cos(angle), (Projectile.Center.Y - 10) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<Psychic>(), 0f, 0f, 0, default(Color), 1.5f);
                }
            }
                if (player.ownedProjectileCounts[ModContent.ProjectileType<CoolDown>()] > 0f)
                {
                    if (Main.rand.NextBool(20))//controls the speed of when the sparkles spawn
                    {
                        float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius2 * sphereRadius2));
                        double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                        if (Projectile.spriteDirection > 0)
                        {
                            sneezespot = 18;
                        }
                        if (Projectile.spriteDirection < 0)
                        {
                            sneezespot = 3;
                        }
                        Dust.NewDust(new Vector2((Projectile.Center.X + sneezespot) + radius * (float)Math.Cos(angle), (Projectile.Center.Y - 10) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<Psychic>(), 0f, 0f, 0, default(Color), 1.5f);
                    }
                }
                    if ((Main.player[Main.myPlayer].active && Main.eclipse) && ((!player.HasBuff(ModContent.BuffType<Soothing>()))))
            {
                player.AddBuff(ModContent.BuffType<EclipseBuff>(), 20);
                if (Main.rand.NextBool(30))//controls the speed of when the sparkles spawn
                {
                    float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius3 * sphereRadius3));
                    double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                    if (Projectile.spriteDirection > 0)
                    {
                        sneezespot = 18;
                    }
                    if (Projectile.spriteDirection < 0)
                    {
                        sneezespot = 3;
                    }
                    Dust.NewDust(new Vector2((Projectile.Center.X + sneezespot) + radius * (float)Math.Cos(angle), (Projectile.Center.Y - 10) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<Blood>(), 0f, 0f, 0, default(Color), 1.5f);
                }
                if (Main.rand.NextBool(20))//controls the speed of when the sparkles spawn
                {
                    float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius2 * sphereRadius2));
                    double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                    if (Projectile.spriteDirection > 0)
                    {
                        sneezespot = 18;
                    }
                    if (Projectile.spriteDirection < 0)
                    {
                        sneezespot = 3;
                    }
                    Dust.NewDust(new Vector2((Projectile.Center.X + sneezespot) + radius * (float)Math.Cos(angle), (Projectile.Center.Y - 10) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<BlackSmoke>(), 0f, 0f, 0, default(Color), 1.5f);
                }
                if (player.ownedProjectileCounts[ModContent.ProjectileType<period>()] <= 0f && Sleep <= 0)
                {
                    Mood = -3600;
                        Projectile.netUpdate = true;
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<Anger>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<period>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                }
            }
            if (player.HasBuff(ModContent.BuffType<BloodmoonBuff>()) && (player.ownedProjectileCounts[ModContent.ProjectileType<period>()] <= 0f) && Sleep <= 0)

            {
                Mood = -3600;
                    Projectile.netUpdate = true;
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<Anger>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<period>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
            }
            if (player.HasBuff(ModContent.BuffType<StatLower>()) && (player.ownedProjectileCounts[ModContent.ProjectileType<period>()] <= 0f) && Sleep <= 0 && Cursed <= 0)

            {
                Mood -= 600;
                    Projectile.netUpdate = true;
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<Sad2>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<period>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
            }
            if (player.HasBuff(ModContent.BuffType<StatRaise>()) && (player.ownedProjectileCounts[ModContent.ProjectileType<period2>()] <= 0f) && Sleep <= 0 && Cursed <= 0)

            {
                Mood += 600;
                    Projectile.netUpdate = true;
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<Smile2>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<period2>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
            }


            //end of Flashupdate stuff

           
            if (Projectile.frame >= 84 && Projectile.frame <= 95 && Transform == 1)
            {
                if (Main.rand.NextBool(8))//controls the speed of when the sparkles spawn
                {
                    float radius = (float)Math.Sqrt(Main.rand.Next(34 * 34));
                    double angle = Main.rand.NextDouble() * 5.0 * Math.PI;


                    {
                        Dust.NewDust(new Vector2(Projectile.Center.X + radius * (float)Math.Cos(angle), Projectile.Center.Y + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<BubbleDust>(), 0f, 0f, 0, default(Color), 1.5f);
                    }
                }
            }
            if (Projectile.frame >= 84 && Projectile.frame <= 95 && Transform == 2)
            {
                if (Main.rand.NextBool(8))//controls the speed of when the sparkles spawn
                {
                    float radius = (float)Math.Sqrt(Main.rand.Next(34 * 34));
                    double angle = Main.rand.NextDouble() * 5.0 * Math.PI;


                    Dust.NewDust(new Vector2(Projectile.Center.X + radius * (float)Math.Cos(angle), Projectile.Center.Y + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<FlameDust>(), 0f, 0f, 0, default(Color), 1.5f);
                }
            }
            if (Projectile.frame >= 84 && Projectile.frame <= 95 && Transform == 3)
            {
                if (Main.rand.NextBool())//controls the speed of when the sparkles spawn
                {
                    float radius = (float)Math.Sqrt(Main.rand.Next(34 * 34));
                    double angle = Main.rand.NextDouble() * 5.0 * Math.PI;


                    Dust.NewDust(new Vector2(Projectile.Center.X + radius * (float)Math.Cos(angle), Projectile.Center.Y + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<StaticDust>(), 0f, 0f, 0, default(Color), 1.5f);
                }
            }
            if (Projectile.frame >= 84 && Projectile.frame <= 95 && Transform == 6)
            {
                if (Main.rand.NextBool())//controls the speed of when the sparkles spawn
                {
                    float radius = (float)Math.Sqrt(Main.rand.Next(34 * 34));
                    double angle = Main.rand.NextDouble() * 5.0 * Math.PI;


                    Dust.NewDust(new Vector2(Projectile.Center.X + radius * (float)Math.Cos(angle), Projectile.Center.Y + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<ShadowFlameDust>(), 0f, 0f, 0, default(Color), 1.5f);
                }
            }

                //Statraise and lower
                if (player.ownedProjectileCounts[ModContent.ProjectileType<CoolDown>()] <= 0f)
                
                    {
                    if (Transform == 0)
                    {


                        if ((Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneCrimson || Main.player[Main.myPlayer].ZoneCorrupt))
                        {
                            if (!player.HasBuff(ModContent.BuffType<StatLower>()))
                            {
                                SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/StatLower"), Projectile.Center);

                                for (int j = 0; j < 1; j++) //set to 2
                                {
                                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<PowerDown>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
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
                                SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/StatRaise"), Projectile.Center);
                                for (int j = 0; j < 1; j++) //set to 2
                                {
                                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<PowerUp>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                                }
                                player.AddBuff(ModContent.BuffType<StatRaise>(), 20);
                            }
                            if (player.HasBuff(ModContent.BuffType<StatRaise>()))
                            {
                                player.AddBuff(ModContent.BuffType<StatRaise>(), 20);
                            }
                        }
                        Projectile.netUpdate = true;
                    }
                    if (Transform == 1)
                    {
                        if ((Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneDesert || Main.player[Main.myPlayer].ZoneJungle) || Main.player[Main.myPlayer].ZoneGlowshroom)
                        {
                            if (!player.HasBuff(ModContent.BuffType<StatLower>()))
                            {
                                SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/StatLower"), Projectile.Center);
                                for (int j = 0; j < 1; j++) //set to 2
                                {
                                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<PowerDown>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);

                                }
                                player.AddBuff(ModContent.BuffType<StatLower>(), 20);
                            }
                            if (player.HasBuff(ModContent.BuffType<StatLower>()))
                            {
                                player.AddBuff(ModContent.BuffType<StatLower>(), 20);
                            }
                            Projectile.netUpdate = true;
                        }
                        if ((Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneUnderworldHeight || Main.player[Main.myPlayer].ZoneRain || Main.player[Main.myPlayer].ZoneBeach || Main.player[Main.myPlayer].ZoneMeteor || Main.player[Main.myPlayer].ZoneWaterCandle) && (!player.HasBuff(ModContent.BuffType<StatLower>())))
                        {
                            if (!player.HasBuff(ModContent.BuffType<StatRaise>()))
                            {
                                SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/StatRaise"), Projectile.Center);
                                for (int j = 0; j < 1; j++) //set to 2
                                {
                                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<PowerUp>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                                }
                                player.AddBuff(ModContent.BuffType<StatRaise>(), 20);
                            }
                            if (player.HasBuff(ModContent.BuffType<StatRaise>()))
                            {
                                player.AddBuff(ModContent.BuffType<StatRaise>(), 20);
                            }
                        }
                        Projectile.netUpdate = true;
                    }
                    if (Transform == 2)
                    {
                        if ((Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneBeach || (Main.player[Main.myPlayer].ZoneRain && !Main.player[Main.myPlayer].ZoneSnow) || Main.player[Main.myPlayer].ZoneSandstorm))
                        {
                            if (!player.HasBuff(ModContent.BuffType<StatRaise>()) && !player.HasBuff(ModContent.BuffType<StatLower>()))
                            {
                                SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/StatLower"), Projectile.Center);
                                for (int j = 0; j < 1; j++) //set to 2
                                {
                                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<PowerDown>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                                }
                                player.AddBuff(ModContent.BuffType<StatLower>(), 20);
                            }
                            if (player.HasBuff(ModContent.BuffType<StatLower>()))
                            {
                                player.AddBuff(ModContent.BuffType<StatLower>(), 20);
                            }
                            Projectile.netUpdate = true;
                        }
                        if ((Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneSnow || Main.player[Main.myPlayer].ZoneGlowshroom || Main.player[Main.myPlayer].ZoneJungle || Main.player[Main.myPlayer].ZoneDungeon || Main.player[Main.myPlayer].ZoneHallow && (!player.HasBuff(ModContent.BuffType<StatLower>()))))
                        {
                            if (!player.HasBuff(ModContent.BuffType<StatRaise>()) && !player.HasBuff(ModContent.BuffType<StatLower>()))
                            {
                                SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/StatRaise"), Projectile.Center);
                                for (int j = 0; j < 1; j++) //set to 2
                                {
                                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<PowerUp>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                                }
                                player.AddBuff(ModContent.BuffType<StatRaise>(), 20);
                            }
                            if (player.HasBuff(ModContent.BuffType<StatRaise>()))
                            {
                                player.AddBuff(ModContent.BuffType<StatRaise>(), 20);
                            }
                        }
                        Projectile.netUpdate = true;
                    }
                    if (Transform == 3)
                    {
                        if ((Main.player[Main.myPlayer].active && (Main.player[Main.myPlayer].ZoneUndergroundDesert || Main.player[Main.myPlayer].ZoneUnderworldHeight || Main.player[Main.myPlayer].ZoneRockLayerHeight || Main.player[Main.myPlayer].ZoneDirtLayerHeight)))
                        {
                            if (!player.HasBuff(ModContent.BuffType<StatRaise>()) && !player.HasBuff(ModContent.BuffType<StatLower>()))
                            {
                                SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/StatLower"), Projectile.Center);
                                for (int j = 0; j < 1; j++) //set to 2
                                {
                                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<PowerDown>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                                }
                                player.AddBuff(ModContent.BuffType<StatLower>(), 20);
                            }
                            if (player.HasBuff(ModContent.BuffType<StatLower>()))
                            {
                                player.AddBuff(ModContent.BuffType<StatLower>(), 20);
                            }
                            Projectile.netUpdate = true;
                        }
                        if ((Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneBeach || Main.player[Main.myPlayer].ZoneRain))
                        {
                            if (!player.HasBuff(ModContent.BuffType<StatRaise>()) && !player.HasBuff(ModContent.BuffType<StatLower>()))
                            {
                                SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/StatRaise"), Projectile.Center);
                                for (int j = 0; j < 1; j++) //set to 2
                                {
                                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<PowerUp>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                                }
                                player.AddBuff(ModContent.BuffType<StatRaise>(), 20);
                            }
                            if (player.HasBuff(ModContent.BuffType<StatRaise>()))
                            {
                                player.AddBuff(ModContent.BuffType<StatRaise>(), 20);
                            }
                        }
                        Projectile.netUpdate = true;
                    }
                    if (Transform == 4)
                    {
                        if ((Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneSkyHeight || Main.player[Main.myPlayer].ZoneRain || Main.player[Main.myPlayer].ZoneBeach))
                        {
                            if (!player.HasBuff(ModContent.BuffType<StatRaise>()) && !player.HasBuff(ModContent.BuffType<StatLower>()))
                            {
                                SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/StatLower"), Projectile.Center);
                                for (int j = 0; j < 1; j++) //set to 2
                                {
                                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<PowerDown>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                                }
                                player.AddBuff(ModContent.BuffType<StatLower>(), 20);
                            }
                            if (player.HasBuff(ModContent.BuffType<StatLower>()))
                            {
                                player.AddBuff(ModContent.BuffType<StatLower>(), 20);
                            }
                            Projectile.netUpdate = true;
                        }
                        if ((Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneUndergroundDesert || Main.player[Main.myPlayer].ZoneUnderworldHeight || Main.player[Main.myPlayer].ZoneRockLayerHeight))
                        {
                            if (!player.HasBuff(ModContent.BuffType<StatRaise>()) && !player.HasBuff(ModContent.BuffType<StatLower>()))
                            {
                                SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/StatRaise"), Projectile.Center);
                                for (int j = 0; j < 1; j++) //set to 2
                                {
                                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<PowerUp>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                                }
                                player.AddBuff(ModContent.BuffType<StatRaise>(), 20);
                            }
                            if (player.HasBuff(ModContent.BuffType<StatRaise>()))
                            {
                                player.AddBuff(ModContent.BuffType<StatRaise>(), 20);
                            }
                        }
                        Projectile.netUpdate = true;
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
                                SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/StatLower"), Projectile.Center);
                                for (int j = 0; j < 1; j++) //set to 2
                                {
                                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<PowerDown>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                                }
                                player.AddBuff(ModContent.BuffType<StatLower>(), 20);
                            }
                            if (player.HasBuff(ModContent.BuffType<StatLower>()))
                            {
                                player.AddBuff(ModContent.BuffType<StatLower>(), 20);
                            }
                            Projectile.netUpdate = true;
                        }
                        if ((Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneCorrupt || Main.player[Main.myPlayer].ZoneCrimson || Main.player[Main.myPlayer].ZoneDungeon || !Main.dayTime))
                        {
                            if (!player.HasBuff(ModContent.BuffType<StatRaise>()) && !player.HasBuff(ModContent.BuffType<StatLower>()))
                            {
                                SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/StatRaise"), Projectile.Center);
                                for (int j = 0; j < 1; j++) //set to 2
                                {
                                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<PowerUp>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                                }
                                player.AddBuff(ModContent.BuffType<StatRaise>(), 20);
                            }
                            if (player.HasBuff(ModContent.BuffType<StatRaise>()))
                            {
                                player.AddBuff(ModContent.BuffType<StatRaise>(), 20);
                            }
                            Projectile.netUpdate = true;
                        }
                    }
                }
                ////////////// Statraise and lower end
                ///



             
                Vector2 idlePosition60 = player.Center;
            float minionPositionOffsetX60 = ((60 + Projectile.minionPos / 80) * player.direction) - 15;
            idlePosition60.Y -= 15f;
            idlePosition60.X += minionPositionOffsetX60;
            Vector2 idlePosition50 = player.Center;
            float minionPositionOffsetX50 = ((50 + Projectile.minionPos / 80) * player.direction) - 15;
            idlePosition50.Y -= 15f;
            idlePosition50.X += minionPositionOffsetX50;
            Vector2 idlePosition40 = player.Center;
            float minionPositionOffsetX40 = ((40 + Projectile.minionPos / 80) * player.direction) - 15;
            idlePosition40.Y -= 15f;
            idlePosition40.X += minionPositionOffsetX40;
            Vector2 idlePosition30 = player.Center;
            float minionPositionOffsetX30 = ((30 + Projectile.minionPos / 80) * player.direction) - 15;
            idlePosition30.Y -= 15f;
            idlePosition30.X += minionPositionOffsetX30;
            Vector2 idlePosition20 = player.Center;
            float minionPositionOffsetX20 = ((30 + Projectile.minionPos / 80) * player.direction) - 15;
            idlePosition20.Y -= 15f;
            idlePosition20.X += minionPositionOffsetX20;
            bool foundTarget = false;

            Vector2 idlePosition = player.Center;
            float nothing = 1;
            float speed = 2;
            float Close = 60;
            if (Eating <= 0 && Sleep <= 0)
            {
                if (player.HeldItem.type == ModContent.ItemType<FrozenYogurt>() || player.HeldItem.type == ModContent.ItemType<SariasConfect>())
                {
                    Close = 20;

                    if ((player.ownedProjectileCounts[ModContent.ProjectileType<Notice>()] <= 0f) && Holding <= 0)
                    {
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<Notice>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                        Holding = 1;
                        MoveTimer = 0;
                            Projectile.netUpdate = true;
                        }
                }
                if ((player.ownedProjectileCounts[ModContent.ProjectileType<FrozenYogurtSignal>()] > 0f))
                {
                    ToEat = 1;
                        Projectile.netUpdate = true;
                    }
                if ((player.ownedProjectileCounts[ModContent.ProjectileType<Competitivetime>()] > 0f))
                {
                    ToEat = 2;
                        Projectile.netUpdate = true;
                    }
                if (player.HeldItem.type == ModContent.ItemType<HealBall>())
                {
                    Close = 60;
                    Holding = 0;
                        Projectile.netUpdate = true;
                    }
                    if (player.HeldItem.type != ModContent.ItemType<FrozenYogurt>() && player.HeldItem.type != ModContent.ItemType<SariasConfect>() && player.HeldItem.type != ModContent.ItemType<HealBall>())
                    {
                        if (player.statLife >= (player.statLifeMax2 - player.statLifeMax2/12))
                        {
                            Close = 30;
                            Holding = 0;
                            Projectile.netUpdate = true;
                        }
                        else
                        {
                            Close = 60;
                            Holding = 0;
                            Projectile.netUpdate = true;
                        }
                    }
                    if (player.moveSpeed <= 5)
                {
                    if ((Collision.SolidCollision(idlePosition60, 20, 5) == true) && (Collision.SolidCollision(idlePosition50, 20, 5) != true) && (Collision.SolidCollision(idlePosition40, 20, 5) != true) && (Collision.SolidCollision(idlePosition30, 20, 5) != true) && (Collision.SolidCollision(idlePosition20, 20, 5) != true))
                    {

                        Close = 50;
                    }
                    if ((Collision.SolidCollision(idlePosition60, 20, 5) == true) && (Collision.SolidCollision(idlePosition50, 20, 5) == true) && (Collision.SolidCollision(idlePosition40, 20, 5) != true) && (Collision.SolidCollision(idlePosition30, 20, 5) != true) && (Collision.SolidCollision(idlePosition20, 20, 5) != true))
                    {

                        Close = 40;
                    }
                    if ((Collision.SolidCollision(idlePosition60, 20, 5) == true) && (Collision.SolidCollision(idlePosition50, 20, 5) == true) && (Collision.SolidCollision(idlePosition40, 20, 5) == true) && (Collision.SolidCollision(idlePosition30, 20, 5) != true) && (Collision.SolidCollision(idlePosition20, 20, 5) != true))
                    {

                        Close = 30;
                    }
                    if ((Collision.SolidCollision(idlePosition60, 20, 5) == true) && (Collision.SolidCollision(idlePosition50, 20, 5) == true) && (Collision.SolidCollision(idlePosition40, 20, 5) == true) && (Collision.SolidCollision(idlePosition30, 20, 5) == true) && (Collision.SolidCollision(idlePosition20, 20, 5) != true))
                    {

                        Close = 20;
                    }
                    if ((Collision.SolidCollision(idlePosition60, 20, 5) == true) && (Collision.SolidCollision(idlePosition50, 20, 5) == true) && (Collision.SolidCollision(idlePosition40, 20, 5) == true) && (Collision.SolidCollision(idlePosition30, 20, 5) == true) && (Collision.SolidCollision(idlePosition20, 20, 5) == true))
                    {

                        Close = 10;
                    }
                }
            }

            float minionPositionOffsetX = ((Close + Projectile.minionPos / 80) * player.direction) - 15;
            idlePosition.Y -= 15f;
            idlePosition.X += minionPositionOffsetX;
            Vector2 vectorToIdlePosition = idlePosition - Projectile.Center;

            float distanceToIdlePosition = vectorToIdlePosition.Length(); 



            Vector2 direction = idlePosition - Projectile.Center;


            if (foundTarget)
            {
                {
                    speed = 2;
                    Projectile.velocity = (((Projectile.velocity * (13 - speed) + direction) / 20) * nothing);
                        Projectile.netUpdate = true;

                    }
            }

            if (!foundTarget)
            {
                nothing = 1;
                    Projectile.netUpdate = true;

                }
            if (MoveTimer >= 275 && (base.Projectile.frame >= 0) && (base.Projectile.frame <= 75) && (distanceToIdlePosition <= 180) && (Math.Abs(Projectile.velocity.X) <= .5) && (player.statLife >= player.statLifeMax2) || Sleep >= 1 || Eating >=1 )
            {
                nothing = 0;
                    Projectile.netUpdate = true;
                }
            
            else
            {
                nothing = 1;
                    Projectile.netUpdate = true;

                }
            if (Sleep >= 1 && (distanceToIdlePosition > 180))
            {
                MoveTimer = 0;
                    Projectile.netUpdate = true;

                }
            Projectile.velocity = ((Projectile.velocity * (13 - speed) + direction) / 20) * nothing;
            if (ToEat == 1 && distanceToIdlePosition <= 20 && Projectile.frame < 75)
            {
                Eating = 1;
                ToEat = 0;
                Projectile.frame = 0;
                    Projectile.netUpdate = true;
                }
            if (ToEat == 2 && distanceToIdlePosition <= 20 && Projectile.frame < 75)
            {
                Eating = 2;
                ToEat = 0;
                Projectile.frame = 0;
                    Projectile.netUpdate = true;
                }
            if (Eating >= 1)
            {
                Projectile.spriteDirection = 1;
                    Projectile.netUpdate = true;
                }
            if (player.statLife < (player.statLifeMax2)/4 && !player.HasBuff(ModContent.BuffType<HealpulseBuff>()))
            {
                Mood -= 600;
                player.statLife += 500;
                player.AddBuff(ModContent.BuffType<HealpulseBuff>(), 8000);
                    Projectile.netUpdate = true;

                    for (int j = 0; j < 1; j++) //set to 2
                {
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<Heal>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<Sad>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                }
            }
            if (!foundTarget)
            {
                if (Projectile.velocity.X >= 0.25)
                {
                    Projectile.spriteDirection = 1;
                        Projectile.netUpdate = true;
                    }
                if (Projectile.velocity.X <= -0.25)
                {
                    Projectile.spriteDirection = -1;
                        Projectile.netUpdate = true;
                    }
            }

            if (Projectile.frame == 65 && Sleep > 0 && MoveTimer >= 550)
            {
                if (Main.rand.NextBool(40))
                {
                    float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius3 * sphereRadius3));
                    double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                    if (Projectile.spriteDirection > 0)
                    {
                        sneezespot = 18;
                    }
                    if (Projectile.spriteDirection < 0)
                    {
                        sneezespot = 3;
                    }
                    Dust.NewDust(new Vector2((Projectile.Center.X + sneezespot) + radius * (float)Math.Cos(angle), (Projectile.Center.Y - 10) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<Z>(), 0f, 0f, 0, default(Color), 1.5f);
                        Projectile.netUpdate = true;
                    }
            }

           
            if (MoveTimer >= (6000) && Projectile.frame == 59 && Mood >= 0)
            {
                
                {
                    Sleep = 1;
                        Projectile.netUpdate = true;
                    }
            }
            if (MoveTimer >= (5000) && Projectile.frame == 59 && Mood < 0)
            {

                {
                    Sleep = 1;
                        Projectile.netUpdate = true;
                    }
            }



            int frameSpeed = 30; //reduced by half due to framecounter speedup
            Projectile.frameCounter += 2;
                if (Projectile.frameCounter >= frameSpeed)
                {
                    Projectile.frameCounter = 0;
                    if (Projectile.frame >= Main.projFrames[ModContent.ProjectileType<Saria>()]) //error here! you had the wrong projectile id, so the animation did not use the right frames
                    {
                        Projectile.frame = 0;

                    }
                    if (base.Projectile.frame == 25 && Eating > 0)
                    {
                        float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius3 * sphereRadius3));
                        double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                        if (Projectile.spriteDirection > 0)
                        {
                            sneezespot = 25;
                        }
                        if (Projectile.spriteDirection < 0)
                        {
                            sneezespot = -2;
                        }
                        for (int j = 0; j < 2; j++)
                        {
                            Dust.NewDust(new Vector2((Projectile.Center.X + sneezespot) + radius * (float)Math.Cos(angle), (Projectile.Center.Y - 10) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<Fog>(), 0f, 0f, 0, default(Color), 1.5f);

                        }
                    }
                    if (base.Projectile.frame == 34 && Eating <= 0)
                    {
                        SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Step2"), Projectile.Center);
                    }
                    if (Projectile.frame == 37 && Eating >= 1)
                    {
                        Projectile.frame = 62;
                        Vector2 Throw = Projectile.Center;
                        Throw.Y += 0f;
                        Throw.X += 40f;
                        Vector2 ThrowToo = Projectile.DirectionTo(Main.MouseWorld).SafeNormalize(Vector2.Zero);

                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Throw, ThrowToo * 10, ModContent.ProjectileType<EmptyCup>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);

                    }
                    if (Projectile.frame == 74 && Eating >= 1)
                    {
                        Eating = 0;
                        Projectile.netUpdate = true;
                    }
                    if (Projectile.ai[0] == 0 || Projectile.ai[0] == 3 || Projectile.ai[0] == 4) //only run these animations if not attacking! no longer overrides
                    {
                        if ((Projectile.velocity.Y) > -3f && (Projectile.velocity.Y) < 4f && Math.Abs(Projectile.velocity.X) <= .5) //Idle animation, notice how I have (
                                                                                                                                    //
                                                                                                                                    //.Y greater than -3f and less than 4f. this DID conflict with the rising and Falling animations but this is how i fixed it.
                        { ////however you set up the attack animation, make sure that none of these other animations override it. 
                          //that's easy legit just
                            Projectile.frame++;
                            if (base.Projectile.frameCounter <= 76)
                            {

                                base.Projectile.frameCounter = 0;
                            }

                            if (Sleep > 0 && MoveTimer > 550)
                            {

                                if (Projectile.frame == 60)
                                {

                                    Projectile.frame = 62;
                                    TimeAsleep += 5;
                                    Projectile.netUpdate = true;
                                    SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Hover"), Projectile.Center);

                                }
                                if (Projectile.frame >= 66 && MoveTimer >= 550)
                                {
                                    TimeAsleep += 5;
                                    Projectile.frame = 62;
                                    Projectile.netUpdate = true;
                                    SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Hover"), Projectile.Center);

                                }

                            }
                            if (Projectile.frame == 66 && (player.ownedProjectileCounts[ModContent.ProjectileType<Notice>()] <= 0f) && Sleep > 0)
                            {
                                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<Notice>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                            }
                            if (base.Projectile.frame == 63 && Sleep <= 0)
                            {
                                SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Step2"), Projectile.Center);
                            }
                            if (base.Projectile.frame >= 76)
                            {
                                base.Projectile.frame = 0;
                                if (Sleep > 0)
                                {
                                    Sleep = 0;
                                    Projectile.netUpdate = true;
                                }
                                SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Step1"), Projectile.Center);
                            }
                            if (base.Projectile.frame == 58 && player.statLife < ((player.statLifeMax2) - (player.statLifeMax2 / 4)) && !player.HasBuff(ModContent.BuffType<HealpulseBuff>()))
                            {
                                Mood -= 600;
                                player.statLife += 500;
                                if (!player.HasBuff(ModContent.BuffType<HealpulseBuff>()))
                                {

                                    for (int j = 0; j < 1; j++) //set to 2
                                    {
                                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<Heal>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<Sad>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                                    }
                                }
                                player.AddBuff(ModContent.BuffType<HealpulseBuff>(), 8000);
                                Projectile.netUpdate = true;
                            }
                        }
                        if ((Projectile.velocity.Y) < 4f && Math.Abs(Projectile.velocity.X) > 0.5f && Math.Abs(Projectile.velocity.X) < 4f) //walking animation and such
                        {
                            Projectile.frame++;
                            Projectile.frameCounter += 3;

                            if (base.Projectile.frame <= 80)
                            {

                                base.Projectile.frameCounter = 0;

                            }
                            if (base.Projectile.frame >= 80)
                            {
                                base.Projectile.frame = 76;
                            }
                            if (base.Projectile.frame < 76)
                            {
                                base.Projectile.frame = 76;
                            }

                        }

                        if ((Projectile.velocity.Y) < 4f && Math.Abs(Projectile.velocity.X) >= 4f)//running or (floating) animation
                        {
                            Projectile.frame++;

                            if (base.Projectile.frameCounter < 83)
                            {

                                base.Projectile.frameCounter = 0;

                            }
                            if (base.Projectile.frame >= 83)
                            {
                                base.Projectile.frame = 80;
                                SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Hover"), Projectile.Center);
                            }
                            if (base.Projectile.frame < 80)
                            {

                                base.Projectile.frame = 80;
                            }
                        }
                        if ((Projectile.velocity.Y) < -3f) //rising animation
                        {
                            Projectile.frame++;

                            if (base.Projectile.frameCounter < 83)
                            {

                                base.Projectile.frameCounter = 0;

                            }
                            if (base.Projectile.frame >= 83)
                            {
                                base.Projectile.frame = 80;
                            }
                            if (base.Projectile.frame < 80)
                            {
                                base.Projectile.frame = 80;
                                SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Fly"), Projectile.Center);
                            }
                        }

                        if (Projectile.velocity.Y > 4f) //falling animation
                        {
                            Projectile.frame++;
                            
                            {
                                if (base.Projectile.frameCounter < 99)
                                {

                                    base.Projectile.frameCounter = 0;

                                }
                                if (base.Projectile.frame >= 99)
                                {
                                    base.Projectile.frame = 97;
                                }
                                if (base.Projectile.frame < 97)
                                {
                                    base.Projectile.frame = 97;
                                }
                            }
                           
                        }

                    }

                    //Main.NewText(projectile.ai[0] + " is state, " + projectile.ai[1] + " is timer. Test");

                    if (Projectile.ai[0] == 4)
                    {
                        Projectile.ai[1] -= 1; //reduce timer
                        if (Projectile.ai[1] == 0)
                        {
                            Projectile.ai[0] = 0; //once at 0, back to normal behavior
                        }
                    }

                    if (Projectile.ai[0] == 3 && Transform != 3) //recovery setup
                    {

                        Projectile.ai[1] = 4; //4 cycles recovery between shots, adjust this for how long she waits between swipes
                        Projectile.ai[0] = 4;
                    }
                    if (Projectile.ai[0] == 3 && Transform == 3)
                    {

                        Projectile.ai[1] = 10; //4 cycles recovery between shots, adjust this for how long she waits between swipes
                        Projectile.ai[0] = 4;
                    }
                    if (Projectile.ai[0] == 2)
                    {
                        base.Projectile.frame++; //increment attack frame
                        base.Projectile.frameCounter += 15;
                        if (base.Projectile.frame == 84)
                        {
                            SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Hover"), Projectile.Center);
                        }
                        if (base.Projectile.frame == 86)
                        {
                            SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Step2"), Projectile.Center);
                        }
                        if (base.Projectile.frame == 89)
                        {
                            base.Projectile.frameCounter = 1;
                        }
                        if (base.Projectile.frame > 90)
                        {
                            base.Projectile.frameCounter = 15;
                        }
                        if (base.Projectile.frame == 93)
                        {
                            SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Hover"), Projectile.Center);
                        }









                        ///////////Transform Attacks
                       
                        if (player.ownedProjectileCounts[ModContent.ProjectileType<CoolDown>()] <= 0f)
                        {
                            if (Transform == 0)
                            //Main.NewText("Frame: " + base.projectile.frame);
                            {

                                if (base.Projectile.frame == 84)
                                {
                                    target = base.Projectile.Center.MinionHoming(500f, player);// the distance she targets enemies
                                    SoundEngine.PlaySound(SoundID.Item77, base.Projectile.Center);
                                    for (int j = 0; j < 1; j++) //set to 2
                                    {
                                        if (Projectile.spriteDirection == -1)
                                        {
                                            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 40, Projectile.position.Y + 20, 0, 0, ModContent.ProjectileType<Locator>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                                        }
                                        if (Projectile.spriteDirection == 1)
                                        {
                                            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 70, Projectile.position.Y + 20, 0, 0, ModContent.ProjectileType<Locator>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                                        }
                                    }
                                }
                                if (base.Projectile.frame == 86)
                                {
                                    target = base.Projectile.Center.MinionHoming(500f, player);// the distance she targets enemies
                                    SoundEngine.PlaySound(SoundID.Item77, base.Projectile.Center);
                                    for (int j = 0; j < 1; j++) //set to 2
                                    {
                                        if (Projectile.spriteDirection == -1)
                                        {
                                            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 40, Projectile.position.Y + 20, 0, 0, ModContent.ProjectileType<Locator>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                                        }
                                        if (Projectile.spriteDirection == 1)
                                        {
                                            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 70, Projectile.position.Y + 20, 0, 0, ModContent.ProjectileType<Locator>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                                        }
                                    }
                                }

                                if (base.Projectile.frame == 89)
                                {
                                    target = base.Projectile.Center.MinionHoming(500f, player);// the distance she targets enemies
                                    SoundEngine.PlaySound(SoundID.Item77, base.Projectile.Center);
                                    for (int j = 0; j < 1; j++) //set to 2
                                    {
                                        if (Projectile.spriteDirection == -1)
                                        {
                                            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 40, Projectile.position.Y + 20, 0, 0, ModContent.ProjectileType<Locator>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                                        }
                                        if (Projectile.spriteDirection == 1)
                                        {
                                            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 70, Projectile.position.Y + 20, 0, 0, ModContent.ProjectileType<Locator>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                                        }
                                    }
                                }
                            }
                            else if (Transform == 1)
                            {
                                if (base.Projectile.frame == 90 && player.ownedProjectileCounts[ModContent.ProjectileType<Bubble2>()] < 1f)
                                {
                                    target = base.Projectile.Center.MinionHoming(500f, player);// the distance she targets enemies
                                    SoundEngine.PlaySound(SoundID.Item77, base.Projectile.Center);



                                    {
                                        for (int j = 0; j < 1; j++) //set to 2
                                        {
                                            SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Nayru"), Projectile.Center);
                                            float Something = (float)Math.Sqrt(Main.rand.Next(70 * 70));
                                            SoundEngine.PlaySound(SoundID.Drown, base.Projectile.Center);
                                            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + Something, Projectile.position.Y + 0, 0, 0, ModContent.ProjectileType<Bubble2>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);

                                        }
                                    }

                                }
                                else if (base.Projectile.frame == 90 && player.HasMinionAttackTargetNPC && player.ownedProjectileCounts[ModContent.ProjectileType<Bubble2>()] >= 1f)
                                {
                                    {
                                        Vector2 Throw = Projectile.Center;
                                        Throw.Y += 0f;
                                        Throw.X += 40f;
                                        Vector2 ThrowToo = Projectile.DirectionTo(Main.MouseWorld).SafeNormalize(Vector2.Zero);
                                        SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Nayru2"), Projectile.Center);
                                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Throw, ThrowToo * 6, ModContent.ProjectileType<Bubble>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
                                    }

                                }
                            }

                            else if (Transform == 2)
                            {
                                if (base.Projectile.frame == 88)
                                {
                                    SoundEngine.PlaySound(SoundID.Item77, base.Projectile.Center);
                                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 60, Projectile.position.Y + 40, 0, 0, ModContent.ProjectileType<RubyPsychicSeeker>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                                }
                            }
                            else if (Transform == 3)
                            {
                                if (base.Projectile.frame == 90 && (player.ownedProjectileCounts[ModContent.ProjectileType<LightningLocator>()] == 0f) && (player.ownedProjectileCounts[ModContent.ProjectileType<LightningLocator2>()] == 0f))
                                {

                                    SoundEngine.PlaySound(SoundID.Item77, base.Projectile.Center);
                                    for (int j = 0; j < 1; j++) //set to 2
                                    {
                                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 10, Projectile.position.Y + 10, 0, 0, ModContent.ProjectileType<LightningLocator>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                                    }
                                }

                                if (base.Projectile.frame == 90 && (player.ownedProjectileCounts[ModContent.ProjectileType<LightningLocator>()] == 0f) && (player.ownedProjectileCounts[ModContent.ProjectileType<LightningLocator2>()] >= 1f))
                                {
                                    SoundEngine.PlaySound(SoundID.Item77, base.Projectile.Center);
                                    int Lightning2 = ModContent.ProjectileType<LightningLocator2>();
                                    for (int g = 0; g < 1000; g++)
                                    {
                                       
                                        
                                            if (Main.projectile[g].active && g != base.Projectile.whoAmI && ((Main.projectile[g].type == Lightning2 && Main.projectile[g].owner == owner)))
                                            {

                                                {
                                                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Main.projectile[g].position.X + 16, Main.projectile[g].position.Y + 16, 0, 0, ModContent.ProjectileType<LightningLocator>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                                                    Main.projectile[g].Kill();
                                                }

                                            }

                                        
                                    }
                                }
                                else if (base.Projectile.frame == 90 && (player.ownedProjectileCounts[ModContent.ProjectileType<LightningLocator>()] > 0f) && (player.ownedProjectileCounts[ModContent.ProjectileType<Static2>()] < 3f) && !player.HasMinionAttackTargetNPC)
                                {

                                    SoundEngine.PlaySound(SoundID.Item77, base.Projectile.Center);
                                    for (int j = 0; j < 1; j++) //set to 2
                                    {
                                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 60, Projectile.position.Y + 40, 0, 0, ModContent.ProjectileType<Static2>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                                    }
                                }
                                else if (base.Projectile.frame == 90 && (player.ownedProjectileCounts[ModContent.ProjectileType<LightningLocator>()] > 0f) && player.HasMinionAttackTargetNPC)
                                {

                                    SoundEngine.PlaySound(SoundID.Item77, base.Projectile.Center);
                                    for (int j = 0; j < 1; j++) //set to 2
                                    {
                                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + 0, 0, 0, ModContent.ProjectileType<Static>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                                    }
                                }

                            }
                            else if (Transform == 4)
                            {
                                if ((player.ownedProjectileCounts[ModContent.ProjectileType<Silverrupee>()] > 0f))
                                {
                                    target = base.Projectile.Center.MinionHoming(500f, player);// the distance she targets enemies
                                    SoundEngine.PlaySound(SoundID.Item77, base.Projectile.Center);
                                    for (int j = 0; j < 1; j++) //set to 2
                                    {
                                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + 0, 0, 0, ModContent.ProjectileType<RupeeAttack>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                                    }
                                }
                                if (base.Projectile.frame == 90 && player.HasMinionAttackTargetNPC)
                                {
                                    if ((player.ownedProjectileCounts[ModContent.ProjectileType<Rupee>()] > 0f) || (player.ownedProjectileCounts[ModContent.ProjectileType<Specialrupee>()] > 0f) || (player.ownedProjectileCounts[ModContent.ProjectileType<Silverrupee>()] > 0f))
                                  {
                                        target = base.Projectile.Center.MinionHoming(500f, player);// the distance she targets enemies
                                        SoundEngine.PlaySound(SoundID.Item77, base.Projectile.Center);
                                        for (int j = 0; j < 1; j++) //set to 2
                                        {
                                            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + 0, 0, 0, ModContent.ProjectileType<RupeeAttack>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                                        }
                                    }
                                    else
                                    {
                                        target = base.Projectile.Center.MinionHoming(500f, player);// the distance she targets enemies
                                        SoundEngine.PlaySound(SoundID.Item77, base.Projectile.Center);
                                        for (int j = 0; j < 1; j++) //set to 2
                                        {
                                            SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/SilverRupee1"), Projectile.Center);
                                            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + 0, 0, 0, ModContent.ProjectileType<Rupee>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                                        }
                                    }
                                }
                                    if (base.Projectile.frame == 90 && !player.HasMinionAttackTargetNPC &&(player.ownedProjectileCounts[ModContent.ProjectileType<Rupee>()] <= 0f) && (player.ownedProjectileCounts[ModContent.ProjectileType<Specialrupee>()] <= 0f) && (player.ownedProjectileCounts[ModContent.ProjectileType<Silverrupee>()] <= 0f))
                                {
                                   
                                    {

                                        target = base.Projectile.Center.MinionHoming(500f, player);// the distance she targets enemies
                                        SoundEngine.PlaySound(SoundID.Item77, base.Projectile.Center);
                                        for (int j = 0; j < 1; j++) //set to 2
                                        {
                                            SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/SilverRupee1"), Projectile.Center);
                                            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + 0, 0, 0, ModContent.ProjectileType<Rupee>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                                        }
                                    }
                                }
                            }
                            else if (Transform == 5)
                            {
                                if (base.Projectile.frame == 90 && SwarmTimer >= 1000)
                                {

                                    if (player.ownedProjectileCounts[ModContent.ProjectileType<DuskBallProjectile>()] <= 0f)
                                    {
                                        if (((Main.rand.NextBool(60)) && (player.ownedProjectileCounts[ModContent.ProjectileType<GreenMoth>()] <= 0f) && (player.ownedProjectileCounts[ModContent.ProjectileType<GreenMothGoliath2>()] <= 0f) && (player.ownedProjectileCounts[ModContent.ProjectileType<AmberGreen>()] <= 0f) && (player.ownedProjectileCounts[ModContent.ProjectileType<GreenMothGiant>()] <= 0f) && (player.ownedProjectileCounts[ModContent.ProjectileType<GreenMothGoliath>()] <= 0f) && ((player.ownedProjectileCounts[ModContent.ProjectileType<RedMoth>()] == 1f) || (player.ownedProjectileCounts[ModContent.ProjectileType<RedMothGiant>()] == 1f)) && ((player.ownedProjectileCounts[ModContent.ProjectileType<PurpleMoth>()] == 1f) || (player.ownedProjectileCounts[ModContent.ProjectileType<PurpleMothGiant>()] == 1f))))
                                        {
                                            {
                                                modPlayer.SariaXp++;
                                                SoundEngine.PlaySound(SoundID.Item77, base.Projectile.Center);
                                                target = base.Projectile.Center.MinionHoming(500f, player);// the distance she targets enemies
                                                SoundEngine.PlaySound(SoundID.Item77, base.Projectile.Center);
                                                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ProjectileType<AmberGreen>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);

                                            }
                                        }
                                        else
                                        {
                                            if ((BugTimer >= 50 && BugTimer <= 350) && ((player.ownedProjectileCounts[ModContent.ProjectileType<RedMoth>()] <= 0f) && (player.ownedProjectileCounts[ModContent.ProjectileType<RedMothGiant>()] <= 0f)))
                                            {
                                                modPlayer.SariaXp++;
                                                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + new Vector2(-250f, 370f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ProjectileType<AmberRed>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);

                                            }
                                            if ((BugTimer >= 50 && BugTimer <= 250) && ((player.ownedProjectileCounts[ModContent.ProjectileType<PurpleMoth>()] <= 0f) && (player.ownedProjectileCounts[ModContent.ProjectileType<PurpleMothGiant>()] <= 0f)))
                                            {
                                                modPlayer.SariaXp++;
                                                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + new Vector2(250f, 370f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ProjectileType<AmberPurple>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);

                                            }
                                            target = base.Projectile.Center.MinionHoming(500f, player);// the distance she targets enemies
                                            SoundEngine.PlaySound(SoundID.Item77, base.Projectile.Center);
                                            for (int j = 0; j < 1; j++) //set to 2
                                            {
                                                modPlayer.SariaXp++;
                                                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + new Vector2(-500f, 370f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ProjectileType<AmberBlack1>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                                                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + new Vector2(500f, 370f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ProjectileType<AmberBlack2>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                                            }
                                        }
                                        SwarmTimer = 0;
                                        Projectile.netUpdate = true;
                                    }
                                }
                            }
                            else if (Transform == 6)
                            {
                                if (base.Projectile.frame == 90)
                                {
                                    target = base.Projectile.Center.MinionHoming(500f, player);// the distance she targets enemies
                                    SoundEngine.PlaySound(SoundID.Item77, base.Projectile.Center);

                                    target = base.Projectile.Center.MinionHoming(500f, player);// the distance she targets enemies
                                    SoundEngine.PlaySound(SoundID.Item77, base.Projectile.Center);
                                    for (int j = 0; j < 1; j++) //set to 2
                                    {
                                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 60, Projectile.position.Y + 40, 0, 0, ModContent.ProjectileType<Shadowmelt>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                                    }
                                }
                            }
                            }
                        else if ((player.ownedProjectileCounts[ModContent.ProjectileType<CoolDown>()] >= 1f) && Projectile.frame >= 84 && Projectile.frame < 96)
                                {
                            Projectile.frame = 96;
                        }
                        //////////////////// End of Transform attacks
                        ///










                        if (base.Projectile.frame > 96) //stop when done
                        {
                            base.Projectile.frame = 10;
                            Projectile.ai[0] = 3;
                        }
                    }

                    if (Projectile.ai[0] == 1 && Transform != 1) //this is set when a target is found
                    {

                        base.Projectile.frame = 83; //animation setup
                        SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Step1"), Projectile.Center);
                        Projectile.ai[0] = 2; //next phase

                    }
                    else if (Projectile.ai[0] == 1 && Transform == 1 && (player.ownedProjectileCounts[ModContent.ProjectileType<Bubble>()] < 1f)) //this is set when a target is found
                    {

                        base.Projectile.frame = 83; //animation setup
                        SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Step1"), Projectile.Center);
                        Projectile.ai[0] = 2; //next phase

                    }



                }
            }
        }
        
        public override void PostDraw(Color lightColor)
        {
            
            { 
                Player player = Main.player[base.Projectile.owner];
            FairyPlayer modPlayer = player.Fairy();
                {


                    Vector2 drawPosition;
                    if (player.ownedProjectileCounts[ModContent.ProjectileType<Flash>()] >= 1f)
                    {
                        Texture2D texture = TextureAssets.Projectile[ModContent.ProjectileType<Flash>()].Value;
                        Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                        int frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<Flash>()];
                        int frameY = frameHeight * Main.projFrames[ModContent.ProjectileType<Flash>()];
                        Color drawColor = Color.Lerp(lightColor, Color.WhiteSmoke, 20f);
                        drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                        Rectangle rectangle = texture.Frame(verticalFrames: 11, frameY: (int)Main.GameUpdateCount / 5 % 11);
                        Vector2 origin = rectangle.Size() / 2f;
                        float rotation = base.Projectile.rotation;
                        float scale = base.Projectile.scale;
                        SpriteEffects spriteEffects = SpriteEffects.None;
                        
                        if (base.Projectile.spriteDirection == -1)
                        {
                            startPos.Y -= 50;
                            startPos.X += +10;
                            spriteEffects = SpriteEffects.FlipHorizontally;
                        }
                        if (base.Projectile.spriteDirection == 1)
                        {
                            startPos.Y -= 50;
                            startPos.X += +25;
                        }
                        Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                    }
                    if (player.ownedProjectileCounts[ModContent.ProjectileType<Competitive>()] >= 1f)
                    {
                        Texture2D texture = TextureAssets.Projectile[ModContent.ProjectileType<Competitive>()].Value;
                        Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                        int frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<Competitive>()];
                        int frameY = frameHeight * Main.projFrames[ModContent.ProjectileType<Competitive>()];
                        Color drawColor = Color.Lerp(lightColor, Color.WhiteSmoke, 20f);
                        drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                        Rectangle rectangle = texture.Frame(verticalFrames: 2, frameY: (int)Main.GameUpdateCount / 60 % 2);
                        Vector2 origin = rectangle.Size() / 2f;
                        float rotation = base.Projectile.rotation;
                        float scale = base.Projectile.scale;
                        SpriteEffects spriteEffects = SpriteEffects.None;

                        if (base.Projectile.spriteDirection == -1)
                        {
                            startPos.Y -= 50;
                            startPos.X += +10;
                            spriteEffects = SpriteEffects.FlipHorizontally;
                        }
                        if (base.Projectile.spriteDirection == 1)
                        {
                            startPos.Y -= 50;
                            startPos.X += +25;
                        }
                        Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                    }
                    if (player.ownedProjectileCounts[ModContent.ProjectileType<Smile>()] >= 1f || player.ownedProjectileCounts[ModContent.ProjectileType<Smile2>()] >= 1f || player.ownedProjectileCounts[ModContent.ProjectileType<Happiness>()] >= 1f)
                    {
                        Texture2D texture = TextureAssets.Projectile[ModContent.ProjectileType<Smile>()].Value;
                        Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                        int frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<Smile>()];
                        int frameY = frameHeight * Main.projFrames[ModContent.ProjectileType<Smile>()];
                        Color drawColor = Color.Lerp(lightColor, Color.WhiteSmoke, 20f);
                        drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                        Rectangle rectangle = texture.Frame(verticalFrames: 2, frameY: (int)Main.GameUpdateCount / 60 % 2);
                        Vector2 origin = rectangle.Size() / 2f;
                        float rotation = base.Projectile.rotation;
                        float scale = base.Projectile.scale;
                        SpriteEffects spriteEffects = SpriteEffects.None;

                        if (base.Projectile.spriteDirection == -1)
                        {
                            startPos.Y -= 50;
                            startPos.X += +10;
                            spriteEffects = SpriteEffects.FlipHorizontally;
                        }
                        if (base.Projectile.spriteDirection == 1)
                        {
                            startPos.Y -= 50;
                            startPos.X += +25;
                        }
                        Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                    }
                    if (player.ownedProjectileCounts[ModContent.ProjectileType<Anger>()] >= 1f)
                    {
                        Texture2D texture = TextureAssets.Projectile[ModContent.ProjectileType<Anger>()].Value;
                        Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                        int frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<Anger>()];
                        int frameY = frameHeight * Main.projFrames[ModContent.ProjectileType<Anger>()];
                        Color drawColor = Color.Lerp(lightColor, Color.WhiteSmoke, 20f);
                        drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                        Rectangle rectangle = texture.Frame(verticalFrames: 2, frameY: (int)Main.GameUpdateCount / 60 % 2);
                        Vector2 origin = rectangle.Size() / 2f;
                        float rotation = base.Projectile.rotation;
                        float scale = base.Projectile.scale;
                        SpriteEffects spriteEffects = SpriteEffects.None;

                        if (base.Projectile.spriteDirection == -1)
                        {
                            startPos.Y -= 50;
                            startPos.X += +10;
                            spriteEffects = SpriteEffects.FlipHorizontally;
                        }
                        if (base.Projectile.spriteDirection == 1)
                        {
                            startPos.Y -= 50;
                            startPos.X += +25;
                        }
                        Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                    }
                    if (player.ownedProjectileCounts[ModContent.ProjectileType<Sad>()] >= 1f || player.ownedProjectileCounts[ModContent.ProjectileType<Sad2>()] >= 1f)
                    {
                        Texture2D texture = TextureAssets.Projectile[ModContent.ProjectileType<Sad>()].Value;
                        Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                        int frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<Sad>()];
                        int frameY = frameHeight * Main.projFrames[ModContent.ProjectileType<Sad>()];
                        Color drawColor = Color.Lerp(lightColor, Color.WhiteSmoke, 20f);
                        drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                        Rectangle rectangle = texture.Frame(verticalFrames: 2, frameY: (int)Main.GameUpdateCount / 60 % 2);
                        Vector2 origin = rectangle.Size() / 2f;
                        float rotation = base.Projectile.rotation;
                        float scale = base.Projectile.scale;
                        SpriteEffects spriteEffects = SpriteEffects.None;

                        if (base.Projectile.spriteDirection == -1)
                        {
                            startPos.Y -= 50;
                            startPos.X += +10;
                            spriteEffects = SpriteEffects.FlipHorizontally;
                        }
                        if (base.Projectile.spriteDirection == 1)
                        {
                            startPos.Y -= 50;
                            startPos.X += +25;
                        }
                        Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                    }
                    if (player.ownedProjectileCounts[ModContent.ProjectileType<Notice>()] >= 1f)
                    {
                        Texture2D texture = TextureAssets.Projectile[ModContent.ProjectileType<Notice>()].Value;
                        Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                        int frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<Notice>()];
                        int frameY = frameHeight * Main.projFrames[ModContent.ProjectileType<Notice>()];
                        Color drawColor = Color.Lerp(lightColor, Color.WhiteSmoke, 20f);
                        drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                        Rectangle rectangle = texture.Frame(verticalFrames: 1, frameY: (int)Main.GameUpdateCount / 1 % 1);
                        Vector2 origin = rectangle.Size() / 2f;
                        float rotation = base.Projectile.rotation;
                        float scale = base.Projectile.scale;
                        SpriteEffects spriteEffects = SpriteEffects.None;

                        if (base.Projectile.spriteDirection == -1)
                        {
                            startPos.Y -= 50;
                            startPos.X += +10;
                            spriteEffects = SpriteEffects.FlipHorizontally;
                        }
                        if (base.Projectile.spriteDirection == 1)
                        {
                            startPos.Y -= 50;
                            startPos.X += +25;
                        }
                        Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                    }
                   
                    for (int i = 1; i < 25; i++)
                    {
                        Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/SariaFeet");
                        Vector2 startPos = base.Projectile.oldPos[i] + base.Projectile.Size * 0.5f - Main.screenPosition;
                        int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
                        int frameY = frameHeight * base.Projectile.frame;
                        float completionRatio = (float)i / (float)base.Projectile.oldPos.Length;
                        Color drawColor = Color.Lerp(lightColor, Color.LightPink, 20f);
                        drawColor = Color.Lerp(drawColor, Color.DarkViolet, completionRatio);
                        drawColor = Color.Lerp(drawColor, Color.Transparent, completionRatio);
                        Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                        Vector2 origin = rectangle.Size() / 2f;
                        float rotation = base.Projectile.rotation;
                        float scale = base.Projectile.scale;
                        SpriteEffects spriteEffects = SpriteEffects.None;
                        startPos.Y += 1;
                        startPos.X += +17;

                        if (base.Projectile.spriteDirection == -1)
                        {
                            spriteEffects = SpriteEffects.FlipHorizontally;
                        }
                        Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, layerDepth: 0f);

                    }
                    for (int i = 1; i < 30; i++)
                    {
                        if (base.Projectile.spriteDirection == 1)
                        {
                            Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/SariaArm");
                            Vector2 startPos = base.Projectile.oldPos[i] + base.Projectile.Size * 0.5f - Main.screenPosition;
                            int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
                            int frameY = frameHeight * base.Projectile.frame;
                            float completionRatio = (float)i / (float)base.Projectile.oldPos.Length;
                            Color drawColor = Color.Lerp(lightColor, Color.LightPink, 20f);
                            drawColor = Color.Lerp(drawColor, Color.DarkViolet, completionRatio);
                            drawColor = Color.Lerp(drawColor, Color.Transparent, completionRatio);
                            Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                            Vector2 origin = rectangle.Size() / 2f;
                            float rotation = base.Projectile.rotation;
                            float scale = base.Projectile.scale;
                            SpriteEffects spriteEffects = SpriteEffects.None;
                            startPos.Y += 1;
                            startPos.X += +17;

                            Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, layerDepth: 0f);
                        }
                        if (base.Projectile.spriteDirection == -1)
                        {
                            Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/SariaArmR");
                            Vector2 startPos = base.Projectile.oldPos[i] + base.Projectile.Size * 0.5f - Main.screenPosition;
                            int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
                            int frameY = frameHeight * base.Projectile.frame;
                            float completionRatio = (float)i / (float)base.Projectile.oldPos.Length;
                            Color drawColor = Color.Lerp(lightColor, Color.LightPink, 20f);
                            drawColor = Color.Lerp(drawColor, Color.DarkViolet, completionRatio);
                            drawColor = Color.Lerp(drawColor, Color.Transparent, completionRatio);
                            Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                            Vector2 origin = rectangle.Size() / 2f;
                            float rotation = base.Projectile.rotation;
                            float scale = base.Projectile.scale;
                            SpriteEffects spriteEffects = SpriteEffects.None;
                            startPos.Y += 1;
                            startPos.X += +17;

                            Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, layerDepth: 0f);
                        }
                    }

                    {
                        if (base.Projectile.spriteDirection == 1)
                        {
                            Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/SariaArmAttack");
                            Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                            int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
                            int frameY = frameHeight * base.Projectile.frame;
                            Color drawColor = Color.Lerp(lightColor, Color.LightPink, 20f);
                            drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                            Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                            Vector2 origin = rectangle.Size() / 2f;
                            float rotation = base.Projectile.rotation;
                            float scale = base.Projectile.scale;
                            SpriteEffects spriteEffects = SpriteEffects.None;
                            startPos.Y += 1;
                            startPos.X += +17;
                            Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                        }

                    }

                    ////////////// Transform Ability
                    if (Transform == 0)
                    {
                        if (Eating >= 1)
                        {
                            Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/SariaEat");
                            Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                            int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
                            int frameY = frameHeight * base.Projectile.frame;
                            Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                            Vector2 origin = rectangle.Size() / 2f;
                            float rotation = base.Projectile.rotation;
                            float scale = base.Projectile.scale;
                            SpriteEffects spriteEffects = SpriteEffects.None;
                            startPos.Y += 1;
                            startPos.X += +17;

                            Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(lightColor), rotation, origin, scale, spriteEffects, 0f);
                            Projectile.netUpdate = true;
                        }
                        if (base.Projectile.spriteDirection == 1 && Eating <= 0)
                        {

                            Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/Sariaa");
                            Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                            int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
                            int frameY = frameHeight * base.Projectile.frame;
                            Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                            Vector2 origin = rectangle.Size() / 2f;
                            float rotation = base.Projectile.rotation;
                            float scale = base.Projectile.scale;
                            SpriteEffects spriteEffects = SpriteEffects.None;
                            startPos.Y += 1;
                            startPos.X += +17;

                            Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(lightColor), rotation, origin, scale, spriteEffects, 0f);
                            Projectile.netUpdate = true;
                        }
                        if (base.Projectile.spriteDirection == -1 && Eating <= 0)
                        {
                            Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/SariaR");
                            Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                            int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
                            int frameY = frameHeight * base.Projectile.frame;
                            Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                            Vector2 origin = rectangle.Size() / 2f;
                            float rotation = base.Projectile.rotation;
                            float scale = base.Projectile.scale;
                            SpriteEffects spriteEffects = SpriteEffects.None;
                            startPos.Y += 1;
                            startPos.X += +17;

                            Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(lightColor), rotation, origin, scale, spriteEffects, 0f);
                            Projectile.netUpdate = true;
                        }
                    }
                    else if (Transform == 1)
                    {
                        if (Eating >= 1)
                        {
                            Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/Saria2Eat");
                            Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                            int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
                            int frameY = frameHeight * base.Projectile.frame;
                            Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                            Vector2 origin = rectangle.Size() / 2f;
                            float rotation = base.Projectile.rotation;
                            float scale = base.Projectile.scale;
                            SpriteEffects spriteEffects = SpriteEffects.None;
                            startPos.Y += 1;
                            startPos.X += +17;

                            Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(lightColor), rotation, origin, scale, spriteEffects, 0f);
                            Projectile.netUpdate = true;
                        }
                        if (base.Projectile.spriteDirection == 1 && Eating <= 0)
                        {
                            Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/Saria2");
                            Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                            int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
                            int frameY = frameHeight * base.Projectile.frame;
                            Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                            Vector2 origin = rectangle.Size() / 2f;
                            float rotation = base.Projectile.rotation;
                            float scale = base.Projectile.scale;
                            SpriteEffects spriteEffects = SpriteEffects.None;
                            startPos.Y += 1;
                            startPos.X += +17;
                            Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(lightColor), rotation, origin, scale, spriteEffects, 0f);
                            Projectile.netUpdate = true;
                        }
                        if (base.Projectile.spriteDirection == -1 && Eating <= 0)
                        {
                            Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/Saria2R");
                            Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                            int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
                            int frameY = frameHeight * base.Projectile.frame;
                            Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                            Vector2 origin = rectangle.Size() / 2f;
                            float rotation = base.Projectile.rotation;
                            float scale = base.Projectile.scale;
                            SpriteEffects spriteEffects = SpriteEffects.None;
                            startPos.Y += 1;
                            startPos.X += +17;

                            Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(lightColor), rotation, origin, scale, spriteEffects, 0f);
                            
                        }
                    }
                    else if (Transform == 2)
                    {
                        if (Eating >= 1)
                        {
                            Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/Saria3Eat");
                            Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                            int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
                            int frameY = frameHeight * base.Projectile.frame;
                            Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                            Vector2 origin = rectangle.Size() / 2f;
                            float rotation = base.Projectile.rotation;
                            float scale = base.Projectile.scale;
                            SpriteEffects spriteEffects = SpriteEffects.None;
                            startPos.Y += 1;
                            startPos.X += +17;

                            Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(lightColor), rotation, origin, scale, spriteEffects, 0f);
                        }
                        if (base.Projectile.spriteDirection == 1 && Eating <= 0)
                        {
                            Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/Saria3");
                            Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                            int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
                            int frameY = frameHeight * base.Projectile.frame;
                            Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                            Vector2 origin = rectangle.Size() / 2f;
                            float rotation = base.Projectile.rotation;
                            float scale = base.Projectile.scale;
                            SpriteEffects spriteEffects = SpriteEffects.None;
                            startPos.Y += 1;
                            startPos.X += +17;
                            Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(lightColor), rotation, origin, scale, spriteEffects, 0f);
                            Projectile.netUpdate = true;
                        }
                        if (base.Projectile.spriteDirection == -1 && Eating <= 0)
                        {
                            Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/Saria3R");
                            Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                            int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
                            int frameY = frameHeight * base.Projectile.frame;
                            Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                            Vector2 origin = rectangle.Size() / 2f;
                            float rotation = base.Projectile.rotation;
                            float scale = base.Projectile.scale;
                            SpriteEffects spriteEffects = SpriteEffects.None;
                            startPos.Y += 1;
                            startPos.X += +17;

                            Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(lightColor), rotation, origin, scale, spriteEffects, 0f);
                            Projectile.netUpdate = true;
                        }
                    }
                    else if (Transform == 3)
                    {
                        if (Eating >= 1)
                        {
                            Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/Saria4Eat");
                            Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                            int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
                            int frameY = frameHeight * base.Projectile.frame;
                            Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                            Vector2 origin = rectangle.Size() / 2f;
                            float rotation = base.Projectile.rotation;
                            float scale = base.Projectile.scale;
                            SpriteEffects spriteEffects = SpriteEffects.None;
                            startPos.Y += 1;
                            startPos.X += +17;

                            Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(lightColor), rotation, origin, scale, spriteEffects, 0f);
                        }
                        if (base.Projectile.spriteDirection == 1 && Eating <= 0)
                        {
                            Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/Saria4");
                            Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                            int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
                            int frameY = frameHeight * base.Projectile.frame;
                            Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                            Vector2 origin = rectangle.Size() / 2f;
                            float rotation = base.Projectile.rotation;
                            float scale = base.Projectile.scale;
                            SpriteEffects spriteEffects = SpriteEffects.None;
                            startPos.Y += 1;
                            startPos.X += +17;
                            Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(lightColor), rotation, origin, scale, spriteEffects, 0f);
                        }
                        if (base.Projectile.spriteDirection == -1 && Eating <= 0)
                        {
                            Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/Saria4R");
                            Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                            int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
                            int frameY = frameHeight * base.Projectile.frame;
                            Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                            Vector2 origin = rectangle.Size() / 2f;
                            float rotation = base.Projectile.rotation;
                            float scale = base.Projectile.scale;
                            SpriteEffects spriteEffects = SpriteEffects.None;
                            startPos.Y += 1;
                            startPos.X += +17;

                            Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(lightColor), rotation, origin, scale, spriteEffects, 0f);
                        }
                    }
                    else if (Transform == 4)
                    {
                        if (Eating >= 1)
                        {
                            Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/Saria5Eat");
                            Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                            int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
                            int frameY = frameHeight * base.Projectile.frame;
                            Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                            Vector2 origin = rectangle.Size() / 2f;
                            float rotation = base.Projectile.rotation;
                            float scale = base.Projectile.scale;
                            SpriteEffects spriteEffects = SpriteEffects.None;
                            startPos.Y += 1;
                            startPos.X += +17;

                            Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(lightColor), rotation, origin, scale, spriteEffects, 0f);
                        }
                        if (base.Projectile.spriteDirection == 1 && Eating <= 0)
                        {
                            Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/Saria5");
                            Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                            int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
                            int frameY = frameHeight * base.Projectile.frame;
                            Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                            Vector2 origin = rectangle.Size() / 2f;
                            float rotation = base.Projectile.rotation;
                            float scale = base.Projectile.scale;
                            SpriteEffects spriteEffects = SpriteEffects.None;
                            startPos.Y += 1;
                            startPos.X += +17;
                            Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(lightColor), rotation, origin, scale, spriteEffects, 0f);
                        }
                        if (base.Projectile.spriteDirection == -1 && Eating <= 0)
                        {
                            Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/Saria5R");
                            Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                            int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
                            int frameY = frameHeight * base.Projectile.frame;
                            Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                            Vector2 origin = rectangle.Size() / 2f;
                            float rotation = base.Projectile.rotation;
                            float scale = base.Projectile.scale;
                            SpriteEffects spriteEffects = SpriteEffects.None;
                            startPos.Y += 1;
                            startPos.X += +17;

                            Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(lightColor), rotation, origin, scale, spriteEffects, 0f);
                        }
                    }
                    else if (Transform == 5)
                    {
                        if (Eating >= 1)
                        {
                            Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/Saria6Eat");
                            Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                            int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
                            int frameY = frameHeight * base.Projectile.frame;
                            Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                            Vector2 origin = rectangle.Size() / 2f;
                            float rotation = base.Projectile.rotation;
                            float scale = base.Projectile.scale;
                            SpriteEffects spriteEffects = SpriteEffects.None;
                            startPos.Y += 1;
                            startPos.X += +17;

                            Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(lightColor), rotation, origin, scale, spriteEffects, 0f);
                        }
                        if (base.Projectile.spriteDirection == 1 && Eating <= 0)
                        {
                            Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/Saria6");
                            Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                            int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
                            int frameY = frameHeight * base.Projectile.frame;
                            Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                            Vector2 origin = rectangle.Size() / 2f;
                            float rotation = base.Projectile.rotation;
                            float scale = base.Projectile.scale;
                            SpriteEffects spriteEffects = SpriteEffects.None;
                            startPos.Y += 1;
                            startPos.X += +17;
                            Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(lightColor), rotation, origin, scale, spriteEffects, 0f);
                        }
                        if (base.Projectile.spriteDirection == -1 && Eating <= 0)
                        {
                            Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/Saria6R");
                            Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                            int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
                            int frameY = frameHeight * base.Projectile.frame;
                            Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                            Vector2 origin = rectangle.Size() / 2f;
                            float rotation = base.Projectile.rotation;
                            float scale = base.Projectile.scale;
                            SpriteEffects spriteEffects = SpriteEffects.None;
                            startPos.Y += 1;
                            startPos.X += +17;

                            Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(lightColor), rotation, origin, scale, spriteEffects, 0f);
                        }
                    }
                    else if (Transform == 6)
                    {
                        if (Eating >= 1)
                        {
                            Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/Saria7Eat");
                            Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                            int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
                            int frameY = frameHeight * base.Projectile.frame;
                            Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                            Vector2 origin = rectangle.Size() / 2f;
                            float rotation = base.Projectile.rotation;
                            float scale = base.Projectile.scale;
                            SpriteEffects spriteEffects = SpriteEffects.None;
                            startPos.Y += 1;
                            startPos.X += +17;

                            Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(lightColor), rotation, origin, scale, spriteEffects, 0f);
                        }
                        if (base.Projectile.spriteDirection == 1 && Eating <= 0)
                        {
                            Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/Saria7");
                            Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                            int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
                            int frameY = frameHeight * base.Projectile.frame;
                            Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                            Vector2 origin = rectangle.Size() / 2f;
                            float rotation = base.Projectile.rotation;
                            float scale = base.Projectile.scale;
                            SpriteEffects spriteEffects = SpriteEffects.None;
                            startPos.Y += 1;
                            startPos.X += +17;
                            Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(lightColor), rotation, origin, scale, spriteEffects, 0f);
                        }
                        if (base.Projectile.spriteDirection == -1 && Eating <= 0)
                        {
                            Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/Saria7R");
                            Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                            int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
                            int frameY = frameHeight * base.Projectile.frame;
                            Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                            Vector2 origin = rectangle.Size() / 2f;
                            float rotation = base.Projectile.rotation;
                            float scale = base.Projectile.scale;
                            SpriteEffects spriteEffects = SpriteEffects.None;
                            startPos.Y += 1;
                            startPos.X += +17;

                            Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(lightColor), rotation, origin, scale, spriteEffects, 0f);
                        }
                    }
                    ///////////faces
                    ///
                    if (Sleep <= 0 && Eating <= 0)
                    {
                        if (Cursed <= 0)
                        {
                            if (Transform != 7 && Transform != 6 && Mood > -1200 && Mood < 3600)
                            {
                                Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/SariaNormalFace");
                                Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                                int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
                                int frameY = frameHeight * base.Projectile.frame;
                                Color drawColor = Color.Lerp(lightColor, Color.LightPink, 20f);
                                drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                                Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                                Vector2 origin = rectangle.Size() / 2f;
                                float rotation = base.Projectile.rotation;
                                float scale = base.Projectile.scale;
                                SpriteEffects spriteEffects = SpriteEffects.None;
                                startPos.Y += 1;
                                startPos.X += +17;
                                if (base.Projectile.spriteDirection == -1)
                                {
                                    spriteEffects = SpriteEffects.FlipHorizontally;
                                }
                                Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                            }
                            if (Transform == 6 && Mood > -1200 && Mood < 3600)
                            {
                                Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/Saria7NormalFace");
                                Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                                int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
                                int frameY = frameHeight * base.Projectile.frame;
                                Color drawColor = Color.Lerp(lightColor, Color.LightBlue, 20f);
                                drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                                Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                                Vector2 origin = rectangle.Size() / 2f;
                                float rotation = base.Projectile.rotation;
                                float scale = base.Projectile.scale;
                                SpriteEffects spriteEffects = SpriteEffects.None;
                                startPos.Y += 1;
                                startPos.X += +17;
                                if (base.Projectile.spriteDirection == -1)
                                {
                                    spriteEffects = SpriteEffects.FlipHorizontally;
                                }
                                Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                            }
                            if (Transform != 7 && Transform != 6 && Mood >= 2400 && Mood < 3600)
                            {
                                Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/SariaHappy");
                                Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                                int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
                                int frameY = frameHeight * base.Projectile.frame;
                                Color drawColor = Color.Lerp(lightColor, Color.LightPink, 20f);
                                drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                                Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                                Vector2 origin = rectangle.Size() / 2f;
                                float rotation = base.Projectile.rotation;
                                float scale = base.Projectile.scale;
                                SpriteEffects spriteEffects = SpriteEffects.None;
                                startPos.Y += 1;
                                startPos.X += +17;
                                if (base.Projectile.spriteDirection == -1)
                                {
                                    spriteEffects = SpriteEffects.FlipHorizontally;
                                }
                                Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                            }
                            if (Transform == 6 && Mood >= 2400 && Mood < 3600)
                            {
                                Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/Saria7Happy");
                                Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                                int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
                                int frameY = frameHeight * base.Projectile.frame;
                                Color drawColor = Color.Lerp(lightColor, Color.LightBlue, 20f);
                                drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                                Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                                Vector2 origin = rectangle.Size() / 2f;
                                float rotation = base.Projectile.rotation;
                                float scale = base.Projectile.scale;
                                SpriteEffects spriteEffects = SpriteEffects.None;
                                startPos.Y += 1;
                                startPos.X += +17;
                                if (base.Projectile.spriteDirection == -1)
                                {
                                    spriteEffects = SpriteEffects.FlipHorizontally;
                                }
                                Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                            }
                            if (Transform != 6 && Transform != 7 && Mood >= 3600)
                            {
                                Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/SariaPumped");
                                Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                                int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
                                int frameY = frameHeight * base.Projectile.frame;
                                Color drawColor = Color.Lerp(lightColor, Color.LightBlue, 20f);
                                drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                                Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                                Vector2 origin = rectangle.Size() / 2f;
                                float rotation = base.Projectile.rotation;
                                float scale = base.Projectile.scale;
                                SpriteEffects spriteEffects = SpriteEffects.None;
                                startPos.Y += 1;
                                startPos.X += +17;
                                if (base.Projectile.spriteDirection == -1)
                                {
                                    spriteEffects = SpriteEffects.FlipHorizontally;
                                }
                                Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                            }
                            if (Transform == 6 && Mood >= 3600)
                            {
                                Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/Saria7Pumped");
                                Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                                int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
                                int frameY = frameHeight * base.Projectile.frame;
                                Color drawColor = Color.Lerp(lightColor, Color.LightBlue, 20f);
                                drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                                Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                                Vector2 origin = rectangle.Size() / 2f;
                                float rotation = base.Projectile.rotation;
                                float scale = base.Projectile.scale;
                                SpriteEffects spriteEffects = SpriteEffects.None;
                                startPos.Y += 1;
                                startPos.X += +17;
                                if (base.Projectile.spriteDirection == -1)
                                {
                                    spriteEffects = SpriteEffects.FlipHorizontally;
                                }
                                Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                            }
                            if (Transform != 6 && Transform != 7 && ((Mood <= -1200 && Mood > -2400) || Mood <= -3600))
                            {
                                Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/SariaSad");
                                Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                                int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
                                int frameY = frameHeight * base.Projectile.frame;
                                Color drawColor = Color.Lerp(lightColor, Color.LightPink, 20f);
                                drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                                Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                                Vector2 origin = rectangle.Size() / 2f;
                                float rotation = base.Projectile.rotation;
                                float scale = base.Projectile.scale;
                                SpriteEffects spriteEffects = SpriteEffects.None;
                                startPos.Y += 1;
                                startPos.X += +17;
                                if (base.Projectile.spriteDirection == -1)
                                {
                                    spriteEffects = SpriteEffects.FlipHorizontally;
                                }
                                Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                            }
                            if (Transform == 6 && ((Mood <= -1200 && Mood > -2400) || Mood <= -3600))
                            {
                                Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/Saria7Sad");
                                Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                                int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
                                int frameY = frameHeight * base.Projectile.frame;
                                Color drawColor = Color.Lerp(lightColor, Color.LightBlue, 20f);
                                drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                                Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                                Vector2 origin = rectangle.Size() / 2f;
                                float rotation = base.Projectile.rotation;
                                float scale = base.Projectile.scale;
                                SpriteEffects spriteEffects = SpriteEffects.None;
                                startPos.Y += 1;
                                startPos.X += +17;
                                if (base.Projectile.spriteDirection == -1)
                                {
                                    spriteEffects = SpriteEffects.FlipHorizontally;
                                }
                                Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                            }
                            if (Transform != 6 && Transform != 7 && Mood <= -2400 && Mood > -3600)
                            {
                                Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/SariaAngry");
                                Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                                int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
                                int frameY = frameHeight * base.Projectile.frame;
                                Color drawColor = Color.Lerp(lightColor, Color.LightBlue, 20f);
                                drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                                Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                                Vector2 origin = rectangle.Size() / 2f;
                                float rotation = base.Projectile.rotation;
                                float scale = base.Projectile.scale;
                                SpriteEffects spriteEffects = SpriteEffects.None;
                                startPos.Y += 1;
                                startPos.X += +17;
                                if (base.Projectile.spriteDirection == -1)
                                {
                                    spriteEffects = SpriteEffects.FlipHorizontally;
                                }
                                Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                            }
                            if (Transform == 6 && Mood <= -2400 && Mood > -3600)
                            {
                                Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/Saria7Angry");
                                Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                                int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
                                int frameY = frameHeight * base.Projectile.frame;
                                Color drawColor = Color.Lerp(lightColor, Color.LightBlue, 20f);
                                drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                                Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                                Vector2 origin = rectangle.Size() / 2f;
                                float rotation = base.Projectile.rotation;
                                float scale = base.Projectile.scale;
                                SpriteEffects spriteEffects = SpriteEffects.None;
                                startPos.Y += 1;
                                startPos.X += +17;
                                if (base.Projectile.spriteDirection == -1)
                                {
                                    spriteEffects = SpriteEffects.FlipHorizontally;
                                }
                                Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                            }
                            if (Projectile.direction == 1)
                            {
                                Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/SariaArm2");
                                Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                                int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
                                int frameY = frameHeight * base.Projectile.frame;
                                Color drawColor = Color.Lerp(lightColor, Color.LightPink, 20f);
                                drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                                Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                                Vector2 origin = rectangle.Size() / 2f;
                                float rotation = base.Projectile.rotation;
                                float scale = base.Projectile.scale;
                                SpriteEffects spriteEffects = SpriteEffects.None;
                                startPos.Y += 1;
                                startPos.X += +17;

                                Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                            }
                            if (Projectile.direction == -1)
                            {
                                Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/SariaArmAttackR");
                                Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                                int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
                                int frameY = frameHeight * base.Projectile.frame;
                                Color drawColor = Color.Lerp(lightColor, Color.LightPink, 20f);
                                drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                                Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                                Vector2 origin = rectangle.Size() / 2f;
                                float rotation = base.Projectile.rotation;
                                float scale = base.Projectile.scale;
                                SpriteEffects spriteEffects = SpriteEffects.None;
                                startPos.Y += 1;
                                startPos.X += +17;

                                Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                            }
                        }
                        if (Cursed >= 1)
                        {
                            {
                                Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/SariaShader");
                                Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                                int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
                                int frameY = frameHeight * base.Projectile.frame;
                                Color drawColor = Color.Lerp(lightColor, Color.LightBlue, 20f);
                                drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                                Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                                Vector2 origin = rectangle.Size() / 2f;
                                float rotation = base.Projectile.rotation;
                                float scale = base.Projectile.scale;
                                SpriteEffects spriteEffects = SpriteEffects.None;
                                startPos.Y += 1;
                                startPos.X += +17;
                                if (base.Projectile.spriteDirection == -1)
                                {
                                    spriteEffects = SpriteEffects.FlipHorizontally;
                                }
                                Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                            }
                            {
                                Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/SariaCursed");
                                Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                                int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
                                int frameY = frameHeight * base.Projectile.frame;
                                Color drawColor = Color.Lerp(lightColor, Color.LightBlue, 20f);
                                drawColor = Color.Lerp(drawColor, Color.MediumVioletRed, 0);
                                Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                                Vector2 origin = rectangle.Size() / 2f;
                                float rotation = base.Projectile.rotation;
                                float scale = base.Projectile.scale;
                                SpriteEffects spriteEffects = SpriteEffects.None;
                                startPos.Y += 1;
                                startPos.X += +17;
                                if (base.Projectile.spriteDirection == -1)
                                {
                                    spriteEffects = SpriteEffects.FlipHorizontally;
                                }
                                Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                            }
                        }
                    }
                    if (Eating >= 1)
                    {
                        Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/SariaEatEyes");
                        Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                        int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
                        int frameY = frameHeight * base.Projectile.frame;
                        Color drawColor = Color.Lerp(lightColor, Color.LightBlue, 20f);
                        drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                        Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                        Vector2 origin = rectangle.Size() / 2f;
                        float rotation = base.Projectile.rotation;
                        float scale = base.Projectile.scale;
                        SpriteEffects spriteEffects = SpriteEffects.None;
                        startPos.Y += 1;
                        startPos.X += +17;

                        Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                    }
                    if (Eating == 1)
                    {
                        Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/SariaEat3");
                        Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                        int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
                        int frameY = frameHeight * base.Projectile.frame;
                        Color drawColor = Color.Lerp(lightColor, Color.LightBlue, 20f);
                        drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                        Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                        Vector2 origin = rectangle.Size() / 2f;
                        float rotation = base.Projectile.rotation;
                        float scale = base.Projectile.scale;
                        SpriteEffects spriteEffects = SpriteEffects.None;
                        startPos.Y += 1;
                        startPos.X += +17;

                        Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                    }
                    if (Eating == 2)
                    {
                        Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/SariaEat2");
                        Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                        int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
                        int frameY = frameHeight * base.Projectile.frame;
                        Color drawColor = Color.Lerp(lightColor, Color.LightBlue, 20f);
                        drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                        Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                        Vector2 origin = rectangle.Size() / 2f;
                        float rotation = base.Projectile.rotation;
                        float scale = base.Projectile.scale;
                        SpriteEffects spriteEffects = SpriteEffects.None;
                        startPos.Y += 1;
                        startPos.X += +17;

                        Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                    }
                    if (Sleep > 0 && Transform != 6 && Transform != 7)
                    {
                        Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/SariaSleep");
                        Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                        int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
                        int frameY = frameHeight * base.Projectile.frame;
                        Color drawColor = Color.Lerp(lightColor, Color.LightPink, 20f);
                        drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                        Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                        Vector2 origin = rectangle.Size() / 2f;
                        float rotation = base.Projectile.rotation;
                        float scale = base.Projectile.scale;
                        SpriteEffects spriteEffects = SpriteEffects.None;
                        startPos.Y += 1;
                        startPos.X += +17;
                        if (base.Projectile.spriteDirection == -1)
                        {
                            spriteEffects = SpriteEffects.FlipHorizontally;
                        }
                        Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                    }
                    if (Sleep > 0 && Transform == 6)
                    {
                        Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/Saria7Sleep");
                        Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                        int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
                        int frameY = frameHeight * base.Projectile.frame;
                        Color drawColor = Color.Lerp(lightColor, Color.LightPink, 20f);
                        drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                        Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                        Vector2 origin = rectangle.Size() / 2f;
                        float rotation = base.Projectile.rotation;
                        float scale = base.Projectile.scale;
                        SpriteEffects spriteEffects = SpriteEffects.None;
                        startPos.Y += 1;
                        startPos.X += +17;
                        if (base.Projectile.spriteDirection == -1)
                        {
                            spriteEffects = SpriteEffects.FlipHorizontally;
                        }
                        Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                    }
                    /////XP Bars
                    if (XpTimer >= 1)
                    {
                        if (modPlayer.XPBarLevel == 0)
                        {
                            Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/SariaXPBar1");
                            Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                            int frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<SariaXPBar1>()];
                            int frameY = frameHeight;
                            Color drawColor = Color.Lerp(lightColor, Color.LightPink, 20f);
                            drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                            Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                            Vector2 origin = rectangle.Size() / 2f;
                            float rotation = base.Projectile.rotation;
                            float scale = base.Projectile.scale;
                            SpriteEffects spriteEffects = SpriteEffects.None;
                            startPos.Y += 60;
                            startPos.X += +17;
                            Main.spriteBatch.Draw(texture, startPos, null, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                        }
                        if (modPlayer.XPBarLevel == 1)
                        {
                            Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/SariaXPBar2");
                            Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                            int frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<SariaXPBar2>()];
                            int frameY = frameHeight;
                            Color drawColor = Color.Lerp(lightColor, Color.LightPink, 20f);
                            drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                            Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                            Vector2 origin = rectangle.Size() / 2f;
                            float rotation = base.Projectile.rotation;
                            float scale = base.Projectile.scale;
                            SpriteEffects spriteEffects = SpriteEffects.None;
                            startPos.Y += 60;
                            startPos.X += +17;
                            Main.spriteBatch.Draw(texture, startPos, null, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                        }
                        if (modPlayer.XPBarLevel == 2)
                        {
                            Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/SariaXPBar3");
                            Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                            int frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<SariaXPBar3>()];
                            int frameY = frameHeight;
                            Color drawColor = Color.Lerp(lightColor, Color.LightPink, 20f);
                            drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                            Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                            Vector2 origin = rectangle.Size() / 2f;
                            float rotation = base.Projectile.rotation;
                            float scale = base.Projectile.scale;
                            SpriteEffects spriteEffects = SpriteEffects.None;
                            startPos.Y += 60;
                            startPos.X += +17;
                            Main.spriteBatch.Draw(texture, startPos, null, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                        }
                        if (modPlayer.XPBarLevel == 3)
                        {
                            Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/SariaXPBar4");
                            Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                            int frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<SariaXPBar4>()];
                            int frameY = frameHeight;
                            Color drawColor = Color.Lerp(lightColor, Color.LightPink, 20f);
                            drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                            Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                            Vector2 origin = rectangle.Size() / 2f;
                            float rotation = base.Projectile.rotation;
                            float scale = base.Projectile.scale;
                            SpriteEffects spriteEffects = SpriteEffects.None;
                            startPos.Y += 60;
                            startPos.X += +17;
                            Main.spriteBatch.Draw(texture, startPos, null, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                        }
                        if (modPlayer.XPBarLevel == 4)
                        {
                            Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/SariaXPBar5");
                            Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                            int frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<SariaXPBar5>()];
                            int frameY = frameHeight;
                            Color drawColor = Color.Lerp(lightColor, Color.LightPink, 20f);
                            drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                            Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                            Vector2 origin = rectangle.Size() / 2f;
                            float rotation = base.Projectile.rotation;
                            float scale = base.Projectile.scale;
                            SpriteEffects spriteEffects = SpriteEffects.None;
                            startPos.Y += 60;
                            startPos.X += +17;
                            Main.spriteBatch.Draw(texture, startPos, null, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                        }
                        if (modPlayer.XPBarLevel == 5)
                        {
                            Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/SariaXPBar6");
                            Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                            int frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<SariaXPBar6>()];
                            int frameY = frameHeight;
                            Color drawColor = Color.Lerp(lightColor, Color.LightPink, 20f);
                            drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                            Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                            Vector2 origin = rectangle.Size() / 2f;
                            float rotation = base.Projectile.rotation;
                            float scale = base.Projectile.scale;
                            SpriteEffects spriteEffects = SpriteEffects.None;
                            startPos.Y += 60;
                            startPos.X += +17;
                            Main.spriteBatch.Draw(texture, startPos, null, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                        }
                        if (modPlayer.XPBarLevel == 6)
                        {
                            Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/SariaXPBar7");
                            Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                            int frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<SariaXPBar7>()];
                            int frameY = frameHeight;
                            Color drawColor = Color.Lerp(lightColor, Color.LightPink, 20f);
                            drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                            Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                            Vector2 origin = rectangle.Size() / 2f;
                            float rotation = base.Projectile.rotation;
                            float scale = base.Projectile.scale;
                            SpriteEffects spriteEffects = SpriteEffects.None;
                            startPos.Y += 60;
                            startPos.X += +17;
                            Main.spriteBatch.Draw(texture, startPos, null, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                        }
                        if (modPlayer.XPBarLevel == 7)
                        {
                            Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/SariaXPBar8");
                            Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                            int frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<SariaXPBar8>()];
                            int frameY = frameHeight;
                            Color drawColor = Color.Lerp(lightColor, Color.LightPink, 20f);
                            drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                            Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                            Vector2 origin = rectangle.Size() / 2f;
                            float rotation = base.Projectile.rotation;
                            float scale = base.Projectile.scale;
                            SpriteEffects spriteEffects = SpriteEffects.None;
                            startPos.Y += 60;
                            startPos.X += +17;
                            Main.spriteBatch.Draw(texture, startPos, null, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                        }
                        if (modPlayer.XPBarLevel == 8)
                        {
                            Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/SariaXPBar9");
                            Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                            int frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<SariaXPBar9>()];
                            int frameY = frameHeight;
                            Color drawColor = Color.Lerp(lightColor, Color.LightPink, 20f);
                            drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                            Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                            Vector2 origin = rectangle.Size() / 2f;
                            float rotation = base.Projectile.rotation;
                            float scale = base.Projectile.scale;
                            SpriteEffects spriteEffects = SpriteEffects.None;
                            startPos.Y += 60;
                            startPos.X += +17;
                            Main.spriteBatch.Draw(texture, startPos, null, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                        }
                        if (modPlayer.Sarialevel == 0)
                        {
                            Texture2D texture = TextureAssets.Projectile[ModContent.ProjectileType<KingSlime>()].Value;
                            Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                            int frameHeight = texture.Height;
                            int frameY = frameHeight;
                            Color drawColor = Color.Lerp(lightColor, Color.LightPink, 20f);
                            drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                            Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                            Vector2 origin = rectangle.Size() / 2f;
                            float rotation = base.Projectile.rotation;
                            float scale = base.Projectile.scale;
                            SpriteEffects spriteEffects = SpriteEffects.None;
                            startPos.Y += 60;
                            startPos.X += +60;
                            Main.spriteBatch.Draw(texture, startPos, null, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                        }
                        if (modPlayer.Sarialevel == 1)
                        {
                            Texture2D texture = TextureAssets.Projectile[ModContent.ProjectileType<QueenBee>()].Value;
                            Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                            int frameHeight = texture.Height;
                            int frameY = frameHeight;
                            Color drawColor = Color.Lerp(lightColor, Color.LightPink, 20f);
                            drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                            Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                            Vector2 origin = rectangle.Size() / 2f;
                            float rotation = base.Projectile.rotation;
                            float scale = base.Projectile.scale;
                            SpriteEffects spriteEffects = SpriteEffects.None;
                            startPos.Y += 60;
                            startPos.X += +60;
                            Main.spriteBatch.Draw(texture, startPos, null, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                        }
                        if (modPlayer.Sarialevel == 2)
                        {
                            Texture2D texture = TextureAssets.Projectile[ModContent.ProjectileType<WallOfFlesh>()].Value;
                            Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                            int frameHeight = texture.Height;
                            int frameY = frameHeight;
                            Color drawColor = Color.Lerp(lightColor, Color.LightPink, 20f);
                            drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                            Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                            Vector2 origin = rectangle.Size() / 2f;
                            float rotation = base.Projectile.rotation;
                            float scale = base.Projectile.scale;
                            SpriteEffects spriteEffects = SpriteEffects.None;
                            startPos.Y += 60;
                            startPos.X += +60;
                            Main.spriteBatch.Draw(texture, startPos, null, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                        }
                        if (modPlayer.Sarialevel == 3)
                        {
                            Texture2D texture = TextureAssets.Projectile[ModContent.ProjectileType<Retinazer>()].Value;
                            Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                            int frameHeight = texture.Height;
                            int frameY = frameHeight;
                            Color drawColor = Color.Lerp(lightColor, Color.LightPink, 20f);
                            drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                            Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                            Vector2 origin = rectangle.Size() / 2f;
                            float rotation = base.Projectile.rotation;
                            float scale = base.Projectile.scale;
                            SpriteEffects spriteEffects = SpriteEffects.None;
                            startPos.Y += 60;
                            startPos.X += +60;
                            Main.spriteBatch.Draw(texture, startPos, null, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                        }
                        if (modPlayer.Sarialevel == 4)
                        {
                            Texture2D texture = TextureAssets.Projectile[ModContent.ProjectileType<Plantera>()].Value;
                            Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                            int frameHeight = texture.Height;
                            int frameY = frameHeight;
                            Color drawColor = Color.Lerp(lightColor, Color.LightPink, 20f);
                            drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                            Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                            Vector2 origin = rectangle.Size() / 2f;
                            float rotation = base.Projectile.rotation;
                            float scale = base.Projectile.scale;
                            SpriteEffects spriteEffects = SpriteEffects.None;
                            startPos.Y += 60;
                            startPos.X += +60;
                            Main.spriteBatch.Draw(texture, startPos, null, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                        }
                        if (modPlayer.Sarialevel == 5)
                        {
                            Texture2D texture = TextureAssets.Projectile[ModContent.ProjectileType<TheDuke>()].Value;
                            Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                            int frameHeight = texture.Height;
                            int frameY = frameHeight;
                            Color drawColor = Color.Lerp(lightColor, Color.LightPink, 20f);
                            drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                            Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                            Vector2 origin = rectangle.Size() / 2f;
                            float rotation = base.Projectile.rotation;
                            float scale = base.Projectile.scale;
                            SpriteEffects spriteEffects = SpriteEffects.None;
                            startPos.Y += 60;
                            startPos.X += +60;
                            Main.spriteBatch.Draw(texture, startPos, null, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                        }
                    }
                }

            }



        }
    }



}