using Terraria;
using Terraria.ModLoader;
namespace SariaMod.Dusts
{
    public abstract class BaseStaticDust : ModDust
    {
        protected virtual float VelocityMultiplier => 0.4f;
        protected virtual bool NoGravity => true;
        protected virtual bool NoLight => true;
        protected virtual float ScaleMultiplier => 2.5f;
        protected virtual int InitialAlpha => 100;
        protected virtual float LightIntensity => 0.35f;
        public override void OnSpawn(Dust dust)
        {
            dust.velocity *= VelocityMultiplier;
            dust.noGravity = NoGravity;
            dust.noLight = NoLight;
            dust.scale *= ScaleMultiplier;
            dust.alpha = InitialAlpha;
        }
        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            dust.alpha += 1;
            if (dust.alpha == 300f)
            {
                dust.active = false;
            }
            dust.scale *= 0.99f;
            float light = LightIntensity * dust.scale;
            Lighting.AddLight(dust.position, light, light, light);
            if (dust.scale < 0.5f)
            {
                dust.active = false;
            }
            return false;
        }
    }
}
