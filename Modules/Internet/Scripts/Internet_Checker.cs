using Cysharp.Threading.Tasks;
using UnityEngine;

namespace GOCD.Framework.Internet
{
    public class Internet_Checker : MonoSingleton<Internet_Checker>
    {
        protected override bool _dontDestroyOnLoad => true;

        [Header("Config")]
        [SerializeField] float checkInterval = 10f;

        static Internet_Checker _instance;

        bool _isRunning;
        float _lastCheckTime;

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _isRunning = false;
        }

        void Start()
        {
            _ = LoopCheck().AttachExternalCancellation(this.GetCancellationTokenOnDestroy());
        }

        async UniTask LoopCheck()
        {
            _isRunning = true;

            while (_isRunning)
            {
                if (Time.unscaledTime - _lastCheckTime >= checkInterval)
                {
                    _lastCheckTime = Time.unscaledTime;

                    // ✅ Gọi đúng hàm async có thể await và có thể mở view nếu mất mạng
                    await InternetHelper.CheckInternetWithView();
                }

                await UniTask.Delay(1000);
            }
        }

        bool IsInternetPopupVisible()
        {
            return InternetHelper.InternetView &&
                   InternetHelper.InternetView.GameObjectCached.activeInHierarchy;
        }

        public static void ForceCheckNow()
        {
            if (_instance == null) return;

            _ = _instance.HandleForceCheck()
                .AttachExternalCancellation(_instance.GetCancellationTokenOnDestroy());
        }

        async UniTask HandleForceCheck()
        {
            await InternetHelper.CheckInternetWithoutView();
        }
    }
}