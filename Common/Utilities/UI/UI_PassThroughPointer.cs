using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace Game
{
    public class UI_PassThroughPointer : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        GameObject _lastPressedObject;

        public void OnPointerDown(PointerEventData eventData)
        {
            _lastPressedObject = PassThrough(eventData, ExecuteEvents.pointerDownHandler);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            GameObject upTarget = PassThrough(eventData, ExecuteEvents.pointerUpHandler);

            if (_lastPressedObject != null && _lastPressedObject == upTarget)
            {
                // Gửi sự kiện OnPointerClick tới đúng object đã nhấn và thả
                ExecuteEvents.Execute(_lastPressedObject, eventData, ExecuteEvents.pointerClickHandler);
            }

            _lastPressedObject = null;
        }

        /// <summary>
        /// Gửi sự kiện xuống object UI bên dưới nếu không phải chính mình, và trả về object được target
        /// </summary>
        GameObject PassThrough<T>(PointerEventData eventData, ExecuteEvents.EventFunction<T> handler)
            where T : IEventSystemHandler
        {
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            foreach (var result in results)
            {
                // 💥 Tránh raycast chính object hiện tại HOẶC object nằm trong cùng cây với nó
                if (result.gameObject == gameObject || result.gameObject.transform.IsChildOf(transform))
                    continue;

                // Nếu là pointerDown thì gán pointerPress
                if (typeof(T) == typeof(IPointerDownHandler))
                {
                    eventData.pointerPress = result.gameObject;
                    eventData.rawPointerPress = result.gameObject;
                    eventData.pointerPressRaycast = result;

                    eventData.eligibleForClick = true;
                    eventData.delta = Vector2.zero;
                    eventData.dragging = false;
                    eventData.useDragThreshold = true;
                    eventData.clickCount = 1;
                    eventData.clickTime = Time.unscaledTime;
                }

                // Gửi sự kiện
                ExecuteEvents.Execute(result.gameObject, eventData, handler);
                return result.gameObject;
            }

            return null;
        }
    }
}
