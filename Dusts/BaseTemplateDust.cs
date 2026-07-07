using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
namespace SariaMod.Dusts
{
    public enum YBounceMode
    {
        ReflectNegative,
        ReflectPositive,
        ClampTo2,
        ClampToHalf,
        None
    }
    public abstract class BaseTemplateDust : ModDust
    {
        protected virtual float VelocityMultiplier => 0.4f;
        protected virtual bool NoLight => false;
        protected virtual float ScaleMultiplier => 1.5f;
        protected virtual float LightIntensity => 0.35f;
        protected virtual YBounceMode BounceMode => YBounceMode.ReflectNegative;
        protected virtual int InitialAlpha => -1;
        public override void OnSpawn(Dust dust)
        {
            dust.velocity *= VelocityMultiplier;
            dust.noGravity = true;
            dust.noLight = NoLight;
            dust.scale *= ScaleMultiplier;
            if (InitialAlpha >= 0)
            {
                dust.alpha = InitialAlpha;
            }
        }
        protected virtual void HandleYBounce(Dust dust)
        {
            switch (BounceMode)
            {
                case YBounceMode.ReflectNegative:
                    if (dust.velocity.Y < 0)
                        dust.velocity.Y *= -1;
                    break;
                case YBounceMode.ReflectPositive:
                    if (dust.velocity.Y > 0)
                        dust.velocity.Y *= -1;
                    break;
                case YBounceMode.ClampTo2:
                    if (dust.velocity.Y < 0)
                        dust.velocity.Y = 2f;
                    break;
                case YBounceMode.ClampToHalf:
                    if (dust.velocity.Y < 0)
                        dust.velocity.Y = .5f;
                    break;
                case YBounceMode.None:
                    break;
            }
        }
        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            dust.rotation += dust.velocity.X * 0.15f;
            HandleYBounce(dust);
            dust.scale *= 0.99f;
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
            Lighting.AddLight(dust.position, 0.1f * strength, 0.2f * strength, 0.7f * strength);
            return false;
        }
        public override Color? GetAlpha(Dust dust, Color lightColor)
            => new Color(lightColor.R, lightColor.G, lightColor.B, 25);
    }
}
