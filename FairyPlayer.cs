using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using SariaMod.Items.Emerald;
using Terraria.DataStructures;
using SariaMod.Items.Bands;
using SariaMod.Items.zPearls;
using Terraria.Audio;
using Microsoft.Xna.Framework;
using SariaMod.Buffs;
using SariaMod.Diagnostics;
using SariaMod.Dusts;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.UI.Elements;
using ReLogic.Content;
using System;
using System.Reflection;
using SariaMod.Items.Ruby;
using System.Collections.Generic;
using SariaMod.Items.Amber;
using SariaMod.Items.Amethyst;
using SariaMod.Items.Sapphire;
using SariaMod.Items.Topaz;
using SariaMod.Items.zTalking;
using Terraria.Localization;
using Terraria.Map;
using Terraria.GameContent;
using SariaMod.Items.Strange;
using SariaMod.Items.zDinner;
using SariaMod.Gores;
namespace SariaMod
{
    public class SariaLevelUpTier
    {
        public int RequiredXP { get; set; }
        public Func<bool> Condition { get; set; }
    }
    public class FairyPlayer : ModPlayer
    {
        private static Dictionary<int, SariaLevelUpTier> levelUpTiers;
        public bool BloodmoonBuff;
        public bool ShowFogBreath;
        public bool LinkCable;
        public Vector2 LinkCableTarget = Vector2.Zero;
        public int FogBreathPacketCooldown;
        public bool SariaCurseD;
        public bool Statlowered;
        public bool Statrisen;
        public bool Sickness;
        public bool externalColdImmunity;
        public bool Burning2;
        public bool GhostBurning;
        public bool PassiveHealing;
        public bool Frostburn2;
        public bool Frostburn3;
        public bool EclipseBuff;
        public int Sarialevel;
        private bool _pendingSariaLevelSync;
        public int StoredHealth;
        public int Serving;
        public bool PlayerGreenGem;
        public bool PlayerPurpleGem;
        public bool PlayerSilverGem;
        public bool PlayerisPsychic;
        public bool PlayerisWater;
        public bool PlayerisFire;
        public bool PlayerisElectric;
        public bool PlayerisRock;
        public bool PlayerisBug;
        public bool PlayerisGhost;
        public bool PlayerisFairy;
        public bool PlayercanCharge;
        public bool holdingleft;
        public bool holdingright;
        public bool holdingdown;
        public bool SariaUnlockWater;
        public bool SariaUnlockFire;
        public bool SariaUnlockElectric;
        public bool SariaUnlockRock;
        public bool SariaUnlockBug;
        public bool SariaUnlockGhost;
        public bool SariaUnlockFairy;
        public bool SariaUnlockPsychic2;
        public bool SariaUnlockWater2;
        public bool SariaUnlockFire2;
        public bool SariaUnlockElectric2;
        public bool SariaUnlockRock2;
        public bool SariaUnlockBug2;
        public bool SariaUnlockGhost2;
        public bool SariaUnlockFairy2;
        public bool SariaUpgrade1;
        public bool SariaUpgrade2;
        public bool SariaUpgrade3;
        public bool SariaUpgrade4;
        public bool SariaUpgrade5;
        public bool SariaUpgrade6;
        public bool SariaUpgrade7;
        public bool SariaUpgrade8;
        public bool SariaUpgrade9;
        public bool SariaUpgrade10;
        public bool SariaUpgrade11;
        public bool SariaUpgrade12;
        public bool SariaUpgrade13;
        public bool SariaUpgrade14;
        public bool SariaUpgrade15;
        public bool SariaUpgrade16;
        public bool SariaUpgrade17;
        public bool SariaUpgrade18;
        public bool SariaUpgrade19;
        public bool SariaUpgrade20;
        public bool SariaUpgrade21;
        public bool CalmMind;
        public bool CorruptMind;
        public bool SoftStepShimmerImmune;
        public int FreezingTemp;
        public int SariaXp;
        public int Timer;
        public int XPBarLevel;
        public int TMPoints;
        public int TMPointsUsed;
        public int EVs;
        public int EVsUsed;
        public int DinnerHoldTimer;
        public int lastHeldItemType;
        public int KingsDinnerCooldownTimer = 0;
        public const int KingsDinnerResetTime = 240;

        public bool HealBallRightHoldActive;
        public int HealBallRightHoldTimer;

        public bool HealBallRightClick;
        private bool healBallPrevRightDown;

        public int totalTalkingTime;
        public int smallTalkingTime;
        private bool wasDialogueActive;

        public bool DebugPanelOpen;
        public bool NetworkProfilerOpen;

        // Meter bar screen-offset position (relative to screen centre). Per-player, persisted.
        public float MeterBarPosX = 0f;
        public float MeterBarPosY = 250f;

        // Dialogue panel screen-offset position (relative to screen centre). Per-player, persisted.
        public float DialoguePanelPosX = 0f;
        public float DialoguePanelPosY = 176f;

