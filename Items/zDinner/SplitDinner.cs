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
namespace SariaMod.Items.zDinner
{
    public class SplitDinner : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Ma boi");
            Main.projFrames[base.Projectile.type] = 1;
            ProjectileID.Sets.MinionShot[base.Projectile.type] = true;
        }
        public bool HasHealed = false;
        public bool HasExploded = false;
        public override void SetDefaults()
        {
            base.Projectile.width = 20;
            base.Projectile.height = 10;
            base.Projectile.netImportant = true;
            base.Projectile.friendly = true;
            base.Projectile.ignoreWater = true;
            base.Projectile.usesLocalNPCImmunity = true;
            base.Projectile.localNPCHitCooldown = 70;
            base.Projectile.minionSlots = 0f;
            base.Projectile.extraUpdates = 1;
            base.Projectile.penetrate = -1;
            Projectile.tileCollide = true;
            base.Projectile.timeLeft = 2600;
            Projectile.scale *= 2;
        }
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(HasHealed);
            writer.Write(HasExploded);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            HasHealed = (bool)reader.ReadBoolean();
            HasExploded = (bool)reader.ReadBoolean();
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
            // If the projectile hits the floor while falling...
            if (oldVelocity.Y > 0)
            {
                if (!HasExploded)
                {
                    // Spawn the new projectile.
                    // Replace <ChildProjectileType> with the ModContent.ProjectileType of your desired projectile.
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<DinnerBomb>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
                    // Set the flag to true so it doesn't fire again.
                    HasExploded = true;
                    Projectile.netUpdate = true; // Tell other clients that the flag has changed.
                }
                // ...stop its movement, but only the vertical movement.
                Projectile.velocity.Y = 0;
                Projectile.velocity.X = 0;
                // This is a flag to stop the gravity effect in AI().
                // We set tileCollide to false so the projectile stops moving entirely.
                Projectile.tileCollide = false;
                SoundEngine.PlaySound(SoundID.Item49, Projectile.Center);
                return false; // Prevents the projectile from being destroyed
            }
            // If the projectile hits a wall from the side...
            else if (oldVelocity.X != 0)
            {
                // ...just stop its horizontal movement and let gravity continue pulling it down.
                Projectile.velocity.X = 0;
                // You can play a different sound here if you wish, or just let it go.
                // SoundEngine.PlaySound(SoundID.Dig, Projectile.Center);
            }
            return false; // Prevent destruction
        }
        public override bool? CanDamage()
        {
            return false;
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            Player player2 = Main.LocalPlayer;
            Lighting.AddLight(Projectile.Center, Color.White.ToVector3() * .8f);
            int Yesh = ((player2.statManaMax2) / 8);
            int Yesh2 = ((player2.statManaMax2) / 5);
            if (!HasHealed)
            {
                for (int i = 0; i < 100; i++)
                {
                    Player player3 = Main.player[i];
                    if (((Main.player[i].statLife < Main.player[i].statLifeMax2) && Main.player[i].Hitbox.Intersects(Projectile.Hitbox) && Main.player[i].active && Main.player[i] != player && !Main.player[i].HasBuff(ModContent.BuffType<Healed>()) && (Main.player[i].team == player.team)))
                    {
                        if ((Main.player[i].statLife < Main.player[i].statLifeMax2) || (Main.player[i].statMana < Main.player[i].statManaMax2))
                        {
                            Main.player[i].AddBuff(ModContent.BuffType<Healed>(), 30);
                            if (!player.HasBuff(ModContent.BuffType<Overcharged>()))
                            {
                                for (int g = 0; g < 50; g++)
                                {
                                    Vector2 speed = Main.rand.NextVector2CircularEdge(1f, 1f);
                                    Dust d = Dust.NewDustPerfect(Main.player[i].Center, ModContent.DustType<Healdust3>(), speed * 2, Scale: 2.1f);
                                    d.noGravity = true;
                                }
                                SoundEngine.PlaySound(SoundID.DD2_DarkMageHealImpact, base.Projectile.Center);
                                Projectile.netUpdate = true;
                                Main.player[i].statMana += Yesh;
                                Main.player[i].ManaEffect(Yesh);
                                Main.player[i].Heal((player.statLifeMax2 / 16));
                                HasHealed = true;
                                Projectile.Kill();
                            }
                        }
                    }
                }
                if (player.Hitbox.Intersects(Projectile.Hitbox) && player.active && !player.HasBuff(ModContent.BuffType<Healed>()))
                {
                    {
                        if ((player.statLife < player.statLifeMax2) || (player.statMana < player.statManaMax2))
                        {
                            player.AddBuff(ModContent.BuffType<Healed>(), 30);
                            if (!player.HasBuff(ModContent.BuffType<Overcharged>()))
                            {
                                for (int i = 0; i < 50; i++)
                                {
                                    Vector2 speed = Main.rand.NextVector2CircularEdge(1f, 1f);
                                    Dust d = Dust.NewDustPerfect(player2.Center, ModContent.DustType<Healdust3>(), speed * 2, Scale: 2.1f);
                                    d.noGravity = true;
                                }
                                SoundEngine.PlaySound(SoundID.DD2_DarkMageHealImpact, base.Projectile.Center);
                                Projectile.netUpdate = true;
                                player.statMana += Yesh;
                                player.ManaEffect(Yesh);
                                player.Heal((player.statLifeMax2 / 15));
                                HasHealed = true;
                                Projectile.Kill();
                            }
                        }
                    }
                }
            }
            if (Projectile.tileCollide)
            {
                Projectile.velocity.Y += 0.5f; // Adjust this value to change the fall speed
                if (Projectile.velocity.Y > 16f) // Cap the max falling speed
                {
                    Projectile.velocity.Y = 16f;
                }
            }
            else
            {
                // If tileCollide is off (because it hit the floor), stop all movement
                Projectile.velocity = Vector2.Zero;
            }
        }
    }
}
