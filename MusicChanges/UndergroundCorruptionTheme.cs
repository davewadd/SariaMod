using Terraria;
using Terraria.ModLoader;
namespace SariaMod.MusicChanges
{
    public class UndergroundCorruptionTheme : ModSceneEffect
    {
        public override bool IsSceneEffectActive(Player player) => (Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneCorrupt && Main.player[Main.myPlayer].ZoneRockLayerHeight && !Main.player[Main.myPlayer].ZoneDungeon);
        public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;
        public override int Music => MusicLoader.GetMusicSlot(Mod, "Sounds/Music/UndergroundCorruption");
    }
}