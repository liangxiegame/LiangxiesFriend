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

		public static int DeathCount
		{
			get { return mModeLogic.DeathCount; }
			set { mModeLogic.DeathCount = value; }
		}

	}

	public interface IModeLogic
	{
		void LevelFinish();

		int DeathCount { get; set; }
	}

	public class NormalModeLogic : IModeLogic
	{
		private int mPlayerDeath;

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

		int IModeLogic.DeathCount
		{
			get { return GameData.CurDeathCount; }
			set { GameData.CurDeathCount = value; }
		}
	}

	public class TrainModeLogic : IModeLogic
	{
		private int mPlayerDeath;

		public void LevelFinish()
		{
			var levelName = SceneManager.GetActiveScene().name;
			
			SceneManager.LoadScene("Empty");

			UIMgr.ClosePanel<UIGamePanel>();

			UIMgr.OpenPanel<UITrainOverPanel>(new UITrainOverPanelData()
			{
				LevelName = levelName,
			});
		}

		int IModeLogic.DeathCount
		{
			get { return GameData.GetCurLevelDeathCount(SceneManager.GetActiveScene().name); }
			set { GameData.SetCurLevelDeathCount(SceneManager.GetActiveScene().name,value); }
		}
	}
}