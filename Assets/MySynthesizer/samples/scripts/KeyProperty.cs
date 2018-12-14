
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace MySpace.Sample
{
    public class KeyProperty : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
    {
        private Image image = null;
        private bool downed = false;
        private readonly UnityEvent onKeyDownEvent = new UnityEvent();
        private readonly UnityEvent onKeyUpEvent = new UnityEvent();

        [SerializeField]
        private Sprite normal = null;
        [SerializeField]
        private Sprite pushed = null;

        public UnityEvent OnKeyDownEvent
        {
            get
            {
                return onKeyDownEvent;
            }
        }
        public UnityEvent OnKeyUpEvent
        {
            get
            {
                return onKeyUpEvent;
            }
        }
        private void down()
        {
            if (!downed)
            {
                downed = true;
                onKeyDownEvent.Invoke();
                if (pushed != null)
                {
                    image.sprite = pushed;
                }
            }
        }
        private void up()
        {
            if (downed)
            {
                downed = false;
                onKeyUpEvent.Invoke();
                if (normal != null)
                {
                    image.sprite = normal;
                }
            }
        }
        private void OnEnable()
        {
            image = GetComponent<Image>();
            if (normal != null)
            {
                image.sprite = normal;
            }
            downed = false;
        }
        private void OnDisable()
        {
            image = null;
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
