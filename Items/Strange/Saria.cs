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
        public int Transform; //used for when saria changes forms
        private int BugTimer; //used for sarias amber form
        private int SwarmTimer;
        private int Mood;
        private int MoveTimer;
        private int SleepHeal;
        private int Sleep;
        private int TimeAsleep;
        private int XpTimer;
        private int Cursed;
        public int ChannelTime; //time the player actually channels
        private int BiomeTime; //short downtime from biome weakness reset
        public int ChannelState; //used for when she is actually using charge animation
        public int IsCharging; //tells when saria is actually in the charging animation state
        private int ChannelAttack;
        private int Eating;
        private int Holding;
        private int ToEat;
        private int CanMove;
        private int SpecialAnimate;//for things like electric Saria's Electric mask animation
        private int SoundTimer;
        private int SoundTimer2;
        private int IsPlayerAsleep;
        private int CantAttackTimer; //used to time how long she cant attack for
        private int CantAttack;// used for when she should not be able to attack between 0 and 1

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
            writer.Write(Projectile.spriteDirection);
            writer.Write(Projectile.frame);
            writer.Write(Projectile.frameCounter);
            writer.Write(Transform);
            writer.Write(IsPlayerAsleep);
            writer.Write(MoveTimer);
            writer.Write(SoundTimer);
            writer.Write(SoundTimer2);
            writer.Write(TimeAsleep);
            writer.Write(Mood);
            writer.Write(Cursed);
            writer.Write(XpTimer);
            writer.Write(Sleep);
            writer.Write(SleepHeal);
            writer.Write(SpecialAnimate);
            writer.Write(BugTimer);
            writer.Write(ChannelTime);
            writer.Write(BiomeTime);
            writer.Write(ChannelState);
            writer.Write(IsCharging);
            writer.Write(ChannelAttack);
            writer.Write(Eating);
            writer.Write(Holding);
            writer.Write(ToEat);
            writer.Write(CanMove);
            writer.Write(CantAttack);
            writer.Write(CantAttackTimer);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            Projectile.spriteDirection = (int)reader.ReadInt32();
            Projectile.frame = (int)reader.ReadInt32();
            Projectile.frameCounter = (int)reader.ReadInt32();
            Transform = (int)reader.ReadInt32();
            IsPlayerAsleep = (int)reader.ReadInt32();
            MoveTimer = (int)reader.ReadInt32();
            SoundTimer = (int)reader.ReadInt32();
            SoundTimer2 = (int)reader.ReadInt32();
            TimeAsleep = (int)reader.ReadInt32();
            Mood = (int)reader.ReadInt32();
            SpecialAnimate = (int)reader.ReadInt32();
            Cursed = (int)reader.ReadInt32();
            XpTimer = (int)reader.ReadInt32();
            Sleep = (int)reader.ReadInt32();
            SleepHeal = (int)reader.ReadInt32();
            BugTimer = (int)reader.ReadInt32();
            BiomeTime = (int)reader.ReadInt32();
            ChannelTime = (int)reader.ReadInt32();
            ChannelState = (int)reader.ReadInt32();
            IsCharging = (int)reader.ReadInt32();
            ChannelAttack = (int)reader.ReadInt32();
            Eating = (int)reader.ReadInt32();
            Holding = (int)reader.ReadInt32();
            ToEat = (int)reader.ReadInt32();
            CanMove = (int)reader.ReadInt32();
            CantAttack = (int)reader.ReadInt32();
            CantAttackTimer = (int)reader.ReadInt32();
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
            modPlayer.SariaXp += 2;
            int myPlayer = Main.myPlayer;
            if (Main.player[myPlayer].position.X + (float)(Main.player[myPlayer].width / 2) < Projectile.position.X + (float)(Projectile.width / 2))
            {
                hitDirection = 1;
            }
            else
            {
                hitDirection = -1;
            }
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
                    if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.position.X + 10, target.position.Y + 2, 0, 0, ModContent.ProjectileType<ShadowClaw>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
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
            ///Main.NewText(Projectile.frame);

            {
                Player player = Main.player[base.Projectile.owner];
                Player player2 = Main.LocalPlayer;
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
                                SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Pokeball"), Main.projectile[i].Center);
                                if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Main.projectile[i].position.X + 0, Main.projectile[i].position.Y + 0, 0, 0, ModContent.ProjectileType<GreenMothGoliath2>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                            }

                        }

                    }
                }
                if (CantAttack >= 1 && CantAttackTimer <= 0)
                {
                    if (Projectile.spriteDirection == 1)
                    {
                        for (int i = 0; i < 50; i++)
                        {
                            Vector2 dustspeed5 = Main.rand.NextVector2CircularEdge(1f, 1f);
                            Dust d = Dust.NewDustPerfect(Projectile.Right, ModContent.DustType<AbsorbPsychic>(), dustspeed5 * -5, Scale: 1.5f);
                            d.noGravity = true;
                        }
                        SoundEngine.PlaySound(SoundID.MaxMana, base.Projectile.Center);
                        CantAttack = 0;
                    }
                    if (Projectile.spriteDirection == -1)
                    {
                        for (int i = 0; i < 50; i++)
                        {
                            Vector2 dustspeed5 = Main.rand.NextVector2CircularEdge(1f, 1f);
                            Dust d = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<AbsorbPsychic>(), dustspeed5 * -5, Scale: 1.5f);
                            d.noGravity = true;
                        }
                        SoundEngine.PlaySound(SoundID.MaxMana, base.Projectile.Center);
                        CantAttack = 0;
                    }
                }
                ///Channeling
                if (player.channel == true && player.HeldItem.type == ModContent.ItemType<HealBall>() && Main.myPlayer == Projectile.owner && !Main.mouseRight)
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
                                if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Main.projectile[i].position.X + 16, Main.projectile[i].position.Y + 16, 0, 0, ModContent.ProjectileType<LightningLocator>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
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
                    Projectile.damage = 10 + (modPlayer.SariaXp / 600);
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
                        if (Main.projectile[i].active && i != base.Projectile.whoAmI && ChannelTime > 0 && ((Main.projectile[i].type == VeilBubble && Main.projectile[i].owner == owner)))
                        {

                            if (ChannelTime <= 20)
                            {
                                if (CantAttackTimer > 0 && Projectile.frame >= 84 && Projectile.frame < 96)
                                {
                                    Projectile.frame = 96;
                                }
                                BiomeTime = 100;
                                CantAttackTimer = 120;
                                Main.projectile[i].Kill();
                                Transform++;
                                Projectile.netUpdate = true;
                                ChannelTime = 0;

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
                            else if (ChannelTime > 20 && Eating <= 0 && Sleep <= 0)
                            {
                                ChannelTime = 0;
                                ChannelState = 0;
                                if (Transform == 2)
                                {
                                    CantAttackTimer = 200;
                                }
                                if (CantAttackTimer <= 0 && Transform != 2)
                                {
                                    ChannelAttack = 1;
                                    Projectile.ai[0] = 1;
                                }
                            }

                        }

                    }

                }
                float between3 = Vector2.Distance(player.Center, Projectile.Center);
                if (player.statLife < player.statLifeMax2 && between3 <= 500 && Transform == 1 && Projectile.ai[0] == 0 && Sleep <= 0 && ChannelTime <= 0 && Eating <= 0)
                {
                    if (CantAttackTimer <= 0)
                    {
                        Projectile.ai[0] = 1;
                    }
                }


                if (player.HeldItem.type != ModContent.ItemType<HealBall>() && CantAttackTimer < 100)
                {
                    CantAttackTimer = 100;
                }

                if (ChannelTime > 20 && Eating <= 0 && Sleep <= 0 && player.channel == true && player.HeldItem.type == ModContent.ItemType<HealBall>() && CantAttackTimer <= 0 && !Main.mouseRight)
                {
                    ChannelState++;

                }
                {
                    if (ChannelState > 20 && Eating <= 0 && Sleep <= 0)
                    {
                        IsCharging = 1;
                    }
                    else
                    {
                        IsCharging = 0;
                    }
                }
                if (CantAttackTimer > 0)
                {
                    CantAttackTimer--;
                }

                if (BiomeTime > 0)
                {
                    BiomeTime--;
                }
                if (Transform == 5)
                {
                    BugTimer--;
                    {
                        if (BugTimer <= 0)
                        {
                            BugTimer = 500;
                            Projectile.netUpdate = true;
                        }
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
                if (Mood <= -2400)
                {
                    if (!player.HasBuff(ModContent.BuffType<Soothing>()) && !player.HasBuff(ModContent.BuffType<Sickness>()))
                    {
                        if (Main.myPlayer == Projectile.owner) player.AddBuff(ModContent.BuffType<Sickness>(), 30000);

                    }
                }

                if (Main.rand.NextBool(550) && SpecialAnimate <= 0)
                {
                    SpecialAnimate = 60;
                }
                if (SpecialAnimate > 0)
                {
                    SpecialAnimate--;
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
                        MoveTimer += 1;
                        Projectile.netUpdate = true;
                    }
                    if (Mood < 0)
                    {
                        MoveTimer += 4;
                        Projectile.netUpdate = true;
                    }
                }
                if (SoundTimer2 > 0)
                {
                    SoundTimer2--;
                }
                if (MoveTimer == 0)
                {
                    TimeAsleep = 0;
                    SleepHeal = 0;
                    Projectile.netUpdate = true;
                }
                if (TimeAsleep >= 200 && SleepHeal <= 0)
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
                        if (SoundTimer <= 0)
                        {
                            SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/StatRaise"), Projectile.Center);
                            for (int j = 0; j < 1; j++) //set to 2
                            {
                                if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + 0, 0, 0, ModContent.ProjectileType<PowerUp>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                            }
                            SoundTimer = 1;
                        }

                        Mood = 600;
                        if (IsPlayerAsleep <= 0)
                        {
                            MoveTimer = 0;
                            SoundTimer = 0;
                        }
                        Projectile.netUpdate = true;
                    }
                }
                if (player.HasBuff(ModContent.BuffType<Soothing>()) && player.HasBuff(ModContent.BuffType<Sickness>()))
                {
                    player.ClearBuff(ModContent.BuffType<Sickness>());
                }
                /////////////// End of Transformation Timer
                ///




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
                if ((player.ownedProjectileCounts[ModContent.ProjectileType<FrozenYogurtSignal>()] >= 1f) && Mood < 600)
                {
                    player.AddBuff(ModContent.BuffType<Soothing>(), 9000);
                    Mood = 600;
                    Projectile.netUpdate = true;
                    if ((player.ownedProjectileCounts[ModContent.ProjectileType<Happiness>()] <= 0f))
                    {
                        if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<Happiness>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                    }
                }

                if (Sleep <= 0 && player.statLife == player.statLifeMax2 && (Projectile.frame >= 20 && Projectile.frame <= 60 && Projectile.ai[0] == 0 && (player.ownedProjectileCounts[ModContent.ProjectileType<SmileTime>()] <= 0f) && (!player.HasBuff(ModContent.BuffType<Sickness>()) && (!player.HasBuff(ModContent.BuffType<BloodmoonBuff>()) && (!player.HasBuff(ModContent.BuffType<StatLower>()) && (player.ownedProjectileCounts[ModContent.ProjectileType<Happiness>()] <= 0f) && (player.ownedProjectileCounts[ModContent.ProjectileType<Anger>()] <= 0f) && (player.ownedProjectileCounts[ModContent.ProjectileType<Sad>()] <= 0f) && (player.ownedProjectileCounts[ModContent.ProjectileType<Smile>()] <= 0f) && player.velocity.X == 0)) && Projectile.spriteDirection != player.direction && (distanceToIdlePosition3 <= 10))) && Main.myPlayer == Projectile.owner)
                {
                    float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius3 * sphereRadius3));
                    double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                    Mood += 600;
                    Projectile.netUpdate = true;
                    if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<SmileTime>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                    if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<Smile>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                    Dust.NewDust(new Vector2((Projectile.Center.X + dustspeed) + radius * (float)Math.Cos(angle), (Projectile.Center.Y - 10) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<HeartDust>(), 0f, 0f, 0, default(Color), 1.5f);
                }
                if ((player.ownedProjectileCounts[ModContent.ProjectileType<Smile>()] >= 1f) && (player.ownedProjectileCounts[ModContent.ProjectileType<Anger>()] <= 0f) && Projectile.spriteDirection == player.direction && Main.myPlayer == Projectile.owner)
                {
                    if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<Anger>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                }
                if ((player.HasBuff(ModContent.BuffType<Sickness>())))
                {

                    Mood = -4800;
                    Projectile.netUpdate = true;
                    if (player.ownedProjectileCounts[ModContent.ProjectileType<Sad>()] <= 0f && (player.ownedProjectileCounts[ModContent.ProjectileType<period>()] <= 0f) && Main.myPlayer == Projectile.owner)
                    {
                        {
                            if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<Sad>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                            if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<period>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                        }
                    }
                }
                if (player.ownedProjectileCounts[ModContent.ProjectileType<Sad2>()] <= 0f && (player.ownedProjectileCounts[ModContent.ProjectileType<period>()] <= 0f) && player.HasBuff(ModContent.BuffType<Extinguished>()) && Main.myPlayer == Projectile.owner)
                {
                    {
                        if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<Sad2>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                        if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<period>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                    }
                }

                if (player.ownedProjectileCounts[ModContent.ProjectileType<Competitivetime>()] == 1f && player.ownedProjectileCounts[ModContent.ProjectileType<Competitive>()] <= 0f && Main.myPlayer == Projectile.owner)
                {
                    {
                        SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/StatRaise"), Projectile.Center);
                        for (int j = 0; j < 1; j++) //set to 2
                        {
                            if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<PowerUp>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                        }
                        if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<Competitive>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
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

                if (Projectile.localAI[0] == 0f && Main.myPlayer == Projectile.owner)
                {

                    Projectile.Fairy().spawnedPlayerMinionProjectileDamageValue = Projectile.damage;
                    for (int j = 0; j < 1; j++) //set to 2
                    {
                        if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<PowerUp>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                        if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<Ztarget>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                        CantAttackTimer = 120;
                    }
                    Projectile.localAI[0] = 1f;
                }
                ////Ztargets
                if (ChannelState > 40 && player.ownedProjectileCounts[ModContent.ProjectileType<Ztarget2>()] <= 0f && Transform == 0 && Main.myPlayer == Projectile.owner)
                {
                    if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Main.MouseWorld.X, Main.MouseWorld.Y, 0, 0, ModContent.ProjectileType<Ztarget2>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);

                }
                if (ChannelState > 40 && player.ownedProjectileCounts[ModContent.ProjectileType<Ztarget3>()] <= 0f && Transform == 1 && Main.myPlayer == Projectile.owner)
                {
                    if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Main.MouseWorld.X, Main.MouseWorld.Y, 0, 0, ModContent.ProjectileType<Ztarget3>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);

                }
                if (ChannelState > 40 && player.ownedProjectileCounts[ModContent.ProjectileType<Ztarget4>()] <= 0f && Transform == 2 && Main.myPlayer == Projectile.owner)
                {
                    if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), player.Center.X, player.Center.Y, 0, 0, ModContent.ProjectileType<Ztarget4>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);

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
                    if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<HealBallProjectile2>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);


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
                    if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 60, Projectile.position.Y + 30, 0, 0, ModContent.ProjectileType<HealBallProjectile2>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);

                    Projectile.Kill();
                }

                /// AiStuff
                NPC target = base.Projectile.Center.MinionHoming(500f, player);// the distance she targets enemies
                if (CantAttackTimer <= 0 && !(player.sleeping.isSleeping))
                {
                    if (target != null && Projectile.ai[0] == 0 && ChannelTime <= 20 && ChannelState <= 0 && Transform != 1 && Transform != 4 && Transform != 3 && Transform != 2 && Sleep <= 0 && ToEat <= 0 && Eating <= 0 && player.HeldItem.type == ModContent.ItemType<HealBall>())
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
                    else if (target != null && Projectile.ai[0] == 0 && ChannelTime <= 20 && ChannelState <= 0 && Transform != 1 && Transform == 2 && Transform != 4 && Transform != 3 && player.ownedProjectileCounts[ModContent.ProjectileType<RubyPsychicSeeker>()] <= 0f && Sleep <= 0 && ToEat <= 0 && Eating <= 0 && player.HeldItem.type == ModContent.ItemType<HealBall>())
                    {
                        Projectile.ai[0] = 1;
                    }
                    else if (target != null && Projectile.ai[0] == 0 && ChannelTime <= 20 && ChannelState <= 0 && Transform != 1 && Transform != 2 && Transform != 4 && Transform == 3 && player.ownedProjectileCounts[ModContent.ProjectileType<Static2>()] < 3f && !player.HasMinionAttackTargetNPC && Sleep <= 0 && ToEat <= 0 && Eating <= 0 && player.HeldItem.type == ModContent.ItemType<HealBall>())
                    {
                        Projectile.ai[0] = 1;
                    }
                    else if (target != null && Projectile.ai[0] == 0 && ChannelTime <= 20 && ChannelState <= 0 && Transform != 1 && Transform != 2 && Transform != 4 && Transform == 3 && player.HasMinionAttackTargetNPC && Sleep <= 0 && ToEat <= 0 && Eating <= 0 && player.HeldItem.type == ModContent.ItemType<HealBall>())
                    {
                        Projectile.ai[0] = 1;
                    }
                    ///transform emerald
                    else if (target != null && Projectile.ai[0] == 0 && ChannelTime <= 20 && ChannelState <= 0 && Transform != 1 && Transform != 2 && Transform == 4 && Transform != 3 && !player.HasMinionAttackTargetNPC && Sleep <= 0 && ToEat <= 0 && Eating <= 0 && player.ownedProjectileCounts[ModContent.ProjectileType<Specialrupee>()] <= 0f && player.ownedProjectileCounts[ModContent.ProjectileType<Rupee>()] <= 0f && player.HeldItem.type == ModContent.ItemType<HealBall>())
                    {
                        Projectile.ai[0] = 1;
                    }
                    else if (target != null && Projectile.ai[0] == 0 && ChannelTime <= 20 && ChannelState <= 0 && Transform != 1 && Transform != 2 && Transform == 4 && Transform != 3 && player.HasMinionAttackTargetNPC && Sleep <= 0 && ToEat <= 0 && Eating <= 0 && player.HeldItem.type == ModContent.ItemType<HealBall>())
                    {
                        Projectile.ai[0] = 1;
                    }

                    else if (target != null && Projectile.ai[0] == 0 && ChannelTime <= 20 && ChannelState <= 0 && Transform == 1 && Transform != 2 && Transform != 4 && Transform != 3 && Sleep <= 0 && ToEat <= 0 && Eating <= 0 && player.HeldItem.type == ModContent.ItemType<HealBall>())
                    {
                        Projectile.ai[0] = 1;
                    }

                    if (target != null && Projectile.ai[0] == 0 && ChannelTime <= 20 && ChannelState <= 0 && Transform == 1 && Transform != 2 && Transform != 3 && player.HasMinionAttackTargetNPC && Sleep <= 0 && ToEat <= 0 && Eating <= 0 && player.HeldItem.type == ModContent.ItemType<HealBall>())
                    {
                        Projectile.ai[0] = 1;
                    }

                    if (Transform == 6 && CantAttackTimer <= 0)
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
                                                    if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), npc.position.X + 0, npc.position.Y + -24, 0, 0, ModContent.ProjectileType<ShadowClaw>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                                                    Projectile.netUpdate = true;
                                                }
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


                        if (Main.projectile[i].active && i != base.Projectile.whoAmI && ((!Main.projectile[i].friendly && Main.projectile[i].hostile) || (Main.projectile[i].trap)) && Main.myPlayer == Projectile.owner)
                        {
                            if ((!player.HasBuff(ModContent.BuffType<Sickness>()) && (!player.HasBuff(ModContent.BuffType<BloodmoonBuff>()) && (!player.HasBuff(ModContent.BuffType<EclipseBuff>()) && (player.ownedProjectileCounts[ModContent.ProjectileType<Sad>()] <= 0f) && (player.ownedProjectileCounts[ModContent.ProjectileType<Flash>()] <= 0f) && (player.ownedProjectileCounts[ModContent.ProjectileType<FlashCooldown>()] <= 0f)))) && Main.myPlayer == Projectile.owner)
                            {
                                if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<Flash>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
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

                if (CantAttackTimer > 0)
                {
                    CantAttack = 1;
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
                    if (player.ownedProjectileCounts[ModContent.ProjectileType<period>()] <= 0f && Sleep <= 0 && Main.myPlayer == Projectile.owner)
                    {
                        Mood = -3600;
                        Projectile.netUpdate = true;
                        if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<Anger>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                        if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<period>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                    }
                }
                if (player.HasBuff(ModContent.BuffType<BloodmoonBuff>()) && (player.ownedProjectileCounts[ModContent.ProjectileType<period>()] <= 0f) && Sleep <= 0 && Main.myPlayer == Projectile.owner)

                {
                    Mood = -3600;
                    Projectile.netUpdate = true;
                    if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<Anger>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                    if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<period>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                }
                if (player.HasBuff(ModContent.BuffType<StatLower>()) && (player.ownedProjectileCounts[ModContent.ProjectileType<period>()] <= 0f) && Sleep <= 0 && Cursed <= 0 && Main.myPlayer == Projectile.owner)

                {

                    Mood -= 600;
                    Projectile.netUpdate = true;
                    if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<Sad2>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                    if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<period>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                }
                if (player.HasBuff(ModContent.BuffType<StatRaise>()) && (player.ownedProjectileCounts[ModContent.ProjectileType<period2>()] <= 0f) && Sleep <= 0 && Cursed <= 0 && Main.myPlayer == Projectile.owner)

                {
                    Mood += 600;
                    Projectile.netUpdate = true;
                    if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<Smile2>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                    if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<period2>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                }
                if (!player.HasBuff(ModContent.BuffType<StatRaise>()) && !player.HasBuff(ModContent.BuffType<StatLower>()) && (player.ownedProjectileCounts[ModContent.ProjectileType<period3>()] <= 0f) && Cursed <= 0 && Mood < 0 && Main.myPlayer == Projectile.owner)

                {
                    Mood += 600;
                    Projectile.netUpdate = true;
                    if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<period3>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                }
                if (!player.HasBuff(ModContent.BuffType<StatRaise>()) && !player.HasBuff(ModContent.BuffType<StatLower>()) && (player.ownedProjectileCounts[ModContent.ProjectileType<period3>()] <= 0f) && Cursed <= 0 && Mood > 0 && Main.myPlayer == Projectile.owner)

                {
                    Mood -= 600;
                    Projectile.netUpdate = true;
                    if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<period3>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
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
                ///biome effectiveness
                if (BiomeTime <= 0f)

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
                                    if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<PowerDown>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
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
                                    if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<PowerUp>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
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
                        if (player.ZoneSnow)
                        {
                            player.AddBuff(ModContent.BuffType<Frostburn2>(), 2);
                        }
                        if (player.ZoneRain || (player.wet && !player.honeyWet && !player.lavaWet))
                        {
                            player.AddBuff(ModContent.BuffType<PassiveHealing>(), 2);
                        }
                        if ((Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneDesert || Main.player[Main.myPlayer].ZoneJungle) || Main.player[Main.myPlayer].ZoneGlowshroom || Main.player[Main.myPlayer].ZoneSnow)
                        {
                            if (!player.HasBuff(ModContent.BuffType<StatLower>()))
                            {
                                SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/StatLower"), Projectile.Center);
                                for (int j = 0; j < 1; j++) //set to 2
                                {
                                    if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<PowerDown>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);

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
                                    if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<PowerUp>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
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
                        float between = Vector2.Distance(player2.Center, Projectile.Center);
                        // Reasonable distance away so it doesn't target across multiple screens
                        if (between < 500f)
                        {
                            player2.resistCold = true;
                            player2.AddBuff(BuffID.Warmth, 20);

                        }
                        if (Projectile.spriteDirection > 0)
                        {
                            sneezespot = 18;
                        }
                        if (Projectile.spriteDirection < 0)
                        {
                            sneezespot = 3;
                        }
                        if ((player.wet && player.honeyWet != true && player.lavaWet != true) || (Collision.WetCollision(Projectile.position, Projectile.width/2, Projectile.height/3)))
                        {
                            if (Main.rand.NextBool(20))//controls the speed of when the sparkles spawn
                            {
                                float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius2 * sphereRadius2));
                                double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                                Dust.NewDust(new Vector2((Projectile.Center.X + sneezespot) + radius * (float)Math.Cos(angle), (Projectile.Center.Y - 10) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<SmokeDust3>(), 0f, 0f, 0, default(Color), 1.5f);
                            }
                            if (SoundTimer2 <= 0)
                            {
                                SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/mist"), player.Center);
                                SoundTimer2 += 200;
                            }
                            player.AddBuff(ModContent.BuffType<Extinguished>(), 20);
                        }
                        Vector2 UpWardPosition = Projectile.Center;
                        UpWardPosition.Y -= 550f;
                        Vector2 UpWardPositionCover = Projectile.Center;
                        float minionPositionOffCover = ((20 + Projectile.minionPos / 80) * player.direction) - 15;
                        UpWardPositionCover.Y -= 50f;
                        UpWardPositionCover.X += minionPositionOffCover;
                        bool Cover = Collision.CanHitLine(UpWardPositionCover, Projectile.width/2, Projectile.height + 0, UpWardPosition, 0, 1);
                        if ((Main.player[Main.myPlayer].ZoneRain && Cover))
                        {
                           
                           
                            if (Main.rand.NextBool(20))//controls the speed of when the sparkles spawn
                            {
                                float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius2 * sphereRadius2));
                                double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                                Dust.NewDust(new Vector2(Projectile.Center.X + sneezespot * (float)Math.Cos(angle), (Projectile.Center.Y - 10) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<SmokeDust3>(), 0f, 0f, 0, default(Color), 1.5f);
                            }
                            if (SoundTimer2 <= 0)
                            {
                                SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/mist"), player.Center);
                                SoundTimer2 += 200;
                            }
                            player.AddBuff(ModContent.BuffType<Extinguished>(), 20);
                        }
                        if (Main.player[Main.myPlayer].ZoneUnderworldHeight && player.HasBuff(ModContent.BuffType<Veil>()))
                        {
                            player.AddBuff(ModContent.BuffType<Burning2>(), 20);
                        }
                        if ((Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneBeach || (Main.player[Main.myPlayer].ZoneRain && !Main.player[Main.myPlayer].ZoneSnow) || Main.player[Main.myPlayer].ZoneSandstorm))
                        {
                            if (!player.HasBuff(ModContent.BuffType<StatRaise>()) && !player.HasBuff(ModContent.BuffType<StatLower>()))
                            {
                                SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/StatLower"), Projectile.Center);
                                for (int j = 0; j < 1; j++) //set to 2
                                {
                                    if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<PowerDown>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                                }
                                player.AddBuff(ModContent.BuffType<StatLower>(), 20);
                            }
                            if (player.HasBuff(ModContent.BuffType<StatLower>()))
                            {
                                player.AddBuff(ModContent.BuffType<StatLower>(), 20);
                            }
                            Projectile.netUpdate = true;
                        }
                        if ((Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneSnow || Main.player[Main.myPlayer].ZoneGlowshroom || Main.player[Main.myPlayer].ZoneUnderworldHeight || Main.player[Main.myPlayer].ZoneJungle || Main.player[Main.myPlayer].ZoneDungeon || Main.player[Main.myPlayer].ZoneHallow && (!player.HasBuff(ModContent.BuffType<StatLower>()))))
                        {
                            if (!player.HasBuff(ModContent.BuffType<StatRaise>()) && !player.HasBuff(ModContent.BuffType<StatLower>()))
                            {
                                SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/StatRaise"), Projectile.Center);
                                for (int j = 0; j < 1; j++) //set to 2
                                {
                                    if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<PowerUp>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
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
                                    if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<PowerDown>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
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
                                    if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<PowerUp>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
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
                                    if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<PowerDown>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
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
                                    if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<PowerUp>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
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
                                    if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<PowerDown>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
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
                                    if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<PowerUp>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
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
                float speed = 2;
                float Close = 60;
                if (Eating <= 0 && Sleep <= 0)
                {
                    if (player.HeldItem.type == ModContent.ItemType<FrozenYogurt>() || player.HeldItem.type == ModContent.ItemType<SariasConfect>())
                    {
                        Close = 20;

                        if ((player.ownedProjectileCounts[ModContent.ProjectileType<Notice>()] <= 0f) && Holding <= 0)
                        {
                            if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<Notice>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
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
                        if (player.statLife >= (player.statLifeMax2 - player.statLifeMax2 / 12))
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
                CanMove = ChannelTime;
                float nothing = 1;
                Vector2 vectorToIdlePosition = idlePosition - Projectile.Center;

                float distanceToIdlePosition = vectorToIdlePosition.Length();

                if (player.HasBuff(ModContent.BuffType<Veil>()) && Transform == 1)
                {
                    player.AddBuff(ModContent.BuffType<Veil>(), 8800);
                }

                Vector2 direction = idlePosition - Projectile.Center;
                if (player2.Hitbox.Intersects(Projectile.Hitbox) && (player2.team == player.team) && Projectile.frame > 42 && Projectile.frame < 47 && ChannelState > 20 && Eating <= 0 && Sleep <= 0)
                {

                }

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
                if (MoveTimer >= 275 && (base.Projectile.frame >= 0) && (base.Projectile.frame <= 75) && (distanceToIdlePosition <= 180) && (Math.Abs(Projectile.velocity.X) <= .5) && (player.statLife >= player.statLifeMax2) || Sleep >= 1 || Eating >= 1 || (ChannelState > 0))
                {
                    nothing = 0;
                    Projectile.netUpdate = true;
                }

                else
                {
                    nothing = 1;
                    Projectile.netUpdate = true;

                }
                if (Sleep >= 1 && (distanceToIdlePosition > 280))
                {
                    MoveTimer = 0;
                    Projectile.netUpdate = true;

                }
                if (ChannelState > 0)
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
                if (player.statLife < (player.statLifeMax2) / 4 && !player.HasBuff(ModContent.BuffType<HealpulseBuff>()) && !player.HasBuff(ModContent.BuffType<Sickness>()) && !player.HasBuff(ModContent.BuffType<BloodmoonBuff>()) && !player.HasBuff(ModContent.BuffType<EclipseBuff>()))
                {
                    Mood -= 600;
                    player.statLife += 500;
                    player.AddBuff(ModContent.BuffType<HealpulseBuff>(), 3000);
                    Projectile.netUpdate = true;

                    for (int j = 0; j < 1; j++) //set to 2
                    {
                        if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<Heal>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                        if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<Sad>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
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

                ///SleepAi
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
                if (player.sleeping.isSleeping)
                {
                    if (IsPlayerAsleep >= 1)
                    {
                        if (Projectile.frame < 54)

                            Projectile.frame = 54;
                        if (MoveTimer >= 20)
                        {
                            MoveTimer = 6000;
                        }
                        Projectile.netUpdate = true;
                    }
                    else if (IsPlayerAsleep <= 0 && Sleep <= 0)
                    {
                        if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<Notice>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                        IsPlayerAsleep = 1;
                        MoveTimer = 0;
                    }
                }
                if (!player.sleeping.isSleeping)
                {
                    IsPlayerAsleep = 0;
                }
                ///end of sleepai
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

                        if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Throw, ThrowToo * 10, ModContent.ProjectileType<EmptyCup>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);

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
                            ///Charging animation
                            if (Sleep <= 0 && Eating <= 0 && ChannelState > 0)
                            {

                                if (base.Projectile.frame >= 76 || base.Projectile.frame < 40)
                                {

                                    base.Projectile.frame = 40;
                                }
                                if (Projectile.frame >= 47 && Projectile.frame < 76)
                                {
                                    Projectile.frame = 43;
                                }
                                if (Projectile.frame > 41 && Projectile.frame < 52)
                                {
                                    if (Transform == 0)
                                    {



                                        if (Projectile.spriteDirection > 0)
                                        {
                                            sneezespot = 24;
                                        }
                                        if (Projectile.spriteDirection < 0)
                                        {
                                            sneezespot = -1;
                                        }

                                    }
                                    if (Transform == 1)
                                    {


                                        if (Main.rand.NextBool(20))
                                        {
                                            if (Projectile.spriteDirection > 0)
                                            {
                                                sneezespot = 24;
                                            }
                                            if (Projectile.spriteDirection < 0)
                                            {
                                                sneezespot = -1;
                                            }
                                            Dust.NewDust(new Vector2((Projectile.Center.X + sneezespot), (Projectile.Center.Y + -4)), 0, 0, ModContent.DustType<BubbleDust>(), 0f, 0f, 0, default(Color), 1.5f);
                                        }
                                    }
                                    if (Transform == 2)
                                    {



                                        if (Projectile.spriteDirection > 0)
                                        {
                                            sneezespot = 24;
                                        }
                                        if (Projectile.spriteDirection < 0)
                                        {
                                            sneezespot = -1;
                                        }

                                        Dust.NewDust(new Vector2((Projectile.Center.X + sneezespot), (Projectile.Center.Y + -4)), 0, 0, ModContent.DustType<FlameDust>(), 0f, 0f, 0, default(Color), 1.5f);

                                    }
                                    if (Transform == 4)
                                    {


                                        if (Main.rand.NextBool(20))
                                        {
                                            if (Projectile.spriteDirection > 0)
                                            {
                                                sneezespot = 84;
                                            }
                                            if (Projectile.spriteDirection < 0)
                                            {
                                                sneezespot = 74;
                                            }

                                            if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + sneezespot, Projectile.position.Y + 18, 2, 0, ModContent.ProjectileType<ShardDust1>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                                        }
                                        if (Main.rand.NextBool(40))
                                        {
                                            if (Projectile.spriteDirection > 0)
                                            {
                                                sneezespot = 84;
                                            }
                                            if (Projectile.spriteDirection < 0)
                                            {
                                                sneezespot = 74;
                                            }
                                            if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + sneezespot, Projectile.position.Y + 18, -3, 0, ModContent.ProjectileType<ShardDust2>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                                        }
                                        if (Main.rand.NextBool(70))
                                        {
                                            if (Projectile.spriteDirection > 0)
                                            {
                                                sneezespot = 84;
                                            }
                                            if (Projectile.spriteDirection < 0)
                                            {
                                                sneezespot = 74;
                                            }

                                            if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + sneezespot, Projectile.position.Y + 18, 1, 0, ModContent.ProjectileType<ShardDust3>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                                        }
                                    }
                                    if (Transform == 6)
                                    {



                                        if (Projectile.spriteDirection > 0)
                                        {
                                            sneezespot = 24;
                                        }
                                        if (Projectile.spriteDirection < 0)
                                        {
                                            sneezespot = -1;
                                        }

                                        Dust.NewDust(new Vector2((Projectile.Center.X + sneezespot), (Projectile.Center.Y + -4)), 0, 0, ModContent.DustType<ShadowFlameDust>(), 0f, 0f, 0, default(Color), 1.5f);

                                    }
                                }
                            }
                            ////end of charging animation
                            if (Projectile.frame == 66 && (player.ownedProjectileCounts[ModContent.ProjectileType<Notice>()] <= 0f) && Sleep > 0)
                            {
                                if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<Notice>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
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
                                        if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<Heal>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                                        if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<Sad>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                                    }
                                }
                                player.AddBuff(ModContent.BuffType<HealpulseBuff>(), 3000);
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
                        if (Projectile.frame < 84)
                        {
                            Projectile.frame = 84;
                        }
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

                        if (CantAttackTimer <= 0 && !player.HasBuff(ModContent.BuffType<HealpulseBuff>()))
                        {
                            if (Transform == 0)
                            {
                                //Main.NewText("Frame: " + base.projectile.frame);
                                if (ChannelAttack == 1 && Main.myPlayer == Projectile.owner && (base.Projectile.frame == 89))
                                {
                                    SoundEngine.PlaySound(SoundID.DD2_EtherianPortalSpawnEnemy, base.Projectile.Center);
                                    if (Main.myPlayer == Projectile.owner && (CantAttackTimer <= 0))
                                    {
                                        if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 40, Projectile.position.Y + 20, 0, 0, ModContent.ProjectileType<LocatorCheck>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                                        for (int i = 0; i < 1000; i++)
                                        {

                                            if (Main.projectile[i].active && Main.projectile[i].ModProjectile is Ztarget2 modProjectile && i != base.Projectile.whoAmI && ((Main.projectile[i].owner == owner)))
                                            {

                                                {
                                                    CantAttackTimer = (modProjectile.ChannelTimer / 2) + 300;
                                                }
                                            }

                                        }
                                    }
                                }
                                if (ChannelAttack == 0)
                                {

                                    if (base.Projectile.frame == 84)
                                    {

                                        target = base.Projectile.Center.MinionHoming(500f, player);// the distance she targets enemies
                                        SoundEngine.PlaySound(SoundID.Item77, base.Projectile.Center);
                                        for (int j = 0; j < 1; j++) //set to 2
                                        {
                                            if (Projectile.spriteDirection == -1 && Main.myPlayer == Projectile.owner)
                                            {
                                                if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 40, Projectile.position.Y + 20, 0, 0, ModContent.ProjectileType<Locator>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                                            }
                                            if (Projectile.spriteDirection == 1 && Main.myPlayer == Projectile.owner)
                                            {
                                                if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 70, Projectile.position.Y + 20, 0, 0, ModContent.ProjectileType<Locator>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                                            }
                                        }
                                    }
                                    if (base.Projectile.frame == 86)
                                    {
                                        target = base.Projectile.Center.MinionHoming(500f, player);// the distance she targets enemies
                                        SoundEngine.PlaySound(SoundID.Item77, base.Projectile.Center);
                                        for (int j = 0; j < 1; j++) //set to 2
                                        {
                                            if (Projectile.spriteDirection == -1 && Main.myPlayer == Projectile.owner)
                                            {
                                                if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 40, Projectile.position.Y + 20, 0, 0, ModContent.ProjectileType<Locator>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                                            }
                                            if (Projectile.spriteDirection == 1 && Main.myPlayer == Projectile.owner)
                                            {
                                                if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 70, Projectile.position.Y + 20, 0, 0, ModContent.ProjectileType<Locator>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                                            }
                                        }
                                    }

                                    if (base.Projectile.frame == 89)
                                    {
                                        target = base.Projectile.Center.MinionHoming(500f, player);// the distance she targets enemies
                                        SoundEngine.PlaySound(SoundID.Item77, base.Projectile.Center);
                                        for (int j = 0; j < 1; j++) //set to 2
                                        {
                                            if (Projectile.spriteDirection == -1 && Main.myPlayer == Projectile.owner)
                                            {
                                                if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 40, Projectile.position.Y + 20, 0, 0, ModContent.ProjectileType<Locator>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                                            }
                                            if (Projectile.spriteDirection == 1 && Main.myPlayer == Projectile.owner)
                                            {
                                                if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 70, Projectile.position.Y + 20, 0, 0, ModContent.ProjectileType<Locator>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                                            }
                                        }
                                    }
                                }
                            }
                            else if (Transform == 1)
                            {
                                if (ChannelAttack == 1 && (base.Projectile.frame == 89))
                                {

                                    if (CantAttackTimer <= 0)
                                    {
                                        SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/WaterForm"), Projectile.Center);
                                        if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 40, Projectile.position.Y + 20, 0, 0, ModContent.ProjectileType<WaterCheck>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                                        CantAttackTimer = 200;
                                    }
                                }
                                if (ChannelAttack == 0 && !player.HasBuff(ModContent.BuffType<StatLower>()) && base.Projectile.frame == 90 && (player.ownedProjectileCounts[ModContent.ProjectileType<WaterBarrier2>()] <= 0f) && Main.myPlayer == Projectile.owner)
                                {
                                    SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Water3"), Projectile.Center);
                                    if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 40, Projectile.position.Y + 20, 0, 0, ModContent.ProjectileType<WaterBarrier2>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);

                                }
                                if (ChannelAttack == 0 && player.HasBuff(ModContent.BuffType<StatLower>()) && base.Projectile.frame == 90 && (player.ownedProjectileCounts[ModContent.ProjectileType<WaterBarrierSmall>()] <= 0f) && Main.myPlayer == Projectile.owner)
                                {
                                    SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Water3"), Projectile.Center);
                                    if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 40, Projectile.position.Y + 20, 0, 0, ModContent.ProjectileType<WaterBarrierSmall>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);

                                }

                            }

                            else if (Transform == 2)
                            {
                                if (ChannelAttack == 1 && (base.Projectile.frame == 89))
                                {

                                    if (CantAttackTimer <= 0)
                                    {

                                        CantAttackTimer = 200;
                                    }
                                }
                                if (ChannelAttack == 0 && base.Projectile.frame == 88)
                                {
                                    SoundEngine.PlaySound(SoundID.Item77, base.Projectile.Center);
                                    if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 60, Projectile.position.Y + 40, 0, 0, ModContent.ProjectileType<RubyPsychicSeeker>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                                    
                                }
                            }
                            else if (Transform == 3)
                            {
                                if (base.Projectile.frame == 90 && (player.ownedProjectileCounts[ModContent.ProjectileType<LightningLocator>()] == 0f) && (player.ownedProjectileCounts[ModContent.ProjectileType<LightningLocator>()] == 0f) && Main.myPlayer == Projectile.owner)
                                {

                                    SoundEngine.PlaySound(SoundID.Item77, base.Projectile.Center);
                                    for (int j = 0; j < 1; j++) //set to 2
                                    {
                                        if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 10, Projectile.position.Y + 10, 0, 0, ModContent.ProjectileType<LightningLocator>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                                    }
                                }

                                if (base.Projectile.frame == 90 && (player.ownedProjectileCounts[ModContent.ProjectileType<LightningLocator>()] == 0f) && (player.ownedProjectileCounts[ModContent.ProjectileType<LightningLocator>()] >= 1f) && Main.myPlayer == Projectile.owner)
                                {
                                    SoundEngine.PlaySound(SoundID.Item77, base.Projectile.Center);
                                    int Lightning2 = ModContent.ProjectileType<LightningLocator>();
                                    for (int g = 0; g < 1000; g++)
                                    {


                                        if (Main.projectile[g].active && g != base.Projectile.whoAmI && ((Main.projectile[g].type == Lightning2 && Main.projectile[g].owner == owner)) && Main.myPlayer == Projectile.owner)
                                        {

                                            {
                                                if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Main.projectile[g].position.X + 16, Main.projectile[g].position.Y + 16, 0, 0, ModContent.ProjectileType<LightningLocator>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                                                Main.projectile[g].Kill();
                                            }

                                        }


                                    }
                                }
                                else if (base.Projectile.frame == 90 && (player.ownedProjectileCounts[ModContent.ProjectileType<LightningLocator>()] > 0f) && (player.ownedProjectileCounts[ModContent.ProjectileType<Static2>()] < 3f) && !player.HasMinionAttackTargetNPC && Main.myPlayer == Projectile.owner)
                                {

                                    SoundEngine.PlaySound(SoundID.Item77, base.Projectile.Center);
                                    for (int j = 0; j < 1; j++) //set to 2
                                    {
                                        if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 60, Projectile.position.Y + 40, 0, 0, ModContent.ProjectileType<Static2>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                                    }
                                }
                                else if (base.Projectile.frame == 90 && (player.ownedProjectileCounts[ModContent.ProjectileType<LightningLocator>()] > 0f) && player.HasMinionAttackTargetNPC && Main.myPlayer == Projectile.owner)
                                {

                                    SoundEngine.PlaySound(SoundID.Item77, base.Projectile.Center);
                                    for (int j = 0; j < 1; j++) //set to 2
                                    {
                                        if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + 0, 0, 0, ModContent.ProjectileType<Static>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                                    }
                                }

                            }
                            else if (Transform == 4)
                            {
                                if ((player.ownedProjectileCounts[ModContent.ProjectileType<Silverrupee>()] > 0f) && Main.myPlayer == Projectile.owner && Main.myPlayer == Projectile.owner)
                                {
                                    target = base.Projectile.Center.MinionHoming(500f, player);// the distance she targets enemies
                                    SoundEngine.PlaySound(SoundID.Item77, base.Projectile.Center);
                                    for (int j = 0; j < 1; j++) //set to 2
                                    {
                                        if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + 0, 0, 0, ModContent.ProjectileType<RupeeAttack>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                                    }
                                }
                                if (base.Projectile.frame == 90 && player.HasMinionAttackTargetNPC)
                                {
                                    if ((player.ownedProjectileCounts[ModContent.ProjectileType<Rupee>()] > 0f) || (player.ownedProjectileCounts[ModContent.ProjectileType<Specialrupee>()] > 0f) || (player.ownedProjectileCounts[ModContent.ProjectileType<Silverrupee>()] > 0f) && Main.myPlayer == Projectile.owner)
                                    {
                                        target = base.Projectile.Center.MinionHoming(500f, player);// the distance she targets enemies
                                        SoundEngine.PlaySound(SoundID.Item77, base.Projectile.Center);
                                        for (int j = 0; j < 1; j++) //set to 2
                                        {
                                            if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + 0, 0, 0, ModContent.ProjectileType<RupeeAttack>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                                        }
                                    }
                                    else
                                    {
                                        target = base.Projectile.Center.MinionHoming(500f, player);// the distance she targets enemies
                                        SoundEngine.PlaySound(SoundID.Item77, base.Projectile.Center);
                                        for (int j = 0; j < 1; j++) //set to 2
                                        {
                                            SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/SilverRupee1"), Projectile.Center);
                                            if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + 0, 0, 0, ModContent.ProjectileType<Rupee>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                                        }
                                    }
                                }
                                if (base.Projectile.frame == 90 && !player.HasMinionAttackTargetNPC && (player.ownedProjectileCounts[ModContent.ProjectileType<Rupee>()] <= 0f) && (player.ownedProjectileCounts[ModContent.ProjectileType<Specialrupee>()] <= 0f) && (player.ownedProjectileCounts[ModContent.ProjectileType<Silverrupee>()] <= 0f) && Main.myPlayer == Projectile.owner)
                                {

                                    {

                                        target = base.Projectile.Center.MinionHoming(500f, player);// the distance she targets enemies
                                        SoundEngine.PlaySound(SoundID.Item77, base.Projectile.Center);
                                        for (int j = 0; j < 1; j++) //set to 2
                                        {
                                            SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/SilverRupee1"), Projectile.Center);
                                            if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + 0, 0, 0, ModContent.ProjectileType<Rupee>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                                        }
                                    }
                                }
                            }
                            else if (Transform == 5)
                            {
                                if (base.Projectile.frame == 90 && SwarmTimer >= 1000)
                                {

                                    if (player.ownedProjectileCounts[ModContent.ProjectileType<DuskBallProjectile>()] <= 0f && Main.myPlayer == Projectile.owner)
                                    {
                                        if (((Main.rand.NextBool(60)) && (player.ownedProjectileCounts[ModContent.ProjectileType<GreenMoth>()] <= 0f) && (player.ownedProjectileCounts[ModContent.ProjectileType<GreenMothGoliath2>()] <= 0f) && (player.ownedProjectileCounts[ModContent.ProjectileType<AmberGreen>()] <= 0f) && (player.ownedProjectileCounts[ModContent.ProjectileType<GreenMothGiant>()] <= 0f) && (player.ownedProjectileCounts[ModContent.ProjectileType<GreenMothGoliath>()] <= 0f) && ((player.ownedProjectileCounts[ModContent.ProjectileType<RedMoth>()] == 1f) || (player.ownedProjectileCounts[ModContent.ProjectileType<RedMothGiant>()] == 1f)) && ((player.ownedProjectileCounts[ModContent.ProjectileType<PurpleMoth>()] == 1f) || (player.ownedProjectileCounts[ModContent.ProjectileType<PurpleMothGiant>()] == 1f))))
                                        {
                                            {
                                                modPlayer.SariaXp++;
                                                SoundEngine.PlaySound(SoundID.Item77, base.Projectile.Center);
                                                target = base.Projectile.Center.MinionHoming(500f, player);// the distance she targets enemies
                                                SoundEngine.PlaySound(SoundID.Item77, base.Projectile.Center);
                                                if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ProjectileType<AmberGreen>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);

                                            }
                                        }
                                        else
                                        {
                                            if (Main.myPlayer == Projectile.owner)
                                            {
                                                if ((BugTimer >= 50 && BugTimer <= 350) && ((player.ownedProjectileCounts[ModContent.ProjectileType<RedMoth>()] <= 0f) && (player.ownedProjectileCounts[ModContent.ProjectileType<RedMothGiant>()] <= 0f)))
                                                {
                                                    modPlayer.SariaXp++;
                                                    if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + new Vector2(-250f, 370f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ProjectileType<AmberRed>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);

                                                }
                                                if ((BugTimer >= 50 && BugTimer <= 250) && ((player.ownedProjectileCounts[ModContent.ProjectileType<PurpleMoth>()] <= 0f) && (player.ownedProjectileCounts[ModContent.ProjectileType<PurpleMothGiant>()] <= 0f)))
                                                {
                                                    modPlayer.SariaXp++;
                                                    if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + new Vector2(250f, 370f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ProjectileType<AmberPurple>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);

                                                }
                                                target = base.Projectile.Center.MinionHoming(500f, player);// the distance she targets enemies
                                                SoundEngine.PlaySound(SoundID.Item77, base.Projectile.Center);
                                                for (int j = 0; j < 1; j++) //set to 2
                                                {
                                                    modPlayer.SariaXp++;
                                                    if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + new Vector2(-500f, 370f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ProjectileType<AmberBlack1>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                                                    if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + new Vector2(500f, 370f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ProjectileType<AmberBlack2>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                                                }
                                            }
                                            SwarmTimer = 0;
                                            Projectile.netUpdate = true;
                                        }
                                    }
                                }
                            }
                            else if (Transform == 6)
                            {
                                if (base.Projectile.frame == 90 && Main.myPlayer == Projectile.owner)
                                {
                                    target = base.Projectile.Center.MinionHoming(500f, player);// the distance she targets enemies
                                    SoundEngine.PlaySound(SoundID.Item77, base.Projectile.Center);

                                    target = base.Projectile.Center.MinionHoming(500f, player);// the distance she targets enemies
                                    SoundEngine.PlaySound(SoundID.Item77, base.Projectile.Center);
                                    for (int j = 0; j < 1; j++) //set to 2
                                    {
                                        if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 60, Projectile.position.Y + 40, 0, 0, ModContent.ProjectileType<Shadowmelt>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                                    }
                                }
                            }
                        }
                        if (Projectile.frame == 88 && player.HasBuff(ModContent.BuffType<HealpulseBuff>()))
                        {
                            SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Error"), Projectile.Center);
                        }

                        //////////////////// End of Transform attacks
                        ///










                        if (base.Projectile.frame > 96) //stop when done
                        {
                            base.Projectile.frame = 10;
                            ChannelAttack = 0;
                            Projectile.ai[0] = 3;
                        }
                    }

                    if (Projectile.ai[0] == 1 && Transform != 1) //this is set when a target is found
                    {

                        base.Projectile.frame = 83; //animation setup
                        Projectile.netUpdate = true;
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
                float sneezespot = 5;
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
                        if (base.Projectile.direction == -1)
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

                        if (base.Projectile.spriteDirection == 1 && Eating <= 0 && ChannelState <= 0)
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
                        if (base.Projectile.spriteDirection == -1 && Eating <= 0 && ChannelState <= 0)
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
                        if (base.Projectile.spriteDirection == 1 && Eating <= 0 && ChannelState <= 0)
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
                        if (base.Projectile.spriteDirection == -1 && Eating <= 0 && ChannelState <= 0)
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
                        if (base.Projectile.spriteDirection == 1 && Eating <= 0 && ChannelState <= 0)
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
                        if (base.Projectile.spriteDirection == -1 && Eating <= 0 && ChannelState <= 0)
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
                            {
                                Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/Saria4EatMask");
                                Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                                int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
                                int frameY = frameHeight * base.Projectile.frame;
                                Color drawColor = Color.Lerp(lightColor, Color.White, 20f);
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
                            if (Main.rand.NextBool(40))
                            {
                                Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/Saria4EatGlowMask");
                                Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                                int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
                                int frameY = frameHeight * base.Projectile.frame;
                                Color drawColor = Color.Lerp(lightColor, Color.White, 20f);
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
                        if (base.Projectile.spriteDirection == 1 && Eating <= 0 && ChannelState <= 0)
                        {
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
                            {
                                Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/Saria4Mask");
                                Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                                int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
                                int frameY = frameHeight * base.Projectile.frame;
                                Color drawColor = Color.Lerp(lightColor, Color.White, 20f);
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

                            {
                                Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/Saria4GlowMask1");
                                Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                                int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
                                int frameY = frameHeight * base.Projectile.frame;
                                Color drawColor = Color.Lerp(lightColor, Color.White, 20f);
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
                            if (Main.rand.NextBool(40))
                            {
                                Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/Saria4GlowMask2");
                                Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                                int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
                                int frameY = frameHeight * base.Projectile.frame;
                                Color drawColor = Color.Lerp(lightColor, Color.White, 20f);
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
                        if (base.Projectile.spriteDirection == -1 && Eating <= 0 && ChannelState <= 0)
                        {
                            
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
                           
                            {
                                Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/Saria4RMask");
                                Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                                int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
                                int frameY = frameHeight * base.Projectile.frame;
                                Color drawColor = Color.Lerp(lightColor, Color.White, 1f);
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
                            
                            {
                                Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/Saria4RGlowMask1");
                                Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                                int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
                                int frameY = frameHeight * base.Projectile.frame;
                                Color drawColor = Color.Lerp(lightColor, Color.White, 1f);
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
                            if (Main.rand.NextBool(40))
                            {
                                Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/Saria4RGlowMask2");
                                Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                                int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
                                int frameY = frameHeight * base.Projectile.frame;
                                Color drawColor = Color.Lerp(lightColor, Color.White, 20f);
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
                        if (base.Projectile.spriteDirection == 1 && Eating <= 0 && ChannelState <= 0)
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
                        if (base.Projectile.spriteDirection == -1 && Eating <= 0 && ChannelState <= 0)
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
                        if (base.Projectile.spriteDirection == 1 && Eating <= 0 && ChannelState <= 0)
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
                        if (base.Projectile.spriteDirection == -1 && Eating <= 0 && ChannelState <= 0)
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
                        if (base.Projectile.spriteDirection == 1 && Eating <= 0 && ChannelState <= 0)
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
                        if (base.Projectile.spriteDirection == -1 && Eating <= 0 && ChannelState <= 0)
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
                    if (Sleep <= 0 && Eating <= 0 && ChannelState <= 0)
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
                            if (Transform != 6 && Transform != 7 && ((Mood <= -1200 && Mood > -2400) || Mood <= -3600) || player.HasBuff(ModContent.BuffType<Extinguished>()))
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
                            if (base.Projectile.direction == -1)
                            {
                                Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/SariaArmAttackRMask");
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
                    ///Channel Attacks
                    if (Sleep <= 0 && Eating <= 0 && ChannelState > 0)
                    {
                        if (Transform == 0)
                        {
                            if (Projectile.spriteDirection == 1)
                            {
                                {
                                    Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/SariaCharging1");
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

                                if (Projectile.frame > 42 && Projectile.frame < 47 && ChannelState > 20)
                                {
                                    Texture2D texture = TextureAssets.Projectile[ModContent.ProjectileType<PinkCharge>()].Value;
                                    Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                                    int frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<PinkCharge>()];
                                    int frameY = frameHeight * Main.projFrames[ModContent.ProjectileType<PinkCharge>()];
                                    Color drawColor = Color.Lerp(lightColor, Color.Pink, 20f);
                                    Lighting.AddLight(Projectile.Center, Color.HotPink.ToVector3() * 0.78f);
                                    drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                                    Rectangle rectangle = texture.Frame(verticalFrames: 4, frameY: (int)Main.GameUpdateCount / 6 % 4);
                                    Vector2 origin = rectangle.Size() / 2f;
                                    float rotation = base.Projectile.rotation;
                                    float scale = base.Projectile.scale;
                                    SpriteEffects spriteEffects = SpriteEffects.None;


                                    if (base.Projectile.spriteDirection == 1)
                                    {
                                        startPos.Y += 0;
                                        startPos.X += +32;
                                    }

                                    Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);

                                    if (Main.rand.NextBool(30))
                                    {
                                        for (int i = 0; i < 50; i++)
                                        {
                                            Vector2 speed = Main.rand.NextVector2CircularEdge(1f, 1f);
                                            Dust d = Dust.NewDustPerfect(Projectile.Right, ModContent.DustType<AbsorbPsychic>(), speed * -5, Scale: 1.5f);
                                            d.noGravity = true;
                                        }
                                        SoundEngine.PlaySound(SoundID.DD2_SkyDragonsFuryShot, base.Projectile.Center);
                                        Lighting.AddLight(Projectile.Center, Color.HotPink.ToVector3() * 4f);
                                    }
                                }
                                {
                                    Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/SariaArmChannel1");
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
                            if (Projectile.spriteDirection == -1)
                            {
                                {
                                    Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/SariaCharging1R");
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

                                if (Projectile.frame > 42 && Projectile.frame < 47 && ChannelState > 20)
                                {
                                    Texture2D texture = TextureAssets.Projectile[ModContent.ProjectileType<PinkCharge>()].Value;
                                    Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                                    int frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<PinkCharge>()];
                                    int frameY = frameHeight * Main.projFrames[ModContent.ProjectileType<PinkCharge>()];
                                    Color drawColor = Color.Lerp(lightColor, Color.Pink, 20f);
                                    Lighting.AddLight(Projectile.Center, Color.HotPink.ToVector3() * 0.78f);
                                    drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                                    Rectangle rectangle = texture.Frame(verticalFrames: 4, frameY: (int)Main.GameUpdateCount / 6 % 4);
                                    Vector2 origin = rectangle.Size() / 2f;
                                    float rotation = base.Projectile.rotation;
                                    float scale = base.Projectile.scale;
                                    SpriteEffects spriteEffects = SpriteEffects.None;

                                    if (base.Projectile.spriteDirection == -1)
                                    {
                                        startPos.Y += 1;
                                        startPos.X += 11;
                                        spriteEffects = SpriteEffects.FlipHorizontally;
                                    }

                                    Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                                    if (Main.rand.NextBool(30))
                                    {
                                        for (int i = 0; i < 50; i++)
                                        {
                                            Vector2 speed = Main.rand.NextVector2CircularEdge(1f, 1f);
                                            Dust d = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<AbsorbPsychic>(), speed * -5, Scale: 1.5f);
                                            d.noGravity = true;
                                        }
                                        SoundEngine.PlaySound(SoundID.DD2_SkyDragonsFuryShot, base.Projectile.Center);
                                        Lighting.AddLight(Projectile.Center, Color.HotPink.ToVector3() * 4f);
                                    }
                                }
                                {
                                    Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/SariaArmChannel1R");
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



                        }
                        if (Transform == 1)
                        {
                            if (Projectile.spriteDirection == 1)
                            {
                                {
                                    Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/SariaCharging2");
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

                                if (Projectile.frame > 42 && Projectile.frame < 47 && ChannelState > 20)
                                {
                                    Texture2D texture = TextureAssets.Projectile[ModContent.ProjectileType<BlueCharge>()].Value;
                                    Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                                    int frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<BlueCharge>()];
                                    int frameY = frameHeight * Main.projFrames[ModContent.ProjectileType<BlueCharge>()];
                                    Color drawColor = Color.Lerp(lightColor, Color.LightBlue, 0f);
                                    Lighting.AddLight(Projectile.Center, Color.LightBlue.ToVector3() * 1f);
                                    drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                                    Rectangle rectangle = texture.Frame(verticalFrames: 8, frameY: (int)Main.GameUpdateCount / 8 % 8);
                                    Vector2 origin = rectangle.Size() / 2f;
                                    float rotation = base.Projectile.rotation;
                                    float scale = base.Projectile.scale;
                                    SpriteEffects spriteEffects = SpriteEffects.None;


                                    if (base.Projectile.spriteDirection == 1)
                                    {
                                        startPos.Y += 0;
                                        startPos.X += +32;
                                    }
                                    Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                                    if (Main.rand.NextBool(30))
                                    {
                                        for (int i = 0; i < 50; i++)
                                        {
                                            Vector2 speed = Main.rand.NextVector2CircularEdge(1f, 1f);
                                            Dust d = Dust.NewDustPerfect(Projectile.Right, ModContent.DustType<BubbleDust2>(), speed * -6, Scale: 2.7f);
                                            d.noGravity = true;
                                        }
                                        SoundEngine.PlaySound(SoundID.Drown, base.Projectile.Center);
                                        Lighting.AddLight(Projectile.Center, Color.White.ToVector3() * 4f);
                                    }
                                }
                                {
                                    Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/SariaArmChannel2");
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
                            if (Projectile.spriteDirection == -1)
                            {
                                {
                                    Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/SariaCharging2R");
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

                                if (Projectile.frame > 42 && Projectile.frame < 47 && ChannelState > 20)
                                {
                                    Texture2D texture = TextureAssets.Projectile[ModContent.ProjectileType<BlueCharge>()].Value;
                                    Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                                    int frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<BlueCharge>()];
                                    int frameY = frameHeight * Main.projFrames[ModContent.ProjectileType<BlueCharge>()];
                                    Color drawColor = Color.Lerp(lightColor, Color.LightBlue, 0f);
                                    Lighting.AddLight(Projectile.Center, Color.LightBlue.ToVector3() * 1f);
                                    drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                                    Rectangle rectangle = texture.Frame(verticalFrames: 8, frameY: (int)Main.GameUpdateCount / 8 % 8);
                                    Vector2 origin = rectangle.Size() / 2f;
                                    float rotation = base.Projectile.rotation;
                                    float scale = base.Projectile.scale;
                                    SpriteEffects spriteEffects = SpriteEffects.None;

                                    if (base.Projectile.spriteDirection == -1)
                                    {
                                        startPos.Y += 1;
                                        startPos.X += 11;
                                        spriteEffects = SpriteEffects.FlipHorizontally;
                                    }

                                    Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                                    if (Main.rand.NextBool(30))
                                    {
                                        for (int i = 0; i < 50; i++)
                                        {
                                            Vector2 speed = Main.rand.NextVector2CircularEdge(1f, 1f);
                                            Dust d = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<BubbleDust2>(), speed * -6, Scale: 2.7f);
                                            d.noGravity = true;
                                        }
                                        SoundEngine.PlaySound(SoundID.Drown, base.Projectile.Center);
                                        Lighting.AddLight(Projectile.Center, Color.White.ToVector3() * 4f);
                                    }
                                }
                                {
                                    Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/SariaArmChannel2R");
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



                        }
                        if (Transform == 2)
                        {
                            
                            if (Projectile.spriteDirection == 1)
                            {
                                {
                                    Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/SariaCharging3");
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

                                if (Projectile.frame > 42 && Projectile.frame < 47 && ChannelState > 20)
                                {
                                    Texture2D texture = TextureAssets.Projectile[ModContent.ProjectileType<RedCharge>()].Value;
                                    Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                                    int frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<RedCharge>()];
                                    int frameY = frameHeight * Main.projFrames[ModContent.ProjectileType<RedCharge>()];
                                    Color drawColor = Color.Lerp(lightColor, Color.LightBlue, 0f);
                                    Lighting.AddLight(Projectile.Center, Color.OrangeRed.ToVector3() * 1f);
                                    drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                                    Rectangle rectangle = texture.Frame(verticalFrames: 4, frameY: (int)Main.GameUpdateCount / 6 % 4);
                                    Vector2 origin = rectangle.Size() / 2f;
                                    float rotation = base.Projectile.rotation;
                                    float scale = base.Projectile.scale;
                                    SpriteEffects spriteEffects = SpriteEffects.None;


                                    {
                                        startPos.Y += 0;
                                        startPos.X += +32;
                                    }
                                    Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                                    if (Main.rand.NextBool(30))
                                    {
                                        for (int i = 0; i < 25; i++)
                                        {
                                            Vector2 speed = Main.rand.NextVector2CircularEdge(1f, 1f);
                                            Dust d = Dust.NewDustPerfect(Projectile.Right, ModContent.DustType<ShadowFlameDustCharge>(), speed * 5, Scale: 4.5f);
                                            d.noGravity = true;
                                        }
                                        for (int i = 0; i < 25; i++)
                                        {
                                            Vector2 speed = Main.rand.NextVector2CircularEdge(1f, 1f);
                                            Dust d = Dust.NewDustPerfect(Projectile.Right, ModContent.DustType<SmokeDust6>(), speed * 6, Scale: 2.5f);
                                            d.noGravity = true;
                                        }
                                        SoundEngine.PlaySound(SoundID.Item88, base.Projectile.Center);
                                        Lighting.AddLight(Projectile.Center, Color.Red.ToVector3() * 4f);
                                    }
                                }
                                {
                                    Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/SariaArmChannel3");
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
                            if (Projectile.spriteDirection == -1)
                            {
                                {
                                    Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/SariaCharging3R");
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

                                if (Projectile.frame > 42 && Projectile.frame < 47 && ChannelState > 20)
                                {
                                    Texture2D texture = TextureAssets.Projectile[ModContent.ProjectileType<RedCharge>()].Value;
                                    Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                                    int frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<RedCharge>()];
                                    int frameY = frameHeight * Main.projFrames[ModContent.ProjectileType<RedCharge>()];
                                    Color drawColor = Color.Lerp(lightColor, Color.LightBlue, 0f);
                                    Lighting.AddLight(Projectile.Center, Color.OrangeRed.ToVector3() * 1f);
                                    drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                                    Rectangle rectangle = texture.Frame(verticalFrames: 4, frameY: (int)Main.GameUpdateCount / 6 % 4);
                                    Vector2 origin = rectangle.Size() / 2f;
                                    float rotation = base.Projectile.rotation;
                                    float scale = base.Projectile.scale;
                                    SpriteEffects spriteEffects = SpriteEffects.None;



                                    {
                                        startPos.Y += 1;
                                        startPos.X += +11;
                                    }
                                    Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                                    if (Main.rand.NextBool(30))
                                    {
                                        for (int i = 0; i < 25; i++)
                                        {
                                                Vector2 speed = Main.rand.NextVector2CircularEdge(1f, 1f);
                                                Dust d = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<ShadowFlameDustCharge>(), speed * 5, Scale: 4.5f);
                                                d.noGravity = true;
                                        }
                                        for (int i = 0; i < 25; i++)
                                        {
                                            Vector2 speed = Main.rand.NextVector2CircularEdge(1f, 1f);
                                            Dust d = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<SmokeDust6>(), speed * 6, Scale: 2.5f);
                                            d.noGravity = true;
                                        }
                                        SoundEngine.PlaySound(SoundID.Item88, base.Projectile.Center);
                                        Lighting.AddLight(Projectile.Center, Color.Red.ToVector3() * 4f);
                                    }
                                }
                                {
                                    Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/SariaArmChannel3R");
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



                        }
                        if (Transform == 3)
                        {
                            if (Projectile.spriteDirection == 1)
                            {
                                {
                                    Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/SariaCharging4");
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
                                {
                                    Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/SariaCharging4Mask");
                                    Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                                    int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
                                    int frameY = frameHeight * base.Projectile.frame;
                                    Color drawColor = Color.Lerp(lightColor, Color.White, 20f);
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
                                if (Main.rand.NextBool(40))
                                {
                                    Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/SariaCharging4GlowMask");
                                    Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                                    int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
                                    int frameY = frameHeight * base.Projectile.frame;
                                    Color drawColor = Color.Lerp(lightColor, Color.White, 20f);
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
                                if (Projectile.frame > 42 && Projectile.frame < 47 && ChannelState > 20)
                                {
                                    Texture2D texture = TextureAssets.Projectile[ModContent.ProjectileType<YellowCharge>()].Value;
                                    Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                                    int frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<YellowCharge>()];
                                    int frameY = frameHeight * Main.projFrames[ModContent.ProjectileType<YellowCharge>()];
                                    Color drawColor = Color.Lerp(lightColor, Color.LightBlue, 0f);
                                    Lighting.AddLight(Projectile.Center, Color.Yellow.ToVector3() * 1f);
                                    drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                                    Rectangle rectangle = texture.Frame(verticalFrames: 4, frameY: (int)Main.GameUpdateCount / 4 % 4);
                                    Vector2 origin = rectangle.Size() / 2f;
                                    float rotation = base.Projectile.rotation;
                                    float scale = base.Projectile.scale;
                                    SpriteEffects spriteEffects = SpriteEffects.None;


                                    if (base.Projectile.spriteDirection == 1)
                                    {
                                        startPos.Y += 0;
                                        startPos.X += +32;
                                    }
                                    Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                                    if (Main.rand.NextBool(30))
                                    {
                                        SoundEngine.PlaySound(SoundID.NPCHit34, base.Projectile.Center);
                                        Lighting.AddLight(Projectile.Center, Color.LightYellow.ToVector3() * 4f);
                                    }
                                }
                                {
                                    Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/SariaArmChannel4");
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
                            if (Projectile.spriteDirection == -1)
                            {
                                {
                                    Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/SariaCharging4R");
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
                                {
                                    Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/SariaCharging4RMask");
                                    Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                                    int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
                                    int frameY = frameHeight * base.Projectile.frame;
                                    Color drawColor = Color.Lerp(lightColor, Color.White, 20f);
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
                                if (Main.rand.NextBool(40))
                                {
                                    Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/SariaCharging4RGlowMask");
                                    Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                                    int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
                                    int frameY = frameHeight * base.Projectile.frame;
                                    Color drawColor = Color.Lerp(lightColor, Color.White, 20f);
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
                                if (Projectile.frame > 42 && Projectile.frame < 47 && ChannelState > 20)
                                {
                                    Texture2D texture = TextureAssets.Projectile[ModContent.ProjectileType<YellowCharge>()].Value;
                                    Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                                    int frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<YellowCharge>()];
                                    int frameY = frameHeight * Main.projFrames[ModContent.ProjectileType<YellowCharge>()];
                                    Color drawColor = Color.Lerp(lightColor, Color.LightBlue, 0f);
                                    Lighting.AddLight(Projectile.Center, Color.Yellow.ToVector3() * 1f);
                                    drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                                    Rectangle rectangle = texture.Frame(verticalFrames: 4, frameY: (int)Main.GameUpdateCount / 4 % 4);
                                    Vector2 origin = rectangle.Size() / 2f;
                                    float rotation = base.Projectile.rotation;
                                    float scale = base.Projectile.scale;
                                    SpriteEffects spriteEffects = SpriteEffects.None;

                                    if (base.Projectile.spriteDirection == -1)
                                    {
                                        startPos.Y += 1;
                                        startPos.X += 11;
                                        spriteEffects = SpriteEffects.FlipHorizontally;
                                    }

                                    Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                                    if (Main.rand.NextBool(30))
                                    {
                                        SoundEngine.PlaySound(SoundID.NPCHit34, base.Projectile.Center);
                                        Lighting.AddLight(Projectile.Center, Color.LightYellow.ToVector3() * 4f);
                                    }
                                }
                                {
                                    Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/SariaArmChannel4R");
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



                        }
                        if (Transform == 4)
                        {
                            if (Projectile.spriteDirection == 1)
                            {
                                {
                                    Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/SariaCharging5");
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

                                if (Projectile.frame > 42 && Projectile.frame < 47 && ChannelState > 20)
                                {
                                    Texture2D texture = TextureAssets.Projectile[ModContent.ProjectileType<GreenCharge>()].Value;
                                    Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                                    int frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<GreenCharge>()];
                                    int frameY = frameHeight * Main.projFrames[ModContent.ProjectileType<GreenCharge>()];
                                    Color drawColor = Color.Lerp(lightColor, Color.LightBlue, 0f);
                                    Lighting.AddLight(Projectile.Center, Color.LightGreen.ToVector3() * 1f);
                                    drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                                    Rectangle rectangle = texture.Frame(verticalFrames: 4, frameY: (int)Main.GameUpdateCount / 4 % 4);
                                    Vector2 origin = rectangle.Size() / 2f;
                                    float rotation = base.Projectile.rotation;
                                    float scale = base.Projectile.scale;
                                    SpriteEffects spriteEffects = SpriteEffects.None;


                                    if (base.Projectile.spriteDirection == 1)
                                    {
                                        startPos.Y += 0;
                                        startPos.X += +32;
                                    }
                                    Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                                    if (Main.rand.NextBool(30))
                                    {
                                        SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact, base.Projectile.Center);
                                        Lighting.AddLight(Projectile.Center, Color.Green.ToVector3() * 6f);
                                    }
                                }
                                {
                                    Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/SariaArmChannel5");
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
                            if (Projectile.spriteDirection == -1)
                            {
                                {
                                    Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/SariaCharging5R");
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

                                if (Projectile.frame > 42 && Projectile.frame < 47 && ChannelState > 20)
                                {
                                    Texture2D texture = TextureAssets.Projectile[ModContent.ProjectileType<GreenCharge>()].Value;
                                    Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                                    int frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<GreenCharge>()];
                                    int frameY = frameHeight * Main.projFrames[ModContent.ProjectileType<GreenCharge>()];
                                    Color drawColor = Color.Lerp(lightColor, Color.LightBlue, 0f);
                                    Lighting.AddLight(Projectile.Center, Color.LightGreen.ToVector3() * 1f);
                                    drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                                    Rectangle rectangle = texture.Frame(verticalFrames: 4, frameY: (int)Main.GameUpdateCount / 4 % 4);
                                    Vector2 origin = rectangle.Size() / 2f;
                                    float rotation = base.Projectile.rotation;
                                    float scale = base.Projectile.scale;
                                    SpriteEffects spriteEffects = SpriteEffects.None;



                                    {
                                        startPos.Y += 1;
                                        startPos.X += +11;
                                    }
                                    Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                                    if (Main.rand.NextBool(30))
                                    {
                                        SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact, base.Projectile.Center);
                                        Lighting.AddLight(Projectile.Center, Color.Green.ToVector3() * 6f);
                                    }
                                }
                                {
                                    Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/SariaArmChannel5R");
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



                        }
                        if (Transform == 5)
                        {
                            if (Projectile.spriteDirection == 1)
                            {
                                {
                                    Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/SariaCharging6");
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

                                if (Projectile.frame > 42 && Projectile.frame < 47 && ChannelState > 20)
                                {
                                    Texture2D texture = TextureAssets.Projectile[ModContent.ProjectileType<OrangeCharge>()].Value;
                                    Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                                    int frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<OrangeCharge>()];
                                    int frameY = frameHeight * Main.projFrames[ModContent.ProjectileType<OrangeCharge>()];
                                    Color drawColor = Color.Lerp(lightColor, Color.LightBlue, 0f);
                                    Lighting.AddLight(Projectile.Center, Color.DarkOrange.ToVector3() * 1f);
                                    drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                                    Rectangle rectangle = texture.Frame(verticalFrames: 8, frameY: (int)Main.GameUpdateCount / 8 % 8);
                                    Vector2 origin = rectangle.Size() / 2f;
                                    float rotation = base.Projectile.rotation;
                                    float scale = base.Projectile.scale;
                                    SpriteEffects spriteEffects = SpriteEffects.None;


                                    if (base.Projectile.spriteDirection == 1)
                                    {
                                        startPos.Y += 0;
                                        startPos.X += +32;
                                    }
                                    Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                                    if (Main.rand.NextBool(30))
                                    {
                                        SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact, base.Projectile.Center);
                                        Lighting.AddLight(Projectile.Center, Color.Orange.ToVector3() * 6f);
                                    }
                                }
                                {
                                    Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/SariaArmChannel6");
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
                            if (Projectile.spriteDirection == -1)
                            {
                                {
                                    Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/SariaCharging6R");
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

                                if (Projectile.frame > 42 && Projectile.frame < 47 && ChannelState > 20)
                                {
                                    Texture2D texture = TextureAssets.Projectile[ModContent.ProjectileType<OrangeCharge>()].Value;
                                    Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                                    int frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<OrangeCharge>()];
                                    int frameY = frameHeight * Main.projFrames[ModContent.ProjectileType<OrangeCharge>()];
                                    Color drawColor = Color.Lerp(lightColor, Color.LightBlue, 0f);
                                    Lighting.AddLight(Projectile.Center, Color.DarkOrange.ToVector3() * 1f);
                                    drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                                    Rectangle rectangle = texture.Frame(verticalFrames: 8, frameY: (int)Main.GameUpdateCount / 8 % 8);
                                    Vector2 origin = rectangle.Size() / 2f;
                                    float rotation = base.Projectile.rotation;
                                    float scale = base.Projectile.scale;
                                    SpriteEffects spriteEffects = SpriteEffects.None;



                                    {
                                        startPos.Y += 1;
                                        startPos.X += +11;
                                    }
                                    Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                                    if (Main.rand.NextBool(30))
                                    {
                                        SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact, base.Projectile.Center);
                                        Lighting.AddLight(Projectile.Center, Color.Orange.ToVector3() * 6f);
                                    }
                                }
                                {
                                    Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/SariaArmChannel6R");
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



                        }
                        if (Transform == 6)
                        {
                            if (Projectile.spriteDirection == 1)
                            {
                                {
                                    Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/SariaCharging7");
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

                                if (Projectile.frame > 42 && Projectile.frame < 47 && ChannelState > 20)
                                {
                                    Texture2D texture = TextureAssets.Projectile[ModContent.ProjectileType<PurpleCharge>()].Value;
                                    Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                                    int frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<PurpleCharge>()];
                                    int frameY = frameHeight * Main.projFrames[ModContent.ProjectileType<PurpleCharge>()];
                                    Color drawColor = Color.Lerp(lightColor, Color.LightBlue, 0f);
                                    Lighting.AddLight(Projectile.Center, Color.MediumPurple.ToVector3() * 1f);
                                    drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                                    Rectangle rectangle = texture.Frame(verticalFrames: 8, frameY: (int)Main.GameUpdateCount / 8 % 8);
                                    Vector2 origin = rectangle.Size() / 2f;
                                    float rotation = base.Projectile.rotation;
                                    float scale = base.Projectile.scale;
                                    SpriteEffects spriteEffects = SpriteEffects.None;



                                    {
                                        startPos.Y += 0;
                                        startPos.X += +32;
                                    }
                                    Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                                    if (Main.rand.NextBool(30))
                                    {
                                        SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact, base.Projectile.Center);
                                        Lighting.AddLight(Projectile.Center, Color.GhostWhite.ToVector3() * 6f);
                                    }
                                }
                                {
                                    Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/SariaArmChannel7");
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
                            if (Projectile.spriteDirection == -1)
                            {
                                {
                                    Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/SariaCharging7R");
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
                                {
                                    Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/SariaCharging7EyesR");
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
                                if (Projectile.frame > 42 && Projectile.frame < 47 && ChannelState > 20)
                                {
                                    Texture2D texture = TextureAssets.Projectile[ModContent.ProjectileType<PurpleCharge>()].Value;
                                    Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                                    int frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<PurpleCharge>()];
                                    int frameY = frameHeight * Main.projFrames[ModContent.ProjectileType<PurpleCharge>()];
                                    Color drawColor = Color.Lerp(lightColor, Color.LightBlue, 0f);
                                    Lighting.AddLight(Projectile.Center, Color.MediumPurple.ToVector3() * 1f);
                                    drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                                    Rectangle rectangle = texture.Frame(verticalFrames: 8, frameY: (int)Main.GameUpdateCount / 8 % 8);
                                    Vector2 origin = rectangle.Size() / 2f;
                                    float rotation = base.Projectile.rotation;
                                    float scale = base.Projectile.scale;
                                    SpriteEffects spriteEffects = SpriteEffects.None;



                                    {
                                        startPos.Y += 1;
                                        startPos.X += +11;
                                    }
                                    Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                                    if (Main.rand.NextBool(30))
                                    {
                                        SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact, base.Projectile.Center);
                                        Lighting.AddLight(Projectile.Center, Color.GhostWhite.ToVector3() * 6f);
                                    }
                                }
                                {
                                    Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/SariaArmChannel7R");
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



                        }
                        if (Transform != 6)
                        {
                            if (Projectile.spriteDirection == -1)

                            {
                                Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/SariaChannelEyesR");
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
                            if (Projectile.spriteDirection == 1)

                            {
                                Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/SariaChannelEyes");
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
                        }
                        if (Transform == 6)
                        {



                            if (Projectile.spriteDirection == 1)

                            {
                                Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Strange/SariaCharging7Eyes");
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
                    ////Other Effects
                    if (Transform == 3 && SpecialAnimate > 0)
                    {
                        
                        {
                            Texture2D texture = TextureAssets.Projectile[ModContent.ProjectileType<SariaSparks>()].Value;
                            Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                            int frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<SariaSparks>()];
                            int frameY = frameHeight * Main.projFrames[ModContent.ProjectileType<SariaSparks>()];
                            Color drawColor = Color.Lerp(lightColor, Color.LightBlue, 2f);
                            drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                            Rectangle rectangle = texture.Frame(verticalFrames: 14, frameY: (int)Main.GameUpdateCount / 3 % 14);
                            Vector2 origin = rectangle.Size() / 2f;
                            float rotation = base.Projectile.rotation;
                            float scale = base.Projectile.scale;
                            SpriteEffects spriteEffects = SpriteEffects.None;
                            
                            startPos.Y += 1;
                            startPos.X += +17;
                           
                                Lighting.AddLight(Projectile.Center, Color.LightBlue.ToVector3() * .9f);
                            
                            if (base.Projectile.spriteDirection == -1)
                            {
                                spriteEffects = SpriteEffects.FlipHorizontally;
                            }
                            Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                            Projectile.netUpdate = true;
                            
                        }
                    }
                    /////XP Bars
                    if (XpTimer >= 1 && Main.myPlayer == Projectile.owner)
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