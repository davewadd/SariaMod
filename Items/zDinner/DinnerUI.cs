using Terraria.Localization;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using SariaMod.Items.Strange;
using SariaMod.Buffs;
namespace SariaMod.Items.zDinner
{
    public class DinnerUI : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("King's Dinner");
        }
        public int ChangeFormSelecter;
        private int yup = 30;
        public override void SetDefaults()
        {
            base.Projectile.width = 30;
            base.Projectile.height = 30;
            base.Projectile.alpha = 0;
            base.Projectile.friendly = true;
            base.Projectile.tileCollide = false;
            base.Projectile.netImportant = true;
            base.Projectile.penetrate = -1;
            base.Projectile.timeLeft = 1500;
            base.Projectile.ignoreWater = true;
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
        public override bool MinionContactDamage()
        {
            return false;
        }
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(ChangeFormSelecter);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            ChangeFormSelecter = (int)reader.ReadInt32();
        }
        public override void AI()
        {
            Player player = Main.player[base.Projectile.owner];
            Projectile mother = Main.projectile[(int)base.Projectile.ai[1]];
            FairyPlayer modPlayer = player.Fairy();
            int owner = player.whoAmI;
            int VeilBubble = ModContent.ProjectileType<DinnerUI>();
            for (int i = 0; i < 1000; i++)
            {
                    if (Main.projectile[i].active && Main.projectile[i].whoAmI != base.Projectile.whoAmI && ((Main.projectile[i].type == VeilBubble && Main.projectile[i].owner == owner)))
                    {
                        Main.projectile[i].Kill();
                    }
            }
            if (player.HeldItem.type == ModContent.ItemType<KingsDinner>())
            {
                Projectile.timeLeft = 30;
            }
            if (!(player.HeldItem.type == ModContent.ItemType<KingsDinner>()) || !player.active)
            {
                if (Main.myPlayer == Projectile.owner) SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/ZtargetCancel"), player.Center);
                Projectile.Kill();
            }
            Projectile.Center = player.Center;
            bool Rightclick = (player.HeldItem.type == ModContent.ItemType<KingsDinner>() && Main.mouseLeft);
                Vector2 mouse = Main.MouseWorld;
                mouse.X += 10f;
                mouse.Y -= 5f;
                Vector2 startPos = player.Center;
                startPos.Y += -125;
                startPos.X += 130;
                Vector2 startPos2 = player.Center;
                startPos2.Y += -125;
                startPos2.X -= 130;
                float between = Vector2.Distance(mouse, startPos);
                float between2 = Vector2.Distance(mouse, startPos2);
                bool MouseOverAny = (((between < yup) || (between2 < yup)) && Main.myPlayer == Projectile.owner);
                if (MouseOverAny == true && ChangeFormSelecter <= 0)
                {
                    SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/MenuCursor"), player.Center);
                    ChangeFormSelecter = 1;
                }
                if (!MouseOverAny && ChangeFormSelecter >= 1)
                {
                    ChangeFormSelecter--;
                }
                if (between < yup && Rightclick && modPlayer.Serving >= 100)
                {
                player.AddBuff(ModContent.BuffType<TriforceofCourage>(), 3000);
                SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/DinnerProtectMe"), player.Center);
                modPlayer.Serving = 0;
                Projectile.Kill();
            }
                if (between2 < yup && Rightclick && modPlayer.Serving >= 100)
                {
                player.AddBuff(ModContent.BuffType<TriforceofPower>(), 3000);
                SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/CantGiveMercy"), player.Center);
                modPlayer.Serving = 0;
                Projectile.Kill();
            }
            else if (Rightclick && ChangeFormSelecter <= 0)
            {
                if (Main.myPlayer == Projectile.owner) SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/ZtargetCancel"), player.Center);
                Projectile.Kill();
            }
        }
        public override void PostDraw(Color lightColor)
        {
            Player player = Main.player[base.Projectile.owner];
            FairyPlayer modPlayer = player.Fairy();
            if (Main.myPlayer == Projectile.owner)
            {
                Vector2 mouse = Main.MouseWorld;
                mouse.X += 10f;
                mouse.Y -= 5f;
                Vector2 startPos1 = player.Center;
                startPos1.Y += -125;
                startPos1.X += 130;
                Vector2 startPos2 = player.Center;
                startPos2.Y += -125;
                startPos2.X -= 130;
                float between = Vector2.Distance(mouse, startPos1);
                float between2 = Vector2.Distance(mouse, startPos2);
                Projectile.FlatImageDraw(((Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/FormIcon").Value), lightColor, 130, -125);
                Projectile.FlatImageDraw(((Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/FormIcon").Value), lightColor, -130, -125);
                if (between < yup)
                {
                    Projectile.VisualSetUpDraw(((Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/FormHighlight").Value), lightColor, 130, -125);
                    if (modPlayer.Serving >= 100)
                    {
                        player.noThrow = 2;
                        player.cursorItemIconEnabled = true;
                        player.cursorItemIconID = ModContent.ItemType<Items.Bands.Blank>();
                        player.cursorItemIconText = (SariaModUtilities.ColorMessage("Defense and Size", new Color(135, 206, 180)));
                    }
                    else if (modPlayer.Serving <= 99)
                    {
                        player.noThrow = 2;
                        player.cursorItemIconEnabled = true;
                        player.cursorItemIconID = ModContent.ItemType<Items.Bands.Blank>();
                        player.cursorItemIconText = (SariaModUtilities.ColorMessage("Come back when you have more Dinner!", new Color(135, 206, 180)));
                    }
                }
                if (between2 < yup)
                {
                    Projectile.VisualSetUpDraw(((Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/FormHighlight").Value), lightColor, -130, -125);
                    if (modPlayer.Serving >= 100)
                    {
                        player.noThrow = 2;
                        player.cursorItemIconEnabled = true;
                        player.cursorItemIconID = ModContent.ItemType<Items.Bands.Blank>();
                        player.cursorItemIconText = (SariaModUtilities.ColorMessage("Attack and Speed", new Color(135, 206, 180)));
                    }
                    else if (modPlayer.Serving <= 99)
                    {
                        player.noThrow = 2;
                        player.cursorItemIconEnabled = true;
                        player.cursorItemIconID = ModContent.ItemType<Items.Bands.Blank>();
                        player.cursorItemIconText = (SariaModUtilities.ColorMessage("Come back when you have more Dinner!", new Color(135, 206, 180)));
                    }
                }
                if (modPlayer.Serving >= 100)
                {
                    Projectile.FrameChargedraw((TextureAssets.Projectile[ModContent.ProjectileType<BlueCharge>()].Value), lightColor, false, true, 130, -125);
                }
                if (modPlayer.Serving <= 99)
                {
                    Projectile.FrameChargedraw((TextureAssets.Projectile[ModContent.ProjectileType<GreyCharge2>()].Value), lightColor, false, true, 130, -125);
                }
                if (modPlayer.Serving >= 100)
                {
                    Projectile.FrameChargedraw((TextureAssets.Projectile[ModContent.ProjectileType<BlueCharge>()].Value), lightColor, false, true, -130, -125);
                }
                if (modPlayer.Serving <= 99)
                {
                    Projectile.FrameChargedraw((TextureAssets.Projectile[ModContent.ProjectileType<GreyCharge2>()].Value), lightColor, false, true, -130, -125);
                }
            }
        }
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            overPlayers.Add(index);
            overWiresUI.Add(index);
        }
    }
}