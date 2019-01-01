using System;
using System.Collections.Generic;
using UniRx;
using Random = UnityEngine.Random;

namespace QFramework
{
	public static class Playlist
	{
		class MusicConfig
		{
			public string Name;

			public float Volume;
		}

		private static List<MusicConfig> mMusicList = new List<MusicConfig>()
		{
			new MusicConfig {Name = "puzzle", Volume = 0.5f},
			new MusicConfig {Name = "boss", Volume = 0.5f},
			new MusicConfig() {Name = "menu", Volume = 0.5f},
			new MusicConfig() {Name = "retro3", Volume = 1.0f},
			new MusicConfig() {Name = "retro4", Volume = 1.0f},
		};

		public static void PlayRandomMusic()
		{
			var rangeIndex = Random.Range(0, mMusicList.Count - 1);

			rangeIndex.LogInfo();

			var toPlayedMusic = mMusicList[rangeIndex];

			AudioManager.PlayMusic(toPlayedMusic.Name, loop: true, volume: toPlayedMusic.Volume);
		}

		public static void PlayMusic(string musicName)
		{
			AudioManager.PlayMusic(musicName);
		}

		public static void StopMusic()
		{
			AudioManager.StopMusic();
		}
	}
}