using System;

namespace SariaMod.TileGlow
{
    public struct TileHeatData
    {
        public int StartTick;
        public int Duration;
        public float DistanceFromCenter;
        public float MaxRadius;
        public float Intensity;
        public int Owner;
        public int Damage;

        public TileHeatData(int startTick, int duration, float distanceFromCenter, float maxRadius, int owner = -1, int damage = 0)
        {
            StartTick = startTick;
            Duration = duration;
            DistanceFromCenter = distanceFromCenter;
            MaxRadius = maxRadius;
            Intensity = 1f;
            Owner = owner;
            Damage = damage;
        }

        public float NormalizedDistance => MaxRadius > 0
            ? Math.Clamp(DistanceFromCenter / MaxRadius, 0f, 1f)
            : 0f;

        public bool IsExpired(int currentTick)
        {
            return currentTick - StartTick > Duration;
        }

        public float GetProgress(int currentTick)
        {
            int elapsed = currentTick - StartTick;
            return Duration > 0 ? elapsed / (float)Duration : 1f;
        }
    }
}
