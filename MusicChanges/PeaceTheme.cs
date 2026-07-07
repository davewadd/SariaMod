using Terraria;
using Terraria.ModLoader;
namespace SariaMod.MusicChanges
{
    public class PeaceTheme : ModSceneEffect
    {
        public override bool IsSceneEffectActive(Player player)
        {
            if (!Main.player[Main.myPlayer].active)
                return false;

            // CalmMind is a BUFF flag, not a position/zone flag, so it survived the
            // SariaPerceptionSystem redirect untouched: even while spectating Saria
            // somewhere else, the real player's own Calming Candle buff kept playing
            // Peace Candle music from the real player's location. While camera-on-Saria
            // is active, read HER candle environment instead so the track follows what
            // she is actually near, exclusively.
            if (SariaPerceptionSystem.TryGetLocalCameraSaria(out Projectile sariaProj))
                return sariaProj.ModProjectile is Items.Strange.Saria saria && saria.SariaHasCalmMindCandle;

            return Main.player[Main.myPlayer].Fairy().CalmMind;
        }
        public override SceneEffectPriority Priority => SceneEffectPriority.Event;
        public override int Music => MusicLoader.GetMusicSlot(Mod, "Sounds/Music/A_Lonely_Figure");
    }
}