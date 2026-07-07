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
    public class WillOWisp2 : ModProjectile
    {
        public bool alphaCounter;
        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Saria");
            ProjectileID.Sets.TrailCacheLength[base.Projectile.type] = 7;
            ProjectileID.Sets.TrailingMode[base.Projectile.type] = 0;
            Main.projFrames[base.Projectile.type] = 4;
        }
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(alphaCounter);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            alphaCounter = (bool)reader.ReadBoolean();
        }
        public override void SetDefaults()
        {
            base.Projectile.width = 15;
            base.Projectile.height = 15;
            base.Projectile.alpha = 120;
            base.Projectile.friendly = true;
            base.Projectile.tileCollide = false;
            base.Projectile.netImportant = true;
            base.Projectile.penetrate = -1;
            base.Projectile.timeLeft = 3500;
            base.Projectile.ignoreWater = true;
            Projectile.scale *= 1.6f;
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
            Player player = Main.player[base.Projectile.owner];
            Player player2 = Main.LocalPlayer;
            float between = Vector2.Distance(player2.Center, Projectile.Center);
            if (alphaCounter)
            {
                Projectile.alpha -= 1;
            }
            if (Projectile.alpha <= 100)
            {
                alphaCounter = false;
            }
            if (!alphaCounter)
            {
                Projectile.alpha += 1;
            }
            if (Projectile.alpha >= 250)
            {
                alphaCounter = true;
            }
            float number = .0002f;
            Lighting.AddLight(Projectile.Center, Color.Purple.ToVector3() * (number * (Projectile.alpha)));
            if (between < 500f)
            {
                player2.resistCold = true;
                player2.AddBuff(BuffID.Warmth, 20);
            }
            int owner = player.whoAmI;
            int Spot = -40;
            Vector2 idlePosition = player.Center;
            Vector2 direction = idlePosition - Projectile.Center;
            float speed = 30f;
            float inertia = 10f;
            idlePosition.Y += 0f;
            Vector2 vectorToIdlePosition = idlePosition - Projectile.Center;
            float distanceToIdlePosition = vectorToIdlePosition.Length();
            if (distanceToIdlePosition > 55)
            {
                Projectile.Center = player.Center;
            }
            if (distanceToIdlePosition > 50)
            {
                speed = 80f;
                inertia = 40f;
            }
            else
            {
                speed = 4f;
                inertia = 80f;
            }
            if (distanceToIdlePosition > 30f)
            {
                vectorToIdlePosition.Normalize();
                vectorToIdlePosition *= speed;
                Projectile.velocity = (Projectile.velocity * (inertia - 8) + vectorToIdlePosition) / inertia;
            }
            if (Projectile.velocity == Vector2.Zero)
            {
                Projectile.velocity.X = -0.25f;
                Projectile.velocity.Y = -0.25f;
            }
            Projectile.position.X += player.velocity.X;
            Projectile.position.Y += player.velocity.Y;
            float overlapVelocity = 0.08f;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                // Fix overlap with other minions
                Projectile other = Main.projectile[i];
                if (i != Projectile.whoAmI && other.active && other.owner == Projectile.owner && Math.Abs(Projectile.position.X - other.position.X) + Math.Abs(Projectile.position.Y - other.position.Y) < Projectile.width / 4)
                {
                    if (Projectile.position.X < other.position.X) Projectile.velocity.X -= overlapVelocity;
                    else Projectile.velocity.X += overlapVelocity;
                    if (Projectile.position.Y < other.position.Y) Projectile.velocity.Y -= overlapVelocity;
                    else Projectile.velocity.Y += overlapVelocity;
                }
            }
            if (player.HasBuff(ModContent.BuffType<WillOWispBuff>()))
            {
                Projectile.timeLeft = 18;
            }
            if (!player.HasBuff(ModContent.BuffType<WillOWispBuff>()))
            {
                Projectile.Kill();
            }
            if (player.ownedProjectileCounts[ModContent.ProjectileType<WillOWisp>()] <= 0)
            {
                Projectile.Kill();
            }
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
            target.AddBuff(ModContent.BuffType<Burning2>(), 6400);
            target.AddBuff(BuffID.Confused, 300);
            int myPlayer = Main.myPlayer;
            modPlayer.SariaXp++;
            knockback = 10;
        }
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            overPlayers.Add(index);
            overWiresUI.Add(index);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            {
                Texture2D starTexture2 = TextureAssets.Projectile[ModContent.ProjectileType<WillOWisp>()].Value;
                Vector2 drawPosition;
                {
                    Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Ruby/WillOWispTexture");
                    Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                    int frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<WillOWisp>()];
                    Color drawColor = Color.Lerp(lightColor, Color.MediumPurple, 20f);
                    Lighting.AddLight(Projectile.Center, Color.DarkViolet.ToVector3() * 0.78f);
                    drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                    Rectangle rectangle = texture.Frame(verticalFrames: 4, frameY: (int)Main.GameUpdateCount / 6 % 4);
                    Vector2 origin = rectangle.Size() / 2f;
                    float rotation = 0;
                    float scale = base.Projectile.scale * .5f;
                    SpriteEffects spriteEffects = SpriteEffects.None;
                    Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                }
                return false;
            }
        }
        public override void Kill(int timeLeft)
        {
            for (int b = 0; b < 10; b++)
            {
                float radius = (float)Math.Sqrt(Main.rand.Next(34 * 34));
                double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                Dust.NewDust(new Vector2(Projectile.Center.X + radius * (float)Math.Cos(angle), Projectile.Center.Y + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<ShadowFlameDust>(), 0f, 0f, 0, default(Color), 1.5f);
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
