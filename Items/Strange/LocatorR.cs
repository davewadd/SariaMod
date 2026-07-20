using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SariaMod.Buffs;
using SariaMod.Dusts;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace SariaMod.Items.Strange
{
    public class LocatorR : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Saria");
            Main.projFrames[base.Projectile.type] = 1;
            ProjectileID.Sets.MinionShot[base.Projectile.type] = true;
            ProjectileID.Sets.TrailingMode[base.Projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[base.Projectile.type] = 30;
        }
        public override void SetDefaults()
        {
            base.Projectile.width = 100;
            base.Projectile.height = 100;
            base.Projectile.netImportant = true;
            base.Projectile.friendly = true;
            base.Projectile.ignoreWater = true;
            base.Projectile.usesLocalNPCImmunity = true;
            base.Projectile.localNPCHitCooldown = 100;
            base.Projectile.minionSlots = 0f;
            base.Projectile.extraUpdates = 1;
            base.Projectile.penetrate = -1;
            base.Projectile.tileCollide = false;
            base.Projectile.timeLeft = 500;
            base.Projectile.minion = true;
        }
        private int HitCount;
        public override bool? CanCutTiles()
        {
            return false;
        }
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(HitCount);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            HitCount = (int)reader.ReadInt32();
        }
        public override bool MinionContactDamage()
        {
            return true;
        }
        public override bool? CanHitNPC(NPC target)
        {
            return target.CanBeChasedBy(base.Projectile);
        }
        private const int sphereRadius = 3;
        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            Player player = Main.player[Projectile.owner];
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
            target.AddBuff(BuffID.Slow, 300);
            target.AddBuff(ModContent.BuffType<SariaCurse2>(), 50);
                if (player.HasBuff(ModContent.BuffType<Overcharged>()) && player.statMana >= 3)
                {
                    player.statMana -= 3;
                    player.manaRegenDelay = 30;
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), base.Projectile.Center + Utils.RandomVector2(Main.rand, 0f, 0f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ProjectileType<LocatorSmall>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                }
                SoundEngine.PlaySound(SoundID.DD2_WitherBeastDeath, base.Projectile.Center);
            FairyPlayer modPlayer = player.Fairy();
            modPlayer.SariaXp++;
            HitCount++;
            knockback /= 4;
        }
        public static Projectile LocateMother(Projectile projectile)
        {
            int Owner = ModContent.ProjectileType<Ztarget2>();
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                if (Main.projectile[i].type != Owner || !Main.projectile[i].active || Main.projectile[i].owner != projectile.owner)
                    continue;
                return Main.projectile[i];
            }
            return null;
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            FairyPlayer modPlayer = player.Fairy();
            Projectile owner = LocateMother(Projectile);
            {
                if (owner is null)
                {
                    Projectile.Kill();
                    return;
                }
                else if (owner != null && Projectile.timeLeft < 100)
                {
                    Projectile.timeLeft = 100;
                }
            }
            if (owner.netUpdate)
            {
                Projectile.netUpdate = true;
                if (Projectile.netSpam > 59)
                    Projectile.netSpam = 59;
            }
            Projectile.knockBack = 10;
            float thesuace = (Math.Abs(Projectile.velocity.X) + Math.Abs(Projectile.velocity.Y)) * .5f;
            for (int i = 0; i < thesuace; i++)
            {
                Vector2 speed2 = Main.rand.NextVector2CircularEdge(1f, 1f);
                if (VisualDustLimiter.TryReserveHalfCapacitySlot())
                {
                    Dust d = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<AbsorbPsychic>(), speed2 * -5, Scale: 1.5f);
                    d.noGravity = true;
                }
            }
            if (HitCount >= 1)
            {
                for (int j = 0; j < 3; j++) //set to 2
                {
                    global::SariaMod.Gores.LocatorShard.Spawn(
                        Projectile.GetSource_FromThis(),
                        base.Projectile.Center + Utils.RandomVector2(Main.rand, 0f, 0f),
                        Vector2.One.RotatedByRandom(MathHelper.TwoPi) * 4f,
                        Projectile.owner);
                }
                Projectile.Kill();
            }
            ///Start of Damage
            Projectile.SariaBaseDamage();
            Projectile.damage /= 4;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            // Starting search distance
            float distanceFromTarget = 10f;
            Vector2 targetCenter = Projectile.position;
            bool foundTarget = false;
            // This code is required if your minion weapon has the targeting feature
            if (player.HasMinionAttackTargetNPC && HitCount <= 0 && player.HeldItem.type == ModContent.ItemType<HealBall>())
            {
                NPC npc = Main.npc[player.MinionAttackTargetNPC];
                float between = Vector2.Distance(npc.Center, Projectile.Center);
                // Reasonable distance away so it doesn't target across multiple screens
                if (between < 2000f)
                {
                    distanceFromTarget = between;
                    targetCenter = npc.Center;
                    foundTarget = true;
                }
            }
            if (!foundTarget && HitCount <= 0 && player.HeldItem.type == ModContent.ItemType<HealBall>())
            {
                // This code is required either way, used for finding a target
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.CanBeChasedBy())
                    {
                        float between = Vector2.Distance(npc.Center, player.Center);
                        bool closest = Vector2.Distance(Projectile.Center, targetCenter) > between;
                        bool inRange = between < distanceFromTarget;
                        // Additional check for this specific minion behavior, otherwise it will stop attacking once it dashed through an enemy while flying though tiles afterwards
                        // The number depends on various parameters seen in the movement code below. Test different ones out until it works alright
                        bool closeThroughWall = between < 1000f;
                        if (((closest && inRange) || !foundTarget) && (closeThroughWall))
                        {
                            distanceFromTarget = between;
                            targetCenter = npc.Center;
                            foundTarget = true;
                        }
                    }
                }
            }
            if (HitCount >= 1)
            {
                foundTarget = true;
            }
            // friendly needs to be set to true so the minion can deal contact damage
            // friendly needs to be set to false so it doesn't damage things like target dummies while idling
            // Both things depend on if it has a target or not, so it's just one assignment here
            // You don't need this assignment if your minion is shooting things instead of dealing contact damage
            float speed = 70f;
            float inertia = 20f;
            Vector2 idlePosition = player.Center;
            if (!foundTarget)
            {
                Projectile.timeLeft = 450;
                float hoverAcceleration = 0.2f;
                float distanceFromOwner = Projectile.Distance(owner.Center);
                if (distanceFromOwner < 200f)
                    hoverAcceleration = 0.12f;
                if (distanceFromOwner < 140f)
                    hoverAcceleration = 0.06f;
                if (distanceFromOwner > 0f)
                {
                    if (Math.Abs(owner.Center.X - Projectile.Center.X) > 0f)
                        Projectile.velocity.X += hoverAcceleration * Math.Sign(owner.Center.X - Projectile.Center.X);
                    if (Math.Abs(owner.Center.Y - Projectile.Center.Y) > 0f)
                        Projectile.velocity.Y += hoverAcceleration * Math.Sign(owner.Center.Y - Projectile.Center.Y);
                }
                else if (Projectile.velocity.Length() > 1f)
                    Projectile.velocity *= 0.96f;
                if (Math.Abs(Projectile.velocity.Y) < 1f)
                    Projectile.velocity.Y -= 0.1f;
                float maxSpeed = 55f;
                if (Projectile.velocity.Length() > maxSpeed)
                    Projectile.velocity = Vector2.Normalize(Projectile.velocity) * maxSpeed;
                if (Projectile.Distance(owner.Center) > 250f)
                {
                    Projectile.Center = owner.Center;
                    Projectile.velocity = Main.rand.NextVector2CircularEdge(3f, 3f);
                    Projectile.netUpdate = true;
                }
            }
            // Default movement parameters (here for attacking)
            if (Projectile.timeLeft == 500)
            {
                SoundEngine.PlaySound(SoundID.DD2_SkyDragonsFuryShot, base.Projectile.Center);
            }
            // Minion has a target: attack (here, fly towards the enemy)
            if (distanceFromTarget > 40f)
            {
                // The immediate range around the target (so it doesn't latch onto it when close)
                Vector2 direction = targetCenter - Projectile.Center;
                direction.Normalize();
                direction *= speed;
                Projectile.velocity = (Projectile.velocity * (inertia - 2) + direction) / inertia;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            {
                Texture2D starTexture2 = TextureAssets.Projectile[ModContent.ProjectileType<Locator>()].Value;
                Texture2D starTexture = TextureAssets.Projectile[ModContent.ProjectileType<LocatorBeam>()].Value;
                Vector2 drawPosition;
                for (int i = 1; i < base.Projectile.oldPos.Length; i++)
                {
                    float completionRatio = (float)i / (float)base.Projectile.oldPos.Length;
                    Color drawColor = Color.Lerp(lightColor, Color.LightPink, 2f);
                    drawColor = Color.Lerp(drawColor, Color.DarkViolet, completionRatio);
                    drawColor = Color.Lerp(drawColor, Color.Transparent, completionRatio);
                    drawPosition = base.Projectile.oldPos[i] + base.Projectile.Size * 0.5f - Main.screenPosition;
                    Main.spriteBatch.Draw(starTexture, drawPosition, null, base.Projectile.GetAlpha(drawColor), 0, Utils.Size(starTexture) * 0.5f, base.Projectile.scale, SpriteEffects.None, 0f);
                }
                for (int j = 0; j < 4; j++)
                {
                    drawPosition = base.Projectile.Center - Main.screenPosition;
                    Main.spriteBatch.Draw(starTexture2, drawPosition, null, Color.White, base.Projectile.oldRot[j], Utils.Size(starTexture2) * 0.5f, base.Projectile.scale, SpriteEffects.None, 0f);
                }
                return false;
            }
        }
    }
}
