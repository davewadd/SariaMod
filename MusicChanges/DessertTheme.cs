using Terraria.ModLoader;

using Terraria;

namespace SariaMod.MusicChanges
{
	public class DessertTheme : ModSceneEffect
	{
        public override bool IsSceneEffectActive(Player player) => (Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneDesert && Main.dayTime && !Main.player[Main.myPlayer].ZoneBeach && !Main.player[Main.myPlayer].ZoneDungeon);

        public override SceneEffectPriority Priority => SceneEffectPriority.BiomeMedium;
        public override int Music => MusicLoader.GetMusicSlot(Mod, "Sounds/Music/Lanayru");
           
	} 
}