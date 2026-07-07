using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using SariaMod.Buffs;
using SariaMod.Dusts;
using SariaMod.Items.Amber;
using SariaMod.Items.Amethyst;
using SariaMod.Items.Bands;
using SariaMod.Items.Emerald;
using SariaMod.Items.Ruby;
using SariaMod.Items.Sapphire;
using SariaMod.Items.Topaz;
using SariaMod.Items.zPearls;
using SariaMod.Items.zTalking;
using Terraria.Localization;
using System;
using Terraria.Map;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace SariaMod.Items.Ruby
{
    public class WillOWisp : ModProjectile
    {
        public int WispHits;
        public int WispLevel;
        public bool isActive;
        public bool isShooting;
        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Saria");
            ProjectileID.Sets.TrailCacheLength[base.Projectile.type] = 7;
            ProjectileID.Sets.TrailingMode[base.Projectile.type] = 0;
            Main.projFrames[base.Projectile.type] = 4;
            Main.projPet[Projectile.type] = true;
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = false;
            ProjectileID.Sets.MinionShot[Projectile.type] = false;
        }
        public override bool? CanCutTiles()
        {
            return false;
        }
        public override bool? CanHitNPC(NPC target)
        {
            Player player = Main.player[base.Projectile.owner];
            bool CanSee = Collision.CanHitLine(target.position, target.width, target.height, player.Center, 1, 1);
            if (player.immune == false && CanSee)
            {
                return target.CanBeChasedBy(Projectile);
            }
            else
            {
                return false;
            }
        }
        public override bool MinionContactDamage()
        {
                return true;
        }
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(WispHits);
            writer.Write(WispLevel);
            writer.Write(isActive);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            WispHits = (int)reader.ReadInt32();
            WispLevel = (int)reader.ReadInt32();
            isActive = (bool)reader.ReadBoolean();
        }
        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
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
            target.AddBuff(ModContent.BuffType<GhostBurning>(), 6400);
            target.AddBuff(BuffID.Confused, 300);
            modPlayer.SariaXp++;
            damage = 1;
            knockback = 30;
            if (target.position.X + (float)(target.width / 2) > Projectile.position.X + (float)(Projectile.width / 2))
            {
                hitDirection = 1;
            }
            else
            {
                hitDirection = -1;
            }
            if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), player.position.X + 0, player.position.Y + 0, 0, 0, ModContent.ProjectileType<ProjectileBurn2>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, Projectile.whoAmI);
        }
        public override void SetDefaults()
        {
            Projectile.width = 80;
            Projectile.height = 70;
            Projectile.alpha = 300;
            Projectile.netImportant = true;
            Projectile.friendly = true;
            Projectile.netUpdate = true;
            Projectile.ignoreWater = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 50;
            Projectile.minionSlots = 0f;
            Projectile.timeLeft = 300;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.minion = false;
        }
        public override void AI()
        {
            Player player = Main.player[base.Projectile.owner];
            Player player2 = Main.LocalPlayer;
            float between = Vector2.Distance(player2.Center, Projectile.Center);
            if (base.Projectile.localAI[0] == 0f)
            {
                base.Projectile.localAI[0] = 1f;
            }
            if (WispHits > 24)
            {
                WispHits = 24;
            }
            //////
            if (isActive == false)
            {
                WispHits = 3;
                isActive = true;
            }
            if (WispHits < 0 && player.ownedProjectileCounts[ModContent.ProjectileType<WillOWisp2>()] <= 0 && Main.myPlayer == Projectile.owner)
            {
                Projectile.Kill();
            }
            if (WispHits < 0)
            {
                WispLevel = -1;
            }
            if (WispHits > 0 && WispHits < 4)
            {
                WispLevel = 0;
            }
            if (WispHits > 3 && WispHits < 7)
            {
                WispLevel = 1;
            }
            if (WispHits > 6 && WispHits < 10)
            {
                WispLevel = 2;
            }
            if (WispHits > 9 && WispHits < 13)
            {
                WispLevel = 3;
            }
            if (WispHits > 12 && WispHits < 16)
            {
                WispLevel = 4;
            }
            if (WispHits > 15 && WispHits < 19)
            {
                WispLevel = 5;
            }
            if (WispHits > 18 && WispHits < 22)
            {
                WispLevel = 6;
            }
            if (WispHits > 21 && WispHits < 25)
            {
                WispLevel = 7;
            }
            ///////
            if (between < 500f)
            {
                player2.resistCold = true;
                player2.AddBuff(BuffID.Warmth, 20);
            }
            Projectile.Center = player.Center;
            int WispMax = 7;
            if (WispLevel == 7)
            {
                WispMax = 7;
            }
            if (WispLevel == 6)
            {
                WispMax = 6;
            }
            if (WispLevel == 5)
            {
                WispMax = 5;
            }
            if (WispLevel == 4)
            {
                WispMax = 4;
            }
            if (WispLevel == 3)
            {
                WispMax = 3;
            }
            if (WispLevel == 2)
            {
                WispMax = 2;
            }
            if (WispLevel == 1)
            {
                WispMax = 1;
            }
            if (WispLevel == 0)
            {
                WispMax = 0;
            }
            if (WispLevel == -1)
            {
                WispMax = -1;
            }
            int owner = player.whoAmI;
            for (int i = 0; i < 1000; i++)
            {
                if (Main.projectile[i].active && i != base.Projectile.whoAmI && player.immune == false && Main.projectile[i].Hitbox.Intersects(base.Projectile.Hitbox) && Main.projectile[i].active && ((!Main.projectile[i].friendly && Main.projectile[i].hostile) || (Main.projectile[i].trap)))
                {
                    bool CanSee = Collision.CanHitLine(Main.projectile[i].position, Main.projectile[i].width, Main.projectile[i].height, player.Center, 1, 1);
                    if (CanSee)
                    {
                        Main.projectile[i].Kill();
                        if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), player.position.X + 0, player.position.Y + 0, 0, 0, ModContent.ProjectileType<ProjectileBurn>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, Projectile.whoAmI);
                    }
                }
            }
            if (player.ownedProjectileCounts[ModContent.ProjectileType<WillOWisp2>()] > WispMax + 1)
            {
                for (int v = 0; v < 1000; v++)
                {
                    if (Main.projectile[v].active && Main.projectile[v].ModProjectile is WillOWisp2 modProjectile && v != base.Projectile.whoAmI && (Main.projectile[v].owner == owner) && (Main.myPlayer == Projectile.owner))
                    {
                        Main.projectile[v].Kill();
                    }
                }
            }
            if (player.ownedProjectileCounts[ModContent.ProjectileType<WillOWisp2>()] <= WispMax && WispLevel >= 0)
            {
                if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), player.position.X + 0, player.position.Y + 0, 0, 0, ModContent.ProjectileType<WillOWisp2>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, Projectile.whoAmI);
            }
            if (player.HasBuff(ModContent.BuffType<WillOWispBuff>()))
            {
                Projectile.timeLeft = 18;
            }
        }
        public override void PostDraw(Color lightColor)
        {
            {
                Texture2D starTexture2 = TextureAssets.Projectile[ModContent.ProjectileType<WillOWisp>()].Value;
                Vector2 drawPosition;
                {
                    Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Ruby/WillOWispTexture");
                    Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                    int frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<WillOWisp>()];
                    Color drawColor = Color.Lerp(lightColor, Color.MediumPurple, 20f);
                    drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                    Rectangle rectangle = texture.Frame(verticalFrames: 4, frameY: (int)Main.GameUpdateCount / 6 % 4);
                    Vector2 origin = rectangle.Size() / 2f;
                    float rotation = 0;
                    float scale = base.Projectile.scale;
                    SpriteEffects spriteEffects = SpriteEffects.None;
                    Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                }
            }
        }
        public override void Kill(int timeLeft)
        {
            for (int i = 0; i < 50; i++)
            {
                Vector2 dustspeed5 = Main.rand.NextVector2CircularEdge(1f, 1f);
                Vector2 newmiddle = Projectile.Center;
                newmiddle.Y += 30;
                Dust d = Dust.NewDustPerfect(newmiddle, ModContent.DustType<ShadowFlameDustCharge>(), dustspeed5 * 10, Scale: 6.5f);
                d.noGravity = true;
            }
            SoundEngine.PlaySound(SoundID.Item20, base.Projectile.Center);
            if (Main.rand.NextBool(11))
            {
                SoundEngine.PlaySound(SoundID.NPCDeath52, base.Projectile.Center);
            }
            else
            {
                SoundEngine.PlaySound(SoundID.NPCDeath6, base.Projectile.Center);
            }
        }
    }
}