        public override void ResetEffects()
        {
            SariaCurseD = false;
            Statrisen = false;
            Statlowered = false;
            Sickness = false;
            CalmMind = false;
            CorruptMind = false;
            SoftStepShimmerImmune = false;
            externalColdImmunity = false;
            BloodmoonBuff = false;
            PassiveHealing = false;
            Burning2 = false;
            GhostBurning = false;
            Frostburn2 = false;
            Frostburn3 = false;
            EclipseBuff = false;
            PlayerisPsychic = false;
            PlayerisWater = false;
            PlayerisFire = false;
            PlayerisElectric = false;
            PlayerisRock = false;
            PlayerisBug = false;
            PlayerisGhost = false;
            PlayerisFairy = false;
        }
        public override void UpdateDead()
        {
            Statrisen = false;
            Statlowered = false;
            SariaCurseD = false;
            Sickness = false;
            Burning2 = false;
            GhostBurning = false;
            CalmMind = false;
            CorruptMind = false;
            SoftStepShimmerImmune = false;
            PassiveHealing = false;
            externalColdImmunity = false;
            BloodmoonBuff = false;
            Frostburn2 = false;
            Frostburn3 = false;
            EclipseBuff = false;
            PlayerisPsychic = false;
            PlayerisWater = false;
            PlayerisFire = false;
            PlayerisElectric = false;
            PlayerisRock = false;
            PlayerisBug = false;
            PlayerisGhost = false;
            PlayerisFairy = false;
            HealBallRightHoldActive = false;
            HealBallRightHoldTimer = 0;
        }
        public override void UpdateBadLifeRegen()
        {
            if (Timer < 40)
            {
                Timer++;
            }
            if (Frostburn3)
            {
                if (Player.lifeRegen > 0)
                {
                    Player.lifeRegen = 0;
                }
                Player.lifeRegenTime = 0;
                Player.lifeRegen -= 30;
            }
            if (Frostburn2)
            {
                if (Player.lifeRegen > 0)
                {
                    Player.lifeRegen = 0;
                }
                Player.lifeRegenTime = 0;
                Player.lifeRegen -= 30;
            }
            if (Sickness)
            {
                {
                    Player.statDefense = 1;
                    if (Player.statLife > ((Player.statLifeMax2) / 3))
                    {
                        if (Player.lifeRegen > 0)
                        {
                            Player.lifeRegen = 0;
                        }
                        Player.lifeRegenTime = 0;
                        Player.lifeRegen -= 32;
                    }
                }
            }
            if (PassiveHealing)
            {
                if (Timer >= 40 && (Player.statLife < Player.statLifeMax2))
                {
                    Player.Heal((3));
                    Timer = 0;
                }
            }
            if (Burning2)
            {
                if (Player.lifeRegen > 0)
                {
                    Player.lifeRegen = 0;
                }
                Player.lifeRegenTime = 0;
                Player.lifeRegen -= 32;
            }
            if (GhostBurning)
            {
                if (Player.lifeRegen > 0)
                {
                    Player.lifeRegen = 0;
                }
                Player.lifeRegenTime = 0;
                Player.lifeRegen -= 32;
            }
            if (SariaCurseD)
            {
                if (Player.lifeRegen > 0)
                {
                    Player.lifeRegen = 0;
                }
                Player.lifeRegenTime = 0;
                Player.lifeRegen -= 16;
            }
            if (Player.HasBuff(ModContent.BuffType<TriforceofCourage>()))
            {
                Player.statDefense += (Player.statDefense / 4);
            }
            if (Player.HasBuff(ModContent.BuffType<TriforceofPower>()))
            {
                Player.statDefense -= (Player.statDefense / 4);
            }
            if (Statlowered)
            {
                Player.statDefense -= (Player.statDefense / 4) * 3;
                Player.statLifeMax2 -= 50;
            }
            if (Statrisen)
            {
                Player.statDefense += Player.statDefense / 4;
            }
            if (BloodmoonBuff)
            {
                if (Player.statLife > ((Player.statLifeMax2) / 3))
                {
                    if (Player.lifeRegen > 0)
                    {
                        Player.lifeRegen = 0;
                    }
                    Player.lifeRegenTime = 0;
                    Player.lifeRegen -= 16;
                }
            }
            if (EclipseBuff)
            {
                if (Player.statLife > ((Player.statLifeMax2) / 3))
                {
                    if (Player.lifeRegen > 0)
                    {
                        Player.lifeRegen = 0;
                    }
                    Player.lifeRegenTime = 0;
                    Player.lifeRegen -= 16;
                }
            }
        }
        public override void Kill(double damage, int hitDirection, bool pvp, Terraria.DataStructures.PlayerDeathReason damageSource)
        {
            
            // Check if the player is holding the KingsDinner item when they die.
            // Player.HeldItem is the item in the currently selected hotbar slot.
            if (Player.HeldItem.type == ModContent.ItemType<Items.zDinner.KingsDinner>())
            {
                Projectile.NewProjectile(Player.GetSource_Death(), Player.Center, Vector2.Zero, ModContent.ProjectileType<Dinner>(), 0, 0, Player.whoAmI);
                // Ensure the projectile spawning only happens on the server (or owning client) in multiplayer.
                // This prevents duplicate projectiles from being spawned.
                {
                    // Roll a random number to select one of the two projectiles.
                    int rand = Main.rand.Next(2); // 0 or 1
                    int projectileType;
                    if (rand == 0)
                    {
                        // Spawn the first projectile type
                        projectileType = ModContent.ProjectileType<DinnerDeathSound1>();
                    }
                    else
                    {
                        // Spawn the second projectile type
                        projectileType = ModContent.ProjectileType<DinnerDeathSound2>();
                    }
                    // Spawn the projectile at the player's last position.
                    if (Main.myPlayer == Player.whoAmI) Projectile.NewProjectile(Player.GetSource_Death(), Player.Center, Vector2.Zero, projectileType, 0, 0, Player.whoAmI);
                  
                }
            }
        }
        public override void SaveData(TagCompound tag)
        {
            tag["Sarialevel"] = Sarialevel;
            tag["Serving"] = Serving;
            tag["StoredHealth"] = StoredHealth;
            tag["SariaXp"] = SariaXp;
            tag["FreezingTemp"] = FreezingTemp;
            tag["XPBarLevel"] = XPBarLevel;
            tag["TMPoints"] = TMPoints;
            tag["TMPointsUsed"] = TMPointsUsed;
            tag["totalTalkingTime"] = totalTalkingTime;
            tag["SariaUnlockWater"] = SariaUnlockWater;
            tag["SariaUnlockFire"] = SariaUnlockFire;
            tag["SariaUnlockElectric"] = SariaUnlockElectric;
            tag["SariaUnlockRock"] = SariaUnlockRock;
            tag["SariaUnlockBug"] = SariaUnlockBug;
            tag["SariaUnlockGhost"] = SariaUnlockGhost;
            tag["SariaUnlockFairy"] = SariaUnlockFairy;
            tag["SariaUnlockPsychic2"] = SariaUnlockPsychic2;
            tag["SariaUnlockWater2"] = SariaUnlockWater2;
            tag["SariaUnlockFire2"] = SariaUnlockFire2;
            tag["SariaUnlockElectric2"] = SariaUnlockElectric2;
            tag["SariaUnlockRock2"] = SariaUnlockRock2;
            tag["SariaUnlockBug2"] = SariaUnlockBug2;
            tag["SariaUnlockGhost2"] = SariaUnlockGhost2;
            tag["SariaUnlockFairy2"] = SariaUnlockFairy2;
            tag["SariaUpgrade1"] = SariaUpgrade1;
            tag["SariaUpgrade2"] = SariaUpgrade2;
            tag["SariaUpgrade3"] = SariaUpgrade3;
            tag["SariaUpgrade4"] = SariaUpgrade4;
            tag["SariaUpgrade5"] = SariaUpgrade5;
            tag["SariaUpgrade6"] = SariaUpgrade6;
            tag["SariaUpgrade7"] = SariaUpgrade7;
            tag["SariaUpgrade8"] = SariaUpgrade8;
            tag["SariaUpgrade9"] = SariaUpgrade9;
            tag["SariaUpgrade10"] = SariaUpgrade10;
            tag["SariaUpgrade11"] = SariaUpgrade11;
            tag["SariaUpgrade12"] = SariaUpgrade12;
            tag["SariaUpgrade13"] = SariaUpgrade13;
            tag["SariaUpgrade14"] = SariaUpgrade14;
            tag["SariaUpgrade15"] = SariaUpgrade15;
            tag["SariaUpgrade16"] = SariaUpgrade16;
            tag["SariaUpgrade17"] = SariaUpgrade17;
            tag["SariaUpgrade18"] = SariaUpgrade18;
            tag["SariaUpgrade19"] = SariaUpgrade19;
            tag["SariaUpgrade20"] = SariaUpgrade20;
            tag["SariaUpgrade21"] = SariaUpgrade21;
            tag["DebugPanelOpen"] = DebugPanelOpen;
            tag["NetworkProfilerOpen"] = NetworkProfilerOpen;
            tag["MeterBarPosX"] = MeterBarPosX;
            tag["MeterBarPosY"] = MeterBarPosY;
            tag["DialoguePanelPosX"] = DialoguePanelPosX;
            tag["DialoguePanelPosY"] = DialoguePanelPosY;
            tag["FeelingRodMood"]     = (int)FeelingRodUISystem.SelectedMood;
            tag["FeelingRodTimer"]    = FeelingRodUISystem.SelectedTimer;
            tag["FeelingRodPriority"] = FeelingRodUISystem.SelectedPriority;
        }
        public override void LoadData(TagCompound tag)
        {
            Sarialevel = tag.GetInt("Sarialevel");
            Serving = tag.GetInt("Serving");
            StoredHealth = tag.GetInt("StoredHealth");
            SariaXp = tag.GetInt("SariaXp");
            FreezingTemp = tag.GetInt("FreezingTemp");
            XPBarLevel = tag.GetInt("XPBarLevel");
            TMPoints = tag.GetInt("TMPoints");
            TMPointsUsed = tag.GetInt("TMPointsUsed");
            totalTalkingTime = tag.GetInt("totalTalkingTime");
            SariaUnlockWater = tag.GetBool("SariaUnlockWater");
            SariaUnlockFire = tag.GetBool("SariaUnlockFire");
            SariaUnlockElectric = tag.GetBool("SariaUnlockElectric");
            SariaUnlockRock = tag.GetBool("SariaUnlockRock");
            SariaUnlockBug = tag.GetBool("SariaUnlockBug");
            SariaUnlockGhost = tag.GetBool("SariaUnlockGhost");
            SariaUnlockFairy = tag.GetBool("SariaUnlockFairy");
            SariaUnlockPsychic2 = tag.GetBool("SariaUnlockPsychic2");
            SariaUnlockWater2 = tag.GetBool("SariaUnlockWater2");
            SariaUnlockFire2 = tag.GetBool("SariaUnlockFire2");
            SariaUnlockElectric2 = tag.GetBool("SariaUnlockElectric2");
            SariaUnlockRock2 = tag.GetBool("SariaUnlockRock2");
            SariaUnlockBug2 = tag.GetBool("SariaUnlockBug2");
            SariaUnlockGhost2 = tag.GetBool("SariaUnlockGhost2");
            SariaUnlockFairy2 = tag.GetBool("SariaUnlockFairy2");
            SariaUpgrade1 = tag.GetBool("SariaUpgrade1");
            SariaUpgrade2 = tag.GetBool("SariaUpgrade2");
            SariaUpgrade3 = tag.GetBool("SariaUpgrade3");
            SariaUpgrade4 = tag.GetBool("SariaUpgrade4");
            SariaUpgrade5 = tag.GetBool("SariaUpgrade5");
            SariaUpgrade6 = tag.GetBool("SariaUpgrade6");
            SariaUpgrade7 = tag.GetBool("SariaUpgrade7");
            SariaUpgrade8 = tag.GetBool("SariaUpgrade8");
            SariaUpgrade9 = tag.GetBool("SariaUpgrade9");
            SariaUpgrade10 = tag.GetBool("SariaUpgrade10");
            SariaUpgrade11 = tag.GetBool("SariaUpgrade11");
            SariaUpgrade12 = tag.GetBool("SariaUpgrade12");
            SariaUpgrade13 = tag.GetBool("SariaUpgrade13");
            SariaUpgrade14 = tag.GetBool("SariaUpgrade14");
            SariaUpgrade15 = tag.GetBool("SariaUpgrade15");
            SariaUpgrade16 = tag.GetBool("SariaUpgrade16");
            SariaUpgrade17 = tag.GetBool("SariaUpgrade17");
            SariaUpgrade18 = tag.GetBool("SariaUpgrade18");
            SariaUpgrade19 = tag.GetBool("SariaUpgrade19");
            SariaUpgrade20 = tag.GetBool("SariaUpgrade20");
            SariaUpgrade21 = tag.GetBool("SariaUpgrade21");
            DebugPanelOpen = tag.GetBool("DebugPanelOpen");
            NetworkProfilerOpen = tag.GetBool("NetworkProfilerOpen");
            MeterBarPosX = tag.ContainsKey("MeterBarPosX") ? tag.GetFloat("MeterBarPosX") : 0f;
            MeterBarPosY = tag.ContainsKey("MeterBarPosY") ? tag.GetFloat("MeterBarPosY") : 250f;
            DialoguePanelPosX = tag.ContainsKey("DialoguePanelPosX") ? tag.GetFloat("DialoguePanelPosX") : 0f;
            DialoguePanelPosY = tag.ContainsKey("DialoguePanelPosY") ? tag.GetFloat("DialoguePanelPosY") : 176f;
            if (tag.ContainsKey("FeelingRodMood"))
                FeelingRodUISystem.SelectedMood = (MoodState)tag.GetInt("FeelingRodMood");
            if (tag.ContainsKey("FeelingRodTimer"))
                FeelingRodUISystem.SelectedTimer = tag.GetInt("FeelingRodTimer");
            if (tag.ContainsKey("FeelingRodPriority"))
                FeelingRodUISystem.SelectedPriority = tag.GetInt("FeelingRodPriority");
        }
        public override void Load()
        {
            levelUpTiers = new Dictionary<int, SariaLevelUpTier>
            {
                { 1, new SariaLevelUpTier { RequiredXP = 3000, Condition = () => NPC.downedSlimeKing } },
                { 2, new SariaLevelUpTier { RequiredXP = 9000, Condition = () => NPC.downedQueenBee } },
                { 3, new SariaLevelUpTier { RequiredXP = 20000, Condition = () => Main.hardMode && NPC.downedMechBossAny } },
                { 4, new SariaLevelUpTier { RequiredXP = 40000, Condition = () => NPC.downedMechBossAny } },
                { 5, new SariaLevelUpTier { RequiredXP = 80000, Condition = () => NPC.downedPlantBoss } },
                { 6, new SariaLevelUpTier { RequiredXP = 240000, Condition = () => NPC.downedFishron } }
            };
        }
        public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
        {
            // Push this player's LinkCable state through the standard tML sync path.
            // tML invokes this in TWO directions:
            //   * on a CLIENT when it joins (fromWho == us, toWho == -1): announce our
            //     own state to the server/everyone;
            //   * on the SERVER when introducing an EXISTING player (fromWho) to a
            //     late joiner (toWho): forward the state the server already holds.
            // The old client-only guard skipped the second direction entirely, so a
            // client that joined AFTER someone toggled LinkCable never learned about
            // it — their copy of the owner's flag stayed false and every
            // LinkCable-conditional guard (e.g. Ztarget's silence-in-guide-mode check)
            // misfired on that client (the "targeting sound on other clients" report).
            // The server runs NPC.SpawnNPC too, so it MUST know who has LinkCable
            // engaged or split spawning never activates there. Mid-session toggles are
            // covered by the SyncLinkCable packet from HealBall.
            if (Main.netMode == NetmodeID.SinglePlayer)
                return;

            ModPacket packet = Mod.GetPacket();
            packet.Write((byte)SariaMod.SoundMessageType.SyncLinkCable);
            packet.Write(Player.whoAmI);
            packet.Write(LinkCable);
            packet.Send(toWho, fromWho);
        }

