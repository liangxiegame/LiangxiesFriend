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

namespace IndieGame
{
	public class UITrainModePanelData : UIPanelData
	{
		// TODO: Query Mgr's Data

		public List<string> UnlockedLevelNames
		{
			get
			{
				var curLevelIndex = LevelConfig.CurrentLevelIndex(GameData.CurLevelName);

				var unlockedLevelNames = LevelConfig.LevelNamesOrder.GetRange(0, curLevelIndex + 1);

				return unlockedLevelNames;
			}
		}
	}

	public partial class UITrainModePanel : UIPanel
	{
		protected override void InitUI(IUIData uiData = null)
		{
			mData = uiData as UITrainModePanelData ?? new UITrainModePanelData();
			//please add init code here

			mData.UnlockedLevelNames.ForEach(levelName =>
			{
				UILevelItemTemplate.Instantiate()
					.Parent(Content)
					.LocalIdentity()
					.ApplySelfTo(self =>
					{
						self.Init(levelName);
						self.Show();
					});
			});

			Observable.NextFrame().Subscribe(_ =>
			{
				var preferredHeight = Content.GetComponent<GridLayoutGroup>().preferredHeight;

				if (preferredHeight > 514)
				{
					Content.GetComponent<ContentSizeFitter>().Enable();
				}
			});
			
			AudioManager.StopVoice();
		}

		protected override void ProcessMsg (int eventId,QMsg msg)
		{
			throw new System.NotImplementedException ();
		}

		protected override void RegisterUIEvent()
		{
			BtnBack.OnClickAsObservable().Subscribe(_ =>
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
			base.OnClose();
		}

		void ShowLog(string content)
		{
			Debug.Log("[ UITrainModePanel:]" + content);
		}
	}
}