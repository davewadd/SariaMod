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
    public class RupeeXPassive3 : ModProjectile
    {
        public bool Mustbreak;
        public int Damage;
        public int Cooldown;
        public bool IsActive;
        public bool HasClicked;
        public int HasClickedTimer;
        public bool Soundtimer;
        public bool ChangeSoundtimer;
        public bool ChangeSoundtimer2;
        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Saria");
            ProjectileID.Sets.TrailingMode[base.Projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[base.Projectile.type] = 8;
            Main.projFrames[base.Projectile.type] = 3;
            Main.projPet[Projectile.type] = true;
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = false;
            ProjectileID.Sets.MinionShot[Projectile.type] = false;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
        }
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(Mustbreak);
            writer.Write(Damage);
            writer.Write(Cooldown);
            writer.Write(IsActive);
            writer.Write(HasClicked);
            writer.Write(HasClickedTimer);
            writer.Write(Soundtimer);
            writer.Write(Projectile.frame);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            Mustbreak = (bool)reader.ReadBoolean();
            Damage = (int)reader.ReadInt32();
            Cooldown = (int)reader.ReadInt32();
            IsActive = (bool)reader.ReadBoolean();
            HasClicked = (bool)reader.ReadBoolean();
            HasClickedTimer = (int)reader.ReadInt32();
            Soundtimer = (bool)reader.ReadBoolean();
            Projectile.frame = (int)reader.ReadInt32();
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
            damage /= 2;
        }
        public override void SetDefaults()
        {
            base.Projectile.width = 30;
            base.Projectile.height = 30;
            base.Projectile.alpha = 300;
            base.Projectile.friendly = true;
            base.Projectile.tileCollide = false;
            base.Projectile.penetrate = 1;
            base.Projectile.timeLeft = 20;
            base.Projectile.ignoreWater = true;
            Projectile.netUpdate = true;
            base.Projectile.usesLocalNPCImmunity = true;
            base.Projectile.localNPCHitCooldown = 4;
        }
        public override bool? CanHitNPC(NPC target)
        {
            return false;
        }
        public override void AI()
        {
            Player player = Main.player[base.Projectile.owner];
            int owner = player.whoAmI;
            int Spot = -40;
            Vector2 idlePosition = player.Center;
            if (player.ownedProjectileCounts[ModContent.ProjectileType<RupeeXPassive>()] > 0f)
            {
                Spot -= 40;
            }
            if (player.ownedProjectileCounts[ModContent.ProjectileType<RupeeXPassive2>()] > 0f)
            {
                Spot -= 40;
            }
            idlePosition.X += (Spot * player.direction) + 5;
            Vector2 direction = idlePosition - Projectile.Center;
            Projectile.velocity = (((Projectile.velocity * (10) + direction) / 16));
            float overlapVelocity = .8f;
            Vector2 targetCenter = Projectile.position;
            bool foundTarget = false;
            base.Projectile.rotation += 0.075f;
            Projectile.friendly = foundTarget;
            Projectile.SneezeDust(ModContent.DustType<RockSparkle>(), 30, 12, -4, -10, 0);
            ///
            if (player.ownedProjectileCounts[ModContent.ProjectileType<Saria>()] > 0f)
            {
                Projectile.SariaBaseDamage();
            }
            if (player.ownedProjectileCounts[ModContent.ProjectileType<Saria>()] <= 0f)
            {
                Projectile.damage = 30;
            }
            if (player.ownedProjectileCounts[ModContent.ProjectileType<Change1_3>()] > 0f)
            {
                for (int U = 0; U < 1000; U++)
                {
                    if (Main.projectile[U].active && Main.projectile[U].ModProjectile is RupeeXPassive modRupee && U != Projectile.whoAmI && ((Main.projectile[U].owner == owner)))
                    {
                        IsActive = true;
                        modRupee.IsActive = false;
                        if (!ChangeSoundtimer)
                        {
                            SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/SilverRupee3"), Projectile.Center);
                            for (int i = 0; i < 50; i++)
                            {
                                Vector2 speed = Main.rand.NextVector2CircularEdge(1f, 1f);
                                Dust d = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<RupeePick>(), speed * 5, Scale: 2.7f);
                                d.noGravity = true;
                            }
                            ChangeSoundtimer = true;
                        }
                    }
                    if (Main.projectile[U].active && Main.projectile[U].ModProjectile is RupeeXPassive2 modRupee2 && U != Projectile.whoAmI && ((Main.projectile[U].owner == owner)))
                        {
                            IsActive = true;
                            modRupee2.IsActive = false;
                            if (!ChangeSoundtimer)
                            {
                                SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/SilverRupee3"), Projectile.Center);
                                for (int i = 0; i < 50; i++)
                                {
                                    Vector2 speed = Main.rand.NextVector2CircularEdge(1f, 1f);
                                    Dust d = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<RupeePick>(), speed * 5, Scale: 2.7f);
                                    d.noGravity = true;
                                }
                                ChangeSoundtimer = true;
                            }
                        }
                }
            }
            if (player.ownedProjectileCounts[ModContent.ProjectileType<Change1_3>()] > 0f)
            {
                IsActive = true;
                if (!ChangeSoundtimer)
                {
                    SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/SilverRupee3"), Projectile.Center);
                    for (int i = 0; i < 50; i++)
                    {
                        Vector2 speed = Main.rand.NextVector2CircularEdge(1f, 1f);
                        Dust d = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<RupeePick>(), speed * 5, Scale: 2.7f);
                        d.noGravity = true;
                    }
                    ChangeSoundtimer = true;
                }
            }
            if (player.ownedProjectileCounts[ModContent.ProjectileType<Change1_3>()] <= 0f && ChangeSoundtimer)
            {
                ChangeSoundtimer = false;
            }
            if (player.ownedProjectileCounts[ModContent.ProjectileType<Change2_3>()] > 0f && !ChangeSoundtimer2)
            {
                IsActive = false;
                if (!ChangeSoundtimer2)
                {
                    SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Close"), Projectile.Center);
                    for (int i = 0; i < 50; i++)
                    {
                        Vector2 speed = Main.rand.NextVector2CircularEdge(1f, 1f);
                        Dust d = Dust.NewDustPerfect(Projectile.Center - speed * 60, ModContent.DustType<RupeePick>(), speed * 5, Scale: 2.7f);
                        d.noGravity = true;
                    }
                    ChangeSoundtimer2 = true;
                }
            }
            if (player.ownedProjectileCounts[ModContent.ProjectileType<Change2_3>()] <= 0f && ChangeSoundtimer2)
            {
                ChangeSoundtimer2 = false;
            }
            ///
            if (Damage <= 20)
            {
                Projectile.frame = 0;
            }
            if (Damage > 20 && Damage <= 30)
            {
                Projectile.frame = 1;
            }
            if (Damage > 30)
            {
                Projectile.frame = 2;
            }
            if (Cooldown >= 1 && (player.ownedProjectileCounts[ModContent.ProjectileType<Emeraldspike3>()] <= 0f))
            {
                Cooldown--;
            }
            if (Cooldown == 1 && (Main.myPlayer == Projectile.owner))
            {
                for (int i = 0; i < 50; i++)
                {
                    Vector2 dustspeed5 = Main.rand.NextVector2CircularEdge(1f, 1f);
                    Dust d = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<RockDust2>(), dustspeed5 * -5, Scale: 1.5f);
                    d.noGravity = true;
                }
                SoundEngine.PlaySound(SoundID.MaxMana, Projectile.Center);
            }
            if (IsActive && player.controlDown && player.releaseJump && Cooldown <= 0 && (player.ownedProjectileCounts[ModContent.ProjectileType<Emeraldspike3>()] <= 0f))
            {
                if (Main.myPlayer == Projectile.owner && Damage <= 20) Projectile.NewProjectile(Projectile.GetSource_FromThis(), player.Center.X + 0, player.Center.Y - 60, 0, 0, ModContent.ProjectileType<BufferProj>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), player.Center.X + 0, player.Center.Y - 60, 0, 0, ModContent.ProjectileType<Emeraldspike3>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                player.velocity.X = 0 * player.direction;
                Cooldown = 60;
            }
            if (player.dead || !player.active)
            {
                if (Main.rand.Next(4) == 0)
                {
                    Item.NewItem(Projectile.GetSource_FromThis(), Projectile.Center + Utils.RandomVector2(Main.rand, -24f, 24f), ModContent.ItemType<LivingSilverShard>());
                }
                for (int i = 0; i < 50; i++)
                {
                    Vector2 speed = Main.rand.NextVector2CircularEdge(1f, 1f);
                    Dust d = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<RockDustRing>(), speed * -6, Scale: 2.7f);
                    d.noGravity = true;
                }
                SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact, Projectile.Center);
                Lighting.AddLight(Projectile.Center, Color.Purple.ToVector3() * 6f);
                Projectile.Kill();
            }
            if (player.HasBuff(ModContent.BuffType<EmeraldBuff>()) && Projectile.timeLeft >= 10)
            {
                Projectile.timeLeft = 11;
            }
            if (!player.HasBuff(ModContent.BuffType<EmeraldBuff>()) && Projectile.timeLeft <= 3)
            {
                Projectile.Kill();
            }
            if (Mustbreak && Projectile.timeLeft >= 10)
            {
                Projectile.Kill();
            }
        }
        public override void PostDraw(Color lightColor)
        {
            Player player = Main.player[base.Projectile.owner];
            int yup = 20;
            Vector2 mouse = Main.MouseWorld;
            mouse.X += 5f;
            mouse.Y -= 5f;
            Vector2 NearRupee = Projectile.Center;
            float between = Vector2.Distance(mouse, NearRupee);
            if (Main.mouseLeft && HasClicked == false)
            {
                HasClickedTimer++;
            }
            if (HasClickedTimer >= 20)
            {
                HasClicked = true;
            }
            if (!Main.mouseLeft)
            {
                HasClicked = false;
                HasClickedTimer = 0;
            }
            if (between <= yup && Main.myPlayer == Projectile.owner)
            {
                Projectile.Rupeedraw(((Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Emerald/RupeeHighlight").Value), lightColor, Color.GhostWhite, 3);
                if (HasClickedTimer < 20)
                {
                    player.mouseInterface = true;
                    {
                        if (!IsActive && Main.mouseLeft && HasClicked == false)
                        {
                            HasClicked = true;
                            if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center.X + 0, Projectile.Center.Y - 0, 0, 0, ModContent.ProjectileType<Change1_3>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                        }
                        if (IsActive && Main.mouseLeft && HasClicked == false)
                        {
                            HasClicked = true;
                            if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center.X + 0, Projectile.Center.Y - 0, 0, 0, ModContent.ProjectileType<Change2_3>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                        }
                    }
                    if (Soundtimer)
                    {
                        SoundEngine.PlaySound(SoundID.MenuTick);
                        Soundtimer = false;
                    }
                }
            }
            if (between > yup)
            {
                Soundtimer = true;
            }
            if (IsActive)
            {
                Texture2D texture = ((Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Emerald/RupeeXPassive3").Value);
                Vector2 startPos = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
                int frameHeight = texture.Height / Main.projFrames[Projectile.type];
                int frameY = frameHeight * Projectile.frame;
                Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                Vector2 origin = rectangle.Size() / 2f;
                float rotation = Projectile.rotation;
                float scale = Projectile.scale;
                SpriteEffects spriteEffects = SpriteEffects.None;
                Color drawColor = lightColor;
                for (int i = 1; i < base.Projectile.oldPos.Length; i++)
                {
                    startPos = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition;
                    startPos.Y += 0;
                    startPos.X += -5;
                    float completionRatio = (float)i / (float)Projectile.oldPos.Length;
                    drawColor = Color.Lerp(drawColor, drawColor, completionRatio);
                    drawColor = Color.Lerp(drawColor, Color.Transparent, completionRatio);
                    Main.spriteBatch.Draw(texture, startPos, rectangle, (drawColor), rotation, origin, scale * 0.9f, spriteEffects, 0f);
                }
            }
            Projectile.Rupeedraw(((Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Emerald/RupeeXPassive3").Value), lightColor, Color.GhostWhite, 3);
            Projectile.RupeeGlowandFadedraw(((Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Emerald/RupeeMask3").Value), lightColor, Color.GhostWhite, 3);
        }
        public override void Kill(int timeLeft)
        {
            Player player = Main.player[Projectile.owner];
            player.AddBuff(ModContent.BuffType<SilverRupeeBlock>(), 3200);
            for (int i = 0; i < 50; i++)
            {
                Vector2 speed = Main.rand.NextVector2CircularEdge(.5f, .5f);
                Dust d = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<RockDustRing>(), speed * -6, Scale: 2.7f);
                d.noGravity = true;
            }
            SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact, Projectile.Center);
            Lighting.AddLight(Projectile.Center, Color.Purple.ToVector3() * 6f);
            if (Main.rand.Next(4) == 0 && Main.myPlayer == Projectile.owner)
            {
                int dropItemType = ModContent.ItemType<Items.Emerald.LivingSilverShard>(); // This the item we want the paper airplane to drop.
                int newItem = Item.NewItem(Projectile.GetSource_DropAsItem(), Projectile.Hitbox, dropItemType); // Create a new item in the world.
                Main.item[newItem].noGrabDelay = 50; // Set the new item to be able to be picked up instantly
                // Here we need to make sure the item is synced in multiplayer games.
                if (Main.netMode == NetmodeID.MultiplayerClient && newItem >= 0)
                {
                    NetMessage.SendData(MessageID.SyncItem, -1, -1, null, newItem, 1f);
                }
            }
            else if (Main.myPlayer == Projectile.owner)
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
            int owner = player.whoAmI;
            if (player.ownedProjectileCounts[ModContent.ProjectileType<RupeeXPassive>()] >= 1f)
            {
                for (int U = 0; U < 1000; U++)
                {
                    if (Main.projectile[U].active && Main.projectile[U].ModProjectile is RupeeXPassive modRupee && U != Projectile.whoAmI && ((Main.projectile[U].owner == owner)))
                    {
                        if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center.X + 0, Projectile.Center.Y - 0, 0, 0, ModContent.ProjectileType<Change1>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                    }
                }
            }
            else if (player.ownedProjectileCounts[ModContent.ProjectileType<RupeeXPassive2>()] >= 1f)
            {
                for (int U = 0; U < 1000; U++)
                {
                    if (Main.projectile[U].active && Main.projectile[U].ModProjectile is RupeeXPassive2 modRupee && U != Projectile.whoAmI && ((Main.projectile[U].owner == owner)))
                    {
                        if (Main.myPlayer == Projectile.owner) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center.X + 0, Projectile.Center.Y - 0, 0, 0, ModContent.ProjectileType<Change1_2>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
                    }
                }
            }
        }
    }
}
