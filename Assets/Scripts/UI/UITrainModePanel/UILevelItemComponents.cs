/****************************************************************************
 * 2018.12 DESKTOP-ALVD4JR
 ****************************************************************************/

using UnityEngine;
using UnityEngine.UI;
using QFramework;

namespace IndieGame
{
	public partial class UILevelItem
	{
		[SerializeField] public Text LevelName;

		public void Clear()
		{
			LevelName = null;
		}

		public override string ComponentName
		{
			get { return "UILevelItem";}
		}
	}
}
