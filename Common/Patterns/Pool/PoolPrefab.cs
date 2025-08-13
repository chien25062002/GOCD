// PoolPrefab.cs (bản rút gọn, chỉ sửa OnRelease + thêm PoolRoot)
using System;
using UnityEngine;
using UnityEngine.Pool;

namespace GOCD.Framework
{
    public sealed class PoolPrefab
    {
        readonly GameObject _prefab;
        readonly bool _dontDestroyOnLoad;
        readonly ObjectPool<GameObject> _pool;

        // ===== NEW: PoolRoot toàn cục để giữ inactive items an toàn qua Destroy parent/scene =====
        static Transform s_PoolRoot;
        static Transform PoolRoot
        {
            get {
                if (s_PoolRoot == null) {
                    var go = new GameObject("__PoolRoot");
                    UnityEngine.Object.DontDestroyOnLoad(go);
                    s_PoolRoot = go.transform;
                }
                return s_PoolRoot;
            }
        }

        const int DefaultCapacity = 16;
        const int MaxSize = 4096;

        public PoolPrefabConfig Config { get; }

        public PoolPrefab(PoolPrefabConfig config)
        {
            if (config == null || !config.prefab)
                throw new InvalidOperationException("PoolPrefab: Config hoặc prefab null!");

            Config = config;
            _prefab = config.prefab;
            _dontDestroyOnLoad = config.dontDestroyOnLoad;

            _pool = new ObjectPool<GameObject>(
                createFunc: Create,
                actionOnGet: OnGet,
                actionOnRelease: OnRelease,
                actionOnDestroy: OnDestroy,
                collectionCheck: false,
                defaultCapacity: DefaultCapacity,
                maxSize: MaxSize
            );
        }

        public PoolPrefab(GameObject prefab)
        {
            if (!prefab) throw new InvalidOperationException("PoolPrefab: prefab null!");
            _prefab = prefab;
            _dontDestroyOnLoad = false;
            Config = null;

            _pool = new ObjectPool<GameObject>(
                createFunc: Create,
                actionOnGet: OnGet,
                actionOnRelease: OnRelease,
                actionOnDestroy: OnDestroy,
                collectionCheck: false,
                defaultCapacity: DefaultCapacity,
                maxSize: MaxSize
            );
        }

        GameObject Create()
        {
            var go = UnityEngine.Object.Instantiate(_prefab, PoolRoot, false);
            if (_dontDestroyOnLoad) UnityEngine.Object.DontDestroyOnLoad(go);
            go.SetActive(false);
            // Quan trọng: ngay từ đầu parent về PoolRoot để an toàn
            return go;
        }

        void OnGet(GameObject go)
        {
            if (!go) return;
            // Khi lấy ra xài, có thể bỏ parent (tuỳ bạn) — thường để null
            go.transform.SetParent(null, false);
            go.SetActive(true);
        }

        void OnRelease(GameObject go)
        {
            if (!go) return;

            // Gọi IPoolPreRelease trước
            var handlers = go.GetComponentsInChildren<IPoolPreRelease>(true);
            for (int i = 0; i < handlers.Length; i++)
            {
                try { handlers[i]?.OnBeforeReleaseToPool(); }
                catch (Exception ex) { Debug.LogException(ex); }
            }

            // Đưa về inactive + parent về PoolRoot để không bị Destroy theo _airplane
            go.SetActive(false);
            go.transform.SetParent(PoolRoot, false);
        }

        void OnDestroy(GameObject go)
        {
            if (!go) return;
#if UNITY_EDITOR
            UnityEngine.Object.DestroyImmediate(go);
#else
            UnityEngine.Object.Destroy(go);
#endif
        }

        public GameObject Get()
        {
            // Lọc "xác chết" nếu có
            for (int i = 0; i < 8; i++)
            {
                var go = _pool.Get();
                if (go) return go;
            }
            var fallback = UnityEngine.Object.Instantiate(_prefab, null, false);
            if (_dontDestroyOnLoad) UnityEngine.Object.DontDestroyOnLoad(fallback);
            fallback.SetActive(true);
            return fallback;
        }

        public void Release(GameObject instance)
        {
            if (!instance) return;
            _pool.Release(instance);
        }

        public void Clear() => _pool.Clear();
    }
}
