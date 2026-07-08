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
namespace SariaMod.Items
{
    public class FormChangeOverlay : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Saria");
            ProjectileID.Sets.TrailCacheLength[base.Projectile.type] = 7;
            ProjectileID.Sets.TrailingMode[base.Projectile.type] = 0;
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
            Projectile.Center = mother.Center;
            if (player.ownedProjectileCounts[ModContent.ProjectileType<Saria>()] == 1f)
            {
                Projectile.timeLeft = 200;
            }
            if (player.ownedProjectileCounts[ModContent.ProjectileType<Saria>()] <= 0f)
            {
                if (Main.myPlayer == Projectile.owner) SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/ZtargetCancel"), player.Center);
                Projectile.Kill();
            }
            bool Rightclick = (player.HeldItem.type == ModContent.ItemType<HealBall>() && Main.mouseLeft);
            for (int i = 0; i < 100; i++)
            {
                if (Main.projectile[i].active && Main.projectile[i].ModProjectile is Saria modProjectile && i != base.Projectile.whoAmI && ((Main.projectile[i].owner == owner)))
                {
                    if (modProjectile.ChangeForm <= 0)
                    {
                        Projectile.Kill();
                    }
                    if (modProjectile.ChangeForm >= 1 && !(player.HeldItem.type == ModContent.ItemType<HealBall>()))
                    {
                        if (Main.myPlayer == Projectile.owner) SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/ZtargetCancel"), player.Center);
                        modProjectile.ChangeForm = 0;
                        Projectile.Kill();
                    }
                    if (modProjectile.ChangeForm >= 1)
                    {
                        Vector2 mouse = Main.MouseWorld;
                        mouse.X += 10f;
                        mouse.Y -= 5f;
                        Vector2 startPos = player.Center;
                        startPos.Y += -175;
                        startPos.X += 0;
                        Vector2 startPos2 = player.Center;
                        startPos2.Y += -125;
                        startPos2.X -= 130;
                        Vector2 startPos3 = player.Center;
                        startPos3.Y += 0;
                        startPos3.X -= 175;
                        Vector2 startPos4 = player.Center;
                        startPos4.Y += 125;
                        startPos4.X -= 130;
                        Vector2 startPos5 = player.Center;
                        startPos5.Y += 175;
                        startPos5.X -= 0;
                        Vector2 startPos6 = player.Center;
                        startPos6.Y += 125;
                        startPos6.X += 130;
                        Vector2 startPos7 = player.Center;
                        startPos7.Y += 0;
                        startPos7.X += 175;
                        Vector2 startPos8 = player.Center;
                        startPos8.Y -= 125;
                        startPos8.X += 130;
                        float between = Vector2.Distance(mouse, startPos);
                        float between2 = Vector2.Distance(mouse, startPos2);
                        float between3 = Vector2.Distance(mouse, startPos3);
                        float between4 = Vector2.Distance(mouse, startPos4);
                        float between5 = Vector2.Distance(mouse, startPos5);
                        float between6 = Vector2.Distance(mouse, startPos6);
                        float between7 = Vector2.Distance(mouse, startPos7);
                        float between8 = Vector2.Distance(mouse, startPos8);
                        bool MouseOverAny = (((between < yup) || (between2 < yup) || (between3 < yup) || (between4 < yup) || (between5 < yup) || (between6 < yup) || (between7 < yup) || (between8 < yup)) && Main.myPlayer == Projectile.owner);
                        if (MouseOverAny == true && ChangeFormSelecter <= 0)
                        {
                            SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/MenuCursor"), player.Center);
                            ChangeFormSelecter = 1;
                        }
                        if (!MouseOverAny && ChangeFormSelecter >= 1)
                        {
                            ChangeFormSelecter--;
                        }
                        if (between < yup && Rightclick)
                        {
                            modProjectile.PendingTransform = 0;
                            modProjectile.TransformTimer = Saria.TransformDuration;
                            modProjectile.CantAttackTimer = 120;
                            SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/OptionSelect"), player.Center);
                            modProjectile.ChangeForm = 0;
                        }
                        if (between2 < yup && modPlayer.SariaUnlockWater == true && Rightclick)
                        {
                            modProjectile.PendingTransform = 1;
                            modProjectile.TransformTimer = Saria.TransformDuration;
                            modProjectile.CantAttackTimer = 120;
                            SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/OptionSelect"), player.Center);
                            modProjectile.ChangeForm = 0;
                        }
                        if (between3 < yup && modPlayer.SariaUnlockFire == true && Rightclick)
                        {
                            modProjectile.PendingTransform = 2;
                            modProjectile.TransformTimer = Saria.TransformDuration;
                            modProjectile.CantAttackTimer = 120;
                            SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/OptionSelect"), player.Center);
                            modProjectile.ChangeForm = 0;
                        }
                        if (between4 < yup && modPlayer.SariaUnlockElectric == true && Rightclick)
                        {
                            modProjectile.PendingTransform = 3;
                            modProjectile.TransformTimer = Saria.TransformDuration;
                            modProjectile.CantAttackTimer = 120;
                            SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/OptionSelect"), player.Center);
                            modProjectile.ChangeForm = 0;
                        }
                        if (between5 < yup && modPlayer.SariaUnlockRock == true && Rightclick)
                        {
                            modProjectile.PendingTransform = 4;
                            modProjectile.TransformTimer = Saria.TransformDuration;
                            modProjectile.CantAttackTimer = 120;
                            SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/OptionSelect"), player.Center);
                            modProjectile.ChangeForm = 0;
                        }
                        if (between6 < yup && modPlayer.SariaUnlockBug == true && Rightclick)
                        {
                            modProjectile.PendingTransform = 5;
                            modProjectile.TransformTimer = Saria.TransformDuration;
                            modProjectile.CantAttackTimer = 120;
                            SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/OptionSelect"), player.Center);
                            modProjectile.ChangeForm = 0;
                        }
                        if (between7 < yup && modPlayer.SariaUnlockGhost == true && Rightclick)
                        {
                            modProjectile.PendingTransform = 6;
                            modProjectile.TransformTimer = Saria.TransformDuration;
                            modProjectile.CantAttackTimer = 120;
                            SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/OptionSelect"), player.Center);
                            modProjectile.ChangeForm = 0;
                        }
                        if (between8 < yup && modPlayer.SariaUnlockFairy == true && Rightclick)
                        {
                            modProjectile.PendingTransform = 7;
                            modProjectile.TransformTimer = Saria.TransformDuration;
                            modProjectile.CantAttackTimer = 120;
                            SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/OptionSelect"), player.Center);
                            modProjectile.ChangeForm = 0;
                        }
                        else if (Rightclick && !modProjectile.SelectSound)
                        {
                            if (Main.myPlayer == Projectile.owner) SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/ZtargetCancel"), player.Center);
                            modProjectile.ChangeForm = 0;
                            Projectile.Kill();
                        }
                    }
                }
            }
            int VeilBubble = ModContent.ProjectileType<FormChangeOverlay>();
            for (int i = 0; i < 1000; i++)
            {
                float between = Vector2.Distance(Main.projectile[i].Center, Projectile.Center);
                {
                    if (Main.projectile[i].active && Main.projectile[i].whoAmI != base.Projectile.whoAmI && ((Main.projectile[i].type == VeilBubble && Main.projectile[i].owner == owner)))
                    {
                        Main.projectile[i].Kill();
                    }
                }
            }
        }
        public override void PostDraw(Color lightColor)
        {
            {
                Player player = Main.player[base.Projectile.owner];
                FairyPlayer modPlayer = player.Fairy();
                if (Main.myPlayer == Projectile.owner)
                {
                    Vector2 mouse = Main.MouseWorld;
                    mouse.X += 10f;
                    mouse.Y -= 5f;
                    Vector2 startPos1 = player.Center;
                    startPos1.Y += -175;
                    startPos1.X += 0;
                    Vector2 startPos2 = player.Center;
                    startPos2.Y += -125;
                    startPos2.X -= 130;
                    Vector2 startPos3 = player.Center;
                    startPos3.Y += 0;
                    startPos3.X -= 175;
                    Vector2 startPos4 = player.Center;
                    startPos4.Y += 125;
                    startPos4.X -= 130;
                    Vector2 startPos5 = player.Center;
                    startPos5.Y += 175;
                    startPos5.X -= 0;
                    Vector2 startPos6 = player.Center;
                    startPos6.Y += 125;
                    startPos6.X += 130;
                    Vector2 startPos7 = player.Center;
                    startPos7.Y += 0;
                    startPos7.X += 175;
                    Vector2 startPos8 = player.Center;
                    startPos8.Y -= 125;
                    startPos8.X += 130;
                    float between = Vector2.Distance(mouse, startPos1);
                    float between2 = Vector2.Distance(mouse, startPos2);
                    float between3 = Vector2.Distance(mouse, startPos3);
                    float between4 = Vector2.Distance(mouse, startPos4);
                    float between5 = Vector2.Distance(mouse, startPos5);
                    float between6 = Vector2.Distance(mouse, startPos6);
                    float between7 = Vector2.Distance(mouse, startPos7);
                    float between8 = Vector2.Distance(mouse, startPos8);
                    Projectile.FlatImageDraw(((Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/FormIcon").Value), lightColor, 130, -125);
                    Projectile.FlatImageDraw(((Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/FormIcon").Value), lightColor, 0, -175);
                    Projectile.FlatImageDraw(((Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/FormIcon").Value), lightColor, -130, -125);
                    Projectile.FlatImageDraw(((Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/FormIcon").Value), lightColor, -175, 0);
                    Projectile.FlatImageDraw(((Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/FormIcon").Value), lightColor, -130, 125);
                    Projectile.FlatImageDraw(((Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/FormIcon").Value), lightColor, 0, 175);
                    Projectile.FlatImageDraw(((Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/FormIcon").Value), lightColor, 130, 125);
                    Projectile.FlatImageDraw(((Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/FormIcon").Value), lightColor, 175, 0);
                    if (between < yup)
                    {
                        Projectile.VisualSetUpDraw(((Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/FormHighlight").Value), lightColor, 0, -175);
                        player.noThrow = 2;
                        player.cursorItemIconEnabled = true;
                        player.cursorItemIconID = ModContent.ItemType<Items.Bands.Blank>();
                        player.cursorItemIconText = (MiscUtilities.ColorMessage("Psyshock", new Color(135, 206, 180)));
                    }
                    if (between2 < yup)
                    {
                        Projectile.VisualSetUpDraw(((Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/FormHighlight").Value), lightColor, -130, -125);
                        if (modPlayer.SariaUnlockWater)
                        {
                            player.noThrow = 2;
                            player.cursorItemIconEnabled = true;
                            player.cursorItemIconID = ModContent.ItemType<Items.Bands.Blank>();
                            player.cursorItemIconText = (MiscUtilities.ColorMessage("AquaRing", new Color(135, 206, 180)));
                        }
                        else if (!modPlayer.SariaUnlockWater)
                        {
                            player.noThrow = 2;
                            player.cursorItemIconEnabled = true;
                            player.cursorItemIconID = ModContent.ItemType<Items.Bands.Blank>();
                            player.cursorItemIconText = (MiscUtilities.ColorMessage("???", new Color(135, 206, 180)));
                        }
                    }
                    if (between3 < yup)
                    {
                        Projectile.VisualSetUpDraw(((Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/FormHighlight").Value), lightColor, -175, 0);
                        if (modPlayer.SariaUnlockFire)
                        {
                            player.noThrow = 2;
                            player.cursorItemIconEnabled = true;
                            player.cursorItemIconID = ModContent.ItemType<Items.Bands.Blank>();
                            player.cursorItemIconText = (MiscUtilities.ColorMessage("Eruption", new Color(135, 206, 180)));
                        }
                        else if (!modPlayer.SariaUnlockFire)
                        {
                            player.noThrow = 2;
                            player.cursorItemIconEnabled = true;
                            player.cursorItemIconID = ModContent.ItemType<Items.Bands.Blank>();
                            player.cursorItemIconText = (MiscUtilities.ColorMessage("???", new Color(135, 206, 180)));
                        }
                    }
                    if (between4 < yup)
                    {
                        Projectile.VisualSetUpDraw(((Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/FormHighlight").Value), lightColor, -130, 125);
                        if (modPlayer.SariaUnlockElectric)
                        {
                            player.noThrow = 2;
                            player.cursorItemIconEnabled = true;
                            player.cursorItemIconID = ModContent.ItemType<Items.Bands.Blank>();
                            player.cursorItemIconText = (MiscUtilities.ColorMessage("ThunderBolt", new Color(135, 206, 180)));
                        }
                        else if (!modPlayer.SariaUnlockElectric)
                        {
                            player.noThrow = 2;
                            player.cursorItemIconEnabled = true;
                            player.cursorItemIconID = ModContent.ItemType<Items.Bands.Blank>();
                            player.cursorItemIconText = (MiscUtilities.ColorMessage("???", new Color(135, 206, 180)));
                        }
                    }
                    if (between5 < yup)
                    {
                        Projectile.VisualSetUpDraw(((Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/FormHighlight").Value), lightColor, 0, 175);
                        if (modPlayer.SariaUnlockRock)
                        {
                            player.noThrow = 2;
                            player.cursorItemIconEnabled = true;
                            player.cursorItemIconID = ModContent.ItemType<Items.Bands.Blank>();
                            player.cursorItemIconText = (MiscUtilities.ColorMessage("PowerGem", new Color(135, 206, 180)));
                        }
                        else if (!modPlayer.SariaUnlockRock)
                        {
                            player.noThrow = 2;
                            player.cursorItemIconEnabled = true;
                            player.cursorItemIconID = ModContent.ItemType<Items.Bands.Blank>();
                            player.cursorItemIconText = (MiscUtilities.ColorMessage("???", new Color(135, 206, 180)));
                        }
                    }
                    if (between6 < yup)
                    {
                        Projectile.VisualSetUpDraw(((Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/FormHighlight").Value), lightColor, 130, 125);
                        if (modPlayer.SariaUnlockBug)
                        {
                            player.noThrow = 2;
                            player.cursorItemIconEnabled = true;
                            player.cursorItemIconID = ModContent.ItemType<Items.Bands.Blank>();
                            player.cursorItemIconText = (MiscUtilities.ColorMessage("AttackOrder", new Color(135, 206, 180)));
                        }
                        else if (!modPlayer.SariaUnlockBug)
                        {
                            player.noThrow = 2;
                            player.cursorItemIconEnabled = true;
                            player.cursorItemIconID = ModContent.ItemType<Items.Bands.Blank>();
                            player.cursorItemIconText = (MiscUtilities.ColorMessage("???", new Color(135, 206, 180)));
                        }
                    }
                    if (between7 < yup)
                    {
                        Projectile.VisualSetUpDraw(((Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/FormHighlight").Value), lightColor, 175, 0);
                        if (modPlayer.SariaUnlockGhost)
                        {
                            player.noThrow = 2;
                            player.cursorItemIconEnabled = true;
                            player.cursorItemIconID = ModContent.ItemType<Items.Bands.Blank>();
                            player.cursorItemIconText = (MiscUtilities.ColorMessage("ShadowBall", new Color(135, 206, 180)));
                        }
                        else if (!modPlayer.SariaUnlockGhost)
                        {
                            player.noThrow = 2;
                            player.cursorItemIconEnabled = true;
                            player.cursorItemIconID = ModContent.ItemType<Items.Bands.Blank>();
                            player.cursorItemIconText = (MiscUtilities.ColorMessage("???", new Color(135, 206, 180)));
                        }
                    }
                    if (between8 < yup)
                    {
                        Projectile.VisualSetUpDraw(((Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/FormHighlight").Value), lightColor, 130, -125);
                        if (modPlayer.SariaUnlockFairy)
                        {
                            player.noThrow = 2;
                            player.cursorItemIconEnabled = true;
                            player.cursorItemIconID = ModContent.ItemType<Items.Bands.Blank>();
                            player.cursorItemIconText = (MiscUtilities.ColorMessage("MoonBlast", new Color(135, 206, 180)));
                        }
                        else if (!modPlayer.SariaUnlockFairy)
                        {
                            player.noThrow = 2;
                            player.cursorItemIconEnabled = true;
                            player.cursorItemIconID = ModContent.ItemType<Items.Bands.Blank>();
                            player.cursorItemIconText = (MiscUtilities.ColorMessage("???", new Color(135, 206, 180)));
                        }
                    }
                    Projectile.FrameChargedraw((TextureAssets.Projectile[ModContent.ProjectileType<PinkCharge>()].Value), lightColor, false, false, 0, -175);
                    if (modPlayer.SariaUnlockWater)
                    {
                        Projectile.FrameChargedraw((TextureAssets.Projectile[ModContent.ProjectileType<BlueCharge>()].Value), lightColor, false, true, -130, -125);
                    }
                    if (!modPlayer.SariaUnlockWater)
                    {
                        Projectile.FrameChargedraw((TextureAssets.Projectile[ModContent.ProjectileType<GreyCharge2>()].Value), lightColor, false, true, -130, -125);
                    }
                    if (modPlayer.SariaUnlockFire)
                    {
                        Projectile.FrameChargedraw((TextureAssets.Projectile[ModContent.ProjectileType<RedCharge>()].Value), lightColor, false, false, -175, 0);
                    }
                    if (!modPlayer.SariaUnlockFire)
                    {
                        Projectile.FrameChargedraw((TextureAssets.Projectile[ModContent.ProjectileType<GreyCharge>()].Value), lightColor, false, false, -175, 0);
                    }
                    if (modPlayer.SariaUnlockElectric)
                    {
                        Projectile.FrameChargedraw((TextureAssets.Projectile[ModContent.ProjectileType<YellowCharge>()].Value), lightColor, false, false, -130, 125);
                    }
                    if (!modPlayer.SariaUnlockElectric)
                    {
                        Projectile.FrameChargedraw((TextureAssets.Projectile[ModContent.ProjectileType<GreyCharge>()].Value), lightColor, false, false, -130, 125);
                    }
                    if (modPlayer.SariaUnlockRock)
                    {
                        Projectile.FrameChargedraw((TextureAssets.Projectile[ModContent.ProjectileType<GreenCharge>()].Value), lightColor, false, false, 0, 175);
                    }
                    if (!modPlayer.SariaUnlockRock)
                    {
                        Projectile.FrameChargedraw((TextureAssets.Projectile[ModContent.ProjectileType<GreyCharge>()].Value), lightColor, false, false, 0, 175);
                    }
                    if (modPlayer.SariaUnlockBug)
                    {
                        Projectile.FrameChargedraw((TextureAssets.Projectile[ModContent.ProjectileType<OrangeCharge>()].Value), lightColor, false, true, 130, 125);
                    }
                    if (!modPlayer.SariaUnlockBug)
                    {
                        Projectile.FrameChargedraw((TextureAssets.Projectile[ModContent.ProjectileType<GreyCharge2>()].Value), lightColor, false, true, 130, 125);
                    }
                    if (modPlayer.SariaUnlockGhost)
                    {
                        Projectile.FrameChargedraw((TextureAssets.Projectile[ModContent.ProjectileType<PurpleCharge>()].Value), lightColor, false, true, 175, 0);
                    }
                    if (!modPlayer.SariaUnlockGhost)
                    {
                        Projectile.FrameChargedraw((TextureAssets.Projectile[ModContent.ProjectileType<GreyCharge2>()].Value), lightColor, false, true, 175, 0);
                    }
                    if (modPlayer.SariaUnlockFairy)
                    {
                        Projectile.FrameChargedraw((TextureAssets.Projectile[ModContent.ProjectileType<FairyCharge>()].Value), lightColor, false, true, 130, -125);
                    }
                    if (!modPlayer.SariaUnlockFairy)
                    {
                        Projectile.FrameChargedraw((TextureAssets.Projectile[ModContent.ProjectileType<GreyCharge2>()].Value), lightColor, false, true, 130, -125);
                    }
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
