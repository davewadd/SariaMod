using Microsoft.Xna.Framework;
using System.Linq;
using System.Reflection;
using System.Text;
using Terraria;
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.GameContent.Events;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ID;
using System;
using System.IO;
using Terraria.ModLoader;
using Terraria.ObjectData;
using SariaMod.Items;
using SariaMod.Buffs;
using SariaMod.Items.Strange;
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
using Terraria.Audio;
using Terraria.UI;
using SariaMod;
using Terraria.GameContent.UI.Elements;
using ReLogic.Content;
using Terraria.DataStructures;
namespace SariaMod
{
    public static class SariaCombatExtensions
    {
        private static readonly Dictionary<int, List<RovaLavaGlob>> ChargeGlobVortices = new();

        public static void SariaBaseDamage(Projectile projectile)
        {
            Player player = Main.player[projectile.owner];
            FairyPlayer modPlayer = player.Fairy();
            if (modPlayer.Sarialevel == 6)
            {
                projectile.damage = 900 + (modPlayer.SariaXp / 20);
                projectile.netUpdate = true;
            }
            else if (modPlayer.Sarialevel == 5)
            {
                projectile.damage = 200 + (modPlayer.SariaXp / 342);
                projectile.netUpdate = true;
            }
            else if (modPlayer.Sarialevel == 4)
            {
                projectile.damage = 75 + (modPlayer.SariaXp / 640);
                projectile.netUpdate = true;
            }
            else if (modPlayer.Sarialevel == 3)
            {
                projectile.damage = 50 + (modPlayer.SariaXp / 1600);
                projectile.netUpdate = true;
            }
            else if (modPlayer.Sarialevel == 2)
            {
                projectile.damage = 26 + (modPlayer.SariaXp / 833);
                projectile.netUpdate = true;
            }
            else if (modPlayer.Sarialevel == 1)
            {
                projectile.damage = 15 + (modPlayer.SariaXp / 818);
                projectile.netUpdate = true;
            }
            else
            {
                projectile.damage = 10 + (modPlayer.SariaXp / 600);
                projectile.netUpdate = true;
            }
            if (player.HasBuff(ModContent.BuffType<StatRaise>()))
            {
                projectile.damage += (projectile.damage) / 4;
            }
            else if (player.HasBuff(ModContent.BuffType<StatLower>()))
            {
                projectile.damage /= 2;
            }
        }

