using UnityEngine;

namespace GOCD.Framework.Optimize
{
    /// <summary>
    /// ⚡ Theo dõi FPS mượt và ổn định (không alloc, cập nhật mỗi frame)
    /// Dùng cho logic adaptive (ví dụ giảm effect, skip coin drop khi FPS thấp)
    /// </summary>
    public static class PerformanceHelper
    {
        const float SMOOTH_FACTOR = 0.05f; // 0.0f = raw, 1.0f = siêu mượt
        static float _smoothedDeltaTime = 0.016f; // ~60fps mặc định

        /// <summary>
        /// FPS trung bình gần nhất (mượt)
        /// </summary>
        public static float CurrentFPS => 1f / Mathf.Max(_smoothedDeltaTime, 0.0001f);

        /// <summary>
        /// DeltaTime trung bình (mượt)
        /// </summary>
        public static float SmoothedDeltaTime => _smoothedDeltaTime;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Init()
        {
            // Tạo 1 updater ẩn trong scene
            var go = new GameObject("[PerformanceHelper]");
            Object.DontDestroyOnLoad(go);
            go.hideFlags = HideFlags.HideAndDontSave;
            go.AddComponent<PerformanceHelperUpdater>();
        }

        // MonoBehaviour riêng để tránh GC trong lambda
        sealed class PerformanceHelperUpdater : MonoBehaviour
        {
            void Update()
            {
                _smoothedDeltaTime = Mathf.Lerp(_smoothedDeltaTime, Time.unscaledDeltaTime, SMOOTH_FACTOR);
            }
        }
    }
}