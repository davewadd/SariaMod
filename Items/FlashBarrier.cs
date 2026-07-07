using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using SariaMod.Buffs;
using SariaMod.Dusts;
using SariaMod.Items.Amber;
using SariaMod.Items.Amethyst;
using SariaMod.Items.Bands;
using SariaMod.Items.Emerald;
using SariaMod.Items.Ruby;
using SariaMod.Items.Sapphire;
using SariaMod.Items.Topaz;
using SariaMod.Items.zPearls;
using SariaMod.Items.zTalking;
using Terraria.Localization;
using System;
using Terraria.Map;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace SariaMod.Items
{
    public class FlashBarrier : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Saria");
            ProjectileID.Sets.TrailCacheLength[base.Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[base.Projectile.type] = 0;
            Main.projFrames[base.Projectile.type] = 1;
        }
        public override bool? CanCutTiles()
        {
            return false;
        }
        public override bool? CanHitNPC(NPC target)
        {
            {
                return false;
            }
        }
        public override void SetDefaults()
        {
            base.Projectile.width = 260;
            base.Projectile.height = 260;
            base.Projectile.alpha = 260;
            base.Projectile.friendly = true;
            base.Projectile.tileCollide = false;
            base.Projectile.penetrate = -1;
            base.Projectile.timeLeft = 250;
            base.Projectile.ignoreWater = true;
            base.Projectile.usesLocalNPCImmunity = true;
            base.Projectile.localNPCHitCooldown = 20;
        }
        public override void AI()
        {
            Player player = Main.player[base.Projectile.owner];
            Lighting.AddLight(base.Projectile.Center, 0f, 0.5f, 0f);
            Projectile.Center = player.Center;
            for (int i = 0; i < 1000; i++)
            {
                if (Main.projectile[i].active && i != base.Projectile.whoAmI && Main.projectile[i].Hitbox.Intersects(base.Projectile.Hitbox) && Main.projectile[i].active && ((!Main.projectile[i].friendly && Main.projectile[i].hostile) || (Main.projectile[i].trap)))
                {
                    for (int o = 0; o < 20; o++)
                    {
                        Vector2 speed2 = Main.rand.NextVector2CircularEdge(.5f, .5f);
                        Dust d = Dust.NewDustPerfect(Main.projectile[i].Center, ModContent.DustType<PsychicRingDust>(), speed2 * 15, Scale: 4f);
                        SoundEngine.PlaySound(SoundID.Item60, Main.projectile[i].Center);
                        SoundEngine.PlaySound(SoundID.Item56, Main.projectile[i].Center);
                        d.noGravity = true;
                    }
                }
            }
        }
    }
}