        public static void SariaBiomeEffectivness(Projectile projectile, int biometime, int transform)
        {
            if (Main.myPlayer != projectile.owner) return;
            Player player = Main.player[projectile.owner];
            Vector2 UpWardPosition = projectile.Center;
            bool NoCover = !projectile.HasCover();
            bool scarfImmune = player.GetModPlayer<FairyPlayer>().SoftStepShimmerImmune;
            if (!(projectile.ModProjectile is Saria saria)) return;
            if (biometime <= 0f)
            {
                if (transform == 0)
                {
                    if (!scarfImmune && (saria.SariaZoneCrimson || saria.SariaZoneCorrupt || saria.SariaZoneUnderworld || saria.SariaZoneGraveyard || saria.SariaZoneDungeon || saria.SariaHasReajCandle))
                    {
                        projectile.SariaStatLower();
                    }
                    if ((saria.SariaZoneSpace || saria.SariaZoneGlowingMushroom || saria.SariaZoneJungle || saria.SariaHasPeaceCandle || saria.SariaZoneHallow || saria.SariaHasCalmMindCandle || (saria.SariaZoneBeach && !Main.dayTime)) && (!player.HasBuff(ModContent.BuffType<StatLower>())))
                    {
                        projectile.SariaStatRaise();
                    }
                }
                if (transform == 1)
                {
                    if (saria.SariaZoneSnow && !saria.SariaHasCampfire && !player.HasBuff(BuffID.Warmth))
                    {
                        player.AddBuff(ModContent.BuffType<Frostburn2>(), 2);
                    }
                    if ((saria.SariaZoneRain && !saria.SariaZoneSpace && NoCover) || (Collision.WetCollision(projectile.position, projectile.width / 2, projectile.height / 2) && !Collision.LavaCollision(projectile.position, projectile.width / 2, projectile.height / 2)) || (projectile.IsUnderThunderCloud()) || projectile.IsTouchingWaterBarrier())
                    {
                        player.AddBuff(ModContent.BuffType<PassiveHealing>(), 2);
                    }
                    if (!scarfImmune && (saria.SariaZoneDesert || saria.SariaZoneJungle || saria.SariaZoneGlowingMushroom || saria.SariaZoneSnow))
                    {
                        projectile.SariaStatLower();
                    }
                    if ((saria.SariaZoneUnderworld || saria.SariaZoneRain || saria.SariaZoneBeach || saria.SariaZoneMeteor || saria.SariaHasWaterCandle) && (!player.HasBuff(ModContent.BuffType<StatLower>())))
                    {
                        projectile.SariaStatRaise();
                    }
                }
                if (transform == 2)
                {
                    if ((Collision.WetCollision(projectile.position, projectile.width / 2, projectile.height / 2)) && (!Collision.LavaCollision(projectile.position, projectile.width / 2, projectile.height / 2)))
                    {
                        player.AddBuff(ModContent.BuffType<Extinguished>(), 20);
                    }
                    if (((saria.SariaZoneRain && !saria.SariaZoneSpace && NoCover && !saria.SariaZoneSnow)) || projectile.IsUnderThunderCloud() || projectile.IsTouchingWaterBarrier())
                    {
                        player.AddBuff(ModContent.BuffType<Extinguished>(), 20);
                    }
                    if (player.ZoneUnderworldHeight && !player.HasBuff(ModContent.BuffType<Veil>()) && Vector2.Distance(player.Center, projectile.Center) <= 200f)
                    {
                        player.AddBuff(ModContent.BuffType<Burning2>(), 20);
                    }
                    if (!scarfImmune && (saria.SariaZoneBeach || (saria.SariaZoneRain && !saria.SariaZoneSnow) || saria.SariaZoneSandstorm))
                    {
                        projectile.SariaStatLower();
                    }
                    if ((saria.SariaZoneSnow || saria.SariaZoneGlowingMushroom || saria.SariaZoneUnderworld || saria.SariaZoneJungle || saria.SariaZoneDungeon || saria.SariaZoneHallow) && (!player.HasBuff(ModContent.BuffType<StatLower>())))
                    {
                        projectile.SariaStatRaise();
                    }
                }
                if (transform == 3)
                {
                    if (!scarfImmune && (saria.SariaZoneUndergroundDesert || saria.SariaZoneUnderworld || saria.SariaZoneRockLayer || saria.SariaZoneDirtLayer || saria.SariaZoneUnderground))
                    {
                        projectile.SariaStatLower();
                    }
                    if ((saria.SariaZoneBeach || saria.SariaZoneRain || saria.SariaZoneSkyHeight) && (!player.HasBuff(ModContent.BuffType<StatLower>())))
                    {
                        projectile.SariaStatRaise();
                    }
                }
                if (transform == 4)
                {
                    if (!scarfImmune && (saria.SariaZoneSkyHeight || saria.SariaZoneRain || saria.SariaZoneBeach || saria.SariaZoneSpace))
                    {
                        projectile.SariaStatLower();
                    }
                    if ((saria.SariaZoneUndergroundDesert || saria.SariaZoneUnderworld || saria.SariaZoneRockLayer || saria.SariaZoneUnderground) && !saria.SariaZoneJungle && (!player.HasBuff(ModContent.BuffType<StatLower>())))
                    {
                        projectile.SariaStatRaise();
                    }
                }
                if (transform == 5)
                {
                    if (!scarfImmune && (saria.SariaZoneUnderworld || saria.SariaZoneSnow || saria.SariaZoneSpace || saria.SariaZoneRain || saria.SariaZoneSandstorm || saria.SariaZoneMeteor || saria.SariaZoneBeach || saria.SariaHasReajCandle))
                    {
                        projectile.SariaStatLower();
                    }
                    if ((saria.SariaZoneJungle || saria.SariaZoneCorrupt || saria.SariaZoneCrimson || saria.SariaZoneGlowingMushroom || saria.SariaZoneGraveyard || saria.SariaZoneUnderground || saria.SariaZoneDirtLayer || saria.SariaZoneHallow || saria.SariaZoneDesert || saria.SariaZoneUndergroundDesert || !Main.dayTime || saria.SariaHasCalmMindCandle) && (!player.HasBuff(ModContent.BuffType<StatLower>())))
                    {
                        projectile.SariaStatRaise();
                    }
                }
                if (transform == 6)
                {
                    if ((saria.SariaZoneCorrupt || saria.SariaZoneCrimson || saria.SariaZoneGraveyard || saria.SariaZoneUnderworld || saria.SariaZoneDungeon || !Main.dayTime || (saria.SariaZoneHallow && saria.SariaZoneUnderground) || saria.SariaHasReajCandle))
                    {
                        projectile.SariaStatRaise();
                    }
                    if (!scarfImmune && (((saria.SariaZoneOverworld || saria.SariaZoneSkyHeight) && Main.dayTime) || saria.SariaHasCalmMindCandle) && (!player.HasBuff(ModContent.BuffType<StatRaise>())))
                    {
                        projectile.SariaStatLower();
                    }
                }
            }
        }

