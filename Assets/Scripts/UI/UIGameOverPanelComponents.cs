/****************************************************************************
 * 2019.1 LIANGXIE
 ****************************************************************************/

namespace IndieGame
{
	using UnityEngine;
	using UnityEngine.UI;

	public partial class UIGameOverPanel
	{
		public const string NAME = "UIGameOverPanel";

		[SerializeField] public Text Title;
		[SerializeField] public Text DeathCountCurrent;
		[SerializeField] public Text DeathCountMin;
		[SerializeField] public Button BtnHome;

		protected override void ClearUIComponents()
		{
			Title = null;
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
