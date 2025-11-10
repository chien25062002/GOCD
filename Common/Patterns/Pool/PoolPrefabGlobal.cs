using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;
using GOCD.Framework.Diagnostics;
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

        static readonly Vector3 s_MoveFarPosition = new(99999f, -99999f, 99999f);
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

        [RuntimeInitializeOnLoadMethod]
        static void Init()
        {
            if (MonoCallback.Instance != null)
                MonoCallback.Instance.EventActiveSceneChanged += MonoCallback_EventActiveSceneChanged;
        }

        static void MonoCallback_EventActiveSceneChanged(Scene cur, Scene next)
        {
            Clean();
        }

        public static void Clean()
        {
            PoolReleaseScheduler.ClearQueue();
            _releasedInstances.Clear();
            _activeInstances.Clear();
            _pendingInstances.Clear();
            _instanceToPrefab.Clear();
            _cachedComponents.Clear();

            foreach (var pool in _poolLookup.Values)
            {
                if (pool.Config != null && pool.Config.dontDestroyOnLoad) continue;
                pool.Clear();
            }
        }

        public static void Construct(params PoolPrefabConfig[] configs)
        {
            for (int i = 0; i < configs.Length; i++)
            {
                var cfg = configs[i];
                if (cfg == null || !cfg.prefab) continue;
                if (!_poolLookup.ContainsKey(cfg.prefab))
                    _poolLookup.Add(cfg.prefab, new PoolPrefab(cfg));
            }
        }

        public static void EnsurePoolCount()
        {
            foreach (var pool in _poolLookup.Values)
                pool.EnsurePoolCount();
        }

        // =========================================================
        public static GameObject Get(PoolPrefabConfig config)
        {
            if (config == null || !config.prefab) return null;
            var go = GetPool(config).Get();
            if (go == null) return null;

            _releasedInstances.Remove(go);
            _pendingInstances.Remove(go);
            _activeInstances.Add(go);
            _instanceToPrefab[go] = config.prefab;
            return go;
        }

        public static GameObject Get(GameObject prefab)
        {
            if (!prefab) return null;
            var go = GetPool(prefab).Get();
            if (go == null) return null;

            _releasedInstances.Remove(go);
            _pendingInstances.Remove(go);
            _activeInstances.Add(go);
            _instanceToPrefab[go] = prefab;
            return go;
        }

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

        // =========================================================
        public static void Release(PoolPrefabConfig config, GameObject inst)
        {
            if (config == null || !inst) return;
            if (_releasedInstances.Contains(inst) || _pendingInstances.Contains(inst)) return;

            inst.SetActive(false);

            if (!TryScheduleIfDirty(inst, config.prefab))
            {
                _releasedInstances.Add(inst);
                _activeInstances.Remove(inst);
                _instanceToPrefab.Remove(inst);
                GetPool(config).Release(inst);
            }
            else
            {
                _pendingInstances.Add(inst);
                _activeInstances.Remove(inst);
                _instanceToPrefab.Remove(inst);
                PoolReleaseScheduler.Enqueue(config.prefab, inst);
            }
        }

        public static void Release(GameObject prefab, GameObject inst)
        {
            if (!prefab || !inst) return;
            if (_releasedInstances.Contains(inst) || _pendingInstances.Contains(inst)) return;

            if (!TryScheduleIfDirty(inst, prefab))
            {
                _releasedInstances.Add(inst);
                _activeInstances.Remove(inst);
                _instanceToPrefab.Remove(inst);
                GetPool(prefab).Release(inst);
            }
            else
            {
                _pendingInstances.Add(inst);
                _activeInstances.Remove(inst);
                _instanceToPrefab.Remove(inst);
                PoolReleaseScheduler.Enqueue(prefab, inst);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Release<T>(PoolPrefabConfig cfg, T inst) where T : Component
        {
            if (!inst) return;
            Release(cfg, inst.gameObject);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Release<T>(GameObject prefab, T inst) where T : Component
        {
            if (!inst) return;
            Release(prefab, inst.gameObject);
        }

        // =========================================================
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ReleaseMoveFar(GameObject prefab, GameObject inst)
        {
            if (!prefab || !inst) return;
            if (_releasedInstances.Contains(inst) || _pendingInstances.Contains(inst)) return;

            inst.transform.position = s_MoveFarPosition;

            if (!TryScheduleIfDirty(inst, prefab))
            {
                _releasedInstances.Add(inst);
                _activeInstances.Remove(inst);
                _instanceToPrefab.Remove(inst);
                GetPool(prefab).skipSetInactive = true;
                GetPool(prefab).Release(inst);
            }
            else
            {
                _pendingInstances.Add(inst);
                _activeInstances.Remove(inst);
                _instanceToPrefab.Remove(inst);
                PoolReleaseScheduler.Enqueue(prefab, inst);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ReleaseMoveFar<T>(GameObject prefab, T inst) where T : Component
        {
            if (!inst) return;
            ReleaseMoveFar(prefab, inst.gameObject);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ReleaseMoveFar(PoolPrefabConfig cfg, GameObject inst)
        {
            if (cfg == null || !inst) return;
            if (_releasedInstances.Contains(inst) || _pendingInstances.Contains(inst)) return;

            inst.transform.position = s_MoveFarPosition;

            if (!TryScheduleIfDirty(inst, cfg.prefab))
            {
                _releasedInstances.Add(inst);
                _activeInstances.Remove(inst);
                _instanceToPrefab.Remove(inst);
                GetPool(cfg).skipSetInactive = true;
                GetPool(cfg).Release(inst);
            }
            else
            {
                _pendingInstances.Add(inst);
                _activeInstances.Remove(inst);
                _instanceToPrefab.Remove(inst);
                PoolReleaseScheduler.Enqueue(cfg.prefab, inst);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ReleaseMoveFar<T>(PoolPrefabConfig cfg, T inst) where T : Component
        {
            if (!inst) return;
            ReleaseMoveFar(cfg, inst.gameObject);
        }

        // =========================================================
        static bool TryScheduleIfDirty(GameObject inst, GameObject prefab)
        {
            var handlers = inst.GetComponentsInChildren<IPoolPreRelease>(true);
            for (int i = 0; i < handlers.Length; i++)
            {
                var h = handlers[i];
                if (h != null && h.IsDirty) return true;
            }
            return false;
        }

        public static PoolPrefab GetPool(PoolPrefabConfig cfg)
        {
            if (!_poolLookup.TryGetValue(cfg.prefab, out var pool))
            {
                pool = new PoolPrefab(cfg);
                _poolLookup.Add(cfg.prefab, pool);
            }
            return pool;
        }

        public static PoolPrefab GetPool(GameObject prefab)
        {
            if (!_poolLookup.TryGetValue(prefab, out var pool))
            {
                pool = new PoolPrefab(prefab);
                _poolLookup.Add(prefab, pool);
            }
            return pool;
        }

        // =========================================================
        public static bool IsReleased(GameObject inst) => inst && _releasedInstances.Contains(inst);
        public static bool IsPending(GameObject inst) => inst && _pendingInstances.Contains(inst);
        public static int ActiveCount => _activeInstances.Count;

        public static UniTask FlushAsync(int maxPerFrame = 256) => PoolReleaseScheduler.FlushAsync(maxPerFrame);

        public static async UniTask WaitForIdleAsync(float timeoutSeconds = 3f)
        {
            float end = Time.realtimeSinceStartup + timeoutSeconds;
            while ((ActiveCount > 0 || PoolReleaseScheduler.PendingCount > 0) &&
                   Time.realtimeSinceStartup < end)
                await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
        }

        public static async UniTask FlushAndWaitIdleAsync(float timeoutSeconds = 3f, int flushBatch = 256)
        {
            await FlushAsync(flushBatch);
            await WaitForIdleAsync(timeoutSeconds);
        }

        public static async UniTask ForceReleaseAllAsync(float timeoutSeconds = 3f, int flushBatch = 256)
        {
            var tmp = new List<GameObject>(_activeInstances);
            for (int i = 0; i < tmp.Count; i++)
            {
                var inst = tmp[i];
                if (!inst) continue;

                if (_instanceToPrefab.TryGetValue(inst, out var prefab) && prefab)
                    Release(prefab, inst);
                else
                {
                    Object.Destroy(inst);
                    _activeInstances.Remove(inst);
                    _releasedInstances.Add(inst);
                    _instanceToPrefab.Remove(inst);
                }
            }

            await FlushAndWaitIdleAsync(timeoutSeconds, flushBatch);
        }

        internal static void MarkAsReleased(GameObject prefab, GameObject inst)
        {
            if (!inst) return;
            _pendingInstances.Remove(inst);
            _releasedInstances.Add(inst);
            _instanceToPrefab.Remove(inst);
            GetPool(prefab).Release(inst);
        }

        // =========================================================
        public static void DestroyPools(IList<PoolPrefabConfig> configs)
        {
            if (configs == null || configs.Count == 0) return;

            int destroyed = 0;
            foreach (var cfg in configs)
            {
                if (cfg == null || cfg.prefab == null) continue;

                if (_poolLookup.TryGetValue(cfg.prefab, out var pool))
                {
                    pool.DestroyAll();
                    _poolLookup.Remove(cfg.prefab);
                    destroyed++;
                }
            }

            // 🧹 Clean orphan references
            _activeInstances.RemoveWhere(x => x == null);
            _releasedInstances.RemoveWhere(x => x == null);
            _pendingInstances.RemoveWhere(x => x == null);

            List<GameObject> removeKeys = new();
            foreach (var kv in _instanceToPrefab)
                if (kv.Key == null || kv.Value == null) removeKeys.Add(kv.Key);
            for (int i = 0; i < removeKeys.Count; i++)
                _instanceToPrefab.Remove(removeKeys[i]);

            PoolPrefab.TryDestroyRootIfEmpty();

            GOCDDebug.Log($"🧹 DestroyPools(): {destroyed}/{configs.Count} pool(s) destroyed.", Color.green);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetPoolFast(GameObject prefab, out PoolPrefab pool)
            => _poolLookup.TryGetValue(prefab, out pool);

        public static Vector3 MoveFarPos => s_MoveFarPosition;
    }
}
