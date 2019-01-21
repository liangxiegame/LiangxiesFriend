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
using UniRx;
using UnityEngine.SceneManagement;

namespace IndieGame
{
	public class UIHomePanelData : UIPanelData
	{
		// TODO: Query Mgr's Data
	}

	public partial class UIHomePanel : UIPanel
	{


		protected override void InitUI(IUIData uiData = null)
		{
			mData = uiData as UIHomePanelData ?? new UIHomePanelData();
			//please add init code here
			DeathCountMin.text = string.Format ("Death Count Min : {0}", GameData.DeathCountMin == int.MaxValue ? "None":GameData.DeathCountMin.ToString());

			Version.text = "v" + Application.version;

			Observable.EveryUpdate().Where(_ => Input.GetKeyDown(KeyCode.Space))
				.Subscribe(_ =>
				{
					StartGame();	
				}).AddTo(this);
			
			
			
			Playlist.PlayMusic("speed");
			
			AudioManager.StopVoice();


			if (GameData.CurLevelName == "Level1" && GameData.CurDeathCount == 0 && !GameData.HardModeUnlocked)
			{
				BtnReset.Hide();
			}
			else
			{
				BtnReset.Show();
			}
		}

		private void StartGame()
		{
			SendMsg(new AudioSoundMsg("Click"));

			if (GameData.CurLevelName == "Level1")
			{
				CloseSelf ();

				
				UnityEngine.Random.InitState(DateTime.Now.Millisecond);
				GameData.StoryIndex = UnityEngine.Random.Range(0, 100) % StoryConfig.Stories.Count;
				
				
				UIMgr.OpenPanel<UIStoryPanel>(new UIStoryPanelData()
				{
					
					OnStoryFinish = storyPanel =>
					{
						storyPanel.DoTransition<UIGamePanel>(new FadeInOut(), uiData: new UIGamePanelData()
						{
							InitLevelName = GameData.CurLevelName
						});
					}
				});
			}
			else
			{
				this.DoTransition<UIGamePanel>(new FadeInOut(), uiData: new UIGamePanelData()
				{
					InitLevelName = GameData.CurLevelName
				});
			}
		}

		protected override void ProcessMsg (int eventId,QMsg msg)
		{
			throw new System.NotImplementedException ();
		}

		protected override void RegisterUIEvent()
		{
			BtnStartGame.onClick.AddListener (StartGame);


			BtnAbout.onClick.AddListener (() =>
			{
				SendMsg(new AudioSoundMsg("Click"));
				CloseSelf();
				UIMgr.OpenPanel<UIAboutPanel> ();
			});

			BtnTrainMode.OnClickAsObservable()
				.Subscribe(_ =>
				{
					SendMsg(new AudioSoundMsg("Click"));
					CloseSelf();
					UIMgr.OpenPanel<UITrainModePanel>();
				});

			BtnReset.OnClickAsObservable()
				.Subscribe(_ =>
				{
					GameData.CurLevelName = "Level1";
					GameData.CurDeathCount = 0;
					GameData.HardModeUnlocked = false;

					BtnReset.Hide();
				});
		}
		
		protected override void OnClose()
		{
			
		}
	}
}