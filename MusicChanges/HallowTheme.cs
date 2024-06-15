using Terraria.ModLoader;

using Terraria;

namespace SariaMod.MusicChanges
{
	public class HallowTheme : ModSceneEffect
	{
        public override bool IsSceneEffectActive(Player player) => (Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneOverworldHeight && !Main.bloodMoon && !Main.eclipse && !Main.player[Main.myPlayer].ZoneBeach && !Main.player[Main.myPlayer].ZoneDesert && Main.player[Main.myPlayer].ZoneHallow);

        public override SceneEffectPriority Priority => SceneEffectPriority.Event;
        public override int Music => MusicLoader.GetMusicSlot(Mod, "Sounds/Music/MarshNight");
           
	} 
}