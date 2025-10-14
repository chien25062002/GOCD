using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CFramework
{
    public class UIPointerDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public Action<PointerEventData> eventDragBegin;
        public Action<PointerEventData> eventDrag;
        public Action<PointerEventData> eventDragEnd;

        // ✅ Dùng public virtual thay vì explicit interface → có thể override
        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            eventDragBegin?.Invoke(eventData);
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            eventDrag?.Invoke(eventData);
        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
            eventDragEnd?.Invoke(eventData);
        }
    }
}