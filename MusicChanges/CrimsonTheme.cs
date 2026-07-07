using Terraria;
using Terraria.ModLoader;
namespace SariaMod.MusicChanges
{
    public class CrimsonTheme : ModSceneEffect
    {
        public override bool IsSceneEffectActive(Player player) => (Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneCrimson && Main.player[Main.myPlayer].ZoneOverworldHeight && !Main.player[Main.myPlayer].ZoneDungeon);
        public override SceneEffectPriority Priority => SceneEffectPriority.Environment;
        public override int Music => MusicLoader.GetMusicSlot(Mod, "Sounds/Music/Crimson");
    }
}