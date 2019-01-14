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

				UIMgr.OpenPanel<UIStoryPanel>(new UIStoryPanelData()
				{
					StoryContent = @"这是一个关于友情的故事。

主角有一个朋友 A，

梦想是要去天空寻找传说中的宝藏。

但是朋友 A 在一次意外中去世了。

在临死前主角答应 A 替 A 完成梦想，

在 A 的葬礼之后，主角开始履行承诺。

踏上了上天之路。
",
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