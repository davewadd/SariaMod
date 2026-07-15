using Microsoft.Xna.Framework;
using SariaMod.Buffs;
using SariaMod.Dusts;
using SariaMod.Netcode.FireSoundSync;
using SariaMod.Netcode.SariaSoundSync;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
namespace SariaMod.Items.Ruby
{
    public class Explosion2 : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Saria");
            ProjectileID.Sets.TrailCacheLength[base.Projectile.type] = 6;
            ProjectileID.Sets.TrailingMode[base.Projectile.type] = 0;
            Main.projFrames[base.Projectile.type] = 5;
        }
        public override void SetDefaults()
        {
            base.Projectile.width = 600;
            base.Projectile.height = 600;
            base.Projectile.aiStyle = 21;
            base.Projectile.alpha = 300;
            base.Projectile.friendly = true;
            base.Projectile.tileCollide = false;
            base.Projectile.penetrate = -1;
            base.Projectile.timeLeft = 200;
            base.Projectile.ignoreWater = true;
            AIType = 274;
            base.Projectile.usesLocalNPCImmunity = true;
            base.Projectile.localNPCHitCooldown = 1000;
        }
        private const int sphereRadius = 100;
        private bool RovaCenterEruptionCenterSet;
        private Vector2 RovaCenterEruptionCenter;

        private bool IsRovaCenterEruption => Projectile.ai[0] < -0.5f;

        public override bool? CanCutTiles()
        {
            return false;
        }
        public override void AI()
        {
            System.Diagnostics.Debug.WriteLine($"[Explosion2 AI] Running - timeLeft: {Projectile.timeLeft}, frame: {Projectile.frame}, owner: {Projectile.owner}, myPlayer: {Main.myPlayer}");
            
            Player player = Main.player[base.Projectile.owner];
            FairyPlayer modPlayer = player.Fairy();
            bool rovaCenterEruption = IsRovaCenterEruption;
            if (rovaCenterEruption)
            {
                if (!RovaCenterEruptionCenterSet)
                {
                    RovaCenterEruptionCenter = Projectile.Center;
                    RovaCenterEruptionCenterSet = true;
                }

                Projectile.Center = RovaCenterEruptionCenter;
                Projectile.velocity = Vector2.Zero;
            }
            else
            {
                FairyProjectile.HomeInOnNPC(base.Projectile, ignoreTiles: true, 600f, 25f, 20f);
            }

            Lighting.AddLight(base.Projectile.Center, 20f, 5f, 0f);
            Projectile.SariaBaseDamage();
            Projectile.damage /= 5;
            Projectile.scale *= 1.05f;
            Projectile.width = 450;
            Projectile.height = 450;
            Vector2 centerthis = Projectile.Center;
            if (!rovaCenterEruption)
            {
                centerthis.X -= 30;
                centerthis.Y -= 35;
            }
            if (Main.rand.NextBool())
            {
                for (int d = 0; d < 1; d++)
                {
                    float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius * sphereRadius));
                    double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                    Dust.NewDust(new Vector2(centerthis.X + radius * (float)Math.Cos(angle), (centerthis.Y - 10) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<SmokeDust5Yellow>(), 0f, 0f, 0, default(Color), 1.5f);
                }
            }
            if (Projectile.frame == 1)
            {
                for (int i = 0; i < 1; i++)
                {
                    Vector2 speed = Main.rand.NextVector2CircularEdge(.7f, .7f);
                    Dust d = Dust.NewDustPerfect(centerthis, ModContent.DustType<SmokeDust5Yellorange>(), speed * -5, Scale: 1f);
                    d.noGravity = true;
                }
            }
            {
                for (int i = 0; i < 1; i++)
                {
                    Vector2 speed = Main.rand.NextVector2CircularEdge(1f, 1f);
                    Dust d = Dust.NewDustPerfect(centerthis, ModContent.DustType<SmokeDust5Red>(), speed * -7, Scale: 4f);
                    d.noGravity = true;
                }
                for (int i = 0; i < 1; i++)
                {
                    Vector2 speed = Main.rand.NextVector2CircularEdge(2f, 2f);
                    Dust d = Dust.NewDustPerfect(centerthis, ModContent.DustType<SmokeDust5>(), speed * -7, Scale: 1f);
                    d.noGravity = true;
                }
            }
            for (int i = 0; i < 1; i++)
            {
                Vector2 speed = Main.rand.NextVector2CircularEdge(1f, 1f);
                Dust d = Dust.NewDustPerfect(centerthis, ModContent.DustType<SmokeDust6>(), speed * 13, Scale: 3.5f);
                d.noGravity = true;
            }
            Lighting.AddLight(Projectile.Center, Color.OrangeRed.ToVector3() * 0.78f);
            {
                Projectile.knockBack = 50;
                if (!rovaCenterEruption)
                {
                    base.Projectile.velocity.X = (1 * player.direction);
                    base.Projectile.velocity.Y = 0;
                }
                base.Projectile.frameCounter++;
                if (base.Projectile.frameCounter >= 5)
                {
                    base.Projectile.frame++;
                    base.Projectile.frameCounter = 0;
                }
                if (base.Projectile.frame >= Main.projFrames[base.Projectile.type])
                {
                    base.Projectile.frame = 0;
                    base.Projectile.Kill();
                }
            }
            if (Projectile.timeLeft >= 200)
            {
                PlaySyncedFireSound(FireSoundId.Bomb);
                PlaySyncedFireSound(FireSoundId.Item116);
            }
            if (Projectile.timeLeft == 2)
            {
                PlaySyncedFireSound(FireSoundId.Dragon);
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            FairyProjectile.DrawCenteredAndAfterimage(base.Projectile, lightColor, ProjectileID.Sets.TrailingMode[base.Projectile.type]);
            return false;
        }
        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            Player player = Main.player[base.Projectile.owner];
            FairyPlayer modPlayer = player.Fairy();
            Vector2 direction = target.Center - player.Center;
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
            modPlayer.SariaXp++;
            if (target.type == NPCID.Mothron || target.type == NPCID.MourningWood || target.type == NPCID.Everscream)
            {
                damage *= 4;
            }
        }
        private void PlaySyncedFireSound(FireSoundId soundId)
        {
            if (Main.netMode == NetmodeID.SinglePlayer)
            {
                SoundStyle style = soundId switch
                {
                    FireSoundId.Bomb => new SoundStyle("SariaMod/Sounds/Bomb"),
                    FireSoundId.Item116 => SoundID.Item116,
                    FireSoundId.Dragon => SoundID.DD2_SkyDragonsFuryShot,
                    _ => default
                };

                if (style != default)
                    SoundEngine.PlaySound(style, Projectile.Center);

                return;
            }

            // In multiplayer, always let the local client hear the sound if this is their projectile.
            if (Main.myPlayer == Projectile.owner)
            {
                SoundStyle localStyle = soundId switch
                {
                    FireSoundId.Bomb => new SoundStyle("SariaMod/Sounds/Bomb"),
                    FireSoundId.Item116 => SoundID.Item116,
                    FireSoundId.Dragon => SoundID.DD2_SkyDragonsFuryShot,
                    _ => default
                };

                if (localStyle != default)
                    SoundEngine.PlaySound(localStyle, Projectile.Center);
            }

            // Clients (including host-and-play) send to the server; the server then broadcasts.
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                if (Main.myPlayer != Projectile.owner)
                    return;

                ModPacket packet = Mod.GetPacket();
                FireSoundSyncMessage.Write(packet, Projectile, soundId);
                packet.Send();
                return;
            }

            // Dedicated server: broadcast directly.
            if (Main.netMode == NetmodeID.Server)
            {
                ModPacket packet = Mod.GetPacket();
                FireSoundSyncMessage.Write(packet, Projectile, soundId);
                packet.Send();
            }
        }
    }
}
