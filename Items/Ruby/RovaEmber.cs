using Microsoft.Xna.Framework;
using SariaMod.Buffs;
using SariaMod.Dusts;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.Ruby
{
    /// <summary>
    /// RovaEmber: small flame projectile that drops from heated tiles.
    /// Spawned by TileHeatManager when tiles are above 50% heat intensity and have air above them.
    /// Applies Burning2 and OnFire on contact.
    /// </summary>
    public class RovaEmber : ModProjectile
    {
        private int Timedown;

        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Saria");
            Main.projFrames[base.Projectile.type] = 6;
            ProjectileID.Sets.MinionShot[base.Projectile.type] = true;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(Timedown);
            writer.Write(Projectile.timeLeft);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            Timedown = reader.ReadInt32();
            Projectile.timeLeft = reader.ReadInt32();
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.netImportant = true;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 7;
            Projectile.minionSlots = 0f;
            Projectile.extraUpdates = 1;
            Projectile.aiStyle = 1;
            Projectile.alpha = 40;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 90; // 1.5 seconds
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // Stop horizontal movement on tile collision
            Projectile.velocity.X = 0;
            return false;
        }

        public override bool? CanCutTiles()
        {
            return false;
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
            target.buffImmune[ModContent.BuffType<Burning2>()] = false;

            target.AddBuff(ModContent.BuffType<Burning2>(), 200);
            target.AddBuff(BuffID.OnFire, 200);

            if (modPlayer != null) modPlayer.SariaXp++;

            knockback /= 2;
        }

        public override void AI()
        {
            // Fire dust visual
            if (Main.rand.NextBool(3))
            {
                float radius = 6f;
                double angle = Main.rand.NextDouble() * 2.0 * Math.PI;
                Dust.NewDust(new Vector2(
                    Projectile.Center.X + radius * (float)Math.Cos(angle),
                    Projectile.Center.Y + radius * (float)Math.Sin(angle)
                ), 0, 0, ModContent.DustType<FlameDust>(), 0f, 0f, 0, default(Color), 1.2f);
            }

            // Fire light
            Lighting.AddLight(Projectile.Center, new Color(255, 100, 20).ToVector3() * 1.0f);
        }

        public override void Kill(int timeLeft)
        {
            // Small fire burst on death
            for (int i = 0; i < 8; i++)
            {
                Vector2 speed = Main.rand.NextVector2CircularEdge(1f, 1f);
                Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<FlameDust>(), speed * 3, Scale: 1.5f);
            }

            SoundEngine.PlaySound(SoundID.Item10, Projectile.Center);
        }
    }
}