        public static NPC MinionHoming(Vector2 origin, float maxDistanceToCheck, Player owner, bool ignoreTiles = true)
        {
            if (owner == null || owner.whoAmI < 0 || owner.whoAmI > 255 || owner.MinionAttackTargetNPC < 0 || owner.MinionAttackTargetNPC > 200)
            {
                return origin.ClosestNPCAt(maxDistanceToCheck, ignoreTiles);
            }
            NPC npc = Main.npc[owner.MinionAttackTargetNPC];
            bool canHit = true;
            if (!ignoreTiles)
            {
                origin = owner.Center;
                canHit = Collision.CanHit(origin, 1, 1, npc.Center, 1, 1);
            }
            if (owner.HasMinionAttackTargetNPC && canHit)
            {
                return npc;
            }
            return origin.ClosestNPCAt(maxDistanceToCheck, ignoreTiles);
        }

        public static NPC ClosestNPCAt(Vector2 origin, float maxDistanceToCheck, bool ignoreTiles = true, bool bossPriority = false)
        {
            NPC closestTarget = null;
            float distance = maxDistanceToCheck;
            if (bossPriority)
            {
                bool bossFound = false;
                for (int index2 = 0; index2 < Main.npc.Length; index2++)
                {
                    if ((bossFound && !Main.npc[index2].boss && Main.npc[index2].type != NPCID.WallofFleshEye) || !Main.npc[index2].CanBeChasedBy())
                    {
                        continue;
                    }
                    float extraDistance2 = Main.npc[index2].width / 2 + Main.npc[index2].height / 2;
                    bool canHit2 = true;
                    if (extraDistance2 < distance && !ignoreTiles)
                    {
                        canHit2 = Collision.CanHit(origin, 1, 1, Main.npc[index2].Center, 1, 1);
                    }
                    if (Vector2.Distance(origin, Main.npc[index2].Center) < distance + extraDistance2 && canHit2)
                    {
                        if (Main.npc[index2].boss || Main.npc[index2].type == NPCID.WallofFleshEye)
                        {
                            bossFound = true;
                        }
                        distance = Vector2.Distance(origin, Main.npc[index2].Center);
                        closestTarget = Main.npc[index2];
                    }
                }
            }
            else
            {
                for (int index = 0; index < Main.npc.Length; index++)
                {
                    if (Main.npc[index].CanBeChasedBy())
                    {
                        float extraDistance = Main.npc[index].width / 2 + Main.npc[index].height / 2;
                        bool canHit = true;
                        if (extraDistance < distance && !ignoreTiles)
                        {
                            canHit = Collision.CanHit(origin, 1, 1, Main.npc[index].Center, 1, 1);
                        }
                        if (Vector2.Distance(origin, Main.npc[index].Center) < distance + extraDistance && canHit)
                        {
                            distance = Vector2.Distance(origin, Main.npc[index].Center);
                            closestTarget = Main.npc[index];
                        }
                    }
                }
            }
            return closestTarget;
        }

