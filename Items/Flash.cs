using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SariaMod.Buffs;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace SariaMod.Items
{
    public class Flash : ModProjectile
    {
        public const float DistanceToCheck = 1100f;
        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Mother");
            Main.projFrames[base.Projectile.type] = 11;
            Main.projPet[Projectile.type] = true;
            ProjectileID.Sets.MinionSacrificable[base.Projectile.type] = false;
            ProjectileID.Sets.MinionTargettingFeature[base.Projectile.type] = true;
        }
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
            base.Projectile.width = 96;
            base.Projectile.height = 78;
            base.Projectile.netImportant = true;
            base.Projectile.friendly = true;
            Projectile.alpha = 300;
            base.Projectile.ignoreWater = false;
            base.Projectile.usesLocalNPCImmunity = true;
            base.Projectile.localNPCHitCooldown = 50;
            base.Projectile.minionSlots = 0f;
            base.Projectile.timeLeft = 200;
            base.Projectile.penetrate = -1;
            base.Projectile.tileCollide = false;
            base.Projectile.timeLeft *= 5;
            base.Projectile.minion = true;
        }
        public override void AI()
        {
            Player player = Main.player[base.Projectile.owner];
            FairyPlayer modPlayer = player.Fairy();
            if (player.ownedProjectileCounts[ModContent.ProjectileType<FlashCooldown>()] <= 0f && !player.HasBuff(ModContent.BuffType<Overcharged>()))
            {
                if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + 0, 0, 0, ModContent.ProjectileType<FlashCooldown>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
            }
            if (Projectile.frame == 1)
            {
                if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), player.Center.X + 0, player.Center.Y + 0, 0, 0, ModContent.ProjectileType<FlashBarrier>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                SoundEngine.PlaySound(SoundID.Item74, base.Projectile.Center);
                SoundEngine.PlaySound(SoundID.DD2_DarkMageHealImpact, base.Projectile.Center);
            }
            int frameSpeed = 5; //reduced by half due to framecounter speedup
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
                    base.Projectile.frame = 11;
                }
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Player player = Main.player[base.Projectile.owner];
            FairyPlayer modPlayer = player.Fairy();
            Vector2 drawPosition;
            {
                Texture2D texture = TextureAssets.Projectile[ModContent.ProjectileType<Flash>()].Value;
                Vector2 startPos = base.Projectile.Center - Main.screenPosition + new Vector2(0f, base.Projectile.gfxOffY);
                int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
                int frameY = frameHeight * base.Projectile.frame;
                Color drawColor = Color.Lerp(lightColor, Color.WhiteSmoke, 20f);
                drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                Vector2 origin = rectangle.Size() / 2f;
                float rotation = base.Projectile.rotation;
                float scale = base.Projectile.scale;
                SpriteEffects spriteEffects = SpriteEffects.None;
                startPos.Y -= 10;
                startPos.X += -30;
                if (base.Projectile.spriteDirection == -1)
                {
                    spriteEffects = SpriteEffects.FlipHorizontally;
                }
                Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
            }
            return false;
        }
    }
}