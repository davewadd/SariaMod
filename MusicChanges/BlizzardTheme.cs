using Terraria;
using Terraria.ModLoader;
namespace SariaMod.MusicChanges
{
    public class BlizzardTheme : ModSceneEffect
    {
        public override bool IsSceneEffectActive(Player player) => (Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneRain && Main.player[Main.myPlayer].ZoneSnow && Main.player[Main.myPlayer].ZoneOverworldHeight);
        public override SceneEffectPriority Priority => SceneEffectPriority.Environment;
        public override int Music => MusicLoader.GetMusicSlot(Mod, "Sounds/Music/Blizzard");
    }
}