/****************************************************************************
 * 2018.12 凉鞋的MacBook Pro (2)
 ****************************************************************************/

namespace IndieGame
{
	using UnityEngine;
	using UnityEngine.UI;

	public partial class UIGamePausePanel
	{
		public const string NAME = "UIGamePausePanel";

		[SerializeField] public Image PauseSplash;

		protected override void ClearUIComponents()
		{
			PauseSplash = null;
		}

		private UIGamePausePanelData mPrivateData = null;

		public UIGamePausePanelData mData
		{
			get { return mPrivateData ?? (mPrivateData = new UIGamePausePanelData()); }
			set
			{
				mUIData = value;
				mPrivateData = value;
			}
		}
	}
}
