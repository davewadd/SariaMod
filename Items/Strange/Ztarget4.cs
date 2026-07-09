using Microsoft.Xna.Framework;
using SariaMod.Buffs;
using SariaMod.Dusts;
using SariaMod.Items.Ruby;
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
        private bool ChargeFire2Played;
        private int NetSyncTimer;
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(ChannelTimer);
            writer.Write(SoundTimer);
            writer.Write(SoundTimer2);
            writer.Write(HitMax);
            writer.Write(ChargeFire1Played);
            writer.Write(ChargeFire2Played);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            ChannelTimer = (int)reader.ReadInt32();
            SoundTimer = (int)reader.ReadInt32();
            SoundTimer2 = (int)reader.ReadInt32();
            HitMax = (int)reader.ReadInt32();
            ChargeFire1Played = (bool)reader.ReadBoolean();
            ChargeFire2Played = (bool)reader.ReadBoolean();
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
            Projectile mother = Main.projectile[(int)base.Projectile.ai[1]];
            base.Projectile.rotation += (float)0.07;

            if (Main.myPlayer == Projectile.owner)
            {
                Projectile.Center = Main.MouseWorld;
                Projectile.velocity = Vector2.Zero;
                NetSyncTimer++;
                if (NetSyncTimer >= 4)
                {
                    NetSyncTimer = 0;
                    Projectile.netUpdate = true;
                }
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
                ChargeFire2Played = false;
            }
            else
            {
                // Normal charge sequence
                if (ChannelTimer >= 60 && !ChargeFire1Played)
                {
                    SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/ChargeFire1"), Projectile.Center);
                    ChargeFire1Played = true;

                    // Spawn the RovaRing visual
                    if (Main.myPlayer == Projectile.owner)
                    {
                        Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(),
                            Projectile.Center.X,
                            Projectile.Center.Y,
                            0f, 0f,
                            ModContent.ProjectileType<RovaRing>(),
                            0, 0f, Projectile.owner,
                            player.whoAmI,
                            base.Projectile.whoAmI
                        );
                    }
                }

                if (ChannelTimer >= 110 && ChargeFire1Played && !ChargeFire2Played)
                {
                    SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/ChargeFire2"), Projectile.Center);
                    ChargeFire2Played = true;

                    // Spawn RovaCenter at ztarget4 position
                    if (Main.myPlayer == Projectile.owner)
                    {
                        // Check no RovaCenter already exists before spawning
                        bool alreadyExists = false;
                        for (int i = 0; i < 1000; i++)
                        {
                            if (Main.projectile[i].active && Main.projectile[i].ModProjectile is RovaCenter && Main.projectile[i].owner == Projectile.owner)
                            {
                                alreadyExists = true;
                                break;
                            }
                        }

                        if (!alreadyExists)
                        {
                            FairyPlayer ownerState = player.Fairy();
                            float persistenceUpgrade = ownerState != null && ownerState.HasRovaSentryPersistenceUpgrade ? 1f : 0f;
                            Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(),
                            Projectile.Center.X,
                            Projectile.Center.Y,
                            0f, 0f,
                            ModContent.ProjectileType<RovaCenter>(),
                            Projectile.damage,
                                0f, Projectile.owner,
                                persistenceUpgrade,
                                base.Projectile.whoAmI
                            );
                        }
                    }
                }

                // Old behavior for when ChargeFire2 plays but we're already done
                if (ChargeFire2Played)
                {
                    ChannelTimer = 0;
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
            damage /= damage / 4;
        }
    }
}
