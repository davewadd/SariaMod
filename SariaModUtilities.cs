using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace SariaMod
{
    public static class SariaModUtilities
    {
        public static float alpha1 { get => SariaDrawingExtensions.alpha1; set => SariaDrawingExtensions.alpha1 = value; }
        public static bool alpha1Counter { get => SariaDrawingExtensions.alpha1Counter; set => SariaDrawingExtensions.alpha1Counter = value; }
        public static float alpha2 { get => SariaDrawingExtensions.alpha2; set => SariaDrawingExtensions.alpha2 = value; }
        public static bool alpha2Counter { get => SariaDrawingExtensions.alpha2Counter; set => SariaDrawingExtensions.alpha2Counter = value; }
        public static float alpha3 { get => SariaDrawingExtensions.alpha3; set => SariaDrawingExtensions.alpha3 = value; }
        public static bool alpha3Counter { get => SariaDrawingExtensions.alpha3Counter; set => SariaDrawingExtensions.alpha3Counter = value; }
        public static int alpha3Phase { get => SariaDrawingExtensions.alpha3Phase; set => SariaDrawingExtensions.alpha3Phase = value; }
        public static int alpha3Timer { get => SariaDrawingExtensions.alpha3Timer; set => SariaDrawingExtensions.alpha3Timer = value; }
        public static int alpha3FlickerCount { get => SariaDrawingExtensions.alpha3FlickerCount; set => SariaDrawingExtensions.alpha3FlickerCount = value; }
        public static float alpha4 { get => SariaDrawingExtensions.alpha4; set => SariaDrawingExtensions.alpha4 = value; }
        public static bool alpha4Counter { get => SariaDrawingExtensions.alpha4Counter; set => SariaDrawingExtensions.alpha4Counter = value; }

        public static void UpdateAlphaCounters() => SariaDrawingExtensions.UpdateAlphaCounters();
        public static void StartSandstorm() => MiscUtilities.StartSandstorm();
        public static void SendPacket(this Player player, ModPacket packet, bool server) => MiscUtilities.SendPacket(player, packet, server);
        internal static void SetUpCandle(ModTile mt, bool lavaImmune = false, int offset = -4) => MiscUtilities.SetUpCandle(mt, lavaImmune, offset);
        public static void StopSandstorm() => MiscUtilities.StopSandstorm();
        public static FairyPlayer Fairy(this Player player) => MiscUtilities.Fairy(player);
        public static FairyProjectile Fairy(this Projectile proj) => MiscUtilities.Fairy(proj);
        public static int CountProjectiles(int Type) => MiscUtilities.CountProjectiles(Type);
        public static bool InSpace(this Player player) => MiscUtilities.InSpace(player);
        public static void HealingProjectile(Projectile projectile, int healing, int playerToHeal, int timeCheck = 120) => MiscUtilities.HealingProjectile(projectile, healing, playerToHeal, timeCheck);
        public static void HealingProjectile2(Projectile projectile, int healing, int playerToHeal, float homingVelocity, float N, bool autoHomes = true, int timeCheck = 120) => MiscUtilities.HealingProjectile2(projectile, healing, playerToHeal, homingVelocity, N, autoHomes, timeCheck);
        public static string ColorMessage(string msg, Color color) => MiscUtilities.ColorMessage(msg, color);
        public static void LightHitWire(int type, int i, int j, int tileX, int tileY) => MiscUtilities.LightHitWire(type, i, j, tileX, tileY);
        public static void SummonRupeeShard(this Projectile projectile, int ProjectileType, int CrystalState) => MiscUtilities.SummonRupeeShard(projectile, ProjectileType, CrystalState);
        public static void SariaStatRaise(this Projectile projectile) => SariaCombatExtensions.SariaStatRaise(projectile);
        public static void SariaStatLower(this Projectile projectile) => SariaCombatExtensions.SariaStatLower(projectile);
        public static void AttackCircleDust(this Projectile projectile, int dusttype, int Severity, int Speed, float Width, float lenght, float Scale) => SariaCombatExtensions.AttackCircleDust(projectile, dusttype, Severity, Speed, Width, lenght, Scale);
        public static void AttackDust(this Projectile projectile, int dusttype, int Severity, int Range) => SariaCombatExtensions.AttackDust(projectile, dusttype, Severity, Range);
        public static void AttackDust2(this Projectile projectile) => SariaCombatExtensions.AttackDust2(projectile);
        public static void RockDust(this Projectile projectile, int dusttype, int Severity, int Range1, int Range2, int dustspotY, int sneezespotGreater, int sneezespotLesser) => SariaCombatExtensions.RockDust(projectile, dusttype, Severity, Range1, Range2, dustspotY, sneezespotGreater, sneezespotLesser);
        public static void RockDustOnVisiblePixels(this Projectile projectile, Texture2D maskTexture, int dustType, int severity, int totalFrames, int currentFrame, bool doesFlip = false, int startPosX = 0) => SariaDrawingExtensions.RockDustOnVisiblePixels(projectile, maskTexture, dustType, severity, totalFrames, currentFrame, doesFlip, startPosX);
        public static void SneezeDust(this Projectile projectile, int dusttype, int Severity, int Range, int dustspotY, int sneezespotGreater, int sneezespotLesser) => SariaCombatExtensions.SneezeDust(projectile, dusttype, Severity, Range, dustspotY, sneezespotGreater, sneezespotLesser);
        public static void FrameChargeElectricitydraw(this Projectile projectile, Texture2D texture, Color lightColor, bool nottoscreen, int startPosX = 0, int startPosY = 0) => SariaDrawingExtensions.FrameChargeElectricitydraw(projectile, texture, lightColor, nottoscreen, startPosX, startPosY);
        public static void FrameChargedraw(this Projectile projectile, Texture2D texture, Color lightColor, bool nottoscreen, bool Eightframes, int startPosX = 0, int startPosY = 0) => SariaDrawingExtensions.FrameChargedraw(projectile, texture, lightColor, nottoscreen, Eightframes, startPosX, startPosY);
        public static void SariaBubbleFaces(this Projectile projectile, Texture2D texture, bool shoulditflip, int FrameSpeed, int NumFrames, int startPosY, Color lightColor) => SariaDrawingExtensions.SariaBubbleFaces(projectile, texture, shoulditflip, FrameSpeed, NumFrames, startPosY, lightColor);
        public static void SariaMaindraw(this Projectile projectile, Texture2D texture, bool Glowinthedark, bool ShoulditFlip, bool DoesitTrail, int startPosY, int HowlongisTrail, Color lightColor, int startPosX = 0, bool pointSample = false, float alphaScale = 1f) => SariaDrawingExtensions.SariaMaindraw(projectile, texture, Glowinthedark, ShoulditFlip, DoesitTrail, startPosY, HowlongisTrail, lightColor, startPosX, pointSample, alphaScale);
        public static void SariaSparksDraw(this Projectile projectile, Texture2D texture, Color lightColor) => SariaDrawingExtensions.SariaSparksDraw(projectile, texture, lightColor);
        public static void SariaElectricMaskDraw(this Projectile projectile, Texture2D texture, bool ShoulditFlip, Color lightColor, int startPosY = 1) => SariaDrawingExtensions.SariaElectricMaskDraw(projectile, texture, ShoulditFlip, lightColor, startPosY);
        public static void FlatImageDraw(this Projectile projectile, Texture2D texture, Color lightColor, int startPosX = 0, int startPosY = 0) => SariaDrawingExtensions.FlatImageDraw(projectile, texture, lightColor, startPosX, startPosY);
        public static void VisualSetUpDraw(this Projectile projectile, Texture2D texture, Color lightColor, int startPosX = 0, int startPosY = 0) => SariaDrawingExtensions.VisualSetUpDraw(projectile, texture, lightColor, startPosX, startPosY);
        public static void SariaEyesGlowandFadedraw(this Projectile projectile, Texture2D texture, Color lightColor, Color WhatColor) => SariaDrawingExtensions.SariaEyesGlowandFadedraw(projectile, texture, lightColor, WhatColor);
        public static void DialogueUEyeMaskdraw(this Projectile projectile, Texture2D texture, Color lightColor, Vector2 startPos2, int NumFrames, int WhichFrame) => SariaDrawingExtensions.DialogueUEyeMaskdraw(projectile, texture, lightColor, startPos2, NumFrames, WhichFrame);
        public static void DialogueUIMask3draw(this Projectile projectile, Color lightColor, int startPosX = 0, int startPosY = 0) => SariaDrawingExtensions.DialogueUIMask3draw(projectile, lightColor, startPosX, startPosY);
        public static void DialogueUIMask2draw(this Projectile projectile, Color lightColor, int startPosX = 0, int startPosY = 0) => SariaDrawingExtensions.DialogueUIMask2draw(projectile, lightColor, startPosX, startPosY);
        public static void DialogueUIMaskdraw(this Projectile projectile, Color lightColor, int startPosX = 0, int startPosY = 0) => SariaDrawingExtensions.DialogueUIMaskdraw(projectile, lightColor, startPosX, startPosY);
        public static void DialogueUIFireMaskdraw(this Projectile projectile, Color lightColor, Texture2D texture, int i, int j, int startPosX = 0, int startPosY = 0) => SariaDrawingExtensions.DialogueUIFireMaskdraw(projectile, lightColor, texture, i, j, startPosX, startPosY);
        public static void Saria5GlowMaskdraw(this Projectile projectile, Texture2D texture, Color lightColor, bool counter1, bool counter2, bool doesFlip = false, int startPosX = 0) => SariaDrawingExtensions.Saria5GlowMaskdraw(projectile, texture, lightColor, counter1, counter2, doesFlip, startPosX);
        public static void Saria3GlowMaskdraw(this Projectile projectile, Texture2D texture, int i, int j, bool ShoulditFlip, Color lightColor) => SariaDrawingExtensions.Saria3GlowMaskdraw(projectile, texture, i, j, ShoulditFlip, lightColor);
        public static void SariaFireHairDraw(this Projectile projectile, Texture2D texture, bool ShoulditFlip, int startPosY, Color lightColor) => SariaDrawingExtensions.SariaFireHairDraw(projectile, texture, ShoulditFlip, startPosY, lightColor);
        public static void DrawFlameEffect(Texture2D flameTexture, int i, int j, int offsetX = 0, int offsetY = 0) => SariaDrawingExtensions.DrawFlameEffect(flameTexture, i, j, offsetX, offsetY);
        public static void DrawFlameSparks(int dustType, int rarity, int i, int j) => SariaDrawingExtensions.DrawFlameSparks(dustType, rarity, i, j);
        public static void BlueRingofdust(Projectile projectile) => SariaDrawingExtensions.BlueRingofdust(projectile);
        public static void SariaBaseDamage(this Projectile projectile) => SariaCombatExtensions.SariaBaseDamage(projectile);
        public static NPC MinionHoming(this Vector2 origin, float maxDistanceToCheck, Player owner, bool ignoreTiles = true) => SariaCombatExtensions.MinionHoming(origin, maxDistanceToCheck, owner, ignoreTiles);
        public static NPC ClosestNPCAt(this Vector2 origin, float maxDistanceToCheck, bool ignoreTiles = true, bool bossPriority = false) => SariaCombatExtensions.ClosestNPCAt(origin, maxDistanceToCheck, ignoreTiles, bossPriority);
    }
}
