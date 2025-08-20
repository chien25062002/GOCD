// PoolPrefab.cs
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;

namespace GOCD.Framework
{
    public sealed class PoolPrefab
    {
        const int DefaultCapacity = 16;
        const int MaxSize = 4096;
        
        readonly GameObject _prefab;
        readonly bool _dontDestroyOnLoad;
        readonly ObjectPool<GameObject> _pool;

        CancelToken _token;
        
        int _spawnAtStart;

        // ===== PoolRoot để giữ inactive items an toàn =====
        static Transform s_PoolRoot;
        static Transform PoolRoot {
            get {
                if (s_PoolRoot == null) {
                    var go = new GameObject("__PoolRoot");
                    UnityEngine.Object.DontDestroyOnLoad(go);
                    s_PoolRoot = go.transform;
                }
                return s_PoolRoot;
            }
        }

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
                defaultCapacity: config.spawnCapacity,
                maxSize: config.spawnCapacityMax
            );

            _spawnAtStart = config.spawnAtStart;
            
            EnsurePoolCount();
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

        public async UniTask EnsurePoolCount(int maxPerFrame = 10)
        {
            _token?.Cancel();    // huỷ vòng cũ
            _token = new CancelToken();
            var ct = _token.Token;   // lấy CancellationToken của bạn
    
            // init number of spawn at start in here
            int needToSpawn = Mathf.Max(0, _spawnAtStart - _pool.CountAll);
            int c = 0;

            try
            {
                for (int i = 0; i < needToSpawn; i++)
                {
                    ct.ThrowIfCancellationRequested();

                    var go = _pool.Get();   // _warming flag như đã bàn
                    _pool.Release(go);

                    c++;
                    if (c >= maxPerFrame)
                    {
                        c = 0;
                        await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate, ct);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // silent cancel
            }
        }
        
        GameObject Create()
        {
            var go = UnityEngine.Object.Instantiate(_prefab, PoolRoot, false);
            if (_dontDestroyOnLoad) UnityEngine.Object.DontDestroyOnLoad(go);
            go.SetActive(false);
            return go;
        }

        void OnGet(GameObject go)
        {
            if (!go) return;
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
            // Lọc “xác chết” nếu có
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
