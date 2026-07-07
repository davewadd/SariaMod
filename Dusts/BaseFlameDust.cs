using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
namespace SariaMod.Dusts
{
    public abstract class BaseFlameDust : ModDust
    {
        protected virtual float InitialScale => 1.5f;
        protected virtual float VelocityMultiplier => 0.4f;
        protected virtual int InitialAlpha => -1;
        protected virtual float UpdateLightIntensity => 0.35f;
        protected virtual Vector3 MidLightColor => new Vector3(0.1f, 0.2f, 0.7f);
        protected virtual float MidLightIntensity => 1f;
        public override void OnSpawn(Dust dust)
        {
            dust.velocity *= VelocityMultiplier;
            dust.noGravity = true;
            dust.scale = InitialScale;
            if (InitialAlpha >= 0)
            {
                dust.alpha = InitialAlpha;
            }
        }
        protected virtual float GetUpdateLight(Dust dust)
        {
            return UpdateLightIntensity * dust.scale;
        }
        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            dust.rotation += dust.velocity.X * 0.15f;
            dust.scale -= 0.1f;
            float light = GetUpdateLight(dust);
            Lighting.AddLight(dust.position, light, light, light);
            if (dust.scale < 0.01f)
            {
                dust.active = false;
            }
            return false;
        }
        public override bool MidUpdate(Dust dust)
        {
            if (!dust.noGravity)
            {
                dust.velocity.Y += 0.05f;
            }
            if (dust.noLight)
            {
                return false;
            }
            float strength = dust.scale * 1.4f;
            if (strength > 1f)
            {
                strength = 1f;
            }
            Lighting.AddLight(dust.position, MidLightColor * MidLightIntensity * strength);
            return false;
        }
        public override Color? GetAlpha(Dust dust, Color lightColor)
            => new Color(lightColor.R, lightColor.G, lightColor.B, 25);
    }
}
