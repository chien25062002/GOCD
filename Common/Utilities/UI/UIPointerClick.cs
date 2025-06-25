using System;
using UnityEngine.EventSystems;

namespace GOCD.Framework
{
    public class UIPointerClick : MonoCached, IPointerDownHandler, IPointerUpHandler
    {
        public event Action eventDown;
        public event Action eventUp;

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            eventDown?.Invoke();
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            eventUp?.Invoke();
        }
    }
}
