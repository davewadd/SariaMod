using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace SariaMod.Dusts
{
    public class BubbleDust : ModDust
    {
        public override void OnSpawn(Dust dust)
        {
            dust.velocity *= 0.2f;
            dust.noGravity = true;
            dust.noLight = false;
            // Smaller, more varied sizes - most are small, some medium, few large
            float sizeRoll = Main.rand.NextFloat();
            if (sizeRoll < 0.6f)
                dust.scale = Main.rand.NextFloat(0.5f, 0.8f); // 60% small
            else if (sizeRoll < 0.9f)
                dust.scale = Main.rand.NextFloat(0.8f, 1.2f); // 30% medium
            else
                dust.scale = Main.rand.NextFloat(1.2f, 1.6f); // 10% large
            
            dust.alpha = 70;
        }

        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            dust.rotation += dust.velocity.X * 0.1f;
            
            // Emit faintest light blue light
            float lightStrength = dust.scale * 0.15f;
            Lighting.AddLight(dust.position, 0.1f * lightStrength, 0.2f * lightStrength, 0.4f * lightStrength);
            
            // Check if bubble is still in ANY liquid (Water, Lava, Honey, Modded)
            // We check the top edge of the bubble to ensure it pops as soon as it breaches the surface.
            // dust.position is roughly the center. 4 pixels is roughly the radius at scale 1.0.
            float radius = 4f * dust.scale;
            Vector2 topEdge = dust.position - new Vector2(0, radius);
            
            if (!Collision.WetCollision(topEdge, 1, 1))
            {
                dust.active = false;
                return false;
            }
            
            // Slowly shrink while in water
            dust.scale *= 0.997f;
            
            // Natural despawn
            if (dust.scale < 0.3f)
            {
                dust.active = false;
            }
            
            return false;
        }
    }
}