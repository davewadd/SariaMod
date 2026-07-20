using Microsoft.Xna.Framework;
using SariaMod.Buffs;
using SariaMod.Dusts;
using SariaMod.Items.Ruby;
using SariaMod.Netcode;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
namespace SariaMod.Items.Strange
{
    public class Ztarget4 : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Saria");
            ProjectileID.Sets.TrailCacheLength[base.Projectile.type] = 7;
            ProjectileID.Sets.TrailingMode[base.Projectile.type] = 0;
            Main.projFrames[base.Projectile.type] = 1;
        }
        private int ChannelTimer;
        private int SoundTimer;
        private int SoundTimer2;
        private int HitMax;
        private bool ChargeFire1Played;
        private bool RovaPersistenceUpgrade;
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(ChannelTimer);
            writer.Write(SoundTimer);
            writer.Write(SoundTimer2);
            writer.Write(HitMax);
            writer.Write(ChargeFire1Played);
            writer.Write(RovaPersistenceUpgrade);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            int syncedChannelTimer = reader.ReadInt32();
            int syncedSoundTimer = reader.ReadInt32();
            int syncedSoundTimer2 = reader.ReadInt32();
            int syncedHitMax = reader.ReadInt32();
            bool syncedChargeFire1Played = reader.ReadBoolean();
            bool syncedPersistenceUpgrade = reader.ReadBoolean();

            // The server derives charge timing from its own Saria copy. Do not
            // let an owner projectile update replace that authoritative timer.
            if (Main.netMode != NetmodeID.Server)
            {
                ChannelTimer = syncedChannelTimer;
                SoundTimer = syncedSoundTimer;
                SoundTimer2 = syncedSoundTimer2;
                HitMax = syncedHitMax;
                ChargeFire1Played = syncedChargeFire1Played;
            }

            RovaPersistenceUpgrade = syncedPersistenceUpgrade;
        }
        private const int sphereRadius = 100;
        public override void SetDefaults()
        {
            base.Projectile.width = 86;
            base.Projectile.height = 86;
            base.Projectile.netImportant = true;
            base.Projectile.alpha = 0;
            base.Projectile.friendly = true;
            base.Projectile.tileCollide = false;
            base.Projectile.penetrate = -1;
            base.Projectile.timeLeft = 401;
            base.Projectile.ignoreWater = true;
            base.Projectile.usesLocalNPCImmunity = true;
            base.Projectile.localNPCHitCooldown = 4;
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
        public override void AI()
        {
            Player player = Main.player[base.Projectile.owner];
            base.Projectile.rotation += (float)0.07;

            if (Main.myPlayer == Projectile.owner)
            {
                bool hasPersistenceUpgrade = player.Fairy().HasRovaSentryPersistenceUpgrade;
                if (RovaPersistenceUpgrade != hasPersistenceUpgrade)
                {
                    RovaPersistenceUpgrade = hasPersistenceUpgrade;
                    Projectile.netUpdate = true;
                }
            }

            SariaCursorNetworking.PublishLocalCursor(Projectile);
            if (SariaCursorNetworking.TryGetCursor(Projectile.owner, out Vector2 cursorPosition))
            {
                Projectile.Center = cursorPosition;
                Projectile.velocity = Vector2.Zero;
            }

            // Check if a RovaCenter already exists for this player
            bool rovaCenterExists = false;
            for (int i = 0; i < 1000; i++)
            {
                if (Main.projectile[i].active && Main.projectile[i].ModProjectile is RovaCenter && Main.projectile[i].owner == Projectile.owner)
                {
                    rovaCenterExists = true;
                    break;
                }
            }

            if (Projectile.timeLeft == 2)
            {
                SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/ZtargetCancel"), Projectile.Center);
            }
            if (Projectile.timeLeft == 401)
            {
                SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/ZtargetDeep"), Projectile.Center);
            }

            bool fireChargeActive = false;
            int owner = player.whoAmI;
            for (int i = 0; i < 1000; i++)
            {
                if (Main.projectile[i].active && Main.projectile[i].ModProjectile is Saria modProjectile && modProjectile.IsCharging >= 1 && modProjectile.Transform == 2 && i != base.Projectile.whoAmI && ((Main.projectile[i].owner == owner)))
                {
                    fireChargeActive = true;
                    Projectile.timeLeft = 20;
                    if (ChannelTimer <= 900)
                    {
                        ChannelTimer++;
                    }
                }
            }

            // --- NEW ROVA CHARGE SYSTEM (replaces old WillOWisp path) ---

            if (!fireChargeActive)
            {
                return;
            }

            if (rovaCenterExists)
            {
                // RovaCenter already exists - no charge countdown
                // Ztarget4 just follows cursor as a visual guide
                // The beam will aim at ztarget4 position via RovaCenter's manual override logic
                ChannelTimer = 0;
                ChargeFire1Played = false;
            }
            else
            {
                // ChargeFire1 starts the RovaCenter presentation and its visual charge-up.
                if (ChannelTimer >= 60)
                {
                    if (!ChargeFire1Played)
                    {
                        ChargeFire1Played = true;
                    }

                    // Player-owned projectiles must be created by their owner so
                    // Terraria assigns an identity that is stable on every peer.
                    if (Main.netMode == NetmodeID.SinglePlayer || Main.myPlayer == Projectile.owner)
                        EnsureRovaCenter(player);
                }
            }
        }

        private void EnsureRovaCenter(Player player)
        {
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                if (Main.projectile[i].active
                    && Main.projectile[i].owner == Projectile.owner
                    && Main.projectile[i].ModProjectile is RovaCenter)
                {
                    return;
                }
            }

            FairyPlayer ownerState = player.Fairy();
            float persistenceUpgrade = ownerState != null && ownerState.HasRovaSentryPersistenceUpgrade ? 1f : 0f;

            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                Projectile.Center.X,
                Projectile.Center.Y,
                0f,
                0f,
                ModContent.ProjectileType<RovaCenter>(),
                Projectile.damage,
                0f,
                Projectile.owner,
                persistenceUpgrade,
                RovaProjectileLink.GetHandle(Projectile));

            // NewProjectile performs the normal owner-client synchronization.
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
            target.buffImmune[ModContent.BuffType<Burning2>()] = false;
            target.buffImmune[BuffID.Frostburn] = false;
            target.buffImmune[BuffID.Poisoned] = false;
            target.buffImmune[BuffID.Venom] = false;
            target.buffImmune[BuffID.Electrified] = false;
            damage /= damage / 4;
        }
    }
}
