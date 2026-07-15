using Microsoft.Xna.Framework;
using SariaMod.Buffs;
using SariaMod.Dusts;
using SariaMod.Gores;
using Terraria;
using Terraria.ModLoader;

namespace SariaMod.Items.Strange
{
    public partial class Saria
    {
        /// <summary>
        /// Applies the NPC Chilled palette, light, and particles to water-form Saria
        /// while her existing snowy-biome Frostburn condition is active. This is only
        /// a visual treatment and deliberately does not register any frozen gore state.
        /// </summary>
        internal bool HasColdWaterFormChilledVisual()
        {
            if (Projectile.owner < 0 || Projectile.owner >= Main.maxPlayers)
            {
                return false;
            }

            Player player = Main.player[Projectile.owner];
            return Transform == 1
                && SariaZoneSnow
                && player.HasBuff(ModContent.BuffType<Frostburn2>());
        }

        /// <summary>
        /// Keeps glow-in-the-dark body layers inside the blue Chilled palette. Without
        /// this, those layers replace the supplied tint with GhostWhite and leave bright
        /// untinted pieces over the chilled body.
        /// </summary>
        internal static Color ResolveChilledAwareGlowColor(Projectile projectile, Color lightColor)
        {
            if (projectile.ModProjectile is Saria saria && saria.HasColdWaterFormChilledVisual())
            {
                return lightColor;
            }

            return Color.Lerp(lightColor, Color.GhostWhite, 20f);
        }

        private Color ApplyColdWaterFormChilledVisuals(Color lightColor)
        {
            if (!HasColdWaterFormChilledVisual())
            {
                return lightColor;
            }

            const float effectStrength = 1f;
            float lightIntensity = effectStrength * 1.2f;
            Lighting.AddLight(
                Projectile.Center,
                0.6f * lightIntensity,
                0.85f * lightIntensity,
                1f * lightIntensity);

            if (Main.rand.NextBool(25))
            {
                Vector2 dustPosition = Projectile.Center + new Vector2(
                    Main.rand.NextFloat(-Projectile.width / 2f, Projectile.width / 2f),
                    Main.rand.NextFloat(-Projectile.height / 2f, Projectile.height / 2f));
                Vector2 dustVelocity = new Vector2(
                    Main.rand.NextFloat(-1f, 1f),
                    Main.rand.NextFloat(-1.5f, 0.5f));
                Dust fog = Dust.NewDustPerfect(
                    dustPosition,
                    ModContent.DustType<Fog>(),
                    dustVelocity,
                    0,
                    default,
                    1.2f);
                fog.noGravity = true;
            }

            if (Main.rand.NextBool(15))
            {
                Vector2 dustPosition = Projectile.Center + new Vector2(
                    Main.rand.NextFloat(-Projectile.width / 2f, Projectile.width / 2f),
                    Main.rand.NextFloat(-Projectile.height / 2f, Projectile.height / 2f));
                Vector2 dustVelocity = new Vector2(
                    Main.rand.NextFloat(-0.5f, 0.5f),
                    Main.rand.NextFloat(-0.5f, 0.5f));
                Dust snow = Dust.NewDustPerfect(
                    dustPosition,
                    ModContent.DustType<Snow2>(),
                    dustVelocity,
                    0,
                    default,
                    Main.rand.NextFloat(0.8f, 1.4f));
                snow.noGravity = true;
            }

            Color chilledColor = FrozenNPCVisualManager.ApplyFrozenPalette(lightColor);
            return Color.Lerp(lightColor, chilledColor, effectStrength);
        }
    }
}
