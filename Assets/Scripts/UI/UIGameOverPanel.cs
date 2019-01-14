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

namespace IndieGame
{
	public class UIGameOverPanelData : UIPanelData
	{
		public int DeathCountCurrent
		{
			get { return GameData.CurDeathCount; }
			set { GameData.CurDeathCount = value; }
		}

		public int DeathCountMin
		{
			get { return GameData.DeathCountMin; }
			set { GameData.DeathCountMin = value; }
		}

		public bool HardModeUnlocked
		{
			get { return GameData.HardModeUnlocked; }
			set { GameData.HardModeUnlocked = true; }
		}
	}

	public partial class UIGameOverPanel : UIPanel
	{
		protected override void InitUI(IUIData uiData = null)
		{
			mData = uiData as UIGameOverPanelData ?? new UIGameOverPanelData();
			//please add init code here
			
			Playlist.PlayMusic("village2");

			mData.DeathCountMin = GameData.DeathCountMin >= mData.DeathCountCurrent ? mData.DeathCountCurrent : mData.DeathCountMin;

			DeathCountCurrent.text = string.Format ("Death Count : {0}", mData.DeathCountCurrent);
			DeathCountMin.text = string.Format ("Death Count Record : {0}", mData.DeathCountMin);

			mData.DeathCountCurrent = 0;
			
			if (mData.DeathCountCurrent == 0 && mData.HardModeUnlocked == false)
			{
				mData.HardModeUnlocked = true;

				Title.text = "Hard Mode Unlocked !!!";
				Title.fontSize = (int) (Title.fontSize * 0.8f);

				mData.DeathCountMin = int.MaxValue;
			}
		}

		protected override void ProcessMsg (int eventId,QMsg msg)
		{
			throw new System.NotImplementedException ();
		}

		protected override void RegisterUIEvent()
		{
			BtnHome.onClick.AddListener (() =>
			{
				SendMsg(new AudioSoundMsg("Click"));
				CloseSelf();
				UIMgr.OpenPanel<UIHomePanel>();
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
		}

		void ShowLog(string content)
		{
			Debug.Log("[ UIGameOverPanel:]" + content);
		}
	}
}