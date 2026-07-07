using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
namespace SariaMod.Dusts
{
    public abstract class BaseStaticDustNormal : ModDust
    {
        protected virtual float VelocityMultiplier => 1.4f;
        protected virtual bool NoGravity => true;
        protected virtual float ScaleMultiplier => 1.2f;
        protected virtual float LightIntensity => 0.95f;
        protected virtual float RotationMultiplier => 0.15f;
        protected virtual float ScaleDecay => 0.98f;
        protected virtual float GravityVelocity => 0.05f;
        protected virtual float MidLightR => 0.1f;
        protected virtual float MidLightG => 0.2f;
        protected virtual float MidLightB => 0.7f;
        protected virtual float MidStrengthMultiplier => 2.4f;
        protected virtual int AlphaColor => 25;
        public override void OnSpawn(Dust dust)
        {
            dust.velocity *= VelocityMultiplier;
            dust.noGravity = NoGravity;
            dust.scale *= ScaleMultiplier;
        }
        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            dust.rotation += dust.velocity.X * RotationMultiplier;
            dust.scale *= ScaleDecay;
            float light = LightIntensity * dust.scale;
            Lighting.AddLight(dust.position, light, light, light);
            if (dust.scale < 0.5f)
            {
                dust.active = false;
            }
            return false;
        }
        public override bool MidUpdate(Dust dust)
        {
            if (!dust.noGravity)
            {
                dust.velocity.Y += GravityVelocity;
            }
            if (dust.noLight)
            {
                return false;
            }
            float strength = dust.scale * MidStrengthMultiplier;
            if (strength > 1f)
            {
                strength = 1f;
            }
            Lighting.AddLight(dust.position, MidLightR * strength, MidLightG * strength, MidLightB * strength);
            return false;
        }
        public override Color? GetAlpha(Dust dust, Color lightColor)
            => new Color(lightColor.R, lightColor.G, lightColor.B, AlphaColor);
    }
}
