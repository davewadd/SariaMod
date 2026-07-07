using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SariaMod.Buffs;
using SariaMod.Dusts;
using SariaMod.Items.Strange;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
namespace SariaMod.Items.Sapphire
{
    public class Bubble2 : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Saria");
            Main.projFrames[base.Projectile.type] = 1;
            ProjectileID.Sets.MinionShot[base.Projectile.type] = true;
            ProjectileID.Sets.TrailingMode[base.Projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[base.Projectile.type] = 30;
        }
        private double rotation;
        private double rotationVariation;
        public override void SetDefaults()
        {
            base.Projectile.width = 34;
            base.Projectile.height = 34;
            base.Projectile.netImportant = true;
            base.Projectile.friendly = false;
            base.Projectile.ignoreWater = true;
            base.Projectile.usesLocalNPCImmunity = true;
            base.Projectile.localNPCHitCooldown = 7;
            base.Projectile.minionSlots = 0f;
            base.Projectile.extraUpdates = 1;
            Projectile.alpha = 100;
            base.Projectile.penetrate = 9;
            base.Projectile.tileCollide = false;
            base.Projectile.timeLeft = 3000;
        }
        public override bool? CanCutTiles()
        {
            return false;
        }
        public override bool MinionContactDamage()
        {
            Player player = Main.player[base.Projectile.owner];
            FairyPlayer modPlayer = player.Fairy();
            NPC target = base.Projectile.Center.MinionHoming(500f, player);
            if (Projectile.frame <= 2)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private const int sphereRadius2 = 6;
        public float OffsetAngle { get; private set; }
        public float Time { get; private set; }
        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            Player player = Main.player[Projectile.owner];
            Player player2 = Main.LocalPlayer;
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
            target.buffImmune[ModContent.BuffType<Frostburn2>()] = false;
            target.AddBuff(ModContent.BuffType<Frostburn2>(), 200);
            FairyPlayer modPlayer = player.Fairy();
            modPlayer.SariaXp++;
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + new Vector2(-250f, 370f), Vector2.One.RotatedByRandom(6.2831854820251465) * 4f, ModContent.ProjectileType<HealBubble>(), (int)(Projectile.damage), 0f, Projectile.owner, player.whoAmI, base.Projectile.whoAmI);
            {
                float radius = (float)Math.Sqrt(Main.rand.Next(34 * 34));
                double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                Dust.NewDust(new Vector2(Projectile.Center.X + radius * (float)Math.Cos(angle), Projectile.Center.Y + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<Water>(), 0f, 0f, 0, default(Color), 1.5f);
            }//end of dust stuff
            {
                float radius = (float)Math.Sqrt(Main.rand.Next(34 * 34));
                double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                for (int j = 0; j < 10; j++) //set to 2
                {
                    Dust.NewDust(new Vector2(Projectile.Center.X + radius * (float)Math.Cos(angle), Projectile.Center.Y + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<BubbleDust>(), 0f, 0f, 0, default(Color), 1.5f);
                }
            }
            knockback /= 2;
            SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Bubblepop"));
            if (Main.rand.NextBool(10))
            {
                float radius = (float)Math.Sqrt(Main.rand.Next(34 * 34));
                double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
            }
            if (player.HasBuff(ModContent.BuffType<StatRaise>()))
            {
                damage = (damage);
            }
            if (player.HasBuff(ModContent.BuffType<StatLower>()))
            {
                damage /= 4;
            }
            else
            {
                damage -= (damage) / 2;
            }
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            FairyPlayer modPlayer = player.Fairy();
            Player player2 = Main.LocalPlayer;
            Projectile mother = Main.projectile[(int)base.Projectile.ai[1]];
            Projectile.SariaBaseDamage();
            if (Main.rand.NextBool(200))
            {
                float radius = (float)Math.Sqrt(Main.rand.Next(34 * 34));
                double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                {
                    Dust.NewDust(new Vector2(Projectile.Center.X + radius * (float)Math.Cos(angle), Projectile.Center.Y + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<BubbleDust>(), 0f, 0f, 0, default(Color), 1.5f);
                }
            }
            if (Main.rand.NextBool(20))
            {
                float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius2 * sphereRadius2));
                double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                Dust.NewDust(new Vector2((Projectile.Center.X) + radius * (float)Math.Cos(angle), (Projectile.Center.Y) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<Cold>(), 0f, 0f, 0, default(Color), 1.5f);
            }
            if (Main.rand.NextBool(20))
            {
                float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius2 * sphereRadius2));
                double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                Dust.NewDust(new Vector2((Projectile.Center.X) + radius * (float)Math.Cos(angle), (Projectile.Center.Y) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<Snow2>(), 0f, 0f, 0, default(Color), 1.5f);
            }
            {
                float between = Vector2.Distance(player2.Center, Projectile.Center);
                // Reasonable distance away so it doesn't target across multiple screens
                if (between < 1000f)
                {
                    player2.AddBuff(BuffID.Regeneration, 30);
                }
            }
            if (player.ownedProjectileCounts[ModContent.ProjectileType<Saria>()] >= 1)
            {
                Projectile.timeLeft = 1000;
            }
            //16
            int SpinSpeed = 16;
            int owner = player.whoAmI;
            Vector2 This = player.Center;
            if (player.ownedProjectileCounts[ModContent.ProjectileType<Bubble>()] > 0f)
            {
                int VeilBubble = ModContent.ProjectileType<Bubble>();
                for (int i = 0; i < 1000; i++)
                {
                    if (Main.projectile[i].active && ((Main.projectile[i].type == VeilBubble && Main.projectile[i].owner == owner)))
                    {
                        {
                            float radius = (float)Math.Sqrt(Main.rand.Next(34 * 34));
                            double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                            if (Main.rand.NextBool(5))
                            {
                                Dust.NewDust(new Vector2(Projectile.Center.X + radius * (float)Math.Cos(angle), Projectile.Center.Y + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<BubbleDust>(), 0f, 0f, 0, default(Color), 1.5f);
                            }
                            This = Main.projectile[i].Center;
                            Projectile.penetrate = 12;
                            if (Main.projectile[i].timeLeft <= 10)
                            {
                                Projectile.Kill();
                            }
                        }
                    }
                    Projectile.netUpdate = true;
                }
            }
            if (player.HasBuff(ModContent.BuffType<StatRaise>()))
            {
                SpinSpeed = 30;
            }
            if (player.HasBuff(ModContent.BuffType<StatLower>()))
            {
                SpinSpeed = 11;
            }
            base.Projectile.Center = This + Utils.RotatedBy(new Vector2(100f, 0f), rotation);
            rotation += SpinSpeed + rotationVariation;
            // If your minion is flying, you want to do this independently of any conditions
            Projectile.scale = 1.5f;
            bool foundTarget = true;
            if (Projectile.timeLeft < 20)
            {
                SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/Bubblepop"));
            }
            // friendly needs to be set to true so the minion can deal contact damage
            // friendly needs to be set to false so it doesn't damage things like target dummies while idling
            // Both things depend on if it has a target or not, so it's just one assignment here
            // You don't need this assignment if your minion is shooting things instead of dealing contact damage
            Projectile.friendly = foundTarget;
            if (Main.rand.NextBool(9000))
            {
                SoundEngine.PlaySound(SoundID.Drown, base.Projectile.Center);
            }
            Lighting.AddLight(Projectile.Center, new Color(0, 0, Main.DiscoB).ToVector3() * 2f);
            // Default movement parameters (here for attacking)
        }
        public override void PostDraw(Color lightColor)
        {
            Player player = Main.player[base.Projectile.owner];
            FairyPlayer modPlayer = player.Fairy();
            {
                Vector2 drawPosition;
                for (int i = 1; i < 30; i++)
                {
                    Texture2D texture = (Texture2D)ModContent.Request<Texture2D>("SariaMod/Items/Sapphire/Bubble2");
                    Vector2 startPos = base.Projectile.oldPos[i] + base.Projectile.Size * 0.5f - Main.screenPosition;
                    int frameHeight = texture.Height / Main.projFrames[base.Projectile.type];
                    int frameY = frameHeight * base.Projectile.frame;
                    float completionRatio = (float)i / (float)base.Projectile.oldPos.Length;
                    Color drawColor = Color.Lerp(lightColor, Color.LightPink, 20f);
                    drawColor = Color.Lerp(drawColor, Color.DarkViolet, completionRatio);
                    drawColor = Color.Lerp(drawColor, Color.Transparent, completionRatio);
                    Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
                    Vector2 origin = rectangle.Size() / 2f;
                    float rotation = base.Projectile.rotation;
                    float scale = base.Projectile.scale;
                    SpriteEffects spriteEffects = SpriteEffects.None;
                    startPos.Y += 1;
                    startPos.X += +17;
                    Main.spriteBatch.Draw(texture, startPos, rectangle, base.Projectile.GetAlpha(drawColor), rotation, origin, scale, spriteEffects, layerDepth: 0f);
                }
            }
        }
    }
}
