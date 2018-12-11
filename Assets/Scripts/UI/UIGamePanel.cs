/****************************************************************************
 * 2018.12 凉鞋的MacBook Pro (2)
 ****************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using QFramework;
using UnityEngine.SceneManagement;
using MoreMountains.CorgiEngine;
using MoreMountains.Tools;

namespace IndieGame
{
	public class UIGamePanelData : UIPanelData
	{
		// TODO: Query Mgr's Data
		public int DeathCount;

		public string InitLevelName = "Level8";
	}

	public partial class UIGamePanel : UIPanel,MMEventListener<CorgiEngineEvent>
	{
		public void OnMMEvent (CorgiEngineEvent eventType)
		{
			if (eventType.EventType == CorgiEngineEventTypes.PlayerDeath)
			{
				mData.DeathCount++;
				DeathCount.text = string.Format ("死亡次数:{0}", mData.DeathCount);
			}
		}
			
		protected override void InitUI(IUIData uiData = null)
		{
			mData = uiData as UIGamePanelData ?? new UIGamePanelData();

			SceneManager.LoadScene (mData.InitLevelName);

			this.MMEventStartListening<CorgiEngineEvent> ();

			SceneManager.sceneLoaded += OnSceneLoaded;
		}

		void OnSceneLoaded(Scene scene,LoadSceneMode mode)
		{
			if (scene.name == "GameWin")
			{
				Debug.LogFormat ("DeathCount:{0}", mData.DeathCount);

				CloseSelf ();
				UIMgr.OpenPanel<UIGameOverPanel> (new UIGameOverPanelData () 
				{
					DeathCountCurrent = mData.DeathCount
				});
			}
		}

		protected override void ProcessMsg (int eventId,QMsg msg)
		{
			throw new System.NotImplementedException ();
		}

		protected override void RegisterUIEvent()
		{
		}

		protected override void OnShow()
		{
			base.OnShow();
		}

		protected override void OnHide()
		{
			base.OnHide();
		}

		protected override void OnClose()
		{
			base.OnClose();

			this.MMEventStopListening<CorgiEngineEvent> ();

			SceneManager.sceneLoaded -= OnSceneLoaded;
		}

		void ShowLog(string content)
		{
			Debug.Log("[ UIGamePanel:]" + content);
		}
	}
}