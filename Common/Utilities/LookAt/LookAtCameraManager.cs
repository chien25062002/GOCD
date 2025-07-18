using System.Collections.Generic;
using UnityEngine;

namespace GOCD.Framework
{
    public class LookAtCameraManager : MonoBehaviour
    {
        static LookAtCameraManager _instance;
        static readonly List<LookAtCamera> _targets = new List<LookAtCamera>();
        static Transform _cam;

        static readonly List<int> _invalidIndexes = new(); // Đánh dấu các object đã bị destroy

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Init()
        {
            if (_instance != null) return;

            var go = new GameObject("[LookAtCameraManager]");
            _instance = go.AddComponent<LookAtCameraManager>();
            DontDestroyOnLoad(go);
        }

        void LateUpdate()
        {
            if (_cam == null)
            {
                if (Camera.main != null)
                    _cam = Camera.main.transform;
                else
                    return;
            }

            _invalidIndexes.Clear();

            for (int i = 0; i < _targets.Count; i++)
            {
                var target = _targets[i];

                // UnityEngine.Object != null vẫn trả true nếu object đã bị Destroy → cần try-catch
                if (target == null)
                {
                    _invalidIndexes.Add(i);
                    continue;
                }

                try
                {
                    target.LookAt(_cam);
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"[LookAtCameraManager] Exception when LookAt: {ex.Message}");
                    _invalidIndexes.Add(i);
                }
            }

            // Remove các object lỗi (phải từ cuối lên)
            for (int i = _invalidIndexes.Count - 1; i >= 0; i--)
            {
                int index = _invalidIndexes[i];
                if (index >= 0 && index < _targets.Count)
                {
                    _targets.RemoveAt(index);
                }
            }
        }

        public static void Register(LookAtCamera obj)
        {
            if (obj != null && !_targets.Contains(obj))
                _targets.Add(obj);
        }

        public static void Unregister(LookAtCamera obj)
        {
            _targets.Remove(obj);
        }
    }
}
