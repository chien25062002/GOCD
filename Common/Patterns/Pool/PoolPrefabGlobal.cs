// PoolPrefabGlobal.cs

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GOCD.Framework
{
    public static class PoolPrefabGlobal
    {
        static readonly Dictionary<GameObject, PoolPrefab> _poolLookup = new();
        static readonly HashSet<GameObject> _releasedInstances = new();
        static readonly HashSet<GameObject> _activeInstances = new();
        static readonly HashSet<GameObject> _pendingInstances = new();
        static readonly Dictionary<GameObject, GameObject> _instanceToPrefab = new();

        // =============== NEW: Cache cho component type ====================
        static readonly Dictionary<(GameObject, System.Type), Component> _cachedComponents = new(512);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static T GetCachedComponent<T>(GameObject go) where T : Component
        {
            if (!go) return null;
            var key = (go, typeof(T));
            if (_cachedComponents.TryGetValue(key, out var c))
                return (T)c;

            var comp = go.GetComponent<T>();
            _cachedComponents[key] = comp;
            return comp;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void RemoveCachedComponents(GameObject go)
        {
            if (!go) return;
            // xóa nhanh không GC
            var keysToRemove = new List<(GameObject, System.Type)>();
            foreach (var kv in _cachedComponents)
                if (kv.Key.Item1 == go)
                    keysToRemove.Add(kv.Key);

            for (int i = 0; i < keysToRemove.Count; i++)
                _cachedComponents.Remove(keysToRemove[i]);
        }
        // ===================================================================

        [RuntimeInitializeOnLoadMethod]
        static void Init()
        {
            if (MonoCallback.Instance != null)
                MonoCallback.Instance.EventActiveSceneChanged += MonoCallback_EventActiveSceneChanged;
        }

        static void MonoCallback_EventActiveSceneChanged(Scene cur, Scene next)
        {
            PoolReleaseScheduler.ClearQueue();

            _releasedInstances.Clear();
            _activeInstances.Clear();
            _pendingInstances.Clear();
            _instanceToPrefab.Clear();
            _cachedComponents.Clear();

            foreach (PoolPrefab pool in _poolLookup.Values)
            {
                if (pool.Config != null && pool.Config.dontDestroyOnLoad) continue;
                pool.Clear();
            }
        }

        public static void Clean()
        {
            PoolReleaseScheduler.ClearQueue();
            _releasedInstances.Clear();
            _activeInstances.Clear();
            _pendingInstances.Clear();
            _instanceToPrefab.Clear();
            _cachedComponents.Clear();

            foreach (PoolPrefab pool in _poolLookup.Values)
            {
                if (pool.Config != null && pool.Config.dontDestroyOnLoad) continue;
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

        public static void EnsurePoolCount()
        {
            foreach (var pool in _poolLookup.Values)
            {
                pool.EnsurePoolCount().SuppressCancellationThrow();
            }
        }

        // ========================= GET =========================
        public static GameObject Get(PoolPrefabConfig config)
        {
            var go = GetPool(config).Get();
            if (go != null)
            {
                _releasedInstances.Remove(go);
                _activeInstances.Add(go);
                _pendingInstances.Remove(go);
                _instanceToPrefab[go] = config.prefab;
            }

            return go;
        }

        public static GameObject Get(GameObject prefab)
        {
            var go = GetPool(prefab).Get();
            if (go != null)
            {
                _releasedInstances.Remove(go);
                _activeInstances.Add(go);
                _pendingInstances.Remove(go);
                _instanceToPrefab[go] = prefab;
            }

            return go;
        }

        // ============ NEW: type-safe Get<T> =============
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Get<T>(PoolPrefabConfig config) where T : Component
        {
            var go = Get(config);
            return GetCachedComponent<T>(go);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Get<T>(GameObject prefab) where T : Component
        {
            var go = Get(prefab);
            return GetCachedComponent<T>(go);
        }
        // ================================================

        // ======================= RELEASE =======================
        public static void Release(PoolPrefabConfig config, GameObject instance)
        {
            if (config == null || !instance) return;
            if (_releasedInstances.Contains(instance) || _pendingInstances.Contains(instance)) return;

            instance.SetActive(false);

            if (!TryScheduleIfDirty(instance, config.prefab))
            {
                _releasedInstances.Add(instance);
                _activeInstances.Remove(instance);
                _instanceToPrefab.Remove(instance);
                RemoveCachedComponents(instance);
                GetPool(config).Release(instance);
            }
            else
            {
                _pendingInstances.Add(instance);
                _activeInstances.Remove(instance);
                _instanceToPrefab.Remove(instance);
                RemoveCachedComponents(instance);
                PoolReleaseScheduler.Enqueue(config.prefab, instance);
            }
        }

        public static void Release(GameObject prefab, GameObject instance)
        {
            if (!prefab || !instance) return;
            if (_releasedInstances.Contains(instance) || _pendingInstances.Contains(instance)) return;

            if (!TryScheduleIfDirty(instance, prefab))
            {
                _releasedInstances.Add(instance);
                _activeInstances.Remove(instance);
                _instanceToPrefab.Remove(instance);
                RemoveCachedComponents(instance);
                GetPool(prefab).Release(instance);
            }
            else
            {
                _pendingInstances.Add(instance);
                _activeInstances.Remove(instance);
                _instanceToPrefab.Remove(instance);
                RemoveCachedComponents(instance);
                PoolReleaseScheduler.Enqueue(prefab, instance);
            }
        }

        // ============ NEW: type-safe Release<T> =============
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Release<T>(PoolPrefabConfig config, T inst) where T : Component
        {
            if (!inst) return;
            RemoveCachedComponents(inst.gameObject);
            Release(config, inst.gameObject);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Release<T>(GameObject prefab, T inst) where T : Component
        {
            if (!inst) return;
            RemoveCachedComponents(inst.gameObject);
            Release(prefab, inst.gameObject);
        }
        // =====================================================

        static bool TryScheduleIfDirty(GameObject instance, GameObject prefab)
        {
            var handlers = instance.GetComponentsInChildren<IPoolPreRelease>(true);
            for (int i = 0; i < handlers.Length; i++)
            {
                var h = handlers[i];
                if (h != null && h.IsDirty)
                {
                    return true; // dirty → để scheduler xử lý
                }
            }

            return false;
        }

        // ======================= GET POOL ======================
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

        // =================== CHECK / WAIT / FORCE ==============
        public static bool IsReleased(GameObject instance) => instance && _releasedInstances.Contains(instance);
        public static bool IsPending(GameObject instance) => instance && _pendingInstances.Contains(instance);
        public static int ActiveCount => _activeInstances.Count;

        public static UniTask FlushAsync(int maxPerFrame = 256) => PoolReleaseScheduler.FlushAsync(maxPerFrame);

        public static async UniTask WaitForIdleAsync(float timeoutSeconds = 3f)
        {
            float end = Time.realtimeSinceStartup + timeoutSeconds;
            while ((ActiveCount > 0 || PoolReleaseScheduler.PendingCount > 0) &&
                   Time.realtimeSinceStartup < end)
            {
                await Cysharp.Threading.Tasks.UniTask.Yield(Cysharp.Threading.Tasks.PlayerLoopTiming
                    .LastPostLateUpdate);
            }
        }

        public static async UniTask FlushAndWaitIdleAsync(float timeoutSeconds = 3f, int flushBatch = 256)
        {
            await FlushAsync(flushBatch);
            await WaitForIdleAsync(timeoutSeconds);
        }

        /// <summary>
        /// FORCE: ép toàn bộ active trả về pool (kể cả chưa gọi Release), rồi flush & wait.
        /// </summary>
        public static async UniTask ForceReleaseAllAsync(float timeoutSeconds = 3f, int flushBatch = 256)
        {
            var tmp = new List<GameObject>(_activeInstances);
            for (int i = 0; i < tmp.Count; i++)
            {
                var inst = tmp[i];
                if (!inst) continue;

                if (_instanceToPrefab.TryGetValue(inst, out var prefab) && prefab)
                {
                    Release(prefab, inst);
                }
                else
                {
                    RemoveCachedComponents(inst);
                    Object.Destroy(inst);
                    _activeInstances.Remove(inst);
                    _releasedInstances.Add(inst);
                    _instanceToPrefab.Remove(inst);
                }
            }

            await FlushAndWaitIdleAsync(timeoutSeconds, flushBatch);
        }

        // Scheduler gọi khi cleanup xong
        internal static void MarkAsReleased(GameObject prefab, GameObject inst)
        {
            if (!inst) return;

            _pendingInstances.Remove(inst);
            _releasedInstances.Add(inst);
            if (_instanceToPrefab.ContainsKey(inst))
                _instanceToPrefab.Remove(inst);

            RemoveCachedComponents(inst);
            GetPool(prefab).Release(inst);
        }
    }
}