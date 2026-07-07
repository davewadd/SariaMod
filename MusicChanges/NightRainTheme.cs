using Terraria;
using Terraria.ModLoader;
namespace SariaMod.MusicChanges
{
    public class NightRainTheme : ModSceneEffect
    {
        public override bool IsSceneEffectActive(Player player) => (Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneRain && !Main.dayTime && !Main.player[Main.myPlayer].ZoneSnow && !Main.player[Main.myPlayer].ZonePeaceCandle && !Main.bloodMoon && Main.player[Main.myPlayer].ZoneOverworldHeight && !Main.player[Main.myPlayer].ZoneGlowshroom && !Main.player[Main.myPlayer].ZoneDesert);
        public override SceneEffectPriority Priority => SceneEffectPriority.BiomeMedium;
        public override int Music => MusicLoader.GetMusicSlot(Mod, "Sounds/Music/Shipwreck");
    }
}