using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using SariaMod.Items.Strange;
using Terraria.Audio;
using Terraria.Graphics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.GameContent.Events;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria.ObjectData;
using SariaMod.Items;
using SariaMod.Buffs;
using SariaMod.Items.zTalking;
using SariaMod.Dusts;
using SariaMod.Items.Amber;
using SariaMod.Items.Amethyst;
using SariaMod.Items.Bands;
using SariaMod.Items.Emerald;
using SariaMod.Items.Ruby;
using SariaMod.Items.Sapphire;
using SariaMod.Items.Topaz;
using SariaMod.Items.zPearls;
using Terraria.Localization;
namespace SariaMod.Items.Emerald
{
    public class BouncingShard2 : ModProjectile
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
            base.Projectile.width = 10;
            base.Projectile.height = 10;
            base.Projectile.netImportant = true;
            base.Projectile.friendly = true;
            base.Projectile.ignoreWater = true;
            base.Projectile.usesLocalNPCImmunity = true;
            base.Projectile.localNPCHitCooldown = 7;
            base.Projectile.minionSlots = 0f;
            base.Projectile.extraUpdates = 1;
            base.Projectile.penetrate = -1;
            Projectile.tileCollide = true;
            base.Projectile.timeLeft = 1600;
        }
        public override bool? CanCutTiles()
        {
            return false;
        }
        public override bool MinionContactDamage()
        {
            return false;
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Player player = Main.player[base.Projectile.owner];
            FairyPlayer modPlayer = player.Fairy();
            if ((Math.Abs(oldVelocity.X) > (Math.Abs(Projectile.velocity.X)) * 2))
            {
                base.Projectile.velocity.X = -1 * (oldVelocity.X * 1f);
                SoundEngine.PlaySound(SoundID.NPCHit3, base.Projectile.Center);
            }
            if ((Math.Abs(oldVelocity.Y) > (Math.Abs(Projectile.velocity.Y)) * 2))
            {
                base.Projectile.velocity.Y = -1 * (oldVelocity.Y * 1f);
                SoundEngine.PlaySound(SoundID.NPCHit3, base.Projectile.Center);
            }
            float distanceFromTarget = 10f;
            Vector2 targetCenter = Projectile.position;
            bool foundTarget = false;
            if (player.HasMinionAttackTargetNPC)
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
            if (!foundTarget)
            {
                // This code is required either way, used for finding a target
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.CanBeChasedBy() && npc.active && (Main.myPlayer == Projectile.owner))
                    {
                        float between = Vector2.Distance(npc.Center, Main.MouseWorld);
                        bool closest = Vector2.Distance(Main.MouseWorld, targetCenter) > between;
                        bool closeThroughWall = between < 1500f;
                        bool CanSee = Collision.CanHitLine(Projectile.position, Projectile.width, Projectile.height, npc.position, 1, 1);
                        if (((closest) || !foundTarget) && (closeThroughWall) && CanSee)
                        {
                            distanceFromTarget = between;
                            targetCenter = npc.Center;
                            foundTarget = true;
                        }
                    }
                }
            }
            float speed = 70f;
            float inertia = 20f;
            if (foundTarget)
            {
                Vector2 direction2 = targetCenter - Projectile.Center;
                direction2.Normalize();
                direction2 *= speed;
                Projectile.velocity = (Projectile.velocity * (inertia - 2) + direction2) / inertia;
            }
            return false;
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            Lighting.AddLight(Projectile.Center, Color.Purple.ToVector3() * 1f);
            base.Projectile.rotation += 0.25f;
            int SpeedCap = 15;
            if (Math.Abs(Projectile.velocity.X) > SpeedCap)
            {
                base.Projectile.velocity.X = 1 * (Projectile.velocity.X * .1f);
            }
            if (Math.Abs(Projectile.velocity.Y) > SpeedCap)
            {
                base.Projectile.velocity.Y = 1 * (Projectile.velocity.Y * .1f);
            }
            if (Math.Abs(Projectile.velocity.X) < SpeedCap)
            {
                base.Projectile.velocity.X = 1 * (Projectile.velocity.X * 1.5f);
            }
            if (Math.Abs(Projectile.velocity.Y) < SpeedCap)
            {
                base.Projectile.velocity.Y = 1 * (Projectile.velocity.Y * 1.5f);
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            {
                Texture2D starTexture2 = TextureAssets.Projectile[ModContent.ProjectileType<BouncingShard2>()].Value;
                Texture2D starTexture = TextureAssets.Projectile[ModContent.ProjectileType<BouncingShard2>()].Value;
                Vector2 drawPosition;
                for (int i = 1; i < base.Projectile.oldPos.Length; i++)
                {
                    float completionRatio = (float)i / (float)base.Projectile.oldPos.Length;
                    Color drawColor = Color.Lerp(lightColor, Color.LightPink, 2f);
                    drawColor = Color.Lerp(drawColor, Color.Purple, completionRatio);
                    drawColor = Color.Lerp(drawColor, Color.Transparent, completionRatio);
                    drawPosition = base.Projectile.oldPos[i] + base.Projectile.Size * 0.5f - Main.screenPosition;
                    Main.spriteBatch.Draw(starTexture, drawPosition, null, base.Projectile.GetAlpha(drawColor), 0, Utils.Size(starTexture) * 0.5f, base.Projectile.scale, SpriteEffects.None, 0f);
                }
                    Projectile.Rupeedraw(((Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Emerald/RupeeShard2").Value), lightColor, Color.GhostWhite, 1);
                    Projectile.RupeeGlowandFadedraw(((Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Emerald/RupeeShardMask2").Value), lightColor, Color.GhostWhite, 1);
                return false;
            }
        }
    }
}
