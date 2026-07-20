using Microsoft.Xna.Framework;
using SariaMod.Buffs;
using SariaMod.Dusts;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using SariaMod.Netcode.FireSoundSync;
namespace SariaMod.Items.Ruby
{
    public class Explosion : ModProjectile
    {
        private const int VisualTailTicks = EruptionSmokeVisuals.LifetimeTicks;
        private const int CoreDiskPuffsPerTick = 2;
        private const int CoreDiskSampleCount = 50;
        private const int InnerShellPuffsPerTick = 3;
        private const int OuterShellPuffsPerTick = 3;
        private const int ClusterEruptionSampleCount = 5;
        private const float ClusterEruptionScatterRadius = 180f;
        private const float OuterShellAngleJitter = 0.06981317f;
        private const float OuterShellSpeedVariation = 0.04f;
        private const float OuterShellSpawnJitter = 3f;
        private readonly EruptionSmokeVisuals eruptionSmokeVisuals = new EruptionSmokeVisuals(256);
        private bool attackFinished;
        private int visualTailTimeLeft;
        private int smokePatternTick;
        private bool SpawnsClusterEruptions => Projectile.ai[1] >= 0.5f;

        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Saria");
            ProjectileID.Sets.TrailCacheLength[base.Projectile.type] = 6;
            ProjectileID.Sets.TrailingMode[base.Projectile.type] = 0;
            ProjectileID.Sets.DrawScreenCheckFluff[base.Projectile.type] = 1200;
            Main.projFrames[base.Projectile.type] = 5;
        }
        private int HitBomb;
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(HitBomb);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            HitBomb = (int)reader.ReadSingle();
        }
        public override void SetDefaults()
        {
            base.Projectile.width = 800;
            base.Projectile.height = 800;
            base.Projectile.alpha = 300;
            base.Projectile.friendly = true;
            base.Projectile.tileCollide = false;
            base.Projectile.penetrate = -1;
            base.Projectile.timeLeft = 200;
            base.Projectile.ignoreWater = true;
            base.Projectile.usesLocalNPCImmunity = true;
            base.Projectile.localNPCHitCooldown = 1000;
        }
        private const int sphereRadius = 60;
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
            FairyProjectile.HomeInOnNPC(base.Projectile, ignoreTiles: true, 600f, 25f, 20f);
            Projectile.SariaBaseDamage();
            Vector2 centerthis = Projectile.Center;
            centerthis.X -= 30;
            centerthis.Y -= 35;

            float identityPhase = EruptionSmokeVisuals.CreatePatternPhase(0, Projectile.identity);
            float patternPhase = EruptionSmokeVisuals.CreatePatternPhase(smokePatternTick, Projectile.identity);
            for (int d = 0; d < CoreDiskPuffsPerTick; d++)
            {
                int sampleIndex = smokePatternTick * CoreDiskPuffsPerTick + d;
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
                    1.4f,
                    EruptionSmokeKind.Yellow,
                    InnerShellPuffsPerTick,
                    patternPhase);
                eruptionSmokeVisuals.SpawnEvenRing(
                    centerthis,
                    3.5f,
                    EruptionSmokeKind.YellowOrange,
                    InnerShellPuffsPerTick,
                    patternPhase + MathHelper.Pi / InnerShellPuffsPerTick);
            }

            float redPhase = patternPhase + MathHelper.Pi / 6f;
            eruptionSmokeVisuals.SpawnEvenRing(
                centerthis,
                7f,
                EruptionSmokeKind.Red,
                OuterShellPuffsPerTick,
                redPhase,
                OuterShellAngleJitter,
                OuterShellSpeedVariation,
                OuterShellSpawnJitter);
            eruptionSmokeVisuals.SpawnEvenRing(
                centerthis,
                14f,
                EruptionSmokeKind.Smoke,
                OuterShellPuffsPerTick,
                redPhase + MathHelper.Pi / OuterShellPuffsPerTick,
                OuterShellAngleJitter,
                OuterShellSpeedVariation,
                OuterShellSpawnJitter);

            for (int i = 0; i < 5; i++)
            {
                Vector2 speed = Main.rand.NextVector2CircularEdge(1f, 1f);
                Dust d = Dust.NewDustPerfect(centerthis, ModContent.DustType<SmokeDust6>(), speed * 13, Scale: 3.5f);
                d.noGravity = true;
            }
            smokePatternTick++;
            Lighting.AddLight(Projectile.Center, Color.OrangeRed.ToVector3() * 6f);
            {
                Projectile.knockBack = 50;
                base.Projectile.frameCounter++;
                if (base.Projectile.frameCounter >= 5)
                {
                    base.Projectile.frame++;
                    if (SpawnsClusterEruptions)
                    {
                        int childIndex = base.Projectile.frame - 1;
                        Vector2 childOffset = EruptionSmokeVisuals.CreateSunflowerOffset(
                            childIndex,
                            ClusterEruptionSampleCount,
                            ClusterEruptionScatterRadius,
                            EruptionSmokeVisuals.CreatePatternPhase(0, Projectile.identity));
                        int childType = ModContent.ProjectileType<Explosion3>();
                        if (Main.myPlayer == Projectile.owner
                            && EruptionProjectileLimitGlobal.CanSpawn(Projectile.owner, childType))
                        {
                            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + new Vector2(0f, -10f) + childOffset, Vector2.Zero, childType, (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                        }
                    }
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
                if (base.Projectile.timeLeft == 199)
                {
                    if (Main.myPlayer == Projectile.owner)
                    {
                        PlaySyncedFireSound(FireSoundId.Bomb);
                        PlaySyncedFireSound(FireSoundId.Item116);
                    }
                }
                if (base.Projectile.timeLeft == 195)
                {
                    int flameType = ModContent.ProjectileType<Flame>();
                    if (EruptionProjectileLimitGlobal.CanSpawn(Projectile.owner, flameType))
                    {
                        bool useSparseLifetime = player.ownedProjectileCounts[flameType]
                            < Flame.SparsePopulationThreshold;
                        for (int j = 0; j < 3; j++) //set to 2
                        {
                            if (!EruptionProjectileLimitGlobal.CanSpawn(Projectile.owner, flameType))
                            {
                                break;
                            }

                            Vector2 thisspot = Projectile.Center;
                            thisspot.X += 100;
                            thisspot.Y += 50;
                            if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), thisspot + Utils.RandomVector2(Main.rand, -204f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, flameType, (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, useSparseLifetime ? 1f : 0f);
                        }
                    }
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            eruptionSmokeVisuals.Draw();
            return !attackFinished;
        }

        public override void Kill(int timeLeft)
        {
            eruptionSmokeVisuals.Clear();
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
            target.buffImmune[BuffID.Frostburn] = false;
            target.buffImmune[BuffID.Poisoned] = false;
            target.buffImmune[BuffID.Venom] = false;
            target.buffImmune[BuffID.Electrified] = false;
            target.buffImmune[ModContent.BuffType<Burning2>()] = false;
            target.AddBuff(ModContent.BuffType<Burning2>(), 200);
            modPlayer.SariaXp++;
            knockback = 20f;
            if (target.type == NPCID.Mothron || target.type == NPCID.MourningWood || target.type == NPCID.Everscream)
            {
                damage *= 6;
            }
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
