using Microsoft.Xna.Framework;
using SariaMod.Buffs;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using SariaMod.Items.Emerald;
using SariaMod.Dusts;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using ReLogic.Content;
using Terraria.Graphics;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria.ObjectData;
namespace SariaMod.Items.Emerald
{
    public class Emeraldspike2 : ModProjectile
    {
        public bool MustBreak;
        public bool groundsound;
        public int soundcheck;
        public static float alpha2;
        public static bool alpha2Counter;
        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Saria");
            ProjectileID.Sets.TrailCacheLength[base.Projectile.type] = 6;
            ProjectileID.Sets.TrailingMode[base.Projectile.type] = 0;
            Main.projFrames[base.Projectile.type] = 3;
        }
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(MustBreak);
            writer.Write(groundsound);
            writer.Write(soundcheck);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            MustBreak = (bool)reader.ReadBoolean();
            groundsound = (bool)reader.ReadBoolean();
            soundcheck = (int)reader.ReadInt32();
        }
        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            Player player = Main.player[base.Projectile.owner];
            Player player2 = Main.LocalPlayer;
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
            target.AddBuff(BuffID.Slow, 300);
            modPlayer.SariaXp++;
            knockback = 10;
            if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), player.Center.X, player.Center.Y + 50, 0, 0, ModContent.ProjectileType<HitCheck2_2>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
            if (target.position.X + (float)(target.width / 2) > Projectile.position.X + (float)(Projectile.width / 2))
            {
                hitDirection = 1;
            }
            else
            {
                hitDirection = -1;
            }
            if (player.HasBuff(ModContent.BuffType<StatRaise>()))
            {
                damage /= 2;
            }
            if (player.HasBuff(ModContent.BuffType<StatLower>()))
            {
                damage /= 6;
            }
            else
            {
                damage /= 4;
            }
        }
        public override void SetDefaults()
        {
            base.Projectile.width = 130;
            base.Projectile.height = 180;
            base.Projectile.alpha = 300;
            base.Projectile.friendly = true;
            base.Projectile.tileCollide = false;
            base.Projectile.penetrate = -1;
            base.Projectile.timeLeft = 200;
            base.Projectile.ignoreWater = true;
            base.Projectile.usesLocalNPCImmunity = true;
            base.Projectile.localNPCHitCooldown = 20;
        }
        public override bool? CanHitNPC(NPC target)
        {
            Player player = Main.player[base.Projectile.owner];
            if (Projectile.frame >= 1 && !target.HasBuff(ModContent.BuffType<MeteorSpikeDebuff>()) && !target.HasBuff(ModContent.BuffType<MeteorLaunchDebuff>()))
            {
                return target.CanBeChasedBy(Projectile);
            }
            else
            {
                return false;
            }
        }
        public override void AI()
        {
            Player player = Main.player[base.Projectile.owner];
            Player player2 = Main.LocalPlayer;
            FairyPlayer modPlayer = player.Fairy();
            Lighting.AddLight(Projectile.Center, Color.Purple.ToVector3() * 0.78f);
            if (alpha2Counter)
            {
                alpha2 -= 0.04f;
            }
            if (alpha2 <= 0)
            {
                alpha2Counter = false;
            }
            if (!alpha2Counter)
            {
                alpha2 += 0.004f;
            }
            if (alpha2 >= 1)
            {
                alpha2Counter = true;
            }
            float distanceFromTarget = 10f;
            Vector2 targetCenter = Projectile.position;
            bool foundTarget = false;
            float speed = 20;
            Vector2 direction = targetCenter - Projectile.Center;
            int owner = player.whoAmI;
            for (int i = 0; i < 1000; i++)
            {
                if (Main.projectile[i].active && i != base.Projectile.whoAmI && player.immune == false && Main.projectile[i].Hitbox.Intersects(base.Projectile.Hitbox) && Main.projectile[i].active && ((!Main.projectile[i].friendly && Main.projectile[i].hostile) || (Main.projectile[i].trap)))
                {
                    Main.projectile[i].Kill();
                    if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), player.position.X + 0, player.position.Y + 0, 0, 0, ModContent.ProjectileType<HitCheck2>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, Projectile.whoAmI);
                }
            }
            if (player.velocity.Y > 0)
            {
                groundsound = true;
            }
            if (groundsound && player.velocity.Y == 0)
            {
                SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Rock"), Projectile.Center);
                SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/RockCrumble"), Projectile.Center);
                for (int i = 0; i < 50; i++)
                {
                    Vector2 speed2 = Main.rand.NextVector2CircularEdge(3f, .5f);
                    Dust d = Dust.NewDustPerfect(Projectile.Bottom, ModContent.DustType<RockDustRing>(), speed2 * -6, Scale: 2.7f);
                    d.noGravity = true;
                }
                groundsound = false;
            }
            if (Projectile.spriteDirection == -1)
            {
                Projectile.position.X = player.Center.X - 80;
            }
            if (Projectile.spriteDirection == 1)
            {
                Projectile.position.X = player.Center.X - 70;
            }
            Projectile.position.X = player.position.X - 50;
            Projectile.position.Y = player.Center.Y - 150;
            if (Projectile.timeLeft >= 196)
            {
                Projectile.spriteDirection = player.direction;
            }
            if (Projectile.timeLeft < 196 && Projectile.timeLeft > 10)
            {
                Projectile.timeLeft = 180;
            }
            {
                float between = Vector2.Distance(player2.Center, Projectile.Center);
                // Reasonable distance away so it doesn't target across multiple screens
                if (between < 1000f)
                {
                    player2.AddBuff(BuffID.Endurance, 3);
                }
            }
            int frameSpeed = 15;
            {
                base.Projectile.frameCounter++;
                if (Projectile.frameCounter >= frameSpeed)
                    if (base.Projectile.frameCounter > 3)
                    {
                        base.Projectile.frame++;
                        base.Projectile.frameCounter = 0;
                    }
                if (base.Projectile.frame >= 3)
                {
                    base.Projectile.frame = 2;
                }
            }
            if (player.velocity.Y > 0)
            {
                player.velocity.Y *= 4f;
            }
            if (!foundTarget)
            {
                // This code is required either way, used for finding a target
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.CanBeChasedBy())
                    {
                        float between = Vector2.Distance(npc.Center, player.Center);
                        bool closest = Vector2.Distance(Projectile.Center, targetCenter) > between;
                        bool inRange = between < distanceFromTarget;
                        // Additional check for this specific minion behavior, otherwise it will stop attacking once it dashed through an enemy while flying though tiles afterwards
                        // The number depends on various parameters seen in the movement code below. Test different ones out until it works alright
                        bool closeThroughWall = between < 1000f;
                        if (((closest && inRange) || !foundTarget) && (closeThroughWall))
                        {
                            distanceFromTarget = between;
                            targetCenter = npc.Center;
                            targetCenter.Y -= 0f;
                            targetCenter.X += 0f;
                            foundTarget = true;
                        }
                    }
                }
            }
            if (player.velocity.Y == 0)
            {
                player.wingTime = player.wingTimeMax;
            }
            if (player.velocity.Y > 0 && (player.ownedProjectileCounts[ModContent.ProjectileType<Sweetspot2>()] <= 0f))
            {
                if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), player.Center.X, player.Center.Y + 50, 0, 0, ModContent.ProjectileType<Sweetspot2>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
            }
            if (Projectile.frame == 0)
            {
                SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact, base.Projectile.Center);
            }
            if (Projectile.frame == 1)
            {
                SoundEngine.PlaySound(SoundID.DD2_WitherBeastDeath, base.Projectile.Center);
            }
            if (Projectile.frame == 1)
            {
                SoundEngine.PlaySound(SoundID.DD2_DarkMageHealImpact, base.Projectile.Center);
            }
            Projectile.RockDust(ModContent.DustType<RockSparkle>(), (5), 120, 120, 0, 0, 0);
            if (Main.myPlayer == Projectile.owner) Main.blockMouse = true;
            
            // Break if player has active hookshot/longshot (conflicts with crystal freeze)
            if ((player.ownedProjectileCounts[ModContent.ProjectileType<Items.Bands.HookshotProjectile>()] > 0 ||
                 player.ownedProjectileCounts[ModContent.ProjectileType<Items.Bands.LongshotProjectile>()] > 0) &&
                Projectile.timeLeft > 20 && Projectile.frame >= 1 && (Main.myPlayer == Projectile.owner))
            {
                Projectile.Kill();
            }
            
            if (((player.controlLeft && modPlayer.holdingleft) || (player.controlRight && modPlayer.holdingright) || (player.controlDown && modPlayer.holdingdown) || Math.Abs(player.velocity.X) >= 0.3f || !player.releaseJump || player.controlMount || MustBreak) && Projectile.timeLeft > 20 && (Main.myPlayer == Projectile.owner))
            {
                if (player.controlLeft || player.controlRight)
                {
                    player.velocity.X = (15 * player.direction);
                }
                if (!player.releaseJump)
                {
                    player.velocity.Y = -15f;
                }
                Projectile.Kill();
            }
            // Break if player tries to use an item (no velocity boost)
            if (player.controlUseItem && Projectile.timeLeft > 20 && Projectile.frame >= 1 && (Main.myPlayer == Projectile.owner))
            {
                Projectile.Kill();
            }
        }
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            overPlayers.Add(index);
            overWiresUI.Add(index);
        }
        public override void PostDraw(Color lightColor)
        {
            Projectile.Emeraldspikedraw(((Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Emerald/Emeraldspike2").Value), lightColor, Color.GhostWhite, true, 0, 0, 3);
            Player player = Main.player[base.Projectile.owner];
            int owner = player.whoAmI;
            if (player.ownedProjectileCounts[ModContent.ProjectileType<RupeeXPassive2>()] >= 1f)
            {
                for (int U = 0; U < 1000; U++)
                {
                    if (Main.projectile[U].active && Main.projectile[U].ModProjectile is RupeeXPassive2 modRupee && U != Projectile.whoAmI && ((Main.projectile[U].owner == owner)))
                    {
                        if (modRupee.Damage <= 10f && Projectile.frame >= 2)
                        {
                            Projectile.Emeraldspikedraw(((Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Emerald/EmeraldspikeFrame1_2").Value), lightColor, Color.GhostWhite, false, 0, 0, 1);
                            Projectile.EmeraldspikeGlowandFadedraw(((Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Emerald/EmeraldspikeFrame1Mask2").Value), lightColor, Color.GhostWhite, alpha2, 1);
                            soundcheck = 0;
                        }
                        if (modRupee.Damage > 10f && modRupee.Damage <= 20f)
                        {
                            if (Projectile.frame >= 2)
                            {
                                Projectile.Emeraldspikedraw(((Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Emerald/EmeraldspikeFrame1_2").Value), lightColor, Color.GhostWhite, false, 0, 0, 1);
                                Projectile.EmeraldspikeGlowandFadedraw(((Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Emerald/EmeraldspikeFrame1Mask2").Value), lightColor, Color.GhostWhite, alpha2, 1);
                                Projectile.Emeraldspikedraw(((Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Emerald/EmeraldspikeFrame1Crack2").Value), lightColor, Color.GhostWhite, false, 0, 0, 1);
                            }
                            if (soundcheck < 1 && Projectile.timeLeft > 180)
                            {
                                soundcheck = 1;
                            }
                            else if (soundcheck < 1 && Projectile.timeLeft <= 180 && Projectile.timeLeft > 20)
                            {
                                for (int j = 0; j < 5; j++) //set to 2
                                {
                                    if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 2.5f, ModContent.ProjectileType<PurpleRupeeShard>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                                }
                                SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact, base.Projectile.Center);
                                soundcheck = 1;
                            }
                        }
                        if (modRupee.Damage > 20f)
                        {
                            if (Projectile.frame >= 2)
                            {
                                Projectile.Emeraldspikedraw(((Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Emerald/EmeraldspikeFrame1_2").Value), lightColor, Color.GhostWhite, false, 0, 0, 1);
                                Projectile.EmeraldspikeGlowandFadedraw(((Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Emerald/EmeraldspikeFrame1Mask2").Value), lightColor, Color.GhostWhite, alpha2, 1);
                                Projectile.Emeraldspikedraw(((Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Emerald/EmeraldspikeFrame2Crack2").Value), lightColor, Color.GhostWhite, false, 0, 0, 1);
                            }
                            if (soundcheck < 2 && Projectile.timeLeft > 180)
                            {
                                soundcheck = 2;
                            }
                            else if (soundcheck < 2 && Projectile.timeLeft <= 180 && Projectile.timeLeft > 20)
                            {
                                for (int j = 0; j < 5; j++) //set to 2
                                {
                                    if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 2.5f, ModContent.ProjectileType<PurpleRupeeShard>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                                }
                                SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact, base.Projectile.Center);
                                soundcheck = 2;
                            }
                        }
                        if (modRupee.Damage > 30f)
                        {
                            modRupee.Mustbreak = true;
                            MustBreak = true;
                        }
                    }
                }
            }
        }
        public override void Kill(int timeLeft)
        {
            Player player = Main.player[base.Projectile.owner];
            int owner = player.whoAmI;
            for (int j = 0; j < 5; j++) //set to 2
            {
                if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 2.5f, ModContent.ProjectileType<ShardDust2>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
            }
            for (int i = 0; i < 50; i++)
            {
                Vector2 dustspeed5 = Main.rand.NextVector2CircularEdge(1.6f, 1.6f);
                Vector2 newmiddle = Projectile.Center;
                newmiddle.Y += 30;
                Dust d = Dust.NewDustPerfect(newmiddle, ModContent.DustType<RockDust2>(), dustspeed5 * -5, Scale: 4.5f);
                d.noGravity = true;
            }
            SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact, base.Projectile.Center);
        }
    }
}
