using Terraria;
using Terraria.ModLoader;
namespace SariaMod.MusicChanges
{
    public class RainTheme : ModSceneEffect
    {
        public override bool IsSceneEffectActive(Player player) => (Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneRain && Main.dayTime && !Main.player[Main.myPlayer].ZoneSnow && !Main.player[Main.myPlayer].ZoneCorrupt && !Main.player[Main.myPlayer].ZoneCrimson && !Main.player[Main.myPlayer].ZonePeaceCandle && Main.player[Main.myPlayer].ZoneOverworldHeight);
        public override SceneEffectPriority Priority => SceneEffectPriority.BiomeMedium;
        public override int Music => MusicLoader.GetMusicSlot(Mod, "Sounds/Music/Rain");
    }
}