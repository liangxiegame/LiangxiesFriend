/****************************************************************************
 * 2018.12 DESKTOP-ALVD4JR
 ****************************************************************************/

namespace IndieGame
{
	using UnityEngine;
	using UnityEngine.UI;

	public partial class UIGamePausePanel
	{
		public const string NAME = "UIGamePausePanel";

		[SerializeField] public Image PauseSplash;
		[SerializeField] public Animator BtnResume;
		[SerializeField] public Animator BtnRestart;
		[SerializeField] public Animator BtnHome;

		protected override void ClearUIComponents()
		{
			PauseSplash = null;
			BtnResume = null;
			BtnRestart = null;
			BtnHome = null;
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