        // Method to add SariaXp and check for level ups
        public void AddSariaXp(int amount)
        {
            // Only add XP if the player is not at max level
            if (Sarialevel >= 6) // Assumes 6 is the max level
            {
                return;
            }
            // Accumulate XP locally
            SariaXp += amount;
            bool leveledUp = false;
            // Check for level ups and process them
            while (levelUpTiers.TryGetValue(Sarialevel + 1, out SariaLevelUpTier nextTier) && SariaXp >= nextTier.RequiredXP && nextTier.Condition())
            {
                leveledUp = true;
                Sarialevel++;
                // Check if the new XP should be reset (all except final level)
                if (Sarialevel < 6)
                {
                    SariaXp = 0;
                }
            }
            // Only play effects if a level up occurred.
            if (leveledUp)
            {
                PlayLevelUpEffects();
            }
        }
        // Separate method for the reusable level-up effects
        private void PlayLevelUpEffects()
        {
            for (int j = 0; j < 72; j++)
            {
                Dust dust = Dust.NewDustPerfect(Player.Center, 113);
                dust.velocity = ((float)Math.PI * 2f * Vector2.Dot(((float)j / 72f * ((float)Math.PI * 2f)).ToRotationVector2(), Player.velocity.SafeNormalize(Vector2.UnitY).RotatedBy((float)j / 72f * ((float)Math.PI * -2f)))).ToRotationVector2();
                dust.velocity = dust.velocity.RotatedBy((float)j / 36f * ((float)Math.PI * 2f)) * 8f;
                dust.noGravity = true;
                dust.scale = 1.9f;
            }
            SoundEngine.PlaySound(SoundID.Item110, Player.Center);
            SoundEngine.PlaySound(SoundID.Item14, Player.Center);
        }
        public void SetSariaLevel(int newLevel)
        {
            if (Sarialevel != newLevel)
            {
                Sarialevel = newLevel;
            }
        }
        public override void OnEnterWorld(Player player)
        {
            // When a player joins, send their own Saria level to all other players.
            // packet.Send(-1, Player.whoAmI) = send to everyone except themselves.
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                // Defer the send to PostUpdate — the player roster (Main.player[i].active)
                // is not populated yet when OnEnterWorld fires.
                _pendingSariaLevelSync = true;
            }

