/****************************************************************************
 * 2018.12 凉鞋的MacBook Pro (2)
 ****************************************************************************/

namespace IndieGame
{
	using UnityEngine;
	using UnityEngine.UI;

	public partial class UIGamePanel
	{
		public const string NAME = "UIGamePanel";

		[SerializeField] public Text DeathCount;

		protected override void ClearUIComponents()
		{
			DeathCount = null;
		}

		private UIGamePanelData mPrivateData = null;

		public UIGamePanelData mData
		{
			get { return mPrivateData ?? (mPrivateData = new UIGamePanelData()); }
			set
			{
				mUIData = value;
				mPrivateData = value;
			}
		}
	}
}
