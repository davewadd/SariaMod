using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SariaMod.Buffs;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using SariaMod.Items.Topaz;
using SariaMod.Dusts;
using SariaMod;
namespace SariaMod.Items.Topaz
{
    public class LightningBody1 : ModProjectile
    {
        public new string LocalizationCategory => "Projectiles.Summon";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            Main.projFrames[base.Projectile.type] = 4;
            ProjectileID.Sets.NeedsUUID[Projectile.type] = true;
        }
        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 0;
            Projectile.scale *= 2f;
            Projectile.timeLeft = 500;
            Projectile.penetrate = -1;
            Projectile.minion = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
            Projectile.DamageType = DamageClass.Summon;
        }
        public int Freakout;
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            Freakout = (int)reader.ReadInt32();
        }
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(Freakout);
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.velocity.Y = 0;
            Projectile.velocity.X = 0;
            return false;
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
        }
        internal static void SegmentAI(Projectile projectile, int offsetFromNextSegment, ref int playerMinionSlots)
        {
            Player owner = Main.player[projectile.owner];
            Player player = Main.player[projectile.owner];
            FairyPlayer modPlayer = owner.Fairy();
            if ((player.HasBuff(ModContent.BuffType<Overcharged>())) && !Main.player[Main.myPlayer].ZoneSnow)
            {
                projectile.AttackDust(ModContent.DustType<StaticDustNormalPurple>(), 20, 34);
                Lighting.AddLight(projectile.Center, Color.Purple.ToVector3());
            }
            else if (Main.player[Main.myPlayer].ZoneSnow)
            {
                projectile.AttackDust(ModContent.DustType<StaticDustNormalPink>(), 20, 34);
                Lighting.AddLight(projectile.Center, Color.Pink.ToVector3());
            }
            else if ((player.HasBuff(ModContent.BuffType<StatRaise>())))
            {
                projectile.AttackDust(ModContent.DustType<StaticDustNormalBlue>(), 20, 34);
                Lighting.AddLight(projectile.Center, Color.Blue.ToVector3());
            }
            else if (player.HasBuff(ModContent.BuffType<StatLower>()))
            {
                projectile.AttackDust(ModContent.DustType<StaticDustNormalRed>(), 20, 34);
                Lighting.AddLight(projectile.Center, Color.Red.ToVector3());
            }
            else
            {
                projectile.AttackDust(ModContent.DustType<StaticDustNormal>(), 20, 34);
                Lighting.AddLight(projectile.Center, Color.Yellow.ToVector3());
            }
            int headProjType = ModContent.ProjectileType<LightningHead1>();
            int bodyProjType = ModContent.ProjectileType<LightningBody1>();
            ref float segmentAheadIdentity = ref projectile.ai[0];
            Projectile segmentAhead = Main.projectile.Take(Main.maxProjectiles).FirstOrDefault(proj => SameIdentity(proj, projectile.owner, (int)projectile.ai[0]));
            if (segmentAhead is null || !Main.projectile.IndexInRange(segmentAhead.whoAmI) || (segmentAhead.type != bodyProjType && segmentAhead.type != headProjType))
            {
                projectile.Kill();
                return;
            }
            if (playerMinionSlots != -1 && (owner.maxMinions < playerMinionSlots || !owner.active))
            {
                int lostSlots = playerMinionSlots - owner.maxMinions;
                while (lostSlots > 0)
                {
                    Projectile ahead = segmentAhead;
                    for (int i = 0; i < 2; ++i)
                    {
                        if (ahead.type != ModContent.ProjectileType<LightningHead1>())
                            projectile.localAI[1] = ahead.localAI[1];
                        segmentAheadIdentity = ahead.ai[0];
                        projectile.netUpdate = true;
                        ahead.Kill();
                        segmentAhead = Main.projectile.Take(Main.maxProjectiles).FirstOrDefault(proj => SameIdentity(proj, projectile.owner, (int)projectile.ai[0]));
                        if (segmentAhead is null || !Main.projectile.IndexInRange(segmentAhead.whoAmI))
                        {
                            projectile.Kill();
                            return;
                        }
                        ahead = segmentAhead;
                    }
                    lostSlots--;
                }
                playerMinionSlots = owner.maxMinions;
            }
            segmentAhead.localAI[0] = projectile.localAI[0] + 1f;
            Projectile head = LocateHead(projectile);
            {
                if (head is null)
                {
                    projectile.Kill();
                    return;
                }
                else if (head != null && projectile.timeLeft < 100)
                {
                    projectile.timeLeft = 100;
                }
            }
            if (head.netUpdate)
            {
                projectile.netUpdate = true;
                if (projectile.netSpam > 59)
                    projectile.netSpam = 59;
            }
            projectile.extraUpdates = head.extraUpdates;
            projectile.velocity = Vector2.Zero;
            Vector2 offsetToDestination = segmentAhead.Center - projectile.Center;
            if (segmentAhead.rotation != projectile.rotation)
            {
                float offsetAngle = MathHelper.WrapAngle(segmentAhead.rotation - projectile.rotation);
                offsetToDestination = offsetToDestination.RotatedBy(offsetAngle * 3f);
            }
            projectile.rotation = offsetToDestination.ToRotation() + MathHelper.PiOver2;
            if (offsetToDestination != Vector2.Zero)
                projectile.Center = segmentAhead.Center - offsetToDestination.SafeNormalize(Vector2.Zero) * offsetFromNextSegment;
            projectile.Center = Vector2.Clamp(projectile.Center, new Vector2(160f), new Vector2(Main.maxTilesX - 10, Main.maxTilesY - 10) * 16);
        }
        public static Projectile LocateHead(Projectile projectile)
        {
            int headType = ModContent.ProjectileType<LightningHead1>();
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                if (Main.projectile[i].type != headType || !Main.projectile[i].active || Main.projectile[i].owner != projectile.owner)
                    continue;
                return Main.projectile[i];
            }
            return null;
        }
        public override void AI()
        {
            Player player = Main.player[base.Projectile.owner];
            FairyPlayer modPlayer = player.Fairy();
            Projectile.SariaBaseDamage();
            if ((player.HasBuff(ModContent.BuffType<Overcharged>())))
            {
                Projectile.damage /= 20;
            }
            else
            {
                Projectile.damage /= 40;
            }
            int _ = 1;
            SegmentAI(Projectile, 58, ref _);
            Projectile.tileCollide = true;
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
