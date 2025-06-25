using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CFramework
{
    public class UIPointerDrag : MonoBehaviour, IDragHandler, IEndDragHandler, IBeginDragHandler
    {
        public event Action<PointerEventData> eventDragBegin;
        public event Action<PointerEventData> eventDrag;
        public event Action<PointerEventData> eventDragEnd;

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            eventDragBegin?.Invoke(eventData);
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            eventDrag?.Invoke(eventData);
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            eventDragEnd?.Invoke(eventData);
        }
    }
}
