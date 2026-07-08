using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using SariaMod.Buffs;
using SariaMod.Dusts;
using SariaMod.Items.Amber;
using SariaMod.Items.Amethyst;
using SariaMod.Items.Bands;
using SariaMod.Items.Emerald;
using SariaMod.Items.Ruby;
using SariaMod.Items.Sapphire;
using SariaMod.Items.Topaz;
using SariaMod.Items.zPearls;
using SariaMod.Items.zTalking;
using SariaMod.Items.Strange;
using Terraria.Localization;
using System;
using Terraria.Map;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Events;
using Terraria.ID;
using Terraria.ModLoader;
using System.Linq;
using SariaMod.Diagnostics;
using SariaMod.Netcode.SariaSoundSync;
using ReLogic.Utilities;
namespace SariaMod.Items.Strange
{
    public partial class Saria
    {
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(Transform);
            writer.Write(Mood);
            writer.Write(MoveTimer);
            writer.Write(SleepHeal);
            writer.Write(Sleep);
            writer.Write(Cursed);
            writer.Write(Follow);
            writer.Write(ChannelTime);
            writer.Write(ChannelState);
            writer.Write(IsCharging);
            writer.Write(ChannelAttack);
            writer.Write(Eating);
            writer.Write(ChangeForm);
            writer.Write(Holding);
            writer.Write(CanMove);
            writer.Write(SpecialAnimate);
            writer.Write(SoundTimer);
            writer.Write(SoundTimer2);
            writer.Write(SelectSound);
            writer.Write(IsPlayerAsleep);
            writer.Write(CantAttackTimer);
            writer.Write(SariaTalking);
            writer.Write(CantAttack);
            writer.Write(Sneezing);
            writer.Write(BloodSneeze);
            writer.Write(frameToSync);
            writer.Write(directionToSync);
            writer.Write(syncedFrameCounter);
            writer.Write(LegsIsCasual);
            writer.Write(LegsGoingToCasual);
            writer.Write(LegsIsProper);
            writer.Write(LegsGoingToProper);
            writer.Write(ArmsIsDown);
            writer.Write(ArmsGoingUp);
            writer.Write(ArmsIsUp);
            writer.Write(ArmsGoingDown);
            writer.Write(EyesLooking);
            writer.Write(EyesBlinking);
            writer.Write(EyesOpening);
            writer.Write(DisplayedMoodSync);
            IdleAnimator.Write(writer);
            writer.Write(FlashCooldownTimer);
            writer.Write(_moodOverrideTimer);
            writer.Write(_moodOverrideTarget);
            writer.Write(_moodPriority);
            writer.Write(SicknessBar);
            writer.Write(_smileActive);
            writer.Write(_smileInteractionUsed);
            writer.Write(_smileInteractionActive);
            writer.Write(_smileLockedUntilRoamReset);
            writer.Write(_playerHasLookedAway);
            writer.Write(_smileAngerTimer);
            writer.Write(_wasEyeRoaming);
            writer.Write(_playerStandingTimer);
            writer.Write(PendingTransform);
            writer.Write(TransformTimer);
            // Saria's own biome zones + depth + environment — packed into two ushorts (4 bytes)
            var (biomes, depthEnv) = PackSariaZones();
            writer.Write(biomes);
            writer.Write(depthEnv);
            writer.Write(_followMarkedPosition.X);
            writer.Write(_followMarkedPosition.Y);
            // A* path to the marked location — sent as compact tile-origin coords.
            int pathCount = _followPath.Count;
            if (pathCount > 255) pathCount = 255;
            writer.Write((byte)pathCount);
            for (int i = 0; i < pathCount; i++)
            {
                Vector2 c = _followPath[i];
                short ox = (short)Math.Round(c.X / 16f - FollowPathFootprintWidth  * 0.5f);
                short oy = (short)Math.Round(c.Y / 16f - FollowPathFootprintHeight * 0.5f);
                writer.Write(ox);
                writer.Write(oy);
            }
            writer.Write((byte)Math.Min(_followPathIndex, 254));
            // Teleport phase sync
            writer.Write(_inWallTeleportTimer);
            writer.Write(_tpActiveDuration);
            writer.Write(_inWallEscapeTarget.X);
            writer.Write(_inWallEscapeTarget.Y);
            writer.Write(_linkCableFollow);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            Transform = reader.ReadInt32();
            Mood = reader.ReadInt32();
            MoveTimer = reader.ReadInt32();
            SleepHeal = reader.ReadInt32();
            Sleep = reader.ReadBoolean();
            Cursed = reader.ReadBoolean();
            Follow = reader.ReadBoolean();
            ChannelTime = reader.ReadInt32();
            ChannelState = reader.ReadInt32();
            IsCharging = reader.ReadInt32();
            ChannelAttack = reader.ReadInt32();
            Eating = reader.ReadInt32();
            ChangeForm = reader.ReadInt32();
            Holding = reader.ReadBoolean();
            CanMove = reader.ReadInt32();
            SpecialAnimate = reader.ReadInt32();
            SoundTimer = reader.ReadInt32();
            SoundTimer2 = reader.ReadInt32();
            SelectSound = reader.ReadBoolean();
            IsPlayerAsleep = reader.ReadBoolean();
            CantAttackTimer = reader.ReadInt32();
            SariaTalking = reader.ReadBoolean();
            CantAttack = reader.ReadBoolean();
            Sneezing = reader.ReadBoolean();
            BloodSneeze = reader.ReadBoolean();
            frameToSync = reader.ReadInt32();
            directionToSync = reader.ReadInt32();
            syncedFrameCounter = reader.ReadInt32();
            Projectile.frame = frameToSync;
            Projectile.frameCounter = syncedFrameCounter;
            Projectile.spriteDirection = directionToSync;
            LegsIsCasual      = reader.ReadBoolean();
            LegsGoingToCasual = reader.ReadBoolean();
            LegsIsProper      = reader.ReadBoolean();
            LegsGoingToProper = reader.ReadBoolean();
            ArmsIsDown    = reader.ReadBoolean();
            ArmsGoingUp   = reader.ReadBoolean();
            ArmsIsUp      = reader.ReadBoolean();
            ArmsGoingDown = reader.ReadBoolean();
            EyesLooking  = reader.ReadBoolean();
            EyesBlinking = reader.ReadBoolean();
            EyesOpening  = reader.ReadBoolean();
            DisplayedMoodSync = reader.ReadInt32();
            IdleAnimator.Read(reader);
            FlashCooldownTimer = reader.ReadInt32();
            _moodOverrideTimer = reader.ReadInt32();
            _moodOverrideTarget = reader.ReadInt32();
            _moodPriority = reader.ReadInt32();
            SicknessBar = reader.ReadInt32();
            _smileActive = reader.ReadBoolean();
            _smileInteractionUsed = reader.ReadBoolean();
            _smileInteractionActive = reader.ReadBoolean();
            _smileLockedUntilRoamReset = reader.ReadBoolean();
            _playerHasLookedAway = reader.ReadBoolean();
            _smileAngerTimer = reader.ReadInt32();
            _wasEyeRoaming = reader.ReadBoolean();
            _playerStandingTimer = reader.ReadInt32();
            PendingTransform = reader.ReadInt32();
            TransformTimer = reader.ReadInt32();
            // Saria's own biome zones + depth + environment — packed into two ushorts (4 bytes)
            UnpackSariaZones(reader.ReadUInt16(), reader.ReadUInt16());
            _followMarkedPosition = new Vector2(reader.ReadSingle(), reader.ReadSingle());
            // A* path to the marked location — rebuilt from compact tile-origin coords.
            int pathCount = reader.ReadByte();
            _followPath.Clear();
            for (int i = 0; i < pathCount; i++)
            {
                short ox = reader.ReadInt16();
                short oy = reader.ReadInt16();
                _followPath.Add(new Vector2(
                    (ox + FollowPathFootprintWidth  * 0.5f) * 16f,
                    (oy + FollowPathFootprintHeight * 0.5f) * 16f));
            }
            int syncedIndex = reader.ReadByte();
            _followPathIndex = Math.Min(syncedIndex, Math.Max(0, _followPath.Count - 1));
            // Teleport phase sync
            _inWallTeleportTimer = reader.ReadInt32();
            _tpActiveDuration    = reader.ReadInt32();
            _inWallEscapeTarget  = new Vector2(reader.ReadSingle(), reader.ReadSingle());
            _linkCableFollow     = reader.ReadBoolean();
        }

