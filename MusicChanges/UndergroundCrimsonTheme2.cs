using Terraria;
using Terraria.ModLoader;
namespace SariaMod.MusicChanges
{
    public class UndergroundCrimsonTheme2 : ModSceneEffect
    {
        public override bool IsSceneEffectActive(Player player) => (Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneCrimson && Main.player[Main.myPlayer].ZoneDirtLayerHeight && !Main.player[Main.myPlayer].ZoneDungeon);
        public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;
        public override int Music => MusicLoader.GetMusicSlot(Mod, "Sounds/Music/UndergroundCrimson");
    }
}