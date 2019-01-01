using QFramework;
using UnityEngine;

namespace IndieGame
{
    public class GameData
    {
        public static int DeathCountMin
        {
            get { return PlayerPrefs.GetInt("DEATH_COUNT_MIN", int.MaxValue); }
            set { PlayerPrefs.SetInt("DEATH_COUNT_MIN", value); }
        }


        public static bool FirstTimeEnterLevel1
        {

            get { return PlayerPrefs.GetInt("FIRST_TIME_ENTER_LEVEL_1", 1) == 1 ? true : false; }
            set { PlayerPrefs.SetInt("FIRST_TIME_ENTER_LEVEL_1", value ? 1 : 0); }
        }

        public static string CurLevelName
        {
            get { return PlayerPrefs.GetString("CUR_LEVEL_NAME", "Level1"); }
            set { PlayerPrefs.SetString("CUR_LEVEL_NAME", value); }
        }

        public static int CurDeathCount
        {
            get { return PlayerPrefs.GetInt("CUR_DEATH_COUNT", 0); }
            set { PlayerPrefs.SetInt("CUR_DEATH_COUNT", value); }
        }

        public static bool HardModeUnlocked
        {
            get { return PlayerPrefs.GetInt("HARD_MODE_UNLOCKED", 0) == 1 ? true : false; }
            set { PlayerPrefs.SetInt("HARD_MODE_UNLOCKED", value ? 1 : 0); }
        }

        public static void SetCurLevelDeathCount(string levelName,int deathCount)
        {
            PlayerPrefs.SetInt("{0}_CUR_DEATH_COUNT".FillFormat(levelName),deathCount);
        }

        public static int GetCurLevelDeathCount(string levelName)
        {
            return PlayerPrefs.GetInt("{0}_CUR_DEATH_COUNT".FillFormat(levelName),0);
        }

        public static void SetMinLevelDeathCount(string levelName, int deathCountMin)
        {
            PlayerPrefs.SetInt("{0}_MIN_DEATH_COUNT".FillFormat(levelName),deathCountMin);
        }

        public static int GetMinLevelDeathCount(string levelName)
        {
            return PlayerPrefs.GetInt("{0}_MIN_DEATH_COUNT".FillFormat(levelName), int.MaxValue);
        }
    }
}