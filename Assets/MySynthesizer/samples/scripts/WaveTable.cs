
using System;
using UnityEngine;

namespace MySpace.Sample
{
    public class WaveTable : MonoBehaviour
    {
        public byte[] WT
        {
            get;
            private set;
        }

        public event Action<int> OnUpdateAction;

        private void Awake()
        {
            WT = new byte[32];
            var masterButton = transform.Find("Button").GetComponent<GridButton>();
            var r = masterButton.GetComponent<RectTransform>();
            var width = r.rect.width;
            var height = r.rect.height;
            var basePos = r.transform.localPosition;
            var baseScl = masterButton.transform.localScale;
            var baseRot = masterButton.transform.rotation;
            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 32; x++)
                {
                    var btn = Instantiate(masterButton);
                    var pos = new Vector3();
                    pos.x = basePos.x + x * width;
                    pos.y = basePos.y + y * height;
                    btn.transform.SetParent(transform);
                    btn.transform.localPosition = pos;
                    btn.transform.localScale = baseScl;
                    btn.transform.rotation = baseRot;
                    btn.Tx = x;
                    btn.Ty = y;
                    btn.WT = this;
                    var tx = x;
                    var ty = y;
                    btn.OnKeyDownEvent.AddListener(() =>
                    {
                        WT[tx] = (byte)ty;
                        if (OnUpdateAction != null)
                        {
                            OnUpdateAction(tx);
                        }
                    });
                }
            }
            masterButton.gameObject.SetActive(false);
        }
    }
}
