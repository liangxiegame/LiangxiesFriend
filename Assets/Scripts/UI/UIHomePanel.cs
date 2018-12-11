/****************************************************************************
 * 2018.12 凉鞋的MacBook Pro (2)
 ****************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using QFramework;
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
			DeathCountMin.text = string.Format ("最小通关死亡次数:{0}", GameData.DeathCountMin == int.MaxValue ? "无":GameData.DeathCountMin.ToString());
		}

		protected override void ProcessMsg (int eventId,QMsg msg)
		{
			throw new System.NotImplementedException ();
		}

		protected override void RegisterUIEvent()
		{
			BtnStartGame.onClick.AddListener (() =>
			{
				CloseSelf ();
				UIMgr.OpenPanel<UIGamePanel> (new UIGamePanelData () 
				{
					InitLevelName = "Level1"
				});
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