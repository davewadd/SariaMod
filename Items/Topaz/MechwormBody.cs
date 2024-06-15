
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using SariaMod.Buffs;
using System.IO;
using Terraria.GameContent;
using SariaMod.Items.Strange;
using SariaMod.Items.Sapphire;
using SariaMod.Items.Ruby;
using SariaMod.Items.Topaz;
using SariaMod.Items.Emerald;
using SariaMod.Items.Amber;
using SariaMod.Items.Amethyst;
using Terraria.Audio;
using SariaMod.Dusts;

namespace SariaMod.Items.Topaz
{
    public class MechwormBody : ModProjectile
    {
        public new string LocalizationCategory => "Projectiles.Summon";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            Main.projFrames[base.Projectile.type] = 4;
            ProjectileID.Sets.NeedsUUID[Projectile.type] = true;
        }
       
        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 0;
            Projectile.minionSlots = 0.5f;
            Projectile.scale *= 2f;
            Projectile.timeLeft = 500;
            Projectile.penetrate = -1;
            Projectile.minion = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
            Projectile.DamageType = DamageClass.Summon;
        }
        public int Freakout;
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            Freakout = (int)reader.ReadInt32();
        }
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(Freakout);
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.velocity.Y = 0;
            Projectile.velocity.X = 0;
            return false;
        }
        internal static bool SameIdentity(Projectile proj, int owner, int identity)
        {
            return proj.owner == owner && (proj.projUUID == identity || proj.identity == identity);
        }

        internal static void SegmentAI(Projectile projectile, int offsetFromNextSegment, ref int playerMinionSlots)
        {
          
           
                Lighting.AddLight(projectile.Center, Color.Yellow.ToVector3());

            Player owner = Main.player[projectile.owner];
            FairyPlayer modPlayer = owner.Fairy();

            
           
            int headProjType = ModContent.ProjectileType<MechwormHead>();
            int bodyProjType = ModContent.ProjectileType<MechwormBody>();

            ref float segmentAheadIdentity = ref projectile.ai[0];
            Projectile segmentAhead = Main.projectile.Take(Main.maxProjectiles).FirstOrDefault(proj => SameIdentity(proj, projectile.owner, (int)projectile.ai[0]));

            
            if (segmentAhead is null || !Main.projectile.IndexInRange(segmentAhead.whoAmI) || (segmentAhead.type != bodyProjType && segmentAhead.type != headProjType))
            {
                projectile.Kill();
                return;
            }

         
            if (playerMinionSlots != -1 && (owner.maxMinions < playerMinionSlots || !owner.active))
            {
                int lostSlots = playerMinionSlots - owner.maxMinions;
                while (lostSlots > 0)
                {
                    Projectile ahead = segmentAhead;
                  
                    for (int i = 0; i < 2; ++i)
                    {
                        if (ahead.type != ModContent.ProjectileType<MechwormHead>())
                            projectile.localAI[1] = ahead.localAI[1];

                
                        segmentAheadIdentity = ahead.ai[0];
                        projectile.netUpdate = true;

                        ahead.Kill();

               
                        segmentAhead = Main.projectile.Take(Main.maxProjectiles).FirstOrDefault(proj => SameIdentity(proj, projectile.owner, (int)projectile.ai[0]));

                      
                        if (segmentAhead is null || !Main.projectile.IndexInRange(segmentAhead.whoAmI))
                        {
                            projectile.Kill();
                            return;
                        }
                        ahead = segmentAhead;
                    }
                    lostSlots--;
                }
                playerMinionSlots = owner.maxMinions;
            }

         
            segmentAhead.localAI[0] = projectile.localAI[0] + 1f;

            Projectile head = LocateHead(projectile);

            {

                if (head is null)
                {
                    projectile.Kill();
                    return;
                }
                else if (head != null && projectile.timeLeft < 100)
                {
                    projectile.timeLeft = 100;
                }
            }
            if (head.netUpdate)
            {
                projectile.netUpdate = true;
                if (projectile.netSpam > 59)
                    projectile.netSpam = 59;
            }

            projectile.extraUpdates = head.extraUpdates;

            
           

            projectile.velocity = Vector2.Zero;
            Vector2 offsetToDestination = segmentAhead.Center - projectile.Center;

            if (segmentAhead.rotation != projectile.rotation)
            {
                float offsetAngle = MathHelper.WrapAngle(segmentAhead.rotation - projectile.rotation);
                if (projectile.timeLeft >= 250)
                {
                    offsetToDestination = offsetToDestination.RotatedBy(offsetAngle * 3f);
                }
                if (projectile.timeLeft < 250)
                {
                    offsetToDestination = offsetToDestination.RotatedBy(offsetAngle * 1.8f);
                }
            }
            projectile.rotation = offsetToDestination.ToRotation() + MathHelper.PiOver2;

     
            if (offsetToDestination != Vector2.Zero)
                projectile.Center = segmentAhead.Center - offsetToDestination.SafeNormalize(Vector2.Zero) * offsetFromNextSegment;

            projectile.Center = Vector2.Clamp(projectile.Center, new Vector2(160f), new Vector2(Main.maxTilesX - 10, Main.maxTilesY - 10) * 16);
        }

        public static Projectile LocateHead(Projectile projectile)
        {
            int headType = ModContent.ProjectileType<MechwormHead>();
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                if (Main.projectile[i].type != headType || !Main.projectile[i].active || Main.projectile[i].owner != projectile.owner)
                    continue;
                return Main.projectile[i];
            }
            return null;
        }

        public override void AI()
        {
            Player player = Main.player[base.Projectile.owner];
            int _ = 1;
            SegmentAI(Projectile, 58, ref _);
            Projectile.tileCollide = true;
            base.Projectile.frameCounter++;
            if (base.Projectile.frameCounter >= 2)
            {
                base.Projectile.frame++;
                base.Projectile.frameCounter = 0;

            }
            if (base.Projectile.frame >= Main.projFrames[base.Projectile.type])
            {
                base.Projectile.frame = 0;

            }
        }

       
       

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindProjectiles.Add(index);
        }

        public override bool ShouldUpdatePosition() => false;
    }
}
