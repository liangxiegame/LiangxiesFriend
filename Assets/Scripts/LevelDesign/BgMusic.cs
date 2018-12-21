/****************************************************************************
 * 2018.12 liangxie
 * 
 * 教程地址:http://www.sikiedu.com/course/327
 ****************************************************************************/

using UnityEngine;
using MoreMountains.CorgiEngine;
using MoreMountains.Tools;

namespace IndieGame
{
	/// <summary>
	/// Add this class to a GameObject to have it play a background music when instanciated.
	/// Careful : only one background music will be played at a time.
	/// </summary>
	public class BgMusic : PersistentHumbleSingleton<BgMusic>
	{
		/// the background music
		public AudioClip SoundClip;

		protected AudioSource _source;

		/// <summary>
		/// Gets the AudioSource associated to that GameObject, and asks the GameManager to play it.
		/// </summary>
		protected virtual void Start()
		{
			_source = gameObject.AddComponent<AudioSource>() as AudioSource;
			_source.playOnAwake = false;
			_source.spatialBlend = 0;
			_source.rolloffMode = AudioRolloffMode.Logarithmic;
			_source.loop = true;

			_source.clip = SoundClip;

			SoundManager.Instance.MusicVolume = 0.3f;
			SoundManager.Instance.PlayBackgroundMusic(_source);
		}
	}
}