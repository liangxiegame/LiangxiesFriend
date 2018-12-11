/****************************************************************************
 * 2018.12 凉鞋的MacBook Pro (2)
 ****************************************************************************/

namespace IndieGame
{
	using UnityEngine;
	using UnityEngine.UI;

	public partial class UIHomePanel
	{
		public const string NAME = "UIHomePanel";

		[SerializeField] public Button BtnStartGame;
		[SerializeField] public Text DeathCountMin;

		protected override void ClearUIComponents()
		{
			BtnStartGame = null;
			DeathCountMin = null;
		}

		private UIHomePanelData mPrivateData = null;

		public UIHomePanelData mData
		{
			get { return mPrivateData ?? (mPrivateData = new UIHomePanelData()); }
			set
			{
				mUIData = value;
				mPrivateData = value;
			}
		}
	}
}
