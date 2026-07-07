using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SariaMod.Buffs;
using SariaMod.Dusts;
using SariaMod.Items;
using SariaMod.Items.Strange;
using SariaMod.Netcode.SariaSoundSync;
using System;
using System.Linq;
using System.Reflection;
using System.Text;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Events;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using SariaMod.Items.zTalking;
using SariaMod.Items.Topaz;
using SariaMod.Items.Ruby;
using SariaMod.Items.Amethyst;
using SariaMod.Items.Sapphire;
using SariaMod.Items.Amber;
using SariaMod.Items.Emerald;
namespace SariaMod
{
    public static class SariaResources
    {
        public static void Ztargets(this Projectile projectile, int ChannelState, int Transform)
        {
            Player player = Main.player[projectile.owner];
            if (ChannelState > 40 && player.ownedProjectileCounts[ModContent.ProjectileType<Ztarget2>()] <= 0f && Transform == 0 && Main.myPlayer == projectile.owner)
            {
                if (Main.myPlayer == projectile.owner) Projectile.NewProjectile(projectile.GetSource_FromThis(), Main.MouseWorld.X, Main.MouseWorld.Y, 0, 0, ModContent.ProjectileType<Ztarget2>(), (int)(projectile.damage), 0f, projectile.owner, player.whoAmI, projectile.whoAmI);
            }
            if (ChannelState > 40 && player.ownedProjectileCounts[ModContent.ProjectileType<Ztarget3>()] <= 0f && Transform == 1 && Main.myPlayer == projectile.owner)
            {
                if (Main.myPlayer == projectile.owner) Projectile.NewProjectile(projectile.GetSource_FromThis(), Main.MouseWorld.X, Main.MouseWorld.Y, 0, 0, ModContent.ProjectileType<Ztarget3>(), (int)(projectile.damage), 0f, projectile.owner, player.whoAmI, projectile.whoAmI);
            }
            if (ChannelState > 40 && player.ownedProjectileCounts[ModContent.ProjectileType<Ztarget4>()] <= 0f && Transform == 2 && Main.myPlayer == projectile.owner)
            {
                if (Main.myPlayer == projectile.owner) Projectile.NewProjectile(projectile.GetSource_FromThis(), player.Center.X, player.Center.Y, 0, 0, ModContent.ProjectileType<Ztarget4>(), (int)(projectile.damage), 0f, projectile.owner, player.whoAmI, projectile.whoAmI);
            }
            if (ChannelState > 40 && player.ownedProjectileCounts[ModContent.ProjectileType<Ztarget5>()] <= 0f && Transform == 3 && Main.myPlayer == projectile.owner)
            {
                if (Main.myPlayer == projectile.owner) Projectile.NewProjectile(projectile.GetSource_FromThis(), Main.MouseWorld.X, Main.MouseWorld.Y, 0, 0, ModContent.ProjectileType<Ztarget5>(), (int)(projectile.damage), 0f, projectile.owner, player.whoAmI, projectile.whoAmI);
            }
            if (ChannelState > 40 && player.ownedProjectileCounts[ModContent.ProjectileType<Ztarget6>()] <= 0f && Transform == 4 && Main.myPlayer == projectile.owner)
            {
                if (Main.myPlayer == projectile.owner) Projectile.NewProjectile(projectile.GetSource_FromThis(), Main.MouseWorld.X, Main.MouseWorld.Y, 0, 0, ModContent.ProjectileType<Ztarget6>(), (int)(projectile.damage), 0f, projectile.owner, player.whoAmI, projectile.whoAmI);
            }
        }
        public static void SariaAttacks(this Projectile projectile, int Transform, int CantAttackTimer, int ChannelAttack, bool foundTarget, Vector2 targetCenter)
        {
            Player player = Main.player[projectile.owner];
            Player player2 = Main.LocalPlayer;
            FairyPlayer modPlayer = player.Fairy();
            int owner = player.whoAmI;
            //Main.NewText(projectile.ai[0] + " is state, " + projectile.ai[1] + " is timer. Test");
            for (int U = 0; U < 1000; U++)
            {
                if (Main.projectile[U].active && Main.projectile[U].ModProjectile is Saria modProjectile && U == projectile.whoAmI && ((Main.projectile[U].owner == owner)))
                {
                    if (projectile.ai[0] == 4)
                    {
                        projectile.ai[1] -= 1; //reduce timer
                        if (projectile.ai[1] == 0)
                        {
                            projectile.ai[0] = 0; //once at 0, back to normal behavior
                        }
                    }
                    if (projectile.ai[0] == 3 && Transform != 3) //recovery setup
                    {
                        projectile.ai[1] = 4; //4 cycles recovery between shots, adjust this for how long she waits between swipes
                        projectile.ai[0] = 4;
                    }
                    if (projectile.ai[0] == 3 && Transform == 3)
                    {
                        projectile.ai[1] = 10; //4 cycles recovery between shots, adjust this for how long she waits between swipes
                        projectile.ai[0] = 4;
                    }
                    if (projectile.ai[0] == 2)
                    {
                        projectile.frame++; //increment attack frame
                        projectile.frameCounter += 20;
                        if (projectile.frame < 44)
                        {
                            projectile.frame = 44;
                        }
                        if (projectile.frame == 44)
                        {
                            SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Hover"), projectile.Center);
                        }
                        if (projectile.frame == 46)
                        {
                            SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Step2"), projectile.Center);
                        }
                        if (projectile.frame == 45)
                        {
                            projectile.frameCounter = 8;
                        }
                        if (projectile.frame > 50 && projectile.frame < 54)
                        {
                            projectile.frameCounter = 18;
                        }
                        if (projectile.frame >= 54)
                        {
                            projectile.frameCounter = 15;
                        }
                        if (projectile.frame == 53)
                        {
                            SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Hover"), projectile.Center);
                        }
                        ///////////Transform Attacks
                        if (CantAttackTimer <= 0 && !player.HasBuff(ModContent.BuffType<HealpulseBuff>()))
                        {
                            if (Transform == 0)
                            {
                                //Main.NewText("Frame: " + projectile.frame);
                                if (ChannelAttack == 1 && (projectile.frame == 49))
                                {
                                    if (CantAttackTimer <= 0)
                                    {
                                        modProjectile.CantAttackTimer = 200;
                                    }
                                }
                                if (ChannelAttack == 0)
                                {
                                    if (projectile.frame == 47)
                                    {
                                        SoundEngine.PlaySound(SoundID.Item77, projectile.Center);

                                        bool hasZtargetReal = false;
                                        int ztargetRealType = ModContent.ProjectileType<ZtargetReal>();
                                        for (int i = 0; i < Main.maxProjectiles; i++)
                                        {
                                            if (Main.projectile[i].active && Main.projectile[i].owner == projectile.owner && Main.projectile[i].type == ztargetRealType)
                                            {
                                                hasZtargetReal = true;
                                                break;
                                            }
                                        }

                                        int locatorType = (hasZtargetReal || modProjectile.LinkCableFollowActive) ? ModContent.ProjectileType<Locator2>() : ModContent.ProjectileType<Locator>();

                                        for (int j = 0; j < 1; j++) //set to 2
                                        {
                                            if (projectile.spriteDirection == -1 && Main.myPlayer == projectile.owner)
                                            {
                                                if (Main.myPlayer == projectile.owner) Projectile.NewProjectile(projectile.GetSource_FromThis(), projectile.position.X + 40, projectile.position.Y + 20, 0, 0, locatorType, (int)(projectile.damage), 0f, projectile.owner, projectile.whoAmI, 0);
                                            }
                                            if (projectile.spriteDirection == 1 && Main.myPlayer == projectile.owner)
                                            {
                                                if (Main.myPlayer == projectile.owner) Projectile.NewProjectile(projectile.GetSource_FromThis(), projectile.position.X + 70, projectile.position.Y + 20, 0, 0, locatorType, (int)(projectile.damage), 0f, projectile.owner, projectile.whoAmI, 0);
                                            }
                                        }
                                    }
                                    if (projectile.frame == 49)
                                    {
                                        SoundEngine.PlaySound(SoundID.Item77, projectile.Center);

                                        bool hasZtargetReal = false;
                                        int ztargetRealType = ModContent.ProjectileType<ZtargetReal>();
                                        for (int i = 0; i < Main.maxProjectiles; i++)
                                        {
                                            if (Main.projectile[i].active && Main.projectile[i].owner == projectile.owner && Main.projectile[i].type == ztargetRealType)
                                            {
                                                hasZtargetReal = true;
                                                break;
                                            }
                                        }

                                        int locatorType = (hasZtargetReal || modProjectile.LinkCableFollowActive) ? ModContent.ProjectileType<Locator2>() : ModContent.ProjectileType<Locator>();

                                        for (int j = 0; j < 1; j++) //set to 2
                                        {
                                            if (projectile.spriteDirection == -1 && Main.myPlayer == projectile.owner)
                                            {
                                                if (Main.myPlayer == projectile.owner) Projectile.NewProjectile(projectile.GetSource_FromThis(), projectile.position.X + 40, projectile.position.Y + 20, 0, 0, locatorType, (int)(projectile.damage), 0f, projectile.owner, projectile.whoAmI, 0);
                                            }
                                            if (projectile.spriteDirection == 1 && Main.myPlayer == projectile.owner)
                                            {
                                               if (Main.myPlayer == projectile.owner) Projectile.NewProjectile(projectile.GetSource_FromThis(), projectile.position.X + 70, projectile.position.Y + 20, 0, 0, locatorType, (int)(projectile.damage), 0f, projectile.owner, projectile.whoAmI, 0);
                                            }
                                        }
                                    }
                                    if (projectile.frame == 52)
                                    {
                                        SoundEngine.PlaySound(SoundID.Item77, projectile.Center);

                                        bool hasZtargetReal = false;
                                        int ztargetRealType = ModContent.ProjectileType<ZtargetReal>();
                                        for (int i = 0; i < Main.maxProjectiles; i++)
                                        {
                                            if (Main.projectile[i].active && Main.projectile[i].owner == projectile.owner && Main.projectile[i].type == ztargetRealType)
                                            {
                                                hasZtargetReal = true;
                                                break;
                                            }
                                        }

                                        int locatorType = (hasZtargetReal || modProjectile.LinkCableFollowActive) ? ModContent.ProjectileType<Locator2>() : ModContent.ProjectileType<Locator>();

                                        for (int j = 0; j < 1; j++) //set to 2
                                        {
                                            if (projectile.spriteDirection == -1 && Main.myPlayer == projectile.owner)
                                            {
                                                if (Main.myPlayer == projectile.owner) Projectile.NewProjectile(projectile.GetSource_FromThis(), projectile.position.X + 40, projectile.position.Y + 20, 0, 0, locatorType, (int)(projectile.damage), 0f, projectile.owner, projectile.whoAmI, 0);
                                            }
                                            if (projectile.spriteDirection == 1 && Main.myPlayer == projectile.owner)
                                            {
                                                if (Main.myPlayer == projectile.owner) Projectile.NewProjectile(projectile.GetSource_FromThis(), projectile.position.X + 70, projectile.position.Y + 20, 0, 0, locatorType, (int)(projectile.damage), 0f, projectile.owner, projectile.whoAmI, 0);
                                            }
                                        }
                                    }
                                }
                            }
                            else if (Transform == 1)
                            {
                                if (ChannelAttack == 1 && (projectile.frame == 49))
                                {
                                    if (CantAttackTimer <= 0)
                                    {
                                        for (int i = 0; i < 1000; i++)
                                        {
                                            if (Main.projectile[i].active && Main.projectile[i].ModProjectile is Ztarget3 modprojectile && modprojectile.Stage >= 3 && i != projectile.whoAmI && ((Main.projectile[i].owner == owner)))
                                            {
                                                SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/WaterForm"), projectile.Center);
                                                if (Main.myPlayer == projectile.owner) Projectile.NewProjectile(projectile.GetSource_FromThis(), projectile.position.X + 40, projectile.position.Y + 20, 0, 0, ModContent.ProjectileType<WaterCheck>(), (int)(projectile.damage), 0f, projectile.owner, player.whoAmI, projectile.whoAmI);
                                                modProjectile.CantAttackTimer = 200;
                                            }
                                            else if (Main.projectile[i].active && Main.projectile[i].ModProjectile is Ztarget3 modprojectile2 && modprojectile2.Stage < 3 && i != projectile.whoAmI && ((Main.projectile[i].owner == owner)))
                                            {
                                                SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Error"), projectile.Center);
                                                modProjectile.CantAttackTimer = 200;
                                            }
                                        }
                                    }
                                }
                                if (ChannelAttack == 0 && projectile.frame == 45 && (player.ownedProjectileCounts[ModContent.ProjectileType<IceBarrier>()] <= 0f))
                                {
                                    if (Main.netMode == NetmodeID.SinglePlayer)
                                    {
                                        SoundEngine.PlaySound(SoundID.Item20, projectile.Center);
                                        SoundEngine.PlaySound(SoundID.Item28, projectile.Center);
                                    }
                                    else if (Main.myPlayer == projectile.owner)
                                    {
                                        SoundEngine.PlaySound(SoundID.Item20, projectile.Center);
                                        SoundEngine.PlaySound(SoundID.Item28, projectile.Center);
                                        if (Main.netMode == NetmodeID.MultiplayerClient || Main.netMode == NetmodeID.Server)
                                        {
                                            ModPacket packet = SariaMod.Instance.GetPacket();
                                            SariaSoundSyncMessage.Write(packet, projectile, SariaSoundId.IceBarrier1);
                                            packet.Send();
                                            packet = SariaMod.Instance.GetPacket();
                                            SariaSoundSyncMessage.Write(packet, projectile, SariaSoundId.IceBarrier2);
                                            packet.Send();
                                        }
                                    }
                                    if (Main.myPlayer == projectile.owner) Projectile.NewProjectile(projectile.GetSource_FromThis(), projectile.position.X + 40, projectile.position.Y + 20, 0, 0, ModContent.ProjectileType<IceBarrier>(), (int)(projectile.damage), 0f, projectile.owner, player.whoAmI, projectile.whoAmI);
                                }
                                if (ChannelAttack == 0 && projectile.frame == 49)
                                {
                                    if (foundTarget && (Main.myPlayer == projectile.owner)) Projectile.NewProjectile(projectile.GetSource_FromThis(), projectile.Center, projectile.DirectionTo(targetCenter) * 2f, ModContent.ProjectileType<ColdWaveCenter>(), projectile.damage, 0f, projectile.owner);
                                }
                            }
                            else if (Transform == 2)
                            {
                                if (ChannelAttack == 1 && (projectile.frame == 49))
                                {
                                    if (CantAttackTimer <= 0)
                                    {
                                        modProjectile.CantAttackTimer = 200;
                                    }
                                }
                                if (ChannelAttack == 0 && projectile.frame == 48)
                                {
                                    SoundEngine.PlaySound(SoundID.Item77, projectile.Center);
                                    if (Main.myPlayer == projectile.owner) Projectile.NewProjectile(projectile.GetSource_FromThis(), projectile.position.X + 60, projectile.position.Y + 40, 0, 0, ModContent.ProjectileType<RubyPsychicSeeker>(), (int)(projectile.damage), 0f, projectile.owner, player.whoAmI, projectile.whoAmI);
                                    modProjectile.CantAttackTimer = 400;
                                }
                            }
                            else if (Transform == 3)
                            {
                                float between3 = Vector2.Distance(projectile.Center, player2.Center);
                                if (ChannelAttack == 1 && (projectile.frame == 50))
                                {
                                    if (CantAttackTimer <= 0)
                                    {
                                        modProjectile.CantAttackTimer = 20;
                                    }
                                }
                                if (ChannelAttack == 0 && projectile.frame == 49)
                                {
                                    int head2 = -1;
                                    for (int i = 0; i < Main.maxProjectiles; i++)
                                    {
                                        if (Main.projectile[i].active && Main.projectile[i].owner == Main.myPlayer)
                                        {
                                            if (head2 == -1 && Main.projectile[i].type == ModContent.ProjectileType<LightningHeadSaria>())
                                            {
                                                head2 = i;
                                            }
                                            if (head2 != -1)
                                            {
                                                break;
                                            }
                                        }
                                    }
                                    if (head2 == -1)
                                    {
                                        int tailIndex2;
                                        {
                                            if (player.ownedProjectileCounts[ModContent.ProjectileType<LightningHeadSaria>()] <= 0f && player.ownedProjectileCounts[ModContent.ProjectileType<LightningBodySaria>()] <= 0f && player.ownedProjectileCounts[ModContent.ProjectileType<LightningBodySaria>()] <= 0f)
                                            {
                                                if (between3 < 2000)
                                                {
                                                    SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Lightning"));
                                                }
                                                tailIndex2 = -1;
                                                if (Main.myPlayer != player.whoAmI)
                                                    return;
                                                int curr = Projectile.NewProjectile(projectile.GetSource_FromThis(), projectile.Center, Vector2.One, ModContent.ProjectileType<LightningHeadSaria>(), (int)(projectile.damage), 0f, projectile.owner, player.whoAmI, projectile.whoAmI);
                                                if (Main.projectile.IndexInRange(curr))
                                                    for (int i = 0; i < 14; i++)
                                                        curr = Projectile.NewProjectile(projectile.GetSource_FromThis(), projectile.Center, Vector2.One, ModContent.ProjectileType<LightningBodySaria>(), (int)(projectile.damage), projectile.owner, player.whoAmI, Main.projectile[curr].identity, 0f);
                                                if (Main.projectile.IndexInRange(curr))
                                                    Main.projectile[curr].originalDamage = projectile.damage;
                                                int prev = curr;
                                                curr = Projectile.NewProjectile(projectile.GetSource_FromThis(), projectile.Center, Vector2.Zero, ModContent.ProjectileType<LightningTailSaria>(), (int)(projectile.damage), projectile.owner, player.whoAmI, Main.projectile[curr].identity, 0f);
                                                if (Main.projectile.IndexInRange(curr))
                                                    Main.projectile[curr].originalDamage = projectile.damage;
                                                Main.projectile[prev].localAI[1] = curr;
                                                tailIndex2 = curr;
                                            }
                                        }
                                    }
                                }
                            }
                            else if (Transform == 4)
                            {
                                //Main.NewText("Frame: " + projectile.frame);
                                if (ChannelAttack == 1 && (projectile.frame == 49))
                                {
                                    if (CantAttackTimer <= 0)
                                    {
                                        modProjectile.CantAttackTimer = 200;
                                    }
                                }
                                if (ChannelAttack == 0 && projectile.frame == 44)
                                {
                                    if (Main.rand.Next(101) <= 4)
                                    {
                                        SoundEngine.PlaySound(SoundID.Item77, projectile.Center);
                                        if (Main.myPlayer == projectile.owner) Projectile.NewProjectile(projectile.GetSource_FromThis(), projectile.position.X + 40, projectile.position.Y + 2000, 0, 0, ModContent.ProjectileType<RupeeXNeutral3>(), (int)(projectile.damage), 0f, projectile.owner, player.whoAmI, projectile.whoAmI);
                                        player.AddBuff(ModContent.BuffType<EmeraldBuff>(), 2);
                                    }
                                    else if (Main.rand.Next(101) > 4 && Main.rand.Next(101) <= 20)
                                    {
                                        SoundEngine.PlaySound(SoundID.Item77, projectile.Center);
                                        if (Main.myPlayer == projectile.owner) Projectile.NewProjectile(projectile.GetSource_FromThis(), projectile.position.X + 40, projectile.position.Y + 2000, 0, 0, ModContent.ProjectileType<RupeeXNeutral2>(), (int)(projectile.damage), 0f, projectile.owner, player.whoAmI, projectile.whoAmI);
                                        player.AddBuff(ModContent.BuffType<EmeraldBuff>(), 2);
                                    }
                                    else
                                    {
                                        SoundEngine.PlaySound(SoundID.Item77, projectile.Center);
                                        if (Main.myPlayer == projectile.owner) Projectile.NewProjectile(projectile.GetSource_FromThis(), projectile.position.X + 40, projectile.position.Y + 2000, 0, 0, ModContent.ProjectileType<RupeeXNeutral>(), (int)(projectile.damage), 0f, projectile.owner, player.whoAmI, projectile.whoAmI);
                                        player.AddBuff(ModContent.BuffType<EmeraldBuff>(), 2);
                                    }
                                }
                            }
                            else if (Transform == 5)
                            {
                                if (projectile.frame == 50)
                                {
                                    if (player.ownedProjectileCounts[ModContent.ProjectileType<DuskBallProjectile>()] <= 0f && Main.myPlayer == projectile.owner)
                                    {
                                        if (((Main.rand.NextBool(60)) && (player.ownedProjectileCounts[ModContent.ProjectileType<GreenMoth>()] <= 0f) && (player.ownedProjectileCounts[ModContent.ProjectileType<GreenMothGoliath2>()] <= 0f) && (player.ownedProjectileCounts[ModContent.ProjectileType<AmberGreen>()] <= 0f) && (player.ownedProjectileCounts[ModContent.ProjectileType<GreenMothGiant>()] <= 0f) && (player.ownedProjectileCounts[ModContent.ProjectileType<GreenMothGoliath>()] <= 0f) && ((player.ownedProjectileCounts[ModContent.ProjectileType<RedMoth>()] == 1f) || (player.ownedProjectileCounts[ModContent.ProjectileType<RedMothGiant>()] == 1f)) && ((player.ownedProjectileCounts[ModContent.ProjectileType<PurpleMoth>()] == 1f) || (player.ownedProjectileCounts[ModContent.ProjectileType<PurpleMothGiant>()] == 1f))))
                                        {
                                            {
                                                modPlayer.SariaXp++;
                                                SoundEngine.PlaySound(SoundID.Item77, projectile.Center);
                                                SoundEngine.PlaySound(SoundID.Item77, projectile.Center);
                                                if (Main.myPlayer == projectile.owner) Projectile.NewProjectile(projectile.GetSource_FromThis(), projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ProjectileType<AmberGreen>(), (int)(projectile.damage), 0f, projectile.owner, player.whoAmI, projectile.whoAmI);
                                            }
                                        }
                                        else
                                        {
                                            if (Main.myPlayer == projectile.owner)
                                            {
                                                if (((player.ownedProjectileCounts[ModContent.ProjectileType<RedMoth>()] <= 0f) && (player.ownedProjectileCounts[ModContent.ProjectileType<RedMothGiant>()] <= 0f)))
                                                {
                                                    modPlayer.SariaXp++;
                                                    if (Main.myPlayer == projectile.owner) Projectile.NewProjectile(projectile.GetSource_FromThis(), projectile.Center + new Vector2(-250f, 370f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ProjectileType<AmberRed>(), (int)(projectile.damage), 0f, projectile.owner, player.whoAmI, projectile.whoAmI);
                                                }
                                                if (((player.ownedProjectileCounts[ModContent.ProjectileType<PurpleMoth>()] <= 0f) && (player.ownedProjectileCounts[ModContent.ProjectileType<PurpleMothGiant>()] <= 0f)))
                                                {
                                                    modPlayer.SariaXp++;
                                                    if (Main.myPlayer == projectile.owner) Projectile.NewProjectile(projectile.GetSource_FromThis(), projectile.Center + new Vector2(250f, 370f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ProjectileType<AmberPurple>(), (int)(projectile.damage), 0f, projectile.owner, player.whoAmI, projectile.whoAmI);
                                                }
                                                SoundEngine.PlaySound(SoundID.Item77, projectile.Center);
                                                for (int j = 0; j < 1; j++) //set to 2
                                                {
                                                    modPlayer.SariaXp++;
                                                    if (Main.myPlayer == projectile.owner) Projectile.NewProjectile(projectile.GetSource_FromThis(), projectile.Center + new Vector2(-500f, 370f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ProjectileType<AmberBlack1>(), (int)(projectile.damage), 0f, projectile.owner, player.whoAmI, projectile.whoAmI);
                                                    if (Main.myPlayer == projectile.owner) Projectile.NewProjectile(projectile.GetSource_FromThis(), projectile.Center + new Vector2(500f, 370f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ProjectileType<AmberBlack2>(), (int)(projectile.damage), 0f, projectile.owner, player.whoAmI, projectile.whoAmI);
                                                }
                                            }
                                            projectile.netUpdate = true;
                                        }
                                    }
                                }
                            }
                            else if (Transform == 6)
                            {
                                if (projectile.frame == 50 && Main.myPlayer == projectile.owner)
                                {
                                    SoundEngine.PlaySound(SoundID.Item77, projectile.Center);
                                    SoundEngine.PlaySound(SoundID.Item77, projectile.Center);
                                    for (int j = 0; j < 1; j++) //set to 2
                                    {
                                        if (Main.myPlayer == projectile.owner) Projectile.NewProjectile(projectile.GetSource_FromThis(), projectile.position.X + 60, projectile.position.Y + 40, 0, 0, ModContent.ProjectileType<Shadowmelt>(), (int)(projectile.damage), 0f, projectile.owner, player.whoAmI, projectile.whoAmI);
                                    }
                                }
                            }
                        }
                        if (projectile.frame == 48 && player.HasBuff(ModContent.BuffType<HealpulseBuff>()))
                        {
                            SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Error"), projectile.Center);
                        }
                        //////////////////// End of Transform attacks
                        ///
                        if (projectile.frame > 56) //stop when done
                        {
                            projectile.frame = 0;
                            modProjectile.ChannelAttack = 0;
                            projectile.ai[0] = 3;
                        }
                    }
                    if (projectile.ai[0] == 1) //this is set when a target is found
                    {
                        projectile.frame = 43; //animation setup
                        projectile.frameCounter += 16;
                        SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Step1"), projectile.Center);
                        projectile.ai[0] = 2; //next phase
                    }
                }
            }
        }
       public static void SariaAI(this Projectile projectile, int Transform, int ChannelTime, bool NotActive, bool foundTarget, bool Sleep, bool HoldingHealBall, int CantAttackTimer, int ChannelState, int Eating, bool CanActuallySee)
        {
            Player player = Main.player[projectile.owner];
            ///Saria second form healing when player is damaged
            float between33 = Vector2.Distance(player.Center, projectile.Center);
            for (int i = 0; i < 100; i++)
            {
                Player player3 = Main.player[i];
                float between35 = Vector2.Distance(Main.player[i].Center, projectile.Center);
                if (((Main.player[i].statLife < Main.player[i].statLifeMax2) && Main.player[i].active && Main.player[i] != player && (Main.player[i].team == player.team) && (between35 <= 100) && Transform == 1 && projectile.ai[0] == 0 && ChannelTime <= 10 && NotActive))
                {
                    if (CantAttackTimer <= 0)
                    {
                        projectile.ai[0] = 1;
                    }
                }
            }
            if ((player.statLife < player.statLifeMax2) && player.active && (between33 <= 250) && Transform == 1 && projectile.ai[0] == 0 && ChannelTime <= 10 && NotActive)
            {
                if (CantAttackTimer <= 0)
                {
                    projectile.ai[0] = 1;
                }
            }
            /// End of saria healing player code
            if (CantAttackTimer <= 0 && !(player.sleeping.isSleeping) && ChannelTime <= 20 && ChannelState <= 0 && foundTarget && projectile.ai[0] == 0 && !Sleep && Eating <= 0 && HoldingHealBall)
            {
                if (Transform == 2)
                {
                    if (player.ownedProjectileCounts[ModContent.ProjectileType<RubyPsychicSeeker>()] <= 0f)
                    {
                        projectile.ai[0] = 1;
                    }
                }
                else if (Transform == 3)
                {
                    if (CanActuallySee && player.ownedProjectileCounts[ModContent.ProjectileType<LightningHeadSaria>()] <= 0f && player.ownedProjectileCounts[ModContent.ProjectileType<LightningBodySaria>()] <= 0f)
                    {
                        projectile.ai[0] = 1;
                    }
                }
                else
                {
                    projectile.ai[0] = 1;
                }
            }
            if (Transform == 6 && CantAttackTimer <= 0)
            {
                for (int b = 0; b < Main.maxNPCs; b++)
                {
                    NPC npc = Main.npc[b];
                    float between2 = Vector2.Distance(npc.Center, projectile.Center);
                    // Reasonable distance away so it doesn't target across multiple screens
                    if (between2 < 1200f && npc.friendly == false && foundTarget)
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
                                            if (Main.myPlayer == projectile.owner) Projectile.NewProjectile(projectile.GetSource_FromThis(), npc.position.X + 0, npc.position.Y + -24, 0, 0, ModContent.ProjectileType<ShadowClaw>(), (int)(projectile.damage), 0f, projectile.owner, player.whoAmI, projectile.whoAmI);
                                            projectile.netUpdate = true;
                                        }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}