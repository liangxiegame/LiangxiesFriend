/****************************************************************************
 * 2018.12 DESKTOP-ALVD4JR
 ****************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using QFramework;
using UniRx;

namespace IndieGame
{
	public partial class UILevelItem : UIElement
	{
		private string mLevelName;
		
		private void Awake()
		{
			GetComponent<Button>()
				.OnClickAsObservable()
				.Subscribe(_ =>
				{
					SendMsg(new AudioSoundMsg("Click"));
					UIMgr.GetPanel<UITrainModePanel>().DoTransition<UIGamePanel>(new FadeInOut(),
						uiData: new UIGamePanelData()
						{
							InitLevelName = mLevelName,
							Mode = GameMode.Train
						});
				});
		}

		public void Init(string levelName)
		{
			mLevelName = levelName;

			LevelName.text = levelName;
		}

		protected override void OnBeforeDestroy()
		{
		}
	}
}