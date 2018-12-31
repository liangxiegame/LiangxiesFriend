using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace IndieGame
{
    public class LevelConfig
    {
        public static string GetBgMusicNameForLevelName(string levelName)
        {
            return mMusicNamesForLevels[levelName];
        }

        public static string GetEffectSoundNameForLevelName(string levelName)
        {
            return mEffectNameForLevels[levelName];
        }
		
        private static Dictionary<string, string> mMusicNamesForLevels = new Dictionary<string, string>()
        {
            {"Level1", "puzzle"},
            {"Level2", "puzzle"},
            {"Level3", "puzzle"},
            {"Level4", "puzzle"},
            {"Level5", "puzzle"},
            {"Level6", "boss"},
            {"Level7", "boss"},
            {"Level8", "boss"},
            {"Level9", "boss"},
            {"Level10", "boss"},
            {"Level11", "menu"},
            {"Level12", "menu"},
            {"Level13", "menu"},
            {"Level14", "menu"},
            {"Level15", "menu"},
            {"Level16", "retro3"},
            {"Level17", "retro3"},
            {"Level18", "retro3"},
            {"Level19", "retro3"},
            {"Level20", "retro3"},
            {"Level21", "retro4"},
            {"Level22", "retro4"},
            {"Level23", "retro4"},
            {"Level24", "retro4"},
        };
        
        private static Dictionary<string, string> mEffectNameForLevels = new Dictionary<string, string>()
        {
            {"Level1", string.Empty},
            {"Level2",  string.Empty},
            {"Level3",  string.Empty},
            {"Level4",  string.Empty},
            {"Level5",  string.Empty},
            {"Level6", "rain_heavy"},
            {"Level7", "rain_heavy"},
            {"Level8", "rain_heavy"},
            {"Level9", "rain_heavy"},
            {"Level10", "rain_heavy"},
            {"Level11",  string.Empty},
            {"Level12",  string.Empty},
            {"Level13",  string.Empty},
            {"Level14",  string.Empty},
            {"Level15",  string.Empty},
            {"Level16",  string.Empty},
            {"Level17",  string.Empty},
            {"Level18",  string.Empty},
            {"Level19",  string.Empty},
            {"Level20",  string.Empty},
            {"Level21",  string.Empty},
            {"Level22",  string.Empty},
            {"Level23",  string.Empty},
            {"Level24",  string.Empty},
        };
		
        static List<string> mLevelNamesOrder = new List<string>()
        {
            "Level1",
            "Level2",
            "Level3",
            "Level4",
            "Level5",
            "Level6",
            "Level7",
            "Level8",
            "Level9",
            "Level10",
            "Level11",
            "Level12",
            "Level13",
            "Level14",
            "Level15",
            "Level16",
            "Level17",
            "Level18",
            "Level19",
            "Level20",
            "Level21",
            "Level22",
            "Level23",
            "Level24",
            "GameWin",
        };

        public static List<string> LevelNamesOrder
        {
            get { return mLevelNamesOrder; }
        }

        public static int CurrentLevelIndex(string curLevelName)
        {
            var curLevelIndex = mLevelNamesOrder.IndexOf(curLevelName);

            return curLevelIndex;
        }

        public static string GetNextLevelName()
        {
            var curLevelIndex = CurrentLevelIndex(SceneManager.GetActiveScene().name);
			
            curLevelIndex++;
            var nextLevelName = mLevelNamesOrder[curLevelIndex];

            if (curLevelIndex == mLevelNamesOrder.Count - 1)
            {
                GameData.CurLevelName = "Level1";
            }
            else
            {
                GameData.CurLevelName = nextLevelName;
            }

            return nextLevelName;
        }
    }
}