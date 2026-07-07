using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SariaMod.Buffs;
using SariaMod.Dusts;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace SariaMod.Items.Strange
{
    public class LocatorRapid : ModProjectile
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
            base.Projectile.width = 5;
            base.Projectile.height = 5;
            base.Projectile.netImportant = true;
            base.Projectile.friendly = true;
            base.Projectile.ignoreWater = true;
            base.Projectile.usesLocalNPCImmunity = true;
            base.Projectile.localNPCHitCooldown = 100;
            base.Projectile.minionSlots = 0f;
            base.Projectile.extraUpdates = 1;
            base.Projectile.penetrate = -1;
            base.Projectile.tileCollide = true;
            base.Projectile.timeLeft = 3000;
            base.Projectile.minion = true;
        }
        public override bool? CanCutTiles()
        {
            return false;
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            {
                Projectile.timeLeft = 300;
                Projectile.velocity.Y = 0;
                Projectile.velocity.X = 0;
            }
            return false;
        }
        public override bool MinionContactDamage()
        {
            return true;
        }
        public override bool? CanHitNPC(NPC target)
        {
            if (Projectile.velocity.X <= 2 && Projectile.velocity.Y <= 2)
            {
                return false;
            }
            return target.CanBeChasedBy(base.Projectile);
        }
        private const int sphereRadius = 3;
        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            Player player = Main.player[Projectile.owner];
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
            target.AddBuff(BuffID.Slow, 300);
            target.AddBuff(ModContent.BuffType<SariaCurse2>(), 50);
            for (int j = 0; j < 3; j++) //set to 2
            {
                SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact, base.Projectile.Center);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), base.Projectile.Center + Utils.RandomVector2(Main.rand, 0f, 0f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ProjectileType<LocatorShard>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                Projectile.Kill();
            }
                SoundEngine.PlaySound(SoundID.DD2_WitherBeastDeath, base.Projectile.Center);
            FairyPlayer modPlayer = player.Fairy();
            modPlayer.SariaXp++;
            knockback /= 4;
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            FairyPlayer modPlayer = player.Fairy();
            Projectile mother = Main.projectile[(int)base.Projectile.ai[1]];
            Projectile.knockBack = 10;
            float thesuace = ((Math.Abs(Projectile.velocity.X) + Math.Abs(Projectile.velocity.Y)) * .5f);
            if (Projectile.timeLeft == 3000)
            {
                Projectile.direction = Projectile.spriteDirection = (Projectile.velocity.X > 0f) ? 1 : -1;
                Projectile.rotation = Projectile.velocity.ToRotation();
                // Since our sprite has an orientation, we need to adjust rotation to compensate for the draw flipping
            }
            Lighting.AddLight(Projectile.Center, Color.HotPink.ToVector3() * 0.58f);
            if (Projectile.velocity.X == 0 && Projectile.velocity.Y == 0)
            {
                thesuace = 0;
            }
            for (int i = 0; i < thesuace; i++)
            {
                Vector2 speed2 = Main.rand.NextVector2CircularEdge(1f, 1f);
                Dust d = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<AbsorbPsychic>(), speed2 * -5, Scale: 1.5f);
                d.noGravity = true;
            }
            if (Projectile.timeLeft <= 3)
            {
                for (int j = 0; j < 3; j++) //set to 2
                {
                    if (Main.rand.Next(4) == 1)
                    {
                        SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact, base.Projectile.Center);
                        for (int i = 0; i < 50; i++)
                        {
                            Vector2 speed2 = Main.rand.NextVector2CircularEdge(1f, 1f);
                            Dust d = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<AbsorbPsychic>(), Utils.RandomVector2(Main.rand, -5f, 5f) * speed2, Scale: 1.5f);
                            d.noGravity = true;
                        }
                        Projectile.Kill();
                    }
                }
            }
            Projectile.SariaBaseDamage();
            Projectile.damage /= 4;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            {
                Texture2D starTexture2 = TextureAssets.Projectile[ModContent.ProjectileType<LocatorRapid>()].Value;
                Texture2D starTexture = TextureAssets.Projectile[ModContent.ProjectileType<LocatorBeam>()].Value;
                Vector2 drawPosition;
                for (int i = 1; i < base.Projectile.oldPos.Length; i++)
                {
                    float completionRatio = (float)i / (float)base.Projectile.oldPos.Length;
                    Color drawColor = Color.Lerp(lightColor, Color.LightPink, 2f);
                    drawColor = Color.Lerp(drawColor, Color.DarkViolet, completionRatio);
                    drawColor = Color.Lerp(drawColor, Color.Transparent, completionRatio);
                    drawPosition = base.Projectile.oldPos[i] + base.Projectile.Size * 0.5f - Main.screenPosition;
                    Main.spriteBatch.Draw(starTexture, drawPosition, null, base.Projectile.GetAlpha(drawColor), 0, Utils.Size(starTexture) * 0.5f, base.Projectile.scale, SpriteEffects.None, 0f);
                }
                for (int j = 0; j < 4; j++)
                {
                    drawPosition = base.Projectile.Center - Main.screenPosition;
                    Main.spriteBatch.Draw(starTexture2, drawPosition, null, Color.White, base.Projectile.oldRot[j], Utils.Size(starTexture2) * 0.5f, base.Projectile.scale * 1.2f, SpriteEffects.None, 0f);
                }
                return false;
            }
        }
    }
}
