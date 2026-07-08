using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace SariaMod
{
    public static class SariaModUtilities2
    {
        public static float alpha4 { get => SariaDrawingExtensions.rupeeAlpha4; set => SariaDrawingExtensions.rupeeAlpha4 = value; }
        public static bool alpha4Counter { get => SariaDrawingExtensions.rupeeAlpha4Counter; set => SariaDrawingExtensions.rupeeAlpha4Counter = value; }
        public static float alpha5 { get => SariaDrawingExtensions.alpha5; set => SariaDrawingExtensions.alpha5 = value; }
        public static bool alpha5Counter { get => SariaDrawingExtensions.alpha5Counter; set => SariaDrawingExtensions.alpha5Counter = value; }
        public static float alpha6 { get => SariaDrawingExtensions.alpha6; set => SariaDrawingExtensions.alpha6 = value; }
        public static bool alpha6Counter { get => SariaDrawingExtensions.alpha6Counter; set => SariaDrawingExtensions.alpha6Counter = value; }

        public static void BlueRingofdust(this Projectile projectile, int howmany) => SariaDrawingExtensions.BlueRingofdust(projectile, howmany);
        public static void SariaSmallChargeSetup(this Projectile projectile, int Transform, bool IsRight, Color lightColor) => SariaCombatExtensions.SariaSmallChargeSetup(projectile, Transform, IsRight, lightColor);
        public static void SariaRandomChargeCircle(this Projectile projectile, int transform, bool isright) => SariaCombatExtensions.SariaRandomChargeCircle(projectile, transform, isright);
        public static bool NewIdlePosition(this Projectile projectile, int howclose) => SariaMovementExtensions.NewIdlePosition(projectile, howclose);
        public static float CalcWallSafeClose(this Projectile projectile, float logicalClose) => SariaMovementExtensions.CalcWallSafeClose(projectile, logicalClose);
        public static void EmeraldspikeGlowandFadedraw(this Projectile projectile, Texture2D texture, Color lightColor, Color WhatColor, float glowspeed, int numframes) => SariaDrawingExtensions.EmeraldspikeGlowandFadedraw(projectile, texture, lightColor, WhatColor, glowspeed, numframes);
        public static void Emeraldspikedraw(this Projectile projectile, Texture2D texture, Color lightColor, Color WhatColor, bool doesanimate, int startposX, int startposY, int NumFrames) => SariaDrawingExtensions.Emeraldspikedraw(projectile, texture, lightColor, WhatColor, doesanimate, startposX, startposY, NumFrames);
        public static void Rupeedraw(this Projectile projectile, Texture2D texture, Color lightColor, Color WhatColor, int NumFrames) => SariaDrawingExtensions.Rupeedraw(projectile, texture, lightColor, WhatColor, NumFrames);
        public static void RupeeGlowandFadedraw(this Projectile projectile, Texture2D texture, Color lightColor, Color WhatColor, int numframes) => SariaDrawingExtensions.RupeeGlowandFadedraw(projectile, texture, lightColor, WhatColor, numframes);
        public static bool IsTouchingWaterBarrier(this Projectile projectile) => MiscUtilities.IsTouchingWaterBarrier(projectile);
        public static bool IsUnderThunderCloud(this Projectile projectile) => MiscUtilities.IsUnderThunderCloud(projectile);
        public static void SariaBubbleFaceSpawner(this Projectile projectile, bool sleep, int canmove, bool cursed, int mood) => SariaDrawingExtensions.SariaBubbleFaceSpawner(projectile, sleep, canmove, cursed, mood);
        public static void SariaBiomeEffectivness(this Projectile projectile, int biometime, int transform) => SariaCombatExtensions.SariaBiomeEffectivness(projectile, biometime, transform);
    }
}
