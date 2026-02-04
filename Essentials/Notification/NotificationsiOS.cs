using System;
using System.Collections;
using UnityEngine;

#if CODESKETCH_NOTIFICATIONS && (UNITY_IOS || UNITY_IPHONE)
using Unity.Notifications.iOS;
#endif

namespace Game
{
    public class NotificationsiOS : MonoBehaviour
    {
        // =====================================================
        // PUBLIC API (ALWAYS VALID)
        // =====================================================

        public IEnumerator RequestAuthorization()
        {
#if CODESKETCH_NOTIFICATIONS && (UNITY_IOS || UNITY_IPHONE)
            using var request = new AuthorizationRequest(
                AuthorizationOption.Alert | AuthorizationOption.Badge,
                true
            );

            while (!request.IsFinished)
                yield return null;
#else
            // noop – vẫn là coroutine hợp lệ
            yield break;
#endif
        }

        public void Send(
            string identifier,
            string title,
            string body,
            string subtitle,
            int delaySeconds
        )
        {
#if CODESKETCH_NOTIFICATIONS && (UNITY_IOS || UNITY_IPHONE)
            var trigger = new iOSNotificationTimeIntervalTrigger
            {
                TimeInterval = TimeSpan.FromSeconds(delaySeconds),
                Repeats = false
            };

            var notification = new iOSNotification
            {
                Identifier = identifier,
                Title = title,
                Body = body,
                Subtitle = subtitle,
                ShowInForeground = true,
                ForegroundPresentationOption =
                    PresentationOption.Alert | PresentationOption.Badge,
                Trigger = trigger
            };

            iOSNotificationCenter.ScheduleNotification(notification);
#endif
            // noop nếu không bật CODESKETCH_NOTIFICATIONS
        }

        public void Cancel(string identifier)
        {
#if CODESKETCH_NOTIFICATIONS && (UNITY_IOS || UNITY_IPHONE)
            iOSNotificationCenter.RemoveScheduledNotification(identifier);
#endif
            // noop nếu không bật define
        }
    }
}
