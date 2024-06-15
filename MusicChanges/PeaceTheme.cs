using Terraria.ModLoader;

using Terraria;

namespace SariaMod.MusicChanges
{
	public class PeaceTheme : ModSceneEffect
	{
        public override bool IsSceneEffectActive(Player player) => (Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZonePeaceCandle);

        public override SceneEffectPriority Priority => SceneEffectPriority.Environment;
        public override int Music => MusicLoader.GetMusicSlot(Mod, "Sounds/Music/A_Lonely_Figure");
           
	} 
}