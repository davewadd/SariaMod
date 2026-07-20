using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace SariaMod.Items.Strange
{
    public class PsychicFieldPlayer : ModPlayer
    {
        private const float DefaultMaxRunSpeed = 3f;
        private const float DefaultRunAcceleration = 0.08f;

        private bool insidePsychicField;
        private bool applyAirborneSpeedBoost;
        private bool jumpWasHeld;

        public override void ResetEffects()
        {
            insidePsychicField = false;
            applyAirborneSpeedBoost = false;
        }

        public override void PostUpdateMiscEffects()
        {
            insidePsychicField = PsychicFieldSystem.TryApplyPortalFallSetup(Player);
            applyAirborneSpeedBoost = insidePsychicField
                && IsAirborne(Player)
                && Player.moveSpeed <= PsychicFieldSystem.AirborneMoveSpeedBonusCutoff;
        }
        
        public override void PostUpdateRunSpeeds()
        {
            // Holding up enables space gravity. Holding down retains the existing fast fall.
            if (insidePsychicField)
            {
                PsychicFieldSystem.ApplyPortalFallAmplification(Player);

                float boostedSpeedCutoff = DefaultMaxRunSpeed
                    * PsychicFieldSystem.AirborneMoveSpeedBonusCutoff;
                if (applyAirborneSpeedBoost && Player.maxRunSpeed <= boostedSpeedCutoff)
                {
                    Player.maxRunSpeed += DefaultMaxRunSpeed
                        * PsychicFieldSystem.AirborneMoveSpeedBonus;
                    Player.accRunSpeed += DefaultMaxRunSpeed
                        * PsychicFieldSystem.AirborneMoveSpeedBonus;
                    Player.runAcceleration += DefaultRunAcceleration
                        * PsychicFieldSystem.AirborneMoveSpeedBonus;
                }
            }
        }

        public override void PreUpdateMovement()
        {
            bool jumpPressed = Player.controlJump && !jumpWasHeld;
            jumpWasHeld = Player.controlJump;

            if (!insidePsychicField)
            {
                return;
            }

            bool jumped = Player.justJumped;
            if (!jumped && jumpPressed && IsAirborne(Player) && !Player.mount.Active)
            {
                Player.velocity.Y = -Player.jumpSpeed * Player.gravDir;
                Player.jump = Player.jumpHeight;
                Player.releaseJump = false;
                Player.justJumped = true;
                jumped = true;
            }

            if (jumped)
            {
                SpawnPsychicJumpPlatform();
            }
        }

        private static bool IsAirborne(Player player)
        {
            Vector2 supportCheckPosition = player.gravDir == 1f
                ? new Vector2(player.position.X, player.position.Y + player.height)
                : new Vector2(player.position.X, player.position.Y - 2f);
            return !Collision.SolidCollision(supportCheckPosition, player.width, 2, true);
        }

        private void SpawnPsychicJumpPlatform()
        {
            if (Main.myPlayer != Player.whoAmI)
            {
                return;
            }

            Vector2 platformPosition = Player.Center
                - Vector2.UnitY * Player.gravDir * Player.height * 0.08f;
            PsychicJumpPlatformProjectile.SpawnPair(
                Player.GetSource_FromThis(),
                platformPosition,
                Player.whoAmI,
                Player.gravDir,
                PsychicJumpPlatformProjectile.PlaySoundFlag);
        }
    }
}