        private (ushort biomes, ushort depthEnv) PackSariaZones()
        {
            ushort biomes = 0;
            if (SariaZoneSnow)            biomes |= ZoneBit_Snow;
            if (SariaZoneJungle)          biomes |= ZoneBit_Jungle;
            if (SariaZoneCorrupt)         biomes |= ZoneBit_Corrupt;
            if (SariaZoneCrimson)         biomes |= ZoneBit_Crimson;
            if (SariaZoneHallow)          biomes |= ZoneBit_Hallow;
            if (SariaZoneDesert)          biomes |= ZoneBit_Desert;
            if (SariaZoneGlowingMushroom) biomes |= ZoneBit_GlowingMushroom;
            if (SariaZoneGraveyard)       biomes |= ZoneBit_Graveyard;
            if (SariaZoneMeteor)          biomes |= ZoneBit_Meteor;
            if (SariaZoneForest)          biomes |= ZoneBit_Forest;
            if (SariaZoneRain)            biomes |= ZoneBit_Rain;
            if (SariaZoneBeach)           biomes |= ZoneBit_Beach;
            if (SariaZoneDungeon)         biomes |= ZoneBit_Dungeon;
            if (SariaZoneSandstorm)       biomes |= ZoneBit_Sandstorm;
            if (SariaZoneUndergroundDesert) biomes |= ZoneBit_UndergroundDesert;

            ushort depthEnv = 0;
            if (SariaZoneSkyHeight)       depthEnv |= DepthBit_SkyHeight;
            if (SariaZoneSpace)           depthEnv |= DepthBit_Space;
            if (SariaZoneOverworld)       depthEnv |= DepthBit_Overworld;
            if (SariaZoneUnderground)     depthEnv |= DepthBit_Underground;
            if (SariaZoneDirtLayer)       depthEnv |= DepthBit_DirtLayer;
            if (SariaZoneRockLayer)       depthEnv |= DepthBit_RockLayer;
            if (SariaZoneUnderworld)      depthEnv |= DepthBit_Underworld;
            if (SariaHasCampfire)         depthEnv |= EnvBit_Campfire;
            if (SariaHasHeartLantern)     depthEnv |= EnvBit_HeartLantern;
            if (SariaHasStarInBottle)     depthEnv |= EnvBit_StarInBottle;
            if (SariaHasWaterCandle)      depthEnv |= EnvBit_WaterCandle;
            if (SariaHasPeaceCandle)      depthEnv |= EnvBit_PeaceCandle;
            if (SariaHasCalmMindCandle)   depthEnv |= EnvBit_CalmMindCandle;
            if (SariaHasReajCandle)       depthEnv |= EnvBit_ReajCandle;

            return (biomes, depthEnv);
        }


