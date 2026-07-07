using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using SariaMod.Dusts;
using Terraria.Audio;
using System.IO;
using SariaMod.Buffs;
using SariaMod.Netcode;
namespace SariaMod.Items.Sapphire
{
    public class HealBubble : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Saria");
            Main.projFrames[base.Projectile.type] = 2;
            ProjectileID.Sets.TrailingMode[base.Projectile.type] = 0;
            ProjectileID.Sets.TrailCacheLength[base.Projectile.type] = 30;
        }
        public int owner = 255;
        public override bool? CanCutTiles()
        {
            return false;
        }
        public override bool MinionContactDamage()
        {
            {
                return false;
            }
        }
        public override void SetDefaults()
        {
            Projectile.netUpdate = true;
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.netImportant = true;
            Projectile.friendly = true;
           Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
            Projectile.minionSlots = 0f;
           Projectile.extraUpdates = 1;
            Projectile.penetrate = 6;
            Projectile.tileCollide = false;
           Projectile.timeLeft = 10000;
            Projectile.minion = true;
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
            modPlayer.SariaXp++;
            int backGoreType = Mod.Find<ModGore>("IceGore3").Type;
            if (!target.boss)
            {
                for (int G = 0; G < 3; G++)
                {
                    Gore B = Gore.NewGorePerfect(Projectile.GetSource_FromThis(), target.position, new Vector2(Main.rand.Next(-6, 7), Main.rand.Next(-6, 7)), backGoreType, 2f);
                    B.light = .5f;
                }
                if (Main.myPlayer == Projectile.owner && !target.HasBuff(ModContent.BuffType<EnemyFrozen>()))
                {
                    SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/HardIce"), target.Center);
                    target.AddBuff(ModContent.BuffType<EnemyFrozen>(), 600);
                }
            }
            damage = 10;
            knockback /= 4;
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            Player player2 = Main.LocalPlayer;
            FairyPlayer modPlayer = player.Fairy();
            Projectile.rotation += Projectile.velocity.X * 0.15f;
            if (Projectile.timeLeft >= 400)
            {
                Projectile.rotation += 0.095f;
            }
            Projectile.netUpdate = true;
            Vector2 mouse = Main.MouseWorld;
            float speed = 40f;
            mouse.X += -20f;
            mouse.Y -= 5f;
            Vector2 idlePosition = player.Center;
            idlePosition.Y -= 48f; // Go up 48 coordinates (three tiles from the center of the player)
            // If your minion doesn't aimlessly move around when it's idle, you need to "put" it into the line of other summoned minions
            // The index is projectile.minionPos
            float minionPositionOffsetX = (10 + Projectile.minionPos * 40) * -player.direction;
            idlePosition.X += minionPositionOffsetX; // Go behind the player
            float between = Vector2.Distance(mouse, Projectile.Center);
            if (Projectile.timeLeft <= 9900 && Main.myPlayer == Projectile.owner)
            {
                Vector2 direction2 = mouse - Projectile.Center;
                direction2.Normalize();
                direction2 *= speed;
                Projectile.velocity = (Projectile.velocity * (19 - 2) + direction2) / 22;
                if (between <= 20)
                {
                    Projectile.Kill();
                }
            }
            int frameSpeed = 20; //reduced by half due to framecounter speedup
            Projectile.frameCounter += 2;
            if (Projectile.frameCounter >= frameSpeed)
            {
                base.Projectile.frameCounter++;
                if (base.Projectile.frameCounter > 2)
                {
                    base.Projectile.frame++;
                    base.Projectile.frameCounter = 0;
                }
                if (base.Projectile.frame >= Main.projFrames[base.Projectile.type])
                {
                    base.Projectile.frame = 0;
                }
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            {
                Vector2 drawPosition;
                for (int i = 1; i < 25; i++)
                {
                    Texture2D texture = TextureAssets.Projectile[ModContent.ProjectileType<HealBubble>()].Value;
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
                return false;
            }
        }
        public override void Kill(int timeLeft)
        {
            Player player = Main.player[Projectile.owner];
            FairyPlayer modPlayer = player.Fairy();
            Projectile.AttackCircleDust(ModContent.DustType<HealingDust>(), 30, 18, .5f, .5f, 1f);
            SoundEngine.PlaySound(SoundID.Item86, Projectile.Center);
            if (modPlayer.StoredHealth <= 240)
            {
                modPlayer.StoredHealth += 10;
            }
            else
            {
                modPlayer.StoredHealth += ((modPlayer.StoredHealth - 250)* -1);
            }
        }
    }
}
