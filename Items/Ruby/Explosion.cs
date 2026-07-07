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
        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Saria");
            ProjectileID.Sets.TrailCacheLength[base.Projectile.type] = 6;
            ProjectileID.Sets.TrailingMode[base.Projectile.type] = 0;
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
            System.Diagnostics.Debug.WriteLine($"[Explosion AI] Running - timeLeft: {Projectile.timeLeft}, frame: {Projectile.frame}, owner: {Projectile.owner}, myPlayer: {Main.myPlayer}");
            
            Player player = Main.player[base.Projectile.owner];
            FairyPlayer modPlayer = player.Fairy();
            FairyProjectile.HomeInOnNPC(base.Projectile, ignoreTiles: true, 600f, 25f, 20f);
            Projectile.SariaBaseDamage();
            Vector2 centerthis = Projectile.Center;
            centerthis.X -= 30;
            centerthis.Y -= 35;
            if (Main.rand.NextBool())
            {
                for (int d = 0; d < 8; d++)
                {
                    float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius * sphereRadius));
                    double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                    Dust.NewDust(new Vector2(centerthis.X + radius * (float)Math.Cos(angle), (centerthis.Y - 10) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<SmokeDust5Yellow>(), 0f, 0f, 0, default(Color), 1.5f);
                }
            }
            if (Projectile.frame == 1)
            {
                for (int i = 0; i < 10; i++)
                {
                    Vector2 speed = Main.rand.NextVector2CircularEdge(.7f, .7f);
                    Dust d = Dust.NewDustPerfect(centerthis, ModContent.DustType<SmokeDust5Yellow>(), speed * -2, Scale: 1f);
                    d.noGravity = true;
                }
                for (int i = 0; i < 10; i++)
                {
                    Vector2 speed = Main.rand.NextVector2CircularEdge(.7f, .7f);
                    Dust d = Dust.NewDustPerfect(centerthis, ModContent.DustType<SmokeDust5Yellorange>(), speed * -5, Scale: 1f);
                    d.noGravity = true;
                }
            }
            {
                for (int i = 0; i < 3; i++)
                {
                    Vector2 speed = Main.rand.NextVector2CircularEdge(1f, 1f);
                    Dust d = Dust.NewDustPerfect(centerthis, ModContent.DustType<SmokeDust5Red>(), speed * -7, Scale: 4f);
                    d.noGravity = true;
                }
                for (int i = 0; i < 3; i++)
                {
                    Vector2 speed = Main.rand.NextVector2CircularEdge(2f, 2f);
                    Dust d = Dust.NewDustPerfect(centerthis, ModContent.DustType<SmokeDust5>(), speed * -7, Scale: 1f);
                    d.noGravity = true;
                }
            }
            for (int i = 0; i < 5; i++)
            {
                Vector2 speed = Main.rand.NextVector2CircularEdge(1f, 1f);
                Dust d = Dust.NewDustPerfect(centerthis, ModContent.DustType<SmokeDust6>(), speed * 13, Scale: 3.5f);
                d.noGravity = true;
            }
            Lighting.AddLight(Projectile.Center, Color.OrangeRed.ToVector3() * 6f);
            {
                Projectile.knockBack = 50;
                base.Projectile.frameCounter++;
                if (base.Projectile.frameCounter >= 5)
                {
                    base.Projectile.frame++;
                    if (player.HasBuff(ModContent.BuffType<Overcharged>()))
                    {
                        float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius * sphereRadius));
                        double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                        if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center.X + radius * (float)Math.Cos(angle), (Projectile.Center.Y - 10) + radius * (float)Math.Sin(angle), 0, 0, ModContent.ProjectileType<Explosion2>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                    }
                    base.Projectile.frameCounter = 0;
                }
                if (base.Projectile.frame >= Main.projFrames[base.Projectile.type])
                {
                    base.Projectile.frame = 0;
                    base.Projectile.Kill();
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
                    if (player.ownedProjectileCounts[ModContent.ProjectileType<Flame>()] < 60f)
                    {
                        for (int j = 0; j < 3; j++) //set to 2
                        {
                            Vector2 thisspot = Projectile.Center;
                            thisspot.X += 100;
                            thisspot.Y += 50;
                            if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), thisspot + Utils.RandomVector2(Main.rand, -204f, 24f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ProjectileType<Flame>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                        }
                    }
                }
            }
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
