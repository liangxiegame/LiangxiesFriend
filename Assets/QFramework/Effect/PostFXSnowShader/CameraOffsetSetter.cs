using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QFramework
{
	public class CameraOffsetSetter : MonoBehaviour
	{

		private PostFXBehaviour mPostFxBehaviour;
		
		private void Awake()
		{
			mPostFxBehaviour =  GetComponent<PostFXBehaviour>();
		}

		// Update is called once per frame
		void Update()
		{
			mPostFxBehaviour.Material.SetFloat("OffsetX",transform.localPosition.x);
			mPostFxBehaviour.Material.SetFloat("OffsetY",transform.localPosition.y);
		}
	}
}