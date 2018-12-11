using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QFramework;

namespace IndieGame
{
	public class AppLoader : MonoBehaviour 
	{
		void Awake()
		{
			ResMgr.Init ();

			UIMgr.SetResolution (1024, 768, 1.0f);

			DontDestroyOnLoad (UIManager.Instance);
		}

		void Start () 
		{
			UIMgr.OpenPanel<UIHomePanel> ();
		}
	}


	public class GameData
	{
		public static int DeathCountMin
		{
			get
			{
				return PlayerPrefs.GetInt ("DEATH_COUNT_MIN", int.MaxValue);
			}
			set
			{
				PlayerPrefs.SetInt ("DEATH_COUNT_MIN", value);
			}
		}
	}
}