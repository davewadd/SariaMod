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
            insidePsychicField = PsychicFieldSystem.TryApplyPortalFallPhysics(Player);
            if (insidePsychicField)
            {
                PsychicFieldSystem.ApplyPortalAirInertia(Player);
            }
        }

        public override void PostUpdateRunSpeeds()
        {
            if (insidePsychicField)
            {
                PsychicFieldSystem.ApplyPortalAirInertia(Player);
            }
        }
    }
}
