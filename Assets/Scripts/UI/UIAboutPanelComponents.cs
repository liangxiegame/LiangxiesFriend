/****************************************************************************
 * 2018.12 凉鞋的MacBook Pro (2)
 ****************************************************************************/

namespace IndieGame
{
	using UnityEngine;
	using UnityEngine.UI;

	public partial class UIAboutPanel
	{
		public const string NAME = "UIAboutPanel";

		[SerializeField] public Button BtnBack;

		protected override void ClearUIComponents()
		{
			BtnBack = null;
		}

		private UIAboutPanelData mPrivateData = null;

		public UIAboutPanelData mData
		{
			get { return mPrivateData ?? (mPrivateData = new UIAboutPanelData()); }
			set
			{
				mUIData = value;
				mPrivateData = value;
			}
		}
	}
}
