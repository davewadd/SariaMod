using Terraria;
using Terraria.ModLoader;
namespace SariaMod.MusicChanges
{
    public class JungleTheme : ModSceneEffect
    {
        public override bool IsSceneEffectActive(Player player) => (Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneJungle && Main.player[Main.myPlayer].ZoneOverworldHeight && !Main.player[Main.myPlayer].ZoneDungeon && Main.dayTime && !Main.player[Main.myPlayer].ZoneGlowshroom);
        public override SceneEffectPriority Priority => SceneEffectPriority.BiomeMedium;
        public override int Music => MusicLoader.GetMusicSlot(Mod, "Sounds/Music/WiseOwlForest");
    }
}