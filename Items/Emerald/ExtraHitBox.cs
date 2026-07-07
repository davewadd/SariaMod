using Microsoft.Xna.Framework;
using SariaMod.Buffs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using SariaMod.Dusts;
namespace SariaMod.Items.Emerald
{
    public class ExtraHitBox : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Saria");
            ProjectileID.Sets.TrailCacheLength[base.Projectile.type] = 6;
            ProjectileID.Sets.TrailingMode[base.Projectile.type] = 0;
            Main.projFrames[base.Projectile.type] = 1;
        }
        public override void SetDefaults()
        {
            base.Projectile.width = 500;
            base.Projectile.height = 100;
            base.Projectile.aiStyle = 21;
            base.Projectile.alpha = 300;
            base.Projectile.friendly = true;
            base.Projectile.tileCollide = false;
            base.Projectile.penetrate = -1;
            base.Projectile.timeLeft = 2000;
            base.Projectile.ignoreWater = true;
            AIType = 274;
            base.Projectile.usesLocalNPCImmunity = true;
            base.Projectile.localNPCHitCooldown = 20;
        }
        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            Player player = Main.player[base.Projectile.owner];
            Player player2 = Main.LocalPlayer;
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
            target.AddBuff(BuffID.Electrified, 300);
            target.AddBuff(BuffID.Slow, 300);
            modPlayer.SariaXp++;
            knockback = 10;
            if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), player.Center.X, player.Center.Y + 50, 0, 0, ModContent.ProjectileType<HitCheck3>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
            if (target.position.X + (float)(target.width / 2) > Projectile.position.X + (float)(Projectile.width / 2))
            {
                hitDirection = 1;
            }
            else
            {
                hitDirection = -1;
            }
            if (player.HasBuff(ModContent.BuffType<StatRaise>()))
            {
                damage += (damage) / 4;
            }
            if (player.HasBuff(ModContent.BuffType<StatLower>()))
            {
                damage /= 2;
            }
        }
        public override bool? CanHitNPC(NPC target)
        {
            Player player = Main.player[base.Projectile.owner];
            if (!target.HasBuff(ModContent.BuffType<MeteorSpikeDebuff>()) && !target.HasBuff(ModContent.BuffType<MeteorLaunchDebuff>()))
            {
                return target.CanBeChasedBy(Projectile);
            }
            else
            {
                return false;
            }
        }
        public override void AI()
        {
            Player player = Main.player[base.Projectile.owner];
            Player player2 = Main.LocalPlayer;
            FairyPlayer modPlayer = player.Fairy();
            if (player.HasBuff(ModContent.BuffType<StatRaise>()))
            {
                Projectile.localNPCHitCooldown = 16;
            }
            if (player.HasBuff(ModContent.BuffType<StatLower>()))
            {
                Projectile.localNPCHitCooldown = 160;
            }
            if (player.ownedProjectileCounts[ModContent.ProjectileType<Emeraldspike3_2>()] <= 0f)
            {
                Projectile.Kill();
            }
                Projectile.localNPCHitCooldown = 14;
            for (int i = 0; i < 1000; i++)
                if (Main.projectile[i].active && i != base.Projectile.whoAmI && Main.projectile[i].Hitbox.Intersects(base.Projectile.Hitbox) && Main.projectile[i].active && ((!Main.projectile[i].friendly && Main.projectile[i].hostile) || (Main.projectile[i].trap)))
                {
                    Main.projectile[i].Kill();
                    if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), player.position.X + 0, player.position.Y + 0, 0, 0, ModContent.ProjectileType<HitCheck3>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, Projectile.whoAmI);
                    if (!player.HasBuff(ModContent.BuffType<Overcharged>()))
                    {
                        if (Main.rand.NextBool(60))
                        {
                            Item.NewItem(Projectile.GetSource_FromThis(), Projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ItemType<LivingSilverShard>());
                        }
                    }
                    if (player.HasBuff(ModContent.BuffType<Overcharged>()))
                    {
                        if (Main.rand.NextBool(25))
                        {
                            Item.NewItem(Projectile.GetSource_FromThis(), Projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ItemType<LivingSilverShard>());
                        }
                    }
                }
            Lighting.AddLight(Projectile.Center, Color.Silver.ToVector3() * 2f);
            if (Projectile.spriteDirection == -1)
            {
                Projectile.position.X = player.Center.X - 80;
            }
            if (Projectile.spriteDirection == 1)
            {
                Projectile.position.X = player.Center.X - 70;
            }
            Projectile.position.X = player.position.X - 230;
            Projectile.position.Y = player.Center.Y - 50;
            if (Projectile.timeLeft >= 196)
            {
                Projectile.spriteDirection = player.direction;
            }
            if (Projectile.timeLeft < 196 && Projectile.timeLeft > 10)
            {
                Projectile.timeLeft = 180;
            }
        }
    }
}
