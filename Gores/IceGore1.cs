using SariaMod.Items.Bands;
using SariaMod.Items.zPearls;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using SariaMod.Items.Emerald;
using Microsoft.Xna.Framework;
using SariaMod.Buffs;
using SariaMod.Dusts;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.UI.Elements;
using ReLogic.Content;
using Terraria.DataStructures;
using System;
namespace SariaMod.Gores
{
    public class IceGore1 : ModGore
    {
        public override bool Update(Gore gore)
        {
            // The first tick this gore appears, set its timeLeft to 100.
            // numFrames is a reliable way to check if this is the first tick.
            if (gore.timeLeft == 1)
            {
                int backGoreType = ModContent.GoreType<IceGore2>();
                if (gore.timeLeft == 1)
                {
                    // Set the flag to true so this only happens once.
                    // Get the type of the other gore you want to spawn.
                    int newGoreType = ModContent.GoreType<IceGore2>();
                    // Use a valid entity source from the current gore.
                    var entitySource = new EntitySource_WorldEvent();
                    for (int G = 0; G < 3; G++)
                    {
                        // Use Gore.NewGore with the correct entity source.
                        Gore.NewGore(entitySource, gore.position, new Vector2(Main.rand.Next(-6, 7), Main.rand.Next(-6, 7)), newGoreType, 2f);
                    }
                    // Play the sound after spawning the gore, but only once.
                    SoundEngine.PlaySound(SoundID.Item27, gore.position);
                    gore.active = false;
                }
            }
            if (Main.rand.NextBool(30))
            {
                float radius = (float)Math.Sqrt(Main.rand.Next(10 * 10));
                double angle = Main.rand.NextDouble() * 5.0 * Math.PI;
                Dust.NewDust(new Vector2((gore.position.X) + radius * (float)Math.Cos(angle), (gore.position.Y) + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<Snow2>(), 0f, 0f, 0, default(Color), 1.5f);
            }
            float light = 0.8f;
            Lighting.AddLight(gore.position, light, light, light);
            // Optional: Make the gore freeze in place after a few ticks.
            // if (gore.timeLeft > 80)
            // {
            //     gore.velocity *= 0.9f;
            // }
            // This must be returned to allow for the default TModLoader gore updating to also happen,
            // which includes the alpha fade-out over time.
            return true;
        }
    }
}
