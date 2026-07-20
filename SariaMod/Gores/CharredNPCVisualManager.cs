using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using SariaMod.Buffs;

namespace SariaMod.Gores
{
    /// <summary>
    /// Keeps the client-side Charred appearance active while an NPC has Burning2,
    /// then fades it over the same duration as Chilled after the buff ends.
    /// </summary>
    public class CharredNPCVisualManager : ModSystem
    {
        private class CharredNPCData
        {
            public int Timer;

            public CharredNPCData()
            {
                Timer = 0;
            }
        }

        private static Dictionary<int, CharredNPCData> charredNPCs = new Dictionary<int, CharredNPCData>();
        private static List<int> keysToRemove = new List<int>();

        public const int CharredDurationTicks = FrozenNPCVisualManager.ChilledDurationTicks;

        // Living Charred enemies are slightly lighter than their fully burned death
        // gores, while remaining in the same dark red-orange palette family.
        public static readonly Color CharredPaletteColor = Color.Lerp(
            BurnedGoreSystem.BurnedPaletteColor,
            new Color(255, 100, 20),
            0.06f);

        public override void Load()
        {
            On.Terraria.NPC.GetNPCColorTintedByBuffs += Hook_NPC_GetNPCColorTintedByBuffs;
        }

        public override void Unload()
        {
            On.Terraria.NPC.GetNPCColorTintedByBuffs -= Hook_NPC_GetNPCColorTintedByBuffs;
            charredNPCs?.Clear();
            charredNPCs = null;
            keysToRemove?.Clear();
            keysToRemove = null;
        }

        public override void PostUpdateEverything()
        {
            if (Main.dedServ)
                return;

            keysToRemove.Clear();

            foreach (var pair in charredNPCs)
            {
                int npcIndex = pair.Key;
                if (!IsValidActiveNPC(npcIndex))
                {
                    keysToRemove.Add(npcIndex);
                    continue;
                }

                CharredNPCData data = pair.Value;
                if (Main.npc[npcIndex].HasBuff(ModContent.BuffType<Burning2>()))
                {
                    data.Timer = 0;
                    continue;
                }

                data.Timer++;
                if (data.Timer >= CharredDurationTicks)
                {
                    keysToRemove.Add(npcIndex);
                }
            }

            foreach (int npcIndex in keysToRemove)
            {
                charredNPCs.Remove(npcIndex);
            }
        }

        public static bool IsNPCCharred(int npcIndex)
        {
            // Do not require npc.active here. HitEffect can query the lingering state
            // during the death transition before the normal inactive-entry cleanup.
            return IsValidNPCIndex(npcIndex)
                && charredNPCs.TryGetValue(npcIndex, out CharredNPCData data)
                && data.Timer < CharredDurationTicks;
        }

        /// <summary>
        /// Creates or fully refreshes Charred while Burning2 is updating the NPC.
        /// </summary>
        public static void RefreshCharredEffect(int npcIndex)
        {
            if (Main.dedServ || !IsValidNPCIndex(npcIndex))
                return;

            if (charredNPCs.TryGetValue(npcIndex, out CharredNPCData data))
            {
                data.Timer = 0;
            }
            else
            {
                charredNPCs[npcIndex] = new CharredNPCData();
            }
        }

        /// <summary>
        /// Death-specific query used to select burned gores after Burning2 has ended.
        /// </summary>
        public static bool HasCharredGoreEffect(int npcIndex)
        {
            return IsNPCCharred(npcIndex);
        }

        /// <summary>
        /// General-purpose alias for callers that do not need death-specific wording.
        /// </summary>
        public static bool HasCharredEffect(int npcIndex)
        {
            return IsNPCCharred(npcIndex);
        }

        public static Color ApplyCharredPalette(Color lightingColor)
        {
            return new Color(
                (CharredPaletteColor.R * lightingColor.R) / 255,
                (CharredPaletteColor.G * lightingColor.G) / 255,
                (CharredPaletteColor.B * lightingColor.B) / 255,
                lightingColor.A);
        }

        /// <summary>
        /// Returns the charred palette with fade strength stored in alpha.
        /// </summary>
        public static Color? GetCharredTintColor(int npcIndex)
        {
            // Burning2 keeps this record at full strength, so the NPC stays visibly
            // charred beneath its active flames. Once the buff ends, the same tint
            // fades out with the lingering Charred timer.
            if (!IsNPCCharred(npcIndex))
                return null;

            CharredNPCData data = charredNPCs[npcIndex];
            float fadeStrength = 1f - data.Timer / (float)CharredDurationTicks;
            return new Color(
                CharredPaletteColor.R,
                CharredPaletteColor.G,
                CharredPaletteColor.B,
                (byte)(fadeStrength * 255f));
        }

        public override void OnWorldLoad()
        {
            ClearAllCharredNPCs();
        }

        public override void OnWorldUnload()
        {
            ClearAllCharredNPCs();
        }

        public override void PreSaveAndQuit()
        {
            ClearAllCharredNPCs();
        }

        public static void ClearAllCharredNPCs()
        {
            charredNPCs?.Clear();
        }

        private static bool IsValidNPCIndex(int npcIndex)
        {
            return npcIndex >= 0 && npcIndex < Main.npc.Length;
        }

        private static bool IsValidActiveNPC(int npcIndex)
        {
            return IsValidNPCIndex(npcIndex) && Main.npc[npcIndex].active;
        }

        /// <summary>
        /// Vanilla buff coloring is applied after GlobalNPC.DrawEffects. Apply Charred
        /// after that final vanilla stage so no later color transform can erase it.
        /// The unmodified input is the NPC's lighting color, matching burned gore math.
        /// </summary>
        private Color Hook_NPC_GetNPCColorTintedByBuffs(
            On.Terraria.NPC.orig_GetNPCColorTintedByBuffs orig,
            NPC self,
            Color npcColor)
        {
            Color? charredTint = GetCharredTintColor(self.whoAmI);
            if (!charredTint.HasValue)
                return orig(self, npcColor);

            bool validLocalPlayer = Main.myPlayer >= 0 && Main.myPlayer < Main.maxPlayers;
            Player localPlayer = validLocalPlayer ? Main.player[Main.myPlayer] : null;
            bool suppressHunterTint = localPlayer != null
                && localPlayer.detectCreature;

            Color vanillaBuffColor;

            if (!suppressHunterTint)
            {
                vanillaBuffColor = orig(self, npcColor);
            }
            else
            {
                bool hunterWasActive = localPlayer.detectCreature;
                localPlayer.detectCreature = false;
                try
                {
                    vanillaBuffColor = orig(self, npcColor);
                }
                finally
                {
                    localPlayer.detectCreature = hunterWasActive;
                }
            }

            float blendStrength = charredTint.Value.A / 255f;
            Color targetColor = ApplyCharredPalette(npcColor);
            return Color.Lerp(vanillaBuffColor, targetColor, blendStrength);
        }
    }
}
