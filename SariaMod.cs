using Terraria.ModLoader;

using Terraria;

namespace SariaMod
{
	public class SariaMod : Mod
	{
		public override void Load()
		{
			if (!Main.dedServ)
			{
			
				
				
			
				
				AddMusicBox(GetSoundSlot(SoundType.Music, "Sounds/Music/Space"), ItemType("OtherworldSpacMusicBox"), TileType("OtherworldSpacMusicBoxTile"));



				AddMusicBox(GetSoundSlot(SoundType.Music, "Sounds/Music/Shipwreck"), ItemType("ShipwreckMusicBox"), TileType("ShipwreckMusicBoxTile"));
			
				
				
		
				AddMusicBox(GetSoundSlot(SoundType.Music, "Sounds/Music/WorldMap"), ItemType("WorldMapMusicBox"), TileType("WorldMapMusicBoxTile"));

			}
		}
			public override void UpdateMusic(ref int music, ref MusicPriority priority)
		{
			

			if (Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneOverworldHeight && !Main.dayTime && !Main.eclipse && !Main.bloodMoon && !Main.player[Main.myPlayer].ZoneBeach && !Main.player[Main.myPlayer].ZoneDesert && !Main.player[Main.myPlayer].ZoneHoly && !(Main.player[Main.myPlayer].ZoneSnow && Main.raining))
			{
				music = GetSoundSlot(SoundType.Music, "Sounds/Music/MechonisField");
				priority = MusicPriority.BiomeLow;
			}
			else if (Main.player[Main.myPlayer].active && Main.bloodMoon && Main.player[Main.myPlayer].ZoneOverworldHeight)
			{
				music = GetSoundSlot(SoundType.Music, "Sounds/Music/Terraria_Overhaul_Music_Eerie_Theme_of_the_B");
				priority = MusicPriority.Event;
			}
			 if (Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneOverworldHeight && !Main.dayTime && !Main.bloodMoon && !Main.eclipse && !Main.player[Main.myPlayer].ZoneBeach && !Main.player[Main.myPlayer].ZoneDesert && Main.player[Main.myPlayer].ZoneHoly)
			{
				music = GetSoundSlot(SoundType.Music, "Sounds/Music/MarshNight");
				priority = MusicPriority.Event;
			}
			else if (Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneOverworldHeight && Main.dayTime && !Main.bloodMoon && !Main.eclipse && !Main.player[Main.myPlayer].ZoneBeach && !Main.player[Main.myPlayer].ZoneDesert && Main.player[Main.myPlayer].ZoneHoly)
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
			else if (Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneRain && Main.player[Main.myPlayer].ZoneSnow && Main.player[Main.myPlayer].ZoneOverworldHeight)
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
				music = GetSoundSlot(SoundType.Music, "Sounds/Music/Terraria-Overhaul-Music-_Underground-Desert_-Theme-of-Underground-Desert (1)");
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
				music = GetSoundSlot(SoundType.Music, "Sounds/Music/JungleNight");
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
		else if (Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].ZoneOverworldHeight && !Main.dayTime && !Main.eclipse && !Main.bloodMoon && !Main.player[Main.myPlayer].ZoneBeach && Main.player[Main.myPlayer].ZoneSnow && !Main.player[Main.myPlayer].ZoneDesert && !Main.raining)
			{
				music = GetSoundSlot(SoundType.Music, "Sounds/Music/MechonisField");
				priority = MusicPriority.BiomeHigh;
			}


		}
	} 
}