using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

namespace GOCD.Framework
{
    public sealed class PoolPrefab
    {
        const int DefaultCapacity = 16;
        const int MaxSize = 4096;

        readonly GameObject _prefab;
        readonly bool _dontDestroyOnLoad;
        readonly ObjectPool<GameObject> _pool;

        readonly Dictionary<GameObject, PoolPrefabItem> _itemCache = new(256);
        readonly List<GameObject> _allCreated = new(128);

        int _spawnAtStart;
        public bool skipSetInactive;
        public PoolPrefabConfig Config { get; }

        static Transform s_PoolRoot;
        static Transform PoolRoot
        {
            get
            {
                if (s_PoolRoot == null)
                {
                    var go = new GameObject("__PoolRoot");
                    if (Application.isPlaying)
                        Object.DontDestroyOnLoad(go);
                    s_PoolRoot = go.transform;
                }
                return s_PoolRoot;
            }
        }

        public PoolPrefab(PoolPrefabConfig config)
        {
            if (config == null || !config.prefab)
                throw new InvalidOperationException("PoolPrefab: Config hoặc prefab null!");

            Config = config;
            _prefab = config.prefab;
            _dontDestroyOnLoad = config.dontDestroyOnLoad;
            _spawnAtStart = config.spawnAtStart;

            _pool = new ObjectPool<GameObject>(
                Create, OnGet, OnRelease, OnDestroy,
                false, config.spawnCapacity, config.spawnCapacityMax
            );

            EnsurePoolCount();
        }

        public PoolPrefab(GameObject prefab)
        {
            if (!prefab) throw new InvalidOperationException("PoolPrefab: prefab null!");
            _prefab = prefab;
            _dontDestroyOnLoad = false;
            Config = null;

            _pool = new ObjectPool<GameObject>(
                Create, OnGet, OnRelease, OnDestroy,
                false, DefaultCapacity, MaxSize
            );
        }

        public void EnsurePoolCount()
        {
            while (_pool.CountInactive < _spawnAtStart)
            {
                var go = Create();
                _pool.Release(go);
            }
        }

        GameObject Create()
        {
            GameObject go;
            if (_dontDestroyOnLoad && Application.isPlaying)
            {
                go = Object.Instantiate(_prefab);
                Object.DontDestroyOnLoad(go);
            }
            else go = Object.Instantiate(_prefab, PoolRoot, false);

            var item = go.GetComponent<PoolPrefabItem>();
            if (item != null) _itemCache[go] = item;
            _allCreated.Add(go);
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void OnRelease(GameObject go)
        {
            if (!go) return;

            if (_itemCache.TryGetValue(go, out var cachedItem) && cachedItem != null)
            {
                var handlers = cachedItem.GetCachedHandlers();
                if (handlers != null)
                {
                    for (int i = 0; i < handlers.Length; i++)
                        handlers[i]?.OnBeforeReleaseToPool();
                }
            }
            else
            {
                var handlers = go.GetComponentsInChildren<IPoolPreRelease>(true);
                for (int i = 0; i < handlers.Length; i++)
                    handlers[i]?.OnBeforeReleaseToPool();

                var newItem = go.GetComponent<PoolPrefabItem>();
                if (newItem != null) _itemCache[go] = newItem;
            }

            if (skipSetInactive)
                go.transform.position = new Vector3(99999f, -99999f, 99999f);
            else
                go.SetActive(false);

            go.transform.SetParent(PoolRoot, false);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void OnDestroy(GameObject go)
        {
            if (!go) return;
#if UNITY_EDITOR
            Object.DestroyImmediate(go);
#else
            Object.Destroy(go);
#endif
            _itemCache.Remove(go);
            _allCreated.Remove(go);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public GameObject Get()
        {
            var go = _pool.Get();
            if (go) return go;

            var fallback = Object.Instantiate(_prefab, PoolRoot, false);
            var item = fallback.GetComponent<PoolPrefabItem>();
            if (item != null) _itemCache[fallback] = item;
            _allCreated.Add(fallback);
            fallback.SetActive(true);
            return fallback;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Release(GameObject instance)
        {
            if (instance) _pool.Release(instance);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => _pool.Clear();

        public void DestroyAll()
        {
            _pool.Clear();

            for (int i = 0; i < _allCreated.Count; i++)
            {
                var go = _allCreated[i];
                if (go != null) Object.Destroy(go);
            }

            _allCreated.Clear();
            _itemCache.Clear();
        }

        // 🧹 Tự hủy PoolRoot nếu không còn pool nào
        internal static void TryDestroyRootIfEmpty()
        {
            if (s_PoolRoot != null && s_PoolRoot.childCount == 0)
            {
                Object.Destroy(s_PoolRoot.gameObject);
                s_PoolRoot = null;
            }
        }
    }
}
