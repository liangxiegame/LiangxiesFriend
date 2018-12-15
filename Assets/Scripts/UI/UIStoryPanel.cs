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
		// TODO: Query Mgr's Data
	}

	public partial class UIStoryPanel : UIPanel
	{
		protected override void InitUI(IUIData uiData = null)
		{
			mData = uiData as UIStoryPanelData ?? new UIStoryPanelData ();
			//please add init code here

			BtnNext.Hide ();

			var text = Content.text;
			Content.text = string.Empty;
			Content.DOText (text, 10.0f)
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
				Content.text = text;
				BtnNext.Show ();

			}).AddTo (this);
		}

		void GotoNextPanel()
		{
			this.DoTransition<UIGamePanel> (new FadeInOut (), uiData: new UIGamePanelData () {
				InitLevelName = "Level1"
			});
		}

		protected override void ProcessMsg (int eventId,QMsg msg)
		{
			throw new System.NotImplementedException ();
		}

		protected override void RegisterUIEvent()
		{
			BtnNext.onClick.AddListener (() =>
			{
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