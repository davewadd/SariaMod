using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SariaMod.Buffs;
using SariaMod.Dusts;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
namespace SariaMod.Items.Topaz
{
    public class LightningCloud : ModProjectile
    {
        private int Find;
        public float Rotation;
        public float Rotation2;
        public int Shoottime;
        private float Soundtimer;
        private bool soundcheck;
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(Find);
            writer.Write(Rotation);
            writer.Write(Shoottime);
            writer.Write(Rotation2);
            writer.Write(Soundtimer);
            writer.Write(soundcheck);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            Find = (int)reader.ReadInt32();
            Rotation = (int)reader.ReadInt32();
            Shoottime = (int)reader.ReadInt32();
            Rotation2 = (int)reader.ReadInt32();
            Soundtimer = (int)reader.ReadInt32();
            soundcheck = (bool)reader.ReadBoolean();
        }
        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Saria");
            ProjectileID.Sets.TrailCacheLength[base.Projectile.type] = 7;
            ProjectileID.Sets.TrailingMode[base.Projectile.type] = 0;
            Main.projFrames[base.Projectile.type] = 1;
        }
        public override void SetDefaults()
        {
            base.Projectile.width = 30;
            base.Projectile.height = 30;
            base.Projectile.alpha = 300;
            base.Projectile.friendly = true;
            base.Projectile.tileCollide = false;
            base.Projectile.penetrate = 1;
            base.Projectile.timeLeft = 5000;
            base.Projectile.ignoreWater = true;
            base.Projectile.usesLocalNPCImmunity = true;
            base.Projectile.localNPCHitCooldown = 4;
        }
        public override bool? CanCutTiles()
        {
            return false;
        }
        public override bool? CanHitNPC(NPC target)
        {
            return false;
        }
        public override void AI()
        {
            //Main.NewText(Count);
            Player player = Main.player[base.Projectile.owner];
            Player player2 = Main.LocalPlayer;
            Projectile mother = Main.projectile[(int)base.Projectile.ai[1]];
            ///Main.NewText(Find);
            if (Soundtimer > 0)
            {
                Soundtimer--;
            }
            if (Main.rand.NextBool(800))
            {
                SoundEngine.PlaySound(SoundID.Thunder, base.Projectile.Center);
            }
            if (Soundtimer <= 0 && !(Main.player[Main.myPlayer].ZoneSnow) && !(Main.player[Main.myPlayer].ZoneRain))
            {
                SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Rain") ,Projectile.Center);
                Soundtimer = 110;
            }
            if (player.HasBuff(ModContent.BuffType<ThunderCloudBuff>()))
            {
                Projectile.timeLeft = 4;
            }
            if (!player.HasBuff(ModContent.BuffType<ThunderCloudBuff>()) && Main.myPlayer == Projectile.owner)
            {
                Projectile.Kill();
            }
            if (!soundcheck)
            {
                SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Switch1"), Projectile.Center);
                for (int i = 0; i < 50; i++)
                {
                    Vector2 speed = Main.rand.NextVector2CircularEdge(1f, 1f);
                    Dust d = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Clouds>(), speed * -11, Scale: 3.5f);
                    d.noGravity = true;
                }
                soundcheck = true;
            }
            SoundEngine.PlaySound(SoundID.BlizzardStrongLoop, base.Projectile.Center);
            FairyProjectile.HomeInOnNPC(base.Projectile, ignoreTiles: true, 600f, 200f, 0f);
            {
                float distanceFromTarget = 10f;
                Vector2 targetCenter = Projectile.position;
                bool foundTarget = false;
                // This code is required if your minion weapon has the targeting feature
                if (player.HasMinionAttackTargetNPC && Find > 0)
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
                    if (CanSee)
                    {
                        distanceFromTarget = between;
                        targetCenter = npc.Center;
                        foundTarget = true;
                    }
                    else if (!CanSee && FirstDot && SpotDot1)
                    {
                        distanceFromTarget = between;
                        targetCenter = Dot1;
                        foundTarget = true;
                    }
                    else if (!CanSee && !(FirstDot && SpotDot1) && (SpotDotMatch && DotMatchFind))
                    {
                        distanceFromTarget = between;
                        targetCenter = DotMatch;
                        foundTarget = true;
                    }
                    else if (!CanSee && !(FirstDot && SpotDot1) && !(SpotDotMatch && DotMatchFind) && (DotPlayer1 && DotPlayer2 && DotPlayer3 && DotPlayer4))
                    {
                        distanceFromTarget = between;
                        targetCenter = DotProjectile;
                        foundTarget = true;
                    }
                }
                if (!foundTarget && Find > 0)
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
                            bool CanSee = Collision.CanHitLine(Projectile.position, Projectile.width, Projectile.height, npc.position, 1, 1);
                            bool FirstDot = Collision.CanHitLine(Projectile.position, Projectile.width, Projectile.height, Dot1, 1, 1);
                            bool SpotDot1 = Collision.CanHitLine(Dot1, Projectile.width, Projectile.height, npc.position, 1, 1);
                            bool DotMatchFind = Collision.CanHitLine(Projectile.position, Projectile.width, Projectile.height, DotMatch, 1, 1);
                            bool SpotDotMatch = Collision.CanHitLine(DotMatch, Projectile.width, Projectile.height, npc.position, 1, 1);
                            bool DotPlayer1 = Collision.CanHitLine(Projectile.position, Projectile.width, Projectile.height, DotProjectile, 1, 1);
                            bool DotPlayer2 = Collision.CanHitLine(DotProjectile, 1, 1, DotPlayer, 1, 1);
                            bool DotPlayer3 = Collision.CanHitLine(DotPlayer, 1, 1, player.position, 1, 1);
                            bool DotPlayer4 = Collision.CanHitLine(player.position, 1, 1, npc.position, 1, 1);
                            bool closeThroughWall = between < 1500f;
                            if (((closest) || !foundTarget) && CanSee && (closeThroughWall))
                            {
                                distanceFromTarget = between;
                                targetCenter = npc.Center;
                                foundTarget = true;
                            }
                            else if (((closest) || !foundTarget) && !CanSee && FirstDot && SpotDot1 && (closeThroughWall))
                            {
                                distanceFromTarget = between;
                                targetCenter = Dot1;
                                foundTarget = true;
                            }
                            else if (((closest) || !foundTarget) && !CanSee && !(FirstDot && SpotDot1) && (SpotDotMatch && DotMatchFind) && (closeThroughWall))
                            {
                                distanceFromTarget = between;
                                targetCenter = DotMatch;
                                foundTarget = true;
                            }
                            else if (((closest) || !foundTarget) && !CanSee && !(FirstDot && SpotDot1) && !(SpotDotMatch && DotMatchFind) && (DotPlayer1 && DotPlayer2 && DotPlayer3 && DotPlayer4) && (closeThroughWall))
                            {
                                distanceFromTarget = between;
                                targetCenter = DotProjectile;
                                foundTarget = true;
                            }
                        }
                    }
                }
                float speed = 5f;
                float inertia = 3f;
                Vector2 idlePosition = Projectile.Center;
                idlePosition.Y += 490;
                Vector2 idlePosition2 = Projectile.Center;
                idlePosition2.Y -= 500;
                Vector2 direction = idlePosition2 - Projectile.Center;
                direction.Normalize();
                direction *= speed;
                bool CanSeeCol = Collision.CanHitLine(Projectile.position, Projectile.width, Projectile.height, idlePosition, Projectile.width, Projectile.height);
                if (!CanSeeCol && Find <= 0)
                {
                        Projectile.velocity = (Projectile.velocity * (inertia - 2) + direction) / inertia;
                }
                else if (CanSeeCol)
                {
                    Projectile.velocity = (Projectile.velocity * (inertia - 2) + direction) *0;
                    Find = 1;
                }
                if (Main.rand.NextBool(30) && player.ownedProjectileCounts[ModContent.ProjectileType<LightningHead1>()] > 0f)
                {
                    SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/StaticSlow"), Projectile.Center);
                    Lighting.AddLight(Projectile.Center, Color.LightYellow.ToVector3() * 4f);
                }
                int SeeCloud = 125;
                if (Projectile.alpha > SeeCloud)
                {
                    Projectile.alpha -= 3;
                }
                if (Projectile.alpha < SeeCloud)
                {
                    Projectile.alpha = SeeCloud;
                }
                float radius3 = (float)Math.Sqrt(Main.rand.Next(20 * 300));
                float radius2 = (float)Math.Sqrt(Main.rand.Next(1 * 300));
                float radius = (float)Math.Sqrt(Main.rand.Next(30 * 300));
                double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                if (Main.player[Main.myPlayer].active && !Main.player[Main.myPlayer].ZoneSnow)
                {
                    if (Main.rand.NextBool(4))
                    {
                        Dust.NewDust(new Vector2(Projectile.Center.X + radius * (float)Math.Cos(angle), (Projectile.Center.Y + 20) + radius2 * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<Rain2>(), 0f, 0f, 0, default(Color), 1.5f);
                    }
                    if (Main.rand.NextBool(2))
                    {
                        Dust.NewDust(new Vector2(Projectile.Center.X + radius * (float)Math.Cos(angle), (Projectile.Center.Y + 20) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<Rain3>(), 0f, 0f, 0, default(Color), 1.5f);
                    }
                }
                if (Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneSnow)
                {
                    if (Main.rand.NextBool(4))
                    {
                        Dust.NewDust(new Vector2(Projectile.Center.X + radius3 * (float)Math.Cos(angle), (Projectile.Center.Y + 10) + radius3 * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<Snow2>(), 0f, 0f, 0, default(Color), 1.5f);
                    }
                    if (Main.rand.NextBool(2))
                    {
                        Dust.NewDust(new Vector2(Projectile.Center.X + radius3 * (float)Math.Cos(angle), (Projectile.Center.Y + 10) + radius3 * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<Snow>(), 0f, 0f, 0, default(Color), 1.5f);
                    }
                }
                Rotation += .065f;
                Rotation2 -= .065f;
                if (Shoottime < 250)
                {
                    Shoottime++;
                }
                float between3 = Vector2.Distance(Projectile.Center, player2.Center);
                if (Shoottime >= 250 && foundTarget)
                {
                    int head2 = -1;
                    for (int i = 0; i < Main.maxProjectiles; i++)
                    {
                        if (Main.projectile[i].active && Main.projectile[i].owner == Main.myPlayer)
                        {
                            if (head2 == -1 && Main.projectile[i].type == ModContent.ProjectileType<LightningHead2>())
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
                            if (player.ownedProjectileCounts[ModContent.ProjectileType<LightningHead2>()] <= 0f && (player.statMana >= player.statManaMax2 / 4) && player.ownedProjectileCounts[ModContent.ProjectileType<LightningBody2>()] <= 0f && player.ownedProjectileCounts[ModContent.ProjectileType<LightningBody2>()] <= 0f)
                            {
                                if (between3 < 2000)
                                {
                                    SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Lightning"));
                                }
                                player.statMana -= player.statManaMax2 / 4;
                                player.manaRegenDelay = 110;
                                tailIndex2 = -1;
                                if (Main.myPlayer != player.whoAmI)
                                    return;
                                int curr = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<LightningHead2>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                                if (Main.projectile.IndexInRange(curr))
                                    for (int i = 0; i < 25; i++)
                                        curr = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.One, ModContent.ProjectileType<LightningBody2>(), (int)(Projectile.damage), Projectile.owner, player.whoAmI, Main.projectile[curr].identity, 0f);
                                if (Main.projectile.IndexInRange(curr))
                                    Main.projectile[curr].originalDamage = Projectile.damage;
                                int prev = curr;
                                curr = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<LightningTail2>(), (int)(Projectile.damage), Projectile.owner, player.whoAmI, Main.projectile[curr].identity, 0f);
                                if (Main.projectile.IndexInRange(curr))
                                    Main.projectile[curr].originalDamage = Projectile.damage;
                                Main.projectile[prev].localAI[1] = curr;
                                tailIndex2 = curr;
                            }
                        }
                    }
                }
                if (foundTarget)
                {
                    Lighting.AddLight(Projectile.Center, Color.DimGray.ToVector3() * 4f);
                    int head = -1;
                    for (int i = 0; i < Main.maxProjectiles; i++)
                    {
                        if (Main.projectile[i].active && Main.projectile[i].owner == Main.myPlayer)
                        {
                            if (head == -1 && Main.projectile[i].type == ModContent.ProjectileType<LightningHead1>())
                            {
                                head = i;
                            }
                            if (head != -1)
                            {
                                break;
                            }
                        }
                    }
                    if (head == -1)
                    {
                        if (player.ownedProjectileCounts[ModContent.ProjectileType<LightningHead1>()] <= 0f && player.ownedProjectileCounts[ModContent.ProjectileType<LightningBody1>()] <= 0f && player.ownedProjectileCounts[ModContent.ProjectileType<LightningBody1>()] <= 0f)
                        {
                            int tailIndex;
                            {
                                tailIndex = -1;
                                if (Main.myPlayer != player.whoAmI)
                                    return;
                                int curr = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<LightningHead1>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                                if (Main.projectile.IndexInRange(curr))
                                    curr = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<LightningBody1>(), (int)(Projectile.damage), Projectile.owner, player.whoAmI, Main.projectile[curr].identity, 0f);
                                if (Main.projectile.IndexInRange(curr))
                                    Main.projectile[curr].originalDamage = Projectile.damage;
                                int prev = curr;
                                for (int i = 0; i < 2; i++)
                                    curr = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<LightningBody1>(), (int)(Projectile.damage), Projectile.owner, player.whoAmI, Main.projectile[curr].identity, 0f);
                                if (Main.projectile.IndexInRange(curr))
                                    Main.projectile[curr].originalDamage = Projectile.damage;
                                Main.projectile[prev].localAI[1] = curr;
                                tailIndex = curr;
                            }
                        }
                    }
                }
            }
        }
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            overPlayers.Add(index);
            overWiresUI.Add(index);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            {
                Vector2 drawPosition;
                {
                    Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Topaz/Cloud");
                    Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                    int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
                    int frameY = frameHeight * base.Projectile.frame;
                    Rectangle rectangle = texture.Frame(verticalFrames: 1, frameY: (int)Main.GameUpdateCount / 6 % 1);
                    Vector2 origin = rectangle.Size() / 2f;
                    float rotation = Rotation;
                    float scale = base.Projectile.scale * 1.10f;
                    SpriteEffects spriteEffects = SpriteEffects.None;
                    startPos.Y -= 10;
                    startPos.X -= 30;
                    Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(lightColor), rotation += .075f, origin, scale, spriteEffects, 0f);
                    Projectile.netUpdate = true;
                }
                {
                    Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Topaz/Cloud");
                    Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                    int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
                    int frameY = frameHeight * base.Projectile.frame;
                    Rectangle rectangle = texture.Frame(verticalFrames: 1, frameY: (int)Main.GameUpdateCount / 6 % 1);
                    Vector2 origin = rectangle.Size() / 2f;
                    float rotation = Rotation;
                    float scale = base.Projectile.scale * 1.10f;
                    SpriteEffects spriteEffects = SpriteEffects.None;
                    startPos.Y -= 10;
                    startPos.X -= 65;
                    Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(lightColor), rotation += .075f, origin, scale, spriteEffects, 0f);
                    Projectile.netUpdate = true;
                }
                {
                    Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Topaz/Cloud");
                    Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                    int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
                    int frameY = frameHeight * base.Projectile.frame;
                    Rectangle rectangle = texture.Frame(verticalFrames: 1, frameY: (int)Main.GameUpdateCount / 6 % 1);
                    Vector2 origin = rectangle.Size() / 2f;
                    float rotation = Rotation;
                    float scale = base.Projectile.scale * .90f;
                    SpriteEffects spriteEffects = SpriteEffects.None;
                    startPos.Y -= 10;
                    startPos.X -= 120;
                    Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(lightColor), rotation += .075f, origin, scale, spriteEffects, 0f);
                    Projectile.netUpdate = true;
                }
                {
                    Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Topaz/Cloud");
                    Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                    int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
                    int frameY = frameHeight * base.Projectile.frame;
                    Rectangle rectangle = texture.Frame(verticalFrames: 1, frameY: (int)Main.GameUpdateCount / 6 % 1);
                    Vector2 origin = rectangle.Size() / 2f;
                    float rotation = Rotation;
                    float scale = base.Projectile.scale * .90f;
                    SpriteEffects spriteEffects = SpriteEffects.None;
                    startPos.Y -= 30;
                    startPos.X -= 10;
                    Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(lightColor), rotation += .075f, origin, scale, spriteEffects, 0f);
                    Projectile.netUpdate = true;
                }
                {
                    Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Topaz/Cloud");
                    Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                    int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
                    int frameY = frameHeight * base.Projectile.frame;
                    Rectangle rectangle = texture.Frame(verticalFrames: 1, frameY: (int)Main.GameUpdateCount / 6 % 1);
                    Vector2 origin = rectangle.Size() / 2f;
                    float rotation = Rotation2;
                    float scale = base.Projectile.scale * .90f;
                    SpriteEffects spriteEffects = SpriteEffects.None;
                    startPos.Y -= 30;
                    startPos.X -= -10;
                    Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(lightColor), rotation, origin, scale, spriteEffects, 0f);
                    Projectile.netUpdate = true;
                }
                {
                    Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Topaz/Cloud");
                    Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                    int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
                    int frameY = frameHeight * base.Projectile.frame;
                    Rectangle rectangle = texture.Frame(verticalFrames: 1, frameY: (int)Main.GameUpdateCount / 6 % 1);
                    Vector2 origin = rectangle.Size() / 2f;
                    float rotation = Rotation2;
                    float scale = base.Projectile.scale * 1.10f;
                    SpriteEffects spriteEffects = SpriteEffects.None;
                    startPos.Y -= 10;
                    startPos.X -= -30;
                    Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(lightColor), rotation, origin, scale, spriteEffects, 0f);
                    Projectile.netUpdate = true;
                }
                {
                    Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Topaz/Cloud");
                    Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                    int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
                    int frameY = frameHeight * base.Projectile.frame;
                    Rectangle rectangle = texture.Frame(verticalFrames: 1, frameY: (int)Main.GameUpdateCount / 6 % 1);
                    Vector2 origin = rectangle.Size() / 2f;
                    float rotation = Rotation2;
                    float scale = base.Projectile.scale * 1.10f;
                    SpriteEffects spriteEffects = SpriteEffects.None;
                    startPos.Y -= 10;
                    startPos.X -= -65;
                    Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(lightColor), rotation, origin, scale, spriteEffects, 0f);
                    Projectile.netUpdate = true;
                }
                {
                    Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Topaz/Cloud");
                    Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                    int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
                    int frameY = frameHeight * base.Projectile.frame;
                    Rectangle rectangle = texture.Frame(verticalFrames: 1, frameY: (int)Main.GameUpdateCount / 6 % 1);
                    Vector2 origin = rectangle.Size() / 2f;
                    float rotation = Rotation2;
                    float scale = base.Projectile.scale * .90f;
                    SpriteEffects spriteEffects = SpriteEffects.None;
                    startPos.Y -= 10;
                    startPos.X -= -120;
                    Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(lightColor), rotation += .075f, origin, scale, spriteEffects, 0f);
                    Projectile.netUpdate = true;
                }
                return false;
            }
        }
    }
}
