using Terraria.ModLoader;

using Terraria;

namespace SariaMod.MusicChanges
{
	public class OceanTheme : ModSceneEffect
	{
        public override bool IsSceneEffectActive(Player player) => (Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneBeach && !Main.dayTime && !Main.bloodMoon && Main.player[Main.myPlayer].ZoneDesert);

        public override SceneEffectPriority Priority => SceneEffectPriority.BiomeMedium;
        public override int Music => MusicLoader.GetMusicSlot(Mod, "Sounds/Music/Shipwreck");
           
	} 
}