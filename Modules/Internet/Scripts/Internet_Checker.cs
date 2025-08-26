using Cysharp.Threading.Tasks;
using UnityEngine;

namespace GOCD.Framework.Internet 
{
    public class Internet_Checker : MonoSingleton<Internet_Checker> {
        protected override bool _dontDestroyOnLoad => true;

        [Header("Settings (ưu tiên)")]
        [SerializeField] InternetSettings _settings;

        [Header("Fallback (nếu thiếu Settings)")]
        [SerializeField] float checkInterval = 10f;

        static Internet_Checker _instance;
        bool _isRunning;
        float _lastCheckTime;

        protected override void Awake()
        {
            base.Awake();
            _instance = this;

            // Nếu chưa gán, thử load trong Resources
            if (_settings == null) {
                _settings = Resources.Load<InternetSettings>(InternetSettings.DefaultResourcesPath);
            }
            // Đẩy settings cho InternetHelper để popup cũ (paramless) vẫn chạy đúng
            InternetHelper.CurrentSettings = _settings;
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            _isRunning = false;
            if (_instance == this) _instance = null;
        }

        void Start() {
            _ = LoopCheck().AttachExternalCancellation(this.GetCancellationTokenOnDestroy());
        }

        async UniTask LoopCheck() {
            _isRunning = true;

            while (_isRunning) {
                float interval = _settings ? _settings.checkInterval : checkInterval;

                if (Time.unscaledTime - _lastCheckTime >= interval) {
                    _lastCheckTime = Time.unscaledTime;

                    // Gọi bản có Settings (ổn định), popup sẽ tự mở/đóng
                    await InternetHelper.CheckInternetWithView(_settings);
                }

                await UniTask.Delay(1000, ignoreTimeScale: true);
            }
        }

        public static void ForceCheckNow() {
            if (_instance == null) return;
            _ = _instance.HandleForceCheck()
                .AttachExternalCancellation(_instance.GetCancellationTokenOnDestroy());
        }

        async UniTask HandleForceCheck() {
            // Cho TryAgain bên trong popup vẫn hoạt động vì nó gọi bản paramless,
            // nhưng ở đây mình chủ động truyền settings để thống nhất.
            await InternetHelper.CheckInternetWithoutView(_settings);
        }
    }
}
