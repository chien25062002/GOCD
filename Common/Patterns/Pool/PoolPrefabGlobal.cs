using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GOCD.Framework
{
    /// <summary>
    /// Global prefab pool manager.
    /// - Track instance đã vào pool bằng HashSet (_releasedInstances).
    /// - Nếu đã Release rồi thì các lần Release sau sẽ skip (không double-release).
    /// - Khi Get() thì remove khỏi HashSet (đánh dấu active).
    /// - Instance bẩn -> enqueue vào PoolReleaseScheduler; chỉ khi thực sự Release về pool mới coi là released.
    /// </summary>
    public static class PoolPrefabGlobal
    {
        static readonly Dictionary<GameObject, PoolPrefab> _poolLookup = new();
        static readonly HashSet<GameObject> _releasedInstances = new();

        [RuntimeInitializeOnLoadMethod]
        static void Init()
        {
            MonoCallback.Instance.EventActiveSceneChanged += MonoCallback_EventActiveSceneChanged;
        }

        static void MonoCallback_EventActiveSceneChanged(Scene sceneCurrent, Scene sceneNext)
        {
            // Dọn scheduler queue trước khi clear pool theo scene
            PoolReleaseScheduler.ClearQueue();

            // Clear released cache (tránh giữ tham chiếu cross-scene)
            _releasedInstances.Clear();

            foreach (PoolPrefab pool in _poolLookup.Values)
            {
                if (pool.Config != null && pool.Config.dontDestroyOnLoad)
                    continue;
                pool.Clear();
            }
        }

        /// <summary>
        /// Clear tất cả pool (trừ những pool có dontDestroyOnLoad)
        /// + xoá hàng đợi scheduler + cache released.
        /// </summary>
        public static void Clean()
        {
            PoolReleaseScheduler.ClearQueue();
            _releasedInstances.Clear();

            foreach (PoolPrefab pool in _poolLookup.Values)
            {
                if (pool.Config != null && pool.Config.dontDestroyOnLoad)
                    continue;
                pool.Clear();
            }
        }

        public static void Construct(params PoolPrefabConfig[] configs)
        {
            for (int i = 0; i < configs.Length; i++)
            {
                if (!_poolLookup.ContainsKey(configs[i].prefab))
                    _poolLookup.Add(configs[i].prefab, new PoolPrefab(configs[i]));
            }
        }

        // =========================
        //            GET
        // =========================

        public static GameObject Get(PoolPrefabConfig config)
        {
            var go = GetPool(config).Get();
            _releasedInstances.Remove(go); // đánh dấu active
            return go;
        }

        public static GameObject Get(GameObject prefab)
        {
            var go = GetPool(prefab).Get();
            _releasedInstances.Remove(go); // đánh dấu active
            return go;
        }

        // =========================
        //          RELEASE
        // =========================

        public static void Release(PoolPrefabConfig config, GameObject instance)
        {
            if (config == null || instance == null) return;

            // Nếu đã release rồi thì thôi (skip)
            if (_releasedInstances.Contains(instance)) return;

            if (!TryScheduleIfDirty(instance, config.prefab))
            {
                // Trả về pool ngay -> đánh dấu đã released
                _releasedInstances.Add(instance);
                GetPool(config).Release(instance);
            }
            // Nếu enqueue vào scheduler thì chưa coi là released;
            // khi scheduler xử lý xong sẽ gọi lại Release(...) và thêm vào set lúc đó.
        }

        public static void Release(GameObject prefab, GameObject instance)
        {
            if (prefab == null || instance == null) return;

            // Nếu đã release rồi thì thôi (skip)
            if (_releasedInstances.Contains(instance)) return;

            if (!TryScheduleIfDirty(instance, prefab))
            {
                _releasedInstances.Add(instance);
                GetPool(prefab).Release(instance);
            }
        }

        /// <summary>
        /// Kiểm tra các IPoolPreRelease có "bẩn" không; nếu bẩn thì enqueue để xử lý trước khi release thật.
        /// </summary>
        static bool TryScheduleIfDirty(GameObject instance, GameObject prefab)
        {
            var handlers = instance.GetComponentsInChildren<IPoolPreRelease>(true);
            bool needs = false;
            for (int i = 0; i < handlers.Length; i++)
            {
                var h = handlers[i];
                if (h != null && h.IsDirty) { needs = true; break; }
            }

            if (!needs) return false;

            PoolReleaseScheduler.Enqueue(prefab, instance);
            return true;
        }

        // =========================
        //        GET POOL
        // =========================

        public static PoolPrefab GetPool(PoolPrefabConfig config)
        {
            if (!_poolLookup.ContainsKey(config.prefab))
                _poolLookup.Add(config.prefab, new PoolPrefab(config));
            return _poolLookup[config.prefab];
        }

        public static PoolPrefab GetPool(GameObject prefab)
        {
            if (!_poolLookup.ContainsKey(prefab))
                _poolLookup.Add(prefab, new PoolPrefab(prefab));
            return _poolLookup[prefab];
        }

        // =========================
        //        CHECK API
        // =========================

        /// <summary>
        /// True nếu instance đã được trả về pool (đã Release và chưa Get lại).
        /// </summary>
        public static bool IsReleased(GameObject instance)
        {
            return instance && _releasedInstances.Contains(instance);
        }
    }
}
