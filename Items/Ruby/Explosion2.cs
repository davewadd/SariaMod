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
        private const int VisualTailTicks = EruptionSmokeVisuals.LifetimeTicks;
        private const int CoreDiskSpawnInterval = 3;
        private const int CoreDiskSampleCount = 9;
        private const float OuterShellAngleJitter = 0.06981317f;
        private const float OuterShellSpeedVariation = 0.04f;
        private const float OuterShellSpawnJitter = 3f;
        private readonly EruptionSmokeVisuals eruptionSmokeVisuals = new EruptionSmokeVisuals(64);
        private bool attackFinished;
        private int visualTailTimeLeft;
        private int smokePatternTick;

        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Saria");
            ProjectileID.Sets.TrailCacheLength[base.Projectile.type] = 6;
            ProjectileID.Sets.TrailingMode[base.Projectile.type] = 0;
            ProjectileID.Sets.DrawScreenCheckFluff[base.Projectile.type] = 1200;
            Main.projFrames[base.Projectile.type] = 5;
        }
        public override void SetDefaults()
        {
            base.Projectile.width = 600;
            base.Projectile.height = 600;
            base.Projectile.alpha = 300;
            base.Projectile.friendly = true;
            base.Projectile.tileCollide = false;
            base.Projectile.penetrate = -1;
            base.Projectile.timeLeft = 200;
            base.Projectile.ignoreWater = true;
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
            eruptionSmokeVisuals.Update();
            if (attackFinished)
            {
                Projectile.friendly = false;
                Projectile.damage = 0;
                Projectile.velocity = Vector2.Zero;
                visualTailTimeLeft--;
                if (visualTailTimeLeft <= 0)
                {
                    Projectile.Kill();
                }

                return;
            }

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

            float identityPhase = EruptionSmokeVisuals.CreatePatternPhase(0, Projectile.identity);
            float patternPhase = EruptionSmokeVisuals.CreatePatternPhase(smokePatternTick, Projectile.identity);
            if (smokePatternTick % CoreDiskSpawnInterval == 0)
            {
                int sampleIndex = smokePatternTick / CoreDiskSpawnInterval;
                Vector2 diskOffset = EruptionSmokeVisuals.CreateSunflowerOffset(
                    sampleIndex,
                    CoreDiskSampleCount,
                    sphereRadius,
                    identityPhase);
                eruptionSmokeVisuals.Spawn(
                    centerthis + new Vector2(0f, -10f) + diskOffset,
                    EruptionSmokeVisuals.CreateDustLikeVelocity(),
                    EruptionSmokeKind.Yellow);
            }

            if (Projectile.frame == 1)
            {
                eruptionSmokeVisuals.SpawnEvenRing(
                    centerthis,
                    3.5f,
                    EruptionSmokeKind.YellowOrange,
                    1,
                    patternPhase + MathHelper.PiOver2);
            }

            eruptionSmokeVisuals.SpawnEvenRing(
                centerthis,
                7f,
                EruptionSmokeKind.Red,
                1,
                patternPhase + MathHelper.Pi / 6f,
                OuterShellAngleJitter,
                OuterShellSpeedVariation,
                OuterShellSpawnJitter);
            eruptionSmokeVisuals.SpawnEvenRing(
                centerthis,
                14f,
                EruptionSmokeKind.Smoke,
                1,
                patternPhase + MathHelper.PiOver2,
                OuterShellAngleJitter,
                OuterShellSpeedVariation,
                OuterShellSpawnJitter);

            for (int i = 0; i < 1; i++)
            {
                Vector2 speed = Main.rand.NextVector2CircularEdge(1f, 1f);
                Dust d = Dust.NewDustPerfect(centerthis, ModContent.DustType<SmokeDust6>(), speed * 13, Scale: 3.5f);
                d.noGravity = true;
            }
            smokePatternTick++;
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
                    base.Projectile.frame = Main.projFrames[base.Projectile.type] - 1;
                    attackFinished = true;
                    visualTailTimeLeft = VisualTailTicks;
                    Projectile.friendly = false;
                    Projectile.damage = 0;
                    Projectile.velocity = Vector2.Zero;
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
            if (!attackFinished)
            {
                FairyProjectile.DrawCenteredAndAfterimage(base.Projectile, lightColor, ProjectileID.Sets.TrailingMode[base.Projectile.type]);
            }

            eruptionSmokeVisuals.Draw();
            return false;
        }

        public override void Kill(int timeLeft)
        {
            eruptionSmokeVisuals.Clear();
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
