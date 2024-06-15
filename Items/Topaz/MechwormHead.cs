
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using SariaMod.Items.Sapphire;
using SariaMod.Items.Ruby;
using SariaMod.Items.Topaz;
using SariaMod.Items.Emerald;
using SariaMod.Items.Amber;
using SariaMod.Items.Amethyst;

using SariaMod.Items.Platinum;
using SariaMod.Items.zPearls;
using System.Linq;
using SariaMod.Items.zBookcases;
using SariaMod.Items.Strange;
using SariaMod.Buffs;
using Terraria.DataStructures;

namespace SariaMod.Items.Topaz
{
    public class MechwormHead : ModProjectile
    {
       

      
        private static Vector2 WorldTopLeft(int tileDist = 15) => new Vector2(tileDist * 16f);
        private static Vector2 WorldBottomRight(int tileDist = 15) => new Vector2(Main.maxTilesX - tileDist, Main.maxTilesY - tileDist) * 16f;

        public int CanCollide;
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            CanCollide = (int)reader.ReadInt32();
        }
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(CanCollide);
        }
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            Main.projFrames[base.Projectile.type] = 4;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
            ProjectileID.Sets.NeedsUUID[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.netImportant = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 500;
            Projectile.alpha = 0;
            Projectile.scale *= 2f;
            Projectile.tileCollide = false;
            Projectile.minion = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
            Projectile.DamageType = DamageClass.Summon;
        }

        #region Syncing

        #endregion

        #region AI
        public override void AI()
        {

            Player player = Main.player[base.Projectile.owner];
            
            Lighting.AddLight(Projectile.Center, Color.White.ToVector3());


            Projectile.Center = Vector2.Clamp(Projectile.Center, WorldTopLeft(10), WorldBottomRight(10));

            Player owner = Main.player[Projectile.owner];
            FairyPlayer modPlayer = owner.Fairy();
            float thisnumber = 250;
            if (Projectile.timeLeft > thisnumber)
            {
                Projectile.velocity.X = 0.05f;
            }
            if (Projectile.timeLeft > thisnumber)
            {
                Projectile.velocity.Y = 0.05f;
            }
            if (Projectile.timeLeft > thisnumber)
            {
                Projectile.alpha = 300;
            }
            if (Projectile.timeLeft < thisnumber)
            {
                Projectile.alpha = 0;
            }
            if (Projectile.timeLeft < (thisnumber + 50))
            {
                Projectile.tileCollide = true;
            }
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
           



           

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            int previousDirection = Projectile.direction;
            Projectile.direction = Projectile.spriteDirection = (Projectile.velocity.X > 0f).ToDirectionInt();

            if (previousDirection != Projectile.direction)
            {
                Projectile.netUpdate = true;
                if (Projectile.netSpam > 59)
                    Projectile.netSpam = 59;
            }












        }
      










        #endregion



    }
}
