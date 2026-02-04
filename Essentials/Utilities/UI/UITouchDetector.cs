using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game
{
    public class UITouchDetector : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
    {
        static int s_touchId;
        
        public event Action<PointerEventData> eventPointerClick;
        public event Action<PointerEventData> eventPointerDown;
        public event Action<PointerEventData> eventPointerUp;

        public event Action<PointerEventData> eventPointerEnter;
        public event Action<PointerEventData> eventPointerExit;

        public event Action<PointerEventData> eventBeginDrag;
        public event Action<PointerEventData> eventDrag;
        public event Action<PointerEventData> eventEndDrag;

        bool _isDragging = false;

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            _isDragging = true;

            eventBeginDrag?.Invoke(eventData);
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
#if !UNITY_EDITOR
            if (s_touchId != s_touchId) return;
#endif
            
            eventDrag?.Invoke(eventData);
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            _isDragging = false;

            eventEndDrag?.Invoke(eventData);
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if (_isDragging)
                return;

            eventPointerClick?.Invoke(eventData);
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
#if !UNITY_EDITOR
            if (s_touchId != s_touchId) return;
            if (s_touchId < 0)
                s_touchId = eventData.pointerId;
#endif
            eventPointerDown?.Invoke(eventData);
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
#if !UNITY_EDITOR
            if (s_touchId != s_touchId) return;
            s_touchId = int.MinValue;
#endif
            eventPointerUp?.Invoke(eventData);
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            eventPointerEnter?.Invoke(eventData);
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            eventPointerExit?.Invoke(eventData);
        }
    }
}