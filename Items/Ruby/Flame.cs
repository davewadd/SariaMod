using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SariaMod.Buffs;
using SariaMod.Dusts;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace SariaMod.Items.Ruby
{
    public class Flame : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Saria");
            Main.projFrames[base.Projectile.type] = 6;
            ProjectileID.Sets.MinionShot[base.Projectile.type] = true;
        }
        private int Timedown;
        private int Startup;
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(Startup);
            writer.Write(Projectile.timeLeft);
            writer.Write(Timedown);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            Timedown = (int)reader.ReadInt32();
            Projectile.timeLeft = (int)reader.ReadInt32();
            Startup = (int)reader.ReadInt32();
        }
        public override void SetDefaults()
        {
            base.Projectile.width = 20;
            base.Projectile.height = 20;
            base.Projectile.netImportant = true;
            base.Projectile.friendly = true;
            base.Projectile.ignoreWater = true;
            base.Projectile.usesLocalNPCImmunity = true;
            base.Projectile.localNPCHitCooldown = 7;
            base.Projectile.minionSlots = 0f;
            base.Projectile.extraUpdates = 1;
            base.Projectile.aiStyle = 1;
            Projectile.alpha = 40;
            base.Projectile.penetrate = -1;
            base.Projectile.timeLeft = 1500;
        }
        private const int sphereRadius = 20;
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Timedown <= 0)
            {
                SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Ignite"), Projectile.Center);
                Timedown = 280;
            }
            if (Main.rand.NextBool(30))
            {
                {
                    float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius * sphereRadius));
                    double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                    Dust.NewDust(new Vector2(Projectile.Center.X + radius * (float)Math.Cos(angle), (Projectile.Center.Y - 15) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<SmokeDust7>(), 0f, 0f, 0, default(Color), 1.5f);
                }
                {
                    float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius * sphereRadius));
                    double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                    Dust.NewDust(new Vector2(Projectile.Center.X + 40 * (float)Math.Cos(angle), (Projectile.Center.Y - 3) + 10 * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<FlameDust2>(), 0f, 0f, 0, default(Color), 1.5f);
                }
            }
            Projectile.velocity.X = 0;
            return false;
        }
        public override bool? CanCutTiles()
        {
            return false;
        }
        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            Player player = Main.player[Projectile.owner];
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
            target.buffImmune[ModContent.BuffType<Burning2>()] = false;
            target.AddBuff(ModContent.BuffType<Burning2>(), 200);
            modPlayer.SariaXp++;
        }
        public override bool MinionContactDamage()
        {
            return true;
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            Player player2 = Main.LocalPlayer;
            FairyPlayer modPlayer = player.Fairy();
            Projectile.SariaBaseDamage();
            Projectile.damage /= 15;
            Projectile.knockBack *= 0;
            if (Timedown == 0)
            {
                Projectile.tileCollide = true;
            }
            if (Timedown > 0)
            {
                Projectile.velocity.Y = 0f;
                Projectile.velocity.X = 0f;
                if (Timedown >= 0)
                {
                    Projectile.width = 40;
                    Projectile.height = 35;
                }
                if (Main.rand.NextBool(30))
                {
                    {
                        float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius * sphereRadius));
                        double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                        Dust.NewDust(new Vector2(Projectile.Center.X + radius * (float)Math.Cos(angle), (Projectile.Center.Y - 15) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<SmokeDust7>(), 0f, 0f, 0, default(Color), 1.5f);
                    }
                    {
                        float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius * sphereRadius));
                        double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                        Dust.NewDust(new Vector2(Projectile.Center.X + 40 * (float)Math.Cos(angle), (Projectile.Center.Y - 3) + 10 * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<FlameDust2>(), 0f, 0f, 0, default(Color), 1.5f);
                    }
                }
                Projectile.tileCollide = false;
            }
            if (Collision.WetCollision(Projectile.position, Projectile.width, Projectile.height))
            {
                for (int i = 0; i < 5; i++)
                {
                    Vector2 speed = Main.rand.NextVector2CircularEdge(1f, 1f);
                    Dust d = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<SmokeDust6>(), speed * 13, Scale: 3.5f);
                    d.noGravity = true;
                }
                for (int i = 0; i < 5; i++)
                {
                    Vector2 speed = Main.rand.NextVector2CircularEdge(1f, 1f);
                    Dust d = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<BubbleDust2>(), speed * 13, Scale: 3.5f);
                    d.noGravity = true;
                }
                SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/mist"), Projectile.Center);
                Projectile.Kill();
            }
            if (Math.Abs(Projectile.velocity.X) > 0f && Math.Abs(Projectile.velocity.Y) > 0f)
            {
                {
                    float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius * sphereRadius));
                    double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                    Dust.NewDust(new Vector2(Projectile.Center.X + radius * (float)Math.Cos(angle), (Projectile.Center.Y - 15) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<SmokeDust7>(), 0f, 0f, 0, default(Color), 1.5f);
                }
            }
            // friendly needs to be set to true so the minion can deal contact damage
            // friendly needs to be set to false so it doesn't damage things like target dummies while idling
            // Both things depend on if it has a target or not, so it's just one assignment here
            // You don't need this assignment if your minion is shooting things instead of dealing contact damage
            Lighting.AddLight(Projectile.Center, Color.OrangeRed.ToVector3() * 2f);
            // Default movement parameters (here for attacking)
            {
                float between = Vector2.Distance(player2.Center, Projectile.Center);
                // Reasonable distance away so it doesn't target across multiple screens
                if (between < 600f)
                {
                    player2.AddBuff(BuffID.Campfire, 30);
                }
            }
            int frameSpeed = 15;
            {
                base.Projectile.frameCounter++;
                if (Projectile.frameCounter >= frameSpeed)
                    if (base.Projectile.frameCounter > 6)
                    {
                        base.Projectile.frame++;
                        base.Projectile.frameCounter = 0;
                    }
                if (base.Projectile.frame >= 6)
                {
                    base.Projectile.frame = 0;
                }
            }
        }
        public override Color? GetAlpha(Color lightColor)
        {
            if (base.Projectile.timeLeft < 85)
            {
                byte b2 = (byte)(base.Projectile.timeLeft * 3);
                byte a2 = (byte)(100f * ((float)(int)b2 / 255f));
                return new Color(b2, b2, b2, a2);
            }
            return new Color(255, 255, 255, 100);
        }
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            overPlayers.Add(index);
        }
        public override void PostDraw(Color lightColor)
        {
            {
                Texture2D texture = TextureAssets.Projectile[ModContent.ProjectileType<Flame>()].Value;
                Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                int frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<Flame>()];
                int frameY = frameHeight * Main.projFrames[ModContent.ProjectileType<Flame>()];
                Color drawColor = Color.Lerp(lightColor, Color.Pink, 20f);
                Lighting.AddLight(Projectile.Center, Color.HotPink.ToVector3() * 0.78f);
                drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                Rectangle rectangle = texture.Frame(verticalFrames: 6, frameY: (int)Main.GameUpdateCount / 7 % 6);
                Vector2 origin = rectangle.Size() / 2f;
                float rotation = base.Projectile.rotation;
                float scale = base.Projectile.scale * 1.9f;
                SpriteEffects spriteEffects = SpriteEffects.None;
                if (base.Projectile.spriteDirection == 1)
                {
                    startPos.Y -= 18;
                    startPos.X += 13;
                }
                Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
            }
            {
                Texture2D texture = TextureAssets.Projectile[ModContent.ProjectileType<Flame>()].Value;
                Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                int frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<Flame>()];
                int frameY = frameHeight * Main.projFrames[ModContent.ProjectileType<Flame>()];
                Color drawColor = Color.Lerp(lightColor, Color.Pink, 20f);
                Lighting.AddLight(Projectile.Center, Color.HotPink.ToVector3() * 0.78f);
                drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                Rectangle rectangle = texture.Frame(verticalFrames: 6, frameY: (int)Main.GameUpdateCount / 8 % 6);
                Vector2 origin = rectangle.Size() / 2f;
                float rotation = base.Projectile.rotation;
                float scale = base.Projectile.scale;
                SpriteEffects spriteEffects = SpriteEffects.None;
                if (base.Projectile.spriteDirection == 1)
                {
                    startPos.Y -= 16;
                    startPos.X -= 32;
                }
                Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
            }
            {
                Texture2D texture = TextureAssets.Projectile[ModContent.ProjectileType<Flame>()].Value;
                Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                int frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<Flame>()];
                int frameY = frameHeight * Main.projFrames[ModContent.ProjectileType<Flame>()];
                Color drawColor = Color.Lerp(lightColor, Color.Pink, 20f);
                Lighting.AddLight(Projectile.Center, Color.HotPink.ToVector3() * 0.78f);
                drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                Rectangle rectangle = texture.Frame(verticalFrames: 6, frameY: (int)Main.GameUpdateCount / 6 % 6);
                Vector2 origin = rectangle.Size() / 2f;
                float rotation = base.Projectile.rotation;
                float scale = base.Projectile.scale;
                SpriteEffects spriteEffects = SpriteEffects.None;
                if (base.Projectile.spriteDirection == 1)
                {
                    startPos.Y -= 16;
                    startPos.X += 32;
                }
                Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
            }
            {
                Texture2D texture = TextureAssets.Projectile[ModContent.ProjectileType<Flame>()].Value;
                Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                int frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<Flame>()];
                int frameY = frameHeight * Main.projFrames[ModContent.ProjectileType<Flame>()];
                Color drawColor = Color.Lerp(lightColor, Color.Pink, 20f);
                Lighting.AddLight(Projectile.Center, Color.HotPink.ToVector3() * 0.78f);
                drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                Rectangle rectangle = texture.Frame(verticalFrames: 6, frameY: (int)Main.GameUpdateCount / 4 % 6);
                Vector2 origin = rectangle.Size() / 2f;
                float rotation = base.Projectile.rotation;
                float scale = base.Projectile.scale * 1.3f;
                SpriteEffects spriteEffects = SpriteEffects.None;
                if (base.Projectile.spriteDirection == 1)
                {
                    startPos.Y -= 22;
                    startPos.X -= +27;
                }
                Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
            }
            {
                Texture2D texture = TextureAssets.Projectile[ModContent.ProjectileType<Flame>()].Value;
                Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                int frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<Flame>()];
                int frameY = frameHeight * Main.projFrames[ModContent.ProjectileType<Flame>()];
                Color drawColor = Color.Lerp(lightColor, Color.Pink, 20f);
                Lighting.AddLight(Projectile.Center, Color.HotPink.ToVector3() * 0.78f);
                drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                Rectangle rectangle = texture.Frame(verticalFrames: 6, frameY: (int)Main.GameUpdateCount / 5 % 6);
                Vector2 origin = rectangle.Size() / 2f;
                float rotation = base.Projectile.rotation;
                float scale = base.Projectile.scale * 1.8f;
                SpriteEffects spriteEffects = SpriteEffects.None;
                if (base.Projectile.spriteDirection == 1)
                {
                    startPos.Y -= 11;
                    startPos.X -= +26;
                }
                Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
            }
            {
                Texture2D texture = TextureAssets.Projectile[ModContent.ProjectileType<Flame>()].Value;
                Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                int frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<Flame>()];
                int frameY = frameHeight * Main.projFrames[ModContent.ProjectileType<Flame>()];
                Color drawColor = Color.Lerp(lightColor, Color.Pink, 20f);
                Lighting.AddLight(Projectile.Center, Color.HotPink.ToVector3() * 0.78f);
                drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                Rectangle rectangle = texture.Frame(verticalFrames: 6, frameY: (int)Main.GameUpdateCount / 7 % 6);
                Vector2 origin = rectangle.Size() / 2f;
                float rotation = base.Projectile.rotation;
                float scale = base.Projectile.scale;
                SpriteEffects spriteEffects = SpriteEffects.None;
                if (base.Projectile.spriteDirection == 1)
                {
                    startPos.Y -= 10;
                    startPos.X -= +22;
                }
                Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
            }
            {
                Texture2D texture = TextureAssets.Projectile[ModContent.ProjectileType<Flame>()].Value;
                Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                int frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<Flame>()];
                int frameY = frameHeight * Main.projFrames[ModContent.ProjectileType<Flame>()];
                Color drawColor = Color.Lerp(lightColor, Color.Pink, 20f);
                Lighting.AddLight(Projectile.Center, Color.HotPink.ToVector3() * 0.78f);
                drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                Rectangle rectangle = texture.Frame(verticalFrames: 6, frameY: (int)Main.GameUpdateCount / 6 % 6);
                Vector2 origin = rectangle.Size() / 2f;
                float rotation = base.Projectile.rotation;
                float scale = base.Projectile.scale;
                SpriteEffects spriteEffects = SpriteEffects.None;
                if (base.Projectile.spriteDirection == 1)
                {
                    startPos.Y -= 10;
                    startPos.X += +22;
                }
                Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
            }
            {
                Texture2D texture = TextureAssets.Projectile[ModContent.ProjectileType<Flame>()].Value;
                Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                int frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<Flame>()];
                int frameY = frameHeight * Main.projFrames[ModContent.ProjectileType<Flame>()];
                Color drawColor = Color.Lerp(lightColor, Color.Pink, 20f);
                Lighting.AddLight(Projectile.Center, Color.HotPink.ToVector3() * 0.78f);
                drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                Rectangle rectangle = texture.Frame(verticalFrames: 6, frameY: (int)Main.GameUpdateCount / 5 % 6);
                Vector2 origin = rectangle.Size() / 2f;
                float rotation = base.Projectile.rotation;
                float scale = base.Projectile.scale;
                SpriteEffects spriteEffects = SpriteEffects.None;
                if (base.Projectile.spriteDirection == 1)
                {
                    startPos.Y -= 6;
                    startPos.X -= +3;
                }
                Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
            }
            {
                Texture2D texture = TextureAssets.Projectile[ModContent.ProjectileType<Flame>()].Value;
                Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                int frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<Flame>()];
                int frameY = frameHeight * Main.projFrames[ModContent.ProjectileType<Flame>()];
                Color drawColor = Color.Lerp(lightColor, Color.Pink, 20f);
                Lighting.AddLight(Projectile.Center, Color.HotPink.ToVector3() * 0.78f);
                drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                Rectangle rectangle = texture.Frame(verticalFrames: 6, frameY: (int)Main.GameUpdateCount / 6 % 6);
                Vector2 origin = rectangle.Size() / 2f;
                float rotation = base.Projectile.rotation;
                float scale = base.Projectile.scale;
                SpriteEffects spriteEffects = SpriteEffects.None;
                if (base.Projectile.spriteDirection == 1)
                {
                    startPos.Y -= 20;
                    startPos.X -= +5;
                }
                Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
            }
            {
                Texture2D texture = TextureAssets.Projectile[ModContent.ProjectileType<Flame>()].Value;
                Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                int frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<Flame>()];
                int frameY = frameHeight * Main.projFrames[ModContent.ProjectileType<Flame>()];
                Color drawColor = Color.Lerp(lightColor, Color.Pink, 20f);
                Lighting.AddLight(Projectile.Center, Color.HotPink.ToVector3() * 0.78f);
                drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                Rectangle rectangle = texture.Frame(verticalFrames: 6, frameY: (int)Main.GameUpdateCount / 5 % 6);
                Vector2 origin = rectangle.Size() / 2f;
                float rotation = base.Projectile.rotation;
                float scale = base.Projectile.scale;
                SpriteEffects spriteEffects = SpriteEffects.None;
                if (base.Projectile.spriteDirection == 1)
                {
                    startPos.Y -= 14;
                    startPos.X -= +22;
                }
                Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
            }
            {
                Texture2D texture = TextureAssets.Projectile[ModContent.ProjectileType<Flame>()].Value;
                Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                int frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<Flame>()];
                int frameY = frameHeight * Main.projFrames[ModContent.ProjectileType<Flame>()];
                Color drawColor = Color.Lerp(lightColor, Color.Pink, 20f);
                Lighting.AddLight(Projectile.Center, Color.HotPink.ToVector3() * 0.78f);
                drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                Rectangle rectangle = texture.Frame(verticalFrames: 6, frameY: (int)Main.GameUpdateCount / 7 % 6);
                Vector2 origin = rectangle.Size() / 2f;
                float rotation = base.Projectile.rotation;
                float scale = base.Projectile.scale;
                SpriteEffects spriteEffects = SpriteEffects.None;
                if (base.Projectile.spriteDirection == 1)
                {
                    startPos.Y -= 2;
                    startPos.X -= +16;
                }
                Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
            }
            {
                Texture2D texture = TextureAssets.Projectile[ModContent.ProjectileType<Flame>()].Value;
                Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                int frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<Flame>()];
                int frameY = frameHeight * Main.projFrames[ModContent.ProjectileType<Flame>()];
                Color drawColor = Color.Lerp(lightColor, Color.Pink, 20f);
                Lighting.AddLight(Projectile.Center, Color.HotPink.ToVector3() * 0.78f);
                drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                Rectangle rectangle = texture.Frame(verticalFrames: 6, frameY: (int)Main.GameUpdateCount / 7 % 6);
                Vector2 origin = rectangle.Size() / 2f;
                float rotation = base.Projectile.rotation;
                float scale = base.Projectile.scale;
                SpriteEffects spriteEffects = SpriteEffects.None;
                if (base.Projectile.spriteDirection == 1)
                {
                    startPos.Y -= 2;
                    startPos.X += +16;
                }
                Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
            }
            {
                Texture2D texture = TextureAssets.Projectile[ModContent.ProjectileType<Flame>()].Value;
                Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                int frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<Flame>()];
                int frameY = frameHeight * Main.projFrames[ModContent.ProjectileType<Flame>()];
                Color drawColor = Color.Lerp(lightColor, Color.Pink, 20f);
                Lighting.AddLight(Projectile.Center, Color.HotPink.ToVector3() * 0.78f);
                drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                Rectangle rectangle = texture.Frame(verticalFrames: 6, frameY: (int)Main.GameUpdateCount / 5 % 6);
                Vector2 origin = rectangle.Size() / 2f;
                float rotation = base.Projectile.rotation;
                float scale = base.Projectile.scale;
                SpriteEffects spriteEffects = SpriteEffects.None;
                if (base.Projectile.spriteDirection == 1)
                {
                    startPos.Y -= 7;
                    startPos.X += +17;
                }
                Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
            }
            {
                Texture2D texture = TextureAssets.Projectile[ModContent.ProjectileType<Flame>()].Value;
                Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                int frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<Flame>()];
                int frameY = frameHeight * Main.projFrames[ModContent.ProjectileType<Flame>()];
                Color drawColor = Color.Lerp(lightColor, Color.Pink, 20f);
                Lighting.AddLight(Projectile.Center, Color.HotPink.ToVector3() * 0.78f);
                drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                Rectangle rectangle = texture.Frame(verticalFrames: 6, frameY: (int)Main.GameUpdateCount / 6 % 6);
                Vector2 origin = rectangle.Size() / 2f;
                float rotation = base.Projectile.rotation;
                float scale = base.Projectile.scale;
                SpriteEffects spriteEffects = SpriteEffects.None;
                if (base.Projectile.spriteDirection == 1)
                {
                    startPos.Y -= 7;
                    startPos.X -= +17;
                }
                Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
            }
            {
                Texture2D texture = TextureAssets.Projectile[ModContent.ProjectileType<Flame>()].Value;
                Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                int frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<Flame>()];
                int frameY = frameHeight * Main.projFrames[ModContent.ProjectileType<Flame>()];
                Color drawColor = Color.Lerp(lightColor, Color.Pink, 20f);
                Lighting.AddLight(Projectile.Center, Color.HotPink.ToVector3() * 0.78f);
                drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                Rectangle rectangle = texture.Frame(verticalFrames: 6, frameY: (int)Main.GameUpdateCount / 6 % 6);
                Vector2 origin = rectangle.Size() / 2f;
                float rotation = base.Projectile.rotation;
                float scale = base.Projectile.scale;
                SpriteEffects spriteEffects = SpriteEffects.None;
                if (base.Projectile.spriteDirection == 1)
                {
                    startPos.Y -= 14;
                    startPos.X += +17;
                }
                Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
            }
            {
                Texture2D texture = TextureAssets.Projectile[ModContent.ProjectileType<Flame>()].Value;
                Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                int frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<Flame>()];
                int frameY = frameHeight * Main.projFrames[ModContent.ProjectileType<Flame>()];
                Color drawColor = Color.Lerp(lightColor, Color.Pink, 20f);
                Lighting.AddLight(Projectile.Center, Color.HotPink.ToVector3() * 0.78f);
                drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                Rectangle rectangle = texture.Frame(verticalFrames: 6, frameY: (int)Main.GameUpdateCount / 5 % 6);
                Vector2 origin = rectangle.Size() / 2f;
                float rotation = base.Projectile.rotation;
                float scale = base.Projectile.scale;
                SpriteEffects spriteEffects = SpriteEffects.None;
                if (base.Projectile.spriteDirection == 1)
                {
                    startPos.Y -= 24;
                    startPos.X += +19;
                }
                Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
            }
            {
                Texture2D texture = TextureAssets.Projectile[ModContent.ProjectileType<Flame>()].Value;
                Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                int frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<Flame>()];
                int frameY = frameHeight * Main.projFrames[ModContent.ProjectileType<Flame>()];
                Color drawColor = Color.Lerp(lightColor, Color.Pink, 20f);
                Lighting.AddLight(Projectile.Center, Color.HotPink.ToVector3() * 0.78f);
                drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                Rectangle rectangle = texture.Frame(verticalFrames: 6, frameY: (int)Main.GameUpdateCount / 7 % 6);
                Vector2 origin = rectangle.Size() / 2f;
                float rotation = base.Projectile.rotation;
                float scale = base.Projectile.scale;
                SpriteEffects spriteEffects = SpriteEffects.None;
                if (base.Projectile.spriteDirection == 1)
                {
                    startPos.Y -= 12;
                    startPos.X += +23;
                }
                Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
            }
            {
                Texture2D texture = TextureAssets.Projectile[ModContent.ProjectileType<Flame>()].Value;
                Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                int frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<Flame>()];
                int frameY = frameHeight * Main.projFrames[ModContent.ProjectileType<Flame>()];
                Color drawColor = Color.Lerp(lightColor, Color.Pink, 20f);
                Lighting.AddLight(Projectile.Center, Color.HotPink.ToVector3() * 0.78f);
                drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                Rectangle rectangle = texture.Frame(verticalFrames: 6, frameY: (int)Main.GameUpdateCount / 5 % 6);
                Vector2 origin = rectangle.Size() / 2f;
                float rotation = base.Projectile.rotation;
                float scale = base.Projectile.scale * 1.2f;
                SpriteEffects spriteEffects = SpriteEffects.None;
                if (base.Projectile.spriteDirection == 1)
                {
                    startPos.Y -= 12;
                    startPos.X -= +40;
                }
                Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
            }
        }
    }
}
