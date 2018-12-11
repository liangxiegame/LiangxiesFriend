/****************************************************************************
 * 2018.12 凉鞋的MacBook Pro (2)
 ****************************************************************************/

namespace IndieGame
{
	using UnityEngine;
	using UnityEngine.UI;

	public partial class UIGameOverPanel
	{
		public const string NAME = "UIGameOverPanel";

		[SerializeField] public Text DeathCountCurrent;
		[SerializeField] public Text DeathCountMin;
		[SerializeField] public Button BtnHome;

		protected override void ClearUIComponents()
		{
			DeathCountCurrent = null;
			DeathCountMin = null;
			BtnHome = null;
		}

		private UIGameOverPanelData mPrivateData = null;

		public UIGameOverPanelData mData
		{
			get { return mPrivateData ?? (mPrivateData = new UIGameOverPanelData()); }
			set
			{
				mUIData = value;
				mPrivateData = value;
			}
		}
	}
}
