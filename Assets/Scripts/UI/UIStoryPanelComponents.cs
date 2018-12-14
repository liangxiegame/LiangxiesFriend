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
		[SerializeField] public Button BtnSkip;

		protected override void ClearUIComponents()
		{
			Content = null;
			BtnSkip = null;
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
