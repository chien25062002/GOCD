using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CFramework
{
    /// <summary>
    /// Giống UIPointerDrag nhưng cho phép kéo ra ngoài vùng UI, vẫn nhận drag/dragEnd.
    /// Kế thừa từ UIPointerDrag để giữ cùng interface event.
    /// </summary>
    public class UIPointerDrag_FullScreen : UIPointerDrag
    {
        bool _isDragging;
        int _activeFingerId = -1;
        Vector2 _lastPos;

        public override void OnBeginDrag(PointerEventData eventData)
        {
            // Gọi base để tương thích (nếu ai override trong UIPointerDrag)
            base.OnBeginDrag(eventData);

            _isDragging = true;
            _activeFingerId = eventData.pointerId;
            _lastPos = eventData.position;
            eventDragBegin?.Invoke(eventData);
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            if (!_isDragging) return;
            _isDragging = false;
            _activeFingerId = -1;
            eventDragEnd?.Invoke(eventData);
        }

        void Update()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            if (_isDragging && Input.GetMouseButton(0))
            {
                var curPos = (Vector2)Input.mousePosition;
                var delta = curPos - _lastPos;
                var e = new PointerEventData(EventSystem.current)
                {
                    position = curPos,
                    delta = delta
                };
                eventDrag?.Invoke(e);
                _lastPos = curPos;
            }
            else if (_isDragging && Input.GetMouseButtonUp(0))
            {
                _isDragging = false;
                eventDragEnd?.Invoke(new PointerEventData(EventSystem.current));
            }
#else
            if (!_isDragging) return;

            foreach (var touch in Input.touches)
            {
                if (touch.fingerId == _activeFingerId)
                {
                    if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
                    {
                        var e = new PointerEventData(EventSystem.current)
                        {
                            position = touch.position,
                            delta = touch.deltaPosition
                        };
                        eventDrag?.Invoke(e);
                    }
                    else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                    {
                        _isDragging = false;
                        _activeFingerId = -1;
                        eventDragEnd?.Invoke(new PointerEventData(EventSystem.current));
                    }
                }
            }
#endif
        }
    }
}
