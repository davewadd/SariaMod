using Terraria.ModLoader;

namespace SariaMod.Items.Strange
{
    public class PsychicFieldPlayer : ModPlayer
    {
        private bool insidePsychicField;

        public override void ResetEffects()
        {
            insidePsychicField = false;
        }

        public override void PostUpdateMiscEffects()
        {
            insidePsychicField = PsychicFieldSystem.TryApplyPortalFallSetup(Player);
        }
        
        public override void PostUpdateRunSpeeds()
        {
            // Apply gravity amplification AFTER jump/gravity/movement is fully settled.
            // Only pulls when actually falling (velocity.Y > 0f) — jumping is left alone.
            if (insidePsychicField)
            {
                PsychicFieldSystem.ApplyPortalFallAmplification(Player);
            }
        }
    }
}
