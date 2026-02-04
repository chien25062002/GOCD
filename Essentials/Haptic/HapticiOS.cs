#if UNITY_IOS
using UnityEngine;
using System.Runtime.InteropServices;

namespace CodeSketch.Haptic
{
    public static class HapticIos
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

        [DllImport("__Internal")]
        static extern bool _HasVibrator();

        [DllImport("__Internal")]
        static extern void _Vibrate();

        [DllImport("__Internal")]
        static extern void _VibratePop();

        [DllImport("__Internal")]
        static extern void _VibratePeek();

        [DllImport("__Internal")]
        static extern void _VibrateNope();

        [DllImport("__Internal")]
        static extern void _impactOccurred(string style);

        [DllImport("__Internal")]
        static extern void _notificationOccurred(string style);

        [DllImport("__Internal")]
        static extern void _selectionChanged();

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

        ///<summary>
        /// Tiny pop vibration
        ///</summary>
        public static void VibratePop()
        {
            _VibratePop();
        }

        ///<summary>
        /// Small peek vibration
        ///</summary>
        public static void VibratePeek()
        {
            _VibratePeek();
        }

        ///<summary>
        /// 3 small vibrations
        ///</summary>
        public static void VibrateNope()
        {
            _VibrateNope();
        }

        public static bool HasVibrator()
        {
            return _HasVibrator();
        }

        public static void Vibrate(HapticType type)
        {
            switch (type)
            {
                case HapticType.Default:
                    Handheld.Vibrate();
                    break;

                case HapticType.ImpactLight:
                    Vibrate(ImpactFeedbackStyle.Light);
                    break;

                case HapticType.ImpactMedium:
                    Vibrate(ImpactFeedbackStyle.Medium);
                    break;

                case HapticType.ImpactHeavy:
                    Vibrate(ImpactFeedbackStyle.Heavy);
                    break;

                case HapticType.Success:
                    Vibrate(NotificationFeedbackStyle.Success);
                    break;

                case HapticType.Failure:
                    Vibrate(NotificationFeedbackStyle.Error);
                    break;

                case HapticType.Warning:
                    Vibrate(NotificationFeedbackStyle.Warning);
                    break;

                default:
                    throw new System.ArgumentException($"Undefined vibrate type: {type}");
            }
        }
    }
}
#endif