using Microsoft.Xna.Framework;
using SariaMod.Items.Strange;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
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
using Terraria.UI;
using SariaMod;
using Terraria.GameContent.UI.Elements;
using ReLogic.Content;
using Terraria.DataStructures;
namespace SariaMod.Items.Emerald
{
    public class RupeeXNeutral3 : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Saria");
            ProjectileID.Sets.TrailingMode[base.Projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[base.Projectile.type] = 30;
            Main.projFrames[base.Projectile.type] = 1;
        }
        public override bool? CanCutTiles()
        {
            return false;
        }
        public override bool? CanHitNPC(NPC target)
        {
            return false;
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
        }
        public override void SetDefaults()
        {
            base.Projectile.width = 30;
            base.Projectile.height = 30;
            base.Projectile.alpha = 300;
            base.Projectile.friendly = true;
            base.Projectile.tileCollide = false;
            base.Projectile.penetrate = 1;
            base.Projectile.timeLeft = 250;
            base.Projectile.ignoreWater = true;
            Projectile.netUpdate = true;
            base.Projectile.usesLocalNPCImmunity = true;
            base.Projectile.localNPCHitCooldown = 4;
        }
        public override void AI()
        {
            Player player = Main.player[base.Projectile.owner];
            Lighting.AddLight(Projectile.Center, Color.Purple.ToVector3() * (2f));
            int owner = player.whoAmI;
            int Spot = 40;
            Vector2 idlePosition = player.Center;
            idlePosition.X += (Spot * player.direction) + 5;
            Vector2 direction = idlePosition - Projectile.Center;
            Projectile.velocity = (((Projectile.velocity * (10) + direction) / 16));
            base.Projectile.rotation += 0.075f;
            float overlapVelocity = .8f;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                // Fix overlap with other minions
                Projectile other = Main.projectile[i];
                if (i != Projectile.whoAmI && other.active && other.owner == Projectile.owner && Main.projectile[i].type == Projectile.type && Math.Abs(Projectile.position.X - other.position.X) < (Projectile.width * 40))
                {
                    if (Projectile.position.X < other.position.X) Projectile.velocity.X -= overlapVelocity;
                    else Projectile.velocity.X += overlapVelocity;
                }
            }
        }
        public override void PostDraw(Color lightColor)
        {
            Projectile.Rupeedraw(((Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Emerald/RupeeXNeutral3").Value), lightColor, Color.GhostWhite, 1);
        }
        public override void Kill(int timeLeft)
        {
            Player player = Main.player[base.Projectile.owner];
            if (Main.myPlayer == Projectile.owner)
            {
                int dropItemType = ModContent.ItemType<Items.Emerald.LivingSilverFragment>(); // This the item we want the paper airplane to drop.
                int newItem = Item.NewItem(Projectile.GetSource_DropAsItem(), Projectile.Hitbox, dropItemType); // Create a new item in the world.
                Main.item[newItem].noGrabDelay = 0; // Set the new item to be able to be picked up instantly
                if (Main.rand.Next(101) <= 60)
                {
                    Main.item[newItem].stack = 2;
                }
                else
                {
                    Main.item[newItem].stack = 1;
                }
                SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact, Projectile.Center);
                Lighting.AddLight(Projectile.Center, Color.Green.ToVector3() * 6f);
                for (int i = 0; i < 50; i++)
                {
                    Vector2 speed = Main.rand.NextVector2CircularEdge(1f, 1f);
                    Dust d = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<RockDustRing>(), speed * -6, Scale: 2.7f);
                    d.noGravity = true;
                }
                // Here we need to make sure the item is synced in multiplayer games.
                if (Main.netMode == NetmodeID.MultiplayerClient && newItem >= 0)
                {
                    NetMessage.SendData(MessageID.SyncItem, -1, -1, null, newItem, 1f);
                }
            }
            if (player.ownedProjectileCounts[ModContent.ProjectileType<RupeeXPassive3>()] <= 0f)
            {
                if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), player.Center.X - 50, player.Center.Y + 150, 0, 0, ModContent.ProjectileType<RupeeXPassive3>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                player.AddBuff(ModContent.BuffType<EmeraldBuff>(), 40);
                Projectile.timeLeft = 2;
            }
            else if (player.ownedProjectileCounts[ModContent.ProjectileType<RupeeXPassive3>()] > 0f)
            {
                SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact, Projectile.Center);
                Lighting.AddLight(Projectile.Center, Color.Purple.ToVector3() * 6f);
                for (int i = 0; i < 3; i++)
                {
                    if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X + 0, Projectile.position.Y - 0, 0, 0, ModContent.ProjectileType<RupeeShard3>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                }
                for (int i = 0; i < 50; i++)
                {
                    Vector2 speed = Main.rand.NextVector2CircularEdge(1f, 1f);
                    Dust d = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<RockDustRing>(), speed * -6, Scale: 2.7f);
                    d.noGravity = true;
                }
            }
        }
    }
}
