using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

namespace CodeSketch.Patterns.Pool
{
    public sealed class PoolPrefab
    {
        readonly GameObject _prefab;
        readonly ObjectPool<PoolPrefabItem> _pool;

        public PoolPrefabConfig Config { get; }


        public PoolPrefab(PoolPrefabConfig config)
        {
            if (config == null || !config.Prefab)
                throw new InvalidOperationException("PoolPrefab: Config hoặc prefab null!");

            Config = config;
            
            _prefab = config.Prefab;

            _pool = new ObjectPool<PoolPrefabItem>(
                Create, OnGet, OnRelease, OnDestroy,
                false, config.PoolCapacity, config.PoolCapacityMax
            );

            // Prewarm
            while (_pool.CountInactive < config.PoolPrewarm)
            {
                var go = Create();
                _pool.Release(go);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        PoolPrefabItem Create()
        {
            PoolPrefabItem item = null;
            
            if (Application.isPlaying)
            {
                var instance = Object.Instantiate(_prefab); 
                item = instance.GetComponent<PoolPrefabItem>();
                
                if (item == null)
                {
                    Object.Destroy(instance);
                    return null;
                }

                item.GameObjectCached.SetActive(false);
            }
            return item;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void OnGet(PoolPrefabItem item)
        {
            if (!item) return;
            
            if (Config.PersistAcrossScenes)
            {
                Pooler.Attach(item);
            }
            else
            {
                item.TransformCached.SetParent(null);
            }
            
            item.GameObjectCached.SetActive(true);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void OnRelease(PoolPrefabItem item)
        {
            if (!item) return;

            // var poolRelease = item.GetCachedInterfaces<IPoolRelease>();
            //
            // foreach (var release in poolRelease)
            // {
            //     release.TaskBeforeRelease();
            // }

            if (!Config.DeactiveOnRelease)
                item.TransformCached.position = new Vector3(99999f, -99999f, 99999f);
            else
                item.GameObjectCached.SetActive(false);

            Pooler.Attach(item);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void OnDestroy(PoolPrefabItem go)
        {
            if (!go) return;
#if UNITY_EDITOR
            Object.DestroyImmediate(go);
#else
            Object.Destroy(go);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PoolPrefabItem Get()
        {
            var item = _pool.Get();
            if (item == null)
            {
                Create();
                return Get();
            }

            return item;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Release(PoolPrefabItem item)
        {
            if (item) _pool.Release(item);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => _pool.Clear();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DestroyAll()
        {
            _pool.Clear();
        }
    }
}
