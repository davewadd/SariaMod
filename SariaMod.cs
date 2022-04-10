using Terraria.ModLoader;
using FairyMod.FaiPlayer;
using Terraria;

namespace SariaMod
{
	public class SariaMod : Mod
	{
		public override void Load()
		{
			if (!Main.dedServ)
			{
				AddMusicBox(GetSoundSlot(SoundType.Music, "Sounds/Music/MascaRaskka"), ItemType("UndergroundDesertMusicBox"), TileType("UndergroundDesertMusicBoxTile"));
				AddMusicBox(GetSoundSlot(SoundType.Music, "Sounds/Music/MechonisField"), ItemType("MechanicalMusicBox"), TileType("MechanicalMusicBoxTile"));
				AddMusicBox(GetSoundSlot(SoundType.Music, "Sounds/Music/1Inside_Jabu-Jabu_s_Belly_-_The_Legend_of_Zelda_Oca"), ItemType("JabuJabuMusicBox"), TileType("JabuJabuMusicBoxTile"));
				AddMusicBox(GetSoundSlot(SoundType.Music, "Sounds/Music/Terraria_Overhaul_Music_Eerie_Theme_of_the_B"), ItemType("BloodMoonMusicBox"), TileType("BloodMoonMusicBoxTile"));
				AddMusicBox(GetSoundSlot(SoundType.Music, "Sounds/Music/Dran"), ItemType("GhostMusicBox"), TileType("GhostMusicBoxTile"));
				AddMusicBox(GetSoundSlot(SoundType.Music, "Sounds/Music/mercury"), ItemType("MercuryMusicBox"), TileType("MercuryMusicBoxTile"));
				AddMusicBox(GetSoundSlot(SoundType.Music, "Sounds/Music/Space"), ItemType("OtherworldSpacMusicBox"), TileType("OtherworldSpacMusicBoxTile"));
				AddMusicBox(GetSoundSlot(SoundType.Music, "Sounds/Music/Corruption"), ItemType("OtherworldCorruptionMusicBox"), TileType("OtherworldCorruptionMusicBoxTile"));
				AddMusicBox(GetSoundSlot(SoundType.Music, "Sounds/Music/Snow"), ItemType("OtherworldSnowMusicBox"), TileType("OtherworldSnowMusicBoxTile"));
				AddMusicBox(GetSoundSlot(SoundType.Music, "Sounds/Music/Rain"), ItemType("OtherworldRainMusicBox"), TileType("OtherworldRainMusicBoxTile"));
				AddMusicBox(GetSoundSlot(SoundType.Music, "Sounds/Music/ThePillars"), ItemType("OtherworldPillarMusicBox"), TileType("OtherworldPillarMusicBoxTile"));
				AddMusicBox(GetSoundSlot(SoundType.Music, "Sounds/Music/Shipwreck"), ItemType("ShipwreckMusicBox"), TileType("ShipwreckMusicBoxTile"));
				AddMusicBox(GetSoundSlot(SoundType.Music, "Sounds/Music/WiseOwlForest"), ItemType("WiseJungleMusicBox"), TileType("WiseJungleMusicBoxTile"));
				AddMusicBox(GetSoundSlot(SoundType.Music, "Sounds/Music/WaterTemple"), ItemType("WaterTempleMusicBox"), TileType("WaterTempleMusicBoxTile"));
				AddMusicBox(GetSoundSlot(SoundType.Music, "Sounds/Music/ThePlant"), ItemType("PlanteraMusicBox"), TileType("PlanteraMusicBoxTile"));
				AddMusicBox(GetSoundSlot(SoundType.Music, "Sounds/Music/A_Lonely_Figure"), ItemType("LonelyFigureMusicBox"), TileType("LonelyFigureMusicBoxTile"));
				AddMusicBox(GetSoundSlot(SoundType.Music, "Sounds/Music/WorldMap"), ItemType("WorldMapMusicBox"), TileType("WorldMapMusicBoxTile"));
				AddMusicBox(GetSoundSlot(SoundType.Music, "Sounds/Music/GlitchXcity"), ItemType("GlitchXcityMusicBox"), TileType("GlitchXcityMusicBoxTile"));
				AddMusicBox(GetSoundSlot(SoundType.Music, "Sounds/Music/UndergroundCrimson"), ItemType("UndergroundcrimsonMusicBox"), TileType("UndergroundcrimsonMusicBoxTile"));
			}
		}
			public override void UpdateMusic(ref int music, ref MusicPriority priority)
		{


			if (Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneOverworldHeight && !Main.dayTime && !Main.bloodMoon && !Main.player[Main.myPlayer].ZoneBeach && !Main.player[Main.myPlayer].ZoneDesert && !Main.player[Main.myPlayer].ZoneHoly)
			{
				music = GetSoundSlot(SoundType.Music, "Sounds/Music/MechonisField");
				priority = MusicPriority.BiomeLow;
			}
			else if (Main.player[Main.myPlayer].active && Main.bloodMoon && Main.player[Main.myPlayer].ZoneOverworldHeight)
			{
				music = GetSoundSlot(SoundType.Music, "Sounds/Music/Terraria_Overhaul_Music_Eerie_Theme_of_the_B");
				priority = MusicPriority.Event;
			}
			 if (Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneOverworldHeight && !Main.dayTime && !Main.bloodMoon && !Main.player[Main.myPlayer].ZoneBeach && !Main.player[Main.myPlayer].ZoneDesert && Main.player[Main.myPlayer].ZoneHoly)
			{
				music = GetSoundSlot(SoundType.Music, "Sounds/Music/MarshNight");
				priority = MusicPriority.Event;
			}
			else if (Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneOverworldHeight && Main.dayTime && !Main.bloodMoon && !Main.player[Main.myPlayer].ZoneBeach && !Main.player[Main.myPlayer].ZoneDesert && Main.player[Main.myPlayer].ZoneHoly)
			{
				music = GetSoundSlot(SoundType.Music, "Sounds/Music/MarshNight");
				priority = MusicPriority.Event;
			}

			if (Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneMeteor && Main.player[Main.myPlayer].ZoneOverworldHeight)
			{
				music = GetSoundSlot(SoundType.Music, "Sounds/Music/Terraria_Overhaul_Music_Eerie_Theme_of_the_B");
				priority = MusicPriority.Event;
			}
			if (Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneSkyHeight && !Main.player[Main.myPlayer].ZoneCrimson && !Main.player[Main.myPlayer].ZoneCorrupt)
			{
				music = GetSoundSlot(SoundType.Music, "Sounds/Music/Space");
				priority = MusicPriority.Environment;
			}
			if (Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZonePeaceCandle)
			{
				music = GetSoundSlot(SoundType.Music, "Sounds/Music/A_Lonely_Figure");
				priority = MusicPriority.Environment;
			}
			if (Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneRain && Main.dayTime && !Main.player[Main.myPlayer].ZoneSnow && !Main.player[Main.myPlayer].ZonePeaceCandle && Main.player[Main.myPlayer].ZoneOverworldHeight)
			{
				music = GetSoundSlot(SoundType.Music, "Sounds/Music/Rain");
				priority = MusicPriority.Environment;
			}
			else if (Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneRain && !Main.dayTime && !Main.player[Main.myPlayer].ZoneSnow && !Main.player[Main.myPlayer].ZonePeaceCandle && !Main.bloodMoon && Main.player[Main.myPlayer].ZoneOverworldHeight && !Main.player[Main.myPlayer].ZoneGlowshroom && !Main.player[Main.myPlayer].ZoneDesert)
			{
				music = GetSoundSlot(SoundType.Music, "Sounds/Music/Shipwreck");
				priority = MusicPriority.Environment;
			}
			else if (Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneRain && Main.player[Main.myPlayer].ZoneSnow && !Main.player[Main.myPlayer].ZonePeaceCandle && Main.player[Main.myPlayer].ZoneOverworldHeight && !Main.bloodMoon)
			{
				music = GetSoundSlot(SoundType.Music, "Sounds/Music/Blizzard");
				priority = MusicPriority.Environment;
			}

			if (Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneDesert && Main.dayTime && !Main.player[Main.myPlayer].ZoneBeach)
			{
				music = GetSoundSlot(SoundType.Music, "Sounds/Music/Lanayru");
				priority = MusicPriority.BiomeMedium;
			}
			if (Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneBeach && !Main.dayTime && !Main.bloodMoon && Main.player[Main.myPlayer].ZoneDesert)
			{
				music = GetSoundSlot(SoundType.Music, "Sounds/Music/Shipwreck");
				priority = MusicPriority.BiomeMedium;
			}
			if (Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneDesert && Main.player[Main.myPlayer].ZoneRockLayerHeight && !Main.player[Main.myPlayer].ZoneDungeon)
			{
				music = GetSoundSlot(SoundType.Music, "Sounds/Music/SpiritTemple");
				priority = MusicPriority.BiomeMedium;
			}
			else if (Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneDesert && Main.player[Main.myPlayer].ZoneDirtLayerHeight && !Main.player[Main.myPlayer].ZoneDungeon)
			{
				music = GetSoundSlot(SoundType.Music, "Sounds/Music/Terraria-Overhaul-Music-_Underground-Desert_-Theme-of-Underground-Desert (1)");
				priority = MusicPriority.BiomeMedium;
			}
			if (Main.player[Main.myPlayer].active && !Main.player[Main.myPlayer].ZoneOverworldHeight && !Main.player[Main.myPlayer].ZoneDirtLayerHeight && !Main.player[Main.myPlayer].ZoneRockLayerHeight && !Main.player[Main.myPlayer].ZoneUnderworldHeight && !Main.player[Main.myPlayer].ZonePeaceCandle)
			{
				music = GetSoundSlot(SoundType.Music, "Sounds/Music/Space");
				priority = MusicPriority.Environment;
			}
			if (Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneGlowshroom && !Main.player[Main.myPlayer].ZoneDungeon)
			{
				music = GetSoundSlot(SoundType.Music, "Sounds/Music/Mushroom");
				priority = MusicPriority.BiomeHigh;
			}
			if (Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneJungle && Main.player[Main.myPlayer].ZoneOverworldHeight && !Main.player[Main.myPlayer].ZoneDungeon && !Main.player[Main.myPlayer].ZoneGlowshroom)
			{
				music = GetSoundSlot(SoundType.Music, "Sounds/Music/WiseOwlForest");
				priority = MusicPriority.BiomeMedium;
			}
			else if (Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneJungle && Main.player[Main.myPlayer].ZoneDirtLayerHeight && !Main.player[Main.myPlayer].ZoneDungeon && !Main.player[Main.myPlayer].ZoneGlowshroom)
			{
				music = GetSoundSlot(SoundType.Music, "Sounds/Music/WiseOwlForest");
				priority = MusicPriority.BiomeMedium;
			}
			else if (Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneJungle && Main.player[Main.myPlayer].ZoneRockLayerHeight && !Main.player[Main.myPlayer].ZoneDungeon && !Main.player[Main.myPlayer].ZoneGlowshroom)
			{
				music = GetSoundSlot(SoundType.Music, "Sounds/Music/UndergroundJungle");
				priority = MusicPriority.BiomeMedium;
			}
			if (Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneCorrupt && Main.player[Main.myPlayer].ZoneOverworldHeight && !Main.player[Main.myPlayer].ZoneDungeon)
			{
				music = GetSoundSlot(SoundType.Music, "Sounds/Music/Corruption");
				priority = MusicPriority.Environment;
			}
			else if (Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneCorrupt && Main.player[Main.myPlayer].ZoneDirtLayerHeight && !Main.player[Main.myPlayer].ZoneDungeon)
			{
				music = GetSoundSlot(SoundType.Music, "Sounds/Music/UndergroundCorruption");
				priority = MusicPriority.BiomeHigh;
			}
			else if (Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneCorrupt && Main.player[Main.myPlayer].ZoneRockLayerHeight && !Main.player[Main.myPlayer].ZoneDungeon)
			{
				music = GetSoundSlot(SoundType.Music, "Sounds/Music/UndergroundCorruption");
				priority = MusicPriority.BiomeHigh;
			}
			if (Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneCrimson && Main.player[Main.myPlayer].ZoneOverworldHeight && !Main.player[Main.myPlayer].ZoneDungeon)
			{
				music = GetSoundSlot(SoundType.Music, "Sounds/Music/Crimson");
				priority = MusicPriority.Environment;
			}
			else if (Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneCrimson && Main.player[Main.myPlayer].ZoneDirtLayerHeight && !Main.player[Main.myPlayer].ZoneDungeon)
			{
				music = GetSoundSlot(SoundType.Music, "Sounds/Music/UndergroundCrimson");
				priority = MusicPriority.BiomeHigh;
			}
			else if (Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneCrimson && Main.player[Main.myPlayer].ZoneRockLayerHeight && !Main.player[Main.myPlayer].ZoneDungeon)
			{
				music = GetSoundSlot(SoundType.Music, "Sounds/Music/UndergroundCrimson");
				priority = MusicPriority.BiomeHigh;
			}
		
			else if (Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneSnow && Main.player[Main.myPlayer].ZoneOverworldHeight && !Main.bloodMoon && !Main.dayTime && !Main.player[Main.myPlayer].ZoneRain && !Main.player[Main.myPlayer].ZonePeaceCandle)
			{
				music = GetSoundSlot(SoundType.Music, "Sounds/Music/MechonisField");
				priority = MusicPriority.BiomeMedium;
			}
		
		}
	} 
}