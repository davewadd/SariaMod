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
using SariaMod.Gores;
namespace SariaMod.Items.zDinner
{
    public class DinnerBomb : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Ma boi");
            Main.projFrames[base.Projectile.type] = 1;
            ProjectileID.Sets.MinionShot[base.Projectile.type] = true;
        }
        public override void SetDefaults()
        {
            base.Projectile.width = 150;
            base.Projectile.height = 150;
            Projectile.alpha = 300;
            base.Projectile.netImportant = true;
            base.Projectile.friendly = true;
            base.Projectile.ignoreWater = true;
            base.Projectile.usesLocalNPCImmunity = true;
            base.Projectile.localNPCHitCooldown = 70;
            base.Projectile.minionSlots = 0f;
            base.Projectile.extraUpdates = 1;
            base.Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            base.Projectile.timeLeft = 13;
            Projectile.scale *= 2;
            Projectile.knockBack = 20f;
        }
        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            Player player = Main.player[Projectile.owner];
            FairyPlayer modPlayer = player.Fairy();
            if (target.position.X + (float)(target.width / 2) > player.Center.X + (float)(Projectile.width / 2))
            {
                hitDirection = 1;
            }
            else
            {
                hitDirection = -1;
            }
        }
        public override bool? CanCutTiles()
        {
            return false;
        }
      
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            Lighting.AddLight(Projectile.Center, Color.White.ToVector3() * .8f);
            if (Projectile.timeLeft <= 2)
            {
                for (int i = 0; i < 20; i++)
                {
                    int rand = Main.rand.Next(3); // Get a random number from 0 to 2
                    int goreType;
                    if (rand == 0)
                    {
                        goreType = ModContent.GoreType<MessyDinner1>();
                    }
                    else if (rand == 1)
                    {
                        goreType = ModContent.GoreType<MessyDinner2>();
                    }
                    else // rand == 2
                    {
                        goreType = ModContent.GoreType<MessyDinner3>();
                    }
                        Vector2 speed = Main.rand.NextVector2CircularEdge(.75f, .75f);
                        Gore B = Gore.NewGorePerfect(Projectile.GetSource_FromThis(), Projectile.Center, speed * 10, goreType, 3f);
                        B.light = .5f;
                        SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/KinglyWhack"), Projectile.Center);
                    Projectile.netUpdate = true;
                        Projectile.Kill();
                }
            }
                Projectile.velocity.Y = 0;
                Projectile.velocity.X = 0;
        }
    }
}
