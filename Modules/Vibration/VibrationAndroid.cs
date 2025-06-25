#if UNITY_ANDROID

using GOCD.Framework.Diagnostics;
using UnityEngine;

namespace GOCD.Framework.Vibration
{
    /// <summary>
    /// Hệ thống rung dành cho Android. Tự động khởi tạo trước khi vào scene đầu tiên.
    /// </summary>
    public static class VibrationAndroid
    {
        // Các giá trị thời gian và biên độ mẫu
        static readonly long _lightDuration = 20;
        public static readonly long _mediumDuration = 40;
        public static readonly long _heavyDuration = 80;

        public static readonly int _lightAmplitude = 40;
        public static readonly int _mediumAmplitude = 120;
        public static readonly int _heavyAmplitude = 255;

        // Các mẫu rung nâng cao
        static readonly long[] _successPattern = { 0, _lightDuration, _lightDuration, _heavyDuration };
        static readonly int[] _successPatternAmplitude = { 0, _lightAmplitude, 0, _heavyAmplitude };

        static readonly long[] _warningPattern = { 0, _heavyDuration, _lightDuration, _mediumDuration };
        static readonly int[] _warningPatternAmplitude = { 0, _heavyAmplitude, 0, _mediumAmplitude };

        static readonly long[] _failurePattern = { 0, _mediumDuration, _lightDuration, _mediumDuration, _lightDuration, _heavyDuration, _lightDuration, _lightDuration };
        static readonly int[] _failurePatternAmplitude = { 0, _mediumAmplitude, 0, _mediumAmplitude, 0, _heavyAmplitude, 0, _lightAmplitude };

        static bool _isInitialized = false;

        static AndroidJavaObject _vibrator = null;
        static AndroidJavaClass _vibrationEffectClass = null;
        static int _defaultAmplitude = 255;

        static int _apiLevel = 1;
        static bool _isSupportVibrationEffect => _apiLevel >= 26;

        #region Initialization

        public static void Init()
        {
            if (_isInitialized || Application.platform != RuntimePlatform.Android)
                return;

            // Lấy phiên bản Android API
            using (var androidVersionClass = new AndroidJavaClass("android.os.Build$VERSION"))
            {
                _apiLevel = androidVersionClass.GetStatic<int>("SDK_INT");
            }

            // Lấy Activity và Vibrator
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                if (currentActivity != null)
                {
                    _vibrator = currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");

                    if (_isSupportVibrationEffect)
                    {
                        _vibrationEffectClass = new AndroidJavaClass("android.os.VibrationEffect");
                        _defaultAmplitude = Mathf.Clamp(_vibrationEffectClass.GetStatic<int>("DEFAULT_AMPLITUDE"), 1, 255);
                    }
                }
            }

            GOCDDebug.Log(typeof(VibrationAndroid), $"Đã khởi tạo\nCó rung = {HasVibrator()}\nHỗ trợ Amplitude = {HasAmplitudeControl()}\nBiên độ mặc định = {_defaultAmplitude}");
            _isInitialized = true;
        }

        #endregion

        #region Public API

        public static void Vibrate(VibrationType type)
        {
            switch (type)
            {
                case VibrationType.Default:
                    Handheld.Vibrate();
                    break;

                case VibrationType.ImpactLight:
                    Vibrate(_lightDuration, _lightAmplitude);
                    break;

                case VibrationType.ImpactMedium:
                    Vibrate(_mediumDuration, _mediumAmplitude);
                    break;

                case VibrationType.ImpactHeavy:
                    Vibrate(_heavyDuration, _heavyAmplitude);
                    break;

                case VibrationType.ImpactSoft:
                    Vibrate(_lightDuration, 20); // Biên độ thấp
                    break;

                case VibrationType.ImpactRigid:
                    Vibrate(_mediumDuration, 200); // Cứng, sắc nét
                    break;

                case VibrationType.Success:
                    Vibrate(_successPattern, _successPatternAmplitude);
                    break;

                case VibrationType.Failure:
                    Vibrate(_failurePattern, _failurePatternAmplitude);
                    break;

                case VibrationType.Warning:
                    Vibrate(_warningPattern, _warningPatternAmplitude);
                    break;

                default:
                    GOCDDebug.Log(typeof(VibrationAndroid), $"Không xác định kiểu rung: {type}");
                    break;
            }
        }

