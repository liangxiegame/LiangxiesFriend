/****************************************************************************
 * 2018.12 ~ 2019.1 liangxie
 * 
 * 教程地址:http://www.sikiedu.com/course/327
 ****************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using QFramework;
using DG.Tweening;
using UniRx;
using Random = UnityEngine.Random;

namespace IndieGame
{
	public class UIStoryPanelData : UIPanelData
	{
		public bool IsEnd = false;

		public int StoryIndex
		{
			get { return GameData.StoryIndex; }
		}

		public Action<UIStoryPanel> OnStoryFinish = storyPanel =>
		{
			storyPanel.DoTransition<UIGamePanel>(new FadeInOut(), uiData: new UIGamePanelData()
			{
				InitLevelName = "Level1"
			});
		};

		public int StoryItemIndex { get; set; }

		public string StoryText
		{
			get { return StoryItems[StoryItemIndex]; }
		}

		private List<string> StoryItems
		{
			get { return IsEnd ? StoryConfig.StorySummary[StoryIndex] : StoryConfig.Stories[StoryIndex]; }
		}

		public bool StoryFinished
		{
			get { return StoryItemIndex >= StoryItems.Count; }
		}
	}

	public partial class UIStoryPanel : UIPanel
	{
		protected override void InitUI(IUIData uiData = null)
		{
			mData = uiData as UIStoryPanelData ?? new UIStoryPanelData();
			//please add init code here
			
			Playlist.PlayMusic("water");
			
			BtnNext.Hide();

			PlayStoryItem();
		}

		private IDisposable mPlayStoryItemTask;

		private void PlayStoryItem()
		{
			if (mPlayStoryItemTask != null)
			{
				mPlayStoryItemTask.Dispose();
				mPlayStoryItemTask = null;
			}

			Content.DOKill();

			Content.text = string.Empty;
			var storyItemText = mData.StoryText;

			Content.DOText(storyItemText, 10.0f / 128 * storyItemText.Length)
				.OnComplete(() => { BtnNext.Show(); });

			// 监听鼠标点击事件
			var skipObservable = Observable.EveryUpdate()
				.Where(_ =>
					Input.anyKeyDown)
				.Do(_ =>
				{
					if (mData.StoryFinished) return;
					
					Content.DOKill();
					Content.text = mData.StoryText;
					BtnNext.Show();
				});

			var nextObservable = Observable.EveryUpdate()
				.Where(_ => Input.GetKeyDown(KeyCode.Space))
				.Do(_ => { Next(); });


			mPlayStoryItemTask = skipObservable.SelectMany(nextObservable)
				.Subscribe(_ => { mPlayStoryItemTask = null; }).AddTo(this);
		}

		void Next()
		{
			SendMsg(new AudioSoundMsg("Click"));

			mData.StoryItemIndex++;
			
			if (mData.StoryFinished)
			{
				BtnNext.Hide();
				mData.OnStoryFinish.InvokeGracefully(this);
			}
			else
			{
				PlayStoryItem();
			}
		}

		protected override void ProcessMsg (int eventId,QMsg msg)
		{
			throw new System.NotImplementedException ();
		}

		protected override void RegisterUIEvent()
		{
			BtnNext.onClick.AddListener (Next);
		}

		protected override void OnClose()
		{
		}

		void ShowLog(string content)
		{
			Debug.Log("[ UIStoryPanel:]" + content);
		}
	}
}