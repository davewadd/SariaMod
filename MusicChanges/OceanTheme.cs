using Terraria;
using Terraria.ModLoader;
namespace SariaMod.MusicChanges
{
    public class OceanTheme : ModSceneEffect
    {
        public override bool IsSceneEffectActive(Player player) => (Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneBeach && !Main.dayTime && !Main.bloodMoon);
        public override SceneEffectPriority Priority => SceneEffectPriority.BiomeMedium;
        public override int Music => MusicLoader.GetMusicSlot(Mod, "Sounds/Music/Shipwreck");
    }
}