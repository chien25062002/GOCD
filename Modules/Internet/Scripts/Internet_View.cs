using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace GOCD.Framework.Internet
{
    public class Internet_View : MonoBehaviour
    {
        [Header("UI References")] [SerializeField]
        GameObject _goError;

        [SerializeField] GameObject _goConnecting;

        [SerializeField] Internet_TryAgain_Button _btnTryAgain;
        [SerializeField] Transform _connectingTrans;

        [Header("Config")] [SerializeField] float _rotateSpeed = 180f;
        [SerializeField] float _minWaitTime = 3.5f; // thời gian tối thiểu "đang kết nối"

        bool _isRotating;
        View _view;

        View View
        {
            get
            {
                if (_view == null) _view = GetComponent<View>();
                return _view;
            }
        }

        void Awake()
        {
            _view = View;
        }

        void OnEnable()
        {
            // Đăng ký: có Internet UI đang mở
            InternetHelper.RegisterUI(this);
            // Mặc định hiện trạng thái lỗi + cho bấm lại
            if (_goError) _goError.SetActive(true);
            if (_goConnecting) _goConnecting.SetActive(false);
            if (_btnTryAgain) _btnTryAgain.SetInteractable(true);
            _isRotating = false;
        }

        void OnDisable()
        {
            // Huỷ đăng ký khi bị tắt
            InternetHelper.UnregisterUI(this);
        }

        void OnDestroy()
        {
            InternetHelper.UnregisterUI(this);
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
            if (_goError) _goError.SetActive(false);
            if (_goConnecting) _goConnecting.SetActive(true);
            _isRotating = true;
            if (_btnTryAgain) _btnTryAgain.SetInteractable(false);

            float startTime = Time.unscaledTime;

            // Chỉ kiểm tra mạng - KHÔNG mở/tạo thêm UI
            bool isOk = await InternetHelper.CheckInternetWithoutView();

            // Ép chờ đủ thời gian tối thiểu
            float elapsed = Time.unscaledTime - startTime;
            if (elapsed < _minWaitTime)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(_minWaitTime - elapsed), ignoreTimeScale: true);
            }

            _isRotating = false;
            if (_goConnecting) _goConnecting.SetActive(false);

            if (isOk)
            {
                // Có mạng -> đóng chính mình
                if (View != null) View.Close();
                else gameObject.SetActive(false);
            }
            else
            {
                if (_goError) _goError.SetActive(true);
                if (_btnTryAgain) _btnTryAgain.SetInteractable(true);
                // Vẫn còn UI này đang mở => registry giúp chặn tạo instance mới
            }
        }
    }
}