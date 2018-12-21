/****************************************************************************
 * 2018.12 liangxie
 * 
 * 教程地址:http://www.sikiedu.com/course/327
 ****************************************************************************/

using UnityEngine;
using MoreMountains.Tools;
using MoreMountains.CorgiEngine;
using QFramework;
using UniRx;
using UnityEngine.SceneManagement;

namespace IndieGame
{
	[AddComponentMenu("独立游戏/关卡设计/下一关")]
	public class NextLevel : ButtonActivated 
	{		
		/// <summary>
		/// When the button is pressed we start the dialogue
		/// </summary>
		public override void TriggerButtonAction()
		{
			if (!CheckNumberOfUses())
			{
				return;
			}
			base.TriggerButtonAction ();
			GoToNextLevel();
			ZoneActivated ();
		}

		/// <summary>
		/// Loads the next level
		/// </summary>
		public virtual void GoToNextLevel()
		{
			AudioManager.Instance.SendMsg(new AudioSoundMsg("pass_level"));
			GameModeLogic.LevelFinish();
		}
	}
}