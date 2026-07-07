using Terraria;
using Terraria.ModLoader;
namespace SariaMod.MusicChanges
{
    public class JungleUndergroundTheme : ModSceneEffect
    {
        public override bool IsSceneEffectActive(Player player) => (Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneJungle && Main.player[Main.myPlayer].ZoneDirtLayerHeight && !Main.player[Main.myPlayer].ZoneDungeon && !Main.player[Main.myPlayer].ZoneGlowshroom);
        public override SceneEffectPriority Priority => SceneEffectPriority.BiomeMedium;
        public override int Music => MusicLoader.GetMusicSlot(Mod, "Sounds/Music/JungleNight");
    }
}