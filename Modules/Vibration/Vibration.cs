namespace GOCD.Framework.Vibration
{
    /// <summary>
    /// Lớp quản lý rung chính cho toàn bộ hệ thống (Android và iOS).
    /// </summary>
    public static class Vibration
    {
        /// <summary>
        /// Bật hoặc tắt toàn bộ tính năng rung.
        /// </summary>
        public static bool Enabled = true;

        /// <summary>
        /// Khởi tạo hệ thống rung tuỳ theo nền tảng.
        /// </summary>
        public static void Init()
        {
#if UNITY_ANDROID
            VibrationAndroid.Init();
#endif
        }

        /// <summary>
        /// Gọi rung với kiểu rung mong muốn.
        /// </summary>
        public static void Vibrate(VibrationType type)
        {
            if (!Enabled) return;

#if UNITY_IOS && !UNITY_EDITOR
            VibrationIOS.Vibrate(type);
#elif UNITY_ANDROID
            VibrationAndroid.Vibrate(type);
#endif
        }

        /// <summary>
        /// Kiểm tra thiết bị có hỗ trợ rung hay không.
        /// </summary>
        public static bool HasVibrator()
        {
#if UNITY_IOS && !UNITY_EDITOR
            return VibrationIOS.HasVibrator();
#elif UNITY_ANDROID
            return VibrationAndroid.HasVibrator();
#else
            return false;
#endif
        }
    }
}