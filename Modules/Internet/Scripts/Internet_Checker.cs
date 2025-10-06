using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace GOCD.Framework.Internet
{
    /// <summary>
    /// Internet checker có "cổng" điều kiện qua UnityEvent.
    /// - OnBeforeCheck: bắn ra trước mỗi lần check; listener có thể gọi DenyCheckThisTick() để chặn.
    /// - ClosePopupWhenDenied: nếu bị chặn, sẽ đóng popup (nếu đang mở).
    /// </summary>
    public class Internet_Checker : MonoSingleton<Internet_Checker>
    {
        protected override bool _dontDestroyOnLoad => true;

        [Header("Settings (ưu tiên)")]
        [SerializeField] InternetSettings _settings;

        [Header("Fallback (nếu thiếu Settings)")]
        [SerializeField] float checkInterval = 10f;

        [SerializeField] float _delayOnStart = 120f;

        [Header("Gate (UnityEvent)")]
        [Tooltip("Sự kiện bắn ra trước khi kiểm tra; listener có thể gọi Internet_Checker.DenyCheckThisTick() để chặn lần kiểm tra.")]
        [SerializeField] UnityEvent _onBeforeCheck;
        [Tooltip("Nếu bị chặn kiểm tra, có tự động đóng popup đang mở không?")]
        [SerializeField] bool _closePopupWhenDenied = true;
        [Tooltip("Sự kiện bắn ra khi lần kiểm tra bị chặn.")]
        [SerializeField] UnityEvent _onDeniedCheck;

        static Internet_Checker _instance;
        bool  _isRunning;
        bool  _isCheckingNow; // chặn re-entrancy
        float _lastCheckTime;

        // Cờ điều khiển bởi listener qua DenyCheckThisTick()
        static bool _allowThisTick = true;

        protected override void Awake()
        {
            base.Awake();
            _instance = this;

            // Nếu chưa gán, thử load trong Resources
            if (_settings == null)
                _settings = Resources.Load<InternetSettings>(InternetSettings.DefaultResourcesPath);

            // Đẩy settings cho InternetHelper (paramless vẫn chạy đúng)
            InternetHelper.CurrentSettings = _settings;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _isRunning = false;
            _isCheckingNow = false;
            if (_instance == this) _instance = null;
        }

        void Start()
        {
            _ = LoopCheck().AttachExternalCancellation(this.GetCancellationTokenOnDestroy());
        }

        async UniTask LoopCheck()
        {
            // Delay khởi động (unscaled để không phụ thuộc Time.timeScale)
            await UniTask.Delay((int)(_delayOnStart * 1000f), ignoreTimeScale: true, cancellationToken: destroyCancellationToken);

            _isRunning = true;

            while (_isRunning)
            {
                float interval = _settings ? _settings.checkInterval : checkInterval;
                if (interval < 1f) interval = 1f;

                if (Time.unscaledTime - _lastCheckTime >= interval && !_isCheckingNow)
                {
                    _lastCheckTime = Time.unscaledTime;
                    _isCheckingNow = true;

                    try
                    {
                        // 1) Reset quyền, bắn sự kiện cho phép listener quyết định.
                        _allowThisTick = true;                 // mặc định cho phép
                        _onBeforeCheck?.Invoke();              // listener có thể gọi DenyCheckThisTick()

                        if (!_allowThisTick)
                        {
                            // Bị chặn: đóng popup (option) và bắn event thông báo.
                            if (_closePopupWhenDenied)
                                InternetHelper.ClosePopupIfShowing();

                            _onDeniedCheck?.Invoke();
                        }
                        else
                        {
                            // 2) Cho phép: gọi bản Popup (không View). Popup tự mở/đóng theo trạng thái mạng.
                            await InternetHelper.CheckInternetWithPopup(_settings);
                        }
                    }
                    finally
                    {
                        _isCheckingNow = false;
                    }
                }

                await UniTask.Delay(1000, ignoreTimeScale: true);
            }
        }

        /// <summary>
        /// Từ listener của OnBeforeCheck, gọi hàm này để CHẶN lần kiểm tra hiện tại.
        /// </summary>
        public static void DenyCheckThisTick()
        {
            _allowThisTick = false;
        }

        /// <summary>
        /// Force check ngay (bỏ qua gate), dùng cho trường hợp bạn thật sự muốn kiểm tra ngay lập tức.
        /// </summary>
        public static void ForceCheckNow()
        {
            if (_instance == null) return;
            _ = _instance.HandleForceCheck()
                .AttachExternalCancellation(_instance.GetCancellationTokenOnDestroy());
        }

        async UniTask HandleForceCheck()
        {
            // Force check nhanh, KHÔNG mở popup (dùng WithoutPopup).
            await InternetHelper.CheckInternetWithoutPopup(_settings);
        }
    }
}
