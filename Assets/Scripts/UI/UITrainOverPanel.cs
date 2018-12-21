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
				return GameData.CurDeathCount;
			}
		}
	}

	public partial class UITrainOverPanel : UIPanel
	{
		protected override void InitUI(IUIData uiData = null)
		{
			mData = uiData as UITrainOverPanelData ?? new UITrainOverPanelData();
			//please add init code here

			LeveDeathCountCurrent.text = "Level1 Death Count: {0}".FillFormat(mData.DeathCountCurrent);
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