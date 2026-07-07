using Terraria;
using Terraria.ModLoader;
namespace SariaMod.MusicChanges
{
    public class DessertTheme : ModSceneEffect
    {
        // ZoneOverworldHeight gate: the underground desert ALSO sets ZoneDesert (it is a
        // tile-count zone), so without a surface check this theme fired underground and
        // outranked vanilla's underground-desert track. Surface only, like the other themes.
        public override bool IsSceneEffectActive(Player player) => (Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneDesert && Main.player[Main.myPlayer].ZoneOverworldHeight && Main.dayTime && !Main.player[Main.myPlayer].ZoneBeach && !Main.player[Main.myPlayer].ZoneDungeon);
        public override SceneEffectPriority Priority => SceneEffectPriority.BiomeMedium;
        public override int Music => MusicLoader.GetMusicSlot(Mod, "Sounds/Music/Lanayru");
    }
}