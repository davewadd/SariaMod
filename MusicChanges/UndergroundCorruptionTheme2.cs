using Terraria;
using Terraria.ModLoader;
namespace SariaMod.MusicChanges
{
    public class UndergroundCorruptionTheme2 : ModSceneEffect
    {
        public override bool IsSceneEffectActive(Player player) => (Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneCorrupt && Main.player[Main.myPlayer].ZoneDirtLayerHeight && !Main.player[Main.myPlayer].ZoneDungeon);
        public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;
        public override int Music => MusicLoader.GetMusicSlot(Mod, "Sounds/Music/UndergroundCorruption");
    }
}