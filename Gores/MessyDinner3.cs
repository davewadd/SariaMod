using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace SariaMod.Gores
{
    public class MessyDinner3 : ModGore
    {
        public override bool Update(Gore gore)
        {
            // The first tick this gore appears, set its timeLeft to 100.
            // numFrames is a reliable way to check if this is the first tick.
            if (gore.numFrames == 0)
            {
                gore.scale = 50f; // Starts at 2x its original size
                gore.timeLeft = 180; // Lasts for 100 ticks
            }
            gore.alpha += 1;
            gore.sticky = true;
            // Optional: Make the gore freeze in place after a few ticks.
            // if (gore.timeLeft > 80)
            // {
            //     gore.velocity *= 0.9f;
            // }
            float light = 0.25f * gore.scale;
            Lighting.AddLight(gore.position, light, light, light);
            // This must be returned to allow for the default TModLoader gore updating to also happen,
            // which includes the alpha fade-out over time.
            return true;
        }
    }
}
