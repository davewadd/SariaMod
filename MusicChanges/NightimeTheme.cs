using Terraria;
using Terraria.ModLoader;
namespace SariaMod.MusicChanges
{
    public class NightimeTheme : ModSceneEffect
    {
        public override bool IsSceneEffectActive(Player player) => (Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneOverworldHeight && !Main.dayTime && !Main.eclipse && !Main.bloodMoon && !Main.player[Main.myPlayer].ZoneBeach && !Main.player[Main.myPlayer].ZoneDesert && !Main.player[Main.myPlayer].ZoneHallow && !Main.player[Main.myPlayer].ZoneCorrupt && !Main.player[Main.myPlayer].ZoneCrimson && !(Main.player[Main.myPlayer].ZoneSnow && Main.raining));
        public override SceneEffectPriority Priority => SceneEffectPriority.BiomeLow;
        public override int Music => MusicLoader.GetMusicSlot(Mod, "Sounds/Music/MechonisField");
    }
}