        /// <summary>
        /// Rung đơn (1 lần) với thời gian và biên độ cụ thể.
        /// </summary>
        public static void Vibrate(long milliseconds, int amplitude = 0, bool cancel = false)
        {
            Init();
            if (!HasVibrator()) return;
            if (cancel) Cancel();

            if (_isSupportVibrationEffect)
            {
                amplitude = Mathf.Clamp(amplitude, -1, 255);
                if (amplitude <= -1 || !HasAmplitudeControl()) amplitude = 255;
                if (amplitude == 0) amplitude = _defaultAmplitude;

                VibrateEffect(milliseconds, amplitude);
            }
            else
            {
                VibrateLegacy(milliseconds);
            }
        }

        /// <summary>
        /// Rung theo chuỗi pattern + amplitudes tương ứng (Off-On-Off...).
        /// </summary>
        public static void Vibrate(long[] pattern, int[] amplitudes = null, int repeat = -1, bool cancel = false)
        {
            Init();
            if (!HasVibrator()) return;
            if (!HasAmplitudeControl()) amplitudes = null;

            if (amplitudes != null && amplitudes.Length != pattern.Length)
            {
                GOCDDebug.LogWarning(typeof(VibrationAndroid), "Độ dài amplitudes không khớp pattern. Sẽ bị bỏ qua.");
                amplitudes = null;
            }

            ClampAmplitudesArray(amplitudes);
            if (cancel) Cancel();

            if (_isSupportVibrationEffect)
            {
                if (amplitudes != null)
                    VibrateEffect(pattern, amplitudes, repeat);
                else
                    VibrateEffect(pattern, repeat);
            }
            else
            {
                VibrateLegacy(pattern, repeat);
            }
        }

        public static bool HasVibrator()
        {
            return _vibrator != null && _vibrator.Call<bool>("hasVibrator");
        }

        public static bool HasAmplitudeControl()
        {
            return HasVibrator() && _isSupportVibrationEffect && _vibrator.Call<bool>("hasAmplitudeControl");
        }

        public static void Cancel()
        {
            if (HasVibrator())
                _vibrator.Call("cancel");
        }

        #endregion

        #region Internal Native Wrappers

        static void VibrateEffect(long milliseconds, int amplitude)
        {
            using (var effect = CreateEffect_OneShot(milliseconds, amplitude))
            {
                _vibrator.Call("vibrate", effect);
            }
        }

        static void VibrateLegacy(long milliseconds)
        {
            _vibrator.Call("vibrate", milliseconds);
        }

        static void VibrateEffect(long[] pattern, int repeat)
        {
            using (var effect = CreateEffect_Waveform(pattern, repeat))
            {
                _vibrator.Call("vibrate", effect);
            }
        }

        static void VibrateLegacy(long[] pattern, int repeat)
        {
            _vibrator.Call("vibrate", pattern, repeat);
        }

        static void VibrateEffect(long[] pattern, int[] amplitudes, int repeat)
        {
            using (var effect = CreateEffect_Waveform(pattern, amplitudes, repeat))
            {
                _vibrator.Call("vibrate", effect);
            }
        }

        static AndroidJavaObject CreateEffect_OneShot(long milliseconds, int amplitude)
        {
            return _vibrationEffectClass.CallStatic<AndroidJavaObject>("createOneShot", milliseconds, amplitude);
        }

        static AndroidJavaObject CreateEffect_Predefined(int effectId)
        {
            return _vibrationEffectClass.CallStatic<AndroidJavaObject>("createPredefined", effectId);
        }

        static AndroidJavaObject CreateEffect_Waveform(long[] timings, int[] amplitudes, int repeat)
        {
            return _vibrationEffectClass.CallStatic<AndroidJavaObject>("createWaveform", timings, amplitudes, repeat);
        }

        static AndroidJavaObject CreateEffect_Waveform(long[] timings, int repeat)
        {
            return _vibrationEffectClass.CallStatic<AndroidJavaObject>("createWaveform", timings, repeat);
        }

        static void ClampAmplitudesArray(int[] amplitudes)
        {
            if (amplitudes == null) return;
            for (int i = 0; i < amplitudes.Length; i++)
                amplitudes[i] = Mathf.Clamp(amplitudes[i], 1, 255);
        }

        #endregion
    }
}

#endif
