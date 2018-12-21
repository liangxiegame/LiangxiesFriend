/****************************************************************************
 * 2018.12 LIANGXIE
 ****************************************************************************/

namespace IndieGame
{
	using UnityEngine;
	using UnityEngine.UI;

	public partial class UITrainOverPanel
	{
		public const string NAME = "UITrainOverPanel";

		[SerializeField] public Text LeveDeathCountCurrent;
		[SerializeField] public Text LevelDeathCountMin;
		[SerializeField] public Button BtnTrainMode;

		protected override void ClearUIComponents()
		{
			LeveDeathCountCurrent = null;
			LevelDeathCountMin = null;
			BtnTrainMode = null;
		}

		private UITrainOverPanelData mPrivateData = null;

		public UITrainOverPanelData mData
		{
			get { return mPrivateData ?? (mPrivateData = new UITrainOverPanelData()); }
			set
			{
				mUIData = value;
				mPrivateData = value;
			}
		}
	}
}
