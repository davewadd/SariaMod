using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SariaMod.Buffs;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using SariaMod.Dusts;
namespace SariaMod.Items.Topaz
{
    public class LightningHead1 : ModProjectile
    {
        private static Vector2 WorldTopLeft(int tileDist = 15) => new Vector2(tileDist * 16f);
        private static Vector2 WorldBottomRight(int tileDist = 15) => new Vector2(Main.maxTilesX - tileDist, Main.maxTilesY - tileDist) * 16f;
        public int CanCollide;
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            CanCollide = (int)reader.ReadInt32();
        }
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(CanCollide);
        }
        public static Projectile LocateMother(Projectile projectile)
        {
            int Owner = ModContent.ProjectileType<LightningCloud>();
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                if (Main.projectile[i].type != Owner || !Main.projectile[i].active || Main.projectile[i].owner != projectile.owner)
                    continue;
                return Main.projectile[i];
            }
            return null;
        }
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            Main.projFrames[base.Projectile.type] = 4;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
            ProjectileID.Sets.NeedsUUID[Projectile.type] = true;
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
        }
        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.netImportant = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 500;
            Projectile.alpha = 0;
            Projectile.scale *= 2f;
            Projectile.tileCollide = false;
            Projectile.minion = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
            Projectile.DamageType = DamageClass.Summon;
        }
        public override void AI()
        {
            Player player = Main.player[base.Projectile.owner];
            FairyPlayer modPlayer = player.Fairy();
            Projectile owner = LocateMother(Projectile);
            if (owner != null)
            {
                float between2 = Vector2.Distance(owner.Center, Projectile.Center);
                if (between2 >= 50)
                {
                    Projectile.Center = owner.Center;
                }
            }
            else if (owner == null)
            {
                Projectile.Kill();
                return;
            }
            Projectile.SariaBaseDamage();
            if ((player.HasBuff(ModContent.BuffType<Overcharged>())))
            {
                Projectile.damage /= 20;
            }
            else
            {
                Projectile.damage /= 40;
            }
            if ((player.HasBuff(ModContent.BuffType<Overcharged>())) && !Main.player[Main.myPlayer].ZoneSnow)
            {
                Lighting.AddLight(Projectile.Center, Color.Purple.ToVector3());
            }
            else if (Main.player[Main.myPlayer].ZoneSnow)
            {
                Lighting.AddLight(Projectile.Center, Color.Pink.ToVector3());
            }
            else if ((player.HasBuff(ModContent.BuffType<StatRaise>())))
            {
                Lighting.AddLight(Projectile.Center, Color.Blue.ToVector3());
            }
            else if (player.HasBuff(ModContent.BuffType<StatLower>()))
            {
                Lighting.AddLight(Projectile.Center, Color.Red.ToVector3());
            }
            else
            {
                Lighting.AddLight(Projectile.Center, Color.Yellow.ToVector3());
            }
            Projectile.Center = Vector2.Clamp(Projectile.Center, WorldTopLeft(10), WorldBottomRight(10));
            Projectile.velocity.X = 0.005f;
            Projectile.velocity.Y = 0.005f;
            Projectile.alpha = 300;
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
            {
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
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
        public override void PostDraw(Color lightColor)
        {
            Player player = Main.player[base.Projectile.owner];
            FairyPlayer modPlayer = player.Fairy();
            {
                if ((player.HasBuff(ModContent.BuffType<Overcharged>())) && !Main.player[Main.myPlayer].ZoneSnow)
                {
                    Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Topaz/LightningPurple1");
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
                    Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Topaz/LightningPink1");
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
                    Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Topaz/LightningBlue1");
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
                    Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Topaz/LightningRed1");
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
                    Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Topaz/LightningBody");
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
