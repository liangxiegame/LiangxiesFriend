/****************************************************************************
 * 2018.12 liangxie
 * 
 * 教程地址:http://www.sikiedu.com/course/327
 ****************************************************************************/

using MoreMountains.CorgiEngine;
using MoreMountains.Tools;
using QFramework;
using UnityEngine;

namespace IndieGame
{
	public class ReactiveFallingPlatform : MonoBehaviour, MMEventListener<CorgiEngineEvent>
	{
		private void Start()
		{
			this.MMEventStartListening();
		}

		public void OnMMEvent(CorgiEngineEvent eventType)
		{
			if (eventType.EventType == CorgiEngineEventTypes.Respawn)
			{
				this.Show();
			}
		}

		private void OnDestroy()
		{
			this.MMEventStopListening();
		}
	}
}
