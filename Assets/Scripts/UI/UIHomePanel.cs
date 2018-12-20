/****************************************************************************
 * 2018.12 凉鞋的MacBook Pro (2)
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
		}

		private void StartGame()
		{
			SendMsg(new AudioSoundMsg("Click"));
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
						InitLevelName = "Level1"
					});
				}
			});
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
				UIMgr.OpenPanel<UIAboutPanel> (UILevel.PopUI);
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
		}

		void ShowLog(string content)
		{
			Debug.Log("[ UIHomePanel:]" + content);
		}
	}
}