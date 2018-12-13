/****************************************************************************
 * 2018.12 凉鞋的MacBook Pro (2)
 ****************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using QFramework;
using MoreMountains.Tools;
using MoreMountains.CorgiEngine;
using UniRx.Triggers;
using UniRx;

namespace IndieGame
{
	public class UIGamePausePanelData : UIPanelData
	{
		// TODO: Query Mgr's Data
	}

	public partial class UIGamePausePanel : UIPanel
	{
		protected override void InitUI(IUIData uiData = null)
		{
			mData = uiData as UIGamePausePanelData ?? new UIGamePausePanelData();
			//please add init code here
		}

		protected override void ProcessMsg (int eventId,QMsg msg)
		{
			throw new System.NotImplementedException ();
		}

		protected override void RegisterUIEvent()
		{
			BtnHome
				.transform
				.Find ("Container/Background")
				.GetComponent<MMTouchButton> ()
				.ButtonPressedFirstTime.AddListener (() =>
			{
				Log.I ("@@@@");
				UIMgr.ClosePanel<UIGamePanel> ();
				CloseSelf ();
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
			Debug.Log("[ UIGamePausePanel:]" + content);
		}
	}
}