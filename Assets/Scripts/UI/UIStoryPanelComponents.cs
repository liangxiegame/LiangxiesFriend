/****************************************************************************
 * 2018.12 凉鞋的MacBook Pro (2)
 ****************************************************************************/

namespace IndieGame
{
	using UnityEngine;
	using UnityEngine.UI;

	public partial class UIStoryPanel
	{
		public const string NAME = "UIStoryPanel";

		[SerializeField] public Text Content;
		[SerializeField] public Button BtnNext;

		protected override void ClearUIComponents()
		{
			Content = null;
			BtnNext = null;
		}

		private UIStoryPanelData mPrivateData = null;

		public UIStoryPanelData mData
		{
			get { return mPrivateData ?? (mPrivateData = new UIStoryPanelData()); }
			set
			{
				mUIData = value;
				mPrivateData = value;
			}
		}
	}
}