            if (Main.netMode == NetmodeID.Server)
            {
                for (int i = 0; i < Main.npc.Length; i++)
                {
                    NPC npc = Main.npc[i];
                    // Check if the NPC is active and has your specific buff.
                    if (npc.active && npc.HasBuff(ModContent.BuffType<EnemyFrozen>()))
                    {
                        // Send a sync packet for this specific NPC to the newly joined player.
                        NetMessage.SendData(MessageID.SyncNPC, player.whoAmI, -1, null, i);
                    }
                }
                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    Player otherPlayer = Main.player[i];
                    // Check if the other player is active and has the buff
                    if (otherPlayer.active && otherPlayer.HasBuff<SariaBuff>()) // Replace 'YourSummonBuff' with your actual summon buff class
                    {
                        // Create and send a buff sync packet to the joining player
                        ModPacket packet = Mod.GetPacket();
                        packet.Write((byte)SariaMod.SoundMessageType.SyncBuff);
                        packet.Write(otherPlayer.whoAmI); // The player who has the buff
                        packet.Write(ModContent.BuffType<SariaBuff>()); // The buff type
                        packet.Write(otherPlayer.buffTime[otherPlayer.FindBuffIndex(ModContent.BuffType<SariaBuff>())]); // The remaining buff time
                        packet.Send(player.whoAmI); // Send the packet to the newly joined player
                    }
                }
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile projectile = Main.projectile[i];
                    // Check if the projectile is an active Saria summon.
                    if (projectile.active && projectile.type == ModContent.ProjectileType<Saria>())
                    {
                        // Send the current frame and direction to the joining player.
                        // The `whoAmI` property of the joining `player` is used as the target client.
                        // The -1 for `ignoreClient` means no one is ignored.
                        SyncSariaProjectile(projectile, toClient: player.whoAmI, ignoreClient: -1);
                    }
                }
            }
        }
        public void ResetKingsDinnerTimer()
        {
            KingsDinnerCooldownTimer = 0;
        }
        public override void PostUpdate()
        {
            // Deferred join sync: send our Saria level once the player roster is populated.
            if (_pendingSariaLevelSync && Main.netMode == NetmodeID.MultiplayerClient)
            {
                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    if (i != Player.whoAmI && Main.player[i].active)
                    {
                        // At least one other player is present — send our level to everyone except ourselves.
                        ModPacket packet = Mod.GetPacket();
                        packet.Write((byte)SariaMod.SoundMessageType.SyncSariaLevel);
                        packet.Write(Player.whoAmI);
                        packet.Write(Sarialevel);
                        packet.Write(SariaXp);
                        NetworkProfiler.RecordSend((byte)SariaMod.SoundMessageType.SyncSariaLevel, packet);
                        packet.Send(-1, Player.whoAmI);
                        break;
                    }
                }
                _pendingSariaLevelSync = false;
            }

            // Check if the player is holding the KingsDinner item
            FairyPlayer modPlayer = Player.Fairy();
            if (KingsDinnerCooldownTimer < KingsDinnerResetTime)
            {
                KingsDinnerCooldownTimer++;
            }
            
            // Timer Logic
            bool isDialogueActive = SariaUISystem.IsDialogueActive;
            if (!isDialogueActive)
            {
                if (totalTalkingTime > 0) totalTalkingTime--;
                if (smallTalkingTime > 0) smallTalkingTime--;
            }

            // Detect UI Close
            if (wasDialogueActive && !isDialogueActive)
            {
                if (InteractionManager.IsInteractiveSession)
                {
                    // Interactive session ended
                    totalTalkingTime = Main.rand.Next(8 * 3600, 24 * 3600 + 1); // 8-24 minutes
                    InteractionManager.IsInteractiveSession = false;
                }
                else
                {
                    // General dialogue ended
                    if (smallTalkingTime < 60 * 60) // < 60s
                    {
                        smallTalkingTime += Main.rand.Next(30 * 60, 40 * 60 + 1); // 30-40 sec
                    }
                }
            }
            wasDialogueActive = isDialogueActive;

            if (modPlayer.Serving >= 101)
            {
                modPlayer.Serving = 100;
            }
            if (Player.HeldItem.type == ModContent.ItemType<Items.zDinner.KingsDinner>())
            {
                DinnerHoldTimer++; // Increment the timer each frame
            }
            else
            {
                DinnerHoldTimer = 0; // Reset the timer if the player is not holding the item
            }
            // Check if the timer has exceeded a certain amount of time (e.g., 180 ticks = 3 seconds)
            if (DinnerHoldTimer >= 18000)
            {
                // Ensure the projectile spawning happens on the server (or owning client) in multiplayer.
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    // Roll a random number to select one of the two projectiles.
                    int randProjectile = Main.rand.Next(2); // 0 or 1
                    int projectileType;
                    if (randProjectile == 0)
                    {
                        projectileType = ModContent.ProjectileType<DinnerSoundHit1>();
                    }
                    else
                    {
                        projectileType = ModContent.ProjectileType<DinnerSoundHit2>();
                    }
                    // Spawn the projectile from the player's position
                    // You may want to spawn it towards the mouse or other location
                    Vector2 mousePosition = Main.MouseWorld;
                    Vector2 direction = mousePosition - Player.Center;
                    direction.Normalize();
                    direction *= 10f; // Set the projectile speed
                    Projectile.NewProjectile(Player.GetSource_ItemUse(Player.HeldItem), Player.Center, direction, projectileType, Player.HeldItem.damage, Player.HeldItem.knockBack, Player.whoAmI);
                }
                DinnerHoldTimer = 0; // Reset the timer after spawning the projectile
            }
            if (lastHeldItemType != Player.HeldItem.type && Player.HeldItem.type == ModContent.ItemType<Items.zDinner.KingsDinner>())
            {
                // Play the sound effect.
                // Replace "SariaMod/Sounds/ItemSelect" with the path to your sound file.
                SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/CoolHuh"), Player.Center);
            }
            // Update the last held item type for the next frame's check
            lastHeldItemType = Player.HeldItem.type;

            // HealBall right mouse: click (<=30 ticks) vs hold (>30 ticks)
            HealBallRightClick = false;

            if (Main.myPlayer == Player.whoAmI)
            {
                bool holdingHealBall = Player.HeldItem.type == ModContent.ItemType<HealBall>();
                bool rightDown = Main.mouseRight && !SariaUISystem.IsDialogueActive;

                if (!holdingHealBall)
                {
                    HealBallRightHoldTimer = 0;
                    HealBallRightHoldActive = false;
                    healBallPrevRightDown = false;
                }
                else
                {
                    if (rightDown)
                    {
                        HealBallRightHoldTimer++;
                        HealBallRightHoldActive = HealBallRightHoldTimer > 30;
                    }
                    else
                    {
                        if (healBallPrevRightDown)
                        {
                            if (HealBallRightHoldTimer > 0 && HealBallRightHoldTimer <= 30)
                                HealBallRightClick = true;
                        }

                        HealBallRightHoldTimer = 0;
                        HealBallRightHoldActive = false;
                    }

                    healBallPrevRightDown = rightDown;
                }

                // Spawn/cleanup ZtargetReal based on HealBallRightHoldActive (hold only)
                int ztargetRealType = ModContent.ProjectileType<ZtargetReal>();
                if (HealBallRightHoldActive)
                {
                    if (Player.ownedProjectileCounts[ztargetRealType] <= 0f)
                    {
                        // When LinkCable is on the camera is on Saria, so spawn the reticle
                        // at her position so it reaches the cursor immediately instead of
                        // travelling all the way from the player first.
                        Vector2 spawnPos = Player.Center;
                        if (LinkCable)
                        {
                            int sariaType = ModContent.ProjectileType<Items.Strange.Saria>();
                            for (int i = 0; i < Main.maxProjectiles; i++)
                            {
                                Projectile p = Main.projectile[i];
                                if (p.active && p.owner == Player.whoAmI && p.type == sariaType)
                                {
                                    spawnPos = p.Center;
                                    break;
                                }
                            }
                        }

                        Projectile.NewProjectile(
                            Player.GetSource_FromThis(),
                            spawnPos,
                            Vector2.Zero,
                            ztargetRealType,
                            0,
                            0f,
                            Player.whoAmI,
                            0f,
                            0f);
                    }
                }
                else
                {
                    if (Player.ownedProjectileCounts[ztargetRealType] > 0f)
                    {
                        for (int i = 0; i < Main.maxProjectiles; i++)
                        {
                            Projectile p = Main.projectile[i];
                            if (p.active && p.owner == Player.whoAmI && p.type == ztargetRealType)
                                p.Kill();
                        }
                    }
                }
            }
            else
            {
                HealBallRightHoldTimer = 0;
                HealBallRightHoldActive = false;
                HealBallRightClick = false;
                healBallPrevRightDown = false;
            }

            // Biome/music perception follow: handled by SariaPerceptionSystem, which hooks
            // Player.UpdateBiomes, Main.UpdateAudio and the global SceneMetrics scan so the
            // game natively recomputes biome state at Saria's position (exclusive semantics —
            // the player's real surroundings contribute nothing). The old approach of
            // mirroring Saria's cached snapshot here was structurally too late: vanilla's
            // audio decision runs at the START of the frame and reads real position.Y for
            // depth-gated tracks, so PostUpdate flag writes could never affect music, and
            // they stomped the live flags with a stale movement-gated snapshot.
        }
        public static void SyncSariaProjectile(Projectile projectile, int toClient = -1, int ignoreClient = -1)
        {
            if (Main.netMode == NetmodeID.Server && projectile.active && projectile.type == ModContent.ProjectileType<Saria>())
            {
                ModPacket packet = ModContent.GetInstance<SariaMod>().GetPacket();
                packet.Write((byte)SariaMod.SoundMessageType.SyncProjectileState);
                packet.Write(projectile.whoAmI);
                packet.Write(projectile.frame);
                packet.Write(projectile.spriteDirection);
                packet.Write(projectile.frameCounter);
                packet.Send(toClient, ignoreClient);
            }
        }
        public override void SetControls()
        {
            int owner = Player.whoAmI;
            if ((Player.ownedProjectileCounts[ModContent.ProjectileType<Emeraldspike>()] <= 0f) && (Player.ownedProjectileCounts[ModContent.ProjectileType<Emeraldspike2>()] <= 0f) && (Player.ownedProjectileCounts[ModContent.ProjectileType<Emeraldspike3>()] <= 0f))
            {
                holdingleft = false;
                holdingright = false;
                holdingdown = false;
            }
            if ((Player.ownedProjectileCounts[ModContent.ProjectileType<Emeraldspike>()] > 0f))
            {
                for (int U = 0; U < 1000; U++)
                {
                    if (Main.projectile[U].active && Main.projectile[U].ModProjectile is Emeraldspike modRupee && ((Main.projectile[U].owner == owner)))
                    {
                        if (Main.projectile[U].frame == 0)
                        {
                            if (Player.controlLeft)
                            {
                                holdingleft = false;
                            }
                            if (Player.controlRight)
                            {
                                holdingright = false;
                            }
                            if (Player.controlDown)
                            {
                                holdingdown = false;
                            }
                        }
                        if (Main.projectile[U].frame >= 1)
                        {
                            if (!Player.controlLeft)
                            {
                                holdingleft = true;
                            }
                            if (!Player.controlRight)
                            {
                                holdingright = true;
                            }
                            if (!Player.controlDown)
                            {
                                holdingdown = true;
                            }
                        }
                    }
                }
            }
            if ((Player.ownedProjectileCounts[ModContent.ProjectileType<Emeraldspike2>()] > 0f))
            {
                for (int U = 0; U < 1000; U++)
                {
                    if (Main.projectile[U].active && Main.projectile[U].ModProjectile is Emeraldspike2 modRupee && ((Main.projectile[U].owner == owner)))
                    {
                        if (Main.projectile[U].frame == 0)
                        {
                            if (Player.controlLeft)
                            {
                                holdingleft = false;
                            }
                            if (Player.controlRight)
                            {
                                holdingright = false;
                            }
                            if (Player.controlDown)
                            {
                                holdingdown = false;
                            }
                        }
                        if (Main.projectile[U].frame >= 1)
                        {
                            if (!Player.controlLeft)
                            {
                                holdingleft = true;
                            }
                            if (!Player.controlRight)
                            {
                                holdingright = true;
                            }
                            if (!Player.controlDown)
                            {
                                holdingdown = true;
                            }
                        }
                    }
                }
            }
            if ((Player.ownedProjectileCounts[ModContent.ProjectileType<Emeraldspike3>()] > 0f))
            {
                for (int U = 0; U < 1000; U++)
                {
                    if (Main.projectile[U].active && Main.projectile[U].ModProjectile is Emeraldspike3 modRupee && ((Main.projectile[U].owner == owner)))
                    {
                        if (Main.projectile[U].frame == 0)
                        {
                            if (Player.controlLeft)
                            {
                                holdingleft = false;
                            }
                            if (Player.controlRight)
                            {
                                holdingright = false;
                            }
                            if (Player.controlDown)
                            {
                                holdingdown = false;
                            }
                        }
                        if (Main.projectile[U].frame >= 1)
                        {
                            if (!Player.controlLeft)
                            {
                                holdingleft = true;
                            }
                            if (!Player.controlRight)
                            {
                                holdingright = true;
                            }
                            if (!Player.controlDown)
                            {
                                holdingdown = true;
                            }
                        }
                    }
                }
            }
        }
        public override void PostUpdateMiscEffects()
        {
            FairyPlayerMiscEffects.FairyPostUpdateMiscEffects(Player, Mod);
        }

        public override void ModifyScreenPosition()
        {
            if (!TryGetCameraSaria(out _, out Projectile saria)) return;

            // Shift screen so it centres on Saria instead of the player.
            Main.screenPosition += saria.Center - Player.Center;

            // NOTE: the biome/music perception override used to be applied here too, but this
            // hook is documented as running "after weapon zoom and camera lerp" — i.e. during
            // camera/render-prep, which is later in the frame than vanilla's own per-tick
            // music/scene-effect selection. That made the override always one frame late for
            // audio (while still arriving in time for that frame's draw call, which is why only
            // the background partially reacted and music never did). It now lives in
            // PostUpdate() instead, which runs at the end of Player.Update() — see there.
        }

        /// <summary>
        /// Resolves the local player's owned Saria projectile when camera-on-Saria mode is
        /// active (holding Heal Ball with Link Cable engaged). Shared by ModifyScreenPosition
        /// (camera shift) and PostUpdate (biome/music perception override) so both hooks agree
        /// on the exact same activation condition and projectile lookup.
        /// </summary>
        private bool TryGetCameraSaria(out Items.Strange.Saria sariaMP, out Projectile sariaProj)
        {
            sariaMP = null;
            sariaProj = null;

            if (Main.myPlayer != Player.whoAmI) return false;
            if (Player.HeldItem.type != ModContent.ItemType<HealBall>() || !LinkCable) return false;

            int sariaType = ModContent.ProjectileType<Items.Strange.Saria>();
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.owner == Player.whoAmI && p.type == sariaType)
                {
                    sariaProj = p;
                    sariaMP = p.ModProjectile as Items.Strange.Saria;
                    return sariaMP != null;
                }
            }
            return false;
        }

            }
        }