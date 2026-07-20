using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;

namespace SariaMod.Items.Strange
{
    public partial class Saria
    {
        private const float PsychicFootWaveInset = 9f;

        private bool wasFallingForPsychicWave;
        private bool wasGroundedForPsychicWave;
        private bool wasHoveringForPsychicWave;

        private void UpdatePsychicMovementWaves()
        {
            bool groundedNow = _dbgGroundTouching || Collision.SolidCollision(
                new Vector2(Projectile.position.X, Projectile.position.Y + Projectile.height),
                Projectile.width,
                3,
                true);
            bool fallingNow = IsUsingFallingAnimation();
            bool hoveringNow = !groundedNow
                && Projectile.frame >= 40
                && Projectile.frame < 43;

            if (hoveringNow && !wasHoveringForPsychicWave)
            {
                SpawnPsychicFootWave(playSound: false);
            }

            if (groundedNow && !wasGroundedForPsychicWave && wasFallingForPsychicWave)
            {
                SpawnPsychicFootWave(playSound: true);
            }

            wasGroundedForPsychicWave = groundedNow;
            wasFallingForPsychicWave = fallingNow;
            wasHoveringForPsychicWave = hoveringNow;
        }

        private bool IsUsingFallingAnimation()
        {
            float horizontalSpeed = Math.Abs(Projectile.velocity.X);
            return (Projectile.velocity.Y > 4f && horizontalSpeed > 0.25f)
                || (Projectile.velocity.Y > 1f && horizontalSpeed < 0.25f);
        }

        private void SpawnPsychicFootWave(bool playSound)
        {
            if (Main.myPlayer != Projectile.owner)
            {
                return;
            }

            Vector2 footPosition = new Vector2(
                Projectile.Center.X,
                Projectile.position.Y + Projectile.height - PsychicFootWaveInset);
            int effectFlags = PsychicJumpPlatformProjectile.DrawAroundProjectileFlag;
            if (playSound)
            {
                effectFlags |= PsychicJumpPlatformProjectile.PlaySoundFlag;
            }

            PsychicJumpPlatformProjectile.SpawnPair(
                Projectile.GetSource_FromThis(),
                footPosition,
                Projectile.owner,
                1f,
                effectFlags);
        }
    }
}
