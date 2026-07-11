using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SariaMod.Buffs;
using SariaMod.Dusts;
using SariaMod.Items.Strange;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.Ruby
{
    /// <summary>
    /// Visual-only charge ring that appears during the charge phase.
    /// Shrinks from large to small at ztarget4 center, spinning clockwise.
    /// Timed to match the ChargeFire1 sound duration.
    /// </summary>
    public class RovaRing : ModProjectile
    {
        private int ChargeTimer;
        private float RingRotation;

        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Saria");
            Main.projFrames[base.Projectile.type] = 1;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(ChargeTimer);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            ChargeTimer = reader.ReadInt32();
        }

        public override void SetDefaults()
        {
            Projectile.width = 86;
            Projectile.height = 86;
            Projectile.netImportant = true;
            Projectile.alpha = 0;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 120;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.minionSlots = 0f;
        }

        public override bool? CanCutTiles()
        {
            return false;
        }

        public override bool? CanHitNPC(NPC target)
        {
            return false;
        }

        public override bool MinionContactDamage()
        {
            return false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            DrawChargeRing();
            return false;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            ChargeTimer++;

            // Follow ztarget4's position
            for (int i = 0; i < 1000; i++)
            {
                if (Main.projectile[i].active && Main.projectile[i].ModProjectile is Ztarget4 && Main.projectile[i].owner == Projectile.owner)
                {
                    Projectile.Center = Main.projectile[i].Center;
                    break;
                }
            }

            // Spin clockwise
            RingRotation += 0.05f;
            Projectile.rotation = RingRotation;

            // Shrink over the charge duration
            float shrinkProgress = ChargeTimer / 90f; // 90 ticks = 1.5s charge
            if (shrinkProgress > 1f) shrinkProgress = 1f;
            Projectile.scale = MathHelper.Lerp(1.5f, 0.3f, shrinkProgress);

            // Spawn fire particles in a ring
            if (Main.rand.NextBool(3))
            {
                float ringRadius = 30f * Projectile.scale;
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                Vector2 offset = new Vector2((float)Math.Cos(angle) * ringRadius, (float)Math.Sin(angle) * ringRadius);
                Vector2 dustPos = Projectile.Center + offset;
                Vector2 dustVel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * -1f;
                Dust d = Dust.NewDustPerfect(dustPos, ModContent.DustType<FlameDust>(), dustVel, 0, default, Main.rand.NextFloat(0.8f, 1.5f));
                d.noGravity = true;
            }

            // Add orange/red light at center
            Lighting.AddLight(Projectile.Center, new Color(255, 120, 30).ToVector3() * 1.5f * Projectile.scale);

            // Kill when charge timer finishes (ChargeFire1 is ~2s, 120 ticks)
            if (ChargeTimer >= 120)
            {
                Projectile.Kill();
            }
        }

        private void DrawChargeRing()
        {
            Texture2D texture = TextureAssets.MagicPixel.Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Rectangle sourceRect = new Rectangle(0, 0, 1, 1);
            float fadeIn = MathHelper.Clamp(ChargeTimer / 18f, 0f, 1f);
            float fadeOut = 1f - MathHelper.Clamp((ChargeTimer - 96f) / 24f, 0f, 1f);
            float alpha = fadeIn * fadeOut;

            Texture2D ringTexture = RovaVisualAssets.Ring;
            if (ringTexture != null)
            {
                float ringScale = 84f * Projectile.scale / Math.Max(ringTexture.Width, ringTexture.Height);
                Main.spriteBatch.Draw(
                    ringTexture,
                    drawPos,
                    null,
                    Color.White * alpha,
                    Projectile.rotation,
                    new Vector2(ringTexture.Width / 2f, ringTexture.Height / 2f),
                    ringScale,
                    SpriteEffects.None,
                    0f);
                return;
            }

            float radius = 42f * Projectile.scale;
            const int segments = 40;

            for (int i = 0; i < segments; i++)
            {
                float angleA = Projectile.rotation + i * MathHelper.TwoPi / segments;
                float angleB = Projectile.rotation + (i + 0.65f) * MathHelper.TwoPi / segments;
                Vector2 pointA = drawPos + angleA.ToRotationVector2() * radius;
                Vector2 pointB = drawPos + angleB.ToRotationVector2() * radius;
                Vector2 segment = pointB - pointA;
                float segmentLength = segment.Length();
                if (segmentLength <= 0f)
                    continue;

                Color outer = new Color(255, 60, 0, 130) * alpha;
                Color inner = new Color(255, 220, 70, 210) * alpha;
                float rotation = segment.ToRotation();
                Main.spriteBatch.Draw(texture, pointA, sourceRect, outer, rotation, new Vector2(0f, 0.5f), new Vector2(segmentLength, 6f * Projectile.scale), SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(texture, pointA, sourceRect, inner, rotation, new Vector2(0f, 0.5f), new Vector2(segmentLength, 2f * Projectile.scale), SpriteEffects.None, 0f);
            }
        }
    }
}
