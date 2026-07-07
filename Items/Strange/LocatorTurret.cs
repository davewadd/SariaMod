using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SariaMod.Dusts;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace SariaMod.Items.Strange
{
    public class LocatorTurret : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Saria");
            Main.projFrames[base.Projectile.type] = 1;
            ProjectileID.Sets.MinionShot[base.Projectile.type] = true;
            ProjectileID.Sets.TrailingMode[base.Projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[base.Projectile.type] = 30;
        }
        public override void SetDefaults()
        {
            base.Projectile.width = 30;
            base.Projectile.height = 30;
            base.Projectile.netImportant = true;
            base.Projectile.friendly = true;
            base.Projectile.ignoreWater = true;
            base.Projectile.usesLocalNPCImmunity = true;
            base.Projectile.localNPCHitCooldown = 7;
            base.Projectile.minionSlots = 0f;
            base.Projectile.extraUpdates = 1;
            base.Projectile.penetrate = -1;
            base.Projectile.tileCollide = false;
            base.Projectile.timeLeft = 201;
            base.Projectile.minion = true;
        }
        public override bool? CanCutTiles()
        {
            return false;
        }
        public override bool MinionContactDamage()
        {
            return false;
        }
        public override bool? CanHitNPC(NPC target)
        {
            return false;
        }
        private const int sphereRadius = 3;
        private int count;
        private int count2;
        private int count3;
        public int Value;
        public override void SendExtraAI(BinaryWriter writer) { writer.Write(count2); writer.Write(count3); writer.Write(count); writer.Write(Value); }
        public override void ReceiveExtraAI(BinaryReader reader) { count2 = (int)reader.ReadInt32(); count3 = (int)reader.ReadInt32(); count = (int)reader.ReadInt32(); Value = (int)reader.ReadInt32(); }
        public override void AI()
        {
            //Main.NewText(count);
            Player player = Main.player[Projectile.owner];
            FairyPlayer modPlayer = player.Fairy();
            Projectile.scale = 1.5f;
            {
                Dust.NewDust(new Vector2(Projectile.Center.X, Projectile.Center.Y), 0, 0, ModContent.DustType<BurningPsychic2>(), 0f, 0f, 0, default(Color), 1.5f);
            }
            {
                Dust.NewDust(new Vector2(Projectile.Center.X, Projectile.Center.Y), 0, 0, ModContent.DustType<BurningPsychic3>(), 0f, 0f, 0, default(Color), 1.5f);
            }
            {
                Dust.NewDust(new Vector2(Projectile.Center.X, Projectile.Center.Y), 0, 0, ModContent.DustType<BurningPsychic>(), 0f, 0f, 0, default(Color), 1.5f);
            }
            Projectile.knockBack = 10;
            Projectile.SariaBaseDamage();
            count2++;
            if (Projectile.timeLeft <= 200 && count2 >= (40 - count3))
            {
                Projectile.timeLeft = 199;
                SoundEngine.PlaySound(SoundID.DD2_SkyDragonsFuryShot, base.Projectile.Center);
                for (int i = 0; i < 50; i++)
                {
                    Vector2 speed = Main.rand.NextVector2CircularEdge(1f, 1f);
                    Dust d = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<AbsorbPsychic>(), speed * -10, Scale: 1.5f);
                    d.noGravity = true;
                }
                if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), base.Projectile.Center, base.Projectile.DirectionTo(Main.MouseWorld).RotatedByRandom(0.0866f) * 12, ModContent.ProjectileType<LocatorRapid>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                count++;
                count2 = 0;
                if (count3 <= 30)
                {
                    count3++;
                }
            }
            int owner = player.whoAmI;
            for (int i = 0; i < 1000; i++)
            {
                if (Main.projectile[i].active && Main.projectile[i].ModProjectile is Ztarget2 modProjectile && i != base.Projectile.whoAmI && ((Main.projectile[i].owner == owner)))
                {
                    {
                        Value = modProjectile.ChannelTimer / 9;
                    }
                }
            }
            if (count >= Value)
            {
                count = 0;
                Projectile.Kill();
            }
            if (Projectile.timeLeft == 500)
            {
                SoundEngine.PlaySound(SoundID.DD2_EtherianPortalOpen, base.Projectile.Center);
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            {
                Texture2D starTexture2 = TextureAssets.Projectile[ModContent.ProjectileType<PinkCharge>()].Value;
                Vector2 drawPosition;
                {
                    Texture2D texture = TextureAssets.Projectile[ModContent.ProjectileType<PinkCharge>()].Value;
                    Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                    int frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<PinkCharge>()];
                    int frameY = frameHeight * Main.projFrames[ModContent.ProjectileType<PinkCharge>()];
                    Color drawColor = Color.Lerp(lightColor, Color.Pink, 20f);
                    Lighting.AddLight(Projectile.Center, Color.HotPink.ToVector3() * 0.78f);
                    drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                    Rectangle rectangle = texture.Frame(verticalFrames: 4, frameY: (int)Main.GameUpdateCount / 6 % 4);
                    Vector2 origin = rectangle.Size() / 2f;
                    float rotation = base.Projectile.rotation;
                    float scale = base.Projectile.scale;
                    SpriteEffects spriteEffects = SpriteEffects.None;
                    Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
                }
                return false;
            }
        }
    }
}
