/****************************************************************************
 * 2018.12 凉鞋的MacBook Pro (2)
 ****************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using QFramework;
using DG.Tweening;
using UniRx;

namespace IndieGame
{
	public class UIStoryPanelData : UIPanelData
	{
		public string StoryContent = @"主角终于找到了传说中的宝藏，

实现了 A 的梦想。";

		public Action<UIStoryPanel> OnStoryFinish = storyPanel =>
		{
			storyPanel.DoTransition<UIGamePanel> (new FadeInOut (), uiData: new UIGamePanelData () {
				InitLevelName = "Level1"
			});
		};
	}

	public partial class UIStoryPanel : UIPanel
	{
		protected override void InitUI(IUIData uiData = null)
		{
			mData = uiData as UIStoryPanelData ?? new UIStoryPanelData ();
			//please add init code here

			BtnNext.Hide ();

			Content.text = string.Empty;
			Content.DOText (mData.StoryContent, 10.0f / 128 * mData.StoryContent.Length)
				.OnComplete (() =>
			{
				BtnNext.Show ();
			});

			// 监听鼠标点击事件
			Observable.EveryUpdate ()
				.Where (_ => 
					Input.GetMouseButtonUp (0) ||
			Input.GetKeyUp (KeyCode.H) ||
			Input.GetKeyUp (KeyCode.Return) ||
			Input.GetKeyUp (KeyCode.Space))
				.Subscribe (_ =>
			{
				Content.DOKill ();
				Content.text = mData.StoryContent;
				BtnNext.Show ();

			}).AddTo (this);
		}

		void GotoNextPanel()
		{
			mData.OnStoryFinish.InvokeGracefully(this);
		}

		protected override void ProcessMsg (int eventId,QMsg msg)
		{
			throw new System.NotImplementedException ();
		}

		protected override void RegisterUIEvent()
		{
			BtnNext.onClick.AddListener (() =>
			{
				SendMsg(new AudioSoundMsg("Click"));
				GotoNextPanel();
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
			Debug.Log("[ UIStoryPanel:]" + content);
		}
	}
}