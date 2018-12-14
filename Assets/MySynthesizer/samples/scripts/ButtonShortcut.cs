using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace MySpace.Sample
{
    public class ButtonShortcut : MonoBehaviour
    {
        [SerializeField]
        private KeyCode key = KeyCode.None;

        private Button button = null;

        private void Awake()
        {
            button = GetComponent<Button>();
        }

        private void Update()
        {
            var cur = EventSystem.current.currentSelectedGameObject;
            if ((cur != null) && (cur.GetComponent<InputField>() != null))
            {
                return;
            }
            if (Input.GetKeyDown(key))
            {
                ExecuteEvents.Execute(button.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerClickHandler);
            }
        }
    }
}
