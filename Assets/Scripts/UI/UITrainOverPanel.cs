/****************************************************************************
 * 2018.12 LIANGXIE
 ****************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using QFramework;
using UniRx;

namespace IndieGame
{
	public class UITrainOverPanelData : UIPanelData
	{
		// TODO: Query Mgr's Data
		public int DeathCountCurrent
		{
			get
			{
				return GameData.GetCurLevelDeathCount(LevelName);
			}
			set { GameData.SetCurLevelDeathCount(LevelName, value); }
		}

		public int DeathCountMin
		{
			get { return GameData.GetMinLevelDeathCount(LevelName); }
			set { GameData.SetMinLevelDeathCount(LevelName, value); }
		}

		public string LevelName { get; set; }
	}

	public partial class UITrainOverPanel : UIPanel
	{
		protected override void InitUI(IUIData uiData = null)
		{
			mData = uiData as UITrainOverPanelData ?? new UITrainOverPanelData();
			//please add init code here

			SendMsg(new AudioMusicMsg("village2"));
			
			LeveDeathCountCurrent.text = "{0} Death Count: {1}".FillFormat(mData.LevelName, mData.DeathCountCurrent);

			mData.DeathCountMin = mData.DeathCountMin > mData.DeathCountCurrent
				? mData.DeathCountCurrent
				: mData.DeathCountMin;

			LevelDeathCountMin.text = "{0} Death Count Record: {1}".FillFormat(mData.LevelName, mData.DeathCountMin);
			
			mData.DeathCountCurrent = 0;
		}

		protected override void ProcessMsg (int eventId,QMsg msg)
		{
			throw new System.NotImplementedException ();
		}

		protected override void RegisterUIEvent()
		{
			BtnTrainMode.OnClickAsObservable()
				.Subscribe(_ =>
				{
					CloseSelf();
					UIMgr.OpenPanel<UITrainModePanel>();
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
			Debug.Log("[ UITrainOverPanel:]" + content);
		}
	}
}