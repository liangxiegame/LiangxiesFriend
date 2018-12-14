
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace MySpace.Sample
{
    public class GridButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField]
        private Color normal = Color.white;
        [SerializeField]
        private Color pushed = Color.green;

        private bool downed;
        private Image image = null;
        private UnityEvent onKeyDownEvent = new UnityEvent();

        public WaveTable WT
        {
            get;
            set;
        }
        public int Tx
        {
            get;
            set;
        }
        public int Ty
        {
            get;
            set;
        }
        public UnityEvent OnKeyDownEvent
        {
            get
            {
                return onKeyDownEvent;
            }
        }
        private void OnEnable()
        {
            image = GetComponent<Image>();
            downed = false;
        }
        private void Update()
        {
            image.color = (WT.WT[Tx] >= Ty) ? pushed : normal;
        }
        private void OnDisable()
        {
            image = null;
        }
        private void down()
        {
            if (!downed)
            {
                downed = true;
                onKeyDownEvent.Invoke();
            }
        }
        private void up()
        {
            if (downed)
            {
                downed = false;
            }
        }
        public void OnPointerDown(PointerEventData data)
        {
            down();
        }
        public void OnPointerUp(PointerEventData data)
        {
            up();
        }
        public void OnPointerEnter(PointerEventData data)
        {
            if (data.eligibleForClick)
            {
                data.pointerPress = gameObject;
                data.pointerDrag = gameObject;
                //data.rawPointerPress = gameObject;
                down();
            }
        }
        public void OnPointerExit(PointerEventData data)
        {
            up();
        }
    }
}
