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
    public class ProjectileBurn : ModProjectile
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
            for (int i = 0; i < 1000; i++)
                if (Main.projectile[i].active && i != base.Projectile.whoAmI && Main.projectile[i].Hitbox.Intersects(base.Projectile.Hitbox) && Main.projectile[i].active && ((!Main.projectile[i].friendly && Main.projectile[i].hostile) || (Main.projectile[i].trap)))
                {
                    Main.projectile[i].Kill();
                }
        }
        public override void Kill(int timeLeft)
        {
            Player player = Main.player[Projectile.owner];
            FairyPlayer modPlayer = player.Fairy();
            int owner = player.whoAmI;
            for (int i = 0; i < 50; i++)
            {
                Vector2 dustspeed5 = Main.rand.NextVector2CircularEdge(1.6f, 1.6f);
                Dust d = Dust.NewDustPerfect(player.Center, ModContent.DustType<ShadowFlameDustRing>(), dustspeed5 * -5, Scale: 4.5f);
                d.noGravity = true;
            }
            SoundEngine.PlaySound(SoundID.Item20, base.Projectile.Center);
            if (Main.rand.NextBool(11))
            {
                SoundEngine.PlaySound(SoundID.NPCDeath52, base.Projectile.Center);
            }
            else
            {
                SoundEngine.PlaySound(SoundID.NPCDeath6, base.Projectile.Center);
            }
            if (player.ownedProjectileCounts[ModContent.ProjectileType<WillOWisp>()] >= 1f)
            {
                for (int U = 0; U < 1000; U++)
                {
                    if (Main.projectile[U].active && Main.projectile[U].ModProjectile is WillOWisp modRupee && U != Projectile.whoAmI && ((Main.projectile[U].owner == owner)))
                    {
                        modRupee.WispHits -= 1;
                    }
                }
            }
        }
    }
}
