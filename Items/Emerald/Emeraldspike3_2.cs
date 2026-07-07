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
    public class Emeraldspike3_2 : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Saria");
            ProjectileID.Sets.TrailCacheLength[base.Projectile.type] = 6;
            ProjectileID.Sets.TrailingMode[base.Projectile.type] = 0;
            Main.projFrames[base.Projectile.type] = 1;
        }
        public static float alpha2;
        public static bool alpha2Counter;
        public override void SetDefaults()
        {
            base.Projectile.width = 350;
            base.Projectile.height = 450;
            base.Projectile.aiStyle = 21;
            base.Projectile.alpha = 300;
            base.Projectile.friendly = true;
            base.Projectile.tileCollide = false;
            base.Projectile.penetrate = -1;
            base.Projectile.timeLeft = 200;
            base.Projectile.ignoreWater = true;
            base.Projectile.usesLocalNPCImmunity = true;
            base.Projectile.localNPCHitCooldown = 20;
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
            if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), player.Center.X, player.Center.Y + 50, 0, 0, ModContent.ProjectileType<HitCheck3>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
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
                damage += (damage) / 4;
            }
            if (player.HasBuff(ModContent.BuffType<StatLower>()))
            {
                damage /= 2;
            }
        }
        public override bool? CanHitNPC(NPC target)
        {
            Player player = Main.player[base.Projectile.owner];
            if (!target.HasBuff(ModContent.BuffType<MeteorSpikeDebuff>()) && !target.HasBuff(ModContent.BuffType<MeteorLaunchDebuff>()))
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
            if (alpha2Counter)
            {
                alpha2 -= 0.004f;
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
            if (player.HasBuff(ModContent.BuffType<StatRaise>()))
            {
                Projectile.localNPCHitCooldown = 16;
            }
            if (player.HasBuff(ModContent.BuffType<StatLower>()))
            {
                Projectile.localNPCHitCooldown = 160;
            }
            Projectile.localNPCHitCooldown = 14;
            if (player.velocity.Y > 0 && (player.ownedProjectileCounts[ModContent.ProjectileType<Sweetspot4>()] <= 0f))
            {
                if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), player.Center.X, player.Center.Y + 50, 0, 0, ModContent.ProjectileType<Sweetspot4>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
            }
            int owner = player.whoAmI;
            for (int U = 0; U < 1000; U++)
            {
                if (Main.projectile[U].active && Main.projectile[U].ModProjectile is RupeeXPassive3 modRupee && U != Projectile.whoAmI && ((Main.projectile[U].owner == owner)))
                {
                    if (modRupee.Damage > 20)
                    {
                        Projectile.Kill();
                    }
                }
            }
                    if (player.ownedProjectileCounts[ModContent.ProjectileType<ExtraHitBox>()] <= 0f)
            {
                if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), player.Center.X - 230, player.Center.Y - 50, 0, 0, ModContent.ProjectileType<ExtraHitBox>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
            }
            if (player.ownedProjectileCounts[ModContent.ProjectileType<Emeraldspike3>()] <= 0f)
            {
                Projectile.Kill();
            }
            for (int i = 0; i < 1000; i++)
                if (Main.projectile[i].active && i != base.Projectile.whoAmI && Main.projectile[i].Hitbox.Intersects(base.Projectile.Hitbox) && Main.projectile[i].active && ((!Main.projectile[i].friendly && Main.projectile[i].hostile) || (Main.projectile[i].trap)))
                {
                    Main.projectile[i].Kill();
                    if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), player.position.X + 0, player.position.Y + 0, 0, 0, ModContent.ProjectileType<HitCheck3>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, Projectile.whoAmI);
                    if (!player.HasBuff(ModContent.BuffType<Overcharged>()))
                    {
                        if (Main.rand.NextBool(60))
                        {
                            Item.NewItem(Projectile.GetSource_FromThis(), Projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ItemType<LivingSilverShard>());
                        }
                    }
                    if (player.HasBuff(ModContent.BuffType<Overcharged>()))
                    {
                        if (Main.rand.NextBool(25))
                        {
                            Item.NewItem(Projectile.GetSource_FromThis(), Projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ItemType<LivingSilverShard>());
                        }
                    }
                }
            FairyProjectile.HomeInOnNPC(base.Projectile, ignoreTiles: true, 600f, 25f, 20f);
            Lighting.AddLight(Projectile.Center, Color.Silver.ToVector3() * 2f);
            Projectile.position.X = player.position.X - 160;
            Projectile.position.Y = player.Center.Y - 400;
                Projectile.spriteDirection = player.direction;
            if (Projectile.timeLeft < 199 && Projectile.timeLeft > 10)
            {
                Projectile.timeLeft = 180;
            }
        }
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            overPlayers.Add(index);
            overWiresUI.Add(index);
        }
        public override void PostDraw(Color lightColor)
        {
            Player player = Main.player[base.Projectile.owner];
            Projectile.Emeraldspikedraw(((Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Emerald/Emeraldspike3_2").Value), lightColor, Color.GhostWhite, false, 0, 0, 1);
            Projectile.EmeraldspikeGlowandFadedraw(((Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Emerald/EmeraldspikeFrame2Mask3").Value), lightColor, Color.GhostWhite, alpha2, 1);
        }
        public override void Kill(int timeLeft)
        {
            Player player = Main.player[Projectile.owner];
            for (int j = 0; j < 4; j++) //set to 2
            {
                if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + Utils.RandomVector2(Main.rand, -200f, 200f), Vector2.One.RotatedByRandom(6.2831854820251465) * 1f, ModContent.ProjectileType<Crystalshard4>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
            }
            for (int j = 0; j < 5; j++) //set to 2
            {
                if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + Utils.RandomVector2(Main.rand, -200f, 200f), Vector2.One.RotatedByRandom(6.2831854820251465) * 1f, ModContent.ProjectileType<ShardDust3>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
            }
            SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact, base.Projectile.Center);
        }
    }
}
