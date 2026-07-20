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
using Terraria.DataStructures;
using SariaMod.Netcode;

namespace SariaMod.Items.Sapphire
{
    public class ColdWaveCenter : ModProjectile
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
            base.Projectile.localNPCHitCooldown = 7;
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
                return false;
        }
        private const int sphereRadius2 = 6;
        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            Player player = Main.player[Projectile.owner];
            Player player2 = Main.LocalPlayer;
            target.buffImmune[BuffID.CursedInferno] = false;
            target.buffImmune[BuffID.Confused] = false;
            target.buffImmune[BuffID.Slow] = false;
            target.buffImmune[BuffID.ShadowFlame] = false;
            target.buffImmune[BuffID.Ichor] = false;
            target.buffImmune[ModContent.BuffType<Burning2>()] = false;
            target.buffImmune[BuffID.Frostburn] = false;
            target.buffImmune[BuffID.Poisoned] = false;
            target.buffImmune[BuffID.Venom] = false;
            target.buffImmune[BuffID.Electrified] = false;
            target.buffImmune[ModContent.BuffType<Frostburn2>()] = false;
            target.AddBuff(ModContent.BuffType<Frostburn2>(), 300);
            
            FairyPlayer modPlayer = player.Fairy();
            modPlayer.SariaXp++;
            knockback /= 100;
        }
        public override void OnSpawn(IEntitySource source)
        {
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            FairyPlayer modPlayer = player.Fairy();
            Projectile.damage = 1;
            if (Projectile.timeLeft == 500)
            {
                SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/IceBurn"), Projectile.Center);
            }
            if (Projectile.localAI[0] == 0f && Main.myPlayer == Projectile.owner)
            {
                int owner = player.whoAmI;
                for (int i = 0; i < 1000; i++)
                {
                    if (Main.projectile[i].active && Main.projectile[i].ModProjectile is Saria modProjectile && modProjectile.Transform == 1 && i != base.Projectile.whoAmI && ((Main.projectile[i].owner == owner)))
                    {
                        modProjectile.CantAttackTimer = 300;
                    }
                }
                Projectile.Fairy().spawnedPlayerMinionProjectileDamageValue = Projectile.damage;
                for (int j = 0; j < 1; j++) //set to 2
                {
                    if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y + 0, 0, 0, ModContent.ProjectileType<ColdWaveHitBox>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, Projectile.whoAmI);
                }
                Projectile.localAI[0] = 1f;
            }
            if (Main.rand.NextBool(7))
            {
                int backGoreType = ModContent.GoreType<IceGore3>();
                for (int G = 0; G < 1; G++)
                {
                    Gore B = Gore.NewGorePerfect(Projectile.GetSource_FromThis(), Projectile.Center, new Vector2(Main.rand.Next(-10, 10), Main.rand.Next(-16, -5)), backGoreType, 2f);
                    B.light = .5f;
                }
            }
            if (Main.rand.NextBool(20))
            {
                float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius2 * sphereRadius2));
                double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                Dust.NewDust(new Vector2((Projectile.Center.X) + radius * (float)Math.Cos(angle), (Projectile.Center.Y) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<Cold>(), 0f, 0f, 0, default(Color), 1.5f);
            }
            if (Main.rand.NextBool(10))
            {
                float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius2 * sphereRadius2));
                double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                Dust.NewDust(new Vector2((Projectile.Center.X) + radius * (float)Math.Cos(angle), (Projectile.Center.Y) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<Snow2>(), 0f, 0f, 0, default(Color), 1.5f);
            }
            Projectile.friendly = true;
            Lighting.AddLight(Projectile.Center, new Color(0, 0, Main.DiscoB).ToVector3() * 2f);
        }
    }
}
