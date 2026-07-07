using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SariaMod.Buffs;
using SariaMod.Dusts;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace SariaMod.Items.Ruby
{
    public class ProjectileBurn2 : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Saria");
            ProjectileID.Sets.TrailCacheLength[base.Projectile.type] = 7;
            ProjectileID.Sets.TrailingMode[base.Projectile.type] = 0;
            Main.projFrames[base.Projectile.type] = 4;
        }
        public override void SetDefaults()
        {
            base.Projectile.width = 150;
            base.Projectile.height = 150;
            base.Projectile.alpha = 300;
            base.Projectile.friendly = true;
            base.Projectile.tileCollide = false;
            base.Projectile.netImportant = true;
            base.Projectile.penetrate = -1;
            base.Projectile.timeLeft = 1;
            base.Projectile.ignoreWater = true;
            Projectile.scale *= 1.6f;
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
        public override void AI()
        {
            Player player = Main.player[base.Projectile.owner];
            Player player2 = Main.LocalPlayer;
            Projectile.Center = player.Center;
            Lighting.AddLight(Projectile.Center, Color.Purple.ToVector3() * 1.5f);
        }
        public override void Kill(int timeLeft)
        {
            Player player = Main.player[Projectile.owner];
            FairyPlayer modPlayer = player.Fairy();
            int owner = player.whoAmI;
            if (player.ownedProjectileCounts[ModContent.ProjectileType<WillOWisp>()] >= 1f)
            {
                for (int U = 0; U < 1000; U++)
                {
                    if (Main.projectile[U].active && Main.projectile[U].ModProjectile is WillOWisp modRupee && U != Projectile.whoAmI && ((Main.projectile[U].owner == owner)))
                    {
                        modRupee.WispHits -= 3;
                    }
                }
            }
        }
    }
}
