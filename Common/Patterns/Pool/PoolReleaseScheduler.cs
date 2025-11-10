using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace GOCD.Framework
{
    public static class PoolReleaseScheduler
    {
        const int ItemsPerFrame = 16;

        static readonly Queue<(GameObject prefab, GameObject instance)> _queue = new();
        static readonly HashSet<GameObject> _inQueue   = new();
        static readonly HashSet<GameObject> _cancelled = new();

        static bool _running;

        [RuntimeInitializeOnLoadMethod]
        static void Boot() => StartLoopIfNeeded();

        static void StartLoopIfNeeded()
        {
            if (_running) return;
            RunLoop().Forget();
        }

        public static int PendingCount => _queue.Count;

        public static void Enqueue(GameObject prefab, GameObject instance)
        {
            if (prefab == null || instance == null) return;
            if (PoolPrefabGlobal.IsReleased(instance)) return;
            if (!_inQueue.Add(instance)) return;

            _queue.Enqueue((prefab, instance));
            StartLoopIfNeeded();
        }

        public static bool Cancel(GameObject instance)
        {
            if (!instance) return false;
            if (!_inQueue.Contains(instance)) return false;
            _cancelled.Add(instance);
            return true;
        }

        public static void ClearQueue()
        {
            _queue.Clear();
            _inQueue.Clear();
            _cancelled.Clear();
        }

        public static bool IsEnqueued(GameObject instance) => instance && _inQueue.Contains(instance);

        public static async UniTask FlushAsync(int maxPerFrame = 128)
        {
            int processedThisFrame = 0;

            while (_queue.Count > 0)
            {
                var (prefab, inst) = _queue.Dequeue();

                if (!inst) { _inQueue.Remove(inst); continue; }
                if (_cancelled.Remove(inst)) { _inQueue.Remove(inst); continue; }

                await PreReleaseValidate(inst);

                if (!inst) { _inQueue.Remove(inst); continue; }
                if (PoolPrefabGlobal.IsReleased(inst)) { _inQueue.Remove(inst); continue; }

                // ✅ cleanup xong, mark thật sự released
                PoolPrefabGlobal.MarkAsReleased(prefab, inst);

                _inQueue.Remove(inst);

                processedThisFrame++;
                if (processedThisFrame >= maxPerFrame)
                {
                    processedThisFrame = 0;
                    await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
                }
            }
        }

        static async UniTaskVoid RunLoop()
        {
            _running = true;
            try
            {
                while (Application.isPlaying)
                {
                    if (_queue.Count == 0)
                    {
                        await UniTask.Yield(PlayerLoopTiming.Update);
                        continue;
                    }

                    int budget = ItemsPerFrame;
                    while (budget-- > 0 && _queue.Count > 0)
                    {
                        var (prefab, inst) = _queue.Dequeue();

                        if (!inst) { _inQueue.Remove(inst); continue; }
                        if (_cancelled.Remove(inst)) { _inQueue.Remove(inst); continue; }

                        await PreReleaseValidate(inst);

                        if (!inst) { _inQueue.Remove(inst); continue; }
                        if (PoolPrefabGlobal.IsReleased(inst)) { _inQueue.Remove(inst); continue; }

                        PoolPrefabGlobal.MarkAsReleased(prefab, inst);

                        _inQueue.Remove(inst);
                    }

                    await UniTask.Yield(PlayerLoopTiming.Update);
                }
            }
            finally
            {
                _running = false;
            }
        }

        static async UniTask PreReleaseValidate(GameObject go)
        {
            if (!go) return;

            var handlers = go.GetComponentsInChildren<IPoolPreRelease>(true);
            for (int i = 0; i < handlers.Length; i++)
            {
                var h = handlers[i];
                if (h == null || !h.IsDirty) continue;

                h.OnBeforeReleaseToPool();

                if (h is IAsyncPoolPreRelease asyncH)
                    await asyncH.OnBeforeReleaseToPoolAsync();
                
                if (i % 32 == 0)
                    await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
            }

            await UniTask.Yield();
        }
    }
}
