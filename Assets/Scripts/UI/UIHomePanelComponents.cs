/****************************************************************************
 * 2019.1 LIANGXIE
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
		[SerializeField] public Text Version;
		[SerializeField] public Button BtnAbout;
		[SerializeField] public Button BtnTrainMode;
		[SerializeField] public Button BtnReset;

		protected override void ClearUIComponents()
		{
			BtnStartGame = null;
			DeathCountMin = null;
			Version = null;
			BtnAbout = null;
			BtnTrainMode = null;
			BtnReset = null;
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
