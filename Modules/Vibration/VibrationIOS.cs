#if UNITY_IOS
using UnityEngine;
using System.Runtime.InteropServices;
using GOCD.Framework.Diagnostics;

namespace GOCD.Framework.Vibration
{
    /// <summary>
    /// Điều khiển rung Haptic cho thiết bị iOS
    /// </summary>
    public static class VibrationIOS
    {
        public enum ImpactFeedbackStyle
        {
            Heavy,
            Medium,
            Light,
            Rigid,
            Soft
        }

        public enum NotificationFeedbackStyle
        {
            Error,
            Success,
            Warning
        }

        [DllImport("__Internal")] private static extern bool _HasVibrator();
        [DllImport("__Internal")] private static extern void _Vibrate();
        [DllImport("__Internal")] private static extern void _VibratePop();
        [DllImport("__Internal")] private static extern void _VibratePeek();
        [DllImport("__Internal")] private static extern void _VibrateNope();
        [DllImport("__Internal")] private static extern void _impactOccurred(string style);
        [DllImport("__Internal")] private static extern void _notificationOccurred(string style);
        [DllImport("__Internal")] private static extern void _selectionChanged();

        public static void Vibrate(ImpactFeedbackStyle style)
        {
            _impactOccurred(style.ToString());
        }

        public static void Vibrate(NotificationFeedbackStyle style)
        {
            _notificationOccurred(style.ToString());
        }

        public static void Vibrate_SelectionChanged()
        {
            _selectionChanged();
        }

        /// <summary> Rung kiểu pop nhẹ </summary>
        public static void VibratePop() => _VibratePop();

        /// <summary> Rung kiểu peek </summary>
        public static void VibratePeek() => _VibratePeek();

        /// <summary> Rung kiểu nope (3 lần nhẹ) </summary>
        public static void VibrateNope() => _VibrateNope();

        public static bool HasVibrator()
        {
            return _HasVibrator();
        }

        /// <summary>
        /// Gọi rung theo kiểu được cấu hình sẵn.
        /// </summary>
        public static void Vibrate(VibrationType type)
        {
            switch (type)
            {
                case VibrationType.Default:
                    Handheld.Vibrate(); break;

                case VibrationType.ImpactLight:
                    Vibrate(ImpactFeedbackStyle.Light); break;

                case VibrationType.ImpactMedium:
                    Vibrate(ImpactFeedbackStyle.Medium); break;

                case VibrationType.ImpactHeavy:
                    Vibrate(ImpactFeedbackStyle.Heavy); break;

                case VibrationType.ImpactSoft:
                    Vibrate(ImpactFeedbackStyle.Soft); break;

                case VibrationType.ImpactRigid:
                    Vibrate(ImpactFeedbackStyle.Rigid); break;

                case VibrationType.Success:
                    Vibrate(NotificationFeedbackStyle.Success); break;

                case VibrationType.Failure:
                    Vibrate(NotificationFeedbackStyle.Error); break;

                case VibrationType.Warning:
                    Vibrate(NotificationFeedbackStyle.Warning); break;

                default:
                    GOCDDebug.Log(typeof(VibrationIOS), $"Không xác định kiểu rung: {type}");
                    break;
            }
        }
    }
}
#endif
