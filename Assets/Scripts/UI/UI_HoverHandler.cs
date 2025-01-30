using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    public class OnHoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public UnityEngine.Events.UnityEvent onHoverEnter;
        public UnityEngine.Events.UnityEvent onHoverExit;

        public void OnPointerEnter(PointerEventData eventData)
        {
            onHoverEnter?.Invoke();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            onHoverExit?.Invoke();
        }
    }
}