        public static void AttackCircleDust(Projectile projectile, int dusttype, int Severity, int Speed, float Width, float lenght, float Scale)
        {
            for (int i = 0; i < Severity; i++)
            {
                Vector2 speed = Main.rand.NextVector2CircularEdge(Width, lenght);
                Dust d = Dust.NewDustPerfect(projectile.Center, dusttype, speed * 15, Scale: Scale);
                d.noGravity = true;
            }
        }

        public static void AttackDust(Projectile projectile, int dusttype, int Severity, int Range)
        {
            if (Main.rand.NextBool(Severity))
            {
                float radius = (float)Math.Sqrt(Main.rand.Next(Range * Range));
                double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                Dust.NewDust(new Vector2(projectile.Center.X + radius * (float)Math.Cos(angle), projectile.Center.Y + radius * (float)Math.Sin(angle)), 0, 0, dusttype, 0f, 0f, 0, default(Color), 1.5f);
            }
        }

        public static void AttackDust2(Projectile projectile)
        {
            Player player = Main.player[projectile.owner];
            if ((player.HasBuff(ModContent.BuffType<Overcharged>())) && !player.ZoneSnow)
            {
                projectile.AttackDust(ModContent.DustType<StaticDustNormalPurple>(), 1, 34);
                Lighting.AddLight(projectile.Center, Color.Purple.ToVector3());
            }
            else if (player.ZoneSnow)
            {
                projectile.AttackDust(ModContent.DustType<StaticDustNormalPink>(), 1, 34);
                Lighting.AddLight(projectile.Center, Color.Pink.ToVector3());
            }
            else if ((player.HasBuff(ModContent.BuffType<StatRaise>())))
            {
                projectile.AttackDust(ModContent.DustType<StaticDustNormalBlue>(), 1, 34);
                Lighting.AddLight(projectile.Center, Color.Blue.ToVector3());
            }
            else if (player.HasBuff(ModContent.BuffType<StatLower>()))
            {
                projectile.AttackDust(ModContent.DustType<StaticDustNormalRed>(), 1, 34);
                Lighting.AddLight(projectile.Center, Color.Red.ToVector3());
            }
            else
            {
                projectile.AttackDust(ModContent.DustType<StaticDustNormal>(), 1, 34);
                Lighting.AddLight(projectile.Center, Color.Yellow.ToVector3());
            }
        }

        public static void RockDust(Projectile projectile, int dusttype, int Severity, int Range1, int Range2, int dustspotY, int sneezespotGreater, int sneezespotLesser)
        {
            float sneezespot = 5;
            if (Main.rand.NextBool(Severity))
            {
                float radius = (float)Math.Sqrt(Main.rand.Next(Range1 * Range2));
                double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                if (projectile.spriteDirection > 0)
                {
                    sneezespot = sneezespotGreater;
                }
                if (projectile.spriteDirection < 0)
                {
                    sneezespot = sneezespotLesser;
                }
                Dust.NewDust(new Vector2((projectile.Center.X + sneezespot) + radius * (float)Math.Cos(angle), (projectile.Center.Y + dustspotY) + radius * (float)Math.Sin(angle)), 0, 0, dusttype, 0f, 0f, 0, default(Color), 1.5f);
            }
        }

        public static void SneezeDust(Projectile projectile, int dusttype, int Severity, int Range, int dustspotY, int sneezespotGreater, int sneezespotLesser)
        {
            float sneezespot = 5;
            if (Main.rand.NextBool(Severity))
            {
                float radius = (float)Math.Sqrt(Main.rand.Next(Range * Range));
                double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                if (projectile.spriteDirection > 0)
                {
                    sneezespot = sneezespotGreater;
                }
                if (projectile.spriteDirection < 0)
                {
                    sneezespot = sneezespotLesser;
                }
                Dust.NewDust(new Vector2((projectile.Center.X + sneezespot) + radius * (float)Math.Cos(angle), (projectile.Center.Y + dustspotY) + radius * (float)Math.Sin(angle)), 0, 0, dusttype, 0f, 0f, 0, default(Color), 1.5f);
            }
        }

