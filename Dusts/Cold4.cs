using Terraria;
using Terraria.ModLoader;
namespace SariaMod.Dusts
{
    public class Cold4 : ModDust
    {
        public override void OnSpawn(Dust dust)
        {
            dust.velocity *= 0.2f;
            dust.noGravity = true;
            dust.noLight = false; // Allow light emission
            dust.scale *= 1f;
            dust.alpha = 0;
        }
        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            dust.rotation += dust.velocity.X * 0.15f;
            dust.scale *= 1.01f;
            dust.alpha++;
            
            // Dampen Y velocity to prevent drifting too far vertically
            dust.velocity.Y *= 0.95f;
            // Also slightly dampen X for more stationary feel
            dust.velocity.X *= 0.98f;
            
            if (dust.alpha >= 300)
            {
                dust.active = false;
            }
            
            // Add a hint of white light - very subtle glow
            float light = 0.08f * dust.scale;
            float whiteHint = 0.06f * dust.scale;
            Lighting.AddLight(dust.position, whiteHint, whiteHint + 0.01f, light);
            
            if (dust.scale < 0.5f)
            {
                dust.active = false;
            }
            return false;
        }
    }
}