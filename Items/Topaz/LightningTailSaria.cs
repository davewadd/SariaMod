using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SariaMod.Buffs;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace SariaMod.Items.Topaz
{
    public class LightningTailSaria : ModProjectile
    {
        public new string LocalizationCategory => "Projectiles.Summon";
        private int playerMinionSlots = 0;
        private bool runCheck = true;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            Main.projFrames[base.Projectile.type] = 4;
            ProjectileID.Sets.NeedsUUID[Projectile.type] = true;
        }
        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 0;
            Projectile.netImportant = true;
            Projectile.timeLeft = 18000;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft *= 5;
            Projectile.minion = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
            Projectile.DamageType = DamageClass.Summon;
        }
        internal static bool SameIdentity(Projectile proj, int owner, int identity)
        {
            return proj.owner == owner && (proj.projUUID == identity || proj.identity == identity);
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
            target.velocity.X = 0;
            target.velocity.Y = 0;
        }
        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            Player player = Main.player[Projectile.owner];
            FairyPlayer modPlayer = player.Fairy();
            Projectile.SariaBaseDamage();
            if ((player.HasBuff(ModContent.BuffType<Overcharged>())))
            {
                Projectile.damage *= 1;
            }
            Projectile.velocity.X = 0;
            Projectile.velocity.Y = 0;
            Projectile.localAI[0] = 0f;
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
            int _ = 1;
            LightningBodySaria.SegmentAI(Projectile, 40, ref _);
            Projectile segmentAhead = Main.projectile.Take(Main.maxProjectiles).FirstOrDefault(proj => SameIdentity(proj, Projectile.owner, (int)Projectile.ai[0]));
            Projectile.alpha = segmentAhead.alpha;
            if ((Math.Abs(segmentAhead.velocity.X) > 0f) && (Math.Abs(segmentAhead.velocity.Y) > 0f))
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
        }
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindProjectiles.Add(index);
        }
        public override bool ShouldUpdatePosition() => false;
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