        private void UnpackSariaZones(ushort biomes, ushort depthEnv)
        {
            SariaZoneSnow            = (biomes & ZoneBit_Snow)            != 0;
            SariaZoneJungle          = (biomes & ZoneBit_Jungle)          != 0;
            SariaZoneCorrupt         = (biomes & ZoneBit_Corrupt)         != 0;
            SariaZoneCrimson         = (biomes & ZoneBit_Crimson)         != 0;
            SariaZoneHallow          = (biomes & ZoneBit_Hallow)          != 0;
            SariaZoneDesert          = (biomes & ZoneBit_Desert)          != 0;
            SariaZoneGlowingMushroom = (biomes & ZoneBit_GlowingMushroom) != 0;
            SariaZoneGraveyard       = (biomes & ZoneBit_Graveyard)       != 0;
            SariaZoneMeteor          = (biomes & ZoneBit_Meteor)          != 0;
            SariaZoneForest          = (biomes & ZoneBit_Forest)          != 0;
            SariaZoneRain            = (biomes & ZoneBit_Rain)            != 0;
            SariaZoneBeach           = (biomes & ZoneBit_Beach)           != 0;
            SariaZoneDungeon         = (biomes & ZoneBit_Dungeon)         != 0;
            SariaZoneSandstorm       = (biomes & ZoneBit_Sandstorm)       != 0;
            SariaZoneUndergroundDesert = (biomes & ZoneBit_UndergroundDesert) != 0;

            SariaZoneSkyHeight       = (depthEnv & DepthBit_SkyHeight)    != 0;
            SariaZoneSpace           = (depthEnv & DepthBit_Space)        != 0;
            SariaZoneOverworld       = (depthEnv & DepthBit_Overworld)    != 0;
            SariaZoneUnderground     = (depthEnv & DepthBit_Underground)  != 0;
            SariaZoneDirtLayer       = (depthEnv & DepthBit_DirtLayer)    != 0;
            SariaZoneRockLayer       = (depthEnv & DepthBit_RockLayer)    != 0;
            SariaZoneUnderworld      = (depthEnv & DepthBit_Underworld)   != 0;
            SariaHasCampfire         = (depthEnv & EnvBit_Campfire)       != 0;
            SariaHasHeartLantern     = (depthEnv & EnvBit_HeartLantern)   != 0;
            SariaHasStarInBottle     = (depthEnv & EnvBit_StarInBottle)   != 0;
            SariaHasWaterCandle      = (depthEnv & EnvBit_WaterCandle)    != 0;
            SariaHasPeaceCandle      = (depthEnv & EnvBit_PeaceCandle)    != 0;
            SariaHasCalmMindCandle   = (depthEnv & EnvBit_CalmMindCandle)  != 0;
            SariaHasReajCandle       = (depthEnv & EnvBit_ReajCandle)      != 0;
        }

        /// <summary>
        /// Samples SceneMetrics at Saria's world position to populate her own zone fields.
        /// Only runs on the owner's client — results are synced to other clients via SendExtraAI.
    }
}
