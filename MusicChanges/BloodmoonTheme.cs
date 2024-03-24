using Terraria.ModLoader;

using Terraria;

namespace SariaMod.MusicChanges
{
	public class BloodmoonTheme : ModSceneEffect
	{
        public override bool IsSceneEffectActive(Player player) => (Main.player[Main.myPlayer].active && Main.bloodMoon && Main.player[Main.myPlayer].ZoneOverworldHeight);

        public override SceneEffectPriority Priority => SceneEffectPriority.Event;
        public override int Music => MusicLoader.GetMusicSlot(Mod, "Sounds/Music/Terraria_Overhaul_Music_Eerie_Theme_of_the_B");
           
	} 
}