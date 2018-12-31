using System.Collections.Generic;
using UnityEngine;
using QFramework;
using UnityEngine.SceneManagement;

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
}