using Microsoft.Xna.Framework;



using SariaMod.Items;
using Microsoft.Xna.Framework.Graphics;
using System;
using SariaMod.Buffs;
using SariaMod.Dusts;
using Terraria;
using Terraria.ID;
using SariaMod.Items.Strange;
using SariaMod.Items.Sapphire;
using Terraria.ModLoader;

namespace SariaMod.Items
{


    public class Flash : ModProjectile
    {
       

        public const float DistanceToCheck = 1100f;

        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Mother");
            Main.projFrames[base.projectile.type] = 11;
            Main.projPet[projectile.type] = true;
             ProjectileID.Sets.MinionSacrificable[base.projectile.type] = false;
            ProjectileID.Sets.MinionTargettingFeature[base.projectile.type] = true;
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
           
            base.projectile.width = 96;
            base.projectile.height = 78;
            
            base.projectile.netImportant = true;
            base.projectile.friendly = true;
            
            base.projectile.ignoreWater = false;
            base.projectile.usesLocalNPCImmunity = true;
             base.projectile.localNPCHitCooldown = 50;
                base.projectile.minionSlots = 0f;
            base.projectile.timeLeft = 200;
            base.projectile.penetrate = -1;
            base.projectile.tileCollide = false;
            base.projectile.timeLeft *= 5;
            base.projectile.minion = true;
        }
       
        public override void AI()
        {

            Player player = Main.player[base.projectile.owner];
            FairyPlayer modPlayer = player.Fairy();


            if (player.ownedProjectileCounts[ModContent.ProjectileType<FlashCooldown>()] <= 0f && !player.HasBuff(ModContent.BuffType<Overcharged>()))
            {
                Projectile.NewProjectile(base.projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 1f, ModContent.ProjectileType<FlashCooldown>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
            }





                Projectile mother = Main.projectile[(int)base.projectile.ai[0]];
            if (!mother.active)
            {
                base.projectile.Kill();
                return;
            }
            
           if (projectile.frame == 1)
            {
                Projectile.NewProjectile(base.projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 1f, ModContent.ProjectileType<FlashBarrier>(), base.projectile.damage, base.projectile.knockBack, player.whoAmI, base.projectile.whoAmI);
                Main.PlaySound(SoundID.Item74, base.projectile.Center);
                Main.PlaySound(SoundID.DD2_DarkMageHealImpact, base.projectile.Center);
            }
           if (projectile.frame >= 3)
            {
                Lighting.AddLight(projectile.Center, Color.LightPink.ToVector3() * 0.78f);
            }
            projectile.position.X = mother.Center.X;
            projectile.position.Y = mother.Center.Y-70;
            projectile.spriteDirection = mother.spriteDirection;



            int frameSpeed = 5; //reduced by half due to framecounter speedup
            projectile.frameCounter += 2;
            if (projectile.frameCounter >= frameSpeed)
            {
                base.projectile.frameCounter++;
                if (base.projectile.frameCounter > 2)
                {
                    base.projectile.frame++;
                    base.projectile.frameCounter = 0;
                }
                if (base.projectile.frame >= Main.projFrames[base.projectile.type])
                {
                    base.projectile.frame = 11;
                }
            }










            }
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Player player = Main.player[base.projectile.owner];
            FairyPlayer modPlayer = player.Fairy();
            Vector2 drawPosition;
            {
                Texture2D texture = Main.projectileTexture[ModContent.ProjectileType<Flash>()];
                Vector2 startPos = base.projectile.Center - Main.screenPosition + new Vector2(0f, base.projectile.gfxOffY);
                int frameHeight = texture.Height / Main.projFrames[base.projectile.type];
                int frameY = frameHeight * base.projectile.frame;
                Color drawColor = Color.Lerp(lightColor, Color.WhiteSmoke, 20f);
                drawColor = Color.Lerp(drawColor, Color.DarkViolet, 0);
                Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                Vector2 origin = rectangle.Size() / 2f;
                float rotation = base.projectile.rotation;
                float scale = base.projectile.scale;
                SpriteEffects spriteEffects = SpriteEffects.None;
                startPos.Y -= 10;
                startPos.X += -30;
                if (base.projectile.spriteDirection == -1)
                {
                    spriteEffects = SpriteEffects.FlipHorizontally;
                }
                Main.spriteBatch.Draw(texture, startPos, rectangle, base.projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, 0f);
            }

            return false;
        }
    }



}