        public static void SariaStatRaise(Projectile projectile)
        {
            Player player = Main.player[projectile.owner];
            Saria saria = projectile.ModProjectile as Saria;
            if (!player.HasBuff(ModContent.BuffType<StatRaise>()))
            {
                if (saria != null && saria.StatRaiseSoundCooldown == 0)
                {
                    SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/StatRaise"), projectile.Center);
                    saria.StatRaiseSoundCooldown = Saria.StatSoundCooldownMax;
                }
                for (int j = 0; j < 1; j++) //set to 2
                {
                    if (Main.myPlayer == projectile.owner) Projectile.NewProjectile(projectile.GetSource_FromThis(), projectile.position.X + 0, projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<PowerUp>(), (int)(projectile.damage), 0f, projectile.owner, player.whoAmI, projectile.whoAmI);
                }
                player.AddBuff(ModContent.BuffType<StatRaise>(), 20);
            }
            if (player.HasBuff(ModContent.BuffType<StatRaise>()))
            {
                player.AddBuff(ModContent.BuffType<StatRaise>(), 20);
            }
        }

        public static void SariaStatLower(Projectile projectile)
        {
            Player player = Main.player[projectile.owner];
            Saria saria = projectile.ModProjectile as Saria;
            if (!player.HasBuff(ModContent.BuffType<StatLower>()))
            {
                if (saria != null && saria.StatLowerSoundCooldown == 0)
                {
                    SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/StatLower"), projectile.Center);
                    saria.StatLowerSoundCooldown = Saria.StatSoundCooldownMax;
                }
                for (int j = 0; j < 1; j++) //set to 2
                {
                    if (Main.myPlayer == projectile.owner) Projectile.NewProjectile(projectile.GetSource_FromThis(), projectile.position.X + 0, projectile.position.Y + -24, 0, 0, ModContent.ProjectileType<PowerDown>(), (int)(projectile.damage), 0f, projectile.owner, player.whoAmI, projectile.whoAmI);
                }
                player.AddBuff(ModContent.BuffType<StatLower>(), 20);
            }
            if (player.HasBuff(ModContent.BuffType<StatLower>()))
            {
                player.AddBuff(ModContent.BuffType<StatLower>(), 20);
            }
        }

        public static void SariaSmallChargeSetup(Projectile projectile, int Transform, bool IsRight, Color lightColor)
        {
            int startpositionx = -50;
            int startpositiony = 10;
            if (IsRight)
            {
                startpositionx = 12;
                startpositiony = 10;
            }
            int formNumber = Transform + 1;
            string sparkPath = $"SariaMod/Items/Strange/{formNumber}SariaAnimations/{formNumber}ChargingSpark";
            projectile.FrameChargeElectricitydraw((Texture2D)ModContent.Request<Texture2D>(sparkPath).Value, lightColor, true, startpositionx, startpositiony);
            projectile.SariaRandomChargeCircle(Transform, IsRight);
        }

