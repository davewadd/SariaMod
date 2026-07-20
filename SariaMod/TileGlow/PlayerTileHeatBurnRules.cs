using System;

namespace SariaMod.TileGlow
{
    internal static class PlayerTileHeatBurnRules
    {
        internal const int DirectBurnDurationTicks = 4 * 60;
        internal const int NearbyBurnMinimumTicks = 60;
        internal const float DirectBurnIntensityThreshold = 0.50f;
        internal const float NearbyBurnIntensityThreshold = 0.35f;

        internal static int ResolveMinimumDurationTicks(
            bool isTouchingBurningHeat,
            bool isNearBurningHeat)
        {
            if (isTouchingBurningHeat)
                return DirectBurnDurationTicks;

            return isNearBurningHeat ? NearbyBurnMinimumTicks : 0;
        }

        internal static int PreserveLongerDurationTicks(
            int currentDurationTicks,
            int minimumDurationTicks)
        {
            return Math.Max(currentDurationTicks, minimumDurationTicks);
        }
    }
}
