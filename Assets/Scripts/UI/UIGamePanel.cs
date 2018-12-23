/****************************************************************************
 * 2018.12 liangxie
 * 
 * 教程地址:http://www.sikiedu.com/course/327
 ****************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using QFramework;
using UnityEngine.SceneManagement;
using MoreMountains.CorgiEngine;
using MoreMountains.Tools;
using DG.Tweening;

namespace IndieGame
{
	public class UIGamePanelData : UIPanelData
	{
		// TODO: Query Mgr's Data
		public int DeathCount
		{
			get { return GameModeLogic.DeathCount; }
			set { GameModeLogic.DeathCount = value; }
		}

		public string InitLevelName = "Level24";

		public GameMode Mode = GameMode.Normal;
	}

	public partial class UIGamePanel : UIPanel,MMEventListener<CorgiEngineEvent>
	{
		public void OnMMEvent (CorgiEngineEvent eventType)
		{
			if (eventType.EventType == CorgiEngineEventTypes.PlayerDeath)
			{
				mData.DeathCount++;
				DeathCount.text = string.Format ("Death Count : {0}", mData.DeathCount);
				this.SendMsg (new AudioSoundMsg (QAssetBundle.Sounds.HIT));
			}
			else if (eventType.EventType == CorgiEngineEventTypes.Pause)
			{
				var pausePanel = UIMgr.GetPanel<UIGamePausePanel> ();

				if (pausePanel)
				{
					UIMgr.ClosePanel<UIGamePausePanel> ();
				}
				else
				{
					UIMgr.OpenPanel<UIGamePausePanel> (UILevel.PopUI);
				}
			}
			else if (eventType.EventType == CorgiEngineEventTypes.UnPause)
			{
				UIMgr.ClosePanel<UIGamePausePanel> ();
			}
		}
			
		protected override void InitUI(IUIData uiData = null)
		{	
			mData = uiData as UIGamePanelData ?? new UIGamePanelData ();

			GameModeLogic.Mode = mData.Mode;

			SceneManager.sceneLoaded += OnSceneLoaded;

			SceneManager.LoadScene (mData.InitLevelName);

			this.MMEventStartListening ();

			KeyBoardHelp.Hide ();
			
			DeathCount.text = string.Format ("Death Count : {0}", mData.DeathCount);
		}

		void OnSceneLoaded(Scene scene,LoadSceneMode mode)
		{			
			if (scene.name == "GameWin")
			{
				SendMsg(new AudioMusicMsg("magic"));
				
				Debug.LogFormat ("Death Count : {0}", mData.DeathCount);

				CloseSelf ();

				UIMgr.OpenPanel<UIStoryPanel>(new UIStoryPanelData()
				{
					StoryContent = @"主角终于找到了传说中的宝藏，

实现了 A 的梦想。",
					OnStoryFinish = storyPanel =>
					{
						storyPanel.DoTransition<UIGameOverPanel>(new FadeInOut(), uiData: new UIGameOverPanelData());
					}
				});
			}
			else if (scene.name.StartsWith ("Level"))
			{
				LevelName.text = scene.name;

				if (scene.name == "Level1")
				{
					if (GameData.FirstTimeEnterLevel1)
					{
						ShowKeyBoardHelp ();
						GameData.FirstTimeEnterLevel1 = false;
					}
				}
				
				SendMsg(new AudioMusicMsg(LevelConfig.GetBgMusicNameForLevelName(scene.name)));
			}
			else
			{
				LevelName.text = string.Empty;
			}
		}

		protected override void ProcessMsg (int eventId,QMsg msg)
		{
			throw new System.NotImplementedException ();
		}


		Sequence mSequence;

		private void ShowKeyBoardHelp()
		{
			if (mSequence != null)
			{
				mSequence.Kill ();
				mSequence = null;
			}

			KeyBoardHelp.DOKill ();

			KeyBoardHelp.Show ();
			KeyBoardHelp.ColorAlpha (1.0f);

			mSequence = DOTween.Sequence ()
				.Append (KeyBoardHelp.DOFade (1.0f, 3.0f))
				.Append (KeyBoardHelp.DOFade (0.0f, 1.0f))
				.OnComplete (() =>
			{
				KeyBoardHelp.Hide ();
				mSequence = null;
			});
		}

		protected override void RegisterUIEvent()
		{
			BtnKeyBoardHelp.onClick.AddListener (() =>
			{
				ShowKeyBoardHelp();
			});
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