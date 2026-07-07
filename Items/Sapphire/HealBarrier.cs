using Microsoft.Xna.Framework;
using SariaMod.Buffs;
using SariaMod.Dusts;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using SariaMod.Items.Amber;
using SariaMod.Items.Amethyst;
using SariaMod.Items.Bands;
using SariaMod.Items.Emerald;
using SariaMod.Items.Ruby;
using SariaMod.Items.Sapphire;
using SariaMod.Items.Topaz;
using System;
using System.IO;
namespace SariaMod.Items.Sapphire
{
    public class HealBarrier : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Saria");
            ProjectileID.Sets.TrailCacheLength[base.Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[base.Projectile.type] = 0;
            Main.projFrames[base.Projectile.type] = 1;
        }
        public bool HasHealed = false;
        public override bool? CanCutTiles()
        {
            return false;
        }
        public override void SetDefaults()
        {
            base.Projectile.width = 250;
            base.Projectile.height = 250;
            base.Projectile.alpha = 300;
            base.Projectile.friendly = true;
            base.Projectile.tileCollide = false;
            base.Projectile.netImportant = true;
            base.Projectile.penetrate = -1;
            base.Projectile.timeLeft = 4;
            base.Projectile.ignoreWater = true;
            Projectile.timeLeft = 24;
            base.Projectile.usesLocalNPCImmunity = true;
            base.Projectile.localNPCHitCooldown = 160;
        }
        public override bool? CanHitNPC(NPC target)
        {
            return false;
        }
        public override bool MinionContactDamage()
        {
                return false;
        }
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(HasHealed);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            HasHealed = (bool)reader.ReadBoolean();
        }
        public override void AI()
        {
            Player player = Main.player[base.Projectile.owner];
            Player player2 = Main.LocalPlayer;
            FairyPlayer modPlayer = player.Fairy();
            Lighting.AddLight(base.Projectile.Center, 0f, 0.5f, 0f);
            int Yesh = ((player2.statManaMax2) / 8);
            int Yesh2 = ((player2.statManaMax2) / 5);
            int HealAmount = (player.statLifeMax2 / (20 - modPlayer.Sarialevel));
            if (player.HasBuff(ModContent.BuffType<Overcharged>()))
            {
                HealAmount = (player.statLifeMax2 / (18 - modPlayer.Sarialevel));
            }
                if (player.ownedProjectileCounts[ModContent.ProjectileType<HealCursorVisual>()] <= 0f)
            {
                if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), player.position.X + 0, player.position.Y + 0, 0, 0, ModContent.ProjectileType<HealCursorVisual>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, Projectile.whoAmI);
            }
            if (Projectile.timeLeft == 24)
            {
                for (int i = 0; i < 50; i++)
                {
                    Vector2 speed = Main.rand.NextVector2CircularEdge(0.45f, 0.45f);
                    Dust d = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<HealingDust>(), speed * -18, Scale: 5.1f);
                    d.noGravity = true;
                }
            }
            
            // MAGIC MISSILE PATTERN: Only owner calculates velocity from mouse
            if (Projectile.owner == Main.myPlayer)
            {
                Vector2 mouse = Main.MouseWorld;
                mouse.X += -20f;
                mouse.Y -= 5f;
                
                float speed2 = 70f;
                float between = Vector2.Distance(mouse, Projectile.Center);
                
                if (between >= 1 && Projectile.timeLeft < 90)
                {
                    Vector2 direction2 = mouse - Projectile.Center;
                    direction2.Normalize();
                    direction2 *= speed2;
                    Projectile.velocity = (Projectile.velocity * (19 - 2) + direction2) / 22;
                }
                
                // Sync velocity to other clients
                Projectile.netUpdate = true;
            }
            // Non-owner clients just use the synced velocity (no mouse calculations)
            
            if (!HasHealed)
            {
                for (int i = 0; i < 100; i++)
                {
                    Player player3 = Main.player[i];
                    if (((Main.player[i].statLife < Main.player[i].statLifeMax2) && Main.player[i].Hitbox.Intersects(Projectile.Hitbox) && Main.player[i].active && Main.player[i] != player && !Main.player[i].HasBuff(ModContent.BuffType<Healed>()) && (Main.player[i].team == player.team)))
                    {
                        if ((Main.player[i].statLife < Main.player[i].statLifeMax2) || (Main.player[i].statMana < Main.player[i].statManaMax2))
                        {
                            Main.player[i].AddBuff(ModContent.BuffType<Healed>(), 30);
                            for (int g = 0; g < 50; g++)
                            {
                                Vector2 speed = Main.rand.NextVector2CircularEdge(1f, 1f);
                                Dust d = Dust.NewDustPerfect(Main.player[i].Center, ModContent.DustType<Healdust3>(), speed * 2, Scale: 2.1f);
                                d.noGravity = true;
                            }
                            SoundEngine.PlaySound(SoundID.DD2_DarkMageHealImpact, base.Projectile.Center);
                            Projectile.netUpdate = true;
                            Main.player[i].statMana += Yesh;
                            Main.player[i].ManaEffect(Yesh);
                            Main.player[i].Heal(HealAmount);
                            HasHealed = true;
                        }
                    }
                }
                if (player.Hitbox.Intersects(Projectile.Hitbox) && player.active && !player.HasBuff(ModContent.BuffType<Healed>()))
                {
                    {
                        if ((player.statLife < player.statLifeMax2) || (player.statMana < player.statManaMax2))
                        {
                            player.AddBuff(ModContent.BuffType<Healed>(), 30);
                            for (int i = 0; i < 50; i++)
                            {
                                Vector2 speed = Main.rand.NextVector2CircularEdge(1f, 1f);
                                Dust d = Dust.NewDustPerfect(player2.Center, ModContent.DustType<Healdust3>(), speed * 2, Scale: 2.1f);
                                d.noGravity = true;
                            }
                            SoundEngine.PlaySound(SoundID.DD2_DarkMageHealImpact, base.Projectile.Center);
                            Projectile.netUpdate = true;
                            player.statMana += Yesh;
                            player.ManaEffect(Yesh);
                            player.Heal(HealAmount);
                            HasHealed = true;
                        }
                    }
                }
            }
        }
        public override void Kill(int timeLeft)
        {
            Player player = Main.player[Projectile.owner];
            FairyPlayer modPlayer = player.Fairy();
            if (HasHealed)
            {
                modPlayer.StoredHealth -= 25;
            }
        }
    }
}
