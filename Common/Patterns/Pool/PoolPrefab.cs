// PoolPrefab.cs
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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

        // Cache PoolPrefabItem để bỏ GetComponent
        readonly Dictionary<GameObject, PoolPrefabItem> _itemCache = new(256);

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
            _token?.Cancel();    
            _token = new CancelToken();
            var ct = _token.Token;
    
            int needToSpawn = Mathf.Max(0, _spawnAtStart - _pool.CountAll);
            int c = 0;

            try
            {
                for (int i = 0; i < needToSpawn; i++)
                {
                    ct.ThrowIfCancellationRequested();

                    var go = _pool.Get();
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
            GameObject go;

            if (_dontDestroyOnLoad)
            {
                go = UnityEngine.Object.Instantiate(_prefab);
                UnityEngine.Object.DontDestroyOnLoad(go);
            }
            else
            {
                go = UnityEngine.Object.Instantiate(_prefab, PoolRoot, false);
            }

            // Cache PoolPrefabItem nếu có
            var item = go.GetComponent<PoolPrefabItem>();
            if (item != null)
                _itemCache[go] = item;

            go.SetActive(false);
            return go;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void OnGet(GameObject go)
        {
            if (!go) return;
            go.transform.SetParent(null, false);
            go.SetActive(true);
        }

        // ====================== ON RELEASE (siêu tối ưu) ======================
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void OnRelease(GameObject go)
        {
            if (!go) return;

            // ✅ Dọn dẹp cực nhanh, zero GetComponent
            if (_itemCache.TryGetValue(go, out var cachedItem) && cachedItem != null)
            {
                var handlers = cachedItem.GetCachedHandlers();
                if (handlers != null)
                {
                    for (int i = 0; i < handlers.Length; i++)
                    {
                        try { handlers[i]?.OnBeforeReleaseToPool(); }
                        catch (Exception ex) { Debug.LogException(ex); }
                    }
                }
            }
            else
            {
                // fallback nếu chưa có cache (prefab không có PoolPrefabItem)
                var handlers = go.GetComponentsInChildren<IPoolPreRelease>(true);
                for (int i = 0; i < handlers.Length; i++)
                {
                    try { handlers[i]?.OnBeforeReleaseToPool(); }
                    catch (Exception ex) { Debug.LogException(ex); }
                }

                // Nếu prefab này có PoolPrefabItem, cache lại để lần sau nhanh
                var newItem = go.GetComponent<PoolPrefabItem>();
                if (newItem != null)
                    _itemCache[go] = newItem;
            }

            go.SetActive(false);
            go.transform.SetParent(PoolRoot, false);
        }
        // ===================================================================

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void OnDestroy(GameObject go)
        {
            if (!go) return;
#if UNITY_EDITOR
            UnityEngine.Object.DestroyImmediate(go);
#else
            UnityEngine.Object.Destroy(go);
#endif
            _itemCache.Remove(go);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public GameObject Get()
        {
            for (int i = 0; i < 8; i++)
            {
                var go = _pool.Get();
                if (go) return go;
            }

            GameObject fallback;
            if (_dontDestroyOnLoad)
            {
                fallback = UnityEngine.Object.Instantiate(_prefab);
                UnityEngine.Object.DontDestroyOnLoad(fallback);
            }
            else
            {
                fallback = UnityEngine.Object.Instantiate(_prefab, PoolRoot, false);
            }

            // Cache luôn nếu có PoolPrefabItem
            var item = fallback.GetComponent<PoolPrefabItem>();
            if (item != null)
                _itemCache[fallback] = item;

            fallback.SetActive(true);
            return fallback;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Release(GameObject instance)
        {
            if (!instance) return;
            _pool.Release(instance);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => _pool.Clear();
    }
}
