using UnityEngine;

namespace GOCD.Framework
{
    /// <summary>
    /// Đặt Transform này làm ứng viên root cho PopupManager.
    /// Khi PopupManager cần root mà chưa có, nó sẽ chọn setter có Priority lớn nhất (phải active & enabled).
    /// </summary>
    public class PopupRootSetter : MonoBehaviour
    {
        [SerializeField] bool _setupOnEnable = true;
        [SerializeField] int _priority = 0;

        /// <summary>Độ ưu tiên chọn làm root khi chưa có root (càng lớn càng ưu tiên).</summary>
        public int Priority => _priority;

        void Awake()
        {
            // Đăng ký làm ứng viên root
            PopupManager.RegisterRootCandidate(this);

            if (_setupOnEnable == false)
            {
                // Nếu bạn muốn set ngay từ Awake
                PopupManager.SetRoot(transform);
            }

            PopupManager.eventRootUndefine += OnRootUndefine;
        }

        void OnEnable()
        {
            // Đăng ký lại nếu được re-enable
            PopupManager.RegisterRootCandidate(this);

            if (_setupOnEnable)
            {
                PopupManager.SetRoot(transform);
            }
        }

        void OnDisable()
        {
            // Hủy đăng ký khi bị disable (không được chọn khi inactive/disabled)
            PopupManager.UnregisterRootCandidate(this);
        }

        void OnDestroy()
        {
            PopupManager.eventRootUndefine -= OnRootUndefine;
            PopupManager.UnregisterRootCandidate(this);
        }

        void OnRootUndefine()
        {
            // Khi PopupManager phát hiện thiếu root, thử set transform này
            // (Manager vẫn sẽ ưu tiên theo priority nếu có nhiều candidates)
            PopupManager.SetRoot(transform);
        }
    }
}