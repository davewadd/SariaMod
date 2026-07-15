using Microsoft.Xna.Framework;
using SariaMod.Buffs;
using SariaMod.Dusts;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using SariaMod.Items.Strange;
using SariaMod.Gores;
using SariaMod.TileGlow;
using SariaMod.Netcode;

namespace SariaMod.Items.Sapphire
{
    public class ColdWaveHitBox : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Saria");
            Main.projFrames[base.Projectile.type] = 1;
            ProjectileID.Sets.MinionShot[base.Projectile.type] = true;
            ProjectileID.Sets.TrailingMode[base.Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            base.Projectile.width = 34;
            base.Projectile.height = 34;
            base.Projectile.netImportant = true;
            base.Projectile.friendly = false;
            base.Projectile.ignoreWater = true;
            base.Projectile.usesLocalNPCImmunity = true;
            base.Projectile.localNPCHitCooldown = 40;
            base.Projectile.minionSlots = 0f;
            base.Projectile.extraUpdates = 1;
            Projectile.alpha = 300;
            base.Projectile.penetrate = -1;
            base.Projectile.tileCollide = false;
            base.Projectile.timeLeft = 500;
            Projectile.DamageType = DamageClass.Summon;
        }
        public override bool? CanCutTiles()
        {
            return false;
        }
        public override bool MinionContactDamage()
        {
            Player player = Main.player[base.Projectile.owner];
            FairyPlayer modPlayer = player.Fairy();
            NPC target = base.Projectile.Center.MinionHoming(500f, player);
            if (target != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private float sphereRadius3 = 2.5f;
        private int sphereRadius2 = 1;
        private float sphereRadius = .05f;
        
        // Tile glow tracking
        private int lastGlowTick = 0;
        private const int GlowUpdateInterval = 10; // Update glow every 10 ticks
        
        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            Player player = Main.player[Projectile.owner];
            Player player2 = Main.LocalPlayer;
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
            target.buffImmune[ModContent.BuffType<Frostburn2>()] = false;
            target.AddBuff(ModContent.BuffType<Frostburn2>(), 200);
            if (Projectile.timeLeft >= 200 && !target.boss)
            {
                int backGoreType = ModContent.GoreType<IceGore3>();
                for (int G = 0; G < 3; G++)
                {
                    Gore B = Gore.NewGorePerfect(Projectile.GetSource_FromThis(), target.position, new Vector2(Main.rand.Next(-10, 10), Main.rand.Next(-16, -5)), backGoreType, 2f);
                    B.light = .5f;
                }
            }
            FairyPlayer modPlayer = player.Fairy();
            modPlayer.SariaXp++;
            if (Main.rand.NextBool())
            {
                float radius = (float)Math.Sqrt(Main.rand.Next(34 * 34));
                double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                Dust.NewDust(new Vector2(Projectile.Center.X + radius * (float)Math.Cos(angle), Projectile.Center.Y + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<Water>(), 0f, 0f, 0, default(Color), 1.5f);
            }//end of dust stuff
            if (Main.myPlayer == Projectile.owner)
            {
                HealBubbleSpawnHelper.SpawnOrOverflow(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f),
                    Vector2.One.RotatedByRandom(6.2831854820251465) * 4f,
                    Projectile.damage,
                    Projectile.knockBack,
                    player.whoAmI);
            }
            knockback /= 100;
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            FairyPlayer modPlayer = player.Fairy();
            int owner = player.whoAmI;
            for (int i = 0; i < 1000; i++)
            {
                if (Main.projectile[i].active && Main.projectile[i].ModProjectile is ColdWaveCenter modProjectile && i != base.Projectile.whoAmI && ((Main.projectile[i].owner == owner)))
                {
                    Projectile.Center = Main.projectile[i].Center;
                }
            }
            Projectile.height += 1;
            Projectile.width += 1;
            sphereRadius2 = Projectile.height;
            sphereRadius3 += .06f;
            Projectile.SariaBaseDamage();
            Projectile.damage = 1;
            
            // Apply tile glow effect
            ApplyTileGlow();
            
            if (Main.rand.NextBool(12))
            {
                float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius2 * sphereRadius2));
                double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                Dust.NewDust(new Vector2((Projectile.Center.X) + radius * (float)Math.Cos(angle), (Projectile.Center.Y) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<Cold>(), 0f, 0f, 0, default(Color), 1f);
            }
            if (Main.rand.NextBool(15))
            {
                float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius2 * sphereRadius2));
                double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                Dust.NewDust(new Vector2((Projectile.Center.X) + radius * (float)Math.Cos(angle), (Projectile.Center.Y) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<SnowRingFog>(), 0f, 0f, 0, default(Color), 2.5f);
            }
            sphereRadius += .003f;
                for (int i = 0; i < 1; i++)
            {
                Vector2 speed = Main.rand.NextVector2CircularEdge(sphereRadius, sphereRadius);
                Dust d = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<SnowRingFog>(), speed * 15, Scale: 4f);
                d.noGravity = true;
            }
            for (int i = 0; i < 3; i++)
            {
                Vector2 speed = Main.rand.NextVector2CircularEdge(sphereRadius, sphereRadius);
                Dust d = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<SnowRing>(), speed * -18, Scale: 5.2f);
                d.noGravity = true;
            }
            if (Projectile.timeLeft == 300)
            {
                SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Hail"), Projectile.Center);
            }
            Projectile.friendly = true;
            Lighting.AddLight(Projectile.Center, new Color(0, 0, Main.DiscoB).ToVector3() * 2f);
        }
        
        /// <summary>
        /// Apply tile glow effect to tiles within the projectile's radius
        /// </summary>
        private void ApplyTileGlow()
        {
            int currentTick = (int)Main.GameUpdateCount;
            
            // Only update glow periodically to reduce performance impact
            if (currentTick - lastGlowTick < GlowUpdateInterval)
                return;
            
            lastGlowTick = currentTick;
            
            // Calculate the current radius (half of width/height since projectile grows)
            float currentRadius = Math.Max(Projectile.width, Projectile.height) / 2f;
            
            // Apply glow locally for visual effect (all clients do this)
            TileGlowManager.ApplyGlowInRadius(Projectile.Center, currentRadius, TileGlowManager.DefaultGlowDuration);
            
            // Tile glow packet send commented out — cosmetic only, was causing per-owner spam every 5 frames
            // if (Main.netMode != NetmodeID.SinglePlayer && Projectile.owner == Main.myPlayer)
            // {
            //     TileGlowNetworking.SendRadiusGlowPacket(Projectile.Center, currentRadius, TileGlowManager.DefaultGlowDuration);
            // }
        }
    }
}
