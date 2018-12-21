/****************************************************************************
 * 2018.12 liangxie
 * 
 * 教程地址:http://www.sikiedu.com/course/327
 ****************************************************************************/

using MoreMountains.CorgiEngine;
using MoreMountains.Tools;
using QFramework;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace IndieGame
{

	public enum GameMode
	{
		Normal,
		Train,
	}

	public class GameModeLogic : MonoBehaviour
	{
		public static GameMode Mode = GameMode.Normal;

		public static void LevelFinish()
		{
			if (Mode == GameMode.Normal)
			{
				var nextLevelName = LevelConfig.GetNextLevelName();

				if (LevelManager.Instance != null)
				{
					LevelManager.Instance.GotoLevel(nextLevelName);
				}
				else
				{
					LoadingSceneManager.LoadScene(nextLevelName);
				}
			}
			else
			{
				SceneManager.LoadScene("Empty");

				UIMgr.ClosePanel<UIGamePanel>();

				UIMgr.OpenPanel<UITrainModePanel>();
			}
		}
	}
}