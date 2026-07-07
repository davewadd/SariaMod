using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SariaMod.Buffs;
using SariaMod.Dusts;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using System;
namespace SariaMod.Items.Topaz
{
    public class LightningHeadSaria : ModProjectile
    {
        private static Vector2 WorldTopLeft(int tileDist = 15) => new Vector2(tileDist * 16f);
        private static Vector2 WorldBottomRight(int tileDist = 15) => new Vector2(Main.maxTilesX - tileDist, Main.maxTilesY - tileDist) * 16f;
        public int hitground;
        public int hitground2;
        public int Search;
        public int Contact;
        public int Colar;
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            hitground = (int)reader.ReadInt32();
            hitground2 = (int)reader.ReadInt32();
            Search = (int)reader.ReadInt32();
            Contact = (int)reader.ReadInt32();
            Colar = (int)reader.ReadInt32();
        }
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(hitground);
            writer.Write(hitground2);
            writer.Write(Search);
            writer.Write(Contact);
            writer.Write(Colar);
        }
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            Main.projFrames[base.Projectile.type] = 4;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
            ProjectileID.Sets.NeedsUUID[Projectile.type] = true;
        }
        public override void SetDefaults()
        {
            Projectile.width = 6;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.netImportant = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 5000;
            Projectile.alpha = 0;
            Projectile.minion = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.DamageType = DamageClass.Summon;
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Player player = Main.player[base.Projectile.owner];
            Player player2 = Main.LocalPlayer;
            FairyPlayer modPlayer = player.Fairy();
            float between3 = Vector2.Distance(Projectile.Center, player2.Center);
            hitground++;
            return false;
        }
        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            Player player = Main.player[base.Projectile.owner];
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
            FairyPlayer modPlayer = player.Fairy();
            modPlayer.SariaXp++;
            Contact++;
            target.velocity.X = 0;
            target.velocity.Y = 0;
        }
        public override void AI()
        {
            Player player = Main.player[base.Projectile.owner];
            Player player2 = Main.LocalPlayer;
            for (int i = 0; i < 20; i++)
            {
                if (Math.Abs(Projectile.velocity.X) >= 10 || Math.Abs(Projectile.velocity.Y) >= 10)
                {
                    if (Colar == 4) Projectile.AttackDust(ModContent.DustType<StaticDustNormalPurple>(), 1, 34);
                    if (Colar == 3) Projectile.AttackDust(ModContent.DustType<StaticDustNormalPink>(), 1, 34);
                    if (Colar == 2) Projectile.AttackDust(ModContent.DustType<StaticDustNormalBlue>(), 1, 34);
                    if (Colar == 1) Projectile.AttackDust(ModContent.DustType<StaticDustNormalRed>(), 1, 34);
                    if (Colar == 0) Projectile.AttackDust(ModContent.DustType<StaticDustNormal>(), 1, 34);
                }
            }
            float Ap = 5;
            if ((player.HasBuff(ModContent.BuffType<Overcharged>())))
            {
                Ap = 10;
            }
            else if ((player.HasBuff(ModContent.BuffType<StatLower>())))
            {
                Ap = 2;
            }
            else if ((player.HasBuff(ModContent.BuffType<StatRaise>())))
            {
                Ap = 7;
            }
            else
            {
                Ap = 5;
            }
            if ((player.HasBuff(ModContent.BuffType<Overcharged>())) && !Main.player[Main.myPlayer].ZoneSnow)
            {
                Lighting.AddLight(Projectile.Center, Color.White.ToVector3());
                Colar = 4;
            }
            else if (Main.player[Main.myPlayer].ZoneSnow)
            {
                Lighting.AddLight(Projectile.Center, Color.LightPink.ToVector3());
                Colar = 3;
            }
            else if ((player.HasBuff(ModContent.BuffType<StatRaise>())))
            {
                Lighting.AddLight(Projectile.Center, Color.LightBlue.ToVector3());
                Colar = 2;
            }
            else if (player.HasBuff(ModContent.BuffType<StatLower>()))
            {
                Lighting.AddLight(Projectile.Center, Color.White.ToVector3());
                Colar = 1;
            }
            else
            {
                Lighting.AddLight(Projectile.Center, Color.White.ToVector3());
                Colar = 0;
            }
            if (Projectile.timeLeft <= 499)
            {
                Projectile.tileCollide = true;
            }
            if (Projectile.alpha >= 300)
            {
                Projectile.Kill();
            }
            Projectile.Center = Vector2.Clamp(Projectile.Center, WorldTopLeft(10), WorldBottomRight(10));
            Player owner = Main.player[Projectile.owner];
            FairyPlayer modPlayer = owner.Fairy();
            float betweenSound = Vector2.Distance(player2.Center, Projectile.Center);
            if (hitground == 1)
            {
                if (betweenSound < 2000)
                {
                    SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Lightning2"));
                }
                for (int i = 0; i < 100; i++)
                {
                    Vector2 speed = Main.rand.NextVector2CircularEdge(7f, 1f);
                    if (Colar == 0)
                    {
                        Dust d = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<StaticDust>(), speed * -.5f, Scale: 1f);
                    }
                    if (Colar == 1)
                    {
                        Dust d = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<StaticDustRed>(), speed * -.5f, Scale: 1f);
                    }
                    if (Colar == 2)
                    {
                        Dust d = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<StaticDustBlue>(), speed * -.5f, Scale: 1f);
                    }
                    if (Colar == 3)
                    {
                        Dust d = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<StaticDustPink>(), speed * -.5f, Scale: 1f);
                    }
                    if (Colar == 4)
                    {
                        Dust d = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<StaticDustPurple>(), speed * -.5f, Scale: 1f);
                    }
                }
                for (int b = 0; b < 100; b++)
                {
                    Vector2 speed = Main.rand.NextVector2CircularEdge(1f, 7f);
                    if (Colar == 0)
                    {
                        Dust d = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<StaticDust>(), speed * -.5f, Scale: 1f);
                    }
                    if (Colar == 1)
                    {
                        Dust d = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<StaticDustRed>(), speed * -.5f, Scale: 1f);
                    }
                    if (Colar == 2)
                    {
                        Dust d = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<StaticDustBlue>(), speed * -.5f, Scale: 1f);
                    }
                    if (Colar == 3)
                    {
                        Dust d = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<StaticDustPink>(), speed * -.5f, Scale: 1f);
                    }
                    if (Colar == 4)
                    {
                        Dust d = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<StaticDustPurple>(), speed * -.5f, Scale: 1f);
                    }
                }
                hitground += 2;
            }
            if (hitground > 0 && hitground2 < 300)
            {
                hitground2 += 2;
            }
            if (hitground <= 0)
            {
                hitground2 = 0;
            }
            if (hitground > 0)
            {
                Projectile.alpha = hitground2;
            }
            Projectile.SariaBaseDamage();
            if ((player.HasBuff(ModContent.BuffType<Overcharged>())))
            {
                Projectile.damage /= 10;
            }
            else
            {
                Projectile.damage /= 20;
            }
            FairyProjectile.HomeInOnNPC(base.Projectile, ignoreTiles: true, 600f, 200f, 0f);
            {
                float distanceFromTarget = 10f;
                Vector2 targetCenter = Projectile.position;
                float speed = 200f;
                float inertia = 3f;
                bool foundTarget = false;
                // This code is required if your minion weapon has the targeting feature
                if (player.HasMinionAttackTargetNPC)
                {
                    NPC npc = Main.npc[player.MinionAttackTargetNPC];
                    Vector2 Dot1 = npc.position;
                    Dot1.Y -= 310;
                    Vector2 DotMatch = npc.position;
                    DotMatch.Y = Projectile.position.Y;
                    Vector2 DotPlayer = player.position;
                    DotPlayer.Y = Projectile.position.Y;
                    Vector2 DotProjectile = Projectile.position;
                    DotProjectile.Y = DotPlayer.Y;
                    float between = Vector2.Distance(npc.Center, Projectile.Center);
                    float between1 = Vector2.Distance(DotProjectile, Projectile.Center);
                    float between2 = Vector2.Distance(DotPlayer, Projectile.Center);
                    bool closest = Vector2.Distance(player.Center, targetCenter) > between;
                    bool CanSee = Collision.CanHitLine(Projectile.position, Projectile.width, Projectile.height, npc.position, 1, 1);
                    bool FirstDot = Collision.CanHitLine(Projectile.position, Projectile.width, Projectile.height, Dot1, 1, 1);
                    bool SpotDot1 = Collision.CanHitLine(Dot1, Projectile.width, Projectile.height, npc.position, 1, 1);
                    bool DotMatchFind = Collision.CanHitLine(Projectile.position, Projectile.width, Projectile.height, DotMatch, 1, 1);
                    bool SpotDotMatch = Collision.CanHitLine(DotMatch, Projectile.width, Projectile.height, npc.position, 1, 1);
                    bool DotPlayer1 = Collision.CanHitLine(Projectile.position, Projectile.width, Projectile.height, DotProjectile, 1, 1);
                    bool DotPlayer2 = Collision.CanHitLine(DotProjectile, 1, 1, DotPlayer, 1, 1);
                    bool DotPlayer3 = Collision.CanHitLine(DotPlayer, 1, 1, player.position, 1, 1);
                    bool DotPlayer4 = Collision.CanHitLine(player.position, 1, 1, npc.position, 1, 1);
                    if (between1 < 50)
                    {
                        Search = 1;
                    }
                    if (between2 < 50)
                    {
                        Search = 2;
                    }
                    if (Contact <= Ap)
                    {
                        if (CanSee)
                        {
                            distanceFromTarget = between;
                            targetCenter = npc.Center;
                            Search = 0;
                            foundTarget = true;
                        }
                        else if (!CanSee && FirstDot && SpotDot1)
                        {
                            distanceFromTarget = between;
                            targetCenter = Dot1;
                            Search = 0;
                            foundTarget = true;
                        }
                        else if (!CanSee && !(FirstDot && SpotDot1) && (SpotDotMatch && DotMatchFind))
                        {
                            distanceFromTarget = between;
                            targetCenter = DotMatch;
                            Search = 0;
                            foundTarget = true;
                        }
                        else if (!CanSee && !(FirstDot && SpotDot1) && !(SpotDotMatch && DotMatchFind) && (DotPlayer1 && DotPlayer2 && DotPlayer3 && DotPlayer4) && Search <= 0)
                        {
                            distanceFromTarget = between;
                            targetCenter = DotProjectile;
                            foundTarget = true;
                        }
                        else if (!CanSee && !(FirstDot && SpotDot1) && !(SpotDotMatch && DotMatchFind) && (DotPlayer1 && DotPlayer2 && DotPlayer3 && DotPlayer4) && Search == 1)
                        {
                            distanceFromTarget = between;
                            targetCenter = DotPlayer;
                            foundTarget = true;
                        }
                        else if (!CanSee && !(FirstDot && SpotDot1) && !(SpotDotMatch && DotMatchFind) && (DotPlayer1 && DotPlayer2 && DotPlayer3 && DotPlayer4) && Search == 2)
                        {
                            distanceFromTarget = between;
                            targetCenter = player.Center;
                            foundTarget = true;
                        }
                    }
                    if (Contact >= (Ap + 1))
                    {
                        if ((CanSee && targetCenter.Y > Projectile.Center.Y))
                        {
                            distanceFromTarget = between;
                            targetCenter = npc.Center;
                            foundTarget = true;
                        }
                    }
                }
                if (!foundTarget)
                {
                    // This code is required either way, used for finding a target
                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        NPC npc = Main.npc[i];
                        if (npc.CanBeChasedBy() && npc.active)
                        {
                            Vector2 Dot1 = npc.position;
                            Dot1.Y -= 310;
                            Vector2 DotMatch = npc.position;
                            DotMatch.Y = Projectile.position.Y;
                            Vector2 DotPlayer = player.position;
                            DotPlayer.Y = Projectile.position.Y;
                            Vector2 DotProjectile = Projectile.position;
                            DotProjectile.Y = DotPlayer.Y;
                            float between = Vector2.Distance(npc.Center, player.Center);
                            float between1 = Vector2.Distance(DotProjectile, Projectile.Center);
                            float between2 = Vector2.Distance(DotPlayer, Projectile.Center);
                            bool closest = Vector2.Distance(player.Center, targetCenter) > between;
                            bool CanSee = Collision.CanHitLine(Projectile.position, Projectile.width * 2, Projectile.height * 2, npc.position, 1, 1);
                            bool FirstDot = Collision.CanHitLine(Projectile.position, Projectile.width, Projectile.height, Dot1, 1, 1);
                            bool SpotDot1 = Collision.CanHitLine(Dot1, Projectile.width, Projectile.height, npc.position, 1, 1);
                            bool DotMatchFind = Collision.CanHitLine(Projectile.position, Projectile.width, Projectile.height, DotMatch, 1, 1);
                            bool SpotDotMatch = Collision.CanHitLine(DotMatch, Projectile.width, Projectile.height, npc.position, 1, 1);
                            bool DotPlayer1 = Collision.CanHitLine(Projectile.position, Projectile.width, Projectile.height, DotProjectile, 1, 1);
                            bool DotPlayer2 = Collision.CanHitLine(DotProjectile, 1, 1, DotPlayer, 1, 1);
                            bool DotPlayer3 = Collision.CanHitLine(DotPlayer, 1, 1, player.position, 1, 1);
                            bool DotPlayer4 = Collision.CanHitLine(player.position, 1, 1, npc.position, 1, 1);
                            bool closeThroughWall = between < 1500f;
                            if (between1 < 50)
                            {
                                Search = 1;
                            }
                            if (between2 < 50)
                            {
                                Search = 2;
                            }
                            if (Contact <= Ap)
                            {
                                if (((closest) || !foundTarget) && CanSee && (closeThroughWall))
                                {
                                    distanceFromTarget = between;
                                    targetCenter = npc.Center;
                                    Search = 0;
                                    foundTarget = true;
                                }
                                else if (((closest) || !foundTarget) && !CanSee && FirstDot && SpotDot1 && (closeThroughWall))
                                {
                                    Search = 0;
                                    distanceFromTarget = between;
                                    targetCenter = Dot1;
                                    foundTarget = true;
                                }
                                else if (((closest) || !foundTarget) && !CanSee && !(FirstDot && SpotDot1) && (SpotDotMatch && DotMatchFind) && (closeThroughWall))
                                {
                                    Search = 0;
                                    distanceFromTarget = between;
                                    targetCenter = DotMatch;
                                    foundTarget = true;
                                }
                                else if (((closest) || !foundTarget) && !CanSee && !(FirstDot && SpotDot1) && !(SpotDotMatch && DotMatchFind) && (DotPlayer1 && DotPlayer2 && DotPlayer3 && DotPlayer4) && (closeThroughWall) && Search <= 0)
                                {
                                    distanceFromTarget = between;
                                    targetCenter = DotProjectile;
                                    foundTarget = true;
                                }
                                else if (((closest) || !foundTarget) && !CanSee && !(FirstDot && SpotDot1) && !(SpotDotMatch && DotMatchFind) && (DotPlayer1 && DotPlayer2 && DotPlayer3 && DotPlayer4) && (closeThroughWall) && Search == 1)
                                {
                                    distanceFromTarget = between;
                                    targetCenter = DotPlayer;
                                    foundTarget = true;
                                }
                                else if (((closest) || !foundTarget) && !CanSee && !(FirstDot && SpotDot1) && !(SpotDotMatch && DotMatchFind) && (DotPlayer1 && DotPlayer2 && DotPlayer3 && DotPlayer4) && (closeThroughWall) && Search == 2)
                                {
                                    distanceFromTarget = between;
                                    targetCenter = player.Center;
                                    foundTarget = true;
                                }
                            }
                            if (Contact >= (Ap + 1))
                            {
                                if (((closest) || !foundTarget) && CanSee && (closeThroughWall) && targetCenter.Y > Projectile.Center.Y)
                                {
                                    distanceFromTarget = between;
                                    targetCenter = npc.Center;
                                    foundTarget = true;
                                }
                            }
                        }
                    }
                }
                Vector2 idlePosition = Projectile.Center;
                idlePosition.Y += 500;
                float Woah = 150;
                if ((player.HasBuff(ModContent.BuffType<Overcharged>())))
                {
                    Woah = 50;
                }
                else if ((player.HasBuff(ModContent.BuffType<StatLower>())))
                {
                    Woah = 200;
                }
                else if ((player.HasBuff(ModContent.BuffType<StatRaise>())))
                {
                    Woah = 100;
                }
                else
                {
                    Woah = 150;
                }
                float UngaBunga = Woah * Contact;
                float betweenThis = Vector2.Distance(Projectile.Center, targetCenter);
                if (hitground <= 0 && foundTarget && betweenThis > UngaBunga)
                {
                    // The immediate range around the target (so it doesn't latch onto it when close)
                    Vector2 direction = targetCenter - Projectile.Center;
                    direction.Normalize();
                    direction *= speed;
                    Projectile.velocity = (Projectile.velocity * (inertia - 2) + direction) / inertia;
                    {
                        Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
                    }
                }
                if (hitground <= 0 && !foundTarget)
                {
                    // The immediate range around the target (so it doesn't latch onto it when close)
                    Projectile.tileCollide = true;
                    Vector2 direction = idlePosition - Projectile.Center;
                    direction.Normalize();
                    direction *= speed;
                    Projectile.velocity = (Projectile.velocity * (inertia - 2) + direction) / inertia;
                    {
                        Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
                    }
                }
                if (hitground <= 0)
                {
                    base.Projectile.frameCounter++;
                    if (base.Projectile.frameCounter >= 2)
                    {
                        base.Projectile.frame++;
                        base.Projectile.frameCounter = 0;
                    }
                    if (base.Projectile.frame >= Main.projFrames[base.Projectile.type])
                    {
                        base.Projectile.frame = 0;
                    }
                }
                if (hitground >= 1)
                {
                    Projectile.velocity.Y = 0;
                    Projectile.velocity.X = 0;
                }
                int previousDirection = Projectile.direction;
                Projectile.direction = Projectile.spriteDirection = (Projectile.velocity.X > 0f).ToDirectionInt();
                if (previousDirection != Projectile.direction)
                {
                    Projectile.netUpdate = true;
                    if (Projectile.netSpam > 59)
                        Projectile.netSpam = 59;
                }
            }
        }
        public override void PostDraw(Color lightColor)
        {
            Player player = Main.player[base.Projectile.owner];
            FairyPlayer modPlayer = player.Fairy();
            {
                if ((player.HasBuff(ModContent.BuffType<Overcharged>())) && !Main.player[Main.myPlayer].ZoneSnow)
                {
                    Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Topaz/LightningPurple");
                    Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                    int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
                    int frameY = frameHeight * base.Projectile.frame;
                    Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                    Vector2 origin = rectangle.Size() / 2f;
                    float rotation = base.Projectile.rotation;
                    float scale = base.Projectile.scale;
                    SpriteEffects spriteEffects = SpriteEffects.None;
                    Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(lightColor), rotation, origin, scale, spriteEffects, 0f);
                }
                else if (Main.player[Main.myPlayer].ZoneSnow)
                {
                    Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Topaz/LightningPink");
                    Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                    int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
                    int frameY = frameHeight * base.Projectile.frame;
                    Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                    Vector2 origin = rectangle.Size() / 2f;
                    float rotation = base.Projectile.rotation;
                    float scale = base.Projectile.scale;
                    SpriteEffects spriteEffects = SpriteEffects.None;
                    Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(lightColor), rotation, origin, scale, spriteEffects, 0f);
                }
                else if ((player.HasBuff(ModContent.BuffType<StatRaise>())))
                {
                    Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Topaz/LightningBlue");
                    Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                    int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
                    int frameY = frameHeight * base.Projectile.frame;
                    Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                    Vector2 origin = rectangle.Size() / 2f;
                    float rotation = base.Projectile.rotation;
                    float scale = base.Projectile.scale;
                    SpriteEffects spriteEffects = SpriteEffects.None;
                    Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(lightColor), rotation, origin, scale, spriteEffects, 0f);
                }
                else if (player.HasBuff(ModContent.BuffType<StatLower>()))
                {
                    Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Topaz/LightningRed");
                    Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                    int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
                    int frameY = frameHeight * base.Projectile.frame;
                    Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                    Vector2 origin = rectangle.Size() / 2f;
                    float rotation = base.Projectile.rotation;
                    float scale = base.Projectile.scale;
                    SpriteEffects spriteEffects = SpriteEffects.None;
                    Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(lightColor), rotation, origin, scale, spriteEffects, 0f);
                }
                else
                {
                    Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Topaz/LightningBody3");
                    Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                    int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
                    int frameY = frameHeight * base.Projectile.frame;
                    Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                    Vector2 origin = rectangle.Size() / 2f;
                    float rotation = base.Projectile.rotation;
                    float scale = base.Projectile.scale;
                    SpriteEffects spriteEffects = SpriteEffects.None;
                    Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(lightColor), rotation, origin, scale, spriteEffects, 0f);
                }
            }
        }
    }
}
