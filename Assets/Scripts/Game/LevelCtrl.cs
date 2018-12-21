/****************************************************************************
 * 2018.12 liangxie
 * 
 * 教程地址:http://www.sikiedu.com/course/327
 ****************************************************************************/

using System.Collections;
using System.Collections.Generic;
using QFramework;
using UniRx;
using UnityEngine;

namespace IndieGame
{
	public class LevelCtrl : MonoBehaviour
	{
		private IEnumerator Start()
		{
			yield return new WaitForSeconds(0.5f);	
			
			var cameraObj = GameObject.Find("UICamera");

			Transform canvasTrans = null;

			yield return new WaitUntil(() => canvasTrans = cameraObj.transform.Find("Canvas"));

			Transform hudTrans = null;

			yield return new WaitUntil(() => hudTrans = canvasTrans.Find("HUD"));

			hudTrans.Show();
			hudTrans.Find("JetpackBar").Show();
			hudTrans.Find("HealthBar").Hide();
			hudTrans.Find("AvatarBackground").Hide();
			hudTrans.Find("AvatarHead").Hide();
		}
	}
}