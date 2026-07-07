namespace SariaMod.TileGlow
{
    public struct TileGlowData
    {
        /// <summary>
        /// The game tick when this glow started
        /// </summary>
        public int StartTick;
        
        /// <summary>
        /// How long the glow should last in ticks (600 = 10 seconds)
        /// </summary>
        public int Duration;
        
        /// <summary>
        /// Distance from the center of the cold wave (0 = center, higher = edge)
        /// Tiles closer to center glow brighter and longer
        /// </summary>
        public float DistanceFromCenter;
        
        /// <summary>
        /// Maximum distance from center when this tile was affected
        /// Used to normalize distance calculations
        /// </summary>
        public float MaxRadius;
        
        /// <summary>
        /// Current intensity of the glow (0-1)
        /// </summary>
        public float Intensity;
        
        public TileGlowData(int startTick, int duration, float distanceFromCenter, float maxRadius)
        {
            StartTick = startTick;
            Duration = duration;
            DistanceFromCenter = distanceFromCenter;
            MaxRadius = maxRadius;
            Intensity = 1f;
        }
        
        /// <summary>
        /// Calculate the normalized distance (0 = center, 1 = edge)
        /// </summary>
        public float NormalizedDistance => MaxRadius > 0 ? DistanceFromCenter / MaxRadius : 0f;
        
        /// <summary>
        /// Check if this glow has expired
        /// </summary>
        public bool IsExpired(int currentTick)
        {
            return currentTick - StartTick > Duration;
        }
        
        /// <summary>
        /// Get the current progress of the glow (0 = just started, 1 = finished)
        /// </summary>
        public float GetProgress(int currentTick)
        {
            int elapsed = currentTick - StartTick;
            return elapsed / (float)Duration;
        }
    }
}