        public static void SariaRandomChargeCircle(Projectile projectile, int transform, bool isright)
        {
            Vector2 ToSpot = projectile.Right;
            if (!isright)
            {
                ToSpot = projectile.Left;
            }
            if (transform == 0)
            {
                if (Main.rand.NextBool(30))
                {
                    for (int i = 0; i < 50; i++)
                    {
                        Vector2 speed = Main.rand.NextVector2CircularEdge(1f, 1f);
                        Dust d = Dust.NewDustPerfect(ToSpot, ModContent.DustType<AbsorbPsychic>(), speed * -5, Scale: 1.5f);
                        d.noGravity = true;
                    }
                    SoundEngine.PlaySound(SoundID.DD2_SkyDragonsFuryShot, projectile.Center);
                    Lighting.AddLight(projectile.Center, Color.HotPink.ToVector3() * 4f);
                }
            }
            if (transform == 1)
            {
                if (Main.rand.NextBool(30))
                {
                    for (int i = 0; i < 50; i++)
                    {
                        Vector2 speed = Main.rand.NextVector2CircularEdge(1f, 1f);
                        Dust d = Dust.NewDustPerfect(ToSpot, ModContent.DustType<BubbleDust2>(), speed * -6, Scale: 2.7f);
                        d.noGravity = true;
                    }
                    SoundEngine.PlaySound(SoundID.Drown, projectile.Center);
                    Lighting.AddLight(projectile.Center, Color.White.ToVector3() * 4f);
                }
            }
            if (transform == 2)
            {
                DrawFireChargeGlobVortex(projectile, ToSpot);

                if (Main.rand.NextBool(30))
                {
                    for (int i = 0; i < 25; i++)
                    {
                        Vector2 speed = Main.rand.NextVector2CircularEdge(1f, 1f);
                        Dust d = Dust.NewDustPerfect(
                            ToSpot,
                            ModContent.DustType<SmokeDust6>(),
                            speed * 6f,
                            Scale: 2.5f);
                        d.noGravity = true;
                    }

                    SoundEngine.PlaySound(SoundID.Item88, projectile.Center);
                    Lighting.AddLight(projectile.Center, Color.Red.ToVector3() * 4f);
                }
            }
            if (transform == 3)
            {
                if (Main.rand.NextBool(30))
                {
                    for (int i = 0; i < 50; i++)
                    {
                        Vector2 speed = Main.rand.NextVector2CircularEdge(1f, 1f);
                        Dust d = Dust.NewDustPerfect(ToSpot, ModContent.DustType<StaticDustRing>(), speed * -6, Scale: 2.7f);
                        d.noGravity = true;
                    }
                    SoundEngine.PlaySound(SoundID.NPCHit34, projectile.Center);
                    Lighting.AddLight(projectile.Center, Color.LightYellow.ToVector3() * 4f);
                }
            }
            if (transform == 4)
            {
                if (Main.rand.NextBool(30))
                {
                    for (int i = 0; i < 50; i++)
                    {
                        Vector2 speed = Main.rand.NextVector2CircularEdge(1f, 1f);
                        Dust d = Dust.NewDustPerfect(ToSpot, ModContent.DustType<RockDustRing>(), speed * -6, Scale: 2.7f);
                        d.noGravity = true;
                    }
                    SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact, projectile.Center);
                    Lighting.AddLight(projectile.Center, Color.Green.ToVector3() * 6f);
                }
            }
            if (transform == 5)
            {
                if (Main.rand.NextBool(30))
                {
                    SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact, projectile.Center);
                    Lighting.AddLight(projectile.Center, Color.Orange.ToVector3() * 6f);
                }
            }
            if (transform == 6)
            {
                if (Main.rand.NextBool(30))
                {
                    SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact, projectile.Center);
                    Lighting.AddLight(projectile.Center, Color.GhostWhite.ToVector3() * 6f);
                }
            }
        }

        private static void DrawFireChargeGlobVortex(Projectile projectile, Vector2 center)
        {
            if (Main.dedServ)
                return;

            if (!ChargeGlobVortices.TryGetValue(projectile.whoAmI, out List<RovaLavaGlob> globs))
            {
                globs = new List<RovaLavaGlob>();
                ChargeGlobVortices[projectile.whoAmI] = globs;
            }

            if (globs.Count < 6 && Main.rand.NextBool(6))
            {
                RovaLavaGlobVisual.SpawnInward(
                    globs,
                    center,
                    1,
                    32f,
                    5f,
                    46f,
                    2f,
                    4.5f);
            }

            RovaLavaGlobVisual.Update(globs);
            RovaLavaGlobVisual.Draw(
                globs,
                Main.screenPosition,
                new Color(255, 65, 8, 225),
                new Color(255, 215, 70, 235),
                0.9f);

            Lighting.AddLight(center, Color.Red.ToVector3() * 4f);
        }
    }
}
