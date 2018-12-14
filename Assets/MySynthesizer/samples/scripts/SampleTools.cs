using UnityEngine;

namespace MySpace.Sample
{
    public class SampleTools : MonoBehaviour
    {
        private float time = 1.0f;
        private Vector3 startPos;
        private Vector3 targetPos;
        public void MoveCameraTo(Vector3 loc)
        {
            time = 0.0f;
            startPos = Camera.main.transform.localPosition;
            targetPos = new Vector3(loc.x, loc.y, startPos.z);
        }
        private void Update()
        {
            if (time < 1.0f)
            {
                var dt = Time.deltaTime;
                time += dt / 0.3f;
                if (time > 1.0f)
                {
                    time = 1.0f;
                }
                var t = (0.5f - 0.5f * UnityEngine.Mathf.Cos(time * UnityEngine.Mathf.PI));
                var pos = startPos + (targetPos - startPos) * t;
                Camera.main.transform.localPosition = pos;
            }
        }
    }
}
