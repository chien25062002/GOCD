using UnityEngine;

namespace CodeSketch.Haptic
{
    public static class Haptic
    {
        public static bool Enabled = true;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void Init()
        {
#if UNITY_ANDROID
            HapticAndroid.Init();
#endif
        }

        public static void Vibrate(HapticType type)
        {
            if (!Enabled) return;
            
#if UNITY_IOS && !UNITY_EDITOR
            HapticIos.Vibrate(type);
#elif UNITY_ANDROID
            HapticAndroid.Vibrate(type);
#endif
        }

        public static bool HasVibrator()
        {
#if UNITY_IOS && !UNITY_EDITOR
            return HapticIos.HasVibrator();
#elif UNITY_ANDROID
            return HapticAndroid.HasVibrator();
#else
            return false;
#endif
        }
    }


}