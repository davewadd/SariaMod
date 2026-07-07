using Terraria;
using Terraria.ModLoader;
namespace SariaMod.MusicChanges
{
    public class HallowTheme : ModSceneEffect
    {
        public override bool IsSceneEffectActive(Player player) => (Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneOverworldHeight && !Main.bloodMoon && !Main.eclipse && !Main.player[Main.myPlayer].ZoneBeach && !Main.player[Main.myPlayer].ZoneDesert && Main.player[Main.myPlayer].ZoneHallow);
        public override SceneEffectPriority Priority => SceneEffectPriority.Environment;
        public override int Music => MusicLoader.GetMusicSlot(Mod, "Sounds/Music/MarshNight");
    }
}