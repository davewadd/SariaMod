using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SariaMod.Dusts;
using SariaMod.Items.Strange;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace SariaMod.Items.Sapphire
{
    public class WaterSummon : ModProjectile
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
            base.Projectile.friendly = false;
            base.Projectile.ignoreWater = true;
            base.Projectile.usesLocalNPCImmunity = true;
            base.Projectile.localNPCHitCooldown = 7;
            base.Projectile.minionSlots = 0f;
            base.Projectile.extraUpdates = 1;
            base.Projectile.penetrate = -1;
            base.Projectile.tileCollide = false;
            base.Projectile.timeLeft = 1600;
            base.Projectile.minion = true;
        }
        private int ChannelTimer;
        private int ChannelTimer2;
        private int ChannelTimer3;
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(ChannelTimer);
            writer.Write(ChannelTimer2);
            writer.Write(ChannelTimer3);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            ChannelTimer = (int)reader.ReadInt32();
            ChannelTimer2 = (int)reader.ReadInt32();
            ChannelTimer3 = (int)reader.ReadInt32();
        }
        public override bool? CanCutTiles()
        {
            return false;
        }
        public override bool MinionContactDamage()
        {
            return false;
        }
        private const int sphereRadius = 3;
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            FairyPlayer modPlayer = player.Fairy();
            Projectile.scale = 3.0f;
            Projectile.rotation += .005f;
            Projectile.knockBack = 10;
            Projectile.SariaBaseDamage();
            if (Projectile.IsUnderThunderCloud())
            {
                Projectile.SneezeDust(ModContent.DustType<BubbleDust>(), 10, 530, 50, 0, 0);
            }
            if (Main.rand.NextBool(18))
            {
                float radius = (float)Math.Sqrt(Main.rand.Next(50 * 50));
                double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                {
                    Dust.NewDust(new Vector2(Projectile.Center.X + radius * (float)Math.Cos(angle), Projectile.Center.Y + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<BubbleDust>(), 0f, 0f, 0, default(Color), 1.5f);
                }
            }
            if (Main.rand.NextBool(50))
            {
                float radius = (float)Math.Sqrt(Main.rand.Next(100 * 100));
                double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                {
                    Dust.NewDust(new Vector2(Projectile.Center.X + radius * (float)Math.Cos(angle), Projectile.Center.Y + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<BubbleDust3>(), 0f, 0f, 0, default(Color), 1.5f);
                }
            }
            ChannelTimer++;
            ChannelTimer2++;
            ChannelTimer3++;
            if (ChannelTimer >= 200)
            {
                for (int i = 0; i < 50; i++)
                {
                    Vector2 speed = Main.rand.NextVector2CircularEdge(1f, 1f);
                    Dust d = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<BubbleDust2>(), speed * -7, Scale: 2.7f);
                    Lighting.AddLight(Projectile.position, Color.DarkBlue.ToVector3() * 6f);
                    d.noGravity = true;
                }
                ChannelTimer = 0;
            }
            if (ChannelTimer2 >= 400)
            {
                SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/WaterLoop"), Projectile.Center);
                ChannelTimer2 = 0;
            }
            if ((player.ownedProjectileCounts[ModContent.ProjectileType<Saria>()] > 0f))
            {
                Projectile.timeLeft = 1500;
            }
            if (Main.rand.Next(1500) == 0)
            {
                SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Water3"), Projectile.Center);
            }
            else if (Main.rand.Next(1500) == 1)
            {
                SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Water1"), Projectile.Center);
            }
            if (ChannelTimer3 >= 300)
            {
                if (Projectile.IsUnderThunderCloud() && player.ownedProjectileCounts[ModContent.ProjectileType<WaterBarrier3>()] <= 0f && (Main.myPlayer == Projectile.owner))
                {
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 20, Projectile.position.Y + 13, 0, 0, ModContent.ProjectileType<WaterBarrier3>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                    ChannelTimer3 = 0;
                }
                if (!Projectile.IsUnderThunderCloud() && player.ownedProjectileCounts[ModContent.ProjectileType<WaterBarrier>()] <= 0f && (Main.myPlayer == Projectile.owner))
                {
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 20, Projectile.position.Y + 13, 0, 0, ModContent.ProjectileType<WaterBarrier>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                    ChannelTimer3 = 0;
                }
            }
            Lighting.AddLight(Projectile.position, Color.DarkBlue.ToVector3() * 3f);
            if (Projectile.timeLeft == 1600)
            {
                SoundEngine.PlaySound(SoundID.DD2_EtherianPortalOpen, base.Projectile.Center);
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            {
                {
                    Texture2D texture = TextureAssets.Projectile[ModContent.ProjectileType<BlueCharge>()].Value;
                    Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                    int frameHeight = texture.Height / Main.projFrames[ModContent.ProjectileType<BlueCharge>()];
                    int frameY = frameHeight * Main.projFrames[ModContent.ProjectileType<BlueCharge>()];
                    Color drawColor = Color.Lerp(lightColor, Color.LightBlue, 0f);
                    Lighting.AddLight(Projectile.Center, Color.LightBlue.ToVector3() * 1f);
                    drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                    Rectangle rectangle = texture.Frame(verticalFrames: 8, frameY: (int)Main.GameUpdateCount / 6 % 8);
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
