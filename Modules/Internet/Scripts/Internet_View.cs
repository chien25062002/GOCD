using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace GOCD.Framework.Internet
{
    public class Internet_View : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] GameObject _goError;
        [SerializeField] GameObject _goConnecting;

        [SerializeField] Internet_TryAgain_Button _btnTryAgain;
        [SerializeField] Transform _connectingTrans;

        [Header("Config")]
        [SerializeField] float _rotateSpeed = 180f;
        [SerializeField] float _minWaitTime = 3.5f; // thời gian tối thiểu "đang kết nối"

        bool _isRotating;
        View _view;

        View View
        {
            get
            {
                if (_view == null)
                    _view = GetComponent<View>();
                return _view;
            }
        }

        void Awake()
        {
            _view = View;
        }

        void Update()
        {
            if (_isRotating && _connectingTrans != null)
            {
                _connectingTrans.Rotate(Vector3.forward, -_rotateSpeed * Time.unscaledDeltaTime);
            }
        }

        public void TryConnect()
        {
            _ = HandleConnecting().AttachExternalCancellation(this.GetCancellationTokenOnDestroy());
        }

        async UniTask HandleConnecting()
        {
            _goError.SetActive(false);
            _goConnecting.SetActive(true);
            _isRotating = true;
            _btnTryAgain.SetInteractable(false);

            float startTime = Time.unscaledTime;

            // Thử kết nối
            bool isOk = await InternetHelper.CheckInternetWithoutView();

            // Ép chờ đủ thời gian tối thiểu
            float elapsed = Time.unscaledTime - startTime;
            if (elapsed < _minWaitTime)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(_minWaitTime - elapsed), ignoreTimeScale: true);
            }

            _isRotating = false;
            _goConnecting.SetActive(false);

            if (isOk)
            {
                View.Close(); // hoặc gameObject.SetActive(false) tuỳ hệ thống View
            }
            else
            {
                _goError.SetActive(true);
                _btnTryAgain.SetInteractable(true);
            }
        }
    }
}
