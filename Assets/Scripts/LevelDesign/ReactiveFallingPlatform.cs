using System.Collections;
using System.Collections.Generic;
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
