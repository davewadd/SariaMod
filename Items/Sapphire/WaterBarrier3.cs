using Microsoft.Xna.Framework;
using SariaMod.Buffs;
using SariaMod.Dusts;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
namespace SariaMod.Items.Sapphire
{
    public class WaterBarrier3 : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Saria");
            ProjectileID.Sets.TrailCacheLength[base.Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[base.Projectile.type] = 0;
            Main.projFrames[base.Projectile.type] = 1;
        }
        public override bool? CanCutTiles()
        {
            return false;
        }
        public override void SetDefaults()
        {
            base.Projectile.width = 1000;
            base.Projectile.height = 1000;
            base.Projectile.alpha = 300;
            base.Projectile.friendly = true;
            base.Projectile.tileCollide = false;
            base.Projectile.penetrate = -1;
            base.Projectile.timeLeft = 4;
            base.Projectile.ignoreWater = true;
            Projectile.timeLeft = 24;
            base.Projectile.usesLocalNPCImmunity = true;
            base.Projectile.localNPCHitCooldown = 160;
        }
        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            Player player = Main.player[base.Projectile.owner];
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
            modPlayer.SariaXp++;
            knockback = 1f;
            int myPlayer = Main.myPlayer;
            if (Main.player[myPlayer].position.X + (float)(Main.player[myPlayer].width / 2) < Projectile.position.X + (float)(Projectile.width / 2))
            {
                hitDirection = 1;
            }
            else
            {
                hitDirection = -1;
            }
        }
        public override void AI()
        {
            Player player = Main.player[base.Projectile.owner];
            Player player2 = Main.LocalPlayer;
            FairyPlayer modPlayer = player.Fairy();
            Lighting.AddLight(base.Projectile.Center, 0f, 0.5f, 0f);
            int Yesh = ((player2.statManaMax2) / 6);
            int Yesh2 = ((player2.statManaMax2) / 4);
            int HealAmount = (player.statLifeMax2 / (20 - modPlayer.Sarialevel));
            if (player.HasBuff(ModContent.BuffType<Overcharged>()))
            {
                HealAmount = (player.statLifeMax2 / (18 - modPlayer.Sarialevel));
            }
            if (Projectile.timeLeft == 24)
            {
                for (int i = 0; i < 70; i++)
                {
                    Vector2 speed = Main.rand.NextVector2CircularEdge(1f, 1f);
                    Dust d = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<BubbleDust2>(), speed * -44, Scale: 5.1f);
                    d.noGravity = true;
                }
                SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Water1"), Projectile.Center);
            }
            Projectile.SariaBaseDamage();
            {
                if (player2.Hitbox.Intersects(Projectile.Hitbox) && player2.active && (player2.team == player.team) && !player2.HasBuff(ModContent.BuffType<Healed>()))
                {
                    {
                        if (player.frozen == false)
                        {
                            player2.AddBuff(ModContent.BuffType<Veil>(), 10800);
                        }
                        if (player.frozen == true)
                        {
                            player2.AddBuff(ModContent.BuffType<Veil>(), 10780);
                        }
                        if ((player2.statLife < player2.statLifeMax2) || (player2.statMana < player2.statManaMax2))
                        {
                            player2.AddBuff(ModContent.BuffType<Healed>(), 30);
                            for (int i = 0; i < 50; i++)
                            {
                                Vector2 speed = Main.rand.NextVector2CircularEdge(1f, 1f);
                                Dust d = Dust.NewDustPerfect(player2.Center, ModContent.DustType<Healdust3>(), speed * 2, Scale: 2.1f);
                                d.noGravity = true;
                            }
                            SoundEngine.PlaySound(SoundID.DD2_DarkMageHealImpact, base.Projectile.Center);
                            player2.statMana += Yesh;
                            player2.ManaEffect(Yesh);
                            player2.Heal(HealAmount);
                        }
                    }
                }
            }
        }
    }
}
