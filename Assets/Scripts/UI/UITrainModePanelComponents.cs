/****************************************************************************
 * 2018.12 DESKTOP-ALVD4JR
 ****************************************************************************/

namespace IndieGame
{
	using UnityEngine;
	using UnityEngine.UI;

	public partial class UITrainModePanel
	{
		public const string NAME = "UITrainModePanel";

		[SerializeField] public UILevelItem UILevelItemTemplate;
		[SerializeField] public RectTransform Content;
		[SerializeField] public Button BtnBack;

		protected override void ClearUIComponents()
		{
			UILevelItemTemplate = null;
			Content = null;
			BtnBack = null;
		}

		private UITrainModePanelData mPrivateData = null;

		public UITrainModePanelData mData
		{
			get { return mPrivateData ?? (mPrivateData = new UITrainModePanelData()); }
			set
			{
				mUIData = value;
				mPrivateData = value;
			}
		}
	}
}
