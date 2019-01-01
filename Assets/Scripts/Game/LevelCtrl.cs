/****************************************************************************
 * 2018.12 liangxie
 * 
 * 教程地址:http://www.sikiedu.com/course/327
 ****************************************************************************/

using System.Collections;
using MoreMountains.CorgiEngine;
using QFramework;
using UnityEngine;

namespace IndieGame
{
	public class LevelCtrl : MonoBehaviour
	{
		private IEnumerator Start()
		{
			if (!GameData.HardModeUnlocked)
			{
				yield break;
			}
			
			// 隐藏掉 player 的火箭的值
			GameObject hudObj = null;
			
			yield return new WaitUntil(()=> hudObj = GameObject.Find("HUD"));

			hudObj.Hide();
			
			
			// 禁用掉 Player 的火箭
			GameObject playerObj = null;
			yield return new WaitUntil(() => playerObj = GameObject.Find("player"));

			var characterJetpack = playerObj.GetComponent<CharacterJetpack>();

			characterJetpack.AbilityPermitted = false;


		}
	}
}