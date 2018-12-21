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

	public class GameModeLogic
	{
		public static GameMode Mode = GameMode.Normal;
		
		static IModeLogic mNormalModeLogic = new NormalModeLogic();
		static IModeLogic mTrainModeLogic = new TrainModeLogic();
		
		private static IModeLogic mModeLogic
		{
			get { return Mode == GameMode.Normal ? mNormalModeLogic : mTrainModeLogic; }
		}
	
		public static void LevelFinish()
		{
			mModeLogic.LevelFinish();
		}
	}

	public interface IModeLogic
	{
		void LevelFinish();
	}

	public class NormalModeLogic : IModeLogic
	{
		public void LevelFinish()
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
	}

	public class TrainModeLogic : IModeLogic
	{
		public void LevelFinish()
		{
			SceneManager.LoadScene("Empty");

			UIMgr.ClosePanel<UIGamePanel>();

			UIMgr.OpenPanel<UITrainOverPanel>();
		}
	}
}